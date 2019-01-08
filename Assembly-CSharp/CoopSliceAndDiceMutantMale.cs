using System;
using Bolt;

public class CoopSliceAndDiceMutantMale : CoopSliceAndDiceMutant
{
	public override NetworkArray_Integer BodyPartsDamage
	{
		get
		{
			return base.entity.GetState<IMutantMaleDummyState>().BodyPartsDamage;
		}
	}
}
