using System;
using UnityEngine;


[ExecuteInEditMode]
public class SteamVR_CameraMask : MonoBehaviour
{
	
	private void Awake()
	{
		Debug.Log("SteamVR_CameraMask is deprecated in Unity 5.4 - REMOVING");
		UnityEngine.Object.DestroyImmediate(this);
	}
}
