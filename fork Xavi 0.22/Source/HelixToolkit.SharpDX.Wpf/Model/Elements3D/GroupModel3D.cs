using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using SharpDX;
namespace HelixToolkit.SharpDX.Wpf
{
    public class GroupModel3D : GroupElement3D, ITransformable, IHitable
    {
        private Stack<Matrix> matrixStack = new Stack<Matrix>();

        protected Matrix modelMatrix = Matrix.Identity;
       
        public Matrix ModelMatrix
        {
            get { return this.modelMatrix; }
        }

        public void PushMatrix(Matrix matrix)
        {
            matrixStack.Push(this.modelMatrix);
            this.modelMatrix = this.modelMatrix * matrix;
        }

        public void PopMatrix()
        {
            this.modelMatrix = matrixStack.Pop();
        }
      

        /// <summary>
        /// The position of the model in world space.
        /// </summary>
        public Point3D Position
        {
            get { return (Point3D)this.GetValue(PositionProperty); }
            private set { this.SetValue(PositionPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey PositionPropertyKey =
            DependencyProperty.RegisterReadOnly("Position", typeof(Point3D), typeof(GroupModel3D), new UIPropertyMetadata(new Point3D()));

        public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

        public Transform3D Transform
        {
            get { return (Transform3D)this.GetValue(TransformProperty); }
            set { this.SetValue(TransformProperty, value); }
        }

        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(Transform3D), typeof(GroupModel3D), new UIPropertyMetadata(Transform3D.Identity, TransformPropertyChanged));

        private static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GroupModel3D)d).OnTransformChanged(e);
        }


        protected virtual void OnTransformChanged(DependencyPropertyChangedEventArgs e)
        {
            var trafo = this.Transform.Value;
            this.Position = new Point3D(trafo.OffsetX, trafo.OffsetY, trafo.OffsetZ);
            this.modelMatrix = trafo.ToMatrix();
        }

        public override void Attach(IRenderHost host)
        {
            this.effectName = host.RenderTechnique.ToString();
            base.Attach(host);
        }

        public override void Render(RenderContext context)
        {
            foreach (var c in this.Children)
            {
                var model = c as ITransformable;
                if (model != null)
                {
                    // push matrix                    
                    model.PushMatrix(this.modelMatrix);
                    // render model
                    c.Render(context);
                    // pop matrix                   
                    model.PopMatrix();
                }
                else
                {
                    c.Render(context);
                }
            }
        }

        public virtual bool HitTest(Ray ray, ref List<HitTestResult> hits)
        {
            bool hit = false;
            foreach (var c in this.Children)
            {
                var hc = c as IHitable;
                if (hc != null)
                {
                    var tc = c as ITransformable;
                    if (tc != null)
                    {
                        tc.PushMatrix(this.modelMatrix);
                        if (hc.HitTest(ray, ref hits))
                        {
                            hit = true;
                        }
                        tc.PopMatrix();
                    }
                    else
                    {
                        if (hc.HitTest(ray, ref hits))
                        {
                            hit = true;
                        }
                    }
                }
            }            
            return hit;
        }        
    }
}