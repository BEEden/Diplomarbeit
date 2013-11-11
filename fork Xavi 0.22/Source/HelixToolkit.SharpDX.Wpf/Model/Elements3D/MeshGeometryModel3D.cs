namespace HelixToolkit.SharpDX.Wpf
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using global::SharpDX;
    using global::SharpDX.Direct3D11;
    using global::SharpDX.DXGI;
    using global::SharpDX.Direct3D;


    public class MeshGeometryModel3D : MaterialGeometryModel3D
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="depthBias"></param>
        protected override void OnRasterStateChanged(int depthBias)
        {
            if (this.isAttached)
            {
                Disposer.RemoveAndDispose(ref this.rasterState);
                /// --- set up rasterizer states
                var rasterStateDesc = new RasterizerStateDescription()
                {
                    FillMode = FillMode.Solid,
                    CullMode = CullMode.Back,
                    DepthBias = depthBias,
                    DepthBiasClamp = -1000,
                    SlopeScaledDepthBias = +0,                   
                    IsDepthClipEnabled = true,
                    IsFrontCounterClockwise = true,
                   
                    //IsMultisampleEnabled = true,
                    //IsAntialiasedLineEnabled = true,                    
                    //IsScissorEnabled = true,
                };
                this.rasterState = new RasterizerState(this.device, rasterStateDesc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        public override void Attach(IRenderHost host)
        {
            /// --- attach
            this.effectName = host.RenderTechnique.ToString();
            base.Attach(host);

            // --- get variables
            this.vertexLayout = host.Effects.GetLayout(this.effectName);
            this.shaderTechnique = effect.GetTechniqueByName(this.effectName);
            this.geometry = this.Geometry as MeshGeometry3D;

            if (this.geometry == null)
            {
                throw new HelixToolkitException("Geometry not found!");
            }
                        
            /// --- transformations
            this.effectTransforms = new EffectTransformVariables(this.effect);

            /// --- material 
            this.AttachMaterial();

            /// scale texcoords
            var texScale = TextureCoodScale;

            /// --- init vertex buffer
            this.vertexBuffer = device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, this.geometry.Positions.Select((x, ii) => new DefaultVertex()
            {
                Position   = new Vector4(x, 1f),
                Color      = this.geometry.Colors == null     ? new Color4(1f, 1f, 1f, 1f) : this.geometry.Colors[ii],
                TexCoord   = this.geometry.TextureCoordinates == null ? new Vector2() : texScale*this.geometry.TextureCoordinates[ii],
                Normal     = this.geometry.Normals == null    ? new Vector3() : this.geometry.Normals[ii],
                Tangent    = this.geometry.Tangents != null   ? this.geometry.BiTangents[ii] : new Vector3(),
                BiTangent  = this.geometry.BiTangents != null ? this.geometry.BiTangents[ii] : new Vector3(),
            }).ToArray());

            /// --- init index buffer
            this.indexBuffer = device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), this.geometry.Indices);

            /// --- init instances buffer            
            this.hasInstances = this.Instances != null;            
            this.bHasInstances = this.effect.GetVariableByName("bHasInstances").AsScalar();
            if (this.hasInstances)
            {                
                this.instanceBuffer = Buffer.Create(this.device, this.instanceArray, new BufferDescription(Matrix.SizeInBytes * this.instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));                            
            }

            /// --- set rasterstate
            this.OnRasterStateChanged(this.DepthBias);

            /// --- flush
            this.device.ImmediateContext.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Detach()
        {
            Disposer.RemoveAndDispose(ref this.vertexBuffer);            
            Disposer.RemoveAndDispose(ref this.indexBuffer);
            Disposer.RemoveAndDispose(ref this.instanceBuffer);
            Disposer.RemoveAndDispose(ref this.effectMaterial);
            Disposer.RemoveAndDispose(ref this.effectTransforms);
            Disposer.RemoveAndDispose(ref this.texDiffuseMapView);
            Disposer.RemoveAndDispose(ref this.texNormalMapView);
            Disposer.RemoveAndDispose(ref this.bHasInstances);

            this.effectName = null;
            this.phongMaterial = null;
            this.geometry = null;            
            this.shaderTechnique = null;
            this.vertexLayout = null;

            base.Detach();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Render(RenderContext renderContext)
        {
            /// --- check to render the model
            {
                if (!this.IsRendering)
                    return;

                if (this.Geometry == null)
                    return;

                if (this.Visibility != System.Windows.Visibility.Visible)
                    return;

                if (renderContext.IsShadowPass)
                    if (!this.IsThrowingShadow)
                        return;
            }

            /// --- set constant paramerers             
            var worldMatrix = this.modelMatrix * renderContext.worldMatrix;
            this.effectTransforms.mWorld.SetMatrix(ref worldMatrix);

            /// --- check shadowmaps
            this.hasShadowMap = this.renderHost.IsShadowMapEnabled;
            this.effectMaterial.bHasShadowMapVariable.Set(this.hasShadowMap);

            /// --- set material params      
            if (phongMaterial != null)
            {
                this.effectMaterial.vMaterialDiffuseVariable.Set(phongMaterial.DiffuseColor);
                this.effectMaterial.vMaterialAmbientVariable.Set(phongMaterial.AmbientColor);
                this.effectMaterial.vMaterialEmissiveVariable.Set(phongMaterial.EmissiveColor);
                this.effectMaterial.vMaterialSpecularVariable.Set(phongMaterial.SpecularColor);
                this.effectMaterial.vMaterialReflectVariable.Set(phongMaterial.ReflectiveColor);
                this.effectMaterial.sMaterialShininessVariable.Set(phongMaterial.SpecularShininess);

                /// --- has samples              
                this.effectMaterial.bHasDiffuseMapVariable.Set(phongMaterial.DiffuseMap != null);
                this.effectMaterial.bHasNormalMapVariable.Set(phongMaterial.NormalMap != null);

                /// --- set samplers
                if (phongMaterial.DiffuseMap != null)
                {
                    this.effectMaterial.texDiffuseMapVariable.SetResource(this.texDiffuseMapView);
                }

                if (phongMaterial.NormalMap != null)
                {
                    this.effectMaterial.texNormalMapVariable.SetResource(this.texNormalMapView);
                }
            }

            /// --- check instancing
            this.hasInstances = this.Instances != null;
            this.bHasInstances.Set(this.hasInstances);

            /// --- set context
            this.device.ImmediateContext.InputAssembler.InputLayout = this.vertexLayout;
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            /// --- set rasterstate            
            this.device.ImmediateContext.Rasterizer.State = this.rasterState;

            if (this.hasInstances)
            {
                /// --- update instance buffer
                if (this.isChanged)
                {
                    this.instanceBuffer = Buffer.Create(this.device, this.instanceArray, new BufferDescription(Matrix.SizeInBytes * this.instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));                            
                    DataStream stream;
                    device.ImmediateContext.MapSubresource(this.instanceBuffer, MapMode.WriteDiscard, global::SharpDX.Direct3D11.MapFlags.None, out stream);
                    stream.Position = 0;
                    stream.WriteRange(this.instanceArray, 0, this.instanceArray.Length);
                    device.ImmediateContext.UnmapSubresource(this.instanceBuffer, 0);
                    stream.Dispose();
                    this.isChanged = false;
                }

                /// --- INSTANCING: need to set 2 buffers            
                this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new[] 
                {
                    new VertexBufferBinding(this.vertexBuffer, DefaultVertex.SizeInBytes, 0),
                    new VertexBufferBinding(this.instanceBuffer, Matrix.SizeInBytes, 0),
                });

                /// --- render the geometry
                this.shaderTechnique.GetPassByIndex(0).Apply(device.ImmediateContext);
                /// --- draw
                this.device.ImmediateContext.DrawIndexedInstanced(this.geometry.Indices.Length, this.instanceArray.Length, 0, 0, 0);

            }
            else
            {
                /// --- bind buffer                
                this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, DefaultVertex.SizeInBytes, 0));
                /// --- render the geometry
                this.shaderTechnique.GetPassByIndex(0).Apply(device.ImmediateContext);
                /// --- draw
                this.device.ImmediateContext.DrawIndexed(this.geometry.Indices.Length, 0, 0);                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            this.Detach();
        }
    }
}