using System;
using System.Collections;
using System.Collections.Generic;
using Serialization;
using UniLinq;
using UnityEngine;


[AddComponentMenu("Storage/Internal/Level Loader (Internal use only, do not add this to your objects!)")]
public class LevelLoader : MonoBehaviour
{
	
	static LevelLoader()
	{
		
		LevelLoader.CreateGameObject = delegate
		{
		};
		LevelLoader.OnDestroyObject = delegate
		{
		};
		LevelLoader.LoadData = delegate
		{
		};
		LevelLoader.LoadComponent = delegate
		{
		};
		LevelLoader.LoadedComponent = delegate
		{
		};
	}

	
	
	
	public static event LevelLoader.CreateObjectDelegate CreateGameObject;

	
	
	
	public static event LevelLoader.SerializedObjectDelegate OnDestroyObject;

	
	
	
	public static event LevelLoader.SerializedObjectDelegate LoadData;

	
	
	
	public static event LevelLoader.SerializedComponentDelegate LoadComponent;

	
	
	
	public static event Action<Component> LoadedComponent;

	
	private void Awake()
	{
		this.timeScaleAfterLoading = Time.timeScale;
		LevelLoader.Current = this;
		if (LevelLoader._pixel == null)
		{
			LevelLoader._pixel = new Texture2D(1, 1);
		}
	}

	
	private void OnDestroy()
	{
		LevelLoader.Current = null;
	}

	
	private void OnGUI()
	{
	}

	
	private void OnLevelWasLoaded(int level)
	{
		if (this.wasLoaded)
		{
			return;
		}
		this.timeScaleAfterLoading = Time.timeScale;
		base.StartCoroutine(this.Load());
	}

	
	private static void SetActive(GameObject go, bool activate)
	{
		go.SetActive(activate);
	}

	
	public IEnumerator Load()
	{
		this.wasLoaded = true;
		yield return base.StartCoroutine(this.Load(1, 0f));
		yield break;
	}

	
	public IEnumerator Load(int numberOfFrames, float timeScale = 0f)
	{
		LevelLoader.loadingCount++;
		float oldFixedTime = Time.fixedDeltaTime;
		Time.fixedDeltaTime = 9f;
		for (;;)
		{
			if (LevelSerializer.LevelLoadingOperation == null || LevelSerializer.LevelLoadingOperation.isDone)
			{
				int num;
				numberOfFrames = (num = numberOfFrames) - 1;
				if (num <= 0)
				{
					break;
				}
			}
			yield return new WaitForEndOfFrame();
		}
		LevelSerializer.LevelLoadingOperation = null;
		if (LevelSerializer.ShouldCollect && timeScale == 0f)
		{
			GC.Collect();
		}
		LevelSerializer.RaiseProgress("Initializing", 0f);
		if (this.Data.rootObject != null)
		{
			Debug.Log((!this.Data.StoredObjectNames.Any((LevelSerializer.StoredItem sn) => sn.Name == this.<>f__this.Data.rootObject)) ? ("Not found " + this.Data.rootObject) : ("Located " + this.Data.rootObject));
		}
		if (!this.DontDelete)
		{
			foreach (UniqueIdentifier go in (from n in UniqueIdentifier.AllIdentifiers
			where this.<>f__this.Data.StoredObjectNames.All((LevelSerializer.StoredItem sn) => sn.Name != i.Id)
			select n).ToList<UniqueIdentifier>())
			{
				try
				{
					bool cancel = false;
					LevelLoader.OnDestroyObject(go.gameObject, ref cancel);
					if (!cancel)
					{
						UnityEngine.Object.Destroy(go.gameObject);
					}
				}
				catch (Exception ex)
				{
					Exception e = ex;
					Radical.LogWarning("Problem destroying object " + go.name + " " + e.ToString());
				}
			}
		}
		List<UniqueIdentifier> flaggedObjects = new List<UniqueIdentifier>();
		flaggedObjects.AddRange(from c in this.Data.StoredObjectNames
		select UniqueIdentifier.GetByName(c.Name) into c
		where c != null
		select c.GetComponent<UniqueIdentifier>());
		LevelSerializer.RaiseProgress("Initializing", 0.25f);
		Vector3 position = new Vector3(0f, 2000f, 2000f);
		foreach (LevelSerializer.StoredItem sto in from c in this.Data.StoredObjectNames
		where UniqueIdentifier.GetByName(c.Name) == null
		select c)
		{
			try
			{
				if (sto.createEmptyObject || sto.ClassId == null || !LevelSerializer.AllPrefabs.ContainsKey(sto.ClassId))
				{
					sto.GameObject = new GameObject("CreatedObject");
					sto.GameObject.transform.position = position;
					EmptyObjectIdentifier emptyObjectMarker = sto.GameObject.AddComponent<EmptyObjectIdentifier>();
					sto.GameObject.AddComponent<StoreMaterials>();
					sto.GameObject.AddComponent<StoreMesh>();
					emptyObjectMarker.IsDeserializing = true;
					emptyObjectMarker.Id = sto.Name;
					if (emptyObjectMarker.Id == this.Data.rootObject)
					{
						Debug.Log("Set the root object on an empty");
					}
					flaggedObjects.Add(emptyObjectMarker);
				}
				else
				{
					GameObject pf = LevelSerializer.AllPrefabs[sto.ClassId];
					bool cancel2 = false;
					LevelLoader.CreateGameObject(pf, ref cancel2);
					if (cancel2)
					{
						Debug.LogWarning("Cancelled");
						continue;
					}
					UniqueIdentifier[] uis = pf.GetComponentsInChildren<UniqueIdentifier>();
					foreach (UniqueIdentifier ui in uis)
					{
						ui.IsDeserializing = true;
					}
					sto.GameObject = (UnityEngine.Object.Instantiate(pf, position, Quaternion.identity) as GameObject);
					sto.GameObject.GetComponent<UniqueIdentifier>().Id = sto.Name;
					if (sto.GameObject.GetComponent<UniqueIdentifier>().Id == this.Data.rootObject)
					{
						Debug.Log("Set the root object on a prefab");
					}
					foreach (UniqueIdentifier ui2 in uis)
					{
						ui2.IsDeserializing = false;
					}
					flaggedObjects.AddRange(sto.GameObject.GetComponentsInChildren<UniqueIdentifier>());
				}
				position += Vector3.right * 50f;
				sto.GameObject.GetComponent<UniqueIdentifier>().Id = sto.Name;
				sto.GameObject.name = sto.GameObjectName;
				if (sto.ChildIds.Count > 0)
				{
					List<UniqueIdentifier> list = sto.GameObject.GetComponentsInChildren<UniqueIdentifier>().ToList<UniqueIdentifier>();
					int m = 0;
					while (m < list.Count && m < sto.ChildIds.Count)
					{
						list[m].Id = sto.ChildIds[m];
						m++;
					}
				}
				if (sto.Children.Count > 0)
				{
					List<StoreInformation> list2 = LevelSerializer.GetComponentsInChildrenWithClause(sto.GameObject);
					this._indexDictionary.Clear();
					foreach (StoreInformation c2 in list2)
					{
						if (sto.Children.ContainsKey(c2.ClassId))
						{
							if (!this._indexDictionary.ContainsKey(c2.ClassId))
							{
								this._indexDictionary[c2.ClassId] = 0;
							}
							c2.Id = sto.Children[c2.ClassId][this._indexDictionary[c2.ClassId]];
							this._indexDictionary[c2.ClassId] = this._indexDictionary[c2.ClassId] + 1;
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Exception e2 = ex2;
				Radical.LogWarning(string.Concat(new object[]
				{
					"Problem creating an object ",
					sto.GameObjectName,
					" with classID ",
					sto.ClassId,
					" ",
					e2
				}));
			}
		}
		HashSet<GameObject> loadedGameObjects = new HashSet<GameObject>();
		LevelSerializer.RaiseProgress("Initializing", 0.75f);
		foreach (LevelSerializer.StoredItem so in this.Data.StoredObjectNames)
		{
			GameObject go2 = UniqueIdentifier.GetByName(so.Name);
			if (go2 == null)
			{
				Radical.LogNow("Could not find " + so.GameObjectName + " " + so.Name, new object[0]);
			}
			else
			{
				loadedGameObjects.Add(go2);
				if (so.Components != null && so.Components.Count > 0)
				{
					List<Component> all = (from c in go2.GetComponents<Component>()
					where !typeof(UniqueIdentifier).IsAssignableFrom(c.GetType())
					select c).ToList<Component>();
					foreach (Component comp in all)
					{
						if (!so.Components.ContainsKey(comp.GetType().FullName))
						{
							UnityEngine.Object.Destroy(comp);
						}
					}
				}
				LevelLoader.SetActive(go2, so.Active);
				if (so.setExtraData)
				{
					go2.layer = so.layer;
					go2.tag = so.tag;
				}
			}
		}
		LevelSerializer.RaiseProgress("Initializing", 0.85f);
		if (this.rootObject != null && UniqueIdentifier.GetByName(this.Data.rootObject) == null)
		{
			Debug.Log("No root object has been configured");
		}
		foreach (LevelSerializer.StoredItem go3 in from c in this.Data.StoredObjectNames
		where !string.IsNullOrEmpty(c.ParentName)
		select c)
		{
			GameObject parent = UniqueIdentifier.GetByName(go3.ParentName);
			GameObject item = UniqueIdentifier.GetByName(go3.Name);
			if (item != null && parent != null)
			{
				item.transform.parent = parent.transform;
			}
		}
		Time.timeScale = timeScale;
		LevelSerializer.RaiseProgress("Initializing", 1f);
		int count = 0;
		int currentProgress = 0;
		UnitySerializer.FinalProcess process;
		try
		{
			if (this.useJSON)
			{
				UnitySerializer.ForceJSONSerialization();
			}
			using (new UnitySerializer.SerializationSplitScope())
			{
				using (new UnitySerializer.SerializationScope())
				{
					var cp;
					Type type;
					foreach (var item2 in this.Data.StoredItems.GroupBy((LevelSerializer.StoredData i) => i.Name, (string name, IEnumerable<LevelSerializer.StoredData> cps) => new
					{
						Name = name,
						Components = (from cp in cps
						where cp.Name == name
						select cp).GroupBy((LevelSerializer.StoredData cp) => cp.Type, (string type, IEnumerable<LevelSerializer.StoredData> components) => new
						{
							Type = type,
							List = components.ToList<LevelSerializer.StoredData>()
						}).ToList()
					}))
					{
						count++;
						GameObject go4 = UniqueIdentifier.GetByName(item2.Name);
						if (go4 == null)
						{
							Radical.LogWarning(item2.Name + " was null");
						}
						else
						{
							using (var enumerator8 = item2.Components.GetEnumerator())
							{
								while (enumerator8.MoveNext())
								{
									cp = enumerator8.Current;
									try
									{
										if (++currentProgress % 100 == 0)
										{
											LevelSerializer.RaiseProgress("Loading", (float)currentProgress / (float)this.Data.StoredItems.Count);
										}
										type = UnitySerializer.GetTypeEx(cp.Type);
										if (type != null)
										{
											this.Last = go4;
											bool cancel3 = false;
											LevelLoader.LoadData(go4, ref cancel3);
											LevelLoader.LoadComponent(go4, type.Name, ref cancel3);
											if (!cancel3)
											{
												List<Component> list3 = (from c in go4.GetComponents(type)
												where c.GetType() == this.<type>__45
												select c).ToList<Component>();
												while (list3.Count > cp.List.Count)
												{
													UnityEngine.Object.DestroyImmediate(list3.Last<Component>());
													list3.Remove(list3.Last<Component>());
												}
												if (type == typeof(NavMeshAgent))
												{
													int l;
													Component comp;
													Action perform = delegate
													{
														<>__AnonType2<string, List<LevelSerializer.StoredData>> comp = this.<cp1>__48;
														Type tp = this.<type>__45;
														string tname = this.<item1>__49.Name;
														UnitySerializer.AddFinalAction(delegate
														{
															GameObject byName = UniqueIdentifier.GetByName(tname);
															List<Component> list4 = (from c in byName.GetComponents(tp)
															where c.GetType() == tp
															select c).ToList<Component>();
															while (list4.Count < comp.List.Count)
															{
																try
																{
																	list4.Add(byName.AddComponent(tp));
																}
																catch
																{
																}
															}
															this.<list>__47 = (from l in this.<list>__47
															where l != null
															select l).ToList<Component>();
															for (int i = 0; i < list4.Count; i++)
															{
																if (LevelSerializer.CustomSerializers.ContainsKey(tp))
																{
																	LevelSerializer.CustomSerializers[tp].Deserialize(comp.List[i].Data, list4[i]);
																}
																else
																{
																	UnitySerializer.DeserializeInto(comp.List[i].Data, list4[i]);
																}
																LevelLoader.LoadedComponent(list4[i]);
															}
														});
													};
													perform();
												}
												else
												{
													while (list3.Count < cp.List.Count)
													{
														try
														{
															list3.Add(go4.AddComponent(type));
														}
														catch
														{
														}
													}
													int l;
													list3 = (from l in list3
													where l != null
													select l).ToList<Component>();
													for (int j = 0; j < list3.Count; j++)
													{
														if (LevelSerializer.CustomSerializers.ContainsKey(type))
														{
															LevelSerializer.CustomSerializers[type].Deserialize(cp.List[j].Data, list3[j]);
														}
														else
														{
															UnitySerializer.DeserializeInto(cp.List[j].Data, list3[j]);
														}
														LevelLoader.LoadedComponent(list3[j]);
													}
												}
											}
										}
									}
									catch
									{
										Debug.LogWarning("Problem deserializing " + cp.Type + " for " + go4.name);
									}
								}
							}
							if (count % 3000 == 0)
							{
								GC.Collect();
							}
						}
					}
					process = UnitySerializer.TakeOwnershipOfFinalization();
				}
			}
		}
		finally
		{
			if (this.useJSON)
			{
				UnitySerializer.UnforceJSONSerialization();
			}
		}
		GC.Collect();
		UnitySerializer.RunDeferredActions(process, 1, false);
		Time.fixedDeltaTime = oldFixedTime;
		Time.timeScale = 1f;
		yield return new WaitForFixedUpdate();
		Time.timeScale = this.timeScaleAfterLoading;
		UnitySerializer.RunDeferredActions(process, 1, true);
		if (LevelSerializer.ShouldCollect && timeScale == 0f)
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
		}
		UnitySerializer.InformDeserializedObjects(process);
		if (this.Data.rootObject != null)
		{
			this.rootObject = UniqueIdentifier.GetByName(this.Data.rootObject);
		}
		else
		{
			this.rootObject = null;
		}
		if (this.rootObject == null && this.Data.rootObject != null)
		{
			Debug.LogError("Could not find the root object");
			Debug.Log(this.Data.rootObject + " not found " + (this.Data.StoredObjectNames.Any((LevelSerializer.StoredItem n) => n.Name == this.<>f__this.Data.rootObject) ? "was in the stored names" : "not in the stored names"));
		}
		foreach (UniqueIdentifier obj in flaggedObjects)
		{
			obj.IsDeserializing = false;
			obj.SendMessage("OnDeserialized", SendMessageOptions.DontRequireReceiver);
		}
		LevelSerializer.IsDeserializing = false;
		this._loading = false;
		RoomManager.loadingRoom = false;
		this.whenCompleted(this.rootObject, loadedGameObjects.ToList<GameObject>());
		LevelSerializer.RaiseProgress("Done", 1f);
		UnityEngine.Object.Destroy(base.gameObject, 0.1f);
		yield break;
	}

	
	public static LevelLoader Current;

	
	private static Texture2D _pixel;

	
	public GameObject rootObject;

	
	private readonly Dictionary<string, int> _indexDictionary = new Dictionary<string, int>();

	
	public LevelSerializer.LevelData Data;

	
	public bool DontDelete;

	
	public GameObject Last;

	
	private float _alpha = 1f;

	
	private bool _loading = true;

	
	public bool showGUI;

	
	public float timeScaleAfterLoading = 1f;

	
	public bool useJSON;

	
	public Action<GameObject, List<GameObject>> whenCompleted = delegate
	{
	};

	
	private bool wasLoaded;

	
	private static int loadingCount = 0;

	
	
	public delegate void CreateObjectDelegate(GameObject prefab, ref bool cancel);

	
	
	public delegate void SerializedComponentDelegate(GameObject gameObject, string componentName, ref bool cancel);

	
	
	public delegate void SerializedObjectDelegate(GameObject gameObject, ref bool cancel);
}
