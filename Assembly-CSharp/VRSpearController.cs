using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class VRSpearController : MonoBehaviour
{
	private void Awake()
	{
		this.initRotY = LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles.y;
	}

	private void OnDisable()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles = new Vector3(LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles.x, this.initRotY, LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles.z);
	}

	private void Update()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World)
		{
			return;
		}
		float num = Vector3.Angle(Vector3.up, LocalPlayer.vrPlayerControl.RightHand.transform.up);
		float b = this.initRotY;
		if (LocalPlayer.vrPlayerControl.RightHand.GetStandardInteractionButton())
		{
			b = this.initRotY + 180f;
			this.ResetAngleTimer = Time.time + 0.25f;
		}
		if (Time.time < this.ResetAngleTimer)
		{
			b = this.initRotY + 180f;
		}
		this.smoothWeaponAngle = Mathf.Lerp(this.smoothWeaponAngle, b, Time.deltaTime * 8f);
		LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles = new Vector3(LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles.x, this.smoothWeaponAngle, LocalPlayer.ScriptSetup.rightHandHeld.localEulerAngles.z);
	}

	public float initRotY;

	private float ResetAngleTimer;

	private float smoothWeaponAngle;
}
