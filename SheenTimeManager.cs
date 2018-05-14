using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class SheenTimeManager : MonoBehaviour
{
	
	public void Update()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		float num = (!(LocalPlayer.Inventory != null) || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Pause) ? Time.time : Time.unscaledTime;
		Shader.SetGlobalFloat("_SheenTime", num * this._globalSheenTimeScale);
	}

	
	public float _globalSheenTimeScale = 0.05f;
}
