using System;

namespace TheForest.Items.Craft
{
	
	[Serializable]
	public class WeaponStatUpgrade
	{
		
		public const WeaponStatUpgrade.Types NoBonus = (WeaponStatUpgrade.Types)(-1);

		
		public const WeaponStatUpgrade.Types UnspecifiedBonus = (WeaponStatUpgrade.Types)(-2);

		
		public WeaponStatUpgrade.Types _type;

		
		public float _amount;

		
		[ItemIdPicker]
		public int _itemId;

		
		public enum Types
		{
			
			weaponDamage,
			
			smashDamage,
			
			weaponSpeed,
			
			tiredSpeed,
			
			staminaDrain,
			
			soundDetectRange,
			
			weaponRange,
			
			BurningWeapon,
			
			StickyProjectile,
			
			WalkmanTrack,
			
			BurningAmmo,
			
			Paint_Green,
			
			Paint_Orange,
			
			DirtyWater,
			
			CleanWater,
			
			Cooked,
			
			ItemPart,
			
			BatteryCharge,
			
			FlareGunAmmo,
			
			SetWeaponAmmoBonus,
			
			blockStaminaDrain,
			
			PoisonnedAmmo,
			
			BurningWeaponExtra,
			
			Incendiary,
			
			RawFood,
			
			DriedFood,
			
			BoneAmmo,
			
			CamCorderTape,
			
			PoisonnedWeapon,
			
			ModernAmmo,
			
			TapedLight
		}
	}
}
