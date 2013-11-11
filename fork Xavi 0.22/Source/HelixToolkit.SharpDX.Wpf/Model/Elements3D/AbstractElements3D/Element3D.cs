namespace HelixToolkit.SharpDX.Wpf
{
    using System;
    using System.Windows;

    /// <summary>
    /// Base class for renderable elements.
    /// </summary>    
    public abstract class Element3D : FrameworkElement
    {
        protected bool isAttached = false;

        protected string effectName;

        protected global::SharpDX.Direct3D11.Device device;

        protected global::SharpDX.Direct3D11.Effect effect;

        protected IRenderHost renderHost;

        /// <summary>
        /// Attaches the element to the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        public virtual void Attach(IRenderHost host)
        {
            this.isAttached = true;
            this.device = host.Device;
            this.effect = host.Effects[effectName];
            this.renderHost = host;
        }

        /// <summary>
        /// Detaches the element from the host.
        /// </summary>
        public virtual void Detach()
        {
            
            this.isAttached = false;
            //this.device = null;
            //this.effect = null;
            //this.renderHost = null;
        }

        /// <summary>
        /// Updates the element by the specified time span.
        /// </summary>
        /// <param name="timeSpan">The time since last update.</param>
        public virtual void Update(TimeSpan timeSpan) { }

        /// <summary>
        /// Renders the element in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Render(RenderContext context)
        {
        }

        /// <summary>
        /// Indicates, if this element should be rendered,
        /// default is true
        /// </summary>
        public static readonly DependencyProperty IsRenderingProperty =
            DependencyProperty.Register("IsRendering", typeof(bool), typeof(Element3D), new UIPropertyMetadata(true));

        /// <summary>
        /// Indicates, if this element should be rendered.
        /// Use this also to make the model visible/unvisible
        /// default is true
        /// </summary>
        public bool IsRendering
        {
            get { return (bool)this.GetValue(IsRenderingProperty); }
            set { this.SetValue(IsRenderingProperty, value); }
        }
    }
}