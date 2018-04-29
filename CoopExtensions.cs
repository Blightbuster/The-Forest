using System;
using Bolt;


public static class CoopExtensions
{
	
	public static bool IsAttached(this BoltEntity entity)
	{
		return BoltNetwork.isRunning && entity != null && entity && entity.isAttached;
	}

	
	public static bool IsOwner(this BoltEntity entity)
	{
		return entity.IsAttached() && entity.isOwner;
	}

	
	public static bool ValidateSender(this GlobalEventListener listener, Event evnt, SenderTypes senderType = SenderTypes.Any)
	{
		return true;
	}
}
