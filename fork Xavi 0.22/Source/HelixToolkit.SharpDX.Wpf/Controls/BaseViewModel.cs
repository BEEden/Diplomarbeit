using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace HelixToolkit.SharpDX.Wpf
{
    /// <summary>
    /// Base ViewModel for Demo Applications?
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        
        public string SubTitle { get; set; }

        public string RenderTechnique { get; set; }
        
        public List<string> ShadingModelCollection { get; set; }
        
        public List<string> CameraModelCollection { get; set; }

        public string CameraModel
        {
            get { return this.cameraModel; }
            set { this.cameraModel = value; OnCameraModelChanged(); OnPropertyChanged("CameraModel"); }
        }

        public Camera Camera
        {
            get { return this.camera; }
            protected set
            {
                this.camera = value;
                this.cameraModel = value is PerspectiveCamera ? ProjectionCamera.Perspective : value is OrthographicCamera ? ProjectionCamera.Orthographic : null;
                OnPropertyChanged("CameraModel");
                OnPropertyChanged("Camera");
            }
        }

        protected OrthographicCamera defaultOrthographicCamera = new OrthographicCamera { Position = new Point3D(0, 0, 5), LookDirection = new Vector3D(-0, -0, -5), UpDirection = new Vector3D(0, 1, 0), NearPlaneDistance = 1, FarPlaneDistance = 100 };

        protected PerspectiveCamera defaultPerspectiveCamera = new PerspectiveCamera { Position = new Point3D(0, 0, 5), LookDirection = new Vector3D(-0, -0, -5), UpDirection = new Vector3D(0, 1, 0), NearPlaneDistance = 0.5, FarPlaneDistance = 150 };        

        public event EventHandler CameraModelChanged;

        public event PropertyChangedEventHandler PropertyChanged;


        protected BaseViewModel()
        {
            //// set lighting model            
            //ShadingModelCollection = new List<string>()
            //{
            //    Effects.RenderPhong,
            //    Effects.RenderBlinnPhong,
            //    Effects.RenderColors,
            //    Effects.RenderWires,
            //    Effects.RenderDeferred,
            //};

            // camera models
            CameraModelCollection = new List<string>()
            {
                ProjectionCamera.Orthographic,
                ProjectionCamera.Perspective,
            };

            // on camera changed callback
            this.CameraModelChanged += (s, e) =>
            {
                if (this.cameraModel == PerspectiveCamera.Orthographic)
                {
                    //if (this.Camera != null)
                    //{
                    //    var newCamera = new OrthographicCamera();
                    //    this.Camera.CopyTo(newCamera);
                    //    newCamera.NearPlaneDistance = znear;
                    //    newCamera.FarPlaneDistance = zfar;
                    //    this.Camera = newCamera;

                    //}
                    //else
                    {
                        this.Camera = this.defaultOrthographicCamera;
                    }
                }
                else if (this.cameraModel == PerspectiveCamera.Perspective)
                {
                    //if (this.Camera != null)
                    //{
                    //    var newCamera = new PerspectiveCamera();
                    //    this.Camera.CopyTo(newCamera);
                    //    newCamera.NearPlaneDistance = znear;
                    //    newCamera.FarPlaneDistance = zfar;
                    //    this.Camera = newCamera;
                    //}
                    //else
                    {
                        this.Camera = this.defaultPerspectiveCamera;
                    }
                }
                else
                {
                    throw new HelixToolkitException("Camera Model Error.");
                }
            };

            // default camera model
            this.CameraModel = ProjectionCamera.Perspective;            

            this.Title = "Demo (HelixToolkitDX)";
            this.SubTitle = "Default Base View Model";
            this.RenderTechnique = Techniques.RenderPhong;
        }

        protected virtual void OnCameraModelChanged()
        {
            var eh = this.CameraModelChanged;
            if (eh != null)
            {
                eh(this, new EventArgs());
            }
        }

        protected void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        private string cameraModel;
        
        private Camera camera;
    }
}
