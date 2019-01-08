using System;
using Bolt;
using TheForest.UI;

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
		return (CoopPeerStarter.Dedicated && evnt.RaisedBy == BoltNetwork.server) || evnt.RaisedBy == null || !(MpPlayerList.GetEntityFromBoltConnexion(evnt.RaisedBy) == null);
	}
}
