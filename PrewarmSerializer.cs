using System;
using System.Threading;
using UnityEngine;


public class PrewarmSerializer : MonoBehaviour
{
	
	private void Start()
	{
		Thread thread = new Thread(new ThreadStart(this.Prewarm));
		thread.Start();
	}

	
	private void Prewarm()
	{
		LevelSerializer.Prewarmed = true;
	}
}
