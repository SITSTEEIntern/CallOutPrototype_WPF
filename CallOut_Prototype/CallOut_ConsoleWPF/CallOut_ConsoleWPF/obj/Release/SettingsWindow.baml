   M S B A M L   `   `   `  ����   CallOut_ConsoleWPF'    !CallOut_ConsoleWPF.SettingsWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  SettingsWindow��$	��300��$	��519��.��� -    �Name$ panelSetting�� � RowDefinitions �� $	��126 ��� �� $��36 �.+�� $��0��$��Center=�$��Center=�$��	37,3,44,3q�$	��430��.+��� $
��Left=�$��	18,16,0,0q�$
��Wrap=�$��IP of Coding Service��$	��Top=��� -    ��Name$ txtCodingSvcIP��$
��Left=�$��23��$��
142,13,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��200���� $
��Left=�$��	72,45,0,0q�$
��Wrap=�$��
Station ID��$	��Top=��� -    ��Name$ comboID��$
��Left=�$��
142,42,0,0q�$	��Top=�$	��200���� $
��Auto��$
��Left=�$��
142,75,0,0q�$	��Top=��� $
��Left=�$��
190,71,0,0q�$	��Top=�$��80���� $
��Left=�$��
275,75,0,0q�$
��Wrap=�$��In��$	��Top=��� $
��Left=�$��23��$��
290,72,0,0q�$
 Wrap=�$	��Top=�$��52���� $
��Left=�$��
347,75,0,0q�$
��Wrap=�$��seconds��$	��Top=��� -    ��Name$ btnSetIP��$��Set IP��$
��Left=�$��
347,14,0,0q�$	��Top=�$��75���� -   $ btnSetID��$	��Set��$
��Left=�$��
142,98,0,0q�$	��Top=�$��95���� -   $��Cancel��$
��Left=�$��
247,98,0,0q�$	��Top=�$��95��� $��1��$��5 5 5 5q�.+��� -    ��Name$ lvConsoleLog�� �� View �� .P��� 	 �� Width$		 100��
 �� DisplayMemberBinding
 �� *
CodingID+���� -   
 � Tag$ CodingID��.��
CodingID�� $		 150��
 �� *AckTimeStamp+���� -	   $ AckTimeStamp��.��AckTimeStamp�� $		 100��
 �� *	AckFrom+���� -
   $ AckFrom��.��	AckFrom�� $		 130��
 �� *	AckStatus+���� -   $ 	AckStatus��.��	AckStatus� $��2��$��Center=�$��Center=�.+��� $
��Left=�$��23��$��5,0,0,0q�$
 Wrap=�$	��Top=�$	��170���� $
��Left=�$��	181,0,0,0q�$	��Top=�$	��110���� $
��Left=�$��	296,0,0,0q�$	��Top=�$	��110���� $��Search��$
��Left=�$��	412,1,0,0q�$	��Top=�$��75��� -   $ panelConnection��$��0��$��0��$��3��$y�	Collapsed=� �� $
��150* ��� $	��50* ��� $
��150* �.+��� $��0��
���   � J� Opacity$	 0.5Q�� $��1��
���   �$	 0.8Q�.+�(� $��Center=�$��Center=�.���� .+��� $$��Lost of Internet Connection...��$
��Wrap=�#��� ��$��Cambria)�$��5 2 5 2q��� $��2��
���   �$	 0.5Q�