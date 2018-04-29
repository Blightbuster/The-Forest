using System;
using Bolt;
using TheForest.Buildings.World;
using TheForest.Items;
using UnityEngine;


public class CoopBodyPart : EntityBehaviour<IPartState>, IPriorityCalculator, IEntityReplicationFilter
{
	
	
	bool IPriorityCalculator.Always
	{
		get
		{
			return true;
		}
	}

	
	public override void Attached()
	{
		if (BoltNetwork.isClient)
		{
			base.state.AddCallback("Effigy", new PropertyCallback(this.EffigyChanged));
		}
		base.state.Transform.SetTransforms(base.transform);
	}

	
	private void EffigyChanged(IState _, string propertyPath, ArrayIndices arrayIndices)
	{
		if (base.state.Effigy)
		{
			Component[] componentsInChildren = base.state.Effigy.GetComponentsInChildren(typeof(EffigyArchitect), true);
			if (componentsInChildren.Length > 0)
			{
				EffigyArchitect effigyArchitect = componentsInChildren[0] as EffigyArchitect;
				if (effigyArchitect)
				{
					effigyArchitect._parts.Add(new EffigyArchitect.Part
					{
						_itemId = ((!this.isTorso) ? this.itemid : -2),
						_position = base.transform.position,
						_rotation = base.transform.rotation.eulerAngles
					});
					base.transform.parent = effigyArchitect.transform;
				}
			}
		}
	}

	
	float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Bolt.Event evnt)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, 1f, 1);
	}

	
	float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, 1f, skipped + 1);
	}

	
	public bool AllowReplicationTo(BoltConnection connection)
	{
		if (this && base.entity && base.entity.isAttached && base.state != null && base.state.Effigy)
		{
			if (base.state.Effigy.isFrozen)
			{
				base.state.Effigy.Freeze(false);
			}
			return connection.ExistsOnRemote(base.state.Effigy) == ExistsResult.Yes;
		}
		return false;
	}

	
	[ItemIdPicker]
	public int itemid;

	
	public bool isTorso;
}
