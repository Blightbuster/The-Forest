﻿using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


public class ArrowDamage : MonoBehaviour
{
	
	private void Start()
	{
		if (this.parent)
		{
			this.bodyCollider = this.parent.GetComponent<CapsuleCollider>();
			this.at = this.parent.GetComponent<arrowTrajectory>();
		}
		if (this.at)
		{
			this.at._spearType = this.spearType;
		}
		if (this.flintLockAmmoType)
		{
			base.Invoke("destroyThisAmmo", 5f);
		}
	}

	
	private void OnEnable()
	{
		this.Live = true;
		this.ignoreTerrain = false;
	}

	
	private void FixedUpdate()
	{
		if (this.Live && this.PhysicBody)
		{
			float num = this.PhysicBody.velocity.magnitude * 1.25f * Time.deltaTime;
			Vector3 normalized = this.PhysicBody.velocity.normalized;
			Vector3 vector = base.transform.position + normalized * -num;
			if (num > 0f && Physics.Raycast(base.transform.position, normalized, out this.hit, num, this.layers))
			{
				this.hitPointUpdated = true;
				this.CheckHit(this.hit.point, this.hit.transform, this.hit.collider.isTrigger, this.hit.collider);
			}
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (this.Live && this.PhysicBody)
		{
			this.CheckHit(other.transform.position, other.transform, other.isTrigger, this.hit.collider);
		}
		if (this.PhysicBody && other.gameObject.CompareTag("SmallTree"))
		{
			other.gameObject.SendMessage("Hit", 2f * this.PhysicBody.velocity.magnitude, SendMessageOptions.DontRequireReceiver);
		}
		base.Invoke("destroyMe", 90f);
	}

	
	private void disableLive()
	{
		if (BoltNetwork.isRunning)
		{
			if (this.at._boltEntity && this.at._boltEntity.isAttached && this.at._boltEntity.isOwner)
			{
				this.Live = false;
			}
		}
		else
		{
			this.Live = false;
		}
	}

	
	public void CheckHit(Vector3 position, Transform target, bool isTrigger, Collider targetCollider)
	{
		if (this.ignoreCollisionEvents(targetCollider))
		{
			return;
		}
		bool headDamage = false;
		if (target.gameObject.layer == LayerMask.NameToLayer("Water"))
		{
			FMODCommon.PlayOneshotNetworked(this.hitWaterEvent, base.transform, FMODCommon.NetworkRole.Any);
		}
		else if (target.CompareTag("SmallTree"))
		{
			FMODCommon.PlayOneshotNetworked(this.hitBushEvent, base.transform, FMODCommon.NetworkRole.Any);
		}
		if (target.CompareTag("PlaneHull"))
		{
			FMODCommon.PlayOneshotNetworked(this.hitMetalEvent, base.transform, FMODCommon.NetworkRole.Any);
		}
		if (target.CompareTag("Tree") || target.root.CompareTag("Tree") || target.CompareTag("Target"))
		{
			if (this.spearType)
			{
				base.StartCoroutine(this.HitTree(this.hit.point - base.transform.forward * 2.1f));
			}
			else if (this.hitPointUpdated)
			{
				base.StartCoroutine(this.HitTree(this.hit.point - base.transform.forward * 0.35f));
			}
			else
			{
				base.StartCoroutine(this.HitTree(base.transform.position - base.transform.forward * 0.35f));
			}
			this.disableLive();
			if (target.CompareTag("Tree") || target.root.CompareTag("Tree"))
			{
				TreeHealth component = target.GetComponent<TreeHealth>();
				if (!component)
				{
					component = target.root.GetComponent<TreeHealth>();
				}
				if (component)
				{
					component.LodTree.AddTreeCutDownTarget(base.gameObject);
				}
			}
		}
		else if (target.CompareTag("enemyCollide") || target.tag == "lb_bird" || target.CompareTag("animalCollide") || target.CompareTag("Fish") || target.CompareTag("enemyRoot") || target.CompareTag("animalRoot"))
		{
			bool flag = target.tag == "lb_bird" || target.CompareTag("lb_bird");
			bool flag2 = target.CompareTag("Fish");
			bool flag3 = target.CompareTag("animalCollide") || target.CompareTag("animalRoot");
			arrowStickToTarget arrowStickToTarget = target.GetComponent<arrowStickToTarget>();
			if (!arrowStickToTarget)
			{
				arrowStickToTarget = target.root.GetComponentInChildren<arrowStickToTarget>();
			}
			if (!this.spearType && !this.flintLockAmmoType && !flag2)
			{
				if (arrowStickToTarget)
				{
					if (flag)
					{
						EventRegistry.Achievements.Publish(TfEvent.Achievements.BirdArrowKill, null);
					}
					arrowStickToTarget.CreatureType(flag3, flag, flag2);
					if (BoltNetwork.isRunning)
					{
						if (this.at && this.at._boltEntity && this.at._boltEntity.isAttached && this.at._boltEntity.isOwner)
						{
							arrowStickToTarget.stickArrowToNearestBone(base.transform);
						}
					}
					else
					{
						arrowStickToTarget.stickArrowToNearestBone(base.transform);
					}
				}
				base.Invoke("destroyMe", 0.1f);
			}
			if (arrowStickToTarget && arrowStickToTarget.isHeadTransform(base.transform))
			{
				headDamage = true;
			}
			base.StartCoroutine(this.HitAi(target, flag || flag3, headDamage));
			if (flag2)
			{
				base.StartCoroutine(this.HitFish(target, this.hit.point - base.transform.forward * 0.35f));
			}
			this.disableLive();
		}
		else if (target.CompareTag("PlayerNet"))
		{
			if (BoltNetwork.isRunning)
			{
				BoltEntity boltEntity = target.GetComponentInParent<BoltEntity>();
				if (!boltEntity)
				{
					boltEntity = target.GetComponent<BoltEntity>();
				}
				if (boltEntity)
				{
					HitPlayer.Create(boltEntity, EntityTargets.OnlyOwner).Send();
					this.disableLive();
				}
			}
		}
		else if (target.CompareTag("TerrainMain"))
		{
			if (this.ignoreTerrain)
			{
				this.ignoreTerrain = false;
				base.StartCoroutine(this.RevokeIgnoreTerrain());
			}
			else
			{
				if (this.spearType)
				{
					if (this.bodyCollider)
					{
						this.bodyCollider.isTrigger = true;
					}
					base.StartCoroutine(this.HitStructure(base.transform.position - base.transform.forward * 2.1f, false));
				}
				else
				{
					Vector3 position2 = base.transform.position - base.transform.forward * -0.8f;
					float num = Terrain.activeTerrain.SampleHeight(base.transform.position) + Terrain.activeTerrain.transform.position.y;
					if (base.transform.position.y < num)
					{
						position2.y = num + 0.5f;
					}
					base.StartCoroutine(this.HitStructure(position2, false));
				}
				this.disableLive();
				FMODCommon.PlayOneshotNetworked(this.hitGroundEvent, base.transform, FMODCommon.NetworkRole.Any);
			}
		}
		else if (target.CompareTag("structure") || target.CompareTag("jumpObject") || target.CompareTag("SLTier1") || target.CompareTag("SLTier2") || target.CompareTag("SLTier3") || target.CompareTag("UnderfootWood"))
		{
			if (target.transform.parent && (target.transform.parent.GetComponent<StickFenceChunkArchitect>() || target.transform.parent.GetComponent<BoneFenceChunkArchitect>()))
			{
				return;
			}
			if (!isTrigger)
			{
				if (this.spearType)
				{
					base.StartCoroutine(this.HitStructure(this.hit.point - base.transform.forward * 2.1f, true));
				}
				else
				{
					base.StartCoroutine(this.HitStructure(this.hit.point - base.transform.forward * 0.35f, true));
				}
				this.disableLive();
			}
		}
		else if (target.CompareTag("CaveDoor"))
		{
			this.ignoreTerrain = true;
			Physics.IgnoreCollision(base.GetComponent<Collider>(), Terrain.activeTerrain.GetComponent<Collider>(), true);
		}
		else if (this.flintLockAmmoType && (target.CompareTag("BreakableWood") || target.CompareTag("BreakableRock")))
		{
			target.SendMessage("Hit", 40, SendMessageOptions.DontRequireReceiver);
		}
		if (!this.Live)
		{
			this.destroyThisAmmo();
			this.parent.BroadcastMessage("OnArrowHit", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void destroyThisAmmo()
	{
		if (this.flintLockAmmoType)
		{
			if (!BoltNetwork.isRunning)
			{
				UnityEngine.Object.Destroy(this.parent.gameObject, Time.deltaTime * 2f + 0.1f);
			}
			else
			{
				BoltEntity componentInParent = base.GetComponentInParent<BoltEntity>();
				if (!componentInParent || !componentInParent.isAttached)
				{
					UnityEngine.Object.Destroy(this.parent.gameObject, Time.deltaTime * 2f + 0.1f);
				}
				else if (componentInParent.isOwner)
				{
					base.Invoke("DelayedMpDestroy", Time.deltaTime * 2f + 0.1f);
				}
			}
		}
	}

	
	private void DelayedMpDestroy()
	{
		BoltNetwork.Destroy(this.parent.gameObject);
	}

	
	private IEnumerator HitFish(Transform target, Vector3 position)
	{
		Quaternion rot = (!this.hitPointUpdated) ? base.transform.root.rotation : this.PhysicBody.transform.rotation;
		yield return null;
		if (this.bodyCollider && !this.spearType && !this.flintLockAmmoType)
		{
			this.bodyCollider.isTrigger = true;
		}
		if (this.PhysicBody && !this.spearType && !this.flintLockAmmoType)
		{
			this.PhysicBody.transform.position = position;
			this.PhysicBody.transform.rotation = rot;
			this.PhysicBody.velocity = Vector3.zero;
			this.PhysicBody.isKinematic = true;
			this.PhysicBody.useGravity = false;
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(true);
		}
		if (!this.spearType && !this.flintLockAmmoType)
		{
			if (this.parent)
			{
				this.parent.transform.parent = target.root;
			}
			else if (this.PhysicBody)
			{
				this.PhysicBody.transform.parent = target.root;
			}
			else
			{
				base.transform.root.parent = target.root;
			}
		}
		this.hitPointUpdated = false;
		yield break;
	}

	
	private IEnumerator HitTree(Vector3 position)
	{
		Quaternion rot = (!this.hitPointUpdated) ? base.transform.root.rotation : this.PhysicBody.transform.rotation;
		yield return null;
		if (this.bodyCollider)
		{
			this.bodyCollider.isTrigger = true;
		}
		if (this.PhysicBody)
		{
			this.PhysicBody.transform.position = position;
			this.PhysicBody.transform.rotation = rot;
			this.PhysicBody.velocity = Vector3.zero;
			this.PhysicBody.isKinematic = true;
			this.PhysicBody.useGravity = false;
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(true);
		}
		FMODCommon.PlayOneshotNetworked(this.hitTreeEvent, base.transform, FMODCommon.NetworkRole.Any);
		this.hitPointUpdated = false;
		yield break;
	}

	
	private IEnumerator HitAi(Transform target, bool hitDelay = false, bool headDamage = false)
	{
		yield return null;
		int sendDamage = this.damage;
		if (headDamage)
		{
			sendDamage *= 20;
		}
		if (this.PhysicBody)
		{
			this.PhysicBody.velocity = Vector3.zero;
		}
		if (this.spearType)
		{
			this.PhysicBody.isKinematic = false;
			this.PhysicBody.useGravity = true;
			this.disableLive();
			if (this.MyPickUp)
			{
				this.MyPickUp.SetActive(true);
			}
		}
		if (target)
		{
			Vector3 localTarget = target.transform.root.GetChild(0).InverseTransformPoint(base.transform.position);
			float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
			int animalHitDirection = animalHealth.GetAnimalHitDirection(targetAngle);
			BoltEntity targetEntity = target.GetComponentInParent<BoltEntity>();
			if (!targetEntity)
			{
				target.GetComponent<BoltEntity>();
			}
			if (BoltNetwork.isClient && targetEntity)
			{
				if (hitDelay)
				{
					target.transform.SendMessageUpwards("getClientHitDirection", 6, SendMessageOptions.DontRequireReceiver);
					target.transform.SendMessageUpwards("StartPrediction", SendMessageOptions.DontRequireReceiver);
					BoltEntity arrowEntity = this.parent.GetComponent<BoltEntity>();
					PlayerHitEnemy ev = PlayerHitEnemy.Raise(GlobalTargets.OnlyServer);
					ev.Target = targetEntity;
					ev.Weapon = arrowEntity;
					ev.getAttacker = 10;
					if (target.gameObject.CompareTag("animalRoot"))
					{
						ev.getAttackDirection = animalHitDirection;
					}
					else
					{
						ev.getAttackDirection = 3;
					}
					ev.getAttackerType = 4;
					ev.Hit = sendDamage;
					ev.Send();
				}
				else
				{
					target.transform.SendMessageUpwards("getClientHitDirection", 6, SendMessageOptions.DontRequireReceiver);
					target.transform.SendMessageUpwards("StartPrediction", SendMessageOptions.DontRequireReceiver);
					PlayerHitEnemy ev2 = PlayerHitEnemy.Raise(GlobalTargets.OnlyServer);
					ev2.Target = targetEntity;
					if (target.gameObject.CompareTag("animalRoot"))
					{
						ev2.getAttackDirection = animalHitDirection;
					}
					else
					{
						ev2.getAttackDirection = 3;
					}
					ev2.getAttackerType = 4;
					ev2.Hit = sendDamage;
					ev2.Send();
				}
			}
			else
			{
				target.gameObject.SendMessageUpwards("getAttackDirection", 3, SendMessageOptions.DontRequireReceiver);
				target.gameObject.SendMessageUpwards("getAttackerType", 4, SendMessageOptions.DontRequireReceiver);
				GameObject attacker = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position);
				target.gameObject.SendMessageUpwards("getAttacker", attacker, SendMessageOptions.DontRequireReceiver);
				if (target.gameObject.CompareTag("lb_bird") || target.gameObject.CompareTag("animalRoot") || target.gameObject.CompareTag("enemyRoot") || target.gameObject.CompareTag("PlayerNet"))
				{
					if (target.gameObject.CompareTag("enemyRoot"))
					{
						EnemyHealth eh = target.GetComponentInChildren<EnemyHealth>();
						if (eh)
						{
							eh.getAttackDirection(3);
							eh.setSkinDamage(2);
							mutantTargetSwitching mts = target.GetComponentInChildren<mutantTargetSwitching>();
							if (mts)
							{
								mts.getAttackerType(4);
								mts.getAttacker(attacker);
							}
							eh.Hit(sendDamage);
						}
					}
					else
					{
						if (target.gameObject.CompareTag("animalRoot"))
						{
							target.gameObject.SendMessage("ApplyAnimalSkinDamage", animalHitDirection, SendMessageOptions.DontRequireReceiver);
						}
						target.gameObject.SendMessage("Hit", sendDamage, SendMessageOptions.DontRequireReceiver);
						target.gameObject.SendMessage("getSkinHitPosition", base.transform, SendMessageOptions.DontRequireReceiver);
					}
				}
				else
				{
					if (target.gameObject.CompareTag("animalCollide"))
					{
						target.gameObject.SendMessageUpwards("ApplyAnimalSkinDamage", animalHitDirection, SendMessageOptions.DontRequireReceiver);
					}
					target.gameObject.SendMessageUpwards("Hit", sendDamage, SendMessageOptions.DontRequireReceiver);
					target.gameObject.SendMessageUpwards("getSkinHitPosition", base.transform, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(true);
		}
		FMODCommon.PlayOneshotNetworked(this.hitAiEvent, base.transform, FMODCommon.NetworkRole.Any);
		yield return null;
		yield break;
	}

	
	private IEnumerator RevokeIgnoreTerrain()
	{
		yield return null;
		Physics.IgnoreCollision(base.GetComponent<Collider>(), Terrain.activeTerrain.GetComponent<Collider>(), false);
		yield break;
	}

	
	private IEnumerator HitStructure(Vector3 position, bool noStick = false)
	{
		this.disableLive();
		Quaternion rot = this.PhysicBody.transform.rotation;
		yield return YieldPresets.WaitForFixedUpdate;
		if (this.PhysicBody)
		{
			this.PhysicBody.transform.position = position;
			this.PhysicBody.transform.rotation = rot;
			if (!noStick)
			{
				this.PhysicBody.velocity = Vector3.zero;
				this.PhysicBody.isKinematic = true;
				this.PhysicBody.useGravity = false;
			}
			else
			{
				this.PhysicBody.AddTorque((float)UnityEngine.Random.Range(-1000, 1000), (float)UnityEngine.Random.Range(-1000, 1000), (float)UnityEngine.Random.Range(-1000, 1000));
			}
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(true);
		}
		if (this.at)
		{
			if (this.at.col && !noStick)
			{
				this.at.col.isTrigger = true;
			}
			this.at.enabled = false;
		}
		FMODCommon.PlayOneshotNetworked(this.hitStructureEvent, base.transform, FMODCommon.NetworkRole.Any);
		yield break;
	}

	
	private void OnTreeCutDown(GameObject trunk)
	{
		if (trunk.GetComponent<Rigidbody>())
		{
			this.at.setArrowDynamic();
		}
	}

	
	private void destroyMe()
	{
		bool flag = false;
		BoltEntity component = this.parent.GetComponent<BoltEntity>();
		if (component)
		{
			if (!component.isAttached)
			{
				flag = true;
			}
			else if (component.isOwner)
			{
				flag = true;
			}
		}
		if (!BoltNetwork.isRunning || flag)
		{
			if (this.parent)
			{
				UnityEngine.Object.Destroy(this.parent);
			}
		}
		else if (BoltNetwork.isServer)
		{
			BoltNetwork.Destroy(this.parent.gameObject);
		}
	}

	
	private bool ignoreCollisionEvents(Collider col)
	{
		if (!col)
		{
			return false;
		}
		if (col.isTrigger)
		{
			return false;
		}
		bool flag = false;
		if (col.transform.GetComponent<girlMutantColliderSetup>() || col.transform.GetComponent<CoopMutantFX>())
		{
			flag = true;
		}
		if (col.transform.parent && (col.transform.parent.GetComponent<StickFenceChunkArchitect>() || col.transform.parent.GetComponent<BoneFenceChunkArchitect>()))
		{
			flag = true;
		}
		if (flag && this.bodyCollider && col && this.bodyCollider.enabled && col.enabled && col.gameObject.activeInHierarchy)
		{
			Physics.IgnoreCollision(this.bodyCollider, col, true);
			return true;
		}
		if (BoltNetwork.isRunning && this.at && this.at._boltEntity && this.at._boltEntity.isAttached && !this.at._boltEntity.isOwner)
		{
			return false;
		}
		if (this.at && this.Live)
		{
			this.PhysicBody.velocity /= 4f;
			this.at.forceDisable = true;
		}
		return false;
	}

	
	private arrowTrajectory at;

	
	public LayerMask layers;

	
	public GameObject MyPickUp;

	
	public GameObject parent;

	
	public Rigidbody PhysicBody;

	
	public bool spearType;

	
	public bool flintLockAmmoType;

	
	public int damage;

	
	[Header("FMOD")]
	public string hitTreeEvent;

	
	public string hitStructureEvent;

	
	public string hitGroundEvent;

	
	public string hitAiEvent;

	
	public string hitBushEvent;

	
	public string hitWaterEvent;

	
	public string hitMetalEvent;

	
	private CapsuleCollider bodyCollider;

	
	private RaycastHit hit;

	
	public bool Live = true;

	
	private bool ignoreTerrain;

	
	private bool hitPointUpdated;
}