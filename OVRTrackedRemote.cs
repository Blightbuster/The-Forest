using System;
using UnityEngine;


public class OVRTrackedRemote : MonoBehaviour
{
	
	private void Start()
	{
		this.m_isOculusGo = (OVRPlugin.productName == "Oculus Go");
	}

	
	private void Update()
	{
		bool flag = OVRInput.IsControllerConnected(this.m_controller);
		if (flag != this.m_prevControllerConnected || !this.m_prevControllerConnectedCached)
		{
			this.m_modelOculusGoController.SetActive(flag && this.m_isOculusGo);
			this.m_modelGearVrController.SetActive(flag && !this.m_isOculusGo);
			this.m_prevControllerConnected = flag;
			this.m_prevControllerConnectedCached = true;
		}
		if (!flag)
		{
			return;
		}
	}

	
	public GameObject m_modelGearVrController;

	
	public GameObject m_modelOculusGoController;

	
	public OVRInput.Controller m_controller;

	
	private bool m_isOculusGo;

	
	private bool m_prevControllerConnected;

	
	private bool m_prevControllerConnectedCached;
}
