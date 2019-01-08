using System;
using System.Collections.Generic;
using Bolt;
using TheForest.Items.Craft;
using UdpKit;

public class CoopWeaponUpgradesToken : IProtocolToken
{
	void IProtocolToken.Write(UdpPacket packet)
	{
		packet.WriteInt(this.Views.Count);
		for (int i = 0; i < this.Views.Count; i++)
		{
			packet.WriteInt(this.Views[i].ItemId);
			packet.WriteVector3(this.Views[i].Position);
			packet.WriteQuaternion(this.Views[i].Rotation);
		}
	}

	void IProtocolToken.Read(UdpPacket packet)
	{
		int num = packet.ReadInt();
		if (this.Views == null)
		{
			this.Views = new List<UpgradeViewReceiver.UpgradeViewData>();
		}
		else
		{
			this.Views.Clear();
		}
		for (int i = 0; i < num; i++)
		{
			UpgradeViewReceiver.UpgradeViewData upgradeViewData = new UpgradeViewReceiver.UpgradeViewData();
			upgradeViewData.ItemId = packet.ReadInt();
			upgradeViewData.Position = packet.ReadVector3();
			upgradeViewData.Rotation = packet.ReadQuaternion();
			this.Views.Add(upgradeViewData);
		}
	}

	public List<UpgradeViewReceiver.UpgradeViewData> Views;
}
