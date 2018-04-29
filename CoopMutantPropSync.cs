using System;
using Bolt;
using UnityEngine;


public class CoopMutantPropSync : EntityBehaviour<IMutantState>
{
	
	private void Awake()
	{
		if (!BoltNetwork.isRunning)
		{
			base.enabled = false;
		}
		else if (BoltNetwork.isClient && this.DisableByDefaultOnClient)
		{
			for (int i = 0; i < this.Props.Length; i++)
			{
				if (this.Props[i])
				{
					this.Props[i].SetActive(false);
				}
			}
		}
	}

	
	public void ApplyPropMask(int props)
	{
		for (int i = 0; i < this.Props.Length; i++)
		{
			if (this.Props[i])
			{
				int num = 1 << i;
				this.Props[i].SetActive((props & num) == num);
			}
		}
	}

	
	public int GetPropMask()
	{
		int num = 0;
		for (int i = 0; i < this.Props.Length; i++)
		{
			if (this.Props[i] && this.Props[i].activeSelf)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	
	public override void Attached()
	{
		if (!base.entity.IsOwner())
		{
			base.state.AddCallback("prop_mask", delegate
			{
				this.ApplyPropMask(base.state.prop_mask);
			});
			this.token = (base.entity.attachToken as CoopMutantDummyToken);
			if (this.token != null)
			{
				this.ApplyPropMask(this.token.Props);
			}
		}
	}

	
	private void Update()
	{
		if (base.entity.IsOwner())
		{
			int num = 0;
			for (int i = 0; i < this.Props.Length; i++)
			{
				if (this.Props[i] && this.Props[i].activeSelf)
				{
					num |= 1 << i;
				}
				if (base.state.prop_mask != num)
				{
					base.state.prop_mask = num;
				}
			}
		}
	}

	
	public bool DisableByDefaultOnClient;

	
	public GameObject[] Props;

	
	private CoopMutantDummyToken token;
}
