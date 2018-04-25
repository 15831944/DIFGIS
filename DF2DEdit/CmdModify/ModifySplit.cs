/*-----------------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����ModifySplit.cs
			// �ļ������������ָ���\��(���ܲ�������Ҫ�أ��ŵ�Ŀ��ͼ���д洢)
			//
			// 
			// ������ʶ��YuanHY 20060107
            // �������裺1��������Ŧ��
			//           2��ѡ��һ��������\��Ҫ��(ֻ��ѡ��ǰ�����\��Ҫ��)��
			//			 3������Ҽ�\�س�\�ո������ʼ�����и��ߣ�
			//           4��˫�����\�س�\�ո����ʵʩ�ָ���\�档
            // ����˵����
			//           1��ESC�� ȡ�����в���
			// �޸ı�ʶ��
			//           1�����Ӳ�׽Ч����              YuanHY  20060217
			//           2������״̬��������ʾ��Ϣ��	YuanHY  20060615��
			//           3��ʹ�²�����Ҫ�ش���Zֵ��Mֵ	YuanHY  20070725 ��    
------------------------------------------------------------------------------*/
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
using WSGRI.DigitalFactory.DFSystem.DFConfig;
using WSGRI.DigitalFactory.DFEditorLib;
using WSGRI.DigitalFactory.Services;
using WSGRI.DigitalFactory.DFFunction;

using ICSharpCode.Core.Services;

namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// ModifySplit ��ժҪ˵����
	/// </summary>
   
	public class ModifySplit:AbstractMapCommand
	{
		private IDFApplication m_App;
		private IMapControl2   m_MapControl;
		private IMap           m_FocusMap;
		private IActiveView    m_pActiveView;
		private ILayer         m_CurrentLayer;
		private IMapView       m_MapView = null;

		private IPoint m_pPoint;
		private bool   m_bIsSelect;            //��ʶ�Ƿ���ѡ��Ҫ��
		private bool   m_bBeginDrawLineFeed;   //��ʶ��ʼ�����и���
		private bool   m_bIsDrawLineFeed;      //��ʶ���ڻ����и���
		
		private INewEnvelopeFeedback  m_pFeedbackEnve; //���ο���ʾ����
		private IDisplayFeedback      m_pFeedback;
		private INewLineFeedback      m_pLineFeed;
		private IArray m_OriginFeatureArray = new ArrayClass();//ԴҪ������

		private IStatusBarService m_pStatusBarService;//״̬����Ϣ����

		private bool	isEnabled   = false;
		private string	strCaption  = "�ָ���/��";
		private string	strCategory = "�߼��༭"; 

		private IEnvelope m_pEnvelope = new EnvelopeClass();

		public ModifySplit()
		{
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

			CurrentTool.m_CurrentToolName = CurrentTool.CurrentToolName.modifySplit;

			//CommonFunction.MapRefresh(m_pActiveView);
           
		}
          
		public override void UnExecute()
		{
			// TODO:  ��� ModifyUnion.UnExecute ʵ��
			m_pStatusBarService.SetStateMessage("����");

		}
	
		public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
		{
			base.OnMouseDown (button, shift, x, y, mapX, mapY);
			
			m_CurrentLayer = m_App.CurrentEditLayer;

			m_bIsSelect = true;

			if (button == 2 && !m_bBeginDrawLineFeed)//�Ҽ���������ʼ���Ʒָ���
			{	
				if (m_OriginFeatureArray.Count==0) return;
				
				m_bBeginDrawLineFeed = true;

				return;
			}

			if(button != 2 && m_bBeginDrawLineFeed)//���Ʒָ���(��������)����
			{
				if (!m_bIsDrawLineFeed)//��1����
				{
					m_bIsDrawLineFeed = true;
					m_pFeedback = new NewLineFeedbackClass(); 
					m_pLineFeed = (INewLineFeedback)m_pFeedback;
					m_pLineFeed.Start(m_pPoint);
					if( m_pFeedback != null)  m_pFeedback.Display = m_pActiveView.ScreenDisplay;
				}
				else//��2��3����
				{
					m_pLineFeed = (INewLineFeedback)m_pFeedback;
					m_pLineFeed.AddPoint(m_pPoint);
				}
				
			}

			if (button == 2 && m_bBeginDrawLineFeed)//ִ�зָ����
			{	
				DoSplit();
			}

		}

		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;
				
			m_pStatusBarService.SetStateMessage("����:1.ѡ����/��Ҫ��(ֻ��ѡ��Ŀ���Ҫ��);2.�Ҽ�,����ѡ��;3.�����������,ȷ���ָ���;4.�Ҽ�,ʵʩ�ָ������(ESC:ȡ��/DEL:ɾ��)");

			if(!m_bIsSelect ) return;

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
		
			if(!m_bBeginDrawLineFeed) //����ѡ��Ҫ���ָ����Ҫ��
			{
				if (m_pFeedbackEnve == null ) 
				{
					m_pFeedbackEnve = new NewEnvelopeFeedbackClass();
					m_pFeedbackEnve.Display = m_pActiveView.ScreenDisplay;
					m_pFeedbackEnve.Start(m_pPoint);
				}
				m_pFeedbackEnve.MoveTo(m_pPoint);	
			
				return;
			}
			else//���ڻ��ơ�������
			{
				//+++++++++++++��ʼ��׽+++++++++++++++++++++					    
				CommonFunction.Snap(m_MapControl,m_App.CurrentConfig.cfgSnapEnvironmentSet,null,m_pPoint);

				if(m_pFeedback != null) m_pFeedback.MoveTo(m_pPoint);				
			}		
			
		}
	
		public override void OnMouseUp(int button, int shift, int x, int y, double mapX, double mapY)
		{
			base.OnMouseUp (button, shift, x, y, mapX, mapY);

			if(!m_bIsSelect ) return;
			if(m_bBeginDrawLineFeed) return;
			
			IGeometry pEnv;
			m_FocusMap.ClearSelection();
			if (m_pFeedbackEnve != null)
			{
				pEnv = m_pFeedbackEnve.Stop();
				m_FocusMap.SelectByShape(pEnv, null,false);
			}
			else
			{
				IEnvelope pRect ;
				double dblConst ;
				dblConst =CommonFunction.ConvertPixelsToMapUnits(m_pActiveView,8);//8�����ش�С
				pRect = CommonFunction.NewRect(m_pPoint,dblConst);
				m_FocusMap.SelectByShape(pRect,null,false);
			}

			if(!m_bBeginDrawLineFeed)//ѡ��һ������Ҫ�ָ����Ҫ��
			{	
				if (((IFeatureLayer)m_App.CurrentEditLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline
					|| ((IFeatureLayer)m_App.CurrentEditLayer).FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon) 
				{

					IArray tempArray = CommonFunction.GetSelectedFeaturesFromCurrentLayerSaveToArray(m_App.CurrentEditLayer);
					for (int i = 0; i<tempArray.Count; i++)
					{
						m_OriginFeatureArray.Add((IFeature)tempArray.get_Element(i)); 
					}
					tempArray.RemoveAll();//�����ʱ����

					m_pEnvelope = CommonFunction.GetMinEnvelopeOfTheFeatures(m_OriginFeatureArray);
					if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(1,1,false);      
					CommonFunction.MadeFeatureArrayOnlyAloneOID(m_OriginFeatureArray);//ʹ��ԴҪ���������Ψһ��

					if(m_OriginFeatureArray.Count !=0) 	
					{
						m_MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
						CommonFunction.ShowSelectionFeatureArray(m_MapControl,m_OriginFeatureArray);//������ʾѡ���Ҫ��
					}
				}

			}

			//ѡ��λ
			m_pFeedbackEnve = null;				
			m_bIsSelect = false;
			m_FocusMap.ClearSelection();//��յ�ͼѡ���Ҫ��
			
		}
		
		public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
		{
			base.OnDoubleClick (button, shift, x, y, mapX, mapY);

			DoSplit();         
		}
		
		public override void OnKeyDown(int keyCode, int shift)
		{
			base.OnKeyDown (keyCode, shift);
            
			if (keyCode == 27 )//ESC ����ȡ�����в���
			{
				Reset();

                this.Stop();
                WSGRI.DigitalFactory.Commands.ICommand command = DFApplication.Application.GetCommand("WSGRI.DigitalFactory.DF2DControl.cmdPan");
                if (command != null) command.Execute();

				return;
			}
			
			if (keyCode == 46)   //DEL��,ɾ��ѡ�е�Ҫ��
			{
				CommonFunction.DelFeaturesFromArray(m_MapControl,ref m_OriginFeatureArray);

				Reset();
			    
				return;
			}

			if (!m_bBeginDrawLineFeed)
			{
				if (keyCode == 13 || keyCode == 32)//��ENTER��SPACEBAR����ʼ�����и���
				{       
					m_bBeginDrawLineFeed = true;
				}

			}
			else//ִ���и���\�����
			{
				DoSplit(); 

				return;

			}

			

		}
	
		public void DoSplit()//�ָ���\��
		{
			switch(((IFeatureLayer)m_App.CurrentEditLayer).FeatureClass.ShapeType)
			{
				case esriGeometryType.esriGeometryPolyline:
					SplitPolylines();
					break;
				case esriGeometryType.esriGeometryPolygon:
					SplitPolygons();
					break;
			}

		}

		public void SplitPolylines()//�ָ���
		{
			m_pLineFeed = (INewLineFeedback)m_pFeedback;	
			IPolyline  pFeatureScissors = (IPolyline)m_pLineFeed.Stop();//���������и���
			if (pFeatureScissors.Length==0)
			{
				Reset();
				return;
			}

			ITopologicalOperator pTopologim_CalOperator = (ITopologicalOperator)pFeatureScissors;

			ILayer pFeatureLayer;
			pFeatureLayer = m_App.CurrentEditLayer;
			IGeometry pOldGeometry;
			IFeature  pOldFeature;
			
			IWorkspaceEdit pWorkspaceEdit; 

			pWorkspaceEdit = (IWorkspaceEdit) CommonFunction.GetLayerWorkspace(pFeatureLayer);
			if (pWorkspaceEdit == null) return;
			if (!pWorkspaceEdit.IsBeingEdited()) return;	
			pWorkspaceEdit.StartEditOperation();

			for (int i =0; i<m_OriginFeatureArray.Count; i++)//����ÿ��ѡ�е�Ҫ��
			{
				pOldFeature=(IFeature)m_OriginFeatureArray.get_Element(i);
				pOldGeometry =(IGeometry)pOldFeature.Shape;

				IArray pArray =  new ArrayClass();//������Ҫ�ص�������Ϣ��ӵ���������
				pArray = CommonFunction.GeometryToArray(pOldGeometry);

				//��ת�����˲����ӿڣ��󡰼�������ѡ��Ҫ�صĽ���
				IGeometry pIntersectGeo = pTopologim_CalOperator.Intersect(pOldGeometry, esriGeometryDimension.esriGeometry0Dimension);
				if (pIntersectGeo == null) return ;//�޽��㣬�򷵻�

				ITopologicalOperator  pTopOp = (ITopologicalOperator) pIntersectGeo;
				pTopOp.Simplify();
				IPointCollection pPointCol = (IPointCollection)pIntersectGeo;//����ļ���

				//���ཻ�ĵ㼯�ϴ�ϸ���
				IPointCollection pTmpPointCol = new MultipointClass();
				pTmpPointCol.AddPointCollection(pPointCol);//��ʱ�㼯

				IPolycurve2 pPolyCurve;
				pPolyCurve = (IPolycurve2)pOldGeometry;//�����е���Ҫ��
				((ITopologicalOperator)pPolyCurve).Simplify();
								
				IGeometryCollection  pGeoCollection;
				IGeometryCollection  pTmpGeoCollection;    //����ÿ�δ�ϲ������߶�			

				pTmpGeoCollection =(IGeometryCollection)pPolyCurve;
				pGeoCollection    =(IGeometryCollection)pPolyCurve;	

				for( int j=0; j< pPointCol.PointCount; j++)//����ÿ������
				{
					IPoint pSplitPoint = pPointCol.get_Point(j);
					
					int GeoCount = 0;
					int pGeoCollectionCount = pGeoCollection.GeometryCount;
					while(GeoCount < pGeoCollectionCount)//����ÿ����������
					{
						IPolycurve2 pTmpPolycurve2;
						pTmpPolycurve2 = CommonFunction.BuildPolyLineFromSegmentCollection((ISegmentCollection)pGeoCollection.get_Geometry(GeoCount));					

						bool bProject;   //�Ƿ�ͶӰ
						bool bCreatePart;//�Ƿ񴴽��µĸ���
						bool bSplitted;  //�����Ƿ�ɹ�
						int lNewPart;
						int lNewSeg;
						bProject    = true;
						bCreatePart = true;

						((ITopologicalOperator)pTmpPolycurve2).Simplify();
					
						pTmpPolycurve2.SplitAtPoint(pSplitPoint,bProject,bCreatePart,out bSplitted,out lNewPart, out lNewSeg);
	
						if(bSplitted)//����pGeoCollection
						{
							pGeoCollection.RemoveGeometries(GeoCount, 1);
							pTmpGeoCollection =(IGeometryCollection)pTmpPolycurve2;
							pGeoCollection.AddGeometryCollection(pTmpGeoCollection);
						}

						GeoCount++;
					}

				}

				IGeometryCollection pGeometryCol = pGeoCollection;//����Ϻ���ߵļ���
				for(int intCount = 0 ;intCount< pGeometryCol.GeometryCount;intCount++)
				{
					IPolycurve2  pPolyline = CommonFunction.BuildPolyLineFromSegmentCollection((ISegmentCollection)pGeometryCol.get_Geometry(intCount));
					CommonFunction.AddFeature(m_MapControl,(IGeometry)pPolyline,m_App.CurrentEditLayer, pOldFeature, pArray); 	
				}
				pOldFeature.Delete();
		
			}

			m_App.Workbench.CommandBarManager.Tools["2dmap.DFEditorTool.Undo"].SharedProps.Enabled = true;

			pWorkspaceEdit.StopEditOperation();
			
			Reset();

		}

		public void SplitPolygons()//�ָ���
		{
			m_pLineFeed = (INewLineFeedback)m_pFeedback;
	
			if(m_pLineFeed==null)
			{
				Reset();
				return;
			}

			IPolyline  pFeatureScissors = m_pLineFeed.Stop();//���������и���
			if (pFeatureScissors.Length==0)
			{
				Reset();
				return;
			}

			ILayer pFeatureLayer;
			pFeatureLayer = m_App.CurrentEditLayer;
			IGeometry pOldGeometry;
			IFeature  pOldFeature;
			
			IWorkspaceEdit pWorkspaceEdit; 

			pWorkspaceEdit = (IWorkspaceEdit) CommonFunction.GetLayerWorkspace(pFeatureLayer);
			if (pWorkspaceEdit == null) return;
			pWorkspaceEdit.StartEditOperation();
			
			for (int i =0; i<m_OriginFeatureArray.Count; i++)//����ÿ��ѡ�е�Ҫ��
			{				
				IArray pArrGeo = new ArrayClass();
				pOldFeature=(IFeature)m_OriginFeatureArray.get_Element(i);
				pOldGeometry =(IGeometry)pOldFeature.Shape;

				if ((pOldGeometry == null) || (pFeatureScissors == null)) return ;
				if (pOldGeometry.GeometryType != esriGeometryType.esriGeometryPolygon) return ;

				ITopologicalOperator pTopologim_CalOperator = (ITopologicalOperator)pOldGeometry;
				IGeometry oRsGeo_1 = null, oRsGeo_2 = null;
				try
				{
					pTopologim_CalOperator.Simplify();
					pTopologim_CalOperator.Cut(pFeatureScissors, out oRsGeo_1, out oRsGeo_2);
				
					IGeometryCollection oGeoCol = (IGeometryCollection) oRsGeo_1;
					for (int j = 0; j < oGeoCol.GeometryCount; j++)
					{
						ISegmentCollection oNewPoly = new PolygonClass();
						oNewPoly.AddSegmentCollection((ISegmentCollection) oGeoCol.get_Geometry(j));
						pArrGeo.Add(oNewPoly);
					}
					oGeoCol = (IGeometryCollection) oRsGeo_2;
					for (int j = 0; j < oGeoCol.GeometryCount; j++)
					{
						ISegmentCollection oNewPoly = new PolygonClass();
						oNewPoly.AddSegmentCollection((ISegmentCollection) oGeoCol.get_Geometry(j));
						pArrGeo.Add(oNewPoly);
					}	

					for(int j=0;j<pArrGeo.Count;j++)
					{ 
						CommonFunction.AddFeature0(m_MapControl,(IGeometry)pArrGeo.get_Element(j), m_App.CurrentEditLayer, pOldFeature);
					}
					pOldFeature.Delete();
				}
				catch 
				{
					//MessageBox.Show(Ex.ToString());
				}
		
			}			
			m_App.Workbench.CommandBarManager.Tools["2dmap.DFEditorTool.Undo"].SharedProps.Enabled = true;

			pWorkspaceEdit.StopEditOperation();
			
			Reset();
		}

		private void Reset()//ȡ�����в���
		{
			m_bIsSelect = false;
			m_bBeginDrawLineFeed = false;
			m_OriginFeatureArray.RemoveAll();

			m_bIsDrawLineFeed    = false;
			m_pFeedback = null;
			m_pLineFeed = null;
			m_pFeedbackEnve = null;

//			CommonFunction.m_SelectArray.RemoveAll();  // ����  2007-09-28
//			CommonFunction.m_OriginArray.RemoveAll();  // ����  2007-09-28
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


