namespace HelixToolkit.SharpDX.Wpf
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using global::SharpDX;
    using global::SharpDX.Direct3D11;
    //using Point3D = global::SharpDX.Vector3;



    /// <summary>
    /// Provides a base class for a scene model
    /// </summary>
    public abstract class Model3D : Element3D, ITransformable
    {
        /// <summary>
        /// This is a hack model matrix. It is always pushed but
        /// never poped. It can be used to get the total model matrix
        /// in functions different than render or hittext, e.g., OnMouse3DMove        
        /// </summary>
        protected Matrix totalModelMatrix = Matrix.Identity;

        protected Matrix modelMatrix = Matrix.Identity;

        private Stack<Matrix> matrixStack = new Stack<Matrix>();

        public void PushMatrix(Matrix matrix)
        {
            matrixStack.Push(this.modelMatrix);
            this.modelMatrix = this.modelMatrix * matrix;
            this.totalModelMatrix = this.modelMatrix;
        }

        public void PopMatrix()
        {
            this.modelMatrix = matrixStack.Pop();
        }

        public Matrix ModelMatrix
        {
            get { return this.modelMatrix; }
        }


        ///// <summary>
        ///// The position of the model in world space.
        ///// </summary>
        //public Point3D Position
        //{
        //    get { return (Point3D)this.GetValue(PositionProperty); }
        //    protected set { this.SetValue(PositionProperty, value); }
        //}

        //public static readonly DependencyProperty PositionProperty =
        //    DependencyProperty.Register("Position", typeof(Point3D), typeof(Model3D), new FrameworkPropertyMetadata(new Point3D()));

        ////protected static readonly DependencyPropertyKey PositionPropertyKey =
        ////    DependencyProperty.RegisterReadOnly("Position", typeof(Point3D), typeof(Model3D), new UIPropertyMetadata(new Point3D()));

        ////public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;



        public Transform3D Transform
        {
            get { return (Transform3D)this.GetValue(TransformProperty); }
            set { this.SetValue(TransformProperty, value); }
        }

        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(Transform3D), typeof(Model3D), new FrameworkPropertyMetadata(Transform3D.Identity, TransformPropertyChanged));

        protected static void TransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Model3D)d).OnTransformChanged(e);
        }

        protected virtual void OnTransformChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.Transform != null)
            {
                var trafo = this.Transform.Value;                
                this.modelMatrix = trafo.ToMatrix();
            }
        }

        public class EffectTransformVariables : System.IDisposable
        {
            public EffectTransformVariables(Effect effect)
            {
                // openGL: uniform variables            
                mWorld = effect.GetVariableByName("mWorld").AsMatrix();
            }
            public EffectMatrixVariable mWorld;            
            public void Dispose()
            {
                mWorld.Dispose();
            }
        }
      

    }
}