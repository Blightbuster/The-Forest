using System;
using TheForest.UI;
using UnityEngine;


public class HintText : MonoBehaviour
{
	
	private void Awake()
	{
		int num = UnityEngine.Random.Range(0, this.texts.Length - 1);
		this.name = UiTranslationDatabase.TranslateKey("HINT_" + num, this.texts[num], true);
		this.MyLabel = base.gameObject.GetComponent<UILabel>();
		this.MyLabel.text = this.name;
	}

	
	private string[] texts = new string[]
	{
		"HINT: COVER YOURSELF IN MUD TO HIDE FROM ENEMIES",
		"HINT: BEING COLD WILL DRAIN YOUR ENERGY",
		"HINT: KEEP YOUR ENERGY UP BY EATING REGULARLY",
		"HINT: YOU CAN REGAIN ENERGY BY RESTING ON BENCH",
		"HINT: EXPLOSIVES AND MOLOTOVS ARE EFFECTIVE AGAINST MOST ENEMIES",
		"HINT: TRY UPGRADING YOUR WEAPONS",
		"HINT: YOU CAN MAKE ARMOR OUT OF LIZARD SKIN",
		"HINT: LIT EFFIGIES WILL SCARE ENEMIES AWAY FROM YOUR CAMP",
		"HINT: HIDE IN BUSHES TO ESCAPE FROM ENEMIES",
		"HINT: BUILD A SHELTER TO SAVE YOUR GAME",
		"HINT: FIND A POT TO BOIL WATER"
	};

	
	private new string name;

	
	private UILabel MyLabel;
}
