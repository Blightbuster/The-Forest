using System;
using Bolt;
using UdpKit;

internal class CoopSingleAnchorToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteBoltEntity(this.Anchor);
		packet.WriteInt(this.Index);
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		this.Anchor = packet.ReadBoltEntity();
		this.Index = packet.ReadInt();
	}

	public BoltEntity Anchor;

	public int Index;
}
