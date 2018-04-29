using System;
using UnityEngine;


public class WarnNoEnemies : MonoBehaviour
{
	
	private void Update()
	{
		if (SteamManager.Initialized == this.SteamNotInitialized.activeSelf)
		{
		}
	}

	
	public GameObject SteamNotInitialized;

	
	public GameObject VeganTitle;

	
	public GameObject VegetarianTitle;

	
	public GameObject ClassicTitle;

	
	public GameObject PermaDeathTitle;
}
