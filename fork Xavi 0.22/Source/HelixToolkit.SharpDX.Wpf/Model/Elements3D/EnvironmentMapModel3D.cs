namespace HelixToolkit.SharpDX.Wpf
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using global::SharpDX;
    using global::SharpDX.DXGI;
    using global::SharpDX.Direct3D;
    using global::SharpDX.Direct3D11;
    using Direct3D = global::SharpDX.Direct3D;
    using Texture2D = global::SharpDX.Direct3D11.Texture2D;
    using Image = global::SharpDX.Toolkit.Graphics.Image;
    using Matrix = global::SharpDX.Matrix;


    public sealed class EnvironmentMap3D : Model3D
    {
        // members to dispose          
        private Buffer vertexBuffer;
        private Buffer indexBuffer;
        private InputLayout vertexLayout;
        private EffectTechnique technique;

        private ShaderResourceView texCubeMapView;
        private EffectShaderResourceVariable texCubeMap;
        private EffectScalarVariable bHasCubeMap;

        private RasterizerState rasterState;
        private DepthStencilState depthStencilState;

        private EffectTransformVariables effectTransforms;
        private MeshGeometry3D geometry;

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty FilenameProperty =
            DependencyProperty.Register("Filename", typeof(string), typeof(EnvironmentMap3D), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the current environment map texture image
        /// (expects DDS Environment Map image)
        /// </summary>
        public string Filename
        {
            get { return (string)this.GetValue(FilenameProperty); }
            set { this.SetValue(FilenameProperty, value); }
        }

        /// <summary>
        /// Indicates, if this element is active, if not, the model will be not 
        /// rendered and not reflected.
        /// default is true.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(Element3D), new UIPropertyMetadata(true, IsActiveChanged));

        /// <summary>
        /// Indicates, if this element is active, if not, the model will be not 
        /// rendered and not reflected.
        /// default is true.
        /// </summary>
        public bool IsActive
        {
            get { return (bool)this.GetValue(IsActiveProperty); }
            set { this.SetValue(IsActiveProperty, value); }
        }

        private static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = ((EnvironmentMap3D)d);
            if (obj.isAttached)
                obj.bHasCubeMap.Set((bool)e.NewValue);
        }

        public override void Attach(IRenderHost host)
        {
            /// --- attach
            this.effectName = Techniques.RenderCubeMap;
            base.Attach(host);

            /// --- get variables               
            this.vertexLayout = host.Effects.GetLayout(this.effectName);
            this.technique = effect.GetTechniqueByName(this.effectName);
            this.effectTransforms = new EffectTransformVariables(this.effect);

            /// -- attach cube map 
            if (this.Filename != null)
            {
                /// -- attach texture
                using (var texture = Texture2D.FromFile<Texture2D>(this.device, this.Filename))
                {
                    this.texCubeMapView = new ShaderResourceView(this.device, texture);
                }
                this.texCubeMap = effect.GetVariableByName("texCubeMap").AsShaderResource();
                this.texCubeMap.SetResource(this.texCubeMapView);
                this.bHasCubeMap = effect.GetVariableByName("bHasCubeMap").AsScalar();
                this.bHasCubeMap.Set(true);

                /// --- set up geometry
                var sphere = new MeshBuilder(false,true,false);
                sphere.AddSphere(new Vector3(0, 0, 0));
                this.geometry = sphere.ToMeshGeometry3D();

                /// --- set up vertex buffer
                this.vertexBuffer = device.CreateBuffer(BindFlags.VertexBuffer, CubeVertex.SizeInBytes, this.geometry.Positions.Select((x, ii) => new CubeVertex() { Position = new Vector4(x, 1f) }).ToArray());

                /// --- set up index buffer
                this.indexBuffer = device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), geometry.Indices);

                /// --- set up rasterizer states
                var rasterStateDesc = new RasterizerStateDescription()
                {
                    FillMode = FillMode.Solid,
                    CullMode = CullMode.Back,
                    IsMultisampleEnabled = true,
                    IsAntialiasedLineEnabled = true,
                    IsFrontCounterClockwise = false,
                };
                this.rasterState = new RasterizerState(this.device, rasterStateDesc);

                /// --- set up depth stencil state
                var depthStencilDesc = new DepthStencilStateDescription()
                {
                    DepthComparison = Comparison.LessEqual,
                    DepthWriteMask = global::SharpDX.Direct3D11.DepthWriteMask.All,
                    IsDepthEnabled = true,
                };
                this.depthStencilState = new DepthStencilState(this.device, depthStencilDesc);
            }

            /// --- flush
            this.device.ImmediateContext.Flush();
        }

        public override void Detach()
        {
            if (!this.isAttached)
                return;

            this.bHasCubeMap.Set(false);

            this.effectName = null;
            this.technique = null;
            this.vertexLayout = null;
            this.geometry = null;

            Disposer.RemoveAndDispose(ref this.vertexBuffer);
            Disposer.RemoveAndDispose(ref this.indexBuffer);
            Disposer.RemoveAndDispose(ref this.texCubeMap);
            Disposer.RemoveAndDispose(ref this.texCubeMapView);
            Disposer.RemoveAndDispose(ref this.bHasCubeMap);
            Disposer.RemoveAndDispose(ref this.rasterState);
            Disposer.RemoveAndDispose(ref this.depthStencilState);
            Disposer.RemoveAndDispose(ref this.effectTransforms);

            base.Detach();
        }

        public override void Render(RenderContext context)
        {
            if (!this.IsRendering) return;

            /// --- set context
            this.device.ImmediateContext.InputAssembler.InputLayout = this.vertexLayout;
            this.device.ImmediateContext.InputAssembler.PrimitiveTopology = Direct3D.PrimitiveTopology.TriangleList;
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, CubeVertex.SizeInBytes, 0));

            this.device.ImmediateContext.Rasterizer.State = rasterState;
            this.device.ImmediateContext.OutputMerger.DepthStencilState = depthStencilState;

            /// --- set constant paramerers 
            var worldMatrix = Matrix.Translation(((PerspectiveCamera)context.Camera).Position.ToVector3());
            this.effectTransforms.mWorld.SetMatrix(ref worldMatrix);

            /// --- render the geometry
            this.technique.GetPassByIndex(0).Apply(device.ImmediateContext);
            this.device.ImmediateContext.DrawIndexed(this.geometry.Indices.Length, 0, 0);
        }
    }
}