/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����EditSet.cs
			// �ļ������������༭����
			//
			// 
			// ������ʶ��YuanHY 20060109
            // ����˵����1��
            //����������    
----------------------------------------------------------------*/
using System;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto; 

using WSGRI.DigitalFactory.Commands;
using WSGRI.DigitalFactory.Gui.Views;
using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.DFSystem.DFConfig;
using WSGRI.DigitalFactory.DFFunction;

namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// EditSet ��ժҪ˵����
	/// </summary>
	public class EditSet:AbstractMapCommand
	{
		private IDFApplication m_App;
		private IMapControl2  m_MapControl;
		private IMapView m_MapView = null;

		//private bool	isEnabled   = true;
		private string	strCaption  = "�༭����" ;
		private string	strCategory = "�༭" ;

		public EditSet()
		{

		}

		#region �������
//		public override bool IsEnabled 
//		{
//			get 
//			{
//				return isEnabled;
//			}
//			set 
//			{
//				isEnabled = value;
//			}
//		}

		public override string Caption 
		{
			get
			{
				return strCaption;
			}
			set
			{
				strCaption = value ;
			}
		}

		public override string Category 
		{
			get
			{
				return strCategory;
			}
			set
			{
				strCategory = value ;
			}
		}
		#endregion

		public override void UnExecute()
		{
			// TODO:  ��� EditSet.UnExecute ʵ��

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
            WSGRI.DigitalFactory.DFEditorTool.frmEditSet fromEditSet = new frmEditSet();
			fromEditSet.m_CfgSet      = m_App.CurrentConfig;
			fromEditSet.m_pActiveView = m_App.CurrentMapControl.ActiveView; 

			fromEditSet.ShowDialog();            

		}
       
	}
}
