using System;
using TheForest.Utils;
using UnityEngine;

public class spawnedLogManager : MonoBehaviour
{
	private void OnEnable()
	{
		if (BoltNetwork.isRunning)
		{
			BoltEntity component = base.GetComponent<BoltEntity>();
			if (component && component.isAttached && !component.IsOwner())
			{
				base.enabled = false;
				return;
			}
		}
		if (Scene.SceneTracker && !Scene.SceneTracker.spawnedLogs.Contains(base.transform))
		{
			Scene.SceneTracker.spawnedLogs.Add(base.transform);
		}
		this.age = 0f;
	}

	private void OnDisable()
	{
		if (BoltNetwork.isRunning)
		{
			BoltEntity component = base.GetComponent<BoltEntity>();
			if (component && component.isAttached && !component.IsOwner())
			{
				return;
			}
		}
		if (Scene.SceneTracker && Scene.SceneTracker.spawnedLogs.Contains(base.transform))
		{
			Scene.SceneTracker.spawnedLogs.Remove(base.transform);
		}
	}

	private void Update()
	{
		this.age += Time.deltaTime;
	}

	public float age;

	private float startTime;
}
