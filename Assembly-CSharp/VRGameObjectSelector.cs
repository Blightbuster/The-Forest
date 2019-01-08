using System;
using System.Collections;
using UnityEngine;

public class VRGameObjectSelector : MonoBehaviour
{
	private void Update()
	{
		VRControllerDisplayManager.VRControllerType vrcontrollerType = (!this.SimulateOculus) ? ((!this.SimulateVive) ? VRControllerDisplayManager.GetActiveControllerType() : VRControllerDisplayManager.VRControllerType.Vive) : VRControllerDisplayManager.VRControllerType.OculusTouch;
		if (this.ActiveControllerType == vrcontrollerType)
		{
			return;
		}
		this.ActiveControllerType = vrcontrollerType;
		if (this.ActiveControllerType == VRControllerDisplayManager.VRControllerType.Vive)
		{
			this.ShowSource(this.ViveSource, this.VivePositionOffset, this.ViveRotationOffset);
		}
		else if (this.ActiveControllerType == VRControllerDisplayManager.VRControllerType.OculusTouch)
		{
			this.ShowSource(this.OculusSource, this.OculusPositionOffset, this.OculusRotationOffset);
		}
	}

	private void ShowSource(GameObject sourceGameObject, Vector3 positionOffset, Vector3 rotationOffset)
	{
		if (sourceGameObject == null)
		{
			return;
		}
		if (this.Instance != null)
		{
			UnityEngine.Object.Destroy(this.Instance);
		}
		this.Instance = UnityEngine.Object.Instantiate<GameObject>(sourceGameObject);
		this.Instance.transform.parent = base.transform;
		this.Instance.transform.localPosition = positionOffset;
		this.Instance.transform.localEulerAngles = rotationOffset;
		this.Instance.transform.localScale = Vector3.one;
		if (this.MatchLayer)
		{
			this.SetLayerRec(this.Instance.transform, base.gameObject.layer);
		}
	}

	private void SetLayerRec(Transform tr, int layer)
	{
		tr.gameObject.layer = layer;
		IEnumerator enumerator = tr.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform tr2 = (Transform)obj;
				this.SetLayerRec(tr2, layer);
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
	}

	public GameObject OculusSource;

	public Vector3 OculusPositionOffset;

	public Vector3 OculusRotationOffset;

	public bool SimulateOculus;

	public GameObject ViveSource;

	public Vector3 VivePositionOffset;

	public Vector3 ViveRotationOffset;

	public bool SimulateVive;

	public bool MatchLayer;

	private GameObject Instance;

	private VRControllerDisplayManager.VRControllerType ActiveControllerType;
}
