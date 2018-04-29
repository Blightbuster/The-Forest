using System;
using TheForest.Items.Craft;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/Active Bonus Listener (Material switch)")]
	[DoNotSerializePublic]
	public class ActiveBonusListenerMat : MonoBehaviour
	{
		
		private void Awake()
		{
			for (int i = 0; i < this._itemViews.Length; i++)
			{
				this._itemViews[i].Properties.ActiveBonusChanged += this.OnActiveBonusChanged;
			}
		}

		
		private void LateUpdate()
		{
			for (int i = 0; i < this._itemViews.Length; i++)
			{
				this.ToggleItemViewMat(this._itemViews[i]);
			}
			base.enabled = false;
		}

		
		private void OnDestroy()
		{
			for (int i = 0; i < this._itemViews.Length; i++)
			{
				this._itemViews[i].Properties.ActiveBonusChanged -= this.OnActiveBonusChanged;
			}
		}

		
		public void ToggleItemViewMat(InventoryItemView iiv)
		{
			if (iiv.ItemCache != null)
			{
				Material material = this._defaultMaterial;
				for (int i = 0; i < this._bonuses.Length; i++)
				{
					if (iiv.ActiveBonus == this._bonuses[i]._bonusToActivate)
					{
						material = this._bonuses[i]._material;
						break;
					}
				}
				if (iiv._renderers[0]._defaultMaterial != material)
				{
					iiv._renderers[0]._defaultMaterial = material;
					iiv.Highlight(false);
				}
				return;
			}
		}

		
		private void OnActiveBonusChanged(WeaponStatUpgrade.Types bonus)
		{
			base.enabled = true;
		}

		
		public InventoryItemView[] _itemViews;

		
		public Material _defaultMaterial;

		
		[NameFromProperty("_bonusToActivate", 0)]
		public ActiveBonusListenerMat.ArrowBonusMat[] _bonuses;

		
		[Serializable]
		public class ArrowBonusMat
		{
			
			public Material _material;

			
			public WeaponStatUpgrade.Types _bonusToActivate;
		}
	}
}
