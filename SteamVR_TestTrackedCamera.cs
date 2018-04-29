using System;
using UnityEngine;
using Valve.VR;


public class SteamVR_TestTrackedCamera : MonoBehaviour
{
	
	private void OnEnable()
	{
		SteamVR_TrackedCamera.VideoStreamTexture videoStreamTexture = SteamVR_TrackedCamera.Source(this.undistorted, 0);
		videoStreamTexture.Acquire();
		if (!videoStreamTexture.hasCamera)
		{
			base.enabled = false;
		}
	}

	
	private void OnDisable()
	{
		this.material.mainTexture = null;
		SteamVR_TrackedCamera.VideoStreamTexture videoStreamTexture = SteamVR_TrackedCamera.Source(this.undistorted, 0);
		videoStreamTexture.Release();
	}

	
	private void Update()
	{
		SteamVR_TrackedCamera.VideoStreamTexture videoStreamTexture = SteamVR_TrackedCamera.Source(this.undistorted, 0);
		Texture2D texture = videoStreamTexture.texture;
		if (texture == null)
		{
			return;
		}
		this.material.mainTexture = texture;
		float num = (float)texture.width / (float)texture.height;
		if (this.cropped)
		{
			VRTextureBounds_t frameBounds = videoStreamTexture.frameBounds;
			this.material.mainTextureOffset = new Vector2(frameBounds.uMin, frameBounds.vMin);
			float num2 = frameBounds.uMax - frameBounds.uMin;
			float num3 = frameBounds.vMax - frameBounds.vMin;
			this.material.mainTextureScale = new Vector2(num2, num3);
			num *= Mathf.Abs(num2 / num3);
		}
		else
		{
			this.material.mainTextureOffset = Vector2.zero;
			this.material.mainTextureScale = new Vector2(1f, -1f);
		}
		this.target.localScale = new Vector3(1f, 1f / num, 1f);
		if (videoStreamTexture.hasTracking)
		{
			SteamVR_Utils.RigidTransform transform = videoStreamTexture.transform;
			this.target.localPosition = transform.pos;
			this.target.localRotation = transform.rot;
		}
	}

	
	public Material material;

	
	public Transform target;

	
	public bool undistorted = true;

	
	public bool cropped = true;
}
