using System;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI.Crafting
{
	
	public class ReceipeProductView : MonoBehaviour
	{
		
		public bool ShowReceipe(Receipe recipe, HudGui.InventoryItemInfo iii)
		{
			if (recipe._type == Receipe.Types.Craft)
			{
				this._title.text = ((iii == null) ? ItemDatabase.ItemById(recipe._productItemID)._name : UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, true));
				if (recipe._productItemAmount > 1)
				{
					UILabel title = this._title;
					title.text = title.text + " x" + recipe._productItemAmount;
				}
			}
			else
			{
				bool flag;
				if (!this.CheckUpgradeBonus(recipe, out flag))
				{
					int itemID = recipe._ingredients[1]._itemID;
					string name = ItemDatabase.ItemById(itemID)._name;
					switch (name)
					{
					case "Feather":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_SPEED_", "+Speed {0}", true);
						iii = this._weaponFakeInfo;
						goto IL_3AA;
					case "Tooth":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_DAMAGE_SPEED_", "+Damage -Speed {0}", true);
						iii = this._weaponFakeInfo;
						goto IL_3AA;
					case "Booze":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_DAMAGE_", "+Damage {0}", true);
						iii = this._weaponFakeInfo;
						goto IL_3AA;
					case "TreeSap":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_STICKY_", "Sticky {0}", true);
						goto IL_3AA;
					case "Cloth":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_BURNING_", "Burning {0}", true);
						iii = this._weaponFakeInfo;
						goto IL_3AA;
					case "OrangePaint":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_ORANGE_", "Orange {0}", true);
						goto IL_3AA;
					case "BluePaint":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_BLUE_", "Blue {0}", true);
						goto IL_3AA;
					case "Battery":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_RECHARGED_", "Recharged {0}", true);
						goto IL_3AA;
					case "PlasticTorch":
						this._title.text = UiTranslationDatabase.TranslateKey("EXTENSION_LIGHT", "{0} with light", true);
						goto IL_3AA;
					case "Cassette 1":
					case "Cassette 2":
					case "Cassette 3":
					case "Cassette 4":
					case "Cassette 5":
						this._title.text = UiTranslationDatabase.TranslateKey(Scene.HudGui.InventoryItemsInfoCache[itemID]._titleTextTranslationKey, Scene.HudGui.InventoryItemsInfoCache[itemID]._titleText, true) + " {0}";
						goto IL_3AA;
					case "CamcorderTape1":
					case "CamcorderTape2":
					case "CamcorderTape3":
					case "CamcorderTape4":
					case "CamcorderTape5":
					case "CamcorderTape6":
						return false;
					}
					this._title.text = "{0}";
					IL_3AA:;
				}
				else if (flag)
				{
					iii = this._weaponFakeInfo;
				}
				this._title.text = string.Format(this._title.text, (iii == null) ? ItemDatabase.ItemById(recipe._productItemID)._name : UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, true));
			}
			int i = this._ingredientTextures.Length - 1;
			foreach (ReceipeIngredient receipeIngredient in recipe._ingredients)
			{
				HudGui.InventoryItemInfo itemInfo = Scene.HudGui.GetItemInfo(receipeIngredient._itemID);
				if (itemInfo != null)
				{
					this._ingredientTextures[i].mainTexture = itemInfo._icon;
					if (receipeIngredient._amount > 1)
					{
						this._ingredientCountLabels[i].text = receipeIngredient._amount.ToString();
						this._ingredientCountLabels[i].enabled = true;
					}
					else
					{
						this._ingredientCountLabels[i].enabled = false;
					}
					if (!this._ingredientTextures[i].gameObject.activeSelf)
					{
						this._ingredientTextures[i].gameObject.SetActive(true);
					}
					i--;
				}
			}
			if (iii == this._weaponFakeInfo)
			{
				this._ingredientTextures[this._ingredientTextures.Length - 1].mainTexture = this._weaponFakeInfo._icon;
				this._ingredientCountLabels[this._ingredientTextures.Length - 1].enabled = false;
			}
			while (i >= 0)
			{
				if (this._ingredientTextures[i].gameObject.activeSelf)
				{
					this._ingredientTextures[i].gameObject.SetActive(false);
				}
				i--;
			}
			this._iconCantCarry.enabled = !recipe.CanCarryProduct;
			this.ShowCrossOff(!recipe.CanCarryProduct, ReceipeProductView._cantCarryProductColor);
			this._ingredientGrid.repositionNow = true;
			return true;
		}

		
		private void ShowCrossOff(bool value, Color color)
		{
			if (this._crossOffLine == null)
			{
				return;
			}
			this._crossOffLine.enabled = value;
			this._crossOffLine.color = color;
		}

		
		private bool CheckUpgradeBonus(Receipe r, out bool useFakeWeaponView)
		{
			useFakeWeaponView = false;
			if (r._weaponStatUpgrades != null && r._weaponStatUpgrades.Length > 0)
			{
				WeaponStatUpgrade.Types type = r._weaponStatUpgrades[0]._type;
				switch (type)
				{
				case WeaponStatUpgrade.Types.FlareGunAmmo:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE__AMMO", "{0} AMMO", true);
					useFakeWeaponView = false;
					return true;
				default:
					if (type != WeaponStatUpgrade.Types.BurningAmmo)
					{
						return false;
					}
					break;
				case WeaponStatUpgrade.Types.PoisonnedAmmo:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_POISONED_", "Poisoned {0}", true);
					return true;
				case WeaponStatUpgrade.Types.BurningWeaponExtra:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_EXTRA_BURNING_", "Extra burning {0}", true);
					return true;
				case WeaponStatUpgrade.Types.Incendiary:
					break;
				case WeaponStatUpgrade.Types.BoneAmmo:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_BONE_", "Bone {0}", true);
					return true;
				case WeaponStatUpgrade.Types.PoisonnedWeapon:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_POISONED_", "Poisoned {0}", true);
					useFakeWeaponView = true;
					return true;
				}
				this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_INCENDIARY_", "Incendiary {0}", true);
				return true;
			}
			return false;
		}

		
		public UISprite _icon;

		
		public UISprite _iconCantCarry;

		
		public UISprite _crossOffLine;

		
		public UILabel _title;

		
		public UIGrid _ingredientGrid;

		
		public UITexture[] _ingredientTextures;

		
		public UILabel[] _ingredientCountLabels;

		
		public HudGui.InventoryItemInfo _weaponFakeInfo;

		
		private static readonly Color _cantCarryProductColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
	}
}
