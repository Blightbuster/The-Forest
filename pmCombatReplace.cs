using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class pmCombatReplace : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.transform.GetComponent<mutantScriptSetup>();
		this.search = this.setup.search;
		this.animControl = this.setup.animControl;
		this.hashId = this.setup.hashs;
		this.ai = this.setup.ai;
		this.animator = this.setup.animator;
		this.tr = base.transform;
		this.rootTr = base.transform.parent.transform;
		this.fsmCurrentAttackerGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentAttackerGo");
		this.fsmCurrentMemberGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo");
		this.fsmWayPointGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("wayPointGO");
		this.fsmTreeGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("treeGO");
		this.fsmNextTreeGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("nextTreeGO");
		this.fsmWallGo = this.setup.pmCombat.FsmVariables.GetFsmGameObject("wallGo");
		this.fsmOnFireBool = this.setup.pmCombat.FsmVariables.GetFsmBool("onFireBool");
		this.fsmOnStructureBool = this.setup.pmCombat.FsmVariables.GetFsmBool("onStructureBool");
		this.fsmFearBool = this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL");
		this.fsmCloseCombatBool = this.setup.pmCombat.FsmVariables.GetFsmBool("closeCombatBool");
		this.fsmStalking = this.setup.pmCombat.FsmVariables.GetFsmBool("stalking");
		this.fsmIsStalkingBool = this.setup.pmCombat.FsmVariables.GetFsmBool("isStalkingBool");
		this.fsmJumpDownBool = this.setup.pmCombat.FsmVariables.GetFsmBool("jumpDownBool");
		this.fsmDeathBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool");
		this.fsmDeathRecoverBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathRecoverBool");
		this.fsmdeathFinalBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathFinal");
		this.fsmInsideBase = this.setup.pmCombat.FsmVariables.GetFsmBool("fsmInsideBase");
		this.fsmDoLeaderCallFollower = this.setup.pmCombat.FsmVariables.GetFsmBool("doLeaderCallFollower");
		this.fsmRescueBool = this.setup.pmCombat.FsmVariables.GetFsmBool("rescueBool");
		this.fsmToClimbWall = this.setup.pmCombat.FsmVariables.GetFsmBool("toClimbWall");
		this.fsmInTreeBool = this.setup.pmCombat.FsmVariables.GetFsmBool("inTreeBool");
		this.fsmAttackStructure = this.setup.pmCombat.FsmVariables.GetFsmBool("attackStructure");
		this.fsmDoFoodStructure = this.setup.pmCombat.FsmVariables.GetFsmBool("doFoodStructure");
		this.fsmEatBodyBool = this.setup.pmCombat.FsmVariables.GetFsmBool("eatBodyBool");
		this.fsmTargetSeenBool = this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool");
		this.fsmLeaderBool = this.setup.pmBrain.FsmVariables.GetFsmBool("leaderBool");
		this.fsmEnableControllerBool = this.setup.pmBrain.FsmVariables.GetFsmBool("enableControllerBool");
		this.fsmFemaleSkinnyBool = this.setup.pmBrain.FsmVariables.GetFsmBool("femaleSkinnyBool");
		this.fsmBrainEatBodyBool = this.setup.pmBrain.FsmVariables.GetFsmBool("eatBodyBool");
		this.fsmFearOverrideBool = this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool");
		this.fsmPlayerIsRed = this.setup.pmBrain.FsmVariables.GetFsmBool("playerIsRed");
		this.fsmDisableControllerBool = this.setup.pmBrain.FsmVariables.GetFsmBool("disableControllerBool");
	}

	
	public void resetCoroutines()
	{
		base.StopAllCoroutines();
	}

	
	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	
	private IEnumerator doResetRoutine()
	{
		if (!this.setup)
		{
			yield break;
		}
		if (!this.rootTr)
		{
			this.rootTr = base.transform.parent.transform;
		}
		this.rootTr.gameObject.layer = 14;
		this.setup.animControl.resetInTrap();
		this.setup.mutantStats.setTargetUp();
		this.setup.search.resetCurrentJumpObj();
		this.setup.ai.resetCombatParams();
		this.setup.enemyEvents.disableAllWeapons();
		this.setup.ai.StartCoroutine("toStop");
		if (UnityEngine.Random.value > 0.91f)
		{
			this.doAction = false;
			if (this.setup.search.findCloseTrapTrigger())
			{
				this.setup.pmCombat.SendEvent("goToTrapTrigger");
				yield break;
			}
		}
		this.setup.search.setToClosestPlayer();
		this.setup.health.getCurrentHealth();
		yield return null;
		yield break;
	}

	
	private IEnumerator gotHitRoutine()
	{
		this.setup.animControl.runGotHitScripts();
		this.setup.ai.resetCombatParams();
		this.setup.animator.SetInteger("randInt2", UnityEngine.Random.Range(0, 2));
		this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool").Value = true;
		this.setup.familyFunctions.Invoke("resetFearOverride", 7f);
		yield return null;
		if (this.animator.GetInteger("hitDirection") < 5)
		{
			Vector3 position = this.setup.search.currentTarget.transform.position;
			position.y = this.tr.position.y;
			this.tr.LookAt(position, Vector3.up);
		}
		if (this.setup.pmSleep.enabled)
		{
			this.setup.pmSleep.SendEvent("gotHit");
		}
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 5));
		this.ai.StartCoroutine("toStop");
		this.setup.health.checkDeathState();
		this.animator.SetBool("damageBOOL", false);
		this.animator.SetBool("damageBehindBOOL", false);
		float timer = Time.time + 0.3f;
		while (Time.time < timer)
		{
			if (this.setup.animControl.currLayerState1.tagHash == this.setup.animControl.deathHash)
			{
				this.setup.pmCombat.SendEvent("toDeath");
				yield break;
			}
			yield return null;
		}
		if (BoltNetwork.isRunning)
		{
			yield return YieldPresets.WaitPointTwoSeconds;
		}
		else
		{
			yield return YieldPresets.WaitPointSixSeconds;
			yield return YieldPresets.WaitPointZeroFiveSeconds;
		}
		this.animator.SetBool("staggerBOOL", false);
		if (this.fsmOnFireBool.Value)
		{
			this.setup.pmCombat.SendEvent("goToOnFire");
			yield break;
		}
		if (this.fsmOnStructureBool.Value && this.setup.ai.mainPlayerHeight < -5f)
		{
			this.setup.pmCombat.SendEvent("goToRunAway");
			yield break;
		}
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		if (this.setup.ai.targetDist < 13f && this.setup.ai.targetDist > 8f)
		{
			this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 4));
			this.animator.SetBool("attackJumpBOOL", true);
		}
		else
		{
			if (this.setup.ai.targetDist >= 8f)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 3));
			this.animator.SetBool("attackBOOL", true);
		}
		bool timerCheck = false;
		timer = Time.time + 0.9f;
		while (!timerCheck)
		{
			if (Time.time > timer)
			{
				timerCheck = true;
			}
			if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.attackTag)
			{
				timerCheck = true;
			}
			yield return null;
		}
		this.animator.SetBool("attackBOOL", false);
		this.animator.SetBool("attackJumpBOOL", false);
		timer = Time.time + 0.5f;
		while (Time.time < timer)
		{
			if (this.setup.search.currentTarget)
			{
				this.doSmoothLookAt(this.setup.search.currentTarget.transform.position, 3f);
			}
			yield return null;
		}
		base.StartCoroutine(this.doPostAttackRoutine());
		yield break;
	}

	
	private IEnumerator doDeathRoutine()
	{
		this.search.StartCoroutine("toDisableVis");
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("stealthKillBool").Value)
		{
			this.animator.SetInteger("deathDirection", 1);
			this.setup.enemyEvents.disableAllWeapons();
			this.animator.SetBool("damageBOOL", false);
			this.animator.SetBool("stealthDeathBool", true);
			this.setup.targetFunctions.forceRemoveAttacker();
		}
		yield return YieldPresets.WaitForEndOfFrame;
		yield return YieldPresets.WaitForEndOfFrame;
		this.ai.resetCombatParams();
		this.rootTr.gameObject.layer = 14;
		this.fsmEnableControllerBool.Value = true;
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("deathFinal").Value)
		{
			base.StartCoroutine(this.deathFinalRoutine());
			yield break;
		}
		this.animator.SetBool("deathBOOL", true);
		this.animator.SetBool("recoverBool", true);
		this.setup.familyFunctions.InvokeRepeating("startRescueEvent", 1f, 1.2f);
		this.setup.familyFunctions.Invoke("cancelRescueEvent", 10f);
		yield return YieldPresets.WaitPointOneSeconds;
		base.StartCoroutine(this.deathOnGroundRoutine());
		yield break;
	}

	
	private IEnumerator deathOnGroundRoutine()
	{
		this.animator.SetBool("rescueBool1", false);
		this.animator.SetBool("recoverBool", true);
		this.animator.SetBool("damageBOOL", false);
		this.animator.SetInteger("deathDirection", 0);
		this.setup.enemyEvents.disableAllWeapons();
		this.setup.mutantStats.setTargetDown();
		this.search.StartCoroutine("toDisableVis");
		this.setup.pmBrain.SendEvent("toSetDying");
		this.setup.pmSearchScript.toDisableSearchEvent();
		this.toRescue = false;
		float timer = Time.time + 60f;
		while (this.animControl.currLayerState1.tagHash != this.animControl.idlehash)
		{
			if (this.fsmdeathFinalBool.Value)
			{
				yield break;
			}
			if (Time.time > timer)
			{
				base.StartCoroutine(this.deathFinalRoutine());
				yield break;
			}
			if (this.toRescue)
			{
				base.StartCoroutine(this.doDraggedRescueRoutine());
				yield break;
			}
			if (this.animControl.currLayerState1.tagHash == this.animControl.runhash)
			{
				break;
			}
			if (this.fsmDeathRecoverBool.Value)
			{
				break;
			}
			yield return null;
		}
		if (this.fsmdeathFinalBool.Value)
		{
			yield break;
		}
		this.setup.familyFunctions.cancelRescueEvent();
		this.setup.familyFunctions.cancelEatMeEvent();
		this.setup.familyFunctions.resetParent();
		this.setup.familyFunctions.resetFamilyParams();
		this.fsmEnableControllerBool.Value = true;
		this.animator.SetBool("actionBOOL1", false);
		this.fsmDeathBool.Value = true;
		this.setup.mutantStats.setTargetUp();
		this.setup.pmBrain.SendEvent("toSetAggressive");
		timer = Time.time + 5f;
		while (Time.time < timer)
		{
			if (this.animControl.currLayerState1.tagHash == this.animControl.idlehash || this.animControl.nextLayerState1.tagHash == this.animControl.idlehash)
			{
				break;
			}
			this.animator.SetBool("recoverBool", false);
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		this.animControl.enablePlayerCollision();
		this.search.StartCoroutine("toLook");
		this.animator.SetBool("deathBOOL", false);
		this.animator.SetBool("recoverBool", false);
		this.fsmDeathBool.Value = false;
		yield return YieldPresets.WaitPointFiveSeconds;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator deathFinalRoutine()
	{
		this.fsmDeathBool.Value = true;
		this.animator.SetBool("deathfinalBOOL", true);
		this.fsmOnFireBool.Value = false;
		this.setup.familyFunctions.cancelRescueEvent();
		this.setup.familyFunctions.cancelEatMeEvent();
		yield return YieldPresets.WaitTwoSeconds;
		this.animator.SetBool("stealthDeathBool", false);
		yield break;
	}

	
	private IEnumerator doDraggedRescueRoutine()
	{
		this.setup.familyFunctions.cancelRescueEvent();
		this.setup.familyFunctions.cancelEatMeEvent();
		this.animator.SetBool("rescueBool1", true);
		this.toDrop = false;
		this.cancelEvent = false;
		float timer = Time.time + 1f;
		while (!this.toDrop)
		{
			Vector3 matchPos = this.setup.pmCombat.FsmVariables.GetFsmGameObject("matchPosGo").Value.transform.position;
			Vector3 lookAtPos = this.setup.pmCombat.FsmVariables.GetFsmGameObject("lookatGo").Value.transform.position;
			lookAtPos += this.setup.pmCombat.FsmVariables.GetFsmGameObject("lookatGo").Value.transform.forward * 10f;
			if (Time.time < timer)
			{
				this.rootTr.position = Vector3.Lerp(this.rootTr.position, matchPos, Time.deltaTime);
				this.doSmoothLookAt(lookAtPos, 1f);
			}
			else
			{
				this.rootTr.position = matchPos;
				this.doSmoothLookAt(lookAtPos, 3f);
			}
			if (this.cancelEvent)
			{
				break;
			}
			yield return null;
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		this.animator.SetBool("rescueBool1", false);
		this.setup.health.addHealth();
		this.setup.familyFunctions.resetFamilyParams();
		yield return YieldPresets.WaitOneSecond;
		base.StartCoroutine(this.deathOnGroundRoutine());
		yield break;
	}

	
	private IEnumerator runToAttackRoutine()
	{
		this.setup.targetFunctions.sendAddAttacker();
		yield return null;
		this.ai.resetCombatParams();
		if (!this.fsmTargetSeenBool.Value)
		{
			this.setup.pmCombat.SendEvent("toTargetLost");
			yield break;
		}
		if (!this.setup.search.currentTarget.CompareTag("enemyRoot") && !Scene.SceneTracker.proxyAttackers.arrayList.Contains(this.setup.hashName))
		{
			this.setup.pmCombat.SendEvent("toKeepAway");
			yield break;
		}
		Dictionary<int, int> weights = new Dictionary<int, int>();
		weights.Add(0, Mathf.FloorToInt(this.setup.aiManager.fsmRunTowardsAttack.Value * 1000f));
		weights.Add(1, Mathf.FloorToInt(this.setup.aiManager.fsmScreamRunTowards.Value * 1000f));
		weights.Add(2, Mathf.FloorToInt(this.setup.aiManager.fsmRunTowardsFlank.Value * 1000f));
		weights.Add(3, Mathf.FloorToInt(this.setup.aiManager.fsmRunTowardsScream.Value * 1000f));
		float timer = Time.time;
		bool doingFlank = false;
		switch (weightsRandomizer.From<int>(weights).TakeOne())
		{
		case 1:
			this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 2));
			this.animator.SetBool("screamBOOL", true);
			timer = Time.time + 1f;
			while (Time.time < timer)
			{
				yield return null;
			}
			this.animator.SetBool("screamBOOL", false);
			break;
		case 2:
			doingFlank = true;
			break;
		}
		if (doingFlank)
		{
			int dir = 1;
			if (UnityEngine.Random.value > 0.5f)
			{
				dir = -1;
			}
			this.setup.search.StartCoroutine(this.setup.search.findPerpToEnemy(dir));
			this.setup.search.setToWaypoint();
			yield return null;
			this.setup.ai.StartCoroutine("toRun");
			yield return YieldPresets.WaitPointTwoSeconds;
			timer = Time.time + 8f;
			bool atTarget = false;
			while (!atTarget)
			{
				if (this.setup.ai.targetDist < 7f)
				{
					atTarget = true;
				}
				if (Time.time > timer)
				{
					atTarget = true;
				}
				yield return null;
			}
			this.setup.search.setToPlayer();
			this.setup.ai.StartCoroutine("toStop");
			yield return YieldPresets.WaitOneSecond;
		}
		if (this.fsmFearBool.Value)
		{
			this.setup.pmCombat.SendEvent("toFear");
			yield break;
		}
		this.setup.search.StartCoroutine(this.setup.search.findJumpAttackObj());
		yield return YieldPresets.WaitPointThreeSeconds;
		if (this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value)
		{
			float value = UnityEngine.Random.value;
			if (value < 0.25f)
			{
				this.setup.pmCombat.SendEvent("goToBurnStructure");
				yield break;
			}
			if (this.setup.ai.targetDist > 17f && this.setup.ai.targetDist < 60f)
			{
				base.StartCoroutine(this.throwFireBombRoutine());
				yield break;
			}
		}
		this.ai.resetCombatParams();
		this.setup.search.setToPlayer();
		this.setup.ai.StartCoroutine("toRun");
		this.runToAttack = true;
		while (this.setup.ai.targetDist > 30f)
		{
			if (this.fsmFearBool.Value || this.fsmPlayerIsRed.Value)
			{
				this.setup.pmCombat.SendEvent("toFear");
				yield break;
			}
			yield return null;
		}
		this.setup.targetFunctions.sendAddAttacker();
		yield return null;
		if (!Scene.SceneTracker.proxyAttackers.arrayList.Contains(this.setup.hashName))
		{
			this.setup.ai.StartCoroutine("toStop");
			this.setup.pmCombat.SendEvent("toKeepAway");
			yield break;
		}
		bool doneSideStep = false;
		timer = Time.time + 12f;
		while (this.setup.ai.targetDist > 13.5f)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toKeepAway");
				yield break;
			}
			if (this.fsmFearBool.Value || this.fsmPlayerIsRed.Value)
			{
				this.setup.pmCombat.SendEvent("toFear");
				yield break;
			}
			if (this.setup.ai.targetDist > 27f && this.setup.ai.targetDist < 24f && !doneSideStep)
			{
				if (UnityEngine.Random.value < 0.4f)
				{
					if (UnityEngine.Random.value > 0.5f)
					{
						this.animator.SetBool("goLeftBOOL", true);
					}
					else
					{
						this.animator.SetBool("goRightBOOL", true);
					}
					float timer2 = Time.time + 0.3f;
					while (Time.time < timer2)
					{
						yield return null;
					}
				}
				doneSideStep = true;
			}
			this.animator.SetBool("goLeftBOOL", false);
			this.animator.SetBool("goRightBOOL", false);
			yield return null;
		}
		this.runLongAttack = false;
		this.setup.targetFunctions.StartCoroutine(this.setup.targetFunctions.getTargetRunningAway());
		yield return YieldPresets.WaitPointTwoSeconds;
		if (this.runLongAttack)
		{
			yield return YieldPresets.WaitPointOneSeconds;
			this.animator.SetBool("attackJumpBOOL", true);
			timer = Time.time + 1.2f;
		}
		else
		{
			timer = Time.time + 2.5f;
			this.animator.SetBool("attackBOOL", true);
		}
		this.setup.ai.StartCoroutine("toStop");
		while (Time.time < timer)
		{
			if (this.setup.ai.target)
			{
				this.doSmoothLookAt(this.setup.ai.target.position, 1.7f);
			}
			yield return null;
		}
		this.animator.SetBool("attackBOOL", false);
		this.animator.SetBool("attackJumpBOOL", false);
		base.StartCoroutine(this.doPostAttackRoutine());
		yield break;
	}

	
	private IEnumerator doCloseAttackRoutine(bool skipAttackerCheck)
	{
		float timer = Time.time + 1f;
		if (!skipAttackerCheck)
		{
			while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
			{
				if (Time.time > timer)
				{
					this.setup.pmCombat.SendEvent("toReset");
					yield break;
				}
				yield return null;
			}
			this.setup.targetFunctions.sendAddAttacker();
			yield return null;
			if (!this.fsmTargetSeenBool.Value)
			{
				this.setup.pmCombat.SendEvent("toTargetLost");
				yield break;
			}
			if (!this.setup.search.currentTarget.CompareTag("enemyRoot") && !Scene.SceneTracker.proxyAttackers.arrayList.Contains(this.setup.hashName))
			{
				this.setup.pmCombat.SendEvent("toKeepAway");
				yield break;
			}
		}
		timer = Time.time + 1f;
		if (this.setup.ai.targetDist > 16f)
		{
			this.setup.pmCombat.SendEvent("toReset");
			this.fsmCloseCombatBool.Value = false;
			yield break;
		}
		if (this.setup.ai.targetDist > 12.5f)
		{
			this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 2));
			this.animator.SetBool("attackMovingBOOL", true);
		}
		else if (this.setup.ai.targetDist > 6.5f)
		{
			this.animator.SetBool("attackJumpBOOL", true);
		}
		else
		{
			timer = Time.time + 0.1f;
			if (this.setup.ai.targetAngle < -120f)
			{
				this.animator.SetBool("attackLeftBOOL", true);
				yield return YieldPresets.WaitPointFiveSeconds;
			}
			else if (this.setup.ai.targetAngle > 120f)
			{
				this.animator.SetBool("attackRightBOOL", true);
				yield return YieldPresets.WaitPointFiveSeconds;
			}
			else
			{
				this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 3));
				this.animator.SetBool("attackBOOL", true);
				timer = Time.time + 0.6f;
			}
		}
		while (Time.time < timer)
		{
			if (this.setup.ai.target)
			{
				this.doSmoothLookAt(this.setup.ai.target.position, 3f);
			}
			yield return null;
		}
		this.animator.SetBool("attackMovingBOOL", false);
		this.animator.SetBool("attackBOOL", false);
		this.animator.SetBool("attackJumpBOOL", false);
		this.animator.SetBool("attackLeftBOOL", false);
		this.animator.SetBool("attackRightBOOL", false);
		base.StartCoroutine(this.doPostAttackRoutine());
		yield break;
	}

	
	private IEnumerator throwFireBombRoutine()
	{
		this.setup.search.setToClosestPlayer();
		this.setup.ai.StartCoroutine("toStop");
		this.animator.SetInteger("randAttackInt1", 7);
		this.animator.SetBool("attackBOOL", true);
		this.setup.search.StartCoroutine("toTrack");
		float timer = Time.time + 3f;
		while (Time.time < timer)
		{
			if (this.setup.ai.target)
			{
				this.doSmoothLookAt(this.setup.ai.target.position, 1f);
			}
			yield return null;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator doPostAttackRoutine()
	{
		switch (weightsRandomizer.From<int>(new Dictionary<int, int>
		{
			{
				0,
				Mathf.FloorToInt(this.setup.aiManager.fsmClimbTreeAfterAttack.Value * 1000f)
			},
			{
				1,
				Mathf.FloorToInt(this.setup.aiManager.fsmRunAwayAfterAttack.Value * 1000f)
			},
			{
				2,
				Mathf.FloorToInt(this.setup.aiManager.fsmfollowUpAfterAttack.Value * 1000f)
			},
			{
				3,
				Mathf.FloorToInt(this.setup.aiManager.fsmRunHideAfterAttack.Value * 1000f)
			},
			{
				4,
				Mathf.FloorToInt(this.setup.aiManager.fsmToStalkAfterAttack.Value * 1000f)
			}
		}).TakeOne())
		{
		case 0:
			if (UnityEngine.Random.value > 0.5f)
			{
				this.setup.pmCombat.SendEvent("goToTree");
				yield break;
			}
			break;
		case 1:
			this.setup.pmCombat.SendEvent("goToRunAway");
			yield break;
		case 3:
			this.setup.pmCombat.SendEvent("toRunHide");
			yield break;
		case 4:
			this.setup.pmCombat.FsmVariables.GetFsmBool("switchStalk").Value = true;
			this.setup.pmCombat.SendEvent("goToRunAway");
			yield break;
		}
		bool counterAttack = false;
		if (this.setup.ai.targetDist < 5f)
		{
			this.animator.SetBool("attackBOOL", true);
			counterAttack = true;
		}
		else
		{
			this.animator.SetBool("backAwayBOOL", true);
		}
		float timer = Time.time + 1.5f;
		while (Time.time < timer)
		{
			if (counterAttack && this.setup.search.currentTarget)
			{
				this.doSmoothLookAt(this.setup.search.currentTarget.transform.position, 3f);
			}
			yield return null;
		}
		this.animator.SetBool("backAwayBOOL", false);
		this.animator.SetBool("attackBOOL", false);
		this.setup.pmCombat.SendEvent("toCloseCombat");
		yield break;
	}

	
	private IEnumerator doCloseCombatRoutine()
	{
		this.setup.targetFunctions.sendAddAttacker();
		yield return null;
		if (!this.fsmTargetSeenBool.Value)
		{
			this.fsmCloseCombatBool.Value = false;
			this.setup.pmCombat.SendEvent("toTargetLost");
			yield break;
		}
		if (!this.setup.search.currentTarget.CompareTag("enemyRoot") && !Scene.SceneTracker.proxyAttackers.arrayList.Contains(this.setup.hashName))
		{
			this.fsmCloseCombatBool.Value = false;
			this.setup.pmCombat.SendEvent("toKeepAway");
			yield break;
		}
		this.setup.ai.resetCombatParams();
		this.setup.ai.StartCoroutine("toStop");
		this.setup.search.setToPlayer();
		this.setup.search.StartCoroutine("toTrack");
		float timer = Time.time + 4f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				this.fsmCloseCombatBool.Value = false;
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.animControl.StartCoroutine(this.setup.animControl.smoothChangeIdle(1f));
		this.fsmCloseCombatBool.Value = true;
		if (this.setup.ai.targetDist > 30f)
		{
			this.fsmCloseCombatBool.Value = false;
			this.fsmCloseCombatBool.Value = false;
			this.setup.pmCombat.SendEvent("toReset");
		}
		else if (this.setup.ai.targetDist < 8f)
		{
			base.StartCoroutine(this.doCloseAttackRoutine(true));
			yield break;
		}
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("rescueBool").Value && UnityEngine.Random.value > 0.5f && this.setup.ai.mainPlayerDist < 50f && this.setup.familyFunctions.currentMemberTarget)
		{
			float num = Vector3.Distance(this.tr.position, this.setup.familyFunctions.currentMemberTarget.transform.position);
			if (num < 50f && num > 13f)
			{
				this.setup.search.StartCoroutine("toDisableVis");
				this.fsmCloseCombatBool.Value = false;
				this.setup.pmCombat.SendEvent("goToRescue1");
				yield break;
			}
		}
		switch (weightsRandomizer.From<int>(new Dictionary<int, int>
		{
			{
				0,
				Mathf.FloorToInt(this.setup.aiManager.fsmAttack.Value * 1000f)
			},
			{
				1,
				Mathf.FloorToInt(this.setup.aiManager.fsmStepLeft.Value * 1000f)
			},
			{
				2,
				Mathf.FloorToInt(this.setup.aiManager.fsmStepRight.Value * 1000f)
			},
			{
				3,
				Mathf.FloorToInt(this.setup.aiManager.fsmScream.Value * 1000f)
			},
			{
				4,
				Mathf.FloorToInt(this.setup.aiManager.fsmTreeAttack.Value * 1000f)
			},
			{
				5,
				Mathf.FloorToInt(this.setup.aiManager.fsmDisengage.Value * 1000f)
			},
			{
				6,
				Mathf.FloorToInt(this.setup.aiManager.fsmTreeClimb.Value * 1000f)
			},
			{
				7,
				Mathf.FloorToInt(this.setup.aiManager.fsmBackAway.Value * 1000f)
			},
			{
				8,
				Mathf.FloorToInt(this.setup.aiManager.fsmToStructure.Value * 1000f)
			}
		}).TakeOne())
		{
		case 0:
			timer = Time.time + 4f;
			while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
			{
				if (Time.time > timer)
				{
					break;
				}
				yield return null;
			}
			this.fsmCloseCombatBool.Value = false;
			base.StartCoroutine(this.doCloseAttackRoutine(true));
			yield break;
		case 1:
			base.StartCoroutine(this.doSideWalkRoutine());
			yield break;
		case 2:
			base.StartCoroutine(this.doSideWalkRoutine());
			yield break;
		case 3:
			this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 3));
			this.animator.SetBool("screamBOOL", true);
			timer = Time.time + 1.2f;
			while (Time.time < timer)
			{
				if (this.setup.ai.target)
				{
					this.doSmoothLookAt(this.setup.ai.target.position, this.rotateSpeed);
				}
				yield return null;
			}
			this.animator.SetBool("screamBOOL", false);
			break;
		case 4:
			this.setup.pmCombat.FsmVariables.GetFsmBool("lowTreeBool").Value = true;
			this.setup.pmCombat.SendEvent("goToTree");
			yield break;
		case 5:
			if (this.toAttractArtifact)
			{
				base.StartCoroutine(this.doSideWalkRoutine());
			}
			else
			{
				this.setup.pmCombat.SendEvent("toTimeout");
			}
			yield break;
		case 6:
			this.setup.pmCombat.SendEvent("goToTree");
			yield break;
		case 7:
			if (this.fsmLeaderBool.Value && UnityEngine.Random.value > 0.3f)
			{
				this.setup.pmCombat.FsmVariables.GetFsmBool("doLeaderCallFollower").Value = true;
			}
			if (UnityEngine.Random.value > 0.5f)
			{
				this.setup.pmCombat.SendEvent("goToRunAway");
			}
			else
			{
				this.setup.pmCombat.SendEvent("goToBackAway");
			}
			yield break;
		case 8:
			this.setup.pmCombat.SendEvent("goToStructure");
			yield break;
		}
		this.setup.ai.resetCombatParams();
		timer = Time.time + 4f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				break;
			}
			yield return null;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator doKeepAwayRoutine()
	{
		this.setup.ai.resetCombatParams();
		this.setup.ai.StartCoroutine("toStop");
		this.fsmCloseCombatBool.Value = false;
		float timer = Time.time;
		if (this.setup.ai.mainPlayerDist > 50f)
		{
			this.setup.search.setToPlayer();
			this.setup.ai.StartCoroutine("toRun");
			timer = Time.time + 7f;
			while (this.setup.ai.targetDist > 33f)
			{
				if (Time.time > timer)
				{
					break;
				}
				yield return null;
			}
			this.setup.ai.StartCoroutine("toStop");
			this.setup.search.StartCoroutine("toTrack");
			this.setup.search.setToPlayer();
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		if (this.setup.ai.mainPlayerDist < 15f)
		{
			this.setup.pmCombat.SendEvent("goToRunAway");
			yield break;
		}
		timer = Time.time + UnityEngine.Random.Range(0.5f, 3f);
		while (Time.time < timer)
		{
			yield return null;
			if (this.setup.ai.mainPlayerDist < 10f)
			{
				base.StartCoroutine(this.doCloseAttackRoutine(false));
				yield break;
			}
		}
		if (this.setup.pmBrain.FsmVariables.GetFsmBool("firemanBool").Value)
		{
			float value = UnityEngine.Random.value;
			if (value > 0.5f)
			{
				if (this.setup.ai.targetDist > 18f)
				{
					base.StartCoroutine(this.throwFireBombRoutine());
					yield break;
				}
			}
			else if (value > 0.1f)
			{
				this.setup.pmCombat.SendEvent("goToBurnStructure");
				yield break;
			}
		}
		switch (weightsRandomizer.From<int>(new Dictionary<int, int>
		{
			{
				0,
				Mathf.FloorToInt(this.setup.aiManager.fsmAwayReturn.Value * 1000f)
			},
			{
				1,
				Mathf.FloorToInt(this.setup.aiManager.fsmAwayScream.Value * 1000f)
			},
			{
				2,
				Mathf.FloorToInt(this.setup.aiManager.fsmAwayFlank.Value * 1000f)
			},
			{
				3,
				Mathf.FloorToInt(this.setup.aiManager.fsmAwayToRock.Value * 1000f)
			},
			{
				4,
				Mathf.FloorToInt(this.setup.aiManager.fsmToStructure.Value * 1000f)
			}
		}).TakeOne())
		{
		case 0:
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		case 1:
			this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 3));
			this.animator.SetBool("screamBOOL", true);
			timer = Time.time + 1.2f;
			while (Time.time < timer)
			{
				if (this.setup.ai.target)
				{
					this.doSmoothLookAt(this.setup.ai.target.position, this.rotateSpeed);
				}
				yield return null;
			}
			this.animator.SetBool("screamBOOL", false);
			break;
		case 2:
			this.setup.pmCombat.SendEvent("goToFlank");
			yield break;
		case 3:
			this.setup.pmCombat.SendEvent("goToRock");
			yield break;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator doRunHideRoutine()
	{
		this.setup.ai.resetCombatParams();
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.findPointAwayFromPlayer(25f));
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.doAction = false;
		yield return YieldPresets.WaitPointOneSeconds;
		this.setup.ai.StartCoroutine("toRun");
		timer = Time.time + 10f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.search.setToClosestPlayer();
		this.animator.SetBool("crouchBOOL", true);
		this.setup.ai.StartCoroutine("toStop");
		timer = Time.time + 1f;
		yield return YieldPresets.WaitOneSecond;
		this.animator.SetBool("stalkingBOOL", true);
		this.animator.SetBool("screamBOOL", false);
		this.setup.ai.StartCoroutine("toStop");
		timer = Time.time + (float)UnityEngine.Random.Range(16, 30);
		while (Time.time < timer)
		{
			if (this.setup.ai.targetDist < 19f)
			{
				if (UnityEngine.Random.value < 0.75f)
				{
					this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 4));
					this.animator.SetBool("attackJumpBOOL", true);
					timer = Time.time + 1.3f;
					while (Time.time < timer)
					{
						if (this.setup.ai.target)
						{
							this.doSmoothLookAt(this.setup.ai.target.position, 5f);
						}
						yield return null;
					}
					this.animator.SetBool("attackJumpBOOL", false);
					this.setup.pmCombat.SendEvent("toReset");
					yield break;
				}
				this.setup.pmCombat.SendEvent("goToRunAway");
				yield break;
			}
			else
			{
				if (this.setup.ai.targetDist > 60f)
				{
					this.setup.ai.StartCoroutine("toRun");
					float timer2 = Time.time + 7f;
					while (Time.time < timer2)
					{
						if (Time.time > timer2)
						{
							break;
						}
						if (this.setup.ai.targetDist < 30f)
						{
							timer = Time.time + (float)UnityEngine.Random.Range(16, 30);
							break;
						}
						yield return null;
					}
				}
				yield return null;
			}
		}
		this.setup.pmCombat.SendEvent("goToFlank");
		yield break;
	}

	
	private IEnumerator doStalkRoutine()
	{
		this.setup.search.StartCoroutine("toTrack");
		this.setup.search.StartCoroutine(this.setup.search.setVisRetry(12));
		this.setup.StartCoroutine(this.setup.disableNonActiveFSM(this.setup.pmCombat.FsmName));
		this.animator.SetBool("stalkingBOOL", true);
		yield return YieldPresets.WaitPointOneSeconds;
		if (this.fsmLeaderBool.Value)
		{
			this.setup.followerFunctions.sendStalkEvent();
		}
		this.setup.enemyEvents.playStalkPlayer();
		this.setup.pmBrain.SendEvent("toSetPassive");
		base.StartCoroutine(this.smoothAnimatorFloat("blendFloat1", 0f, 1f, 1f));
		this.setup.ai.resetCombatParams();
		this.setup.ai.StartCoroutine("toStop");
		float timer = Time.time + 4f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.animControl.StartCoroutine(this.setup.animControl.smoothChangeIdle(2f));
		if (UnityEngine.Random.value < 0.2f && this.setup.search.findCloseTrapTrigger())
		{
			this.setup.pmCombat.SendEvent("goToTrapTrigger");
			yield break;
		}
		timer = Time.time + UnityEngine.Random.Range(3f, 9f);
		while (Time.time < timer)
		{
			if (this.setup.ai.targetDist < 8f)
			{
				this.setup.pmCombat.SendEvent("goToAttack");
				yield break;
			}
			if (this.setup.ai.targetDist < this.setup.aiManager.fsmRunAwayDist.Value)
			{
				switch (weightsRandomizer.From<int>(new Dictionary<int, int>
				{
					{
						0,
						Mathf.FloorToInt(this.setup.aiManager.fsmRunAwayChance.Value * 1000f)
					},
					{
						1,
						Mathf.FloorToInt(this.setup.aiManager.fsmAttackChance.Value * 1000f)
					},
					{
						2,
						Mathf.FloorToInt(this.setup.aiManager.fsmDisengage.Value * 1000f)
					},
					{
						3,
						Mathf.FloorToInt(this.setup.aiManager.fsmLeaderCallFollowers.Value * 1000f)
					}
				}).TakeOne())
				{
				case 0:
					if (UnityEngine.Random.value > 0.5f)
					{
						this.setup.pmCombat.SendEvent("goToBackAway");
					}
					else
					{
						this.setup.pmCombat.SendEvent("goToRunAway");
					}
					yield break;
				case 1:
					this.setup.pmCombat.SendEvent("goToAttack");
					yield break;
				case 2:
					if (this.toAttractArtifact)
					{
						this.setup.pmCombat.SendEvent("goToAttack");
					}
					else
					{
						this.setup.pmCombat.SendEvent("toTimeout");
					}
					yield break;
				case 3:
					if (this.fsmLeaderBool.Value)
					{
						this.setup.pmCombat.SendEvent("goToCallFollower");
					}
					else
					{
						this.setup.pmCombat.SendEvent("goToRunAway");
					}
					yield break;
				}
			}
			else if (this.setup.ai.targetDist > this.setup.aiManager.fsmRunTowardPlayerDist.Value)
			{
				int num = weightsRandomizer.From<int>(new Dictionary<int, int>
				{
					{
						0,
						Mathf.FloorToInt(this.setup.aiManager.fsmRunForward.Value * 1000f)
					},
					{
						1,
						Mathf.FloorToInt(this.setup.aiManager.fsmRunForwardToTree.Value * 1000f)
					},
					{
						2,
						Mathf.FloorToInt(this.setup.aiManager.fsmSneakForward.Value * 1000f)
					}
				}).TakeOne();
				if (num == 0)
				{
					this.setup.pmCombat.SendEvent("goToRunForward");
					yield break;
				}
				if (num == 1)
				{
					this.setup.pmCombat.SendEvent("goToTree");
					yield break;
				}
				if (num == 2)
				{
					this.setup.pmCombat.SendEvent("toCreepForward");
					yield break;
				}
			}
			yield return null;
		}
		switch (weightsRandomizer.From<int>(new Dictionary<int, int>
		{
			{
				0,
				Mathf.FloorToInt(this.setup.aiManager.fsmStalkToTree.Value * 1000f)
			},
			{
				1,
				Mathf.FloorToInt(this.setup.aiManager.fsmStalkToFlank.Value * 1000f)
			},
			{
				2,
				Mathf.FloorToInt(this.setup.aiManager.fsmStalkRunTowards.Value * 1000f)
			},
			{
				3,
				Mathf.FloorToInt(this.setup.aiManager.fsmStalkLeaveArea.Value * 1000f)
			},
			{
				4,
				Mathf.FloorToInt(this.setup.aiManager.fsmStalkToSneakForward.Value * 1000f)
			},
			{
				5,
				Mathf.FloorToInt(this.setup.aiManager.fsmStalkToRock.Value * 1000f)
			}
		}).TakeOne())
		{
		case 0:
			this.setup.pmCombat.SendEvent("goToTree");
			yield break;
		case 1:
			this.setup.pmCombat.SendEvent("goToFlank");
			yield break;
		case 2:
			this.setup.pmCombat.SendEvent("goToRunForward");
			yield break;
		case 3:
			if (this.toAttractArtifact)
			{
				this.setup.pmCombat.SendEvent("goToAttack");
			}
			else
			{
				this.setup.pmCombat.SendEvent("toTimeout");
			}
			yield break;
		case 4:
			this.setup.pmCombat.SendEvent("toCreepForward");
			yield break;
		case 5:
			this.setup.pmCombat.SendEvent("goToRock");
			yield break;
		default:
			yield break;
		}
	}

	
	private IEnumerator doFearRoutine()
	{
		this.setup.ai.resetCombatParams();
		this.animator.SetBool("fearBOOL", true);
		this.setup.familyFunctions.resetFamilyParams();
		this.fsmCloseCombatBool.Value = false;
		this.setup.ai.StartCoroutine("toStop");
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (this.setup.animControl.nextLayerState1.tagHash == this.setup.hashs.idleTag)
			{
				break;
			}
			if (this.setup.ai.target)
			{
				this.doSmoothLookAt(this.setup.ai.target.position, 1f);
			}
			yield return null;
		}
		if (this.toRepelArtifact)
		{
			this.setup.pmCombat.SendEvent("toTimeout");
			yield break;
		}
		if (this.setup.ai.mainPlayerDist < 20f)
		{
			this.setup.pmCombat.SendEvent("goToBackAway");
			yield return YieldPresets.WaitOneSecond;
			yield break;
		}
		float timer = Time.time + UnityEngine.Random.Range(1f, 4f);
		while (Time.time < timer)
		{
			if (this.setup.ai.targetDist < 5f && UnityEngine.Random.value > 0.25f)
			{
				this.setup.pmCombat.SendEvent("goToAttack");
				yield break;
			}
			if (this.setup.ai.targetDist < 12f)
			{
				this.setup.pmCombat.SendEvent("goToRunAway");
				yield break;
			}
			yield return null;
		}
		Dictionary<int, int> weights2 = new Dictionary<int, int>();
		int prayWeight = 1000;
		if (this.fsmPlayerIsRed.Value)
		{
			prayWeight = 5000;
		}
		weights2.Add(0, 1000);
		weights2.Add(1, 1000);
		weights2.Add(2, Mathf.FloorToInt(this.setup.aiManager.fsmDisengage.Value * 1000f));
		weights2.Add(3, prayWeight);
		switch (weightsRandomizer.From<int>(weights2).TakeOne())
		{
		case 0:
			this.setup.pmCombat.SendEvent("goToFlank");
			yield break;
		case 1:
			this.setup.pmCombat.SendEvent("goToRunAway");
			yield break;
		case 2:
			if (UnityEngine.Random.value > 0.7f && !this.toAttractArtifact)
			{
				this.setup.pmCombat.SendEvent("toTimeout");
			}
			else
			{
				this.setup.pmCombat.SendEvent("goToRunAway");
			}
			yield break;
		case 3:
			if (this.fsmPlayerIsRed.Value)
			{
				this.setup.pmCombat.SendEvent("goToPrayToPlayer");
			}
			else
			{
				this.setup.pmCombat.SendEvent("goToFlank");
			}
			yield break;
		default:
			yield break;
		}
	}

	
	private IEnumerator doPrayToPlayerRoutine()
	{
		this.setup.familyFunctions.resetFamilyParams();
		this.fsmCloseCombatBool.Value = false;
		this.setup.ai.StartCoroutine("toStop");
		this.search.setToClosestPlayer();
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 2));
		this.animator.SetBool("ritualBOOL", true);
		float timer = Time.time + 20f;
		float timer2 = Time.time + 1f;
		while (this.setup.ai.targetDist < 30f)
		{
			if (this.setup.ai.mainPlayerDist > 40f)
			{
				this.setup.ai.StartCoroutine("toStop");
				this.animator.SetBool("ritualBOOL", false);
				this.setup.pmCombat.SendEvent("toReset");
				this.setup.pmSearchScript.toResetSearchEvent();
				Debug.Log("resetting red player");
				yield break;
			}
			if (this.setup.ai.mainPlayerDist < 7f || this.fsmFearOverrideBool.Value)
			{
				this.animator.SetBool("screamBOOL", true);
				this.animator.SetBool("ritualBOOL", false);
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			timer2 = Time.time + 1f;
			yield return null;
		}
		this.animator.SetBool("ritualBOOL", false);
		this.setup.pmCombat.SendEvent("toReset");
		this.setup.pmSearchScript.toResetSearchEvent();
		yield return null;
		yield break;
	}

	
	private IEnumerator doOnRockRoutine()
	{
		this.toOnRock = false;
		this.setup.search.StartCoroutine(this.setup.search.findCloseJumpObj());
		float timer = Time.time + 1f;
		while (!this.toOnRock)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.search.setToWaypoint();
		this.setup.ai.StartCoroutine("toRun");
		this.animator.SetBool("jumpBlockBool", true);
		timer = Time.time + 6f;
		while ((double)this.setup.ai.targetDist > 9.5)
		{
			if (Time.time > timer)
			{
				this.setup.ai.StartCoroutine("toStop");
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.animator.SetBool("onRockBOOL", true);
		Vector3 rockPos = this.setup.pmCombat.FsmVariables.GetFsmVector3("closeRockPOS").Value;
		timer = Time.time + 2f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.onRockTag)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			this.doSmoothLookAt(rockPos, 4f);
			yield return null;
		}
		MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(1f, 1f, 1f), 0f);
		timer = Time.time + 2f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (!this.animator.IsInTransition(0))
			{
				this.animator.MatchTarget(rockPos, Quaternion.identity, AvatarTarget.LeftFoot, _weightMask, 0.3f, 0.8f);
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toStop");
		this.setup.search.setToClosestPlayer();
		this.animator.SetBool("screamBOOL", false);
		bool onRockIdle = true;
		bool doAttack = false;
		timer = Time.time + (float)UnityEngine.Random.Range(3, 9);
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointFiveSeconds;
		while (onRockIdle)
		{
			if (this.setup.ai.targetDist < 15f)
			{
				if (UnityEngine.Random.value > 0.75f)
				{
					onRockIdle = false;
				}
				else
				{
					doAttack = true;
					onRockIdle = false;
				}
			}
			if (Time.time > timer)
			{
				float rand = UnityEngine.Random.value;
				if (rand > 0.7f)
				{
					this.animator.SetInteger("randInt1", 1);
					this.animator.SetBool("screamBOOL", true);
					yield return YieldPresets.WaitOneSecond;
					timer = Time.time + UnityEngine.Random.Range(3f, 8f);
					this.animator.SetBool("screamBOOL", false);
				}
				else if (rand > 0.4f)
				{
					onRockIdle = false;
				}
			}
			yield return null;
		}
		if (doAttack)
		{
			timer = Time.time + 0.8f;
			this.animator.SetBool("attackBOOL", true);
			while (Time.time < timer)
			{
				if (this.setup.ai.target)
				{
					this.doSmoothLookAt(this.setup.ai.target.position, 3f);
				}
				yield return null;
			}
			this.animator.SetBool("onRockBOOL", false);
		}
		this.setup.search.resetCurrentJumpObj();
		this.animator.SetBool("onRockBOOL", false);
		this.animator.SetBool("jumpBlockBool", false);
		this.setup.enemyEvents.enableCollision();
		this.setup.pmBrain.FsmVariables.GetFsmBool("enableControllerBool").Value = true;
		yield return YieldPresets.WaitPointFiveSeconds;
		this.setup.pmCombat.SendEvent("goToRunAway");
		yield break;
	}

	
	private IEnumerator doOnTreeRoutine()
	{
		this.setup.search.StartCoroutine(this.setup.search.findCloseTree(60f));
		this.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitPointOneSeconds;
		if (this.fsmTreeGo.Value == null)
		{
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.ai.resetCombatParams();
		this.ai.cancelDefaultActions();
		this.search.setToWaypoint();
		this.search.StartCoroutine("toDisableVis");
		float timer = Time.time + 3f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		if (this.fsmLeaderBool.Value)
		{
			this.setup.followerFunctions.sendTreeStalkEvent();
		}
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 9f;
		if (this.fsmTreeGo.Value)
		{
			Collider component = this.fsmTreeGo.Value.GetComponent<Collider>();
			if (this.fsmTreeGo.Value.activeSelf && component && component.enabled && this.rootTr.gameObject.activeSelf && this.setup.controller.enabled)
			{
				Physics.IgnoreCollision(component, this.setup.controller, true);
			}
		}
		while (Vector3.Distance(this.fsmTreeGo.Value.transform.position, this.tr.position) > 12f)
		{
			if (Time.time > timer || this.fsmTreeGo.Value == null)
			{
				if (!this.ai.startedRun)
				{
					this.ai.StartCoroutine("toRun");
				}
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		Vector3 treePos = this.setup.pmCombat.FsmVariables.GetFsmVector3("treePos").Value;
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("lowTreeBool").Value)
		{
		}
		Vector3 attachPos = this.search.findTreeAttachPos(treePos, 1.7f);
		this.animator.SetBool("treeBOOL", true);
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.onRockTag)
		{
			if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.idleTag)
			{
				break;
			}
			this.doSmoothLookAt(treePos, 5f);
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 0f);
		timer = Time.time + 3f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (this.setup.animControl.nextLayerState1.tagHash == this.setup.hashs.idleTag)
			{
				break;
			}
			if (!this.animator.IsInTransition(0))
			{
				this.animator.MatchTarget(attachPos, Quaternion.identity, AvatarTarget.Body, _weightMask, 0.2f, 0.55f);
			}
			this.doSmoothLookAt(treePos, 5f);
			yield return null;
		}
		this.setup.mutantStats.setTargetDown();
		timer = Time.time + UnityEngine.Random.Range(1f, 3f);
		this.animator.SetBool("actionBOOL1", true);
		while (Time.time < timer)
		{
			this.doSmoothLookAt(treePos, 3f);
			yield return null;
		}
		this.animator.SetBool("actionBOOL1", false);
		this.ai.StartCoroutine("toStop");
		bool stalkInTree = false;
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("stalking").Value)
		{
			stalkInTree = true;
		}
		float baseTimer = Time.time + UnityEngine.Random.Range(2f, 5f);
		bool inTree = true;
		this.fsmNextTreeGo.Value = null;
		int jumpCount = 0;
		while (inTree)
		{
			this.fsmNextTreeGo.Value = null;
			this.animator.SetBool("actionBOOL1", false);
			if (this.fsmJumpDownBool.Value)
			{
				base.StartCoroutine(this.doTreeJumpDownRoutine());
				yield break;
			}
			if (Time.time > baseTimer)
			{
				if (UnityEngine.Random.value < 0.2f || jumpCount > 8 || this.ai.mainPlayerDist > 120f)
				{
					base.StartCoroutine(this.doTreeJumpDownRoutine());
					yield break;
				}
				if (stalkInTree)
				{
					if (this.setup.ai.mainPlayerDist <= 45f)
					{
						base.StartCoroutine(this.doTreeJumpDownRoutine());
						yield break;
					}
					this.search.StartCoroutine(this.search.findJumpTree(true));
					yield return YieldPresets.WaitPointFiveSeconds;
				}
				else if (this.setup.ai.mainPlayerDist < 30f)
				{
					if (UnityEngine.Random.value > 0.5f && (this.ai.mainPlayerAngle < -135f || this.ai.mainPlayerAngle > 135f || (this.ai.mainPlayerAngle > -45f && this.ai.mainPlayerAngle < 45f)))
					{
						base.StartCoroutine(this.goToTreeAttackRoutine());
						yield break;
					}
					this.search.StartCoroutine(this.search.findJumpTree(false));
					yield return YieldPresets.WaitPointFiveSeconds;
				}
				else
				{
					this.search.StartCoroutine(this.search.findJumpTree(true));
					yield return YieldPresets.WaitPointFiveSeconds;
				}
				if (this.fsmNextTreeGo.Value != null)
				{
					jumpCount++;
					Vector3 invertPos = this.tr.InverseTransformPoint(this.setup.pmCombat.FsmVariables.GetFsmVector3("nextTreePos").Value);
					invertPos *= -1f;
					invertPos = this.tr.TransformPoint(invertPos);
					bool rotateToward = true;
					while (rotateToward)
					{
						Vector3 localTarget = this.tr.InverseTransformPoint(invertPos);
						float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * 57.29578f;
						if (targetAngle > 45f)
						{
							this.animator.SetBool("turnLeftBOOL", true);
							this.animator.SetBool("turnRightBOOL", false);
						}
						else if (targetAngle < -45f)
						{
							this.animator.SetBool("turnLeftBOOL", false);
							this.animator.SetBool("turnRightBOOL", true);
						}
						else
						{
							rotateToward = false;
						}
						yield return null;
					}
					this.animator.SetBool("turnLeftBOOL", false);
					this.animator.SetBool("turnRightBOOL", false);
					while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
					{
						yield return null;
					}
					timer = Time.time + 1f;
					while (Time.time < timer)
					{
						this.doSmoothLookAt(invertPos, 3f);
						yield return null;
					}
					timer = 0f;
					while (timer < 1f)
					{
						timer += Time.deltaTime;
						if (timer > 0.4f)
						{
							this.animator.SetBool("treeJumpBOOL", true);
						}
						yield return null;
					}
					while (this.setup.animControl.nextLayerState1.tagHash != this.setup.hashs.onRockTag)
					{
						if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.onRockTag)
						{
							break;
						}
						yield return null;
					}
					timer = Time.time + 4f;
					while (Time.time < timer)
					{
						this.doSmoothLookAt(this.fsmNextTreeGo.Value.transform.position, 3f);
						Vector3 pos = this.fsmNextTreeGo.Value.transform.position;
						if (this.animControl.currLayerState1.tagHash == this.animControl.landingHash)
						{
							break;
						}
						pos.y = this.tr.position.y;
						if ((double)Vector3.Distance(pos, this.tr.position) < 3.7)
						{
							break;
						}
						yield return null;
					}
					Vector3 landPos = this.search.findTreeAttachPos(this.setup.pmCombat.FsmVariables.GetFsmVector3("nextTreePos").Value, 1.7f);
					this.animator.SetBool("treeJumpBOOL", false);
					while (this.setup.animControl.nextLayerState1.tagHash != this.setup.hashs.idleTag)
					{
						if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.idleTag)
						{
							break;
						}
						if (this.fsmNextTreeGo.Value == null)
						{
							base.StartCoroutine(this.doTreeJumpDownRoutine());
							yield break;
						}
						this.doSmoothLookAt(this.fsmNextTreeGo.Value.transform.position, 4f);
						if (!this.animator.IsInTransition(0))
						{
							this.animator.MatchTarget(landPos, Quaternion.identity, AvatarTarget.Body, _weightMask, 0f, 0.3f);
						}
						yield return null;
					}
					float tHeight = Terrain.activeTerrain.SampleHeight(this.tr.position) + Terrain.activeTerrain.transform.position.y;
					timer = Time.time + UnityEngine.Random.Range(1f, 3f);
					while (this.tr.position.y - tHeight < 16f)
					{
						this.animator.SetBool("actionBOOL1", true);
						if (Time.time > timer)
						{
							break;
						}
						yield return null;
					}
					baseTimer = (baseTimer = Time.time + UnityEngine.Random.Range(2f, 5f));
				}
				else
				{
					baseTimer = Time.time + UnityEngine.Random.Range(2f, 5f);
				}
			}
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator doTreeJumpDownRoutine()
	{
		this.animator.SetBool("actionBOOL1", false);
		this.animator.SetBool("treeJumpBOOL", false);
		this.animator.SetBool("turnLeftBOOL", false);
		this.animator.SetBool("turnRightBOOL", false);
		float timer = Time.time + 2f;
		while (this.setup.animControl.currLayerState1.tagHash != this.setup.hashs.idleTag)
		{
			if (Time.time > timer)
			{
				break;
			}
			yield return null;
		}
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 2));
		this.animator.SetBool("treeBOOL", false);
		timer = Time.time + 4f;
		while (this.animControl.currLayerState1.tagHash != this.animControl.landingHash)
		{
			if (Time.time > timer)
			{
				this.animControl.forceJumpLand();
				break;
			}
			this.search.StartCoroutine("toTrack");
			yield return YieldPresets.WaitOneSecond;
			this.rootTr.gameObject.layer = 14;
			this.setup.mutantStats.setTargetUp();
			this.fsmJumpDownBool.Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmBool("inTreeBool").Value = false;
			this.fsmTreeGo.Value = null;
			if (this.fsmStalking.Value)
			{
				this.fsmIsStalkingBool.Value = true;
				this.setup.pmCombat.SendEvent("goToStalking");
			}
			else
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.setup.pmCombat.SendEvent("toReset");
			}
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator doLowTreeAttackRoutine()
	{
		Vector3 treePos = this.setup.pmCombat.FsmVariables.GetFsmVector3("treePos").Value;
		Vector3 attachPos = this.search.findTreeAttachPos(treePos, 1f);
		this.animator.SetBool("treeJumpBOOL", true);
		float timer = Time.time + 3f;
		while (this.animControl.currLayerState1.tagHash != this.animControl.landingHash)
		{
			if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.idleTag)
			{
				this.resetTreeParams();
				this.search.StartCoroutine(this.search.toLook());
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			this.doSmoothLookAt(treePos, 4f);
			if (Time.time > timer)
			{
				break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(1f, 1f, 1f), 0f);
		timer = Time.time + 4f;
		while (Time.time < timer)
		{
			if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.idleTag)
			{
				this.resetTreeParams();
				this.search.StartCoroutine(this.search.toLook());
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (this.animControl.currLayerState1.tagHash == this.animControl.inTreehash || this.animControl.currLayerState1.tagHash == this.animControl.inTreeMirhash || this.animControl.nextLayerState1.tagHash == this.animControl.inTreehash || this.animControl.nextLayerState1.tagHash == this.animControl.inTreeMirhash)
			{
				break;
			}
			if (!this.animator.IsInTransition(0))
			{
				this.animator.MatchTarget(attachPos, Quaternion.identity, AvatarTarget.LeftFoot, _weightMask, 0.22f, 0.5f);
			}
			yield return null;
		}
		this.search.setToClosestPlayer();
		timer = Time.time + UnityEngine.Random.Range(6f, 15f);
		bool doAttack = false;
		while (Time.time < timer)
		{
			this.doSmoothLookAt(treePos, 2f);
			if (this.ai.targetDist < 10f)
			{
				doAttack = true;
				break;
			}
			if (this.ai.targetDist > 45f || this.fsmTreeGo.Value == null)
			{
				break;
			}
			if (this.setup.animControl.currLayerState1.tagHash == this.setup.hashs.idleTag)
			{
				this.resetTreeParams();
				this.search.StartCoroutine(this.search.toLook());
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		if (doAttack && this.ai.mainPlayerAngle > 160f && this.ai.mainPlayerAngle < 180f)
		{
			this.animator.SetBool("attackJumpBOOL", true);
			this.search.setToClosestPlayer();
			yield return YieldPresets.WaitOnePointThreeSeconds;
			this.animator.SetBool("attackJumpBOOL", false);
			this.setup.pmCombat.FsmVariables.GetFsmBool("lowTreeBool").Value = false;
		}
		this.resetTreeParams();
		yield return YieldPresets.WaitOneSecond;
		if (this.fsmStalking.Value)
		{
			this.fsmIsStalkingBool.Value = true;
			this.setup.pmCombat.SendEvent("goToStalking");
		}
		else
		{
			this.setup.pmBrain.SendEvent("toSetAggressive");
			this.setup.pmCombat.SendEvent("toReset");
		}
		yield return null;
		yield break;
	}

	
	private void resetTreeParams()
	{
		this.animator.SetBool("treeJumpBOOL", false);
		this.rootTr.gameObject.layer = 14;
		this.setup.mutantStats.setTargetUp();
		this.fsmJumpDownBool.Value = false;
		this.setup.pmCombat.FsmVariables.GetFsmBool("inTreeBool").Value = false;
		this.fsmTreeGo.Value = null;
	}

	
	private IEnumerator doSideWalkRoutine()
	{
		float timer = Time.time;
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 2));
		this.animator.SetBool("sideWalkBOOL", true);
		timer = Time.time + UnityEngine.Random.Range(1f, 2f);
		while (Time.time < timer)
		{
			if (this.setup.ai.target)
			{
				this.doSmoothLookAt(this.setup.ai.target.position, this.rotateSpeed);
			}
			if (this.setup.ai.targetDist < 9f)
			{
				base.StartCoroutine(this.doCloseAttackRoutine(true));
				yield break;
			}
			yield return null;
		}
		timer = Time.time + UnityEngine.Random.Range(2f, 4f);
		if (UnityEngine.Random.value > 0.65f)
		{
			while (Time.time < timer)
			{
				if (this.setup.ai.target)
				{
					this.doSmoothLookAt(this.setup.ai.target.position, this.rotateSpeed);
				}
				if (this.setup.ai.targetDist < 10f)
				{
					base.StartCoroutine(this.doCloseAttackRoutine(true));
					yield break;
				}
				yield return null;
			}
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToRandomPointRoutine()
	{
		this.setup.ai.resetCombatParams();
		this.setup.ai.StartCoroutine("toStop");
		yield return null;
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.castPointAroundPlayer(30f));
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		bool crouch = false;
		if ((double)UnityEngine.Random.value > 0.5)
		{
			crouch = true;
		}
		timer = Time.time + 10f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.animator.SetBool("crouchBOOL", crouch);
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitOneSecond;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator doFlankRoutine()
	{
		this.resetDefaultParams();
		this.setup.search.StartCoroutine(this.setup.search.findFlankPointToPlayer(30f));
		yield return null;
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		bool crouch = false;
		if ((double)UnityEngine.Random.value > 0.5)
		{
			crouch = true;
		}
		timer = Time.time + 8f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.animator.SetBool("crouchBOOL", crouch);
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitOneSecond;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator doRunAwayRoutine()
	{
		this.resetDefaultParams();
		this.setup.search.StartCoroutine(this.setup.search.findPointAwayFromPlayer(20f));
		yield return null;
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		timer = Time.time + 8f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		bool crouch = false;
		if ((double)UnityEngine.Random.value > 0.5)
		{
			crouch = true;
		}
		this.animator.SetBool("crouchBOOL", crouch);
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("doLeaderCallFollower").Value)
		{
			this.setup.pmCombat.SendEvent("goToCallFollower");
			yield break;
		}
		if (this.setup.pmCombat.FsmVariables.GetFsmBool("switchStalk").Value)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("switchStalk").Value = false;
			this.setup.pmBrain.SendEvent("toSetPassive");
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		yield return YieldPresets.WaitOneSecond;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator doTimeoutRoutine()
	{
		this.ai.resetCombatParams();
		this.setup.pmCombat.FsmVariables.GetFsmBool("timeOutBool").Value = false;
		this.toRepelArtifact = false;
		if (this.fsmLeaderBool.Value)
		{
			this.setup.familyFunctions.sendAllFleeArea();
		}
		if (this.fsmInsideBase.Value)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("timeOutBool").Value = false;
			this.search.StartCoroutine(this.search.toLook());
			this.ai.StartCoroutine("toStop");
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.findPointAwayFromPlayer(250f));
		yield return null;
		float timer = Time.time + 1f;
		bool tryRandom = false;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				tryRandom = true;
				break;
			}
			yield return null;
		}
		if (tryRandom)
		{
			this.doAction = false;
			this.setup.search.StartCoroutine(this.setup.search.findRandomPoint(250f));
			timer = Time.time + 1f;
			while (!this.doAction)
			{
				if (Time.time > timer)
				{
					this.setup.pmCombat.SendEvent("goToRandomPoint");
					yield break;
				}
				yield return null;
			}
		}
		if (this.doAction)
		{
			this.search.setToWaypoint();
		}
		yield return null;
		this.search.StartCoroutine("toDisableVis");
		this.setup.ai.StartCoroutine("toRun");
		this.setup.followerFunctions.sendSearchEvent();
		yield return null;
		timer = Time.time + 20f;
		while (this.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.search.StartCoroutine("toLook");
				this.setup.followerFunctions.sendSearchEvent();
				this.setup.pmBrain.SendEvent("toSetSearching");
				this.setup.pmCombat.SendEvent("toDisableFSM");
				yield break;
			}
			yield return null;
		}
		bool crouch = false;
		if ((double)UnityEngine.Random.value > 0.5)
		{
			crouch = true;
		}
		this.animator.SetBool("crouchBOOL", crouch);
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		this.search.StartCoroutine("toLook");
		this.setup.followerFunctions.sendSearchEvent();
		this.setup.pmBrain.SendEvent("toSetSearching");
		this.setup.pmCombat.SendEvent("toDisableFSM");
		yield break;
	}

	
	private IEnumerator goToSpawnRoutine()
	{
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.castPointAroundSpawn(9f));
		yield return null;
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		timer = Time.time + 8f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToExplodeRunRoutine()
	{
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.findPointAwayFromExplosion(25f));
		yield return null;
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.search.setToWaypoint();
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		timer = Time.time + 8f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitOneSecond;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToNearbyPointRoutine()
	{
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.findRandomPoint((float)UnityEngine.Random.Range(35, 60)));
		yield return null;
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.search.setToWaypoint();
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		timer = Time.time + 8f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.search.setToClosestPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitPointFiveSeconds;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToBackAwayRoutine()
	{
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.setup.search.StartCoroutine(this.setup.search.findPointAwayFromPlayer(20f));
		yield return null;
		float timer = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("goToRandomPoint");
				yield break;
			}
			yield return null;
		}
		this.search.setToWaypoint();
		this.animator.SetInteger("randInt1", 0);
		this.animator.SetBool("backAwayBOOL", true);
		this.ai.StartCoroutine("toSearch");
		yield return null;
		timer = Time.time + UnityEngine.Random.Range(4f, 7f);
		while (this.setup.ai.targetDist > 15f)
		{
			if (Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			Vector3 lookPos = this.ai.wantedDir * -1f;
			lookPos = this.tr.position + lookPos;
			this.doSmoothLookAt(lookPos, 1.4f);
			yield return null;
		}
		this.setup.search.setToClosestPlayer();
		this.animator.SetBool("backAwayBOOL", false);
		this.animator.SetBool("dodgeBOOL", false);
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.setup.ai.StartCoroutine("toStop");
		if (this.fsmDoLeaderCallFollower.Value)
		{
			this.setup.pmCombat.SendEvent("goToCallFollower");
			yield break;
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToRescueRoutine()
	{
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.fsmRescueBool.Value = false;
		this.doAction = false;
		this.cancelEvent = false;
		float targetDist = Vector3.Distance(this.fsmCurrentMemberGo.Value.transform.position, this.tr.position);
		float targetAngle = this.returnTargetObjectAngle(this.fsmCurrentMemberGo.Value);
		this.setup.familyFunctions.checkIfOccupied();
		yield return null;
		if (this.setup.ai.mainPlayerDist > 65f || targetDist > 65f)
		{
			this.setup.familyFunctions.resetFamilyParams();
			yield return YieldPresets.WaitPointFiveSeconds;
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		if (this.cancelEvent)
		{
			this.setup.pmCombat.SendEvent("goToGuard1");
			yield break;
		}
		this.setup.familyFunctions.setupDragParams();
		this.setup.familyFunctions.setOccupied();
		if ((targetAngle > -80f && targetAngle < 80f) || targetDist < 16f)
		{
			this.search.setToRescueRunPos();
			yield return YieldPresets.WaitPointZeroFiveSeconds;
			this.ai.StartCoroutine("toRun");
			timer = Time.time + 5f;
			while (this.ai.targetDist > 7f)
			{
				if (Time.time > timer)
				{
					this.ai.StartCoroutine("toStop");
					this.setup.familyFunctions.resetFamilyParams();
					this.setup.familyFunctions.resetTargetOccupied();
					this.animator.SetBool("rescueBool1", false);
					yield return YieldPresets.WaitPointFiveSeconds;
					this.setup.pmCombat.SendEvent("toReset");
					yield break;
				}
				yield return null;
			}
			this.search.setToCurrentMember();
			yield return YieldPresets.WaitPointZeroFiveSeconds;
		}
		bool cancelThis = false;
		this.cancelEvent = false;
		this.search.setToCurrentMember();
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 5f;
		yield return YieldPresets.WaitPointFiveSeconds;
		while (this.ai.targetDist > 14f && !cancelThis)
		{
			if (this.setup.familyFunctions.targetFamilyFunctions == null)
			{
				cancelThis = true;
				break;
			}
			if (!this.setup.familyFunctions.targetFamilyFunctions.dying)
			{
				cancelThis = true;
			}
			if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf)
			{
				cancelThis = true;
			}
			if (Time.time > timer)
			{
				cancelThis = true;
				break;
			}
			yield return null;
		}
		if (!cancelThis && !this.cancelEvent)
		{
			this.setup.familyFunctions.checkTargetOccupied();
			yield return YieldPresets.WaitPointOneSeconds;
			timer = Time.time + 0.7f;
			while (Time.time < timer && !cancelThis && !this.cancelEvent)
			{
				this.animator.SetBool("rescueBool1", true);
				if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf)
				{
					cancelThis = true;
				}
				this.doSmoothLookAt(this.setup.pmCombat.FsmVariables.GetFsmGameObject("matchPosGo").Value.transform.position, 3f);
				yield return null;
			}
			this.search.findPointAwayFromPlayer(24f);
			this.search.setToWaypoint();
			timer = Time.time + 1f;
			while (Time.time < timer && !cancelThis && !this.cancelEvent)
			{
				if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf)
				{
					cancelThis = true;
				}
				this.doSmoothLookAt(this.fsmCurrentMemberGo.Value.transform.position, 3f);
				yield return null;
			}
			timer = Time.time + 6f;
			while (Time.time < timer && !cancelThis && !this.cancelEvent)
			{
				if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf)
				{
					cancelThis = true;
				}
				Vector3 lookPos = this.ai.wantedDir * -1f;
				lookPos = this.tr.position + lookPos;
				this.doSmoothLookAt(lookPos, 1.4f);
				if (this.ai.targetDist < 5f)
				{
					break;
				}
				yield return null;
			}
		}
		if (cancelThis || this.cancelEvent)
		{
			this.ai.StartCoroutine("toStop");
			this.setup.familyFunctions.resetFamilyParams();
			this.setup.familyFunctions.resetTargetOccupied();
			this.animator.SetBool("rescueBool1", false);
			yield return YieldPresets.WaitPointFiveSeconds;
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.setup.enemyEvents.sendDropEvent();
		this.ai.StartCoroutine("toStop");
		this.setup.familyFunctions.resetFamilyParams();
		this.setup.familyFunctions.resetTargetOccupied();
		this.animator.SetBool("rescueBool1", false);
		yield return YieldPresets.WaitOneSecond;
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToGuardRoutine()
	{
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.cancelEvent = false;
		float targetDist = Vector3.Distance(this.fsmCurrentMemberGo.Value.transform.position, this.tr.position);
		if (this.ai.mainPlayerDist < 12f || targetDist < 12f || targetDist > 50f)
		{
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.search.setToGuardPosition(this.fsmCurrentMemberGo.Value);
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 8f;
		while (Time.time < timer)
		{
			if (this.ai.targetDist < 8f)
			{
				break;
			}
			yield return null;
		}
		this.search.setToPlayer();
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.ai.StartCoroutine("toStop");
		this.ai.resetCombatParams();
		this.animator.SetBool("crouchBOOL", false);
		timer = UnityEngine.Random.Range(9f, 15f) + Time.time;
		bool toAttack = false;
		bool toRetreat = false;
		while (Time.time < timer && !toAttack && !toRetreat)
		{
			targetDist = Vector3.Distance(this.fsmCurrentMemberGo.Value.transform.position, this.tr.position);
			if (this.ai.mainPlayerDist < 7f)
			{
				toAttack = true;
			}
			if (targetDist > 10f)
			{
				toRetreat = true;
			}
			yield return null;
		}
		if (toRetreat)
		{
			timer = Time.time + UnityEngine.Random.Range(7f, 10f);
			this.search.setToCurrentMember();
			this.ai.StartCoroutine("toSearch");
			this.animator.SetInteger("randInt1", 3);
			this.animator.SetBool("dodgeBOOL", true);
			while (Time.time < timer && !toAttack)
			{
				if (this.ai.mainPlayerDist < 7f)
				{
					toAttack = true;
				}
				targetDist = Vector3.Distance(this.fsmCurrentMemberGo.Value.transform.position, this.tr.position);
				if (targetDist < 6f)
				{
					break;
				}
				Vector3 lookPos = this.ai.wantedDir * -1f;
				lookPos = this.tr.position + lookPos;
				this.doSmoothLookAt(lookPos, 1.4f);
				yield return null;
			}
		}
		this.animator.SetBool("dodgeBOOL", false);
		this.ai.StartCoroutine("toStop");
		if (toAttack)
		{
			base.StartCoroutine(this.doCloseAttackRoutine(true));
			yield break;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToClimbWallRoutine()
	{
		float timer = Time.time;
		this.animator.SetInteger("climbDirInt", 0);
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.cancelEvent = false;
		yield return YieldPresets.WaitPointOneSeconds;
		if (this.wallGo == null || this.ai.mainPlayerHeight > -3f)
		{
			this.cancelEvent = true;
			base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
			yield break;
		}
		this.search.StartCoroutine("toDisableVis");
		this.search.setToWallRunUp();
		timer = Time.time + 3f;
		while (Time.time < timer)
		{
			if (this.animControl.currLayerState1.tagHash == this.animControl.idlehash)
			{
				break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toRun");
		this.ai.cancelDefaultActions();
		timer = Time.time + 14f;
		float wallDist = Vector3.Distance(this.wallGo.transform.position, this.tr.position);
		while (Time.time < timer)
		{
			if (this.wallGo == null)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
				yield break;
			}
			wallDist = Vector3.Distance(this.search.currentWaypoint.transform.position, this.tr.position);
			if (wallDist < 7f)
			{
				break;
			}
			yield return null;
		}
		this.search.setToWall();
		yield return YieldPresets.WaitPointOneSeconds;
		timer = Time.time + 8f;
		while (Time.time < timer)
		{
			if (this.wallGo == null)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
				yield break;
			}
			wallDist = Vector3.Distance(this.search.currentWaypoint.transform.position, this.tr.position);
			if (wallDist < 11f)
			{
				break;
			}
			yield return null;
		}
		Vector3 attachPos = Vector3.zero;
		if (!this.search.checkWallHeight())
		{
			this.animator.SetBool("attackJumpBOOL", true);
			this.animator.SetBool("jumpBOOL", true);
			timer = Time.time + 0.75f;
			while (Time.time < timer)
			{
				if (this.wallGo)
				{
					this.doSmoothLookAt(this.wallGo.transform.position, 2f);
				}
				yield return null;
			}
			base.StartCoroutine(this.cancelClimbWallRoutine(true, false));
			yield break;
		}
		attachPos = this.search.findWallAttachPos(this.wallGo);
		this.animator.SetInteger("climbDirInt", 2);
		this.animator.SetBool("treeBOOL", true);
		timer = Time.time + 4f;
		while (Time.time < timer && !this.cancelEvent)
		{
			if (this.wallGo == null)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
				yield break;
			}
			this.doSmoothLookAt(this.wallGo.transform.position, 4f);
			if (this.animControl.currLayerState1.tagHash == this.animControl.onRockHash || this.animControl.currLayerState1.tagHash == this.animControl.idlehash)
			{
				break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 0f);
		timer = Time.time + 5f;
		while (Time.time < timer && !this.cancelEvent)
		{
			if (this.animControl.currLayerState1.tagHash == this.animControl.idlehash || this.animControl.nextLayerState1.tagHash == this.animControl.idlehash)
			{
				break;
			}
			if (this.wallGo == null)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
				yield break;
			}
			this.doSmoothLookAt(this.wallGo.transform.position, 4f);
			if (!this.animator.IsInTransition(0))
			{
				this.animator.MatchTarget(attachPos, Quaternion.identity, AvatarTarget.Body, _weightMask, 0.2f, 0.55f);
			}
			yield return null;
		}
		this.doAction = false;
		timer = Time.time + 10f;
		this.setup.mutantStats.setTargetUp();
		this.ai.setClimbWallCoolDown();
		while (Time.time < timer && !this.doAction && !this.cancelEvent)
		{
			this.animator.SetBool("actionBOOL1", true);
			if (this.wallGo == null)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
				yield break;
			}
			this.doSmoothLookAt(this.wallGo.transform.position, 4f);
			if (!this.animator.IsInTransition(0))
			{
				this.animator.MatchTarget(attachPos, Quaternion.identity, AvatarTarget.Body, _weightMask, 0f, 1f);
			}
			yield return null;
		}
		if (this.cancelEvent)
		{
			base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
			yield break;
		}
		if (this.doAction)
		{
			this.ai.setClimbWallCoolDown();
			this.animator.SetInteger("climbDirInt", 1);
			yield return YieldPresets.WaitPointTwoSeconds;
			this.ai.StartCoroutine("toStop");
			this.fsmToClimbWall.Value = false;
			this.animator.SetBool("treeBOOL", false);
			yield return YieldPresets.WaitOnePointThreeSeconds;
			if (this.ai.mainPlayerHeight < 3f)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(true, false));
			}
			else
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(false, true));
			}
			yield break;
		}
		yield break;
	}

	
	private IEnumerator cancelClimbWallRoutine(bool doAttack, bool attackBelow)
	{
		this.fsmAttackStructure.Value = false;
		this.fsmInTreeBool.Value = false;
		this.fsmToClimbWall.Value = false;
		this.fsmStalking.Value = false;
		this.fsmIsStalkingBool.Value = false;
		this.animator.SetBool("attackBelowBOOL", false);
		this.animator.SetBool("treeBOOL", false);
		this.animator.SetBool("attackJumpBOOL", false);
		this.animator.SetBool("jumpBOOL", false);
		this.animator.SetBool("actionBOOL1", false);
		this.animator.SetBool("crouchBOOL", false);
		if (!attackBelow)
		{
			this.search.StartCoroutine(this.search.toLook());
		}
		this.search.resetWallOccupied();
		if (doAttack)
		{
			this.animator.SetInteger("climbDirInt", 0);
		}
		else
		{
			this.animator.SetInteger("climbDirInt", -1);
		}
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitPointFiveSeconds;
		if (attackBelow)
		{
			this.setup.pmCombat.SendEvent("goToAttackBelow");
			yield break;
		}
		if (doAttack)
		{
			this.setup.pmBrain.SendEvent("toSetAggressive");
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToAttackBelowRoutine()
	{
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.search.setToLastPlayerTarget();
		this.doAction = false;
		this.cancelEvent = false;
		yield return YieldPresets.WaitPointTwoSeconds;
		if (this.ai.mainPlayerDist > 15f)
		{
			this.ai.StartCoroutine("toRun");
			timer = Time.time + 5f;
			while (Time.time < timer)
			{
				if (this.ai.mainPlayerDist < 15f || Vector3.Distance(this.search.currentWaypoint.transform.position, this.tr.position) < 7f)
				{
					break;
				}
				yield return null;
			}
		}
		this.fsmAttackStructure.Value = true;
		this.animator.SetBool("crouchBOOL", true);
		this.animator.SetBool("stalkingBOOL", false);
		this.animator.SetBool("feedingBOOL", false);
		this.ai.StartCoroutine("toStop");
		this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 2));
		this.animator.SetBool("attackBelowBOOL", true);
		yield return YieldPresets.WaitPointSevenSeconds;
		timer = Time.time + (float)UnityEngine.Random.Range(30, 60);
		while (Time.time < timer)
		{
			if (this.ai.mainPlayerDist < 15f && this.ai.mainPlayerHeight < 2f)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(true, false));
				yield break;
			}
			if (!this.fsmOnStructureBool.Value)
			{
				base.StartCoroutine(this.cancelClimbWallRoutine(true, false));
				yield break;
			}
			yield return null;
		}
		base.StartCoroutine(this.cancelClimbWallRoutine(false, false));
		yield break;
	}

	
	private IEnumerator goToStructureRoutine()
	{
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.cancelEvent = false;
		this.noValidTarget = false;
		if (this.fsmFemaleSkinnyBool.Value || UnityEngine.Random.value > 0.75f)
		{
			this.search.StartCoroutine(this.search.findCloseFoodStructure());
			yield return YieldPresets.WaitPointFiveSeconds;
			if (this.doAction)
			{
				this.fsmDoFoodStructure.Value = true;
			}
		}
		else
		{
			this.search.StartCoroutine(this.search.findCloseStructure());
			yield return YieldPresets.WaitPointFiveSeconds;
		}
		if (this.noValidTarget)
		{
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.animator.SetBool("stalkingBOOL", false);
		this.search.StartCoroutine("toDisableVis");
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 8f;
		while (this.ai.targetDist > 7f)
		{
			if (this.structureGo == null || Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.search.setToStructure();
		yield return YieldPresets.WaitPointOneSeconds;
		timer = Time.time + 3f;
		while (this.ai.targetDist > 7f)
		{
			if (this.structureGo == null || Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			this.doSmoothLookAt(this.structureGo.transform.position, 3f);
			yield return null;
		}
		this.cancelEvent = false;
		float timeout = (float)UnityEngine.Random.Range(8, 20);
		if (this.fsmFemaleSkinnyBool.Value)
		{
			timeout = 7f;
		}
		this.fsmAttackStructure.Value = true;
		this.animator.SetBool("crouchBOOL", false);
		if (this.fsmDoFoodStructure.Value)
		{
			timer = Time.time + 30f;
			this.animator.SetInteger("randInt1", 3);
			this.animator.SetBool("feedingBOOL", true);
			this.ai.StartCoroutine("toStop");
			this.doAction = false;
			this.search.StartCoroutine(this.search.stealFoodRoutine());
			while (Time.time < timer)
			{
				if (this.structureGo == null)
				{
					this.setup.pmCombat.SendEvent("toReset");
				}
				if (this.ai.mainPlayerDist < 20f || this.cancelEvent)
				{
					this.animator.SetBool("feedingBOOL", false);
					this.fsmDoFoodStructure.Value = false;
					this.setup.pmCombat.SendEvent("toReset");
					yield break;
				}
				if (this.doAction)
				{
					break;
				}
				if (this.structureGo)
				{
					this.doSmoothLookAt(this.structureGo.transform.position, 2f);
				}
				yield return null;
			}
		}
		this.animator.SetBool("feedingBOOL", false);
		this.fsmDoFoodStructure.Value = false;
		this.fsmAttackStructure.Value = true;
		this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 3));
		this.animator.SetBool("attackBOOL", true);
		this.ai.StartCoroutine("toStop");
		timeout = Time.time + timeout;
		while (Time.time < timeout)
		{
			if (this.structureGo == null)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (this.ai.mainPlayerDist < 12f)
			{
				this.fsmDoFoodStructure.Value = false;
				break;
			}
			if (this.structureGo)
			{
				this.doSmoothLookAt(this.structureGo.transform.position, 2f);
			}
			yield return null;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToVisibleStructureRoutine()
	{
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.cancelEvent = false;
		this.noValidTarget = false;
		if (this.search.lastHitGo == null)
		{
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.search.updateCurrentWaypoint(this.search.lastHitGo.transform.position);
		this.setup.pmCombat.FsmVariables.GetFsmGameObject("structureGo").Value = this.search.lastHitGo;
		if (this.setup.pmCombatScript)
		{
			this.setup.pmCombatScript.structureGo = this.search.lastHitGo;
		}
		this.search.lastHitGo = null;
		this.animator.SetBool("stalkingBOOL", false);
		this.search.StartCoroutine("toDisableVis");
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 8f;
		while (this.ai.targetDist > 7f)
		{
			if (this.structureGo == null || Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.search.setToStructure();
		yield return YieldPresets.WaitPointOneSeconds;
		timer = Time.time + 3f;
		while (this.ai.targetDist > 7f)
		{
			if (this.structureGo == null || Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			this.doSmoothLookAt(this.structureGo.transform.position, 3f);
			yield return null;
		}
		this.cancelEvent = false;
		float timeout = (float)UnityEngine.Random.Range(9, 18);
		this.fsmAttackStructure.Value = true;
		this.animator.SetBool("crouchBOOL", false);
		if (this.fsmDoFoodStructure.Value)
		{
			timer = Time.time + 20f;
			this.animator.SetInteger("randInt1", 3);
			this.animator.SetBool("feedingBOOL", true);
			this.ai.StartCoroutine("toStop");
			this.doAction = false;
			this.search.StartCoroutine(this.search.stealFoodRoutine());
			while (Time.time < timer)
			{
				if (this.structureGo == null)
				{
					this.setup.pmCombat.SendEvent("toReset");
				}
				if (this.ai.mainPlayerDist < 7f || this.cancelEvent)
				{
					this.animator.SetBool("feedingBOOL", false);
					this.fsmDoFoodStructure.Value = false;
					this.setup.pmCombat.SendEvent("toReset");
					yield break;
				}
				if (this.doAction)
				{
					break;
				}
				if (this.structureGo)
				{
					this.doSmoothLookAt(this.structureGo.transform.position, 2f);
				}
				yield return null;
			}
		}
		this.animator.SetBool("feedingBOOL", false);
		this.fsmDoFoodStructure.Value = false;
		this.fsmAttackStructure.Value = true;
		this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 3));
		this.animator.SetBool("attackBOOL", true);
		this.ai.StartCoroutine("toStop");
		timeout = Time.time + timeout;
		while (Time.time < timeout)
		{
			if (this.structureGo == null)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (this.ai.mainPlayerDist < 7f)
			{
				this.fsmDoFoodStructure.Value = false;
				break;
			}
			if (this.structureGo)
			{
				this.doSmoothLookAt(this.structureGo.transform.position, 2f);
			}
			yield return null;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToBurnStructureRoutine()
	{
		Debug.Log(base.transform.parent.gameObject.name + " starting BURN structure");
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.cancelEvent = false;
		this.noValidTarget = false;
		this.search.StartCoroutine(this.search.findCloseBurnStructure());
		yield return YieldPresets.WaitPointFiveSeconds;
		if (this.noValidTarget || this.structureGo == null)
		{
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.animator.SetBool("stalkingBOOL", false);
		this.ai.StartCoroutine("toWalk");
		timer = Time.time + 15f;
		float dist = Vector3.Distance(this.structureGo.transform.position, this.tr.position);
		while (dist > 7f)
		{
			dist = Vector3.Distance(this.structureGo.transform.position, this.tr.position);
			if (dist < 9.5f)
			{
				this.doSmoothLookAt(this.structureGo.transform.position, 2f);
			}
			if (this.structureGo == null || Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitTwoSeconds;
		timer = Time.time + 2f;
		this.animator.SetInteger("randAttackInt1", 5);
		this.animator.SetBool("attackBOOL", true);
		while (Time.time < timer)
		{
			if (this.structureGo == null)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (this.structureGo)
			{
				this.doSmoothLookAt(this.structureGo.transform.position, 2f);
			}
			yield return null;
		}
		this.animator.SetBool("attackBOOL", false);
		this.setup.pmCombat.SendEvent("goToRunAway");
		yield break;
	}

	
	private IEnumerator goToEatBodyRoutine()
	{
		float timer = Time.time;
		float bodyDist = float.PositiveInfinity;
		if (this.currentMemberGo)
		{
			bodyDist = Vector3.Distance(this.currentMemberGo.transform.position, this.tr.position);
		}
		if (bodyDist < 12f || bodyDist > 90f || this.animator.GetBool("deathBOOL") || this.animator.GetBool("treeBOOL") || this.currentMemberGo == null)
		{
			this.fsmEatBodyBool.Value = false;
			this.fsmBrainEatBodyBool.Value = false;
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this.ai.resetCombatParams();
		this.doAction = false;
		this.cancelEvent = false;
		this.noValidTarget = false;
		this.search.setToBody();
		this.setup.familyFunctions.setOccupied();
		this.ai.StartCoroutine("toRun");
		this.search.StartCoroutine("toDisableVis");
		yield return YieldPresets.WaitPointFiveSeconds;
		float currDist = float.PositiveInfinity;
		float t = Time.time + 5f;
		while (currDist > 13.4f && !this.cancelEvent)
		{
			if (this.fsmFearBool.Value)
			{
				this.setup.pmCombat.SendEvent("toFear");
				yield break;
			}
			if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf || Time.time > t)
			{
				this.cancelEvent = true;
			}
			this.search.setToBody();
			currDist = Vector3.Distance(this.tr.position, this.fsmCurrentMemberGo.Value.transform.position);
			this.animator.SetBool("jumpBlockBool", true);
			yield return null;
		}
		if (!this.cancelEvent)
		{
			this.search.StartCoroutine(this.search.findEatingPos(this.fsmCurrentMemberGo.Value.transform.position));
			this.setup.enemyEvents.disableCollision();
			this.animator.SetBool("feedingBOOL", true);
			this.animator.SetBool("screamBOOL", false);
			this.ai.StartCoroutine("toStop");
			yield return null;
			while (this.animControl.fullBodyState.tagHash != this.animControl.onRockHash && !this.cancelEvent)
			{
				if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf)
				{
					this.cancelEvent = true;
				}
				if (this.fsmCurrentMemberGo.Value)
				{
					this.doSmoothLookAt(this.fsmCurrentMemberGo.Value.transform.position, 2f);
				}
				yield return null;
			}
			this.search.setToPlayer();
			MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 0f);
			t = Time.time + 5f;
			while (this.setup.animControl.fullBodyState.tagHash != this.setup.hashs.attackTag && !this.cancelEvent)
			{
				if (Time.time > t)
				{
					this.cancelEvent = true;
				}
				if (!this.animator.IsInTransition(0))
				{
					this.animator.MatchTarget(this.attachPos, Quaternion.identity, AvatarTarget.LeftFoot, _weightMask, 0.2f, 0.8f);
				}
				yield return null;
			}
			this.ai.StartCoroutine("toStop");
			this.search.StartCoroutine("toLook");
			t = Time.time + (float)UnityEngine.Random.Range(10, 20);
			while (Time.time < t && !this.cancelEvent)
			{
				if (this.fsmCurrentMemberGo.Value == null || !this.fsmCurrentMemberGo.Value.activeSelf || !this.fsmEatBodyBool.Value || Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.tr.position) < 15f)
				{
					this.cancelEvent = true;
				}
				if (this.fsmCurrentMemberGo.Value)
				{
					this.doSmoothLookAt(this.fsmCurrentMemberGo.Value.transform.position, 2f);
				}
				yield return null;
			}
		}
		if (this.fsmCurrentMemberGo.Value && !this.cancelEvent)
		{
			try
			{
				this.setup.worldSearch.StartCoroutine(this.setup.worldSearch.gibCurrentMutant(this.fsmCurrentMemberGo.Value));
			}
			catch
			{
				Debug.Log("fsmCurrentMemberGo was null");
			}
		}
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 3));
		this.setup.familyFunctions.resetFamilyParams();
		this.fsmEatBodyBool.Value = false;
		this.search.StartCoroutine("toLook");
		this.fsmBrainEatBodyBool.Value = false;
		this.ai.StartCoroutine("toStop");
		this.animator.SetBool("feedingBOOL", false);
		this.animator.SetBool("jumpBlockBool", false);
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToFreakoutRoutine()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("freakoutBool").Value = false;
		this.ai.StartCoroutine("toStop");
		this.animator.SetInteger("randInt1", 0);
		this.animator.SetBool("freakoutBOOL", true);
		float t = Time.time + 4f;
		while (Time.time < t)
		{
			if (this.currentMemberGo != null)
			{
				this.doSmoothLookAt(this.currentMemberGo.transform.position, 3f);
			}
			yield return null;
		}
		this.setup.search.setToPlayer();
		this.setup.pmCombat.FsmVariables.GetFsmBool("freakoutBool").Value = false;
		this.animator.SetBool("freakoutBOOL", false);
		this.setup.pmCombat.SendEvent("goToRunAway");
		yield return null;
		yield break;
	}

	
	private IEnumerator goToRockJumpAttackRoutine()
	{
		this.setup.search.setToWaypoint();
		this.ai.StartCoroutine("toRun");
		float t = Time.time + 0.5f;
		while (Time.time < t)
		{
			if (this.fsmFearBool.Value)
			{
				this.setup.pmCombat.SendEvent("toFear");
				yield break;
			}
			yield return null;
		}
		t = Time.time + 5f;
		while ((double)this.ai.targetDist > 9.5)
		{
			if (this.fsmFearBool.Value)
			{
				this.setup.pmCombat.SendEvent("toFear");
				yield break;
			}
			if (Time.time > t)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			if (this.fsmWayPointGo.Value)
			{
				this.doSmoothLookAt(this.fsmWayPointGo.Value.transform.position, 3f);
			}
			yield return null;
		}
		t = Time.time + 0.35f;
		while (Time.time < t)
		{
			if (this.fsmWayPointGo.Value)
			{
				this.doSmoothLookAt(this.fsmWayPointGo.Value.transform.position, 3f);
			}
			this.fsmDisableControllerBool.Value = true;
			this.animator.SetBool("attackJumpBOOL", true);
			this.animator.SetBool("jumpBOOL", true);
			yield return null;
		}
		this.search.setToPlayer();
		t = Time.time + 0.4f;
		while (Time.time < t)
		{
			if (this.fsmWayPointGo.Value)
			{
				this.doSmoothLookAt(this.fsmWayPointGo.Value.transform.position, 3f);
			}
			this.animator.SetBool("attackJumpBOOL", false);
			this.animator.SetBool("jumpBOOL", false);
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		this.fsmDisableControllerBool.Value = false;
		t = Time.time + 5f;
		while (this.animControl.fullBodyState.tagHash != this.animControl.idlehash)
		{
			if (Time.time > t)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToTreeAttackRoutine()
	{
		float t = Time.time + 4f;
		this.search.setToClosestPlayer();
		while (this.animControl.fullBodyState.tagHash != this.animControl.idlehash)
		{
			if (Time.time > t)
			{
				base.StartCoroutine(this.doTreeJumpDownRoutine());
				yield break;
			}
			yield return null;
		}
		if (this.ai.mainPlayerAngle < -135f || this.ai.mainPlayerAngle > 135f)
		{
			this.animator.SetInteger("randInt1", 0);
		}
		else
		{
			this.animator.SetInteger("randInt1", 1);
		}
		this.animator.SetBool("attackBOOL", true);
		t = Time.time + 4f;
		while (this.animControl.fullBodyState.tagHash != this.animControl.jumpFallHash)
		{
			if (Time.time > t)
			{
				base.StartCoroutine(this.doTreeJumpDownRoutine());
				yield break;
			}
			yield return null;
		}
		this.rootTr.gameObject.layer = 31;
		this.search.setToClosestPlayer();
		Vector3 landPos = this.ai.target.position;
		MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(0.4f, 0f, 0.4f), 0f);
		t = Time.time + 4f;
		while (this.setup.animControl.fullBodyState.tagHash != this.animControl.landingHash)
		{
			if (Time.time > t)
			{
				this.animControl.forceJumpLand();
				break;
			}
			if (!this.animator.IsInTransition(0))
			{
				this.animator.MatchTarget(landPos, Quaternion.identity, AvatarTarget.Body, _weightMask, 0.1f, 1f);
			}
			this.doSmoothLookAt(landPos, 2f);
			yield return null;
		}
		this.animator.SetBool("actionBOOL1", false);
		this.animator.SetBool("attackBOOL", false);
		this.search.StartCoroutine("toTrack");
		this.rootTr.gameObject.layer = 14;
		this.setup.mutantStats.setTargetUp();
		this.fsmInTreeBool.Value = false;
		this.setup.pmBrain.SendEvent("toSetAggressive");
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToExitWaterRoutine()
	{
		this.animControl.resetAllActionBools();
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this.doAction = false;
		this.noValidTarget = false;
		this.search.StartCoroutine(this.search.findRandomPoint(30f));
		float t = Time.time + 1f;
		while (!this.doAction)
		{
			if (Time.time > t)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toRun");
		t = Time.time + 8f;
		while (this.ai.targetDist > 7f && Time.time < t)
		{
			if (this.animator.GetFloat("Speed") < 0.2f)
			{
				this.ai.StartCoroutine("toRun");
			}
			yield return null;
		}
		this.setup.pmCombat.FsmVariables.GetFsmBool("exitWaterBool").Value = false;
		this.ai.StartCoroutine("toStop");
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private IEnumerator goToBurningRoutine()
	{
		this.ai.StartCoroutine("toStop");
		this.ai.resetCombatParams();
		this.search.StartCoroutine("toDisableVis");
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 4));
		this.animator.SetBool("burning", true);
		this.toBurnRecover = false;
		float t = Time.time + 20f;
		while (!this.toBurnRecover && Time.time < t)
		{
			yield return null;
		}
		this.toBurnRecover = false;
		this.animator.SetBool("burning", false);
		this.fsmOnFireBool.Value = false;
		this.search.StartCoroutine("toLook");
		t = Time.time + 7f;
		while (this.animControl.fullBodyState.tagHash != this.animControl.idlehash && Time.time < t)
		{
			yield return null;
		}
		this.setup.pmCombat.SendEvent("toReset");
		yield break;
	}

	
	private void resetDefaultParams()
	{
		this.setup.ai.resetCombatParams();
		this.setup.ai.StartCoroutine("toStop");
		this.doAction = false;
	}

	
	public void doSmoothLookAt(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = this.tr.position.y;
		Vector3 vector = lookAtPos - this.tr.position;
		Quaternion quaternion = this.tr.rotation;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
		this.tr.rotation = quaternion;
	}

	
	private void doSmoothLookAtDir(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = this.tr.position.y;
		Vector3 vector = lookAtPos - this.tr.position;
		Quaternion quaternion = this.tr.rotation;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
		this.tr.rotation = quaternion;
	}

	
	private float returnTargetObjectAngle(GameObject go)
	{
		Vector3 vector = go.transform.InverseTransformPoint(this.tr.position);
		float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		this.setup.pmCombat.FsmVariables.GetFsmFloat("objectAngle").Value = num;
		this.setup.pmBrain.FsmVariables.GetFsmFloat("objectAngle").Value = num;
		return num;
	}

	
	private IEnumerator smoothAnimatorFloat(string name, float val, float damp, float speed)
	{
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * speed;
			this.animator.SetFloat(name, val, damp, t);
			yield return null;
		}
		yield return val;
		yield break;
	}

	
	private mutantScriptSetup setup;

	
	private mutantSearchFunctions search;

	
	private mutantAnimatorControl animControl;

	
	private mutantMaleHashId hashId;

	
	private mutantAI ai;

	
	private Animator animator;

	
	private Transform tr;

	
	private Transform rootTr;

	
	public GameObject wallGo;

	
	public GameObject structureGo;

	
	public GameObject currentMemberGo;

	
	private FsmGameObject fsmCurrentAttackerGo;

	
	public FsmGameObject fsmCurrentMemberGo;

	
	private FsmGameObject fsmWayPointGo;

	
	private FsmGameObject fsmTreeGo;

	
	private FsmGameObject fsmNextTreeGo;

	
	private FsmGameObject fsmWallGo;

	
	private FsmBool fsmOnFireBool;

	
	private FsmBool fsmOnStructureBool;

	
	private FsmBool fsmFearBool;

	
	private FsmBool fsmCloseCombatBool;

	
	private FsmBool fsmStalking;

	
	private FsmBool fsmIsStalkingBool;

	
	private FsmBool fsmJumpDownBool;

	
	private FsmBool fsmDeathBool;

	
	private FsmBool fsmDeathRecoverBool;

	
	private FsmBool fsmdeathFinalBool;

	
	private FsmBool fsmInsideBase;

	
	private FsmBool fsmDoLeaderCallFollower;

	
	private FsmBool fsmRescueBool;

	
	private FsmBool fsmToClimbWall;

	
	private FsmBool fsmInTreeBool;

	
	private FsmBool fsmAttackStructure;

	
	private FsmBool fsmDoFoodStructure;

	
	private FsmBool fsmEatBodyBool;

	
	private FsmBool fsmFearOverrideBool;

	
	private FsmBool fsmTargetSeenBool;

	
	private FsmBool fsmLeaderBool;

	
	private FsmBool fsmEnableControllerBool;

	
	private FsmBool fsmFemaleSkinnyBool;

	
	private FsmBool fsmBrainEatBodyBool;

	
	private FsmBool fsmPlayerIsRed;

	
	private FsmBool fsmDisableControllerBool;

	
	public Vector3 onRockPos;

	
	public Vector3 attachPos;

	
	public float rotateSpeed = 3f;

	
	public float waypointArriveDist = 12f;

	
	public bool doAction;

	
	public bool toOnRock;

	
	public bool toRescue;

	
	public bool toDrop;

	
	public bool cancelEvent;

	
	public bool runToAttack;

	
	public bool runLongAttack;

	
	public bool noValidTarget;

	
	public bool toBurnRecover;

	
	public bool toAttractArtifact;

	
	public bool toRepelArtifact;

	
	public List<IEnumerator> activeRoutines = new List<IEnumerator>();

	
	private Quaternion desiredRotation;
}
