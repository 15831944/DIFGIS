/*----------------------------------------------------------------
            // Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
            // ��Ȩ���С� 
            //
            // �ļ�����ModifyPoint.cs
            // �ļ������������޸ĵ�����ͼ��Ҫ�صļ�����Ϣ
            //
            // 
            // ������ʶ������20051230
            //
            // �޸ı�ʶ��
            // �޸�������
            //
            // �޸ı�ʶ��
            // �޸�������
----------------------------------------------------------------*/

using System;
using System.Data;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Forms;

using DF2DEdit.Interface;

namespace DF2DEdit.CmdModify
{
	/// <summary>
	/// ModifyPoint ��ժҪ˵����
	/// </summary>
	public class ModifyPoint : IUpdatePoint 
	{
		public ModifyPoint()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

        private IFeature m_Feature;

        #region IUpdatePoint ��Ա

        public IFeature Feature
        {
            get
            {
                // TODO:  ��� ModifyPoint.Feature getter ʵ��
                return m_Feature;
            }
            set
            {
                // TODO:  ��� ModifyPoint.Feature setter ʵ��
                m_Feature = value;
            }
        }

        public void UpdatePoint(int partIndex, int pointIndex, IPoint newPoint)
        {
            // TODO:  ��� ModifyPoint.WSGRI.DigitalFactory.DFQuery.IUpdatePoint.UpdatePoint ʵ��
            if (newPoint!=null)
            {
                Feature.Shape = newPoint;
                Feature.Store();
            }

        }

        #endregion
    }
}
