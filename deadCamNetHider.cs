using System;
using TheForest.Utils;
using UnityEngine;


public class deadCamNetHider : MonoBehaviour
{
	
	private void Update()
	{
		if (!LocalPlayer.Transform)
		{
			return;
		}
		Vector3 position = LocalPlayer.Transform.position + LocalPlayer.Transform.up * 100f;
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			GameObject gameObject = Scene.SceneTracker.allPlayers[i];
			if (Scene.SceneTracker.allPlayers[i] != null && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
			{
				Scene.SceneTracker.allPlayers[i].transform.position = position;
			}
		}
	}
}
