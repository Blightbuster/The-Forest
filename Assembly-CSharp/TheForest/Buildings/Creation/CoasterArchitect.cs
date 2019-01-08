using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[AddComponentMenu("Buildings/Creation/Coaster Architect")]
	[DoNotSerializePublic]
	public class CoasterArchitect : MonoBehaviour, ICoopStructure, IProceduralStructure
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
			this._coasterRoot = new GameObject("CoasterRoot").transform;
			if (this._multiPointsPositions == null)
			{
				this._multiPointsPositions = new List<Vector3>(5);
			}
			Renderer foundRenderer = this._logPrefabs[0].GetComponentInChildren<Renderer>();
			if (foundRenderer)
			{
				this._logLength = foundRenderer.bounds.size.z;
				this._logWidth = foundRenderer.bounds.size.x;
				this._logHeight = foundRenderer.bounds.size.y;
			}
			else
			{
				this._logLength = this._logRenderer.bounds.size.z;
				this._logWidth = this._logRenderer.bounds.size.x;
				this._logHeight = this._logRenderer.bounds.size.y;
			}
			yield return null;
			if (this._wasBuilt && !isDeserializing)
			{
				this.OnDeserialized();
				if (!BoltNetwork.isClient && LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, true);
				}
			}
			else if (this._craftStructure)
			{
				this._craftStructure.CustomLockCheck = new Func<bool>(this.ArePlayersInside);
				Craft_Structure craftStructure = this._craftStructure;
				craftStructure.OnBuilt = (Action<GameObject>)Delegate.Combine(craftStructure.OnBuilt, new Action<GameObject>(this.OnBuilt));
				this._craftStructure._playTwinkle = false;
				if (LocalPlayer.Create)
				{
					LocalPlayer.Create.Grabber.ClosePlace();
				}
			}
			if (isDeserializing || this._wasPlaced || this._wasBuilt)
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
			if (this._multiPointsPositions.Count > 0)
			{
				Vector3 vector = (this._multiPointsPositions.Count <= 1) ? base.transform.position : MathEx.TryAngleSnap(base.transform.position, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
				vector.y = base.transform.position.y;
				Vector3 vector2 = this._multiPointsPositions.Last<Vector3>();
				if (Vector3.Distance(vector2, vector) > this._logWidth)
				{
					if (Vector3.Distance(vector2, vector) >= this._maxEdgeLength)
					{
						vector = vector2 + (vector - vector2).normalized * this._maxEdgeLength;
					}
					this._multiPointsPositions.Add(vector);
					Transform coasterRoot = this.SpawnStructure();
					this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					if (this._coasterRoot)
					{
						UnityEngine.Object.Destroy(this._coasterRoot.gameObject);
					}
					this._coasterRoot = coasterRoot;
				}
			}
			else
			{
				this._multiPointsPositions.Add(base.transform.position);
				this._multiPointsPositions.Add(base.transform.position + base.transform.forward * (this._logWidth * 0.8f));
				Transform coasterRoot2 = this.SpawnStructure();
				this._multiPointsPositions.RemoveAt(0);
				this._multiPointsPositions.RemoveAt(0);
				if (this._coasterRoot)
				{
					UnityEngine.Object.Destroy(this._coasterRoot.gameObject);
				}
				this._coasterRoot = coasterRoot2;
			}
			Create.CanLock = this.CheckLockPoint();
			bool flag = this._multiPointsPositions.Count > 1 || (this._multiPointsPositions.Count > 0 && Create.CanLock);
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag;
			}
			this.CheckUnlockPoint();
			bool flag2 = this._multiPointsPositions.Count == 0;
			bool canLock = Create.CanLock && this._multiPointsPositions.Count > 0;
			bool canUnlock = !flag2;
			Scene.HudGui.RoofConstructionIcons.Show(flag2, false, false, flag, canLock, canUnlock, false);
		}

		private void OnDestroy()
		{
			if (this._coasterRoot)
			{
				UnityEngine.Object.Destroy(this._coasterRoot.gameObject);
			}
			if (Scene.HudGui)
			{
				Scene.HudGui.RoofConstructionIcons.Shutdown();
			}
		}

		private void OnSerializing()
		{
			this._multiPointsPositions.Capacity = this._multiPointsPositions.Count;
			this._multiPointsPositionsCount = this._multiPointsPositions.Count;
		}

		private void OnDeserialized()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				this._multiPointsPositions.RemoveRange(this._multiPointsPositionsCount, this._multiPointsPositions.Count - this._multiPointsPositionsCount);
				if (this._wasBuilt)
				{
					this.CreateStructure(false);
					this._coasterRoot.transform.parent = base.transform;
					this._logPool = null;
					this._newPool = null;
				}
				else if (this._wasPlaced)
				{
					base.StartCoroutine(this.OnPlaced());
				}
			}
		}

		private IEnumerator OnPlaced()
		{
			if (!this._wasPlaced && !this._wasBuilt && !BoltNetwork.isClient)
			{
				base.transform.position = this._multiPointsPositions.First<Vector3>();
				base.transform.LookAt(this._multiPointsPositions.Last<Vector3>());
				this._multiPointsPositions = (from p in this._multiPointsPositions
				select this.$this.transform.InverseTransformPoint(p)).ToList<Vector3>();
			}
			this.WasPlaced = true;
			base.enabled = false;
			this._craftStructure.GetComponent<Collider>().enabled = false;
			Scene.HudGui.RoofConstructionIcons.Shutdown();
			Scene.HudGui.RotateIcon.SetActive(false);
			yield return null;
			if (this._craftStructure)
			{
				UnityEngine.Object.Destroy(base.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy(base.GetComponent<Collider>());
				Transform newCoaster = this.SpawnStructure();
				if (this._coasterRoot)
				{
					UnityEngine.Object.Destroy(this._coasterRoot.gameObject);
				}
				this._coasterRoot = newCoaster;
				Transform ghostRoot = this._coasterRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				this._logPool = null;
				this._newPool = null;
				this.UpdateBuildingIngredients();
				ghostRoot.transform.parent = null;
				this._coasterRoot.transform.parent = base.transform;
				ghostRoot.transform.parent = base.transform;
				BoxCollider bc;
				if (this._craftStructure.GetComponent<Collider>() is BoxCollider)
				{
					bc = (BoxCollider)this._craftStructure.GetComponent<Collider>();
				}
				else
				{
					bc = this._craftStructure.gameObject.AddComponent<BoxCollider>();
					bc.isTrigger = true;
				}
				Bounds b = default(Bounds);
				foreach (Vector3 position in this._multiPointsPositions)
				{
					b.Encapsulate(this._craftStructure.transform.InverseTransformPoint(base.transform.TransformPoint(position)));
				}
				b.Encapsulate(this._craftStructure.transform.InverseTransformPoint(this.GetPointFloorPosition(this._craftStructure.transform.position)));
				Vector3 localSize = b.size;
				localSize.x = Mathf.Max(localSize.x, 6f);
				localSize.y = Mathf.Max(localSize.y, 3f);
				localSize.z = Mathf.Max(localSize.z, 3f);
				bc.size = localSize;
				bc.center = b.center;
				bc.enabled = true;
				yield return null;
				this._craftStructure.manualLoading = true;
				while (LevelSerializer.IsDeserializing && !this._craftStructure.WasLoaded)
				{
					yield return null;
				}
				this._craftStructure.Initialize();
				this._craftStructure.gameObject.SetActive(true);
				yield return null;
				bc.enabled = false;
				bc.enabled = true;
				if (!this._craftStructure.gameObject.GetComponent<getStructureStrength>())
				{
					getStructureStrength getStructureStrength = this._craftStructure.gameObject.AddComponent<getStructureStrength>();
					getStructureStrength._strength = getStructureStrength.strength.normal;
					getStructureStrength._type = getStructureStrength.structureType.wall;
				}
			}
			yield break;
		}

		private void UpdateBuildingIngredients()
		{
			Dictionary<int, Craft_Structure.BuildIngredients> dictionary = new Dictionary<int, Craft_Structure.BuildIngredients>();
			Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
			foreach (Craft_Structure.BuildIngredients buildIngredients in this._craftStructure._requiredIngredients)
			{
				dictionary.Add(buildIngredients._itemID, buildIngredients);
				dictionary2.Add(buildIngredients._itemID, 0f);
			}
			foreach (IngredientInfo ingredientInfo in this._coasterRoot.GetComponentsInChildren<IngredientInfo>())
			{
				if (ingredientInfo.ItemId != 0)
				{
					Craft_Structure.BuildIngredients buildIngredients2;
					if (!dictionary.TryGetValue(ingredientInfo.ItemId, out buildIngredients2))
					{
						buildIngredients2 = new Craft_Structure.BuildIngredients();
						buildIngredients2._subIngredients = new List<Craft_Structure.BuildIngredients.SubIngredient>();
						buildIngredients2._itemID = ingredientInfo.ItemId;
						dictionary.Add(ingredientInfo.ItemId, buildIngredients2);
						dictionary2.Add(ingredientInfo.ItemId, 0f);
						this._craftStructure._requiredIngredients.Add(buildIngredients2);
					}
					buildIngredients2.AddRuntimeObjects(ingredientInfo.Objects, ingredientInfo.Material);
					Dictionary<int, float> dictionary3;
					int itemId;
					(dictionary3 = dictionary2)[itemId = ingredientInfo.ItemId] = dictionary3[itemId] + ingredientInfo.CostMultiplier;
					buildIngredients2._amount = Mathf.CeilToInt(dictionary2[ingredientInfo.ItemId]);
				}
			}
		}

		private void OnBuilt(GameObject built)
		{
			CoasterArchitect component = built.GetComponent<CoasterArchitect>();
			component._multiPointsPositions = this._multiPointsPositions;
			component._wasBuilt = true;
			component.OnSerializing();
		}

		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				if (this._coasterRoot)
				{
					UnityEngine.Object.Destroy(this._coasterRoot.gameObject);
				}
				this._coasterRoot = null;
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this._coasterRoot = this.SpawnStructure();
			if (this._wasBuilt && isRepair)
			{
				this._coasterRoot.parent = base.transform;
			}
		}

		private float ClosestDistanceToCoaster(Vector3 point, List<Vector3> polygon)
		{
			float num = float.MaxValue;
			for (int i = 1; i < polygon.Count; i++)
			{
				Vector3 linePoint = polygon[i];
				Vector3 linePoint2 = polygon[i - 1];
				Vector3 vector = point;
				linePoint.y = (linePoint2.y = (vector.y = 0f));
				Vector3 a = MathEx.ProjectPointOnLineSegment(linePoint, linePoint2, vector);
				float num2 = Vector3.Distance(a, vector);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		private void GetPolyYBounds(List<Vector3> polygon, out float min, out float max)
		{
			min = float.MaxValue;
			max = float.MinValue;
			foreach (Vector3 vector in polygon)
			{
				if (vector.y < min)
				{
					min = vector.y;
				}
				if (vector.y > max)
				{
					max = vector.y;
				}
			}
		}

		private bool ArePlayersInside()
		{
			if (LocalPlayer.Transform)
			{
				for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
				{
					Vector3 point = base.transform.InverseTransformPoint(Scene.SceneTracker.allPlayers[i].transform.position);
					float num;
					float num2;
					this.GetPolyYBounds(this._multiPointsPositions, out num, out num2);
					num2 += 2.5f;
					num -= 2.5f;
					if (point.y > num && point.y < num2)
					{
						float num3 = this.ClosestDistanceToCoaster(point, this._multiPointsPositions);
						if (num3 < 5.25f)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private bool CheckLockPoint()
		{
			bool flag = this._multiPointsPositions.Count > 0;
			bool flag2 = this._multiPointsPositions.Count == 0 && LocalPlayer.Create.BuildingPlacer.ClearOfCollision;
			bool flag3 = false;
			Vector3 vector = (this._multiPointsPositions.Count <= 1) ? base.transform.position : MathEx.TryAngleSnap(base.transform.position, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
			vector.y = base.transform.position.y;
			if (!flag2 && LocalPlayer.Create.BuildingPlacer.ClearOfCollision)
			{
				Vector3 vector2 = vector - this._multiPointsPositions.Last<Vector3>();
				Vector3 vector3 = vector2;
				vector3.y = 0f;
				float magnitude = vector2.magnitude;
				float num = vector3.magnitude;
				if (num > this._logLength || this._multiPointsPositions.Count >= 2)
				{
					num -= this._logWidth * 3f;
				}
				if (this._multiPointsPositions.Count >= 2 && this._multiPointsPositions[this._multiPointsPositions.Count - 1].y > vector.y)
				{
					num -= this._logWidth * 3f;
				}
				vector3.y = 0f;
				flag2 = (magnitude > this._logWidth * 1f && num * 1.5f >= Mathf.Abs(vector2.y));
				flag3 = (magnitude >= this._maxEdgeLength);
			}
			if (flag2 && (TheForest.Utils.Input.GetButtonDown("Fire1") || (flag && TheForest.Utils.Input.GetButtonDown("Build"))))
			{
				Vector3 vector4 = (this._multiPointsPositions.Count <= 0) ? vector : this._multiPointsPositions.Last<Vector3>();
				if (flag3)
				{
					vector = vector4 + (vector - vector4).normalized * this._maxEdgeLength;
				}
				this._multiPointsPositions.Add(vector);
			}
			Scene.HudGui.RotateIcon.SetActive(this._multiPointsPositions.Count == 0);
			return flag2;
		}

		private void CheckUnlockPoint()
		{
			if (TheForest.Utils.Input.GetButtonDown("AltFire") && this._multiPointsPositions.Count > 0)
			{
				this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
			}
		}

		private Vector3 GetPointFloorPosition(Vector3 point)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(point, Vector3.down, out raycastHit, 150f, Scene.ValidateFloorLayers(point, this._floorLayers.value)))
			{
				return raycastHit.point;
			}
			point.y -= this._logLength / 2f;
			return point;
		}

		private void SpawnVerticalStilt(Vector3 position, Transform edgeTr)
		{
			Vector3 pointFloorPosition = this.GetPointFloorPosition(position + Vector3.down * this._logWidth);
			if (Mathf.Abs(pointFloorPosition.y - position.y) > this._logWidth * 2f)
			{
				Transform transform = UnityEngine.Object.Instantiate<Transform>(this._stiltPrefab, position, Quaternion.identity);
				transform.parent = edgeTr;
				transform.localScale = new Vector3(1f, Mathf.Abs(position.y - pointFloorPosition.y) / 4.5f, 1f);
				if (!this._wasBuilt && !this._wasPlaced)
				{
					Create.ApplyGhostMaterial(transform, false);
				}
				else
				{
					transform.gameObject.SetActive(true);
				}
			}
		}

		private Vector3 GetWorldPoint(int pointIndex)
		{
			return (!this._wasPlaced && !this._wasBuilt) ? this._multiPointsPositions[pointIndex] : base.transform.TransformPoint(this._multiPointsPositions[pointIndex]);
		}

		public Transform SpawnStructure()
		{
			Transform transform = new GameObject("CoasterRoot").transform;
			transform.position = this.GetWorldPoint(0);
			Stack<Transform> stack = new Stack<Transform>();
			if (this._logPool == null)
			{
				this._logPool = new Stack<Transform>();
			}
			int i = 1;
			int num = 1;
			while (i < this._multiPointsPositions.Count)
			{
				Vector3 worldPoint = this.GetWorldPoint(i);
				Vector3 worldPoint2 = this.GetWorldPoint(i - num);
				Vector3 vector = worldPoint2;
				Vector3 a = worldPoint;
				Vector3 forward = a - vector;
				int num2 = Mathf.CeilToInt(forward.magnitude / this._logWidth);
				Vector3 b = forward.normalized * this._logWidth;
				Transform transform2 = new GameObject("CoasterEdge" + i).transform;
				transform2.parent = transform;
				Vector3 vector2 = vector;
				forward = a - vector2;
				transform2.rotation = Quaternion.LookRotation(forward);
				transform2.position = vector2;
				this.SpawnVerticalStilt(vector, transform2);
				Transform transform3 = UnityEngine.Object.Instantiate<Transform>(this._stiltPrefab);
				transform3.parent = transform2;
				transform3.localPosition = new Vector3(0f, -0.75f, 0f);
				transform3.localEulerAngles = new Vector3(-90f, 0f, 0f);
				transform3.localScale = new Vector3(1f, Mathf.Abs(forward.magnitude) / 4.68f, 1f);
				if (!this._wasBuilt && !this._wasPlaced)
				{
					Create.ApplyGhostMaterial(transform3, false);
				}
				else
				{
					transform3.gameObject.SetActive(true);
				}
				for (int j = 0; j < num2; j++)
				{
					Transform transform4 = this.NewLog(vector2, transform2.rotation * Quaternion.Euler(0f, 90f, 0f));
					transform4.parent = transform2;
					stack.Push(transform4);
					vector2 += b;
				}
				if (this._wasBuilt)
				{
					for (int k = 0; k < this._colliderSource.Length; k++)
					{
						GameObject gameObject = new GameObject(string.Format("Collider_{0:000}", k));
						gameObject.transform.parent = transform2;
						gameObject.transform.localPosition = this._colliderSource[k].transform.localPosition;
						gameObject.transform.localScale = this._colliderSource[k].transform.localScale;
						gameObject.transform.localRotation = this._colliderSource[k].transform.localRotation;
						BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
						boxCollider.sharedMaterial = this._colliderSource[k].sharedMaterial;
						gameObject.tag = this._colliderSource[k].gameObject.tag;
						gameObject.layer = this._colliderSource[k].gameObject.layer;
						float num3 = forward.magnitude + 0.5f;
						boxCollider.size = new Vector3(this._colliderSource[k].size.x, this._colliderSource[k].size.y, num3);
						gameObject.transform.localPosition += new Vector3(0f, 0f, num3 / 2f);
					}
					transform2.tag = "UnderfootWood";
					transform2.gameObject.AddComponent<BuildingHealthHitRelay>();
					transform2.gameObject.layer = 21;
					getStructureStrength getStructureStrength = transform2.gameObject.AddComponent<getStructureStrength>();
					getStructureStrength._strength = getStructureStrength.strength.normal;
					getStructureStrength._type = getStructureStrength.structureType.wall;
					gridObjectBlocker gridObjectBlocker = transform2.gameObject.AddComponent<gridObjectBlocker>();
					gridObjectBlocker.GatherChildColliders = true;
					FoundationArchitect.CreateWindSFX(transform3);
				}
				i += num;
			}
			if (!this._wasPlaced && !this._wasBuilt)
			{
				this._logPool = stack;
			}
			return transform;
		}

		private Transform NewLog(Vector3 position, Quaternion rotation)
		{
			if (this._logPool.Count > 0)
			{
				Transform transform = this._logPool.Pop();
				transform.position = position;
				transform.rotation = rotation;
				if (!this._wasBuilt && !this._wasPlaced)
				{
					Create.ApplyGhostMaterial(transform, true);
				}
				return transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefabs[UnityEngine.Random.Range(0, this._logPrefabs.Length)], position, rotation);
			if (!this._wasBuilt && !this._wasPlaced)
			{
				Create.ApplyGhostMaterial(transform2, true);
			}
			else if (this._wasBuilt)
			{
				transform2.rotation *= this.RandomLogRotation();
			}
			return transform2;
		}

		private Quaternion RandomLogRotation()
		{
			return Quaternion.Euler((float)UnityEngine.Random.Range(-3, 3), (float)UnityEngine.Random.Range(-2, 2), (float)UnityEngine.Random.Range(-5, 5));
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

		public int MultiPointsCount
		{
			get
			{
				return this._multiPointsPositionsCount;
			}
			set
			{
				this._multiPointsPositionsCount = value;
			}
		}

		public List<Vector3> MultiPointsPositions
		{
			get
			{
				if (!this._wasPlaced && !this._wasBuilt)
				{
					base.transform.position = this._multiPointsPositions.First<Vector3>();
					base.transform.LookAt(this._multiPointsPositions.Last<Vector3>());
					this._multiPointsPositions = (from p in this._multiPointsPositions
					select base.transform.InverseTransformPoint(p)).ToList<Vector3>();
					this._wasPlaced = true;
				}
				return this._multiPointsPositions;
			}
			set
			{
				this._multiPointsPositions = value;
			}
		}

		public IProtocolToken CustomToken
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[SerializeThis]
		public bool _wasPlaced;

		[SerializeThis]
		public bool _wasBuilt;

		public Transform[] _logPrefabs;

		public Transform _stiltPrefab;

		public BoxCollider[] _colliderSource;

		public Renderer _logRenderer;

		public float _maxEdgeLength = 12f;

		public float _closureSnappingDistance = 2.5f;

		public float _logCost = 1f;

		public float _minAngleBetweenEdges = 90f;

		public int _maxPoints = 50;

		public LayerMask _floorLayers;

		public Craft_Structure _craftStructure;

		private bool _initialized;

		[SerializeThis]
		private List<Vector3> _multiPointsPositions;

		[SerializeThis]
		private int _multiPointsPositionsCount;

		private Vector3 _placerOffset;

		private Transform _coasterRoot;

		private float _logLength;

		private float _logWidth;

		private float _logHeight;

		private float _maxChunkLength;

		private float _minChunkLength;

		private Stack<Transform> _logPool;

		private Stack<Transform> _newPool;
	}
}
