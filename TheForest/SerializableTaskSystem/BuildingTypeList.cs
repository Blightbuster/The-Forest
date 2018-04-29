using System;
using TheForest.Buildings.Creation;

namespace TheForest.SerializableTaskSystem
{
	
	[Serializable]
	public class BuildingTypeList : ACondition
	{
		
		public override void Init()
		{
		}

		
		public BuildingTypes[] _types;
	}
}
