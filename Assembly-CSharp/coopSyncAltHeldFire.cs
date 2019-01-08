using System;
using Bolt;

public class coopSyncAltHeldFire : EntityEventListener<IPlayerState>
{
	private void Start()
	{
	}

	private void OnEnable()
	{
		if (base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			base.state.altHeldFireIndex[this.altFireStateIndex] = 1;
		}
	}

	private void OnDisable()
	{
		if (base.entity && base.entity.isAttached && base.entity.isOwner)
		{
			base.state.altHeldFireIndex[this.altFireStateIndex] = 0;
		}
	}

	public int altFireStateIndex;
}
