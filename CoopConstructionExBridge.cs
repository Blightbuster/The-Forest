using System;
using Bolt;
using TheForest.Buildings.Creation;


public class CoopConstructionExBridge : EntityBehaviour
{
	
	public override void Attached()
	{
		this.arch = base.GetComponent<BridgeArchitect>();
		this.arch.WasPlaced = true;
		this.arch.CustomToken = this.entity.attachToken;
		this.entity.SendMessage("OnDeserialized");
	}

	
	private BridgeArchitect arch;
}
