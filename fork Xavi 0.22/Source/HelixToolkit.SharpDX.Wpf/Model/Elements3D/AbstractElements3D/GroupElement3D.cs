using System.Collections.Generic;
using System.Windows;
namespace HelixToolkit.SharpDX.Wpf
{
    public abstract class GroupElement3D : Element3D, IElement3DCollection
    {

        public Element3DCollection Children
        {
            get { return (Element3DCollection)this.GetValue(ChildrenProperty); }
            set { this.SetValue(ChildrenProperty, value); }
        }

        public static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register("Children", typeof(Element3DCollection), typeof(GroupElement3D), new UIPropertyMetadata(new Element3DCollection()));

        public GroupElement3D()
        {            
        }

        public override void Attach(IRenderHost host)
        {
            base.Attach(host);
            foreach (var c in this.Children)
            {
                c.Attach(host);
            }
        }

        public override void Detach()
        {
            base.Detach();
            foreach (var c in this.Children)
            {
                c.Detach();
            }
        }

        public override void Render(RenderContext context)
        {
            base.Render(context);
            foreach (var c in this.Children)
            {
                c.Render(context);
            }
        }
    }
}