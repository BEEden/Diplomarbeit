﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EllipsoidVisual3D.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System.Windows;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// A visual element that shows an axis aligned ellipsoid.
    /// </summary>
    public class EllipsoidVisual3D : MeshElement3D
    {
        /// <summary>
        /// The phi div property.
        /// </summary>
        public static readonly DependencyProperty PhiDivProperty = DependencyProperty.Register(
            "PhiDiv", typeof(int), typeof(SphereVisual3D), new PropertyMetadata(30, GeometryChanged));

        /// <summary>
        /// The x radius property.
        /// </summary>
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(
            "RadiusX", typeof(double), typeof(SphereVisual3D), new PropertyMetadata(1.0, GeometryChanged));

        /// <summary>
        /// The y radius property.
        /// </summary>
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(
            "RadiusY", typeof(double), typeof(SphereVisual3D), new PropertyMetadata(1.0, GeometryChanged));

        /// <summary>
        /// The z radius property.
        /// </summary>
        public static readonly DependencyProperty RadiusZProperty = DependencyProperty.Register(
            "RadiusZ", typeof(double), typeof(SphereVisual3D), new PropertyMetadata(1.0, GeometryChanged));

        /// <summary>
        /// The theta div property.
        /// </summary>
        public static readonly DependencyProperty ThetaDivProperty = DependencyProperty.Register(
            "ThetaDiv", typeof(int), typeof(SphereVisual3D), new PropertyMetadata(60, GeometryChanged));

        /// <summary>
        /// Gets or sets the number of divisions in the phi direction (from "top" to "bottom").
        /// </summary>
        /// <value>The number of divisions.</value>
        public int PhiDiv
        {
            get
            {
                return (int)this.GetValue(PhiDivProperty);
            }

            set
            {
                this.SetValue(PhiDivProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the X equatorial radius of the ellipsoid.
        /// </summary>
        /// <value>The radius.</value>
        public double RadiusX
        {
            get
            {
                return (double)this.GetValue(RadiusXProperty);
            }

            set
            {
                this.SetValue(RadiusXProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the Y equatorial radius of the ellipsoid.
        /// </summary>
        /// <value>The radius.</value>
        public double RadiusY
        {
            get
            {
                return (double)this.GetValue(RadiusYProperty);
            }

            set
            {
                this.SetValue(RadiusYProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the polar radius of the ellipsoid.
        /// </summary>
        /// <value>The radius.</value>
        public double RadiusZ
        {
            get
            {
                return (double)this.GetValue(RadiusZProperty);
            }

            set
            {
                this.SetValue(RadiusZProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the number of divisions in the theta direction (around the sphere).
        /// </summary>
        /// <value>The number of divisions.</value>
        public int ThetaDiv
        {
            get
            {
                return (int)this.GetValue(ThetaDivProperty);
            }

            set
            {
                this.SetValue(ThetaDivProperty, value);
            }
        }

        /// <summary>
        /// The tessellate.
        /// </summary>
        /// <returns>The mesh.</returns>
        protected override MeshGeometry3D Tessellate()
        {
            var builder = new MeshBuilder(false, true);
            builder.AddSphere(new Point3D(0, 0, 0), 1.0, this.ThetaDiv, this.PhiDiv);
            builder.Scale(this.RadiusX, this.RadiusY, this.RadiusZ);
            return builder.ToMesh();
        }

    }
}