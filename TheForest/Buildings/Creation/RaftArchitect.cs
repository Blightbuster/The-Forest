using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Items;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/Creation/Raft Architect")]
	public class RaftArchitect : EntityBehaviour, IHoleStructure, IStructureSupport, ICoopStructure
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
			this._raftRoot = new GameObject("RaftRoot").transform;
			this._raftRoot.position = base.transform.position;
			this._raftRoot.rotation = base.transform.rotation;
			if (this._multiPointsPositions == null)
			{
				this._multiPointsPositions = new List<Vector3>(5);
			}
			if (this._holes == null)
			{
				this._holes = new List<Hole>();
			}
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
			this._maxChunkLength = this._logLength * this._maxLogScale;
			this._minChunkLength = this._logLength * this._minLogScale;
			yield return null;
			if (this._wasBuilt && !isDeserializing)
			{
				base.StartCoroutine(this.OnDeserialized());
				if (!BoltNetwork.isClient && LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, true);
				}
			}
			else if (this._craftStructure)
			{
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
				base.GetComponent<Renderer>().sharedMaterial = this._logMat;
				base.enabled = true;
			}
			yield break;
		}

		
		private void Update()
		{
			bool flag = this._multiPointsPositions.Count >= 3;
			bool flag2 = false;
			if (TheForest.Utils.Input.GetButtonDown("AltFire") && this._multiPointsPositions.Count > 0)
			{
				if (this._multiPointsPositions.Count == 0)
				{
					if (this._raftRoot)
					{
						UnityEngine.Object.Destroy(this._raftRoot.gameObject);
						this._raftRoot = null;
					}
					this._newPool.Clear();
					this._logPool.Clear();
					base.GetComponent<Renderer>().enabled = true;
				}
				this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
			}
			bool flag3 = this._multiPointsPositions.Count < this._maxPoints && LocalPlayer.Create.BuildingPlacer.Clear;
			if (flag3 && this._multiPointsPositions.Count > 0)
			{
				Vector3 vector = base.transform.position - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
				flag3 = (vector.sqrMagnitude > this._minChunkLength * this._minChunkLength);
				if (this._multiPointsPositions.Count > 1)
				{
					Vector3 vector2 = this._multiPointsPositions[this._multiPointsPositions.Count - 2] - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
					flag3 = (flag3 && Vector3.Angle(vector2.normalized, vector.normalized) >= this._minAngleBetweenEdges);
				}
			}
			if (flag3)
			{
				Vector3 vector3 = this.GetCurrentEdgePoint();
				bool flag4 = this._multiPointsPositions.Count > 2 && Vector3.Distance(vector3, this._multiPointsPositions[0]) < this._closureSnappingDistance;
				if (flag4)
				{
					vector3 = this._multiPointsPositions[0];
				}
				if (TheForest.Utils.Input.GetButtonDown("Fire1"))
				{
					this._multiPointsPositions.Add(vector3);
					if (this._multiPointsPositions.Count > 1)
					{
						base.GetComponent<Renderer>().enabled = false;
					}
					flag2 = flag4;
				}
			}
			bool flag5 = this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions.First<Vector3>(), this._multiPointsPositions.Last<Vector3>()) > this._minChunkLength;
			if (flag5 && (TheForest.Utils.Input.GetButtonDown("Rotate") || TheForest.Utils.Input.GetButtonDown("Build")))
			{
				this._multiPointsPositions.Add(this._multiPointsPositions.First<Vector3>());
				this.RefreshCurrentFloor();
				flag2 = true;
			}
			if (this._multiPointsPositions.Count > 0)
			{
				bool flag6 = false;
				bool flag7 = this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions[0], this._multiPointsPositions[this._multiPointsPositions.Count - 1]) < this._closureSnappingDistance;
				if (!flag7)
				{
					if (Vector3.Distance(base.transform.position, this._multiPointsPositions[0]) > this._closureSnappingDistance)
					{
						flag6 = true;
						this._multiPointsPositions.Add(this.GetCurrentEdgePoint());
					}
					if (this._multiPointsPositions.Count > 2 || !flag6)
					{
						this._multiPointsPositions.Add(this._multiPointsPositions[0]);
					}
				}
				this.RefreshCurrentFloor();
				if (!flag7)
				{
					if (this._multiPointsPositions.Count > 2 || !flag6)
					{
						this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					}
					if (flag6)
					{
						this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					}
				}
			}
			if (flag2)
			{
				base.enabled = false;
				base.GetComponent<Renderer>().enabled = false;
				LocalPlayer.Create.PlaceGhost(false);
			}
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag;
				if (base.GetComponent<Renderer>().enabled)
				{
					base.GetComponent<Renderer>().sharedMaterial = this._logMat;
				}
			}
			this._logMat.SetColor("_TintColor", (!flag3 && this._multiPointsPositions.Count <= 0) ? LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor") : LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
			Scene.HudGui.RoofConstructionIcons.Show(this._multiPointsPositions.Count == 0, false, false, flag, flag3, this._multiPointsPositions.Count > 0, false);
		}

		
		private void OnDestroy()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.RoofConstructionIcons.Shutdown();
			}
			if (!this._wasPlaced && LocalPlayer.Create)
			{
				LocalPlayer.Create.Grabber.ClosePlace();
			}
			this.Clear();
		}

		
		private void OnSerializing()
		{
			this._multiPointsPositions.Capacity = this._multiPointsPositions.Count;
			this._multiPointsPositionsCount = this._multiPointsPositions.Count;
			this._holes.Capacity = this._holes.Count;
			this._holesCount = this._holes.Count;
		}

		
		private IEnumerator OnDeserialized()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				this._multiPointsPositions.RemoveRange(this._multiPointsPositionsCount, this._multiPointsPositions.Count - this._multiPointsPositionsCount);
				this._holes.RemoveRange(this._holesCount, this._holes.Count - this._holesCount);
				if (Vector3.Distance(this._multiPointsPositions.First<Vector3>(), this._multiPointsPositions.Last<Vector3>()) > this._closureSnappingDistance)
				{
					this._multiPointsPositions.Add(this._multiPointsPositions[0]);
				}
				if (this._wasBuilt)
				{
					this.CreateStructure(false);
					this._rowPointsBuffer = null;
					this._holesRowPointsBuffer = null;
					this._raftRoot.transform.parent = base.transform;
					yield return null;
					if (this._voxelize)
					{
						this._buoyancy.Voxelize(this._raftRoot, -0.15f);
					}
					Rigidbody rb = base.GetComponent<Rigidbody>();
					if (rb)
					{
						rb.isKinematic = false;
					}
				}
				else if (this._wasPlaced)
				{
					this.CreateStructure(false);
					base.StartCoroutine(this.OnPlaced());
				}
				this._logPool = null;
				this._newPool = null;
				this._rowPointsBuffer = null;
				this._holesRowPointsBuffer = null;
			}
			yield break;
		}

		
		private IEnumerator OnPlaced()
		{
			if (Vector3.Distance(this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[0]) >= this._closureSnappingDistance)
			{
				this._multiPointsPositions.Add(this._multiPointsPositions[0]);
				this.RefreshCurrentFloor();
			}
			base.enabled = false;
			this._wasPlaced = true;
			yield return null;
			if (!this._craftStructure)
			{
				this._craftStructure = base.GetComponentInChildren<Craft_Structure>();
			}
			UnityEngine.Object.Destroy(base.GetComponent<Collider>());
			UnityEngine.Object.Destroy(base.GetComponent<Rigidbody>());
			this._craftStructure.GetComponent<Collider>().enabled = false;
			if (Scene.HudGui)
			{
				Scene.HudGui.RoofConstructionIcons.Shutdown();
			}
			if (this._craftStructure)
			{
				Transform ghostRoot = this._raftRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				Transform logGhostPrefab = this._logPrefab;
				this._logPrefab = Prefabs.Instance.LogFloorBuiltPrefab;
				this._logPool = new Stack<Transform>();
				this._newPool = new Stack<Transform>();
				this._raftRoot = new GameObject("RaftRootBuilt").transform;
				this._raftRoot.position = this._multiPointsPositions[0];
				this.SpawnFloor();
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.<>f__this._logItemId);
				List<GameObject> logStacks = new List<GameObject>();
				foreach (object obj in this._raftRoot)
				{
					Transform logStack = (Transform)obj;
					logStack.gameObject.SetActive(false);
					logStacks.Add(logStack.gameObject);
				}
				if (ri._renderers != null)
				{
					ri._renderers = logStacks.Union(ri._renderers).ToArray<GameObject>();
				}
				else
				{
					ri._renderers = logStacks.ToArray();
				}
				ri._amount += this._raftRoot.childCount;
				this._logPrefab = logGhostPrefab;
				this._raftRoot.transform.parent = base.transform;
				ghostRoot.transform.parent = base.transform;
				this._craftStructure.GetComponent<Collider>().enabled = true;
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
				Bounds b = new Bounds(base.transform.position, Vector3.zero);
				for (int j = 1; j < this._multiPointsPositions.Count; j++)
				{
					b.Encapsulate(this._multiPointsPositions[j]);
				}
				Vector3 bottom = base.transform.position;
				bottom.y -= 5f;
				b.Encapsulate(bottom);
				bc.center = base.transform.InverseTransformPoint(b.center);
				Vector3 finalColliderSize = base.transform.InverseTransformVector(b.size);
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
				base.GetComponent<Renderer>().enabled = false;
				yield return null;
				if (bc != null)
				{
					bc.enabled = false;
					bc.enabled = true;
				}
				this._logPool = null;
				this._newPool = null;
				this._rowPointsBuffer = null;
			}
			yield break;
		}

		
		public void OnBuilt(GameObject built)
		{
			RaftArchitect component = built.GetComponent<RaftArchitect>();
			Vector3 centerPosition = this._multiPointsPositions.GetCenterPosition();
			built.transform.position = ((!this._wasBuilt) ? centerPosition : base.transform.TransformPoint(centerPosition));
			built.transform.rotation = this._raftRoot.rotation;
			component._multiPointsPositions = ((!this._wasBuilt) ? (from p in this._multiPointsPositions
			select built.transform.InverseTransformPoint(p)).ToList<Vector3>() : new List<Vector3>(this._multiPointsPositions));
			component._holes = this._holes;
			component._wasBuilt = true;
			component.OnSerializing();
		}

		
		public void Clear()
		{
			this._rowPointsBuffer = null;
			this._holesRowPointsBuffer = null;
			if (this._raftRoot)
			{
				UnityEngine.Object.Destroy(this._raftRoot.gameObject);
			}
		}

		
		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				this.Clear();
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this.RefreshCurrentFloor();
			if (this._wasBuilt)
			{
				this._raftRoot.parent = base.transform;
			}
		}

		
		public Hole AddSquareHole(Vector3 position, float yRotation, Vector2 size)
		{
			Hole hole = new Hole
			{
				_position = position,
				_yRotation = yRotation,
				_size = size,
				_used = true
			};
			this._holes.Add(hole);
			this._holesCount = this._holes.Count;
			return hole;
		}

		
		public void RemoveHole(Hole hole)
		{
			this._holes.Remove(hole);
			this._holesCount = this._holes.Count;
		}

		
		public void OnHolePlaced()
		{
			for (int i = this._holes.Count - 1; i >= 0; i--)
			{
				if (!this._holes[i]._used)
				{
					this._holes.RemoveAt(i);
				}
			}
			this.UpdateToken();
		}

		
		public void RefreshCurrentFloor()
		{
			if (this._logPool == null)
			{
				this._logPool = new Stack<Transform>();
			}
			this._newPool = new Stack<Transform>();
			Transform raftRoot = this._raftRoot;
			if (this._multiPointsPositions.Count >= 3)
			{
				this._raftRoot = new GameObject("RaftRoot").transform;
				this._raftRoot.position = ((!this._wasBuilt) ? this._multiPointsPositions[0] : base.transform.TransformPoint(this._multiPointsPositions[0]));
				this.SpawnFloor();
			}
			if (raftRoot)
			{
				UnityEngine.Object.Destroy(raftRoot.gameObject);
			}
			this._logPool = this._newPool;
		}

		
		private Vector3 GetCurrentEdgePoint()
		{
			Vector3 position = base.transform.position;
			if (this._multiPointsPositions.Count > 0)
			{
				position.y = this._multiPointsPositions[0].y;
			}
			return position;
		}

		
		private Vector3 GetSegmentPointFloorPosition(Vector3 segmentPoint)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(segmentPoint, Vector3.down, out raycastHit, 500f, Scene.ValidateFloorLayers(segmentPoint, this._floorLayers.value)))
			{
				return raycastHit.point;
			}
			segmentPoint.y -= this._logLength / 2f;
			return segmentPoint;
		}

		
		private void InitRowPointsBuffer(int rowCount, List<Vector3>[] buffer, out List<Vector3>[] outBuffer)
		{
			if (buffer != null && buffer.Length >= rowCount)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					if (i > rowCount && buffer[i].Count == 0)
					{
						break;
					}
					buffer[i].Clear();
				}
			}
			else
			{
				if (buffer != null)
				{
					rowCount = Mathf.Max(new int[]
					{
						15,
						rowCount,
						buffer.Length * 2
					});
				}
				buffer = new List<Vector3>[rowCount];
				for (int j = 0; j < rowCount; j++)
				{
					buffer[j] = new List<Vector3>(6);
				}
			}
			outBuffer = buffer;
		}

		
		private int GetLowerRowIndex(float zPosition, float minZ)
		{
			return Mathf.FloorToInt(Mathf.Abs(zPosition - minZ) / this._logWidth);
		}

		
		private int GetUpperRowIndex(float zPosition, float minZ)
		{
			return Mathf.CeilToInt(Mathf.Abs(zPosition - minZ) / this._logWidth);
		}

		
		private void CalcRowPointBufferForArray(Vector3[] localPoints, float minLocalZ, List<Vector3>[] rowPointsBuffer)
		{
			for (int i = 1; i < localPoints.Length; i++)
			{
				Vector3 vector = localPoints[i] - localPoints[i - 1];
				Vector3 item = localPoints[i - 1];
				int num = (vector.z <= 0f) ? this.GetLowerRowIndex(item.z, minLocalZ) : this.GetUpperRowIndex(item.z, minLocalZ);
				int num2 = 0;
				float num3;
				float num4;
				float num5;
				if (vector.z > 0f)
				{
					num3 = Vector3.Angle(Vector3.forward, vector) * 0.0174532924f;
					if (Vector3.Cross(Vector3.forward, vector).y < 0f)
					{
						num3 *= -1f;
					}
					num4 = this._logWidth;
					num5 = (float)num * num4 + minLocalZ - item.z;
				}
				else
				{
					num3 = Vector3.Angle(Vector3.back, vector) * 0.0174532924f;
					if (Vector3.Cross(Vector3.back, vector).y < 0f)
					{
						num3 *= -1f;
					}
					num4 = -this._logWidth;
					num5 = -((float)num * num4 - minLocalZ + item.z);
				}
				float num6 = Mathf.Tan(num3);
				float num7 = num6 * num4;
				float num8 = num6 * num5;
				num2 += Mathf.CeilToInt((Mathf.Abs(vector.z) - Mathf.Abs(num5)) / this._logWidth);
				item.z += num5;
				item.x += num8;
				for (int j = 0; j < num2; j++)
				{
					if (num >= 0 && num < rowPointsBuffer.Length)
					{
						rowPointsBuffer[num].Add(item);
					}
					num += ((vector.z <= 0f) ? -1 : 1);
					item.z += num4;
					item.x += num7;
				}
			}
		}

		
		private void SpawnFloor()
		{
			float num = float.MaxValue;
			float num2 = float.MinValue;
			Vector3[] array;
			if (this._wasBuilt)
			{
				this._raftRoot.rotation = base.transform.rotation;
				array = this._multiPointsPositions.ToArray();
			}
			else
			{
				this._raftRoot.rotation = Quaternion.LookRotation(this._multiPointsPositions[1] - this._multiPointsPositions[0]);
				array = (from p in this._multiPointsPositions
				select base.transform.InverseTransformPoint(p)).ToArray<Vector3>();
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].z < num)
				{
					num = array[i].z;
				}
				if (array[i].z > num2)
				{
					num2 = array[i].z;
				}
			}
			float num3 = Mathf.Abs(num2 - num);
			this._rowCount = Mathf.CeilToInt(num3 / this._logWidth);
			this.InitRowPointsBuffer(this._rowCount, this._rowPointsBuffer, out this._rowPointsBuffer);
			this.CalcRowPointBufferForArray(array, num, this._rowPointsBuffer);
			if (this._holes != null)
			{
				for (int j = 0; j < this._holes.Count; j++)
				{
					Vector3[] array2 = new Vector3[5];
					Hole hole = this._holes[j];
					hole._used = false;
					if (this._wasBuilt)
					{
						array2[0] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
						array2[1] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
						array2[2] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
						array2[3] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					}
					else
					{
						array2[0] = hole._position + base.transform.InverseTransformPoint(new Vector3(hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
						array2[1] = hole._position + base.transform.InverseTransformPoint(new Vector3(hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
						array2[2] = hole._position + base.transform.InverseTransformPoint(new Vector3(-hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
						array2[3] = hole._position + base.transform.InverseTransformPoint(new Vector3(-hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					}
					array2[4] = array2[0];
					for (int k = 0; k < 5; k++)
					{
						array2[k].y = array[0].y;
					}
					this.InitRowPointsBuffer(this._rowCount, this._holesRowPointsBuffer, out this._holesRowPointsBuffer);
					this.CalcRowPointBufferForArray(array2, num, this._holesRowPointsBuffer);
					for (int l = 0; l < this._rowCount; l++)
					{
						this._rowPointsBuffer[l].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						this._holesRowPointsBuffer[l].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						List<Vector3> list = this._rowPointsBuffer[l];
						List<Vector3> list2 = this._holesRowPointsBuffer[l];
						int num4 = list.Count;
						int count = list2.Count;
						for (int m = 1; m < num4; m += 2)
						{
							for (int n = 1; n < count; n += 2)
							{
								if (list[m - 1].x > list2[n - 1].x && list[m].x < list2[n].x)
								{
									list.RemoveAt(m);
									list.RemoveAt(m - 1);
									hole._used = true;
									if (n + 2 >= count)
									{
										if (m + 2 >= num4)
										{
											break;
										}
										m -= 2;
										num4 -= 2;
									}
								}
								else if (list[m - 1].x < list2[n - 1].x && list[m].x > list2[n].x)
								{
									list.Insert(m, list2[n - 1]);
									list.Insert(m + 1, list2[n]);
									hole._used = true;
								}
								else if (list[m - 1].x > list2[n - 1].x && list[m - 1].x < list2[n].x)
								{
									list[m - 1] = list2[n];
									hole._used = true;
								}
								else if (list[m].x > list2[n - 1].x && list[m].x < list2[n].x)
								{
									list[m] = list2[n - 1];
									hole._used = true;
								}
							}
						}
					}
				}
			}
			Transform transform = this._raftRoot;
			float num5 = 0f;
			float num6 = (this._maxScaleLogCost <= 0f) ? 0f : (this._maxLogScale / this._maxScaleLogCost);
			Vector3 one = Vector3.one;
			Quaternion rotation = Quaternion.LookRotation(base.transform.right);
			for (int num7 = 0; num7 < this._rowCount; num7++)
			{
				this._rowPointsBuffer[num7].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
				int count2 = this._rowPointsBuffer[num7].Count;
				for (int num8 = 1; num8 < count2; num8 += 2)
				{
					Vector3 vector = base.transform.TransformPoint(this._rowPointsBuffer[num7][num8 - 1]);
					Vector3 a = base.transform.TransformPoint(this._rowPointsBuffer[num7][num8]) - vector;
					Vector3 normalized = a.normalized;
					if (num8 - 1 == 0)
					{
						vector -= normalized;
					}
					if (num8 + 1 == count2)
					{
						a += normalized * 2f;
					}
					else
					{
						a += normalized;
					}
					float magnitude = a.magnitude;
					int num9 = Mathf.CeilToInt(magnitude / this._maxChunkLength);
					one.z = magnitude / (float)num9 / this._logLength;
					Vector3 b = a / (float)num9;
					for (int num10 = 0; num10 < num9; num10++)
					{
						Transform transform2 = this.NewLog(vector, rotation);
						transform2.parent = null;
						if (num5 > 0f)
						{
							num5 -= one.z;
							transform2.parent = transform;
							Vector3 localScale = one;
							localScale.z /= transform.localScale.z;
							transform2.localScale = localScale;
						}
						else
						{
							transform = transform2;
							num5 = num6 - one.z;
							transform2.parent = this._raftRoot;
							transform2.localScale = one;
						}
						this._newPool.Push(transform2);
						vector += b;
					}
				}
			}
		}

		
		private void SpawnFloorCollider()
		{
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
			Transform transform2 = (Transform)UnityEngine.Object.Instantiate(this._logPrefab, position, rotation);
			if (!this._wasBuilt)
			{
				if (!this._wasPlaced)
				{
					transform2.GetComponentInChildren<Renderer>().sharedMaterial = this._logMat;
				}
			}
			else
			{
				transform2.rotation *= this.RandomLogRotation();
			}
			return transform2;
		}

		
		private Quaternion RandomLogRotation()
		{
			return Quaternion.Euler(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-1.5f, 1.5f));
		}

		
		
		
		[SerializeThis]
		public bool Enslaved { get; set; }

		
		public float GetLevel()
		{
			return ((!this._wasBuilt) ? this._multiPointsPositions[0].y : (base.transform.position.y + this._multiPointsPositions[0].y)) + this.GetHeight();
		}

		
		public float GetHeight()
		{
			return this._logWidth * 0.4f;
		}

		
		public List<Vector3> GetMultiPointsPositions(bool inherit = true)
		{
			return this._multiPointsPositions;
		}

		
		
		public virtual Vector3 SupportCenter
		{
			get
			{
				Vector3 centerPosition = this._multiPointsPositions.GetCenterPosition();
				centerPosition.y = this.GetLevel();
				return centerPosition;
			}
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

		
		
		public float LogWidth
		{
			get
			{
				return this._logWidth;
			}
		}

		
		
		public int HoleCount
		{
			get
			{
				return this._holes.Count;
			}
		}

		
		public override void Attached()
		{
			if (!this.entity.isOwner)
			{
				IBuildingDestructibleState buildingDestructibleState;
				if (this.entity.TryFindState<IBuildingDestructibleState>(out buildingDestructibleState))
				{
					buildingDestructibleState.AddCallback("Token", new PropertyCallbackSimple(this.OnTokenUpdated));
				}
			}
			else
			{
				this.UpdateToken();
			}
		}

		
		public void UpdateToken()
		{
			BoltEntity component = base.GetComponent<BoltEntity>();
			IBuildingDestructibleState buildingDestructibleState;
			if (component && component.isAttached && component.isOwner && component.TryFindState<IBuildingDestructibleState>(out buildingDestructibleState))
			{
				buildingDestructibleState.Token = this.CustomToken;
			}
		}

		
		private void OnTokenUpdated()
		{
			IBuildingDestructibleState buildingDestructibleState;
			if (this.entity.TryFindState<IBuildingDestructibleState>(out buildingDestructibleState))
			{
				this.CustomToken = buildingDestructibleState.Token;
			}
		}

		
		
		
		public IProtocolToken CustomToken
		{
			get
			{
				CoopFloorToken coopFloorToken = new CoopFloorToken();
				if (this._holes != null)
				{
					coopFloorToken.Holes = this._holes.ToArray();
				}
				return coopFloorToken;
			}
			set
			{
				if (this._wasBuilt && (value as CoopFloorToken).Holes != null)
				{
					this._holes = (value as CoopFloorToken).Holes.ToList<Hole>();
					this.CreateStructure(false);
				}
			}
		}

		
		public Buoyancy _buoyancy;

		
		[SerializeThis]
		public bool _wasPlaced;

		
		[SerializeThis]
		public bool _wasBuilt;

		
		public Transform _logPrefab;

		
		public Renderer _logRenderer;

		
		public float _closureSnappingDistance = 2.5f;

		
		public float _minLogScale = 0.3f;

		
		public float _maxLogScale = 2f;

		
		public float _maxScaleLogCost = 1f;

		
		public float _minAngleBetweenEdges = 90f;

		
		public int _maxPoints = 50;

		
		public LayerMask _floorLayers;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		
		public bool _voxelize;

		
		private bool _initialized;

		
		[SerializeThis]
		private List<Vector3> _multiPointsPositions;

		
		[SerializeThis]
		private int _multiPointsPositionsCount;

		
		[SerializeThis]
		private List<Hole> _holes;

		
		[SerializeThis]
		private int _holesCount;

		
		private Transform _raftRoot;

		
		private float _logLength;

		
		private float _logWidth;

		
		private float _maxChunkLength;

		
		private float _minChunkLength;

		
		private int _rowCount;

		
		private List<Vector3>[] _rowPointsBuffer;

		
		private List<Vector3>[] _holesRowPointsBuffer;

		
		private Stack<Transform> _logPool;

		
		private Stack<Transform> _newPool;

		
		private Material _logMat;
	}
}
