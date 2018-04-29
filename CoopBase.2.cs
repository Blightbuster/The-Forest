using System;
using Bolt;
using UnityEngine;


public abstract class CoopBase<T> : EntityEventListener<T>, IPriorityCalculator
{
	
	float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Bolt.Event evnt)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, this.MultiplayerPriority, 1);
	}

	
	float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, this.MultiplayerPriority, skipped);
	}

	
	public override void OnEvent(SendMessageEvent evnt)
	{
		base.entity.SendMessage(evnt.Message, SendMessageOptions.DontRequireReceiver);
	}

	
	
	bool IPriorityCalculator.Always
	{
		get
		{
			return false;
		}
	}

	
	[SerializeField]
	public float MultiplayerPriority = 0.5f;
}
