using System;
using UnityEngine;


internal class OVRMRForegroundCameraManager : MonoBehaviour
{
	
	private void OnPreRender()
	{
		if (this.clipPlaneGameObj)
		{
			if (this.clipPlaneMaterial == null)
			{
				this.clipPlaneMaterial = this.clipPlaneGameObj.GetComponent<MeshRenderer>().material;
			}
			this.clipPlaneGameObj.GetComponent<MeshRenderer>().material.SetFloat("_Visible", 1f);
		}
	}

	
	private void OnPostRender()
	{
		if (this.clipPlaneGameObj)
		{
			this.clipPlaneGameObj.GetComponent<MeshRenderer>().material.SetFloat("_Visible", 0f);
		}
	}

	
	public GameObject clipPlaneGameObj;

	
	private Material clipPlaneMaterial;
}
