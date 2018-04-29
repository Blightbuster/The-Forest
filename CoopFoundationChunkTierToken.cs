using System;
using Bolt;
using UdpKit;


internal class CoopFoundationChunkTierToken : IProtocolToken
{
	
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteInt(this.ChunkIndex);
	}

	
	void IProtocolToken.Read(UdpPacket packet)
	{
		this.ChunkIndex = packet.ReadInt();
	}

	
	public bool Applied;

	
	public int ChunkIndex;
}
