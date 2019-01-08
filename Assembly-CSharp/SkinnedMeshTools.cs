﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkinnedMeshTools
{
	public static List<GameObject> AddSkinnedMeshTo(GameObject obj, Transform root)
	{
		return SkinnedMeshTools.AddSkinnedMeshTo(obj, root, true);
	}

	public static List<GameObject> AddSkinnedMeshTo(GameObject obj, Transform root, bool hideFromObj)
	{
		List<GameObject> list = new List<GameObject>();
		SkinnedMeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer thisRenderer in componentsInChildren)
		{
			list.Add(SkinnedMeshTools.ProcessBonedObject(thisRenderer, root));
		}
		if (hideFromObj)
		{
			obj.SetActiveRecursively(false);
		}
		return list;
	}

	private static GameObject ProcessBonedObject(SkinnedMeshRenderer ThisRenderer, Transform root)
	{
		GameObject gameObject = new GameObject(ThisRenderer.gameObject.name);
		gameObject.transform.parent = root;
		SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		Transform[] array = new Transform[ThisRenderer.bones.Length];
		for (int i = 0; i < ThisRenderer.bones.Length; i++)
		{
			array[i] = SkinnedMeshTools.FindChildByName(ThisRenderer.bones[i].name, root);
		}
		skinnedMeshRenderer.bones = array;
		skinnedMeshRenderer.sharedMesh = ThisRenderer.sharedMesh;
		skinnedMeshRenderer.sharedMaterials = ThisRenderer.sharedMaterials;
		return gameObject;
	}

	private static Transform FindChildByName(string ThisName, Transform ThisGObj)
	{
		if (ThisGObj.name == ThisName)
		{
			return ThisGObj.transform;
		}
		IEnumerator enumerator = ThisGObj.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform thisGObj = (Transform)obj;
				Transform transform = SkinnedMeshTools.FindChildByName(ThisName, thisGObj);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		return null;
	}
}
