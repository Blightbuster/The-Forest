using System;
using UnityEngine;

namespace TheForest.Player.Clothing
{
	
	[Serializable]
	public class ClothingItem
	{
		
		public bool CompatibleWith(ClothingItem.SlotTypes otherItemType)
		{
			return (this._slotType & otherItemType) == (ClothingItem.SlotTypes)0;
		}

		
		public void ValidateProperties()
		{
			this.RefreshSlotType();
		}

		
		private void RefreshSlotType()
		{
			this._slotType = (ClothingItem.SlotTypes)0;
			ClothingItem.DisplayTypes displayType = this._displayType;
			switch (displayType)
			{
			case ClothingItem.DisplayTypes.TopPartial_Hands:
			case ClothingItem.DisplayTypes.TopPartial_Arms:
			case ClothingItem.DisplayTypes.TopFull_Hands:
			case ClothingItem.DisplayTypes.TopFull_Arms:
				break;
			default:
				if (displayType != ClothingItem.DisplayTypes.TShirt)
				{
					if (displayType == ClothingItem.DisplayTypes.Pants)
					{
						this._slotType = ClothingItem.SlotTypes.Bottom;
						return;
					}
					if (displayType == ClothingItem.DisplayTypes.Hat)
					{
						this._slotType = ClothingItem.SlotTypes.Hat;
						return;
					}
					if (displayType != ClothingItem.DisplayTypes.FullBody)
					{
						return;
					}
					this._slotType = ClothingItem.SlotTypes.Exclusive;
					return;
				}
				break;
			}
			this._slotType = ClothingItem.SlotTypes.Top;
		}

		
		public int _id;

		
		[Header("Core")]
		[EnumFlags]
		public ClothingItem.DisplayTypes _displayType;

		
		[EnumFlags]
		public ClothingItem.SlotTypes _slotType;

		
		public bool _hideShoes;

		
		public bool _hidePants;

		
		public string _name;

		
		public string _displayName;

		
		public string _translationKey;

		
		public Mesh[] _meshLods;

		
		public Material[] _materials;

		
		public MaterialSet[] _materialVariations;

		
		public Material[] _materialsRed;

		
		public ClothingItem.BoneGroup _boneGroup;

		
		[Header("Outfit changes")]
		public int _baseRollChance = 20;

		
		public int _ownedRollChanceReduction = 19;

		
		[Flags]
		public enum DisplayTypes
		{
			
			TopPartial_Hands = 1,
			
			TopPartial_Arms = 2,
			
			TopFull_Hands = 4,
			
			TopFull_Arms = 8,
			
			TShirt = 16,
			
			Pants = 32,
			
			Hat = 64,
			
			FullBody = 128
		}

		
		public enum DisplayTypesOld
		{
			
			TopPartial_Hands = 1,
			
			TopPartial_Arms,
			
			TopFull_Hands,
			
			TopFull_Arms,
			
			TShirt,
			
			Hat,
			
			Pants,
			
			FullBody
		}

		
		[Flags]
		public enum SlotTypes
		{
			
			Top = 1,
			
			Bottom = 2,
			
			Hat = 4,
			
			Exclusive = 8
		}

		
		public enum BoneGroup
		{
			
			Beanies,
			
			Tops,
			
			JacketAndSuit,
			
			Vest,
			
			Pants,
			
			ShirtLow1,
			
			ShirtLow2,
			
			ShirtLow3,
			
			StewardessDress,
			
			BathRobePants,
			
			Bathrobe,
			
			TennisPlayer,
			
			Pilot
		}
	}
}
