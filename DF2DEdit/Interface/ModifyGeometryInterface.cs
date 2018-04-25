using System;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace DF2DEdit.Interface
{
	/// <summary>
	/// ����һ�������Ϣ
	/// </summary>
    public interface IUpdatePoint
    {
        /// <summary>
        /// Ҫ�޸ļ�����Ϣ��Ҫ��
        /// </summary>
        IFeature Feature{get;set;}
        /// <summary>
        /// ���µ�partIndex���ֵ�pointIndex����
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="pointIndex"></param>
        /// <param name="newPoint"></param>
        void UpdatePoint(int partIndex,int pointIndex,IPoint newPoint);
    }

    /// <summary>
    /// �޸ļ�����Ϣ
    /// </summary>
    public interface IModifyGeometry
    {
        /// <summary>
        /// Ҫ�޸ļ�����Ϣ��Ҫ��
        /// </summary>
        IFeature Feature{get;set;}
        /// <summary>
        /// �ڵ�partIndex���ֵ�pointIndex�������µĵ�
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="pointIndex"></param>
        /// <param name="newPoint"></param>
        void InsertPoint(int partIndex , int pointIndex , IPoint newPoint);
        /// <summary>
        /// ɾ����partIndex���ֵĵ�pointIndex����
        /// </summary>
        /// <param name="partIndex"></param>
        /// <param name="pointIndex"></param>
        void RemovePoint(int partIndex , int pointIndex);
        /// <summary>
        /// ɾ����partIndex����
        /// </summary>
        /// <param name="partIndex"></param>
        void RemovePart(int partIndex);
    }
}
