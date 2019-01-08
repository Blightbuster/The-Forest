using System;
using TheForest.Utils;
using UnityEngine;

public class VRLaserPointerMouseScreenPos : MonoBehaviour
{
	private void LateUpdate()
	{
		if (this._cam && this._cam.enabled)
		{
			TheForest.Utils.Input.VRMouseScreenPos = this._cam.ViewportToScreenPoint(LaserPointer.LastRenderTextureHitViewportPos);
		}
	}

	public Camera _cam;
}
