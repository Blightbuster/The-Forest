using System;
using Bolt;
using UnityEngine;


public class TripWireTrigger : EntityBehaviour<ITripWireState>
{
	
	public override void Attached()
	{
		base.state.AddCallback("Tripped", delegate
		{
			if (base.state.Tripped)
			{
				this.OnTripped();
			}
		});
	}

	
	public override void Detached()
	{
		if (!this.tripped)
		{
			this.OnTripped();
		}
	}

	
	private void OnTripped()
	{
		if (!this.tripped)
		{
			this.tripped = true;
			this.BombScript.SkipAttach = true;
			this.BombScript.enabled = true;
			this.BombScript.transform.parent = null;
			if (BoltNetwork.isRunning)
			{
				if (base.entity && base.entity.isOwner)
				{
					UnityEngine.Object.Destroy(base.entity.gameObject, 0.1f);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(base.transform.parent.gameObject);
			}
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerNet") || other.gameObject.CompareTag("enemyRoot") || other.gameObject.CompareTag("TennisBall") || other.gameObject.CompareTag("Weapon") || other.gameObject.CompareTag("Rock") || other.transform.GetComponent<ArrowDamage>())
		{
			if (BoltNetwork.isRunning)
			{
				if (base.entity && base.entity.isOwner)
				{
					base.state.Tripped = true;
				}
				else
				{
					TripWire tripWire = TripWire.Create(GlobalTargets.OnlyServer);
					tripWire.WireEntity = base.entity;
					tripWire.Send();
				}
			}
			else
			{
				this.OnTripped();
			}
		}
	}

	
	private bool tripped;

	
	public Bomb BombScript;
}
