using System;
using Bolt;
using UdpKit;
using UnityEngine;

internal class CoopZiplineToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteVector3(this.p1);
		packet.WriteVector3(this.p2);
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		this.p1 = packet.ReadVector3();
		this.p2 = packet.ReadVector3();
	}

	public Vector3 p1;

	public Vector3 p2;
}
