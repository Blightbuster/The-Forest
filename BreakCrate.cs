using System;
using Bolt;
using UnityEngine;


public class BreakCrate : EntityBehaviour
{
	
	private void Hit(int damage)
	{
		if (this.health > 0)
		{
			this.health -= damage;
			if (this.health <= 0)
			{
				this.Explosion();
			}
		}
	}

	
	private void Explosion()
	{
		if (BoltNetwork.isRunning && this.entity.IsAttached())
		{
			SendMessageEvent sendMessageEvent = SendMessageEvent.Create(GlobalTargets.OnlyServer);
			sendMessageEvent.Target = this.entity;
			sendMessageEvent.Message = "ExplosionReal";
			sendMessageEvent.Send();
		}
		else
		{
			this.ExplosionReal();
		}
	}

	
	public override void Detached()
	{
		if (BoltNetwork.isClient && this.entity.detachToken is CoopCreateBreakToken)
		{
			this.ExplosionReal();
		}
	}

	
	private void ExplosionReal()
	{
		FMODCommon.PlayOneshotNetworked(this.breakEvent, base.transform, FMODCommon.NetworkRole.Server);
		GameObject gameObject = UnityEngine.Object.Instantiate(this.Broken, base.transform.position, base.transform.rotation) as GameObject;
		if (this.entity.IsOwner())
		{
			BoltNetwork.Detach(this.entity, new CoopCreateBreakToken());
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	public GameObject Broken;

	
	public int health = 20;

	
	[Header("FMOD")]
	public string breakEvent = "event:/physics/wood/wood_small_break";
}
