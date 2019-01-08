using System;
using UnityEngine;

public class SheenTimeManager : MonoBehaviour
{
	public void Update()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		float unscaledTime = Time.unscaledTime;
		Shader.SetGlobalFloat("_SheenTime", unscaledTime / 20f);
	}
}
