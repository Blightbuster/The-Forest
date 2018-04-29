using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public class gridObjectBlockerManager : MonoBehaviour
{
	
	private void Awake()
	{
		this._callbacks = new HashSet<gridObjectBlocker>();
	}

	
	private void OnDestroy()
	{
		if (gridObjectBlockerManager.instance == this)
		{
			gridObjectBlockerManager.instance = null;
		}
	}

	
	public static void Register(gridObjectBlocker gob)
	{
		if (!gridObjectBlockerManager.Instance._callbacks.Contains(gob))
		{
			gridObjectBlockerManager.Instance._callbacks.Add(gob);
			if (!gridObjectBlockerManager.Instance._running)
			{
				gridObjectBlockerManager.Instance.StartCoroutine(gridObjectBlockerManager.Instance.NavCutRountine());
			}
		}
	}

	
	public static void Unregister(gridObjectBlocker gob)
	{
		if (gridObjectBlockerManager.Instance._callbacks.Contains(gob))
		{
			gridObjectBlockerManager.Instance._callbacks.Remove(gob);
		}
	}

	
	public IEnumerator NavCutRountine()
	{
		if (!this._running)
		{
			this._running = true;
			yield return YieldPresets.WaitPointOneSeconds;
			while (!Scene.SceneTracker || !Scene.SceneTracker.waitForLoadSequence || !Scene.SceneTracker.gameObject.activeSelf)
			{
				yield return null;
			}
			foreach (gridObjectBlocker gridObjectBlocker in this._callbacks)
			{
				if (gridObjectBlocker)
				{
					gridObjectBlocker.doNavCut();
				}
			}
			this._callbacks = new HashSet<gridObjectBlocker>();
			this._running = false;
		}
		yield break;
	}

	
	
	private static gridObjectBlockerManager Instance
	{
		get
		{
			if (!gridObjectBlockerManager.instance)
			{
				gridObjectBlockerManager.instance = new GameObject("gridObjectBlockerManager").AddComponent<gridObjectBlockerManager>();
			}
			return gridObjectBlockerManager.instance;
		}
	}

	
	private bool _running;

	
	private HashSet<gridObjectBlocker> _callbacks;

	
	private static gridObjectBlockerManager instance;
}
