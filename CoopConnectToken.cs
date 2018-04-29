using System;
using Bolt;
using UdpKit;


public class CoopConnectToken : IProtocolToken
{
	
	void IProtocolToken.Read(UdpPacket packet)
	{
		this.PlayerName = packet.ReadString();
	}

	
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteString(this.PlayerName);
	}

	
	public string PlayerName;
}
