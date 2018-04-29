using System;
using UnityEngine;


[ExecuteInEditMode]
public class SteamVR_GameView : MonoBehaviour
{
	
	private void Awake()
	{
		Debug.Log("SteamVR_GameView is deprecated in Unity 5.4 - REMOVING");
		UnityEngine.Object.DestroyImmediate(this);
	}
}
