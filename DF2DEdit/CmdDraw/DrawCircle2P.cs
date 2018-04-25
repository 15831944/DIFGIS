/*-------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawCircle2P.cs
			// �ļ���������������Բֱ�����˵����� p1+p2������Բ\Բ������
			//
			// 
			// ������ʶ��YuanHY 20051226
            // ����˵������shift�����޸�Բֱ�����˵�����
    		//           A�������������
            //     ����  R�������������
			//           P��ƽ�г�
			//���������� ESC��ȡ�����в���
			//           ENTER����SPACEBAR����������
            //
			// �޸ļ�¼������ƽ�г߹���				By YuanHY  20060309
            //           �����������ܡ���			By YuanHY  20060309����
			//           ����״̬��������ʾ��Ϣ	By YuanHY  20060615 ������    
-----------------------------------------------------------------------*/
using System;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
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
	/// DrawCircle2P ��ժҪ˵����
	/// </summary>
	public class DrawCircle2P:AbstractMapCommand
	{

		private IDFApplication m_App;        
        private IMapControl2   m_MapControl;
        private IMap           m_FocusMap;
        private ILayer         m_CurrentLayer;
        private IActiveView    m_pActiveView;
		private IMapView       m_MapView = null;

        private IDisplayFeedback   m_pFeedback;
        private INewCircleFeedback m_pCircleFeed;

        private bool m_bFirst;
        private bool m_bSecond;

		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private IPoint m_pLastPoint;
        public  static bool m_bModify;

        public  static IPoint m_pPoint1 = new PointClass();
        public  static IPoint m_pPoint2 = new PointClass();
		public  static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��

        private IPoint m_pCenterPoint   = new PointClass();

		private double   m_dblTolerance;     //�̶�����ֵ
		private ISegment m_pSegment = null;  //ƽ�г߷����޸�ê������ʱ����׽���ı��ߵ�ĳ��Ƭ��
		private bool     m_bKeyCodeP;		 //�Ƿ�P��������ƽ�г�
		private IPoint   m_BeginConstructParallelPoint;//��ʼƽ�гߣ�����һ��ĵ�

		private EditContextMenu  m_editContextMenu;//�Ҽ��˵�

		private IStatusBarService m_pStatusBarService;//״̬������

		private bool	isEnabled   = false;
		private string	strCaption  = "����Բֱ�����˵����꣬����Բ/Բ������";
		private string	strCategory = "�༭";

		private IEnvelope m_pEnvelope = new EnvelopeClass();

		public DrawCircle2P()
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

			CurrentTool.m_CurrentToolName  = CurrentTool.CurrentToolName.drawCircle2P;

			CommonFunction.MapRefresh(m_pActiveView);
           
			m_dblTolerance=CommonFunction.ConvertPixelsToMapUnits(m_MapControl.ActiveView, 4);

			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;

			m_pStatusBarService.SetStateMessage("��ʾ������ָ��Բֱ���ϵ�1.һ��;2.��һ�㡣(A:����XY/R:���XY/P:ƽ�г�/ESC:ȡ��/ENTER:����/+shift:�޸�����)");

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "����Բ��";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

		}
    
        public override void UnExecute()
        {
            // TODO:  ��� DrawCircle2P.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");

        }  

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawCircle2P.OnMouseDown ʵ��
            base.OnMouseDown (button, shift, x, y, mapX, mapY);

			m_pStatusBarService.SetStateMessage("����ָ��Բֱ����:1.һ��;2.��һ�㡣(A:����XY/R:���XY/P:ƽ�г�/ESC:ȡ��/ENTER:����/+shift:�޸�����)");

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
				DrawCircle2PMouseDown(m_pAnchorPoint,shift); 
			}
			else
			{
				MessageBox.Show("������ͼ��Χ");
			}	
			           

        }

		#region//�Ҽ��˵����Ƿ����

		private void toolbarsManagerToolsEnabledOrNot()
		{
			if(!m_bFirst)//���
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
			else //һ��
			{
				m_editContextMenu.toolbarsManager.Tools["btnUndo"].SharedProps.Enabled       = false;
				m_editContextMenu.toolbarsManager.Tools["btnLeftCorner"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnFixAzim"].SharedProps.Enabled    = false; 
				m_editContextMenu.toolbarsManager.Tools["btnFixLength"].SharedProps.Enabled	 = false;
				m_editContextMenu.toolbarsManager.Tools["btnSideLength"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnLengthAzim"].SharedProps.Enabled = false;
				m_editContextMenu.toolbarsManager.Tools["btnAbsXYZ"].SharedProps.Enabled     = true; 
				m_editContextMenu.toolbarsManager.Tools["btnRelaXYZ"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnParllel"].SharedProps.Enabled    = true;
				m_editContextMenu.toolbarsManager.Tools["btnRt"].SharedProps.Enabled         = false; 
				m_editContextMenu.toolbarsManager.Tools["btnColse"].SharedProps.Enabled      = false;
				m_editContextMenu.toolbarsManager.Tools["btnEnd"].SharedProps.Enabled        = true;
				m_editContextMenu.toolbarsManager.Tools["btnESC"].SharedProps.Enabled        = true;
			}
			
		}
		#endregion
       
		private void DrawCircle2PMouseDown(IPoint pPoint,int shift)
		{
			IGeometry pGeom = null;
			IPolyline pPolyline;
			IPolygon pPolygon;

			if(!m_bFirst) //�������û��ʹ��
			{
 ��������       m_pPoint1 = pPoint;
				m_bFirst  = true;
				m_bSecond = false;

				m_pFeedback = new NewCircleFeedbackClass();

				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,m_pPoint1);
			}
			else if (!m_bSecond)//�����������ʹ��
			{
				m_pPoint2 = pPoint;
				m_bFirst = false;

				m_pCenterPoint = CommonFunction.GetCircleCenter_P12(m_pPoint1,pPoint);
                       
				m_pCircleFeed = (NewCircleFeedbackClass)m_pFeedback;
				m_pCircleFeed.Display = m_pActiveView.ScreenDisplay;             
				m_pCircleFeed.Stop();
				m_pCircleFeed.Start(m_pCenterPoint);
				m_pFeedback.MoveTo(pPoint);

				ICircularArc pCircularArc = new CircularArcClass();
				pCircularArc = m_pCircleFeed.Stop();
				m_pCenterPoint = pCircularArc.CenterPoint;

				if (shift == 1)//������סshift���͵����Ի������û��޸�Բ���ϵ�����ֵ
				{
					frmCircle2P formCircle2P = new frmCircle2P();
					formCircle2P.ShowDialog();

					if( m_bModify)//�޸�����ֵ��
					{
������������            //����Բ������
						m_pCenterPoint = CommonFunction.GetCircleCenter_P12(m_pPoint1, m_pPoint2);       
						m_bModify = false;   
					}
				}

				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,m_pPoint2);

				switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
				{
					case  esriGeometryType.esriGeometryPolyline:  
						pPolyline = CommonFunction.ArcToPolyline(pCircularArc.FromPoint, pCircularArc.CenterPoint, pCircularArc.FromPoint,esriArcOrientation.esriArcClockwise);
						pGeom = (IGeometry)pPolyline;
						break;
					case esriGeometryType.esriGeometryPolygon:
						pPolyline = CommonFunction.ArcToPolyline(pCircularArc.FromPoint, pCircularArc.CenterPoint, pCircularArc.FromPoint,esriArcOrientation.esriArcClockwise);
						pPolygon  =  CommonFunction.PolylineToPolygon(pPolyline);
						pGeom = (IGeometry)pPolygon;
						break;
					default:
						break;

				}//end switch

				m_pEnvelope = pGeom.Envelope;
				if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(10,10,false);;
				CommonFunction.CreateFeature(m_App.Workbench, pGeom, m_FocusMap, m_CurrentLayer);  
								����   
				Reset();
              
			}//end if(!m_bSecond)

			m_pLastPoint = pPoint;

			m_pSegment  = null;

		}
        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawCircle2P.OnMouseMove ʵ��
            base.OnMouseMove (button, shift, x, y, mapX, mapY);

			
            m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
			
			m_pAnchorPoint = m_pPoint;

			//+++++++++++++��ʼ��׽+++++++++++++++++++++	
			if(m_pLastPoint!= null)
			{
				if(m_pLastPoint.IsEmpty) 
				{
					bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,null,m_pAnchorPoint);
				}
			}
			else
			{
				if(!m_pLastPoint.IsEmpty) 
				{
					bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,m_pLastPoint,m_pAnchorPoint);
				}  
			}

            if (m_bFirst)
            { 
				//########################ƽ�г�########################			
				CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_pPoint,ref m_pAnchorPoint);
				
				//&&&&&&&&&&&&&&&&&&&&&&&& �� �� &&&&&&&&&&&&&&&&&&&&&&&
				CommonFunction.PositiveCross(m_pLastPoint,ref m_pAnchorPoint,m_App.CurrentConfig.cfgPositiveCross.IsPositiveCross ); 		
			
                m_pCenterPoint = CommonFunction.GetCircleCenter_P12(m_pPoint1,m_pAnchorPoint);
                       
                m_pCircleFeed = (NewCircleFeedbackClass)m_pFeedback;
                m_pCircleFeed.Display = m_pActiveView.ScreenDisplay;
             
                m_pCircleFeed.Stop();
                m_pCircleFeed.Start(m_pCenterPoint);
                m_pFeedback.MoveTo(m_pAnchorPoint);
            }



        }

        public override void OnBeforeScreenDraw(int hdc)
        {
            // TODO:  ��� DrawCircle2P.OnBeforeScreenDraw ʵ��
            base.OnBeforeScreenDraw (hdc);
           
            if (m_pFeedback != null)  
            {
                m_pFeedback.MoveTo(m_pCenterPoint);              
            }    
        }

		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� DrawCircle2P.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);
			
			if (keyCode == 65)//��A��,�����������
			{    				
				frmAbsXYZ.m_pPoint = m_pAnchorPoint;
				frmAbsXYZ formXYZ = new frmAbsXYZ();
				formXYZ.ShowDialog();

				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                    
					DrawCircle2PMouseDown(m_pAnchorPoint,0);
				}

				return;
			}

			if (keyCode == 82 && m_bFirst)//��R��,�����������
			{    				
				IPoint tempPoint = new PointClass();
				tempPoint.X = m_pLastPoint.X;
				tempPoint.Y = m_pLastPoint.Y;
				frmRelaXYZ.m_pPoint = tempPoint;
				frmRelaXYZ formRelaXYZ = new frmRelaXYZ();
				formRelaXYZ.ShowDialog();

				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                   
					DrawCircle2PMouseDown(m_pAnchorPoint,0);  
				}

				return;
			}

			if (keyCode == 80 && m_bFirst)//��P��ƽ�г�
			{							
				m_pSegment  = null;
				m_bKeyCodeP = true;	
						
				return;
			}
			
			if ((keyCode == 13 || keyCode == 32) && m_bFirst)//��ENTER ����SPACEBAR ��
			{     
				DrawCircle2PMouseDown(m_pAnchorPoint,shift);

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
				case "��������(&A)...":
					frmAbsXYZ.m_pPoint = m_pAnchorPoint;
					frmAbsXYZ formXYZ = new frmAbsXYZ();
					formXYZ.ShowDialog();

					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{                    
						DrawCircle2PMouseDown(m_pAnchorPoint,0);
					}

					break;

				case "�������(&R)...":
					IPoint tempPoint = new PointClass();
					tempPoint.X = m_pLastPoint.X;
					tempPoint.Y = m_pLastPoint.Y;
					frmRelaXYZ.m_pPoint = tempPoint;
					frmRelaXYZ formRelaXYZ = new frmRelaXYZ();
					formRelaXYZ.ShowDialog();

					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{                   
						DrawCircle2PMouseDown(m_pAnchorPoint,0);  
					}

					break;

				case "ƽ��(&P)...":
					m_pSegment  = null;
					m_bKeyCodeP = true;
					CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_BeginConstructParallelPoint,ref m_pAnchorPoint);

					break;

				case "���(&E)":
					DrawCircle2PMouseDown(m_pAnchorPoint,0);

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
			m_bFirst  = false;
			m_bSecond = false;
			m_bInputWindowCancel = true;
            m_pCircleFeed = null;
			m_pFeedback   = null;
			if(m_pLastPoint != null) m_pLastPoint.SetEmpty();;
			m_pSegment  = null;
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
