using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class girlMutantAiManager : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.transform.GetComponent<mutantScriptSetup>();
		this.fsmBlockBabySpawn = this.setup.pmCombat.FsmVariables.GetFsmBool("fsmBlockBabySpawn");
		this.fsmSpawnBabyWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmSpawnBabyWeight");
		this.fsmChargeAttackWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmChargeAttackWeight");
		this.fsmLongAttackWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmLongAttackWeight");
		this.fsmStrafeWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmStrafeWeight");
		this.fsmWalkBackWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmWalkBackWeight");
		this.fsmWalkForwardWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmWalkForwardWeight");
		this.fsmToIdleWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmToIdleWeight");
		this.fsmRunForwardWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmRunForwardWeight");
		this.fsmDodgeWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmDodgeWeight");
		this.fsmGoHomeBool = this.setup.pmCombat.FsmVariables.GetFsmBool("fsmGoHomeBool");
		this.fsmSpinAttackWeight = this.setup.pmCombat.FsmVariables.GetFsmFloat("fsmSpinAttackWeight");
		base.InvokeRepeating("setAiParams", 1f, 1f);
		this.updateDelay = Time.time + 1f;
	}

	
	private void Update()
	{
		if (Time.time > this.updateDelay)
		{
			this.setAiParams();
			this.updateDelay = Time.time + 1f;
		}
	}

	
	private void setAiParams()
	{
		float closestPlayerDistanceFromPos = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(this.girlStartPos);
		if (closestPlayerDistanceFromPos > 310f)
		{
			this.fsmGoHomeBool.Value = true;
		}
		else
		{
			this.fsmGoHomeBool.Value = false;
		}
		int num = 0;
		if (Scene.SceneTracker.allPlayers.Count > 1)
		{
			for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (Scene.SceneTracker.allPlayers[i] && (Scene.SceneTracker.allPlayers[i].transform.position - base.transform.position).sqrMagnitude < 625f)
				{
					num++;
				}
			}
			if (num > 1)
			{
				this.fsmSpinAttackWeight.Value = 1f;
			}
			else
			{
				this.fsmSpinAttackWeight.Value = 0.03f;
			}
		}
		else
		{
			this.fsmSpinAttackWeight.Value = 0.03f;
		}
		if (this.setup.ai.mainPlayerDist > 35f)
		{
			this.fsmSpawnBabyWeight.Value = 6f;
			this.fsmChargeAttackWeight.Value = 0.5f;
			this.fsmLongAttackWeight.Value = 5f;
			this.fsmStrafeWeight.Value = 0f;
			this.fsmWalkBackWeight.Value = 0f;
			this.fsmWalkForwardWeight.Value = 2f;
			this.fsmToIdleWeight.Value = 0f;
			this.fsmRunForwardWeight.Value = 0f;
		}
		else if (this.setup.health.Health > this.setup.health.maxHealth / 2)
		{
			this.fsmSpawnBabyWeight.Value = 6f;
			this.fsmChargeAttackWeight.Value = 0.5f;
			this.fsmLongAttackWeight.Value = 4.5f;
			this.fsmStrafeWeight.Value = 1f;
			this.fsmWalkBackWeight.Value = 0.6f;
			this.fsmWalkForwardWeight.Value = 1f;
			this.fsmToIdleWeight.Value = 1f;
			this.fsmRunForwardWeight.Value = 0f;
		}
		else
		{
			this.fsmSpawnBabyWeight.Value = 6f;
			this.fsmChargeAttackWeight.Value = 1f;
			this.fsmLongAttackWeight.Value = 2f;
			this.fsmStrafeWeight.Value = 0.6f;
			this.fsmWalkBackWeight.Value = 0.2f;
			this.fsmWalkForwardWeight.Value = 1.5f;
			this.fsmToIdleWeight.Value = 1f;
			this.fsmRunForwardWeight.Value = 0f;
		}
		this.spawnedBabies.RemoveAll((GameObject o) => o == null);
		if (this.spawnedBabies.Count > 2)
		{
			this.fsmBlockBabySpawn.Value = true;
		}
		else
		{
			this.fsmBlockBabySpawn.Value = false;
		}
	}

	
	public void setInitialWeightParams()
	{
		this.fsmDodgeWeight.Value = 0f;
		base.Invoke("enableDodgeWeight", 15f);
	}

	
	private void enableDodgeWeight()
	{
		this.fsmDodgeWeight.Value = 0.25f;
	}

	
	public void setupHealthParams()
	{
		if (Scene.SceneTracker.allPlayers.Count > 1)
		{
			int num = 0;
			for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (Scene.SceneTracker.allPlayers[i] && (Scene.SceneTracker.allPlayers[i].transform.position - base.transform.position).sqrMagnitude < 122500f)
				{
					num++;
				}
			}
			int num2 = this.setup.health.Health + this.setup.health.Health / 3 * num;
			if (num2 > 800)
			{
				num2 = 800;
			}
			this.setup.health.Health = num2;
			this.setup.health.maxHealth = num2;
		}
	}

	
	public void killAllBossBabies()
	{
		foreach (GameObject gameObject in this.spawnedBabies)
		{
			if (gameObject)
			{
				spawnMutants component = gameObject.GetComponent<spawnMutants>();
				if (component && component.allMembers.Count > 0)
				{
					component.allMembers[0].SendMessage("killThisEnemy");
				}
			}
		}
	}

	
	public Vector3 girlStartPos;

	
	private mutantScriptSetup setup;

	
	private FsmBool fsmBlockBabySpawn;

	
	private FsmFloat fsmSpawnBabyWeight;

	
	private FsmFloat fsmChargeAttackWeight;

	
	private FsmFloat fsmLongAttackWeight;

	
	private FsmFloat fsmStrafeWeight;

	
	private FsmFloat fsmWalkBackWeight;

	
	private FsmFloat fsmWalkForwardWeight;

	
	private FsmFloat fsmToIdleWeight;

	
	private FsmFloat fsmRunForwardWeight;

	
	private FsmFloat fsmDodgeWeight;

	
	private FsmBool fsmGoHomeBool;

	
	private FsmFloat fsmSpinAttackWeight;

	
	private float updateDelay;

	
	public List<GameObject> spawnedBabies = new List<GameObject>();
}
