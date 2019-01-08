using System;

namespace TheForest.Items.Craft.Interfaces
{
	public interface ICraftOverride
	{
		bool CanCombine();

		void Combine();

		bool IsCombining { get; }

		Item.Types AcceptedTypes { get; }
	}
}
