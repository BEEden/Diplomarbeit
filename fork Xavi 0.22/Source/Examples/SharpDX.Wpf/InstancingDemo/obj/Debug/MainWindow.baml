   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5   	   N JSharpDX, Version=2.4.0.0, Culture=neutral, PublicKeyToken=627a3d6d1956f55a2&clr-namespace:SharpDX;assembly=SharpDXSharpDX 5   	   Z VHelixToolkit.SharpDX.Wpf, Version=2012.1.1.27767, Culture=neutral, PublicKeyToken=nulleHclr-namespace:HelixToolkit.SharpDX.Wpf;assembly=HelixToolkit.SharpDX.WpfHelixToolkit.SharpDX.Wpf 5   	    InstancingDemo   InstancingDemo.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   6sys&clr-namespace:System;assembly=mscorlib     5   	   6sdx&clr-namespace:SharpDX;assembly=SharpDX     5   	   XhtxHclr-namespace:HelixToolkit.SharpDX.Wpf;assembly=HelixToolkit.SharpDX.Wpf     5   	   $	��500��5
   !   $	��800��6.     � Title  6	   �� *Title+ � 	Resources 5      �� 5   
    �� Member$% htx:Techniques.RenderTechniques��6      KeyRenderTechniques  6E   .��5      �  � RowDefinitions 5   
   �� 5      .+�5   
   *  $HelixToolkit.SharpDX.Wpf.Viewport3DX  -   5          Name$ view1��5      $��0��5         	TextBrush
 �   �   ShowCoordinateSystem .  &   CoordinateSystemLabelForeground
 �����   UseDefaultGestures . 	   Camera	 5      �� *Camera+
   Title
 5      �� *Title+   SubTitle 5      �� *
SubTitle+   RenderTechnique 5      �� *RenderTechnique+.@�5      -  'HelixToolkit.SharpDX.Wpf.AmbientLight3D  &   HelixToolkit.SharpDX.Wpf.Light3D   Color 6!   �� *AmbientLightColor+1  +HelixToolkit.SharpDX.Wpf.DirectionalLight3D  5!       6%   �� *DirectionalLightColor+   	Direction 6M   �� *DirectionalLightDirection+/  )HelixToolkit.SharpDX.Wpf.EnvironmentMap3D  5#      -   5          Name$ envMap��5#   #      Filename$ Cubemap_Grandcanyon.dds��63   2  ,HelixToolkit.SharpDX.Wpf.MeshGeometryModel3D  5%      -   5          Name$
 mesh��5%   &   6  0HelixToolkit.SharpDX.Wpf.MaterialGeometryModel3D   	Instances 65   �� *ModelInstances+.  (HelixToolkit.SharpDX.Wpf.GeometryModel3D   Geometry 6Z   �� *Model+&	   HelixToolkit.SharpDX.Wpf.Model3D 	  	Transform 6u   �� *ModelTransform+   Material 6�   �� *ModelMaterial+2
  ,HelixToolkit.SharpDX.Wpf.LineGeometryModel3D
  5'      -   5         
 Name$ lines��5'   &    
  	Instances 65   �� *ModelInstances+ 6Z   �� *Lines+ 6u   �� *ModelTransform+5)      �� 5+   
   $��0��6   .+�5,      9� -   5         9�Name$ shadingPanel��5,   1   $��Render Technique��6   $��Right=�6G   $	��150��6c   $	��Top=�6o   
������� 9� 
IsExpanded . $��10q�6�   ��5-      �� 5.       �� Opacity$
 0.25Q�6&    �� Color$ 
WhiteSmoke��65   .��50      �  � ColumnDefinitions 51      �� 52      $	��23* �6+   �� 53      $	��51* �6+    55      �� 56      .+�58      �� $��0��6     �� Text$ --- Select Model ---��6-    �� 
IsEditable .  �� 
IsReadOnly . $��3q�6�   $��2��6�   ��6I   ��   RenderTechniques  �� Source#  [ ! �� SelectedItem! 6�   �� *RenderTechnique+6�   59      5:      9� 5;      $��
Instancing��6   $��Right=�6+   $	��150��6G   $	��Top=�6S   
������� . $��10q�6�   ��5<      �� 5=      $
 0.25Q�6&   $ 
WhiteSmoke��65   .��5?      �  5@      �� 5A      �� 5B      .+�5D      �� $��0��6   $��4q�6,   " d� 	IsChecked" 67   �� *IsRendering+# �� ElementName$
# mesh��.��6k   Render Geometry6|   �� 5E      $��1��6   $��4q�6,   " 67   �� *IsRendering+$# lines��.��6l   Render Lines6z   5F      5G      5H      �� 5I   
   $��0��6   $��Bottom=�6!   ��5J      �� 5K      $	 0.5Q�6"   $ 
WhiteSmoke��60   .@�5M      �� .��5N      �� $	��Top=�6   $
��Left=�64   ��6O   �� *FpsCounter.Value+$# view1��$ �� StringFormat$$ D3D11 - {0:0.00} FPS��5O      �� 5P      �� 5Q      .��5R      �� ��6   �� *Camera.Position+$$ Position: {0:0.0}��5S      �� 5T      �� 5U      .��5V      �� ��6   �� *Camera.LookDirection+$$ LookDirection: {0:0.0}��5W      �� 5X      �� 5Y      .��5Z      �� ��6   �� *Camera.UpDirection+$$ UpDirection: {0:0.0}��5[      �� 5\      �� 5]      .��5^      �� ��6   �� *Items.Count+$# view1��$$ Children: {0}��5_      5`      5a      5b      