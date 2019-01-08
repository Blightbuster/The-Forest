using System;
using UnityEngine;

[ExecuteInEditMode]
public class SteamVR_CameraFlip : MonoBehaviour
{
	private void Awake()
	{
		Debug.Log("SteamVR_CameraFlip is deprecated in Unity 5.4 - REMOVING");
		UnityEngine.Object.DestroyImmediate(this);
	}
}
