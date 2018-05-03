using System;
using UnityEngine;
using UnityEngine.PostProcessing;


public class InventoryImageEffectOptimizer : MonoBehaviour
{
	
	private void Awake()
	{
		this.MyCamera = base.gameObject.GetComponent<Camera>();
		this.amplifyOcclusion = base.GetComponent<AmplifyOcclusionEffect>();
		this.bleedEffect = base.GetComponent<BleedBehavior>();
		PostProcessingBehaviour component = base.GetComponent<PostProcessingBehaviour>();
		if (component != null && component.profile != null && InventoryImageEffectOptimizer._instancedProfile == null)
		{
			component.profile = UnityEngine.Object.Instantiate<PostProcessingProfile>(component.profile);
			InventoryImageEffectOptimizer._instancedProfile = component.profile;
		}
	}

	
	private void Update()
	{
		TheForestQualitySettings.SSAOTechnique ssaotechnique = TheForestQualitySettings.UserSettings.SSAO;
		if (SystemInfo.systemMemorySize <= 4096 || PlayerPreferences.LowMemoryMode)
		{
			ssaotechnique = TheForestQualitySettings.SSAOTechnique.Off;
		}
		if (this.amplifyOcclusion && (TheForestQualitySettings.UserSettings.SSAOType == TheForestQualitySettings.SSAOTypes.AMPLIFY || TheForestQualitySettings.UserSettings.SSAOType == TheForestQualitySettings.SSAOTypes.UNITY || !InventoryImageEffectOptimizer._instancedProfile))
		{
			this.amplifyOcclusion.enabled = (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			if (InventoryImageEffectOptimizer._instancedProfile)
			{
				InventoryImageEffectOptimizer._instancedProfile.ambientOcclusion.enabled = false;
			}
			if (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Ultra)
			{
				if (ssaotechnique != TheForestQualitySettings.SSAOTechnique.High)
				{
					if (ssaotechnique == TheForestQualitySettings.SSAOTechnique.Low)
					{
						this.amplifyOcclusion.SampleCount = AmplifyOcclusionBase.SampleCountLevel.Low;
					}
				}
				else
				{
					this.amplifyOcclusion.SampleCount = AmplifyOcclusionBase.SampleCountLevel.High;
				}
			}
			else
			{
				this.amplifyOcclusion.SampleCount = AmplifyOcclusionBase.SampleCountLevel.VeryHigh;
			}
		}
		else if (InventoryImageEffectOptimizer._instancedProfile && !this.amplifyOcclusion)
		{
			InventoryImageEffectOptimizer._instancedProfile.ambientOcclusion.enabled = (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			if (this.amplifyOcclusion)
			{
				this.amplifyOcclusion.enabled = false;
			}
			AmbientOcclusionModel.Settings settings = InventoryImageEffectOptimizer._instancedProfile.ambientOcclusion.settings;
			if (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Ultra)
			{
				if (ssaotechnique != TheForestQualitySettings.SSAOTechnique.High)
				{
					if (ssaotechnique == TheForestQualitySettings.SSAOTechnique.Low)
					{
						settings.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
					}
				}
				else
				{
					settings.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
				}
			}
			else
			{
				settings.sampleCount = AmbientOcclusionModel.SampleCount.High;
			}
			InventoryImageEffectOptimizer._instancedProfile.ambientOcclusion.settings = settings;
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

	
	private AmplifyOcclusionEffect amplifyOcclusion;

	
	private BleedBehavior bleedEffect;

	
	private static PostProcessingProfile _instancedProfile;
}
