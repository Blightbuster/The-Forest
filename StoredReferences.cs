using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;


[Serializable]
public class StoredReferences : ScriptableObject
{
	
	public void Clear()
	{
		this.ById.Clear();
		this.ByObject.Clear();
		this.entries.Clear();
	}

	
	
	public int Count
	{
		get
		{
			this.FixEntries();
			return this.entries.Count;
		}
	}

	
	
	public GameObject[] AllReferences
	{
		get
		{
			this.FixEntries();
			return (from g in this.entries
			select g.gameObject).ToArray<GameObject>();
		}
	}

	
	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			StoredReferences.betweenSceneReferences = (from g in this.entries
			where g.gameObject != null
			select g).ToList<SaveGameManager.StoredEntry>();
		}
	}

	
	private void OnEnable()
	{
		if (Application.isPlaying)
		{
			this.entries = (from g in this.entries.Concat(from g in StoredReferences.betweenSceneReferences
			where g.gameObject != null
			select g)
			where g.gameObject != null
			select g).ToList<SaveGameManager.StoredEntry>();
			this.FixEntries();
			this.ById.Clear();
			this.ByObject.Clear();
			StoredReferences.betweenSceneReferences = (from g in this.entries
			where g.gameObject != null
			select g).ToList<SaveGameManager.StoredEntry>();
		}
	}

	
	public SaveGameManager.StoredEntry this[string id]
	{
		get
		{
			this.EnsureDictionaries();
			if (!this.ById.ContainsKey(id))
			{
				return null;
			}
			SaveGameManager.StoredEntry storedEntry = this.entries[this.ById[id]];
			if (storedEntry.gameObject == null)
			{
				this.ById.Remove(id);
				return null;
			}
			return storedEntry;
		}
		set
		{
			int value2 = 0;
			if (!this.ById.TryGetValue(id, out value2))
			{
				value2 = this.entries.Count;
				this.ById[id] = value2;
				this.entries.Add(value);
			}
			this.entries[this.ById[id]] = value;
			this.ByObject[value.gameObject] = value2;
		}
	}

	
	private void EnsureDictionaries()
	{
		if (this.ById.Count == 0 && this.entries.Count > 0)
		{
			this.FixEntries();
			int value = 0;
			foreach (SaveGameManager.StoredEntry storedEntry in this.entries)
			{
				this.ById[storedEntry.Id] = value;
				this.ByObject[storedEntry.gameObject] = value++;
			}
		}
	}

	
	public SaveGameManager.StoredEntry this[GameObject id]
	{
		get
		{
			this.EnsureDictionaries();
			if (this.ByObject.ContainsKey(id))
			{
				return this.entries[this.ByObject[id]];
			}
			return null;
		}
		set
		{
			int value2 = 0;
			if (!this.ByObject.TryGetValue(id, out value2))
			{
				value2 = this.entries.Count;
				this.ByObject[id] = value2;
				this.entries.Add(value);
			}
			this.entries[this.ByObject[id]] = value;
			this.ById[value.Id] = value2;
		}
	}

	
	public void Remove(GameObject go)
	{
		SaveGameManager.StoredEntry storedEntry = this[go];
		if (storedEntry != null)
		{
			this.ById.Remove(storedEntry.Id);
			this.ByObject.Remove(storedEntry.gameObject);
		}
	}

	
	public StoredReferences Alive()
	{
		StoredReferences storedReferences = ScriptableObject.CreateInstance<StoredReferences>();
		foreach (SaveGameManager.StoredEntry storedEntry in this.entries)
		{
			if (storedEntry.gameObject != null)
			{
				storedReferences[storedEntry.Id] = storedEntry;
			}
		}
		return storedReferences;
	}

	
	private void FixEntries()
	{
		for (int i = this.entries.Count - 1; i >= 0; i--)
		{
			SaveGameManager.StoredEntry storedEntry = this.entries[i];
			if (storedEntry == null || !storedEntry.gameObject || !storedEntry.gameObject.GetComponent<UniqueIdentifier>())
			{
				this.entries.RemoveAt(i);
			}
		}
	}

	
	private static List<SaveGameManager.StoredEntry> betweenSceneReferences = new List<SaveGameManager.StoredEntry>();

	
	public List<SaveGameManager.StoredEntry> entries = new List<SaveGameManager.StoredEntry>();

	
	private Dictionary<string, int> ById = new Dictionary<string, int>();

	
	private Dictionary<GameObject, int> ByObject = new Dictionary<GameObject, int>();
}
