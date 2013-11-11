using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

namespace HelixToolkit.SharpDX.Wpf
{

    /// <summary>
    /// Example class how to implement mouse dragging for objects.
    /// Probably it should be moved to a "Dragging Demo."
    /// </summary>
    public class DraggableGeometryModel3D : MeshGeometryModel3D
    {
        protected bool isCaptured;
        protected Viewport3DX viewport;
        protected Camera camera;
        protected Point3D lastHitPos;

        public override void OnMouse3DDown(object sender, RoutedEventArgs e)
        {
            base.OnMouse3DDown(sender, e);

            var args = e as Mouse3DEventArgs;
            if (args == null) return;
            if (args.Viewport == null) return;

            this.isCaptured = true;
            this.viewport = args.Viewport;
            this.camera = args.Viewport.Camera;
            this.lastHitPos = args.HitTestResult.PointHit;
        }

        public override void OnMouse3DUp(object sender, RoutedEventArgs e)
        {
            base.OnMouse3DUp(sender, e);
            if (this.isCaptured)
            {
                this.isCaptured = false;
                this.camera = null;
                this.viewport = null;
            }
        }

        public override void OnMouse3DMove(object sender, RoutedEventArgs e)
        {
            base.OnMouse3DMove(sender, e);
            if (this.isCaptured)
            {
                var args = e as Mouse3DEventArgs;

                // move dragmodel                         
                var normal = this.camera.LookDirection;

                // hit position                        
                var newHit = this.viewport.UnProjectOnPlane(args.Position, lastHitPos, normal);
                if (newHit.HasValue)
                {
                    var offset = (newHit.Value - lastHitPos);
                    var dragTrafo = new TranslateTransform3D((Vector3D)this.ModelMatrix.TranslationVector.ToVector3D() + offset);
                    this.Transform = dragTrafo;
                    this.lastHitPos = newHit.Value;
                }
            }
        }
    }
}
