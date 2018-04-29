using System;
using System.Collections;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class mutantFamilyFunctions : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.GetComponentInChildren<mutantScriptSetup>();
		this.spawnSetup = base.transform.root.GetComponent<mutantTypeSetup>();
		this.ai = base.GetComponentInChildren<mutantAI>();
		this.search = base.GetComponentInChildren<mutantSearchFunctions>();
		this.animator = base.GetComponentInChildren<Animator>();
		if (this.dragPoint)
		{
			this.dragPointInit = this.dragPoint.transform.localPosition;
		}
		this.mutantControl = Scene.MutantControler;
		this.health = base.GetComponentInChildren<EnemyHealth>();
		this.fsmDeathBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool");
	}

	
	private void OnDisable()
	{
		if (this.setup)
		{
			this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool").Value = false;
		}
		this.timeout1 = false;
		base.StopAllCoroutines();
	}

	
	private void startEatMeEvent()
	{
		if (base.gameObject.activeSelf)
		{
			base.StartCoroutine("sendEatMeEvent");
		}
	}

	
	private void startRescueEvent()
	{
		if (base.gameObject.activeSelf)
		{
			base.StartCoroutine("sendRescueEvent");
		}
	}

	
	public void cancelRescueEvent()
	{
		base.CancelInvoke("startRescueEvent");
		base.StopCoroutine("sendRescueEvent");
		this.dying = false;
		if (this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject)
				{
					gameObject.SendMessage("resetRescueEvent", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public void cancelEatMeEvent()
	{
		base.CancelInvoke("startEatMeEvent");
		base.StopCoroutine("sendEatMeEvent");
		foreach (GameObject gameObject in this.mutantControl.activeSkinnyCannibals)
		{
			if (gameObject)
			{
				gameObject.SendMessage("cancelEatMe", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private void resetRescueEvent()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("rescueBool").Value = false;
	}

	
	public IEnumerator sendAmbushEvent()
	{
		if (!this.spawnSetup)
		{
			yield break;
		}
		if (this.spawnSetup.spawner)
		{
			foreach (GameObject go in this.spawnSetup.spawner.allMembers)
			{
				if (go)
				{
					go.SendMessage("switchToAmbush", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator sendRescueEvent()
	{
		if (!this.spawnSetup)
		{
			yield break;
		}
		this.dying = true;
		if (this.spawnSetup.spawner)
		{
			for (int i = this.spawnSetup.spawner.allMembers.Count - 1; i >= 0; i--)
			{
				if (i <= this.spawnSetup.spawner.allMembers.Count - 1)
				{
					GameObject go = this.spawnSetup.spawner.allMembers[i];
					if (go)
					{
						go.SendMessage("switchToRescueFriend", base.gameObject, SendMessageOptions.DontRequireReceiver);
					}
					yield return YieldPresets.WaitOnePointThreeSeconds;
					if (go)
					{
						go.SendMessage("switchToFreakout", base.gameObject, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator sendEatMeEvent()
	{
		if (Scene.SceneTracker.hasSwarmedByEnemies)
		{
			yield break;
		}
		foreach (GameObject go in this.mutantControl.activeSkinnyCannibals)
		{
			if ((go.transform.position - base.transform.position).sqrMagnitude < 40000f)
			{
				if (go)
				{
					go.SendMessage("switchToEatMe", base.gameObject, SendMessageOptions.DontRequireReceiver);
				}
				yield return YieldPresets.WaitForFixedUpdate;
			}
		}
		foreach (GameObject go2 in this.mutantControl.activeInstantSpawnedCannibals)
		{
			if ((go2.transform.position - base.transform.position).sqrMagnitude < 40000f)
			{
				if (go2)
				{
					go2.SendMessage("switchToEatMe", base.gameObject, SendMessageOptions.DontRequireReceiver);
				}
				yield return YieldPresets.WaitForFixedUpdate;
			}
		}
		yield return null;
		yield break;
	}

	
	public void sendAttackStructureEvent()
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (this.setup.search.fsmInCave.Value)
		{
			return;
		}
		if (this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject && gameObject.name != base.gameObject.name)
				{
					gameObject.SendMessage("switchToAttackStructure", this.setup.pmSearchScript._structureGo, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public void sendTargetSpotted()
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (!this.ai.maleSkinny && !this.ai.femaleSkinny && this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject && gameObject.name != base.gameObject.name)
				{
					gameObject.SendMessage("switchToTargetSpotted", this.ai.target, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public void sendAggressive()
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (!this.ai.maleSkinny && !this.ai.femaleSkinny && this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject)
				{
					gameObject.SendMessage("switchToCombat", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public void sendAggressiveCombat(float timeout)
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (!this.ai.maleSkinny && !this.ai.femaleSkinny && this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject)
				{
					gameObject.SendMessage("switchToAggressiveCombat", timeout, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public void sendFleeArea()
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (this.setup.search.fsmInCave.Value)
		{
			return;
		}
		if (this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject && gameObject.name != base.gameObject.name)
				{
					gameObject.SendMessage("switchToFleeArea", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public void sendAllFleeArea()
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (this.setup.search.fsmInCave.Value)
		{
			return;
		}
		if (UnityEngine.Random.value > 0.4f)
		{
			return;
		}
		foreach (GameObject gameObject in Scene.MutantControler.activeWorldCannibals)
		{
			if (gameObject && gameObject.name != base.gameObject.name)
			{
				gameObject.SendMessage("switchToFleeArea", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void sendWakeUp()
	{
		if (!this.spawnSetup)
		{
			return;
		}
		if (this.spawnSetup.spawner)
		{
			foreach (GameObject gameObject in this.spawnSetup.spawner.allMembers)
			{
				if (gameObject)
				{
					gameObject.SendMessage("switchToWakeUp", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	private void switchToAmbush()
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (!this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.animator.GetBool("trapBool") && !this.setup.health.onFire && !this.setup.pmCombat.FsmVariables.GetFsmBool("doingAmbush").Value)
		{
			this.setup.pmCombat.SendEvent("goToAmbush");
		}
	}

	
	private IEnumerator switchToRescueFriend(GameObject go)
	{
		if (go)
		{
			if (!this.busy)
			{
				this.targetFamilyFunctions = go.GetComponent<mutantFamilyFunctions>();
			}
			if (!this.targetFamilyFunctions)
			{
				yield break;
			}
			if (!this.busy && !this.ai.female && this.targetFamilyFunctions.dying && !this.ai.maleSkinny && !this.ai.femaleSkinny && !this.setup.health.onFire && go != base.gameObject)
			{
				this.currentMemberTarget = go;
				if (go.transform.GetChild(0))
				{
					this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = go.transform.GetChild(0).gameObject;
				}
				this.setup.pmCombat.SendEvent("toRescue1");
				this.setup.pmCombat.FsmVariables.GetFsmBool("rescueBool").Value = true;
			}
		}
		yield return null;
		yield break;
	}

	
	private void switchToGuardFriend(GameObject go)
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (!this.ai.maleSkinny && !this.ai.femaleSkinny && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			if (!this.busy)
			{
				this.targetFamilyFunctions = go.GetComponent<mutantFamilyFunctions>();
			}
			if (!this.targetFamilyFunctions)
			{
				return;
			}
			this.currentMemberTarget = go;
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = go.transform.GetChild(0).gameObject;
			if (this.targetFamilyFunctions.dying)
			{
				this.setup.pmCombat.SendEvent("goToGuard1");
			}
		}
	}

	
	private void switchToFreakout(GameObject go)
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (!this.busy && this.setup.ai.female && !this.freakOverride && !this.ai.maleSkinny && !this.ai.femaleSkinny && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.targetFamilyFunctions = go.GetComponent<mutantFamilyFunctions>();
			if (!this.targetFamilyFunctions)
			{
				return;
			}
			this.currentMemberTarget = go;
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = go.transform.GetChild(0).gameObject;
			this.setup.pmCombat.FsmVariables.GetFsmBool("freakoutBool").Value = true;
			this.freakOverride = true;
			base.Invoke("resetFreakOverride", 30f);
		}
	}

	
	private void switchToEatMe(GameObject go)
	{
		if (!this.busy && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			if (this.animator.enabled && this.animator.GetBool("deathBOOL"))
			{
				return;
			}
			if (this.setup.ai.femaleSkinny || this.setup.ai.maleSkinny)
			{
				this.currentMemberTarget = go;
				this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = go;
				if (this.setup.pmCombatScript)
				{
					this.setup.pmCombatScript.currentMemberGo = go;
				}
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._currentMemberGo = go;
				}
				this.setup.pmBrain.FsmVariables.GetFsmBool("eatBodyBool").Value = true;
				this.setup.pmCombat.FsmVariables.GetFsmBool("eatBodyBool").Value = true;
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._eatBody = true;
				}
			}
		}
	}

	
	private void cancelEatMe(GameObject go)
	{
		if ((this.setup.ai.femaleSkinny || this.setup.ai.maleSkinny) && this.currentMemberTarget == go)
		{
			this.setup.pmBrain.FsmVariables.GetFsmBool("eatBodyBool").Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmBool("eatBodyBool").Value = false;
			this.setup.pmCombat.FsmVariables.GetFsmGameObject("currentMemberGo").Value = null;
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript._eatBody = false;
				this.setup.pmSearchScript._currentMemberGo = null;
			}
			this.currentMemberTarget = null;
		}
	}

	
	private void switchToTargetSpotted(Transform target)
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (this.setup.pmSearchScript._searchActive && !this.animator.GetBool("trapBool"))
		{
			this.setup.pmSearchScript.toTargetSpottedEvent();
			this.search.updateCurrentWaypoint(target.position);
		}
	}

	
	public void switchToCombat()
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.setup.pmBrain.SendEvent("toSetAggressive");
			if (!this.timeout1)
			{
				this.setup.pmCombat.FsmVariables.GetFsmBool("fearBOOL").Value = false;
				this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool").Value = true;
				this.timeout1 = true;
				base.Invoke("resetFearOverride", 15f);
			}
		}
	}

	
	public void switchToAggressiveCombat(float timeout)
	{
		this.setup.aiManager.setAggressiveCombat();
		this.setup.aiManager.CancelInvoke("setDefaultCombat");
		this.setup.aiManager.Invoke("setDefaultCombat", timeout);
	}

	
	public void switchToOnStructureCombat(float timeout)
	{
		this.setup.aiManager.setOnStructureCombat();
		this.setup.aiManager.CancelInvoke("setDefaultCombat");
		this.setup.aiManager.Invoke("setDefaultCombat", timeout);
	}

	
	private void switchToFleeArea()
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (this.setup.search.fsmInCave.Value)
		{
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.setup.ai.forceTreeDown();
			this.setup.pmCombat.FsmVariables.GetFsmBool("skipActionBool").Value = true;
			this.setup.pmCombat.SendEvent("toTimeout");
		}
	}

	
	private void switchToRunAway(GameObject target)
	{
		this.setup.pmBrain.SendEvent("toSetRunAway");
		this.setup.pmBrain.FsmVariables.GetFsmGameObject("fearTargetGo").Value = target;
	}

	
	private void switchToWakeUp()
	{
		if (!this.animator.enabled)
		{
			return;
		}
		if (!this.animator.GetBool("trapBool"))
		{
			this.setup.pmSleep.SendEvent("toWakeUp");
		}
	}

	
	private void resetFreakOverride()
	{
		this.freakOverride = false;
	}

	
	private void resetEncounter()
	{
		this.setup.pmEncounter.SendEvent("toResetEncounter");
	}

	
	public void setOccupied()
	{
		if (this.targetFamilyFunctions)
		{
			this.targetFamilyFunctions.occupied = true;
		}
		this.busy = true;
	}

	
	private void setThisOccupied()
	{
		this.occupied = true;
		this.busy = true;
	}

	
	public void checkTargetOccupied()
	{
		if (!this.targetFamilyFunctions.occupied || !this.targetFamilyFunctions.dying)
		{
			this.setup.pmCombat.SendEvent("cancelEvent");
			this.setup.pmCombatScript.cancelEvent = true;
		}
	}

	
	public void checkIfOccupied()
	{
		if (!this.targetFamilyFunctions)
		{
			this.setup.pmCombatScript.cancelEvent = true;
		}
		if (this.targetFamilyFunctions.occupied)
		{
			this.setup.pmCombat.SendEvent("cancelEvent");
			this.setup.pmCombatScript.cancelEvent = true;
		}
	}

	
	public void resetFamilyParams()
	{
		this.busy = false;
		this.occupied = false;
		this.dying = false;
		this.currentMemberTarget = null;
		this.dragPoint.transform.localPosition = this.dragPointInit;
		this.animator.SetBoolReflected("rescueBool1", false);
	}

	
	public void sendCancelEvent()
	{
		if (this.targetFamilyFunctions)
		{
			this.targetFamilyFunctions.setup.pmCombat.SendEvent("cancelEvent");
			this.targetFamilyFunctions.setup.pmCombatScript.cancelEvent = true;
		}
	}

	
	public void resetTargetOccupied()
	{
		if (this.targetFamilyFunctions)
		{
			this.targetFamilyFunctions.occupied = false;
		}
	}

	
	private void resetFearOverride()
	{
		this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool").Value = false;
		this.timeout1 = false;
	}

	
	public void setupDragParams()
	{
		this.setup.pmCombat.FsmVariables.GetFsmGameObject("matchPosGo").Value = this.targetFamilyFunctions.dragPoint;
		this.targetFamilyFunctions.currentHelper = base.gameObject;
		this.dragPoint.transform.localPosition = new Vector3(0f, 0f, 1f);
		this.targetFamilyFunctions.setup.pmCombat.FsmVariables.GetFsmGameObject("matchPosGo").Value = this.dragPoint;
		this.targetFamilyFunctions.setup.pmCombat.FsmVariables.GetFsmGameObject("lookatGo").Value = this.setup.thisGo;
		this.setup.animator.speed = 1f;
		this.targetFamilyFunctions.setup.animator.speed = 1f;
	}

	
	private void explodeMutant()
	{
		this.health.Explosion(5f);
	}

	
	public void resetParent()
	{
		if (base.transform.parent != null)
		{
			base.transform.parent = null;
		}
	}

	
	private void setRunAwayFromPlayer()
	{
		this.setup.aiManager.setRunAwayOverride();
	}

	
	public mutantScriptSetup setup;

	
	public mutantFamilyFunctions targetFamilyFunctions;

	
	private mutantTypeSetup spawnSetup;

	
	private mutantAI ai;

	
	private mutantSearchFunctions search;

	
	private Animator animator;

	
	private mutantController mutantControl;

	
	private EnemyHealth health;

	
	public GameObject currentMemberTarget;

	
	public GameObject currentHelper;

	
	public GameObject dragPoint;

	
	public Vector3 dragPointInit;

	
	public bool occupied;

	
	public bool busy;

	
	public bool dying;

	
	private bool timeout1;

	
	private bool freakOverride;

	
	private FsmBool fsmDeathBool;
}
