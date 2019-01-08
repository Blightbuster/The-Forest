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
	public class CraneArchitect : EntityBehaviour, ICoopTokenConstruction
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
			this._craneRoot = new GameObject("CraneRoot").transform;
			this._logMat = ((!this._wasPlaced && !this._wasBuilt) ? new Material(this._logRenderer.sharedMaterial) : this._logRenderer.sharedMaterial);
			if (this._logPrefab.GetComponent<Renderer>())
			{
				this._logLength = this._logPrefab.GetComponent<Renderer>().bounds.size.z;
			}
			else
			{
				this._logLength = this._logRenderer.bounds.size.z;
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
			}
			else
			{
				if (this._wasPlaced)
				{
					UnityEngine.Object.Destroy(this._holeCutter.gameObject);
					this.OnDeserialized();
				}
				else
				{
					if (!CoopPeerStarter.DedicatedHost && LocalPlayer.Create.BuildingPlacer)
					{
						LocalPlayer.Create.Grabber.ClosePlace();
					}
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
				this._logMat.SetColor("_TintColor", (!LocalPlayer.Create.BuildingPlacer.ClearOfCollision) ? LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor") : LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
				if (this._lockMode == CraneArchitect.LockModes.Bottom)
				{
					bool flag = LocalPlayer.Create.BuildingPlacer.ClearOfCollision && LocalPlayer.Create.BuildingPlacer.OnDynamicClear;
					LocalPlayer.Create.BuildingPlacer.IgnoreLookAtCollision = false;
					LocalPlayer.Create.BuildingPlacer.Airborne = false;
					if (flag)
					{
						LocalPlayer.Create.BuildingPlacer.SetClear();
					}
					else
					{
						LocalPlayer.Create.BuildingPlacer.SetNotclear();
					}
					LocalPlayer.Create.BuildingPlacer.Clear = false;
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
						this._bottomY = LocalPlayer.Create.BuildingPlacer.MinHeight;
						this._lockMode = CraneArchitect.LockModes.Top;
						base.transform.parent = null;
						LocalPlayer.Sfx.PlayWhoosh();
					}
					if (this._holeCutter.gameObject.activeSelf)
					{
						this._holeCutter.gameObject.SetActive(false);
					}
				}
				else
				{
					LocalPlayer.Create.BuildingPlacer.IgnoreLookAtCollision = true;
					LocalPlayer.Create.BuildingPlacer.Airborne = true;
					if (!this._holeCutter.gameObject.activeSelf)
					{
						this._holeCutter.gameObject.SetActive(true);
					}
					BoxCollider component = this._holeCutter.GetComponent<BoxCollider>();
					component.size = new Vector3(this._logLength * 0.9f * 2f, base.transform.position.y - this._bottomY - 3f, this._logLength * 0.9f * 2f);
					component.center = new Vector3(0f, component.size.y / -2f, 0f);
					base.transform.eulerAngles = new Vector3(0f, LocalPlayer.Create.BuildingPlacer.YRotation, 0f);
					base.transform.position = new Vector3(base.transform.position.x, Mathf.Clamp(LocalPlayer.Create.BuildingPlacer.AirBorneHeight, this._bottomY + this._logLength, this._bottomY + this._logLength + 60f), base.transform.position.z);
					if (TheForest.Utils.Input.GetButtonDown("AltFire"))
					{
						base.transform.parent = LocalPlayer.Create.BuildingPlacer.transform;
						base.transform.localPosition = LocalPlayer.Create.GetGhostOffsetWithPlacer(base.gameObject);
						base.transform.localRotation = Quaternion.identity;
						this._lockMode = CraneArchitect.LockModes.Bottom;
						LocalPlayer.Create.BuildingPlacer.SetRenderer(null);
						LocalPlayer.Create.BuildingPlacer.ForcedParent = null;
						LocalPlayer.Sfx.PlayWhoosh();
					}
					if (LocalPlayer.Create.BuildingPlacer.Clear && TheForest.Utils.Input.GetButtonDown("Build"))
					{
						this._holeCutter.OnPlaced();
					}
				}
				this._aboveGround = !LocalPlayer.IsInCaves;
				this.CreateStructure(false);
				bool showManualfillLockIcon = this._lockMode == CraneArchitect.LockModes.Bottom && LocalPlayer.Create.BuildingPlacer.ClearOfCollision;
				bool flag2 = this._lockMode == CraneArchitect.LockModes.Top;
				bool showManualPlace = this._lockMode == CraneArchitect.LockModes.Top && LocalPlayer.Create.BuildingPlacer.Clear;
				Scene.HudGui.FoundationConstructionIcons.Show(showManualfillLockIcon, false, false, showManualPlace, false, flag2, false);
				if (Scene.HudGui.RotateIcon.activeSelf == flag2)
				{
					Scene.HudGui.RotateIcon.SetActive(!flag2);
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
					FoundationArchitect.CreateWindSFX(this._craneRoot.transform);
					this._craneRoot.transform.parent = base.transform;
					this._logPool = null;
					this._newPool = null;
					if (this._groundTrigger)
					{
						this._groundTrigger.position = new Vector3(this._groundTrigger.position.x, this._bottomY + 2.5f, this._groundTrigger.position.z);
						CapsuleCollider component = this._groundTrigger.GetComponent<CapsuleCollider>();
						component.height = base.transform.position.y - this._bottomY - 4f;
						component.center = new Vector3(0f, component.height / 2f, 0f);
					}
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
				Transform ghostRoot = this._craneRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				int totalLogs = this._craneRoot.childCount;
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
				float minY = base.transform.position.y;
				this._logMat = this._logRenderer.sharedMaterial;
				IEnumerator enumerator = this._craneRoot.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						IEnumerator enumerator2 = transform2.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								object obj2 = enumerator2.Current;
								Transform transform3 = (Transform)obj2;
								Renderer componentInChildren = transform3.GetComponentInChildren<Renderer>();
								componentInChildren.sharedMaterial = this._logMat;
								logs.Add(transform3.gameObject);
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator2 as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
						if (transform2.position.y < minY)
						{
							minY = transform2.position.y;
						}
					}
				}
				finally
				{
					IDisposable disposable2;
					if ((disposable2 = (enumerator as IDisposable)) != null)
					{
						disposable2.Dispose();
					}
				}
				ri.AddRuntimeObjects(logs.AsEnumerable<GameObject>().Reverse<GameObject>(), Prefabs.Instance.CraneLogBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				this._craneRoot.transform.parent = base.transform;
				ghostRoot.transform.parent = base.transform;
				this._craftStructure.GetComponent<Collider>().enabled = true;
				BoxCollider bc = this._craftStructure.gameObject.AddComponent<BoxCollider>();
				bc.isTrigger = true;
				Bounds b = new Bounds(base.transform.position, new Vector3(this._logLength * 1.8f, 0f, this._logLength * 1.8f));
				b.Encapsulate(new Vector3(base.transform.position.x, minY - 6f, base.transform.position.z));
				bc.center = this._craftStructure.transform.InverseTransformPoint(b.center);
				Vector3 finalColliderSize = this._craftStructure.transform.InverseTransformPoint(b.size + this._craftStructure.transform.position);
				finalColliderSize.x = 12f;
				finalColliderSize.z = 12f;
				float alpha = 0f;
				while (alpha < 1f)
				{
					bc.size = Vector3.Lerp(Vector3.zero, finalColliderSize, alpha += Time.deltaTime * 5f);
					yield return null;
				}
				bc.size = finalColliderSize;
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

		public void OnBuilt(GameObject built)
		{
			CraneArchitect component = built.GetComponent<CraneArchitect>();
			component._wasBuilt = true;
			component._aboveGround = this._aboveGround;
			component._bottomY = this._bottomY;
			if (this._craftStructure)
			{
				Craft_Structure craftStructure = this._craftStructure;
				craftStructure.OnBuilt = (Action<GameObject>)Delegate.Remove(craftStructure.OnBuilt, new Action<GameObject>(this.OnBuilt));
			}
		}

		public void Clear()
		{
			if (this._craneRoot)
			{
				this._craneRoot.transform.position = new Vector3(0f, 2000f, 0f);
				UnityEngine.Object.Destroy(this._craneRoot.gameObject);
				this._craneRoot = null;
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
			Transform craneRoot = this.SpawnCrane();
			this._logPool = this._newPool;
			if (this._craneRoot)
			{
				UnityEngine.Object.Destroy(this._craneRoot.gameObject);
			}
			this._craneRoot = craneRoot;
			if (this._wasBuilt)
			{
				base.GetComponent<BuildingHealth>()._renderersRoot = this._craneRoot.gameObject;
				if (isRepair)
				{
					this._craneRoot.transform.parent = base.transform;
				}
			}
		}

		public void Clone(CraneArchitect other)
		{
			this._logPrefab = other._logPrefab;
			this._logRenderer = other._logRenderer;
			this._maxLogScale = other._maxLogScale;
			this._craftStructure = other._craftStructure;
			this._logItemId = other._logItemId;
			this._aboveGround = other._aboveGround;
		}

		private Vector3 GetSegmentPointFloorPosition(Vector3 segmentPoint)
		{
			if (this._lockMode == CraneArchitect.LockModes.Top || this._wasBuilt || this._wasPlaced)
			{
				segmentPoint.y = this._bottomY;
				return segmentPoint;
			}
			segmentPoint.y -= this._logLength;
			return segmentPoint;
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

		private Transform SpawnCrane()
		{
			Transform transform = new GameObject("CraneRoot").transform;
			float num = this._logLength * 0.9f;
			Vector3 vector = base.transform.TransformPoint(new Vector3(-num, 0f, -num));
			Vector3 vector2 = base.transform.TransformPoint(new Vector3(-num, 0f, num));
			Vector3 vector3 = base.transform.TransformPoint(new Vector3(num, 0f, num));
			Vector3 vector4 = base.transform.TransformPoint(new Vector3(num, 0f, -num));
			Vector3 segmentPointFloorPosition = this.GetSegmentPointFloorPosition(vector);
			Vector3 segmentPointFloorPosition2 = this.GetSegmentPointFloorPosition(vector2);
			Vector3 segmentPointFloorPosition3 = this.GetSegmentPointFloorPosition(vector3);
			Vector3 segmentPointFloorPosition4 = this.GetSegmentPointFloorPosition(vector4);
			this.SpawnColumn(vector, segmentPointFloorPosition, transform);
			this.SpawnColumn(vector2, segmentPointFloorPosition2, transform);
			this.SpawnColumn(vector3, segmentPointFloorPosition3, transform);
			this.SpawnColumn(vector4, segmentPointFloorPosition4, transform);
			this.SpawnInterColumn(1, vector, segmentPointFloorPosition, vector2, segmentPointFloorPosition2, transform);
			this.SpawnInterColumn(2, vector2, segmentPointFloorPosition2, vector3, segmentPointFloorPosition3, transform);
			this.SpawnInterColumn(1, vector3, segmentPointFloorPosition3, vector4, segmentPointFloorPosition4, transform);
			this.SpawnInterColumn(2, vector4, segmentPointFloorPosition4, vector, segmentPointFloorPosition, transform);
			return transform;
		}

		private void SpawnColumn(Vector3 cTop, Vector3 cBottom, Transform parent)
		{
			int num = Mathf.CeilToInt((cTop.y - cBottom.y) / (this._logLength * 0.97f));
			Quaternion rotation = Quaternion.LookRotation(Vector3.down);
			for (int i = 0; i < num; i++)
			{
				Transform transform = this.NewLog(cTop + new Vector3(0f, this._logLength * 0.97f * (float)(-(float)i), 0f), rotation);
				transform.localScale = Vector3.one;
				this._newPool.Push(transform);
				transform.parent = parent;
			}
		}

		private void SpawnInterColumn(int offset, Vector3 c1Top, Vector3 c1Bottom, Vector3 c2Top, Vector3 c2Bottom, Transform parent)
		{
			int a = Mathf.CeilToInt((c1Top.y - c1Bottom.y) / (this._logLength * 0.97f));
			int b = Mathf.CeilToInt((c2Top.y - c2Bottom.y) / (this._logLength * 0.97f));
			int num = Mathf.Max(a, b) - 1;
			Quaternion rotation = Quaternion.LookRotation(c2Top - c1Top);
			for (int i = offset; i < num; i += 2)
			{
				Transform transform = this.NewLog(c1Top + new Vector3(0f, this._logLength * 0.97f * (float)(-(float)i), 0f), rotation);
				transform.localScale = new Vector3(1f, 1f, 1.8f);
				this._newPool.Push(transform);
				transform.parent = parent;
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

		public float MaxHeight { get; private set; }

		public Transform CraneRoot
		{
			get
			{
				return this._craneRoot;
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
				return new CoopCraneToken
				{
					bottomY = this._bottomY
				};
			}
			set
			{
				CoopCraneToken coopCraneToken = (CoopCraneToken)value;
				this._bottomY = coopCraneToken.bottomY;
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

		[SerializeThis]
		public bool _aboveGround;

		public Transform _logPrefab;

		public Renderer _logRenderer;

		public float _maxLogScale = 2f;

		public float _maxScaleLogCost = 1f;

		public LayerMask _floorLayers;

		public Craft_Structure _craftStructure;

		public FloorHoleArchitect _holeCutter;

		public Transform _groundTrigger;

		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		private bool _initialized;

		[SerializeThis]
		private float _bottomY;

		private Transform _craneRoot;

		private GameObject _tmpEdgeGo;

		private float _logLength;

		private float _logWidth;

		private Stack<Transform> _logPool;

		private Stack<Transform> _newPool;

		private Material _logMat;

		private CraneArchitect.LockModes _lockMode;

		private enum LockModes
		{
			Bottom,
			Top
		}
	}
}
