namespace HelixToolkit.SharpDX.Wpf
{
    using global::SharpDX;
    using System;
    using System.Windows.Controls;

    public interface IRenderable
    {        
        void Attach(IRenderHost host);
        void Detach();
        void Update(TimeSpan timeSpan);
        void Render(RenderContext context);
        
        bool IsShadowMappingEnabled { get; }
        string RenderTechnique { get; }
        ItemCollection Items { get; }
        Camera Camera { get; }
        Color4 BackgroundColor { get; }
    }
}