using System;
using Bolt;
using UdpKit;


internal class CoopJoinDedicatedServerFailed : IProtocolToken
{
	
	public void Write(UdpPacket packet)
	{
		packet.WriteString(this.Error);
	}

	
	public void Read(UdpPacket packet)
	{
		this.Error = packet.ReadString();
	}

	
	public string Error;
}
