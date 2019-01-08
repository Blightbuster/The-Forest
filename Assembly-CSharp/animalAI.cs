using System;
using System.Collections;
using HutongGames.PlayMaker;
using Pathfinding;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;

public class animalAI : MonoBehaviour
{
	private void Awake()
	{
		this.avoidScript = base.transform.GetComponentInChildren<animalAvoidance1>();
		this.sceneInfo = Scene.SceneTracker;
		if (this.parentSetup)
		{
			this.avatar = base.GetComponentInChildren<Animator>();
		}
		else
		{
			this.avatar = base.GetComponent<Animator>();
		}
		this.spawnFunctions = base.GetComponent<animalSpawnFunctions>();
		this.rootTr = base.transform;
		this.Tr = this.avatar.transform;
		this.deadZone *= 0.0174532924f;
		this.searchFunctions = base.transform.GetComponent<animalSearchFunctions>();
		this.seeker = base.GetComponent<Seeker>();
		this.layerMask2 = 33556480;
		PlayMakerFSM[] components = base.gameObject.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "aiBaseFSM")
			{
				this.playMaker = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "moveFSM")
			{
				this.pmMove = playMakerFSM;
			}
		}
	}

	private void Start()
	{
		this.avatar.enabled = true;
		base.gameObject.tag = "animalRoot";
		this.fsmPathDir = this.playMaker.FsmVariables.GetFsmFloat("pathDir");
		this.fsmSmoothPathDir = this.playMaker.FsmVariables.GetFsmFloat("smoothedPathDir");
		this.fsmTargetDir = this.playMaker.FsmVariables.GetFsmFloat("targetDir");
		if (this.pmMove)
		{
			this.fsmTargetVec = this.pmMove.FsmVariables.GetFsmVector3("targetDir");
		}
		this.fsmPlayerDist = this.playMaker.FsmVariables.GetFsmFloat("playerDist");
		this.fsmPlayerAngle = this.playMaker.FsmVariables.GetFsmFloat("playerAngle");
		if (this.pmMove)
		{
			this.fsmRotateSpeed = this.pmMove.FsmVariables.GetFsmFloat("rotateSpeed");
		}
		this.fsmTreeBool = this.playMaker.FsmVariables.GetFsmBool("treeBool");
		this.fsmFollowGo = this.playMaker.FsmVariables.GetFsmGameObject("closeAnimalGo");
		this.fsmIsAttacking = this.playMaker.FsmVariables.GetFsmBool("isAttacking");
		this.animatorTr = base.transform;
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name.Contains("ANIM_base"))
			{
				this.playMaker.FsmVariables.GetFsmGameObject("animatorGo").Value = transform.gameObject;
				this.playMaker.FsmVariables.GetFsmGameObject("animatorGO").Value = transform.gameObject;
				if (this.pmMove)
				{
					this.pmMove.FsmVariables.GetFsmGameObject("animatorGo").Value = transform.gameObject;
				}
				this.animatorTr = transform;
			}
			float time = UnityEngine.Random.Range(0.1f, 1.3f);
			if (!base.IsInvoking("updatePlayerTargets"))
			{
				base.InvokeRepeating("updatePlayerTargets", time, 1.2f);
			}
			this.doEnable = true;
		}
		if (this.searchFunctions.currentWaypoint)
		{
			this.target = this.searchFunctions.currentWaypoint.transform;
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			GameObject closestPlayerFromPos = Scene.SceneTracker.GetClosestPlayerFromPos(this.Tr.position);
			if (closestPlayerFromPos)
			{
				this.target = closestPlayerFromPos.transform;
			}
		}
		if (this.followOtherAnimals)
		{
			this.playMaker.FsmVariables.GetFsmBool("followOthersBool").Value = true;
		}
	}

	private void OnEnable()
	{
		if (!this.avatar)
		{
			if (this.parentSetup)
			{
				this.avatar = base.GetComponentInChildren<Animator>();
			}
			else
			{
				this.avatar = base.GetComponent<Animator>();
			}
		}
		this.avatar.enabled = true;
		this.ragdollSpawned = false;
		if (this.disableWhenOffscreen.Length > 0)
		{
			for (int i = 0; i < this.disableWhenOffscreen.Length; i++)
			{
				this.disableWhenOffscreen[i].SetActive(true);
			}
		}
		float time = UnityEngine.Random.Range(0.1f, 1.3f);
		if (!base.IsInvoking("updatePlayerTargets"))
		{
			base.InvokeRepeating("updatePlayerTargets", time, 2f);
		}
		if (this.doEnable)
		{
			this.isFollowing = false;
			this.inTree = false;
			if (this.playMaker)
			{
				this.playMaker.SendEvent("START");
			}
			if (this.spawnFunctions.deer)
			{
				float num = UnityEngine.Random.Range(1.1f, 1.27f);
				this.animatorTr.localScale = new Vector3(num, num, num);
			}
			if (this.spawnFunctions.rabbit)
			{
				float num2 = UnityEngine.Random.Range(6.6f, 7.8f);
				this.animatorTr.localScale = new Vector3(num2, num2, num2);
			}
			if (this.spawnFunctions.raccoon)
			{
				float num3 = UnityEngine.Random.Range(0.95f, 1.1f);
				this.animatorTr.localScale = new Vector3(num3, num3, num3);
			}
		}
	}

	private void updatePlayerTargets()
	{
	}

	public void setInTree()
	{
		this.inTree = true;
	}

	public void disableFollowingBool()
	{
		this.isFollowing = false;
	}

	protected float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	public void goRagdoll()
	{
		if (this.ragdollSpawned)
		{
			return;
		}
		this.ragdollSpawned = true;
		Transform transform = this.ragdoll.metgoragdoll(default(Vector3));
		if (PoolManager.Pools["creatures"].IsSpawned(this.rootTr))
		{
			this.spawnFunctions.despawn();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		CoopMutantDummyToken coopMutantDummyToken = new CoopMutantDummyToken();
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		this.skinRenderer.GetPropertyBlock(materialPropertyBlock);
		coopMutantDummyToken.skinDamage1 = materialPropertyBlock.GetFloat("_Damage1");
		coopMutantDummyToken.skinDamage2 = materialPropertyBlock.GetFloat("_Damage2");
		coopMutantDummyToken.skinDamage3 = materialPropertyBlock.GetFloat("_Damage3");
		coopMutantDummyToken.skinDamage4 = materialPropertyBlock.GetFloat("_Damage4");
		if (BoltNetwork.isRunning)
		{
			BoltNetwork.Attach(transform.gameObject, coopMutantDummyToken);
		}
	}

	public void goBurntRagdoll()
	{
		if (this.ragdollSpawned || !this.BurntRagdoll)
		{
			return;
		}
		this.ragdollSpawned = true;
		Transform transform = UnityEngine.Object.Instantiate<GameObject>(this.BurntRagdoll, base.transform.position, base.transform.rotation).transform;
		transform.localScale = base.transform.localScale;
		if (PoolManager.Pools["creatures"].IsSpawned(this.rootTr))
		{
			this.spawnFunctions.despawn();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (BoltNetwork.isRunning)
		{
			BoltNetwork.Attach(transform.gameObject);
		}
	}

	public virtual void SearchPath()
	{
		if (this.target == null || !this.cansearch)
		{
			return;
		}
		this.seeker.StartPath(this.Tr.position, this.target.position, new OnPathDelegate(this.OnPathComplete));
	}

	private void getPlayerAngle()
	{
		if (Scene.SceneTracker.allPlayers.Count > 0)
		{
			if (SteamDSConfig.isDedicatedServer)
			{
				GameObject closestPlayerFromPos = Scene.SceneTracker.GetClosestPlayerFromPos(this.Tr.position);
				if (closestPlayerFromPos)
				{
					this.playerTarget = this.animatorTr.InverseTransformPoint(closestPlayerFromPos.transform.position);
					this.fsmPlayerAngle.Value = Mathf.Atan2(this.playerTarget.x, this.playerTarget.z) * 57.29578f;
				}
			}
			else if (Scene.SceneTracker.allPlayers[0] != null)
			{
				this.playerTarget = this.animatorTr.InverseTransformPoint(Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position).transform.position);
				this.fsmPlayerAngle.Value = Mathf.Atan2(this.playerTarget.x, this.playerTarget.z) * 57.29578f;
			}
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			this.fsmPlayerDist.Value = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.Tr.position);
		}
		else if (LocalPlayer.Transform)
		{
			this.fsmPlayerDist.Value = Vector3.Distance(LocalPlayer.Transform.position, this.Tr.position);
		}
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

	private void OnDisable()
	{
		base.CancelInvoke("startRandomSwimSpeed");
		base.CancelInvoke("repeatSwimSpeed");
		base.CancelInvoke("updatePlayerTargets");
		base.CancelInvoke("closeObjectAvoidanc");
		base.StopAllCoroutines();
	}

	private void OnDestroy()
	{
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = null;
	}

	private void Update()
	{
		if (Scene.SceneTracker.allPlayers.Count > 0)
		{
			GameObject closestPlayerFromPos = Scene.SceneTracker.GetClosestPlayerFromPos(this.Tr.position);
			if (closestPlayerFromPos)
			{
				if (this.target == null && this.searchFunctions.currentWaypoint)
				{
					this.target = this.searchFunctions.currentWaypoint.transform;
				}
				this.fsmPlayerDist.Value = Vector3.Distance(closestPlayerFromPos.transform.position, this.Tr.position);
				this.playerTarget = this.animatorTr.InverseTransformPoint(closestPlayerFromPos.transform.position);
				this.fsmPlayerAngle.Value = Mathf.Atan2(this.playerTarget.x, this.playerTarget.z) * 57.29578f;
				if (this.spawnFunctions.deer || this.spawnFunctions.crocodile || this.spawnFunctions.boar)
				{
					this.avatar.SetFloatReflected("playerAngle", this.fsmPlayerAngle.Value);
				}
				this.animalTarget = LocalPlayer.Transform.InverseTransformPoint(this.animatorTr.position);
				if (SteamDSConfig.isDedicatedServer)
				{
					GameObject closestPlayerFromPos2 = Scene.SceneTracker.GetClosestPlayerFromPos(this.Tr.position);
					if (closestPlayerFromPos2)
					{
						this.animalTarget = closestPlayerFromPos2.transform.InverseTransformPoint(this.animatorTr.position);
					}
				}
				this.animalAngle = Mathf.Atan2(this.animalTarget.x, this.animalTarget.z) * 57.29578f;
				if (this.target)
				{
					Vector3 vector = base.transform.InverseTransformPoint(this.target.position);
					float value = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
					this.fsmTargetDir.Value = value;
				}
				if (!this.skinRenderer.isVisible && (this.animalAngle < -90f || this.animalAngle > 90f) && this.fsmPlayerDist.Value > 10f && !this.fsmTreeBool.Value && !this.spawnFunctions.crocodile && !BoltNetwork.isRunning)
				{
					if (this.avatar.enabled)
					{
						this.avatar.enabled = false;
					}
					if (this.disableWhenOffscreen.Length > 0)
					{
						for (int i = 0; i < this.disableWhenOffscreen.Length; i++)
						{
							this.disableWhenOffscreen[i].SetActive(false);
						}
					}
				}
				else
				{
					if (!this.avatar.enabled)
					{
						this.avatar.enabled = true;
					}
					if (this.disableWhenOffscreen.Length > 0)
					{
						for (int j = 0; j < this.disableWhenOffscreen.Length; j++)
						{
							this.disableWhenOffscreen[j].SetActive(true);
						}
					}
				}
			}
		}
	}

	private void startRunning()
	{
		FMODCommon.PlayOneshotNetworked(this.startleEvent, base.transform, FMODCommon.NetworkRole.Server);
	}

	public void startMovement()
	{
		if (!base.IsInvoking("SearchPath"))
		{
			base.InvokeRepeating("SearchPath", 0f, 3f);
		}
		this.doMove = true;
		this.cansearch = true;
		base.StartCoroutine("doMovement");
	}

	public void stopMovement()
	{
		base.CancelInvoke("SearchPath");
		this.cansearch = false;
		this.doMove = false;
		base.StopCoroutine("doMovement");
	}

	private IEnumerator doMovement()
	{
		IL_754:
		while (this.doMove)
		{
			if (this.spawnFunctions.turtle && this.avatar && this.target && this.avatar.GetBool("swimming"))
			{
				this.getDir = (this.target.position - this.avatar.rootPosition).normalized;
				this.wantedDir = this.getDir;
				if (this.pmMove)
				{
					this.fsmTargetVec.Value = this.wantedDir;
				}
			}
			while (this.path == null || this.avatar == null)
			{
				yield return null;
			}
			if (this.forceDir && this.forceTarget)
			{
				this.getDir = (this.forceTarget.position - this.avatar.rootPosition).normalized;
			}
			else
			{
				try
				{
					this.getDir = (this.path.vectorPath[this.currentWaypoint] - this.avatar.rootPosition).normalized;
				}
				catch
				{
					base.StartCoroutine("restartDoMovement");
				}
			}
			if (this.spawnFunctions.crocodile || this.spawnFunctions.lizard)
			{
				if (this.avatar.GetFloat("Speed") > 0.8f)
				{
					this.wantedDir = Vector3.Slerp(this.wantedDir, this.getDir, Time.deltaTime * 15f);
				}
				else
				{
					this.wantedDir = Vector3.Slerp(this.wantedDir, this.getDir, Time.deltaTime * 5f);
				}
			}
			else
			{
				this.wantedDir = this.getDir;
			}
			if (this.pmMove)
			{
				this.fsmTargetVec.Value = this.wantedDir;
			}
			while (this.currentWaypoint < this.path.vectorPath.Count - 1)
			{
				float num = this.XZSqrMagnitude(this.path.vectorPath[this.currentWaypoint], this.Tr.position);
				if (num >= this.nextWaypointDistance * this.nextWaypointDistance)
				{
					IL_3B6:
					this.currentDir = (this.avatar.rootRotation * Vector3.forward).normalized;
					this.angle = this.FindAngle(this.Tr.forward, this.wantedDir, this.Tr.up);
					if (this.turnInt == 1 && !this.fsmTreeBool.Value)
					{
						if (this.spawnFunctions.boar)
						{
							this.absDir = -1f;
						}
						else
						{
							this.absDir = 1f;
						}
					}
					else if (this.turnInt == -1 && !this.fsmTreeBool.Value)
					{
						if (this.spawnFunctions.boar)
						{
							this.absDir = 1f;
						}
						else
						{
							this.absDir = -1f;
						}
					}
					else if (Mathf.Abs(this.angle) < this.deadZone)
					{
						this.absDir = 0f;
					}
					else if (Vector3.Dot(this.currentDir, this.wantedDir) > 0f)
					{
						this.absDir = Vector3.Cross(this.currentDir, this.wantedDir).y;
					}
					else
					{
						this.absDir = (float)((Vector3.Cross(this.currentDir, this.wantedDir).y <= 0f) ? -1 : 1);
					}
					float curVel = 0f;
					this.smoothSpeed = 0.08f;
					if (this.spawnFunctions.deer)
					{
						this.smoothSpeed = 0.04f;
					}
					if (this.spawnFunctions.crocodile || this.spawnFunctions.lizard)
					{
						if (this.avatar.GetFloat("Speed") > 0.8f)
						{
							this.smoothSpeed = 0.09f;
						}
						else
						{
							this.smoothSpeed = 0.2f;
						}
					}
					this.smoothedDir = Mathf.SmoothDamp(this.smoothedDir, this.absDir, ref curVel, this.smoothSpeed);
					if (this.spawnFunctions.crocodile)
					{
						this.avatar.SetFloatReflected("Direction", this.smoothedDir * 10f);
					}
					else
					{
						this.avatar.SetFloatReflected("Direction", this.smoothedDir);
					}
					this.fsmSmoothPathDir.Value = this.smoothedDir;
					this.fsmPathDir.Value = this.absDir;
					yield return null;
					goto IL_754;
				}
				this.currentWaypoint++;
			}
			goto IL_3B6;
		}
		yield break;
	}

	private void getTargetAngle()
	{
		if (!this.target)
		{
			return;
		}
		Vector3 vector = base.transform.InverseTransformPoint(this.target.position);
		float value = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		this.fsmTargetDir.Value = value;
	}

	private void closeObjectAvoidance()
	{
	}

	private float FindAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector)
	{
		if (toVector == Vector3.zero)
		{
			return 0f;
		}
		float num = Vector3.Angle(fromVector, toVector);
		Vector3 lhs = Vector3.Cross(fromVector, toVector);
		num *= Mathf.Sign(Vector3.Dot(lhs, upVector));
		return num * 0.0174532924f;
	}

	public IEnumerator enableForceTarget(GameObject target)
	{
		this.forceDir = true;
		this.forceTarget = target.transform;
		yield return null;
		yield break;
	}

	public void disableForceTarget()
	{
		this.forceDir = false;
		this.forceTarget = null;
	}

	private IEnumerator restartDoMovement()
	{
		base.StopCoroutine("doMovement");
		yield return YieldPresets.WaitForFixedUpdate;
		yield return YieldPresets.WaitForFixedUpdate;
		while (this.path == null)
		{
			yield return YieldPresets.WaitForFixedUpdate;
		}
		if (this.doMove)
		{
			base.StartCoroutine("doMovement");
		}
		yield break;
	}

	private IEnumerator toMove()
	{
		if (this.startedMove)
		{
			yield break;
		}
		base.StopCoroutine("toRun");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toSearch");
		base.StopCoroutine("toTrot");
		base.StopCoroutine("toWalk");
		this.startMovement();
		this.startedMove = true;
		this.startedWalk = false;
		this.startedRun = false;
		this.startedFollow = false;
		this.startedTrot = false;
		if (this.spawnFunctions.deer || this.spawnFunctions.raccoon)
		{
			this.avatar.SetFloatReflected("Speed", UnityEngine.Random.Range(0.21f, 1f));
		}
		for (;;)
		{
			if (!this.spawnFunctions.deer && !this.spawnFunctions.raccoon)
			{
				this.avatar.SetFloatReflected("Speed", 0.3f);
			}
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	private IEnumerator toWalk()
	{
		if (this.startedWalk)
		{
			yield break;
		}
		base.StopCoroutine("toRun");
		base.StopCoroutine("toMove");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toSearch");
		base.StopCoroutine("toTrot");
		this.startMovement();
		this.startedWalk = true;
		this.startedRun = false;
		this.startedMove = false;
		this.startedFollow = false;
		this.startedTrot = false;
		for (;;)
		{
			if (!this.spawnFunctions.turtle)
			{
				this.avatar.SetFloatReflected("Speed", 0.3f);
			}
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	private IEnumerator toTrot()
	{
		if (this.startedTrot)
		{
			yield break;
		}
		base.StopCoroutine("toRun");
		base.StopCoroutine("toMove");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toSearch");
		base.StopCoroutine("toWalk");
		this.startMovement();
		this.startedTrot = true;
		this.startedRun = false;
		this.startedMove = false;
		this.startedFollow = false;
		this.startedWalk = false;
		for (;;)
		{
			if (!this.spawnFunctions.turtle)
			{
				this.avatar.SetFloatReflected("Speed", 0.6f);
			}
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	private IEnumerator toRun()
	{
		if (this.startedRun)
		{
			yield break;
		}
		base.StopCoroutine("toMove");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toSearch");
		base.StopCoroutine("toTrot");
		base.StopCoroutine("toWalk");
		this.startMovement();
		if (this.spawnFunctions.rabbit)
		{
			this.avatar.speed = UnityEngine.Random.Range(1.5f, 2f);
		}
		if (this.spawnFunctions.lizard)
		{
			this.avatar.speed = UnityEngine.Random.Range(1.2f, 1.6f);
		}
		if (this.spawnFunctions.boar)
		{
			this.avatar.speed = 1.15f;
		}
		if (this.spawnFunctions.raccoon)
		{
			this.avatar.speed = UnityEngine.Random.Range(1.05f, 1.35f);
		}
		this.startedRun = true;
		this.startedMove = false;
		this.startedFollow = false;
		this.startedTrot = false;
		this.startedWalk = false;
		for (;;)
		{
			if (!this.spawnFunctions.turtle)
			{
				this.avatar.SetFloatReflected("Speed", 1f);
			}
			yield return YieldPresets.WaitPointThreeSeconds;
		}
		yield break;
	}

	private IEnumerator toFollow()
	{
		if (this.startedFollow)
		{
			yield break;
		}
		base.StopCoroutine("toMove");
		base.StopCoroutine("toRun");
		base.StopCoroutine("toStop");
		base.StopCoroutine("toTrot");
		base.StopCoroutine("toWalk");
		this.startMovement();
		this.startedFollow = true;
		this.startedRun = false;
		this.startedMove = false;
		this.startedTrot = false;
		this.startedWalk = false;
		this.followDist = Vector3.Distance(this.fsmFollowGo.Value.transform.position, this.Tr.position);
		for (;;)
		{
			if (!this.spawnFunctions.turtle)
			{
				if (this.followDist < 8f)
				{
					this.avatar.SetFloatReflected("Speed", 0.3f);
				}
				else if (this.followDist > 15f)
				{
					this.avatar.SetFloatReflected("Speed", 1f);
				}
				else
				{
					this.avatar.SetFloatReflected("Speed", 0.55f);
				}
			}
			yield return YieldPresets.WaitPointOneSeconds;
		}
		yield break;
	}

	private IEnumerator toStop()
	{
		base.StopCoroutine("toMove");
		base.StopCoroutine("toRun");
		base.StopCoroutine("toSearch");
		base.StopCoroutine("toTrot");
		base.StopCoroutine("toWalk");
		this.stopMovement();
		if (!this.spawnFunctions.turtle)
		{
			this.avatar.SetFloatReflected("Speed", 0f);
		}
		this.startedRun = false;
		this.startedMove = false;
		this.startedFollow = false;
		this.startedTrot = false;
		this.startedWalk = false;
		yield return null;
		yield break;
	}

	public void startRandomSwimSpeed()
	{
		if (base.enabled && !base.IsInvoking("repeatSwimSpeed"))
		{
			base.InvokeRepeating("repeatSwimSpeed", 2f, (float)UnityEngine.Random.Range(15, 35));
		}
	}

	private void repeatSwimSpeed()
	{
		if (base.enabled)
		{
			base.StartCoroutine("enableRandomSwimSpeed");
		}
	}

	public IEnumerator enableRandomSwimSpeed()
	{
		if (!base.enabled)
		{
			yield break;
		}
		float currSpeed = this.avatar.GetFloat("swimSpeed");
		float setSpeed = UnityEngine.Random.Range(0f, 0.4f);
		float t = 0f;
		while (t < 2f)
		{
			this.avatar.SetFloatReflected("swimSpeed", setSpeed, 2f, Time.deltaTime);
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	public void disableRandomSwimSpeed()
	{
		base.CancelInvoke("repeatSwimSpeed");
	}

	public void getPlayerInWater()
	{
		if (Scene.SceneTracker.allPlayers.Count > 0 && Scene.SceneTracker.allPlayers[0])
		{
			this.playMaker.FsmVariables.GetFsmBool("playerInWater").Value = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position).GetComponent<playerAiInfo>().isSwimming;
		}
	}

	private animalAvoidance1 avoidScript;

	public SkinnedMeshRenderer skinRenderer;

	public GameObject[] disableWhenOffscreen;

	public bool parentSetup;

	public float avoidanceRate;

	public bool followOtherAnimals;

	public string startleEvent;

	public clsragdollify ragdoll;

	public GameObject BurntRagdoll;

	private animalSpawnFunctions spawnFunctions;

	private sceneTracker sceneInfo;

	public animalSearchFunctions searchFunctions;

	public Animator avatar;

	private AnimatorStateInfo currentState;

	private AnimatorStateInfo nextState;

	public PlayMakerFSM playMaker;

	public PlayMakerFSM pmMove;

	public float smoothedDir;

	public float absDir;

	public float repathRate;

	public Transform forceTarget;

	private Transform Tr;

	private Transform rootTr;

	public Transform animatorTr;

	public float approachDist;

	public float deadZone;

	public float rotationSpeed;

	public float smoothSpeed;

	private Vector3 getDir;

	public Vector3 wantedDir;

	public bool aimAtWaypoint;

	private Vector3 currentDir;

	private float angle;

	public bool doMove;

	public bool closeTurn;

	public int turnInt;

	private bool doEnable;

	private Vector3 playerTarget;

	private Vector3 animalTarget;

	private bool forceDir;

	private float animalAngle;

	public bool swimming;

	private FsmFloat fsmPathDir;

	private FsmFloat fsmSmoothPathDir;

	private FsmFloat fsmSpeed;

	private FsmFloat fsmTargetDir;

	private FsmVector3 fsmTargetVec;

	private FsmFloat fsmIkBlend;

	public FsmFloat fsmPlayerDist;

	private FsmFloat fsmPlayerAngle;

	private FsmGameObject fsmAnimatorGO;

	public FsmFloat fsmRotateSpeed;

	private FsmBool fsmTreeBool;

	private FsmGameObject fsmFollowGo;

	private FsmBool fsmIsAttacking;

	private int layerMask2;

	private RaycastHit hit2;

	private Vector3 pos;

	public Transform target;

	private Seeker seeker;

	public Path path;

	public float speed = 5f;

	public float nextWaypointDistance = 3f;

	public int currentWaypoint;

	public bool isFollowing;

	public bool inTree;

	public bool cansearch;

	private bool ragdollSpawned;

	private bool startedRun;

	private bool startedMove;

	private bool startedFollow;

	private bool startedTrot;

	private bool startedWalk;

	private float followDist;
}
