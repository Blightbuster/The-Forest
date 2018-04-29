using System;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI.Crafting
{
	
	public class ReceipeProductView : MonoBehaviour
	{
		
		public bool ShowReceipe(Receipe receipe, HudGui.InventoryItemInfo iii)
		{
			if (iii != null)
			{
				this._icon.mainTexture = iii._icon;
				this._icon.enabled = true;
			}
			else
			{
				this._icon.enabled = false;
			}
			if (receipe._type == Receipe.Types.Craft)
			{
				this._title.text = ((iii == null) ? ItemDatabase.ItemById(receipe._productItemID)._name : UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, false));
				if (receipe._productItemAmount > 1)
				{
					UILabel title = this._title;
					title.text = title.text + " x" + receipe._productItemAmount;
				}
			}
			else
			{
				if (!this.CheckUpgradeBonus(receipe))
				{
					int itemID = receipe._ingredients[1]._itemID;
					string name = ItemDatabase.ItemById(itemID)._name;
					string text = name;
					switch (text)
					{
					case "Feather":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_SPEED_", "+Speed {0}", false);
						goto IL_3BA;
					case "Tooth":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_DAMAGE_SPEED_", "+Damage -Speed {0}", false);
						goto IL_3BA;
					case "Booze":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_DAMAGE_", "+Damage {0}", false);
						goto IL_3BA;
					case "TreeSap":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_STICKY_", "Sticky {0}", false);
						goto IL_3BA;
					case "Cloth":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_BURNING_", "Burning {0}", false);
						goto IL_3BA;
					case "OrangePaint":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_ORANGE_", "Orange {0}", false);
						goto IL_3BA;
					case "BluePaint":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_BLUE_", "Blue {0}", false);
						goto IL_3BA;
					case "Battery":
						this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_RECHARGED_", "Recharged {0}", false);
						goto IL_3BA;
					case "PlasticTorch":
						this._title.text = UiTranslationDatabase.TranslateKey("EXTENSION_LIGHT", "{0} with light", false);
						goto IL_3BA;
					case "Cassette 1":
					case "Cassette 2":
					case "Cassette 3":
					case "Cassette 4":
					case "Cassette 5":
						this._title.text = UiTranslationDatabase.TranslateKey(Scene.HudGui.InventoryItemsInfoCache[itemID]._titleTextTranslationKey, Scene.HudGui.InventoryItemsInfoCache[itemID]._titleText, false) + " {0}";
						goto IL_3BA;
					case "CamcorderTape1":
					case "CamcorderTape2":
					case "CamcorderTape3":
					case "CamcorderTape4":
					case "CamcorderTape5":
					case "CamcorderTape6":
						return false;
					}
					this._title.text = "{0}";
				}
				IL_3BA:
				this._title.text = string.Format(this._title.text, (iii == null) ? ItemDatabase.ItemById(receipe._productItemID)._name : UiTranslationDatabase.TranslateKey(iii._titleTextTranslationKey, iii._titleText, false));
			}
			return true;
		}

		
		private bool CheckUpgradeBonus(Receipe r)
		{
			if (r._weaponStatUpgrades != null && r._weaponStatUpgrades.Length > 0)
			{
				WeaponStatUpgrade.Types type = r._weaponStatUpgrades[0]._type;
				switch (type)
				{
				case WeaponStatUpgrade.Types.PoisonnedAmmo:
				case WeaponStatUpgrade.Types.PoisonnedWeapon:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_POISONED_", "Poisoned {0}", false);
					return true;
				case WeaponStatUpgrade.Types.BurningWeaponExtra:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_EXTRA_BURNING_", "Extra burning {0}", false);
					return true;
				case WeaponStatUpgrade.Types.Incendiary:
					break;
				default:
					if (type != WeaponStatUpgrade.Types.BurningAmmo)
					{
						return false;
					}
					break;
				case WeaponStatUpgrade.Types.BoneAmmo:
					this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_BONE_", "Bone {0}", false);
					return true;
				}
				this._title.text = UiTranslationDatabase.TranslateKey("UPGRADE_INCENDIARY_", "Incendiary {0}", false);
				return true;
			}
			return false;
		}

		
		public UITexture _icon;

		
		public UILabel _title;
	}
}
