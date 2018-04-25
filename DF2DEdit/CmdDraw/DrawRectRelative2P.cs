
/*----------------------------------------------------------------------
            // Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawRectRelative2P.cs
			// �ļ������������������ζԽ����� + ���εĿ�ȣ����ƾ�����\��
			//
			// 
			// ������ʶ��YuanHY 20051226
            // ����˵����U������
			//   		 F�����볤��+��λ��
			//     ����  D������̶�����
            //     ����  O������̶�����
            //           B����������߳�
			//           A�������������
            //     ����  R�������������
			//           E��\Enter��\Space������ 
			//           ESC��ȡ�����в���            
            //�޸ļ�¼�� ���ӻ��˹���				By YuanHY  20060104  
			//           ����ƽ�г߹���				By YuanHY  20060309
            //           �����������ܡ���			By YuanHY  20060309��
			//           �����Ҽ��˵�����			By YuanHY  20060330 
			//           ����״̬��������ʾ��Ϣ	By YuanHY  20060615 ������    
------------------------------------------------------------------------*/

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
	/// DrawRectRelative2P ��ժҪ˵����
	/// </summary>
	public class DrawRectRelative2P:AbstractMapCommand
	{
        private IDFApplication m_App;      
		private IMapControl2   m_MapControl;
        private IMap           m_FocusMap;
        private ILayer         m_CurrentLayer;
		private IMapView       m_MapView = null;
 
        private IActiveView      m_pActiveView;
        private IDisplayFeedback m_pFeedback;
        private INewLineFeedback m_pLineFeed;

        private bool          m_bInUse;
        public  static IPoint m_pPoint;
        public  static IPoint m_pAnchorPoint;
        private IPoint        m_pLastPoint;

        private int           m_mouseDownCount;
        public  static bool   m_bFixLength;   //�Ƿ��ѹ̶�����
        public  static double m_dblFixLength;
        public  static bool   m_bFixDirection;//�Ƿ��ѹ̶�����
        public  static double m_dblFixDirection;
        public  static bool   m_bFixSideLength;//�Ƿ�̶��߳�
        public  static double m_dblSideLength;
		public  static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��
     
		private double m_dblTolerance;     //�̶�����ֵ
		private ISegment m_pSegment = null;//ƽ�г߷����޸�ê������ʱ����׽���ı��ߵ�ĳ��Ƭ��
		private bool m_bKeyCodeP;          //�Ƿ�P��������ƽ�г�
		private IPoint   m_BeginConstructParallelPoint;//��ʼƽ�гߣ�����һ��ĵ�

        private IArray m_pUndoArray      = new ArrayClass();
        private IArray m_pSavePointArray = new ArrayClass();

		private EditContextMenu  m_editContextMenu;//�Ҽ��˵�

		private IStatusBarService m_pStatusBarService;//״̬����Ϣ����

		private bool	isEnabled   = false;
		private string	strCaption  = "�������ζԽ����� + ���εĿ�ȣ����ƾ�����/��";
		private string	strCategory = "�༭"; 

		private IEnvelope m_pEnvelope = new EnvelopeClass();
  
        public DrawRectRelative2P()
		{   //�Ҽ��˵�	
			m_editContextMenu = new EditContextMenu();
			m_editContextMenu.toolbarsManager.ToolClick += new Infragistics.Win.UltraWinToolbars.ToolClickEventHandler(toolManager_ToolClick);           
        
			//���״̬���ķ���
			//m_pStatusBarService = (IStatusBarService)ServiceManager.Services.GetService(typeof(WSGRI.DigitalFactory.Services.UltraStatusBarService));

����	}

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

			CurrentTool.m_CurrentToolName = CurrentTool.CurrentToolName.drawRectRelative2P;

			CommonFunction.MapRefresh(m_pActiveView);
	         
			m_dblTolerance=CommonFunction.ConvertPixelsToMapUnits(m_MapControl.ActiveView, 4);

			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;

			m_pStatusBarService.SetStateMessage("����ָ��:1.�Խ���������;2.��ȡ�(U:����/F:���ȣ�����/D:����/O:��λ��/B:�߳�/A:����XY/R:���XY/Enter:����/ESC:ȡ��)");

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "���ƾ���";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

        }
    
        public override void UnExecute()
        {
            // TODO:  ��� DrawRectRelative2P.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");

        }

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawRectRelative2P.OnMouseDown ʵ��
            base.OnMouseDown (button, shift, x, y, mapX, mapY);

			m_pStatusBarService.SetStateMessage("��ʾ������ָ���Խ��������㡢һ���ߵĿ�ȡ�(U:����/F:���ȣ�����/D:����/O:��λ��/B:�߳�/A:����XY/R:���XY/Enter:����/ESC:ȡ��)");
	
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
				 DrawRectRelative2PMouseDown(m_pAnchorPoint);
			}
			else
			{
				Reset();
				MessageBox.Show("������ͼ��Χ");
			}
		}

		//�Ҽ��˵����Ƿ����
		private void toolbarsManagerToolsEnabledOrNot()
		{
			if(m_pUndoArray.Count==0)//��㣬���
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
			else if(m_pUndoArray.Count<2)//һ�㣬���
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = true;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = true; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = true;
				m_editContextMenu.toolbarsManager.Tools["btnSideLength"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = true;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = false;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;
			}
			else if(m_pUndoArray.Count==2)//
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = true;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = true; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = false;
				m_editContextMenu.toolbarsManager.Tools["btnSideLength"].SharedProps.Enabled = true;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = true;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = false;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = true;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;
			}
			
		}

        private void DrawRectRelative2PMouseDown(IPoint pPoint)
        {
			IPoint tempPoint = new PointClass();   

			m_mouseDownCount = m_mouseDownCount + 1;

            if (m_mouseDownCount < 3) //���������С��3ʱ
            {  
                //������  
                if (m_bInUse == false)
                {
                    m_pFeedback = new NewLineFeedbackClass();						
                    m_pLineFeed =(INewLineFeedback)m_pFeedback;
					m_pLineFeed.Start(pPoint);

                    tempPoint.X = pPoint.X;
                    tempPoint.Y = pPoint.Y;                  

                    CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,pPoint);
                    m_pUndoArray.Add(tempPoint);//����һ���㱣�浽����
                   
                    m_pLastPoint = pPoint;
                    m_bInUse = true;
                }
                else
                {
                    m_pLineFeed = (INewLineFeedback)m_pFeedback;
					tempPoint.X = m_pAnchorPoint.X;
					tempPoint.Y = m_pAnchorPoint.Y;
                    m_pLineFeed.AddPoint(tempPoint);

                    CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,tempPoint);
                    m_pUndoArray.Add(tempPoint); //���ڶ����㱣�浽����

                }

                if ( m_pFeedback != null )  m_pFeedback.Display = m_pActiveView.ScreenDisplay;

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
            }//m_mouseDownCount < 3
            else if(m_mouseDownCount == 3) //�������������3��,ֹͣ����
            {     
				EndDrawRectRelative2P();              

            }//m_mouseDownCount = 3

			m_pSegment = null;//��ղ�׽����Ƭ��

        }

		private void EndDrawRectRelative2P()
		{
			IGeometry pGeom = null;
			IPolyline pPolyline;
			IPolygon  pPolygon;
			IPointCollection pPointCollection;
����������������
			switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
			{
				case  esriGeometryType.esriGeometryPolyline:
					pPolyline = m_pLineFeed.Stop();
					pPointCollection =(IPointCollection) pPolyline;
					pGeom = (IGeometry)pPointCollection;                          
					break;

				case esriGeometryType.esriGeometryPolygon:
					pPolyline = m_pLineFeed.Stop();
					pPolygon  = CommonFunction.PolylineToPolygon(pPolyline);
					pPointCollection =(IPointCollection) pPolygon;
					pGeom = (IGeometry)pPointCollection;                      
					break;

				default:
					break;

			}// end switch 

			m_pEnvelope = pGeom.Envelope;
			if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);;

			CommonFunction.CreateFeature(m_App.Workbench,pGeom, m_FocusMap, m_CurrentLayer);
			Reset();

					
		}
            
        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawRectRelative2P.OnMouseMove ʵ��
            base.OnMouseMove (button, shift, x, y, mapX, mapY);

			
			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

			m_pAnchorPoint = m_pPoint;
			//+++++++++++++��ʼ��׽+++++++++++++++++++++			
			bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,(IGeometry)m_pLastPoint,m_pAnchorPoint);
		
			if (m_bInUse == true)
            {
				//########################ƽ�г�########################			
				CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_pPoint,ref m_pAnchorPoint);

				//&&&&&&&&&&&&&&&&&&&&&&&& �� �� &&&&&&&&&&&&&&&&&&&&&&&
				CommonFunction.PositiveCross(m_pLastPoint,ref m_pAnchorPoint,m_App.CurrentConfig.cfgPositiveCross.IsPositiveCross ); 
		
                if(m_mouseDownCount ==1 )//������С��2ʱ
                {                   
                    double dx, dy;
                    double tempA;

                    if(m_bFixDirection && m_bInputWindowCancel == false)//�̶�m_pAnchorPointʹ����һ���̶�������
                    {
                        m_pPoint = CommonFunction.GetTwoPoint_FormPointMousePointFixDirection(m_pLastPoint,m_pPoint,m_dblFixDirection);
						m_pAnchorPoint = m_pPoint;
					}
                    else if( m_bFixLength && m_bInputWindowCancel == false)//�Ը���һ������ֵ
                    {
                        m_dblFixDirection =  CommonFunction.GetAzimuth_P12(m_pLastPoint,m_pPoint);

                        tempA = CommonFunction.azimuth(m_pLastPoint,  m_pPoint);
						dx = m_dblFixLength * Math.Cos((90 - tempA) * Math.PI / 180);
						dy = m_dblFixLength * Math.Sin((90 - tempA) * Math.PI / 180);
    
                        //����������ê������ֵ
                        dx = m_pLastPoint.X + dx;
                        dy = m_pLastPoint.Y + dy;
    
                        m_pPoint.PutCoords(dx, dy);

						m_pAnchorPoint = m_pPoint;
                
                    }                     
    
					m_pFeedback.MoveTo(m_pAnchorPoint);	
                         		
                }
                else if (m_mouseDownCount == 2)
                {
                    m_pLineFeed.Stop();
      
                    double RelativeLength = CommonFunction.GetDistance_P12((IPoint)m_pUndoArray.get_Element(0),(IPoint)m_pUndoArray.get_Element(1));

					if(m_bFixDirection && m_bInputWindowCancel == false)  //�̶�m_pAnchorPointʹ����һ���̶�������
					{
						m_pPoint = CommonFunction.GetTwoPoint_FormPointMousePointFixDirection(m_pLastPoint,m_pPoint,m_dblFixDirection);
						m_pAnchorPoint = m_pPoint;
					}                 
					else if (!m_bFixSideLength)//��ȡ����һ�߳�
					{
                        m_dblSideLength =CommonFunction.GetDistance_P12(m_pAnchorPoint,(IPoint)m_pUndoArray.get_Element(1));
                        if (m_dblSideLength > RelativeLength)
                        {
                            m_dblSideLength = RelativeLength;
                        }
                    }
					else if (m_bInputWindowCancel == false)//��B�����û�����߳�������m_dblSideLengthֵ
					{
                        if (m_dblSideLength > RelativeLength)
                        {
                            m_dblSideLength = RelativeLength;
                        }          			
					}
											
					//�ж�����Ƿ�λ��P1��P2������ұ�
					bool bRight = false;
					bRight = CommonFunction.GetRectP0_Right((IPoint)m_pUndoArray.get_Element(0), (IPoint)m_pUndoArray.get_Element(1),m_pAnchorPoint);
                    //��ȡ��������������
                    m_pSavePointArray.RemoveAll();
					m_pSavePointArray = CommonFunction.GetPointRectangleOfRelative_Length((IPoint)m_pUndoArray.get_Element(0), (IPoint)m_pUndoArray.get_Element(1), m_dblSideLength, bRight); 
			
					//���ƾ���
					m_pLineFeed.Start((IPoint)m_pUndoArray.get_Element(0));
					m_pLineFeed.AddPoint((IPoint)m_pSavePointArray.get_Element(0));			  
					m_pLineFeed.AddPoint((IPoint)m_pUndoArray.get_Element(1));									 
					m_pLineFeed.AddPoint((IPoint)m_pSavePointArray.get_Element(1));							
					m_pLineFeed.AddPoint((IPoint)m_pUndoArray.get_Element(0));

                    m_pSavePointArray.Insert(0,(IPoint)m_pUndoArray.get_Element(0));
                    m_pSavePointArray.Insert(2,(IPoint)m_pUndoArray.get_Element(1));
                    m_pSavePointArray.Add((IPoint)m_pSavePointArray.get_Element(0));

                }//m_mouseDownCount == 2
                    
            }
                
        }

        //���˲���
        private void  Undo()
        {
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
			if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);;

            m_pUndoArray.Remove(m_pUndoArray.Count-1);//ɾ�����������һ����  
            m_mouseDownCount--;
            
            //��Ļˢ��
            m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground,null,m_pEnvelope);
            m_pActiveView.ScreenDisplay.UpdateWindow();
       
            //��ʼ����λ����
            if (m_pUndoArray.Count!=0)
            {                
                CommonFunction.DisplaypSegmentColToScreen(m_MapControl,ref m_pUndoArray);
   
                m_pLastPoint=(IPoint)m_pUndoArray.get_Element(m_pUndoArray.Count-1);               

                m_pFeedback = new NewLineFeedbackClass(); 
                m_pLineFeed =(NewLineFeedback)m_pFeedback;
                m_pLineFeed.Display = m_pActiveView.ScreenDisplay;
                if (m_pLineFeed !=null) m_pLineFeed.Stop();
                m_pLineFeed.Start(m_pLastPoint);
   
				m_MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,m_pLastPoint);      
				m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��
  
            }
            else 
            {  //��λ
			   m_pFeedback.MoveTo(m_pAnchorPoint);
               Reset();
            }
        }

        public override void OnBeforeScreenDraw(int hdc)
        {
            // TODO:  ��� DrawRectRelative2P.OnBeforeScreenDraw ʵ��
            base.OnBeforeScreenDraw (hdc);           
//            if (m_pLineFeed !=null)  
//            {
//                m_pLineFeed.Stop();              
//            }
        }

        public override void OnAfterScreenDraw(int hdc)
        {
            // TODO:  ��� DrawRectRelative2P.OnAfterScreenDraw ʵ��
            base.OnAfterScreenDraw (hdc);
            CommonFunction.DisplaypSegmentColToScreen(m_MapControl,ref m_pUndoArray);
        }

		private void Reset()
		{	
			m_pActiveView.GraphicsContainer.DeleteAllElements(); 
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��

			m_pEnvelope = null;
			m_bInUse = false;
			m_pUndoArray.RemoveAll();//��ջ������� 
			m_pLineFeed = null;
			m_mouseDownCount = 0; 
			m_bFixSideLength=false;
			m_bInputWindowCancel = true;

			m_pStatusBarService.SetStateMessage("����");

		}
     
        public override void OnKeyDown(int keyCode, int shift)
        {
            // TODO:  ��� DrawRectRelative2P.OnKeyDown ʵ��
            base.OnKeyDown (keyCode, shift);
         
			if (keyCode == 85 && m_mouseDownCount==1)//��U��,����
			{
				Undo();

				return;
			}
		
			if (keyCode == 70 && m_mouseDownCount==1)//��F��,���볤��+��λ��
			{  
				frmLengthAzim.m_pPoint = m_pLastPoint;
				frmLengthAzim fromLengthDirect = new frmLengthAzim();                   
				fromLengthDirect.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{ 
					DrawRectRelative2PMouseDown(m_pAnchorPoint);
				}

				return;
			}

			if (keyCode == 68 && m_mouseDownCount==1)//��D��,����̶�����
			{
				frmFixLength fromFixLength = new frmFixLength();
				fromFixLength.ShowDialog();    
             
				return;
			}
			
			if (keyCode == 79 && m_mouseDownCount==1)//��(O)orientation��,���뷽��
			{     
				frmFixAzim fromFixAzim = new frmFixAzim();
				fromFixAzim.ShowDialog();     

				return;
			}

			if (keyCode == 66 && m_mouseDownCount==2)//��B��,����߳�
			{
				frmFixSideLength fromFixSideLength = new frmFixSideLength();
				fromFixSideLength.ShowDialog();

				return;
			}   

			if (keyCode == 65 && m_mouseDownCount==1)//��A��,�����������
			{       
				frmAbsXYZ.m_pPoint = m_pAnchorPoint;
				frmAbsXYZ formXYZ = new frmAbsXYZ();
				formXYZ.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{ 
					DrawRectRelative2PMouseDown(m_pAnchorPoint);
				}

				return;
			}

			if (keyCode == 82 && m_mouseDownCount==1)//��R��,�����������
			{
				frmRelaXYZ.m_pPoint = m_pLastPoint;
				frmRelaXYZ formRelaXYZ = new frmRelaXYZ();
				formRelaXYZ.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{ 
					DrawRectRelative2PMouseDown(m_pAnchorPoint);  
				}

				return;
			}		

			if (keyCode == 80 && m_mouseDownCount==1)//��P��,ƽ�г�
			{							
				m_pSegment  = null;
				m_bKeyCodeP = true;
						
				return;
			}

			if ((keyCode == 69 || keyCode == 13 || keyCode == 32) &&  m_mouseDownCount==2)//��E����ENTER ����SPACEBAR ����������
			{
				EndDrawRectRelative2P();
                
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
								
					break;

				case "����+��λ��(&F)..":
					frmLengthAzim.m_pPoint = m_pLastPoint;
					frmLengthAzim fromLengthDirect = new frmLengthAzim();                   
					fromLengthDirect.ShowDialog();
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{ 
						DrawRectRelative2PMouseDown(m_pAnchorPoint);
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

				case "���α߳�(&B)...":
					frmFixSideLength fromFixSideLength = new frmFixSideLength();
					fromFixSideLength.ShowDialog();

					break;				

				case "��������(&A)...":
					frmAbsXYZ.m_pPoint = m_pAnchorPoint;
					frmAbsXYZ formXYZ = new frmAbsXYZ();
					formXYZ.ShowDialog();
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{ 
						DrawRectRelative2PMouseDown(m_pAnchorPoint);
					}

					break;

				case "�������(&R)...":
					frmRelaXYZ.m_pPoint = m_pLastPoint;
					frmRelaXYZ formRelaXYZ = new frmRelaXYZ();
					formRelaXYZ.ShowDialog();
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{ 
						DrawRectRelative2PMouseDown(m_pAnchorPoint);  
					}

					break;

				case "ƽ��(&P)...":
					m_pSegment  = null;
					m_bKeyCodeP = true;
					CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_BeginConstructParallelPoint,ref m_pAnchorPoint);

					break;

				case "ֱ��(&S)...":
				
					break;

				case "������(&C)":

					break;

				case "���(&E)":
					EndDrawRectRelative2P();

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