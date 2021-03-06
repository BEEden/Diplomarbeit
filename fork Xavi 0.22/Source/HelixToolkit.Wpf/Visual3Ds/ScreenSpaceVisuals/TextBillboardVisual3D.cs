﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextBillboardVisual3D.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// <summary>
//   A visual element that contains a text billboard.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// A visual element that contains a text billboard.
    /// </summary>
    public class TextBillboardVisual3D : BillboardVisual3D
    {
        /// <summary>
        /// The font family property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            "FontFamily", typeof(FontFamily), typeof(TextBillboardVisual3D), new UIPropertyMetadata(null, TextChanged));

        /// <summary>
        /// The font size property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(double), typeof(TextBillboardVisual3D), new UIPropertyMetadata(0.0, TextChanged));

        /// <summary>
        /// The font weight property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            "FontWeight", 
            typeof(FontWeight), 
            typeof(TextBillboardVisual3D), 
            new UIPropertyMetadata(FontWeights.Normal, TextChanged));

        /// <summary>
        /// The foreground property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground", typeof(Brush), typeof(TextBillboardVisual3D), new UIPropertyMetadata(Brushes.Black));

        /// <summary>
        /// The height factor property
        /// </summary>
        public static readonly DependencyProperty HeightFactorProperty = DependencyProperty.Register(
            "HeightFactor", typeof(double), typeof(TextBillboardVisual3D), new PropertyMetadata(1.0));

        /// <summary>
        /// The text property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TextBillboardVisual3D), new UIPropertyMetadata(null, TextChanged));

        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        /// <value>The font family.</value>
        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)this.GetValue(FontFamilyProperty);
            }

            set
            {
                this.SetValue(FontFamilyProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        /// <value>The size of the font.</value>
        public double FontSize
        {
            get
            {
                return (double)this.GetValue(FontSizeProperty);
            }

            set
            {
                this.SetValue(FontSizeProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        /// <value>The font weight.</value>
        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)this.GetValue(FontWeightProperty);
            }

            set
            {
                this.SetValue(FontWeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the foreground brush.
        /// </summary>
        /// <value>The foreground.</value>
        public Brush Foreground
        {
            get
            {
                return (Brush)this.GetValue(ForegroundProperty);
            }

            set
            {
                this.SetValue(ForegroundProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the height factor.
        /// </summary>
        /// <value>
        /// The height factor.
        /// </value>
        public double HeightFactor
        {
            get
            {
                return (double)this.GetValue(HeightFactorProperty);
            }

            set
            {
                this.SetValue(HeightFactorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get
            {
                return (string)this.GetValue(TextProperty);
            }

            set
            {
                this.SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// The text changed.
        /// </summary>
        /// <param name="d">
        /// The d.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextBillboardVisual3D)d).OnTextChanged();
        }

        /// <summary>
        /// The on text changed.
        /// </summary>
        private void OnTextChanged()
        {
            var textBlock = new TextBlock(new Run(this.Text));
            textBlock.SetBinding(TextBlock.ForegroundProperty, new Binding("Foreground") { Source = this });

            if (this.FontFamily != null)
            {
                textBlock.FontFamily = this.FontFamily;
            }

            textBlock.FontWeight = this.FontWeight;

            if (this.FontSize > 0)
            {
                textBlock.FontSize = this.FontSize;
            }

            this.Material = new DiffuseMaterial(new VisualBrush(textBlock));

            textBlock.Measure(new Size(1000, 1000));
            this.Width = textBlock.DesiredSize.Width;
            this.Height = textBlock.DesiredSize.Height * this.HeightFactor;
        }
    }
}