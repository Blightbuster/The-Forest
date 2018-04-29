using System;
using Bolt;


public class CoopSliceAndDiceMutantMale : CoopSliceAndDiceMutant
{
	
	
	public override NetworkArray_Integer BodyPartsDamage
	{
		get
		{
			return this.entity.GetState<IMutantMaleDummyState>().BodyPartsDamage;
		}
	}
}
