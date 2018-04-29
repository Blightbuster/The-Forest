using System;
using Bolt;
using UdpKit;
using UnityEngine;


internal class CoopGardenToken : IProtocolToken
{
	
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteVector2(this.size);
	}

	
	void IProtocolToken.Read(UdpPacket packet)
	{
		this.size = packet.ReadVector2();
	}

	
	public Vector2 size;
}
