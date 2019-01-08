using System;
using TheForest.Utils;
using UnityEngine;

public class VRFlareController : MonoBehaviour
{
	private void OnEnable()
	{
		if (ForestVR.Enabled)
		{
			this.CapVR.SetActive(true);
			this.CapRegular.SetActive(false);
			this.setUnlit();
			this.prevHoverRadius = LocalPlayer.vrPlayerControl.LeftHand.hoverSphereRadius;
			LocalPlayer.vrPlayerControl.LeftHand.hoverSphereRadius = 0.1f;
		}
	}

	private void OnDisable()
	{
		if (ForestVR.Enabled)
		{
			this.CapVR.SetActive(true);
			this.CapRegular.SetActive(false);
			this.setUnlit();
			LocalPlayer.vrPlayerControl.LeftHand.hoverSphereRadius = this.prevHoverRadius;
		}
	}

	public void setLit()
	{
		if (this._flareLight)
		{
			this._flareLight.SetActive(true);
		}
		this._flareBody.sharedMaterial = this._litMat;
		this.CapVR.SetActive(false);
		LocalPlayer.Inventory.UseAltWorldPrefab = false;
	}

	public void setUnlit()
	{
		if (this._flareLight)
		{
			this._flareLight.SetActive(false);
		}
		this._flareBody.sharedMaterial = this._unlitMat;
		this.CapVR.SetActive(true);
		this.CapVR.transform.parent = base.transform;
		this.CapVR.transform.localPosition = this.restTransform.localPosition;
		this.CapVR.transform.localRotation = this.restTransform.localRotation;
	}

	public GameObject CapVR;

	public GameObject CapRegular;

	public Renderer _flareBody;

	public Transform restTransform;

	public GameObject _flareLight;

	public Material _litMat;

	public Material _unlitMat;

	private float prevHoverRadius;
}
