using System;
using Bolt;

public class CoopTreeHouse : EntityBehaviour<ITreeHouseState>
{
	public override void Attached()
	{
		if (base.entity.isOwner)
		{
			base.state.Transform.SetTransforms(base.transform);
		}
		else
		{
			base.state.AddCallback("Transform", new PropertyCallbackSimple(this.ReceivedTransfrom));
		}
	}

	private void ReceivedTransfrom()
	{
		base.state.RemoveCallback("Transform", new PropertyCallbackSimple(this.ReceivedTransfrom));
		base.state.Transform.SetTransforms(base.transform);
	}
}
