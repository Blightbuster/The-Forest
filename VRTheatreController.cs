using System;
using TheForest.Utils;
using UnityEngine;


public class VRTheatreController : MonoBehaviour
{
	
	private void Start()
	{
		if (this.SourceCamera == null)
		{
			this.SourceCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		}
		if (this.VignetteController == null)
		{
			this.VignetteController = LocalPlayer.GameObject.GetComponent<VRVignetteController>();
		}
		this.SwitchToGameMode();
	}

	
	private void Update()
	{
		if (ForestVR.Enabled)
		{
			if (LocalPlayer.AnimControl.useRootMotion || LocalPlayer.CamFollowHead.flying || LocalPlayer.CamFollowHead.followAnim)
			{
				this.SwitchToTheatreMode();
				this.comfortMode = true;
			}
			else if (this.comfortMode)
			{
				this.SwitchToGameMode();
				this.comfortMode = false;
			}
			else if (TheForest.Utils.Input.GetButtonDown("Inventory"))
			{
				GameObject inventoryGO = LocalPlayer.Inventory._inventoryGO;
				inventoryGO.GetComponent<InventoryFakeGround>().enabled = false;
				inventoryGO.GetComponentInChildren<Camera>(true).gameObject.SetActive(false);
				inventoryGO.transform.SetParent(this.InventoryBone);
				inventoryGO.transform.localPosition = Vector3.zero;
				inventoryGO.transform.localRotation = Quaternion.identity;
				if (this.theatreOn)
				{
					this.SwitchToGameMode();
				}
				else
				{
					this.SwitchToTheatreMode();
				}
			}
		}
	}

	
	public void SwitchToTheatreMode()
	{
		if (!this.theatreOn)
		{
			this.SourceCamera.forceIntoRenderTexture = true;
			this.SourceCamera.targetTexture = this.TheatreRT;
			this.TheatreObject.SetActive(true);
			if (this.VignetteController == null)
			{
				this.VignetteController = LocalPlayer.GameObject.GetComponent<VRVignetteController>();
				if (this.VignetteController != null)
				{
					this.VignetteController.SetVignetteIntensity(0f);
					this.VignetteController.enabled = false;
				}
			}
			this.theatreOn = true;
		}
	}

	
	public void SwitchToGameMode()
	{
		if (this.theatreOn)
		{
			this.SourceCamera.forceIntoRenderTexture = false;
			this.SourceCamera.targetTexture = null;
			this.TheatreObject.SetActive(false);
			this.VignetteController.enabled = true;
			this.theatreOn = false;
		}
	}

	
	public Camera SourceCamera;

	
	public GameObject TheatreObject;

	
	public RenderTexture TheatreRT;

	
	public Transform InventoryBone;

	
	public VRVignetteController VignetteController;

	
	private bool theatreOn;

	
	private bool comfortMode;
}
