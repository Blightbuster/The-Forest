using System;
using Bolt;
using UnityEngine;

public class CoopMutantClientHider : EntityBehaviour
{
	public override void Attached()
	{
		if (base.entity.IsAttached() && !base.entity.isOwner)
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
		if (base.entity.IsAttached() && !base.entity.isOwner && base.entity.isFrozen != this._old)
		{
			for (int i = 0; i < this.ren.Length; i++)
			{
				if (this.ren[i])
				{
					this.ren[i].enabled = !base.entity.isFrozen;
				}
			}
			this._old = base.entity.isFrozen;
		}
	}

	private bool _old;

	private mutantAI_net ai;

	private Renderer[] ren;
}
