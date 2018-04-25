/*---------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����DrawPolyline.cs
			// �ļ��������������ƶ�����\������״���ߵ���
			//
			// 
			// ������ʶ��YuanHY 20060102
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
            //           H������Բ��
            //           L������ֱ��
            //           T��,����Բ������ ����
			// �޸ļ�¼�����Ӳ�׽Ч��				By YuanHY  20060217
			//           ����ֱ�Ƿ��				By WangShM 20060307
			//           ����ƽ�г�					By YuanHY  20060308
			//           �����������ܡ���			By YuanHY  20060308
			//           �����Ҽ��˵���				By YuanHY  20060331 
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

using DF2DControl.Base;
using DF2DControl.Command;
using DF2DControl.UserControl.View;
using DFWinForms.Service;

namespace DF2DEdit.CmdDraw
{
	/// <summary>
	/// DrawPolyline ��ժҪ˵����
	/// </summary>
    public class DrawPolyline : AbstractMap2DCommand
	{
        private DF2DApplication m_App;      
		private IMapControl2   m_MapControl;
		private IMap           m_FocusMap;
		private ILayer         m_CurrentLayer;
		private IActiveView    m_pActiveView;
		
		private IDisplayFeedback m_pFeedback;
		private INewLineFeedback m_pLineFeed;  
		private IDisplayFeedback m_pLastFeedback;
		private INewLineFeedback m_pLastLineFeed;

		public  static IPoint m_pPoint;
		public  static IPoint m_pAnchorPoint;
		private static IPoint m_pLastPoint;
        
		public static bool   m_bFixLength;//�Ƿ��ѹ̶�����
		public static double m_dblFixLength;
		public static bool   m_bFixDirection;//�Ƿ��ѹ̶�����
		public static double m_dblFixDirection;
		public static bool   m_bFixLeftCorner;//�Ƿ����۽�
		public static double m_dbFixlLeftCorner;
		public static bool   m_bInputWindowCancel = true;//��ʶ���봰���Ƿ�ȡ��

		private bool   m_bKeyCodeP;//��P��ƽ����
		private bool   m_bkeyCodeS;//��S��ֱ�Ƿ��
		private IPoint m_BeginConstructParallelPoint;//��ʼƽ�гߣ�����һ��ĵ�

		private bool   m_bInUse;
		private string m_drawState = "";//����״̬
		private int    m_drawType = 0; //�������:=1ֱ���ϵĵ�,��2ΪԲ���ϵĵ�
		private IPoint m_pFromPoint;  //����ֱ�߶����¼ֱ�����;����Բ�����¼Բ�������
		private IPoint m_pMiddlePoint;//����ֱ�߶����¼ֱ�ߵ��м��,�Թ���û���κ�����;����Բ�����¼Բ�������
		private IPoint m_pToPoint;����//����ֱ�߶����¼ֱ��ֹ��;����Բ�����¼Բ����ֹ��
		private static double TempTA; //���ߵķ�λ��
        
		private IArray m_pLineFeedArray = new ArrayClass();//�����ϵĵ�����
		private IArray m_pUndoArray     = new ArrayClass();
		private double m_dblTolerance;

		private ISegment m_pSegment = null;//��ƽ�г߷����޸�ê������ʱ�䲶׽�����ߵ�ĳ����

		private bool	isEnabled   = false;
		private string	strCaption  = "���ƶ�����/������״���ߵ���";
		private string	strCategory = "�༭";   
  
		private	IEnvelope  m_pEnvelope =  new EnvelopeClass();

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
        }

		public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawPolyline.OnMouseDown ʵ��
			base.OnMouseDown (button, shift, x, y, mapX, mapY);

            m_CurrentLayer = Class.Common.CurEditLayer; 

			//�����Ƿ񳬳���ͼ��Χ
			if(Class.Common.PointIsOutMap(m_CurrentLayer,m_pAnchorPoint) == true)
			{
				DrawPolylineMouseDown(m_pAnchorPoint,m_drawState); 
	
				m_pSegment = null;
			}
			else
			{
				MessageBox.Show("������ͼ��Χ");
			}	
			           
		}
		
		public void DrawPolylineMouseDown( IPoint pPoint,string drawState)
		{   
            m_pLastPoint  = pPoint;
        
			if (m_drawState =="")  m_drawState = "Line_Line";//Ĭ���ǻ���ֱ��
            
			//��ֱ�߶����������Բ�����������꣨���+�е�+ֹ�㣩�������������,���ڻ��˲�������õĴ洢����
			if (m_pLineFeedArray.Count > 2)
			{
				m_pFromPoint    = ((IPoint)m_pLineFeedArray.get_Element(0)) ;         
				m_pMiddlePoint  = ((IPoint)m_pLineFeedArray.get_Element((int)(m_pLineFeedArray.Count/2)));                 
				m_pToPoint      = m_pAnchorPoint;

				if (m_drawType != 0) //������ӵ�UnDo������
				{
					AddPointUndoArray(m_pFromPoint, m_drawType,ref m_pUndoArray);
					if (m_drawType == 2)//��ΪԲ���򣬴���Բ���е����꣬�������ڻ���Բ��ʱ�������
					{
						AddPointUndoArray(m_pMiddlePoint, m_drawType,ref m_pUndoArray);
					}
					AddPointUndoArray(m_pToPoint, m_drawType,ref m_pUndoArray);
				}

				switch(m_drawType)//�������߷�λ��;
				{
					case 1:
						TempTA = CommonFunction.GetAzimuth_P12(m_pFromPoint, m_pToPoint); 
						break;
					case 2:
						TempTA = CommonFunction.GetTangentLineAzi(m_pFromPoint, m_pMiddlePoint, m_pToPoint);                     
						break;
				}
����
				m_pLineFeed.Stop();
				m_pLineFeedArray =new ArrayClass();
				m_pLineFeedArray.RemoveAll(); //��ջ�ͼ����������Ԫ��	
  ��            m_bInUse = false;

				DisplaypSegmentColToScreen(m_MapControl, ref m_pUndoArray);//����ˢ����Ļ��
			}

			//��ʼm_pLastLineFeed��λ����
			if ((m_pUndoArray.Count!=0)&&(((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon))
			{                
				if (m_pLastLineFeed != null) m_pLastLineFeed.Stop();
				m_pLastFeedback = new NewLineFeedbackClass();
				m_pLastLineFeed = (INewLineFeedback)m_pLastFeedback;
				m_pLastLineFeed.Start(((PointStruct)m_pUndoArray.get_Element(0)).Point);
			}

			if (!m_bInUse )//�������û��ʹ��
			{
				m_pFeedback = new NewLineFeedbackClass(); 
				m_pLineFeed =(NewLineFeedback)m_pFeedback;
				m_pLineFeed.Display = m_pActiveView.ScreenDisplay;
 
				IPoint tempPoint = new PointClass();
				tempPoint.X  = m_pAnchorPoint.X;
				tempPoint.Y  = m_pAnchorPoint.Y; 
               
				m_pLineFeedArray.Add(tempPoint);
                   
				switch (m_drawState)
				{				
					case "Line_Line"://����ֱ��        
						m_drawType = 1;   
						break;
					case "Line_Arc": //�ɻ���ֱ��-Բ��              
						m_drawType = 2;         						
						break;
					case "Arc_Arc":  //����Բ��-Բ��                       
						m_drawType = 2;                							
						break;
					case "Arc_TLine"://����Բ��-����    
						m_drawType = 1;            
						break;                    
					default:
						break;
				}//end switch	

				m_bInUse = true;          

			}
    
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

            CommonFunction.DrawPointSMSSquareSymbol(m_MapControl,m_pAnchorPoint);

			if (m_pUndoArray.Count > 1)
			{
				DisplaypSegmentColToScreen(m_MapControl, ref m_pUndoArray);//����ˢ����Ļ��
			}	

			if (m_bkeyCodeS == true) //ֱ�ǽ���
			{
				m_bkeyCodeS = false;
				if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
				{
					m_pLastLineFeed.Stop();
				}		
			}

			m_pSegment = null;
			
		}
	
		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawPolyline.OnMouseMove ʵ��
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

			m_MapView.CurrentTool =this;
			
			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y); //��ȡ�������	 
         
			if (m_bkeyCodeS == true)//��S������ֱ��
			{
				IPoint pStartPoint = new PointClass();
				IPoint pEndPoint   = new PointClass();
				pStartPoint=((PointStruct)m_pUndoArray.get_Element(0)).Point;
				pEndPoint=((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;
				m_pPoint = CommonFunction.SquareEnd(pStartPoint,pEndPoint,m_pPoint);
			}

			double dx, dy;
			double tempA;
			IPoint p0;//Բ��

			if(m_bFixDirection && m_bInputWindowCancel == false)  //�˴��̶�m_pAnchorPointʹ����һ���̶�������
			{
				m_pPoint=CommonFunction.GetTwoPoint_FormPointMousePointFixDirection(m_pToPoint,m_pPoint,m_dblFixDirection);
			}
			else if(m_bFixLength && m_bInputWindowCancel == false)// ����һ������ֵ
			{
				m_dblFixDirection =  CommonFunction.GetAzimuth_P12(m_pToPoint,m_pPoint);

				tempA = CommonFunction.azimuth(m_pToPoint,  m_pPoint);
				dx = m_dblFixLength * Math.Cos(tempA * Math.PI / 180);
				dy = m_dblFixLength * Math.Sin(tempA * Math.PI / 180);
    
				//����������ê������ֵ
				dx = m_pToPoint.X + dx;
				dy = m_pToPoint.Y + dy;
    
				m_pPoint.PutCoords( dx, dy);                
			}
			else if (m_bFixLeftCorner && m_bInputWindowCancel == false )//�������۽�
			{
				tempA =(180 + CommonFunction.RadToDeg(TempTA)) - m_dbFixlLeftCorner;
                
				if (m_dbFixlLeftCorner>360) m_dbFixlLeftCorner = m_dbFixlLeftCorner-360;

				if (m_dbFixlLeftCorner!=tempA)
				{
					m_pPoint=CommonFunction.GetOnePoint_FormPointMousePointFixDirection(m_pToPoint,m_pPoint,tempA);
				}
			}             
           
			m_pAnchorPoint= m_pPoint;
			       
			//+++++++++++++��ʼ��׽+++++++++++++++++++++			
			bool flag = CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,(IGeometry)m_pLastPoint,m_pAnchorPoint);
	
			if (m_bInUse)//�����������ʹ��
			{	
				//########################ƽ�г�########################			
				CommonFunction.ParallelRule(ref m_bKeyCodeP,m_pActiveView,m_dblTolerance,ref m_pSegment, m_pLastPoint,m_pPoint,ref m_pAnchorPoint);

				//&&&&&&&&&&&&&&&&&&&&&&&& �� �� &&&&&&&&&&&&&&&&&&&&&&&
				CommonFunction.PositiveCross(m_pLastPoint,ref m_pAnchorPoint,m_App.CurrentConfig.cfgPositiveCross.IsPositiveCross ); 
	
				switch (m_drawState)
				{				
					case "Line_Line"://����ֱ��        
						m_drawType = 1;              
						m_pLineFeedArray.Add(m_pAnchorPoint); //���㱣�浽����		
						m_pLineFeed.Stop(); 
						m_pLineFeed.Start((IPoint)m_pLineFeedArray.get_Element(0)); 
						m_pFeedback.MoveTo(m_pAnchorPoint);
						break;
					case "Line_Arc": //����ֱ��-Բ��                       
						m_drawType = 2;
						p0 = CommonFunction.GetCenterL(m_pFromPoint, m_pToPoint, m_pAnchorPoint); //��ȡԲ������                        
						DrawArc_FromPCenterPToPTa(m_pToPoint,p0,m_pAnchorPoint,TempTA);//���+Բ��+�˵�+ͨ���������߷�λ�ǻ���				
						break;
					case "Arc_Arc":  //����Բ��-Բ��                        
						m_drawType = 2;          		
						p0 = CommonFunction.GetCenterC(m_pFromPoint, m_pMiddlePoint, m_pToPoint, m_pAnchorPoint); //��ȡԲ������  
						DrawArc_FromPCenterPToPTa(m_pToPoint,p0,m_pAnchorPoint,TempTA);//���+Բ��+�˵�+ͨ���������߷�λ�ǻ���      
						break;
					case "Arc_TLine"://����Բ��-����                       
						m_drawType = 1;             
						double d = CommonFunction.GetDistance_P12(m_pToPoint, m_pAnchorPoint);   //�������㵽���˵���� 
						//�������߷����Ͼ���d�ĵ�����
						IPoint tPoint = new PointClass();
						tPoint.X = m_pToPoint.X + d * Math.Cos(TempTA);
						tPoint.Y = m_pToPoint.Y + d * Math.Sin(TempTA);                      
						m_pLineFeedArray.Add(tPoint); //���㱣�浽����		
						m_pAnchorPoint = tPoint;
						m_pLineFeed.Stop(); 
						m_pLineFeed.Start((IPoint)m_pLineFeedArray.get_Element(0)); 
						m_pFeedback.MoveTo(m_pAnchorPoint);                    
						break;
					default:                      
						break;
				}//end switch
	
			}//end if (m_bInUse)

			if ((m_pUndoArray.Count > 1) && (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon || m_bkeyCodeS == true))
			{
				if( m_pLastFeedback != null)  m_pLastFeedback.Display = m_pActiveView.ScreenDisplay;
				m_pLastFeedback.MoveTo(m_pAnchorPoint);
			}

		}
	
		public void EndDrawPolyline()
		{
			if (CurrentTool.m_CurrentToolName  != CurrentTool.CurrentToolName.drawPolyline ) return;
         
	        IGeometry pGeom = null;
			IPolyline pPolyline;
			IPolygon pPolygon;
			IPointCollection pPointCollection;

			//����ʱ���������S��ִ��ֱ��...����������ӽ�����
			if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline && m_bkeyCodeS == true)
			{
				m_drawType = 1;
				m_pFromPoint = ((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count - 1)).Point;
				m_pToPoint = ((PointStruct)m_pUndoArray.get_Element(0)).Point;
				AddPointUndoArray(m_pFromPoint, m_drawType, ref m_pUndoArray);	
				AddPointUndoArray(m_pToPoint, m_drawType, ref m_pUndoArray);��
			}
    
			pPolyline =(IPolyline)MadeSegmentCollection(ref m_pUndoArray);
                         
			if(m_bInUse)
			{            
				switch (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType)
				{
					case  esriGeometryType.esriGeometryPolyline:
						pPointCollection =(IPointCollection)pPolyline; 

//						((ITopologicalOperator)pPolyline).Simplify();

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
						pPolyline = (IPolyline)pPolyline;		                        
						pPolygon= CommonFunction.PolylineToPolygon(pPolyline);

//						((ITopologicalOperator)pPolygon).Simplify();

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

				CommonFunction.CreateFeature(m_App.Workbench, pGeom, m_FocusMap,m_CurrentLayer);
				
                   
				Reset();//��λ 
                     
			} 

			m_pSegment = null;//��ղ�׽����Ƭ��

		}
	
		public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� DrawPolyline.OnDoubleClick ʵ��
			base.OnDoubleClick (button, shift, x, y, mapX, mapY);
    
			EndDrawPolyline();

		}
        
		public override void OnBeforeScreenDraw(int hdc)
		{
			// TODO:  ��� DrawPolyline.OnBeforeScreenDraw ʵ��
			base.OnBeforeScreenDraw (hdc);
            
			if (m_pUndoArray.Count !=0)
			{
				IPoint pStartPoint = new PointClass();
				IPoint pEndPoint   = new PointClass();         
				pStartPoint = ((PointStruct)m_pUndoArray.get_Element(0)).Point;
				pEndPoint   = ((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count -1)).Point;
    
				if (m_pLineFeed !=null)  m_pLineFeed.MoveTo(pEndPoint);
				if (m_pLastLineFeed !=null)  m_pLastLineFeed.MoveTo(pStartPoint);
			}

		}

		public override void OnAfterScreenDraw(int hdc)
		{
			// TODO:  ��� DrawPolyline.OnAfterScreenDraw ʵ��
			base.OnAfterScreenDraw (hdc);
			DisplaypSegmentColToScreen(m_MapControl, ref m_pUndoArray);//����ˢ����Ļ��
		}

		//��SegmentCollection��ʾ����Ļ
		private  void DisplaypSegmentColToScreen( IMapControl2 MapControl,ref IArray PointArray)
		{            
			IActiveView pActiveView = MapControl.ActiveView;
			ISegmentCollection pPolylineCol;
			pPolylineCol = new PolylineClass();
			ISegmentCollection  pSegmentCollection = MadeSegmentCollection(ref PointArray);
			pPolylineCol.AddSegmentCollection(pSegmentCollection);
     
			pActiveView.ScreenDisplay.ActiveCache = (short)esriScreenCache.esriNoScreenCache; 
			ISimpleLineSymbol pLineSym = new SimpleLineSymbolClass();
			pLineSym.Color=CommonFunction.GetRgbColor(0,0,0);
             
			pActiveView.ScreenDisplay.StartDrawing(m_pActiveView.ScreenDisplay.hDC, (short)esriScreenCache.esriNoScreenCache);
			pActiveView.ScreenDisplay.SetSymbol((ISymbol)pLineSym);      
			pActiveView.ScreenDisplay.DrawPolyline((IPolyline)pPolylineCol);
			pActiveView.ScreenDisplay.FinishDrawing();
		}
       
		//������������깹��SegmentCollection
		private ISegmentCollection MadeSegmentCollection(ref IArray PointArray)
		{
			ISegment pSegment;
			ISegmentCollection pSegmentCollection= new PolylineClass();
			IPoint fromPoint;
			IPoint toPoint;
			IPoint middlePoint;

			object a = System.Reflection.Missing.Value;  
			object b = System.Reflection.Missing.Value;
            
			for (int i = 0; i< PointArray.Count; i++)
			{
				PointStruct pointStruct;
				pointStruct=(PointStruct)PointArray.get_Element(i);

				if (pointStruct.Type == 1)
				{	//��ȡ�����Ͷ˵�
					fromPoint = ((PointStruct)PointArray.get_Element(i)).Point; 
					toPoint   = ((PointStruct)PointArray.get_Element(i+1)).Point;                     
					pSegment = new LineClass();
					pSegment = MadeLineSeg_2Point(fromPoint,toPoint);  
					pSegmentCollection.AddSegment(pSegment,ref a,ref b);

					i = i + 1;
				}
				else if (pointStruct.Type == 2)
				{   //��ȡ����㡢�е㡢�˵�
					fromPoint   = ((PointStruct)PointArray.get_Element(i)).Point;  
					middlePoint = ((PointStruct)PointArray.get_Element(i+1)).Point; 
					toPoint     = ((PointStruct)PointArray.get_Element(i+2)).Point;
					pSegment = new CircularArcClass();
					pSegment = MadeArcSeg_3Point(fromPoint, middlePoint,toPoint);
					pSegmentCollection.AddSegment(pSegment,ref a,ref b);

					i = i + 2;
				}
 
			}// end for

			return pSegmentCollection;

		}

		//���㷨���컡�ε�Segment
		private ISegment MadeArcSeg_3Point( IPoint pPoint1, IPoint pPoint2, IPoint pPoint3)
		{               
			IConstructCircularArc pArc = new CircularArcClass();
			pArc.ConstructThreePoints(pPoint1, pPoint2, pPoint3,true);
			return (ISegment)pArc;      
		}
		//���㹹���߶ε�Segment
		private ISegment MadeLineSeg_2Point( IPoint pPoint1, IPoint pPoint2)
		{               
			ILine pLine = new LineClass();
			pLine.PutCoords(pPoint1, pPoint2);
			return (ISegment)pLine;     
		}        
            
		//���+Բ��+�˵�+ͨ���������߷�λ��(�������� is major or minor?)����
		private void DrawArc_FromPCenterPToPTa(IPoint pFromPoint, IPoint p0, IPoint pToPoint, double TA)
		{	            
			double R;//����뾶
			R = CommonFunction.GetDistance_P12(pFromPoint,p0);

			//����p0�����p1�ķ�λ�Ǻ͵��˵�ķ�λ��
			double Ap01;
			double Ap03;					
			Ap01=CommonFunction.GetAzimuth_P12(p0,pFromPoint);
			Ap03=CommonFunction.GetAzimuth_P12(p0,pToPoint);			
           
			double Ap13; //������㵽�˵�ķ�λ��	
			Ap13=CommonFunction.GetAzimuth_P12(pFromPoint,pToPoint);
			//���ۼ���Բ�Ľ�
			double m_Ca = 0;
			double dA;
			dA = Ap13 - TA;
			if (dA < 0)
			{
				dA = dA + Math.PI * 2; 
			}
			if (dA >= 0 && dA < Math.PI) 
			{
				m_Ca = Ap03 - Ap01;
			}
			else if(dA >= Math.PI && dA < Math.PI * 2)
			{
				m_Ca = Ap01 - Ap03;	
			}
			if (m_Ca<0)
			{
				m_Ca = m_Ca + 2 * Math.PI; 
			}

			//����˵�����
			pToPoint.X =p0.X + R*Math.Cos(Ap03);
			pToPoint.Y =p0.Y + R*Math.Sin(Ap03);
					
			m_pLineFeed.Stop(); 
			m_pLineFeed.Start(pFromPoint);
			m_pLineFeedArray.RemoveAll(); 
			m_pLineFeedArray.Add(pFromPoint);

			IPoint pTempPoint = new PointClass();			
			if (dA >= 0 && dA < Math.PI) 
			{
				for (int i = 0; i <= m_Ca/CommonFunction.DegToRad(5)-1; i++)
				{			
					pTempPoint.X = p0.X + R * Math.Cos(Ap01 + CommonFunction.DegToRad(5 * (i + 1)));
					pTempPoint.Y = p0.Y + R * Math.Sin(Ap01 + CommonFunction.DegToRad(5 * (i + 1)));
					m_pLineFeed.AddPoint(pTempPoint);
					m_pLineFeedArray.Add(pTempPoint); 
				}
			}
			else if(dA >= Math.PI && dA < Math.PI * 2)
			{
				for (int i = 0; i <= m_Ca/CommonFunction.DegToRad(5)-1; i++)
				{			
					pTempPoint.X = p0.X + R * Math.Cos(Ap01 - CommonFunction.DegToRad(5 * (i + 1)));
					pTempPoint.Y = p0.Y + R * Math.Sin(Ap01 - CommonFunction.DegToRad(5 * (i + 1)));
					m_pLineFeed.AddPoint(pTempPoint);
					m_pLineFeedArray.Add(pTempPoint);
				}	
			}	
		
			m_pLineFeed.AddPoint(pToPoint);

			m_pLineFeedArray.Add(pToPoint);

		}

		//���˲���
		private void  Undo()
		{			
			//ɾ�����������һ���          
			int count = m_pUndoArray.Count;
			if(count==0)
			{
				Reset();
				return;
			}

			#region ������Сˢ�µľ���
			IArray pTempArray = new ArrayClass();
			for(int i=0; i<m_pUndoArray.Count; i++)
			{
				pTempArray.Add(((PointStruct)m_pUndoArray.get_Element(i)).Point);
			}
			if(pTempArray.Count >2)
			{
				m_pEnvelope = CommonFunction.GetMinEnvelopeOfTheArray(pTempArray);
			}
			else if(pTempArray.Count ==2)
			{
				IPoint pTempPoint = new PointClass();
				m_pEnvelope = CommonFunction.GetMinEnvelopeOfTheArray(pTempArray);
				m_pEnvelope.Union(m_pPoint.Envelope);
			}
			if(m_pEnvelope != null) m_pEnvelope.Expand(10,10,false);
			pTempArray.RemoveAll();
			#endregion

			PointStruct pointStruct =(PointStruct)m_pUndoArray.get_Element(count-1);

			IPoint pPoint0 = new PointClass();
			pPoint0 = pointStruct.Point;
			IEnvelope enve = new EnvelopeClass();
			enve =CommonFunction.NewRect(pPoint0,m_dblTolerance);

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
            
			if (pointStruct.Type == 1)
			{
				m_pUndoArray.Remove(m_pUndoArray.Count-1);
				m_pUndoArray.Remove(m_pUndoArray.Count-1);
			}
			else if (pointStruct.Type == 2)
			{
				m_pUndoArray.Remove(m_pUndoArray.Count-1);
				m_pUndoArray.Remove(m_pUndoArray.Count-1);
				m_pUndoArray.Remove(m_pUndoArray.Count-1);
			}

			//��Ļˢ��
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics , null, m_pEnvelope);//��ͼˢ��
			m_pActiveView.ScreenDisplay.UpdateWindow();
       
			//��ʼ����λ����
			m_pLineFeedArray.RemoveAll();
			if (m_pUndoArray.Count!=0)
			{    
				m_pLastPoint = ((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;

				DisplaypSegmentColToScreen(m_MapControl,ref m_pUndoArray);

				m_drawState="Line_Line";//Ĭ�ϻ���֮�󣬻���ֱ��
      
				IPoint pPoint = new PointClass();
				pPoint=((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;               
				m_pLineFeedArray.Add(pPoint);����������������

				m_pFeedback = new NewLineFeedbackClass(); 
				m_pLineFeed =(NewLineFeedback)m_pFeedback;
				m_pLineFeed.Display = m_pActiveView.ScreenDisplay;
				if (m_pLineFeed !=null) m_pLineFeed.Stop();
				m_pLineFeed.Start(pPoint);
				m_pLineFeed.MoveTo(m_pPoint);
              
				//����m_pFromPoint��m_pMiddlePoint��m_pToPoint
				if (((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Type == 1)//���������һ��Ϊ�߶�
				{
					m_pFromPoint =((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-2)).Point; 
					m_pToPoint =((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;
				}
				else if (((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Type == 2)//���������һ��ΪԲ��
				{
					m_pFromPoint =((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-3)).Point; 
					m_pMiddlePoint =((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-2)).Point; 
					m_pToPoint =((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;
				}

			}
			else //if (m_pUndoArray.Count = 0)
			{  
				Reset(); //��λ
			}
           
		}
        
		//������ӵ�m_pUnDoArray����
		private void AddPointUndoArray(IPoint pPoint, double drawType,ref IArray pUndoArray)
		{
			PointStruct pointStruct = new PointStruct();
			pointStruct.Point = pPoint;

			pointStruct.Type = (int)drawType;
           
			pUndoArray.Add(pointStruct);
       
			return;            
		}    
      
		//����һ�����ݽṹ�����㽫��Ϣ����������
		struct PointStruct
		{
			private IPoint point;
			private int type;
			public IPoint Point
			{
				get 
				{
					return point;
				}
				set 
				{
					point = value;
				}
			}
			public int Type
			{
				get 
				{
					return type;
				}
				set 
				{
					type = value;
				}
			}
		}

		private void Reset()
		{
			m_MapControl.ActiveView.FocusMap.ClearSelection();  
			
			m_pActiveView.GraphicsContainer.DeleteAllElements(); 			
		
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground , null, m_pEnvelope);//��ͼˢ��

			m_pStatusBarService.SetStateMessage("����");

			m_bInputWindowCancel = true;
			m_bInUse    = false;
			m_bkeyCodeS = false;//��Sֱ�Ƿ��
			m_drawState = "";
			if(m_pLastPoint != null) m_pLastPoint.SetEmpty();;
			m_pLineFeedArray.RemoveAll();//��ջ�ͼ����
			m_pUndoArray.RemoveAll();    //��ջ������� 
			m_pEnvelope = null;
			if(m_pLineFeed     !=null) m_pLineFeed  = null;
			if(m_pLastLineFeed !=null) m_pLastLineFeed.Stop();

		}
        
		public override void OnKeyDown(int keyCode, int shift)
		{
			// TODO:  ��� DrawPolyline.OnKeyDown ʵ��
			base.OnKeyDown (keyCode, shift);

			IPoint tempPoint   = new PointClass();
			tempPoint.X = m_pLastPoint.X;
			tempPoint.Y = m_pLastPoint.Y;

			if (keyCode == 72)//��H��,����Բ��
			{
				if (m_drawType ==1)
				{
					m_drawState="Line_Arc";//����ֱ�ߡ���Բ��
				}
				else if (m_drawType ==2)
				{
					m_drawState="Arc_Arc";//����Բ������Բ��
				} 

				return;
			}

			if (keyCode == 76)//��L��,����ֱ��
			{
                
				if (m_drawType ==1)
				{
					m_drawState="Line_Line";;//����ֱ�ߡ���ֱ��
				}
				else if (m_drawType ==2)
				{
					m_drawState="Line_Line";//����Բ������ֱ��
				} 
 
				return;
			}

			if (keyCode == 84)//��T��,����Բ����������
			{ 
				m_drawState="Arc_TLine"; 
 
				return;
			}
           
			if (keyCode == 85)//��U��,����
			{
				Undo();

				return;
			}

			if (keyCode == 78 && m_pUndoArray.Count>=2)//��N��,�������۽�
			{    
				frmLeftCorner fromFixLeftCorner = new frmLeftCorner();
				fromFixLeftCorner.ShowDialog(); 
 
				return;  
			}

			if (keyCode == 79 && m_bInUse)//��(O)orientation��,���뷽��
			{    
				frmFixAzim fromFixAzim = new frmFixAzim();  
				fromFixAzim.ShowDialog();
  
				return;  
			}

			if (keyCode == 68 && m_bInUse)//��D��,����̶�����
			{ 
				frmFixLength fromFixLength = new frmFixLength();	
				fromFixLength.ShowDialog(); 

				return;   
			}


			if (keyCode == 70 && m_bInUse)//��F��,���볤��+��λ��
			{  
				frmLengthAzim.m_pPoint = tempPoint;
				frmLengthAzim fromLengthDirect = new frmLengthAzim();     
				fromLengthDirect.ShowDialog();
                    
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                    
					DrawPolylineMouseDown(m_pAnchorPoint,m_drawState);
				}

				return;
			}

			if (keyCode == 65 )//��A��,�����������
			{       
				frmAbsXYZ.m_pPoint = m_pAnchorPoint;
				frmAbsXYZ formXYZ = new frmAbsXYZ();
				formXYZ.ShowDialog();
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                                        
					DrawPolylineMouseDown(m_pAnchorPoint,m_drawState); 
				}

				return;
			}

			if (keyCode == 82 && m_bInUse)//��R��,�����������
			{ 
				frmRelaXYZ.m_pPoint = tempPoint;// m_pToPoint;
				frmRelaXYZ formRelaXYZ = new frmRelaXYZ();    
				formRelaXYZ.ShowDialog();
                
				if(m_bInputWindowCancel == false)//���û�û��ȡ������
				{                    
					DrawPolylineMouseDown(m_pAnchorPoint,m_drawState); 
				}

				return;
			}

			if (keyCode == 80 && m_bInUse)//��P��,����ƽ����
			{							
				m_pSegment = null;
				m_bKeyCodeP = true;
							
				return;
			}

			if (keyCode == 83 && m_pUndoArray.Count>=2)//��S��,����ֱ��
			{
				m_bkeyCodeS = true;
				if (((IFeatureLayer)m_CurrentLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline )
				{
					m_pLastFeedback = new NewLineFeedbackClass();					
					m_pLastLineFeed = (INewLineFeedback)m_pLastFeedback;
					IPoint pStartPoint = ((PointStruct)m_pUndoArray.get_Element(0)).Point;
					m_pLastLineFeed.Start(pStartPoint);  
				}		  
	
				return;
			}

			if (keyCode == 67 && m_pUndoArray.Count>=4)//��C��,��ս�������
			{			
				IPoint pStartPoint = new PointClass();
				IPoint pEndPoint   = new PointClass();
				pStartPoint=((PointStruct)m_pUndoArray.get_Element(0)).Point;
				pEndPoint=((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;

				AddPointUndoArray(pEndPoint, 1, ref m_pUndoArray);
				AddPointUndoArray(pStartPoint, 1, ref m_pUndoArray);

				EndDrawPolyline();			 

				return;
			}


			if ((keyCode == 69 || keyCode == 13 || keyCode == 32) && m_bInUse)//��E����ENTER ����SPACEBAR ����������
			{
				EndDrawPolyline();

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
			IPoint pStartPoint = new PointClass();
			IPoint pEndPoint   = new PointClass();
			IPoint tempPoint   = new PointClass();
			tempPoint.X = m_pLastPoint.X;
			tempPoint.Y = m_pLastPoint.Y;

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
					frmLengthAzim.m_pPoint = tempPoint;
					frmLengthAzim fromLengthDirect = new frmLengthAzim();     
					fromLengthDirect.ShowDialog();
                    
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{                    
						DrawPolylineMouseDown(m_pAnchorPoint,m_drawState);
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
						DrawPolylineMouseDown(m_pAnchorPoint,m_drawState); 
					}

					break;

				case "�������(&R)...":
					frmRelaXYZ.m_pPoint = tempPoint;
					frmRelaXYZ formRelaXYZ = new frmRelaXYZ();    
					formRelaXYZ.ShowDialog();
                
					if(m_bInputWindowCancel == false)//���û�û��ȡ������
					{                    
						DrawPolylineMouseDown(m_pAnchorPoint,m_drawState); 
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
						pStartPoint = ((PointStruct)m_pUndoArray.get_Element(0)).Point;
						m_pLastLineFeed.Start(pStartPoint);  
					}		

					break;

				case "������(&C)":
					pStartPoint=((PointStruct)m_pUndoArray.get_Element(0)).Point;
					pEndPoint=((PointStruct)m_pUndoArray.get_Element(m_pUndoArray.Count-1)).Point;

					AddPointUndoArray(pEndPoint, 1, ref m_pUndoArray);
					AddPointUndoArray(pStartPoint, 1, ref m_pUndoArray);

					EndDrawPolyline();

					break;

				case "���(&E)":
					EndDrawPolyline();

					break;

				case "ȡ��(ESC)":
					Reset();

					break;

				default:

					break;
			}
			
		}
	
		public override bool Deactivate()
		{
			// TODO:  ��� DrawPolyline.Deactivate ʵ��
			//EndDrawPolyline();
		    return base.Deactivate();

		}
        public override void Stop()
        {
           // this.Reset();
            base.Stop();
        }

	}
}
