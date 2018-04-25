/*----------------------------------------------------------------
			// Copyright (C) 2017 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����CfgSnapEnvironmentSet.cs
			// �ļ�������������׽������������
			//
			// 
			// ������ʶ��YuanHY 20060214
            //����������    
----------------------------------------------------------------*/
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;

namespace DF2DEdit.Class
{
	/// <summary>
	/// CfgSnapEnvironmentSet ��ժҪ˵����
	/// </summary>
	public class CfgSnapEnvironmentSet
	{
		private double dblTolerence;
		private bool   isOpen;
		private SnapStruct.BoolSnapMode snapMode;
		private SnapStruct.EnumSnapType snapType;

		private IActiveView  activeView;
        public IArray featurLayerSnapArray = new ESRI.ArcGIS.esriSystem.ArrayClass();

        public IMap mapSnap = new MapClass();

		public CfgSnapEnvironmentSet()
		{
            
		}

		public double Tolerence
		{
			get
			{
				return dblTolerence;
			}
			set
			{
				dblTolerence = value;
			}
		}

        public bool IsOpen
		{
			get
			{
				return isOpen;
			}
			set
			{
				isOpen = value;
			}
		}

        public SnapStruct.BoolSnapMode SnapMode
		{
			get
			{
				return snapMode;
			}
			set
			{
				snapMode = value;
			}
		}

        public SnapStruct.EnumSnapType SnapType
		{
			get
			{
				return snapType;
			}
			set
			{
				snapType = value;
			}
		}

		public  IActiveView  ActiveView
		{
			get
			{
				return activeView;
			}
			set
			{
				activeView = value;
			}
		}
	}
}
