using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;

public class fixDeadCamHair : MonoBehaviour
{
	private void OnEnable()
	{
		for (int i = 0; i < this.hair.Length; i++)
		{
			if (this.hair[i] != null)
			{
				this.hair[i].shadowCastingMode = ShadowCastingMode.On;
			}
		}
		if (ForestVR.Enabled)
		{
			Camera component = base.GetComponent<Camera>();
			if (component == null)
			{
				return;
			}
			component.renderingPath = RenderingPath.Forward;
			foreach (Renderer renderer in LocalPlayer.vrAdapter.PlayerShadowOnly)
			{
				renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				Debug.Log("set to shadows only");
			}
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < this.hair.Length; i++)
		{
			if (this.hair[i] != null)
			{
				this.hair[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
			}
		}
	}

	public Renderer[] hair;
}
