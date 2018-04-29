using System;
using TheForest.Items.Craft;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items
{
	
	[Serializable]
	public class Item
	{
		
		public bool MatchType(Item.Types type)
		{
			return (this._type & type) != (Item.Types)0;
		}

		
		public bool MatchRangedStyle(Item.RangedStyle style)
		{
			return (this._rangedStyle & style) != (Item.RangedStyle)0;
		}

		
		public bool CanFallbackTo(int itemId)
		{
			if (this._id == itemId)
			{
				return true;
			}
			if (this._fallbackItemIds != null && this._fallbackItemIds.Length > 0)
			{
				foreach (int id in this._fallbackItemIds)
				{
					if (ItemDatabase.ItemById(id).CanFallbackTo(itemId))
					{
						return true;
					}
				}
			}
			return false;
		}

		
		public override int GetHashCode()
		{
			return this._id;
		}

		
		[HideInInspector]
		public int _id;

		
		[EnumFlags]
		[Header("Core")]
		public Item.Types _type;

		
		public string _name;

		
		[Header("Inventory")]
		[Tooltip("weapons, ranged, projectile and edible do by default but other item can gain the ability with this")]
		public bool _allowQuickSelect;

		
		public float _weight;

		
		[Tooltip("-1 means it cannot be stashed in inventory\n0 means no limit")]
		public int _maxAmount;

		
		[Tooltip("Prevents item with 'Equipement' type to be equiped from inventory")]
		public bool _inventoryTooltipOnly;

		
		public bool _preventClosingInventoryAfterEquip;

		
		[Tooltip("When calling inventory.RemoveItem()/Owns()/AmountOf() for a not owned item, it will try to fallback to this if defined")]
		[ItemIdPicker]
		public int[] _fallbackItemIds;

		
		public StatEffect[] _ownedStatEffect;

		
		[Header("Craft")]
		public int _maxUpgradesAmount = 30;

		
		[Header("Equip")]
		public Item.EquipmentSlot _equipmentSlot;

		
		public Item.AnimatorVariables[] _equipedAnimVars;

		
		public StatEffect[] _equipedStatEffect;

		
		public float _unequipDelay = 0.7f;

		
		[ItemIdPicker]
		[Header("Edible (reset: NoBonus / Unspecified)")]
		public int _rechargeOfItemId;

		
		[FieldReset((WeaponStatUpgrade.Types)(-1), (WeaponStatUpgrade.Types)(-2))]
		public WeaponStatUpgrade.Types _edibleCondition = (WeaponStatUpgrade.Types)(-1);

		
		[FieldReset((WeaponStatUpgrade.Types)(-1), (WeaponStatUpgrade.Types)(-2))]
		public WeaponStatUpgrade.Types _altEdibleCondition = (WeaponStatUpgrade.Types)(-1);

		
		[FieldReset((WeaponStatUpgrade.Types)(-1), (WeaponStatUpgrade.Types)(-2))]
		public WeaponStatUpgrade.Types _secondaryEdibleCondition = (WeaponStatUpgrade.Types)(-1);

		
		[FieldReset((WeaponStatUpgrade.Types)(-1), (WeaponStatUpgrade.Types)(-2))]
		public WeaponStatUpgrade.Types _secondaryAltEdibleCondition = (WeaponStatUpgrade.Types)(-1);

		
		public StatEffect[] _usedStatEffect;

		
		[Header("SFX")]
		public Item.SFXCommands _equipedSFX;

		
		public Item.SFXCommands _usedSFX;

		
		public Item.SFXCommands _stashSFX;

		
		public Item.SFXCommands _attackSFX;

		
		public Item.SFXCommands _attackReleaseSFX;

		
		public Item.SFXCommands _dryFireSFX;

		
		[Header("PM Events")]
		public Item.FSMEvents _attackEvent;

		
		public Item.FSMEvents _lastAmmoAttackEvent;

		
		public Item.FSMEvents _attackReleaseEvent;

		
		public Item.FSMEvents _blockEvent;

		
		public Item.FSMEvents _unblockEvent;

		
		[EnumFlags]
		[Header("Ranged")]
		public Item.RangedStyle _rangedStyle;

		
		public float _reloadDuration;

		
		public float _dryFireReloadDuration;

		
		public float _projectileMaxChargeDuration;

		
		public float _projectileThrowDelay;

		
		public RandomRange _projectileThrowForceRange;

		
		public RandomRange _projectileThrowTorqueRange;

		
		[ItemIdPicker(Item.Types.Ammo)]
		public int _ammoItemId;

		
		[Header("Prefabs")]
		public Transform _pickupPrefab;

		
		public Transform _pickupPrefabMP;

		
		public Transform _bareItemPrefab;

		
		public Transform _throwerProjectilePrefab;

		
		public Item.ItemPrefabs _ammoPrefabs;

		
		[Header("Materials")]
		public Material _addMat;

		
		public Material _takeMat;

		
		[Header("World")]
		public bool _allowUpsideDownPlacement;

		
		public Vector3 _upsideDownOffset;

		
		[Header("Rumble")]
		public float _hitRumbleStrength = 1f;

		
		public float _hitRumbleDuration = 0.1f;

		
		[Flags]
		public enum Types
		{
			
			Equipment = 1,
			
			CraftingTool = 2,
			
			CraftingMaterial = 4,
			
			Craftable = 2048,
			
			Edible = 8,
			
			Droppable = 16,
			
			Ammo = 32,
			
			Projectile = 64,
			
			Special = 128,
			
			Plant = 256,
			
			RangedWeapon = 512,
			
			Story = 1024,
			
			Weapon = 4096,
			
			Extension = 8192
		}

		
		public enum EquipmentSlot
		{
			
			RightHand,
			
			LeftHand,
			
			Chest,
			
			Feet
		}

		
		public enum AnimatorVariables
		{
			
			axeHeld,
			
			ballHeld,
			
			bowHeld,
			
			bowFireBool,
			
			checkArms,
			
			drawBool,
			
			drawBowBool,
			
			flareHeld,
			
			flaregunHeld,
			
			genericHeld,
			
			genericWideHeld,
			
			itemHeld,
			
			molotovHeld,
			
			leanForward,
			
			lighterIgnite,
			
			lighterHeld,
			
			logHeld,
			
			onHand,
			
			pedHeld,
			
			rockHeld,
			
			smallAxe,
			
			spearHeld,
			
			spearRaiseBool,
			
			stickHeld,
			
			walkmanHeld,
			
			flashLightHeld,
			
			genericHoldPouch,
			
			shellHeld,
			
			mapHeld,
			
			lookAtPhoto,
			
			lookAtItemRight,
			
			flintLockHeld,
			
			sapHeld,
			
			repairHammerHeld,
			
			chainSawHeld,
			
			camCorderHeld,
			
			potHeld,
			
			slingShotHeld,
			
			aimSlingBool,
			
			waterSkinHeld
		}

		
		[Flags]
		public enum RangedStyle
		{
			
			Forward = 1,
			
			Bell = 2,
			
			Instantaneous = 4,
			
			Charged = 8,
			
			Shoot = 16
		}

		
		public enum SFXCommands
		{
			
			None,
			
			PlayPlantRustle,
			
			PlayTorchOn,
			
			PlayTorchOff,
			
			PlayCoinsSfx,
			
			PlayShootFlareSfx,
			
			PlayPickUp,
			
			PlayWhoosh,
			
			PlayTwinkle,
			
			PlayRemove,
			
			PlayEat,
			
			PlayDrink,
			
			PlayHurtSound,
			
			PlayHammer,
			
			PlayFindBodyTwinkle,
			
			PlayColdSfx,
			
			StopColdSfx,
			
			PlayCough,
			
			PlayStaminaBreath,
			
			PlayLighterSound,
			
			PlayKillRabbit,
			
			PlayTurnPage,
			
			PlayOpenInventory,
			
			PlayCloseInventory,
			
			playVisWarning,
			
			stopVisWarning,
			
			PlayBowSnap,
			
			PlayBowDraw,
			
			PlayMusicTrack,
			
			StopMusic,
			
			PlayInjured,
			
			StopPlaying,
			
			PlayDryFlareFireSfx,
			
			PlayEatMeds,
			
			StartWalkyTalky,
			
			StopWalkyTalky,
			
			PlayShootFlintLockSfx
		}

		
		public enum FSMEvents
		{
			
			None,
			
			drawBow,
			
			releaseBow,
			
			toPedHeld,
			
			toPedStash,
			
			axeAttack,
			
			ballAttack,
			
			flareAttack,
			
			molotovAttack,
			
			rockAttack,
			
			stickAttack,
			
			spearAttack,
			
			resetBlock,
			
			stickBlock,
			
			stickShove,
			
			spearThrow,
			
			toReset2,
			
			toResetSpear,
			
			toGenericAttack,
			
			toRepairAttack,
			
			toChainSawAttack,
			
			toFlintLockAim,
			
			toFlareGunAim
		}

		
		public enum Pools
		{
			
			Pool_BigProps,
			
			Bushes,
			
			Caves,
			
			Creatures,
			
			Creatures_net,
			
			Enemies,
			
			Greebles,
			
			Misc,
			
			Particles,
			
			PickUps,
			
			Rocks,
			
			Trees
		}

		
		[Serializable]
		public class ItemPrefabs
		{
			
			public Transform GetPrefabForBonus(WeaponStatUpgrade.Types bonus, bool fallbackToDefault = true)
			{
				if (bonus == (WeaponStatUpgrade.Types)(-2) || bonus == (WeaponStatUpgrade.Types)(-1))
				{
					return this._default;
				}
				for (int i = 0; i < this._prefabs.Length; i++)
				{
					if (this._prefabs[i]._bonus == bonus)
					{
						return this._prefabs[i]._prefab;
					}
				}
				return (!fallbackToDefault) ? null : this._default;
			}

			
			[SerializeField]
			private Transform _default;

			
			[SerializeField]
			private Item.PrefabPerBonus[] _prefabs;
		}

		
		[Serializable]
		public class PrefabPerBonus
		{
			
			[FieldReset((WeaponStatUpgrade.Types)(-1), (WeaponStatUpgrade.Types)(-2))]
			public WeaponStatUpgrade.Types _bonus;

			
			public Transform _prefab;
		}
	}
}
