/*-------------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����CalculateCurveParameter.cs
			// �ļ���������������Ԫ�ؼ���
			//
			// 
			// ������ʶ��YuanHY 20060510
            // �������裺1��������Ŧ��
			//           2��ѡ��һ��Ҫ�أ������Ի���;
			//           3�����ȷ����ťʵʩ������������������
			// ����˵����ESC�� ȡ�����в���
			//           DEL�� ɾ��ѡ�е�Ҫ�ء���
			// �޸ı�ʶ����
			//           1������״̬��������ʾ��Ϣ��By YuanHY  20060615	
  		    // �޸ı�ʶ��Modify by YuanHY20081112
            // �޸�������������ESC���Ĳ����� 
----------------------------------------------------------------------*/
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
    public class CalculateCurveParameter : AbstractMapCommand
    {
        private IDFApplication m_App;
        private IMapControl2 m_MapControl;
        private IMap m_FocusMap;
        private IActiveView m_pActiveView;
        private IMapView m_MapView = null;
        private ILayer m_CurrentLayer;

        private IPoint m_pPoint;
        private bool m_bIsUse;
        private INewEnvelopeFeedback m_pFeedbackEnve; //���ο���ʾ����

        private IArray m_OriginFeatureArray = new ArrayClass();

        private IStatusBarService m_pStatusBarService;//״̬����Ϣ����

        //private bool isEnabled = false;
        private string strCaption = "����Ԫ�ؼ���";
        private string strCategory = "����";

        private IEnvelope m_pEnvelope = new EnvelopeClass();


        private frmCalculateCurveParameter m_formCalculateCurveParameter;

        public CalculateCurveParameter()
        {
            //���״̬���ķ���
            m_pStatusBarService = (IStatusBarService)ServiceManager.Services.GetService(typeof(WSGRI.DigitalFactory.Services.UltraStatusBarService));

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
                strCaption = value;
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
                strCategory = value;
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

            m_MapControl = m_App.CurrentMapControl;
            m_FocusMap = m_MapControl.ActiveView.FocusMap;
            m_pActiveView = (IActiveView)this.m_FocusMap;
            m_pStatusBarService = m_App.StatusBarService;//���״̬����

            CurrentTool.m_CurrentToolName = CurrentTool.CurrentToolName.CalculateCurveParameter;

            //CommonFunction.MapRefresh(m_pActiveView);

        }

        public override void UnExecute()
        {
            m_pStatusBarService.SetStateMessage("����");
        }

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            base.OnMouseDown(button, shift, x, y, mapX, mapY);

            m_CurrentLayer = m_App.CurrentEditLayer;

            m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

            m_bIsUse = true;

            if (button == 2)//ִ��ת������
            {
               DoCalculate();
            }
        }

        public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� ModifyAddVertex.OnDoubleClick ʵ��
            base.OnDoubleClick(button, shift, x, y, mapX, mapY);
            Reset();
        }

        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            base.OnMouseMove(button, shift, x, y, mapX, mapY);

            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;

            m_pStatusBarService.SetStateMessage("����:1.ѡ��һ�λ�����·Ҫ��;2.�Ҽ�/ENTER��/SPACEBAR/,����ѡ�񣬵�����·����Ԫ�ؼ���Ի���3.ָ��������ϵ�һ�����ߡ�Բ���ߡ��ڶ������ߵĵ�;4.������㰴ť��(ESC:ȡ��)");

            if (!m_bIsUse) return;

            if (m_pFeedbackEnve == null)
            {
                m_pFeedbackEnve = new NewEnvelopeFeedbackClass();
                m_pFeedbackEnve.Display = m_pActiveView.ScreenDisplay;
                m_pFeedbackEnve.Start(m_pPoint);
            }

            m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            m_pFeedbackEnve.MoveTo(m_pPoint);

        }

        public override void OnMouseUp(int button, int shift, int x, int y, double mapX, double mapY)
        {
            base.OnMouseUp(button, shift, x, y, mapX, mapY);

            if (!m_bIsUse) return;

            IGeometry pEnv;

            if (m_pFeedbackEnve != null)
            {
                pEnv = m_pFeedbackEnve.Stop();
                m_FocusMap.SelectByShape(pEnv, null, false);
            }
            else
            {
                IEnvelope pRect;
                double dblConst;
                dblConst = CommonFunction.ConvertPixelsToMapUnits(m_pActiveView, 8);//8�����ش�С
                pRect = CommonFunction.NewRect(m_pPoint, dblConst);
                m_FocusMap.SelectByShape(pRect, null, false);
            }

            IArray tempArray = CommonFunction.GetSelectFeatureSaveToArray(m_FocusMap);
            for (int i = 0; i < tempArray.Count; i++)
            {
                if (((IFeature)tempArray.get_Element(i)).Shape.GeometryType == esriGeometryType.esriGeometryPolyline ||
                    ((IFeature)tempArray.get_Element(i)).Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                {//ֻ��ѡ����Ҫ�ػ���Ҫ��
                    m_OriginFeatureArray.Add((IFeature)tempArray.get_Element(i)); //����Ҫ����ӵ�Դ������
                }
            }
            tempArray.RemoveAll();//�����ʱ����

            CommonFunction.MadeFeatureArrayOnlyAloneOID(m_OriginFeatureArray);//ʹ��ԴҪ���������Ψһ��

            m_MapControl.ActiveView.FocusMap.ClearSelection(); //��յ�ͼѡ���Ҫ��

            m_pEnvelope = CommonFunction.GetMinEnvelopeOfTheFeatures(m_OriginFeatureArray);
            if (m_pEnvelope != null && !m_pEnvelope.IsEmpty) m_pEnvelope.Expand(1, 1, false);

            if (m_OriginFeatureArray.Count != 0)
            {
                m_MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
                CommonFunction.ShowSelectionFeatureArray(m_MapControl, m_OriginFeatureArray);//������ʾѡ���Ҫ��

                for (int i = 0; i < m_OriginFeatureArray.Count; i++)
                {
                    IPointCollection pPointCollection = (IPointCollection)(m_OriginFeatureArray.get_Element(i) as IFeature ).Shape;

                    for (int j = 0; j < pPointCollection.PointCount; j++)
                    {
                        CommonFunction.DrawPointSMSSquareSymbol(m_MapControl, pPointCollection.get_Point(j));
                    }
                }
            }


            //ѡ��λ
            m_pFeedbackEnve = null;
            m_bIsUse = false;

        }

        private void Reset()//ȡ�����в���
        {
            m_pFeedbackEnve = null;
            m_bIsUse = false;
            m_OriginFeatureArray.RemoveAll();

            m_pActiveView.GraphicsContainer.DeleteAllElements();
            m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, m_pEnvelope);//��ͼˢ��

            m_pStatusBarService.SetStateMessage("����");

        }

        public override void Stop()
        {
            //this.Reset();
            base.Stop();
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            base.OnKeyDown(keyCode, shift);

            if (keyCode == 27)//ESC ����ȡ�����в���
            {
                Reset();

                this.Stop();
                WSGRI.DigitalFactory.Commands.ICommand command = DFApplication.Application.GetCommand("WSGRI.DigitalFactory.DF2DControl.cmdPan");
                if (command != null) command.Execute();

                return;
            }

            if (keyCode == 13 || keyCode == 32)//��ENTER ����SPACEBAR ����ִ��ת������
            {
                DoCalculate();//�������
            }

        }

        public void DoCalculate()//�������
        {
            if (m_OriginFeatureArray.Count == 0)
            {
                Reset();
                return;
            }
            System.Windows.Forms.Form m_mainForm = (System.Windows.Forms.Form)m_App.Workbench;

            frmCalculateCurveParameter.m_pFeatureArray = m_OriginFeatureArray;
            frmCalculateCurveParameter.m_pMapControl = this.m_MapControl;

            m_formCalculateCurveParameter = new frmCalculateCurveParameter();
            m_formCalculateCurveParameter.Owner = m_mainForm;

           // m_formCalculateCurveParameter.Close();

            m_formCalculateCurveParameter.Show();

            m_pFeedbackEnve = null;
            m_bIsUse = false;
            m_OriginFeatureArray.RemoveAll();


        }
    }
}
