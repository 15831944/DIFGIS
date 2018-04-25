/*-----------------------------------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����ModifyFeaturePro.cs
			// �ļ������������޸�Ҫ������
			//
			// 
			// ������ʶ��LuoXuan 20171022
            // �������裺1��ѡ��һ��������\��\��Ҫ�أ�
			//           2��������ť��
			//			 3������Ҫ�����ԶԻ����޸����ԣ�
-----------------------------------------------------------------------------------------*/
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

using ESRI.ArcGIS;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;

using DF2DControl.Base;
using DF2DControl.Command;
using DF2DControl.UserControl.View;
using DF2DEdit.Class;
using DFWinForms.Service;

namespace DF2DEdit.CmdModify
{
	/// <summary>
	/// cmdModify ��ժҪ˵����
	/// </summary>
    public class ModifyFeaturePro : AbstractMap2DCommand
	{
        private IMapControl2	m_MapControl;
        private IActiveView		m_ActiveView;
        private IScreenDisplay	m_Display;
        private DF2DApplication m_App;
        private IFeatureLayer m_pCurEditLayer;

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
            m_MapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
            m_ActiveView = m_MapControl.ActiveView;
            m_Display = m_ActiveView.ScreenDisplay;
            this.m_pCurEditLayer = Class.Common.CurEditLayer as IFeatureLayer;

            if (Class.Common.System_ModifyFeature_Form == null)
            {
                Class.Common.System_ModifyFeature_Form = new Form.frmModifyFeaturePro();
                Class.Common.System_ModifyFeature_Form.MapControl = m_MapControl;
                Class.Common.System_ModifyFeature_Form.Show();
            }
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

        public override void OnMouseDown(int button, int shift, int x, int y, double mapX, double mapY)
        {
            if (button==1)
            {
                IGeometry pGeoSel;
                IRubberBand band = new RubberRectangularPolygonClass();
                IGeometry geo = band.TrackNew(m_Display, null);

                SelectionEnvironmentClass s = Class.SelectionEnv.System_Selection_Environment(m_ActiveView);
                int dis = Class.SelectionEnv.System_Selection_Environment(m_ActiveView).SearchTolerance;
                IFeatureSelection pFeaSel = m_pCurEditLayer as IFeatureSelection;

                if (shift == 1)
                {
                    pFeaSel.CombinationMethod = esriSelectionResultEnum.esriSelectionResultAdd;
                    //s.CombinationMethod = esriSelectionResultEnum.esriSelectionResultAdd;
                }
                else
                {
                    pFeaSel.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                    this.m_ActiveView.FocusMap.ClearSelection();
                }

                if (geo.IsEmpty)
                {
                    IPoint searchPoint = new PointClass();
                    searchPoint.PutCoords(mapX, mapY);
                    pGeoSel = Class.Common.DoBuffer(searchPoint, dis);
                    //m_ActiveView.FocusMap.SelectByShape(Class.Common.DoBuffer(searchPoint, dis), s, false);
                }
                else
                {
                    pGeoSel = Class.Common.DoBuffer(geo, dis);
                    //m_ActiveView.FocusMap.SelectByShape(Class.Common.DoBuffer(geo, dis), s, false);
                }
                ISpatialFilter pSpaFilter = new SpatialFilter();
                pSpaFilter.Geometry = pGeoSel;
                pSpaFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                pFeaSel.SelectFeatures(pSpaFilter, pFeaSel.CombinationMethod, false);

                m_ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

                if (Class.Common.System_ModifyFeature_Form != null)
                {
                    Class.Common.System_ModifyFeature_Form.RefreshSelection();
                }

            }
        }

        public override void OnSelectionChanged() 
        {
            base.OnSelectionChanged ();
            if (Class.Common.System_ModifyFeature_Form != null)
            {
                Class.Common.System_ModifyFeature_Form.RefreshSelection();
            }
        }

        public override void OnKeyDown(int keyCode, int shift)
        {
            // TODO:  ��� ModifyOffset.OnKeyDown ʵ��
            base.OnKeyDown(keyCode, shift);

            if (keyCode == 27)//ESC ����ȡ�����в���
            {
                DF2DApplication.Application.Workbench.BarPerformClick("Pan");
                return;
            }
        }


	}
}
