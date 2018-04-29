using System;
using Bolt;
using TheForest.Buildings.Creation;


public class CoopBuildingExBridge : EntityBehaviour
{
	
	public override void Attached()
	{
		this.arch = base.GetComponent<BridgeArchitect>();
		this.arch.WasBuilt = true;
		this.arch.CustomToken = base.entity.attachToken;
		if (!base.entity.isOwner)
		{
			base.entity.SendMessage("OnDeserialized");
		}
	}

	
	private BridgeArchitect arch;
}
