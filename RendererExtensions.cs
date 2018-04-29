using System;
using UnityEngine;


public static class RendererExtensions
{
	
	public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
	{
		if (renderer == null)
		{
			Debug.LogError("Renderer is null!");
			return false;
		}
		if (camera == null)
		{
			Debug.LogError("Camera is null!");
			return false;
		}
		Vector3 vector = camera.WorldToViewportPoint(renderer.transform.position);
		return vector.z > 0f && vector.z < camera.farClipPlane && vector.x > 0f && vector.y < 1f && vector.y > 0f && vector.y < 1f;
	}
}
