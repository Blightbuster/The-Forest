using System;
using UnityEngine;


[ExecuteInEditMode]
public class InventoryImageEffectOptimizer : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.MyCamera = base.gameObject.GetComponent<Camera>();
		this.ssao = base.GetComponent<SESSAO>();
		this.bleedEffect = base.GetComponent<BleedBehavior>();
	}

	
	private void Update()
	{
		TheForestQualitySettings.SSAOTechnique ssaotechnique = TheForestQualitySettings.UserSettings.SSAO;
		if (SystemInfo.systemMemorySize <= 4096 || PlayerPreferences.LowMemoryMode)
		{
			ssaotechnique = TheForestQualitySettings.SSAOTechnique.Off;
		}
		if (this.ssao)
		{
			this.ssao.enabled = (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			TheForestQualitySettings.SSAOTechnique ssaotechnique2 = ssaotechnique;
			if (ssaotechnique2 != TheForestQualitySettings.SSAOTechnique.Ultra)
			{
				if (ssaotechnique2 == TheForestQualitySettings.SSAOTechnique.High)
				{
					this.ssao.halfSampling = true;
				}
			}
			else
			{
				this.ssao.halfSampling = false;
			}
		}
		if (this.bleedEffect)
		{
			if (Application.isPlaying)
			{
				this.bleedEffect.enabled = (BleedBehavior.BloodAmount > 0f);
			}
			else
			{
				this.bleedEffect.enabled = (this.bleedEffect.TestingBloodAmount > 0f);
			}
		}
	}

	
	private Camera MyCamera;

	
	private SESSAO ssao;

	
	private BleedBehavior bleedEffect;
}
