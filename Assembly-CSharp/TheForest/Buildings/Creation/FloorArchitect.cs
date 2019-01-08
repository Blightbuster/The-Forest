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
	[AddComponentMenu("Buildings/Creation/Floor Architect")]
	[DoNotSerializePublic]
	public class FloorArchitect : EntityBehaviour, IStructureSupport, IHoleStructure, ICoopStructure, IRoof, IEntityReplicationFilter
	{
		private void Awake()
		{
			this._autofillmode = (!this._autoSupported && PlayerPreferences.ExFloorsAutofill);
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		private IEnumerator DelayedAwake(bool isDeserializing)
		{
			this._logPool = new Stack<Transform>();
			this._newPool = new Stack<Transform>();
			this._floorRoot = new GameObject("FloorRoot").transform;
			if (this._multiPointsPositions == null)
			{
				this._multiPointsPositions = new List<Vector3>(5);
			}
			if (this._holes == null)
			{
				this._holes = new List<Hole>();
			}
			if (this._logPrefabs[0].GetComponent<Renderer>())
			{
				this._logLength = this._logPrefabs[0].GetComponent<Renderer>().bounds.size.z;
				this._logWidth = this._logPrefabs[0].GetComponent<Renderer>().bounds.size.x * 1.025f;
			}
			else
			{
				this._logLength = this._logRenderer.bounds.size.z;
				this._logWidth = this._logRenderer.bounds.size.x * 1.025f;
			}
			this._maxChunkLength = this._logLength * this._maxLogScale;
			this._minChunkLength = this._logWidth;
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
				if (!this._wasPlaced)
				{
					if (this._autoSupported)
					{
						this._logMat = Prefabs.Instance.GhostClear;
					}
					else
					{
						this._logMat = new Material(this._logRenderer.sharedMaterial);
					}
				}
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
			bool flag = false;
			bool flag2 = false;
			bool flag3 = this._multiPointsPositions.Count >= 3;
			if (!this._autoSupported && TheForest.Utils.Input.GetButtonDown("Craft"))
			{
				this._autofillmode = !this._autofillmode;
				PlayerPrefs.SetInt("ExFloorsAutofill", (!PlayerPreferences.ExFloorsAutofill) ? 0 : 1);
				PlayerPrefs.Save();
				this.UpdateAutoFill(true);
				base.GetComponent<Renderer>().enabled = (this.CurrentSupport == null);
				Scene.HudGui.MultipointShapeGizmo.Shutdown();
			}
			Vector3 vector = this.GetCurrentEdgePoint();
			bool flag4;
			if (!this._autofillmode)
			{
				if (this._autoSupported)
				{
					flag4 = false;
					List<Vector3> multiPointsPositions = base.GetComponent<FoundationArchitect>().MultiPointsPositions;
					if (multiPointsPositions.Count > this._multiPointsPositions.Count)
					{
						this._multiPointsPositions.Add(multiPointsPositions[multiPointsPositions.Count - 1] + new Vector3(0f, 0.4f, 0f));
					}
					else if (multiPointsPositions.Count < this._multiPointsPositions.Count)
					{
						this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
						if (this._multiPointsPositions.Count == 0)
						{
							this.ClearTemp();
						}
					}
				}
				else
				{
					if (TheForest.Utils.Input.GetButtonDown("AltFire") && this._multiPointsPositions.Count > 0)
					{
						if (this._multiPointsPositions.Count == 1)
						{
							this.ClearTemp();
						}
						this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
					}
					flag4 = (LocalPlayer.Create.BuildingPlacer.OnDynamicClear && (this._autoSupported || (this.CurrentSupport != null && this.CurrentSupport.GetMultiPointsPositions(false) != null && Mathf.Abs(vector.y - this.SupportHeight) < this._closureSnappingDistance)) && (this._multiPointsPositions.Count == 0 || Mathf.Abs(this._multiPointsPositions[0].y - this.SupportHeight) < this._closureSnappingDistance) && this._multiPointsPositions.Count < this._maxPoints);
					if (flag4 && this._multiPointsPositions.Count > 0)
					{
						Vector3 vector2 = vector - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
						flag4 = (vector2.sqrMagnitude > this._minChunkLength * this._minChunkLength);
						if (this._multiPointsPositions.Count > 1)
						{
							Vector3 vector3 = this._multiPointsPositions[this._multiPointsPositions.Count - 2] - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
							flag4 = (flag4 && Vector3.Angle(vector3.normalized, vector2.normalized) >= this._minAngleBetweenEdges);
						}
					}
					if (!this._autoSupported && this._multiPointsPositions.Count > 1)
					{
						flag2 = MathEx.IsLineSegmentCrossingMultipointList(this._multiPointsPositions[this._multiPointsPositions.Count - 1], vector, this._multiPointsPositions);
						if (flag2)
						{
							flag4 = false;
							flag3 = false;
						}
					}
					if (flag4)
					{
						bool flag5 = this._multiPointsPositions.Count > 2 && Vector3.Distance(vector, this._multiPointsPositions[0]) < this._closureSnappingDistance;
						bool flag6 = !flag5;
						if (flag5)
						{
							vector = this._multiPointsPositions[0];
						}
						if (TheForest.Utils.Input.GetButtonDown("Fire1") || (this._multiPointsPositions.Count >= 2 && TheForest.Utils.Input.GetButtonDown("Build")))
						{
							LocalPlayer.Sfx.PlayWhoosh(vector);
							this._multiPointsPositions.Add(vector);
							if (this._multiPointsPositions.Count > 0)
							{
								base.GetComponent<Renderer>().enabled = false;
							}
							flag = flag5;
						}
						else
						{
							flag3 |= (this._multiPointsPositions.Count >= 2 && flag4 && !flag2);
						}
					}
					else
					{
						flag3 |= (this._multiPointsPositions.Count >= 2 && flag4 && !flag2);
					}
					bool flag7 = LocalPlayer.Create.BuildingPlacer.OnDynamicClear && !flag2 && this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions.First<Vector3>(), this._multiPointsPositions.Last<Vector3>()) > this._minChunkLength;
					if (flag7 && (TheForest.Utils.Input.GetButtonDown("Rotate") || TheForest.Utils.Input.GetButtonDown("Build")))
					{
						this._multiPointsPositions.Add(this._multiPointsPositions.First<Vector3>());
						this.RefreshCurrentFloor();
						flag = true;
					}
				}
			}
			else
			{
				flag4 = false;
				if (this._autofillmode && flag3 && TheForest.Utils.Input.GetButtonDown("Build"))
				{
					flag = true;
				}
			}
			if (!this._autofillmode)
			{
				if (this._multiPointsPositions.Count > 0)
				{
					Scene.HudGui.MultipointShapeGizmo.Show(this, default(Vector3));
					if (this._multiPointsPositions.Count == 1)
					{
						Vector3 vector4 = this.GetCurrentEdgePoint();
						if (Vector3.Distance(vector4, this._multiPointsPositions[0]) < this._logWidth / 2f)
						{
							vector4 = this._multiPointsPositions[0] + (vector4 - this._multiPointsPositions[0]).normalized * 0.5f;
						}
						this._multiPointsPositions.Add(vector4);
						Vector3 b = (this._multiPointsPositions[1] - this._multiPointsPositions[0]).RotateY(-90f).normalized * 0.5f;
						this._multiPointsPositions.Add(this._multiPointsPositions[1] + b);
						this._multiPointsPositions.Add(this._multiPointsPositions[0] + b);
						this._multiPointsPositions.Add(this._multiPointsPositions[0]);
						this.RefreshCurrentFloor();
						this._multiPointsPositions.RemoveRange(1, 4);
					}
					else
					{
						bool flag8 = false;
						bool flag9 = this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions[0], this._multiPointsPositions[this._multiPointsPositions.Count - 1]) < this._closureSnappingDistance;
						if (!flag9)
						{
							if (Vector3.Distance(vector, this._multiPointsPositions[0]) > this._closureSnappingDistance)
							{
								flag8 = true;
								this._multiPointsPositions.Add(vector);
							}
							if (this._multiPointsPositions.Count > 2 || !flag8)
							{
								this._multiPointsPositions.Add(this._multiPointsPositions[0]);
							}
						}
						this.RefreshCurrentFloor();
						if (!flag9)
						{
							if (this._multiPointsPositions.Count > 2 || !flag8)
							{
								this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
							}
							if (flag8)
							{
								this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
							}
						}
					}
				}
				else if (this.CurrentSupport != null)
				{
					Scene.HudGui.MultipointShapeGizmo.Show(this, default(Vector3));
					Vector3 currentEdgePoint = this.GetCurrentEdgePoint();
					this._multiPointsPositions.Add(currentEdgePoint);
					Vector3 b2 = LocalPlayer.Transform.right * this._logWidth * 1f;
					this._multiPointsPositions.Add(this._multiPointsPositions[0] + b2);
					Vector3 b3 = (this._multiPointsPositions[1] - this._multiPointsPositions[0]).RotateY(-90f).normalized * 0.5f;
					this._multiPointsPositions.Add(this._multiPointsPositions[1] + b3);
					this._multiPointsPositions.Add(this._multiPointsPositions[0] + b3);
					this._multiPointsPositions.Add(this._multiPointsPositions[0]);
					this.RefreshCurrentFloor();
					this._multiPointsPositions.Clear();
				}
				else
				{
					Scene.HudGui.MultipointShapeGizmo.Shutdown();
				}
			}
			if (flag)
			{
				base.enabled = false;
				base.GetComponent<Renderer>().enabled = false;
				LocalPlayer.Create.PlaceGhost(false);
			}
			if (!this._autoSupported)
			{
				this._caster.CastForAnchors<PrefabIdentifier>(new Action<PrefabIdentifier>(this.CheckTargetingSupport));
				bool flag10 = !this._autofillmode && this._multiPointsPositions.Count == 0 && this.CurrentSupport != null;
				bool canToggleAutofill = (flag4 || flag3 || this._multiPointsPositions.Count > 0) && this.CurrentSupport != null;
				bool canLock = !flag10 && flag4;
				bool canUnlock = !this._autofillmode && this._multiPointsPositions.Count > 0;
				bool showAutofillPlace = this._autofillmode && !flag2 && this._multiPointsPositions.Count >= 3;
				bool showManualPlace = !this._autofillmode && !flag2 && this._multiPointsPositions.Count >= ((!flag4) ? 3 : 2);
				Scene.HudGui.RoofConstructionIcons.Show(flag10, canToggleAutofill, showAutofillPlace, showManualPlace, canLock, canUnlock, false);
			}
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag3)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag3;
			}
			if (!this._autoSupported)
			{
				this._logMat.SetColor("_TintColor", (flag2 || (!flag4 && !flag3 && this._multiPointsPositions.Count <= 0)) ? LocalPlayer.Create.BuildingPlacer.RedMat.GetColor("_TintColor") : LocalPlayer.Create.BuildingPlacer.ClearMat.GetColor("_TintColor"));
			}
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
				if (this._multiPointsPositions.Count == 0)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					return;
				}
				if (Cheats.ResetHoles && !BoltNetwork.isClient)
				{
					this._holes.Clear();
					this._holesCount = 0;
				}
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
					this._floorRoot.transform.parent = base.transform;
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
				this.RefreshCurrentFloor();
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
				Transform ghostRoot = this._floorRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.$this._logItemId);
				List<GameObject> logs = new List<GameObject>();
				int amount = 0;
				IEnumerator enumerator = this._floorRoot.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						Transform transform2 = (Transform)obj;
						amount++;
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
				ri._amount += amount;
				ri.AddRuntimeObjects(logs.AsEnumerable<GameObject>().Reverse<GameObject>(), Prefabs.Instance.LogFloorBuiltPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
				this._floorRoot.transform.parent = base.transform;
				ghostRoot.transform.parent = base.transform;
				BoxCollider bc;
				if (!this._autoSupported)
				{
					this._craftStructure.GetComponent<Collider>().enabled = true;
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
					bottom.y -= this._logWidth / 2f;
					b.Encapsulate(bottom);
					Vector3 top = this._multiPointsPositions[0];
					top.y += this._logWidth / 2f;
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
				}
				else
				{
					bc = null;
				}
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

		private void CheckTargetingSupport(PrefabIdentifier structureRoot)
		{
			IStructureSupport structureSupport = (!structureRoot) ? null : structureRoot.GetComponent<IStructureSupport>();
			if (structureSupport != null)
			{
				if ((this.CurrentSupport == null || this.CurrentSupport.GetLevel() - structureSupport.GetLevel() < this._closureSnappingDistance) && (this._autoSupported || !this._autofillmode || !(structureSupport as MonoBehaviour).GetComponent<FloorArchitect>()))
				{
					base.GetComponent<Renderer>().enabled = false;
					this.CurrentSupport = structureSupport;
					if (this.CurrentSupport != null)
					{
						LocalPlayer.Create.BuildingPlacer.ForcedParent = (this.CurrentSupport as MonoBehaviour).gameObject;
					}
					this.SupportHeight = this.CurrentSupport.GetLevel() - 0.35f;
					this.UpdateAutoFill(false);
				}
			}
			else if (this.CurrentSupport != null)
			{
				this.CurrentSupport = null;
				this.UpdateAutoFill(false);
				if (this._multiPointsPositions.Count == 0)
				{
					base.GetComponent<Renderer>().enabled = true;
					this.RefreshCurrentFloor();
				}
			}
		}

		private void UpdateAutoFill(bool doCleanUp)
		{
			bool flag = !this._autoSupported && this._autofillmode && this.CurrentSupport != null && this.CurrentSupport.GetMultiPointsPositions(true) != null;
			if (flag)
			{
				this._multiPointsPositions = new List<Vector3>(this.CurrentSupport.GetMultiPointsPositions(true));
				int count = this._multiPointsPositions.Count;
				float num = this.CurrentSupport.GetHeight() - 0.35f;
				float num2 = this.CurrentSupport.GetLevel() - 0.35f;
				for (int i = this._multiPointsPositions.Count - 1; i >= 0; i--)
				{
					Vector3 value = this._multiPointsPositions[i];
					if (Mathf.Abs(value.y + num - num2) < this._closureSnappingDistance)
					{
						value.y = num2;
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
					this.RefreshCurrentFloor();
					base.GetComponent<Renderer>().enabled = false;
				}
				else
				{
					this._multiPointsPositions.Clear();
					base.GetComponent<Renderer>().enabled = true;
				}
			}
			else if (this._autofillmode || doCleanUp)
			{
				this._multiPointsPositions.Clear();
				this.RefreshCurrentFloor();
				base.GetComponent<Renderer>().enabled = true;
			}
		}

		public void OnBuilt(GameObject built)
		{
			FloorArchitect component = built.GetComponent<FloorArchitect>();
			component._multiPointsPositions = this._multiPointsPositions;
			component._holes = this._holes;
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
			if (this._floorRoot)
			{
				UnityEngine.Object.Destroy(this._floorRoot.gameObject);
			}
		}

		public void ClearTemp()
		{
			if (this._floorRoot)
			{
				UnityEngine.Object.Destroy(this._floorRoot.gameObject);
				this._floorRoot = null;
			}
			this._newPool.Clear();
			this._logPool.Clear();
			base.GetComponent<Renderer>().enabled = (this.CurrentSupport == null);
			Scene.HudGui.MultipointShapeGizmo.Shutdown();
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
				this._floorRoot.parent = base.transform;
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

		public void RefreshCurrentFloor()
		{
			if (this._logPool == null)
			{
				this._logPool = new Stack<Transform>();
			}
			this._newPool = new Stack<Transform>();
			Transform floorRoot = this._floorRoot;
			if (this._multiPointsPositions.Count >= 3)
			{
				this._floorRoot = new GameObject("FloorRoot").transform;
				this.SpawnFloor();
			}
			if (floorRoot)
			{
				UnityEngine.Object.Destroy(floorRoot.gameObject);
			}
			this._logPool = this._newPool;
		}

		public Vector3 GetCurrentEdgePoint()
		{
			Vector3 vector;
			if (this._multiPointsPositions.Count > 1)
			{
				vector = MathEx.TryAngleSnap((this.CurrentSupport != null) ? this._caster.LastHit.point : base.transform.position, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
			}
			else
			{
				vector = base.transform.position;
			}
			Vector3 result = (!this._autoSupported) ? ((this.CurrentSupport == null) ? vector : MathEx.ClosestPointOnPolygon(base.transform.position, this.CurrentSupport.GetMultiPointsPositions(false))) : vector;
			if (this._multiPointsPositions.Count > 0)
			{
				result.y = this._multiPointsPositions[0].y;
			}
			else if (!this._autoSupported)
			{
				result.y = this.SupportHeight;
			}
			return result;
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
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			Vector3 forward = this._multiPointsPositions[1] - this._multiPointsPositions[0];
			base.transform.rotation = Quaternion.LookRotation(forward);
			Vector3[] array = (from p in this._multiPointsPositions
			select base.transform.InverseTransformPoint(p)).ToArray<Vector3>();
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
			if (this._holes != null)
			{
				for (int k = 0; k < this._holes.Count; k++)
				{
					Vector3[] array6 = new Vector3[5];
					Hole hole = this._holes[k];
					hole._used = false;
					array6[0] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					array6[1] = base.transform.InverseTransformPoint(hole._position + new Vector3(hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
					array6[2] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, -hole._size.y).RotateY(hole._yRotation));
					array6[3] = base.transform.InverseTransformPoint(hole._position + new Vector3(-hole._size.x, 0f, hole._size.y).RotateY(hole._yRotation));
					array6[4] = array6[0];
					for (int l = 0; l < 5; l++)
					{
						array6[l].y = array[0].y;
					}
					this.InitRowPointsBuffer(this._rowCount, this._holesRowPointsBuffer, out this._holesRowPointsBuffer);
					this.CalcRowPointBufferForArray(array6, num, this._holesRowPointsBuffer);
					for (int m = 0; m < this._rowCount; m++)
					{
						this._rowPointsBuffer[m].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						this._holesRowPointsBuffer[m].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
						List<Vector3> list = this._rowPointsBuffer[m];
						List<Vector3> list2 = this._holesRowPointsBuffer[m];
						int num11 = list.Count;
						int count = list2.Count;
						for (int n = 1; n < num11; n += 2)
						{
							for (int num12 = 1; num12 < count; num12 += 2)
							{
								if (list[n - 1].x > list2[num12 - 1].x && list[n].x < list2[num12].x)
								{
									list.RemoveAt(n);
									list.RemoveAt(n - 1);
									hole._used = true;
									if (num12 + 2 >= count)
									{
										if (n + 2 >= num11)
										{
											break;
										}
										n -= 2;
										num11 -= 2;
									}
								}
								else if (list[n - 1].x < list2[num12 - 1].x && list[n].x > list2[num12].x)
								{
									list.Insert(n, list2[num12 - 1]);
									list.Insert(n + 1, list2[num12]);
									hole._used = true;
								}
								else if (list[n - 1].x > list2[num12 - 1].x && list[n - 1].x < list2[num12].x)
								{
									list[n - 1] = list2[num12];
									hole._used = true;
								}
								else if (list[n].x > list2[num12 - 1].x && list[n].x < list2[num12].x)
								{
									list[n] = list2[num12 - 1];
									hole._used = true;
								}
							}
						}
					}
				}
			}
			Transform transform = this._floorRoot;
			float num13 = 0f;
			float num14 = (this._maxScaleLogCost <= 0f) ? 0f : (this._maxLogScale / this._maxScaleLogCost);
			Vector3 one = Vector3.one;
			Quaternion rotation = Quaternion.LookRotation(base.transform.right);
			for (int num15 = 0; num15 < this._rowCount; num15++)
			{
				this._rowPointsBuffer[num15].Sort((Vector3 x1, Vector3 x2) => x1.x.CompareTo(x2.x));
				int count2 = this._rowPointsBuffer[num15].Count;
				for (int num16 = 1; num16 < count2; num16 += 2)
				{
					Vector3 vector = base.transform.TransformPoint(this._rowPointsBuffer[num15][num16 - 1]);
					Vector3 a = base.transform.TransformPoint(this._rowPointsBuffer[num15][num16]) - vector;
					float magnitude = a.magnitude;
					int num17 = Mathf.CeilToInt(magnitude / this._maxChunkLength);
					one.z = magnitude / (float)num17 / this._logLength;
					Vector3 b = a / (float)num17;
					for (int num18 = 0; num18 < num17; num18++)
					{
						Transform transform2 = this.NewLog(vector, rotation);
						transform2.parent = null;
						if (num13 > 0f)
						{
							num13 -= one.z;
							transform2.parent = transform;
							Vector3 localScale = one;
							localScale.z /= transform.localScale.z;
							transform2.localScale = localScale;
						}
						else
						{
							transform = transform2;
							num13 = num14 - one.z;
							transform2.parent = this._floorRoot;
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
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefabs[UnityEngine.Random.Range(0, this._logPrefabs.Length)], position, rotation);
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
			return Quaternion.Euler(UnityEngine.Random.Range(-0.35f, 0.35f), UnityEngine.Random.Range(-0.35f, 0.35f), UnityEngine.Random.Range(-1f, 1f));
		}

		[SerializeThis]
		public bool Enslaved { get; set; }

		public float GetLevel()
		{
			return this._multiPointsPositions[0].y + this.GetHeight();
		}

		public float GetHeight()
		{
			return this._logWidth * 0.45f;
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

		public Transform FloorRoot
		{
			get
			{
				return this._floorRoot;
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

		bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
		{
			return this.CurrentSupport == null || connection.ExistsOnRemote((this.CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>()) == ExistsResult.Yes;
		}

		[SerializeThis]
		public bool _wasPlaced;

		[SerializeThis]
		public bool _wasBuilt;

		public SpherecastAnchoring _caster;

		public bool _autoSupported;

		public Transform[] _logPrefabs;

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

		private Transform _floorRoot;

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
