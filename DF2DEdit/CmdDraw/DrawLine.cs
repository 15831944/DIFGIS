/*---------------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawLine.cs
			// �ļ�������������������\��
			//
			// 
			// ������ʶ��LuoXuan
            // ����˵����U������
			//           A�������������
			//���������� C����ս���
            //           E��\Enter��\Space������            
			//           ESC��ȡ�����в���
            // �޸�������
-----------------------------------------------------------------------*/

using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

using DF2DControl.Base;
using DF2DControl.Command;
using DF2DControl.UserControl.View;
using DFWinForms.Service;
using DF2DEdit.Form;
using DevExpress.XtraEditors;

namespace DF2DEdit.CmdDraw
{
	/// <summary>
	/// DrawLine ��ժҪ˵����
	/// </summary>
    public class DrawLine : AbstractMap2DCommand
	{
        private DF2DApplication m_App;      
		private IMapControl2   m_MapControl;
		private IMap           m_FocusMap;
		private ILayer         m_CurrentLayer;
		private IActiveView    m_pActiveView; 

		private IDisplayFeedback m_pFeedback;
		private IDisplayFeedback m_pLastFeedback;
		private INewLineFeedback m_pLineFeed;
		private INewLineFeedback m_pLastLineFeed;

		private bool          m_bInUse;
		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private IPoint        m_pLastPoint;
		public static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��
		private double   m_dblTolerance;       //�̶�����ֵ
		private IArray   m_pUndoArray = new ArrayClass();	
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
            m_App.Workbench.SetStatusInfo("��ݼ���ʾ��U:����/A:��������XY/C:��ս���/Enter:����/ESC:ȡ��");//��״̬��������ʾ��Ϣ
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
			// TODO:  ��� DrawLine.OnMouseDown ʵ��
			base.OnMouseDown (button, shift, x, y, mapX, mapY);

            m_CurrentLayer = Class.Common.CurEditLayer;

            if (Class.Common.PointIsOutMap(m_CurrentLayer, m_pAnchorPoint) == true)
            {
				DrawLineMouseDown(m_pAnchorPoint);	
			}
			else
			{
				XtraMessageBox.Show("������ͼ��Χ");
			}
		}

		private void DrawLineMouseDown(IPoint pPoint )
		{   			     
			if(!m_bInUse)//�������û��ʹ��
			{ 
				m_bInUse = true; 
  
				m_pUndoArray.Add(pPoint);

				m_pLastPoint = pPoint;

				Class.Common.DrawPointSMSSquareSymbol(m_MapControl,pPoint);

				m_pFeedback = new NewLineFeedbackClass(); 
				m_pLineFeed = (INewLineFeedback)m_pFeedback;
				m_pLineFeed.Start(pPoint);
				if( m_pFeedback != null)  
                    m_pFeedback.Display = m_pActiveView.ScreenDisplay;

				if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
				{��
					//����ǰͼ������㣬����ʾ������ʼ�㵽������߶�
					m_pLastFeedback = new NewLineFeedbackClass();
					m_pLastLineFeed = (INewLineFeedback)m_pLastFeedback;
					m_pLastLineFeed.Start(pPoint);
					if( m_pLastFeedback != null)  m_pLastFeedback.Display = m_pActiveView.ScreenDisplay;
				}

			}
			else//����������ʹ����
			{
				m_pLineFeed.Stop();
				m_pLineFeed.Start(pPoint);
                 
				IPoint tempPoint = new PointClass();
				tempPoint.X = pPoint.X;
				tempPoint.Y = pPoint.Y;              
				m_pUndoArray.Add(tempPoint);
                
				m_pLastPoint = m_pAnchorPoint;

				Class.Common.DisplaypSegmentColToScreen(m_MapControl, ref m_pUndoArray);//����ˢ����Ļ��
      
			}
		}
        
		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawLine.OnMouseMove ʵ��
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

            m_App.Workbench.SetStatusInfo("��ݼ���ʾ��U:����/A:��������XY/C:��ս���/Enter:����/ESC:ȡ��");//��״̬��������ʾ��Ϣ

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

            m_pAnchorPoint = m_pPoint;
            //+++++++++++++��ʼ��׽+++++++++++++++++++++			
            //bool flag = CommonFunction.Snap(m_MapControl, m_App.CurrentConfig.cfgSnapEnvironmentSet, (IGeometry)m_pLastPoint, m_pAnchorPoint);

            if (!m_bInUse) return;

			m_pFeedback.MoveTo(m_pAnchorPoint);
    
			if((m_pUndoArray.Count > 1) && ((((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)))
			{
				if( m_pLastFeedback != null)  m_pLastFeedback.Display = m_pActiveView.ScreenDisplay;
				m_pLastFeedback.MoveTo(m_pAnchorPoint);
			}

		}
  
		public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawLine.OnDoubleClick ʵ��
			base.OnDoubleClick (button, shift, x, y, mapX, mapY);
            
			EndDrawLine();
		}

		public void EndDrawLine()
		{       
			IGeometry pGeom = null;
			IPolyline pPolyline;
			IPolygon pPolygon;
			IPointCollection pPointCollection;
	
			//��������ӽ�����
			if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
			{
				m_pUndoArray.Add(m_pUndoArray.get_Element(0));		��
			}

			pPolyline =(IPolyline)Class.Common.MadeSegmentCollection(ref m_pUndoArray);
                         
			if(m_bInUse)
			{            
				switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
				{
					case  esriGeometryType.esriGeometryPolyline:
						pPointCollection =(IPointCollection)pPolyline;                 
						if(pPointCollection.PointCount < 2)
						{
							MessageBox.Show("���ϱ�����������!");
						}
						else
						{
							pGeom = (IGeometry)pPointCollection;                          
						}
						break;
					case  esriGeometryType.esriGeometryPolygon:                     
						pPolygon= Class.Common.PolylineToPolygon(pPolyline);
						pPointCollection =(IPointCollection) pPolygon;
						pGeom = (IGeometry)pPointCollection;
						if(pPointCollection.PointCount < 3)
						{
							MessageBox.Show("���ϱ�����������!");
						}
						else
						{
							pGeom = (IGeometry)pPointCollection;                            
						}
						break;
					default:
						break;
             
				}

				m_pEnvelope = pGeom.Envelope;
				if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);

				Class.Common.CreateFeature(pGeom, m_FocusMap, m_CurrentLayer);
                m_App.Workbench.UpdateMenu();   

				Reset();//��λ  
			

			} 
		}
     
		private void  Undo()
		{
			if(m_pUndoArray.Count >1)
			{
				m_pEnvelope = Class.Common.GetMinEnvelopeOfTheArray(m_pUndoArray);
			}
			else if(m_pUndoArray.Count ==1)
			{
				IPoint pTempPoint = new PointClass();
				pTempPoint.X = (m_pUndoArray.get_Element(0) as Point).X;
				pTempPoint.Y = (m_pUndoArray.get_Element(0) as Point).Y;

				m_pEnvelope.Width  = Math.Abs(m_pPoint.X - pTempPoint.X);
				m_pEnvelope.Height = Math.Abs(m_pPoint.Y - pTempPoint.Y);

				pTempPoint.X = (pTempPoint.X + m_pPoint.X)/2;
				pTempPoint.Y = (pTempPoint.Y + m_pPoint.Y)/2;

				m_pEnvelope.CenterAt(pTempPoint);				

			}
			if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);

			IPoint pPoint = new PointClass();
			pPoint = (IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count-1);
			IEnvelope enve = new EnvelopeClass();
			enve =Class.Common.NewRect(pPoint,m_dblTolerance);

			IEnumElement  pEnumElement = m_MapControl.ActiveView.GraphicsContainer.LocateElementsByEnvelope(enve);
			if (pEnumElement != null)
			{
				pEnumElement.Reset();
				IElement pElement = pEnumElement.Next();

				while(pElement!=null)
				{
					m_MapControl.ActiveView.GraphicsContainer.DeleteElement(pElement);
					pElement = pEnumElement.Next();
				}
			}
				
			m_pUndoArray.Remove(m_pUndoArray.Count-1);//ɾ�����������һ����  
            
			//��Ļˢ��
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_pEnvelope);//��ͼˢ��
			m_pActiveView.ScreenDisplay.UpdateWindow();
       
			//��ʼ����λ����
			if (m_pUndoArray.Count!=0)
			{                
				Class.Common.DisplaypSegmentColToScreen(m_MapControl,ref m_pUndoArray);
   
				m_pLastPoint=(IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count-1);               

				m_pFeedback = new NewLineFeedbackClass(); 
				m_pLineFeed =(NewLineFeedback)m_pFeedback;
				m_pLineFeed.Display = m_pActiveView.ScreenDisplay;
				if (m_pLineFeed !=null) m_pLineFeed.Stop();
				m_pLineFeed.Start(m_pLastPoint);
				m_pLineFeed.MoveTo(m_pPoint);       
			}
			else 
			{   
				Reset(); //��λ
			}           
		}

		public override void OnBeforeScreenDraw(int hdc)
		{
			// TODO:  ��� DrawLine.OnBeforeScreenDraw ʵ��
			base.OnBeforeScreenDraw (hdc);
           
			if(m_pUndoArray.Count !=0)
			{
				IPoint pStartPoint = new PointClass();
				IPoint pEndPoint = new PointClass();
				pStartPoint = (IPoint)m_pUndoArray.get_Element(0);
				pEndPoint = (IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count -1);

				if (m_pLineFeed !=null)      m_pLineFeed.MoveTo(pEndPoint);
				if (m_pLastLineFeed !=null)  m_pLastLineFeed.MoveTo(pStartPoint);
			}      
		}

		//��λ
		public override void OnAfterScreenDraw(int hdc)
		{
			// TODO:  ��� DrawLine.OnAfterScreenDraw ʵ��
			base.OnAfterScreenDraw (hdc);
		}

		private void Reset()
		{
			m_pActiveView.FocusMap.ClearSelection();  
			m_pActiveView.GraphicsContainer.DeleteAllElements();//ɾ�������ĵ�ͼԪ��

			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��

            m_App.Workbench.SetStatusInfo("����");

			m_bInUse = false;
			if(m_pLastPoint != null) m_pLastPoint.SetEmpty();;
			m_pUndoArray.RemoveAll();//��ջ������� 
			m_pLineFeed =null;
			m_pLastLineFeed=null;
			m_bInputWindowCancel = true;
			m_pEnvelope = null;

		}

		#region �����¼�(����Ŀ�ݼ�)
		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� DrawLine.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);
         
			if (keyCode == 85 && m_bInUse)//��U��,����
			{
				Undo();                
				return;
			}
            
			if (keyCode == 65)//��A��,�����������
			{    				
				frmAbsXYZ.m_pPoint = m_pAnchorPoint;
				frmAbsXYZ formXYZ = new frmAbsXYZ();
				formXYZ.ShowDialog();

				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                    
					DrawLineMouseDown(m_pAnchorPoint);
				}

				return;
			}

			if (keyCode == 67 && m_pUndoArray.Count>=3)//��C��,��ս�������
			{
				if(m_bInUse)
				{
					IPoint pStartPoint = new PointClass();
					pStartPoint=(IPoint)m_pUndoArray.get_Element(0);
					m_pUndoArray.Add(pStartPoint);

					EndDrawLine();
				}  

				return;
			}

			if ((keyCode == 69 || keyCode == 13 || keyCode == 32) && m_bInUse && m_pUndoArray.Count>=2)//��E����ENTER ����SPACEBAR ����������
			{
				EndDrawLine();
                
				return;
			}

			if (keyCode == 27)//ESC ����ȡ�����в���
			{
				Reset();

                DF2DApplication.Application.Workbench.BarPerformClick("Pan");

				return;
			}

		}
		#endregion
	}
}
