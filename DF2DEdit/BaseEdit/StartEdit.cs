using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase; 

using WSGRI.DigitalFactory.Commands;
using WSGRI.DigitalFactory.Gui.Views;
using WSGRI.DigitalFactory.Gui;
using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.DFEditorLib;
using WSGRI.DigitalFactory.DFFunction;

using Infragistics.Win; 
using Infragistics.Win.UltraWinToolbars; 
/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����StartEdit.cs
			// �ļ�������������ʼ�༭
			//
			// 
			// ������ʶ��YuanHY 20060109
            // ����˵����
            //����������    
----------------------------------------------------------------*/
namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// StartEdit ��ժҪ˵����
	/// </summary>
	public class StartEdit: AbstractMapCommand
	{		
		private IDFApplication	m_App;
		private IMapControl2	m_MapControl;
		private IMap			m_FocusMap;
		private IMapView		m_MapView = null;
	    private bool            isEnabled = true;

		public StartEdit()
		{

		}

		#region �������
		public override bool IsEnabled
		{
			get 
			{
                if (((IDFApplication)this.Hook).Workbench.CommandBarManager.Tools["2dmap.DFEditorTool.Start"].SharedProps.Enabled == true)
				{
					return true;
				}
				else
				{
					return false;
				}		
 			}
			set 
			{
				isEnabled = value;
			}
		}

		public override string Caption
		{
			get
			{
				return "StartEdit";
			}
			set
			{
				
			}
		}
		#endregion

		public override void UnExecute()
		{
			// TODO:  ��� StartEdit.UnExecute ʵ��
		}
	
		public override void Execute()
		{
			if (!(this.Hook is IDFApplication))
			{
				return;
			}
			else
			{
				m_App = (IDFApplication)this.Hook;
			}
           
			m_MapView = m_App.Workbench.GetView(typeof(MapView)) as IMapView;
			if (m_MapView == null)
			{
				return;
			}
			else
			{
				//���¼�
				//m_MapView.CurrentTool = this;
			}

			m_MapControl   = m_App.CurrentMapControl;

			m_FocusMap     = m_App.CurrentMapControl.ActiveView.FocusMap;		         
            
			CommonFunction.StartEditing(m_FocusMap);
            m_App.IsModifyWorkSpace = true;

			UltraToolbarsManager tbManager;
			tbManager = m_App.Workbench.CommandBarManager;
			this.isEnabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Start"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Save"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.DFEditorTool.Stop"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.DFEditorTool.CurrentEditLayerLabel"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.DFEditorTool.CurrentEditLayer"].SharedProps.Enabled = true;
            //tbManager.Tools["2dmap.DFEditorTool.Modi.ConstructGeoObj"].SharedProps.Enabled = true;
            //tbManager.Tools["2dmap.DFEditorTool.Modi.ConstructGeoObjSelect"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.Select.Layer.EditFeature"].SharedProps.Enabled = true;
							
			tbManager2dmapeditEnabledOrNot();

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "��ʼ�༭";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

		}

		#region//�༭��ť���Ƿ����
		private void tbManager2dmapeditEnabledOrNot()
		{
			UltraToolbarsManager tbManager;
			tbManager = m_App.Workbench.CommandBarManager;
          
			try
			{

				ComboBoxTool tbUltraCombo;
                tbUltraCombo = (ComboBoxTool)tbManager.Tools["2dmap.DFEditorTool.CurrentEditLayer"];	

				m_App.CurrentEditLayer = tbUltraCombo.Value as ILayer ;
    				
				//WorkbenchSingleton.Workbench.UpdateTools(); 

				if(m_App.CurrentEditLayer is IAnnotationLayer)
				{
					for(int i =0; i<tbManager.Toolbars["map.annotation"].Tools.Count;i++)
					{
						tbManager.Toolbars["map.annotation"].Tools[i].SharedProps.Enabled= true; 
					}

				}
				else
				{
					for(int i =0; i<tbManager.Toolbars["map.annotation"].Tools.Count;i++)
					{
						tbManager.Toolbars["map.annotation"].Tools[i].SharedProps.Enabled= false; 
					}
				}
    		
			}
			catch{}

            tbManager.Tools["2dmap.DFEditorTool.Save"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.DFEditorTool.Stop"].SharedProps.Enabled = true;
			
			

//			//�༭����������Ϊ������
//			for(int i =0; i<tbManager.Toolbars["2dmap.edit"].Tools.Count;i++)
//			{
//				tbManager.Toolbars["2dmap.edit"].Tools[i].SharedProps.Enabled= true; 
//				Infragistics.Win.UltraWinToolbars.ToolBase toolBase = tbManager.Toolbars["2dmap.edit"].Tools[i];
//				Type typeOfToolBase = toolBase.GetType();
//				if (typeOfToolBase == typeof(PopupMenuTool))
//				{
//					for (int j = 0; j < ((PopupMenuTool)toolBase).Tools.Count; j++)//������ť��
//					{
//						((PopupMenuTool)toolBase).Tools[j].SharedProps.Enabled = true;
//					}
//				}
//
//			}
//			tbManager.Tools["2dmap.edit.start"].SharedProps.Enabled = false;
//
//			//�༭����������Ϊ������
//			for(int i =0; i<tbManager.Toolbars["2dmap.edit.advanced"].Tools.Count;i++)
//			{
//				tbManager.Toolbars["2dmap.edit.advanced"].Tools[i].SharedProps.Enabled= true; 
//
//			}		

		}

		#endregion


	}
}
