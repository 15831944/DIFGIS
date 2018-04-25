/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����CalculateDistance.cs
			// �ļ������������������
			//
			// 
			// ������ʶ��YuanHY 20060614
            // �������裺1��������Ŧ��
			//           2���ڵ�ͼ�ϵ����ꣻ
			//			 3��˫���������Ҽ�\�س�\�ո���������Ի���;
			//           4�����ȷ����ť,ɾ�����ɵĵ�ͼԪ�ء�������������
			// ����˵����ESC�� ȡ�����в���
��
			// �޸ı�ʶ��Modify by YuanHY20081112
            // �޸�������������ESC���Ĳ�����    
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
    /// CalculateDistance ��ժҪ˵����
    /// </summary>
    public class CalculateDistance : AbstractMapCommand
    {
        private IDFApplication m_App;
        private IMapControl2 m_MapControl;
        private IMap m_FocusMap;
        private IActiveView m_pActiveView;
        private IMapView m_MapView = null;

        private IDisplayFeedback m_pFeedback;
        private INewLineFeedback m_pLineFeed;

        private bool m_bInUse;
        public static IPoint m_pPoint;
        public static IPoint m_pAnchorPoint;
        private IPoint m_pLastPoint;

        private IArray m_pRecordPointArray = new ArrayClass();
        private double m_dblDistance;
        private double m_dblTotalDistance;

        private IStatusBarService m_pStatusBarService;

        //private bool	isEnabled   = true;
        private string strCaption = "�������";
        private string strCategory = "����";


        public CalculateDistance()
        {
            //
            // TODO: �ڴ˴���ӹ��캯���߼�
            //
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

            CurrentTool.m_CurrentToolName = CurrentTool.CurrentToolName.CalculateDistance;

            CommonFunction.MapRefresh(m_pActiveView);

            //��¼�û�����
            clsUserLog useLog = new clsUserLog();
            useLog.UserName = DFApplication.LoginUser;
            useLog.UserRoll = DFApplication.LoginSubSys;
            useLog.Operation = "��������";
            useLog.LogTime = System.DateTime.Now;
            useLog.TableLog = (m_App.CurrentWorkspace as IFeatureWorkspace).OpenTable("WSGRI_LOG");
            useLog.setUserLog();

        }

        public override void UnExecute()
        {
            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;

        }

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawLine.OnMouseDown ʵ��
            base.OnMouseDown(button, shift, x, y, mapX, mapY);

            CalculateDistanceMouseDown(m_pAnchorPoint);

        }


        private void CalculateDistanceMouseDown(IPoint pPoint)
        {
            if (!m_bInUse)//�������û��ʹ��
            {
                m_bInUse = true;

                m_pRecordPointArray.Add(pPoint);

                m_pLastPoint = pPoint;

                CommonFunction.DrawPointSMSSquareSymbol(m_MapControl, pPoint);

                m_pFeedback = new NewLineFeedbackClass();
                m_pLineFeed = (INewLineFeedback)m_pFeedback;
                m_pLineFeed.Start(pPoint);
                if (m_pFeedback != null) m_pFeedback.Display = m_pActiveView.ScreenDisplay;

            }
            else//����������ʹ����
            {
                m_pLineFeed.Stop();
                m_pLineFeed.Start(pPoint);

                IPoint tempPoint = new PointClass();
                tempPoint.X = pPoint.X;
                tempPoint.Y = pPoint.Y;
                m_pRecordPointArray.Add(tempPoint);

                m_pLastPoint = tempPoint;

                m_dblDistance = CommonFunction.GetDistance_P12((IPoint)m_pRecordPointArray.get_Element(m_pRecordPointArray.Count - 2), m_pLastPoint);//���һ�εĳ���
                m_dblTotalDistance = m_dblTotalDistance + m_dblDistance;  //�ܳ���			

                CommonFunction.DisplaypSegmentColToScreen(m_MapControl, ref m_pRecordPointArray);//����ˢ����Ļ��

            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawLine.OnMouseMove ʵ��
            base.OnMouseMove(button, shift, x, y, mapX, mapY);

            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;

            m_pPoint = m_pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);

            m_pAnchorPoint = m_pPoint;
            //+++++++++++++��ʼ��׽+++++++++++++++++++++			
            bool flag = CommonFunction.Snap(m_MapControl, m_App.CurrentConfig.cfgSnapEnvironmentSet, (IGeometry)m_pLastPoint, m_pAnchorPoint);

            if (!m_bInUse) return;

            m_pFeedback.MoveTo(m_pAnchorPoint);

            //��ϵͳ��״̬����ת����Ϣ�����һ�εĳ���
            m_dblDistance = CommonFunction.GetDistance_P12(m_pAnchorPoint, m_pLastPoint);

            m_pStatusBarService.SetStateMessage("���һ�εĳ���:" + m_dblDistance.ToString(".###") + "�ף��ܳ��ȣ�" + (m_dblTotalDistance + m_dblDistance).ToString(".###") + "��");

        }

        public override void OnDoubleClick(int button, int shift, int x, int y, double mapX, double mapY)
        {
            // TODO:  ��� DrawLine.OnDoubleClick ʵ��
            base.OnDoubleClick(button, shift, x, y, mapX, mapY);

            EndCalculateDistance();

        }

        //�������㹤��
        public void EndCalculateDistance()
        {
            IGeometry pGeom = null;
            IPolyline pPolyline;
            IPointCollection pPointCollection;
            System.Windows.Forms.DialogResult result;

            pPolyline = (IPolyline)CommonFunction.MadeSegmentCollection(ref m_pRecordPointArray);

            if (m_bInUse)
            {

                pPointCollection = (IPointCollection)pPolyline;
                if (pPointCollection.PointCount < 2)
                {
                    MessageBox.Show("���ϱ�����������!");
                }
                else
                {
                    pGeom = (IGeometry)pPointCollection;
                }

                CommonFunction.AddElement(m_MapControl, pGeom);//���Ƶ�ͼԪ��

                result = MessageBox.Show("�ܳ�Ϊ:" + m_dblTotalDistance.ToString(".###") + "��", "�������", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    Reset();//��λ;
                }

            }
        }

        private void Reset()
        {
            m_bInUse = false;
            m_pRecordPointArray.RemoveAll();//��ջ������� 
            m_pLineFeed = null;
            m_dblDistance = 0;
            m_dblTotalDistance = 0;

            m_pActiveView.FocusMap.ClearSelection();
            m_pActiveView.GraphicsContainer.DeleteAllElements();//ɾ�������ĵ�ͼԪ��
            m_pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, m_MapControl.ActiveView.Extent);//��ͼˢ��		

            m_pStatusBarService.SetStateMessage("����");

        }

        public override void Stop()
        {
            //this.Reset();
            base.Stop();
        }

        public override void OnBeforeScreenDraw(int hdc)
        {
            // TODO:  ��� DrawLine.OnBeforeScreenDraw ʵ��
            base.OnBeforeScreenDraw(hdc);

            if (m_pRecordPointArray.Count != 0)
            {
                IPoint pStartPoint = new PointClass();
                IPoint pEndPoint = new PointClass();
                pStartPoint = (IPoint)m_pRecordPointArray.get_Element(0);
                pEndPoint = (IPoint)m_pRecordPointArray.get_Element(m_pRecordPointArray.Count - 1);

                if (m_pLineFeed != null) m_pLineFeed.MoveTo(pEndPoint);
            }
        }

        //�����¼�(����Ŀ�ݼ�)
        public override void OnKeyDown(int keyCode, int shift)
        {
            // TODO:  ��� DrawLine.OnKeyDown ʵ��
            base.OnKeyDown(keyCode, shift);

            if ((keyCode == 69 || keyCode == 13 || keyCode == 32) && m_bInUse && m_pRecordPointArray.Count >= 2)//��E����ENTER ����SPACEBAR ����������
            {
                EndCalculateDistance();

                return;
            }

            if (keyCode == 27)//ESC ����ȡ�����в���
            {
                Reset();

                this.Stop();
                WSGRI.DigitalFactory.Commands.ICommand command = DFApplication.Application.GetCommand("WSGRI.DigitalFactory.DF2DControl.cmdPan");
                if (command != null) command.Execute();


                return;
            }

        }
    }
}
