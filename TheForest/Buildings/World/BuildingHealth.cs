using System;
using System.Collections;
using Bolt;
using ModAPI;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Interfaces;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.Utils.Enums;
using TheForest.World;
using UltimateCheatmenu;
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

		
		public void __LocalizedHit__Original(LocalizedHitData data)
		{
			if (!PlayerPreferences.NoDestruction && this._lastHit + 0.5f < Time.realtimeSinceStartup)
			{
				this._lastHit = Time.realtimeSinceStartup;
				if (BoltNetwork.isRunning)
				{
					LocalizedHit localizedHit = global::LocalizedHit.Create(GlobalTargets.OnlyServer);
					localizedHit.Building = this.entity;
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
				addRepairMaterial.Building = this.entity;
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
				if (BoltNetwork.isRunning && this.entity.isAttached && other.entity.isAttached)
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
				Renderer[] array = (!this._renderersRoot) ? base.transform.GetComponentsInChildren<Renderer>() : this._renderersRoot.GetComponentsInChildren<Renderer>();
				float num = Mathf.Clamp(data._damage * data._distortRatio * 10f / this._maxHP, 1f, Mathf.Lerp(2f, 10f, (this._maxHP - 100f) / 400f));
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].enabled)
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
					activateBench ab = base.transform.GetComponentInChildren<activateBench>();
					if (ab)
					{
						ab.gameObject.SetActive(false);
						if (ab.Sheen)
						{
							ab.Sheen.SetActive(false);
						}
						if (ab.MyPickUp)
						{
							ab.MyPickUp.SetActive(false);
						}
					}
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
					RaftPush[] rp = base.transform.GetComponentsInChildren<RaftPush>(true);
					if (rp.Length > 0)
					{
						foreach (RaftPush r in rp)
						{
							r.SendMessage("offRaft");
						}
					}
				}
				for (int i = base.transform.childCount - 1; i >= 0; i--)
				{
					LODGroup lg = base.transform.GetChild(i).GetComponent<LODGroup>();
					if (lg)
					{
						LOD[] lods = lg.GetLODs();
						if (lods != null && (lods.Length > 1 & lods[1].renderers != null))
						{
							for (int r2 = 0; r2 < lods[1].renderers.Length; r2++)
							{
								UnityEngine.Object.Destroy(lods[1].renderers[r2].gameObject);
							}
						}
						break;
					}
				}
				if (!BoltNetwork.isClient)
				{
					yield return YieldPresets.WaitPointZeroFiveSeconds;
					for (int j = base.transform.childCount - 1; j >= 0; j--)
					{
						Transform child = base.transform.GetChild(j);
						BuildingHealth bh = child.GetComponent<BuildingHealth>();
						if (bh)
						{
							child.parent = null;
							bh.Collapse(child.position);
							Transform trigger = child.Find("Trigger");
							if (trigger)
							{
								UnityEngine.Object.Destroy(trigger.gameObject);
							}
						}
						else
						{
							FoundationHealth fh = child.GetComponent<FoundationHealth>();
							if (fh)
							{
								child.parent = null;
								fh.Collapse(child.position);
							}
							else if (BoltNetwork.isRunning && child.GetComponent<BoltEntity>())
							{
								child.parent = null;
								destroyAfter da = child.gameObject.AddComponent<destroyAfter>();
								da.destroyTime = 2.5f;
								Transform trigger2 = child.Find("Trigger");
								if (trigger2)
								{
									UnityEngine.Object.Destroy(trigger2.gameObject);
								}
							}
						}
						if (BoltNetwork.isRunning)
						{
							CoopAutoAttach caa = child.GetComponent<CoopAutoAttach>();
							if (caa)
							{
								UnityEngine.Object.Destroy(caa);
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
				foreach (MeshRenderer r3 in (!this._renderersRoot) ? base.GetComponentsInChildren<MeshRenderer>() : this._renderersRoot.GetComponentsInChildren<MeshRenderer>())
				{
					GameObject go = r3.gameObject;
					if (r3.enabled && go.activeInHierarchy)
					{
						Transform tr = r3.transform;
						if (!go.GetComponent<Collider>())
						{
							MeshFilter mf = r3.GetComponent<MeshFilter>();
							if (!mf || !mf.sharedMesh)
							{
								goto IL_7CA;
							}
							BoxCollider bc = go.AddComponent<BoxCollider>();
							Bounds meshBounds = mf.sharedMesh.bounds;
							bc.size = meshBounds.size * 0.9f;
							bc.center = meshBounds.center;
						}
						go.layer = this._detachedLayer;
						tr.parent = null;
						Rigidbody rb = go.AddComponent<Rigidbody>();
						if (rb)
						{
							rb.AddForce(((tr.position - origin).normalized + Vector3.up) * (2.5f * this._destructionForceMultiplier), ForceMode.Impulse);
							rb.AddRelativeTorque(Vector3.up * (2f * this._destructionForceMultiplier), ForceMode.Impulse);
							rb.mass = 10f;
						}
						destroyAfter da2 = go.AddComponent<destroyAfter>();
						if (this._type == BuildingTypes.WeaponRack)
						{
							da2.destroyTime = 1.5f;
						}
						else
						{
							da2.destroyTime = 5f;
						}
					}
					IL_7CA:;
				}
				if (this._repairTrigger)
				{
					UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
					this._repairTrigger = null;
				}
				if (this._dustPrefab)
				{
					UnityEngine.Object.Instantiate(this._dustPrefab, base.transform.position, base.transform.rotation);
				}
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

		
		public void Collapse(Vector3 origin)
		{
			if (BoltNetwork.isRunning)
			{
				if (this.entity.isAttached && base.state != null)
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
			if (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner)
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
				performRepairBuilding.Building = this.entity;
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
				if (this._repairMode == BuildingHealth.RepairModes.RenderersOnly)
				{
					base.SendMessage("RespawningRenderersFrom", gameObject, SendMessageOptions.DontRequireReceiver);
					Transform transform = (!this._renderersRoot) ? base.transform : this._renderersRoot.transform;
					for (int i = transform.childCount - 1; i >= 0; i--)
					{
						Transform child = transform.GetChild(i);
						if ((!child.GetComponent<StoreInformation>() || child.GetComponent<ForceRepairRespawn>()) && !child.GetComponent<SkipRepairRespawn>() && !child.name.Contains("Anchor"))
						{
							UnityEngine.Object.Destroy(child.gameObject);
						}
					}
					Transform transform2 = (!this._renderersRoot) ? gameObject.transform : gameObject.GetComponent<BuildingHealth>()._renderersRoot.transform;
					for (int j = transform2.childCount - 1; j >= 0; j--)
					{
						Transform child2 = transform2.GetChild(j);
						if ((!child2.GetComponent<StoreInformation>() || child2.GetComponent<ForceRepairRespawn>()) && !child2.GetComponent<SkipRepairRespawn>() && !child2.name.Contains("Anchor"))
						{
							child2.parent = transform;
						}
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
				dynamicBuilding.Invoke("UnlockPhysics", 0.1f);
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
			if (this._colliders == null || this._colliders.Length == 0 || this._colliders[0] == null)
			{
				this._colliders = (from x in base.GetComponentsInChildren<Collider>()
				where x.CompareTag("structure")
				select x).ToArray<Collider>();
			}
			if (this._chunks.Length > 0 || this._colliders.Length > 0)
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
			if (this.entity.StateIs<IBuildingDestructibleState>())
			{
				if (this.entity.isOwner)
				{
					base.state.hp = this._hp;
					if (this._repairTrigger)
					{
						base.state.repairTrigger = true;
					}
				}
				if (!this.entity.isOwner)
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
				if (this._colliders.Length > 0)
				{
					Collider collider = this._colliders[UnityEngine.Random.Range(0, this._colliders.Length)];
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
				if (BoltNetwork.isRunning && this.entity && this.entity.isAttached)
				{
					return base.state.hp;
				}
				return this._hp;
			}
			set
			{
				if (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner)
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
				if (BoltNetwork.isRunning && this.entity && this.entity.isAttached)
				{
					return Mathf.Max(base.state.repairMaterial - base.state.RepairMaterialTotal, 0);
				}
				return this._repairMaterial;
			}
			set
			{
				if (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner)
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
				if (BoltNetwork.isRunning && this.entity && this.entity.isAttached)
				{
					return base.state.repairLogs;
				}
				return this._repairLogs;
			}
			set
			{
				if (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner)
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

		
		public void LocalizedHit(LocalizedHitData data)
		{
			try
			{
				this.__LocalizedHit__Original(DestroyBuildings.GetLocalizedHitData(data));
			}
			catch (Exception ex)
			{
				Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
				this.__LocalizedHit__Original(data);
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

		
		private Collider[] _colliders = new Collider[0];

		
		private int _distorts;

		
		private bool _collapsing;

		
		public enum RepairModes
		{
			
			RenderersOnly,
			
			FullReplace
		}
	}
}
