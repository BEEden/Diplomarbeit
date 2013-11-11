namespace HelixToolkit.SharpDX.Wpf
{
    using global::SharpDX;
    using global::SharpDX.Direct3D11;
    using System.Windows.Media.Media3D;
    using System.Runtime.InteropServices;
    using System;
    using System.Windows;

    public sealed class DirectionalLight3D : Light3D
    {        
        public DirectionalLight3D()
        {            
            this.Color = global::SharpDX.Color.White;
            this.LightType = Wpf.LightType.Directional;
        }
        
        public override void Attach(IRenderHost host)
        {
            /// --- attach
            base.Attach(host);

            /// --- light constant params            
            this.vLightDir = this.effect.GetVariableByName("vLightDir").AsVector();
            this.vLightColor = this.effect.GetVariableByName("vLightColor").AsVector();
            this.iLightType = this.effect.GetVariableByName("iLightType").AsScalar();

            /// --- Set light type
            lightTypes[lightIndex] = (int)Light3D.Type.Directional;

            /// --- flush
            this.device.ImmediateContext.Flush();
        }

        public override void Detach()
        {
            Disposer.RemoveAndDispose(ref this.vLightDir);
            Disposer.RemoveAndDispose(ref this.vLightColor);
            Disposer.RemoveAndDispose(ref this.iLightType);
            base.Detach();
        }

        public override void Render(RenderContext context)
        {
            if (renderHost.RenderTechnique == Techniques.RenderDeferred || renderHost.RenderTechnique ==  Techniques.RenderGBuffer)
            {
                return;
            }            

            /// --- Setup lighting parameters
            lightDirections[lightIndex] = -this.Direction.ToVector4();
            lightColors[lightIndex] = this.Color;
            
            /// --- Update lighting variables                
            this.vLightDir.Set(lightDirections);
            this.vLightColor.Set(lightColors);
            this.iLightType.Set(lightTypes);

            /// --- if shadow-map enabled
            if (this.renderHost.IsShadowMapEnabled)
            {                
                /// update shader
                this.mLightView.SetMatrix(lightViewMatrices);
                this.mLightProj.SetMatrix(lightProjMatrices);
            }
        }
    }
}