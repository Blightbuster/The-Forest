using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	public class GardenArchitect : EntityBehaviour, ICoopTokenConstruction
	{
		
		private void Awake()
		{
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		
		private IEnumerator DelayedAwake(bool isDeserializing)
		{
			this._logPool = new Stack<Transform>();
			this._newPool = new Stack<Transform>();
			this._gardenRoot = new GameObject("GardenRoot").transform;
			if (this._logPrefab.GetComponent<Renderer>())
			{
				this._logLength = this._logPrefab.GetComponent<Renderer>().bounds.size.z;
				this._logWidth = this._logPrefab.GetComponent<Renderer>().bounds.size.x;
			}
			else
			{
				this._logLength = this._logRenderer.bounds.size.z;
				this._logWidth = this._logRenderer.bounds.size.x;
			}
			yield return null;
			if (this._wasBuilt)
			{
				if (!isDeserializing)
				{
					this.OnDeserialized();
					if (!BoltNetwork.isClient && LocalPlayer.Sfx)
					{
						LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, true);
					}
				}
				this.InitGardenGrowSpots(isDeserializing || BoltNetwork.isClient);
			}
			else
			{
				if (this._wasPlaced)
				{
					this.OnDeserialized();
				}
				else
				{
					if (!CoopPeerStarter.DedicatedHost && LocalPlayer.Create.BuildingPlacer)
					{
						LocalPlayer.Create.Grabber.ClosePlace();
					}
					this._logMat = new Material(this._logRenderer.sharedMaterial);
					this._dirtCube.GetComponent<Renderer>().sharedMaterial = this._logMat;
					LocalPlayer.Create.BuildingPlacer.SetRenderer(null);
				}
				if (this._craftStructure)
				{
					Craft_Structure craftStructure = this._craftStructure;
					craftStructure.OnBuilt = (Action<GameObject>)Delegate.Combine(craftStructure.OnBuilt, new Action<GameObject>(this.OnBuilt));
				}
			}
			if (isDeserializing || this._wasBuilt || this._wasPlaced)
			{
				base.enabled = false;
			}
			else
			{
				base.enabled = true;
			}
			yield break;
		}

		
		private void LateUpdate()
		{
			if (this._logMat)
			{
				bool flag = false;
				bool flag2 = false;
				if (this._lockMode == GardenArchitect.LockModes.Position)
				{
					flag = (LocalPlayer.Create.BuildingPlacer.ClearOfCollision && LocalPlayer.Create.BuildingPlacer.OnDynamicClear);
					LocalPlayer.Create.BuildingPlacer.SetNotclear();
					if (flag && TheForest.Utils.Input.GetButtonDown("Fire1"))
					{
						LocalPlayer.Create.BuildingPlacer.SetClear();
						if (LocalPlayer.Create.BuildingPlacer.LastHit != null)
						{
							BoltEntity componentInParent = LocalPlayer.Create.BuildingPlacer.LastHit.Value.transform.GetComponentInParent<BoltEntity>();
							if (componentInParent)
							{
								LocalPlayer.Create.BuildingPlacer.ForcedParent = componentInParent.gameObject;
							}
						}
						this._lockMode = GardenArchitect.LockModes.Size;
						base.transform.parent = null;
						LocalPlayer.Sfx.PlayWhoosh();
					}
				}
				else
				{
					Vector3 vector = base.transform.InverseTransformPoint(LocalPlayer.Create.BuildingPlacer.transform.position);
					this._size.x = Mathf.Clamp(Mathf.Abs(vector.x) + 5f, 1f, this._maxSize) * (float)((vector.x >= 0f) ? 1 : -1);
					this._size.y = Mathf.Clamp(Mathf.Abs(vector.z) + 5f, 1f, this._maxSize) * (float)((vector.z >= 0f) ? 1 : -1);
					if (TheForest.Utils.Input.GetButtonDown("AltFire"))
					{
						this._size.x = 5f;
						this._size.y = 5f;
						base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
						base.transform.localPosition = Vector3.zero;
						base.transform.localRotation = Quaternion.identity;
						this._lockMode = GardenArchitect.LockModes.Position;
						LocalPlayer.Sfx.PlayWhoosh();
					}
					flag2 = (vector.y < 0.25f && vector.y > -0.25f);
				}
				if (LocalPlayer.Create.BuildingPlacer.Clear != flag2)
				{
					LocalPlayer.Create.BuildingPlacer.Clear = flag2;
				}
				this._logMat.SetColor("_TintColor", (!flag && !flag2) ? LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor") : LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
				this.CreateStructure(false);
				bool showManualfillLockIcon = this._lockMode == GardenArchitect.LockModes.Position && LocalPlayer.Create.BuildingPlacer.ClearOfCollision;
				bool flag3 = this._lockMode == GardenArchitect.LockModes.Size;
				bool showManualPlace = this._lockMode == GardenArchitect.LockModes.Size;
				Scene.HudGui.FoundationConstructionIcons.Show(showManualfillLockIcon, false, false, showManualPlace, false, flag3, false);
				if (Scene.HudGui.RotateIcon.activeSelf == flag3)
				{
					Scene.HudGui.RotateIcon.SetActive(!flag3);
				}
			}
		}

		
		private void OnDestroy()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.FoundationConstructionIcons.Shutdown();
			}
			this.Clear();
		}

		
		private void OnDeserialized()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				if (this._wasBuilt)
				{
					this.CreateStructure(false);
					this._gardenRoot.transform.parent = base.transform;
					this._logPool = null;
					this._newPool = null;
					CutGrass component = base.GetComponent<CutGrass>();
					if (component)
					{
						component.Radius = this._size.x;
						component.Length = -this._size.y;
						component.enabled = true;
					}
					this._garden.transform.localPosition = this._dirtCube.localPosition;
					BoxCollider component2 = this._garden.GetComponent<BoxCollider>();
					component2.size = this._dirtCube.localScale;
				}
				else
				{
					this.CreateStructure(false);
					base.StartCoroutine(this.OnPlaced());
				}
			}
		}

		
		private IEnumerator OnPlaced()
		{
			base.enabled = false;
			this._wasPlaced = true;
			yield return null;
			yield return null;
			if (!this._craftStructure)
			{
				this._craftStructure = base.GetComponentInChildren<Craft_Structure>();
			}
			this._craftStructure.GetComponent<Collider>().enabled = false;
			Scene.HudGui.FoundationConstructionIcons.Shutdown();
			if (this._tmpEdgeGo)
			{
				this._tmpEdgeGo.transform.parent = null;
				UnityEngine.Object.Destroy(this._tmpEdgeGo);
			}
			if (this._craftStructure)
			{
				Transform ghostRoot = this._gardenRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				int totalLogs = this._gardenRoot.childCount;
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.$this._logItemId);
				if (ri == null)
				{
					ri = new Craft_Structure.BuildIngredients();
					ri._itemID = this._logItemId;
					ri._amount = 0;
					ri._renderers = new GameObject[0];
					this._craftStructure._requiredIngredients.Insert(0, ri);
				}
				List<GameObject> logs = new List<GameObject>();
				IEnumerator enumerator = this._gardenRoot.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						logs.Add(transform2.GetComponentInChildren<Renderer>().gameObject);
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
				ri._amount += logs.Count;
				ri.AddRuntimeObjects(logs.AsEnumerable<GameObject>(), Prefabs.Instance.LogFloorBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				this._gardenRoot.transform.parent = base.transform;
				ghostRoot.transform.parent = base.transform;
				this._craftStructure.GetComponent<Collider>().enabled = true;
				this._craftStructure.transform.localPosition = this._dirtCube.localPosition;
				BoxCollider bc = this._craftStructure.gameObject.GetComponent<BoxCollider>();
				bc.size = this._dirtCube.localScale;
				yield return null;
				this._craftStructure.manualLoading = true;
				while (LevelSerializer.IsDeserializing && !this._craftStructure.WasLoaded)
				{
					yield return null;
				}
				this._craftStructure.Initialize();
				this._craftStructure.gameObject.SetActive(true);
				yield return null;
				this._logPool = null;
				this._newPool = null;
			}
			yield break;
		}

		
		private void OnBuilt(GameObject built)
		{
			GardenArchitect component = built.GetComponent<GardenArchitect>();
			component._wasBuilt = true;
			component._size = this._size;
			Craft_Structure craftStructure = this._craftStructure;
			craftStructure.OnBuilt = (Action<GameObject>)Delegate.Remove(craftStructure.OnBuilt, new Action<GameObject>(this.OnBuilt));
		}

		
		public void Clear()
		{
			if (this._gardenRoot)
			{
				this._gardenRoot.transform.position = new Vector3(0f, 2000f, 0f);
				UnityEngine.Object.Destroy(this._gardenRoot.gameObject);
				this._gardenRoot = null;
			}
		}

		
		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				this.Clear();
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this._newPool = new Stack<Transform>();
			Transform gardenRoot = this.SpawnGarden();
			this._logPool = this._newPool;
			if (this._gardenRoot)
			{
				UnityEngine.Object.Destroy(this._gardenRoot.gameObject);
			}
			this._gardenRoot = gardenRoot;
			if (this._wasBuilt)
			{
				base.GetComponent<BuildingHealth>()._renderersRoot = this._gardenRoot.gameObject;
				if (isRepair)
				{
					this._gardenRoot.transform.parent = base.transform;
				}
			}
		}

		
		public void Clone(GardenArchitect other)
		{
			this._logPrefab = other._logPrefab;
			this._logRenderer = other._logRenderer;
			this._maxLogScale = other._maxLogScale;
			this._craftStructure = other._craftStructure;
			this._logItemId = other._logItemId;
		}

		
		private void InitGardenGrowSpots(bool searchExisting)
		{
			Renderer component = this._dirtCube.GetComponent<Renderer>();
			if (component)
			{
				MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
				component.GetPropertyBlock(materialPropertyBlock);
				materialPropertyBlock.SetVector("_MainTex_ST", new Vector4(0f, 0f, this._size.x / 5f, this._size.y / 5f));
				component.SetPropertyBlock(materialPropertyBlock);
			}
			int num = Mathf.Abs(Mathf.RoundToInt(this._size.x / 5f * 2f));
			int num2 = Mathf.Abs(Mathf.RoundToInt(this._size.y / 5f * 2f));
			List<Transform> list = new List<Transform>();
			Vector3 localPosition = this._dirtCube.localPosition;
			for (int i = 0; i < num; i++)
			{
				int j = 0;
				while (j < num2)
				{
					if (!searchExisting)
					{
						goto IL_12B;
					}
					Transform transform = base.transform.Find(string.Concat(new object[]
					{
						"GrowSpot_",
						i,
						"_",
						j
					}));
					if (!transform)
					{
						goto IL_12B;
					}
					list.Add(transform);
					IL_22F:
					j++;
					continue;
					IL_12B:
					GameObject gameObject = new GameObject(string.Concat(new object[]
					{
						"GrowSpot_",
						i,
						"_",
						j
					}));
					gameObject.transform.parent = base.transform;
					gameObject.transform.localPosition = new Vector3(((float)i + 0.5f - (float)num / 2f) / (float)num * this._size.x + UnityEngine.Random.Range(-0.5f, 0.5f), 0.25f, ((float)j + 0.5f - (float)num2 / 2f) / (float)num2 * this._size.y + UnityEngine.Random.Range(-0.5f, 0.5f)) + localPosition;
					gameObject.transform.localRotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
					gameObject.AddComponent<StoreInformation>();
					list.Add(gameObject.transform);
					goto IL_22F;
				}
			}
			this._garden.GrowSpots = list.ToArray();
		}

		
		private void StackLogsByScale(ref float currentLogStackRemainingScale, float logStackScaleRatio, float logScale, ref Transform currentLogStackTr, Transform log, Transform parent)
		{
			if (currentLogStackRemainingScale > 0f)
			{
				currentLogStackRemainingScale -= logScale;
			}
			else
			{
				currentLogStackTr = new GameObject("ls").transform;
				currentLogStackTr.position = log.position;
				currentLogStackTr.parent = parent;
				currentLogStackRemainingScale = logStackScaleRatio - logScale;
			}
			log.parent = currentLogStackTr;
		}

		
		private Transform SpawnGarden()
		{
			Transform transform = new GameObject("GardenRoot").transform;
			transform.position = base.transform.position;
			transform.rotation = base.transform.rotation;
			float num = (this._size.x >= 0f) ? 1f : -1f;
			float num2 = (this._size.y >= 0f) ? 1f : -1f;
			float num3 = this._logWidth / this._logLength;
			float num4 = this._logWidth / 2f * 0.95f;
			float num5 = this._size.x / 2f + num4 * num;
			float num6 = this._size.y / 2f + num4 * num2;
			this._dirtCube.localScale = new Vector3(this._size.x * 1.05f, this._dirtCube.localScale.y, this._size.y * 1.05f);
			this._dirtCube.localPosition = new Vector3((this._size.x - num * 5f) / 2f, this._dirtCube.localPosition.y, (this._size.y - num2 * 5f) / 2f);
			Vector3 from = base.transform.TransformPoint(new Vector3(-num5 - num4 * num, 0f, -num6) + this._dirtCube.localPosition);
			Vector3 from2 = base.transform.TransformPoint(new Vector3(num5, 0f, -num6 - num4 * num2) + this._dirtCube.localPosition);
			Vector3 from3 = base.transform.TransformPoint(new Vector3(num5 + num4 * num, 0f, num6) + this._dirtCube.localPosition);
			Vector3 from4 = base.transform.TransformPoint(new Vector3(-num5, 0f, num6 + num4 * num2) + this._dirtCube.localPosition);
			this.SpawnEdge(from, base.transform.right * num, Mathf.Abs(this._size.x) / this._logLength + num3, transform);
			this.SpawnEdge(from2, base.transform.forward * num2, Mathf.Abs(this._size.y) / this._logLength + num3, transform);
			this.SpawnEdge(from3, -base.transform.right * num, Mathf.Abs(this._size.x) / this._logLength + num3, transform);
			this.SpawnEdge(from4, -base.transform.forward * num2, Mathf.Abs(this._size.y) / this._logLength + num3, transform);
			return transform;
		}

		
		private void SpawnEdge(Vector3 from, Vector3 axis, float scale, Transform parent)
		{
			int num = Mathf.Max(Mathf.CeilToInt(scale / this._maxLogScale), 1);
			for (int i = 0; i < num; i++)
			{
				Transform transform = this.NewLog(from, Quaternion.LookRotation(axis));
				transform.localScale = new Vector3(1f, 1f, scale / (float)num);
				this._newPool.Push(transform);
				transform.parent = parent;
				if (i + 1 < num)
				{
					from += axis * (this._logLength * 0.99f * scale / (float)num);
				}
			}
		}

		
		private Transform NewLog(Vector3 position, Quaternion rotation)
		{
			if (this._logPool.Count > 0)
			{
				Transform transform = this._logPool.Pop();
				transform.position = position;
				transform.rotation = rotation;
				return transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefab, position, rotation);
			if (!this._wasBuilt && !this._wasPlaced)
			{
				transform2.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
			}
			return transform2;
		}

		
		
		
		public bool WasBuilt
		{
			get
			{
				return this._wasBuilt;
			}
			set
			{
				this._wasBuilt = value;
			}
		}

		
		
		
		public bool WasPlaced
		{
			get
			{
				return this._wasPlaced;
			}
			set
			{
				this._wasPlaced = value;
			}
		}

		
		public override void Attached()
		{
			this.CustomToken = base.entity.attachToken;
		}

		
		
		
		public IProtocolToken CustomToken
		{
			get
			{
				return new CoopGardenToken
				{
					size = this._size
				};
			}
			set
			{
				CoopGardenToken coopGardenToken = (CoopGardenToken)value;
				this._size = coopGardenToken.size;
				if (!this._wasBuilt)
				{
					this._wasPlaced = true;
				}
			}
		}

		
		[SerializeThis]
		public bool _wasPlaced;

		
		[SerializeThis]
		public bool _wasBuilt;

		
		public Transform _dirtCube;

		
		public Transform _logPrefab;

		
		public Renderer _logRenderer;

		
		public float _maxSize = 25f;

		
		public float _maxLogScale = 2f;

		
		public float _maxScaleLogCost = 1f;

		
		public Craft_Structure _craftStructure;

		
		public Garden _garden;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		
		private bool _initialized;

		
		[SerializeThis]
		private Vector2 _size = new Vector2(5f, 5f);

		
		private Transform _gardenRoot;

		
		private GameObject _tmpEdgeGo;

		
		private float _dirtLength;

		
		private float _logLength;

		
		private float _logWidth;

		
		private Stack<Transform> _logPool;

		
		private Stack<Transform> _newPool;

		
		private Material _logMat;

		
		private GardenArchitect.LockModes _lockMode;

		
		private enum LockModes
		{
			
			Position,
			
			Size
		}
	}
}
