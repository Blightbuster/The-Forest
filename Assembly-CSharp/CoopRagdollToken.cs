using System;
using Bolt;
using UdpKit;

internal class CoopRagdollToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteBool(this.onFireApplied);
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		this.onFireApplied = packet.ReadBool();
	}

	public bool onFireApplied;
}
