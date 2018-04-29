using System;
using TheForest.Items.Inventory;

namespace TheForest.Items.Core
{
	
	public interface IItemStorage
	{
		
		bool IsValidItem(Item itemId);

		
		
		bool IsEmpty { get; }

		
		int Add(int itemId, int amount = 1, ItemProperties properties = null);

		
		int Remove(int itemId, int amount = 1, ItemProperties properties = null);

		
		void Open();

		
		void Close();
	}
}
