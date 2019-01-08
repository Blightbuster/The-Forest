using System;
using TheForest.Utils;
using UnityEngine;

public class HackFogCave : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (LocalPlayer.IsInCaves)
		{
			base.gameObject.GetComponent<Camera>().farClipPlane = 10000f;
		}
		else if (PlayerPreferences.Preset == 4)
		{
			base.gameObject.GetComponent<Camera>().farClipPlane = 1000f;
		}
		else
		{
			base.gameObject.GetComponent<Camera>().farClipPlane = 10000f;
		}
	}
}
