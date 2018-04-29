using System;
using TheForest.Buildings.Interfaces;
using TheForest.Interfaces;
using TheForest.Utils.Settings;
using UnityEngine;


public class mutantHitReceiver : MonoBehaviour, IWetable, IBurnable
{
	
	private void Awake()
	{
		this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
		this.health = base.transform.root.GetComponentInChildren<EnemyHealth>();
	}

	
	public void forceEnemyStop()
	{
		if (!this.setup)
		{
			return;
		}
		if (this.setup.ai)
		{
			this.setup.ai.StartCoroutine("toStop");
			if (this.setup.pmCombat)
			{
				this.setup.pmCombat.SendEvent("toReset");
			}
			if (this.setup.pmSearchScript)
			{
				this.setup.pmSearchScript.toResetSearchEvent();
			}
			if (this.setup.pmBrain)
			{
				this.setup.pmBrain.SendEvent("toSetAggressive");
			}
		}
	}

	
	public void DisableWeaponHits(float time)
	{
		this.DisableWeaponHits();
		base.Invoke("EnableWeaponHits", time);
	}

	
	public void DisableWeaponHits()
	{
		this.disableWeaponHit = true;
	}

	
	public void EnableWeaponHits()
	{
		this.disableWeaponHit = false;
	}

	
	public void sendHitFallDown(float damage)
	{
		if (!this.setup)
		{
			return;
		}
		if (this.setup.ai.pale && !this.setup.ai.femaleSkinny && !this.setup.ai.maleSkinny)
		{
			this.health.getCombo(3);
			this.health.Hit(Mathf.FloorToInt(damage));
		}
		else if (UnityEngine.Random.value < 1f - GameSettings.Ai.heavyAttackKnockDownChance)
		{
			this.health.getCombo(3);
			this.health.Hit(Mathf.FloorToInt(damage));
		}
		else
		{
			this.health.hitFallDown(damage);
		}
	}

	
	public void Douse()
	{
		if (!this.doused)
		{
			this.doused = true;
			if (this.health)
			{
				this.health.setFireDouse();
			}
			base.Invoke("resetDoused", 0.5f);
		}
	}

	
	public void Burn()
	{
		if (this.isHead)
		{
			return;
		}
		if (!this.burning)
		{
			if (this.health)
			{
				this.health.Burn();
			}
			this.burning = true;
			base.Invoke("resetBurning", 5f);
		}
	}

	
	public void Poison()
	{
		if (!this.poisonned)
		{
			if (this.health)
			{
				this.health.Poison();
			}
			this.poisonned = true;
			base.Invoke("resetPoisonned", 10f);
		}
	}

	
	private void resetDoused()
	{
		this.doused = false;
	}

	
	private void resetBurning()
	{
		this.burning = false;
	}

	
	private void resetPoisonned()
	{
		this.poisonned = false;
	}

	
	public void GotClean()
	{
		base.CancelInvoke("resetDoused");
		base.CancelInvoke("resetBurning");
		this.resetDoused();
		this.resetBurning();
		if (this.health)
		{
			this.health.resetDouse();
			this.health.disableBurn();
		}
	}

	
	
	public bool IsBurning
	{
		get
		{
			return this.burning;
		}
	}

	
	private void getSkinHitPosition(Transform hitTr)
	{
		if (!this.health)
		{
			return;
		}
		Vector3 vector = base.transform.InverseTransformPoint(hitTr.position);
		if (vector.y > 1.1f)
		{
			this.health.setSkinDamage(0);
		}
		else if (vector.x > 0f)
		{
			this.health.setSkinDamage(1);
		}
		else
		{
			this.health.setSkinDamage(2);
		}
	}

	
	public void getAttackDirection(int dir)
	{
		if (this.health)
		{
			if (this.isLeftLeg)
			{
				this.health.getAttackDirection(0);
			}
			else if (this.isRightLeg)
			{
				this.health.getAttackDirection(1);
			}
			else
			{
				this.health.getAttackDirection(dir);
			}
		}
	}

	
	public void getStealthAttack()
	{
		if (this.health)
		{
			this.health.getStealthAttack();
		}
	}

	
	public void takeDamage(int dir)
	{
		if (this.health)
		{
			this.health.takeDamage(dir);
		}
	}

	
	public void getCombo(int combo)
	{
		if (this.health)
		{
			this.health.getCombo(combo);
		}
	}

	
	public void hitRelay(int damage)
	{
		if (this.disableWeaponHit)
		{
			return;
		}
		if (this.health)
		{
			if (this.isLeftLeg)
			{
				this.health.hitLimb = 1;
			}
			else if (this.isRightLeg)
			{
				this.health.hitLimb = 1;
			}
			else if (this.isWomb)
			{
				this.health.hitLimb = 2;
			}
			else
			{
				this.health.hitLimb = 0;
			}
			this.health.Hit(damage);
		}
	}

	
	private mutantScriptSetup setup;

	
	private EnemyHealth health;

	
	public bool isHead;

	
	public bool isLeftLeg;

	
	public bool isRightLeg;

	
	public bool isWomb;

	
	private bool doused;

	
	private bool burning;

	
	private bool poisonned;

	
	private bool disableWeaponHit;

	
	public bool inNooseTrap;
}
