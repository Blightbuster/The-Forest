using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class playerStealthMeter : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		this.setup = LocalPlayer.ScriptSetup;
		base.enabled = false;
		while (!this.vis)
		{
			this.vis = LocalPlayer.Transform.GetComponent<visRangeSetup>();
			yield return null;
		}
		base.enabled = true;
		yield break;
	}

	
	private void Update()
	{
		if (LocalPlayer.Animator == null || Scene.HudGui == null)
		{
			return;
		}
		if (LocalPlayer.Animator.GetFloat("crouch") > 5f && !this.vis.currentlyTargetted && PlayerPreferences.ShowStealthMeter)
		{
			Scene.HudGui.EyeIcon.SetActive(true);
		}
		else
		{
			Scene.HudGui.EyeIcon.SetActive(false);
		}
		if (Scene.HudGui.EyeIcon.activeSelf)
		{
			float num = (100f - this.vis.modVisRange) / 100f;
			num = 1f - Mathf.Clamp(num, 0f, 1f);
			if (num < 0.1f)
			{
				num = 0f;
			}
			if (this.vis.currentlyTargetted)
			{
				Scene.HudGui.EyeIconFill1.fillAmount = 1f;
				Scene.HudGui.EyeIconFill2.fillAmount = 1f;
			}
			else
			{
				Scene.HudGui.EyeIconFill1.fillAmount = Mathf.SmoothDamp(Scene.HudGui.EyeIconFill1.fillAmount, num, ref this.fillVelocity, 0.45f);
				Scene.HudGui.EyeIconFill2.fillAmount = Scene.HudGui.EyeIconFill1.fillAmount;
			}
		}
		else
		{
			this.fillVelocity = 0f;
		}
	}

	
	private playerScriptSetup setup;

	
	private visRangeSetup vis;

	
	public float stealthValue = 100f;

	
	public float treeDensity = 1f;

	
	private float fillVelocity;
}
