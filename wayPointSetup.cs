using System;
using TheForest.Utils;
using UnityEngine;


public class wayPointSetup : MonoBehaviour
{
	
	private void Start()
	{
		if (!Scene.SceneTracker.caveWayPoints.Contains(base.transform))
		{
			Scene.SceneTracker.caveWayPoints.Add(base.transform);
		}
	}

	
	public Transform nextWaypoint;

	
	public bool stopAtWaypoint;

	
	public float minWaitTime = 5f;

	
	public float maxWaitTime = 12f;
}
