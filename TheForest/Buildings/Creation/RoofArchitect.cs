using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	[AddComponentMenu("Buildings/Creation/Roof Architect")]
	[DoNotSerializePublic]
	public class RoofArchitect : EntityBehaviour, IStructureSupport, IHoleStructure, ICoopStructure, IRoof, IEntityReplicationFilter
	{
		
		private void Awake()
		{
			this._autofillmode = PlayerPreferences.ExFloorsAutofill;
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		
		private IEnumerator DelayedAwake(bool isDeserializing)
		{
			this._logPool = new Stack<Transform>();
			this._newPool = new Stack<Transform>();
			this._roofRoot = new GameObject("RoofRoot").transform;
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
			this._maxSegmentHorizontalLength = this._logLength * this._maxLogScale;
			this._minEdgeLength = this._logWidth;
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
				this._craftStructure.OnBuilt = new Action<GameObject>(this.OnBuilt);
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

		
		private void Update()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = this._multiPointsPositions.Count >= 3 && this._roofHeight > this._minHeight;
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag3)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag3;
			}
			if (this._lockMode == RoofArchitect.LockModes.Shape && TheForest.Utils.Input.GetButtonDown("Craft"))
			{
				this._autofillmode = !this._autofillmode;
				PlayerPrefs.SetInt("ExFloorsAutofill", (!PlayerPreferences.ExFloorsAutofill) ? 0 : 1);
				PlayerPrefs.Save();
				this.UpdateAutoFill(true);
				Scene.HudGui.MultipointShapeGizmo.Shutdown();
			}
			if (TheForest.Utils.Input.GetButtonDown("AltFire") && this._multiPointsPositions.Count > 0)
			{
				if (this._multiPointsPositions.Count == 1)
				{
					if (this._roofRoot)
					{
						UnityEngine.Object.Destroy(this._roofRoot.gameObject);
						this._roofRoot = null;
					}
					this._newPool.Clear();
					this._logPool.Clear();
					Scene.HudGui.MultipointShapeGizmo.Shutdown();
				}
				this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
				if (this._lockMode == RoofArchitect.LockModes.Height)
				{
					this._lockMode = RoofArchitect.LockModes.Shape;
					LocalPlayer.Sfx.PlayWhoosh();
				}
				this._roofHeight = 0f;
			}
			if (!this._autofillmode)
			{
				if (this._lockMode == RoofArchitect.LockModes.Shape)
				{
					flag2 = this.UpdateShape(out flag);
				}
				else
				{
					this.UpdateHeight();
				}
			}
			else if (this._lockMode == RoofArchitect.LockModes.Shape)
			{
				if (!flag && this._multiPointsPositions.Count >= 3 && TheForest.Utils.Input.GetButtonDown("Build"))
				{
					if (this.CurrentSupport != null)
					{
						LocalPlayer.Create.BuildingPlacer.ForcedParent = (this.CurrentSupport as MonoBehaviour).gameObject;
					}
					this._lockMode = RoofArchitect.LockModes.Height;
					LocalPlayer.Sfx.PlayWhoosh();
				}
				this._caster.CastForAnchors<PrefabIdentifier>(new Action<PrefabIdentifier>(this.CheckTargetingSupport));
			}
			else
			{
				this.UpdateHeight();
			}
			if (this._multiPointsPositions.Count > 0)
			{
				if (!this._autofillmode && this._lockMode == RoofArchitect.LockModes.Shape)
				{
					Scene.HudGui.MultipointShapeGizmo.Show(this, new Vector3(0f, 0.15f, 0f));
				}
				if (this._multiPointsPositions.Count == 1)
				{
					Vector3 vector = this.GetCurrentEdgePoint();
					if (Vector3.Distance(vector, this._multiPointsPositions[0]) < this._logWidth / 2f)
					{
						vector = this._multiPointsPositions[0] + (vector - this._multiPointsPositions[0]).normalized * 0.5f;
					}
					this._multiPointsPositions.Add(vector);
					Vector3 b = (this._multiPointsPositions[1] - this._multiPointsPositions[0]).RotateY(-90f).normalized * 0.5f;
					this._multiPointsPositions.Add(this._multiPointsPositions[1] + b);
					this._multiPointsPositions.Add(this._multiPointsPositions[0] + b);
					this._multiPointsPositions.Add(this._multiPointsPositions[0]);
					this.RefreshCurrentRoof();
					this._multiPointsPositions.RemoveRange(1, 4);
				}
				else
				{
					bool flag4 = false;
					bool flag5 = this._lockMode == RoofArchitect.LockModes.Height || (this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions[0], this._multiPointsPositions[this._multiPointsPositions.Count - 1]) < this._closureSnappingDistance);
					if (!flag5)
					{
						Vector3 currentEdgePoint = this.GetCurrentEdgePoint();
						if (Vector3.Distance(currentEdgePoint, this._multiPointsPositions[0]) > this._closureSnappingDistance)
						{
							flag4 = true;
							this._multiPointsPositions.Add(currentEdgePoint);
						}
						if (this._multiPointsPositions.Count > 2 || !flag4)
						{
							this._multiPointsPositions.Add(this._multiPointsPositions[0]);
						}
					}
					this.RefreshCurrentRoof();
					if (!flag5)
					{
						if (this._multiPointsPositions.Count > 2 || !flag4)
						{
							this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
						}
						if (flag4)
						{
							this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
						}
					}
				}
			}
			else if (this.CurrentSupport != null)
			{
				Scene.HudGui.MultipointShapeGizmo.Show(this, new Vector3(0f, 0.15f, 0f));
				Vector3 currentEdgePoint2 = this.GetCurrentEdgePoint();
				this._multiPointsPositions.Add(currentEdgePoint2);
				Vector3 b2 = LocalPlayer.Transform.right * this._logWidth * 1f;
				this._multiPointsPositions.Add(this._multiPointsPositions[0] + b2);
				Vector3 b3 = (this._multiPointsPositions[1] - this._multiPointsPositions[0]).RotateY(-90f).normalized * 0.5f;
				this._multiPointsPositions.Add(this._multiPointsPositions[1] + b3);
				this._multiPointsPositions.Add(this._multiPointsPositions[0] + b3);
				this._multiPointsPositions.Add(this._multiPointsPositions[0]);
				this.RefreshCurrentRoof();
				this._multiPointsPositions.Clear();
			}
			else
			{
				Scene.HudGui.MultipointShapeGizmo.Shutdown();
			}
			bool canLock = (this._lockMode != RoofArchitect.LockModes.Height) ? (!flag && (flag2 || flag3 || this._multiPointsPositions.Count > 0)) : flag3;
			Create.CanLock = canLock;
			Renderer component = base.GetComponent<Renderer>();
			if (component)
			{
				component.sharedMaterial = Create.CurrentGhostMat;
				component.enabled = (this.CurrentSupport == null && this._multiPointsPositions.Count == 0);
			}
			bool flag6 = this._lockMode == RoofArchitect.LockModes.Height;
			bool flag7 = !this._autofillmode && this._multiPointsPositions.Count == 0 && this.CurrentSupport != null;
			bool canToggleAutofill = this._lockMode == RoofArchitect.LockModes.Shape && (flag2 || flag3 || this._multiPointsPositions.Count > 0 || this.CurrentSupport != null);
			bool canLock2 = !flag7 && flag2 && this._lockMode == RoofArchitect.LockModes.Shape;
			bool canUnlock = !this._autofillmode && !flag6 && this._multiPointsPositions.Count > 0 && this._lockMode == RoofArchitect.LockModes.Shape;
			bool showAutofillPlace = this._autofillmode && !flag6 && !flag && this._multiPointsPositions.Count >= 3;
			bool showManualPlace = !this._autofillmode && !flag6 && !flag && this._multiPointsPositions.Count >= ((!flag2) ? 3 : 2);
			Scene.HudGui.RoofConstructionIcons.Show(flag7, canToggleAutofill, showAutofillPlace, showManualPlace, canLock2, canUnlock, flag6);
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

		
		private void OnDeserialized()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				if (Cheats.ResetHoles && !BoltNetwork.isClient)
				{
					this._holes.Clear();
					this._holesCount = 0;
				}
				this._multiPointsPositions.RemoveRange(this._multiPointsPositionsCount, this._multiPointsPositions.Count - this._multiPointsPositionsCount);
				this._holes.RemoveRange(this._holesCount, this._holes.Count - this._holesCount);
				if (this._wasBuilt)
				{
					this.CreateStructure(false);
					this._rowPointsBuffer = null;
					this._holesRowPointsBuffer = null;
					this._roofRoot.transform.parent = base.transform;
					InsideCheck.AddRoof(this);
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
		}

		
		protected virtual void OnBeginCollapse()
		{
			InsideCheck.RemoveRoof(this);
		}

		
		private IEnumerator OnPlaced()
		{
			if (Vector3.Distance(this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[0]) >= this._closureSnappingDistance)
			{
				this._multiPointsPositions.Add(this._multiPointsPositions[0]);
				this.RefreshCurrentRoof();
			}
			base.enabled = false;
			this._wasPlaced = true;
			Scene.HudGui.SupportPlacementGizmo.Hide();
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
				Transform ghostRoot = this._roofRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.$this._logItemId);
				List<GameObject> logs = new List<GameObject>();
				IEnumerator enumerator = this._roofRoot.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						ri._amount++;
						IEnumerator enumerator2 = transform2.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								object obj2 = enumerator2.Current;
								Transform transform3 = (Transform)obj2;
								logs.Add(transform3.GetComponentInChildren<Renderer>().gameObject);
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
				ri.AddRuntimeObjects(logs, Prefabs.Instance.LogRoofBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				this._roofRoot.transform.parent = base.transform;
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
				Bounds b = new Bounds(this._multiPointsPositions[0], Vector3.zero);
				for (int j = 1; j < this._multiPointsPositions.Count; j++)
				{
					b.Encapsulate(this._multiPointsPositions[j]);
				}
				Vector3 bottom = this._multiPointsPositions[0];
				bottom.y -= this._logWidth / 3f;
				b.Encapsulate(bottom);
				Vector3 top = this._multiPointsPositions[0];
				top.y += this._roofHeight;
				b.Encapsulate(top);
				bc.center = base.transform.InverseTransformPoint(b.center);
				Vector3 finalColliderSize = b.size;
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
				this._logPool = null;
				this._newPool = null;
				this._rowPointsBuffer = null;
			}
			yield break;
		}

		
		public void OnBuilt(GameObject built)
		{
			RoofArchitect component = built.GetComponent<RoofArchitect>();
			component._multiPointsPositions = this._multiPointsPositions;
			component._holes = this._holes;
			component._roofHeight = this._roofHeight;
			component._wasBuilt = true;
			component.OnSerializing();
			if (this.CurrentSupport != null)
			{
				this.CurrentSupport.Enslaved = true;
			}
		}

		
		public void Clear()
		{
			this._rowPointsBuffer = null;
			this._holesRowPointsBuffer = null;
			if (this._roofRoot)
			{
				UnityEngine.Object.Destroy(this._roofRoot.gameObject);
			}
		}

		
		public void CreateStructure(bool isRepair = false)
		{
			if (this._wasBuilt && isRepair)
			{
				this.Clear();
				base.StartCoroutine(this.DelayedAwake(true));
			}
			this.RefreshCurrentRoof();
			if (this._wasBuilt)
			{
				this._roofRoot.parent = base.transform;
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
			return hole;
		}

		
		public void RemoveHole(Hole hole)
		{
			this._holes.Remove(hole);
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

		
		private void CheckTargetingSupport(PrefabIdentifier structureRoot)
		{
			IStructureSupport structureSupport = (!structureRoot) ? null : structureRoot.GetComponent<IStructureSupport>();
			if (structureSupport != null)
			{
				if (this.CurrentSupport == null && (!this._autofillmode || !(structureSupport as MonoBehaviour).GetComponent<FloorArchitect>()))
				{
					this.CurrentSupport = structureSupport;
					if (this.CurrentSupport != null)
					{
						LocalPlayer.Create.BuildingPlacer.ForcedParent = (this.CurrentSupport as MonoBehaviour).gameObject;
					}
					this.SupportHeight = this.CurrentSupport.GetLevel();
					this.UpdateAutoFill(false);
				}
			}
			else if (this.CurrentSupport != null)
			{
				this.CurrentSupport = null;
				this.UpdateAutoFill(false);
				if (this._multiPointsPositions.Count == 0 && this._lockMode == RoofArchitect.LockModes.Shape)
				{
					base.GetComponent<Renderer>().enabled = true;
					this.RefreshCurrentRoof();
				}
			}
		}

		
		private void UpdateAutoFill(bool doCleanUp)
		{
			bool flag = this._autofillmode && this.CurrentSupport != null && this.CurrentSupport.GetMultiPointsPositions(true) != null && this._lockMode == RoofArchitect.LockModes.Shape;
			if (flag)
			{
				this._multiPointsPositions = new List<Vector3>(this.CurrentSupport.GetMultiPointsPositions(true));
				int count = this._multiPointsPositions.Count;
				float height = this.CurrentSupport.GetHeight();
				float num = this.CurrentSupport.GetLevel() - 0.5f;
				for (int i = this._multiPointsPositions.Count - 1; i >= 0; i--)
				{
					Vector3 value = this._multiPointsPositions[i];
					if (Mathf.Abs(value.y + height - num) < this._closureSnappingDistance)
					{
						value.y = num;
						this._multiPointsPositions[i] = value;
					}
					else
					{
						this._multiPointsPositions.RemoveAt(i);
					}
				}
				if (this._multiPointsPositions.Count >= ((count != 3) ? 4 : 3))
				{
					if (this._multiPointsPositions.Last<Vector3>() != this._multiPointsPositions.First<Vector3>())
					{
						this._multiPointsPositions.Add(this._multiPointsPositions.First<Vector3>());
					}
					this.RefreshCurrentRoof();
				}
				else
				{
					this._multiPointsPositions.Clear();
				}
			}
			else if ((this._autofillmode && this._lockMode == RoofArchitect.LockModes.Shape) || doCleanUp)
			{
				this._multiPointsPositions.Clear();
				this.RefreshCurrentRoof();
			}
		}

		
		private bool UpdateShape(out bool edgeCrossing)
		{
			edgeCrossing = false;
			Vector3 vector = this.GetCurrentEdgePoint();
			bool flag = LocalPlayer.Create.BuildingPlacer.OnDynamicClear && this.CurrentSupport != null && this.CurrentSupport.GetMultiPointsPositions(true) != null && Mathf.Abs(vector.y - this.SupportHeight) < this._closureSnappingDistance && (this._multiPointsPositions.Count == 0 || Mathf.Abs(this._multiPointsPositions[0].y - this.SupportHeight) < this._closureSnappingDistance) && this._multiPointsPositions.Count < this._maxPoints;
			if (flag && this._multiPointsPositions.Count > 0)
			{
				Vector3 to = vector - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
				flag = (to.sqrMagnitude > this._minEdgeLength * this._minEdgeLength);
				if (this._multiPointsPositions.Count > 1)
				{
					Vector3 from = this._multiPointsPositions[this._multiPointsPositions.Count - 2] - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
					flag = (flag && Vector3.Angle(from, to) >= this._minAngleBetweenEdges);
				}
			}
			if (this._multiPointsPositions.Count > 1)
			{
				edgeCrossing = MathEx.IsLineSegmentCrossingMultipointList(this._multiPointsPositions[this._multiPointsPositions.Count - 1], vector, this._multiPointsPositions);
				if (edgeCrossing)
				{
					flag = false;
				}
			}
			if (flag)
			{
				bool flag2 = this._multiPointsPositions.Count > 2 && Vector3.Distance(vector, this._multiPointsPositions[0]) < this._closureSnappingDistance;
				bool flag3 = !flag2;
				if (flag2)
				{
					vector = this._multiPointsPositions[0];
				}
				bool flag4 = this._multiPointsPositions.Count >= 2 && TheForest.Utils.Input.GetButtonDown("Build");
				if (TheForest.Utils.Input.GetButtonDown("Fire1") || flag4)
				{
					LocalPlayer.Sfx.PlayWhoosh(vector);
					this._multiPointsPositions.Add(vector);
					if (flag4 && !flag2)
					{
						this._multiPointsPositions.Add(this._multiPointsPositions[0]);
					}
					if (flag2 || flag4)
					{
						this._lockMode = RoofArchitect.LockModes.Height;
						Scene.HudGui.SupportPlacementGizmo.Hide();
					}
				}
			}
			bool flag5 = !edgeCrossing && this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions.First<Vector3>(), this._multiPointsPositions.Last<Vector3>()) > this._minEdgeLength;
			if (flag5 && (TheForest.Utils.Input.GetButtonDown("Rotate") || TheForest.Utils.Input.GetButtonDown("Build")))
			{
				this._multiPointsPositions.Add(this._multiPointsPositions.First<Vector3>());
				this._lockMode = RoofArchitect.LockModes.Height;
				Scene.HudGui.SupportPlacementGizmo.Hide();
			}
			this._caster.CastForAnchors<PrefabIdentifier>(new Action<PrefabIdentifier>(this.CheckTargetingSupport));
			return flag;
		}

		
		private void UpdateHeight()
		{
			this._roofHeight = base.transform.position.y - this._multiPointsPositions[0].y;
			bool flag = this._roofHeight > this._minHeight;
			if (flag && (TheForest.Utils.Input.GetButtonDown("Fire1") || TheForest.Utils.Input.GetButtonDown("Build")))
			{
				LocalPlayer.Create.PlaceGhost(false);
			}
		}

		
		public Vector3 GetCurrentEdgePoint()
		{
			Vector3 result = (this.CurrentSupport == null) ? base.transform.position : MathEx.ClosestPointOnPolygon(base.transform.position, this.CurrentSupport.GetMultiPointsPositions(true));
			if (this._multiPointsPositions.Count > 0)
			{
				result.y = this._multiPointsPositions[0].y;
			}
			else
			{
				result.y = this.SupportHeight - 0.5f;
			}
			return result;
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

		
		private void RefreshCurrentRoof()
		{
			if (this._logPool == null)
			{
				this._logPool = new Stack<Transform>();
			}
			this._newPool = new Stack<Transform>();
			Transform roofRoot = this._roofRoot;
			if (this._multiPointsPositions.Count >= 3)
			{
				this._roofRoot = new GameObject("RoofRoot").transform;
				this.SpawnRoof();
			}
			if (roofRoot)
			{
				UnityEngine.Object.Destroy(roofRoot.gameObject);
			}
			this._logPool = this._newPool;
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

		
		private void SpawnRoof()
		{
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			Vector3 forward = this._multiPointsPositions[1] - this._multiPointsPositions[0];
			base.transform.rotation = Quaternion.LookRotation(forward);
			Vector3[] array = (from p in this._multiPointsPositions
			select base.transform.InverseTransformPoint(p)).ToArray<Vector3>();
			float y = array[0].y;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].y = 0f;
				if (array[i].z < num)
				{
					num = array[i].z;
				}
				if (array[i].z > num2)
				{
					num2 = array[i].z;
				}
				if (array[i].x < num3)
				{
					num3 = array[i].x;
				}
				if (array[i].x > num4)
				{
					num4 = array[i].x;
				}
			}
			float num5 = 0.65f * this._logWidth;
			float logWidth = this._logWidth;
			for (int j = 0; j < array.Length; j++)
			{
				if (Mathf.Abs(array[j].z - num) < logWidth)
				{
					Vector3[] array2 = array;
					int num6 = j;
					array2[num6].z = array2[num6].z - num5;
				}
				else if (Mathf.Abs(array[j].z - num2) < logWidth)
				{
					Vector3[] array3 = array;
					int num7 = j;
					array3[num7].z = array3[num7].z + num5;
				}
				if (Mathf.Abs(array[j].x - num3) < logWidth)
				{
					Vector3[] array4 = array;
					int num8 = j;
					array4[num8].x = array4[num8].x - num5;
				}
				else if (Mathf.Abs(array[j].x - num4) < logWidth)
				{
					Vector3[] array5 = array;
					int num9 = j;
					array5[num9].x = array5[num9].x + num5;
				}
			}
			num -= num5;
			num2 += num5;
			float num10 = Mathf.Abs(num2 - num);
			this._rowCount = Mathf.CeilToInt(num10 / this._logWidth);
			this.InitRowPointsBuffer(this._rowCount, this._rowPointsBuffer, out this._rowPointsBuffer);
			this.CalcRowPointBufferForArray(array, num, this._rowPointsBuffer);
			float num11 = float.MinValue;
			float num12 = float.MaxValue;
			List<Vector3>[] array6 = new List<Vector3>[this._rowPointsBuffer.Length];
			for (int k = 0; k < this._rowCount; k++)
			{
				this._rowPointsBuffer[k].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
				array6[k] = new List<Vector3>();
				int count = this._rowPointsBuffer[k].Count;
				for (int l = 1; l < count; l += 2)
				{
					Vector3 a = this._rowPointsBuffer[k][l - 1];
					Vector3 vector = this._rowPointsBuffer[k][l];
					array6[k].Add(Vector3.Lerp(a, vector, 0.5f));
					vector.y = 1f;
					this._rowPointsBuffer[k][l] = vector;
				}
				for (int m = 1; m < count; m += 2)
				{
					int num13 = 0;
					while (m < count && this._rowPointsBuffer[k][m].y < 0f)
					{
						num13 += 2;
						m += 2;
					}
					if (m < count)
					{
						float num14 = Mathf.Abs(this._rowPointsBuffer[k][m - 1 - num13].x - this._rowPointsBuffer[k][m].x);
						if (num14 > num11)
						{
							num11 = num14;
						}
						if (num14 < num12)
						{
							num12 = num14;
						}
					}
				}
			}
			float num15 = 1f - num12 / num11;
			if (this._holes != null)
			{
				for (int n = 0; n < this._holes.Count; n++)
				{
					Vector3[] array7 = new Vector3[5];
					Hole hole = this._holes[n];
					hole._used = false;
					array7[0] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					array7[1] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
					array7[2] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
					array7[3] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					array7[4] = array7[0];
					for (int num16 = 0; num16 < 5; num16++)
					{
						array7[num16].y = array[0].y;
					}
					this.InitRowPointsBuffer(this._rowCount, this._holesRowPointsBuffer, out this._holesRowPointsBuffer);
					this.CalcRowPointBufferForArray(array7, num, this._holesRowPointsBuffer);
					for (int num17 = 0; num17 < this._rowCount; num17++)
					{
						this._rowPointsBuffer[num17].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						this._holesRowPointsBuffer[num17].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						List<Vector3> list = this._rowPointsBuffer[num17];
						List<Vector3> list2 = this._holesRowPointsBuffer[num17];
						int num18 = list.Count;
						int count2 = list2.Count;
						for (int num19 = 1; num19 < num18; num19 += 2)
						{
							int num20 = num19 - 1;
							while (num20 > 1 && this._rowPointsBuffer[num17][num20].y < 0f)
							{
								num20 -= 2;
							}
							int num21 = num19;
							while (num21 + 2 < num18 && this._rowPointsBuffer[num17][num21].y < 0f)
							{
								num21 += 2;
							}
							bool flag = num19 - 1 == num20;
							bool flag2 = num19 == num21;
							float x = this._rowPointsBuffer[num17][num20].x;
							float x5 = this._rowPointsBuffer[num17][num21].x;
							float num22 = Mathf.Abs(x - x5);
							float num23 = Mathf.Lerp(x, x5, Mathf.Abs(list[num19 - 1].y));
							float num24 = Mathf.Lerp(x, x5, Mathf.Abs(list[num19].y));
							for (int num25 = 1; num25 < count2; num25 += 2)
							{
								float x3 = list2[num25 - 1].x;
								float x4 = list2[num25].x;
								if (num23 > x3 && num24 < x4)
								{
									if (num18 > 2 && (flag2 || flag) && (!flag2 || !flag))
									{
										if (flag)
										{
											list.RemoveAt(num19);
											Vector3 value = list[num19 - 1];
											value.y = Mathf.Abs(list[num19].y);
											list[num19 - 1] = value;
											list.RemoveAt(num19);
										}
										else
										{
											Vector3 value2 = list[num19];
											value2.y = Mathf.Abs(list[num19 - 1 - 1].y);
											list[num19] = value2;
											list.RemoveAt(num19 - 1);
											list.RemoveAt(num19 - 1 - 1);
										}
									}
									else
									{
										list.RemoveAt(num19);
										list.RemoveAt(num19 - 1);
									}
									hole._used = true;
									if (num25 + 2 >= count2)
									{
										if (num19 + 2 >= num18)
										{
											break;
										}
										num19 -= 2;
										num18 -= 2;
									}
								}
								else if (num23 < x3 && num24 > x4)
								{
									Vector3 item = list2[num25 - 1];
									item.y = -Mathf.Abs(x3 - x) / num22;
									Vector3 item2 = list2[num25];
									item2.y = -Mathf.Abs(x4 - x) / num22;
									list.Insert(num19, item);
									list.Insert(num19 + 1, item2);
									hole._used = true;
									num19 += 2;
									num18 += 2;
								}
								else if (num23 > x3 && num23 < x4)
								{
									Vector3 value3 = list[num19 - 1];
									value3.y = Mathf.Abs(x4 - x) / num22;
									if (!flag)
									{
										value3.x = x4;
										value3.y = -value3.y;
									}
									list[num19 - 1] = value3;
									hole._used = true;
								}
								else if (num24 > x3 && num24 < x4)
								{
									Vector3 value4 = list[num19];
									value4.y = Mathf.Abs(x3 - x) / num22;
									if (!flag2)
									{
										value4.x = x3;
										value4.y = -value4.y;
									}
									list[num19] = value4;
									hole._used = true;
								}
							}
						}
					}
				}
			}
			Transform roofRoot = this._roofRoot;
			float num26 = 0f;
			float logStackScaleRatio = (this._maxScaleLogCost <= 0f) ? 0f : (this._maxLogScale / this._maxScaleLogCost);
			for (int num27 = 0; num27 < this._rowCount; num27++)
			{
				int count3 = this._rowPointsBuffer[num27].Count;
				for (int num28 = 1; num28 < count3; num28 += 2)
				{
					int num29 = num28 - 1;
					while (num29 > 1 && this._rowPointsBuffer[num27][num29].y < 0f)
					{
						num29 -= 2;
					}
					int num30 = num28;
					while (num30 + 2 < count3 && this._rowPointsBuffer[num27][num30].y < 0f)
					{
						num30 += 2;
					}
					int num31 = 0;
					while (num31 + 1 < array6[num27].Count && this._rowPointsBuffer[num27][num29].x > array6[num27][num31].x)
					{
						num31++;
					}
					Vector3 vector2 = this._rowPointsBuffer[num27][num28 - 1];
					Vector3 vector3 = this._rowPointsBuffer[num27][num28];
					Vector3 vector4 = this._rowPointsBuffer[num27][num29];
					Vector3 vector5 = this._rowPointsBuffer[num27][num30];
					float y2 = vector2.y;
					float y3 = vector3.y;
					float num32 = Mathf.Abs(y2) * 2f - 1f;
					float num33 = Mathf.Abs(y3) * 2f - 1f;
					float value5 = Mathf.Abs(vector4.x - vector5.x);
					vector4.y = y;
					vector5.y = y;
					vector4 = base.transform.TransformPoint(vector4);
					vector5 = base.transform.TransformPoint(vector5);
					Vector3 vector6 = array6[num27][num31];
					vector6.y = y + (1f - Mathf.InverseLerp(num11, num12, value5) * num15) * this._roofHeight;
					vector6 = base.transform.TransformPoint(vector6);
					if ((num32 > 0f && num33 > 0f) || (num32 < 0f && num33 < 0f))
					{
						if (num32 > 0f)
						{
							Vector3 vector7 = Vector3.Lerp(vector6, vector5, Mathf.Abs(num32));
							Vector3 a2 = Vector3.Lerp(vector6, vector5, Mathf.Abs(num33));
							this.SpawnChunk(a2 - vector7, vector7, roofRoot, logStackScaleRatio, ref num26);
						}
						else if (num32 < 0f)
						{
							Vector3 vector8 = Vector3.Lerp(vector6, vector4, Mathf.Abs(num32));
							Vector3 a3 = Vector3.Lerp(vector6, vector4, Mathf.Abs(num33));
							this.SpawnChunk(a3 - vector8, vector8, roofRoot, logStackScaleRatio, ref num26);
						}
					}
					else
					{
						Vector3 vector9 = Vector3.Lerp(vector6, vector4, Mathf.Abs(num32));
						Vector3 a4 = vector6;
						this.SpawnChunk(a4 - vector9, vector9, roofRoot, logStackScaleRatio, ref num26);
						vector9 = Vector3.Lerp(vector6, vector5, Mathf.Abs(num33));
						this.SpawnChunk(a4 - vector9, vector9, roofRoot, logStackScaleRatio, ref num26);
					}
				}
			}
		}

		
		private void SpawnChunk(Vector3 chunk, Vector3 chunkStart, Transform currentLogStackTr, float logStackScaleRatio, ref float currentLogStackRemainingScale)
		{
			if (chunk.sqrMagnitude > 0f)
			{
				Quaternion rotation = Quaternion.LookRotation(chunk);
				float magnitude = chunk.magnitude;
				int num = Mathf.CeilToInt(magnitude / this._maxSegmentHorizontalLength);
				Vector3 one = Vector3.one;
				one.z = magnitude / (float)num / this._logLength;
				Vector3 b = chunk / (float)num;
				for (int i = 0; i < num; i++)
				{
					Transform transform = this.NewLog(chunkStart, rotation);
					transform.parent = null;
					if (currentLogStackRemainingScale > 0f)
					{
						currentLogStackRemainingScale -= one.z;
					}
					else
					{
						currentLogStackTr = new GameObject("ls").transform;
						currentLogStackTr.position = transform.position;
						currentLogStackTr.parent = this._roofRoot;
						currentLogStackRemainingScale = logStackScaleRatio - one.z;
					}
					transform.parent = currentLogStackTr;
					transform.localScale = one;
					this._newPool.Push(transform);
					chunkStart += b;
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
				if (!this._wasBuilt && !this._wasPlaced)
				{
					transform.GetComponentInChildren<Renderer>().sharedMaterial = Create.CurrentGhostMat;
				}
				return transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefab, position, rotation);
			if (!this._wasBuilt && !this._wasPlaced)
			{
				transform2.GetComponentInChildren<Renderer>().sharedMaterial = Create.CurrentGhostMat;
			}
			return transform2;
		}

		
		
		
		[SerializeThis]
		public bool Enslaved { get; set; }

		
		public float GetLevel()
		{
			return this._multiPointsPositions[0].y + this.GetHeight();
		}

		
		public float GetHeight()
		{
			return this._logWidth / 2f;
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

		
		public List<Vector3> GetPolygon()
		{
			return this.GetMultiPointsPositions(true);
		}

		
		
		public RoofArchitect.LockModes LockMode
		{
			get
			{
				return this._lockMode;
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

		
		
		
		public float SupportHeight { get; set; }

		
		
		
		public IStructureSupport CurrentSupport { get; set; }

		
		
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
			if (!base.entity.isOwner)
			{
				IBuildingDestructibleState buildingDestructibleState;
				if (base.entity.TryFindState<IBuildingDestructibleState>(out buildingDestructibleState))
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
			if (base.entity.TryFindState<IBuildingDestructibleState>(out buildingDestructibleState))
			{
				this.CustomToken = buildingDestructibleState.Token;
			}
		}

		
		
		
		public IProtocolToken CustomToken
		{
			get
			{
				CoopRoofToken coopRoofToken = new CoopRoofToken();
				if (this._holes != null)
				{
					coopRoofToken.Holes = this._holes.ToArray();
				}
				coopRoofToken.Height = this._roofHeight;
				return coopRoofToken;
			}
			set
			{
				CoopRoofToken coopRoofToken = (CoopRoofToken)value;
				if (coopRoofToken.Height > 0f)
				{
					if (coopRoofToken.Holes != null)
					{
						this._holes = coopRoofToken.Holes.ToList<Hole>();
					}
					this._roofHeight = coopRoofToken.Height;
					if (!base.entity.isOwner || this._wasBuilt)
					{
						this.CreateStructure(false);
					}
				}
			}
		}

		
		bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
		{
			return this.CurrentSupport == null || connection.ExistsOnRemote((this.CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>()) == ExistsResult.Yes;
		}

		
		[SerializeThis]
		public bool _wasPlaced;

		
		[SerializeThis]
		public bool _wasBuilt;

		
		public SpherecastAnchoring _caster;

		
		public Transform _logPrefab;

		
		public Renderer _logRenderer;

		
		public float _closureSnappingDistance = 2.5f;

		
		public float _minLogScale = 0.3f;

		
		public float _maxLogScale = 2f;

		
		public float _maxScaleLogCost = 1f;

		
		public int _maxPoints = 50;

		
		public float _minAngleBetweenEdges = 90f;

		
		public float _minHeight = 2f;

		
		public LayerMask _supportLayers;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		
		private bool _initialized;

		
		private bool _autofillmode = true;

		
		[SerializeThis]
		private List<Vector3> _multiPointsPositions;

		
		[SerializeThis]
		private int _multiPointsPositionsCount;

		
		[SerializeThis]
		private List<Hole> _holes;

		
		[SerializeThis]
		private int _holesCount;

		
		[SerializeThis]
		private float _roofHeight;

		
		private Transform _roofRoot;

		
		private float _logLength;

		
		private float _logWidth;

		
		private float _maxSegmentHorizontalLength;

		
		private float _minEdgeLength;

		
		private int _rowCount;

		
		private List<Vector3>[] _rowPointsBuffer;

		
		private List<Vector3>[] _holesRowPointsBuffer;

		
		private Stack<Transform> _logPool;

		
		private Stack<Transform> _newPool;

		
		private RoofArchitect.LockModes _lockMode;

		
		public enum LockModes
		{
			
			Shape,
			
			Height
		}
	}
}
