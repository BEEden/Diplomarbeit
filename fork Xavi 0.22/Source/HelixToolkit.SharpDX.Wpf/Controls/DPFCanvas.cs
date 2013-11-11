#define MSAA

namespace HelixToolkit.SharpDX.Wpf
{    
    using System.Collections.Generic;   
    using System.ComponentModel;
    using System.Linq;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using global::SharpDX;
    using global::SharpDX.DXGI;
    using global::SharpDX.Direct3D11;
    using global::SharpDX.D3DCompiler;
    using global::SharpDX.Direct3D; 

    using Direct3D = global::SharpDX.Direct3D;
    using Direct3D11 = global::SharpDX.Direct3D11;
    using Device = global::SharpDX.Direct3D11.Device;       
     

    // ---- BASED ON ORIGNAL CODE FROM -----
    // Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
    // 
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    // 
    // The above copyright notice and this permission notice shall be included in
    // all copies or substantial portions of the Software.
    // 
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    // THE SOFTWARE.

    public class DPFCanvas : Image, IRenderHost
    {
        private Device device;
        private Texture2D renderTarget;
        private Texture2D depthStencil;
        private RenderTargetView renderTargetView;
        private DepthStencilView depthStencilView;
        private DX11ImageSource surfaceD3D;
        private Stopwatch renderTimer;
        private IRenderable renderRenderable;
        private RenderContext renderContext;
        private EffectsManager effects;
        private DeferredRenderer deferredRenderer;
        private bool sceneAttached;        
        private int targetWidth, targetHeight;

#if MSAA
        private Texture2D renderTargetNMS;
#endif


        /// <summary>
        /// 
        /// </summary>
        public Color4 ClearColor { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsShadowMapEnabled { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsMSAAEnabled { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string RenderTechnique { get; private set; }

        /// <summary>
        /// The instance of currently attached IRenderable - this is in general the Viewport3DX
        /// </summary>
        public IRenderable Renderable
        {
            get
            {
                return this.renderRenderable;
            }
            set
            {
                if (ReferenceEquals(this.renderRenderable, value))
                {
                    return;
                }

                if (this.renderRenderable != null)
                {
                    this.renderRenderable.Detach();
                    this.renderRenderable = null;
                }

                this.sceneAttached = false;
                this.renderRenderable = value;
            }
        }

        /// <summary>
        /// The currently used Direct3D Device
        /// </summary>
        Device IRenderHost.Device
        {
            get { return this.device; }
        }

        /// <summary>
        /// The currently used Effects class, that is basically an effect manager
        /// </summary>
        EffectsManager IRenderHost.Effects
        {
            get { return this.effects; }
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode
        /// (running in Blend or Visual Studio).
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                var prop = DesignerProperties.IsInDesignModeProperty;
                return (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static Adapter GetBestAdapter()
        {
            using (var f = new Factory())
            {
                Adapter bestAdapter = null;
                var bestLevel = Direct3D.FeatureLevel.Level_10_0;

                foreach (var item in f.Adapters)
                {
                    var level = Direct3D11.Device.GetSupportedFeatureLevel(item);
                    if (bestAdapter == null || level > bestLevel)
                    {
                        bestAdapter = item;
                        bestLevel = level;
                    }
                }
                return bestAdapter;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        static DPFCanvas()
        {
            StretchProperty.OverrideMetadata(typeof(DPFCanvas), new FrameworkPropertyMetadata(Stretch.Fill));
        }

        /// <summary>
        /// 
        /// </summary>
        public DPFCanvas()
        {
            this.renderTimer = new Stopwatch();
            this.Loaded += this.WindowLoaded;
            this.Unloaded += this.WindowClosing;
            this.ClearColor = global::SharpDX.Color.Gray;
            this.IsShadowMapEnabled = false;            
            this.IsMSAAEnabled = true;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (IsInDesignMode)
            {
                return;
            }

            this.StartD3D();
            this.StartRendering();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosing(object sender, RoutedEventArgs e)
        {
            if (IsInDesignMode)
            {
                return;
            }

            this.StopRendering();
            this.EndD3D();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartD3D()
        {
#if DX11
            var adapter = GetBestAdapter();
            if (adapter == null)
            {                
                System.Windows.MessageBox.Show("No DirectX 10 or higher adapter found, a software adapter will be used!", "Warning");
                this.device = new Direct3D11.Device(DriverType.Warp, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0);
            }
            else
            {
                this.device = new Direct3D11.Device( adapter, DeviceCreationFlags.BgraSupport);
                //this.device = new Direct3D11.Device(Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport, Direct3D.FeatureLevel.Level_11_0); 
            }       
#else
            this.device = new Direct3D11.Device(Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport, Direct3D.FeatureLevel.Level_10_1);                        
#endif            
            this.surfaceD3D = new DX11ImageSource();
            this.surfaceD3D.IsFrontBufferAvailableChanged += this.OnIsFrontBufferAvailableChanged;
            this.effects = EffectsManager.Instance;
            this.effects.InitEffects(this.device);
            this.deferredRenderer = new DeferredRenderer();
            
            this.CreateAndBindTargets();
            this.SetDefaultRenderTargets();
            this.Source = this.surfaceD3D;
        }

        /// <summary>
        /// 
        /// </summary>
        private void EndD3D()
        {
            if (this.renderRenderable != null)
            {
                this.renderRenderable.Detach();
                this.sceneAttached = false;
            }

            this.surfaceD3D.IsFrontBufferAvailableChanged -= this.OnIsFrontBufferAvailableChanged;
            this.Source = null;

            Disposer.RemoveAndDispose(ref this.deferredRenderer);
            Disposer.RemoveAndDispose(ref this.surfaceD3D);
            Disposer.RemoveAndDispose(ref this.renderTargetView);
            Disposer.RemoveAndDispose(ref this.depthStencilView);
            Disposer.RemoveAndDispose(ref this.renderTarget);
            Disposer.RemoveAndDispose(ref this.depthStencil);
            Disposer.RemoveAndDispose(ref this.effects);
            Disposer.RemoveAndDispose(ref this.device);

#if MSAA
            Disposer.RemoveAndDispose(ref this.renderTargetNMS);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateAndBindTargets()
        {
            this.surfaceD3D.SetRenderTargetDX11(null);

            int width = System.Math.Max((int)this.ActualWidth, 100);
            int height = System.Math.Max((int)this.ActualHeight, 100);

            Disposer.RemoveAndDispose(ref this.renderTargetView);
            Disposer.RemoveAndDispose(ref this.depthStencilView);
            Disposer.RemoveAndDispose(ref this.renderTarget);
            Disposer.RemoveAndDispose(ref this.depthStencil);
#if MSAA
            Disposer.RemoveAndDispose(ref this.renderTargetNMS);

            // check 8,4,2,1
            int sampleCount = 8;
            int sampleQuality = 0;

            if (this.IsMSAAEnabled)
            {
                do
                {
                    sampleQuality = this.device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, sampleCount) - 1;
                    if (sampleQuality > 0)
                    {
                        break;
                    }
                    else
                    {
                        sampleCount /= 2;
                    }

                    if (sampleCount == 1)
                    {
                        sampleQuality = 0;
                        break;
                    }
                }
                while (true);
            }
            else
            {
                sampleCount = 1;
                sampleQuality = 0;
            }

            var sampleDesc = new SampleDescription(sampleCount, sampleQuality);
            var optionFlags = ResourceOptionFlags.None;
#else
            var sampleDesc = new SampleDescription(1, 0);
            var optionFlags = ResourceOptionFlags.Shared;
#endif

            var colordesc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = sampleDesc,
                Usage = ResourceUsage.Default,
                OptionFlags = optionFlags,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            var depthdesc = new Texture2DDescription
            {
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float_S8X24_UInt,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = sampleDesc,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1,
            };

            this.renderTarget = new Texture2D(this.device, colordesc);
            this.depthStencil = new Texture2D(this.device, depthdesc);

            this.renderTargetView = new RenderTargetView(this.device, this.renderTarget);
            this.depthStencilView = new DepthStencilView(this.device, this.depthStencil);

#if MSAA
            var colordescNMS = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            this.renderTargetNMS = new Texture2D(this.device, colordescNMS);
            this.device.ImmediateContext.ResolveSubresource(this.renderTarget, 0, this.renderTargetNMS, 0, Format.B8G8R8A8_UNorm);
            this.surfaceD3D.SetRenderTargetDX11(this.renderTargetNMS);
#else
            this.surfaceD3D.SetRenderTargetDX11(this.renderTarget);
#endif                       
        }

        /// <summary>
        /// Sets the default render-targets
        /// </summary>
        public void SetDefaultRenderTargets()
        {
            this.SetDefaultRenderTargets(this.renderTarget.Description.Width, this.renderTarget.Description.Height);
        }

        /// <summary>
        /// Sets the default render-targets
        /// </summary>
        public void SetDefaultRenderTargets(int width, int height)
        {
            this.targetWidth = width;
            this.targetHeight = height;

            this.device.ImmediateContext.OutputMerger.SetTargets(this.depthStencilView, this.renderTargetView);
            this.device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, width, height, 0.0f, 1.0f));

            this.device.ImmediateContext.ClearRenderTargetView(this.renderTargetView, this.ClearColor);
            this.device.ImmediateContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        /// <summary>
        /// Clears the buffers with the clear-color
        /// </summary>
        /// <param name="clearBackBuffer"></param>
        /// <param name="clearDepthStencilBuffer"></param>
        internal void ClearRenderTarget(bool clearBackBuffer = true, bool clearDepthStencilBuffer = true)
        {
            if (clearBackBuffer)
                this.device.ImmediateContext.ClearRenderTargetView(this.renderTargetView, this.ClearColor);
            if (clearDepthStencilBuffer)
                this.device.ImmediateContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        /// <summary>
        /// Sets the given resources as render targets:
        /// </summary>
        /// <param name="width">width of the target</param>
        /// <param name="height">height of the target</param>
        /// <param name="renderTargetView">the color buffer resource</param>
        /// <param name="depthStencilView">the depth-stencil buffer resource</param>
        //private void SetRenderTargets(int width, int height, RenderTargetView renderTargetView, DepthStencilView depthStencilView)
        //{
        //    this.device.ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
        //    this.device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, width, height, 0.0f, 1.0f));

        //    this.device.ImmediateContext.ClearRenderTargetView(renderTargetView, this.ClearColor);
        //    this.device.ImmediateContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneTime"></param>
        private void Render(System.TimeSpan sceneTime)
        {
            var device = this.device;
            if (device == null)
                return;

            var renderTarget = this.renderTarget;
            if (renderTarget == null)
                return;

            if (this.Renderable != null)
            {
                /// ---------------------------------------------------------------------------
                /// this part is done only if the scene is not attached
                /// it is an attach and init pass for all elements in the scene-graph                
                if (!this.sceneAttached)
                {
                    try
                    {
                        Light3D.LightCount = 0;
                        this.sceneAttached = true;
                        this.ClearColor = this.renderRenderable.BackgroundColor;                            
                        this.IsShadowMapEnabled = this.renderRenderable.IsShadowMappingEnabled;                        
                        this.RenderTechnique = this.renderRenderable.RenderTechnique == null ? Techniques.RenderBlinn : this.renderRenderable.RenderTechnique;
                            
                        if (this.renderContext != null)
                        {
                            this.renderContext.Dispose();
                        }
                        this.renderContext = new RenderContext(this, this.effects.GetEffect(this.RenderTechnique));
                        this.renderRenderable.Attach(this);
                        this.SetDefaultRenderTargets();
                        switch (this.RenderTechnique)
                        {
                            default:
                                break;
                            case Techniques.RenderDeferred:                                
                                this.deferredRenderer.CreateBuffers(this, Format.R32G32B32A32_Float);
                                break;
                            case Techniques.RenderGBuffer:
                                this.deferredRenderer.CreateBuffers(this, Format.B8G8R8A8_UNorm);
                                break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("DPFCanvas: Error attaching element: " + string.Format(ex.Message), "Error");
                    }
                }

                /// ---------------------------------------------------------------------------
                /// this part is per frame                
                if (this.RenderTechnique == Techniques.RenderDeferred)
                {                  
                    /// set G-Buffer                    
                    this.deferredRenderer.SetGBufferTargets();

                    /// render G-Buffer pass                
                    this.renderRenderable.Update(this.renderTimer.Elapsed);
                    this.renderRenderable.Render(this.renderContext);

                    /// reset render targets and run lighting pass
                    this.SetDefaultRenderTargets();
                    /// set the lights                    
                    this.deferredRenderer.RenderLighting(this.renderContext, this.renderRenderable.Items.OfType<ILight3D>());                    
                }
                else if (this.RenderTechnique == Techniques.RenderGBuffer)
                {
                    /// set G-Buffer
                    this.deferredRenderer.SetGBufferTargets(targetWidth / 2, targetHeight / 2);
                    
                    /// render G-Buffer pass                    
                    this.renderRenderable.Update(this.renderTimer.Elapsed);
                    this.renderRenderable.Render(this.renderContext);

                    /// reset render targets and run lighting pass                                         
#if MSAA                    
                    this.deferredRenderer.RenderGBufferOutput(ref this.renderTargetNMS);
#else
                    this.deferredRenderer.RenderGBufferOutput(ref this.renderTarget);
#endif
                }
                else
                {
                    this.device.ImmediateContext.ClearRenderTargetView(this.renderTargetView, this.ClearColor);
                    this.device.ImmediateContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
                    
                    this.renderRenderable.Update(this.renderTimer.Elapsed);
                    this.renderRenderable.Render(this.renderContext);
                }
            }

            this.device.ImmediateContext.Flush();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartRendering()
        {
            if (this.renderTimer.IsRunning)
                return;

            CompositionTarget.Rendering += OnRendering;
            this.renderTimer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StopRendering()
        {
            if (!this.renderTimer.IsRunning)
                return;

            CompositionTarget.Rendering -= OnRendering;
            this.renderTimer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRendering(object sender, System.EventArgs e)
        {
            if (!this.renderTimer.IsRunning)
                return;

            this.Render(this.renderTimer.Elapsed);

#if MSAA
            this.device.ImmediateContext.ResolveSubresource(this.renderTarget, 0, this.renderTargetNMS, 0, Format.B8G8R8A8_UNorm);
#endif
            this.surfaceD3D.InvalidateD3DImage();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            switch (this.RenderTechnique)
            {
                default:
                    break;
                case Techniques.RenderDeferred:
                    this.deferredRenderer.CreateBuffers(this, Format.R32G32B32A32_Float);
                    break;
                case Techniques.RenderGBuffer:
                    this.deferredRenderer.CreateBuffers(this, Format.B8G8R8A8_UNorm);
                    break;
            }

            this.CreateAndBindTargets();            
            this.SetDefaultRenderTargets();
            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // this fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (this.surfaceD3D.IsFrontBufferAvailable)
            {
                this.StartRendering();
            }
            else
            {
                this.StopRendering();
            }
        }
    }
}
