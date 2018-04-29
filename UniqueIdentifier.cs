using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;


[DontStore]
[ExecuteInEditMode]
[AddComponentMenu("Storage/Unique Identifier")]
public class UniqueIdentifier : MonoBehaviour
{
	
	
	
	public string Id
	{
		get
		{
			if (base.gameObject == null)
			{
				return this._id;
			}
			if (!string.IsNullOrEmpty(this._id))
			{
				return this._id;
			}
			return this._id = SaveGameManager.GetId(base.gameObject);
		}
		set
		{
			this._id = value;
			SaveGameManager.Instance.SetId(base.gameObject, value);
		}
	}

	
	public static GameObject GetByName(string id)
	{
		GameObject byId = SaveGameManager.Instance.GetById(id);
		return byId ?? GameObject.Find(id);
	}

	
	
	
	public static List<UniqueIdentifier> AllIdentifiers
	{
		get
		{
			for (int i = UniqueIdentifier.allIdentifiers.Count - 1; i >= 0; i--)
			{
				if (!UniqueIdentifier.allIdentifiers[i])
				{
					UniqueIdentifier.allIdentifiers.RemoveAt(i);
				}
			}
			return UniqueIdentifier.allIdentifiers;
		}
		set
		{
			UniqueIdentifier.allIdentifiers = value;
		}
	}

	
	
	
	public string ClassId
	{
		get
		{
			return this.classId;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = Guid.NewGuid().ToString();
			}
			this.classId = value;
		}
	}

	
	public void FullConfigure()
	{
		this.ConfigureId();
		foreach (UniqueIdentifier uniqueIdentifier in from c in base.GetComponentsInChildren<UniqueIdentifier>(true)
		where !c.gameObject.activeInHierarchy
		select c)
		{
			uniqueIdentifier.ConfigureId();
		}
	}

	
	protected virtual void Awake()
	{
		foreach (UniqueIdentifier obj in from t in base.GetComponents<UniqueIdentifier>()
		where t.GetType() == typeof(UniqueIdentifier) && t != this
		select t)
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
		SaveGameManager.Initialize(delegate
		{
			if (base.gameObject == null)
			{
				return;
			}
			this.FullConfigure();
		});
	}

	
	private void ConfigureId()
	{
		this._id = SaveGameManager.GetId(base.gameObject);
		UniqueIdentifier.AllIdentifiers.Add(this);
	}

	
	private void OnDestroy()
	{
		if (UniqueIdentifier.AllIdentifiers.Count > 0)
		{
			UniqueIdentifier.AllIdentifiers.Remove(this);
		}
	}

	
	[HideInInspector]
	public bool IsDeserializing;

	
	public string _id = string.Empty;

	
	private static List<UniqueIdentifier> allIdentifiers = new List<UniqueIdentifier>();

	
	[HideInInspector]
	public string classId = Guid.NewGuid().ToString();
}
