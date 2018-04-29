using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Interfaces;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.Utils.Enums;
using TheForest.World;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class BuildingHealth : EntityEventListener<IBuildingDestructibleState>, IRepairableStructure
	{
		
		private void Awake()
		{
			if (!LevelSerializer.IsDeserializing || this.HpActual == 0f)
			{
				this.HpActual = this._maxHP;
			}
		}

		
		private void Start()
		{
			raftOnLand componentInParent = base.transform.GetComponentInParent<raftOnLand>();
			if (componentInParent && componentInParent.BuildContainer && base.transform.parent != componentInParent.BuildContainer)
			{
				base.transform.parent = componentInParent.BuildContainer;
			}
		}

		
		private void OnDestroy()
		{
			if (this._repairTrigger)
			{
				UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
			}
		}

		
		private void OnDeserialized()
		{
			if (this.HpActual == 0f)
			{
				this.HpActual = this._maxHP;
			}
			if (this.HpActual < this._maxHP && !BoltNetwork.isClient)
			{
				this.SpawnRepairTrigger();
			}
		}

		
		private void OnWillDestroy()
		{
			if (!BoltNetwork.isRunning || BoltNetwork.isServer)
			{
				this.LocalizedHit(new LocalizedHitData(base.transform.position, this.HpActual));
			}
		}

		
		public void LocalizedHit(LocalizedHitData data)
		{
			if (!PlayerPreferences.NoDestruction && this._lastHit + 0.5f < Time.realtimeSinceStartup)
			{
				this._lastHit = Time.realtimeSinceStartup;
				if (BoltNetwork.isRunning)
				{
					LocalizedHit localizedHit = global::LocalizedHit.Create(GlobalTargets.OnlyServer);
					localizedHit.Building = base.entity;
					localizedHit.Damage = data._damage;
					localizedHit.Position = data._position;
					localizedHit.Chunk = -1;
					localizedHit.Send();
					if (BoltNetwork.isClient)
					{
						Prefabs.Instance.SpawnHitPS(this._particleType, data._position, Quaternion.LookRotation(base.transform.right));
					}
				}
				else
				{
					this.LocalizedHitReal(data);
				}
			}
		}

		
		public int CalcMissingRepairMaterial()
		{
			return Mathf.Max(this.CalcTotalRepairMaterial() - this.RepairMaterialActual, 0);
		}

		
		public int CalcTotalRepairMaterial()
		{
			if (this.HpActual < this._maxHP)
			{
				return Mathf.Max(Mathf.RoundToInt((1f - this.HpActual / (this._maxHP - 1f)) * (float)this._maxRepaitHit), 2);
			}
			return 0;
		}

		
		public int CalcMissingRepairLogs()
		{
			return Mathf.Max(this._collapsedLogs / 3 - this._repairLogs, 0);
		}

		
		public void AddRepairMaterial(bool isLog)
		{
			if (BoltNetwork.isRunning)
			{
				AddRepairMaterial addRepairMaterial = global::AddRepairMaterial.Create(GlobalTargets.OnlyServer);
				addRepairMaterial.Building = base.entity;
				addRepairMaterial.IsLog = isLog;
				addRepairMaterial.Send();
			}
			else
			{
				this.AddRepairMaterialReal(isLog);
			}
		}

		
		public void AddRepairMaterialReal(bool isLog)
		{
			if (isLog)
			{
				this._repairLogs++;
			}
			else
			{
				this.RepairMaterialActual++;
			}
			if (this.CalcMissingRepairMaterial() == 0 && this.CalcMissingRepairLogs() == 0)
			{
				FoundationHealth component = base.GetComponent<FoundationHealth>();
				if (!component)
				{
					this.Repair();
				}
				else
				{
					component.CheckRepairStatus(this);
				}
			}
		}

		
		public void DamageOnly(LocalizedHitData data, int collapsedLogs = 0)
		{
			if (this.HpActual > 0f && data._damage > 0f)
			{
				this.HpActual -= data._damage;
				if (collapsedLogs > 0 && LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, this.HpActual / this._maxHP);
				}
				this._collapsedLogs += collapsedLogs;
				if (this.HpActual <= 0f)
				{
					this.Collapse(data._position);
				}
				else if (!this._repairTrigger)
				{
					this.SpawnRepairTrigger();
				}
			}
		}

		
		public void LocalizedHitReal(LocalizedHitData data)
		{
			if (!PlayerPreferences.NoDestruction && this.HpActual > 0f)
			{
				Prefabs.Instance.SpawnHitPS(this._particleType, data._position, Quaternion.LookRotation(data._position - base.transform.position));
				this.DamageOnly(data, 0);
				if (this.HpActual > 0f)
				{
					this.Distort(data);
				}
			}
		}

		
		public void CopyDamageFrom(BuildingHealth other)
		{
			if (other && other.Hp < other._maxHP)
			{
				this.DamageOnly(new LocalizedHitData
				{
					_damage = other._maxHP - other.Hp
				}, 0);
				if (BoltNetwork.isRunning && base.entity.isAttached && other.entity.isAttached)
				{
					base.state.BuildingHits = other.state.BuildingHits;
					if (base.state.BuildingHits > 0)
					{
						this.DistortRandom();
					}
				}
			}
		}

		
		private void DistortReal(LocalizedHitData data)
		{
			if (data._damage > 0f)
			{
				Renderer[] array = null;
				if (this._renderersRoot)
				{
					LOD_GroupToggle component = this._renderersRoot.GetComponent<LOD_GroupToggle>();
					if (component && component._levels != null && component._levels.Length > 0)
					{
						array = component._levels[0].Renderers;
					}
				}
				if (array == null)
				{
					array = ((!(this._renderersRoot != null)) ? base.gameObject : this._renderersRoot).GetComponentsInChildren<Renderer>();
				}
				float num = Mathf.Clamp(data._damage * data._distortRatio * 10f / this._maxHP, 1f, Mathf.Lerp(2f, 10f, (this._maxHP - 100f) / 400f));
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] && array[i].enabled)
					{
						Transform transform = array[i].transform;
						GameObject gameObject = transform.gameObject;
						float num2 = Vector3.Distance(array[i].bounds.center, data._position);
						if (num2 < 12f)
						{
							float num3 = (1f - num2 / 12f) * num;
							transform.localRotation *= Quaternion.Euler((float)UnityEngine.Random.Range(-1, 1) * num3, (float)UnityEngine.Random.Range(-1, 1) * num3, (float)UnityEngine.Random.Range(-1, 1) * num3);
						}
					}
				}
			}
		}

		
		private void Distort(LocalizedHitData data)
		{
			if (data._damage > 0f)
			{
				if (BoltNetwork.isRunning)
				{
					base.state.BuildingHits++;
				}
				else
				{
					this.DistortReal(data);
				}
			}
		}

		
		private IEnumerator CollapseRoutine(Vector3 origin)
		{
			if (!this._collapsing)
			{
				this._collapsing = true;
				if (this._type == BuildingTypes.WorkBench || this._type == BuildingTypes.Chair || this._type == BuildingTypes.BoneChair)
				{
					activateBench componentInChildren = base.transform.GetComponentInChildren<activateBench>();
					if (componentInChildren)
					{
						componentInChildren.gameObject.SetActive(false);
						if (componentInChildren.Sheen)
						{
							componentInChildren.Sheen.SetActive(false);
						}
						if (componentInChildren.MyPickUp)
						{
							componentInChildren.MyPickUp.SetActive(false);
						}
					}
				}
				if (BoltNetwork.isServer)
				{
					yield return YieldPresets.WaitPointOneSeconds;
				}
				if (this._type == BuildingTypes.WeaponRack)
				{
					base.BroadcastMessage("OnBeginCollapse", SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					base.SendMessage("OnBeginCollapse", SendMessageOptions.DontRequireReceiver);
				}
				if ((this._type == BuildingTypes.HouseBoat || this._type == BuildingTypes.Raft || this._type == BuildingTypes.RaftEx) && LocalPlayer.Transform.IsChildOf(base.transform))
				{
					RaftPush[] componentsInChildren = base.transform.GetComponentsInChildren<RaftPush>(true);
					if (componentsInChildren.Length > 0)
					{
						foreach (RaftPush raftPush in componentsInChildren)
						{
							raftPush.SendMessage("offRaft");
						}
					}
				}
				for (int j = base.transform.childCount - 1; j >= 0; j--)
				{
					LODGroup component = base.transform.GetChild(j).GetComponent<LODGroup>();
					if (component)
					{
						LOD[] lods = component.GetLODs();
						if (lods != null && (lods.Length > 1 & lods[1].renderers != null))
						{
							for (int k = 0; k < lods[1].renderers.Length; k++)
							{
								UnityEngine.Object.Destroy(lods[1].renderers[k].gameObject);
							}
						}
						break;
					}
				}
				Transform renderersRoot = (!this._renderersRoot) ? base.transform : this._renderersRoot.transform;
				LOD_GroupToggle lgt = (!renderersRoot.parent) ? null : renderersRoot.parent.GetComponent<LOD_GroupToggle>();
				if (lgt)
				{
					lgt.ForceVisibility(0);
					for (int l = lgt._levels.Length - 1; l > 0; l--)
					{
						foreach (Renderer renderer in lgt._levels[l].Renderers)
						{
							if (renderer)
							{
								renderer.transform.parent = null;
								UnityEngine.Object.Destroy(renderer.gameObject);
							}
						}
					}
					UnityEngine.Object.Destroy(lgt);
				}
				if (!BoltNetwork.isClient)
				{
					yield return YieldPresets.WaitPointZeroFiveSeconds;
					for (int n = base.transform.childCount - 1; n >= 0; n--)
					{
						Transform child = base.transform.GetChild(n);
						BuildingHealth component2 = child.GetComponent<BuildingHealth>();
						if (component2)
						{
							child.parent = null;
							component2.Collapse(child.position);
							Transform transform = child.Find("Trigger");
							if (transform)
							{
								UnityEngine.Object.Destroy(transform.gameObject);
							}
						}
						else
						{
							FoundationHealth component3 = child.GetComponent<FoundationHealth>();
							if (component3)
							{
								child.parent = null;
								component3.Collapse(child.position);
							}
							else if (BoltNetwork.isRunning && child.GetComponent<BoltEntity>())
							{
								child.parent = null;
								destroyAfter destroyAfter = child.gameObject.AddComponent<destroyAfter>();
								destroyAfter.destroyTime = 2.5f;
								Transform transform2 = child.Find("Trigger");
								if (transform2)
								{
									UnityEngine.Object.Destroy(transform2.gameObject);
								}
							}
						}
						if (BoltNetwork.isRunning)
						{
							CoopAutoAttach component4 = child.GetComponent<CoopAutoAttach>();
							if (component4)
							{
								UnityEngine.Object.Destroy(component4);
							}
						}
					}
				}
				if (LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, this._maxHP / 2000f);
				}
				if (LocalPlayer.HitReactions != null)
				{
					LocalPlayer.HitReactions.disableImpact(0.15f);
				}
				Renderer[] renderers = null;
				if (lgt && lgt._levels != null && lgt._levels.Length > 0)
				{
					renderers = lgt._levels[0].Renderers;
				}
				if (renderers == null || renderers.Length == 0)
				{
					renderers = ((!this._renderersRoot) ? base.GetComponentsInChildren<MeshRenderer>() : this._renderersRoot.GetComponentsInChildren<MeshRenderer>());
				}
				foreach (Renderer renderer2 in renderers)
				{
					GameObject gameObject = renderer2.gameObject;
					if (renderer2.enabled && gameObject.activeInHierarchy)
					{
						Transform transform3 = renderer2.transform;
						if (!gameObject.GetComponent<Collider>())
						{
							MeshFilter component5 = renderer2.GetComponent<MeshFilter>();
							if (!component5 || !component5.sharedMesh)
							{
								goto IL_7BB;
							}
							BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
							Bounds bounds = component5.sharedMesh.bounds;
							boxCollider.size = bounds.size * 0.9f;
							boxCollider.center = bounds.center;
						}
						gameObject.layer = this._detachedLayer;
						transform3.parent = null;
						Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
						if (rigidbody)
						{
							rigidbody.AddForce(((transform3.position - origin).normalized + Vector3.up) * (2.5f * this._destructionForceMultiplier), ForceMode.Impulse);
							rigidbody.AddRelativeTorque(Vector3.up * (2f * this._destructionForceMultiplier), ForceMode.Impulse);
							rigidbody.mass = 10f;
						}
						destroyAfter destroyAfter2 = gameObject.AddComponent<destroyAfter>();
						if (this._type == BuildingTypes.WeaponRack)
						{
							destroyAfter2.destroyTime = 1.5f;
						}
						else
						{
							destroyAfter2.destroyTime = 5f;
						}
					}
					IL_7BB:;
				}
				if (this._repairTrigger)
				{
					UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
					this._repairTrigger = null;
				}
				this.SpawnCollapseDust();
				if (!BoltNetwork.isClient)
				{
					if (!BoltNetwork.isRunning)
					{
						yield return YieldPresets.WaitOneSecond;
					}
					else
					{
						yield return YieldPresets.WaitTwoSeconds;
					}
					GameStats.DestroyedStructure.Invoke();
					if (this._destroyTarget)
					{
						this._destroyTarget.transform.parent = null;
						if (!BoltNetwork.isRunning)
						{
							UnityEngine.Object.Destroy(this._destroyTarget);
						}
						else
						{
							BoltNetwork.Destroy(this._destroyTarget);
						}
					}
					else
					{
						base.transform.parent = null;
						if (!BoltNetwork.isRunning)
						{
							UnityEngine.Object.Destroy(base.gameObject);
						}
						else
						{
							BoltNetwork.Destroy(base.gameObject);
						}
					}
				}
			}
			yield break;
		}

		
		public void SpawnCollapseDust()
		{
			if (this._dustPrefab)
			{
				UnityEngine.Object.Instantiate<GameObject>(this._dustPrefab, base.transform.position, base.transform.rotation);
			}
		}

		
		public void Collapse(Vector3 origin)
		{
			if (BoltNetwork.isRunning)
			{
				if (base.entity.isAttached && base.state != null)
				{
					base.state.BuildingCollapsePoint = origin;
				}
			}
			else if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.CollapseRoutine(origin));
			}
		}

		
		private void SpawnRepairTrigger()
		{
			if (this._repairTrigger || !this._canBeRepaired)
			{
				return;
			}
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
			{
				base.state.repairTrigger = true;
			}
			this._repairTrigger = UnityEngine.Object.Instantiate<BuildingRepair>(Prefabs.Instance.BuildingRepairTriggerPrefab);
			this._repairTrigger._target = this;
			this._repairTrigger.transform.parent = base.transform;
			IStructureSupport structureSupport = (IStructureSupport)base.GetComponent(typeof(IStructureSupport));
			if (structureSupport != null)
			{
				if (structureSupport is WallChunkArchitect)
				{
					WallChunkArchitect wallChunkArchitect = (WallChunkArchitect)structureSupport;
					this._repairTrigger.transform.position = Vector3.Lerp(wallChunkArchitect.P1, wallChunkArchitect.P2, 0.5f) + base.transform.right + base.transform.TransformDirection(this._repairTriggerOffset);
					this._repairTrigger.transform.rotation = Quaternion.LookRotation(base.transform.right);
				}
				else
				{
					this._repairTrigger.transform.position = MultipointUtils.CenterOf(structureSupport.GetMultiPointsPositions(true)) + base.transform.TransformDirection(this._repairTriggerOffset);
				}
			}
			else
			{
				this._repairTrigger.transform.position = base.transform.position + base.transform.TransformDirection(this._repairTriggerOffset);
			}
		}

		
		public void Repair()
		{
			this.RepairMaterialActual = 0;
			this._repairLogsActual = 0;
			this._collapsedLogs = 0;
			if (this._repairTrigger)
			{
				UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
				this._repairTrigger = null;
			}
			this.HpActual = this._maxHP;
			if (BoltNetwork.isServer)
			{
				PerformRepairBuilding performRepairBuilding = PerformRepairBuilding.Create(GlobalTargets.AllClients);
				performRepairBuilding.Building = base.entity;
				performRepairBuilding.Send();
			}
			this.RespawnBuilding();
		}

		
		public void RespawnBuilding()
		{
			if (BoltNetwork.isServer)
			{
				base.state.repairTrigger = false;
			}
			else if (BoltNetwork.isClient)
			{
				this.HpActual = this._maxHP;
				LocalPlayer.Create.Grabber.Reset();
			}
			DynamicBuilding dynamicBuilding = (!base.transform.parent) ? null : base.transform.parent.GetComponentInParent<DynamicBuilding>();
			if (dynamicBuilding == null)
			{
				dynamicBuilding = base.transform.GetComponent<DynamicBuilding>();
			}
			if (dynamicBuilding)
			{
				dynamicBuilding.LockPhysics();
			}
			GameObject gameObject2;
			if (this._type != BuildingTypes.None)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Instance.Constructions._blueprints.Find((BuildingBlueprint bp) => bp._type == this._type)._builtPrefab);
				gameObject.transform.position = base.transform.position;
				gameObject.transform.rotation = base.transform.rotation;
				if (dynamicBuilding && this._repairMode == BuildingHealth.RepairModes.RenderersOnly)
				{
					Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>(true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = false;
					}
				}
				if (this._repairMode == BuildingHealth.RepairModes.RenderersOnly)
				{
					base.SendMessage("RespawningRenderersFrom", gameObject, SendMessageOptions.DontRequireReceiver);
					Transform transform = (!this._renderersRoot) ? base.transform : this._renderersRoot.transform;
					LOD_GroupToggle lod_GroupToggle = (!transform.parent) ? null : transform.parent.GetComponent<LOD_GroupToggle>();
					for (int j = transform.childCount - 1; j >= 0; j--)
					{
						Transform child = transform.GetChild(j);
						if ((!child.GetComponent<StoreInformation>() || child.GetComponent<ForceRepairRespawn>()) && !child.GetComponent<SkipRepairRespawn>() && !child.name.Contains("Anchor"))
						{
							UnityEngine.Object.Destroy(child.gameObject);
						}
					}
					Transform transform2 = (!this._renderersRoot) ? gameObject.transform : gameObject.GetComponent<BuildingHealth>()._renderersRoot.transform;
					for (int k = transform2.childCount - 1; k >= 0; k--)
					{
						Transform child2 = transform2.GetChild(k);
						if ((!child2.GetComponent<StoreInformation>() || child2.GetComponent<ForceRepairRespawn>()) && !child2.GetComponent<SkipRepairRespawn>() && !child2.name.Contains("Anchor"))
						{
							child2.parent = transform;
							if (lod_GroupToggle)
							{
								lod_GroupToggle._levels[0].Renderers[k] = child2.GetComponentInChildren<Renderer>();
							}
						}
					}
					if (lod_GroupToggle)
					{
						lod_GroupToggle.RefreshVisibility(true);
					}
					UnityEngine.Object.Destroy(gameObject);
					gameObject2 = base.gameObject;
				}
				else
				{
					TreeStructure component = base.GetComponent<TreeStructure>();
					if (component)
					{
						TreeStructure component2 = gameObject.GetComponent<TreeStructure>();
						component2.TreeId = component.TreeId;
					}
					UnityEngine.Object.Destroy(base.gameObject);
					gameObject2 = gameObject;
					if (BoltNetwork.isServer)
					{
						BoltNetwork.Attach(gameObject);
					}
				}
				if (this._type == BuildingTypes.Shelter)
				{
					EventRegistry.Achievements.Publish(TfEvent.Achievements.RepairedShelter, null);
				}
			}
			else
			{
				gameObject2 = base.gameObject;
			}
			gameObject2.SendMessage("ResetHP", SendMessageOptions.DontRequireReceiver);
			gameObject2.SendMessage("CreateStructure", true, SendMessageOptions.DontRequireReceiver);
			trapTrigger componentInChildren = base.GetComponentInChildren<trapTrigger>();
			if (componentInChildren && componentInChildren.GetComponentInParent<PrefabIdentifier>().gameObject == base.gameObject)
			{
				componentInChildren.CopyTrapStatusTo(gameObject2.GetComponentInChildren<trapTrigger>());
			}
			if (dynamicBuilding)
			{
				raftOnLand component3 = base.transform.GetComponent<raftOnLand>();
				if (component3)
				{
					dynamicBuilding.UnlockPhysics();
					component3.StartCoroutine(component3.fixBounceOnSpawn());
				}
				else
				{
					dynamicBuilding.Invoke("UnlockPhysics", 0.1f);
				}
			}
			if (LocalPlayer.Sfx)
			{
				LocalPlayer.Sfx.PlayTwinkle();
			}
		}

		
		
		
		public float Hp
		{
			get
			{
				return this.HpActual;
			}
			set
			{
				this.HpActual = value;
			}
		}

		
		
		public int RepairMaterial
		{
			get
			{
				return this.RepairMaterialActual;
			}
		}

		
		
		public int RepairLogs
		{
			get
			{
				return this._repairLogsActual;
			}
		}

		
		
		public int CollapsedLogs
		{
			get
			{
				return this._collapsedLogs / 3;
			}
		}

		
		
		public bool CanBeRepaired
		{
			get
			{
				return this._canBeRepaired;
			}
		}

		
		public int GetChunkIndex(BuildingHealthChunk buildingHealthChunk)
		{
			for (int i = 0; i < this._chunks.Length; i++)
			{
				if (object.ReferenceEquals(this._chunks[i], buildingHealthChunk))
				{
					return i;
				}
			}
			return -1;
		}

		
		public override void OnEvent(LocalizedHit evnt)
		{
		}

		
		public BuildingHealthChunk GetChunk(int index)
		{
			return this._chunks[index];
		}

		
		private void CheckRepairTriggerState()
		{
			if (base.state.repairTrigger)
			{
				this.SpawnRepairTrigger();
			}
			else if (this._repairTrigger)
			{
				UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
				this._repairTrigger = null;
			}
		}

		
		private void CheckBuildingHits()
		{
			if (this._mpRandomDistortColliders == null || this._mpRandomDistortColliders.Length == 0 || this._mpRandomDistortColliders[0] == null)
			{
				this._mpRandomDistortColliders = (from x in base.GetComponentsInChildren<Collider>()
				where !x.isTrigger && (x.CompareTag("structure") || x.CompareTag("jumpObject"))
				select x).ToArray<Collider>();
			}
			if (this._chunks.Length > 0 || this._mpRandomDistortColliders.Length > 0)
			{
				while (this._distorts < base.state.BuildingHits)
				{
					this.DistortRandom();
					this._distorts++;
				}
			}
		}

		
		private void CheckBuildingCollapsePoint()
		{
			base.StartCoroutine(this.CollapseRoutine(base.state.BuildingCollapsePoint));
		}

		
		public override void Attached()
		{
			if (base.entity.StateIs<IBuildingDestructibleState>())
			{
				if (base.entity.isOwner)
				{
					base.state.hp = this._hp;
					if (this._repairTrigger)
					{
						base.state.repairTrigger = true;
					}
				}
				if (!base.entity.isOwner)
				{
					base.state.AddCallback("repairTrigger", new PropertyCallbackSimple(this.CheckRepairTriggerState));
					this.CheckRepairTriggerState();
				}
				base.state.AddCallback("BuildingHits", new PropertyCallbackSimple(this.CheckBuildingHits));
				this.CheckBuildingHits();
				base.state.AddCallback("BuildingCollapsePoint", new PropertyCallbackSimple(this.CheckBuildingCollapsePoint));
			}
			else
			{
				Debug.Log("Wrong bolt state on: " + base.gameObject.name);
			}
		}

		
		public override void Detached()
		{
			for (int i = base.transform.childCount - 1; i >= 0; i--)
			{
				Transform child = base.transform.GetChild(i);
				if (child.GetComponent<BoltEntity>())
				{
					child.parent = null;
				}
			}
		}

		
		public void SetMpRandomDistortColliders(params Collider[] colliders)
		{
			this._mpRandomDistortColliders = colliders;
		}

		
		public static Vector3 GetPointInCollider(BoxCollider c)
		{
			Bounds bounds = c.bounds;
			Vector3 center = bounds.center;
			float y = UnityEngine.Random.Range(center.y - bounds.extents.y, center.y + bounds.extents.z);
			float x = UnityEngine.Random.Range(center.x - bounds.extents.x, center.x + bounds.extents.x);
			float z = UnityEngine.Random.Range(center.z - bounds.extents.z, center.z + bounds.extents.z);
			return new Vector3(x, y, z);
		}

		
		public static Vector3 GetPointInCollider(SphereCollider c)
		{
			Vector3 center = c.bounds.center;
			float y = UnityEngine.Random.Range(center.y - c.radius, center.y + c.radius);
			float x = UnityEngine.Random.Range(center.x - c.radius, center.x + c.radius);
			float z = UnityEngine.Random.Range(center.z - c.radius, center.z + c.radius);
			return new Vector3(x, y, z);
		}

		
		private void DistortRandom()
		{
			if (this._chunks.Length <= 0)
			{
				if (this._mpRandomDistortColliders != null && this._mpRandomDistortColliders.Length > 0)
				{
					Collider collider = this._mpRandomDistortColliders[UnityEngine.Random.Range(0, this._mpRandomDistortColliders.Length)];
					Vector3 vector = default(Vector3);
					if (collider is BoxCollider)
					{
						vector = BuildingHealth.GetPointInCollider(collider as BoxCollider);
					}
					if (collider is SphereCollider)
					{
						vector = BuildingHealth.GetPointInCollider(collider as SphereCollider);
					}
					if (vector != Vector3.zero)
					{
						LocalizedHitData data = default(LocalizedHitData);
						float num = (1f - this.Hp / this._maxHP) / (float)base.state.BuildingHits;
						data._damage = num * this._maxHP;
						data._position = vector;
						data._distortRatio = num * 50f;
						this.DistortReal(data);
					}
				}
			}
		}

		
		
		
		private float HpActual
		{
			get
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
				{
					return base.state.hp;
				}
				return this._hp;
			}
			set
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
				{
					base.state.hp = value;
				}
				this._hp = value;
			}
		}

		
		
		
		private int RepairMaterialActual
		{
			get
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
				{
					return Mathf.Max(base.state.repairMaterial - base.state.RepairMaterialTotal, 0);
				}
				return this._repairMaterial;
			}
			set
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
				{
					base.state.repairMaterial = value + base.state.RepairMaterialTotal;
				}
				this._repairMaterial = value;
			}
		}

		
		
		
		private int _repairLogsActual
		{
			get
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
				{
					return base.state.repairLogs;
				}
				return this._repairLogs;
			}
			set
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
				{
					base.state.repairLogs = value;
				}
				this._repairLogs = value;
			}
		}

		
		
		public BuildingRepair RepairTrigger
		{
			get
			{
				return this._repairTrigger;
			}
		}

		
		public int _detachedLayer;

		
		public float _maxHP = 100f;

		
		public int _maxRepaitHit = 15;

		
		public bool _canBeRepaired = true;

		
		public HitParticles _particleType;

		
		public BuildingHealth.RepairModes _repairMode;

		
		public Vector3 _repairTriggerOffset;

		
		public float _destructionForceMultiplier = 1f;

		
		public GameObject _renderersRoot;

		
		public GameObject _destroyTarget;

		
		public GameObject _dustPrefab;

		
		public BuildingTypes _type;

		
		public CapsuleDirections _capsuleDirection = CapsuleDirections.Z;

		
		public BuildingHealthChunk[] _chunks = new BuildingHealthChunk[0];

		
		public Collider[] _mpRandomDistortColliders;

		
		[SerializeThis]
		private float _hp;

		
		[SerializeThis]
		private int _collapsedLogs;

		
		[SerializeThis]
		private int _repairMaterial;

		
		[SerializeThis]
		private int _repairLogs;

		
		private float _lastHit;

		
		private BuildingRepair _repairTrigger;

		
		private int _distorts;

		
		private bool _collapsing;

		
		public enum RepairModes
		{
			
			RenderersOnly,
			
			FullReplace
		}
	}
}
