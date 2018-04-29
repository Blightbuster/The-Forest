using System;
using System.Collections.Generic;
using UnityEngine;


internal class OVRCompositionUtil
{
	
	public static void SafeDestroy(GameObject obj)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(obj);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	
	public static void SafeDestroy(ref GameObject obj)
	{
		OVRCompositionUtil.SafeDestroy(obj);
		obj = null;
	}

	
	public static OVRPlugin.CameraDevice ConvertCameraDevice(OVRManager.CameraDevice cameraDevice)
	{
		if (cameraDevice == OVRManager.CameraDevice.WebCamera0)
		{
			return OVRPlugin.CameraDevice.WebCamera0;
		}
		if (cameraDevice == OVRManager.CameraDevice.WebCamera1)
		{
			return OVRPlugin.CameraDevice.WebCamera1;
		}
		if (cameraDevice == OVRManager.CameraDevice.ZEDCamera)
		{
			return OVRPlugin.CameraDevice.ZEDCamera;
		}
		return OVRPlugin.CameraDevice.None;
	}

	
	public static OVRBoundary.BoundaryType ToBoundaryType(OVRManager.VirtualGreenScreenType type)
	{
		if (type == OVRManager.VirtualGreenScreenType.OuterBoundary)
		{
			return OVRBoundary.BoundaryType.OuterBoundary;
		}
		if (type == OVRManager.VirtualGreenScreenType.PlayArea)
		{
			return OVRBoundary.BoundaryType.PlayArea;
		}
		Debug.LogWarning("Unmatched VirtualGreenScreenType");
		return OVRBoundary.BoundaryType.OuterBoundary;
	}

	
	public static Vector3 GetWorldPosition(Vector3 trackingSpacePosition)
	{
		OVRPose trackingSpacePose;
		trackingSpacePose.position = trackingSpacePosition;
		trackingSpacePose.orientation = Quaternion.identity;
		return OVRExtensions.ToWorldSpacePose(trackingSpacePose).position;
	}

	
	public static float GetMaximumBoundaryDistance(Camera camera, OVRBoundary.BoundaryType boundaryType)
	{
		if (!OVRManager.boundary.GetConfigured())
		{
			return float.MaxValue;
		}
		Vector3[] geometry = OVRManager.boundary.GetGeometry(boundaryType);
		if (geometry.Length == 0)
		{
			return float.MaxValue;
		}
		float num = float.MinValue;
		foreach (Vector3 trackingSpacePosition in geometry)
		{
			Vector3 worldPosition = OVRCompositionUtil.GetWorldPosition(trackingSpacePosition);
			float num2 = Vector3.Dot(camera.transform.forward, worldPosition);
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	
	public static Mesh BuildBoundaryMesh(OVRBoundary.BoundaryType boundaryType, float topY, float bottomY)
	{
		if (!OVRManager.boundary.GetConfigured())
		{
			return null;
		}
		List<Vector3> list = new List<Vector3>(OVRManager.boundary.GetGeometry(boundaryType));
		if (list.Count == 0)
		{
			return null;
		}
		list.Add(list[0]);
		int count = list.Count;
		Vector3[] array = new Vector3[count * 2];
		Vector2[] array2 = new Vector2[count * 2];
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = list[i];
			array[i] = new Vector3(vector.x, bottomY, vector.z);
			array[i + count] = new Vector3(vector.x, topY, vector.z);
			array2[i] = new Vector2((float)i / (float)(count - 1), 0f);
			array2[i + count] = new Vector2(array2[i].x, 1f);
		}
		int[] array3 = new int[(count - 1) * 2 * 3];
		for (int j = 0; j < count - 1; j++)
		{
			array3[j * 6] = j;
			array3[j * 6 + 1] = j + count;
			array3[j * 6 + 2] = j + 1 + count;
			array3[j * 6 + 3] = j;
			array3[j * 6 + 4] = j + 1 + count;
			array3[j * 6 + 5] = j + 1;
		}
		return new Mesh
		{
			vertices = array,
			uv = array2,
			triangles = array3
		};
	}
}
