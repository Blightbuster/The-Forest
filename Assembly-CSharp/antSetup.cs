using System;
using TheForest.Utils;
using UnityEngine;

public class antSetup : MonoBehaviour
{
	private void Start()
	{
		this.updateRate = Time.time + 10f;
	}

	private void Update()
	{
		if (Time.time > this.updateRate)
		{
			if (Scene.SceneTracker && Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) > 10f)
			{
				base.gameObject.SetActive(false);
			}
			this.updateRate = Time.time + 10f;
		}
	}

	private float updateRate = 10f;
}
