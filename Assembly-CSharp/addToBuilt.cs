using System;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class addToBuilt : MonoBehaviour
{
	private void OnDeserialized()
	{
		this.setupLists();
	}

	private void Start()
	{
		this.setupLists();
	}

	private void OnEnable()
	{
		this.setupLists();
	}

	private void setupLists()
	{
		if (!this.setupDone)
		{
			if (this.addToStructures && !Scene.SceneTracker.structuresBuilt.Contains(base.gameObject))
			{
				Scene.SceneTracker.addToStructures(base.gameObject);
			}
			if (this.addToRecentlyBuilt && !Scene.SceneTracker.recentlyBuilt.Contains(base.gameObject))
			{
				Scene.SceneTracker.addToBuilt(base.gameObject);
			}
			if (this.addToRabbitTraps && !Scene.SceneTracker.allRabbitTraps.Contains(base.gameObject))
			{
				Scene.SceneTracker.allRabbitTraps.Add(base.gameObject);
			}
			if (this.addToFires && !Scene.SceneTracker.allPlayerFires.Contains(base.gameObject))
			{
				Scene.SceneTracker.allPlayerFires.Add(base.gameObject);
			}
			this.setupDone = true;
		}
	}

	private void OnDestroy()
	{
		if (Scene.SceneTracker)
		{
			if (this.addToStructures && Scene.SceneTracker.structuresBuilt.Contains(base.gameObject))
			{
				Scene.SceneTracker.structuresBuilt.Remove(base.gameObject);
			}
			if (this.addToRecentlyBuilt && Scene.SceneTracker.recentlyBuilt.Contains(base.gameObject))
			{
				Scene.SceneTracker.recentlyBuilt.Remove(base.gameObject);
			}
			if (this.addToRabbitTraps && Scene.SceneTracker.allRabbitTraps.Contains(base.gameObject))
			{
				Scene.SceneTracker.allRabbitTraps.Remove(base.gameObject);
			}
			if (this.addToFires && Scene.SceneTracker.allPlayerFires.Contains(base.gameObject))
			{
				Scene.SceneTracker.allPlayerFires.Remove(base.gameObject);
			}
		}
	}

	public bool addToStructures;

	public bool addToRecentlyBuilt;

	public bool addToRabbitTraps;

	public bool addToFires;

	private bool setupDone;
}
