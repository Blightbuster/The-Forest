using System;
using System.Collections.Generic;
using TheForest.Items;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class IngredientInfo : MonoBehaviour
	{
		[ItemIdPicker(Item.Types.Equipment)]
		public int ItemId;

		public Material Material;

		public List<GameObject> Objects;

		public float CostMultiplier = 1f;
	}
}
