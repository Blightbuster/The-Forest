using System;
using System.Collections;
using System.Collections.Generic;
using Serialization;
using UniLinq;
using UnityEngine;
using UnityEngine.AI;


[AddComponentMenu("Storage/Internal/Level Loader (Internal use only, do not add this to your objects!)")]
public class JSONLevelLoader : MonoBehaviour
{
	
	
	
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
			Debug.Log((!this.Data.StoredObjectNames.Any((JSONLevelSerializer.StoredItem sn) => sn.Name == this.$this.Data.rootObject)) ? ("Not found " + this.Data.rootObject) : ("Located " + this.Data.rootObject));
		}
		if (!this.DontDelete)
		{
			foreach (UniqueIdentifier uniqueIdentifier in UniqueIdentifier.AllIdentifiers.Where(delegate(UniqueIdentifier n)
			{
				JSONLevelLoader $this = this.$this;
				return this.$this.Data.StoredObjectNames.All((JSONLevelSerializer.StoredItem sn) => sn.Name != n.Id);
			}).ToList<UniqueIdentifier>())
			{
				try
				{
					bool flag = false;
					JSONLevelLoader.OnDestroyObject(uniqueIdentifier.gameObject, ref flag);
					if (!flag)
					{
						UnityEngine.Object.Destroy(uniqueIdentifier.gameObject);
					}
				}
				catch (Exception ex)
				{
					Radical.LogWarning("Problem destroying object " + uniqueIdentifier.name + " " + ex.ToString());
				}
			}
		}
		List<UniqueIdentifier> flaggedObjects = new List<UniqueIdentifier>();
		LevelSerializer.RaiseProgress("Initializing", 0.25f);
		Vector3 position = new Vector3(0f, 2000f, 2000f);
		foreach (JSONLevelSerializer.StoredItem storedItem in from c in this.Data.StoredObjectNames
		where UniqueIdentifier.GetByName(c.Name) == null
		select c)
		{
			try
			{
				if (storedItem.createEmptyObject || storedItem.ClassId == null || !LevelSerializer.AllPrefabs.ContainsKey(storedItem.ClassId))
				{
					storedItem.GameObject = new GameObject("CreatedObject");
					storedItem.GameObject.transform.position = position;
					EmptyObjectIdentifier emptyObjectIdentifier = storedItem.GameObject.AddComponent<EmptyObjectIdentifier>();
					storedItem.GameObject.AddComponent<StoreMaterials>();
					storedItem.GameObject.AddComponent<StoreMesh>();
					emptyObjectIdentifier.IsDeserializing = true;
					emptyObjectIdentifier.Id = storedItem.Name;
					if (emptyObjectIdentifier.Id == this.Data.rootObject)
					{
						Debug.Log("Set the root object on an empty");
					}
					flaggedObjects.Add(emptyObjectIdentifier);
				}
				else
				{
					GameObject gameObject = LevelSerializer.AllPrefabs[storedItem.ClassId];
					bool flag2 = false;
					JSONLevelLoader.CreateGameObject(gameObject, ref flag2);
					if (flag2)
					{
						Debug.LogWarning("Cancelled");
						continue;
					}
					UniqueIdentifier[] componentsInChildren = gameObject.GetComponentsInChildren<UniqueIdentifier>();
					foreach (UniqueIdentifier uniqueIdentifier2 in componentsInChildren)
					{
						uniqueIdentifier2.IsDeserializing = true;
					}
					storedItem.GameObject = UnityEngine.Object.Instantiate<GameObject>(gameObject, position, Quaternion.identity);
					storedItem.GameObject.GetComponent<UniqueIdentifier>().Id = storedItem.Name;
					if (storedItem.GameObject.GetComponent<UniqueIdentifier>().Id == this.Data.rootObject)
					{
						Debug.Log("Set the root object on a prefab");
					}
					foreach (UniqueIdentifier uniqueIdentifier3 in componentsInChildren)
					{
						uniqueIdentifier3.IsDeserializing = false;
					}
					flaggedObjects.AddRange(storedItem.GameObject.GetComponentsInChildren<UniqueIdentifier>());
				}
				position += Vector3.right * 50f;
				storedItem.GameObject.GetComponent<UniqueIdentifier>().Id = storedItem.Name;
				storedItem.GameObject.name = storedItem.GameObjectName;
				if (storedItem.ChildIds.Count > 0)
				{
					List<UniqueIdentifier> list4 = storedItem.GameObject.GetComponentsInChildren<UniqueIdentifier>().ToList<UniqueIdentifier>();
					int num2 = 0;
					while (num2 < list4.Count && num2 < storedItem.ChildIds.Count)
					{
						list4[num2].Id = storedItem.ChildIds[num2];
						num2++;
					}
				}
				if (storedItem.Children.Count > 0)
				{
					List<StoreInformation> componentsInChildrenWithClause = JSONLevelSerializer.GetComponentsInChildrenWithClause(storedItem.GameObject);
					this._indexDictionary.Clear();
					foreach (StoreInformation storeInformation in componentsInChildrenWithClause)
					{
						if (storedItem.Children.ContainsKey(storeInformation.ClassId))
						{
							if (!this._indexDictionary.ContainsKey(storeInformation.ClassId))
							{
								this._indexDictionary[storeInformation.ClassId] = 0;
							}
							storeInformation.Id = storedItem.Children[storeInformation.ClassId][this._indexDictionary[storeInformation.ClassId]];
							this._indexDictionary[storeInformation.ClassId] = this._indexDictionary[storeInformation.ClassId] + 1;
						}
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogError(ex2);
				Radical.LogWarning(string.Concat(new object[]
				{
					"Problem creating an object ",
					storedItem.GameObjectName,
					" with classID ",
					storedItem.ClassId,
					" ",
					ex2
				}));
			}
		}
		HashSet<GameObject> loadedGameObjects = new HashSet<GameObject>();
		LevelSerializer.RaiseProgress("Initializing", 0.75f);
		foreach (JSONLevelSerializer.StoredItem storedItem2 in this.Data.StoredObjectNames)
		{
			GameObject byName = UniqueIdentifier.GetByName(storedItem2.Name);
			if (byName == null)
			{
				Radical.LogNow("Could not find " + storedItem2.GameObjectName + " " + storedItem2.Name, new object[0]);
			}
			else
			{
				loadedGameObjects.Add(byName);
				if (storedItem2.Components != null && storedItem2.Components.Count > 0)
				{
					List<Component> list2 = (from c in byName.GetComponents<Component>()
					where !typeof(UniqueIdentifier).IsAssignableFrom(c.GetType())
					select c).ToList<Component>();
					foreach (Component component in list2)
					{
						if (!storedItem2.Components.ContainsKey(component.GetType().FullName))
						{
							UnityEngine.Object.Destroy(component);
						}
					}
				}
				JSONLevelLoader.SetActive(byName, storedItem2.Active);
				if (storedItem2.setExtraData)
				{
					byName.layer = storedItem2.layer;
					byName.tag = storedItem2.tag;
				}
			}
		}
		LevelSerializer.RaiseProgress("Initializing", 0.85f);
		if (this.rootObject != null && UniqueIdentifier.GetByName(this.Data.rootObject) == null)
		{
			Debug.Log("No root object has been configured");
		}
		foreach (JSONLevelSerializer.StoredItem storedItem3 in from c in this.Data.StoredObjectNames
		where !string.IsNullOrEmpty(c.ParentName)
		select c)
		{
			GameObject byName2 = UniqueIdentifier.GetByName(storedItem3.ParentName);
			GameObject byName3 = UniqueIdentifier.GetByName(storedItem3.Name);
			if (byName3 != null && byName2 != null)
			{
				byName3.transform.parent = byName2.transform;
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
						foreach (var <>__AnonType in this.Data.StoredItems.GroupBy((JSONLevelSerializer.StoredData i) => i.Name, (string name, IEnumerable<JSONLevelSerializer.StoredData> cps) => new
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
							GameObject byName4 = UniqueIdentifier.GetByName(<>__AnonType.Name);
							if (byName4 == null)
							{
								Radical.LogWarning(<>__AnonType.Name + " was null");
							}
							else
							{
								foreach (var <>__AnonType2 in <>__AnonType.Components)
								{
									try
									{
										LevelSerializer.RaiseProgress("Loading", (float)(++currentProgress) / (float)this.Data.StoredItems.Count);
										Type type = UnitySerializer.GetTypeEx(<>__AnonType2.Type);
										if (type != null)
										{
											this.Last = byName4;
											bool flag3 = false;
											JSONLevelLoader.LoadData(byName4, ref flag3);
											JSONLevelLoader.LoadComponent(byName4, type.Name, ref flag3);
											if (!flag3)
											{
												List<Component> list = (from c in byName4.GetComponents(type)
												where c.GetType() == type
												select c).ToList<Component>();
												while (list.Count > <>__AnonType2.List.Count)
												{
													UnityEngine.Object.DestroyImmediate(list.Last<Component>());
													list.Remove(list.Last<Component>());
												}
												if (type == typeof(NavMeshAgent))
												{
													<>__AnonType2<string, List<JSONLevelSerializer.StoredData>> cp1 = <>__AnonType2;
													<>__AnonType3<string, List<<>__AnonType2<string, List<JSONLevelSerializer.StoredData>>>> item1 = <>__AnonType;
													int l;
													Action action = delegate
													{
														<>__AnonType2<string, List<JSONLevelSerializer.StoredData>> comp = cp1;
														Type tp = type;
														string tname = item1.Name;
														UnitySerializer.AddFinalAction(delegate
														{
															GameObject byName5 = UniqueIdentifier.GetByName(tname);
															List<Component> list3 = (from c in byName5.GetComponents(tp)
															where c.GetType() == tp
															select c).ToList<Component>();
															while (list3.Count < comp.List.Count)
															{
																try
																{
																	list3.Add(byName5.AddComponent(tp));
																}
																catch
																{
																}
															}
															list = (from l in list
															where l != null
															select l).ToList<Component>();
															for (int i = 0; i < list3.Count; i++)
															{
																if (JSONLevelSerializer.CustomSerializers.ContainsKey(tp))
																{
																	JSONLevelSerializer.CustomSerializers[tp].Deserialize(UnitySerializer.TextEncoding.GetBytes(UnitySerializer.UnEscape(comp.List[i].Data)), list3[i]);
																}
																else
																{
																	UnitySerializer.JSONDeserializeInto(UnitySerializer.UnEscape(comp.List[i].Data), list3[i]);
																}
																JSONLevelLoader.LoadedComponent(list3[i]);
															}
														});
													};
													action();
												}
												else
												{
													while (list.Count < <>__AnonType2.List.Count)
													{
														try
														{
															list.Add(byName4.AddComponent(type));
														}
														catch
														{
														}
													}
													int l;
													list = (from l in list
													where l != null
													select l).ToList<Component>();
													for (int k = 0; k < list.Count; k++)
													{
														Radical.Log(string.Format("Deserializing {0} for {1}", type.Name, byName4.GetFullName()), new object[0]);
														if (JSONLevelSerializer.CustomSerializers.ContainsKey(type))
														{
															JSONLevelSerializer.CustomSerializers[type].Deserialize(UnitySerializer.TextEncoding.GetBytes(<>__AnonType2.List[k].Data), list[k]);
														}
														else
														{
															UnitySerializer.JSONDeserializeInto(<>__AnonType2.List[k].Data, list[k]);
														}
														JSONLevelLoader.LoadedComponent(list[k]);
													}
												}
											}
										}
									}
									catch (Exception ex3)
									{
										Radical.LogWarning(string.Concat(new string[]
										{
											"Problem deserializing ",
											<>__AnonType2.Type,
											" for ",
											byName4.name,
											" ",
											ex3.ToString()
										}));
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
				Debug.Log(this.Data.rootObject + " not found " + (this.Data.StoredObjectNames.Any((JSONLevelSerializer.StoredItem n) => n.Name == this.$this.Data.rootObject) ? "was in the stored names" : "not in the stored names"));
			}
			foreach (UniqueIdentifier uniqueIdentifier4 in flaggedObjects)
			{
				uniqueIdentifier4.IsDeserializing = false;
				uniqueIdentifier4.SendMessage("OnDeserialized", SendMessageOptions.DontRequireReceiver);
			}
			LevelSerializer.IsDeserializing = false;
			RoomManager.loadingRoom = false;
			this.whenCompleted(this.rootObject, loadedGameObjects.ToList<GameObject>());
			UnityEngine.Object.Destroy(base.gameObject, 0.1f);
		}
		yield break;
	}

	
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
		JSONLevelLoader.loadingCount = 0;
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

	
	private static int loadingCount;

	
	
	public delegate void CreateObjectDelegate(GameObject prefab, ref bool cancel);

	
	
	public delegate void SerializedComponentDelegate(GameObject gameObject, string componentName, ref bool cancel);

	
	
	public delegate void SerializedObjectDelegate(GameObject gameObject, ref bool cancel);
}
