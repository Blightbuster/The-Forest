using System;
using TheForest.UI;
using UnityEngine;


public class AlphaCountdown : MonoBehaviour
{
	
	private void Awake()
	{
		this.Ibl = base.GetComponent<UILabel>();
	}

	
	private void Update()
	{
		DateTime d = new DateTime(this.Year, this.Month, this.Day, this.Hour, 0, 0);
		this.TimeRemaining = d - DateTime.UtcNow + new TimeSpan(8, 0, 0);
		if (this.TimeRemaining.TotalSeconds > 0.0)
		{
			this.Ibl.text = string.Format(UiTranslationDatabase.TranslateKey(this.TimeFormatKey, "{0} DAYS {1} HOURS {2} MINUTES {3} SECONDS", false), new object[]
			{
				this.TimeRemaining.Days,
				this.TimeRemaining.Hours,
				this.TimeRemaining.Minutes,
				this.TimeRemaining.Seconds
			});
		}
		else
		{
			this.Ibl.text = UiTranslationDatabase.TranslateKey(this.TimeElapsedText, "NEW PATCH RELEASING SOON             ", false);
			this.Ibl.color = this.TimeElapsedColor;
		}
	}

	
	[HideInInspector]
	public TimeSpan TimeRemaining;

	
	[Range(2014f, 2020f)]
	public int Year;

	
	[Range(1f, 12f)]
	public int Month;

	
	[Range(1f, 31f)]
	public int Day;

	
	[Range(0f, 23f)]
	public int Hour = 9;

	
	public string TimeFormatKey = "ALPHA_COUNT_DOWN";

	
	public string TimeElapsedText = "ALPHA_COUNT_DOWN_ELAPSED";

	
	public Color TimeElapsedColor;

	
	private UILabel Ibl;
}
