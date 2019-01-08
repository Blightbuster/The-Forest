using System;
using UnityEngine;

namespace TheForest.Player.Clothing
{
	public class ClothingItemIdPicker : PropertyAttribute
	{
		public ClothingItemIdPicker()
		{
			this.Type = (ClothingItem.DisplayTypes)(-1);
			this.Restricted = false;
		}

		public ClothingItemIdPicker(ClothingItem.DisplayTypes restrictedToType)
		{
			this.Type = restrictedToType;
			this.Restricted = true;
		}

		public ClothingItem.DisplayTypes Type { get; set; }

		public bool Restricted { get; private set; }
	}
}
