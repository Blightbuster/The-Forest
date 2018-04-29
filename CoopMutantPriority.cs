using System;
using Bolt;


internal class CoopMutantPriority : EntityBehaviour, IPriorityCalculator
{
	
	
	bool IPriorityCalculator.Always
	{
		get
		{
			return false;
		}
	}

	
	float IPriorityCalculator.CalculateEventPriority(BoltConnection connection, Event evnt)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, 2f, 1);
	}

	
	float IPriorityCalculator.CalculateStatePriority(BoltConnection connection, int skipped)
	{
		return CoopUtils.CalculatePriorityFor(connection, base.entity, 2f, skipped);
	}
}
