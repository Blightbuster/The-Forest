using System;
using TheForest.Items.Craft;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[Serializable]
	public class ItemProperties
	{
		
		
		
		public event Action<WeaponStatUpgrade.Types> ActiveBonusChanged = delegate
		{
		};

		
		
		
		public WeaponStatUpgrade.Types ActiveBonus
		{
			get
			{
				return this._activeBonus;
			}
			set
			{
				this._activeBonus = value;
				this.ActiveBonusChanged(value);
			}
		}

		
		
		
		public virtual float ActiveBonusValue
		{
			get
			{
				return this._activeBonusValue;
			}
			set
			{
				this._activeBonusValue = value;
			}
		}

		
		public override bool Equals(object obj)
		{
			return base.Equals(obj) || obj == ItemProperties.Any;
		}

		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		
		public virtual bool Match(ItemProperties other)
		{
			return other == ItemProperties.Any || other.ActiveBonus == (WeaponStatUpgrade.Types)(-2) || (this.ActiveBonus == other.ActiveBonus && Mathf.Approximately(this.ActiveBonusValue, other.ActiveBonusValue));
		}

		
		public virtual void Copy(ItemProperties other)
		{
			if (other != ItemProperties.Any)
			{
				this.ActiveBonus = other.ActiveBonus;
				this.ActiveBonusValue = other.ActiveBonusValue;
			}
			else
			{
				this.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
				this.ActiveBonusValue = 0f;
			}
		}

		
		public virtual void Copy(ItemInfo coopItemInfo)
		{
			this.ActiveBonus = (WeaponStatUpgrade.Types)coopItemInfo.Bonus;
			this.ActiveBonusValue = coopItemInfo.BonusValue;
		}

		
		public virtual void Fill(PlayerAddItem pai)
		{
			pai.ActiveBonus = (int)this.ActiveBonus;
			pai.ActiveBonusValue = this.ActiveBonusValue;
		}

		
		public virtual void Fill(ItemInfo ii)
		{
			ii.Bonus = (int)this.ActiveBonus;
			ii.BonusValue = this.ActiveBonusValue;
		}

		
		public static ItemProperties CreateFrom(PlayerAddItem pai)
		{
			int itemPropertiesType = pai.ItemPropertiesType;
			if (itemPropertiesType != 1)
			{
				return new ItemProperties
				{
					ActiveBonus = (WeaponStatUpgrade.Types)pai.ActiveBonus,
					ActiveBonusValue = pai.ActiveBonusValue
				};
			}
			return new DecayingInventoryItemView.DecayingItemProperties
			{
				ActiveBonus = (WeaponStatUpgrade.Types)pai.ActiveBonus,
				ActiveBonusValue = pai.ActiveBonusValue,
				_state = (DecayingInventoryItemView.DecayStates)pai.IntVal1,
				_decayDoneTime = pai.FloatVal1
			};
		}

		
		public virtual ItemProperties Clone()
		{
			return new ItemProperties
			{
				ActiveBonus = this.ActiveBonus,
				ActiveBonusValue = this.ActiveBonusValue
			};
		}

		
		
		public static ItemProperties Any
		{
			get
			{
				return null;
			}
		}

		
		[SerializeThis]
		[FieldReset((WeaponStatUpgrade.Types)(-1))]
		[SerializeField]
		protected WeaponStatUpgrade.Types _activeBonus = (WeaponStatUpgrade.Types)(-1);

		
		[SerializeThis]
		[SerializeField]
		protected float _activeBonusValue;
	}
}
