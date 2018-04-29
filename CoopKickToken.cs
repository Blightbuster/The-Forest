using System;
using Bolt;
using UdpKit;


internal class CoopKickToken : IProtocolToken
{
	
	public void Write(UdpPacket packet)
	{
		packet.WriteString(this.KickMessage);
	}

	
	public void Read(UdpPacket packet)
	{
		this.KickMessage = packet.ReadString();
	}

	
	public bool Banned;

	
	public string KickMessage;
}
