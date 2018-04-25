/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����StopEdit.cs
			// �ļ�����������ֹͣ�༭
			//
			// 
			// ������ʶ��YuanHY 20060109
            // ����˵����
            //����������    
----------------------------------------------------------------*/

using System;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto; 

using WSGRI.DigitalFactory.Commands;
using WSGRI.DigitalFactory.Gui.Views;
using WSGRI.DigitalFactory.Gui;
using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.DFEditorLib;
using WSGRI.DigitalFactory.DFFunction;


using Infragistics.Win; 
using Infragistics.Win.UltraWinToolbars; 

namespace WSGRI.DigitalFactory.DFEditorTool
{
    /// <summary>
    /// StopEdit ��ժҪ˵����
    /// </summary>
    public class StopEdit: AbstractMapCommand
    {
		private IDFApplication	m_App;
		private IMapControl2	m_MapControl;
		private IMap			m_FocusMap;
		private IMapView		m_MapView = null;
		private bool            isEnabled=false;

		public StopEdit()
		{	
			this.IsEnabled = false;
		}
		#region �������

		public override bool IsEnabled
		{
			get 
			{
                if (((IDFApplication)this.Hook).Workbench.CommandBarManager.Tools["2dmap.DFEditorTool.Stop"].SharedProps.Enabled == true)
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
				return "StopEdit";
			}
			set
			{
				
			}
		}
		#endregion

        public override void UnExecute()
        {

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
           
           
            SaveEditing(m_FocusMap);
            m_App.IsModifyWorkSpace = false;

			tbManager2dmapeditEnabledOrNot();

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "ֹͣ�༭";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as ESRI.ArcGIS.Geodatabase.IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

        }

        private void SaveEditing(IMap pMap)
        {
            CommonFunction.StopEditing(pMap,true,true);

        }

		#region//�༭��ť���Ƿ����
		private void tbManager2dmapeditEnabledOrNot()
		{
			UltraToolbarsManager tbManager;
			tbManager = m_App.Workbench.CommandBarManager;
          
			//�༭����������Ϊ������
            for (int i = 0; i < tbManager.Toolbars["2dmap.DFEditorTool"].Tools.Count; i++)
			{
                tbManager.Toolbars["2dmap.DFEditorTool"].Tools[i].SharedProps.Enabled = false;
                Infragistics.Win.UltraWinToolbars.ToolBase toolBase = tbManager.Toolbars["2dmap.DFEditorTool"].Tools[i];
                Type typeOfToolBase = toolBase.GetType();
                if (typeOfToolBase == typeof(PopupMenuTool))
                {
                    for (int j = 0; j < ((PopupMenuTool)toolBase).Tools.Count; j++)//������ť��
                    {
                        ((PopupMenuTool)toolBase).Tools[j].SharedProps.Enabled = false;
                    }
                }
			}
            //((ComboBoxTool)tbManager.Tools["2dmap.DFEditorTool.CurrentEditLayer"]).ResetText();
            
            tbManager.Tools["2dmap.DFEditorTool.Modi.Operation"].SharedProps.Enabled = true;
			this.IsEnabled = false;

            tbManager.Tools["2dmap.DFEditorTool.Stop"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Save"].SharedProps.Enabled = false;
			isEnabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Setup"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.DFEditorTool.Start"].SharedProps.Enabled = true;
            tbManager.Tools["2dmap.DFEditorTool.CurrentEditLayer"].SharedProps.Enabled = false;
		
			//�༭����������Ϊ������
            for (int i = 0; i < tbManager.Toolbars["2dmap.DFEditorTool.Advanced"].Tools.Count; i++)
			{
                tbManager.Toolbars["2dmap.DFEditorTool.Advanced"].Tools[i].SharedProps.Enabled = false; 
			}	
            ////ע�ǹ���������Ϊ������
            //for(int i =0; i<tbManager.Toolbars["map.annotation"].Tools.Count;i++)
            //{
            //    tbManager.Toolbars["map.annotation"].Tools[i].SharedProps.Enabled= false; 
            //}
			//�༭����������Ϊ������
            tbManager.Tools["2dmap.DFEditorTool.Undo"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Redo"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Cut"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Copy"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Paste"].SharedProps.Enabled = false;
            tbManager.Tools["2dmap.DFEditorTool.Delete"].SharedProps.Enabled = false; 
				
			
		}

		#endregion
    }
}
