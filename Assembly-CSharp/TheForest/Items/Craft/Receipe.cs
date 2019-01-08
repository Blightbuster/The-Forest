using System;
using System.Collections.Generic;
using TheForest.Utils;
using UniLinq;

namespace TheForest.Items.Craft
{
	[Serializable]
	public class Receipe
	{
		public string IngredientHash
		{
			get
			{
				if (string.IsNullOrEmpty(this.hash))
				{
					this.hash = Receipe.IngredientsToRecipeHash(this._ingredients);
				}
				return this.hash;
			}
		}

		public bool CanCarryProduct { get; set; }

		public static string IngredientsToRecipeHash(IEnumerable<ReceipeIngredient> ingredients)
		{
			return string.Join("_", (from i in ingredients
			orderby i._itemID
			select i._itemID.ToString()).ToArray<string>());
		}

		public bool HasIngredient(ReceipeIngredient searchIngredient)
		{
			return this._ingredients != null && this._ingredients.Any((ReceipeIngredient eachIngredient) => searchIngredient._itemID == eachIngredient._itemID);
		}

		public int _id;

		public Receipe.Types _type;

		public string _name;

		public bool _batchUpgrade;

		public Item.Types _productItemType = (Item.Types)(-1);

		public int _productItemID;

		public RandomRange _productItemAmount;

		public ReceipeIngredient[] _ingredients;

		public WeaponStatUpgrade[] _weaponStatUpgrades;

		public bool _hidden;

		public bool _forceUnique;

		private string hash;

		public enum Types
		{
			Craft,
			Upgrade,
			Extension
		}
	}
}
