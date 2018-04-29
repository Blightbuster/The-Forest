using System;
using Bolt;
using UnityEngine;


public class CoopMutantClientHider : EntityBehaviour
{
	
	public override void Attached()
	{
		if (this.entity.IsAttached() && !this.entity.isOwner)
		{
			this.ren = base.GetComponentsInChildren<Renderer>(true);
		}
		if (!this.ai)
		{
			this.ai = base.transform.GetComponent<mutantAI_net>();
		}
	}

	
	private void Update()
	{
		if (this.ai && this.ai.creepy_boss)
		{
			UnityEngine.Object.Destroy(base.GetComponent<CoopMutantClientHider>());
		}
		if (this.entity.IsAttached() && !this.entity.isOwner && this.entity.isFrozen != this._old)
		{
			for (int i = 0; i < this.ren.Length; i++)
			{
				if (this.ren[i])
				{
					this.ren[i].enabled = !this.entity.isFrozen;
				}
			}
			this._old = this.entity.isFrozen;
		}
	}

	
	private bool _old;

	
	private mutantAI_net ai;

	
	private Renderer[] ren;
}
