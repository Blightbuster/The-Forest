﻿using System;
using UniLinq;
using UnityEngine;

[AddComponentMenu("Storage/Internal/Room Loader (Internal use only, do not add this to your scene)")]
public class RoomLoader : MonoBehaviour
{
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnLevelWasLoaded(int level)
	{
		foreach (GameObject gameObject in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)).Cast<GameObject>())
		{
			gameObject.SendMessage("OnRoomWasLoaded", SendMessageOptions.DontRequireReceiver);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
