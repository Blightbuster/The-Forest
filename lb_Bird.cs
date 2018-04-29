using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;


public class lb_Bird : MonoBehaviour
{
	
	private string[] AllEventPaths()
	{
		return new string[]
		{
			this.SongEvent,
			this.FlyAwayEvent,
			this.HitEvent,
			this.DieEvent,
			this.StartleEvent
		};
	}

	
	private void Awake()
	{
		this.seeker = base.transform.GetComponent<Seeker>();
		this.anim = base.gameObject.GetComponent<Animator>();
		if (CoopPeerStarter.DedicatedHost)
		{
			AmplifyMotionObject[] componentsInChildren = base.transform.GetComponentsInChildren<AmplifyMotionObject>(true);
			AmplifyMotionObjectBase[] componentsInChildren2 = base.transform.GetComponentsInChildren<AmplifyMotionObjectBase>(true);
			foreach (AmplifyMotionObject obj in componentsInChildren)
			{
				UnityEngine.Object.Destroy(obj);
			}
			foreach (AmplifyMotionObjectBase obj2 in componentsInChildren2)
			{
				UnityEngine.Object.Destroy(obj2);
			}
		}
	}

	
	private void Start()
	{
		this.skin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		this.tr = base.transform;
		this.ragDoll = base.GetComponent<clsragdollify>();
		this.initFlyingSpeed = this.flyingSpeed;
		this.initRotateSpeed = this.rotateSpeed;
		this.layerMask1 = 67108864;
		this.ragDoll.bird = true;
		base.Invoke("initThis", 0.5f);
		this.rg = AstarPath.active.astarData.recastGraph;
		base.InvokeRepeating("CleanupEventInstances", 1f, 1f);
	}

	
	private void initThis()
	{
		this.initBool = true;
	}

	
	private void OnDeserialized()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	private void OnEnable()
	{
		this.tr = base.transform;
		this.anim = base.gameObject.GetComponent<Animator>();
		this.ragDoll = base.GetComponent<clsragdollify>();
		this.layerMask1 = 67108864;
		this.lastPos = this.tr.position;
		this.smallCircleCheckDelay = Time.time + 5f;
		this.idleAnimationHash = Animator.StringToHash("Base Layer.Idle");
		this.flyTagHash = Animator.StringToHash("flying");
		this.hopIntHash = Animator.StringToHash("hop");
		this.flyingBoolHash = Animator.StringToHash("flying");
		this.peckBoolHash = Animator.StringToHash("peck");
		this.ruffleBoolHash = Animator.StringToHash("ruffle");
		this.preenBoolHash = Animator.StringToHash("preen");
		this.landingBoolHash = Animator.StringToHash("landing");
		this.singTriggerHash = Animator.StringToHash("sing");
		this.flyingDirectionHash = Animator.StringToHash("flyingDirectionX");
		this.anim.SetFloatReflected("IdleAgitated", this.agitationLevel);
		this.health = 1;
		if (this.anim.enabled)
		{
			this.anim.SetBool(this.flyingBoolHash, true);
			this.anim.SetBool(this.landingBoolHash, false);
		}
		if (!base.IsInvoking("groundHeightCheck"))
		{
			FMODCommon.PreloadEvents(this.AllEventPaths());
		}
		this.fleeing = false;
	}

	
	private void OnDisable()
	{
		if (!this.initBool)
		{
			return;
		}
		base.CancelInvoke("groundHeightCheck");
		this.fleeing = false;
		base.StopAllCoroutines();
		this.StopEvents();
		FMODCommon.UnloadEvents(this.AllEventPaths());
		this.flyAwayCoolDown = false;
		this.flying = false;
		this.landing = false;
		this.onGround = false;
		this.idle = false;
	}

	
	private void OnDestroy()
	{
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = null;
	}

	
	private void groundHeightCheck()
	{
		if (this.tr.position.y - this.groundHeight < 2f && !this.landing && !this.onGround && this.distanceToTarget > 10f && !this.groundCoolDown)
		{
			Debug.Log("doing flee from ground height check");
			this.Flee();
			this.groundCoolDown = true;
			base.Invoke("resetGroundCoolDown", 3f);
		}
	}

	
	private void resetGroundCoolDown()
	{
		this.groundCoolDown = false;
	}

	
	private void PauseBird()
	{
		this.originalAnimSpeed = this.anim.speed;
		this.anim.speed = 0f;
		this.StopEvents();
		this.paused = true;
	}

	
	private void UnPauseBird()
	{
		this.anim.speed = this.originalAnimSpeed;
		this.paused = false;
	}

	
	private IEnumerator FlyToTarget(Vector3 getTarget)
	{
		this.flying = true;
		this.landing = false;
		this.onGround = false;
		this.anim.applyRootMotion = false;
		this.anim.SetBool(this.flyingBoolHash, true);
		this.anim.SetBool(this.landingBoolHash, false);
		this.target = getTarget;
		this.generatePathToTarget();
		this.distanceToTarget = Vector3.Distance(base.transform.position, this.target);
		while (this.anim.GetCurrentAnimatorStateInfo(0).tagHash != this.flyTagHash)
		{
			yield return null;
		}
		while (this.distanceToTarget >= 4.5f || this.takeoff)
		{
			if (!this.paused)
			{
				this.distanceToTarget = Vector3.Distance(this.tr.position, this.target);
				float modFlyingSpeed = this.flyingSpeed;
				if (this.diff.y > 0f)
				{
					modFlyingSpeed = this.flyingSpeed * 0.9f;
				}
				else
				{
					modFlyingSpeed = this.flyingSpeed * 1.1f;
				}
				this.tr.position += this.tr.forward * modFlyingSpeed * Time.deltaTime;
				Vector3 modTargetWaypoint = this.targetWaypoint;
				if (this.path == null || this.path.vectorPath.Count < 3 || !this.onValidArea)
				{
					this.vectorDirectionToTarget = (this.target - base.transform.position).normalized;
				}
				else
				{
					float tempDist = this.distanceToTarget * 0.2f;
					if (tempDist < 1f)
					{
						tempDist = 1f;
					}
					float targetY = (base.transform.position.y - this.target.y) / tempDist;
					if (this.path != null && this.path.vectorPath.Count > 1)
					{
						if ((double)(this.tr.position.y - this.path.vectorPath[this.currentWaypoint].y) < 4.5 && this.distanceToTarget > 16f && !this.takeoff)
						{
							modTargetWaypoint.y = this.path.vectorPath[this.currentWaypoint].y + 5f;
						}
						else
						{
							modTargetWaypoint.y = base.transform.position.y - targetY;
						}
					}
					if (this.distanceToTarget < 15f)
					{
						modTargetWaypoint.y = this.target.y;
					}
					this.vectorDirectionToTarget = (modTargetWaypoint - base.transform.position).normalized;
				}
				this.checkPerchTarget();
				Vector3 groundHeightVector = this.vectorDirectionToTarget;
				if (this.tr.position.y - this.groundHeight < 1.4f && !this.landing && !this.onGround && this.distanceToTarget > 8f)
				{
					this.groundAvoidTimer = Time.time + 1f;
					groundHeightVector.y = 0.4f;
				}
				if (Time.time < this.groundAvoidTimer)
				{
					groundHeightVector.y = 0.4f;
				}
				else if (Time.time < this.groundAvoidTimer + 1f)
				{
					groundHeightVector.y = Mathf.Lerp(groundHeightVector.y, this.vectorDirectionToTarget.y, Time.deltaTime);
				}
				this.finalRotation = Quaternion.LookRotation(groundHeightVector);
				this.currentDir = (this.tr.rotation * Vector3.forward).normalized;
				this.lastDir = (this.desiredRotation * Vector3.forward).normalized;
				if (Vector3.Dot(this.lastDir, this.currentDir) > 0f)
				{
					this.absDir = Vector3.Cross(this.lastDir, this.currentDir).y;
				}
				else
				{
					this.absDir = (float)((Vector3.Cross(this.lastDir, this.currentDir).y <= 0f) ? -1 : 1);
				}
				this.smoothedDir = this.absDir * -1f;
				if (this.anim)
				{
					this.anim.SetFloatReflected(this.flyingDirectionHash, this.smoothedDir * 5f);
				}
				if (this.takeoff)
				{
					this.diff = groundHeightVector.normalized;
				}
				else
				{
					this.diff = Vector3.Slerp(this.diff, groundHeightVector, this.rotateSpeed * Time.deltaTime);
				}
				if (this.diff.sqrMagnitude > 0.02f)
				{
					this.desiredRotation = Quaternion.LookRotation(this.diff, Vector3.up);
				}
				if (this.takeoff)
				{
					this.lastRotation = this.desiredRotation;
				}
				this.lastRotation = Quaternion.Slerp(this.lastRotation, this.desiredRotation, this.rotateSpeed * Time.deltaTime);
				this.tr.rotation = this.lastRotation;
				if (this.fleeing)
				{
					this.rotateSpeed = this.initRotateSpeed * 1.4f;
				}
				else if (this.distanceToTarget < 10f)
				{
					this.rotateSpeed = this.initRotateSpeed * 2.3f;
					this.flyingSpeed = this.initFlyingSpeed * 0.8f;
				}
				if (this.distanceToTarget < 3.5f)
				{
					this.rotateSpeed = this.initRotateSpeed;
					break;
				}
			}
			yield return null;
			if (this.path != null)
			{
				if (this.currentWaypoint != this.path.vectorPath.Count - 1 || !this.flying || this.landing || this.distanceToTarget > 45f)
				{
				}
				if (this.path.vectorPath.Count >= 21 || this.currentWaypoint != this.path.vectorPath.Count - 1 || !this.flying || !this.takeoff)
				{
				}
			}
			if (this.distanceToTarget < 4f && !this.flyingToPerch && !this.takeoff && !this.landing)
			{
				this.findNewLandingTarget();
				Debug.Log("finding new path due to very close to target but not landing");
			}
		}
		this.anim.SetFloatReflected(this.flyingDirectionHash, 0f);
		Vector3 vel = Vector3.zero;
		this.flying = true;
		this.landing = true;
		this.anim.SetBool(this.landingBoolHash, true);
		this.anim.SetBool(this.flyingBoolHash, false);
		float t = 0f;
		this.startingRotation = base.transform.rotation;
		base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		Vector3 tempFinalRotation = this.target;
		Vector3 tempFinalPosition = this.target;
		tempFinalRotation.y = this.tr.position.y;
		this.finalRotation = Quaternion.LookRotation((tempFinalRotation - this.tr.position).normalized);
		base.transform.rotation = this.startingRotation;
		while (this.distanceToTarget > 0.1f)
		{
			this.distanceToTarget = Vector3.Distance(base.transform.position, tempFinalPosition);
			if (!this.paused)
			{
				base.transform.rotation = Quaternion.Slerp(this.startingRotation, this.finalRotation, t);
				base.transform.position = Vector3.SmoothDamp(base.transform.position, tempFinalPosition, ref vel, 0.5f);
				t += Time.deltaTime * 2f;
			}
			base.transform.rotation = this.finalRotation;
			yield return null;
		}
		this.anim.SetBool(this.landingBoolHash, false);
		this.landing = false;
		this.flying = false;
		base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		base.transform.position = tempFinalPosition;
		this.anim.applyRootMotion = true;
		this.lastRotation = this.tr.rotation;
		this.finalRotation = this.tr.rotation;
		this.diff = this.tr.forward;
		this.flyingSpeed = UnityEngine.Random.Range(this.initFlyingSpeed * 0.8f, this.initFlyingSpeed * 1.2f);
		this.onGround = true;
		if (this.flyingToPerch)
		{
			this.perched = true;
		}
		yield break;
	}

	
	private void checkPerchTarget()
	{
		if (this.flyingToPerch)
		{
			if (this.currentPerchTarget)
			{
				if (!this.currentPerchTarget.activeInHierarchy)
				{
					this.Flee();
				}
			}
			else
			{
				this.Flee();
			}
		}
	}

	
	private void OnGroundBehaviors()
	{
		this.idle = (this.anim.GetCurrentAnimatorStateInfo(0).nameHash == this.idleAnimationHash);
		if (!base.GetComponent<Rigidbody>().isKinematic)
		{
			base.GetComponent<Rigidbody>().isKinematic = true;
		}
		this.checkPerchTarget();
		if (this.idle)
		{
			if (!this.songBreak)
			{
				this.PlaySong();
			}
			if ((double)UnityEngine.Random.value < (double)Time.deltaTime * 0.33)
			{
				float value = UnityEngine.Random.value;
				if ((double)value < 0.3)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.sing);
				}
				else if ((double)value < 0.5)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.peck);
				}
				else if ((double)value < 0.6)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.preen);
				}
				else if (!this.perched && (double)value < 0.7)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.ruffle);
				}
				else if (!this.perched && (double)value < 0.85)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.hopForward);
				}
				else if (!this.perched && (double)value < 0.9)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.hopLeft);
				}
				else if (!this.perched && (double)value < 0.95)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.hopRight);
				}
				else if (!this.perched && value <= 1f)
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.hopBackward);
				}
				else
				{
					this.DisplayBehavior(lb_Bird.birdBehaviors.sing);
				}
				this.anim.SetFloatReflected("IdleAgitated", UnityEngine.Random.value);
			}
			if ((double)UnityEngine.Random.value < (double)Time.deltaTime * 0.1)
			{
				this.FlyAway();
			}
		}
	}

	
	private void DisplayBehavior(lb_Bird.birdBehaviors behavior)
	{
		this.idle = false;
		switch (behavior)
		{
		case lb_Bird.birdBehaviors.sing:
			this.ResetHopInt();
			this.anim.SetTrigger(this.singTriggerHash);
			break;
		case lb_Bird.birdBehaviors.preen:
			this.ResetHopInt();
			this.anim.SetTrigger(this.preenBoolHash);
			break;
		case lb_Bird.birdBehaviors.ruffle:
			this.ResetHopInt();
			this.anim.SetTrigger(this.ruffleBoolHash);
			break;
		case lb_Bird.birdBehaviors.peck:
			this.ResetHopInt();
			this.anim.SetTrigger(this.peckBoolHash);
			break;
		case lb_Bird.birdBehaviors.hopForward:
			this.anim.SetInteger(this.hopIntHash, 1);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		case lb_Bird.birdBehaviors.hopBackward:
			this.anim.SetInteger(this.hopIntHash, -1);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		case lb_Bird.birdBehaviors.hopLeft:
			this.anim.SetInteger(this.hopIntHash, -2);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		case lb_Bird.birdBehaviors.hopRight:
			this.anim.SetInteger(this.hopIntHash, 2);
			base.Invoke("ResetHopInt", UnityEngine.Random.Range(0.5f, 1f));
			break;
		}
	}

	
	private void onSoundAlert(Collider getCol)
	{
		this.OnTriggerEnter(getCol);
	}

	
	private void OnTriggerEnter(Collider col)
	{
		if (col.tag == "lb_bird")
		{
			this.FlyAway();
		}
		if ((col.CompareTag("soundAlert") || col.CompareTag("enemyRoot") || col.GetComponent<pushRigidBody>()) && (col.transform.position - base.transform.position).sqrMagnitude < 121f)
		{
			if (!this.flying)
			{
				this.PlayEvent(this.StartleEvent);
			}
			this.FlyAway();
		}
	}

	
	private void OnTriggerExit(Collider col)
	{
		if (!this.onGround || col.tag == "lb_groundTarget" || col.tag == "lb_perchTarget")
		{
		}
	}

	
	public void Burn()
	{
		this.DieFire();
	}

	
	private void FlyAway()
	{
		if (!this.flyAwayCoolDown && !this.flying)
		{
			this.sendDisablePerchTarget();
			base.StopCoroutine("FlyToTarget");
			this.PlayEvent(this.FlyAwayEvent);
			this.takeoff = true;
			base.Invoke("resetTakeoff", 2f);
			this.anim.SetBool(this.landingBoolHash, false);
			base.StopCoroutine("FleeHigh");
			base.StartCoroutine("FleeHigh");
			this.flyAwayCoolDown = true;
			this.FeatherDice = UnityEngine.Random.Range(0, 10);
			if (this.currentPerchTarget && this.currentPerchTarget.GetComponent<birdHousePerchTargetSetup>())
			{
				UnityEngine.Random.Range(0, 3);
			}
			if (this.FeatherDice == 1)
			{
				this.SpawnAFeather();
			}
		}
	}

	
	private void setNewFlyTarget(Vector3 getPos)
	{
		if (!this.flying)
		{
			base.StopCoroutine("FlyToTarget");
			base.StartCoroutine("FlyToTarget", getPos);
		}
		else if (!this.landing && this.flying)
		{
			this.target = getPos;
			this.generatePathToTarget();
		}
	}

	
	private void findNewTarget()
	{
		if (!this.controller)
		{
			return;
		}
		this.controller.SendMessage("BirdFindTarget", base.gameObject);
	}

	
	private void findNewLandingTarget()
	{
		if (!this.controller)
		{
			return;
		}
		this.controller.SendMessage("BirdFindTarget", base.gameObject);
	}

	
	private void findNewRandomTarget(bool restart)
	{
		Vector2 vector = this.Circle2(75f);
		Vector3 vector2 = new Vector3(this.tr.position.x + vector.x, this.tr.position.y + (float)UnityEngine.Random.Range(-5, 5), vector.y + this.tr.position.z);
		if (!this.landing && this.flying)
		{
			this.target = vector2;
			this.generatePathToTarget();
			this.flyingToPerch = false;
			if (restart)
			{
				base.StopCoroutine("FlyToTarget");
				base.StartCoroutine("FlyToTarget", this.target);
			}
		}
	}

	
	private void findNewTargetIgnorePathing()
	{
	}

	
	private void resetIgnorePathing()
	{
	}

	
	private void setFlyingToPerch()
	{
		this.flyingToPerch = true;
	}

	
	public void flyToRandomTarget(Vector3 randomPos)
	{
		if (!this.landing)
		{
			this.target = randomPos;
			this.generatePathToTarget();
		}
		this.flyingToPerch = false;
	}

	
	private void retryFlyToTarget()
	{
		base.Invoke("findNewTarget", (float)UnityEngine.Random.Range(6, 9));
		base.CancelInvoke("setRandomTarget");
	}

	
	private void resetTakeoff()
	{
		this.flyAwayCoolDown = false;
		this.takeoff = false;
	}

	
	private void SpawnAFeather()
	{
		UnityEngine.Object.Instantiate(this.MyFeather, base.transform.position, base.transform.rotation);
	}

	
	private EventInstance PlayEvent(string path)
	{
		EventInstance eventInstance = FMODCommon.PlayOneshot(path, base.transform);
		if (eventInstance != null)
		{
			this.eventInstances.Add(eventInstance);
		}
		return eventInstance;
	}

	
	private void StopEvents()
	{
		foreach (EventInstance instance in this.eventInstances)
		{
			FMODCommon.StopEvent(instance);
		}
		this.eventInstances.Clear();
		this.StopSong();
	}

	
	private void StopSong()
	{
		FMODCommon.StopEvent(this.songInstance);
		this.songInstance = null;
	}

	
	private void CleanupEventInstances()
	{
		int i = 0;
		while (i < this.eventInstances.Count)
		{
			if (!this.eventInstances[i].isValid())
			{
				this.eventInstances.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	
	private void HitReal(int d)
	{
		this.Hit(d);
	}

	
	private void Hit(int damage)
	{
		this.health -= damage;
		if (this.health < 1)
		{
			this.die();
		}
		else
		{
			this.PlayEvent(this.HitEvent);
		}
	}

	
	private void Explosion(float dist)
	{
		this.die();
	}

	
	private void die()
	{
		if (!this.controller)
		{
			return;
		}
		this.sendDisablePerchTarget();
		if (this.controller.initIdealBirds > 0)
		{
			this.controller.initIdealBirds--;
		}
		if (this.controller.initMaxBirds > 0)
		{
			this.controller.initMaxBirds--;
		}
		base.StopAllCoroutines();
		FMODCommon.PlayOneshot(this.DieEvent, base.transform);
		Transform transform = this.ragDoll.metgoragdoll(default(Vector3));
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			BoltNetwork.Attach(transform.gameObject);
		}
		this.controller.Unspawn(base.gameObject);
	}

	
	private void DieFire()
	{
		if (!this.controller)
		{
			return;
		}
		this.sendDisablePerchTarget();
		if (this.controller.initIdealBirds > 0)
		{
			this.controller.initIdealBirds--;
		}
		if (this.controller.initMaxBirds > 0)
		{
			this.controller.initMaxBirds--;
		}
		base.StopAllCoroutines();
		FMODCommon.PlayOneshot(this.DieEvent, base.transform);
		this.ragDoll.burning = true;
		Transform transform = this.ragDoll.metgoragdoll(default(Vector3));
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			BoltNetwork.Attach(transform.gameObject);
		}
		this.controller.Unspawn(base.gameObject);
	}

	
	private void Flee()
	{
		this.sendDisablePerchTarget();
		this.StopSong();
		this.tempPos = this.Circle2(65f);
		this.farAwayTarget = new Vector3(this.tr.position.x + this.tempPos.x, this.tr.position.y + (float)UnityEngine.Random.Range(20, 35), this.tempPos.y + this.tr.position.z);
		if (!this.flying)
		{
			base.StopCoroutine("FlyToTarget");
			base.StartCoroutine("FlyToTarget", this.farAwayTarget);
		}
		else if (!this.landing && this.flying)
		{
			this.target = this.farAwayTarget;
			this.generatePathToTarget();
		}
		base.Invoke("findNewTarget", UnityEngine.Random.Range(5f, 8f));
		this.flyingToPerch = false;
	}

	
	private IEnumerator FleeHigh()
	{
		this.sendDisablePerchTarget();
		this.StopSong();
		this.tempPos = this.Circle2(70f);
		this.flyingToPerch = false;
		this.farAwayTarget = new Vector3(this.tr.position.x + this.tempPos.x, this.tr.position.y + (float)UnityEngine.Random.Range(20, 35), this.tempPos.y + this.tr.position.z);
		if (!this.flying)
		{
			base.StopCoroutine("FlyToTarget");
			base.StartCoroutine("FlyToTarget", this.farAwayTarget);
		}
		else if (!this.landing && this.flying)
		{
			this.target = this.farAwayTarget;
			this.generatePathToTarget();
		}
		yield return YieldPresets.WaitTwoSeconds;
		float timer = Time.time + UnityEngine.Random.Range(5f, 8f);
		bool endWait = false;
		while (!endWait)
		{
			if (Time.time > timer)
			{
				endWait = true;
			}
			if (Vector3.Distance(this.tr.position, this.farAwayTarget) < 10f)
			{
				endWait = true;
			}
			yield return null;
		}
		this.tempPos = this.Circle2(75f);
		this.farAwayTarget = new Vector3(this.tr.position.x + this.tempPos.x, this.tr.position.y + (float)UnityEngine.Random.Range(-12, 12), this.tempPos.y + this.tr.position.z);
		if (!this.landing && this.flying)
		{
			this.target = this.farAwayTarget;
			this.generatePathToTarget();
		}
		yield return YieldPresets.WaitTwoSeconds;
		timer = Time.time + UnityEngine.Random.Range(5f, 8f);
		endWait = false;
		while (!endWait)
		{
			if (Time.time > timer)
			{
				endWait = true;
			}
			if (Vector3.Distance(this.tr.position, this.farAwayTarget) < 10f)
			{
				endWait = true;
			}
			yield return null;
		}
		this.findNewTarget();
		yield break;
	}

	
	public void FleeBehind()
	{
		this.sendDisablePerchTarget();
		this.StopSong();
		this.farAwayTarget = this.tr.position + this.tr.forward * -30f + (this.tr.right * (float)UnityEngine.Random.Range(-50, 50) + new Vector3(0f, (float)UnityEngine.Random.Range(0, 10), 0f));
		if (!this.flying)
		{
			base.StopCoroutine("FlyToTarget");
			base.StartCoroutine("FlyToTarget", this.farAwayTarget);
		}
		else if (!this.landing && this.flying)
		{
			this.target = this.farAwayTarget;
			this.generatePathToTarget();
		}
		base.Invoke("findNewTarget", UnityEngine.Random.Range(4f, 7f));
		this.fleeing = true;
		base.Invoke("resetFleeing", 1.5f);
		this.flyingToPerch = false;
	}

	
	public void FleeDodgeTree()
	{
		this.sendDisablePerchTarget();
		this.StopSong();
		float d = 1f;
		if (UnityEngine.Random.value > 0.5f)
		{
			d = -1f;
		}
		this.target = this.tr.position + this.tr.forward * 5f + this.tr.right * 30f * d;
		base.Invoke("findNewTarget", UnityEngine.Random.Range(2f, 3f));
		this.fleeing = true;
		base.Invoke("resetFleeing", 1.5f);
		this.flyingToPerch = false;
	}

	
	private void resetFleeing()
	{
		this.fleeing = false;
	}

	
	private void setRandomTarget()
	{
		this.tempPos = this.Circle2(65f);
		this.farAwayTarget = new Vector3(this.tr.position.x + this.tempPos.x, this.tr.position.y + (float)UnityEngine.Random.Range(-3, 10), this.tempPos.y + this.tr.position.z);
		this.target = this.farAwayTarget;
	}

	
	private void SetController(lb_BirdController cont)
	{
		this.controller = cont;
	}

	
	private void ResetHopInt()
	{
		if (base.gameObject.activeSelf)
		{
			this.anim.SetInteger(this.hopIntHash, 0);
		}
	}

	
	private void ResetFlyingLandingVariables()
	{
		if (this.flying || this.landing)
		{
			this.flying = false;
			this.landing = false;
		}
	}

	
	private void PlaySong()
	{
		if (UnityEngine.Random.value < 0.005f && !Clock.planecrash)
		{
			if (base.gameObject.activeSelf)
			{
				this.songInstance = FMODCommon.PlayOneshot(this.SongEvent, base.transform);
			}
			this.songBreak = true;
			base.Invoke("resetSongBreak", 12f);
		}
	}

	
	private void resetSongBreak()
	{
		this.songBreak = false;
	}

	
	private Vector2 Circle2(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	private void setPerchTarget(GameObject target)
	{
		if (target)
		{
			this.currentPerchTarget = target;
		}
	}

	
	private void clearPerchTarget()
	{
		this.currentPerchTarget = null;
	}

	
	private void sendDisablePerchTarget()
	{
		if (this.currentPerchTarget)
		{
			perchTargetSetup component = this.currentPerchTarget.GetComponent<perchTargetSetup>();
			if (!component)
			{
				this.currentPerchTarget.AddComponent<perchTargetSetup>();
			}
			this.currentPerchTarget.SendMessage("disableThisTarget", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void Update()
	{
		if (this.onGround && !this.paused)
		{
			this.OnGroundBehaviors();
		}
		if (!this.controller)
		{
			return;
		}
		if (this.path == null)
		{
			this.isPath = false;
		}
		else
		{
			this.isPath = true;
			this.pathWaypointCount = this.path.vectorPath.Count;
		}
		if (this.flying)
		{
			if (Time.time > this.smallCircleCheckDelay)
			{
				float num = Vector3.Distance(this.lastPos, this.tr.position);
				if (num < 4.5f && this.flying)
				{
					this.findNewRandomTarget(true);
				}
				this.lastPos = this.tr.position;
				this.smallCircleCheckDelay = Time.time + 2f;
			}
			this.groundHeight = Terrain.activeTerrain.SampleHeight(this.tr.position) + Terrain.activeTerrain.transform.position.y;
			if (Time.time > this.updatePathTimer)
			{
				Vector3 position = this.tr.position;
				position.y = this.groundHeight;
				GraphNode node = this.rg.GetNearest(position, NNConstraint.Default).node;
				if (node != null)
				{
					bool flag = false;
					using (List<uint>.Enumerator enumerator = Scene.MutantControler.mostCommonArea.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int num2 = (int)enumerator.Current;
							if ((long)num2 == (long)((ulong)node.Area))
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						this.onValidArea = true;
					}
					else
					{
						this.onValidArea = false;
					}
				}
				if (this.onValidArea)
				{
					this.generatePathToTarget();
				}
				this.updatePathTimer = Time.time + 3f;
			}
		}
	}

	
	private void playFMODEvent(string path)
	{
	}

	
	public virtual void SearchPath()
	{
		this.seeker.StartPath(base.transform.position, this.target, new OnPathDelegate(this.OnPathComplete));
	}

	
	public void OnPathComplete(Path p)
	{
		if (!p.error)
		{
			if (this.path != null)
			{
				this.path.Release(this, false);
			}
			this.path = p;
			this.currentWaypoint = 1;
			this.path.Claim(this);
		}
	}

	
	private void generatePathToTarget()
	{
		this.SearchPath();
		base.StopCoroutine("doMovement");
		base.StartCoroutine("doMovement");
	}

	
	private IEnumerator doMovement()
	{
		for (;;)
		{
			while (this.path == null)
			{
				yield return null;
			}
			if (this.currentWaypoint < this.path.vectorPath.Count - 1)
			{
				Vector3 modPos = this.path.vectorPath[this.currentWaypoint];
				modPos.y = base.transform.position.y;
				float dist = this.XZSqrMagnitude(modPos, base.transform.position);
				if (dist < this.nextWaypointDistance * this.nextWaypointDistance)
				{
					this.currentWaypoint++;
				}
				this.targetWaypoint = this.path.vectorPath[this.currentWaypoint];
				this.targetWaypoint.y = base.transform.position.y;
			}
			yield return null;
		}
		yield break;
	}

	
	protected float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	
	[Header("FMOD")]
	public string SongEvent;

	
	public string FlyAwayEvent;

	
	public string HitEvent;

	
	public string DieEvent;

	
	public string StartleEvent;

	
	[Space(10f)]
	public bool typeSparrow;

	
	public bool typeSeagull;

	
	public bool typeCrow;

	
	public bool typeRedBird;

	
	public bool typeBlueBird;

	
	public float rotateSpeed;

	
	public float flyingSpeed;

	
	private float initFlyingSpeed;

	
	private float initRotateSpeed;

	
	public GameObject MyFeather;

	
	public int health;

	
	private bool songBreak;

	
	public bool birdVisible;

	
	public GameObject currentPerchTarget;

	
	private Animator anim;

	
	public lb_BirdController controller;

	
	private clsragdollify ragDoll;

	
	public SkinnedMeshRenderer skin;

	
	private Transform tr;

	
	private bool paused;

	
	public bool idle = true;

	
	public bool flying;

	
	public bool landing;

	
	public bool perched;

	
	public bool onGround = true;

	
	public bool takeoff;

	
	public bool fleeing;

	
	public bool flyingToPerch;

	
	public float distanceToTarget;

	
	private float agitationLevel = 0.5f;

	
	private float originalAnimSpeed = 1f;

	
	public float repathRate;

	
	public float approachDist;

	
	public float deadZone;

	
	private Vector3 getDir;

	
	public Vector3 wantedDir;

	
	public bool aimAtWaypoint;

	
	public Transform pathTarget;

	
	public Seeker seeker;

	
	public Path path;

	
	public float speed = 5f;

	
	public float nextWaypointDistance = 3f;

	
	public int currentWaypoint;

	
	public bool isFollowing;

	
	public bool inTree;

	
	public bool cansearch;

	
	public bool isPath;

	
	public int pathWaypointCount;

	
	private RecastGraph rg;

	
	private List<EventInstance> eventInstances = new List<EventInstance>();

	
	private EventInstance songInstance;

	
	private int idleAnimationHash;

	
	private int singAnimationHash;

	
	private int ruffleAnimationHash;

	
	private int preenAnimationHash;

	
	private int peckAnimationHash;

	
	private int hopForwardAnimationHash;

	
	private int hopBackwardAnimationHash;

	
	private int hopLeftAnimationHash;

	
	private int hopRightAnimationHash;

	
	private int worriedAnimationHash;

	
	private int landingAnimationHash;

	
	private int flyTagHash;

	
	private int hopIntHash;

	
	private int flyingBoolHash;

	
	private int peckBoolHash;

	
	private int ruffleBoolHash;

	
	private int preenBoolHash;

	
	private int landingBoolHash;

	
	private int singTriggerHash;

	
	private int flyingDirectionHash;

	
	private int FeatherDice;

	
	private Quaternion lastRotation;

	
	private Quaternion desiredRotation;

	
	private Quaternion startingRotation;

	
	private Quaternion finalRotation;

	
	private Vector3 vectorDirectionToTarget;

	
	private Vector3 desiredDirection;

	
	private Vector3 farAwayTarget;

	
	private Vector3 diff;

	
	private Vector3 lastDir;

	
	private Vector3 currentDir;

	
	public Vector3 target;

	
	private Vector2 tempPos;

	
	private float absDir;

	
	private float smoothedDir;

	
	private int layerMask1;

	
	private RaycastHit hit;

	
	private bool initBool;

	
	private Vector3 lastPos;

	
	private Vector3 currPos;

	
	private float smallCircleCheckDelay;

	
	private bool groundCoolDown;

	
	private float groundAvoidTimer;

	
	private bool flyAwayCoolDown;

	
	private float updatePathTimer;

	
	private bool onValidArea;

	
	private float groundHeight;

	
	public Vector3 targetWaypoint;

	
	private enum birdBehaviors
	{
		
		sing,
		
		preen,
		
		ruffle,
		
		peck,
		
		hopForward,
		
		hopBackward,
		
		hopLeft,
		
		hopRight
	}
}
