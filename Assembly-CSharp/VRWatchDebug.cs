using System;
using TheForest.Utils;
using UnityEngine;

public class VRWatchDebug : MonoBehaviour
{
	private void Start()
	{
		Scene.HudGui.FpsLabel.gameObject.SetActive(false);
	}

	public void ToggleFpsDisplay()
	{
		if (this.fpsOff)
		{
			Scene.HudGui.FpsLabel.gameObject.SetActive(true);
			Scene.HudGui.Stomach.gameObject.SetActive(false);
			Scene.HudGui.Hydration.gameObject.SetActive(false);
			Debug.Log("ENABLED VR FPS DISPLAY");
		}
		else
		{
			Scene.HudGui.FpsLabel.gameObject.SetActive(false);
			Scene.HudGui.Stomach.gameObject.SetActive(true);
			Scene.HudGui.Hydration.gameObject.SetActive(true);
			Debug.Log("DISABLED VR FPS DISPLAY");
		}
		this.fpsOff = !this.fpsOff;
	}

	private bool fpsOff = true;
}
