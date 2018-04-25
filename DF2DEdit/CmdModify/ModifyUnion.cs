/*-------------------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����ModifyUnion.cs
			// �ļ�����������������\��(���ܲ�������Ҫ�أ��ŵ�Ŀ��ͼ���д洢)
			//
			// 
			// ������ʶ��LuoXuan 20171011
            // �������裺1��������Ŧ��
			//           2��ѡ�����߻���Ҫ��(����ѡ����ͼ���Ҫ��)��
			//			 3������Ҽ�\�س�\�ո����ʵʩ���ϲ�����
			// ����˵����
			//           1��ESC�� ȡ�����в���
			//           2��DEL�� ɾ��ѡ�е�Ҫ�ء���
---------------------------------------------------------------------------*/
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
using DF2DEdit.Class;
using DFWinForms.Service;

namespace DF2DEdit.CmdModify
{
	/// <summary>
	/// ModifyUnion ��ժҪ˵����
	/// </summary>

    public class ModifyUnion : AbstractMap2DCommand
	{
        private DF2DApplication m_App;
		private IMapControl2   m_MapControl;
		private IMap           m_FocusMap;
		private IActiveView    m_pActiveView;
		private ILayer         m_CurrentLayer;

		private IPoint m_pPoint;
		private bool   m_bIsSelect;//��ʶ�Ƿ���ѡ��Ҫ��		
		private INewEnvelopeFeedback  m_pFeedbackEnve; //���ο���ʾ����
		private IArray m_FeatureArray = new ArrayClass();//ԴҪ������
		private IEnvelope m_pEnvelope = new EnvelopeClass();

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

			CurrentTool.m_CurrentToolName = CurrentTool.CurrentToolName.modifyUnion;

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

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

			m_bIsSelect = true;

			if (button == 2)//���ϲ���
			{	
				DoUnion();
			}

		}

		public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
		{
			// TODO:  ��� ModifyAddVertex.OnDoubleClick ʵ��
			base.OnDoubleClick (button, shift, x, y, mapX, mapY);
			Reset();
		}

		public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
		{
			base.OnMouseMove (button, shift, x, y, mapX, mapY);

			m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair ;

			m_pStatusBarService.SetStateMessage("����:1.ѡ����Ҫ��(�߻���);2.�Ҽ�/�س�/�ո��,ʵʩ���ɶಿ��(����)������(ESC:ȡ��/DEL:ɾ��)");

			if(!m_bIsSelect ) return;

			m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);	
	
			if (m_pFeedbackEnve == null ) 
			{
				m_pFeedbackEnve = new NewEnvelopeFeedbackClass();
				m_pFeedbackEnve.Display = m_pActiveView.ScreenDisplay;
				m_pFeedbackEnve.Start(m_pPoint);
			}
			m_pFeedbackEnve.MoveTo(m_pPoint);		
						
		}
	
		public override void OnMouseUp(int button, int shift, int x, int y, double mapX, double mapY)
		{
			base.OnMouseUp (button, shift, x, y, mapX, mapY);

			if(!m_bIsSelect ) return;
			
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
				dblConst = Class.Common.ConvertPixelsToMapUnits(m_pActiveView,8);//8�����ش�С
                pRect = Class.Common.NewRect(m_pPoint, dblConst);
				m_FocusMap.SelectByShape(pRect,null,false);
			}

            IArray tempArray = Class.Common.GetSelectedFeaturesSaveToArray(m_FocusMap, ((IFeatureLayer)m_App.CurrentEditLayer).FeatureClass.ShapeType);
			
			if(tempArray.Count>0)
			{
				if(((IFeature)tempArray.get_Element(0)).Shape.GeometryType == esriGeometryType.esriGeometryPoint) 
				{   //�������ϵ�Ҫ��
					Reset();
					return;
				}

				for (int i = 0; i<tempArray.Count; i++)
				{
					m_FeatureArray.Add((IFeature)tempArray.get_Element(i)); 
				}
				tempArray.RemoveAll();//�����ʱ����             
        
				CommonFunction.MadeFeatureArrayOnlyAloneOID(m_FeatureArray);//ʹ��ԴҪ���������Ψһ��

				m_pEnvelope = CommonFunction.GetMinEnvelopeOfTheFeatures(m_FeatureArray);
				if(m_pEnvelope != null &&!m_pEnvelope.IsEmpty )  m_pEnvelope.Expand(1,1,false);

				if(m_FeatureArray.Count !=0)
				{
					m_MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
					CommonFunction.ShowSelectionFeatureArray(m_MapControl,m_FeatureArray);//������ʾѡ���Ҫ��
				}
			}
			//ѡ��λ
			m_pFeedbackEnve = null;				
			m_bIsSelect = false;
			m_FocusMap.ClearSelection();//��յ�ͼѡ���Ҫ��
			
		}
		
		private void Reset()//ȡ�����в���
		{
			m_bIsSelect = false;
			m_FeatureArray.RemoveAll();
			m_pFeedbackEnve =null;
			m_FocusMap.ClearSelection();//��յ�ͼѡ���Ҫ��
		
//			CommonFunction.m_SelectArray.RemoveAll();  // ����  2007-09-28
//			CommonFunction.m_OriginArray.RemoveAll();  // ����  2007-09-28
			m_pActiveView.GraphicsContainer.DeleteAllElements(); 
			m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��
			m_pEnvelope  = null;

			m_pStatusBarService.SetStateMessage("����");

		}	

		public override void Stop()
		{
			//this.Reset();
			base.Stop();
		}

		public void DoUnion()//���ϲ���
		{
			if (m_FeatureArray.Count<2)
			{
				Reset();
				return;
			}

			ILayer pFeatureLayer;
			pFeatureLayer = m_App.CurrentEditLayer;
			IGeometry pOldGeometry;
			IFeature  pOldFeature;
			IGeometry pOtherGeo;
			
			IWorkspaceEdit pWorkspaceEdit;
			pWorkspaceEdit = (IWorkspaceEdit) CommonFunction.GetLayerWorkspace(pFeatureLayer);
			if (pWorkspaceEdit == null) return;
			if (!pWorkspaceEdit.IsBeingEdited()) return;
			pWorkspaceEdit.StartEditOperation();
	
			pOldFeature  = (IFeature)m_FeatureArray.get_Element(0);
			pOldGeometry = (IGeometry)pOldFeature.Shape;
			IArray pArrayPoint = new ArrayClass();//��������Ϣ�洢����������
			pArrayPoint = CommonFunction.GeometryToArray(pOldFeature.ShapeCopy);

			for (int i=1; i<m_FeatureArray.Count; i++)//����ÿ��ѡ�е�Ҫ��
			{				
				pOtherGeo    = (IGeometry)((IFeature)m_FeatureArray.get_Element(i)).Shape;				
				
				IArray pTempArrayPoint = new ArrayClass();//��������Ϣ�洢����������
				pTempArrayPoint = CommonFunction.GeometryToArray(((IFeature)m_FeatureArray.get_Element(i)).ShapeCopy);
				for(int j=0;j<pTempArrayPoint.Count;j++)
				{
					pArrayPoint.Add(pTempArrayPoint.get_Element(j) as Point);
				}

				pOldGeometry = CommonFunction.UnionGeometry(pOldGeometry,pOtherGeo);						
			}		
	
			CommonFunction.AddFeature(m_MapControl,pOldGeometry,m_App.CurrentEditLayer,pOldFeature,pArrayPoint);

			//����ɾ��ѡ�е�Ҫ����
			for (int i = 0; i < m_FeatureArray.Count; i++)//����ÿ��ѡ�е�Ҫ��
			{				
				((IFeature)m_FeatureArray.get_Element(i)).Delete();								
			}

			m_App.Workbench.CommandBarManager.Tools["2dmap.DFEditorTool.Undo"].SharedProps.Enabled = true;

			pWorkspaceEdit.StopEditOperation();
			
			Reset();
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

			if (keyCode == 13 || keyCode == 32)//��ENTER��SPACEBAR�� ��ʼ�������ϲ���
			{       
				DoUnion();

				return;
			}

			if(keyCode == 46)   //DEL��,ɾ��ѡ�е�Ҫ��
			{
				CommonFunction.DelFeaturesFromArray(m_MapControl,ref m_FeatureArray);
			    Reset();
				return;
			}
		}
		
	}
}
