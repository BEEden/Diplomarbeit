// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IModelReader.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.SharpDX.Wpf
{
    using System.IO;
    using System.Windows.Media.Media3D;
    using Mesh3DGroup = System.Collections.Generic.List<Mesh3D>;

    public enum MeshFaces
    {
        Default, 
        QuadPatches,
    }

    public struct ModelInfo
    {
        public MeshFaces Faces { get; set; }
    }

    /// <summary>
    /// Interface for model readers.
    /// </summary>
    public interface IModelReader
    {
        /// <summary>
        /// Reads the model from the specified path.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The model.
        /// </returns>
        Mesh3DGroup Read(string path, ModelInfo info = default(ModelInfo));

        /// <summary>
        /// Reads the model from the specified stream.
        /// </summary>
        /// <param name="s">
        /// The stream.
        /// </param>
        /// <returns>
        /// The model.
        /// </returns>
        Mesh3DGroup Read(Stream s, ModelInfo info = default(ModelInfo));

    }
}