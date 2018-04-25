using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.Gui.Views;
using WSGRI.DigitalFactory.Commands; 
using WSGRI.DigitalFactory.DFEditorLib;
using WSGRI.DigitalFactory.DFFunction;
/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����SelectFeature.cs
			// �ļ�����������ѡ��Ҫ��
			//
			// 
			// ������ʶ��YuanHY 20060109
            // ����˵�������������ѡ��
            //����������    
----------------------------------------------------------------*/
namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// ModifyPropertyCopy ��ժҪ˵����
	/// </summary>
	public class SelectFeature:AbstractMapCommand
	{
		private IDFApplication m_App;
		private IMap           m_FocusMap;
		private ILayer         m_CurrentLayer;
		private IActiveView    m_pActiveView;
		private IMapView       m_MapView = null;

		private IPoint m_pPoint;
		private bool   m_bIsUse;
		private INewEnvelopeFeedback  m_pFeedbackEnve; //���ο���ʾ����

		public override string Caption
		{
			get
			{
				return "SelectFeature";
			}
			set
			{
				
			}
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
			m_pActiveView = m_App.CurrentMapControl.ActiveView;
			m_FocusMap = m_pActiveView.FocusMap;
			if (m_MapView == null)
			{
				return;
			}
			else
			{
				//���¼�
				m_MapView.CurrentTool = this;
			}
    
			CurrentTool.m_CurrentToolName  = CurrentTool.CurrentToolName.selectFeature;
			
			CommonFunction.MapRefresh(m_pActiveView);

		}
          
		public override void UnExecute()
		{
			// TODO:  ��� DrawLine.UnExecute ʵ��

		}

		public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawRectBorder2P.OnMouseDown ʵ��
			base.OnMouseDown (button, shift, x, y, mapX, mapY);

			if (CurrentTool.m_CurrentToolName == CurrentTool.CurrentToolName.selectFeature) 
			{
				m_CurrentLayer = m_App.CurrentEditLayer;
			}
			else
			{
				return;
			}			

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

			m_bIsUse = true;

		}

		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� ModifyPropertyMatch.OnMouseMove ʵ��
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

			if(!m_bIsUse) return;
           
			if (m_pFeedbackEnve == null ) 
			{
				m_pFeedbackEnve = new NewEnvelopeFeedbackClass();
				m_pFeedbackEnve.Display = m_pActiveView.ScreenDisplay;
				m_pFeedbackEnve.Start(m_pPoint);
			}

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
			m_pFeedbackEnve.MoveTo(m_pPoint);
		}
	
		public override void OnMouseUp(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� ModifyPropertyMatch.OnMouseUp ʵ��
			base.OnMouseUp (button, shift, x, y, mapX, mapY);

			if (m_bIsUse)
			{
				IGeometry pEnv;
				m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection , null, null);
				if (m_pFeedbackEnve !=null)
				{
					pEnv = m_pFeedbackEnve.Stop();
					m_FocusMap.SelectByShape(pEnv, null, false);
				}
				else
				{
					IEnvelope pRect ;
					double dblConst ;
					dblConst =CommonFunction.ConvertPixelsToMapUnits(m_pActiveView,8);//    '8�����ش�С
					pRect = new EnvelopeClass();
					pRect.XMin = m_pPoint.X - dblConst; // �����߽�Ŀ��Ϊ16�����ش�С
					pRect.YMin = m_pPoint.Y - dblConst;
					pRect.XMax = m_pPoint.X + dblConst;
					pRect.YMax = m_pPoint.Y + dblConst;
					m_FocusMap.SelectByShape(pRect,null, false);

				}

				m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
				m_pFeedbackEnve = null;
				m_bIsUse = false;
			}

		}
	}
}

