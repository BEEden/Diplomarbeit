namespace HelixToolkit.SharpDX.Wpf
{    
    using global::SharpDX;    
    using global::SharpDX.Direct3D11;
    using System.Windows.Media.Media3D;
    using System.Runtime.InteropServices;
    using System;
    using System.Windows;

    public sealed class PointLight3D : PointLightBase3D
    {
        public PointLight3D()
        {            
            this.LightType = Wpf.LightType.Point;
        }
        
        public override void Attach(IRenderHost host)
        {
            /// --- attach
            base.Attach(host);

            /// --- light constant params            
            this.vLightPos = this.effect.GetVariableByName("vLightPos").AsVector();
            this.vLightColor = this.effect.GetVariableByName("vLightColor").AsVector();
            this.vLightAtt = this.effect.GetVariableByName("vLightAtt").AsVector();
            this.iLightType = this.effect.GetVariableByName("iLightType").AsScalar();                        

            /// --- Set light type
            lightTypes[lightIndex] = (int)Light3D.Type.Point;   

            /// --- flush
            this.device.ImmediateContext.Flush();
        }

        public override void Detach()
        {            
            Disposer.RemoveAndDispose(ref this.vLightPos);
            Disposer.RemoveAndDispose(ref this.vLightColor);
            Disposer.RemoveAndDispose(ref this.vLightAtt);
            Disposer.RemoveAndDispose(ref this.iLightType);
            base.Detach();       
        }

        public override void Render(RenderContext context)
        {
            if (renderHost.RenderTechnique == Techniques.RenderDeferred || renderHost.RenderTechnique == Techniques.RenderGBuffer)
            {
                return;
            } 

            /// --- Set lighting parameters
            lightPositions[lightIndex] = this.Position.ToVector4();
            lightColors[lightIndex] = this.Color;            
            lightAtt[lightIndex] = new Vector4((float)this.Attenuation.X, (float)this.Attenuation.Y, (float)this.Attenuation.Z, (float)this.Range);

            /// --- Update lighting variables    
            this.vLightPos.Set(lightPositions);
            this.vLightColor.Set(lightColors);
            this.vLightAtt.Set(lightAtt);
            this.iLightType.Set(lightTypes);
        }
    }
}