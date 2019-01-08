using System;
using Bolt;
using UdpKit;

public class CoopSyncGirlPickupToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteBoltEntity(this.playerTarget);
		packet.WriteBool(this.pickup);
		packet.WriteBool(this.putDown);
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		this.playerTarget = packet.ReadBoltEntity();
		this.pickup = packet.ReadBool();
		this.putDown = packet.ReadBool();
	}

	public bool pickup;

	public bool putDown;

	public BoltEntity playerTarget;
}
