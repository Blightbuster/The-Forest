using System;
using TheForest.Utils;
using UnityEngine;

public class VRIconSetup : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.gameObject.SetActive(false);
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		float num = Vector3.Distance(LocalPlayer.MainCamTr.position, base.transform.position);
		num = Mathf.Clamp(num, this._nearRange, this._farRange);
		float num2 = Mathf.Lerp(this._nearRangeTextSize, this._farRangeTextSize, (num - this._nearRange) / (this._farRange - this._nearRange));
		float num3 = num2 / this._vrScaleFactor;
		this._vrIcon.localScale = new Vector3(num3, num3, num3);
		this._vrIcon.LookAt(LocalPlayer.MainCamTr.position, LocalPlayer.Transform.rotation * Vector3.up);
	}

	public Transform _vrIcon;

	public float _nearRange = 4f;

	public float _farRange = 9f;

	public float _nearRangeTextSize = 24f;

	public float _farRangeTextSize = 10f;

	public float _vrScaleFactor = 1f;
}
