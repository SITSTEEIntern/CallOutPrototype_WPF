   M S B A M L   `   `   `  ����   CallOut_ConsoleWPF#    CallOut_ConsoleWPF.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     �Name$  
mainwindow�� � Title$ Call Out Console��$��403.507��$	��525��.��� -    �Name$ panelDisplay��
���   � � ColumnDefinitions �� $
��380* ��� $	��50* ��� $	��50* � � RowDefinitions �� $	��43* ��� $	��43* ��� $	��40* ��� $	��33* ��� $	��33* ��� $	��43* ��� $	��45* ��� $	��40* ��� $��5* �.+�� -   $ gIncidentSummary��$��0��$��3�����	#FFCA0000$y�	Collapsed=�.+�(� $��Center=�$��Center=�.���� .+��� -    ��Name$ txtIncidentSummary��$�� ��#��� ��$��Cambria)�$��5 2 5 2q�� $��1��$��3��#��� ��.+�(�  (� Stretch$ Uniform=� (� StretchDirection$
 Both=�.���� .+��� -   $ txtLocationSummary��$�� ��#��� ��$��Cambria)�$��5 2 5 2q�(� $��3��$��1��$��5 2 5 2q�$��2��$ Uniform=�.���� -    ��Name$ btnAck��$��Stretch=�$��Stretch=�
������ $y�	Collapsed=���Z� ���� .R��� $��Center=�$��Center=�._��� $��Icon/AckButton.png��	 �� BitmapScalingMode$	 HighQuality=�(� $��4��$��1��$��5 2 5 2q�$��2��$ Uniform=�.���� -   $ 	btnReject��$��Stretch=�$��Stretch=�
������ $y�	Collapsed=���Z� ���� .R��� $��Center=�$��Center=�._��� $��Icon/RejectButton.png��$	 HighQuality=�� -   $ gPriorAlarm��$��2��$��0��#��� ��$��10 0 5 0q�$y�	Collapsed=�.+�(� $
��Left=�$��Stretch=�.���� .+��� -	   $ txtPriorAlarm��$�� ��#��� ��$��Cambria)�$��5 2 5 2q�� -
   $ gAddress�����	#FFFFF700$��3��$��0��$��3��$��10 0 5 0q�$y�	Collapsed=�.+�(� $
��Left=�$��Stretch=�.���� $��5 5 5 5q�.+��� -   $ txtLocationName��$�� ��$��Cambria)��� -   $ txtLocationStreet��$�� ��$��Cambria)��� -   $ txtLocationUnit��$�� ��$��Cambria)��� -   $ txtLocationStateCity��$�� ��$��Cambria)��� -   $ txtLocationPostal��$�� ��$��Cambria)�(� $��6��$��0��$��2��$
��Left=�$	��Top=�$
 Fill=�$��10px 0px 5px 0pxq�.���� -   
 ��Name$
 lvUnits��$	��100��$y�	Collapsed=� �� View �� .P���  �� Width$	 200�� �� DisplayMemberBinding �� *
CallSign+���� -   
 � Tag$ CallSign��.��	Call Sign�� $	 200�� �� *
UnitType+���� -   $ UnitType��.��	Unit Type(� $��1��$��7��$ Uniform=�$��5 5 5 5q�.���� -   $ btnSettings��$��Stretch=�$��Stretch=�
������ ��Z� ���� .R��� $��Center=�$��Center=�._��� $��Icon/Settings.png��$	 HighQuality=�(� $��2��$��7��$ Uniform=�$��5 5 5 5q�.��d� -    d�Name$ 
btnConnect��$��Stretch=�$��Stretch=�
������ ��Z� ����  �� 
TargetType" d�.R��� $��Center=�$��Center=�._���  ��Name$ 
imgConnect��$��Icon/Logout.png��$	 HighQuality=� �� Triggers P�  P� Property �@d�	IsChecked P� Value$
 TrueY�. ���  �� 
TargetName$ 
imgConnect�� �� Property � �� �� Value$ Icon/Login.png��� -   $ panelStatus��$��0��$��0��$��7��$��3�� �� $	��75* ��� $	��30* ��� $
��100* ��� $
��100* � �� $
��250* ��� $
��250* �.+�� $��0��$��0��$��2��.+�(� $��Center=�$��Center=�.���� .+��� -   $ txtConsoleName��$�� ��#��� ��$��Cambria)�$��5 5 5 5q�� $��1��$��0��$��2��.+�(� $��Center=�$��Center=�.���� .+��� -   $ txtStatsTitle��$��Today Statistic��#��� ��$��Cambria)�$��5 2 5 2q�� $��2��$��0��$��20q� �� $	��70* ��� $	��30* �.+���  �� RadiusX$ 5�� �� RadiusY$ 5��#��� ��
�������$��1��$��2�� J� BitmapEffect F�  F� ShadowDepth$ 10Q� F� Softness$ 0Q� F� Color$ 	LightGray��� $��0��$��0��.+�(� $��Center=�$��Center=�.���� .+��� -   $ txtIncidentAmt��$��0��$
��Bold"�
���   �$��Cambria)�� $��1��$��0��.+�(� $��Center=�$��Center=�.���� .+��� $��Incident��$
��Bold"�
���   �$��Cambria)�$��2 0 2 2q�� $��2��$��1��$��20q� �� $	��70* ��� $	��30* �.+��� $ 5��$ 5��#��� ��
�������$��1��$��2�� F� $ 10Q�$ 0Q�$ 	LightGray��� $��0��$��0��.+�(� $��Center=�$��Center=�.���� .+��� -   $ 	txtAckAmt��$��0��$
��Bold"�
���   �$��Cambria)�� $��1��$��0��.+�(� $��Center=�$��Center=�.���� .+��� $��Acknowledged��$
��Bold"�
���   �$��Cambria)�$��2 0 2 2q�� $��3��$��0��$��20q� �� $	��70* ��� $	��30* �.+��� $ 5��$ 5��#��� ��
�������$��1��$��2�� F� $ 10Q�$ 0Q�$ 	LightGray��� $��0��$��0��.+�(� $��Center=�$��Center=�.���� .+��� -   $ txtRejectAmt��$��0��$
��Bold"�
���   �$��Cambria)�� $��1��$��0��.+�(� $��Center=�$��Center=�.���� .+��� $��Rejected��$
��Bold"�
���   �$��Cambria)�$��2 0 2 2q�� $��3��$��1��$��20q� �� $	��70* ��� $	��30* �.+��� $ 5��$ 5��#��� ��
�������$��1��$��2�� F� $ 10Q�$ 0Q�$ 	LightGray��� $��0��$��0��.+�(� $��Center=�$��Center=�.���� .+��� -   $ txtFailedAmt��$��0��$
��Bold"�
���   �$��Cambria)�� $��1��$��0��.+�(� $��Center=�$��Center=�.���� .+��� $��Failed��$
��Bold"�
���   �$��Cambria)�$��2 0 2 2q�� -   $ panelConnection��$��0��$��0��$��9��$��3��$y�	Collapsed=� �� $
��150* ��� $	��50* ��� $
��150* �.+��� $��0��
���   � J� Opacity$	 0.5Q�� $��1��
���   �$	 0.8Q�.+�(� $��Center=�$��Center=�.���� .+��� $$��Lost of Internet Connection...��$
��Wrap=�#��� ��$��Cambria)�$��5 2 5 2q��� $��2��
���   �$	 0.5Q�