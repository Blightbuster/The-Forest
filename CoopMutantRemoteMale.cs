using System;
using Bolt;
using UnityEngine;


public class CoopMutantRemoteMale : EntityBehaviour<IMutantState>
{
	
	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			base.state.AddCallback("BlendShapeWeight0", delegate
			{
				this.MeshRenderer.SetBlendShapeWeight(28, base.state.BlendShapeWeight0);
			});
		}
	}

	
	[SerializeField]
	public SkinnedMeshRenderer MeshRenderer;
}
