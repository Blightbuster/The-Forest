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
	
	[AddComponentMenu("Buildings/Creation/Dock Architect")]
	[DoNotSerializePublic]
	public class DockArchitect : MonoBehaviour, IProceduralStructure, ICoopStructure, TriggerTagSensor.ITarget
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
			this._logMat = ((!this._wasPlaced && !this._wasBuilt) ? new Material(this._logRenderer.sharedMaterial) : this._logRenderer.sharedMaterial);
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
				LocalPlayer.Create.BuildingPlacer.Clear = false;
				if (base.GetComponent<Renderer>())
				{
					base.GetComponent<Renderer>().sharedMaterial = this._logMat;
				}
			}
			yield break;
		}

		
		private void Update()
		{
			this.CheckWaterLevel();
			bool flag = this._multiPointsPositions.Count > 0;
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag;
				if (base.GetComponent<Renderer>().enabled)
				{
					base.GetComponent<Renderer>().sharedMaterial = this._logMat;
				}
			}
			this._logMat.SetColor("_TintColor", (this._waterLevel != float.MaxValue) ? LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor") : LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor"));
			if (this._waterLevel == 3.40282347E+38f)
			{
				Scene.HudGui.RotateIcon.SetActive(this._multiPointsPositions.Count == 0);
				Scene.HudGui.RoofConstructionIcons.Shutdown();
				return;
			}
			if (this._multiPointsPositions.Count > 0)
			{
				Vector3 vector = base.transform.position;
				Vector3 vector2 = this._multiPointsPositions.Last<Vector3>();
				if (this._multiPointsPositions.Count == 1 && Vector3.Distance(vector2, vector) < this._logWidth)
				{
					vector.y = vector2.y;
					Vector3 vector3 = vector - vector2;
					if (vector3.sqrMagnitude == 0f)
					{
						vector3 = Quaternion.Euler(0f, base.transform.eulerAngles.y, 0f) * Vector3.forward;
					}
					vector = vector2 + vector3.normalized * (this._logWidth * 0.75f);
				}
				vector.y = vector2.y;
				if (Vector3.Distance(vector2, vector) >= this._logWidth * 0.5f)
				{
					if (Vector3.Distance(vector2, vector) >= this._maxEdgeLength)
					{
						vector = vector2 + (vector - vector2).normalized * this._maxEdgeLength;
					}
					this._multiPointsPositions.Add(vector);
					Transform dockRoot = this.SpawnStructure();
					this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					if (this._dockRoot)
					{
						UnityEngine.Object.Destroy(this._dockRoot.gameObject);
					}
					this._dockRoot = dockRoot;
				}
			}
			else
			{
				if (this._dockRoot)
				{
					base.GetComponent<Renderer>().enabled = true;
					UnityEngine.Object.Destroy(this._dockRoot.gameObject);
					this._dockRoot = null;
					this._logPool.Clear();
				}
				if (base.transform.parent.position.y < this._waterLevel + 2f)
				{
					Vector3 position = base.transform.parent.position;
					position.y = this._waterLevel + 2f;
					base.transform.position = position;
					base.transform.rotation = Quaternion.Euler(0f, base.transform.eulerAngles.y, 0f);
				}
			}
			this.CheckLockPoint();
			this.CheckUnlockPoint();
		}

		
		private void OnDestroy()
		{
			if (this._dockRoot)
			{
				UnityEngine.Object.Destroy(this._dockRoot.gameObject);
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
					this._dockRoot.transform.parent = base.transform;
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
				Transform newDock = this.SpawnStructure();
				if (this._dockRoot)
				{
					UnityEngine.Object.Destroy(this._dockRoot.gameObject);
				}
				this._dockRoot = newDock;
				Transform ghostRoot = this._dockRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				this._logPool.Clear();
				Transform logGhostPrefab = this._logPrefabs[0];
				Transform StiltGhostPrefab = this._stiltPrefab;
				this._logPrefabs[0] = Prefabs.Instance.LogStairsBuiltPrefab;
				this._stiltPrefab = Prefabs.Instance.LogStiltStairsGhostBuiltPrefab;
				this._dockRoot = this.SpawnStructure();
				this._dockRoot.name = "DockRootBuilt";
				this._logPool = null;
				this._newPool = null;
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.<>f__this._logItemId);
				List<GameObject> logStacks = new List<GameObject>();
				foreach (object obj in this._dockRoot)
				{
					Transform edge = (Transform)obj;
					foreach (object obj2 in edge)
					{
						Transform logStack = (Transform)obj2;
						logStack.gameObject.SetActive(false);
						logStacks.Add(logStack.gameObject);
					}
				}
				ri._renderers = logStacks.ToArray();
				ri._amount += logStacks.Count;
				this._logPrefabs[0] = logGhostPrefab;
				this._stiltPrefab = StiltGhostPrefab;
				ghostRoot.transform.parent = null;
				base.transform.position = this._multiPointsPositions.First<Vector3>();
				base.transform.LookAt(this._multiPointsPositions.Last<Vector3>());
				this._dockRoot.transform.parent = base.transform;
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
				for (int j = 1; j < this._multiPointsPositions.Count; j++)
				{
					Vector3 p = this._multiPointsPositions[j];
					Vector3 axis = (p - this._multiPointsPositions[j - 1]).normalized;
					Vector3 pAxis = Vector3.Cross(Vector3.up, axis) * 3.5f;
					b.Encapsulate(base.transform.InverseTransformPoint(p + pAxis));
					b.Encapsulate(base.transform.InverseTransformPoint(p - pAxis));
				}
				bc.size = new Vector3(b.size.x, 6f, b.size.z);
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
			}
			yield break;
		}

		
		private void OnBuilt(GameObject built)
		{
			DockArchitect component = built.GetComponent<DockArchitect>();
			component._multiPointsPositions = this._multiPointsPositions;
			component._wasBuilt = true;
			component.OnSerializing();
		}

		
		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				if (this._dockRoot)
				{
					UnityEngine.Object.Destroy(this._dockRoot.gameObject);
				}
				this._dockRoot = null;
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this._dockRoot = this.SpawnStructure();
			if (this._wasBuilt && isRepair)
			{
				this._dockRoot.parent = base.transform;
			}
		}

		
		private bool ArePlayersInside()
		{
			if (LocalPlayer.Transform)
			{
				for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
				{
					Transform transform = Scene.SceneTracker.allPlayers[i].transform;
					if (transform.position.y - 2.5f < this._multiPointsPositions[0].y)
					{
						float num = MathEx.ClosestDistanceToPolygon(transform.position, this._multiPointsPositions);
						if (num < 5.25f)
						{
							if (Vector3.Distance(this._multiPointsPositions[0], this._multiPointsPositions[this._multiPointsPositions.Count - 1]) >= 1f)
							{
								float a = Vector3.Distance(this._multiPointsPositions[0], transform.position);
								bool flag = Mathf.Approximately(a, num);
								if (flag)
								{
									return MathEx.PointOnWhichSideOfLineSegment(this._multiPointsPositions[0], this._multiPointsPositions[1], transform.position) == 0;
								}
								float a2 = Vector3.Distance(this._multiPointsPositions[this._multiPointsPositions.Count - 1], transform.position);
								bool flag2 = Mathf.Approximately(a2, num);
								if (flag2)
								{
									return MathEx.PointOnWhichSideOfLineSegment(this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], transform.position) == 0;
								}
							}
							return true;
						}
					}
				}
			}
			return false;
		}

		
		private void CheckWaterLevel()
		{
			Vector3 position = base.transform.parent.position;
			position.y += 10f;
			RaycastHit[] array = Physics.SphereCastAll(position, 0.2f, Vector3.down, 20f, this._buildingCollisionLayers);
			if (array.Length == 0)
			{
				this._waterLevel = float.MaxValue;
			}
			else
			{
				float num = float.MinValue;
				float num2 = float.MinValue;
				for (int i = 0; i < array.Length; i++)
				{
					float y = array[i].point.y;
					if (array[i].collider.gameObject.CompareTag("Water"))
					{
						if (y > num2)
						{
							num2 = y;
						}
					}
					else if (y > num)
					{
						num = y;
					}
				}
				if (num2 + this.groundToWaterMaxDistance > num)
				{
					this._waterLevel = Mathf.Max(num2, num);
				}
				else
				{
					this._waterLevel = float.MaxValue;
				}
			}
		}

		
		private void CheckLockPoint()
		{
			bool flag = this._multiPointsPositions.Count == 0;
			bool flag2 = false;
			if (!flag && this._multiPointsPositions.Count < this._maxPoints)
			{
				Vector3 vector = base.transform.position - this._multiPointsPositions.Last<Vector3>();
				Vector3 vector2 = vector;
				vector2.y = 0f;
				float magnitude = vector.magnitude;
				vector2.y = 0f;
				flag = (magnitude > this._logLength);
				if (flag && this._multiPointsPositions.Count > 1)
				{
					Vector3 from = this._multiPointsPositions.Last<Vector3>() - this._multiPointsPositions[this._multiPointsPositions.Count - 2];
					float num = Vector3.Angle(from, vector);
					flag = (num < this._maxAngleBetweenEdges);
				}
				flag2 = (magnitude >= this._maxEdgeLength);
			}
			if ((flag && TheForest.Utils.Input.GetButtonDown("Fire1")) || (this._multiPointsPositions.Count >= 1 && TheForest.Utils.Input.GetButtonDown("Build")))
			{
				Vector3 vector3 = base.transform.position;
				vector3.y = this._waterLevel + 2f;
				Vector3 vector4 = (this._multiPointsPositions.Count <= 0) ? vector3 : this._multiPointsPositions.Last<Vector3>();
				if (flag2)
				{
					vector3 = vector4 + (vector3 - vector4).normalized * this._maxEdgeLength;
				}
				if (base.GetComponent<Renderer>().enabled)
				{
					base.GetComponent<Renderer>().enabled = false;
				}
				this._multiPointsPositions.Add(vector3);
			}
			if (this._multiPointsPositions.Count == 1 && !flag)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = false;
			}
			bool flag3 = this._multiPointsPositions.Count == 0;
			bool canLock = flag && this._multiPointsPositions.Count > 0;
			bool canUnlock = !flag3;
			bool showManualPlace = this._multiPointsPositions.Count > 0 && (flag || this._multiPointsPositions.Count > 1);
			Scene.HudGui.RotateIcon.SetActive(this._multiPointsPositions.Count == 0);
			Scene.HudGui.RoofConstructionIcons.Show(flag3, false, false, showManualPlace, canLock, canUnlock, false);
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

		
		public Transform SpawnStructure()
		{
			Transform transform = new GameObject("DockRoot").transform;
			transform.position = this._multiPointsPositions.First<Vector3>();
			Stack<Transform> stack = new Stack<Transform>();
			if (this._logPool == null)
			{
				this._logPool = new Stack<Transform>();
			}
			for (int i = 1; i < this._multiPointsPositions.Count; i++)
			{
				Vector3 vector = this._multiPointsPositions[i - 1];
				Vector3 a = this._multiPointsPositions[i];
				Vector3 vector2 = vector;
				Vector3 forward = a - vector;
				Vector3 rhs = new Vector3(forward.x, 0f, forward.z);
				Vector3 forward2 = Vector3.Cross(Vector3.up, rhs);
				int num = Mathf.CeilToInt(rhs.magnitude / this._logWidth);
				Vector3 b = rhs.normalized * this._logWidth;
				Quaternion rotation = Quaternion.LookRotation(forward2);
				if (i % 2 == 1)
				{
					vector2.y += 0.2f;
				}
				Transform transform2 = new GameObject("DockEdge" + i).transform;
				transform2.parent = transform;
				transform2.rotation = Quaternion.LookRotation(forward);
				transform2.position = vector2;
				Vector3 b2 = transform2.right * (this._logLength / 2f + this._logWidth * 0.3f);
				for (int j = 0; j < num; j++)
				{
					Transform transform3 = this.NewLog(vector2, rotation);
					transform3.parent = transform2;
					stack.Push(transform3);
					bool flag = j + 1 == num;
					if (j % 4 == 0 || flag)
					{
						bool flag2 = false;
						bool flag3 = false;
						if (flag && i + 1 < this._multiPointsPositions.Count)
						{
							if (Vector3.Cross(this._multiPointsPositions[i - 1] - this._multiPointsPositions[i], this._multiPointsPositions[i + 1] - this._multiPointsPositions[i]).y > 0f)
							{
								flag2 = true;
							}
							else
							{
								flag3 = true;
							}
							Vector3 normalized = ((this._multiPointsPositions[i - 1] - this._multiPointsPositions[i]).normalized + (this._multiPointsPositions[i + 1] - this._multiPointsPositions[i]).normalized).normalized;
							Transform transform4 = (Transform)UnityEngine.Object.Instantiate(this._stiltPrefab, vector2 + normalized * (this._logLength / 2f + this._logWidth * 0.3f) + Vector3.up, Quaternion.identity);
							transform4.parent = transform2;
							transform4.localScale = new Vector3(1f, (Mathf.Abs(vector2.y - this.GetPointFloorPosition(vector2).y) + 1f) / 4.5f, 1f);
							if (!this._wasBuilt && !this._wasPlaced)
							{
								transform4.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
							}
						}
						if (i > 1 && j == 0)
						{
							if (Vector3.Cross(this._multiPointsPositions[i] - this._multiPointsPositions[i - 1], this._multiPointsPositions[i - 2] - this._multiPointsPositions[i - 1]).y < 0f)
							{
								flag2 = true;
							}
							else
							{
								flag3 = true;
							}
						}
						if (!flag3)
						{
							Transform transform4 = (Transform)UnityEngine.Object.Instantiate(this._stiltPrefab, vector2 + b2 + Vector3.up, Quaternion.identity);
							transform4.parent = transform2;
							transform4.localScale = new Vector3(1f, (Mathf.Abs(vector2.y - this.GetPointFloorPosition(vector2).y) + 1f) / 4.5f, 1f);
							if (!this._wasBuilt && !this._wasPlaced)
							{
								transform4.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
							}
						}
						if (!flag2)
						{
							Transform transform4 = (Transform)UnityEngine.Object.Instantiate(this._stiltPrefab, vector2 - b2 + Vector3.up, Quaternion.identity);
							transform4.parent = transform2;
							transform4.localScale = new Vector3(1f, (Mathf.Abs(vector2.y - this.GetPointFloorPosition(vector2).y) + 1f) / 4.5f, 1f);
							if (!this._wasBuilt && !this._wasPlaced)
							{
								transform4.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
							}
						}
					}
					vector2 += b;
				}
				if (this._wasBuilt)
				{
					BoxCollider boxCollider = transform2.gameObject.AddComponent<BoxCollider>();
					boxCollider.size = new Vector3(this._logLength, this._logHeight, forward.magnitude);
					boxCollider.center = new Vector3(0f, -0.34f, boxCollider.size.z / 2f);
					transform2.tag = "UnderfootWood";
					transform2.gameObject.AddComponent<BuildingHealthHitRelay>();
					transform2.gameObject.layer = 21;
					transform2.gameObject.AddComponent<gridObjectBlocker>();
				}
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
			Transform transform2 = (Transform)UnityEngine.Object.Instantiate(this._logPrefabs[UnityEngine.Random.Range(0, this._logPrefabs.Length)], position, rotation);
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

		
		public void OnTargetTagTrigerEnter(Collider other)
		{
			this._waterLevel = other.bounds.max.y;
			this._logMat.SetColor("_TintColor", LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
		}

		
		public void OnTargetTagTrigerExit(Collider other)
		{
			this._waterLevel = float.MaxValue;
			this._logMat.SetColor("_TintColor", LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor"));
			Scene.HudGui.RoofConstructionIcons.Shutdown();
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
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

		
		public float _maxAngleBetweenEdges = 60f;

		
		public float groundToWaterMaxDistance = 1.3f;

		
		public int _maxPoints = 50;

		
		public LayerMask _floorLayers;

		
		public LayerMask _buildingCollisionLayers;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		
		private bool _initialized;

		
		[SerializeThis]
		private List<Vector3> _multiPointsPositions;

		
		[SerializeThis]
		private int _multiPointsPositionsCount;

		
		private Vector3 _placerOffset;

		
		private Transform _dockRoot;

		
		private float _logLength;

		
		private float _logWidth;

		
		private float _logHeight;

		
		private float _maxChunkLength;

		
		private float _minChunkLength;

		
		private float _waterLevel = float.MaxValue;

		
		private Stack<Transform> _logPool;

		
		private Stack<Transform> _newPool;

		
		private Material _logMat;
	}
}
