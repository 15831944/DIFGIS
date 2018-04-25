/*---------------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawCircleCentRad.cs
			// �ļ���������������:Բ������p + �뾶������Բ\Բ������
			//
			// 
			// ������ʶ��LuoXuan20171010
            // ����˵����
			//           A�������������
			//           B������Բ�ġ��뾶
			//���������� ESC��ȡ�����в���
			//           ENTER����SPACEBAR����������
            //
-------------------------------------------------------------------------*/
using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

using DF2DControl.Base;
using DF2DControl.Command;
using DF2DControl.UserControl.View;
using DFWinForms.Service;
using DF2DEdit.Form;
using DevExpress.XtraEditors;

namespace DF2DEdit.CmdDraw
{
	/// <summary>
	/// DrawCircleCentRad ��ժҪ˵����
	/// </summary>
    public class DrawCircleCentRad : AbstractMap2DCommand
	{
        private DF2DApplication m_App;
        private IMapControl2 m_MapControl;
        private IMap           m_FocusMap;
        private ILayer         m_CurrentLayer;
        private IActiveView    m_pActiveView;

		private IDisplayFeedback   m_pFeedback;
		private INewCircleFeedback m_pCircleFeed;

        private bool   m_bInUse;
		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private IPoint        m_pLastPoint;

		public  static IPoint m_pCenterPoint  = new PointClass();
        public  static bool   m_bFixRadius;
        public  static double m_dblRadius;
		public  static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��

		private double m_dblTolerance;     //�̶�����ֵ
		private IPoint   m_BeginConstructParallelPoint;//��ʼƽ�гߣ�����һ��ĵ�
		private IEnvelope m_pEnvelope = new EnvelopeClass();

        public override void Run(object sender, System.EventArgs e)
        {
            Map2DCommandManager.Push(this);
            IMap2DView mapView = UCService.GetContent(typeof(Map2DView)) as Map2DView;
            if (mapView == null) return;
            bool bBind = mapView.Bind(this);
            if (!bBind) return;

            m_App = DF2DApplication.Application;
            if (m_App == null || m_App.Current2DMapControl == null) return;

            m_MapControl = m_App.Current2DMapControl;
            m_FocusMap = m_MapControl.ActiveView.FocusMap;
            m_pActiveView = (IActiveView)this.m_FocusMap;
            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            m_dblTolerance = Class.Common.ConvertPixelsToMapUnits(m_MapControl.ActiveView, 4);

            Class.Common.MapRefresh(m_pActiveView);
            m_App.Workbench.SetStatusInfo("��ʾ������ָ��1.Բ��;2.Բ���ϵ�һ�㡣(A:����XY/ESC:ȡ��/ENTER:����)");//��״̬��������ʾ��Ϣ
        }

        public override void RestoreEnv()
        {
            IMap2DView mapView = UCService.GetContent(typeof(Map2DView)) as Map2DView;
            if (mapView == null) return;
            mapView.UnBind(this);
            DF2DApplication app = DF2DApplication.Application;
            if (app == null || app.Current2DMapControl == null) return;
            app.Current2DMapControl.MousePointer = esriControlsMousePointer.esriPointerDefault;
            Map2DCommandManager.Pop();

            Reset();
        }

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawCircleCentRad.OnMouseDown ʵ��
            base.OnMouseDown (button, shift, x, y, mapX, mapY);

            m_App.Workbench.SetStatusInfo("����ָ��:1.Բ��;2.Բ���ϵ�һ�㡣(A:����XY/ESC:ȡ��/ENTER:����)");//��״̬��������ʾ��Ϣ

            m_CurrentLayer = Class.Common.CurEditLayer;
			
			//�����Ƿ񳬳���ͼ��Χ
			if(Class.Common.PointIsOutMap(m_CurrentLayer,m_pAnchorPoint) == true)
			{
				DrawCircleCentRadMouseDown(m_pAnchorPoint,shift);  
			}
			else
			{
				XtraMessageBox.Show("������ͼ��Χ");
			}	
			 

        }

        //private void EndDrawCircleCentRadWihtShift()
        //{
        //    frmCentRad formCentRad = new frmCentRad();
        //    formCentRad.ShowDialog();

        //    if( m_bFixRadius )
        //    {
        //        IGeometry pGeom = null;
        //        IPolyline pPolyline;
        //        IPolygon  pPolygon;

        //   IPoint pPoint = new PointClass();
        //        pPoint.X = m_pCenterPoint.X + m_dblRadius;
        //        pPoint.Y = m_pCenterPoint.Y;
       
        //        pPolyline = CommonFunction.ArcToPolyline(pPoint, m_pCenterPoint, pPoint,esriArcOrientation.esriArcClockwise);
   
        //        switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
        //        {
        //            case  esriGeometryType.esriGeometryPolyline:  
        //                pGeom = pPolyline; 
        //                break;
        //            case esriGeometryType.esriGeometryPolygon:
        //                pPolygon  =  CommonFunction.PolylineToPolygon(pPolyline);
        //                pGeom = pPolygon;              ����                          
        //                break;
        //            default:
        //                break;
        //        }//end switch

        //        m_pEnvelope = pGeom.Envelope;
        //        if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);;
        //        CommonFunction.CreateFeature(m_App.Workbench,pGeom, m_FocusMap, m_CurrentLayer);

        //        m_App.Workbench.CommandBarManager.Tools["2dmap.DFEditorTool.Undo"].SharedProps.Enabled = true;    
                   
        //    }

        //    Reset();
        //}

		private void DrawCircleCentRadMouseDown(IPoint pPoint,int shift)
		{			
			Class.Common.DrawPointSMSSquareSymbol(m_MapControl,pPoint);	

			if(!m_bInUse)//�������û��ʹ��
			{ 
				m_bInUse = true;
				m_pCenterPoint = pPoint;

				m_pFeedback = new NewCircleFeedbackClass();
				m_pCircleFeed = (NewCircleFeedbackClass)m_pFeedback;
				m_pCircleFeed.Display = m_pActiveView.ScreenDisplay;             
				m_pCircleFeed.Start(m_pCenterPoint);            
			}
			else //��������Ѿ�ʹ��ʹ��
			{
				IGeometry pGeom = null;
				IPolyline pPolyline;
				IPolygon  pPolygon;
				ICircularArc pCircularArc = new CircularArcClass();

                //if (shift == 1)//������סshift�������Ի������û��޸�Բ���ϵ�����ֵ
                //{
                //     EndDrawCircleCentRadWihtShift();
                //}
                //else
                //{
                    m_pFeedback.MoveTo(pPoint);
					pCircularArc = m_pCircleFeed.Stop();
					m_dblRadius= pCircularArc.Radius;

					switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
					{
						case  esriGeometryType.esriGeometryPolyline:  
							pPolyline = Class.Common.ArcToPolyline(pCircularArc.FromPoint, pCircularArc.CenterPoint, pCircularArc.FromPoint,esriArcOrientation.esriArcClockwise);
							pGeom = pPolyline; 
							break;
						case esriGeometryType.esriGeometryPolygon:
							pPolyline = Class.Common.ArcToPolyline(pCircularArc.FromPoint, pCircularArc.CenterPoint, pCircularArc.FromPoint,esriArcOrientation.esriArcClockwise);
							pPolygon  =  Class.Common.PolylineToPolygon(pPolyline);
							pGeom = pPolygon;              ����                          
							break;
						default:
							break;
					}//end switch

					m_pEnvelope = pGeom.Envelope;
					if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);

					Class.Common.CreateFeature(pGeom, m_FocusMap, m_CurrentLayer);
                    m_App.Workbench.UpdateMenu();   

					Reset();

                //}
			}

			m_pLastPoint = pPoint;
		}

   
        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawCircleCentRad.OnMouseMove ʵ��
            base.OnMouseMove (button, shift, x, y, mapX, mapY);

			
			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
			
			m_pAnchorPoint = m_pPoint;

			//+++++++++++++��ʼ��׽+++++++++++++++++++++	
            //if(m_pCenterPoint.IsEmpty)
            //{
            //    bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,null,m_pAnchorPoint);
            //}
            //else
            //{
            //    bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,m_pCenterPoint,m_pAnchorPoint);
            //}
        }

        public override void OnBeforeScreenDraw(int hdc)
        {
            // TODO:  ��� DrawCircleCentRad.OnBeforeScreenDraw ʵ��
            base.OnBeforeScreenDraw (hdc);
           
            if (m_pFeedback !=null)  
            {
                m_pFeedback.MoveTo(m_pAnchorPoint);              
            }    
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            // TODO:  ��� DrawCircleCentRad.OnKeyDown ʵ��
            base.OnKeyDown (keyCode, shift);
            
			if (keyCode == 65)//��A��,�����������
			{    				
				frmAbsXYZ.m_pPoint = m_pAnchorPoint;
				frmAbsXYZ formXYZ = new frmAbsXYZ();
				formXYZ.ShowDialog();

				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                    
					DrawCircleCentRadMouseDown(m_pAnchorPoint,0);
				}


				return;
			}


			if ((keyCode == 13 || keyCode == 32) && m_bInUse)//��ENTER ����SPACEBAR ��
			{   
				DrawCircleCentRadMouseDown(m_pAnchorPoint,shift); 
 
				return;
			
			}

			if (keyCode == 27 )//ESC ����ȡ�����в���
			{
				Reset();

                DF2DApplication.Application.Workbench.BarPerformClick("Pan");

                return;
			}			
        }

		private void Reset()
		{
			m_bFixRadius = false;
			m_bInUse = false;
			m_bInputWindowCancel = true;
			m_pCircleFeed = null;
			if(m_pCenterPoint != null) m_pCenterPoint.SetEmpty();
			m_pFeedback = null;
			if(m_pLastPoint != null) m_pLastPoint.SetEmpty();

			m_pActiveView.GraphicsContainer.DeleteAllElements();
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��

            m_App.Workbench.SetStatusInfo("����");

		}
    }
}
