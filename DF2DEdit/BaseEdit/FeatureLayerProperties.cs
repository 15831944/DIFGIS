/*----------------------------------------------------------------
            // Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
            // ��Ȩ���С� 
            //
            // �ļ�����FeatureLayerProperties.cs
            // �ļ�����������ͼ������
            //               
            // 
            // ������ʶ��LuoXuan
            //
            // �޸�������

----------------------------------------------------------------*/
using System;
using System.Collections;
using System.ComponentModel;

using DF2DControl.Base;
using DF2DControl.Command;
using DF2DControl.UserControl.View;
using DFWinForms.Service;

using ESRI.ArcGIS.Carto;

namespace DF2DEdit.BaseEdit
{
	/// <summary>
	/// cmdLayerProperties ��ժҪ˵����
	/// </summary>
    public class FeatureLayerProperties : AbstractMap2DCommand
	{
        public override void Run(object sender, System.EventArgs e)
        {
            Map2DCommandManager.Push(this);
            IMap2DView mapView = UCService.GetContent(typeof(Map2DView)) as Map2DView;
            if (mapView == null) return;
            bool bBind = mapView.Bind(this);
            if (!bBind) return;
            DF2DApplication app = DF2DApplication.Application;
            if (app == null || app.Current2DMapControl == null) return;

            if (Class.Common.CurEditLayer != null)
            {
                System.Windows.Forms.Form mainForm = (System.Windows.Forms.Form)app.Workbench;
                Form.frmLayerProperty frm = new Form.frmLayerProperty();
                frm.FeatureLayer = Class.Common.CurEditLayer as IFeatureLayer;
                frm.MapControl = app.Current2DMapControl; 

                if (frm.ShowDialog(mainForm) == System.Windows.Forms.DialogResult.OK)
                {
                    app.Current2DMapControl.ActiveView.Refresh();
                }
            }
        }

        public override void RestoreEnv()
        {
            IMap2DView mapView = UCService.GetContent(typeof(Map2DView)) as Map2DView;
            if (mapView == null) return;
            mapView.UnBind(this);
            DF2DApplication app = DF2DApplication.Application;
            if (app == null || app.Current2DMapControl == null) return;
            Map2DCommandManager.Pop();
        }

	}
}
