namespace HelixToolkit.SharpDX.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using global::SharpDX;
    using global::SharpDX.DXGI;
    using global::SharpDX.Direct3D11;
    using global::SharpDX.D3DCompiler;
    using System.Runtime.InteropServices;
    using System.IO;

    public static class Techniques
    {

        /// <summary>
        /// Names of techniques which are implemented by default
        /// </summary>
        public const string RenderBlinn = "RenderBlinn";
        public const string RenderPhong = "RenderPhong";


        public const string RenderDiffuse = "RenderDiffuse";
        public const string RenderColors = "RenderColors";
        public const string RenderPositions = "RenderPositions";
        public const string RenderNormals = "RenderNormals";
        public const string RenderTangents = "RenderTangents";
        public const string RenderTexCoords = "RenderTexCoords";
        public const string RenderWires = "RenderWires";

        public const string RenderDeferred = "RenderDeferred";
        public const string RenderGBuffer = "RenderGBuffer";

#if DX11
        public const string RenderPNTriangs = "RenderPNTriangs";
        public const string RenderPNQuads = "RenderPNQuads";
#endif
        
        //public const string RenderInstanced = "RenderInstanced";
        public const string RenderCubeMap = "RenderCubeMap";
        public const string RenderLines = "RenderLines";
        public const string RenderDeferredLighting = "RenderDeferredLighting";
        public const string RenderSpineTest = "RenderSpineTest";

        public static IEnumerable<string> RenderTechniques { get { return renderTechniques; } }

        private static List<string> renderTechniques = new List<string>
        { 
            RenderBlinn,
            RenderPhong,

            RenderColors,
            RenderDiffuse,
            RenderPositions,
            RenderNormals,
            RenderTangents, 
            RenderTexCoords,
            RenderWires,

            RenderDeferred,
            RenderGBuffer,   

#if DX11
            RenderPNTriangs, 
            RenderPNQuads,
#endif
            //RenderInstanced,
            //RenderCubeMap,
            //RenderLines,
            //RenderDeferredLighting,
            //RenderSpineTest,
        };

        internal static void RegisterPublicTechnique(string techniqueName)
        {
            renderTechniques.Add(techniqueName);            
        }

        internal static void UnregsterPublicTechniqe(string techniqueName)
        {
            renderTechniques.Remove(techniqueName);
        }

    }


    public class EffectsManager : IDisposable
    {
        public static EffectsManager Instance { get { return instance; } }

        private static EffectsManager instance = new EffectsManager();         
        private global::SharpDX.Direct3D11.Device device;
        private Dictionary<string, object> data = new Dictionary<string, object>();        
        private EffectsManager()
        {                      
        }

        ~EffectsManager()
        {
            this.Dispose();
        }

        internal void InitEffects(global::SharpDX.Direct3D11.Device device)
        {
            this.device = device;

            var sFlags = ShaderFlags.None;
            var eFlags = EffectFlags.None;
#if DEBUG
            sFlags |= ShaderFlags.Debug;
            eFlags |= EffectFlags.None;
#endif
            try
            {

                // ------------------------------------------------------------------------------------
#if DX11
                RegisterEffect(new StreamReader(@"./Shaders/Tessellation.fx").ReadToEnd(),
#else
                RegisterEffect(new StreamReader(@"./Shaders/Default.fx").ReadToEnd(),
#endif
                    new[] 
                { 
                    // put here the techniques which you want to use with this effect
                    Techniques.RenderPhong, 
                    Techniques.RenderBlinn, 
                    Techniques.RenderCubeMap, 
                    Techniques.RenderColors, 
                    Techniques.RenderDiffuse,
                    Techniques.RenderPositions,
                    Techniques.RenderNormals, 
                    Techniques.RenderTangents, 
                    Techniques.RenderTexCoords, 
                    Techniques.RenderWires, 
                    
#if DX11
                    Techniques.RenderPNTriangs,                     
                    Techniques.RenderPNQuads,
#endif

                    //Techniques.RenderLines,
                    Techniques.RenderSpineTest
                    
                });

                // ------------------------------------------------------------------------------------
                //RegisterEffect(new StreamReader(@"./Shaders/Lines.fx").ReadToEnd(), //Properties.Resources.Deferred,
                //    new[] 
                //{ 
                //    Techniques.RenderLines 
                //});

                // ------------------------------------------------------------------------------------

                // ------------------------------------------------------------------------------------
                /*
                RegisterEffect(new StreamReader(@"./Shaders/SpineTest.fx").ReadToEnd(), //Properties.Resources.Deferred,
                    new[] 
                { 
                    Techniques.RenderSpineTest
                });
                */
                // ------------------------------------------------------------------------------------

                RegisterEffect(new StreamReader(@"./Shaders/Deferred.fx").ReadToEnd(), //Properties.Resources.Deferred,
                    new[] 
                { 
                    Techniques.RenderDeferred, 
                    Techniques.RenderGBuffer,
                });

                // ------------------------------------------------------------------------------------
                RegisterEffect(new StreamReader(@"./Shaders/DeferredLighting.fx").ReadToEnd(),//Properties.Resources.DeferredLighting,
                    new[] 
                { 
                   Techniques.RenderDeferredLighting 
                });

                // ------------------------------------------------------------------------------------
                RegisterLayout(
                    new[] 
                { 
                    // put here techniques which use the vertex layout below
                    Techniques.RenderPhong, 
                    Techniques.RenderBlinn,
                                        
                    Techniques.RenderDiffuse,
                    Techniques.RenderPositions,
                    Techniques.RenderNormals, 
                    Techniques.RenderTangents, 
                    Techniques.RenderTexCoords, 
                    Techniques.RenderColors, 
                    Techniques.RenderWires, 

                    Techniques.RenderDeferred,
                    Techniques.RenderGBuffer,
                },
                    new InputLayout(device, this[Techniques.RenderPhong].GetTechniqueByName(Techniques.RenderPhong).GetPassByIndex(0).Description.Signature,
                    new[] 
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float,       InputElement.AppendAligned, 0),
                    new InputElement("NORMAL",   0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),             
                    new InputElement("TANGENT",  0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),             
                    new InputElement("BINORMAL", 0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),  
           
                    //INSTANCING: die 4 texcoords sind die matrix, die mit jedem buffer reinwandern
                    new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),                 
                    new InputElement("TEXCOORD", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                    new InputElement("TEXCOORD", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                    new InputElement("TEXCOORD", 4, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                }));

#if DX11
                // ------------------------------------------------------------------------------------
                var layout = new InputLayout(device, this[Techniques.RenderPNTriangs].GetTechniqueByName(Techniques.RenderPNTriangs).GetPassByIndex(0).Description.Signature,
                    new[] 
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float,       InputElement.AppendAligned, 0),
                    new InputElement("NORMAL",   0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),             
                    new InputElement("TANGENT",  0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),             
                    new InputElement("BINORMAL", 0, Format.R32G32B32_Float,    InputElement.AppendAligned, 0),            
                });

                RegisterLayout(
                    new[] 
                { 
                    // put here techniques which use the vertex layout below
                    Techniques.RenderPNTriangs,
                    Techniques.RenderPNQuads, 
                },
                    layout
                );
#endif

                // ------------------------------------------------------------------------------------
                RegisterLayout(
                    new[] 
                { 
                    Techniques.RenderCubeMap, 
                    Techniques.RenderDeferredLighting
                },
                    new InputLayout(device, this[Techniques.RenderCubeMap].GetTechniqueByName(Techniques.RenderCubeMap).GetPassByIndex(0).Description.Signature, new[] 
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),                                                             
                }));

                // ------------------------------------------------------------------------------------
                RegisterLayout(
                    new[] 
                { 
                    //Techniques.RenderLines,
                    Techniques.RenderSpineTest
                },
                    new InputLayout(device, this[Techniques.RenderSpineTest].GetTechniqueByName(Techniques.RenderSpineTest).GetPassByIndex(0).Description.Signature, new[] 
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),

                    //INSTANCING: die 4 texcoords sind die matrix, die mit jedem buffer reinwandern
                    new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),                 
                    new InputElement("TEXCOORD", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                    new InputElement("TEXCOORD", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                    new InputElement("TEXCOORD", 4, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                }));

                // ------------------------------------------------------------------------------------
                /*
                RegisterLayout(
                    new[] 
                { 
                    Techniques.RenderSpineTest
                },
                    new InputLayout(device, this[Techniques.RenderSpineTest].GetTechniqueByName(Techniques.RenderSpineTest).GetPassByIndex(0).Description.Signature, new[] 
                {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),

                    //INSTANCING: die 4 texcoords sind die matrix, die mit jedem buffer reinwandern
                    new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),                 
                    new InputElement("TEXCOORD", 2, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                    new InputElement("TEXCOORD", 3, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                    new InputElement("TEXCOORD", 4, Format.R32G32B32A32_Float, InputElement.AppendAligned, 1, InputClassification.PerInstanceData, 1),
                }));
                 * */

            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show(string.Format("Error registering effect: {0}", ex.Message), "Error");
            }

        }

        internal void RegisterEffect(string shaderEffectString, string techniqueName, ShaderFlags sFlags = ShaderFlags.None, EffectFlags eFlags = EffectFlags.None)
        {
            RegisterEffect(shaderEffectString, new[] { techniqueName }, sFlags, eFlags);
        }

        internal void RegisterEffect(string shaderEffectString, string[] techniqueNames, ShaderFlags sFlags = ShaderFlags.None, EffectFlags eFlags = EffectFlags.None)
        {
#if DEBUG
            sFlags |= ShaderFlags.Debug;
            eFlags |= EffectFlags.None;
#endif

            var preprocess = ShaderBytecode.Preprocess(shaderEffectString, null, new IncludeHandler());
            var hashCode = preprocess.GetHashCode();
            if (!File.Exists(hashCode.ToString()))
            {
                try
                {
                    var shaderBytes = ShaderBytecode.Compile(preprocess, "fx_5_0", sFlags, eFlags);
                    shaderBytes.Bytecode.Save(hashCode.ToString());
                    this.RegisterEffect(shaderBytes.Bytecode, techniqueNames);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(string.Format("Error compiling effect: {0}", ex.Message), "Error");
                }
            }
            else
            {
                var shaderBytes = ShaderBytecode.FromFile(hashCode.ToString());
                this.RegisterEffect(shaderBytes, techniqueNames);
            }
        }

        internal void RegisterEffect(byte[] shaderEffectBytecode, string[] techniqueNames, EffectFlags eFlags = EffectFlags.None)
        {                        
            var effect = new Effect(device, shaderEffectBytecode, eFlags);
            foreach (var name in techniqueNames)
                data[name] = effect;     
        }

        /// <summary>
        /// Register a techniqe to the Effects container 
        /// This technique will be visible globally and will be also
        /// exposed in the RenderTechniqes combo-box in the GUI
        /// </summary>
        /// <param name="techniqeName">Name of the Technique, it must be identical to the name used in the FX file!</param>
        internal void RegisterTechnique(string techniqeName)
        {
            Techniques.RegisterPublicTechnique(techniqeName);
        }


        internal void RegisterLayout(string techniqueName, InputLayout layout)
        {
            data[techniqueName + "Layout"] = layout;
        }

        internal void RegisterLayout(string[] techniqueNames, InputLayout layout)
        {
            foreach (var name in techniqueNames)
                data[name + "Layout"] = layout;                      
        }

        public Effect GetEffect(string effectName)
        {
            return (Effect)data[effectName];
        }

        public InputLayout GetLayout(string effectName)
        {
            return (InputLayout)data[effectName + "Layout"];
        }

        public Effect this[string effectName]
        {
            get { return (Effect)data[effectName]; }
        }

        public void Dispose()
        {
            if (data != null)
            {
                foreach (var item in data)
                {
                    var o = item.Value as IDisposable;
                    Disposer.RemoveAndDispose(ref o);
                }
            }
            this.device = null;
            this.data = null;   
        }


        private class IncludeHandler : Include
        {
            public void Close(Stream stream)
            {
                stream.Close();
            }

            public Stream Open(IncludeType type, string fileName, Stream parentStream)
            {
                stream = new StreamReader(fileName).BaseStream;
                return stream;
            }

            public IDisposable Shadow
            {
                get
                {
                    return this.stream;
                }
                set
                {
                    if (this.stream != null)
                        this.stream.Dispose();
                    this.stream = value as Stream;
                }
            }

            private Stream stream;
            public void Dispose()
            {
                stream.Dispose();
            }
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DefaultVertex
    {
        public Vector4 Position;
        public Color4 Color;
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;

        public const int SizeInBytes = 4 * (4 + 4 + 2 + 3 + 3 + 3);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LinesVertex
    {
        public Vector4 Position;
        public Color4 Color;
        public const int SizeInBytes = 4 * (4 + 4);
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CubeVertex
    {
        public Vector4 Position;        
        public const int SizeInBytes = 4 * 4;
    }
}
