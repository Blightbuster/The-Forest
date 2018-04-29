using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Creation;


public class CoopAutoParent : EntityBehaviour<IHierarchyObjectState>, IEntityReplicationFilter
{
	
	public override void Attached()
	{
		base.StartCoroutine(this.DelayedAttached());
	}

	
	private IEnumerator DelayedAttached()
	{
		yield return null;
		if (BoltNetwork.isServer)
		{
			if (base.transform.parent)
			{
				BoltEntity parentEntity = base.transform.parent.GetComponentInParent<BoltEntity>();
				while (parentEntity && !base.state.ParentHack)
				{
					yield return null;
					if (parentEntity.isAttached)
					{
						base.state.ParentHack = parentEntity;
					}
				}
			}
			this._ready = true;
		}
		else
		{
			for (int i = 0; i < 10; i++)
			{
				if (base.state.ParentHack && base.state.ParentHack.transform)
				{
					DynamicBuilding component = base.state.ParentHack.transform.GetComponent<DynamicBuilding>();
					if (component && component._parentOverride)
					{
						base.transform.parent = component._parentOverride;
					}
					else
					{
						base.transform.parent = base.state.ParentHack.transform;
					}
					break;
				}
				yield return null;
			}
		}
		yield break;
	}

	
	bool IEntityReplicationFilter.AllowReplicationTo(BoltConnection connection)
	{
		if (base.transform.parent)
		{
			BoltEntity componentInParent = base.transform.parent.GetComponentInParent<BoltEntity>();
			return this._ready && connection.ExistsOnRemote(componentInParent) == ExistsResult.Yes;
		}
		return this._ready;
	}

	
	private bool _ready;
}
