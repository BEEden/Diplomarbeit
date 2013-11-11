namespace HelixToolkit.SharpDX.Wpf
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using global::SharpDX;
    using global::SharpDX.Direct3D11;
    using global::SharpDX.DXGI;
    using global::SharpDX.Direct3D;
    using HelixToolkit.SharpDX.Wpf;
    using HelixToolkit.SharpDX;
    using System.IO;

#if DX11
    public static class PNTechniques
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly string[] shadingPasses = new[]
        {
            "Solid",
            "Wires",
            "Positions",
            "Normals",
            "TexCoords",
            "Tangents",
            "Colors",
        };

        private static readonly string[] shadingTechniques = new[]
        {
            Techniques.RenderPNTriangs,
            Techniques.RenderPNQuads,
        };

        /// <summary>
        /// Techniqes available for this Model3D
        /// </summary>
        public static IEnumerable<string> ShadingTechniques { get { return shadingTechniques; } }

        /// <summary>
        /// Passes available for this Model3D
        /// </summary>
        public static IEnumerable<string> ShadingPasses { get { return shadingPasses; } }
    }

    public class PNPatchGeometryModel3D : MaterialGeometryModel3D
    {


        /// <summary>
        /// 
        /// </summary>
        public string Shading
        {
            get { return (string)this.GetValue(ShadingProperty); }
            set { this.SetValue(ShadingProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty ShadingProperty =
            DependencyProperty.Register("Shading", typeof(string), typeof(PNPatchGeometryModel3D), new UIPropertyMetadata("Solid", ShadingChanged));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        protected static void ShadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (PNPatchGeometryModel3D)d;
            if (obj.isAttached)
            {
                var shadingPass = e.NewValue as string;
                if (PNTechniques.ShadingPasses.Contains(shadingPass))
                {
                    // --- change the pass
                    obj.shaderPass = obj.shaderTechnique.GetPassByName(shadingPass);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public double TessellationFactor
        {
            get { return (double)this.GetValue(TessellationFactorProperty); }
            set { this.SetValue(TessellationFactorProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty TessellationFactorProperty =
            DependencyProperty.Register("TessellationFactor", typeof(double), typeof(PNPatchGeometryModel3D), new UIPropertyMetadata(1.0, TessellationFactorChanged));

        /// <summary>
        /// 
        /// </summary>
        protected static void TessellationFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (PNPatchGeometryModel3D)d;
            if (obj.isAttached)
            {
                obj.vTessellationVariables.Set(new Vector4((float)obj.TessellationFactor, 0, 0, 0));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public PNPatchGeometryModel3D()
        {
            System.Console.WriteLine();

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

            // --- get the pass
            this.shaderPass = this.shaderTechnique.GetPassByName(this.Shading);

            // -- get geometry
            this.geometry = this.Geometry as MeshGeometry3D;


            if (this.geometry == null)
            {
                throw new HelixToolkitException("Geometry not found!");
            }

            /// --- model transformation
            this.effectTransforms = new EffectTransformVariables(this.effect);

            /// --- material 
            this.AttachMaterial();

            /// --- init vertex buffer
            this.vertexBuffer = device.CreateBuffer(BindFlags.VertexBuffer, DefaultVertex.SizeInBytes, this.geometry.Positions.Select((x, ii) => new DefaultVertex()
            {
                Position = new Vector4(x, 1f),
                Color = this.geometry.Colors != null ? this.geometry.Colors[ii] : new Color4(1f, 0f, 0f, 1f),
                TexCoord = this.geometry.TextureCoordinates != null ? this.geometry.TextureCoordinates[ii] : new Vector2(),
                Normal = this.geometry.Normals != null ? this.geometry.Normals[ii] : new Vector3(),
                Tangent = this.geometry.Tangents != null ? this.geometry.BiTangents[ii] : new Vector3(),
                BiTangent = this.geometry.BiTangents != null ? this.geometry.BiTangents[ii] : new Vector3(),
            }).ToArray());

            /// --- init index buffer
            this.indexBuffer = device.CreateBuffer(BindFlags.IndexBuffer, sizeof(int), this.geometry.Indices);

            ///// --- init instances buffer            
            //this.hasInstances = this.Instances != null;            
            //this.bHasInstances = this.effect.GetVariableByName("bHasInstances").AsScalar();
            //if (this.hasInstances)
            //{                
            //    this.instanceBuffer = Buffer.Create(this.device, this.instanceArray, new BufferDescription(Matrix.SizeInBytes * this.instanceArray.Length, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0));                            
            //}

            /// --- init tessellation vars
            this.vTessellationVariables = effect.GetVariableByName("vTessellation").AsVector();
            this.vTessellationVariables.Set(new Vector4((float)this.TessellationFactor, 0, 0, 0));

            /// --- flush
            this.device.ImmediateContext.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Detach()
        {
            base.Detach();
        }

        /// <summary>
        /// 
        /// </summary>        
        public override void Update(System.TimeSpan timeSpan)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Render(RenderContext renderContext)
        {
            /// --- check if to render the model
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

            /// --- set model transform paramerers                         
            this.effectTransforms.mWorld.SetMatrix(ref this.modelMatrix);

            /// --- set material props
            if (phongMaterial != null)
            {
                /// --- set material lighting-params      
                this.effectMaterial.vMaterialDiffuseVariable.Set(phongMaterial.DiffuseColor);
                this.effectMaterial.vMaterialAmbientVariable.Set(phongMaterial.AmbientColor);
                this.effectMaterial.vMaterialEmissiveVariable.Set(phongMaterial.EmissiveColor);
                this.effectMaterial.vMaterialSpecularVariable.Set(phongMaterial.SpecularColor);
                this.effectMaterial.vMaterialReflectVariable.Set(phongMaterial.ReflectiveColor);
                this.effectMaterial.sMaterialShininessVariable.Set(phongMaterial.SpecularShininess);

                /// --- set samplers boolean flags
                this.effectMaterial.bHasDiffuseMapVariable.Set(phongMaterial.DiffuseMap != null && this.RenderDiffuseMap);
                this.effectMaterial.bHasNormalMapVariable.Set(phongMaterial.NormalMap != null && this.RenderNormalMap);
                this.effectMaterial.bHasDisplacementMapVariable.Set(phongMaterial.DisplacementMap != null && this.RenderDisplacementMap);

                /// --- set samplers
                if (phongMaterial.DiffuseMap != null)
                {
                    this.effectMaterial.texDiffuseMapVariable.SetResource(this.texDiffuseMapView);
                }
                if (phongMaterial.NormalMap != null)
                {
                    this.effectMaterial.texNormalMapVariable.SetResource(this.texNormalMapView);
                }
                if (phongMaterial.DisplacementMap != null)
                {
                    this.effectMaterial.texDisplacementMapVariable.SetResource(this.texDisplacementMapView);
                }
            }

            /// --- set primitive type
            if (this.effectName == Techniques.RenderPNTriangs)
            {
                this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith3ControlPoints;
            }
            else if (this.effectName == Techniques.RenderPNQuads)
            {
                this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith4ControlPoints;
            }
            else
            {
                this.device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            }

            /// --- set vertex layout
            this.device.ImmediateContext.InputAssembler.InputLayout = this.vertexLayout;

            /// --- set index buffer
            this.device.ImmediateContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            /// --- set vertex buffer                
            this.device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, DefaultVertex.SizeInBytes, 0));

            /// --- apply chosen pass
            this.shaderPass.Apply(device.ImmediateContext);

            /// --- render the geometry
            this.device.ImmediateContext.DrawIndexed(this.geometry.Indices.Length, 0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            this.Detach();
        }


        private EffectVectorVariable vTessellationVariables;
        private EffectPass shaderPass;
    } 
#endif
}