using System;
using UnityEngine;


public class GetThemeColor : MonoBehaviour
{
	
	private void Start()
	{
		this.spr = base.gameObject.GetComponent<UISprite>();
		this.sspr = base.gameObject.GetComponent<UISprite>();
		if (this.spr != null)
		{
			this.spr.color = new Color32(GetThemeColor.COLOR_R, GetThemeColor.COLOR_G, GetThemeColor.COLOR_B, byte.MaxValue);
		}
		if (this.sspr != null)
		{
			this.sspr.color = new Color32(GetThemeColor.COLOR_R, GetThemeColor.COLOR_G, GetThemeColor.COLOR_B, byte.MaxValue);
		}
	}

	
	private void Update()
	{
		if (this.realTime)
		{
			if (this.spr != null)
			{
				this.spr.color = new Color32(GetThemeColor.COLOR_R, GetThemeColor.COLOR_G, GetThemeColor.COLOR_B, byte.MaxValue);
			}
			if (this.sspr != null)
			{
				this.sspr.color = new Color32(GetThemeColor.COLOR_R, GetThemeColor.COLOR_G, GetThemeColor.COLOR_B, byte.MaxValue);
			}
		}
	}

	
	public bool realTime = true;

	
	public static byte COLOR_R = byte.MaxValue;

	
	public static byte COLOR_G = byte.MaxValue;

	
	public static byte COLOR_B = byte.MaxValue;

	
	private UISprite spr;

	
	private UISprite sspr;
}
