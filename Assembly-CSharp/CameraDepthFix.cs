using System;
using UnityEngine;

public class CameraDepthFix : MonoBehaviour
{
	private void Start()
	{
		this.cam.depthTextureMode |= DepthTextureMode.Depth;
		this.cam.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	public Camera cam;
}
