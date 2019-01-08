using System;
using Bolt;
using UdpKit;
using UnityEngine;

internal class CoopZiplineTreeToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteVector3(this.p1);
		packet.WriteVector3(this.p2);
		packet.WriteInt(this.treeId);
		packet.WriteInt(this.treeId2);
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		this.p1 = packet.ReadVector3();
		this.p2 = packet.ReadVector3();
		this.treeId = packet.ReadInt();
		this.treeId2 = packet.ReadInt();
	}

	public Vector3 p1;

	public Vector3 p2;

	public int treeId;

	public int treeId2;
}
