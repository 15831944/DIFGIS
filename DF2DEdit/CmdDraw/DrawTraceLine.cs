/*-----------------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawTraceLine.cs
			// �ļ���������������
			//
			// 
			// ������ʶ��YuanHY  WangShM 20060225
            // �������裺1��������Ŧ��
			//           2��ѡ��һ������\��\��Ҫ�أ�
			//			 3������Ҽ���ENTER ����SPACEBAR ����ֹͣѡ��Ҫ�ز�����
			//           4����������ȷ�����ٵĿ�ʼ�㣻
			//           5����������ȷ�����ٵĽ�����,��ɸ��ٲ�����
            // ����˵����ESC�� ȡ�����в���
			//           DEL�� ɾ��ѡ�е�Ҫ��
			// �޸ı�ʶ��
            //           �����������ܡ���			By YuanHY  20060309
			//           DEL�� ɾ��ѡ�е�Ҫ��		By YuanHY  20060311 
			//           ����״̬��������ʾ��Ϣ	By YuanHY  20060615 ������������   ����������    
-------------------------------------------------------------------------------*/

using System;
using System.Windows.Forms;

using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.Gui;
using WSGRI.DigitalFactory.Gui.Views;
using WSGRI.DigitalFactory.Commands; 
using WSGRI.DigitalFactory.DFSystem.DFConfig;
using WSGRI.DigitalFactory.DFEditorLib;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;


using WSGRI.DigitalFactory.Services;
using WSGRI.DigitalFactory.DFFunction;

using ICSharpCode.Core.Services;


namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// DrawTraceLine ��ժҪ˵����
	/// </summary>
	public class DrawTraceLine:AbstractMapCommand
	{
		private IDFApplication m_App;
		private IMapControl2  m_MapControl;
		private IMap m_FocusMap;
		private ILayer m_CurrentLayer;
		private IActiveView m_pActiveView;
		private IMapView m_MapView = null;
	
		private IPoint m_pPoint;
		private IPoint m_pPoint0 = new PointClass();

		private bool m_bIsSelect;
		private bool bBegineMove;//��ʼ����
        private int button1 = 1; //��һ�ε�����

		private ISegmentGraph m_pGraph;
		private IConstructCurve m_pOffsetTraceLine;
		private ISegmentGraphCursor m_pCursor;
		private IPolyline m_pTraceLine;
		public  static double m_dOffsetDistance;
		private int m_lMoveCounter;
		private ISimpleLineSymbol m_pSymbol;
		private IScreenDisplay  m_pDisplay;
		private ILine m_pConnectLine = new LineClass();
				
		private INewEnvelopeFeedback  m_pFeedbackEnve; //���ο���ʾ����
	
		private IArray m_OriginFeatureArray = new ArrayClass();

		private IStatusBarService m_pStatusBarService;//״̬����Ϣ����

		private bool	isEnabled   = false;
		private string	strCaption  = "����";
		private string	strCategory = "�༭"; 

		private IEnvelope m_pEnvelope = new EnvelopeClass();

		public DrawTraceLine()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
			Class_Initialize();

		}

		private void Class_Initialize()
		{
			INewLineFeedback pNewLineFeedback;
			bBegineMove  = false;
			m_pOffsetTraceLine = new PolygonClass();
			pNewLineFeedback = new NewLineFeedbackClass();
			m_pSymbol =(ISimpleLineSymbol)pNewLineFeedback.Symbol;
			m_pGraph = new SegmentGraphClass();

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
			m_pDisplay     = m_pActiveView.ScreenDisplay; 
			m_pStatusBarService = m_App.StatusBarService;//���״̬����
   
			CurrentTool.m_CurrentToolName  = CurrentTool.CurrentToolName.drawTraceLine;

			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;
     
			CommonFunction.MapRefresh(m_pActiveView);

			frmFixLength formFixDist = new frmFixLength();  
            formFixDist.ShowDialog();

			m_pStatusBarService.SetStateMessage("���裺1.ѡ��Ҫ��;2.�Ҽ�/ENTER/SPACEBAR,����ѡ��;3.���,ȷ�����ٵ����;4.���,�������١�(ESC: ȡ��/DEL:ɾ��)");

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "���Ƹ�����";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

		}
          
		public override void UnExecute()
		{
			// TODO:  ��� DrawTraceLine.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");

		}	

		public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawTraceLine.OnMouseDown ʵ��
			base.OnMouseDown (button, shift, x, y, mapX, mapY);

			m_pStatusBarService.SetStateMessage("1.ѡ��Ҫ��;2.�Ҽ�/ENTER/SPACEBAR,����ѡ��;3.���,ȷ�����ٵ����;4.���,�������١�(ESC: ȡ��/DEL:ɾ��)");

			m_CurrentLayer = ((IDFApplication)this.Hook).CurrentEditLayer;
			
        	m_bIsSelect = true;
			
			if (button != 2 && bBegineMove && button1 == 2)//�������٣���λ
			{
				object a = esriConstructOffsetEnum.esriConstructOffsetSimple;
				object b = System.Reflection.Missing.Value;				     
				m_pTraceLine = m_pCursor.CurrentTrace;
				m_pOffsetTraceLine.ConstructOffset(m_pTraceLine, m_dOffsetDistance,ref a ,ref b);
				IGeometry pGeometry = (IGeometry)m_pOffsetTraceLine;
				if (((IFeatureLayer)m_App.CurrentEditLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
				{
					pGeometry = (IGeometry)CommonFunction.GetPolygonBoundary((IPolygon)pGeometry);
				}

				CommonFunction.CreateFeature(m_App.Workbench,pGeometry, m_FocusMap,m_CurrentLayer);	
				
				Reset();//��λ
				
				return;
			}

			if (button != 2 && bBegineMove && button1 == 1)//��ʼ����
			{	
				//'Start a trace if one doesn't exist
				m_pCursor    = m_pGraph.GetCursor(m_pPoint);
				m_pTraceLine = m_pCursor.CurrentTrace;
				object a = esriConstructOffsetEnum.esriConstructOffsetSimple;
				object b = System.Reflection.Missing.Value;		
				m_pOffsetTraceLine.ConstructOffset(m_pTraceLine, m_dOffsetDistance,ref a ,ref b);
				RefreshTraceline();
				m_lMoveCounter = 0;
				button1 = 2;
				return;
			}

			if (button == 2 && !bBegineMove)//�Ҽ�����,ֹͣѡ��
			{
				if (m_OriginFeatureArray.Count == 0) return;

				CreateGraph();

				bBegineMove = true;	
			
				return;
			}		
				
		}

		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawTraceLine.OnMouseMove ʵ��
			base.OnMouseMove(button, shift, x, y, mapX, mapY);				
			
			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
					
			//+++++++++++++��ʼ��׽+++++++++++++++++++++			
			bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,m_pPoint0,m_pPoint);
			 
			if(m_bIsSelect  && !bBegineMove)//ѡ��Ҫ��
			{
				if (m_pFeedbackEnve == null) 
				{
					m_pFeedbackEnve = new NewEnvelopeFeedbackClass();
					m_pFeedbackEnve.Display = m_pActiveView.ScreenDisplay;
					m_pFeedbackEnve.Start(m_pPoint);
				}

				m_pFeedbackEnve.MoveTo(m_pPoint);					
				
			}
			else //���ٵ�ͼԪ��
			{
				if(bBegineMove && button1 == 2)
				{   
					//Only create the trace every other time the mouse moves
					m_lMoveCounter = m_lMoveCounter + 1;
					if (m_lMoveCounter < 2) return;
				
					m_lMoveCounter = 0;
  
					//Call refresh
					RefreshTraceline();
  
					if (m_pCursor.MoveTo(m_pPoint) == false) 
					{
						m_pCursor.FinishMoveTo(m_pPoint);
					}
  
					m_pTraceLine = m_pCursor.CurrentTrace;

					object a = esriConstructOffsetEnum.esriConstructOffsetSimple;
					object b = System.Reflection.Missing.Value;
					m_pOffsetTraceLine.ConstructOffset(m_pTraceLine, m_dOffsetDistance,ref a ,ref b);

					//Call refresh
					RefreshTraceline();				
				}				
			}					
		}

		public override void OnMouseUp(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawTraceLine.OnMouseUp ʵ��
			base.OnMouseUp (button, shift, x, y, mapX, mapY);

			if(!m_bIsSelect ) return;
			if(bBegineMove) return;
			
			IGeometry pEnv;

			if (m_pFeedbackEnve != null)
			{
				pEnv = m_pFeedbackEnve.Stop();
				m_FocusMap.SelectByShape(pEnv, null, false);
			}
			else
			{
				IEnvelope pRect ;
				double dblConst ;
				dblConst =CommonFunction.ConvertPixelsToMapUnits(m_pActiveView,8);//8�����ش�С
				pRect = CommonFunction.NewRect(m_pPoint,dblConst);
				m_FocusMap.SelectByShape(pRect,null, false);
			}	    
			IArray tempArray = CommonFunction.GetSelectFeatureSaveToArray(m_FocusMap);
			for (int i = 0; i<tempArray.Count; i++)
			{
				m_OriginFeatureArray.Add((IFeature)tempArray.get_Element(i)); 
			}
			tempArray.RemoveAll();

			if(m_OriginFeatureArray.Count==0)
			{
				//ѡ��λ
				m_pFeedbackEnve = null;				
				m_bIsSelect = false;
				return; 
			}
		
			m_MapControl.ActiveView.FocusMap.ClearSelection(); //��յ�ͼѡ���Ҫ��

			CommonFunction.ShowSelectionFeatureArray(m_MapControl,m_OriginFeatureArray);//������ʾѡ���Ҫ��

			//ѡ��λ
			m_pFeedbackEnve = null;				
			m_bIsSelect = false;	
			
		}

		private void RefreshTraceline()
		{
			if (!bBegineMove) return;

			m_pActiveView.ScreenDisplay.StartDrawing(m_pActiveView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
			m_pActiveView.ScreenDisplay.SetSymbol((ISymbol)m_pSymbol);
			m_pActiveView.ScreenDisplay.DrawPolyline((IGeometry)m_pOffsetTraceLine);
			m_pDisplay.FinishDrawing();
		}
	
		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� DrawTraceLine.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);

			//if (CurrentTool.m_CurrentToolName  != CurrentTool.CurrentToolName.drawTraceLine) return;
           
			if (keyCode == 27 )//ESC ����ȡ�����в���
			{
				Reset();

                this.Stop();
                WSGRI.DigitalFactory.Commands.ICommand command = DFApplication.Application.GetCommand("WSGRI.DigitalFactory.DF2DControl.cmdPan");
                if (command != null) command.Execute();

				return;
			}

			if (keyCode == 68)//��D������̶�����
			{  
                frmFixLength formFixDist = new frmFixLength();    
				formFixDist.ShowDialog();
				return;          
			}
			
			if ((keyCode == 13 || keyCode == 32) && m_OriginFeatureArray.Count > 0)//��ENTER ����SPACEBAR ����ֹͣѡ��
			{      
				if (m_OriginFeatureArray.Count == 0) return;
				
				if (button1 == 2)//�������٣���λ
				{
					object a = esriConstructOffsetEnum.esriConstructOffsetSimple;
					object b = System.Reflection.Missing.Value;				     
					m_pTraceLine = m_pCursor.CurrentTrace;
					m_pOffsetTraceLine.ConstructOffset(m_pTraceLine, m_dOffsetDistance,ref a ,ref b);

					IGeometry pGeometry = (IGeometry)m_pOffsetTraceLine;
					if (((IFeatureLayer)m_App.CurrentEditLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
					{
						pGeometry = (IGeometry)CommonFunction.GetPolygonBoundary((IPolygon)pGeometry);

					}
					m_pEnvelope = pGeometry.Envelope;
					if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(1,1,false);;
					CommonFunction.CreateFeature(m_App.Workbench,pGeometry, m_FocusMap,m_CurrentLayer);	
					Reset();//��λ
                    return;
				}

				CreateGraph();
				bBegineMove = true;
				return;
			}		
			
			if(keyCode == 46)   //DEL��,ɾ��ѡ�е�Ҫ��
			{
				CommonFunction.DelFeaturesFromArray(m_MapControl,ref m_OriginFeatureArray);
				Reset();			    
				return;
			}
		}

		private void Reset()
		{
			if (m_pCursor != null) 
			{
				m_pCursor = null;
			}
			if (m_pGraph != null)
			{
				if(m_pGraph != null) m_pGraph.SetEmpty();
			}
			bBegineMove  = false; 

			m_bIsSelect = false;
			bBegineMove = false;
			button1 = 1;

			m_pFeedbackEnve = null;	
			m_OriginFeatureArray.RemoveAll();            
		
			m_pActiveView.GraphicsContainer.DeleteAllElements(); 
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��
			m_pEnvelope = null;

			m_pStatusBarService.SetStateMessage("����");

		}

		private void CreateGraph()
		{
			IClone pClone;
			IFeature pFeature;
			IGeometryCollection pGeometryBag;
			esriGeometryType GeoType; 	
			pGeometryBag = new GeometryBagClass();
			
			for(int i=0; i<m_OriginFeatureArray.Count;i++)
			{
				pFeature = (IFeature) m_OriginFeatureArray.get_Element(i);
				GeoType = pFeature.Shape.GeometryType;
				if(GeoType == esriGeometryType.esriGeometryPolygon || GeoType == esriGeometryType.esriGeometryPolyline)
				{
					object a = System.Reflection.Missing.Value; 
					object b = System.Reflection.Missing.Value;
					pClone =(IClone)pFeature.Shape;
					pGeometryBag.AddGeometry((IGeometry)pClone.Clone(),ref a,ref b);
				}
			}

			if(bBegineMove ) 
			{
				if(m_pGraph != null) m_pGraph.SetEmpty();
				m_pCursor = null;
			}

			IEnumGeometry pEnumGeometry = (IEnumGeometry)pGeometryBag;
			m_pGraph.Load(pEnumGeometry, false, true);
			bBegineMove  = false;
		}	

		public override void Stop()
		{
			//this.Reset();
			base.Stop();
		}
		
	}
}