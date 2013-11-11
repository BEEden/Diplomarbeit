namespace HelixToolkit.SharpDX.Wpf
{
    using Point3D = global::SharpDX.Vector3;
    using Point2D = global::SharpDX.Vector2;
    using System.Collections.Generic;
    using System;

    [Serializable]
    public class MeshGeometry3D : Geometry3D
    {
        public Point3D[] Normals { get; set; }
        public Point2D[] TextureCoordinates { get; set; }        

        public Point3D[] Tangents { get; set; }
        public Point3D[] BiTangents { get; set; }

        public IEnumerable<Geometry3D.Triangle> Triangles
        {
            get
            {
                for (int i = 0; i < Indices.Length; i += 3)
                {
                    yield return new Triangle() { P0 = Positions[Indices[i]], P1 = Positions[Indices[i + 1]], P2 = Positions[Indices[i + 2]], };
                }
            }
        }
    }
}