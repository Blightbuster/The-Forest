using System;
using Bolt;
using TheForest.Buildings.Creation;
using UdpKit;


internal class CoopRoofToken : IProtocolToken
{
	
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteBoltEntity(this.Parent);
		packet.WriteFloat(this.Height);
		packet.WriteBoltEntity(this.Support);
		if (packet.WriteBool(this.Holes != null && this.Holes.Length > 0))
		{
			packet.WriteInt(this.Holes.Length);
			for (int i = 0; i < this.Holes.Length; i++)
			{
				packet.WriteVector3(this.Holes[i]._position);
				packet.WriteFloat(this.Holes[i]._yRotation);
				packet.WriteVector2(this.Holes[i]._size);
			}
		}
	}

	
	void IProtocolToken.Read(UdpPacket packet)
	{
		this.Parent = packet.ReadBoltEntity();
		this.Height = packet.ReadFloat();
		this.Support = packet.ReadBoltEntity();
		if (packet.ReadBool())
		{
			this.Holes = new Hole[packet.ReadInt()];
			for (int i = 0; i < this.Holes.Length; i++)
			{
				this.Holes[i] = new Hole();
				this.Holes[i]._position = packet.ReadVector3();
				this.Holes[i]._yRotation = packet.ReadFloat();
				this.Holes[i]._size = packet.ReadVector2();
			}
		}
	}

	
	public float Height;

	
	public BoltEntity Support;

	
	public BoltEntity Parent;

	
	public Hole[] Holes;
}
