
//#define EXERCISE

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Device = global::SharpDX.Direct3D11.Device;
using Buffer = global::SharpDX.Direct3D11.Buffer;


namespace HelixToolkit.SharpDX.Wpf
{
    public class DeferredRenderer : IDisposable
    {
        private Texture2D gBufferDepth;
        private Texture2D[] gBuffer;
        private RenderTargetView[] gBufferRenderTargetView;
        private ShaderResourceView[] gBufferShaderResourceView;
        private DepthStencilView gBufferDepthStencilView;
        private DeferredLightingVariables deferredLightingVariables;
        private Color4 ambientLightColor = new Color4(0.2f, 0.2f, 0.2f, 1.0f);

        private int targetHeight, targetWidth;
        private IRenderHost renderHost;
        private Device device;

        private LightGeometryData screenQuad;
        private LightGeometryData screenSphere;
        private LightGeometryData screenCone;


        /// <summary>
        /// This is the main lighting render function. 
        /// It is called per frame. 
        /// </summary>
        internal void RenderLighting(RenderContext context, IEnumerable<ILight3D> lightsCollection)
        {
            /// --- extract all lights from collections an build one IEnumerable seqence
            var lights = lightsCollection.Where(l => l is Light3D).Concat(lightsCollection.Where(l => l is Light3DCollection).SelectMany(x => (x as Light3DCollection).Children.Select(xx => xx as ILight3D)));

            /// --- eye position
            this.deferredLightingVariables.vEyePos.Set(context.Camera.Position.ToVector4());

            /// --- bind quad geometry (good for ambient and directional lights)
            this.BindQuadBuffer();


            /// -- get the ambient light  (there must be only one)
            var ambientLight = lightsCollection.Where(l => l is AmbientLight3D);
            if (ambientLight.Count() > 0)
            {
                ambientLightColor = ((AmbientLight3D)ambientLight.First()).Color;
            }

            /// --- set and render the ambient light pass 
            this.deferredLightingVariables.vLightAmbient.Set(ambientLightColor);
            this.deferredLightingVariables.renderPassAmbient.Apply(device.ImmediateContext);
            this.device.ImmediateContext.DrawIndexed(this.screenQuad.IndexCount, 0, 0);


            /// --- all next passes are with additive blending
            var pass = this.deferredLightingVariables.renderPassDirectionalLight;

            /// --- go over all directional lights
            foreach (DirectionalLight3D light in lights.Where(l => l is DirectionalLight3D))
            {
                /// set light variables    
                this.deferredLightingVariables.vLightColor.Set(light.Color);
                this.deferredLightingVariables.vLightDir.Set(light.Direction.ToVector4());

                /// --- render the geometry
                pass.Apply(device.ImmediateContext);
                this.device.ImmediateContext.DrawIndexed(this.screenQuad.IndexCount, 0, 0);
            }

            /// --- now bind sphere geometry (good for point-lights)
            this.BindSphereBuffer();

            /// --- set the point-light pass (with additive blending)
            pass = this.deferredLightingVariables.renderPassPointLight;

            /// --- go over all point lights
            foreach (PointLight3D light in lights.Where(l => l is PointLight3D))
            {
                /// set light variables    
                this.deferredLightingVariables.vLightColor.Set(light.Color);
                this.deferredLightingVariables.vLightPos.Set(light.Position.ToVector4());
                this.deferredLightingVariables.vLightAtt.Set(light.Attenuation.ToVector4(100f));

#if EXERCISE

                // TODO 2:
                //  - calculate and transfer the lightModelMatrix to the shader (calculate an appropriate scaling depending on the attenuation)

                this.deferredLightingVariables.mLightView.SetMatrix(context.viewMatrix);
                this.deferredLightingVariables.mLightProj.SetMatrix(context.projectionMatrix);
#else


                //generate light model matrix
                //attenuation - not influenced by color yet
                float maxDist = light.Color.Red;
                if (light.Color.Green > maxDist)
                {
                    maxDist = light.Color.Green;
                }
                if (light.Color.Blue > maxDist)
                {
                    maxDist = light.Color.Blue;
                }
                double c = light.Attenuation.X;
                double l = light.Attenuation.Y;
                double q = light.Attenuation.Z;
                double lightRadius = 100.0; // some maximum (but smaller than view frustum)
                double colMax = 64;
                if (q > 0)
                {
                    lightRadius = (Math.Sqrt(c * c - 4 * l * q + 4 * colMax * q) - c) / (2 * q);
                }
                else if (q == 0 && c > 0)
                {
                    lightRadius = (colMax - l) / c;
                }
                Matrix lightModelMatrix = Matrix.Scaling((float)lightRadius) * Matrix.Translation(light.Position.ToVector3());

                this.deferredLightingVariables.mLightModel.SetMatrix(lightModelMatrix);
                this.deferredLightingVariables.mLightView.SetMatrix(context.viewMatrix);
                this.deferredLightingVariables.mLightProj.SetMatrix(context.projectionMatrix);


#endif

                /// --- render the geometry
                pass.Apply(device.ImmediateContext);
                this.device.ImmediateContext.DrawIndexed(this.screenSphere.IndexCount, 0, 0);
            }

            /// --- bind cone buffer for spot-lights
            this.BindConeBuffer();

            /// --- set the spot-light pass (with additive blending)
            pass = this.deferredLightingVariables.renderPassSpotLight;

            /// --- go over all spot-lights
            foreach (SpotLight3D light in lights.Where(l => l is SpotLight3D))
            {
                var spot = new Vector4((float)Math.Cos(light.OuterAngle / 360.0 * Math.PI), (float)Math.Cos(light.InnerAngle / 360.0 * Math.PI), (float)light.Falloff, 0);
                this.deferredLightingVariables.vLightSpot.Set(ref spot);

                /// TODO BONUS 1: cone-geometry rendering
            }

            /// --- deactivate blending
            this.device.ImmediateContext.OutputMerger.SetBlendState(null);
        }

        /// <summary>
        /// This render-function is use to directly display the 4 G-Buffer textures in one screen. 
        /// It does not compute any lighting, it just copies the (resized) buffer-textures into the back-buffer. 
        /// It is called per frame. 
        /// </summary>
        internal void RenderGBufferOutput(ref Texture2D renderTarget, bool merge = false)
        {
            var midX = this.targetWidth / 2;
            var midY = this.targetHeight / 2;

            if (merge)
            {
                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[0], 0,
                    new ResourceRegion(0, 0, 0, midX, midY, 1),
                    renderTarget, 0, 0, 0, 0);

                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[1], 0,
                    new ResourceRegion(midX, 0, 0, 2 * midX, midY, 1),
                    renderTarget, 0, midX, 0, 0);

                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[2], 0,
                    new ResourceRegion(0, midY, 0, midX, 2 * midY, 1),
                    renderTarget, 0, 0, midY, 0);

                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[3], 0,
                    new ResourceRegion(midX, midY, 0, 2 * midX, 2 * midY, 1),
                    renderTarget, 0, midX, midY, 0);
            }
            else
            {
                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[0], 0,
                    new ResourceRegion(0, 0, 0, midX, midY, 1),
                    renderTarget, 0, 0, 0, 0);

                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[1], 0,
                    new ResourceRegion(0, 0, 0, midX, midY, 1),
                    renderTarget, 0, midX, 0, 0);

                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[2], 0,
                    new ResourceRegion(0, 0, 0, midX, midY, 1),
                    renderTarget, 0, 0, midY, 0);

                this.device.ImmediateContext.CopySubresourceRegion(this.gBuffer[3], 0,
                    new ResourceRegion(0, 0, 0, midX, midY, 1),
                    renderTarget, 0, midX, midY, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void SetGBufferTargets()
        {
            this.SetGBufferTargets(this.targetWidth, this.targetHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal void SetGBufferTargets(int width, int height)
        {
            /// --- set rasterizes state here with proper shadow-bias, as depth-bias and slope-bias in the rasterizer            
            this.device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, width, height, 0.0f, 1.0f));
            this.device.ImmediateContext.OutputMerger.SetTargets(this.gBufferDepthStencilView, this.gBufferRenderTargetView);

            // 0 normal
            // 1 diffuse
            // 2 spec
            // 3 pos

            /// --- clear buffers
            this.device.ImmediateContext.ClearDepthStencilView(this.gBufferDepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            this.device.ImmediateContext.ClearRenderTargetView(this.gBufferRenderTargetView[0], new Color4());
            this.device.ImmediateContext.ClearRenderTargetView(this.gBufferRenderTargetView[1], new Color4()); //this.renderHost.ClearColor);
            this.device.ImmediateContext.ClearRenderTargetView(this.gBufferRenderTargetView[2], new Color4());
            this.device.ImmediateContext.ClearRenderTargetView(this.gBufferRenderTargetView[3], new Color4());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="format"></param>
        internal void CreateBuffers(IRenderHost host, Format format = Format.R32G32B32A32_Float)
        {
            /// set class variables
            this.renderHost = host;
            this.device = host.Device;
            this.targetWidth = Math.Max((int)host.ActualWidth, 100);
            this.targetHeight = Math.Max((int)host.ActualHeight, 100);

            /// clear old buffers
            this.ClearGBuffer();
            this.screenQuad.Dispose();
            this.screenSphere.Dispose();
            this.screenCone.Dispose();

            /// create new buffers            
            this.CreateQuadBuffer();
            this.CreateSphereBuffer();
            this.CreateConeBuffer();
            this.CreateGBuffer(format);



            /// flush
            this.device.ImmediateContext.Flush();
        }


        /// <summary>
        /// Create the G-Buffer
        /// call it once on init
        /// </summary>
        private void CreateGBuffer(Format format = Format.R32G32B32A32_Float)
        {
            /// alloc buffer arrays
            this.gBuffer = new Texture2D[4];
            this.gBufferRenderTargetView = new RenderTargetView[4];
            this.gBufferShaderResourceView = new ShaderResourceView[4];

            /// Create the render targets for our unoptimized G-Buffer, 
            /// which just uses 32-bit floats for everything
            for (int i = 0; i < 4; i++)
            {
                // texture
                this.gBuffer[i] = (new Texture2D(device, new Texture2DDescription()
                {
                    Format = format,
                    //Format = Format.R32G32B32A32_Float,
                    //Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Width = targetWidth,
                    Height = targetHeight,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.None,
                }));

                // render target
                this.gBufferRenderTargetView[i] = new RenderTargetView(this.device, this.gBuffer[i]);

                // shader resource
                this.gBufferShaderResourceView[i] = new ShaderResourceView(this.device, this.gBuffer[i], new ShaderResourceViewDescription()
                {
                    Format = format,
                    //Format = Format.R32G32B32A32_Float,
                    //Format = Format.B8G8R8A8_UNorm,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    }
                });
            }

            /// Create Depth-Stencil map for g-buffer            
            this.gBufferDepth = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.R32_Typeless, //!!!! because of depth and shader resource
                //Format = global::SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = targetWidth,
                Height = targetHeight,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource, //!!!!
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            });

            this.gBufferDepthStencilView = new DepthStencilView(device, gBufferDepth, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new DepthStencilViewDescription.Texture2DResource()
                {
                    MipSlice = 0
                }
            });

            /// --- set the g-buffer resources
            for (int i = 0; i < gBuffer.Length; i++)
            {
                this.deferredLightingVariables.gBufferShaderResourceVariables[i].SetResource(this.gBufferShaderResourceView[i]);
            }
        }

        /// <summary>
        /// Create the geometry and the VBO (vertex and index buffers) 
        /// for light-geoemtry 
        /// Call it once in the scene initialization 
        /// </summary>
        private void CreateQuadBuffer()
        {
            this.deferredLightingVariables = new DeferredLightingVariables(this.renderHost.Effects);

            var vertices = new[]
            {
                new Vector4(-1.0f, -1.0f, 0.0f, 1.0f), 
                new Vector4(+1.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(+1.0f, +1.0f, 0.0f, 1.0f),
                new Vector4(-1.0f, +1.0f, 0.0f, 1.0f),
            };
            int[] indices = { 0, 1, 2, 2, 3, 0 };

            this.screenQuad = new LightGeometryData()
            {
                IndexBuffer = this.device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), indices),
                VertexBuffer = this.device.CreateBuffer(BindFlags.VertexBuffer, Vector4.SizeInBytes, vertices),
                IndexCount = indices.Length,
            };
        }

        /// <summary>
        /// Create the geometry and the VBO (vertex and index buffers) 
        /// for light-geoemtry 
        /// Call it once in the scene initialization 
        /// </summary>
        private void CreateSphereBuffer()
        {
            this.deferredLightingVariables = new DeferredLightingVariables(this.renderHost.Effects);

#if EXERCISE


            // TODO 2:
            //  - generate a sphere mesh using the MeshBuilder()
            //  - fill the screenSphere struct


#else


            var b1 = new MeshBuilder();
            b1.AddSphere(new Vector3(0, 0, 0), 1.0);
            MeshGeometry3D meshGeometry = b1.ToMeshGeometry3D();

            var vertices = meshGeometry.Positions.Select(p => new Vector4(p, 1.0f)).ToArray();
            int[] indices = meshGeometry.Indices;

            this.screenSphere = new LightGeometryData()
            {
                IndexBuffer = this.device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), indices),
                VertexBuffer = this.device.CreateBuffer(BindFlags.VertexBuffer, Vector4.SizeInBytes, vertices),
                IndexCount = meshGeometry.Indices.Length,
            };


#endif

        }

        /// <summary>
        /// Create the geometry and the VBO (vertex and index buffers) 
        /// for light-geoemtry 
        /// Call it once in the scene initialization 
        /// </summary>
        private void CreateConeBuffer()
        {
            /// TODO BONUS 1
        }

        /// <summary>
        /// Bind the quad-buffer (context switch!)
        /// call it in the render function
        /// minimize the number of calls
        /// </summary>
        private void BindQuadBuffer()
        {
            /// --- set quad context
            this.device.ImmediateContext.InputAssembler.InputLayout = this.deferredLightingVariables.screenGeometryLayout;
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.screenQuad.IndexBuffer, Format.R32_UInt, 0);
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.screenQuad.VertexBuffer, Vector4.SizeInBytes, 0));
        }

        /// <summary>
        /// Bind the sphere-buffer (context switch!)
        /// call it in the render function
        /// minimize the number of calls
        /// </summary>
        private void BindSphereBuffer()
        {
#if EXERCISE

            // TODO 2:
            //  - set sphere context

#else

            /// --- set sphere context
            this.device.ImmediateContext.InputAssembler.InputLayout = this.deferredLightingVariables.screenGeometryLayout;
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.screenSphere.IndexBuffer, Format.R32_UInt, 0);
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.screenSphere.VertexBuffer, Vector4.SizeInBytes, 0));

#endif

        }

        /// <summary>
        /// Bind the cone-buffer (context switch!)
        /// call it in the render function
        /// minimize the number of calls
        /// </summary>
        private void BindConeBuffer()
        {
#if EXERCISE

            // TODO BONUS 1

#else

            /// --- set cone context
            this.device.ImmediateContext.InputAssembler.InputLayout = this.deferredLightingVariables.screenGeometryLayout;
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.screenCone.IndexBuffer, Format.R32_UInt, 0);
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.screenCone.VertexBuffer, Vector4.SizeInBytes, 0));
#endif

        }

        /// <summary>
        /// Totaly clear the g-buffer
        /// </summary>
        private void ClearGBuffer()
        {
            Disposer.RemoveAndDispose(ref this.gBufferDepth);
            Disposer.RemoveAndDispose(ref this.gBufferDepthStencilView);

            /// cleanup buffer if any
            if (this.gBuffer != null)
            {
                for (int i = 0; i < gBuffer.Length; i++)
                {
                    var buf = gBuffer[i];
                    var view = gBufferRenderTargetView[i];
                    var res = gBufferShaderResourceView[i];
                    Disposer.RemoveAndDispose(ref view);
                    Disposer.RemoveAndDispose(ref res);
                    Disposer.RemoveAndDispose(ref buf);
                }
            }
        }

        /// <summary>
        /// CG is not working on DX11 objects, 
        /// so we have to cleanup here by ourseves
        /// </summary>
        public void Dispose()
        {
            this.ClearGBuffer();
            this.screenQuad.Dispose();
            this.screenSphere.Dispose();
            this.screenCone.Dispose();
            Disposer.RemoveAndDispose(ref this.deferredLightingVariables);
        }


        /// <summary>
        /// helper-struct that wrapps the VBO 
        /// for particular scene objects, 
        /// like quad, sphere, or cone for deferred lights
        /// </summary>
        private struct LightGeometryData : IDisposable
        {
            public Buffer VertexBuffer;
            public Buffer IndexBuffer;
            public int IndexCount;

            public void Dispose()
            {
                Disposer.RemoveAndDispose(ref this.VertexBuffer);
                Disposer.RemoveAndDispose(ref this.IndexBuffer);
            }
        }

        /// <summary>
        /// Helper-class which wrapps all parameters needed for deferred lighting
        /// </summary>
        private class DeferredLightingVariables : System.IDisposable
        {
            public DeferredLightingVariables(EffectsManager effectsContainer)
            {
                this.screenGeometryEffect = effectsContainer.GetEffect(Techniques.RenderDeferredLighting);
                this.screenGeometryLayout = effectsContainer.GetLayout(Techniques.RenderDeferredLighting);
                this.screenGeometryTechnique = screenGeometryEffect.GetTechniqueByName(Techniques.RenderDeferredLighting);

                this.renderPassAmbient = this.screenGeometryTechnique.GetPassByName("AmbientPass");
                this.renderPassDirectionalLight = this.screenGeometryTechnique.GetPassByName("DirectionalLightPass");
                this.renderPassPointLight = this.screenGeometryTechnique.GetPassByName("PointLightPass");
                this.renderPassSpotLight = this.screenGeometryTechnique.GetPassByName("SpotLightPass");

                this.vLightDir = this.screenGeometryEffect.GetVariableByName("vLightDir").AsVector();
                this.vLightPos = this.screenGeometryEffect.GetVariableByName("vLightPos").AsVector();
                this.vLightAtt = this.screenGeometryEffect.GetVariableByName("vLightAtt").AsVector();
                this.vLightSpot = this.screenGeometryEffect.GetVariableByName("vLightSpot").AsVector();
                this.vLightColor = this.screenGeometryEffect.GetVariableByName("vLightColor").AsVector();
                this.vLightAmbient = this.screenGeometryEffect.GetVariableByName("vLightAmbient").AsVector();

                this.vEyePos = this.screenGeometryEffect.GetVariableByName("vEyePos").AsVector();
                this.mLightModel = this.screenGeometryEffect.GetVariableByName("mLightModel").AsMatrix();
                this.mLightView = this.screenGeometryEffect.GetVariableByName("mLightView").AsMatrix();
                this.mLightProj = this.screenGeometryEffect.GetVariableByName("mLightProj").AsMatrix();

                this.gDepthStencilShaderResourceVariables = this.screenGeometryEffect.GetVariableByName("DepthTexture").AsShaderResource();
                this.gBufferShaderResourceVariables = new EffectShaderResourceVariable[4];
                this.gBufferShaderResourceVariables[0] = this.screenGeometryEffect.GetVariableByName("NormalTexture").AsShaderResource();
                this.gBufferShaderResourceVariables[1] = this.screenGeometryEffect.GetVariableByName("DiffuseAlbedoTexture").AsShaderResource();
                this.gBufferShaderResourceVariables[2] = this.screenGeometryEffect.GetVariableByName("SpecularAlbedoTexture").AsShaderResource();
                this.gBufferShaderResourceVariables[3] = this.screenGeometryEffect.GetVariableByName("PositionTexture").AsShaderResource();
            }

            // effect variables
            public Effect screenGeometryEffect;
            public InputLayout screenGeometryLayout;

            public EffectPass renderPassAmbient;
            public EffectPass renderPassDirectionalLight;
            public EffectPass renderPassPointLight;
            public EffectPass renderPassSpotLight;
            public EffectTechnique screenGeometryTechnique;

            public EffectVectorVariable vLightDir;
            public EffectVectorVariable vLightPos;
            public EffectVectorVariable vLightAtt;
            public EffectVectorVariable vLightSpot;        //(outer angle , inner angle, falloff, light-range!!!)            
            public EffectVectorVariable vLightColor;
            public EffectVectorVariable vLightAmbient;
            public EffectVectorVariable vEyePos;
            public EffectMatrixVariable mLightModel;
            public EffectMatrixVariable mLightView;
            public EffectMatrixVariable mLightProj;

            public EffectShaderResourceVariable[] gBufferShaderResourceVariables;
            public EffectShaderResourceVariable gDepthStencilShaderResourceVariables;

            public void Dispose()
            {
                Disposer.RemoveAndDispose(ref this.screenGeometryTechnique);
                Disposer.RemoveAndDispose(ref this.renderPassAmbient);
                Disposer.RemoveAndDispose(ref this.renderPassPointLight);
                Disposer.RemoveAndDispose(ref this.renderPassDirectionalLight);

                Disposer.RemoveAndDispose(ref this.vLightDir);
                Disposer.RemoveAndDispose(ref this.vLightPos);
                Disposer.RemoveAndDispose(ref this.vLightAtt);
                Disposer.RemoveAndDispose(ref this.vLightSpot);
                Disposer.RemoveAndDispose(ref this.vLightColor);
                Disposer.RemoveAndDispose(ref this.vEyePos);
                Disposer.RemoveAndDispose(ref this.mLightModel);
                Disposer.RemoveAndDispose(ref this.mLightView);
                Disposer.RemoveAndDispose(ref this.mLightProj);

                if (this.gBufferShaderResourceVariables != null)
                {
                    for (int i = 0; i < this.gBufferShaderResourceVariables.Length; i++)
                    {
                        var buf = gBufferShaderResourceVariables[i];
                        Disposer.RemoveAndDispose(ref buf);
                    }
                }
                Disposer.RemoveAndDispose(ref this.gDepthStencilShaderResourceVariables);
            }
        }
    }
}
