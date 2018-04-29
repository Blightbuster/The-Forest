using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;


[DontStore]
[ExecuteInEditMode]
[AddComponentMenu("Storage/Store Information")]
public class StoreInformation : UniqueIdentifier
{
	
	protected override void Awake()
	{
		base.Awake();
		foreach (UniqueIdentifier obj in from t in base.GetComponents<UniqueIdentifier>()
		where t.GetType() == typeof(UniqueIdentifier) || (t.GetType() == typeof(StoreInformation) && t != this)
		select t)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	
	public bool StoreAllComponents = true;

	
	[HideInInspector]
	public List<string> Components = new List<string>();
}
