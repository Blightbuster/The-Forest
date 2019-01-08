using System;
using Bolt;

public class wormHitReceiver : EntityBehaviour<IMutantWormState>
{
	private void Start()
	{
		this.skinDamage = base.transform.GetComponentInParent<wormSkinDamage>();
		this.health = base.transform.GetComponentInParent<wormHealth>();
	}

	private void Hit(int val)
	{
		if (BoltNetwork.isClient)
		{
			HitWorm hitWorm = HitWorm.Create(GlobalTargets.OnlyServer);
			hitWorm.Target = base.entity;
			hitWorm.Damage = val;
			hitWorm.Send();
			if (this.skinDamage)
			{
				this.skinDamage.setSkinDamage(0.5f);
			}
		}
		else
		{
			this.health.Hit(val);
		}
	}

	private void Burn()
	{
		if (BoltNetwork.isClient)
		{
			HitWorm hitWorm = HitWorm.Create(GlobalTargets.OnlyServer);
			hitWorm.Target = base.entity;
			hitWorm.Burn = true;
			hitWorm.Damage = 5;
			hitWorm.Send();
			if (this.skinDamage)
			{
				this.skinDamage.setSkinDamage(0.5f);
			}
		}
		else
		{
			this.health.Burn();
		}
	}

	private void Explosion(int val)
	{
		this.health.Hit(val);
	}

	private wormHealth health;

	private wormSkinDamage skinDamage;
}
