using System;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;


public class setupNavRemoveRoot : MonoBehaviour
{
	
	private void Start()
	{
		base.Invoke("gatherBounds", 0.5f);
	}

	
	private void gatherBounds()
	{
		List<Collider> list = new List<Collider>();
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			list.Add(componentsInChildren[i]);
		}
		this.combinedBounds = list[0].bounds;
		for (int j = 0; j < list.Count; j++)
		{
			this.combinedBounds.Encapsulate(list[j].bounds);
		}
	}

	
	private void OnDestroy()
	{
		if (Scene.SceneTracker && !MenuMain.exitingToMenu)
		{
			Scene.SceneTracker.StartCoroutine(Scene.SceneTracker.startDummyNavRemove(base.gameObject, base.transform.position, this.combinedBounds));
		}
	}

	
	public void setBounds(Bounds b)
	{
	}

	
	private bool isAboveTerrain = true;

	
	public Bounds combinedBounds;
}
