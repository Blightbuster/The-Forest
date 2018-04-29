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
	
	[AddComponentMenu("Buildings/Creation/Stairs Architect")]
	[DoNotSerializePublic]
	public class StairsArchitect : MonoBehaviour, ICoopStructure
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
			this._logMat = new Material(this._logRenderer.sharedMaterial);
			this._stairsRoot = new GameObject("StairsRoot").transform;
			if (this._multiPointsPositions == null)
			{
				this._multiPointsPositions = new List<Vector3>(5);
			}
			if (this._logPrefabs[0].GetComponent<Renderer>())
			{
				this._logLength = this._logPrefabs[0].GetComponent<Renderer>().bounds.size.z;
				this._logWidth = this._logPrefabs[0].GetComponent<Renderer>().bounds.size.x;
				this._logHeight = this._logPrefabs[0].GetComponent<Renderer>().bounds.size.y;
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
				if (!BoltNetwork.isClient)
				{
					LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, true);
				}
			}
			else if (this._craftStructure)
			{
				Craft_Structure craftStructure = this._craftStructure;
				craftStructure.OnBuilt = (Action<GameObject>)Delegate.Combine(craftStructure.OnBuilt, new Action<GameObject>(this.OnBuilt));
				this._craftStructure._playTwinkle = false;
				LocalPlayer.Create.Grabber.ClosePlace();
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

		
		private void Update()
		{
			bool flag = this._multiPointsPositions.Count > 1;
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag;
				if (base.GetComponent<Renderer>().enabled)
				{
					base.GetComponent<Renderer>().sharedMaterial = this._logMat;
				}
			}
			if (this._multiPointsPositions.Count > 0)
			{
				Vector3 vector = (this._multiPointsPositions.Count <= 1) ? base.transform.position : MathEx.TryAngleSnap(base.transform.position, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
				vector.y = base.transform.position.y;
				Vector3 vector2 = this._multiPointsPositions.Last<Vector3>();
				if (this._multiPointsPositions.Count > 1)
				{
					if (this._multiPointsPositions[0].y < this._multiPointsPositions[1].y)
					{
						if (vector.y < vector2.y)
						{
							vector.y = vector2.y;
						}
					}
					else if (vector.y > vector2.y)
					{
						vector.y = vector2.y;
					}
				}
				if (Vector3.Distance(vector2, vector) > this._logWidth)
				{
					if (Vector3.Distance(vector2, vector) >= this._maxEdgeLength)
					{
						vector = vector2 + (vector - vector2).normalized * this._maxEdgeLength;
					}
					this._multiPointsPositions.Add(vector);
					Transform stairsRoot = this.SpawnStairs();
					this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					if (this._stairsRoot)
					{
						UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
					}
					this._stairsRoot = stairsRoot;
				}
			}
			else if (this._stairsRoot)
			{
				base.GetComponent<Renderer>().enabled = true;
				UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
				this._stairsRoot = null;
				this._logPool.Clear();
			}
			this.CheckLockPoint();
			this.CheckUnlockPoint();
		}

		
		private void OnDestroy()
		{
			if (this._stairsRoot)
			{
				UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
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
					this._stairsRoot.transform.parent = base.transform;
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
			this._wasPlaced = true;
			base.enabled = false;
			this._craftStructure.GetComponent<Collider>().enabled = false;
			Scene.HudGui.RoofConstructionIcons.Shutdown();
			yield return null;
			if (this._craftStructure)
			{
				UnityEngine.Object.Destroy(base.GetComponent<Rigidbody>());
				UnityEngine.Object.Destroy(base.GetComponent<Collider>());
				UnityEngine.Object.Destroy(base.GetComponent<Renderer>());
				Transform newStairs = this.SpawnStairs();
				if (this._stairsRoot)
				{
					UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
				}
				this._stairsRoot = newStairs;
				Transform ghostRoot = this._stairsRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				this._logPool.Clear();
				Transform logGhostPrefab = this._logPrefabs[0];
				Transform StiltGhostPrefab = this._stiltPrefab;
				this._logPrefabs[0] = Prefabs.Instance.LogStairsBuiltPrefab;
				this._stiltPrefab = Prefabs.Instance.LogStiltStairsGhostBuiltPrefab;
				this._stairsRoot = this.SpawnStairs();
				this._stairsRoot.name = "StairsRootBuilt";
				this._logPool = null;
				this._newPool = null;
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.$this._logItemId);
				List<GameObject> logStacks = new List<GameObject>();
				IEnumerator enumerator = this._stairsRoot.GetEnumerator();
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
								transform3.gameObject.SetActive(false);
								logStacks.Add(transform3.gameObject);
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
				ri._renderers = logStacks.ToArray();
				ri._amount += logStacks.Count;
				this._logPrefabs[0] = logGhostPrefab;
				this._stiltPrefab = StiltGhostPrefab;
				ghostRoot.transform.parent = null;
				base.transform.position = this._multiPointsPositions.First<Vector3>();
				base.transform.LookAt(this._multiPointsPositions.Last<Vector3>());
				this._stairsRoot.transform.parent = base.transform;
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
					b.Encapsulate(this._craftStructure.transform.InverseTransformPoint(position));
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
					getStructureStrength._type = getStructureStrength.structureType.floor;
				}
			}
			yield break;
		}

		
		private void OnBuilt(GameObject built)
		{
			StairsArchitect component = built.GetComponent<StairsArchitect>();
			component._multiPointsPositions = this._multiPointsPositions;
			component._wasBuilt = true;
			component.OnSerializing();
		}

		
		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				if (this._stairsRoot)
				{
					UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
				}
				this._stairsRoot = null;
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this._stairsRoot = this.SpawnStairs();
			if (this._wasBuilt && isRepair)
			{
				this._stairsRoot.parent = base.transform;
			}
		}

		
		private void CheckLockPoint()
		{
			bool flag = this._multiPointsPositions.Count == 0;
			bool flag2 = false;
			Vector3 vector = (this._multiPointsPositions.Count <= 1) ? base.transform.position : MathEx.TryAngleSnap(base.transform.position, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
			vector.y = base.transform.position.y;
			if (!flag)
			{
				Vector3 vector2 = vector - this._multiPointsPositions.Last<Vector3>();
				Vector3 vector3 = vector2;
				vector3.y = 0f;
				float magnitude = vector2.magnitude;
				float num = vector3.magnitude;
				if (num > this._logLength || this._multiPointsPositions.Count >= 2 || (this._multiPointsPositions.Count > 0 && this._multiPointsPositions[0].y < vector.y))
				{
					num -= this._logWidth * 3f;
				}
				vector3.y = 0f;
				flag = (magnitude > this._logWidth * 2f && num * 1.1f >= Mathf.Abs(vector2.y));
				flag2 = (magnitude >= this._maxEdgeLength);
			}
			if (flag && TheForest.Utils.Input.GetButtonDown("Fire1"))
			{
				Vector3 vector4 = (this._multiPointsPositions.Count <= 0) ? vector : this._multiPointsPositions.Last<Vector3>();
				if (this._multiPointsPositions.Count > 1)
				{
					if (this._multiPointsPositions[0].y < this._multiPointsPositions[1].y)
					{
						if (vector.y < vector4.y)
						{
							vector.y = vector4.y;
						}
					}
					else if (vector.y > vector4.y)
					{
						vector.y = vector4.y;
					}
				}
				if (flag2)
				{
					vector = vector4 + (vector - vector4).normalized * this._maxEdgeLength;
				}
				if (base.GetComponent<Renderer>().enabled)
				{
					base.GetComponent<Renderer>().enabled = false;
				}
				this._multiPointsPositions.Add(vector);
			}
		}

		
		private void CheckUnlockPoint()
		{
			if (TheForest.Utils.Input.GetButtonDown("AltFire") && this._multiPointsPositions.Count > 0)
			{
				this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
				if (this._multiPointsPositions.Count == 0)
				{
					base.GetComponent<Renderer>().enabled = true;
				}
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

		
		private Transform SpawnStairs()
		{
			Transform transform = new GameObject("StairsRoot").transform;
			transform.position = this._multiPointsPositions.First<Vector3>();
			Stack<Transform> stack = new Stack<Transform>();
			int num;
			int num2;
			bool flag;
			if (this._multiPointsPositions[0].y < this._multiPointsPositions[1].y)
			{
				num = this._multiPointsPositions.Count - 2;
				num2 = -1;
				flag = false;
			}
			else
			{
				num = 1;
				num2 = 1;
				flag = true;
			}
			while (num >= 0 && num < this._multiPointsPositions.Count)
			{
				Vector3 vector = this._multiPointsPositions[num - num2];
				Vector3 a = this._multiPointsPositions[num];
				Vector3 vector2 = vector;
				Vector3 forward = a - vector;
				Vector3 rhs = new Vector3(forward.x, 0f, forward.z);
				Vector3 vector3 = Vector3.Cross(Vector3.up, rhs);
				int num3 = Mathf.CeilToInt(rhs.magnitude / this._logWidth);
				Vector3 vector4 = rhs.normalized * this._logWidth;
				Quaternion rotation = Quaternion.LookRotation(vector3);
				bool flag2 = flag && rhs.magnitude < this._logLength;
				flag = false;
				Transform transform2 = new GameObject("StairEdge" + num).transform;
				transform2.parent = transform;
				float d = (!flag2) ? 2.5f : 0.5f;
				forward = a - (vector2 + vector4 * d);
				transform2.rotation = Quaternion.LookRotation(forward);
				transform2.position = vector2 + vector4 * d;
				Vector3 pointFloorPosition = this.GetPointFloorPosition(vector2 + Vector3.down);
				Transform transform3 = UnityEngine.Object.Instantiate<Transform>(this._stiltPrefab, vector2, Quaternion.identity);
				transform3.parent = transform2;
				transform3.localScale = new Vector3(1f, Mathf.Abs(vector2.y - pointFloorPosition.y) / 4.5f, 1f);
				if (!this._wasBuilt && !this._wasPlaced)
				{
					transform3.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
				}
				if (!flag2)
				{
					vector2 -= vector4 * 2.5f;
					for (int i = 0; i < 5; i++)
					{
						Debug.DrawLine(vector2 - vector3, vector2 + vector3, Color.cyan);
						Transform transform4 = this.NewLog(vector2, rotation);
						transform4.parent = transform2;
						stack.Push(transform4);
						vector2 += vector4;
						if (this._wasBuilt && i == 2)
						{
							transform4.rotation = rotation;
							transform4.tag = "UnderfootWood";
							BoxCollider boxCollider = transform4.gameObject.AddComponent<BoxCollider>();
							boxCollider.size = new Vector3(this._logWidth * 5.5f, this._logHeight, this._logLength);
							boxCollider.center = new Vector3(-0.42f, -0.34f, 0f);
							transform4.gameObject.AddComponent<gridObjectBlocker>();
						}
					}
					num3 -= 3;
				}
				Transform transform5 = UnityEngine.Object.Instantiate<Transform>(this._stiltPrefab);
				transform5.parent = transform2;
				transform5.localPosition = new Vector3(0f, -0.75f, 0f);
				transform5.localEulerAngles = new Vector3(-90f, 0f, 0f);
				transform5.localScale = new Vector3(1f, Mathf.Abs(forward.magnitude) / 4.68f, 1f);
				if (!this._wasBuilt && !this._wasPlaced)
				{
					transform5.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
				}
				Vector3 b = new Vector3(0f, forward.y / (float)num3, 0f);
				for (int j = 0; j < num3; j++)
				{
					Debug.DrawLine(vector2 - vector3, vector2 + vector3, Color.cyan);
					Transform transform6 = this.NewLog(vector2, rotation);
					transform6.parent = transform2;
					stack.Push(transform6);
					vector2 += vector4;
					vector2 += b;
				}
				if (this._wasBuilt)
				{
					BoxCollider boxCollider2 = transform2.gameObject.AddComponent<BoxCollider>();
					boxCollider2.size = new Vector3(this._logLength, this._logHeight, forward.magnitude);
					boxCollider2.center = new Vector3(0f, -0.34f, boxCollider2.size.z / 2f);
					transform2.tag = "UnderfootWood";
					transform2.gameObject.AddComponent<BuildingHealthHitRelay>();
					transform2.gameObject.layer = 21;
					getStructureStrength getStructureStrength = transform2.gameObject.AddComponent<getStructureStrength>();
					getStructureStrength._strength = getStructureStrength.strength.normal;
					getStructureStrength._type = getStructureStrength.structureType.floor;
					transform2.gameObject.AddComponent<gridObjectBlocker>();
					FoundationArchitect.CreateWindSFX(transform5);
				}
				num += num2;
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
				return transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefabs[UnityEngine.Random.Range(0, this._logPrefabs.Length)], position, rotation);
			if (!this._wasBuilt && !this._wasPlaced)
			{
				transform2.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
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

		
		public Renderer _logRenderer;

		
		public float _maxEdgeLength = 12f;

		
		public float _closureSnappingDistance = 2.5f;

		
		public float _logCost = 1f;

		
		public float _minAngleBetweenEdges = 90f;

		
		public int _maxPoints = 50;

		
		public LayerMask _floorLayers;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		
		private bool _initialized;

		
		[SerializeThis]
		private List<Vector3> _multiPointsPositions;

		
		[SerializeThis]
		private int _multiPointsPositionsCount;

		
		private Vector3 _placerOffset;

		
		private Transform _stairsRoot;

		
		private float _logLength;

		
		private float _logWidth;

		
		private float _logHeight;

		
		private float _maxChunkLength;

		
		private float _minChunkLength;

		
		private Stack<Transform> _logPool;

		
		private Stack<Transform> _newPool;

		
		private Material _logMat;
	}
}
