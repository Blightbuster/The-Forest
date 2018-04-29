using System;
using System.Collections;
using System.Collections.Generic;
using Serialization;
using UniLinq;
using UnityEngine;


[AddComponentMenu("Storage/Internal/Level Loader (Internal use only, do not add this to your objects!)")]
public class JSONLevelLoader : MonoBehaviour
{
	
	static JSONLevelLoader()
	{
		
		JSONLevelLoader.CreateGameObject = delegate
		{
		};
		JSONLevelLoader.OnDestroyObject = delegate
		{
		};
		JSONLevelLoader.LoadData = delegate
		{
		};
		JSONLevelLoader.LoadComponent = delegate
		{
		};
		JSONLevelLoader.LoadedComponent = delegate
		{
		};
	}

	
	
	
	public static event JSONLevelLoader.CreateObjectDelegate CreateGameObject;

	
	
	
	public static event JSONLevelLoader.SerializedObjectDelegate OnDestroyObject;

	
	
	
	public static event JSONLevelLoader.SerializedObjectDelegate LoadData;

	
	
	
	public static event JSONLevelLoader.SerializedComponentDelegate LoadComponent;

	
	
	
	public static event Action<Component> LoadedComponent;

	
	private void Awake()
	{
		this.timeScaleAfterLoading = Time.timeScale;
		JSONLevelLoader.Current = this;
		if (JSONLevelLoader.pixel == null)
		{
			JSONLevelLoader.pixel = new Texture2D(1, 1);
		}
	}

	
	private void OnDestroy()
	{
		JSONLevelLoader.Current = null;
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
		this.wasLoaded = true;
		this.timeScaleAfterLoading = Time.timeScale;
		base.StartCoroutine(this.Load());
	}

	
	private static void SetActive(GameObject go, bool activate)
	{
		go.SetActive(activate);
	}

	
	public IEnumerator Load()
	{
		yield return base.StartCoroutine(this.Load(2, 0f));
		yield break;
	}

	
	public IEnumerator Load(int numberOfFrames, float timeScale = 0f)
	{
		JSONLevelLoader.loadingCount++;
		float oldFixedTime = Time.fixedDeltaTime;
		Time.fixedDeltaTime = 9f;
		for (;;)
		{
			int num;
			numberOfFrames = (num = numberOfFrames) - 1;
			if (num <= 0)
			{
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		if (LevelSerializer.ShouldCollect && timeScale == 0f)
		{
			GC.Collect();
		}
		LevelSerializer.RaiseProgress("Initializing", 0f);
		if (this.Data.rootObject != null)
		{
			Debug.Log((!this.Data.StoredObjectNames.Any((JSONLevelSerializer.StoredItem sn) => sn.Name == this.<>f__this.Data.rootObject)) ? ("Not found " + this.Data.rootObject) : ("Located " + this.Data.rootObject));
		}
		if (!this.DontDelete)
		{
			foreach (UniqueIdentifier go in (from n in UniqueIdentifier.AllIdentifiers
			where this.<>f__this.Data.StoredObjectNames.All((JSONLevelSerializer.StoredItem sn) => sn.Name != i.Id)
			select n).ToList<UniqueIdentifier>())
			{
				try
				{
					bool cancel = false;
					JSONLevelLoader.OnDestroyObject(go.gameObject, ref cancel);
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
		LevelSerializer.RaiseProgress("Initializing", 0.25f);
		Vector3 position = new Vector3(0f, 2000f, 2000f);
		foreach (JSONLevelSerializer.StoredItem sto in from c in this.Data.StoredObjectNames
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
					JSONLevelLoader.CreateGameObject(pf, ref cancel2);
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
					List<StoreInformation> list2 = JSONLevelSerializer.GetComponentsInChildrenWithClause(sto.GameObject);
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
				Debug.LogError(e2);
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
		foreach (JSONLevelSerializer.StoredItem so in this.Data.StoredObjectNames)
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
				JSONLevelLoader.SetActive(go2, so.Active);
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
		foreach (JSONLevelSerializer.StoredItem go3 in from c in this.Data.StoredObjectNames
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
		using (new Radical.Logging())
		{
			int currentProgress = 0;
			UnitySerializer.FinalProcess process;
			using (new UnitySerializer.ForceJSON())
			{
				using (new UnitySerializer.SerializationSplitScope())
				{
					using (new UnitySerializer.SerializationScope())
					{
						var cp;
						Type type;
						foreach (var item2 in this.Data.StoredItems.GroupBy((JSONLevelSerializer.StoredData i) => i.Name, (string name, IEnumerable<JSONLevelSerializer.StoredData> cps) => new
						{
							Name = name,
							Components = (from cp in cps
							where cp.Name == name
							select cp).GroupBy((JSONLevelSerializer.StoredData cp) => cp.Type, (string type, IEnumerable<JSONLevelSerializer.StoredData> components) => new
							{
								Type = type,
								List = components.ToList<JSONLevelSerializer.StoredData>()
							}).ToList()
						}))
						{
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
											LevelSerializer.RaiseProgress("Loading", (float)(++currentProgress) / (float)this.Data.StoredItems.Count);
											type = UnitySerializer.GetTypeEx(cp.Type);
											if (type != null)
											{
												this.Last = go4;
												bool cancel3 = false;
												JSONLevelLoader.LoadData(go4, ref cancel3);
												JSONLevelLoader.LoadComponent(go4, type.Name, ref cancel3);
												if (!cancel3)
												{
													List<Component> list3 = (from c in go4.GetComponents(type)
													where c.GetType() == this.<type>__46
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
															<>__AnonType2<string, List<JSONLevelSerializer.StoredData>> comp = this.<cp1>__49;
															Type tp = this.<type>__46;
															string tname = this.<item1>__50.Name;
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
																this.<list>__48 = (from l in this.<list>__48
																where l != null
																select l).ToList<Component>();
																for (int i = 0; i < list4.Count; i++)
																{
																	if (JSONLevelSerializer.CustomSerializers.ContainsKey(tp))
																	{
																		JSONLevelSerializer.CustomSerializers[tp].Deserialize(UnitySerializer.TextEncoding.GetBytes(UnitySerializer.UnEscape(comp.List[i].Data)), list4[i]);
																	}
																	else
																	{
																		UnitySerializer.JSONDeserializeInto(UnitySerializer.UnEscape(comp.List[i].Data), list4[i]);
																	}
																	JSONLevelLoader.LoadedComponent(list4[i]);
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
															Radical.Log(string.Format("Deserializing {0} for {1}", type.Name, go4.GetFullName()), new object[0]);
															if (JSONLevelSerializer.CustomSerializers.ContainsKey(type))
															{
																JSONLevelSerializer.CustomSerializers[type].Deserialize(UnitySerializer.TextEncoding.GetBytes(cp.List[j].Data), list3[j]);
															}
															else
															{
																UnitySerializer.JSONDeserializeInto(cp.List[j].Data, list3[j]);
															}
															JSONLevelLoader.LoadedComponent(list3[j]);
														}
													}
												}
											}
										}
										catch (Exception ex3)
										{
											Exception e3 = ex3;
											Radical.LogWarning(string.Concat(new string[]
											{
												"Problem deserializing ",
												cp.Type,
												" for ",
												go4.name,
												" ",
												e3.ToString()
											}));
										}
									}
								}
							}
						}
						process = UnitySerializer.TakeOwnershipOfFinalization();
					}
				}
			}
			UnitySerializer.RunDeferredActions(process, 2, false);
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
				Debug.Log(this.Data.rootObject + " not found " + (this.Data.StoredObjectNames.Any((JSONLevelSerializer.StoredItem n) => n.Name == this.<>f__this.Data.rootObject) ? "was in the stored names" : "not in the stored names"));
			}
			foreach (UniqueIdentifier obj in flaggedObjects)
			{
				obj.IsDeserializing = false;
				obj.SendMessage("OnDeserialized", SendMessageOptions.DontRequireReceiver);
			}
			LevelSerializer.IsDeserializing = false;
			RoomManager.loadingRoom = false;
			this.whenCompleted(this.rootObject, loadedGameObjects.ToList<GameObject>());
			UnityEngine.Object.Destroy(base.gameObject, 0.1f);
		}
		yield break;
	}

	
	public static JSONLevelLoader Current;

	
	private static Texture2D pixel;

	
	public JSONLevelSerializer.LevelData Data;

	
	public bool DontDelete;

	
	public GameObject Last;

	
	public bool showGUI;

	
	public float timeScaleAfterLoading = 1f;

	
	public Action<GameObject, List<GameObject>> whenCompleted = delegate
	{
	};

	
	public GameObject rootObject;

	
	private readonly Dictionary<string, int> _indexDictionary = new Dictionary<string, int>();

	
	private bool wasLoaded;

	
	private static int loadingCount = 0;

	
	
	public delegate void CreateObjectDelegate(GameObject prefab, ref bool cancel);

	
	
	public delegate void SerializedComponentDelegate(GameObject gameObject, string componentName, ref bool cancel);

	
	
	public delegate void SerializedObjectDelegate(GameObject gameObject, ref bool cancel);
}
