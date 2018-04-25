/*--------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawBeizerCurve.cs
			// �ļ��������������Ʊ���������\���߱ߵ���
			//
			// 
			// ������ʶ��YuanHY 20051226
            // ����˵����U������
			//           A�������������
            //     ����  R�������������
			//     ����  N���������۽�
			//     ����  O������̶�����
            //���������� F�����볤�ȣ�����
            //     ����  D������̶�����
			//���������� P��ƽ�г�            
			//           S��ֱ��...	
			//���������� C����ս���
            //           E��\Enter��\Space������            
			//           ESC��ȡ�����в���
            //�޸ļ�¼�� ���ӻ��˹���				By YuanHY  20060104  
			//           ���Ӳ�׽Ч��				By YuanHY  20060217
			//           ������������				By YuanHY  20060308
			//           ����ƽ�г�					By YuanHY  20060309
			//           ����ֱ��...				By YuanHY  20060330
			//           �����Ҽ��˵���				By YuanHY  20060330  
			//           ����״̬��������ʾ��Ϣ	By YuanHY  20060615         
-----------------------------------------------------------------------*/
using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;

using WSGRI.DigitalFactory.Commands;
using WSGRI.DigitalFactory.Gui.Views;
using WSGRI.DigitalFactory.Gui;
using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.DFEditorLib;
using WSGRI.DigitalFactory.Services;
using WSGRI.DigitalFactory.DFFunction;

using ICSharpCode.Core.Services;

namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// DrawBeizerCurve ��ժҪ˵����
	/// </summary>
	public class DrawBezierCurve:AbstractMapCommand
	{
		private IDFApplication m_App;
		private IMapControl2   m_MapControl;
		private IMap           m_FocusMap;
		private ILayer         m_CurrentLayer;
		private IActiveView    m_pActiveView; 
		private IMapView       m_MapView = null;

		private IDisplayFeedback        m_pFeedback;
		private IDisplayFeedback        m_pLastFeedback;
		private INewBezierCurveFeedback m_pBezierCurveFeed;
		private INewLineFeedback        m_pLastLineFeed;

		private bool          m_bInUse;
		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private IPoint        m_pLastPoint;

		public static bool   m_bFixLength;//�Ƿ��ѹ̶�����
		public static double m_dblFixLength;
		public static bool   m_bFixDirection;//�Ƿ��ѹ̶�����
		public static double m_dblFixDirection;
		public static bool   m_bFixLeftCorner;//�Ƿ����۽�
		public static double m_dbFixlLeftCorner;
		public static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��

		private double   m_dblTolerance;   //�̶�����ֵ
		private bool     m_bkeyCodeS;      //�Ƿ�S��������ֱ��...
		private ISegment m_pSegment = null;//ƽ�г߷����޸�ê������ʱ����׽���ı��ߵ�ĳ��Ƭ��
		private bool     m_bKeyCodeP;  
		private IPoint   m_BeginConstructParallelPoint;//��ʼƽ�гߣ�����һ��ĵ�

		private IArray m_pUndoArray = new ArrayClass();

		private EditContextMenu  m_editContextMenu;//�Ҽ��˵�

		private IStatusBarService m_pStatusBarService;//״̬������

		private bool	isEnabled   = false;
		private string	strCaption  = "���Ʊ���������/���߱ߵ���";
		private string	strCategory = "�༭";

		private IEnvelope m_pEnvelope = null;


		public DrawBezierCurve()
		{	//�Ҽ��˵�	
			m_editContextMenu = new EditContextMenu();
			m_editContextMenu.toolbarsManager.ToolClick += new Infragistics.Win.UltraWinToolbars.ToolClickEventHandler(toolManager_ToolClick);
		
		}

		#region �������
		public override bool IsEnabled
		{
			get 
			{
				isEnabled = false;

				m_App = (IDFApplication)this.Hook;

				if (m_App == null)   return false;
				IMapView mapView     = null;
				mapView = (IMapView)m_App.Workbench.GetView(typeof(MapView));
				IFeatureLayer pFeatureLayer = (IFeatureLayer)m_App.CurrentEditLayer;
                if(m_App.CurrentEditLayer is IGeoFeatureLayer)
                {
					if (pFeatureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
						||pFeatureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
						isEnabled = true;
				}
				return isEnabled;
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
		
		public override void Execute()
		{
			base.Execute();
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
	
            m_MapView.CurrentTool = this;

			m_MapControl   = m_App.CurrentMapControl;            
			m_FocusMap     = m_MapControl.ActiveView.FocusMap;
			m_pActiveView  = (IActiveView)this.m_FocusMap;
			m_CurrentLayer = m_App.CurrentEditLayer;
			m_pStatusBarService = m_App.StatusBarService;//���״̬����

			CurrentTool.m_CurrentToolName  = CurrentTool.CurrentToolName.drawBezier;

			CommonFunction.MapRefresh(m_pActiveView);
     
			m_dblTolerance=CommonFunction.ConvertPixelsToMapUnits(m_MapControl.ActiveView, 4);

			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;

			m_pStatusBarService.SetStateMessage("��ʾ:U:����/A:����XY/R:���XY/N:���۽�/O:��λ��/F:���ȣ�����/D:����/P:ƽ�г�/S:ֱ��.../C:��ս���/Enter:����/ESC:ȡ��");//��״̬��������ʾ��Ϣ

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "��������";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

		}
    
		public override void UnExecute()
		{
			// TODO:  ��� DrawBeizerCurve.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");

		}
     
		public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawBeizerCurve.OnMouseDown ʵ��
			base.OnMouseDown (button, shift, x, y, mapX, mapY);

			m_pStatusBarService.SetStateMessage("��ʾ:U:����/A:����XY/R:���XY/N:���۽�/O:��λ��/F:���ȣ�����/D:����/P:ƽ�г�/S:ֱ��.../C:��ս���/Enter:����/ESC:ȡ��");//��״̬��������ʾ��Ϣ
           
			m_CurrentLayer = ((IDFApplication)this.Hook).CurrentEditLayer;
		
			//���ݲ˵�
			if (button==2)
			{
				//��¼������꣬����ƽ�г߹���
				m_BeginConstructParallelPoint = m_pAnchorPoint;
		
				toolbarsManagerToolsEnabledOrNot();
				m_editContextMenu.ActiveEditContextMenu("drawPopupMenuTool",WSGRI.DigitalFactory.Gui.DefaultWorkbench.ActiveForm);

				return;

			}
			//�����Ƿ񳬳���ͼ��Χ
			if(CommonFunction.PointIsOutMap(m_CurrentLayer,m_pAnchorPoint) == true)
			{
				DrawBezierCurveMouseDown(m_pAnchorPoint); 
			}
			else
			{
				MessageBox.Show("������ͼ��Χ");
			}	
			

		}

		#region//�Ҽ��˵����Ƿ����
		private void toolbarsManagerToolsEnabledOrNot()
		{
			if(m_pUndoArray.Count==0)
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = false;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = false; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = false;
				m_editContextMenu.toolbarsManager.Tools["btnSideLength"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = false;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = false;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = false;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = false;

			}
			else if(0<m_pUndoArray.Count && m_pUndoArray.Count<2)
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = true;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = true; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = true;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = true;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = false;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;
			}
			else if(2<=m_pUndoArray.Count && m_pUndoArray.Count<3)
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = true;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = true;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = true; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = true;
				m_editContextMenu.toolbarsManager.Tools["btnSideLength"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = true;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = true; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = true;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;

			}
			else if(m_pUndoArray.Count>=3)
			{
				//m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = true;
				//m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = true;
				//m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = true; 
				//m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = true;
				//m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = true;
				//m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				//m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = true;
				//m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				//m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = true; 
				  m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = true;
				//m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = true;
				//m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;

			}
		}
		#endregion
		private void DrawBezierCurveMouseDown(IPoint pPoint )
		{          
			if(!m_bInUse)//�������û��ʹ��
			{ 
				m_bInUse = true;

				m_pUndoArray.Add(pPoint);

				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,pPoint);

				m_pFeedback = new NewBezierCurveFeedbackClass();                
				m_pBezierCurveFeed = (INewBezierCurveFeedback)m_pFeedback;
				m_pBezierCurveFeed.Start(pPoint);
				if( m_pFeedback != null)  m_pFeedback.Display = m_pActiveView.ScreenDisplay;

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
				m_pBezierCurveFeed = (INewBezierCurveFeedback)m_pFeedback;            
				m_pBezierCurveFeed.AddPoint(pPoint);
      
				IPoint tempPoint = new PointClass();
				tempPoint.X  = pPoint.X;
				tempPoint.Y  = pPoint.Y;              
				m_pUndoArray.Add(tempPoint);
				UpdataBezierCurveFeed(m_MapControl,ref m_pUndoArray);//ˢ����Ļ

				if( (m_bFixLength ==true ) && ( m_bFixDirection == false) )//���Ը���һ������ֵ
				{
					m_bFixLength = false;                  
				}  
				else if( (m_bFixLength == false) && ( m_bFixDirection ==true ))//���Ը���һ���̶�����ֵ
				{
					m_bFixDirection = false;                    
				} 
				else if ( (m_bFixLength == true) && ( m_bFixDirection ==true ))
				{
					m_bFixLength = false;
					m_bFixDirection = false;
				}

				if (m_bFixLeftCorner)
				{
					m_bFixLeftCorner=false;
				}
			}

			m_pLastPoint = pPoint;

			if (m_bkeyCodeS == true) //ֱ�ǽ���
			{
				m_bkeyCodeS = false;
				if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
				{
					m_pLastLineFeed.Stop();
				}		
			}

			m_pSegment = null;//��ղ�׽����Ƭ��

		}

		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawBeizerCurve.OnMouseMove ʵ��
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

			
			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

			if (m_bkeyCodeS == true)//��S������ֱ��
			{
				m_pPoint = CommonFunction.SquareEnd((IPoint)m_pUndoArray.get_Element(0),(IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count-1),m_pPoint);
			}

			double dx, dy;
			double tempA;

			if(m_bFixDirection && m_bInputWindowCancel == false)  //�˴��̶�m_pAnchorPointʹ����һ���̶�������
			{
				m_pPoint = CommonFunction.GetTwoPoint_FormPointMousePointFixDirection(m_pLastPoint,m_pPoint,m_dblFixDirection);
			}
			else if(m_bFixLength && m_bInputWindowCancel == false)//�Ը���һ������ֵ
			{
				m_dblFixDirection =  CommonFunction.GetAzimuth_P12(m_pLastPoint,m_pPoint);

				tempA = CommonFunction.azimuth(m_pLastPoint,  m_pPoint);
				dx = m_dblFixLength * Math.Cos(tempA * Math.PI / 180);
				dy = m_dblFixLength * Math.Sin(tempA * Math.PI / 180);
    
				//����������ê������ֵ
				dx = m_pLastPoint.X + dx;
				dy = m_pLastPoint.Y + dy;
    
				m_pPoint.PutCoords( dx, dy);                
			}
			else if (m_bFixLeftCorner && m_pUndoArray.Count>1&& m_bInputWindowCancel == false)//�������۽�
			{
				//�������һ�εķ�λ��
				double TempTA = CommonFunction.GetAzimuth_P12((IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count - 2),(IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count - 1)); 
����������������//���㽫Ҫ�γɵ�һ�εķ�λ��
				tempA =(180 + CommonFunction.RadToDeg(TempTA)) - m_dbFixlLeftCorner;
                
				if (m_dbFixlLeftCorner>360) m_dbFixlLeftCorner = m_dbFixlLeftCorner - 360;

				if (m_dbFixlLeftCorner!=tempA)
				{
					m_pPoint=CommonFunction.GetOnePoint_FormPointMousePointFixDirection(m_pLastPoint, m_pPoint, tempA);
				}
			}    
    
			m_pAnchorPoint = m_pPoint;

			//+++++++++++++��ʼ��׽+++++++++++++++++++++			
			bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,(IGeometry)m_pLastPoint,m_pAnchorPoint);
               
			if(! m_bInUse)	return ;

			//########################ƽ�г�########################			
			CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_pPoint,ref m_pAnchorPoint);

			//&&&&&&&&&&&&&&&&&&&&&&&& �� �� &&&&&&&&&&&&&&&&&&&&&&&
			CommonFunction.PositiveCross(m_pLastPoint,ref m_pAnchorPoint,m_App.CurrentConfig.cfgPositiveCross.IsPositiveCross ); 

			m_pFeedback.MoveTo(m_pAnchorPoint);

			if((m_pUndoArray.Count > 1) && ((((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon) || m_bkeyCodeS == true))
			{
				if( m_pLastFeedback != null)  m_pLastFeedback.Display = m_pActiveView.ScreenDisplay;
				m_pLastFeedback.MoveTo(m_pAnchorPoint);
			}
         
		}
                
		public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawBeizerCurve.OnDoubleClick ʵ��
			base.OnDoubleClick (button, shift, x, y, mapX, mapY);

			EndDrawBezierCurve(); 
		}

		private void EndDrawBezierCurve()
		{            
			IGeometry pGeom = null;
			IPolyline pPolyline;  
			IPolygon pPolygon;
			IPointCollection pPointCollection;
          
			if(m_bInUse)
			{
				switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
				{
					case  esriGeometryType.esriGeometryPolyline:
						m_pBezierCurveFeed = (INewBezierCurveFeedback)m_pFeedback;
						m_pFeedback.MoveTo((IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count -1 ));;
						pPolyline = m_pBezierCurveFeed.Stop();

						((ITopologicalOperator)pPolyline).Simplify();

						pPointCollection =(IPointCollection) pPolyline;                    
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
						m_pBezierCurveFeed = (INewBezierCurveFeedback)m_pFeedback;
						m_pBezierCurveFeed.AddPoint((IPoint)m_pUndoArray.get_Element(0));
						pPolyline = m_pBezierCurveFeed.Stop();						                      
						pPolygon= CommonFunction.PolylineToPolygon((IPolyline)pPolyline);

						((ITopologicalOperator)pPolygon).Simplify();

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
 
				}//end switch

				m_pEnvelope = pGeom.Envelope;
				if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);;

				CommonFunction.CreateFeature(m_App.Workbench, pGeom, m_FocusMap, m_CurrentLayer);

			}

			Reset();//��λ

			m_bInUse = false;

		}

		//���˲���
		private void  Undo()
		{
			if (m_pUndoArray.Count==0) return;

			if(m_pUndoArray.Count >1)
			{
				m_pEnvelope = CommonFunction.GetMinEnvelopeOfTheArray(m_pUndoArray);
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
			enve =CommonFunction.NewRect(pPoint,m_dblTolerance);

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
			m_MapControl.ActiveView.Refresh();

			m_pUndoArray.Remove(m_pUndoArray.Count-1);//ɾ�����������һ���� 

			//��Ļˢ��
			//m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground,null,null);

			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_pEnvelope);//��ͼˢ��

			m_pActiveView.ScreenDisplay.UpdateWindow();                          
                    
			//��ʼ����λ����
			if (m_pUndoArray.Count!=0)
			{                
				UpdataBezierCurveFeed(m_MapControl, ref m_pUndoArray);
				m_pBezierCurveFeed.MoveTo(m_pAnchorPoint);
			}
			else
			{   
				Reset(); //��λ
			}
		}

		//����m_pBezierCurveFeed
		public void UpdataBezierCurveFeed(IMapControl2 MapControl, ref IArray pUndoArray)
		{
			if (m_pBezierCurveFeed !=null) m_pBezierCurveFeed.Stop();
          
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground,null,null);
			m_pActiveView.ScreenDisplay.UpdateWindow();                      
          
			m_pBezierCurveFeed.Start((IPoint)m_pUndoArray.get_Element(0));
			CommonFunction.DrawPointSMSSquareSymbol(MapControl,(IPoint)pUndoArray.get_Element(0));
			for (int i = 0; i< pUndoArray.Count;i++)
			{
				m_pBezierCurveFeed.AddPoint((IPoint)pUndoArray.get_Element(i));  
				CommonFunction.DrawPointSMSSquareSymbol(MapControl,(IPoint)pUndoArray.get_Element(i));     
			} 
		}

		public override void OnBeforeScreenDraw(int hdc)
		{
			// TODO:  ��� DrawBeizerCurve.OnBeforeScreenDraw ʵ��
			base.OnBeforeScreenDraw (hdc);
           
			if(m_pUndoArray.Count !=0)
			{
				IPoint pStartPoint = new PointClass();
				IPoint pEndPoint = new PointClass();
				pStartPoint = (IPoint)m_pUndoArray.get_Element(0);
				pEndPoint = (IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count -1);

				if (m_pBezierCurveFeed !=null)  m_pBezierCurveFeed.MoveTo(pEndPoint);
				if (m_pLastLineFeed !=null)  m_pLastLineFeed.MoveTo(pStartPoint);
			}   

		}

		public override void OnAfterScreenDraw(int hdc)
		{
			// TODO:  ��� DrawBeizerCurve.OnAfterScreenDraw ʵ��
			base.OnAfterScreenDraw (hdc);
		}

		//��SegmentCollection��ʾ����Ļ
		private  void DisplaypSegmentColToScreen( IMapControl2 MapControl,ref IArray pUndoArray)
		{
			IActiveView pActiveView = MapControl.ActiveView;
			IPolyline pPolyline;
			INewBezierCurveFeedback pBezierCurveFeed;
			IDisplayFeedback pFeedback;
			pFeedback = new NewBezierCurveFeedbackClass();                
			pBezierCurveFeed = (INewBezierCurveFeedback)pFeedback;

			pBezierCurveFeed.Start((IPoint)m_pUndoArray.get_Element(0));
			for (int i = 0; i< m_pUndoArray.Count-1;i++)
			{
				pBezierCurveFeed.AddPoint((IPoint)m_pUndoArray.get_Element(i));                       
			}
			pPolyline = pBezierCurveFeed.Stop();

			IPointCollection pPointCollection;
			pPointCollection=(IPointCollection)pPolyline;
   
			pActiveView.ScreenDisplay.ActiveCache = (short)esriScreenCache.esriNoScreenCache; 
			ISimpleLineSymbol pLineSym = new SimpleLineSymbolClass();
			pLineSym.Color=CommonFunction.GetRgbColor(0,0,0);
             
			pActiveView.ScreenDisplay.StartDrawing(m_pActiveView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
			pActiveView.ScreenDisplay.SetSymbol((ISymbol)pLineSym);      
			pActiveView.ScreenDisplay.DrawPolyline(pPolyline);
			pActiveView.ScreenDisplay.FinishDrawing();

			for(int i=0; i<pPointCollection.PointCount; i++)
			{
				CommonFunction.DrawPointSMSSquareSymbol(MapControl,pPointCollection.get_Point(i));
			}
		}
		
		private void Reset()
		{
			m_pActiveView.FocusMap.ClearSelection(); 			
			m_pActiveView.GraphicsContainer.DeleteAllElements();
	        m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground , null, m_pEnvelope);//��ͼˢ��
			m_pStatusBarService.SetStateMessage("����");

			m_bInUse = false;
			if(m_pLastPoint != null)m_pLastPoint.SetEmpty();
			m_pUndoArray.RemoveAll();//��ջ������� 
			m_pBezierCurveFeed =null;
			m_pLastLineFeed    =null;
			m_bInputWindowCancel = true;
			m_pEnvelope = null;
   
		}

		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� DrawBeizerCurve.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);
         
					
			if (keyCode == 85 && m_bInUse)//��U��,����
			{
				Undo();

				return;
			}

			if (keyCode == 78 && m_pUndoArray.Count>=2 )//��N�������۽Ƿ���
			{
				frmLeftCorner fromFixLeftCorner = new frmLeftCorner();
				fromFixLeftCorner.ShowDialog();

				return ;   
			}

			if (keyCode == 79 && m_bInUse)//��(O)orientation������̶�����
			{  
				frmFixAzim fromFixAzim = new frmFixAzim();
				fromFixAzim.ShowDialog();   

				return ;
			}

			if (keyCode == 68 && m_bInUse)//��D������̶�����
			{  
				frmFixLength fromFixLength = new frmFixLength(); 
				fromFixLength.ShowDialog();

				return ; 
			}

			if (keyCode == 70 && m_bInUse)//��F�����볤��+��λ��
			{  
				frmLengthAzim.m_pPoint = m_pLastPoint;
				frmLengthAzim fromLengthDirect = new frmLengthAzim(); 
				fromLengthDirect.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{       
					DrawBezierCurveMouseDown(m_pAnchorPoint);
				}

				return;
			}

			if (keyCode == 65 )//��A�������������
			{       
				frmAbsXYZ.m_pPoint = m_pAnchorPoint;
				frmAbsXYZ formXYZ = new frmAbsXYZ();
				formXYZ.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{        
					DrawBezierCurveMouseDown(m_pAnchorPoint);
				}

				return;
			}

			if (keyCode == 82 && m_bInUse)//��R�������������
			{     
				frmRelaXYZ.m_pPoint = m_pLastPoint;
				frmRelaXYZ formRelaXYZ = new frmRelaXYZ(); 
				formRelaXYZ.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{       
					DrawBezierCurveMouseDown(m_pAnchorPoint); 
				}
  
				return;
			}
				
            
			if ((keyCode == 69 || keyCode == 13 || keyCode == 32) && m_bInUse && m_pUndoArray.Count>=2)//��E����ENTER ����SPACEBAR ����������
			{
				EndDrawBezierCurve();

				return;
                  
			}
			
			if (keyCode == 83 && m_pUndoArray.Count>=2)//��S������ֱ��
			{	
				m_bkeyCodeS = true;

				if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
				{
					m_pLastFeedback = new NewLineFeedbackClass();					
					m_pLastLineFeed = (INewLineFeedback)m_pLastFeedback;				
					m_pLastLineFeed.Start((IPoint)m_pUndoArray.get_Element(0));  
				}		  
				
				return;
			}
       
			if (keyCode == 80 && m_bInUse)//��P��ƽ�г�
			{							
				m_pSegment = null;
				m_bKeyCodeP = true;		
					
				return;
			}	

			if (keyCode == 67 && m_pUndoArray.Count>=3)//��C����ս�������
			{
				if(m_bInUse)
				{
					m_pUndoArray.Add((IPoint)m_pUndoArray.get_Element(0));

					EndDrawBezierCurve();
				}

				return;
			} 
   
			if (keyCode == 27 )//ESC ����ȡ�����в���
			{
				Reset();

                this.Stop();
                WSGRI.DigitalFactory.Commands.ICommand command = DFApplication.Application.GetCommand("WSGRI.DigitalFactory.DF2DControl.cmdPan");
                if (command != null) command.Execute();

				return;
			}

    
		} 
  
		//�Ҽ��˵�����¼�
		private void toolManager_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
		{
			string strItemName = e.Tool.SharedProps.Caption.ToString();
			
			switch (strItemName)
			{

				case "������(&U)":
					Undo();

					break;

				case "�������۽�(&N)...":
					frmLeftCorner fromFixLeftCorner = new frmLeftCorner();
					fromFixLeftCorner.ShowDialog();
		
					break;

				case "����+��λ��(&F)..":
					frmLengthAzim.m_pPoint = m_pLastPoint;
					frmLengthAzim fromLengthDirect = new frmLengthAzim(); 
					fromLengthDirect.ShowDialog();
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{       
						DrawBezierCurveMouseDown(m_pAnchorPoint);
					}
                 
					break;

				case "���뷽λ��(&O)...":
					frmFixAzim fromFixAzim = new frmFixAzim();
					fromFixAzim.ShowDialog();
   
					break;

				case "���볤��(&D)...":
					frmFixLength fromFixLength = new frmFixLength(); 
					fromFixLength.ShowDialog();

					break;

				case "��������(&A)...":
					frmAbsXYZ.m_pPoint = m_pAnchorPoint;
					frmAbsXYZ formXYZ = new frmAbsXYZ();
					formXYZ.ShowDialog();
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{        
						DrawBezierCurveMouseDown(m_pAnchorPoint);
					}

					break;

				case "�������(&R)...":
					frmRelaXYZ.m_pPoint = m_pLastPoint;
					frmRelaXYZ formRelaXYZ = new frmRelaXYZ(); 
					formRelaXYZ.ShowDialog();
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{       
						DrawBezierCurveMouseDown(m_pAnchorPoint); 
					}

					break;

				case "ƽ��(&P)...":
					m_pSegment = null;
					m_bKeyCodeP = true;
					CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_BeginConstructParallelPoint,ref m_pAnchorPoint);

					break;

				case "ֱ��(&S)...":								
					m_bkeyCodeS = true;

					if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
					{
						m_pLastFeedback = new NewLineFeedbackClass();					
						m_pLastLineFeed = (INewLineFeedback)m_pLastFeedback;				
						m_pLastLineFeed.Start((IPoint)m_pUndoArray.get_Element(0));  
					}		  
			
					break;

				case "������(&C)":
					if(m_bInUse)
					{
						m_pUndoArray.Add((IPoint)m_pUndoArray.get_Element(0));

						EndDrawBezierCurve();
					}  

					break;

				case "���(&E)":
					EndDrawBezierCurve();

					break;

				case "ȡ��(ESC)":
					Reset();

					break;

				default:

					break;
			}
			
		}

		public override void Stop()
		{
			//this.Reset();
			base.Stop();
		}

	
	}

}
