using System;
using Bolt;


public class CoopCooking : EntityBehaviour<ICookingState>
{
	
	public override void Attached()
	{
		base.state.Transform.SetTransforms(base.transform);
	}
}
