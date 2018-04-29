using System;
using Bolt;


public class CoopMutantRemote : EntityBehaviour<IMutantState>
{
	
	public override void Attached()
	{
		this.bodyVariation = base.GetComponentInChildren<setupBodyVariation>();
		if (base.state is IMutantFemaleState)
		{
		}
	}

	
	private setupBodyVariation bodyVariation;
}
