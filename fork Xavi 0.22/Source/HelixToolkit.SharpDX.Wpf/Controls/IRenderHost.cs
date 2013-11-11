namespace HelixToolkit.SharpDX
{
    using global::SharpDX;
    using global::SharpDX.Direct3D11;
    using HelixToolkit.SharpDX.Wpf;
    
    public interface IRenderHost
    {
        Device Device { get; }
        EffectsManager Effects { get; }        
        Color4 ClearColor { get; }
        bool IsShadowMapEnabled { get; }
        //bool IsDeferredEnabled { get;  }
        bool IsMSAAEnabled { get;  }
        IRenderable Renderable { get; }
        void SetDefaultRenderTargets();

        /// <summary>
        /// This technique is used for the entire render pass 
        /// by all Element3D if not specified otherwise in
        /// the elements itself
        /// </summary>
        string RenderTechnique { get; }

        double ActualHeight { get; }
        double ActualWidth { get; }
    }
}