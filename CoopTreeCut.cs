using System;
using UniLinq;
using UnityEngine;


public class CoopTreeCut : CoopBase<ITreeCutState>
{
	
	public override void Attached()
	{
		this.chunks = (from x in base.GetComponentsInChildren<TreeCutChunk>()
		orderby int.Parse(x.transform.parent.gameObject.name)
		select x).ToArray<TreeCutChunk>();
		base.state.AddCallback("TreeId", delegate
		{
			CoopTreeId coopTreeId = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId x) => x.Id == base.state.TreeId);
			if (coopTreeId)
			{
				LOD_Trees component = coopTreeId.GetComponent<LOD_Trees>();
				if (component)
				{
					component.enabled = false;
					component.DontSpawn = true;
					if (component.CurrentView)
					{
						component.Pool.Despawn(component.CurrentView.transform);
						component.CurrentView = null;
						component.CurrentLodTransform = null;
					}
				}
			}
		});
		base.state.AddCallback("Damage", delegate
		{
			if (this.entity.isOwner && base.state.Damage == 16f)
			{
				this.entity.DestroyDelayed(10f);
				BoltEntity boltEntity = BoltNetwork.Instantiate(this.CoopTree.NetworkFallPrefab, this.entity.transform.position, this.entity.transform.rotation);
				boltEntity.GetState<ITreeFallState>().CutTree = this.entity;
				boltEntity.GetComponent<Rigidbody>().AddForce(new Vector3(UnityEngine.Random.value * 0.01f, 0f, UnityEngine.Random.value * 0.01f), ForceMode.VelocityChange);
			}
		});
	}

	
	private TreeCutChunk[] chunks;

	
	[HideInInspector]
	public CoopTreeId CoopTree;
}
