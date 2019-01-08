using System;
using TheForest.Items.Craft;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	[AddComponentMenu("Items/Inventory/Held Item Identifier")]
	public class HeldItemIdentifier : MonoBehaviour
	{
		public ItemProperties Properties
		{
			get
			{
				if (this._properties == null)
				{
					this._properties = new ItemProperties
					{
						ActiveBonus = (WeaponStatUpgrade.Types)(-1)
					};
				}
				return this._properties;
			}
		}

		[ItemIdPicker]
		public int _itemId;

		protected ItemProperties _properties;
	}
}
