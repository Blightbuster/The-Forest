using System;
using TheForest.Items.Craft;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Items.World
{
	[DoNotSerializePublic]
	public class SetActiveBonusProxy : MonoBehaviour
	{
		public void SetActiveBonus(WeaponStatUpgrade.Types activeBonus)
		{
			foreach (SetActiveBonusProxy.ActiveBonusData activeBonusData in this._supportedBonuses)
			{
				if (activeBonusData._type == activeBonus)
				{
					activeBonusData._event.Invoke();
					break;
				}
			}
		}

		public SetActiveBonusProxy.ActiveBonusData[] _supportedBonuses;

		[Serializable]
		public class ActiveBonusData
		{
			[FieldReset((WeaponStatUpgrade.Types)(-1), (WeaponStatUpgrade.Types)(-2))]
			public WeaponStatUpgrade.Types _type = (WeaponStatUpgrade.Types)(-1);

			public UnityEvent _event;
		}
	}
}
