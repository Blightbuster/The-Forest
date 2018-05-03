using System;
using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class pmSearchReplace : MonoBehaviour
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
		this.fsmBrainLeaderGo = this.setup.pmBrain.FsmVariables.GetFsmGameObject("leaderGo");
		this.fsmBrainFiremanBool = this.setup.pmBrain.FsmVariables.GetFsmBool("getFiremanBool");
		this.fsmBrainFemaleSkinnyBool = this.setup.pmBrain.FsmVariables.GetFsmBool("femaleSkinnyBool");
		this.fsmBrainLeaderBool = this.setup.pmBrain.FsmVariables.GetFsmBool("leaderBool");
		this.fsmBrainEatBodyBool = this.setup.pmBrain.FsmVariables.GetFsmBool("eatBodyBool");
	}

	
	private void OnEnable()
	{
		this.toDisableSearchEvent();
	}

	
	private void stopAllSearchRoutines()
	{
		base.StopAllCoroutines();
	}

	
	private void Update()
	{
		this.fsmBrainTargetSeenBool = this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value;
		if (this._searchActive && Time.time > this.resetSearchTimer)
		{
			float sqrMagnitude = (base.transform.position - this.lastResetPos).sqrMagnitude;
			if (sqrMagnitude < 20f)
			{
				this.resetSearchState();
			}
			this.resetSearchTimer = Time.time + 15f;
			this.lastResetPos = base.transform.position;
		}
	}

	
	private void resetSearchState()
	{
		base.StopAllCoroutines();
		this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
	}

	
	private IEnumerator goToStartSearchRoutine()
	{
		this._searchActive = true;
		this.search.StartCoroutine("toLook");
		this.ai.StartCoroutine(this.ai.setRepathRate(1f));
		this._doFoodStructure = false;
		this._attackStructure = false;
		this._targetSpotted = false;
		this._toPlayerNoise = false;
		this.animator.SetBool("attackBOOL", false);
		this.animator.SetBool("treeBOOL", false);
		this.animator.SetBool("stalkingBOOL", false);
		this.animator.SetBool("sleepBOOL", false);
		this.animator.SetInteger("randInt1", 0);
		this.animator.SetBool("onCeilingBool", false);
		this.setup.pmSleep.FsmVariables.GetFsmBool("paleOnCeiling").Value = false;
		this.setup.dayCycle.sleepBlocker = true;
		base.StartCoroutine(this.setup.disableNonActiveFSM("action_searchFSM"));
		this.setup.pmBrain.SendEvent("toSetSearching");
		if (this._toRepelArtifact)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToArtifactRoutine(false));
			yield break;
		}
		if (!(this.fsmBrainLeaderGo.Value == null) && this.fsmBrainLeaderGo.Value.activeSelf)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToFollowLeaderRoutine());
			yield break;
		}
		if (!this.search.playerAware || this._longSearchBool || this._CloseSearchResetBool)
		{
			if (this._CloseSearchResetBool)
			{
				this._CloseSearchResetBool = false;
			}
			this.ActiveRoutine = base.StartCoroutine(this.goToLongSearchRoutine());
			yield break;
		}
		if (this.search.playerAware && this.ai.mainPlayerDist < 50f)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToLastSightingRoutine());
			yield break;
		}
		this.ActiveRoutine = base.StartCoroutine(this.goToLongSearchRoutine());
		yield break;
	}

	
	private IEnumerator goToFollowLeaderRoutine()
	{
		this.setup.search.setToLeader();
		yield return YieldPresets.WaitPointFiveSeconds;
		base.StartCoroutine(this.setup.search.getRandomTargetOffset(5f));
		this.animator.SetBool("crouchBOOL", false);
		this.animator.SetBool("backAwayBOOL", false);
		this.animator.SetBool("stalkingBOOL", false);
		this.search.StartCoroutine(this.setup.search.toLook());
		this._toPlayerNoise = false;
		this._toPlaceArt = false;
		this._toStructure = false;
		float findTrapTimer = Time.time + 1f;
		float findBurnTimer = 0f;
		while (!(this.fsmBrainLeaderGo.Value == null) && this.fsmBrainLeaderGo.Value.activeSelf)
		{
			this.search.setToLeader();
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this._toPlaceArt)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToPlaceArtRoutine());
				this._toPlaceArt = false;
				yield break;
			}
			if (this._toStructure)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToBurnStructureRoutine());
				this._toStructure = false;
				yield break;
			}
			if (this.setup.pmCombat.FsmVariables.GetFsmBool("rescueBool").Value)
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.toDisableSearchEvent();
				yield break;
			}
			if (Time.time > findTrapTimer)
			{
				if (this.setup.search.findCloseTrapTrigger())
				{
					this.ActiveRoutine = base.StartCoroutine(this.goToTrapTriggerRoutine());
					yield break;
				}
				findTrapTimer = Time.time + 2f;
			}
			if (this.ai.targetDist > 35f && !this.ai.startedRun)
			{
				this.ai.StartCoroutine("toRun");
			}
			else if (this.ai.targetDist <= this.waypointArriveDist || this.ai.targetDist >= 22f || this.ai.startedMove)
			{
				if (this.ai.targetDist < this.waypointArriveDist && this.ai.movingBool)
				{
					this.ai.StartCoroutine("toStop");
					if (Time.time > findBurnTimer && this.fsmBrainFiremanBool.Value && this.ai.mainPlayerDist < 70f)
					{
						this.setup.search.findCloseStructure();
						findBurnTimer = Time.time + 5f;
					}
				}
			}
			yield return YieldPresets.WaitPointTwoSeconds;
		}
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield break;
		yield break;
	}

	
	private IEnumerator goToLastSightingRoutine()
	{
		this.animator.SetBool("searchingBOOL", true);
		this.animator.SetBool("stalkingBOOL", false);
		float walkType = 0f;
		this._toPlayerNoise = false;
		this._targetFound = false;
		this.search.StartCoroutine("enableSearchTimeout");
		if (this.search.playerAware)
		{
			walkType = 0f;
			this.animControl.StartCoroutine(this.animControl.smoothChangeIdle(4f));
		}
		else
		{
			this._cautiousBool = true;
			this.animControl.StartCoroutine(this.animControl.smoothChangeIdle(3f));
		}
		this.ai.StartCoroutine("toStop");
		float timer = Time.time + 2.4f;
		while (Time.time < timer)
		{
			this.animator.SetFloat("walkTypeFloat1", walkType, 0.5f, Time.deltaTime);
			yield return null;
		}
		this.search.setToLastSighting();
		this.search.StartCoroutine("toLook");
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 20f;
		while (this.ai.targetDist > 12f)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		timer = Time.time + (float)UnityEngine.Random.Range(3, 5);
		while (Time.time < timer)
		{
			if (this.ai.mainPlayerDist > 80f)
			{
				this.search.playerAware = false;
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		this._doAction = false;
		if (!this.setup.typeSetup.inCave)
		{
			base.StartCoroutine(this.search.findCloseBush(12f));
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		if (this._doAction)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToCloseBushRoutine());
		}
		else
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator goToCloseBushRoutine()
	{
		this.ai.enablePathSearch();
		yield return YieldPresets.WaitForFixedUpdate;
		this._toPlayerNoise = false;
		this._targetFound = false;
		this._validPath = true;
		this.ai.StartCoroutine("toRun");
		float timer = Time.time + 10f;
		while (this.ai.targetDist > 6.5f)
		{
			if (Time.time > timer || !this._validPath)
			{
				this.ai.StartCoroutine("toStop");
				this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
				yield break;
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		this.animator.SetFloat("aggression", 2f);
		yield return YieldPresets.WaitTwoSeconds;
		this.animator.SetInteger("randAttackInt1", 5);
		this.animator.SetBool("attackBOOL", true);
		timer = Time.time + 3f;
		while (Time.time < timer)
		{
			if (Time.time < timer - 1.5f)
			{
				this.doSmoothLookAt(this.search.currentWaypoint.transform.position, 3f);
			}
			else
			{
				this.animator.SetBool("attackBOOL", false);
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		this.animator.SetBool("attackBOOL", false);
		this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToCloseSearchRoutine()
	{
		this.animator.SetBool("attackBOOL", false);
		this.ai.StartCoroutine("toStop");
		this.search.StartCoroutine("toLook");
		this._doAction = false;
		this._targetFound = false;
		this._toPlayerNoise = false;
		float timer = Time.time + 1f;
		while (Time.time < timer && !this._doAction)
		{
			this.search.StartCoroutine(this.search.castPointAroundLastSighting(21f));
			yield return YieldPresets.WaitPointOneSeconds;
		}
		if (!this._doAction)
		{
			this._longSearchBool = true;
			this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
			yield break;
		}
		this.ai.enablePathSearch();
		this.ai.StartCoroutine("toWalk");
		while (this.ai.targetDist > 8f)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this.setup.pmCombatScript.toRescue)
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.toDisableSearchEvent();
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		timer = Time.time + (float)UnityEngine.Random.Range(3, 5);
		while (Time.time < timer)
		{
			if (this.ai.mainPlayerDist > 80f)
			{
				this.search.playerAware = false;
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this.setup.pmCombatScript.toRescue)
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.toDisableSearchEvent();
				yield break;
			}
			yield return null;
		}
		if (this._CloseSearchResetBool)
		{
			this._longSearchBool = true;
			this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
			yield break;
		}
		this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToLongSearchRoutine()
	{
		this.animator.SetBool("stalkingBOOL", false);
		this.animator.SetBool("sleepBOOL", false);
		this._longSearchBool = false;
		this.setup.followerFunctions.sendSearchEvent();
		this.ai.StartCoroutine("toStop");
		this.search.StartCoroutine("toLook");
		this._doSearchAction = false;
		if (this._doPatrol)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToPatrolRoutine());
			yield break;
		}
		this.resetEncounterAnimParams();
		int searchCounter = 0;
		for (;;)
		{
			if (this.setup.typeSetup.inCave)
			{
				base.StartCoroutine(this.search.findRandomPoint(22f));
			}
			else if (searchCounter > 5)
			{
				base.StartCoroutine(this.search.findRandomPoint(this.setup.aiManager.fsmRandomSearchRange.Value));
			}
			else
			{
				switch (weightsRandomizer.From<int>(new Dictionary<int, int>
				{
					{
						0,
						Mathf.FloorToInt(this.setup.aiManager.fsmFindPointNearPlayer.Value * 1000f)
					},
					{
						1,
						Mathf.FloorToInt(this.setup.aiManager.fsmFindRandomPoint.Value * 1000f)
					},
					{
						2,
						Mathf.FloorToInt(this.setup.aiManager.fsmFindWayPoint.Value * 1000f)
					},
					{
						3,
						Mathf.FloorToInt(this.setup.aiManager.fsmFindStructure.Value * 1000f)
					},
					{
						4,
						Mathf.FloorToInt(this.setup.aiManager.fsmFindPlaneCrash.Value * 1000f)
					},
					{
						5,
						Mathf.FloorToInt(this.setup.aiManager.fsmFindPlayerRangedPoint.Value * 1000f)
					}
				}).TakeOne())
				{
				case 0:
					base.StartCoroutine(this.search.castPointAroundPlayer(50f));
					break;
				case 1:
					base.StartCoroutine(this.search.findRandomPoint(this.setup.aiManager.fsmRandomSearchRange.Value));
					break;
				case 2:
					base.StartCoroutine(this.search.findCloseWaypoint());
					break;
				case 3:
					base.StartCoroutine(this.search.findClosePlayerFire());
					if (!this._doSearchAction)
					{
						base.StartCoroutine(this.search.findDistantStructure(45f));
					}
					break;
				case 4:
					this.search.findPlaneCrash();
					break;
				case 5:
					base.StartCoroutine(this.search.findPointInFrontOfPlayer(330f));
					break;
				}
			}
			this.search.setToWaypoint();
			this.ai.enablePathSearch();
			yield return YieldPresets.WaitPointFiveSeconds;
			if (this._doSearchAction && this._validPath)
			{
				break;
			}
			if (!this._validPath)
			{
				searchCounter++;
			}
			this._doSearchAction = false;
			yield return YieldPresets.WaitOneSecond;
		}
		this.ActiveRoutine = base.StartCoroutine(this.moveToSearchPointRoutine());
		this._doSearchAction = false;
		yield break;
		yield break;
	}

	
	private IEnumerator moveToSearchPointRoutine()
	{
		this.search.setToWaypoint();
		this.search.StartCoroutine(this.search.toLook());
		this._toPlayerNoise = false;
		this._toStructure = false;
		if ((UnityEngine.Random.value < 0.4f || this.ai.mainPlayerDist > 350f) && !this.setup.typeSetup.inCave)
		{
			this.ai.StartCoroutine("toRun");
		}
		else
		{
			this.ai.StartCoroutine("toWalk");
		}
		this.animator.SetBool("crouchBOOL", false);
		float timer = Time.time + 20f;
		while (this.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				if (this.ai.mainPlayerDist < 140f && !this._encounterCoolDown && UnityEngine.Random.value * 2f < this.setup.aiManager.fsmSearchToEncounter.Value && !this.setup.typeSetup.inCave)
				{
					if (this.fsmBrainFemaleSkinnyBool.Value)
					{
						if (this._eatBody)
						{
							this.ActiveRoutine = base.StartCoroutine(this.goToEatBodyRoutine());
							yield break;
						}
					}
					else if (this.setup.worldSearch.findCloseGroupEncounter())
					{
						this.toDisableSearchEvent();
						yield break;
					}
				}
				else if (this.ai.mainPlayerDist > 450f && !this.setup.typeSetup.inCave)
				{
					base.StartCoroutine(this.search.findPointInFrontOfPlayer(330f));
				}
				timer = Time.time + 20f;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this._toAttractArtifact)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToArtifactRoutine(true));
				this._toAttractArtifact = false;
				yield break;
			}
			if (this._toRepelArtifact)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToArtifactRoutine(false));
				this._toRepelArtifact = false;
				yield break;
			}
			if (!this._validPath)
			{
				break;
			}
			yield return null;
		}
		this.animator.SetFloat("playerAngle", (float)UnityEngine.Random.Range(-180, 180));
		int r = UnityEngine.Random.Range(0, 3);
		base.StartCoroutine(this.setup.animControl.smoothChangeIdle((float)r));
		this.ai.StartCoroutine("toStop");
		if (this.ai.mainPlayerDist < 250f && !this.setup.typeSetup.inCave && UnityEngine.Random.value < this.setup.aiManager.fsmSearchToStructure.Value)
		{
			base.StartCoroutine(this.search.findCloseStructure());
		}
		yield return YieldPresets.WaitOneSecond;
		if (this._toStructure)
		{
			if (this.fsmBrainFiremanBool.Value)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToBurnStructureRoutine());
			}
			else
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToStructureRoutine());
			}
			this._toStructure = false;
			yield break;
		}
		float timer2 = Time.time + UnityEngine.Random.Range(this.setup.aiManager.FsmSearchWaitMin.Value, this.setup.aiManager.FsmSearchWaitMax.Value);
		while (Time.time < timer2)
		{
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		if (!this.search.playerAware)
		{
			this.setup.dayCycle.disableSleepOverride();
		}
		this.animator.SetBool("encounterBOOL", false);
		if (!this.setup.typeSetup.inCave)
		{
			switch (weightsRandomizer.From<int>(new Dictionary<int, int>
			{
				{
					0,
					Mathf.FloorToInt(this.setup.aiManager.fsmSearchToSearch.Value * 1000f)
				},
				{
					1,
					Mathf.FloorToInt(this.setup.aiManager.fsmSearchToEncounter.Value * 1000f)
				},
				{
					2,
					Mathf.FloorToInt(this.setup.aiManager.fsmSearchToCall.Value * 1000f)
				},
				{
					3,
					Mathf.FloorToInt(this.setup.aiManager.fsmSearchToPlaceArt.Value * 1000f)
				}
			}).TakeOne())
			{
			case 0:
				this.ActiveRoutine = base.StartCoroutine(this.goToLongSearchRoutine());
				break;
			case 1:
				if (this.ai.mainPlayerDist < 200f && !this._encounterCoolDown)
				{
					if (this.fsmBrainFemaleSkinnyBool.Value)
					{
						if (this._eatBody)
						{
							this.ActiveRoutine = base.StartCoroutine(this.goToEatBodyRoutine());
							yield break;
						}
					}
					else if (this.setup.worldSearch.findCloseGroupEncounter())
					{
						this.toDisableSearchEvent();
						yield break;
					}
				}
				this.ActiveRoutine = base.StartCoroutine(this.goToLongSearchRoutine());
				break;
			case 2:
				this.animator.SetInteger("randInt1", UnityEngine.Random.Range(2, 4));
				this.animator.SetBool("callBool", true);
				yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));
				this.animator.SetBool("callBool", false);
				this.ActiveRoutine = base.StartCoroutine(this.goToLongSearchRoutine());
				break;
			case 3:
				this.ActiveRoutine = base.StartCoroutine(this.goToPlaceArtRoutine());
				break;
			}
			yield return null;
			yield break;
		}
		if (this.search.playerAware || this.ai.playerDist < 30f)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToLongSearchRoutine());
			yield break;
		}
		this.setup.dayCycle.sleepBlocker = false;
		this.setup.pmSleep.enabled = true;
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		this.setup.pmSleep.SendEvent("goToSleep");
		yield break;
	}

	
	private IEnumerator goToTrapTriggerRoutine()
	{
		this.ai.StartCoroutine("toStop");
		this.ai.enablePathSearch();
		yield return YieldPresets.WaitPointTwoSeconds;
		this.animator.SetBool("stalkingBOOL", false);
		this.animator.SetBool("crouchBOOL", false);
		this.search.StartCoroutine("toDisableVis");
		this.ai.StartCoroutine("toRun");
		this._validPath = true;
		float timer = Time.time + 10f;
		while (this.ai.targetDist > 8f)
		{
			if (this._structureGo == null || !this._structureGo.activeSelf || Time.time > timer || !this._validPath)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			yield return null;
		}
		this.animator.SetBool("attackBOOL", true);
		this.ai.StartCoroutine("toStop");
		timer = Time.time + 2f;
		while (Time.time < timer)
		{
			if (this._structureGo == null || !this._structureGo.activeSelf)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			this.doSmoothLookAt(this._structureGo.transform.position, 3f);
			yield return null;
		}
		this.animator.SetBool("attackBOOL", false);
		this.search.StartCoroutine("toLook");
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToArtifactRoutine(bool attract)
	{
		if (attract)
		{
			this.search.updateCurrentWaypoint(this._lastArtifactPos);
			this._doingAttract = true;
			this._doingRepel = false;
		}
		else
		{
			base.StartCoroutine(this.search.findPointAwayFromArtifact());
			this._doingRepel = true;
			this._doingAttract = false;
		}
		this.search.setToWaypoint();
		this.ai.StartCoroutine("toStop");
		this.animator.SetBool("ritualBOOL", false);
		this.animator.SetBool("encounterBOOL", false);
		this.animator.SetBool("crouchBOOL", false);
		this.ai.StartCoroutine("toRun");
		this._toRepelArtifact = false;
		this._toAttractArtifact = false;
		this._targetFound = false;
		this._toPlayerNoise = false;
		this._validPath = true;
		this.setup.lastSighting.transform.position = this.search.currentWaypoint.transform.position;
		float timer = Time.time + 30f;
		while (this.ai.targetDist > 20f)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (!this._validPath)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._toAttractArtifact && !this._doingAttract)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToArtifactRoutine(true));
				yield break;
			}
			if (this._toRepelArtifact && !this._doingRepel)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToArtifactRoutine(false));
				yield break;
			}
			yield return null;
		}
		this.setup.lastSighting.transform.position = this.search.currentWaypoint.transform.position;
		this.ai.StartCoroutine("toStop");
		this.animator.SetBool("attackBOOL", false);
		this.animControl.StartCoroutine(this.animControl.smoothChangeIdle(3f));
		timer = Time.time + UnityEngine.Random.Range(2f, 5f);
		while (Time.time < timer)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToNoiseRoutine()
	{
		this.search.setToLastSighting();
		this.animator.SetBool("soundBool1", true);
		this.search.StartCoroutine("toLook");
		this.ai.StartCoroutine("toStop");
		this.search.disableSearchTimeout();
		this.search.StartCoroutine("enableSearchTimeout");
		this.animator.SetBool("ritualBOOL", false);
		this.animator.SetBool("encounterBOOL", false);
		this.animator.SetBool("crouchBOOL", false);
		if (this._runToNoise)
		{
			this.ai.StartCoroutine("toRun");
			this._runToNoise = false;
		}
		else
		{
			yield return YieldPresets.WaitTwoSeconds;
			yield return YieldPresets.WaitPointFiveSeconds;
		}
		this.ai.StartCoroutine("toWalk");
		this.animator.SetBool("soundBool1", false);
		this._targetFound = false;
		this._toPlayerNoise = false;
		this._validPath = true;
		float timer = Time.time + 10f;
		while (this.ai.targetDist > 15f)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (!this._validPath)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this.setup.pmCombatScript.toRescue)
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.toDisableSearchEvent();
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		this.animator.SetBool("attackBOOL", false);
		this.animControl.StartCoroutine(this.animControl.smoothChangeIdle(3f));
		timer = Time.time + UnityEngine.Random.Range(2f, 5f);
		while (Time.time < timer)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this.setup.pmCombatScript.toRescue)
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
				this.toDisableSearchEvent();
				yield break;
			}
			yield return null;
		}
		if (this.search.playerAware)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToCloseBushRoutine());
		}
		else
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator goToPlaceArtRoutine()
	{
		if (this.ai.mainPlayerDist > 200f || this.search.playerAware)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
			yield break;
		}
		if (this.fsmBrainLeaderBool.Value)
		{
			this.setup.followerFunctions.sendArtEvent();
		}
		this.animator.SetBool("crouchBOOL", false);
		this.ai.StartCoroutine("toStop");
		this.search.StartCoroutine("toLook");
		this._toPlaceArt = false;
		this._doSearchAction = false;
		this._targetFound = false;
		this._toPlayerNoise = false;
		float timer = Time.time + 1f;
		while (!this._doSearchAction && Time.time < timer)
		{
			base.StartCoroutine(this.search.findRandomPoint(15f));
			yield return null;
		}
		if (!this._doSearchAction)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
			yield break;
		}
		this.ai.enablePathSearch();
		this.ai.StartCoroutine("toWalk");
		this._validPath = true;
		timer = Time.time + 10f;
		while (this.ai.targetDist > 7f)
		{
			if (!this._validPath || Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
				yield break;
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		this.setup.propManager.Invoke("spawnRandomArt", 3f);
		this.animator.SetInteger("randInt1", 3);
		this.animator.SetBool("ritualBOOL", true);
		timer = Time.time + 7f;
		while (Time.time < timer)
		{
			if (Time.time > timer - 4f)
			{
				this.animator.SetBool("ritualBOOL", false);
			}
			if (this._targetFound)
			{
				this.animator.SetBool("ritualBOOL", false);
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				this._targetFound = false;
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.animator.SetBool("ritualBOOL", false);
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			yield return null;
		}
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.9f));
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToBurnStructureRoutine()
	{
		float timer = Time.time;
		this.ai.resetCombatParams();
		this.ai.StartCoroutine("toStop");
		this._doAction = false;
		this._cancelEvent = false;
		this._noValidTarget = false;
		yield return YieldPresets.WaitPointFiveSeconds;
		if (this._noValidTarget || this._structureGo == null)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
			yield break;
		}
		this.animator.SetBool("stalkingBOOL", false);
		this.ai.StartCoroutine("toWalk");
		timer = Time.time + 15f;
		float dist = Vector3.Distance(this._structureGo.transform.position, this.tr.position);
		while (dist > 7f)
		{
			if (this._structureGo == null || Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			dist = Vector3.Distance(this._structureGo.transform.position, this.tr.position);
			if (dist < 9.5f)
			{
				this.doSmoothLookAt(this._structureGo.transform.position, 2f);
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
			if (this._structureGo == null)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._structureGo)
			{
				this.doSmoothLookAt(this._structureGo.transform.position, 2f);
			}
			yield return null;
		}
		this.animator.SetBool("attackBOOL", false);
		this.ActiveRoutine = base.StartCoroutine(this.goToRunAwayRoutine());
		yield break;
	}

	
	private IEnumerator goToStructureRoutine()
	{
		float timer = Time.time;
		this.animControl.resetAllActionBools();
		this.ai.StartCoroutine("toStop");
		this._doAction = false;
		this._doSearchAction = false;
		this._cancelEvent = false;
		this._noValidTarget = false;
		this._validPath = true;
		if (this.fsmBrainFemaleSkinnyBool.Value || UnityEngine.Random.value > 0.75f)
		{
			this.search.StartCoroutine(this.search.findCloseFoodStructure());
			yield return YieldPresets.WaitPointFiveSeconds;
			if (this._doSearchAction)
			{
				this._doFoodStructure = true;
			}
		}
		else
		{
			this.search.StartCoroutine(this.search.findCloseStructure());
			yield return YieldPresets.WaitPointFiveSeconds;
		}
		if (this._noValidTarget || !this._validPath)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
			yield break;
		}
		this.animator.SetBool("stalkingBOOL", false);
		this.search.StartCoroutine("toDisableVis");
		this.ai.StartCoroutine("toRun");
		timer = Time.time + 8f;
		while (this.ai.targetDist > 7f)
		{
			if (this._structureGo == null || Time.time > timer || !this._validPath)
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
			if (this._structureGo == null || Time.time > timer)
			{
				this.setup.pmCombat.SendEvent("toReset");
				yield break;
			}
			this.doSmoothLookAt(this._structureGo.transform.position, 3f);
			yield return null;
		}
		this._cancelEvent = false;
		float timeout = (float)UnityEngine.Random.Range(8, 20);
		if (this.fsmBrainFemaleSkinnyBool.Value)
		{
			timeout = 7f;
		}
		this._attackStructure = true;
		this.animator.SetBool("crouchBOOL", false);
		if (this._doFoodStructure)
		{
			timer = Time.time + 30f;
			this.animator.SetInteger("randInt1", 3);
			this.animator.SetBool("feedingBOOL", true);
			this.ai.StartCoroutine("toStop");
			this._doAction = false;
			this.search.StartCoroutine(this.search.stealFoodRoutine());
			while (Time.time < timer)
			{
				if (this._structureGo == null)
				{
					this.setup.pmCombat.SendEvent("toReset");
				}
				if (this._structureGo == null || this.ai.mainPlayerDist < 13f || this._cancelEvent)
				{
					this.animator.SetBool("feedingBOOL", false);
					this._doFoodStructure = false;
					this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
					yield break;
				}
				if (this._doAction)
				{
					break;
				}
				if (this._structureGo)
				{
					this.doSmoothLookAt(this._structureGo.transform.position, 2f);
				}
				yield return null;
			}
		}
		this.animator.SetBool("feedingBOOL", false);
		this._doFoodStructure = false;
		this._attackStructure = true;
		this.animator.SetInteger("randAttackInt1", UnityEngine.Random.Range(0, 3));
		this.animator.SetBool("attackBOOL", true);
		this.ai.StartCoroutine("toStop");
		timeout = Time.time + timeout;
		while (Time.time < timeout)
		{
			if (this._structureGo == null)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				this.animator.SetBool("attackBOOL", false);
				yield break;
			}
			if (this.ai.mainPlayerDist < 12f)
			{
				this._doFoodStructure = false;
				break;
			}
			if (this._structureGo)
			{
				this.doSmoothLookAt(this._structureGo.transform.position, 2f);
			}
			yield return null;
		}
		this.ActiveRoutine = base.StartCoroutine(this.goToRunAwayRoutine());
		yield break;
	}

	
	private IEnumerator goToPatrolRoutine()
	{
		if (!this.search.findCloseCaveWayPoint())
		{
			this._doPatrol = false;
			this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
			yield break;
		}
		this.animControl.enablePatrolAnimSpeed();
		this.search.setWayPointParams();
		this.search.setToWaypoint();
		this._toPlayerNoise = false;
		this._validPath = true;
		yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 3.5f));
		this.ai.StartCoroutine("toWalk");
		this.search.StartCoroutine("toLook");
		float timer = Time.time + 20f;
		while (this.ai.targetDist > 4f)
		{
			if (Time.time > timer)
			{
				this.animControl.disablePatrolAnimSpeed();
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.animControl.disablePatrolAnimSpeed();
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				yield break;
			}
			yield return null;
		}
		if (!this.search.getNextWayPoint())
		{
			this._doPatrol = false;
			this.animControl.disablePatrolAnimSpeed();
			this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
			yield break;
		}
		this.search.setToWaypoint();
		yield return YieldPresets.WaitPointThreeSeconds;
		float waitTimer = 0f;
		while (!this._toPlayerNoise)
		{
			if (!this._validPath)
			{
				this.animControl.disablePatrolAnimSpeed();
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this.ai.targetDist > 4f && Time.time > waitTimer)
			{
				if (!this.ai.startedMove)
				{
					this.ai.StartCoroutine("toWalk");
				}
			}
			else if (this.ai.startedMove)
			{
				if (!this.search.getNextWayPoint())
				{
					this._doPatrol = false;
					this.animControl.disablePatrolAnimSpeed();
					this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
					yield break;
				}
				this.search.setToWaypoint();
				if (this.search.wpSetup.stopAtWaypoint)
				{
					waitTimer = Time.time + UnityEngine.Random.Range(this.search.wpSetup.minWaitTime, this.search.wpSetup.maxWaitTime);
					this.ai.StartCoroutine("toStop");
				}
				yield return YieldPresets.WaitPointTwoSeconds;
			}
			yield return null;
		}
		this.animControl.disablePatrolAnimSpeed();
		this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
		yield break;
		yield break;
	}

	
	private IEnumerator goToRunAwayRoutine()
	{
		this.setup.animControl.resetAllActionBools();
		this.setup.search.StartCoroutine(this.setup.search.findPointAwayFromPlayer(20f));
		yield return null;
		float timer = Time.time + 1f;
		while (!this._doSearchAction)
		{
			if (Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToRandomPointRoutine());
				yield break;
			}
			yield return null;
		}
		this.search.setToWaypoint();
		this.setup.ai.StartCoroutine("toRun");
		yield return null;
		this._validPath = true;
		this._toPlayerNoise = false;
		this._targetFound = false;
		timer = Time.time + 8f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer || !this._validPath)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				yield break;
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield break;
	}

	
	private IEnumerator goToRandomPointRoutine()
	{
		this.animControl.resetAllActionBools();
		this.setup.ai.StartCoroutine("toStop");
		yield return null;
		this._doSearchAction = false;
		this.search.StartCoroutine(this.search.findRandomPoint(60f));
		float timer = Time.time + 1f;
		while (!this._doSearchAction)
		{
			if (Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			yield return null;
		}
		this.setup.ai.StartCoroutine("toRun");
		this._validPath = true;
		this._targetFound = false;
		this._toPlayerNoise = false;
		yield return null;
		timer = Time.time + 10f;
		while (this.setup.ai.targetDist > this.waypointArriveDist)
		{
			if (Time.time > timer)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			if (this._toPlayerNoise)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToNoiseRoutine());
				this._toPlayerNoise = false;
				yield break;
			}
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				yield break;
			}
			yield return null;
		}
		yield return YieldPresets.WaitPointZeroFiveSeconds;
		this.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitOneSecond;
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToEatBodyRoutine()
	{
		float timer = Time.time;
		float bodyDist = float.PositiveInfinity;
		if (this._currentMemberGo)
		{
			bodyDist = Vector3.Distance(this._currentMemberGo.transform.position, this.tr.position);
		}
		if (bodyDist < 12f || bodyDist > 90f || this.animator.GetBool("deathBOOL") || this.animator.GetBool("treeBOOL") || this._currentMemberGo == null)
		{
			this._eatBody = false;
			this.fsmBrainEatBodyBool.Value = false;
			this.setup.pmCombat.SendEvent("toReset");
			yield break;
		}
		this._doAction = false;
		this._cancelEvent = false;
		this._validPath = false;
		this._noValidTarget = false;
		this.search.setToBody();
		this.setup.familyFunctions.setOccupied();
		this.ai.StartCoroutine("toRun");
		this.search.StartCoroutine("toDisableVis");
		yield return YieldPresets.WaitPointFiveSeconds;
		float currDist = float.PositiveInfinity;
		float t = Time.time + 5f;
		while (currDist > 13.4f && !this._cancelEvent)
		{
			if (this._currentMemberGo == null || !this._currentMemberGo.activeSelf || Time.time > t || !this._validPath)
			{
				this._cancelEvent = true;
			}
			this.search.setToBody();
			currDist = Vector3.Distance(this.tr.position, this._currentMemberGo.transform.position);
			this.animator.SetBool("jumpBlockBool", true);
			yield return null;
		}
		if (!this._cancelEvent)
		{
			this.search.StartCoroutine(this.search.findEatingPos(this._currentMemberGo.transform.position));
			this.setup.enemyEvents.disableCollision();
			this.animator.SetBool("feedingBOOL", true);
			this.animator.SetBool("screamBOOL", false);
			this.ai.StartCoroutine("toStop");
			yield return null;
			while (this.animControl.fullBodyState.tagHash != this.animControl.onRockHash && !this._cancelEvent)
			{
				if (this._currentMemberGo == null || !this._currentMemberGo.activeSelf)
				{
					this._cancelEvent = true;
				}
				if (this._currentMemberGo)
				{
					this.doSmoothLookAt(this._currentMemberGo.transform.position, 2f);
				}
				yield return null;
			}
			this.search.setToPlayer();
			MatchTargetWeightMask _weightMask = new MatchTargetWeightMask(new Vector3(1f, 0f, 1f), 0f);
			t = Time.time + 5f;
			while (this.setup.animControl.fullBodyState.tagHash != this.setup.hashs.attackTag && !this._cancelEvent)
			{
				if (Time.time > t)
				{
					this._cancelEvent = true;
				}
				if (!this.animator.IsInTransition(0))
				{
					this.animator.MatchTarget(this._attachPos, Quaternion.identity, AvatarTarget.LeftFoot, _weightMask, 0.2f, 0.8f);
				}
				yield return null;
			}
			this.ai.StartCoroutine("toStop");
			this.search.StartCoroutine("toLook");
			this._toPlayerNoise = false;
			t = Time.time + (float)UnityEngine.Random.Range(10, 25);
			while (Time.time < t && !this._cancelEvent)
			{
				if (this._currentMemberGo == null || !this._currentMemberGo.activeSelf || !this.fsmBrainEatBodyBool.Value || Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.tr.position) < 15f || this._toPlayerNoise)
				{
					this._cancelEvent = true;
				}
				if (this._currentMemberGo)
				{
					this.doSmoothLookAt(this._currentMemberGo.transform.position, 2f);
				}
				yield return null;
			}
		}
		if (this._currentMemberGo && !this._cancelEvent)
		{
			try
			{
				this.setup.worldSearch.StartCoroutine(this.setup.worldSearch.gibCurrentMutant(this._currentMemberGo));
			}
			catch
			{
			}
		}
		this.animator.SetInteger("randInt1", UnityEngine.Random.Range(0, 3));
		this.setup.familyFunctions.resetFamilyParams();
		this.fsmBrainEatBodyBool.Value = false;
		this.search.StartCoroutine("toLook");
		this.fsmBrainEatBodyBool.Value = false;
		this.ai.StartCoroutine("toStop");
		this.animator.SetBool("feedingBOOL", false);
		this.animator.SetBool("jumpBlockBool", false);
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToTargetFoundRoutine()
	{
		if (!this._searchActive)
		{
			this.toDisableSearchEvent();
		}
		this.resetSearchparams();
		this.setup.familyFunctions.resetFamilyParams();
		this._eatBody = false;
		this._cautiousBool = false;
		this._searchActive = false;
		this.animator.SetBool("feedingBOOL", false);
		this.animator.SetBool("jumpBlockBool", false);
		this.animator.SetBool("sleepBOOL", false);
		this.animator.SetBool("soundBool1", false);
		this.animator.SetBool("searchingBOOL", false);
		this.animator.SetBool("idleWaryBOOL", false);
		this.animator.SetBool("treeBOOL", false);
		this.fsmBrainEatBodyBool.Value = false;
		base.StartCoroutine(this.animControl.smoothChangeIdle(1f));
		this.search.disableSearchTimeout();
		this.animControl.disablePatrolAnimSpeed();
		this.search.updateCurrentTarget();
		this.ai.StartCoroutine("toStop");
		this.search.playSearchScream();
		this._targetSpotted = true;
		this.setup.pmBrain.SendEvent("toActivateFSM");
		float timer = 0f;
		float val = this.animator.GetFloat("walkTypeFloat1");
		while (timer < 1f)
		{
			val = Mathf.Lerp(val, 0f, timer);
			this.animator.SetFloat("walkTypeFloat1", val);
			timer += Time.deltaTime;
			this.doSmoothLookAt(this.ai.target.position, 2f);
			yield return null;
		}
		timer = Time.time + 3f;
		while (this.animControl.fullBodyState.tagHash != this.animControl.idlehash)
		{
			if (Time.time > timer)
			{
				break;
			}
			yield return null;
		}
		this.animator.SetBool("screamBOOL", false);
		this.animator.SetBool("sleepBOOL", false);
		this.setup.pmBrain.FsmVariables.GetFsmBool("sleepBool").Value = false;
		this.setup.familyFunctions.sendTargetSpotted();
		this.toDisableSearchEvent();
		yield return null;
		yield break;
	}

	
	private IEnumerator goToTargetSpottedRoutine()
	{
		this.resetSearchparams();
		this.ai.StartCoroutine("toStop");
		float timer;
		if (this._targetSpotted)
		{
			timer = Time.time + 3f;
			while (this.animControl.fullBodyState.tagHash != this.animControl.idlehash)
			{
				if (Time.time > timer)
				{
					break;
				}
				yield return null;
			}
			this.animator.SetBool("screamBOOL", false);
			this.animator.SetBool("sleepBOOL", false);
			this.setup.pmBrain.FsmVariables.GetFsmBool("sleepBool").Value = false;
			this.setup.familyFunctions.sendTargetSpotted();
			this.setup.pmBrain.SendEvent("toActivateFSM");
			this.toDisableSearchEvent();
			yield break;
		}
		this.animControl.resetAllActionBools();
		this.ai.StartCoroutine("toWalk");
		this._validPath = true;
		this._targetFound = false;
		timer = Time.time + 10f;
		while (this.ai.targetDist > 15f)
		{
			if (this._targetFound)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
				yield break;
			}
			if (Time.time > timer || !this._validPath)
			{
				this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
				yield break;
			}
			yield return null;
		}
		this.ai.StartCoroutine("toStop");
		yield return YieldPresets.WaitOneSecond;
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
		yield return null;
		yield break;
	}

	
	private IEnumerator goToResetRoutine()
	{
		this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
		yield return null;
		yield break;
	}

	
	public IEnumerator gotHitRoutine()
	{
		this.animator.SetBool("idleWaryBOOL", false);
		this.animControl.disablePatrolAnimSpeed();
		this.setup.familyFunctions.sendAggressive();
		this.setup.pmBrain.SendEvent("toSetAggressive");
		this.toDisableSearchEvent();
		yield return null;
		yield break;
	}

	
	public IEnumerator goToActivateRoutine()
	{
		if (this.setup.pmBrain && this.setup.pmBrain.FsmVariables.GetFsmBool("targetSeenBool").Value)
		{
			this.setup.pmBrain.SendEvent("toActivateFSM");
			this.toDisableSearchEvent();
			yield break;
		}
		if (!this._searchActive)
		{
			this.ActiveRoutine = base.StartCoroutine(this.goToStartSearchRoutine());
		}
		yield return null;
		yield break;
	}

	
	public void toResetSearchEvent()
	{
		if (!this._searchActive)
		{
			return;
		}
		base.StopAllCoroutines();
		this.ActiveRoutine = base.StartCoroutine(this.goToResetRoutine());
	}

	
	public void toDisableSearchEvent()
	{
		if (!this.animator)
		{
			this.animator = base.transform.GetComponent<Animator>();
		}
		base.StopAllCoroutines();
		this.resetEncounterAnimParams();
		this.animator.SetFloat("walkTypeFloat1", 0f);
		this.animator.SetBool("idleWaryBOOL", false);
		this.animator.SetBool("callBool", false);
		this.animator.SetBool("treeBOOL", false);
		this.animator.SetBool("onRockBOOL", false);
		this.animator.SetBool("screamBOOL", false);
		this.animator.SetBool("searchingBOOL", false);
		this.animator.SetBool("soundBool1", false);
		if (this.search)
		{
			this.search.resetTargetOffset();
		}
		this._longSearchBool = false;
		this._toPlayerNoise = false;
		this._toPlaceArt = false;
		this._toStructure = false;
		this._doSearchAction = false;
		this._eatBody = false;
		this._cautiousBool = false;
		this._CloseSearchResetBool = false;
		this._targetFound = false;
		this._doAction = false;
		this._runToNoise = false;
		this._searchActive = false;
	}

	
	private void resetSearchparams()
	{
		this._longSearchBool = false;
		this._toPlayerNoise = false;
		this._toPlaceArt = false;
		this._toStructure = false;
		this._doSearchAction = false;
		this._eatBody = false;
		this._cautiousBool = false;
		this._CloseSearchResetBool = false;
		this._doAction = false;
		this._runToNoise = false;
	}

	
	public void toTargetSpottedEvent()
	{
		base.StopAllCoroutines();
		this.ActiveRoutine = base.StartCoroutine(this.goToTargetSpottedRoutine());
	}

	
	public void toTargetFoundEvent()
	{
		base.StopAllCoroutines();
		this.ActiveRoutine = base.StartCoroutine(this.goToTargetFoundRoutine());
	}

	
	public void toActivateCloseSearchEvent()
	{
		base.StopAllCoroutines();
		this.ActiveRoutine = base.StartCoroutine(this.goToCloseSearchRoutine());
	}

	
	private void resetEncounterAnimParams()
	{
		this.animator.SetBool("ritualBOOL", false);
		this.animator.SetBool("dodgeBOOL", false);
		this.animator.SetBool("sleepBOOL", false);
		this.animator.SetBool("feedingBOOL", false);
		this.animator.SetBool("backAwayBOOL", false);
		this.animator.SetBool("callBool", false);
	}

	
	public void doSmoothLookAt(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = this.tr.position.y;
		Vector3 vector = lookAtPos - this.tr.position;
		Quaternion quaternion = this.tr.rotation;
		Quaternion b = quaternion;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			b = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, b, speed * Time.deltaTime);
		this.tr.rotation = quaternion;
	}

	
	private void doSmoothLookAtDir(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = this.tr.position.y;
		Vector3 vector = lookAtPos - this.tr.position;
		Quaternion quaternion = this.tr.rotation;
		Quaternion b = quaternion;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			b = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, b, speed * Time.deltaTime);
		this.tr.rotation = quaternion;
	}

	
	private mutantScriptSetup setup;

	
	private mutantSearchFunctions search;

	
	private mutantAnimatorControl animControl;

	
	private mutantMaleHashId hashId;

	
	private mutantAI ai;

	
	private Animator animator;

	
	private Transform tr;

	
	private Transform rootTr;

	
	public Coroutine ActiveRoutine;

	
	public GameObject _currentMemberGo;

	
	public GameObject _structureGo;

	
	public bool _longSearchBool;

	
	public bool _toPlayerNoise;

	
	public bool _toPlaceArt;

	
	public bool _toStructure;

	
	public bool _doPatrol;

	
	public bool _doSearchAction;

	
	public bool _validPath;

	
	public bool _eatBody;

	
	public bool _encounterCoolDown;

	
	public bool _cautiousBool;

	
	public bool _CloseSearchResetBool;

	
	public bool _targetFound;

	
	public bool _doAction;

	
	public bool _runToNoise;

	
	public bool _cancelEvent;

	
	public bool _noValidTarget;

	
	public bool _searchActive;

	
	public bool _targetSpotted;

	
	public bool _doFoodStructure;

	
	public bool _attackStructure;

	
	public bool _toAttractArtifact;

	
	public bool _toRepelArtifact;

	
	public bool _doingAttract;

	
	public bool _doingRepel;

	
	public bool fsmBrainTargetSeenBool;

	
	public float waypointArriveDist = 13f;

	
	public Vector3 _attachPos;

	
	public float _targetStopDist;

	
	public Vector3 _lastArtifactPos;

	
	public Transform _lastArtifactTr;

	
	private float resetSearchTimer;

	
	private Vector3 lastResetPos;

	
	private FsmGameObject fsmBrainLeaderGo;

	
	private FsmBool fsmBrainFiremanBool;

	
	private FsmBool fsmBrainFemaleSkinnyBool;

	
	private FsmBool fsmBrainLeaderBool;

	
	private FsmBool fsmBrainEatBodyBool;
}
