using System;
using System.Collections.Generic;

namespace TheForest.Utils
{
	[Serializable]
	public class PlayerStatConditionList
	{
		public bool IsValid(PlayerStats playerStats)
		{
			if (this._conditions.Count > 0)
			{
				foreach (PlayerStatCondition playerStatCondition in this._conditions)
				{
					bool flag = playerStatCondition.IsValid(playerStats);
					PlayerStatConditionList.ValidationTypes validationType = this._validationType;
					if (validationType != PlayerStatConditionList.ValidationTypes.AnyTrue)
					{
						if (validationType == PlayerStatConditionList.ValidationTypes.AllTrue)
						{
							if (!flag)
							{
								return false;
							}
						}
					}
					else if (flag)
					{
						return true;
					}
				}
				return this._validationType == PlayerStatConditionList.ValidationTypes.AllTrue;
			}
			return true;
		}

		public PlayerStatConditionList.ValidationTypes _validationType;

		[NameFromProperty("_stat", 0)]
		public List<PlayerStatCondition> _conditions;

		public enum ValidationTypes
		{
			AnyTrue,
			AllTrue
		}
	}
}
