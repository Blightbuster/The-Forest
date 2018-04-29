using System;
using Bolt;
using TheForest.Utils;


public class coopGirlTokenHandler : EntityBehaviour<IDynamicPickup>
{
	
	public override void Attached()
	{
		CoopSyncGirlPickupToken coopSyncGirlPickupToken = (CoopSyncGirlPickupToken)this.entity.attachToken;
		if (!CoopPeerStarter.DedicatedHost && BoltNetwork.isServer && coopSyncGirlPickupToken.playerTarget == LocalPlayer.Entity)
		{
			return;
		}
		girlPickupCoopSync componentInChildren = base.transform.GetComponentInChildren<girlPickupCoopSync>();
		if (coopSyncGirlPickupToken.putDown && componentInChildren)
		{
			componentInChildren.setGirlPutDownAnimation(coopSyncGirlPickupToken.playerTarget.transform);
		}
	}
}
