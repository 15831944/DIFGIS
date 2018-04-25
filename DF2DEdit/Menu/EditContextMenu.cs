using System;
using Infragistics.Win.UltraWinToolbars ;
/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����EditContextMenu.cs
			// �ļ���������:���ơ��޸�ʱ�Ҽ����������ݲ˵�
			//
			// 
			// ������ʶ��YuanHY 20060329
            // ����˵����
			// �޸ļ�¼��
----------------------------------------------------------------*/
namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// EditContextMenu ��ժҪ˵����
	/// </summary>
	public class EditContextMenu
	{
		public UltraToolbarsManager toolbarsManager = null;
		
		public EditContextMenu()
		{
			toolbarsManager = new UltraToolbarsManager();
		
			this.createDrawContextMenu();
		}

		//�������ݲ˵�
		private void createDrawContextMenu()
		{
			ButtonTool btnUndo       = new ButtonTool("btnUndo");
			ButtonTool btnLeftCorner = new ButtonTool("btnLeftCorner");
			ButtonTool btnFixAzim    = new ButtonTool("btnFixAzim");
			ButtonTool btnLengthAzim = new ButtonTool("btnLengthAzim");
			ButtonTool btnSideLength = new ButtonTool("btnSideLength");
			ButtonTool btnFixLength  = new ButtonTool("btnFixLength");
			ButtonTool btnAbsXYZ     = new ButtonTool("btnAbsXYZ");			
			ButtonTool btnRelaXYZ    = new ButtonTool("btnRelaXYZ");
			ButtonTool btnParllel    = new ButtonTool("btnParllel");
			ButtonTool btnRt         = new ButtonTool("btnRt");	
			ButtonTool btnColse      = new ButtonTool("btnColse");
			ButtonTool btnEnd        = new ButtonTool("btnEnd");
			ButtonTool btnESC        = new ButtonTool("btnESC");
		 
			btnUndo.SharedProps.Caption       = "������(&U)";
			btnLeftCorner.SharedProps.Caption = "�������۽�(&N)...";
			btnFixAzim.SharedProps.Caption    = "���뷽λ��(&O)...";
			btnFixLength.SharedProps.Caption  = "���볤��(&D)...";
			btnLengthAzim.SharedProps.Caption = "����+��λ��(&F)..";
			btnSideLength.SharedProps.Caption = "���α߳�(&B)...";
			btnAbsXYZ.SharedProps.Caption     = "��������(&A)...";
			btnRelaXYZ.SharedProps.Caption    = "�������(&R)...";	
			btnParllel.SharedProps.Caption    = "ƽ��(&P)...";	
			btnRt.SharedProps.Caption         = "ֱ��(&S)...";			
			btnColse.SharedProps.Caption      = "������(&C)";
			btnEnd.SharedProps.Caption        = "���(&E)";
			btnESC.SharedProps.Caption        = "ȡ��(ESC)";


			PopupMenuTool drawPopupMenuTool  = new PopupMenuTool("drawPopupMenuTool");
			drawPopupMenuTool.Tools.Add(btnUndo);
			drawPopupMenuTool.Tools.Add(btnLeftCorner);
			drawPopupMenuTool.Tools.Add(btnFixAzim);
			drawPopupMenuTool.Tools.Add(btnFixLength);
			drawPopupMenuTool.Tools.Add(btnLengthAzim);
			drawPopupMenuTool.Tools.Add(btnSideLength);
			drawPopupMenuTool.Tools.Add(btnAbsXYZ);
			drawPopupMenuTool.Tools.Add(btnRelaXYZ);	
			drawPopupMenuTool.Tools.Add(btnParllel);
			drawPopupMenuTool.Tools.Add(btnRt);	
			drawPopupMenuTool.Tools.Add(btnColse);
			drawPopupMenuTool.Tools.Add(btnEnd);
			drawPopupMenuTool.Tools.Add(btnESC);

			PopupMenuTool modifyPopupMenuTool = new PopupMenuTool("modifyPopupMenuTool");
			modifyPopupMenuTool.Tools.Add(btnFixAzim);
			modifyPopupMenuTool.Tools.Add(btnFixLength);
			modifyPopupMenuTool.Tools.Add(btnParllel);
			modifyPopupMenuTool.Tools.Add(btnESC);			

			
			drawPopupMenuTool.Tools["btnLeftCorner"].InstanceProps.IsFirstInGroup = true;
			drawPopupMenuTool.Tools["btnFixAzim"].InstanceProps.IsFirstInGroup    = true;
			drawPopupMenuTool.Tools["btnSideLength"].InstanceProps.IsFirstInGroup = true;
			drawPopupMenuTool.Tools["btnAbsXYZ"].InstanceProps.IsFirstInGroup     = true;
			drawPopupMenuTool.Tools["btnParllel"].InstanceProps.IsFirstInGroup    = true;
			drawPopupMenuTool.Tools["btnColse"].InstanceProps.IsFirstInGroup      = true;

			modifyPopupMenuTool.Tools["btnParllel"].InstanceProps.IsFirstInGroup   = true;
			modifyPopupMenuTool.Tools["btnESC"].InstanceProps.IsFirstInGroup       = true;


			toolbarsManager.Tools.Add(btnUndo);
			toolbarsManager.Tools.Add(btnLeftCorner);
			toolbarsManager.Tools.Add(btnFixAzim);
			toolbarsManager.Tools.Add(btnFixLength);
			toolbarsManager.Tools.Add(btnLengthAzim);
			toolbarsManager.Tools.Add(btnSideLength);
			toolbarsManager.Tools.Add(btnAbsXYZ);
			toolbarsManager.Tools.Add(btnRelaXYZ);
			toolbarsManager.Tools.Add(btnParllel);
			toolbarsManager.Tools.Add(btnRt);
			toolbarsManager.Tools.Add(btnColse);
			toolbarsManager.Tools.Add(btnEnd);
			toolbarsManager.Tools.Add(btnESC);
			toolbarsManager.Tools.Add(drawPopupMenuTool);
			toolbarsManager.Tools.Add(modifyPopupMenuTool);

		}

		public void ActiveEditContextMenu(string menuStr, System.Windows.Forms.Control control)
		{
			toolbarsManager.ShowPopup(menuStr,control);
		}


	}
}
