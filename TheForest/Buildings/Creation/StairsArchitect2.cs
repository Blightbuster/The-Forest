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
	public class StairsArchitect2 : MonoBehaviour, ICoopStructure, IProceduralStructure
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
					Transform stairsRoot = this.SpawnStructure();
					this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					if (this._stairsRoot)
					{
						UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
					}
					this._stairsRoot = stairsRoot;
				}
			}
			else
			{
				this._multiPointsPositions.Add(base.transform.position);
				this._multiPointsPositions.Add(base.transform.position + base.transform.forward * (this._logWidth * 0.8f));
				Transform stairsRoot2 = this.SpawnStructure();
				this._multiPointsPositions.RemoveAt(0);
				this._multiPointsPositions.RemoveAt(0);
				if (this._stairsRoot)
				{
					UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
				}
				this._stairsRoot = stairsRoot2;
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
				Transform newStairs = this.SpawnStructure();
				if (this._stairsRoot)
				{
					UnityEngine.Object.Destroy(this._stairsRoot.gameObject);
				}
				this._stairsRoot = newStairs;
				Transform ghostRoot = this._stairsRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				this._logPool = null;
				this._newPool = null;
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.$this._logItemId);
				int amount = 0;
				List<GameObject> stairsLogs = new List<GameObject>();
				List<GameObject> stiltsLogs = new List<GameObject>();
				Material stiltMat = Prefabs.Instance.LogStiltStairsGhostBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
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
								amount++;
								IEnumerator enumerator3 = transform3.GetEnumerator();
								try
								{
									while (enumerator3.MoveNext())
									{
										object obj3 = enumerator3.Current;
										Transform transform4 = (Transform)obj3;
										Renderer componentInChildren = transform4.GetComponentInChildren<Renderer>();
										if (componentInChildren.sharedMaterial == stiltMat)
										{
											stiltsLogs.Add(componentInChildren.gameObject);
										}
										else
										{
											stairsLogs.Add(componentInChildren.gameObject);
										}
									}
								}
								finally
								{
									IDisposable disposable;
									if ((disposable = (enumerator3 as IDisposable)) != null)
									{
										disposable.Dispose();
									}
								}
							}
						}
						finally
						{
							IDisposable disposable2;
							if ((disposable2 = (enumerator2 as IDisposable)) != null)
							{
								disposable2.Dispose();
							}
						}
					}
				}
				finally
				{
					IDisposable disposable3;
					if ((disposable3 = (enumerator as IDisposable)) != null)
					{
						disposable3.Dispose();
					}
				}
				ri.AddRuntimeObjects(stiltsLogs.AsEnumerable<GameObject>().Reverse<GameObject>(), Prefabs.Instance.LogStiltStairsGhostBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				ri.AddRuntimeObjects(stairsLogs.AsEnumerable<GameObject>().Reverse<GameObject>(), Prefabs.Instance.LogStairsBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				ri._amount += amount;
				ghostRoot.transform.parent = null;
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
					getStructureStrength._type = getStructureStrength.structureType.floor;
				}
			}
			yield break;
		}

		
		private void OnBuilt(GameObject built)
		{
			StairsArchitect2 component = built.GetComponent<StairsArchitect2>();
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
			this._stairsRoot = this.SpawnStructure();
			if (this._wasBuilt && isRepair)
			{
				this._stairsRoot.parent = base.transform;
			}
		}

		
		private float ClosestDistanceToStairs(Vector3 point, List<Vector3> polygon)
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
						float num3 = this.ClosestDistanceToStairs(point, this._multiPointsPositions);
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
				flag2 = (magnitude > this._logWidth * 1f && num * 1.1f >= Mathf.Abs(vector2.y));
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
					transform.GetComponentInChildren<Renderer>().sharedMaterial = Create.CurrentGhostMat;
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
			Transform transform = new GameObject("StairsRoot").transform;
			transform.position = this.GetWorldPoint(0);
			Stack<Transform> stack = new Stack<Transform>();
			if (this._logPool == null)
			{
				this._logPool = new Stack<Transform>();
			}
			bool flag = true;
			int i = 1;
			int num = 1;
			int num2 = -1;
			while (i < this._multiPointsPositions.Count)
			{
				Vector3 worldPoint = this.GetWorldPoint(i);
				Vector3 worldPoint2 = this.GetWorldPoint(i - num);
				bool flag2 = worldPoint.y > worldPoint2.y;
				int num3 = -1;
				Vector3 vector;
				Vector3 a;
				if (flag2 || num2 - (i - num) == 0)
				{
					vector = worldPoint;
					a = worldPoint2;
					num2 = i;
				}
				else
				{
					vector = worldPoint2;
					a = worldPoint;
					num2 = i - num;
					num3 = ((i + num >= this._multiPointsPositions.Count) ? -1 : ((worldPoint.y <= this.GetWorldPoint(i + num).y) ? (i + num) : i));
				}
				Vector3 forward = a - vector;
				Vector3 rhs = new Vector3(forward.x, 0f, forward.z);
				Vector3 vector2 = Vector3.Cross(Vector3.up, rhs);
				int num4 = Mathf.CeilToInt(rhs.magnitude / this._logWidth);
				Vector3 vector3 = rhs.normalized * this._logWidth;
				Quaternion rotation = Quaternion.LookRotation(vector2);
				bool flag3 = i + num == this._multiPointsPositions.Count;
				bool flag4 = flag;
				flag = (flag3 && rhs.magnitude < this._logLength);
				Transform transform2 = new GameObject("StairEdge" + i).transform;
				transform2.parent = transform;
				float d = (!flag4) ? 2.5f : 0f;
				float d2 = (!flag) ? 2.5f : 0.5f;
				Vector3 vector4 = vector;
				forward = a - vector3 * d - (vector4 + vector3 * d2);
				transform2.rotation = Quaternion.LookRotation(forward);
				transform2.position = vector4 + vector3 * d2;
				this.SpawnVerticalStilt(vector, transform2);
				if (!flag)
				{
					vector4 -= vector3 * 2.5f;
					for (int j = 0; j < 5; j++)
					{
						Debug.DrawLine(vector4 - vector2, vector4 + vector2, Color.cyan);
						Transform transform3 = this.NewLog(vector4, rotation);
						transform3.parent = transform2;
						stack.Push(transform3);
						vector4 += vector3;
						if (this._wasBuilt && j == 2)
						{
							transform3.rotation = rotation;
							transform3.tag = "UnderfootWood";
							BoxCollider boxCollider = transform3.gameObject.AddComponent<BoxCollider>();
							boxCollider.size = new Vector3(this._logWidth * 5.5f, this._logHeight, this._logLength);
							boxCollider.center = new Vector3(-0.42f, -0.34f, 0f);
							transform3.gameObject.AddComponent<gridObjectBlocker>();
							transform3.gameObject.AddComponent<BuildingHealthHitRelay>();
						}
					}
					num4 -= 3;
				}
				if (!flag4)
				{
					num4 -= 3;
				}
				Transform transform4 = UnityEngine.Object.Instantiate<Transform>(this._stiltPrefab);
				transform4.parent = transform2;
				transform4.localPosition = new Vector3(0f, -0.75f, 0f);
				transform4.localEulerAngles = new Vector3(-90f, 0f, 0f);
				transform4.localScale = new Vector3(1f, Mathf.Abs(forward.magnitude) / 4.68f, 1f);
				if (!this._wasBuilt && !this._wasPlaced)
				{
					transform4.GetComponentInChildren<Renderer>().sharedMaterial = Create.CurrentGhostMat;
				}
				else
				{
					transform4.gameObject.SetActive(true);
				}
				Vector3 b = new Vector3(0f, forward.y / (float)num4, 0f);
				for (int k = 0; k < num4; k++)
				{
					Debug.DrawLine(vector4 - vector2, vector4 + vector2, Color.cyan);
					Transform transform5 = this.NewLog(vector4, rotation);
					transform5.parent = transform2;
					stack.Push(transform5);
					vector4 += vector3;
					vector4 += b;
				}
				if (num3 > i)
				{
					vector4 += vector3 * -0.65f;
					for (int l = 0; l < 5; l++)
					{
						Debug.DrawLine(vector4 - vector2, vector4 + vector2, Color.cyan);
						Transform transform6 = this.NewLog(vector4, rotation);
						transform6.parent = transform2;
						stack.Push(transform6);
						if (l == 2)
						{
							this.SpawnVerticalStilt(vector4, transform2);
							if (this._wasBuilt)
							{
								transform6.rotation = rotation;
								transform6.tag = "UnderfootWood";
								BoxCollider boxCollider2 = transform6.gameObject.AddComponent<BoxCollider>();
								boxCollider2.size = new Vector3(this._logWidth * 5.5f, this._logHeight, this._logLength);
								boxCollider2.center = new Vector3(-0.42f, -0.34f, 0f);
								transform6.gameObject.AddComponent<gridObjectBlocker>();
							}
						}
						vector4 += vector3;
					}
				}
				if (this._wasBuilt)
				{
					BoxCollider boxCollider3 = transform2.gameObject.AddComponent<BoxCollider>();
					boxCollider3.size = new Vector3(this._logLength, this._logHeight, forward.magnitude);
					boxCollider3.center = new Vector3(0f, -0.34f, boxCollider3.size.z / 2f);
					transform2.tag = "UnderfootWood";
					transform2.gameObject.AddComponent<BuildingHealthHitRelay>();
					transform2.gameObject.layer = 21;
					getStructureStrength getStructureStrength = transform2.gameObject.AddComponent<getStructureStrength>();
					getStructureStrength._strength = getStructureStrength.strength.normal;
					getStructureStrength._type = getStructureStrength.structureType.floor;
					transform2.gameObject.AddComponent<gridObjectBlocker>();
					FoundationArchitect.CreateWindSFX(transform4);
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
					transform.GetComponentInChildren<Renderer>().sharedMaterial = Create.CurrentGhostMat;
				}
				return transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefabs[UnityEngine.Random.Range(0, this._logPrefabs.Length)], position, rotation);
			if (!this._wasBuilt && !this._wasPlaced)
			{
				transform2.GetComponentInChildren<Renderer>().sharedMaterial = Create.CurrentGhostMat;
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
	}
}
