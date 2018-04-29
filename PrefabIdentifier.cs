using System;
using UniLinq;
using UnityEngine;


[DontStore]
[AddComponentMenu("Storage/Prefab Identifier")]
[ExecuteInEditMode]
public class PrefabIdentifier : StoreInformation
{
	
	public bool IsInScene()
	{
		return this.inScenePrefab;
	}

	
	protected override void Awake()
	{
		this.inScenePrefab = true;
		base.Awake();
		foreach (UniqueIdentifier obj in from t in base.GetComponents<UniqueIdentifier>()
		where t.GetType() == typeof(UniqueIdentifier) || (t.GetType() == typeof(PrefabIdentifier) && t != this) || t.GetType() == typeof(StoreInformation)
		select t)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	
	private bool inScenePrefab;
}
