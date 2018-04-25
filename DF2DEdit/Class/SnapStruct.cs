/*----------------------------------------------------------------
            // Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
            // ��Ȩ���С� 
            //
            // �ļ�����SnapStruct.cs
            // �ļ�������������׽ģʽ�ṹ
            //               
            // 
            // ������ʶ��LuoXuan
            //
            // �޸�������

----------------------------------------------------------------*/
using System;
using ESRI.ArcGIS.Carto;

namespace DF2DEdit.Class
{
	public class SnapStruct
	{
		public SnapStruct()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

		#region//��׽����
		public struct EnumSnapType
		{
			string partBoundary ;//����
			string partVertex;//�ڵ�
			string endpoint;//�˵�
			string intersection;//����

			public string PartBoundary//����
			{
				set
				{ 
					partBoundary = value;
				}
				get 
				{
					return "partBoundary";
				}
			}

			public string PartVertex //�ڵ�
			{
				set 
				{ 
					partVertex = value; 
				}
				get 
				{
					return "partVertex";
				}
			}
			public string Endpoint //�˵�
			{
				set 
				{ 
					endpoint = value; 
				}
				get 
				{
					return "endpoint";
				}
			}

			public string Intersection //����
			{
				set 
				{ 
					intersection = value; 
				}
				get 
				{
					return "intersection";
				}
			}
		}
		#endregion

		#region//��׽ģʽ����ʶ��׽�����Ƿ�򿪣�
		public struct BoolSnapMode
		{
			bool bPartBoundary ;//����
			bool bPartVertex;//�ڵ�
			bool bEndpoint;//�˵�
			bool bIntersection;//����

			public bool PartBoundary//�� ��
			{
				set
				{ 
					bPartBoundary = value;
				}
				get 
				{
					return bPartBoundary;
				}
			}

			public bool PartVertex //�ڵ�
			{
				set 
				{ 
					bPartVertex = value; 
				}
				get 
				{
					return bPartVertex;
				}
			}
			public bool Endpoint //�˵�
			{
				set 
				{ 
					bEndpoint = value; 
				}
				get 
				{
					return bEndpoint;
				}
			}
			public bool Intersection //����
			{
				set 
				{ 
					bIntersection = value; 
				}
				get 
				{
					return bIntersection;
				}
			}
		}
		#endregion	
	
		//����һ�����ݽṹ�����㽫��Ϣ����������
		public struct FeatureLayerSnap
		{
			public IFeatureLayer pFeatureLayer;
			public bool bSnap;
			
			public IFeatureLayer FeatureLayer
			{
				get 
				{
					return pFeatureLayer;
				}
				set 
				{
					pFeatureLayer = value;
				}
			}
			public bool IsSnap
			{
				get 
				{
					return bSnap;
				}
				set 
				{
					bSnap = value;
				}
			}
		}
	}
}
