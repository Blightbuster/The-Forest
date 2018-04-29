using System;

namespace TheForest.Items.Inventory
{
	
	public interface ISaveableView
	{
		
		void InitDataToSave(ActiveBonusMassSaver.ViewData viewData);

		
		void ApplySavedData(ActiveBonusMassSaver.ViewData viewData);
	}
}
