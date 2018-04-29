using System;
using UnityEngine;

namespace TheForest.Items
{
	
	public class ItemTypeFilter : PropertyAttribute
	{
		
		public ItemTypeFilter(Item.Types type)
		{
			this.Type = type;
		}

		
		
		
		public Item.Types Type { get; private set; }
	}
}
