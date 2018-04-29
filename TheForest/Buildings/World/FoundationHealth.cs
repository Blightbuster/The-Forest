using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Interfaces;
using TheForest.Utils;
using TheForest.World;
using UniLinq;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class FoundationHealth : EntityBehaviour<IFoundationState>, IRepairableStructure
	{
		
		private void Awake()
		{
			this._foundation.SegmentTierValidation = new FoundationArchitect.SegmentTierValidator(this.ChunkTierValidation);
			base.StartCoroutine(this.DelayedAwake());
		}

		
		private IEnumerator DelayedAwake()
		{
			yield return null;
			yield return null;
			this.Initialize();
			yield break;
		}

		
		private void OnDestroy()
		{
			if (this._repairTrigger)
			{
				UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
			}
		}

		
		public void TierDestroyed(FoundationChunkTier tier, Vector3 position)
		{
			if (!this._collapsing && !BoltNetwork.isClient)
			{
				this.TierDestroyed_Actual(tier);
			}
		}

		
		public void LocalizedTierHit(LocalizedHitData data, FoundationChunkTier tier)
		{
			if (!PlayerPreferences.NoDestruction && !BoltNetwork.isClient)
			{
				this._lastHit = Time.realtimeSinceStartup;
				FoundationArchitect.Edge edge = this._foundation.Edges[tier._edgeNum];
				int num = this._foundation.Edges.Take(tier._edgeNum).Sum((FoundationArchitect.Edge e) => e._segments.Length);
				int num2 = tier._segmentNum + num;
				this._chunks[num2]._tiers[tier._tierNum]._hp -= data._damage;
				float hp = this._chunks[num2]._tiers[tier._tierNum]._hp;
				if (hp <= 0f)
				{
					if (tier.transform.parent)
					{
						tier.transform.parent = null;
						Renderer[] componentsInChildren = tier.GetComponentsInChildren<Renderer>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							Transform transform = componentsInChildren[i].transform;
							GameObject gameObject = transform.gameObject;
							transform.parent = null;
							gameObject.layer = 31;
							CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
							capsuleCollider.radius = 0.5f;
							capsuleCollider.height = 4.5f;
							capsuleCollider.direction = 0;
							Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
							if (rigidbody)
							{
								rigidbody.AddForce((transform.position - data._position).normalized * 2f, ForceMode.Impulse);
								rigidbody.AddRelativeTorque(Vector3.up * 2f, ForceMode.Impulse);
							}
							destroyAfter destroyAfter = gameObject.AddComponent<destroyAfter>();
							destroyAfter.destroyTime = 1.75f;
							this._collapsedLogs++;
						}
						if (LocalPlayer.Sfx)
						{
							LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.2f);
						}
						UnityEngine.Object.Destroy(tier.gameObject);
						if (BoltNetwork.isServer)
						{
							this.PublishDestroyedTierData(this.LightWeightExport());
						}
						this.IntegrityCheck(num2, tier._tierNum);
					}
				}
				else
				{
					if (BoltNetwork.isServer && base.entity && base.entity.isAttached)
					{
						this.CalcTotalRepairMaterial();
					}
					this.Distort(data);
				}
				if (!this._collapsing)
				{
					this.CheckSpawnRepairTrigger();
				}
				Prefabs.Instance.SpawnHitPS(HitParticles.Wood, data._position, Quaternion.LookRotation(tier.transform.right));
			}
		}

		
		private void Collapse(int collapseChunkNum)
		{
			if (!this._collapsing)
			{
				int index = 0;
				while (collapseChunkNum >= this._foundation.Edges[index]._segments.Length)
				{
					collapseChunkNum -= this._foundation.Edges[index++]._segments.Length;
				}
				Vector3 p = this._foundation.Edges[index]._segments[collapseChunkNum]._p1;
				this.Collapse(p);
			}
		}

		
		public void Collapse(Vector3 collapsePoint)
		{
			if (BoltNetwork.isRunning)
			{
				if (base.entity.isAttached && base.entity.isOwner && base.state != null)
				{
					base.state.BuildingCollapsePoint = collapsePoint;
				}
			}
			else if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.CollapseRoutine(collapsePoint));
			}
		}

		
		public int CalcMissingRepairMaterial()
		{
			return Mathf.Max(this.CalcTotalRepairMaterial() - this.RepairMaterial, 0);
		}

		
		public int CalcTotalRepairMaterial()
		{
			int num = 2;
			int num2 = 7;
			float num3 = 0f;
			if (this._chunks != null)
			{
				if (!BoltNetwork.isClient)
				{
					for (int i = 0; i < this._chunks.Length; i++)
					{
						for (int j = 0; j < 3; j++)
						{
							num3 += this._chunkTierHP + (float)this._bonusHp - Mathf.Max(this._chunks[i]._tiers[j]._hp, 0f);
						}
					}
					if (num3 > 0f)
					{
						int num4 = Mathf.Min(num + Mathf.RoundToInt(num3 / 500f * (float)num2), 20);
						if (BoltNetwork.isServer && base.entity && base.entity.isAttached)
						{
							base.state.RepairMaterialTotal = num4;
						}
						return num4;
					}
					if (BoltNetwork.isServer && base.entity && base.entity.isAttached)
					{
						base.state.RepairMaterialTotal = 0;
					}
				}
				else if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
				{
					return base.state.RepairMaterialTotal;
				}
			}
			return 0;
		}

		
		public int CalcMissingRepairLogs()
		{
			return Mathf.Max(this._collapsedLogs / 3 - this.RepairLogs, 0);
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
				this.RepairLogs++;
			}
			else
			{
				this.RepairMaterial++;
			}
			BuildingHealth component = base.GetComponent<BuildingHealth>();
			if (!component || (component.CalcMissingRepairMaterial() == 0 && component.CalcMissingRepairLogs() == 0))
			{
				this.CheckRepairStatus(component);
			}
		}

		
		public bool CheckRepairStatus(BuildingHealth bh)
		{
			if (this.CalcMissingRepairMaterial() == 0 && this.CalcMissingRepairLogs() == 0)
			{
				if (this._foundation._mode == FoundationArchitect.Modes.Auto)
				{
					bh.Repair();
				}
				else
				{
					this.ResetHP();
					if (!BoltNetwork.isClient)
					{
						if (LocalPlayer.Sfx)
						{
							LocalPlayer.Sfx.PlayTwinkle();
						}
						this.PublishDestroyedTierData(this.LightWeightExport());
						this.RespawnFoundation(false);
					}
				}
				GameStats.RepairedStructure.Invoke();
				return true;
			}
			return false;
		}

		
		public void ResetHP()
		{
			if (this._chunks != null)
			{
				for (int i = 0; i < this._chunks.Length; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						this._chunks[i]._tiers[j]._hp = this._chunkTierHP + (float)this._bonusHp;
					}
				}
			}
			this.RepairMaterial = 0;
			this.RepairLogs = 0;
			this._collapsedLogs = 0;
		}

		
		public void RespawnFoundation(bool respawnOnly)
		{
			if (BoltNetwork.isServer && !respawnOnly)
			{
				base.state.repairTrigger = false;
			}
			if (this._type != BuildingTypes.None)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Prefabs.Instance.Constructions._blueprints.Find((BuildingBlueprint bp) => bp._type == this._type)._builtPrefab);
				gameObject.transform.position = base.transform.position;
				gameObject.transform.rotation = base.transform.rotation;
				for (int i = base.transform.childCount - 1; i >= 0; i--)
				{
					Transform child = base.transform.GetChild(i);
					if (!child.GetComponent<PrefabIdentifier>() && !child.name.Contains("Anchor"))
					{
						UnityEngine.Object.Destroy(child.gameObject);
					}
				}
				for (int j = gameObject.transform.childCount - 1; j >= 0; j--)
				{
					Transform child2 = gameObject.transform.GetChild(j);
					if (!child2.name.Contains("Anchor"))
					{
						child2.parent = base.transform;
					}
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			base.SendMessage("CreateStructure", true);
			if (!respawnOnly)
			{
				this.RepairMaterial = 0;
				this.RepairLogs = 0;
				this._collapsedLogs = 0;
				if (this._repairTrigger)
				{
					UnityEngine.Object.Destroy(this._repairTrigger.gameObject);
					this._repairTrigger = null;
				}
			}
		}

		
		private void Initialize()
		{
			if (this._foundation.Edges != null)
			{
				this._bonusHp = Mathf.RoundToInt((float)this._foundation.Edges.Sum((FoundationArchitect.Edge e) => e._totalLogs) * this._perLogBonusHP);
			}
			if (this._chunks == null || this._chunks.Length == 0)
			{
				this._chunks = new FoundationHealth.Chunk[this._foundation.ChunkCount];
				for (int i = 0; i < this._chunks.Length; i++)
				{
					this._chunks[i] = new FoundationHealth.Chunk
					{
						_tiers = new FoundationHealth.Chunk.Tier[3]
					};
					for (int j = 0; j < 3; j++)
					{
						this._chunks[i]._tiers[j] = new FoundationHealth.Chunk.Tier
						{
							_hp = this._chunkTierHP + (float)this._bonusHp
						};
					}
				}
				if (BoltNetwork.isClient && base.entity.isAttached)
				{
					foreach (int num in base.state.DestroyedTiers)
					{
						if (num != 0)
						{
							this.ReceivedDestroyedTierData();
							break;
						}
					}
				}
			}
			else
			{
				this.CheckSpawnRepairTrigger();
			}
			if (BoltNetwork.isServer && base.entity.isAttached)
			{
				this.PublishDestroyedTierData(this.LightWeightExport());
			}
		}

		
		private bool ChunkTierValidation(int chunkNum, int tierNum)
		{
			return this._chunks == null || this._chunks.Length == 0 || this._chunks[chunkNum]._tiers[tierNum]._hp > 0f;
		}

		
		public void TierDestroyed_Actual(FoundationChunkTier tier)
		{
			if (!PlayerPreferences.NoDestruction)
			{
				try
				{
					if (!this._collapsing)
					{
						int num = tier._segmentNum + this._foundation.Edges.Take(tier._edgeNum).Sum((FoundationArchitect.Edge e) => e._segments.Length);
						this._chunks[num]._tiers[tier._tierNum]._hp = -42f;
						Transform exists = this._foundation.FoundationRoot.transform.Find("Edge" + tier._edgeNum);
						if (exists)
						{
							Renderer[] componentsInChildren = tier.GetComponentsInChildren<Renderer>();
							for (int i = 0; i < componentsInChildren.Length; i++)
							{
								Transform transform = componentsInChildren[i].transform;
								GameObject gameObject = transform.gameObject;
								transform.parent = null;
								gameObject.layer = 31;
								CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
								capsuleCollider.radius = 0.5f;
								capsuleCollider.height = 4.5f;
								capsuleCollider.direction = 2;
								Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
								if (rigidbody)
								{
									rigidbody.AddForce((transform.position - tier.transform.position).normalized * 2.5f, ForceMode.Impulse);
									rigidbody.AddRelativeTorque(Vector3.up * 2f, ForceMode.Impulse);
								}
								destroyAfter destroyAfter = gameObject.AddComponent<destroyAfter>();
								destroyAfter.destroyTime = 1.75f;
								this._collapsedLogs++;
							}
						}
						if (BoltNetwork.isServer)
						{
							this.PublishDestroyedTierData(this.LightWeightExport());
						}
						this.IntegrityCheck(num, tier._tierNum);
						if (!this._collapsing)
						{
							this.CheckSpawnRepairTrigger();
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		
		public void Distort(LocalizedHitData data)
		{
			if (data._damage > 0f)
			{
				Renderer[] componentsInChildren = this._foundation.FoundationRoot.GetComponentsInChildren<Renderer>();
				this.DistortRenderers(data, componentsInChildren);
				if (this._foundation._mode == FoundationArchitect.Modes.ManualSlave)
				{
					FloorArchitect component = base.GetComponent<FloorArchitect>();
					if (component)
					{
						componentsInChildren = component.FloorRoot.GetComponentsInChildren<Renderer>();
						this.DistortRenderers(data, componentsInChildren);
					}
				}
			}
		}

		
		private void DistortRenderers(LocalizedHitData data, Renderer[] rs)
		{
			float num = Mathf.Clamp(data._damage * data._distortRatio * 10f / (this._chunkTierHP + (float)this._bonusHp), 1f, 10f);
			for (int i = 0; i < rs.Length; i++)
			{
				if (rs[i].enabled)
				{
					Transform transform = rs[i].transform;
					GameObject gameObject = transform.gameObject;
					if (Vector3.Distance(rs[i].bounds.center, data._position) < 12f)
					{
						transform.localRotation *= Quaternion.Euler(UnityEngine.Random.Range(-0.6f, 0.6f) * num, UnityEngine.Random.Range(-0.6f, 0.6f) * num, UnityEngine.Random.Range(-0.6f, 0.6f) * num);
					}
				}
			}
		}

		
		private void IntegrityCheck(int brokenChunkNum, int brokenTierNum)
		{
			int num = brokenChunkNum - 1;
			int num2 = brokenChunkNum + 1;
			for (int i = 0; i < this._chunks.Length; i++)
			{
				int num3 = (num + this._chunks.Length * 2) % this._chunks.Length;
				if (this._chunks[num3]._tiers[brokenTierNum]._hp > 0f && ((brokenTierNum == 1 && (this._chunks[num3]._tiers[0]._hp > 0f || this._chunks[num3]._tiers[2]._hp > 0f)) || ((brokenTierNum == 0 || brokenTierNum == 2) && this._chunks[num3]._tiers[1]._hp > 0f)))
				{
					break;
				}
				num--;
				if ((float)(Mathf.Abs(num - num2) - 1) > (float)this._chunks.Length * 0.25f)
				{
					this.Collapse((int)Mathf.Repeat((float)(num + Mathf.RoundToInt((float)Mathf.Abs(num - num2) / 2f)), (float)this._chunks.Length));
					return;
				}
			}
			for (int j = 0; j < this._chunks.Length; j++)
			{
				int num4 = num2 % this._chunks.Length;
				if (this._chunks[num4]._tiers[brokenTierNum]._hp > 0f && ((brokenTierNum == 1 && (this._chunks[num4]._tiers[0]._hp > 0f || this._chunks[num4]._tiers[2]._hp > 0f)) || ((brokenTierNum == 0 || brokenTierNum == 2) && this._chunks[num4]._tiers[1]._hp > 0f)))
				{
					break;
				}
				num2++;
				if ((float)(Mathf.Abs(num - num2) - 1) > (float)this._chunks.Length * 0.25f)
				{
					this.Collapse((int)Mathf.Repeat((float)(num2 + Mathf.RoundToInt((float)Mathf.Abs(num - num2) / 2f)), (float)this._chunks.Length));
					return;
				}
			}
			if ((float)(Mathf.Abs(num - num2) - 1) > (float)this._chunks.Length * 0.25f)
			{
				this.Collapse((int)Mathf.Repeat((float)(num + Mathf.RoundToInt((float)Mathf.Abs(num - num2) / 2f)), (float)this._chunks.Length));
				return;
			}
		}

		
		private IEnumerator CollapseRoutine(Vector3 collapsePoint)
		{
			Transform[] logs = this._foundation.FoundationRoot.GetComponentsInChildren<Transform>();
			Vector3 startPos = base.transform.position;
			Vector3 fallVelocity = Vector3.zero;
			float tierFallGoal = 0f;
			float fallprogress = 0f;
			if (!this._collapsing)
			{
				this._collapsing = true;
				GameStats.DestroyedStructure.Invoke();
				int nbPickupSpawned = 0;
				int totalPickupSpawned = Mathf.Min(this._chunks.Length, 12);
				Vector3 centerPosition = this._foundation.MultiPointsPositions.GetCenterPosition();
				collapsePoint.y = this._foundation.MultiPointsPositions[0].y;
				Vector3 endPos = startPos + Vector3.Lerp(centerPosition, collapsePoint, 0.3f) - centerPosition;
				endPos.y -= this._foundation.MaxHeight;
				Quaternion fallRotation = Quaternion.LookRotation((collapsePoint - centerPosition).normalized);
				for (int t = 3; t > 0; t--)
				{
					string tierTag = "SLTier" + t;
					tierFallGoal = 0.17f * (float)(4 - t);
					float tierDuration = this._collapseFallDuration / 3f;
					float startTime = Time.time;
					for (int i = 0; i < logs.Length; i++)
					{
						Transform transform = logs[i];
						if (transform && transform.CompareTag(tierTag))
						{
							IEnumerator enumerator = transform.GetEnumerator();
							try
							{
								while (enumerator.MoveNext())
								{
									object obj = enumerator.Current;
									Transform transform2 = (Transform)obj;
									GameObject gameObject = transform2.gameObject;
									transform2.parent = null;
									logs[i] = null;
									if (t == 1 && nbPickupSpawned < totalPickupSpawned && (float)i / (float)logs.Length > (float)nbPickupSpawned / (float)totalPickupSpawned && !BoltNetwork.isClient)
									{
										nbPickupSpawned++;
										Transform transform3 = UnityEngine.Object.Instantiate<Transform>(Prefabs.Instance.LogPickupPrefab);
										transform3.position = transform2.position;
										transform3.rotation = transform2.rotation;
										Rigidbody component = transform3.GetComponent<Rigidbody>();
										if (component)
										{
											component.AddForce(transform3.right * -UnityEngine.Random.Range(2.5f, 10f), ForceMode.Impulse);
											component.AddRelativeTorque(Vector3.up * (float)UnityEngine.Random.Range(1, 10), ForceMode.Impulse);
										}
										if (BoltNetwork.isRunning)
										{
											BoltNetwork.Attach(transform3.gameObject);
										}
										UnityEngine.Object.Destroy(gameObject);
									}
									else
									{
										if (!gameObject.GetComponent<Rigidbody>())
										{
											gameObject.layer = 31;
											CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
											capsuleCollider.radius = 0.5f;
											capsuleCollider.height = 4.5f;
											capsuleCollider.direction = 2;
											Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
											if (rigidbody)
											{
												rigidbody.AddForce(transform2.right * -UnityEngine.Random.Range(2.5f, 10f), ForceMode.Impulse);
												rigidbody.AddRelativeTorque(Vector3.up * (float)UnityEngine.Random.Range(-10, 10), ForceMode.Impulse);
											}
										}
										destroyAfter destroyAfter = gameObject.AddComponent<destroyAfter>();
										destroyAfter.destroyTime = tierDuration + 2f;
									}
								}
							}
							finally
							{
								IDisposable disposable;
								if ((disposable = (enumerator as IDisposable)) != null)
								{
									disposable.Dispose();
								}
							}
						}
					}
					if (LocalPlayer.Sfx)
					{
						LocalPlayer.Sfx.PlayStructureBreak(base.gameObject, 0.4f + (float)t * 0.3f);
					}
					while (Time.time - startTime < tierDuration)
					{
						float alpha = fallprogress + (Time.time - startTime) / tierDuration * tierFallGoal;
						base.transform.position = Vector3.Lerp(startPos, endPos, alpha);
						yield return null;
					}
					fallprogress += tierFallGoal;
					if (t == 2 && this._foundation._mode != FoundationArchitect.Modes.Auto)
					{
						Transform transform4 = UnityEngine.Object.Instantiate<Transform>(Prefabs.Instance.BuildingCollapsingDust);
						Vector3 position = centerPosition;
						position.y = endPos.y + 2.5f;
						transform4.position = position;
					}
					else if (t == 1 && BoltNetwork.isClient)
					{
						for (int j = base.transform.childCount - 1; j >= 0; j--)
						{
							Transform child = base.transform.GetChild(j);
							BoltEntity component2 = child.GetComponent<BoltEntity>();
							if (component2)
							{
								child.parent = null;
								if (!component2.GetComponent<destroyAfter>())
								{
									child.gameObject.AddComponent<destroyAfter>().destroyTime = 1f;
								}
							}
						}
					}
					yield return null;
				}
				if (!BoltNetwork.isClient)
				{
					yield return null;
					for (int k = base.transform.childCount - 1; k >= 0; k--)
					{
						Transform child2 = base.transform.GetChild(k);
						BuildingHealth component3 = child2.GetComponent<BuildingHealth>();
						if (component3)
						{
							child2.parent = null;
							component3.Collapse(child2.position);
						}
						else
						{
							FoundationHealth component4 = child2.GetComponent<FoundationHealth>();
							if (component4)
							{
								child2.parent = null;
								component4.Collapse(child2.position);
							}
							else if (BoltNetwork.isRunning && child2.GetComponent<BoltEntity>())
							{
								child2.parent = null;
								destroyAfter destroyAfter2 = child2.gameObject.AddComponent<destroyAfter>();
								destroyAfter2.destroyTime = 3f;
							}
						}
					}
					if (this._foundation._mode == FoundationArchitect.Modes.Auto)
					{
						BuildingHealth bh = base.GetComponent<BuildingHealth>();
						bh.Collapse(base.transform.position);
						yield return YieldPresets.WaitPointFiveSeconds;
					}
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
			yield break;
		}

		
		private void CheckSpawnRepairTrigger()
		{
			if (!this._repairTrigger && this._foundation._mode != FoundationArchitect.Modes.Auto && (this.CalcMissingRepairMaterial() > 0 || this.CalcMissingRepairLogs() > 0))
			{
				this.SpawnRepairTrigger();
				if (BoltNetwork.isServer && base.entity.isAttached)
				{
					base.state.repairTrigger = true;
				}
			}
		}

		
		private void SpawnRepairTrigger()
		{
			if (this._foundation._mode != FoundationArchitect.Modes.Auto && !this._repairTrigger)
			{
				this._repairTrigger = UnityEngine.Object.Instantiate<BuildingRepair>(Prefabs.Instance.BuildingRepairTriggerPrefab);
				this._repairTrigger._target = this;
				this._repairTrigger.transform.position = this._foundation.MultiPointsPositions.GetCenterPosition() + Vector3.one;
			}
		}

		
		public override void Attached()
		{
			base.state.AddCallback("BuildingCollapsePoint", delegate
			{
				base.StartCoroutine(this.CollapseRoutine(base.state.BuildingCollapsePoint));
			});
			if (BoltNetwork.isClient)
			{
				base.state.AddCallback("DestroyedTiers[]", new PropertyCallbackSimple(this.ReceivedDestroyedTierData));
				if (this._foundation._mode != FoundationArchitect.Modes.Auto)
				{
					base.state.AddCallback("repairTrigger", delegate
					{
						if (base.state.repairTrigger)
						{
							this.SpawnRepairTrigger();
						}
						else if (this._repairTrigger)
						{
							this.RespawnFoundation(false);
							UnityEngine.Object.Destroy(this._repairTrigger);
							this._repairTrigger = null;
						}
					});
				}
				else
				{
					base.state.AddCallback("repairLogs", delegate
					{
						if (base.state.repairLogs == 0)
						{
							base.Invoke("ResetHP", 0.1f);
						}
					});
				}
			}
			else if (BoltNetwork.isServer && base.entity.isAttached)
			{
				this.PublishDestroyedTierData(this.LightWeightExport());
			}
			if (BoltNetwork.isServer && this._repairTrigger)
			{
				this.CalcTotalRepairMaterial();
				base.state.repairTrigger = true;
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

		
		public int[] LightWeightExport()
		{
			int num = Mathf.Min(this._chunks.Length * 3, 320);
			int[] array = new int[Mathf.CeilToInt((float)num / 32f)];
			for (int i = 0; i < num; i++)
			{
				if (this._chunks[i / 3]._tiers[i % 3]._hp > 0f)
				{
					array[i / 32] |= 1 << i % 32;
				}
			}
			return array;
		}

		
		public void ImportLightWeight(int[] data)
		{
			bool flag = false;
			int num = Mathf.Min(this._chunks.Length * 3, 320);
			for (int i = 0; i < num; i++)
			{
				bool flag2 = this._chunks[i / 3]._tiers[i % 3]._hp > 0f;
				bool flag3 = (data[i / 32] & 1 << i % 32) != 0;
				if (flag2 != flag3)
				{
					this._chunks[i / 3]._tiers[i % 3]._hp = ((!flag3) ? 0f : (this._chunkTierHP + (float)this._bonusHp));
					if (!flag3)
					{
						FoundationChunkTier chunk = this._foundation.GetChunk(i);
						if (chunk)
						{
							this.TierDestroyed_Actual(chunk);
						}
					}
					else
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				this.RespawnFoundation(true);
			}
		}

		
		private void PublishDestroyedTierData(int[] data)
		{
			if (base.entity.isAttached)
			{
				for (int i = 0; i < data.Length; i++)
				{
					if (base.state.DestroyedTiers[i] != data[i])
					{
						base.state.DestroyedTiers[i] = data[i];
					}
				}
			}
		}

		
		public void ReceivedDestroyedTierData()
		{
			this.ImportLightWeight(base.state.DestroyedTiers.ToArray<int>());
		}

		
		
		
		public int RepairMaterial
		{
			get
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
				{
					return Mathf.Clamp(base.state.repairMaterial, 0, base.state.RepairMaterialTotal);
				}
				return this._repairMaterial;
			}
			set
			{
				if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner)
				{
					base.state.repairMaterial = value;
				}
				this._repairMaterial = value;
			}
		}

		
		
		
		public int RepairLogs
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

		
		
		public int CollapsedLogs
		{
			get
			{
				return this._collapsedLogs / 3;
			}
		}

		
		
		public bool Collapsing
		{
			get
			{
				return this._collapsing;
			}
		}

		
		
		public bool CanBeRepaired
		{
			get
			{
				return true;
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

		
		public FoundationArchitect _foundation;

		
		public float _perLogBonusHP = 2f;

		
		public float _chunkTierHP = 300f;

		
		public float _collapseFallDuration = 2f;

		
		public BuildingTypes _type;

		
		[SerializeThis]
		private FoundationHealth.Chunk[] _chunks;

		
		[SerializeThis]
		private int _collapsedLogs;

		
		[SerializeThis]
		private int _repairMaterial;

		
		[SerializeThis]
		private int _repairLogs;

		
		private int _bonusHp;

		
		private bool _collapsing;

		
		private float _lastHit;

		
		private BuildingRepair _repairTrigger;

		
		[Serializable]
		public class Chunk
		{
			
			public FoundationHealth.Chunk.Tier[] _tiers;

			
			[Serializable]
			public class Tier
			{
				
				public float _hp;
			}
		}
	}
}
