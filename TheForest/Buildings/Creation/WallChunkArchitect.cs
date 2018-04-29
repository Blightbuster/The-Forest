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
	
	[DoNotSerializePublic]
	[AddComponentMenu("Buildings/Creation/Wall Chunk Architect")]
	public class WallChunkArchitect : EntityBehaviour, IEntityReplicationFilter, IProceduralStructure, IStructureSupport, ICoopStructure
	{
		
		bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
		{
			return this.CurrentSupport == null || connection.ExistsOnRemote((this.CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>()) == ExistsResult.Yes;
		}

		
		protected virtual void Awake()
		{
			base.StartCoroutine(this.DelayedAwake(LevelSerializer.IsDeserializing));
			base.enabled = false;
		}

		
		protected IEnumerator DelayedAwake(bool isDeserializing)
		{
			Vector3 rendererWorldSize = this._logRenderer.bounds.size;
			this._logLength = rendererWorldSize.z;
			this._logWidth = rendererWorldSize.x;
			yield return null;
			if (this._forceWasBuilt)
			{
				this._wasBuilt = true;
			}
			if (!isDeserializing)
			{
				base.StartCoroutine(this.OnDeserialized());
				this._wallRoot.transform.parent = base.transform;
				if (this._wasBuilt && !BoltNetwork.isClient && !BoltNetwork.isClient && LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayBuildingComplete(base.gameObject, true);
				}
			}
			if (this._craftStructure)
			{
				this._craftStructure.OnBuilt = new Action<GameObject>(this.OnBuilt);
				this._craftStructure._playTwinkle = false;
			}
			yield break;
		}

		
		private void OnSerializing()
		{
			this._multiPointsPositions.Capacity = this._multiPointsPositions.Count;
			this._multiPointsPositionsCount = this._multiPointsPositions.Count;
		}

		
		protected virtual IEnumerator OnDeserialized()
		{
			if (!this._initialized)
			{
				this._initialized = true;
				if (this._forceWasBuilt)
				{
					this._wasBuilt = true;
				}
				if (this._p1 == Vector3.zero)
				{
					this._p1 = base.transform.position;
				}
				if (this._p2 == Vector3.zero)
				{
					PrefabIdentifier pi = base.GetComponent<PrefabIdentifier>();
					if (pi && !pi.Components.Contains(base.GetType().FullName))
					{
						pi.Components.Add(base.GetType().FullName);
					}
					Transform parentStructure = base.transform.parent;
					while (parentStructure && parentStructure.parent)
					{
						parentStructure = parentStructure.parent;
						yield return null;
					}
					IStructureSupport structureSupport;
					if (base.transform.parent)
					{
						IStructureSupport component = base.transform.parent.GetComponent<IStructureSupport>();
						structureSupport = component;
					}
					else
					{
						structureSupport = null;
					}
					IStructureSupport support = structureSupport;
					if (support != null)
					{
						this.CurrentSupport = support;
						List<Vector3> supportPoints = support.GetMultiPointsPositions(true);
						if (supportPoints != null && supportPoints.Count > 1)
						{
							Vector3 testPos = this._p1;
							testPos.y = supportPoints[0].y;
							int p2i = -1;
							int p1i;
							for (p1i = 1; p1i < supportPoints.Count; p1i++)
							{
								Vector3 projection = MathEx.ProjectPointOnLineSegment(supportPoints[p1i], supportPoints[p1i - 1], testPos);
								float distance = Vector3.Distance(projection, testPos);
								if (distance < 0.025f)
								{
									if (Vector3.Dot(supportPoints[p1i] - supportPoints[p1i - 1], base.transform.forward) > 0.98f)
									{
										p2i = p1i - 1;
										break;
									}
									if (Vector3.Dot(supportPoints[p1i] - supportPoints[(p1i + 1) % supportPoints.Count], base.transform.forward) > 0.98f)
									{
										p2i = (p1i - 1) % supportPoints.Count;
										break;
									}
								}
							}
							if (p2i > -1)
							{
								Vector3 edge = supportPoints[p1i] - supportPoints[p2i];
								float edgeLength = Vector3.Scale(edge, new Vector3(1f, 0f, 1f)).magnitude;
								int chunkCount = Mathf.CeilToInt(edgeLength / this._architect.MaxSegmentHorizontalLength);
								float chunkLength = edgeLength / (float)chunkCount;
								this._p2 = this._p1 + base.transform.forward * chunkLength;
							}
						}
					}
					if (this._p2 == Vector3.zero)
					{
						this._p2 = this._p1 + base.transform.forward * this._architect.MaxSegmentHorizontalLength * 0.95f;
					}
				}
				if (this._height == 0f)
				{
					this._height = 5f;
				}
				if (this._multiPointsPositions == null)
				{
					this._multiPointsPositions = new List<Vector3>
					{
						this._p1,
						this._p2
					};
					this._multiPointsPositionsCount = 2;
				}
				else if (this._multiPointsPositionsCount == 0)
				{
					this._multiPointsPositionsCount = this._multiPointsPositions.Skip(1).IndexOf(this._multiPointsPositions[0]);
					if (this._multiPointsPositionsCount < 0)
					{
						this._multiPointsPositionsCount = this._multiPointsPositions.Count;
					}
					else
					{
						this._multiPointsPositionsCount += 2;
					}
				}
				if (this._multiPointsPositions.Count > this._multiPointsPositionsCount)
				{
					this._multiPointsPositions.RemoveRange(this._multiPointsPositionsCount, this._multiPointsPositions.Count - this._multiPointsPositionsCount);
				}
				if (this._wasBuilt)
				{
					this.CreateStructure(false);
					this._wallRoot.transform.parent = base.transform;
				}
				else
				{
					this.CreateStructure(false);
					base.StartCoroutine(this.OnPlaced());
				}
			}
			yield break;
		}

		
		private IEnumerator OnPlaced()
		{
			base.enabled = false;
			if (!this._craftStructure)
			{
				this._craftStructure = base.GetComponentInChildren<Craft_Structure>();
			}
			try
			{
				this._craftStructure.GetComponent<Collider>().enabled = false;
			}
			catch (Exception ex)
			{
				Exception exn = ex;
				Debug.LogError(exn);
			}
			yield return null;
			if (this._craftStructure)
			{
				Transform ghostRoot = this._wallRoot;
				Transform transform = ghostRoot;
				transform.name += "Ghost";
				Transform logGhostPrefab = this._logPrefab;
				this._logPrefab = this.BuiltLogPrefab;
				Transform wallBuiltTr = this.SpawnStructure();
				wallBuiltTr.parent = base.transform;
				this.InitAdditionTrigger();
				Craft_Structure.BuildIngredients ri = this._craftStructure._requiredIngredients.FirstOrDefault((Craft_Structure.BuildIngredients i) => i._itemID == this.<>f__this._logItemId);
				if (ri == null)
				{
					ri = new Craft_Structure.BuildIngredients();
					ri._itemID = this._logItemId;
					ri._amount = 0;
					ri._renderers = new GameObject[0];
					this._craftStructure._requiredIngredients.Insert(0, ri);
				}
				ri._amount += this.GetLogCost();
				List<GameObject> builtRenderers = this.GetBuiltRenderers(wallBuiltTr);
				ri._renderers = builtRenderers.ToArray();
				this._logPrefab = logGhostPrefab;
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
				Vector3 axis = base.transform.InverseTransformVector(this._p2 - this._p1);
				bc.size = new Vector3(Mathf.Abs(axis.x), 4.5f, Mathf.Abs(axis.z));
				this._craftStructure.transform.localPosition = bc.size / 2f;
				yield return null;
				this._craftStructure.manualLoading = true;
				while (LevelSerializer.IsDeserializing && !this._craftStructure.WasLoaded)
				{
					yield return null;
				}
				this._craftStructure.Initialize();
				this._craftStructure.gameObject.SetActive(true);
				base.GetComponent<Renderer>().enabled = false;
				base.enabled = false;
				yield return null;
				bc.enabled = false;
				bc.enabled = true;
			}
			yield break;
		}

		
		protected virtual void OnBuilt(GameObject built)
		{
			this.HideToggleAdditionIcon();
			WallChunkArchitect component = built.GetComponent<WallChunkArchitect>();
			component._multiPointsPositions = this._multiPointsPositions;
			component._p1 = this._p1;
			component._p2 = this._p2;
			component._addition = this._addition;
			component._height = this._height;
			component._wasBuilt = true;
			component.CurrentSupport = this.CurrentSupport;
			if (this.CurrentSupport != null)
			{
				this.CurrentSupport.Enslaved = true;
			}
		}

		
		protected virtual void OnBeginCollapse()
		{
			if (this._gridToken >= 0)
			{
				InsideCheck.RemoveWallChunk(this._gridToken);
			}
		}

		
		protected virtual int GetLogCost()
		{
			float num = Vector3.Distance(this._p1, this._p2);
			return (num <= this._logWidth * 3.5f) ? this._wallRoot.childCount : ((int)this._height);
		}

		
		public virtual void ShowToggleAdditionIcon()
		{
			WallChunkArchitect.Additions additions = this.SegmentNextAddition(this._addition);
			switch (additions + 1)
			{
			case WallChunkArchitect.Additions.Window:
				if (!Scene.HudGui.ToggleWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleWallIcon.SetActive(true);
				}
				if (Scene.HudGui.ToggleDoor1Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor1Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleDoor2Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor2Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleWindowIcon.activeSelf)
				{
					Scene.HudGui.ToggleWindowIcon.SetActive(false);
				}
				break;
			case WallChunkArchitect.Additions.Door1:
				if (Scene.HudGui.ToggleWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleWallIcon.SetActive(false);
				}
				if (Scene.HudGui.ToggleDoor1Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor1Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleDoor2Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor2Icon.SetActive(false);
				}
				if (!Scene.HudGui.ToggleWindowIcon.activeSelf)
				{
					Scene.HudGui.ToggleWindowIcon.SetActive(true);
				}
				break;
			case WallChunkArchitect.Additions.Door2:
				if (Scene.HudGui.ToggleWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleWallIcon.SetActive(false);
				}
				if (!Scene.HudGui.ToggleDoor1Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor1Icon.SetActive(true);
				}
				if (Scene.HudGui.ToggleDoor2Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor2Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleWindowIcon.activeSelf)
				{
					Scene.HudGui.ToggleWindowIcon.SetActive(false);
				}
				break;
			case WallChunkArchitect.Additions.LockedDoor1:
				if (Scene.HudGui.ToggleWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleWallIcon.SetActive(false);
				}
				if (Scene.HudGui.ToggleDoor1Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor1Icon.SetActive(false);
				}
				if (!Scene.HudGui.ToggleDoor2Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor2Icon.SetActive(true);
				}
				if (Scene.HudGui.ToggleWindowIcon.activeSelf)
				{
					Scene.HudGui.ToggleWindowIcon.SetActive(false);
				}
				break;
			}
		}

		
		public virtual void HideToggleAdditionIcon()
		{
			if (Scene.HudGui)
			{
				if (Scene.HudGui.ToggleWallIcon.activeSelf)
				{
					Scene.HudGui.ToggleWallIcon.SetActive(false);
				}
				if (Scene.HudGui.ToggleDoor1Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor1Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleDoor2Icon.activeSelf)
				{
					Scene.HudGui.ToggleDoor2Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleGate1Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate1Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleGate2Icon.activeSelf)
				{
					Scene.HudGui.ToggleGate2Icon.SetActive(false);
				}
				if (Scene.HudGui.ToggleWindowIcon.activeSelf)
				{
					Scene.HudGui.ToggleWindowIcon.SetActive(false);
				}
			}
		}

		
		public void ToggleSegmentAddition()
		{
			if (BoltNetwork.isRunning)
			{
				this.HideToggleAdditionIcon();
				this.ShowToggleAdditionIcon();
				ToggleWallAddition toggleWallAddition = ToggleWallAddition.Create(GlobalTargets.OnlyServer);
				toggleWallAddition.Wall = base.GetComponent<BoltEntity>();
				toggleWallAddition.Send();
			}
			else
			{
				this.HideToggleAdditionIcon();
				this.PerformToggleAddition();
				this.ShowToggleAdditionIcon();
			}
		}

		
		public virtual void PerformToggleAddition()
		{
			if (BoltNetwork.isRunning)
			{
				this.entity.GetState<IWallChunkConstructionState>().Addition = (int)(this._addition = this.SegmentNextAddition(this._addition));
			}
			else
			{
				this.UpdateAddition(this.SegmentNextAddition(this._addition));
			}
		}

		
		public virtual void UpdateAddition(WallChunkArchitect.Additions addition)
		{
			this._addition = addition;
			UnityEngine.Object.Destroy(this._wallRoot.gameObject);
			this._wallRoot = this.SpawnStructure();
			this._wallRoot.parent = base.transform;
			if (!this._wasBuilt && this._addition >= WallChunkArchitect.Additions.Door1 && this._addition <= WallChunkArchitect.Additions.LockedDoor2)
			{
				Vector3 position = Vector3.Lerp(this._p1, this._p2, 0.5f);
				position.y -= this._logWidth / 2f;
				Vector3 worldPosition = (this._addition != WallChunkArchitect.Additions.Door1) ? this._p1 : this._p2;
				worldPosition.y = position.y;
				Transform transform = (Transform)UnityEngine.Object.Instantiate(Prefabs.Instance.DoorGhostPrefab, position, this._wallRoot.rotation);
				transform.LookAt(worldPosition);
				transform.parent = this._wallRoot;
			}
		}

		
		public void Clear()
		{
			if (this._wallRoot)
			{
				UnityEngine.Object.Destroy(this._wallRoot.gameObject);
			}
		}

		
		protected virtual void CreateStructure(bool isRepair = false)
		{
			if (isRepair)
			{
				this.Clear();
				base.StartCoroutine(this.DelayedAwake(true));
			}
			int num = LayerMask.NameToLayer("Prop");
			this._wallRoot = this.SpawnStructure();
			this._wallRoot.parent = base.transform;
			if (this._wasBuilt)
			{
				this._gridToken = InsideCheck.AddWallChunk(this._p1, this._p2, 4.75f * this._logWidth + 1f);
				if (this._lods)
				{
					UnityEngine.Object.Destroy(this._lods.gameObject);
				}
				this._lods = new GameObject("lods").AddComponent<WallChunkLods>();
				this._lods.transform.parent = base.transform;
				this._lods.DefineChunk(this._p1, this._p2, 4.44f * this._logWidth, this._wallRoot, this.Addition);
				BuildingHealth component = base.GetComponent<BuildingHealth>();
				if (component)
				{
					component._renderersRoot = this._wallRoot.gameObject;
				}
				if (!this._wallCollision)
				{
					GameObject gameObject = new GameObject("collision");
					gameObject.transform.parent = this._wallRoot.parent;
					gameObject.transform.position = this._wallRoot.position;
					gameObject.transform.rotation = this._wallRoot.rotation;
					gameObject.tag = "structure";
					this._wallCollision = gameObject.transform;
					GameObject gameObject2 = this._wallRoot.gameObject;
					int layer = num;
					gameObject.layer = layer;
					gameObject2.layer = layer;
					float num3;
					float num4;
					Vector3 size;
					Vector3 vector;
					if (this.UseHorizontalLogs)
					{
						float num2 = Vector3.Distance(this._p1, this._p2) / this._logLength;
						num3 = 7.4f * num2;
						num4 = 6.75f * (0.31f + (num2 - 1f) / 2f);
						size = new Vector3(1.75f, 0.9f * this._height * this._logWidth, num3 * 1f);
						vector = Vector3.Lerp(this._p1, this._p2, 0.5f);
						vector = this._wallRoot.InverseTransformPoint(vector);
						vector.y = size.y / 2f - this._logWidth / 2f;
					}
					else
					{
						num3 = this._logWidth * (float)(this._wallRoot.childCount - 1) + 1.5f;
						num4 = 0f;
						vector = Vector3.zero;
						foreach (object obj in this._wallRoot)
						{
							Transform transform = (Transform)obj;
							vector += transform.position;
						}
						size = new Vector3(1.75f, 0.92f * this._height * this._logWidth, num3);
						vector /= (float)this._wallRoot.childCount;
						vector = this._wallRoot.InverseTransformPoint(vector);
						vector.y = size.y / 2f - this._logWidth / 2f;
					}
					getStructureStrength getStructureStrength = gameObject.AddComponent<getStructureStrength>();
					getStructureStrength._strength = getStructureStrength.strength.strong;
					BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
					boxCollider.center = vector;
					boxCollider.size = size;
					boxCollider.isTrigger = true;
					WallChunkArchitect.Additions addition = this._addition;
					if (addition != WallChunkArchitect.Additions.Wall)
					{
						BoxCollider boxCollider2;
						if (this._height > 4f)
						{
							vector.y += this._logWidth * 2f;
							size.y = 1f * this._logWidth;
							boxCollider2 = gameObject.AddComponent<BoxCollider>();
							boxCollider2.center = vector;
							boxCollider2.size = size;
						}
						FMOD_StudioEventEmitter.CreateAmbientEmitter(gameObject.transform, gameObject.transform.TransformPoint(vector), "event:/ambient/wind/wind_moan_structures");
						size.y = Mathf.Clamp(this._height, 0f, 4f) * this._logWidth;
						size.z = num4;
						vector.y = size.y / 2f - this._logWidth / 2f;
						vector.z = num3 - num4 / 2f;
						boxCollider2 = gameObject.AddComponent<BoxCollider>();
						boxCollider2.center = vector;
						boxCollider2.size = size;
						vector.z = num4 / 2f;
						boxCollider2 = gameObject.AddComponent<BoxCollider>();
						boxCollider2.center = vector;
						boxCollider2.size = size;
						if (this._addition == WallChunkArchitect.Additions.Window)
						{
							size.y = this._logWidth * 1.9f;
							size.z = num3 - num4 * 2f;
							vector.z += num4 / 2f + size.z / 2f;
							vector.y = size.y / 2f - this._logWidth / 2f;
							boxCollider2 = gameObject.AddComponent<BoxCollider>();
							boxCollider2.center = vector;
							boxCollider2.size = size;
							GameObject gameObject3 = new GameObject("PerchTarget");
							SphereCollider sphereCollider = gameObject3.AddComponent<SphereCollider>();
							sphereCollider.isTrigger = true;
							sphereCollider.radius = 0.145f;
							gameObject3.transform.parent = this._wallRoot;
							vector.y += size.y / 2f;
							gameObject3.transform.localPosition = vector;
						}
					}
					else
					{
						BoxCollider boxCollider2 = gameObject.AddComponent<BoxCollider>();
						boxCollider2.center = vector;
						boxCollider2.size = size;
					}
					gridObjectBlocker gridObjectBlocker = gameObject.AddComponent<gridObjectBlocker>();
					gridObjectBlocker.ignoreOnDisable = true;
					addToBuilt addToBuilt = gameObject.AddComponent<addToBuilt>();
					addToBuilt.addToStructures = true;
					BuildingHealthHitRelay buildingHealthHitRelay = gameObject.AddComponent<BuildingHealthHitRelay>();
				}
				if (this.Addition >= WallChunkArchitect.Additions.Door1 && this._addition <= WallChunkArchitect.Additions.LockedDoor2 && !isRepair)
				{
					Vector3 position = Vector3.Lerp(this._p1, this._p2, 0.5f);
					position.y -= this._logWidth / 2f;
					Vector3 worldPosition = (this._addition != WallChunkArchitect.Additions.Door1 && this._addition != WallChunkArchitect.Additions.LockedDoor1) ? this._p1 : this._p2;
					worldPosition.y = position.y;
					Transform transform2 = (Transform)UnityEngine.Object.Instantiate(Prefabs.Instance.DoorPrefab, position, this._wallRoot.rotation);
					transform2.LookAt(worldPosition);
					transform2.parent = base.transform;
				}
			}
		}

		
		protected virtual WallChunkArchitect.Additions SegmentNextAddition(WallChunkArchitect.Additions addition)
		{
			int num = (int)this._height;
			if (addition == WallChunkArchitect.Additions.Wall)
			{
				return WallChunkArchitect.Additions.Window;
			}
			if (addition == WallChunkArchitect.Additions.Window && Mathf.Abs(Vector3.Dot((this._p2 - this._p1).normalized, Vector3.up)) < this._doorAdditionMaxSlope)
			{
				return WallChunkArchitect.Additions.Door1;
			}
			if (addition == WallChunkArchitect.Additions.Door1 && Mathf.Abs(Vector3.Dot((this._p2 - this._p1).normalized, Vector3.up)) < this._doorAdditionMaxSlope)
			{
				return WallChunkArchitect.Additions.Door2;
			}
			return WallChunkArchitect.Additions.Wall;
		}

		
		public virtual Transform SpawnStructure()
		{
			Transform transform = new GameObject("WallChunk").transform;
			transform.transform.position = this._p1;
			Vector3 vector = this._p2 - this._p1;
			int num = Mathf.RoundToInt(this._height);
			if (this.UseHorizontalLogs)
			{
				Vector3 b = new Vector3(0f, this._logWidth * 0.95f, 0f);
				Quaternion rotation = Quaternion.LookRotation(vector);
				Vector3 vector2 = this._p1;
				transform.position = this._p1;
				transform.LookAt(this._p2);
				Vector3 localScale = new Vector3(1f, 1f, vector.magnitude / this._logLength);
				Vector3 localScale2 = new Vector3(1f, 1f, 0.31f + (localScale.z - 1f) / 2f);
				float d = 1f - localScale2.z / localScale.z;
				for (int i = 0; i < num; i++)
				{
					Transform transform2 = this.NewLog(vector2, rotation);
					transform2.parent = transform;
					WallChunkArchitect.Additions addition = this._addition;
					switch (addition + 1)
					{
					case WallChunkArchitect.Additions.Window:
						transform2.localScale = localScale;
						break;
					case WallChunkArchitect.Additions.Door1:
						if (i == 2 || i == 3)
						{
							transform2.localScale = localScale2;
							Transform transform3 = this.NewLog(vector2 + vector * d, rotation);
							transform3.parent = transform2;
							transform3.localScale = Vector3.one;
						}
						else
						{
							transform2.localScale = localScale;
						}
						break;
					case WallChunkArchitect.Additions.Door2:
					case WallChunkArchitect.Additions.LockedDoor1:
					case WallChunkArchitect.Additions.LockedDoor2:
					case (WallChunkArchitect.Additions)5:
						if (i < 4)
						{
							transform2.localScale = localScale2;
							Transform transform4 = this.NewLog(vector2 + vector * d, rotation);
							transform4.parent = transform2;
							transform4.localScale = Vector3.one;
						}
						else
						{
							transform2.localScale = localScale;
						}
						break;
					}
					vector2 += b;
				}
			}
			else
			{
				Vector3 normalized = Vector3.Scale(vector, new Vector3(1f, 0f, 1f)).normalized;
				float y = Mathf.Tan(Vector3.Angle(vector, normalized) * 0.0174532924f) * this._logWidth;
				Quaternion rotation2 = Quaternion.LookRotation(Vector3.up);
				Vector3 localScale3 = new Vector3(1f, 1f, (float)num * 0.95f * this._logWidth / this._logLength);
				float num2 = this._logWidth / 2f * 0.98f;
				float num3 = Vector3.Distance(this._p1, this._p2);
				int num4 = Mathf.Max(Mathf.RoundToInt((num3 - this._logWidth * 0.96f / 2f) / this._logWidth), 1);
				Vector3 vector3 = normalized * this._logWidth;
				vector3.y = y;
				if (vector.y < 0f)
				{
					vector3.y *= -1f;
				}
				Vector3 vector4 = this._p1;
				vector4.y -= num2;
				vector4 += vector3 / 2f;
				transform.position = this._p1;
				transform.LookAt(this._p2);
				transform.eulerAngles = Vector3.Scale(transform.localEulerAngles, Vector3.up);
				for (int j = 0; j < num4; j++)
				{
					Transform transform5 = this.NewLog(vector4, rotation2);
					transform5.parent = transform;
					vector4 += vector3;
					transform5.localScale = localScale3;
				}
			}
			return transform;
		}

		
		protected virtual void InitAdditionTrigger()
		{
			if (Vector3.Distance(this._p1, this._p2) >= this._logLength && this._height >= 3f)
			{
				base.GetComponentInChildren<Craft_Structure>().gameObject.AddComponent<WallAdditionTrigger>();
			}
		}

		
		protected Transform NewLog(Vector3 position, Quaternion rotation)
		{
			return (Transform)UnityEngine.Object.Instantiate(this._logPrefab, position, (!this._wasBuilt) ? rotation : this.RandomizeLogRotation(rotation));
		}

		
		protected virtual Quaternion RandomizeLogRotation(Quaternion logRot)
		{
			return logRot * Quaternion.Euler(0f, UnityEngine.Random.Range(-1.5f, 1.5f), (float)UnityEngine.Random.Range(0, 359));
		}

		
		protected virtual List<GameObject> GetBuiltRenderers(Transform wallRoot)
		{
			List<GameObject> list = new List<GameObject>(5);
			foreach (object obj in wallRoot)
			{
				Transform transform = (Transform)obj;
				list.Add(transform.gameObject);
				transform.gameObject.SetActive(false);
			}
			return list;
		}

		
		
		public virtual Transform BuiltLogPrefab
		{
			get
			{
				return Prefabs.Instance.LogWallExBuiltPrefab;
			}
		}

		
		
		
		public WallChunkArchitect.Additions Addition
		{
			get
			{
				return this._addition;
			}
			set
			{
				this._addition = value;
			}
		}

		
		
		
		public List<Vector3> MultipointPositions
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

		
		
		
		public Vector3 P1
		{
			get
			{
				return this._p1;
			}
			set
			{
				this._p1 = value;
			}
		}

		
		
		
		public Vector3 P2
		{
			get
			{
				return this._p2;
			}
			set
			{
				this._p2 = value;
			}
		}

		
		
		
		public float Height
		{
			get
			{
				return this._height;
			}
			set
			{
				this._height = value;
			}
		}

		
		
		public float LogWidth
		{
			get
			{
				return this._logWidth;
			}
		}

		
		
		public virtual bool UseHorizontalLogs
		{
			get
			{
				return Vector3.Distance(this._p1, this._p2) > this._logWidth * 3.5f;
			}
		}

		
		
		
		[SerializeThis]
		public IStructureSupport CurrentSupport { get; set; }

		
		
		
		[SerializeThis]
		public bool Enslaved { get; set; }

		
		public virtual float GetLevel()
		{
			return this._p1.y + this.GetHeight();
		}

		
		public virtual float GetHeight()
		{
			return 0.92f * this._height * this._logWidth;
		}

		
		public virtual List<Vector3> GetMultiPointsPositions(bool inherit = true)
		{
			return (!inherit || (this._multiPointsPositions != null && this._multiPointsPositions.Count >= 2) || this.CurrentSupport == null) ? this._multiPointsPositions : this.CurrentSupport.GetMultiPointsPositions(true);
		}

		
		
		public virtual Vector3 SupportCenter
		{
			get
			{
				Vector3 result = Vector3.Lerp(this._p1, this._p2, 0.5f);
				result.y = this.GetLevel();
				return result;
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

		
		
		
		public bool WasPlaced { get; set; }

		
		
		
		public int MultiPointsCount
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		
		
		
		public List<Vector3> MultiPointsPositions
		{
			get
			{
				return new List<Vector3>
				{
					this._p1,
					this._p2
				};
			}
			set
			{
				this._p1 = value[0];
				this._p2 = value[1];
			}
		}

		
		
		
		public IProtocolToken CustomToken
		{
			get
			{
				if (BoltNetwork.isServer && this.entity.isAttached)
				{
					if (this.entity.StateIs<IWallChunkBuildingState>())
					{
						this.entity.GetState<IWallChunkBuildingState>().Addition = (int)this._addition;
					}
					else
					{
						this.entity.GetState<IWallChunkConstructionState>().Addition = (int)this._addition;
					}
				}
				CoopWallChunkToken coopWallChunkToken = new CoopWallChunkToken
				{
					P1 = this._p1,
					P2 = this._p2,
					Additions = this._addition,
					Height = this._height
				};
				if (this._multiPointsPositions != null)
				{
					coopWallChunkToken.PointsPositions = this._multiPointsPositions.ToArray();
				}
				if (this.CurrentSupport != null)
				{
					coopWallChunkToken.Support = (this.CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
					CoopSteamServerStarter.AttachBuildingBoltEntity(coopWallChunkToken.Support);
				}
				return coopWallChunkToken;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		
		public override void Detached()
		{
			this.HideToggleAdditionIcon();
		}

		
		public Transform _logPrefab;

		
		public Renderer _logRenderer;

		
		public float _doorAdditionMaxSlope = 0.2f;

		
		public Craft_Structure _craftStructure;

		
		[ItemIdPicker(Item.Types.Equipment)]
		public int _logItemId;

		
		public bool _forceWasBuilt;

		
		public WallArchitect _architect;

		
		protected bool _initialized;

		
		[SerializeThis]
		protected int _multiPointsPositionsCount;

		
		[SerializeThis]
		protected List<Vector3> _multiPointsPositions;

		
		[SerializeThis]
		protected bool _wasBuilt;

		
		[SerializeThis]
		protected Vector3 _p1;

		
		[SerializeThis]
		protected Vector3 _p2;

		
		[SerializeThis]
		protected WallChunkArchitect.Additions _addition = WallChunkArchitect.Additions.Wall;

		
		[SerializeThis]
		protected float _height = 5f;

		
		protected Transform _wallRoot;

		
		protected Transform _wallCollision;

		
		protected WallChunkLods _lods;

		
		protected float _logLength;

		
		protected float _logWidth;

		
		protected int _gridToken = -1;

		
		public enum Additions
		{
			
			Destroyed = -2,
			
			Wall,
			
			Window,
			
			Door1,
			
			Door2,
			
			LockedDoor1,
			
			LockedDoor2
		}
	}
}
