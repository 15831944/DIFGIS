using System;

using  WSGRI.DigitalFactory.Commands;
using WSGRI.DigitalFactory.Base;
using WSGRI.DigitalFactory.DFSystem.DFConfig; 

/*----------------------------------------------------------------
			// Copyright (C) 2005 ��ұ�����人�����о�Ժ���޹�˾
			// ��Ȩ���С� 
			//
			// �ļ�����SnapEndpoint.cs
			// �ļ������������˵㲶׽
			//
			// 
			// ������ʶ��YuanHY XXXXXXXX
            // �������裺1��
			//           
			// ����˵������
			// �޸ı�ʶ������    
----------------------------------------------------------------*/
namespace WSGRI.DigitalFactory.DFEditorTool
{
	/// <summary>
	/// SnapEndpoint ��ժҪ˵����
	/// </summary>
	public class SnapEndpoint:AbstractMapCommand
	{
		private IDFApplication m_App;
		private SnapStruct.EnumSnapType  m_strSnapTpye;

		public SnapEndpoint()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

		public override string Caption
		{
			get
			{
				return "SnapEndpoint";
			}
			set
			{
				
			}
		}
            
		public override void Execute()
		{
			if (!(this.Hook is IDFApplication))
			{
				return;
			}
			else
			{
				m_App = (IDFApplication)this.Hook;
			}

			Console.WriteLine(m_App.CurrentConfig.cfgSnapEnvironmentSet.CurrentSnapType);

			if(m_App.CurrentConfig.cfgSnapEnvironmentSet.CurrentSnapType != m_strSnapTpye.Endpoint)
			{
				m_App.CurrentConfig.cfgSnapEnvironmentSet.CurrentSnapType = m_strSnapTpye.Endpoint;
				m_App.CurrentConfig.cfgSnapEnvironmentSet.IsUseMixSnap =  false;
			}
			else
			{
				m_App.CurrentConfig.cfgSnapEnvironmentSet.CurrentSnapType = null;
				
			}
	
			
		}
          
		public override void UnExecute()
		{
			// TODO:  ��� SnapEndpoint.UnExecute ʵ��

		}

	}
}
