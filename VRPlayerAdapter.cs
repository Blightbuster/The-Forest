using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;


public class VRPlayerAdapter : MonoBehaviour
{
	
	private void Start()
	{
		this.ConvertToVR();
	}

	
	private void ConvertToVR()
	{
		if (ForestVR.Enabled)
		{
			Debug.LogWarning("ENABLING VR");
			for (int i = 0; i < this.MainCamera.transform.childCount; i++)
			{
				this.MainCamera.transform.GetChild(i).SetParent(this.VREyeCamera);
			}
			AudioListener componentInChildren = this.MainCamera.GetComponentInChildren<AudioListener>(true);
			if (componentInChildren)
			{
				UnityEngine.Object.Destroy(componentInChildren);
			}
			this.MainCamera.SetActive(false);
			this.VRCameraRig.SetActive(true);
			this.Player.GetComponent<FirstPersonHeadBob>().enabled = false;
			this.Player.GetComponent<SimpleMouseRotator>().enabled = false;
			camFollowHead componentInChildren2 = this.PlayerBase.GetComponentInChildren<camFollowHead>(true);
			IEnumerator enumerator = this.PlayerMeshesToDisable.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					SkinnedMeshRenderer component = transform.GetComponent<SkinnedMeshRenderer>();
					if (component)
					{
						component.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			this.VRPlayerHands.gameObject.SetActive(true);
			this.VRPlayerHands.shadowCastingMode = ShadowCastingMode.On;
			this.VRWatchGo.SetActive(true);
			this.defaultPlayerArms.gameObject.SetActive(false);
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("vrBool").Value = true;
		}
	}

	
	public GameObject MainCamera;

	
	public GameObject VRCameraRig;

	
	public GameObject VREyeCamera;

	
	public GameObject Player;

	
	public GameObject PlayerBase;

	
	public GameObject cameraChildren;

	
	public GameObject RightWeaponOffset;

	
	public GameObject VRWatchGo;

	
	public bool BindCamHead;

	
	public bool BindNewHeight;

	
	public bool BindScale;

	
	public GameObject PlayerMeshesToDisable;

	
	public Renderer PlayerHead1;

	
	public Renderer VRPlayerHands;

	
	public Renderer defaultPlayerArms;
}
