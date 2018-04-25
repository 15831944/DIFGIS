/*----------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����SetTargetLayer.cs
			// �ļ��������������õ�ǰ�ɱ༭ͼ��(Ŀ��ͼ��)
			//
			// 
			// ������ʶ��LuoXuan
            // ����˵����
            //����������    
----------------------------------------------------------------*/
using System;
using System.Collections.Generic;

using DF2DControl.Command;
using DF2DControl.UserControl.View;
using DF2DControl.Base;
using DF2DData.Class;
using DF2DEdit.Class;
using DFWinForms.Service;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;

namespace DF2DEdit.BaseEdit
{
	/// <summary>
	/// SetEditLayer ��ժҪ˵����
	/// </summary>
    public class SetTargetLayer : AbstractMap2DCommand 			
	{
        private DF2DApplication m_App;
        private IMap m_pMap;

        public override void Init(object sender)
        {
            IMap2DView mapView = UCService.GetContent(typeof(Map2DView)) as Map2DView;
            if (mapView == null) return;
            bool bBind = mapView.Bind(this);
            if (!bBind) return;

            m_App = DF2DApplication.Application;
            if (m_App == null || m_App.Current2DMapControl == null) return;

            m_pMap = m_App.Current2DMapControl.ActiveView.FocusMap;
            if (m_pMap == null)
            {
                return;
            }
            if (m_pMap.LayerCount == 0)
            {
                return;
            }

            BarEditItem item = sender as BarEditItem;
            if (item.Edit is RepositoryItemComboBox)
            {
                RepositoryItemComboBox ricb = item.Edit as RepositoryItemComboBox;
                for (int i = 0; i < m_pMap.LayerCount; i++)
                {
                    ILayer curLayer = m_pMap.get_Layer(i);
                    AddLayersToTargetLayerComboBox(curLayer, ricb);
                }
            }

        }

        public override void Run(object sender, System.EventArgs e)
        {
            IMap2DView mapView = UCService.GetContent(typeof(Map2DView)) as Map2DView;
            if (mapView == null) return;
            bool bBind = mapView.Bind(this);
            if (!bBind) return;
            if (m_App == null || m_App.Current2DMapControl == null) return;

            ComboBoxEdit cbEdit = sender as ComboBoxEdit;
            Item item = cbEdit.SelectedItem as Item;
            Common.CurEditLayer = item.Value as ILayer;

            //ѡ����ǰ�༭ͼ����Զ������༭ģʽ
            Common.StartEditing(m_pMap);

            //�Զ�������ϲ�׽
            CfgSnapEnvironmentSet cfgSnapEnvironmentSet = new CfgSnapEnvironmentSet();
            cfgSnapEnvironmentSet.Tolerence = 15;
            cfgSnapEnvironmentSet.IsOpen = true;

            SnapStruct.BoolSnapMode mode = cfgSnapEnvironmentSet.SnapMode;
            mode.Endpoint = true;
            mode.Intersection = true;
            mode.PartBoundary = true;
            mode.PartVertex = true;
            cfgSnapEnvironmentSet.SnapMode = mode;

            m_App.Workbench.UpdateMenu();
        }

        #region ���Ŀ��ͼ��������
        /// <summary>
        /// ���Ŀ��ͼ��������
        /// </summary>
        /// <param name="pLayer"></param>
        /// <param name="ricb"></param>
        private void AddLayersToTargetLayerComboBox(ILayer pLayer, RepositoryItemComboBox ricb)
        {
            if (pLayer is IGroupLayer)//��������ͼ��
            {
                ICompositeLayer groupLayer = (ICompositeLayer)pLayer;

                for (int j = 0; j < groupLayer.Count; j++)
                {
                    //�ݹ�
                    AddLayersToTargetLayerComboBox(groupLayer.get_Layer(j), ricb);
                }
            }
            else if (pLayer is IFeatureLayer) //����ǵ���Ҫ��ͼ��
            {
                //�ų�CADͼ��
                if ((pLayer as IFeatureLayer).DataSourceType == "CAD Annotation Feature Class" || (pLayer as IFeatureLayer).DataSourceType == "CAD Point Feature Class"
                    || (pLayer as IFeatureLayer).DataSourceType == "CAD Polyline Feature Class" || (pLayer as IFeatureLayer).DataSourceType == "CAD Polygon Feature Class")
                {
                    return;
                }
                //�ж�ͼ���Ƿ�ɱ༭
                IDatasetEditInfo pEdit = (IDatasetEditInfo)((pLayer as IFeatureLayer).FeatureClass);
                if (!pEdit.CanEdit)
                {
                    return;
                }
                //��ͼ����ӽ�ComBox
                Item item = new Item(pLayer.Name, pLayer);
                ricb.Items.Add(item);
            }
        }
        #endregion

	}		

}
