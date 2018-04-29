using System;
using TheForest.Utils;
using UnityEngine;


public class CameraGUIOff : MonoBehaviour
{
	
	public void GuiOff()
	{
		base.GetComponent<GUILayer>().enabled = false;
		Scene.HudGui.ShowHud(false);
	}

	
	public void GuiOn()
	{
		Scene.HudGui.ShowHud(true);
		base.GetComponent<GUILayer>().enabled = true;
	}
}
