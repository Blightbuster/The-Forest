using System;
using TheForest.Items.Craft;
using TheForest.Items.Utils;
using TheForest.Items.World;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[DoNotSerializePublic]
	[AddComponentMenu("Items/Inventory/Decaying Item Inventory View")]
	public class DecayingInventoryItemView : InventoryItemView, ISaveableView
	{
		
		private void Awake()
		{
			this.Init();
		}

		
		private void OnDestroy()
		{
			if (DecayingInventoryItemView.LastUsed == this)
			{
				DecayingInventoryItemView.LastUsed = null;
			}
			if (this._properties != null)
			{
				((DecayingInventoryItemView.DecayingItemProperties)this._properties).StateChanged -= this.SetDecayState;
			}
		}

		
		public override void OnDeserialized()
		{
			if (base.GetComponent<EmptyObjectIdentifier>())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (this.DecayProperties._state > DecayingInventoryItemView.DecayStates.None && this.DecayProperties._state < DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				this.DecayProperties._state--;
			}
			this.Decay();
		}

		
		public override void OnSerializing()
		{
			this.DecayProperties._decayDoneTime = Time.time - this._decayStartTime;
		}

		
		public override void OnItemAdded()
		{
			DecayingInventoryItemView.LastUsed = this;
			if (this.DecayProperties._state == DecayingInventoryItemView.DecayStates.None)
			{
				this.Decay();
			}
			else
			{
				this._canDropFromInventory = (this.DecayProperties._state == DecayingInventoryItemView.DecayStates.Spoilt && base.ItemCache.MatchType(Item.Types.Droppable));
			}
		}

		
		public override void OnItemRemoved()
		{
			DecayingInventoryItemView.LastUsed = this;
			this.ToggleState(DecayingInventoryItemView.DecayStates.None);
		}

		
		public override void OnItemDropped(GameObject worldGo)
		{
			DecayingItem component = worldGo.GetComponent<DecayingItem>();
			if (component)
			{
				DecayingInventoryItemView.LastUsed = this;
				component.DecayState = this.DecayProperties._state;
			}
		}

		
		public override void OnItemEquipped()
		{
			if (this._prevState < DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				this.DecayProperties._state = (DecayingInventoryItemView.DecayStates)Mathf.Max(this._prevState - DecayingInventoryItemView.DecayStates.Fresh, 0);
			}
			else
			{
				this.DecayProperties._state = this._prevState;
			}
			this.Decay();
			this._held.SetActive(true);
			Renderer renderer = this._held.GetComponent<Renderer>();
			if (!renderer)
			{
				renderer = this._held.GetComponentInChildren<Renderer>();
			}
			if (renderer)
			{
				renderer.sharedMaterial = this.GetMaterialForState(this.DecayProperties._state);
			}
		}

		
		public void Decay()
		{
			switch (this.DecayProperties._state)
			{
			case DecayingInventoryItemView.DecayStates.None:
				this.ToggleState(DecayingInventoryItemView.DecayStates.Fresh);
				this._decayStartTime = Time.time;
				LocalPlayer.ItemDecayMachine.NewDecayCommand(this, this._decayDelay - this.DecayProperties._decayDoneTime);
				this.DecayProperties._decayDoneTime = 0f;
				break;
			case DecayingInventoryItemView.DecayStates.Fresh:
				this.ToggleState(DecayingInventoryItemView.DecayStates.Edible);
				this._decayStartTime = Time.time;
				LocalPlayer.ItemDecayMachine.NewDecayCommand(this, this._decayDelay - this.DecayProperties._decayDoneTime);
				this.DecayProperties._decayDoneTime = 0f;
				break;
			case DecayingInventoryItemView.DecayStates.Edible:
			case DecayingInventoryItemView.DecayStates.Spoilt:
				this.ToggleState(DecayingInventoryItemView.DecayStates.Spoilt);
				break;
			case DecayingInventoryItemView.DecayStates.DriedFresh:
			case DecayingInventoryItemView.DecayStates.DriedEdible:
			case DecayingInventoryItemView.DecayStates.DriedSpoilt:
				this.ToggleState(this.DecayProperties._state);
				break;
			}
		}

		
		public void SetDecayState(DecayingInventoryItemView.DecayStates state)
		{
			LocalPlayer.ItemDecayMachine.CancelCommandFor(this);
			this.ToggleState((state <= DecayingInventoryItemView.DecayStates.None) ? DecayingInventoryItemView.DecayStates.Fresh : state);
			this._decayStartTime = Time.time;
			if (state < DecayingInventoryItemView.DecayStates.Spoilt)
			{
				LocalPlayer.ItemDecayMachine.NewDecayCommand(this, this._decayDelay - this.DecayProperties._decayDoneTime);
			}
		}

		
		private void ToggleState(DecayingInventoryItemView.DecayStates state)
		{
			this._prevState = this.DecayProperties._state;
			this.DecayProperties._state = state;
			base.NormalMaterial = this.GetMaterialForState(state);
			if (LocalPlayer.Inventory.RightHand == this)
			{
				this._held.GetComponentInChildren<Renderer>().sharedMaterial = base.NormalMaterial;
			}
			foreach (InventoryItemView.RendererDefinition rendererDefinition in this._renderers)
			{
				rendererDefinition._defaultMaterial = base.NormalMaterial;
				if (!this._hovered)
				{
					rendererDefinition._renderer.sharedMaterial = base.NormalMaterial;
				}
			}
			if (state == DecayingInventoryItemView.DecayStates.None || state >= DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				LocalPlayer.ItemDecayMachine.CancelCommandFor(this);
				this.DecayProperties._decayDoneTime = Time.time - this._decayStartTime;
			}
			this.ActiveBonus = ((state >= DecayingInventoryItemView.DecayStates.DriedFresh) ? WeaponStatUpgrade.Types.DriedFood : WeaponStatUpgrade.Types.RawFood);
			this._canDropFromInventory = ((this.DecayProperties._state == DecayingInventoryItemView.DecayStates.Spoilt || this.DecayProperties._state == DecayingInventoryItemView.DecayStates.DriedSpoilt) && base.ItemCache.MatchType(Item.Types.Droppable));
		}

		
		public override void UseEdible()
		{
			InventoryItemView.CombiningItemId = -1;
			LocalPlayer.Sfx.PlayWhoosh();
			int calories = 0;
			switch (this.DecayProperties._state)
			{
			case DecayingInventoryItemView.DecayStates.Spoilt:
				LocalPlayer.Stats.Invoke("PoisonMe", 2f);
				break;
			default:
				LocalPlayer.Stats.AteFreshMeat(this._isLimb, this._size, calories);
				break;
			case DecayingInventoryItemView.DecayStates.DriedEdible:
				LocalPlayer.Stats.AteEdibleMeat(this._isLimb, this._size, calories);
				break;
			case DecayingInventoryItemView.DecayStates.DriedSpoilt:
				LocalPlayer.Stats.AteSpoiltMeat(this._isLimb, this._size, calories);
				break;
			}
			if (this.DecayProperties._state >= DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				LocalPlayer.Stats.Thirst += 0.05f * GameSettings.Survival.DriedMeatThirstRatio;
			}
			this._inventory.BubbleUpInventoryView(this);
			this._inventory.RemoveItem(this._itemId, 1, false, true);
			EventRegistry.Player.Publish(TfEvent.UsedItem, this._itemId);
			ItemUtils.ApplyEffectsToStats(base.ItemCache._usedStatEffect, true, 1);
			if (base.ItemCache._usedSFX != Item.SFXCommands.None)
			{
				this._inventory.SendMessage("PlayInventorySound", base.ItemCache._usedSFX);
			}
			this.ActiveBonus = WeaponStatUpgrade.Types.RawFood;
		}

		
		public Material GetMaterialForState(DecayingInventoryItemView.DecayStates state)
		{
			int num = Mathf.Clamp((int)state, 0, this._decayStatesMats.Length - 1);
			return this._decayStatesMats[num];
		}

		
		public void InitDataToSave(ActiveBonusMassSaver.ViewData viewData)
		{
			viewData._intValue1 = (int)this.DecayProperties._state;
			viewData._floatValue1 = (float)((int)this.DecayProperties._decayDoneTime);
		}

		
		public void ApplySavedData(ActiveBonusMassSaver.ViewData viewData)
		{
			if (viewData != null)
			{
				this.DecayProperties._decayDoneTime = viewData._floatValue1;
				this.SetDecayState((DecayingInventoryItemView.DecayStates)viewData._intValue1);
			}
			else
			{
				this.DecayProperties._decayDoneTime = 0f;
				this.DecayProperties._state = DecayingInventoryItemView.DecayStates.None;
			}
		}

		
		
		public override ItemProperties Properties
		{
			get
			{
				if (this._properties == null)
				{
					this._properties = new DecayingInventoryItemView.DecayingItemProperties
					{
						ActiveBonus = (WeaponStatUpgrade.Types)(-1)
					};
					((DecayingInventoryItemView.DecayingItemProperties)this._properties).StateChanged += this.SetDecayState;
				}
				return this._properties;
			}
		}

		
		
		public DecayingInventoryItemView.DecayingItemProperties DecayProperties
		{
			get
			{
				return this.Properties as DecayingInventoryItemView.DecayingItemProperties;
			}
		}

		
		
		
		public static DecayingInventoryItemView LastUsed { get; set; }

		
		public DecayingInventoryItemView.DecayStates _prevState;

		
		[NameFromEnumIndex(typeof(DecayingInventoryItemView.DecayStates))]
		public Material[] _decayStatesMats = new Material[4];

		
		public float _decayDelay = 10f;

		
		public bool _isLimb;

		
		public float _size;

		
		private float _decayStartTime;

		
		private bool _initDone;

		
		public enum DecayStates
		{
			
			None,
			
			Fresh,
			
			Edible,
			
			Spoilt,
			
			DriedFresh,
			
			DriedEdible,
			
			DriedSpoilt
		}

		
		[Serializable]
		public class DecayingItemProperties : ItemProperties
		{
			
			
			
			public event Action<DecayingInventoryItemView.DecayStates> StateChanged = delegate
			{
			};

			
			public override bool Match(ItemProperties other)
			{
				return other == ItemProperties.Any || !(other is DecayingInventoryItemView.DecayingItemProperties) || other.ActiveBonus == (WeaponStatUpgrade.Types)(-2) || (base.ActiveBonus == other.ActiveBonus && this._state == ((DecayingInventoryItemView.DecayingItemProperties)other)._state);
			}

			
			public override void Copy(ItemProperties other)
			{
				if (other != ItemProperties.Any)
				{
					base.ActiveBonus = other.ActiveBonus;
					this.ActiveBonusValue = other.ActiveBonusValue;
					DecayingInventoryItemView.DecayingItemProperties decayingItemProperties = other as DecayingInventoryItemView.DecayingItemProperties;
					if (decayingItemProperties != null)
					{
						this._state = decayingItemProperties._state;
						this._decayDoneTime = decayingItemProperties._decayDoneTime;
						this.StateChanged(this._state);
					}
				}
				else
				{
					base.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
					this.ActiveBonusValue = 0f;
				}
			}

			
			public override void Copy(ItemInfo coopItemInfo)
			{
				base.ActiveBonus = (WeaponStatUpgrade.Types)coopItemInfo.Bonus;
				this.ActiveBonusValue = coopItemInfo.BonusValue;
				this._state = (DecayingInventoryItemView.DecayStates)coopItemInfo.IntVal1;
				this._decayDoneTime = coopItemInfo.FloatVal1;
			}

			
			public override void Fill(PlayerAddItem pai)
			{
				base.Fill(pai);
				pai.IntVal1 = (int)this._state;
				pai.FloatVal1 = this._decayDoneTime;
				pai.ItemPropertiesType = 1;
			}

			
			public override void Fill(ItemInfo ii)
			{
				base.Fill(ii);
				ii.IntVal1 = (int)this._state;
				ii.FloatVal1 = this._decayDoneTime;
			}

			
			public override ItemProperties Clone()
			{
				return new DecayingInventoryItemView.DecayingItemProperties
				{
					ActiveBonus = base.ActiveBonus,
					ActiveBonusValue = this.ActiveBonusValue,
					_state = this._state,
					_decayDoneTime = this._decayDoneTime
				};
			}

			
			[SerializeThis]
			public DecayingInventoryItemView.DecayStates _state;

			
			[SerializeThis]
			public float _decayDoneTime;
		}
	}
}
