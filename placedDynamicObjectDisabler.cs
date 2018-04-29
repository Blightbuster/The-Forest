using System;
using TheForest.Utils;
using UnityEngine;


public class placedDynamicObjectDisabler : MonoBehaviour
{
	
	private void Start()
	{
		if (this.checkAllPlayers)
		{
			if (!Scene.SceneTracker.placedDynamicObjectsAllPlayers.Contains(this))
			{
				Scene.SceneTracker.placedDynamicObjectsAllPlayers.Add(this);
			}
		}
		else if (!Scene.SceneTracker.placedDynamicObjects.Contains(base.transform))
		{
			Scene.SceneTracker.placedDynamicObjects.Add(base.transform);
		}
	}

	
	private void OnEnable()
	{
		if (this.checkAllPlayers)
		{
			if (!Scene.SceneTracker.placedDynamicObjectsAllPlayers.Contains(this))
			{
				Scene.SceneTracker.placedDynamicObjectsAllPlayers.Add(this);
			}
		}
		else if (!Scene.SceneTracker.placedDynamicObjects.Contains(base.transform))
		{
			Scene.SceneTracker.placedDynamicObjects.Add(base.transform);
		}
	}

	
	public bool checkAllPlayers;

	
	public bool enableOnDedicatedServer;
}
