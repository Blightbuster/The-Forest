using System;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;

public class mutantHitReactions : MonoBehaviour
{
	private void Start()
	{
		this.setup = base.transform.GetComponentInChildren<mutantScriptSetup>();
		this.tr = this.setup.transform;
		if (this.setup.pmCombat)
		{
			this.fsmOnStructureBool = this.setup.pmCombat.FsmVariables.GetFsmBool("onStructureBool");
		}
	}

	private void lookAtExplosion(Vector3 pos)
	{
		Vector3 worldPosition = pos;
		worldPosition.y = this.tr.position.y;
		this.tr.LookAt(worldPosition, this.tr.up);
	}

	private void killEnemyInSinkHole()
	{
		if (this.setup && this.setup.health)
		{
			this.setup.health.Hit(5000);
		}
	}

	private void staggerFromHit()
	{
		this.setup.animator.SetTriggerReflected("hitTrigger");
	}

	private void Douse()
	{
	}

	private void setPlayerAttacking()
	{
		float num = 8f;
		if (LocalPlayer.Animator != null && ((LocalPlayer.Animator != null && LocalPlayer.Animator.GetBool("bowHeld")) || LocalPlayer.Animator.GetBool("spearHeld") || LocalPlayer.Animator.GetBool("molotovHeld") || LocalPlayer.Animator.GetBool("ballHeld")))
		{
			num = 30f;
		}
		if ((this.setup.ai.maleSkinny || this.setup.ai.femaleSkinny || this.setup.ai.male || this.setup.ai.female) && this.setup.ai.mainPlayerDist < num && this.setup.ai.mainPlayerAngle < 40f && this.setup.ai.mainPlayerAngle > -40f && UnityEngine.Random.value > 0.6f && this.setup.search.playerAware)
		{
			float value = UnityEngine.Random.value;
			if (value < 0.3f && this.setup.ai.mainPlayerDist < 6f)
			{
				if (value < 0.15f)
				{
					this.setup.animator.SetIntegerReflected("randInt1", 5);
				}
				else
				{
					this.setup.animator.SetIntegerReflected("randInt1", 6);
				}
			}
			else if (UnityEngine.Random.value < 0.65f)
			{
				this.setup.animator.SetIntegerReflected("randInt1", 3);
			}
			else
			{
				this.setup.animator.SetIntegerReflected("randInt1", 4);
			}
			this.setup.animator.SetBoolReflected("dodgePlayer", true);
			base.Invoke("resetPlayerAttacking", 1f);
		}
	}

	private void setEnemyParried()
	{
		this.setup.animator.SetTriggerReflected("parryTrigger");
	}

	private void resetPlayerAttacking()
	{
		this.setup.animator.SetBoolReflected("dodgePlayer", false);
	}

	private void Poison()
	{
		if (this.setup && this.setup.health)
		{
			this.setup.health.Poison();
		}
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.gameObject.CompareTag("structure") || hit.gameObject.CompareTag("UnderfootWood"))
		{
			if (this.setup.pmCombat)
			{
				this.fsmOnStructureBool.Value = true;
			}
			this.onStructure = true;
		}
		else
		{
			this.onStructure = false;
			if (this.setup.pmCombat)
			{
				this.fsmOnStructureBool.Value = false;
			}
		}
	}

	private Transform tr;

	private mutantScriptSetup setup;

	public bool onStructure;

	private FsmBool fsmOnStructureBool;
}
