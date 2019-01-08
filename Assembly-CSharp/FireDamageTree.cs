using System;
using Bolt;

public class FireDamageTree : FireDamage
{
	public override bool isBurning
	{
		get
		{
			if (!BoltNetwork.isRunning)
			{
				return base.isBurning;
			}
			LOD_Trees lodTree = base.GetComponent<TreeHealth>().LodTree;
			if (lodTree)
			{
				CoopTreeId component = lodTree.GetComponent<CoopTreeId>();
				return component.entity.isAttached && component.state.Burning;
			}
			return false;
		}
		set
		{
			if (!BoltNetwork.isRunning)
			{
				base.isBurning = value;
			}
			else
			{
				CoopTreeId component = base.GetComponent<TreeHealth>().LodTree.GetComponent<CoopTreeId>();
				if (component.entity.isAttached)
				{
					if (component.entity.isOwner)
					{
						component.state.Burning = value;
						component.entity.Freeze(false);
					}
					else
					{
						BurnTree burnTree = BurnTree.Create(GlobalTargets.OnlyServer);
						burnTree.Entity = component.entity;
						burnTree.IsBurning = value;
						burnTree.Send();
					}
				}
			}
		}
	}

	protected override float usedFuelSeconds
	{
		get
		{
			if (!BoltNetwork.isRunning)
			{
				return base.usedFuelSeconds;
			}
			return base.GetComponent<TreeHealth>().LodTree.GetComponent<CoopTreeId>().state.Fuel;
		}
		set
		{
			if (!BoltNetwork.isRunning)
			{
				base.usedFuelSeconds = value;
			}
		}
	}

	protected override void Burn()
	{
		if (this.MyFuel == 0f)
		{
			this.MyFuel = 15f;
		}
		base.Ignite(base.transform.position, 0);
	}

	protected override void BurningOff()
	{
		if (!BoltNetwork.isRunning)
		{
			this.isBurning = false;
		}
	}

	protected override void FinishedBurning()
	{
		if (!BoltNetwork.isClient)
		{
			this.isBurning = false;
			this.usedFuelSeconds = 0f;
			base.StopBurning();
		}
		else
		{
			base.SetMaterialBurnAmount(0f);
			base.ResetFirePoints();
		}
	}
}
