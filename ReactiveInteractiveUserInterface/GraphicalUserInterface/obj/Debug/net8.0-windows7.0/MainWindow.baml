   M S B A M L   `   `   `  ����i  eTP.ConcurrentProgramming.PresentationViewModel, Version=4.2.0.0, Culture=neutral, PublicKeyToken=null�uclr-namespace:TP.ConcurrentProgramming.Presentation.ViewModel;assembly=TP.ConcurrentProgramming.PresentationViewModel/TP.ConcurrentProgramming.Presentation.ViewModel  5   	   - )TP.ConcurrentProgramming.PresentationView:   4TP.ConcurrentProgramming.PresentationView.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   9i+http://schemas.microsoft.com/xaml/behaviors     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   �vmuclr-namespace:TP.ConcurrentProgramming.Presentation.ViewModel;assembly=TP.ConcurrentProgramming.PresentationViewModel     5   	     � Title$5  /Presentation layer targeting reactive behaviour��5	   	   $	��600��5
   	   $	��600��6    � DataContext 5      I   CTP.ConcurrentProgramming.Presentation.ViewModel.MainWindowViewModel  5   
   .��5      �  � RowDefinitions 5      �� 5   
   $
��Auto �6   �� 5   
   $��* �6   .+�5      �� $��0��6    �� Orientation$ Vertical=�6   $
��Left=�65   $	��Top=�6P   .+�5   
   �� $��Billard Table��6   $
��Bold"�6)   $��5q�6;   $
��Left=�6F   $	��Top=�6a   
����  �6y   �� 5   
   $��
Simulation��6   $��5q�6&   $
��Left=�61   $	��Top=�6L   
���   �6d   �� 5   
   $	��200��5      $��30��6   $��5q�6)   $
��Left=�5      $	��Top=�6,   
��� � �$
��Bold"�5   $   ��5      �� *NumberOfBalls+ �� StringFormat$ Number of balls: {0}��5   $   �� 5   
   $��Start Simulation��6   $	��150��6,   $��50��68   $��5q�6D   $
��Left=�6O   $	��Top=�6j   ��5      �� *StartCommand+�� 5!   
   $	��150��6   $��30��6   $��5q�6*   $
��Left=�65   ��5"      �� *NumberOfBalls+ �� UpdateSourceTrigger$ PropertyChanged=�5%      (� 5&      $��0��6
    (� Stretch$ Uniform=�6   $��	160,0,0,0q�6,   .��5'      �� -   5         ��Name$ Table��5'      $��1��6   $��4q�6)   
��� � �$	��400��6Q   $	��400��6]   ._�5(   
   �� $
}�True��6   .+�5)      �� ��6   �� *Balls+��5*      �� 5+      .R�5,      �� 5-      ��5/      �� 50      .R�51      A� 
����  ���6"   �� *
Diameter+��6=   �� *
Diameter+6Y   52      ��54      �� 55       �� 
TargetType" ��6   .�56      �� 	 �� Property	 �@��Left
 �� Value
 68   �� *Left+�� 57      	 �@��Top
 67   �� *Top+�� 58      	 �@J�RenderTransformOrigin$
 0.5,0.5&�6B   �� 59      	 � z�
 5:      �� 5;   "    �� Angle 62   �� *Angle+5=      5>      5@      5A      5B      5C      5D      5E      