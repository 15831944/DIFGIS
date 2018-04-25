/*----------------------------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawArcCenterFromTo.cs
			// �ļ���������������Բ�ģ��뾶����ǣ�ֹ�ǣ����ƻ�\��������
			//
			// 
			// ������ʶ��YuanHY 20051226
            // ����˵������shift�����޸ĸ���Բ�ģ��뾶����ǣ�ֹ��
            //           P��ƽ�г�
			//���������� ESC��ȡ�����в���
			//           ENTER����SPACEBAR����������
            //
			// �޸ļ�¼������ƽ�г߹���									By YuanHY  20060308
            //           �����������ܡ���								By YuanHY  20060308����
			//           ��Բ��δ�γ�֮ǰ������һ��ֱ������ǿ�Ӿ�Ч��	By YuanHY  20060403��
			//           ����״̬��������ʾ��Ϣ						By YuanHY  20060615
            // �޸ı�ʶ��Modify by YuanHY20081112
            // �޸�������������ESC���Ĳ���
------------------------------------------------------------------------------------------*/
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
	/// DrawArcCenRadFATA ��ժҪ˵����
	/// </summary>
    public class DrawArcCenRadFATA:AbstractMapCommand
    {
        private IDFApplication m_App;
		private IMapControl2   m_MapControl;
        private IMap           m_FocusMap;
        private ILayer         m_CurrentLayer;
        private IActiveView    m_pActiveView;
		private IMapView       m_MapView = null;

        private IDisplayFeedback m_pFeedback;
        private INewLineFeedback m_pLineFeed;
    
		private bool          m_bInUse;
		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private IPoint        m_pLastPoint;
        public  static bool   m_bModify;

        private double m_Ca = 0;//Բ�Ľ�
        public  static double m_R;
        public  static IPoint m_pCenterPoint;
        public  static double m_FromAangle;
        public  static double m_ToAangle;
		public  static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��

		private double m_dblTolerance;     //�̶�����ֵ
		private ISegment m_pSegment = null;//ƽ�г߷����޸�ê������ʱ����׽���ı��ߵ�ĳ��Ƭ��
		private bool m_bKeyCodeP;          //�Ƿ�P��������ƽ�г�
		private IPoint   m_BeginConstructParallelPoint;//��ʼƽ�гߣ�����һ��ĵ�

        private IArray pLineArray = new ESRI.ArcGIS.esriSystem.ArrayClass();

		private EditContextMenu  m_editContextMenu;//�Ҽ��˵�

		private IStatusBarService m_pStatusBarService;//״̬������

		private bool	isEnabled   = false;
		private string	strCaption  = "����Բ�� + �뾶 + ��� + ֹ�ǣ����ƻ�/��������" ;
		private string	strCategory = "�༭" ;

		private IEnvelope m_pEnvelope = new EnvelopeClass();


		public DrawArcCenRadFATA()
        {
			//�Ҽ��˵�	
			m_editContextMenu = new EditContextMenu();
			m_editContextMenu.toolbarsManager.ToolClick += new Infragistics.Win.UltraWinToolbars.ToolClickEventHandler(toolManager_ToolClick);	
		
			//���״̬���ķ���
			//m_pStatusBarService = (IStatusBarService)ServiceManager.Services.GetService(typeof(WSGRI.DigitalFactory.Services.UltraStatusBarService));

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
					if (pFeatureLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)						isEnabled = true;
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

			CurrentTool.m_CurrentToolName  = CurrentTool.CurrentToolName.drawArcCenRadFATA;

			CommonFunction.MapRefresh(m_pActiveView);
            
			m_dblTolerance=CommonFunction.ConvertPixelsToMapUnits(m_pActiveView, 4);
			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;

			m_pStatusBarService.SetStateMessage("��ʾ������ָ��1.Բ������;2.��ʼ��(�뾶);3.��ֹ�ǡ�(P:ƽ�г�/ESC:ȡ��/ENTER:����/+shift:�޸�����)");

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "���ƻ�/����";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

		}
    
        public override void UnExecute()
        {
            // TODO:  ��� DrawArcCenRadFATA.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");
        }

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawArcCenRadFATA.OnMouseDown ʵ��
            base.OnMouseDown (button, shift, x, y, mapX, mapY);

			m_pStatusBarService.SetStateMessage("����ָ��:1.Բ������;2.��ʼ��(�뾶);3.��ֹ�ǡ�(P:ƽ�г�/ESC:ȡ��/ENTER:����/+shift:�޸�����)");


			//�Ժ����ɾ��֮
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
				DrawArcCenRadFATAMouseDown(m_pAnchorPoint,shift);  
			}
			else
			{
				MessageBox.Show("������ͼ��Χ");
			}		
			          
			
        } 

		private void DrawArcCenRadFATAMouseDown(IPoint pPoint, int shift)
		{
			if (!m_bInUse)
			{							
				m_pFeedback = new NewLineFeedbackClass(); 
				m_pLineFeed = (INewLineFeedback)m_pFeedback;
				m_pLineFeed.Display = m_pActiveView.ScreenDisplay;

				pLineArray.Add(pPoint);

				if(pLineArray.Count==1)
				{
					m_pLineFeed.Start(pPoint); 
				}

				m_pLastPoint  = pPoint ;
                
				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,pPoint);
               
				if (pLineArray.Count == 2)
				{
					m_bInUse = true;
					m_pLineFeed.Stop();
					m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics ,null,m_pEnvelope);
				}

			} 
			else
			{
				if (pLineArray.Count >= 2)
				{
					EndDrawArcCenRadFATA(pPoint,shift);
				}
			}

			m_pSegment = null;//��ղ�׽����Ƭ��

		}

		#region//�Ҽ��˵����Ƿ����
		private void toolbarsManagerToolsEnabledOrNot()
		{
			if(!m_bInUse)//���
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
			
			if(pLineArray.Count ==1 )//һ��
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = false;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = false; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = false;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = false; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = false;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = false;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;
			}
			else if (pLineArray.Count >1)
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = false;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = false; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = false;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = false; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = false;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = true;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;
			}
			
		}
		#endregion

        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawArcCenRadFATA.OnMouseMove ʵ��
            base.OnMouseMove (button, shift, x, y, mapX, mapY);

			
			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

			m_pAnchorPoint = m_pPoint;

			//+++++++++++++��ʼ��׽+++++++++++++++++++++			
			bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,null,m_pAnchorPoint);
         
			if (m_pLastPoint!= null)
			{
				//########################ƽ�г�########################			
				CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_pPoint,ref m_pAnchorPoint);

				//&&&&&&&&&&&&&&&&&&&&&&&& �� �� &&&&&&&&&&&&&&&&&&&&&&&
				CommonFunction.PositiveCross(m_pLastPoint,ref m_pAnchorPoint,m_App.CurrentConfig.cfgPositiveCross.IsPositiveCross ); 
			}

			if(pLineArray.Count >=1)
			{
				if( m_pFeedback != null)  m_pFeedback.Display = m_pActiveView.ScreenDisplay;
				m_pFeedback.MoveTo(m_pAnchorPoint);
			}

			if (m_bInUse)
            {
                pLineArray.Add(m_pAnchorPoint); 

                if (pLineArray.Count >= 3)
                {
                    IPoint pPoint1 = (IPoint)pLineArray.get_Element(1); 
                    IPoint p0 = (IPoint)pLineArray.get_Element(0); 
                    IPoint pPoint3 = (IPoint)pLineArray.get_Element(pLineArray.Count-1 );

                    //����뾶
                    m_R = CommonFunction.GetDistance_P12(pPoint1,p0);

                    //����p0�����p1�ķ�λ�Ǻ͵��˵�ķ�λ��
                    double Ap01;
                    double Ap03;
					
                    Ap01=CommonFunction.GetAzimuth_P12(p0,pPoint1);
                    Ap03=CommonFunction.GetAzimuth_P12(p0,pPoint3);
                    //����˵�����
                    pPoint3.X =p0.X + m_R*Math.Cos(Ap03);
                    pPoint3.Y =p0.Y + m_R*Math.Sin(Ap03);
					
                    //����Բ�Ľ�          		
                    m_Ca = Ap03 - Ap01;
					
                    if (m_Ca<0)
                    {
                        m_Ca = m_Ca + 2 * Math.PI; 
                    }
					
                    m_pLineFeed.Stop(); 
                    m_pLineFeed.Start(pPoint1); 
                    IPoint pm = new PointClass();			
                    for (int i = 0; i <= m_Ca/CommonFunction.DegToRad(5)-1; i++)
                    {			
                        pm.X = p0.X + m_R * Math.Cos(Ap01+CommonFunction.DegToRad(5 * (i + 1)));
                        pm.Y = p0.Y + m_R * Math.Sin(Ap01+CommonFunction.DegToRad(5 * (i + 1)));
                        m_pLineFeed.AddPoint(pm);
                    }
					
                    m_pLineFeed.AddPoint(pPoint3);
                }
            }

        }

        public override void OnBeforeScreenDraw(int hdc)
        {
            // TODO:  ��� DrawArcCenRadFATA.OnBeforeScreenDraw ʵ��
            base.OnBeforeScreenDraw (hdc);
           
			if (m_pFeedback !=null && pLineArray.Count!=0)  
			{                
				if(pLineArray.Count>=0 && pLineArray.Count<2)
				{
					m_pLineFeed.MoveTo((IPoint)pLineArray.get_Element(pLineArray.Count-1));
				}

				if(pLineArray.Count>=2)
				{
					m_pLineFeed.Stop();
				}

			}    
        }

		private void EndDrawArcCenRadFATA(IPoint pPoint,int shift)
		{
			IGeometry pGeom = null;
			IPolyline pPolyline;
			IPolygon  pPolygon;

			CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,pPoint);

			m_pLineFeed = (INewLineFeedback)m_pFeedback;
			if( m_pLineFeed != null) m_pLineFeed.Stop();
                      
			m_pCenterPoint = (IPoint)pLineArray.get_Element(0); 
			IPoint pFromPoint = (IPoint)pLineArray.get_Element(1);

			m_ToAangle   = CommonFunction.GetAzimuth_P12(m_pCenterPoint,pPoint);
			m_FromAangle = CommonFunction.GetAzimuth_P12(m_pCenterPoint,pFromPoint);

                               
			if (m_bInUse)
			{        
				if (shift == 1)//������סshift���͵����Ի������û��޸�Բ���ϵ�����ֵ
				{
					frmArcCenRadFATA formArcCenRadFATA = new frmArcCenRadFATA();
					formArcCenRadFATA.ShowDialog();

					if( m_bModify)//�޸�����ֵ��
					{
						m_bModify = false;
					}
				}

				ICircularArc pArc = new CircularArcClass();
            
				pArc.PutCoordsByAngle(m_pCenterPoint, m_FromAangle,( m_ToAangle-m_FromAangle),m_R);

				if ( m_Ca<Math.PI ) //Բ�Ľ�С�ڦ�
				{
					pPolyline = CommonFunction.ArcToPolyline(pArc.FromPoint,pArc.CenterPoint, pArc.ToPoint,esriArcOrientation.esriArcMinor);
				}
				else //Բ�ĽǴ��ڦ�
				{
					pPolyline = CommonFunction.ArcToPolyline(pArc.FromPoint,pArc.CenterPoint, pArc.ToPoint,esriArcOrientation.esriArcMajor);
				}
            
				switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
				{
					case  esriGeometryType.esriGeometryPolyline:                                     
						pGeom = (IGeometry)pPolyline;
						break;

					case  esriGeometryType.esriGeometryPolygon:                                         
						ILine  pCenterToFormPointLine = new LineClass();
						pCenterToFormPointLine.PutCoords(pArc.CenterPoint, pArc.ToPoint);
						ILine  pToPointToCenterLine = new LineClass();
						pToPointToCenterLine.PutCoords(pArc.FromPoint ,m_pCenterPoint);
                                            
						ISegmentCollection pSegsPolyline;
						pSegsPolyline =(ISegmentCollection) pPolyline;   
                        
						object a = System.Reflection.Missing.Value; 
						object b = System.Reflection.Missing.Value; 
                       
						pSegsPolyline.AddSegment((ISegment)pCenterToFormPointLine, ref a, ref b);                                            
						pSegsPolyline.AddSegment((ISegment)pToPointToCenterLine, ref a, ref b);
                 
						pPolygon =  CommonFunction.PolylineToPolygon((IPolyline)pSegsPolyline);
						pGeom = (IGeometry)pPolygon;
						break;
  
				}//end switch

				m_pEnvelope = pGeom.Envelope;
				m_pEnvelope.Union(m_pCenterPoint.Envelope);
				if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);;
				CommonFunction.CreateFeature(m_App.Workbench, pGeom, m_FocusMap,m_CurrentLayer);  

				Reset();     
				pLineArray.RemoveAll();
				m_bInUse = false;

			}//end else if (m_bInUse)      
		}

		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� DrawArcCenRadFATA.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);
              
			if (keyCode == 13 || keyCode == 32)//��ENTER ����SPACEBAR ��
			{       
				EndDrawArcCenRadFATA(m_pAnchorPoint,shift);
			}

			if (keyCode == 27 )//ESC ����ȡ�����в���
			{
				Reset();

                this.Stop();
                WSGRI.DigitalFactory.Commands.ICommand command = DFApplication.Application.GetCommand("WSGRI.DigitalFactory.DF2DControl.cmdPan");
                if (command != null) command.Execute();

                return;
			}

			if (keyCode == 80)//��P��ƽ�г�
			{							
				m_pSegment  = null;
				m_bKeyCodeP = true;							
				return;
			}
		}

		//�Ҽ��˵�����¼�
		private void toolManager_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
		{
			string strItemName = e.Tool.SharedProps.Caption.ToString();
			
			switch (strItemName)
			{
				case "��������(&A)...":
					frmAbsXYZ.m_pPoint = m_pAnchorPoint;
					frmAbsXYZ formXYZ = new frmAbsXYZ();
					formXYZ.ShowDialog();

					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{                    
						DrawArcCenRadFATAMouseDown(m_pAnchorPoint,0);
					}

					break;

				case "�������(&R)...":
					frmRelaXYZ.m_pPoint = m_pLastPoint;
					frmRelaXYZ formRelaXYZ = new frmRelaXYZ();
					formRelaXYZ.ShowDialog();

					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{                   
						DrawArcCenRadFATAMouseDown(m_pAnchorPoint,0);  
					}

					break;

				case "ƽ��(&P)...":
					m_pSegment  = null;
					m_bKeyCodeP = true;
					CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_BeginConstructParallelPoint,ref m_pAnchorPoint);

					break;

				case "���(&E)":
					EndDrawArcCenRadFATA(m_pAnchorPoint,0);

					break;
					
				case "ȡ��(ESC)":
					Reset();

					break;

				default:

					break;
			}
			
		}

		private void Reset()
		{
			m_pFeedback = null;
			m_pLineFeed = null;   
			m_bInUse    = false;
			m_bModify   = false;
			m_bInputWindowCancel = true;

			if(m_pLastPoint != null) m_pLastPoint.SetEmpty();
			pLineArray.RemoveAll();

			m_pActiveView.GraphicsContainer.DeleteAllElements();
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��
			m_pEnvelope = null;

			m_pStatusBarService.SetStateMessage("����");
		}

		public override void Stop()
		{
			//this.Reset();
			base.Stop();
		}

    }
}
