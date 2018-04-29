using System;
using UnityEngine;


public class BoltPlayerOnPlaneSetup : MonoBehaviour
{
	
	
	private bool ReceivedAllVariations
	{
		get
		{
			return this.playerVariation > -1 && this.playerVariationTShirtType > -1 && this.playerVariationTShirtMat > -1 && this.playerVariationPantsType > -1 && this.playerVariationPantsMat > -1;
		}
	}

	
	private void PlayerVariationTShirtTypeSetup(int variationNumber)
	{
		this.playerVariationTShirtType = variationNumber;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerVariationTShirtMatSetup(int variationNumber)
	{
		this.playerVariationTShirtMat = variationNumber;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerVariationPantsTypeSetup(int variationNumber)
	{
		this.playerVariationPantsType = variationNumber;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerVariationPantsMatSetup(int variationNumber)
	{
		this.playerVariationPantsMat = variationNumber;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerVariationSetup(int variationNumber)
	{
		this.playerVariation = variationNumber;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerClothingSetup(PlayerCloting clothing)
	{
		this.playerClothing = clothing;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerClothingVariationSetup(int clothingVariation)
	{
		this.playerClothingVariation = clothingVariation;
		if (this.ReceivedAllVariations)
		{
			CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
			component.SetVariation(this.playerVariation, this.playerVariationTShirtType, this.playerVariationTShirtMat, this.playerVariationPantsType, this.playerVariationPantsMat, this.playerVariationHair, this.playerClothing, this.playerClothingVariation);
			component.UpdateSkinVariation(false, false, false, false);
		}
	}

	
	private void PlayerVariationSetupClean(int variationNumber)
	{
		CoopPlayerVariations component = base.GetComponent<CoopPlayerVariations>();
		component.UpdateSkinVariation(false, false, false, false);
	}

	
	private int playerVariation = -1;

	
	private int playerVariationTShirtType = -1;

	
	private int playerVariationTShirtMat = -1;

	
	private int playerVariationPantsType = -1;

	
	private int playerVariationPantsMat = -1;

	
	private int playerVariationHair = -1;

	
	private PlayerCloting playerClothing;

	
	private int playerClothingVariation;
}
