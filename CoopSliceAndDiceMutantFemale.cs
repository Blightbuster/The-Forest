using System;
using Bolt;


public class CoopSliceAndDiceMutantFemale : CoopSliceAndDiceMutant
{
	
	
	public override NetworkArray_Integer BodyPartsDamage
	{
		get
		{
			return base.entity.GetState<IMutantFemaleDummyState>().BodyPartsDamage;
		}
	}
}
