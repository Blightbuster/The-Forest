using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	[AddComponentMenu("Buildings/Creation/Wall Architect")]
	public class WallArchitect : MonoBehaviour
	{
		private void Awake()
		{
			this._autofillmode = PlayerPreferences.ExWallAutofill;
			if (!LevelSerializer.IsDeserializing)
			{
				base.StartCoroutine(this.DelayedAwake());
			}
			else
			{
				base.enabled = false;
			}
		}

		private IEnumerator DelayedAwake()
		{
			this._logPool = new Queue<Transform>();
			this._newPool = new Queue<Transform>();
			this._edges = new List<WallArchitect.Edge>();
			this._wallRoot = new GameObject("WallRoot");
			if (this._multiPointsPositions == null)
			{
				this._multiPointsPositions = new List<Vector3>();
			}
			Vector3 rendererWorldSize = this._logRenderer.bounds.size;
			this._logLength = rendererWorldSize.z;
			this._logWidth = rendererWorldSize.x;
			this._maxSegmentHorizontalLength = this.MaxSegmentHorizontalLength;
			yield return null;
			if (LocalPlayer.Create)
			{
				LocalPlayer.Create.Grabber.ClosePlace();
			}
			yield break;
		}

		private void Update()
		{
			if (this._canPlayerToggleAutofillmode && TheForest.Utils.Input.GetButtonDown("Craft") && this.CurrentSupport != null)
			{
				this._canScriptToggleAutofillmode = false;
				this._autofillmode = !this._autofillmode;
				PlayerPreferences.ExWallAutofill = this._autofillmode;
				PlayerPrefs.SetInt("ExWallAutofill", (!PlayerPreferences.ExWallAutofill) ? 0 : 1);
				this._saveAutofill = true;
				this.UpdateAutoFill(true);
			}
			bool flag = false;
			bool flag3;
			if (!this._autofillmode)
			{
				bool flag2 = this._multiPointsPositions.Count > 0;
				if (flag2 && TheForest.Utils.Input.GetButtonDown("AltFire"))
				{
					this._tmpEdge.Clear();
					this._tmpEdge = null;
					if (this._multiPointsPositions.Count > 1)
					{
						this._edges[this._edges.Count - 1].Clear();
						this._edges.RemoveAt(this._edges.Count - 1);
					}
					this._multiPointsPositions.RemoveAt(this._multiPointsPositions.Count - 1);
				}
				Create.CanLock = this.CanLock;
				this._canPlaceLock = this.CanPlaceLock;
				flag3 = (this._canPlaceLock || this.CanPlaceNoLock);
				if ((Create.CanLock || this._canPlaceLock) && this._multiPointsPositions.Count > 0)
				{
					Vector3 vector = base.transform.position - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
					RaycastHit raycastHit;
					if (Physics.SphereCast(this._multiPointsPositions[this._multiPointsPositions.Count - 1] + Vector3.up, 0.5f, vector.normalized, out raycastHit, vector.magnitude, 1 << LayerMask.NameToLayer("treeMid")))
					{
						Create.CanLock = false;
						this._canPlaceLock = false;
						flag3 = false;
						LocalPlayer.Create.BuildingPlacer.Clear = false;
					}
				}
				if ((Create.CanLock && TheForest.Utils.Input.GetButtonDown("Fire1")) || (this._canPlaceLock && TheForest.Utils.Input.GetButtonDown("Build")))
				{
					this.LockCurrentPoint();
					base.GetComponent<Renderer>().enabled = false;
				}
				this.UpdateHeight();
				bool flag4 = this._multiPointsPositions.Count > 2 && Vector3.Distance(this._multiPointsPositions.First<Vector3>(), this._multiPointsPositions.Last<Vector3>()) > this._logLength;
				if (flag4 && TheForest.Utils.Input.GetButtonDown("Rotate"))
				{
					this._multiPointsPositions.Add(this._multiPointsPositions.First<Vector3>());
					this._newPool = new Queue<Transform>();
					WallArchitect.Edge edge = this.CreateEdge(this._multiPointsPositions[this._multiPointsPositions.Count - 2], this._multiPointsPositions[this._multiPointsPositions.Count - 1]);
					edge._root.parent = this._wallRoot.transform;
					this._edges.Add(edge);
					this._logPool.Clear();
					flag = true;
				}
				this.ShowTempWall();
			}
			else
			{
				flag3 = false;
				this.UpdateHeight();
				if (this._multiPointsPositions.Count > 1)
				{
					Create.CanLock = true;
					if (this._autofillmode && TheForest.Utils.Input.GetButtonDown("Build"))
					{
						flag = true;
					}
				}
				else
				{
					Create.CanLock = false;
					this.ShowTempWall();
				}
			}
			bool flag5 = LocalPlayer.Create.BuildingPlacer.OnDynamicClear && (Create.CanLock || this._canPlaceLock || flag3) && (this._multiPointsPositions.Count > 1 || (this._multiPointsPositions.Count == 1 && Vector3.Distance(base.transform.position, this._multiPointsPositions[this._multiPointsPositions.Count - 1]) > this.MinPlaceLockLength));
			if (LocalPlayer.Create.BuildingPlacer.Clear != flag5)
			{
				LocalPlayer.Create.BuildingPlacer.Clear = flag5;
			}
			if (flag)
			{
				LocalPlayer.Create.PlaceGhost(false);
			}
			else
			{
				this._caster.CastForAnchors<PrefabIdentifier>(new Action<PrefabIdentifier>(this.CheckTargetingSupport));
			}
			bool flag6 = !this._autofillmode && this._multiPointsPositions.Count == 0;
			bool canToggleAutofill = this._canPlayerToggleAutofillmode && this.CurrentSupport != null;
			bool canLock = !flag6 && Create.CanLock;
			bool canUnlock = !this._autofillmode && this._multiPointsPositions.Count > 0;
			bool showAutofillPlace = this._autofillmode && flag5;
			bool showManualPlace = !this._autofillmode && flag5;
			this.ConstructionIcons.Show(flag6, canToggleAutofill, showAutofillPlace, showManualPlace, canLock, canUnlock, false);
		}

		private void OnDestroy()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.WallConstructionIcons.Shutdown();
			}
			if (LocalPlayer.Create)
			{
				LocalPlayer.Create.Grabber.ClosePlace();
			}
			this.Clear();
			if (this._saveAutofill)
			{
				PlayerPrefs.Save();
			}
		}

		private void OnDeserialized()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private IEnumerator OnPlaced()
		{
			base.enabled = false;
			yield return null;
			if (!this._autofillmode && this.CanLock && this._multiPointsPositions.Last<Vector3>() != this._multiPointsPositions.First<Vector3>())
			{
				this.LockCurrentPoint();
			}
			if (Scene.HudGui)
			{
				Scene.HudGui.WallConstructionIcons.Shutdown();
			}
			if (this._tmpEdge != null && this._tmpEdge._root)
			{
				this._tmpEdge._root.parent = null;
				UnityEngine.Object.Destroy(this._tmpEdge._root.gameObject);
			}
			LocalPlayer.Create.Grabber.ClosePlace();
			int chunkCount = 0;
			float lengthCount = 0f;
			for (int i = 0; i < this._edges.Count; i++)
			{
				WallArchitect.Edge edge = this._edges[i];
				float height = (!this._autofillmode) ? edge._height : this._currentHeight;
				lengthCount += edge._hlength;
				for (int j = 0; j < edge._segments.Length; j++)
				{
					WallArchitect.HorizontalSegment horizontalSegment = edge._segments[j];
					if (this.CheckForDoubledUpGhost(horizontalSegment))
					{
						IStructureSupport segmentSupport = this.GetSegmentSupport(horizontalSegment);
						if (BoltNetwork.isRunning)
						{
							PlaceWallChunk placeWallChunk = PlaceWallChunk.Create(GlobalTargets.OnlyServer);
							CoopWallChunkToken coopWallChunkToken = new CoopWallChunkToken();
							coopWallChunkToken.P1 = horizontalSegment._p1;
							coopWallChunkToken.P2 = horizontalSegment._p2;
							coopWallChunkToken.PointsPositions = this.MultiPointsPositions.ToArray();
							coopWallChunkToken.Additions = WallChunkArchitect.Additions.Wall;
							coopWallChunkToken.Height = height;
							placeWallChunk.parent = ((segmentSupport == null) ? base.transform.GetComponentInParent<BoltEntity>() : (segmentSupport as Component).GetComponentInParent<BoltEntity>());
							placeWallChunk.token = coopWallChunkToken;
							placeWallChunk.prefab = this.ChunkPrefab.GetComponent<BoltEntity>().prefabId;
							placeWallChunk.support = null;
							placeWallChunk.Send();
						}
						else
						{
							WallChunkArchitect wallChunkArchitect = UnityEngine.Object.Instantiate<WallChunkArchitect>(this.ChunkPrefab);
							wallChunkArchitect.transform.parent = ((segmentSupport == null) ? base.transform.parent : (segmentSupport as Component).transform);
							wallChunkArchitect.transform.position = horizontalSegment._p1;
							wallChunkArchitect.transform.LookAt(horizontalSegment._p2);
							wallChunkArchitect.MultipointPositions = this._multiPointsPositions;
							wallChunkArchitect.P1 = horizontalSegment._p1;
							wallChunkArchitect.P2 = horizontalSegment._p2;
							wallChunkArchitect.CurrentSupport = segmentSupport;
							wallChunkArchitect.Height = height;
						}
						chunkCount++;
					}
				}
			}
			if (!base.GetType().IsSubclassOf(typeof(WallArchitect)))
			{
				EventRegistry.Achievements.Publish(TfEvent.Achievements.PlacedWall, Mathf.FloorToInt(lengthCount));
			}
			if (!BoltNetwork.isRunning)
			{
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				CoopDestroyPredictedGhost coopDestroyPredictedGhost = base.gameObject.AddComponent<CoopDestroyPredictedGhost>();
				coopDestroyPredictedGhost.count = (float)chunkCount;
				coopDestroyPredictedGhost.delay = 0.005f;
				base.gameObject.AddComponent<destroyAfter>().destroyTime = 2f;
			}
			yield break;
		}

		private void CheckTargetingSupport(PrefabIdentifier structureRoot)
		{
			IStructureSupport structureSupport = (!structureRoot) ? null : structureRoot.GetComponent<IStructureSupport>();
			if (structureSupport != null)
			{
				this.InitSupport(structureSupport);
			}
			else
			{
				RaycastHit raycastHit;
				if (Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out raycastHit, 2f, this._floorLayers))
				{
					if (raycastHit.collider.CompareTag("TerrainMain"))
					{
						if (this._autofillmode || this.CurrentSupport != null)
						{
							this.CurrentSupport = null;
							if (this._autofillmode || this._multiPointsPositions.Count == 0)
							{
								this._autofillmode = false;
								this.UpdateAutoFill(true);
							}
						}
						return;
					}
					IStructureSupport componentInParent = raycastHit.collider.GetComponentInParent<IStructureSupport>();
					if (componentInParent != null)
					{
						this.InitSupport(componentInParent);
						return;
					}
				}
				if (this.CurrentSupport != null)
				{
					this.CurrentSupport = null;
					this.UpdateAutoFill(false);
				}
			}
		}

		private void InitSupport(IStructureSupport support)
		{
			List<Vector3> multiPointsPositions = support.GetMultiPointsPositions(true);
			if (multiPointsPositions != null && multiPointsPositions.Count > 1)
			{
				if (!this._autofillmode && this._canScriptToggleAutofillmode && this._multiPointsPositions.Count == 0)
				{
					this._autofillmode = true;
				}
				this.CurrentSupport = support;
				this.UpdateAutoFill(false);
			}
		}

		private IStructureSupport GetSegmentSupport(WallArchitect.HorizontalSegment hs)
		{
			RaycastHit raycastHit;
			IStructureSupport structureSupport;
			if (Physics.SphereCast(Vector3.Lerp(hs._p1, hs._p2, 0.5f) + new Vector3(0f, 0.5f, 0f), 0.5f, Vector3.down, out raycastHit, 1f, this._caster._anchorLayers))
			{
				structureSupport = raycastHit.collider.GetComponentInParent<IStructureSupport>();
			}
			else
			{
				structureSupport = null;
			}
			return (structureSupport == null) ? this.CurrentSupport : structureSupport;
		}

		private void UpdateAutoFill(bool doCleanUp)
		{
			bool flag = this._autofillmode && this.CurrentSupport != null && this.CurrentSupport.GetMultiPointsPositions(true) != null;
			if (flag)
			{
				if (this._multiPointsPositions.Count > 0)
				{
					this.Clear();
					this._wallRoot = new GameObject("WallRoot");
					this._multiPointsPositions.Clear();
				}
				LocalPlayer.Create.BuildingPlacer.Clear = false;
				this._canPlaceLock = true;
				Create.CanLock = true;
				List<Vector3> multiPointsPositions = this.CurrentSupport.GetMultiPointsPositions(true);
				float height = this.CurrentSupport.GetHeight();
				if (multiPointsPositions.Count > 1)
				{
					List<Transform> list = new List<Transform>();
					for (int i = 0; i < multiPointsPositions.Count; i++)
					{
						Vector3 item = multiPointsPositions[i];
						item.y += height;
						this._multiPointsPositions.Add(item);
						if (i > 0)
						{
							this._newPool = new Queue<Transform>();
							WallArchitect.Edge edge = this.CreateEdge(this._multiPointsPositions[this._multiPointsPositions.Count - 2], this._multiPointsPositions[this._multiPointsPositions.Count - 1]);
							edge._root.parent = this._wallRoot.transform;
							this._edges.Add(edge);
							list.AddRange(this._newPool);
							this._newPool.Clear();
						}
					}
					this._logPool = new Queue<Transform>(list);
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
				this.Clear();
				this._wallRoot = new GameObject("WallRoot");
				this._multiPointsPositions.Clear();
				this.ShowTempWall();
				base.GetComponent<Renderer>().enabled = true;
			}
		}

		public void Clear()
		{
			if (this._edges != null)
			{
				foreach (WallArchitect.Edge edge in this._edges)
				{
					edge.Clear();
				}
				this._edges.Clear();
			}
			if (this._tmpEdge != null)
			{
				this._tmpEdge.Clear();
			}
			if (this._wallRoot)
			{
				UnityEngine.Object.Destroy(this._wallRoot);
			}
		}

		private bool CheckForDoubledUpGhost(WallArchitect.HorizontalSegment hs)
		{
			float maxDistance = 1f;
			Vector3 vector = Vector3.Cross((hs._p2 - hs._p1).normalized, Vector3.up) * 0.75f;
			Vector3 origin = Vector3.Lerp(hs._p1, hs._p2, 0.5f) + Vector3.up - vector;
			origin.y += 0.1f;
			int mask = LayerMask.GetMask(new string[]
			{
				"PickUp",
				"Prop"
			});
			int num = 1 << LayerMask.NameToLayer("PickUp") | 1 << LayerMask.NameToLayer("Prop");
			RaycastHit raycastHit;
			if (Physics.SphereCast(origin, 0.25f, vector, out raycastHit, maxDistance, mask))
			{
				Collider collider = raycastHit.collider;
				if (collider.transform.parent)
				{
					WallChunkArchitect component = collider.transform.parent.GetComponent<WallChunkArchitect>();
					if (component)
					{
						Vector3 normalized = (component.P1 - component.P2).normalized;
						float num2 = Vector3.Dot(normalized, (hs._p1 - hs._p2).normalized);
						if (Mathf.Abs(num2) > 0.98f)
						{
							float num3 = Vector3.Dot(normalized, (hs._p1 - component.P1).normalized);
							float num4 = Vector3.Dot(-normalized, (hs._p2 - component.P2).normalized);
							if (num2 > 0f)
							{
								if (num3 <= 0f || num4 <= 0f)
								{
									return false;
								}
							}
							else if (-num3 <= 0f || -num4 <= 0f)
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		private bool LockCurrentPoint()
		{
			bool result = false;
			if (this._multiPointsPositions.Count > 2 && Vector3.Distance(base.transform.position, this._multiPointsPositions[0]) < this._closureSnappingDistance)
			{
				this._multiPointsPositions.Add(this._multiPointsPositions[0]);
				result = true;
			}
			else
			{
				Vector3 supportSnappedPoint = this.GetSupportSnappedPoint();
				if (this._multiPointsPositions.Count > 1)
				{
					Vector3 item = MathEx.TryAngleSnap(supportSnappedPoint, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
					item.y = base.transform.position.y;
					this._multiPointsPositions.Add(item);
				}
				else
				{
					this._multiPointsPositions.Add(supportSnappedPoint);
				}
			}
			if (this._multiPointsPositions.Count > 1)
			{
				this._newPool = new Queue<Transform>();
				WallArchitect.Edge edge = this.CreateEdge(this._multiPointsPositions[this._multiPointsPositions.Count - 2], this._multiPointsPositions[this._multiPointsPositions.Count - 1]);
				edge._root.parent = this._wallRoot.transform;
				this._edges.Add(edge);
				this._logPool.Clear();
			}
			return result;
		}

		protected virtual Vector3 GetSupportSnappedPoint()
		{
			Vector3 vector;
			if (this.CurrentSupport == null)
			{
				vector = base.transform.position;
			}
			else
			{
				vector = MathEx.ClosestPointOnPolygon(base.transform.position, this.CurrentSupport.GetMultiPointsPositions(false));
				if (Vector3.Distance(vector, base.transform.position) < this._caster._anchorCastRadius * 2f)
				{
					if (Mathf.Abs(vector.y - base.transform.position.y) > this.CurrentSupport.GetHeight() / 2f)
					{
						vector.y += this.CurrentSupport.GetHeight();
					}
				}
				else
				{
					vector = base.transform.position;
				}
			}
			return vector;
		}

		protected virtual void UpdateHeight()
		{
			float y = base.transform.position.y;
			float y2 = LocalPlayer.Create.BuildingPlacer.LookingAtPointCam.y;
			float value = (float)Mathf.RoundToInt((y2 - y) / this._logWidth);
			this._currentHeight = Mathf.Clamp(value, this._minHeight, this._maxHeight);
		}

		protected virtual void ShowTempWall()
		{
			Vector3 p = (this._multiPointsPositions.Count <= 0) ? (base.transform.position - LocalPlayer.MainCamTr.right / 2f) : this._multiPointsPositions[this._multiPointsPositions.Count - 1];
			Vector3 p2;
			if (this._multiPointsPositions.Count > 2 && Vector3.Distance(base.transform.position, this._multiPointsPositions[0]) < this._closureSnappingDistance)
			{
				p2 = this._multiPointsPositions[0];
			}
			else if (this._multiPointsPositions.Count == 0)
			{
				p2 = base.transform.position + LocalPlayer.MainCamTr.right / 2f;
			}
			else
			{
				Vector3 supportSnappedPoint = this.GetSupportSnappedPoint();
				if (this._multiPointsPositions.Count > 1)
				{
					p2 = MathEx.TryAngleSnap(supportSnappedPoint, this._multiPointsPositions[this._multiPointsPositions.Count - 1], this._multiPointsPositions[this._multiPointsPositions.Count - 2], PlayerPreferences.BuildingSnapAngle, PlayerPreferences.BuildingSnapRange, true);
					p2.y = base.transform.position.y;
				}
				else
				{
					p2 = supportSnappedPoint;
				}
			}
			GameObject gameObject = null;
			if (this._tmpEdge != null && this._tmpEdge._root)
			{
				gameObject = this._tmpEdge._root.gameObject;
			}
			this._newPool = new Queue<Transform>();
			this._tmpEdge = this.CreateEdge(p, p2);
			this._tmpEdge._root.name = "TempWall";
			this._logPool = this._newPool;
			if (gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}

		private Vector3 GetSegmentPointFloorPosition(bool aboveTerrain, Vector3 segmentPoint, out bool holeBellow)
		{
			if (!this._checkGround)
			{
				holeBellow = false;
				return segmentPoint;
			}
			Vector3 origin = segmentPoint;
			if (aboveTerrain)
			{
				origin.y = Mathf.Max(Terrain.activeTerrain.SampleHeight(segmentPoint), segmentPoint.y);
			}
			origin.y += this.SegmentPointTestOffset;
			RaycastHit raycastHit;
			if (Physics.SphereCast(origin, Mathf.Max(this._logWidth * 0.4f, 0.4f), Vector3.up, out raycastHit, 250f, Scene.ValidateFloorLayers(segmentPoint, this._floorLayers.value)))
			{
				origin.y = raycastHit.point.y - this.SegmentPointTestOffset / 2f;
			}
			if (Physics.SphereCast(origin, Mathf.Max(this._logWidth * 0.4f, 0.4f), Vector3.down, out raycastHit, 500f, Scene.ValidateFloorLayers(segmentPoint, this._floorLayers.value)))
			{
				holeBellow = (Mathf.Abs(raycastHit.point.y - segmentPoint.y) > this.SegmentPointTestOffset);
				return raycastHit.point;
			}
			holeBellow = true;
			return segmentPoint;
		}

		protected WallArchitect.Edge CreateEdge(Vector3 p1, Vector3 p2)
		{
			WallArchitect.Edge edge = this.CalcEdge(p1, p2);
			edge._root = this.SpawnEdge(edge);
			edge._height = this._currentHeight;
			return edge;
		}

		private WallArchitect.Edge CalcEdge(Vector3 p1, Vector3 p2)
		{
			int num = 0;
			Vector3 a = p1;
			WallArchitect.Edge edge = new WallArchitect.Edge
			{
				_p1 = p1,
				_p2 = p2
			};
			bool aboveTerrain = p1.y - Terrain.activeTerrain.SampleHeight(p1) > -1f;
			edge._hlength = Vector3.Scale(p2 - p1, new Vector3(1f, 0f, 1f)).magnitude;
			edge._segments = new WallArchitect.HorizontalSegment[Mathf.CeilToInt(edge._hlength / this._maxSegmentHorizontalLength)];
			float d = edge._hlength / (float)edge._segments.Length;
			float num2 = this._logWidth / 2f * 0.98f;
			Vector3 b = (p2 - p1).normalized * d;
			Vector3 vector = p1;
			for (int i = 0; i < edge._segments.Length; i++)
			{
				bool flag = false;
				WallArchitect.HorizontalSegment horizontalSegment = new WallArchitect.HorizontalSegment();
				horizontalSegment._p1 = vector;
				horizontalSegment._p2 = ((i + 1 != edge._segments.Length) ? this.GetSegmentPointFloorPosition(aboveTerrain, vector + b, out flag) : p2);
				horizontalSegment._axis = (horizontalSegment._p2 - horizontalSegment._p1).normalized * d;
				horizontalSegment._length = Vector3.Distance(horizontalSegment._p1, horizontalSegment._p2);
				if (num == 1 && !flag)
				{
					Vector3 vector2 = Vector3.Lerp(a, horizontalSegment._p2, 0.5f);
					vector2.y += this.SegmentPointTestOffset / 2f;
					horizontalSegment._p1 = vector2;
					vector2.y += num2;
					edge._segments[i - 1]._p2 = vector2;
				}
				if (flag && edge._segments.Length == 2)
				{
					num++;
				}
				else
				{
					num = 0;
					a = horizontalSegment._p2;
				}
				vector = horizontalSegment._p2;
				WallArchitect.HorizontalSegment horizontalSegment2 = horizontalSegment;
				horizontalSegment2._p1.y = horizontalSegment2._p1.y + num2;
				WallArchitect.HorizontalSegment horizontalSegment3 = horizontalSegment;
				horizontalSegment3._p2.y = horizontalSegment3._p2.y + num2;
				edge._segments[i] = horizontalSegment;
			}
			return edge;
		}

		protected virtual Transform SpawnEdge(WallArchitect.Edge edge)
		{
			Transform transform = new GameObject("WallEdge").transform;
			transform.transform.position = edge._p1;
			Vector3 vector = edge._p2 - edge._p1;
			int num = Mathf.RoundToInt(this._currentHeight);
			if (this._autofillmode || Vector3.Distance(edge._p1, edge._p2) > this._logWidth * 3.5f)
			{
				Vector3 b = new Vector3(0f, this._logWidth * 0.95f, 0f);
				for (int i = 0; i < edge._segments.Length; i++)
				{
					WallArchitect.HorizontalSegment horizontalSegment = edge._segments[i];
					Quaternion rotation = Quaternion.LookRotation(horizontalSegment._axis);
					Vector3 vector2 = horizontalSegment._p1;
					Transform transform2 = new GameObject("Segment" + i).transform;
					transform2.parent = transform;
					transform2.LookAt(horizontalSegment._axis);
					horizontalSegment._root = transform2;
					transform2.position = horizontalSegment._p1;
					Vector3 localScale = new Vector3(1f, 1f, horizontalSegment._length / this._logLength);
					Vector3 vector3 = new Vector3(1f, 1f, 0.31f + (localScale.z - 1f) / 2f);
					float num2 = 1f - vector3.z / localScale.z;
					for (int j = 0; j < num; j++)
					{
						Transform transform3 = this.NewLog(vector2, rotation);
						transform3.parent = transform2;
						this._newPool.Enqueue(transform3);
						transform3.localScale = localScale;
						vector2 += b;
					}
				}
			}
			else
			{
				Vector3 normalized = Vector3.Scale(vector, new Vector3(1f, 0f, 1f)).normalized;
				float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._logWidth;
				Quaternion rotation2 = Quaternion.LookRotation(Vector3.up);
				Vector3 localScale2 = new Vector3(1f, 1f, (float)num * 0.95f * this._logWidth / this._logLength);
				float num3 = this._logWidth / 2f * 0.98f;
				for (int k = 0; k < edge._segments.Length; k++)
				{
					WallArchitect.HorizontalSegment horizontalSegment2 = edge._segments[k];
					Vector3 vector4 = horizontalSegment2._p1;
					vector4.y -= num3;
					Transform transform4 = new GameObject("Segment" + k).transform;
					transform4.parent = transform;
					horizontalSegment2._root = transform4;
					transform4.position = horizontalSegment2._p1;
					float num4 = Vector3.Distance(horizontalSegment2._p1, horizontalSegment2._p2);
					int num5 = Mathf.Max(Mathf.RoundToInt((num4 - this._logWidth * 0.96f / 2f) / this._logWidth), 1);
					Vector3 vector5 = normalized * this._logWidth * 0.98f;
					vector5.y = y;
					vector4 += vector5 / 2f;
					if (vector.y < 0f)
					{
						vector5.y *= -1f;
					}
					for (int l = 0; l < num5; l++)
					{
						Transform transform5 = this.NewLog(vector4, rotation2);
						transform5.parent = transform4;
						this._newPool.Enqueue(transform5);
						vector4 += vector5;
						transform5.localScale = localScale2;
					}
				}
			}
			return transform;
		}

		protected Transform NewLog(Vector3 position, Quaternion rotation)
		{
			if (this._logPool.Count > 0)
			{
				Transform transform = this._logPool.Dequeue();
				transform.position = position;
				transform.rotation = rotation;
				transform.GetComponentInChildren<Renderer>().sharedMaterial = ((!Create.CanLock && !this._canPlaceLock) ? Prefabs.Instance.GhostBlocked : Prefabs.Instance.GhostClear);
				return transform;
			}
			Transform transform2 = UnityEngine.Object.Instantiate<Transform>(this._logPrefab, position, rotation);
			transform2.GetComponentInChildren<Renderer>().sharedMaterial = ((!Create.CanLock && !this._canPlaceLock) ? Prefabs.Instance.GhostBlocked : Prefabs.Instance.GhostClear);
			return transform2;
		}

		private void CreateStructure()
		{
			for (int i = 1; i < this._multiPointsPositions.Count; i++)
			{
				WallArchitect.Edge item = this.CalcEdge(this._multiPointsPositions[i - 1], this._multiPointsPositions[i]);
				this._edges.Add(item);
			}
		}

		private bool CanLock
		{
			get
			{
				bool flag = LocalPlayer.Create.BuildingPlacer.OnDynamicClear && this._multiPointsPositions.Count < this._maxPoints && (this.CurrentSupport == null || this.CurrentSupport.GetMultiPointsPositions(true) != null);
				if (flag && this._multiPointsPositions.Count > 0)
				{
					Vector3 to = base.transform.position - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
					flag = (to.sqrMagnitude > this.SqrMinLockLength);
					if (this._multiPointsPositions.Count > 1)
					{
						Vector3 from = this._multiPointsPositions[this._multiPointsPositions.Count - 2] - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
						flag = (flag && Vector3.Angle(from, to) >= this._minAngleBetweenEdges);
					}
				}
				return flag;
			}
		}

		protected virtual float MinLockLength
		{
			get
			{
				return this._logLength;
			}
		}

		protected virtual float SqrMinLockLength
		{
			get
			{
				return this.MinLockLength * this.MinLockLength;
			}
		}

		private bool CanPlaceLock
		{
			get
			{
				bool flag = LocalPlayer.Create.BuildingPlacer.OnDynamicClear && this._multiPointsPositions.Count > 0 && this._multiPointsPositions.Count < this._maxPoints && (this.CurrentSupport == null || this.CurrentSupport.GetMultiPointsPositions(true) != null);
				if (flag)
				{
					Vector3 to = base.transform.position - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
					flag = (to.sqrMagnitude > this.SqrMinPlaceLockLength);
					if (this._multiPointsPositions.Count > 1)
					{
						Vector3 from = this._multiPointsPositions[this._multiPointsPositions.Count - 2] - this._multiPointsPositions[this._multiPointsPositions.Count - 1];
						flag = (flag && Vector3.Angle(from, to) >= this._minAngleBetweenEdges);
					}
				}
				return flag;
			}
		}

		private bool CanPlaceNoLock
		{
			get
			{
				bool flag = LocalPlayer.Create.BuildingPlacer.OnDynamicClear && this._multiPointsPositions.Count > 0 && this._multiPointsPositions.Count < this._maxPoints && (this.CurrentSupport == null || this.CurrentSupport.GetMultiPointsPositions(true) != null);
				if (flag)
				{
					flag = ((base.transform.position - this._multiPointsPositions[this._multiPointsPositions.Count - 1]).sqrMagnitude <= this.SqrMinPlaceLockLength);
				}
				return flag;
			}
		}

		protected virtual float MinPlaceLockLength
		{
			get
			{
				return 0.5f;
			}
		}

		protected virtual float SqrMinPlaceLockLength
		{
			get
			{
				return this.MinPlaceLockLength * this.MinPlaceLockLength;
			}
		}

		public virtual float MaxSegmentHorizontalLength
		{
			get
			{
				return this._logRenderer.bounds.size.z * this._maxSegmentHorizontalScale;
			}
		}

		public virtual float MaxSegments
		{
			get
			{
				return 12f;
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

		protected virtual float SegmentPointTestOffset
		{
			get
			{
				return this._logWidth * 3.5f;
			}
		}

		private IStructureSupport CurrentSupport { get; set; }

		protected virtual WallChunkArchitect ChunkPrefab
		{
			get
			{
				return (!this._autofillmode || !this._autofillChunkArchitectPrefab) ? this._chunkArchitectPrefab : this._autofillChunkArchitectPrefab;
			}
		}

		protected virtual ConstructionIcons ConstructionIcons
		{
			get
			{
				return Scene.HudGui.WallConstructionIcons;
			}
		}

		[SerializeThis]
		public bool Enslaved { get; set; }

		public float GetLevel()
		{
			return this._multiPointsPositions[0].y + this.GetHeight();
		}

		public virtual float GetHeight()
		{
			return 4.3f * this._logWidth;
		}

		public List<Vector3> GetMultiPointsPositions()
		{
			return this._multiPointsPositions;
		}

		public SpherecastAnchoring _caster;

		public WallChunkArchitect _chunkArchitectPrefab;

		public WallChunkArchitect _autofillChunkArchitectPrefab;

		public Transform _logPrefab;

		public Renderer _logRenderer;

		public float _closureSnappingDistance = 2.5f;

		public float _maxSegmentHorizontalScale = 2f;

		public int _maxPoints = 50;

		public float _minAngleBetweenEdges = 90f;

		public LayerMask _floorLayers;

		public float _minHeight;

		public float _maxHeight;

		protected float _currentHeight = 5f;

		protected bool _autofillmode = true;

		private bool _saveAutofill;

		private bool _canPlaceLock;

		protected bool _canScriptToggleAutofillmode = true;

		protected bool _canPlayerToggleAutofillmode = true;

		protected bool _checkGround = true;

		protected List<Vector3> _multiPointsPositions;

		protected GameObject _wallRoot;

		protected List<WallArchitect.Edge> _edges;

		protected WallArchitect.Edge _tmpEdge;

		protected float _logLength;

		protected float _logWidth;

		protected float _maxSegmentHorizontalLength;

		protected Queue<Transform> _logPool;

		protected Queue<Transform> _newPool;

		[Serializable]
		public class HorizontalSegment
		{
			public void Clear()
			{
				this._root = null;
			}

			public Vector3 _p1;

			public Vector3 _p2;

			public Vector3 _axis;

			public float _length;

			public Transform _root;
		}

		[Serializable]
		public class Edge
		{
			public void Clear()
			{
				if (this._segments != null)
				{
					foreach (WallArchitect.HorizontalSegment horizontalSegment in this._segments)
					{
						horizontalSegment.Clear();
					}
					this._segments = null;
				}
				if (this._root != null)
				{
					UnityEngine.Object.Destroy(this._root.gameObject);
					this._root = null;
				}
			}

			public Vector3 _p1;

			public Vector3 _p2;

			public float _hlength;

			public WallArchitect.HorizontalSegment[] _segments;

			public Transform _root;

			public float _height;
		}

		[Serializable]
		public class WallAddition
		{
			public int _edgeNum;

			public int _segmentNum;
		}
	}
}
