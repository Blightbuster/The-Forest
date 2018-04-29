using System;
using UnityEngine;

namespace TheForest.Items.Inventory
{
	
	[AddComponentMenu("Items/Inventory/Held Item Identifier")]
	public class HeldItemIdentifier : MonoBehaviour
	{
		
		[ItemIdPicker]
		public int _itemId;
	}
}
