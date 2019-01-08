using System;
using TheForest.Items;

namespace TheForest.SerializableTaskSystem
{
	[Serializable]
	public class ItemIdList : ACondition
	{
		public override void Init()
		{
		}

		[ItemIdPicker]
		public int[] _itemIds;
	}
}
