   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  Z VHelixToolkit.SharpDX.Wpf, Version=2012.1.1.21432, Culture=neutral, PublicKeyToken=nulleHclr-namespace:HelixToolkit.SharpDX.Wpf;assembly=HelixToolkit.SharpDX.WpfHelixToolkit.SharpDX.Wpf M IEnvironmentMapDemo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:EnvironmentMapDemoEnvironmentMapDemo #   EnvironmentMapDemo.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Z VWPFToolkit.Extended, Version=1.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  0sys&clr-namespace:System;assembly=mscorlib  RhtxHclr-namespace:HelixToolkit.SharpDX.Wpf;assembly=HelixToolkit.SharpDX.Wpf  ,local clr-namespace:EnvironmentMapDemo    � Title$  Environment Mapping Demo��$	��500��$
��1200��$��11(� � DataContext &   EnvironmentMapDemo.MainViewModel   � 	Resources    MarginLarge  MarginSmall �� Member  RenderTechniques��%`   &	        &	      &	      u�10q�u�2q��� $% htx:Techniques.RenderTechniques��.���  � RowDefinitions �� .+�*  $HelixToolkit.SharpDX.Wpf.Viewport3DX  -     Name$ view1��$��0��   	TextBrush
 �   �   ShowCoordinateSystem . &   CoordinateSystemLabelForeground
 �����	   UseDefaultGestures	 .  
   Camera
 �� *Camera+   Title �� *Title+   SubTitle �� *
SubTitle+   RenderTechnique �� *RenderTechnique+ J� InputBindings �� 
 �� Key$ B�� �� Command$# htx:ViewportCommands.BackView���� $ F��$$ htx:ViewportCommands.FrontView���� $ U��$" htx:ViewportCommands.TopView���� $ D��$% htx:ViewportCommands.BottomView���� $ L��$# htx:ViewportCommands.LeftView���� $ R��$$ htx:ViewportCommands.RightView����  �� Gesture$ 	Control+E��$&  htx:ViewportCommands.ZoomExtents��d�  d� Gesture$ 
RightClicka�$! htx:ViewportCommands.Rotate��d� $ MiddleClicka�$ htx:ViewportCommands.Zoom��d� $ 	LeftClicka�$ htx:ViewportCommands.Pan��.@�-  'HelixToolkit.SharpDX.Wpf.AmbientLight3D  &   HelixToolkit.SharpDX.Wpf.Light3D   Color �� *AmbientLightColor+1  +HelixToolkit.SharpDX.Wpf.DirectionalLight3D   �� *DirectionalLightColor+   	Direction �� *DirectionalLightDirection+/  )HelixToolkit.SharpDX.Wpf.EnvironmentMap3D  -     Name$ envMap��   Filename$ Cubemap_Grandcanyon.dds��2  ,HelixToolkit.SharpDX.Wpf.MeshGeometryModel3D  -     Name$ model��.  (HelixToolkit.SharpDX.Wpf.GeometryModel3D   Geometry �� *Model+&	   HelixToolkit.SharpDX.Wpf.Model3D 	  	Transform �� *ModelTransform+6
  0HelixToolkit.SharpDX.Wpf.MaterialGeometryModel3D 
  Material �� *ModelMaterial+�� .+�9� -    9�Name$ shadingPanel��$��Render Technique��$��Right=�$	��200��$	��Top=�
������� 9� 
IsExpanded . $��10q�����  �� Opacity$
 0.25Q� �� Color$ 
WhiteSmoke��.���� $��0�� �� Text$ --- Select Model ---��  �� 
IsEditable  . ! �� 
IsReadOnly! . $��3q�$��2������ " �� Source#" [ # �� SelectedItem# �� *RenderTechnique+9� -   $ propertiesPanel��$��
Properties��$	��200��$
��Auto��$��Right=�$	��Top=�
������� . #��[  ���� $
 0.25Q�$ 
WhiteSmoke��.���  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��#��[ $ d� 	IsChecked$ �� *
IsActive+% �� ElementName$% envMap��.��Reflect Environment Map�� $��1��#��[ $ �� *IsRendering+$% envMap��.��Render Environment Map�� $��2��#��[ 	Material:�� -   & ��Name$& matCombo��$��3��#��[ $ Select Material...��-  'HelixToolkit.SharpDX.Wpf.PhongMaterials'   	Materials#��Z'   . ! . # �� *
Material+$% model��(  "EnvironmentMapDemo.MaterialControl  -   (  Name$( 
matControl��$��4��#��[  �� *SelectedItem+$% matCombo���� $��0��$��20��$��Bottom=�$��Stretch=����� $
 0.25Q�$ 
WhiteSmoke��.@��� .���� $	��Top=�$
��Left=����� *FpsCounter.Value+$% view1��) �� StringFormat$) D3D11 - {0:0.00} FPS���� �� .���� ���� *Camera.Position+$) Position: {0:0.0}���� �� .���� ���� *Camera.LookDirection+$) LookDirection: {0:0.0}���� �� .���� ���� *Camera.UpDirection+$) UpDirection: {0:0.0}���� �� .���� ���� *Items.Count+$% view1��$) Children: {0}��