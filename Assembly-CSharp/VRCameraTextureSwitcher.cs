using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class VRCameraTextureSwitcher : MonoBehaviour
{
	private void Update()
	{
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory && this.VRCam.targetTexture == null)
		{
			this.VRCam.targetTexture = this.TheatreMode;
		}
		else if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory && this.VRCam.targetTexture == this.TheatreMode)
		{
			this.VRCam.targetTexture = null;
		}
	}

	public Camera VRCam;

	public RenderTexture TheatreMode;
}
