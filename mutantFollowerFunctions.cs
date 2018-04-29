using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class mutantFollowerFunctions : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.GetComponentInChildren<Animator>();
		this.setup = base.GetComponentInChildren<mutantScriptSetup>();
		this.worldFunctions = base.GetComponentInChildren<mutantWorldSearchFunctions>();
		this.fsmDeathBool = this.setup.pmCombat.FsmVariables.GetFsmBool("deathBool");
	}

	
	private void sendFollowerEvent(string e)
	{
		for (int i = 0; i < this.followersList.Count; i++)
		{
			if (this.followersList[i])
			{
				this.followersList[i].SendMessage(e, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private void sendToLeaderEvent()
	{
		this.sendFollowerEvent("switchToGoToLeader");
	}

	
	public void sendArtEvent()
	{
		this.sendFollowerEvent("switchToArt");
	}

	
	public void sendSearchEvent()
	{
		this.sendFollowerEvent("switchToSearch");
	}

	
	public void sendCloseSearchEvent()
	{
		this.sendFollowerEvent("switchToCloseSearch");
	}

	
	public void sendStalkEvent()
	{
		this.sendFollowerEvent("switchToStalk");
	}

	
	private void sendEncounterEvent()
	{
		for (int i = 0; i < this.followersList.Count; i++)
		{
			GameObject encounterGo = this.worldFunctions.encounterGo;
			if (this.followersList[i])
			{
				this.followersList[i].SendMessage("switchToEncounter", encounterGo, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void sendResetEvent()
	{
		this.sendFollowerEvent("resetEncounter");
	}

	
	public void sendAttackEvent()
	{
		this.sendFollowerEvent("switchToAttack");
	}

	
	public void sendTreeStalkEvent()
	{
		this.sendFollowerEvent("switchToTreeStalk");
	}

	
	public void sendTreeDownEvent()
	{
		this.sendFollowerEvent("switchToTreeDown");
	}

	
	public void sendAlertEvent()
	{
		this.sendFollowerEvent("switchToAlerted");
	}

	
	public void sendSleepEvent()
	{
		this.sendFollowerEvent("switchToSleep");
	}

	
	private void switchToGoToLeader()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("doGoToLeader").Value = true;
		base.Invoke("resetGoToLeader", 10f);
	}

	
	private void resetGoToLeader()
	{
		this.setup.pmCombat.FsmVariables.GetFsmBool("doGoToLeader").Value = false;
	}

	
	private void switchToArt()
	{
		if (this.setup.pmSearchScript._searchActive)
		{
			this.setup.pmSearchScript._toPlaceArt = true;
		}
	}

	
	private void switchToSearch()
	{
		if (!this.animator.enabled)
		{
			base.StartCoroutine(this.setup.pmSearchScript.goToActivateRoutine());
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			base.StartCoroutine(this.setup.pmSearchScript.goToActivateRoutine());
			this.animator.SetBoolReflected("treeBOOL", false);
			this.animator.SetBoolReflected("onRockBOOL", false);
			this.animator.SetIntegerReflected("randInt1", 0);
			this.animator.SetBoolReflected("screamBOOL", false);
		}
	}

	
	private void switchToCloseSearch()
	{
		if (!this.animator.enabled)
		{
			if (!this.setup.pmCombat.enabled && !this.setup.pmSleep.enabled)
			{
				this.setup.pmSearchScript.toActivateCloseSearchEvent();
			}
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire && !this.setup.pmCombat.enabled && !this.setup.pmSleep.enabled)
		{
			this.setup.pmSearchScript.toActivateCloseSearchEvent();
		}
	}

	
	private void switchToStalk()
	{
		if (!this.animator.enabled)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmBrain.SendEvent("toSetPassive");
			this.setup.ai.forceTreeDown();
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmBrain.SendEvent("toSetPassive");
			this.setup.ai.forceTreeDown();
			this.animator.SetBoolReflected("onRockBOOL", false);
			this.animator.SetBoolReflected("screamBOOL", false);
		}
	}

	
	private void switchToEncounter(GameObject encounter)
	{
		if (!this.setup.pmCombat.enabled)
		{
			this.worldFunctions.getEncounterComponent(encounter);
			this.worldFunctions.setEncounterType();
			this.worldFunctions.encounterGo = encounter;
			this.setup.pmEncounter.FsmVariables.GetFsmGameObject("encounterGo").Value = encounter;
			this.setup.ai.forceTreeDown();
		}
	}

	
	private void switchToAttack()
	{
		if (!this.animator.enabled)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmBrain.SendEvent("toSetAggressive");
			this.setup.ai.forceTreeDown();
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.setup.pmCombat.enabled = true;
			this.setup.pmBrain.SendEvent("toSetAggressive");
			this.setup.ai.forceTreeDown();
		}
	}

	
	private void switchToTreeStalk()
	{
		if (!this.animator.enabled)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("stalking").Value = true;
			this.setup.pmCombat.SendEvent("goToTree");
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.setup.pmCombat.FsmVariables.GetFsmBool("stalking").Value = true;
			this.setup.pmCombat.SendEvent("goToTree");
		}
	}

	
	private void switchToTreeDown()
	{
		this.setup.ai.forceTreeDown();
	}

	
	private void switchToAlerted()
	{
		if (!LocalPlayer.PlayerBase)
		{
			return;
		}
		if (!this.animator.enabled)
		{
			return;
		}
		if (!this.animator.GetBool("trapBool") && !this.animator.GetBool("deathBOOL") && !this.fsmDeathBool.Value && !this.setup.health.onFire)
		{
			this.setup.lastSighting.transform.position = LocalPlayer.PlayerBase.transform.position;
			if (!this.setup.pmCombat.enabled)
			{
				if (this.setup.pmSearchScript)
				{
					this.setup.pmSearchScript._runToNoise = true;
					this.setup.pmSearchScript._toPlayerNoise = true;
				}
				if (this.setup.pmSleep)
				{
					this.setup.pmSleep.SendEvent("toNoise");
				}
				if (this.setup.pmEncounter)
				{
					this.setup.pmEncounter.SendEvent("toNoise");
				}
			}
		}
	}

	
	private void switchToSleep()
	{
		if (!this.animator.GetBool("trapBool") && this.setup.pmSleep)
		{
			this.setup.ai.forceTreeDown();
			this.setup.pmBrain.FsmVariables.GetFsmBool("fearOverrideBool").Value = true;
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript.toDisableSearchEvent();
			}
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("toDisableFSM");
			}
			this.setup.pmBrain.SendEvent("toSetSleep");
		}
	}

	
	private void resetEncounter()
	{
		this.setup.pmEncounter.SendEvent("toResetEncounter");
	}

	
	public List<GameObject> followersList = new List<GameObject>();

	
	private mutantScriptSetup setup;

	
	private mutantWorldSearchFunctions worldFunctions;

	
	private Animator animator;

	
	private FsmBool fsmDeathBool;
}
