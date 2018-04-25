/*-----------------------------------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����ModifyMove.cs
			// �ļ������������ƶ�
			//
			// 
			// ������ʶ��LuoXuan 20170822
            // �������裺1��������Ŧ��
			//           2��ѡ��һ��������\��\��Ҫ�أ�
			//			 3������Ҽ���ENTER ����SPACEBAR ����ֹͣѡ��Ҫ�ز�����
			//           4����������ȷ���ƶ��ĵ�1���㣻
			//           5����������ȷ���ƶ��ĵ�2����,����ƶ�������
            // ����˵����
			//           1��ESC�� ȡ�����в���
-----------------------------------------------------------------------------------------*/
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
    /// ModifyMove ��ժҪ˵����
    /// </summary>
    public class ModifyMove : AbstractMap2DCommand
    {
        private DF2DApplication m_App;
        private IMapControl2 m_MapControl;
        private IMap m_FocusMap;
        private ILayer m_CurrentLayer;
        private IActiveView m_pActiveView;

        private static IPoint m_pPoint;
        private IPoint m_pPoint0 = new PointClass();
        private IPoint m_pPoint1 = new PointClass();
        private IPoint m_pPoint2 = new PointClass();

        private bool bBegineMove;//��ʼ�ƶ�
        private bool bMoving;    //�����ƶ�
        public static IPoint m_pAnchorPoint;
        private IPoint m_pLastPoint;
        private double m_dblTolerance;     //�̶�����ֵ

        private IArray m_OriginFeatureArray = new ArrayClass();

        private IElement m_pBasePointElement;

        private IEnvelope m_pEnvelope;//��ͼˢ�µ���С��Χ

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
            m_CurrentLayer = Class.Common.CurEditLayer;
            m_OriginFeatureArray.RemoveAll();
            m_dblTolerance = Class.Common.ConvertPixelsToMapUnits(m_MapControl.ActiveView, 4);
            bBegineMove = true;
            bMoving = false;

            IArray tempArray = Class.Common.GetSelectFeatureSaveToArray_2(m_FocusMap);
            for (int i = 0; i < tempArray.Count; i++)
            {
                m_OriginFeatureArray.Add((IFeature)tempArray.get_Element(i));
            }
            tempArray.RemoveAll();
            Class.Common.MadeFeatureArrayOnlyAloneOID(m_OriginFeatureArray);//��֤����Ԫ�ص�Ψһ��

            m_pEnvelope = Class.Common.GetMinEnvelopeOfTheFeatures(m_OriginFeatureArray);
            if (m_pEnvelope != null && !m_pEnvelope.IsEmpty) m_pEnvelope.Expand(1, 1, false);
        }

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� ModifyMove.OnMouseDown ʵ��
            base.OnMouseDown(button, shift, x, y, mapX, mapY);

            if (button == 1 && bBegineMove)//ȷ���ƶ��ĵ�1����
            {
                bBegineMove = false;
                bMoving = true;

                m_pPoint0 = m_pAnchorPoint;
                m_pPoint1 = m_pAnchorPoint;
                m_pPoint2 = m_pAnchorPoint;
                m_pLastPoint = m_pAnchorPoint;

                m_pBasePointElement = Class.Common.DrawPointSMSXSymbol(m_MapControl, m_pAnchorPoint);

                return;
            }

            if (button == 1 && bMoving) //ȷ���ƶ��ĵ�2����,ִ���ƶ�����
            {
                m_pPoint2 = m_pAnchorPoint;

                MoveFeature();

                Reset();//��λ

                return;
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� ModifyMove.OnMouseMove ʵ��
            base.OnMouseMove(button, shift, x, y, mapX, mapY);

            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;

            m_App.Workbench.SetStatusInfo("����:1.�����ȷ���ƶ��ĵ�1������;2.�����ȷ���ƶ��ĵ�2������,ʵʩ�ƶ�������(ESC:ȡ��/DEL:ɾ��)");

            m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            m_pAnchorPoint = m_pPoint;

            if (bBegineMove || bMoving)
            {
                //+++++++++++++��ʼ��׽+++++++++++++++++++++			
                //bool flag = Class.Common.Snap(m_MapControl, Class.SelectionEnv.cfgSnapEnvironmentSet, m_pPoint0, m_pPoint);
            }

            if (bMoving)//�ƶ������ĵ�ͼԪ�أ�����ƶ��Ӿ�
            {
                m_pPoint1 = m_pPoint2;
                m_pPoint2 = m_pAnchorPoint;

                if (m_pPoint1 != null && m_pPoint2 != null)
                {
                    if (!m_pPoint1.IsEmpty && !m_pPoint2.IsEmpty)
                    {
                        MoveElement();
                    }
                }
            }

        }

        public override void OnMouseUp(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� ModifyMove.OnMouseUp ʵ��
            base.OnMouseUp(button, shift, x, y, mapX, mapY);

            if (bBegineMove) return;
            if (bMoving) return;
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            // TODO:  ��� ModifyMove.OnKeyDown ʵ��
            base.OnKeyDown(keyCode, shift);

            if (keyCode == 27)//ESC ����ȡ�����в���
            {
                Reset();

                DF2DApplication.Application.Workbench.BarPerformClick("Pan");

                return;
            }
            if (keyCode == 46)   //DEL��,ɾ��ѡ�е�Ҫ��
            {
                Class.Common.DelFeaturesFromArray(m_MapControl, ref m_OriginFeatureArray);

                Reset();

                return;
            }
        }

        private void Reset()
        {
            //m_bIsSelect = false;
            bBegineMove = false;
            bMoving = false;

            m_pPoint0.SetEmpty();
            m_pPoint1.SetEmpty();
            m_pPoint2.SetEmpty();
            m_pAnchorPoint.SetEmpty();

            if (m_pLastPoint != null) m_pLastPoint.SetEmpty();

            m_OriginFeatureArray.RemoveAll();

            //			CommonFunction.m_SelectArray.RemoveAll();  // ����  2007-09-28
            //			CommonFunction.m_OriginArray.RemoveAll();  // ����  2007-09-28
            m_pActiveView.GraphicsContainer.DeleteAllElements();//ɾ��ѡ��ʱ�����ĵ�ͼԪ��

            m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��
            m_pEnvelope = null;

            m_App.Workbench.SetStatusInfo("����");

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
        }

        #region  //�ƶ�ѡ��Ҫ��
        private void MoveFeature()
        {
            ITransform2D pTrans2d;
            IWorkspaceEdit pWorkspaceEdit;	//�����ռ�ı༭�ӿ�	
            
            pWorkspaceEdit = Class.Common.CurWspEdit;
            pWorkspaceEdit.StartEditOperation();

            m_pEnvelope = ((IFeature)m_OriginFeatureArray.get_Element(0)).Extent;

            for (int i = 0; i < m_OriginFeatureArray.Count; i++)
            {
                IFeature pFeature = (IFeature)m_OriginFeatureArray.get_Element(i);

                if (pFeature.FeatureType == esriFeatureType.esriFTAnnotation)
                {
                    IAnnotationFeature pAnnotationFeature = pFeature as IAnnotationFeature;
                    IElement element = new TextElementClass();
                    element = pAnnotationFeature.Annotation;

                    IPoint pPointOld = null;
                    if (element.Geometry.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        IPolyline pPolyline = element.Geometry as IPolyline;
                        pPointOld = pPolyline.FromPoint;
                    }
                    else if (element.Geometry.GeometryType == esriGeometryType.esriGeometryPoint)
                    {
                        pPointOld = element.Geometry as IPoint;
                    }
                    IPoint pPointNew = new PointClass();
                    pPointNew.PutCoords(pPointOld.X + (m_pPoint2.X - m_pPoint0.X), pPointOld.Y + (m_pPoint2.Y - m_pPoint0.Y));
                    element.Geometry = pPointNew;

                    try
                    {
                        pAnnotationFeature.Annotation = element;
                        ((IFeature)pAnnotationFeature).Store();
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("��ǰ����������Ч���귶Χ�ڣ�����ʧ��");
                    }
                    m_pEnvelope.Union(pFeature.Shape.Envelope);

                }
                else
                {
                    pTrans2d = (ITransform2D)pFeature.Shape; //�ӿڵ���ת
                    pTrans2d.Move(m_pPoint2.X - m_pPoint0.X, m_pPoint2.Y - m_pPoint0.Y);

                    m_pEnvelope.Union(((IGeometry)pTrans2d).Envelope);

                    pFeature.Shape = (IGeometry)pTrans2d;
                    pFeature.Store();
                }
            }
            pWorkspaceEdit.StopEditOperation();
            m_App.Workbench.UpdateMenu();

            //m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_pEnvelope);//��ͼˢ��


        }
        #endregion

        #region  //�ƶ���ѡ����ͼҪ�ش����ĵ�ͼԪ�أ�����ƶ��Ӿ�
        private void MoveElement()
        {
            ITransform2D pTrans2d;

            IGraphicsContainer pGraphicsContainer;
            pGraphicsContainer = m_MapControl.ActiveView.GraphicsContainer;

            pGraphicsContainer.Reset();
            IElement pElement = pGraphicsContainer.Next();

            if (pElement != null)
            {
                m_pEnvelope = pElement.Geometry.Envelope;
            }

            while (pElement != null)
            {
                if (!pElement.Equals((object)m_pBasePointElement))
                {

                    pTrans2d = (ITransform2D)pElement.Geometry; //�ӿڵ���ת
                    pTrans2d.Move(m_pPoint2.X - m_pPoint1.X, m_pPoint2.Y - m_pPoint1.Y);
                    pElement.Geometry = (IGeometry)pTrans2d;

                }

                m_pEnvelope.Union(pElement.Geometry.Envelope);

                pElement = pGraphicsContainer.Next();
            }

            m_pEnvelope.Expand(2, 2, false);

            m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_pEnvelope);//��ͼˢ��
        }
        #endregion

    }
}
