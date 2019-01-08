using System;
using Bolt;
using UdpKit;

internal class CoopJoinDedicatedServerToken : IProtocolToken
{
	public void Write(UdpPacket packet)
	{
		packet.WriteString(this.AdminPassword);
		packet.WriteString(this.ServerPassword);
		if (this.steamBlobToken != null)
		{
			packet.WriteInt(this.steamBlobToken.Length);
			for (int i = 0; i < this.steamBlobToken.Length; i++)
			{
				packet.WriteByte(this.steamBlobToken[i]);
			}
		}
	}

	public void Read(UdpPacket packet)
	{
		this.AdminPassword = packet.ReadString();
		this.ServerPassword = packet.ReadString();
		if (packet.CanRead())
		{
			int num = packet.ReadInt();
			if (num > 0)
			{
				this.steamBlobToken = new byte[num];
				for (int i = 0; i < this.steamBlobToken.Length; i++)
				{
					this.steamBlobToken[i] = packet.ReadByte();
				}
			}
		}
	}

	public string AdminPassword;

	public string ServerPassword;

	public byte[] steamBlobToken;
}
