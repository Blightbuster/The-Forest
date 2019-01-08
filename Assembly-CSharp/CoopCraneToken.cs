using System;
using Bolt;
using UdpKit;

internal class CoopCraneToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteFloat(this.bottomY);
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		this.bottomY = packet.ReadFloat();
	}

	public float bottomY;
}
