    /*----------------------------------------------------------------
                // Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
                // ��Ȩ���С� 
                //
                // �ļ�����ModifyPolygon.cs
                // �ļ������������޸�������ͼ��Ҫ�صļ�����Ϣ
                //
                // 
                // ������ʶ������20051231
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
	/// ModifyPolyline ��ժҪ˵����
	/// </summary>
	public class ModifyPolyline : IUpdatePoint, IModifyGeometry
	{
        public ModifyPolyline()
        {
            //
            // TODO: �ڴ˴���ӹ��캯���߼�
            //
        }

        private IFeature m_Feature;
        private IGeometryCollection m_GeoColl;

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
                m_GeoColl = m_Feature.Shape as IGeometryCollection;
            }
        }
        
        public void UpdatePoint(int partIndex, int pointIndex, IPoint newPoint)
        {
            // TODO:  ��� ModifyPoint.WSGRI.DigitalFactory.DFQuery.IUpdatePoint.UpdatePoint ʵ��
            IPointCollection pColl = (IPointCollection)m_GeoColl.get_Geometry(partIndex);
            if ((pointIndex > -1) && (pointIndex < pColl.PointCount) )
            {
                pColl.UpdatePoint(pointIndex, newPoint);
                Feature.Shape = m_GeoColl as IGeometry;
                Feature.Store();
            }
        }

        #endregion

        #region IModifyGeometry ��Ա

        public void InsertPoint(int partIndex, int pointIndex, IPoint newPoint)
        {
            // TODO:  ��� ModifyMultiPoint.WSGRI.DigitalFactory.DFQuery.IModifyGeometry.InsertPoint ʵ��
            IPointCollection pColl = (IPointCollection)m_GeoColl.get_Geometry(partIndex);
            if ((pointIndex >= -1) && (pointIndex <= pColl.PointCount))
            {
                //object none = Type.Missing;
                pColl.InsertPoints(pointIndex,1,ref newPoint);
                Feature.Store();
            }
        }

        public void RemovePoint(int partIndex, int pointIndex)
        {
            // TODO:  ��� ModifyMultiPoint.WSGRI.DigitalFactory.DFQuery.IModifyGeometry.RemovePoint ʵ��
            IPointCollection pColl = (IPointCollection)m_GeoColl.get_Geometry(partIndex);
            if ((pointIndex > -1) && (pointIndex < pColl.PointCount) && pColl.PointCount > 2)
            {
                pColl.RemovePoints(pointIndex,1);
                Feature.Shape = (IGeometry)m_GeoColl;
                Feature.Store();
            }
        }

        public void RemovePart(int partIndex)
        {
            // TODO:  ��� ModifyMultiPoint.WSGRI.DigitalFactory.DFQuery.IModifyGeometry.RemovePart ʵ��
            if (m_GeoColl.GeometryCount<2) return;
            if(partIndex<m_GeoColl.GeometryCount)
            {
                m_GeoColl.RemoveGeometries(partIndex,1);
                Feature.Shape = (IGeometry)m_GeoColl;
                Feature.Store();
            }
        }

        #endregion

    }
}
