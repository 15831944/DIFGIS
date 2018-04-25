/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����CalculateCorner.cs
			// �ļ���������������н�
			//
			// 
			// ������ʶ��YuanHY 20060614
            // �������裺1��������Ŧ��
			//           2����
			//			 3��˫���������Ҽ�\�س�\�ո���������Ի���;
			//           4�����ȷ����ťɾ�����ɵĵ�ͼԪ�ء�������������
			// ����˵����ESC�� ȡ�����в���
��
		    // �޸ı�ʶ��Modify by YuanHY20081112
            // �޸�������������ESC���Ĳ����� ����    
----------------------------------------------------------------*/
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
	/// CalculateCorner ��ժҪ˵����
	/// </summary>
	public class CalculateCorner:AbstractMapCommand
	{
		private IDFApplication m_App;        
		private IMapControl2   m_MapControl;
		private IMap           m_FocusMap;
		private IActiveView    m_pActiveView;
		private IMapView       m_MapView = null;

		private IDisplayFeedback m_pFeedback;
		private INewLineFeedback m_pLineFeed;

		private bool m_bInUse;

		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private        IPoint m_pLastPoint;
		private 	   IPoint m_pPoint1 = new PointClass();
		private        IPoint m_pPoint2 = new PointClass();

		private IArray   m_pRecordPointArray = new ArrayClass();

		const string CRLF = "\r\n";

		private IStatusBarService m_pStatusBarService;

		//private bool	isEnabled   = true;
		private string	strCaption  = "����н�";
		private string	strCategory = "����";


		public CalculateCorner()
		{
			//���״̬���ķ���
			//m_pStatusBarService = (IStatusBarService)ServiceManager.Services.GetService(typeof(WSGRI.DigitalFactory.Services.UltraStatusBarService));

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
			else
			
            m_MapView.CurrentTool = this;

			m_MapControl   = m_App.CurrentMapControl;            
			m_FocusMap     = m_MapControl.ActiveView.FocusMap;
			m_pActiveView  = (IActiveView)this.m_FocusMap;
			m_pStatusBarService = m_App.StatusBarService;//���״̬����

			CurrentTool.m_CurrentToolName  = CurrentTool.CurrentToolName.CalculateCorner;

			CommonFunction.MapRefresh(m_pActiveView);
                  
		}
    
		public override void UnExecute()
		{
			// TODO:  ��� CalculateCorner.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");

		}  

		public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� CalculateCorner.OnMouseDown ʵ��
			base.OnMouseDown (button, shift, x, y, mapX, mapY);

			CalculateCornerMouseDown(m_pAnchorPoint);            

		}
      
		private void CalculateCornerMouseDown(IPoint pPoint)
		{
			IGeometry pGeom = null;

			if(!m_bInUse) //�������û��ʹ��
			{
 ��������       m_pPoint1 = pPoint;
				m_pLastPoint = pPoint;
				m_pRecordPointArray.Add(m_pPoint1);
				m_bInUse  = true;

				m_pFeedback = new NewLineFeedbackClass(); 
				m_pLineFeed = (INewLineFeedback)m_pFeedback;
				m_pLineFeed.Start(pPoint);
				if( m_pFeedback != null)  m_pFeedback.Display = m_pActiveView.ScreenDisplay;

				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,m_pPoint1);
			}
			else //�����������ʹ��
			{
				m_pPoint2 = pPoint;
				m_pRecordPointArray.Add(pPoint);
				m_bInUse = true;

				m_pLineFeed.AddPoint(pPoint);
			
				CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,m_pPoint2);
				if(m_pRecordPointArray.Count>2)
				{
					IPolyline pPolyline;	
					pPolyline =(IPolyline)CommonFunction.MadeSegmentCollection(ref m_pRecordPointArray);
					pGeom = (IGeometry)pPolyline;  
					CommonFunction.AddElement(m_MapControl,pGeom);  

					double dblZimuth = CommonFunction.GetAngleZuo_P123(m_pPoint1,(IPoint)m_pRecordPointArray.get_Element(1), m_pPoint2);
					dblZimuth = CommonFunction.RadToDeg(dblZimuth);

					System.Windows.Forms.DialogResult result;
					string  strResult = "����н�:" + dblZimuth.ToString(".#####") + "(����)";
					strResult = strResult + CRLF;
					strResult = strResult + "��һ������ X=" + m_pPoint1.X.ToString(".###")  + "Y=" + m_pPoint1.Y.ToString(".###"); 
					strResult = strResult + CRLF;
					strResult = strResult + "�ڶ������� X=" + ((IPoint)m_pRecordPointArray.get_Element(1)).X.ToString(".###")  + "Y=" + ((IPoint)m_pRecordPointArray.get_Element(1)).Y.ToString(".###"); 
					strResult = strResult + CRLF;
					strResult = strResult + "���������� X=" + m_pPoint2.X.ToString(".###")  + "Y=" + m_pPoint2.Y.ToString(".###"); 
            
					result = MessageBox.Show(strResult, "����нǼ���",	MessageBoxButtons.OK,MessageBoxIcon.Information);

					if(result == DialogResult.OK)
					{
						Reset();//��λ;
					}	
				}
		              
			}
		}
		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� CalculateCorner.OnMouseMove ʵ��
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
		
			m_pStatusBarService.SetStateMessage("��ʾ�����ε������");

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
  
			m_pAnchorPoint = m_pPoint;           
			//+++++++++++++��ʼ��׽+++++++++++++++++++++			
			bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,(IGeometry)m_pLastPoint,m_pAnchorPoint);
			
			if(!m_bInUse) return;
		
			m_pFeedback.MoveTo(m_pAnchorPoint);

		}

		public override void OnBeforeScreenDraw(int hdc)
		{
			// TODO:  ��� CalculateCorner.OnBeforeScreenDraw ʵ��
			base.OnBeforeScreenDraw (hdc);
           
			if (m_pFeedback != null)  
			{
				m_pFeedback.MoveTo(m_pPoint1);              
			}    
		}

		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� CalculateCorner.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);
			
		
			if ((keyCode == 13 || keyCode == 32) && m_bInUse)//��ENTER ����SPACEBAR ��
			{     
				CalculateCornerMouseDown(m_pAnchorPoint);

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

		private void Reset()
		{
			m_bInUse  = false;
			m_pFeedback  = null;
			m_pLastPoint = null;
			m_pPoint1 = null;
			m_pPoint2 = null;
			m_pRecordPointArray.RemoveAll();

			m_pActiveView.GraphicsContainer.DeleteAllElements();
            //m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_MapControl.ActiveView.Extent);//��ͼˢ��
            m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_MapControl.ActiveView.Extent);//��ͼˢ��		
		
			m_pStatusBarService.SetStateMessage("����");

		}

		public override void Stop()
		{
			//this.Reset();
			base.Stop();
		}

	}
}
