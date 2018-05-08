using System;
using Ceto;
using CetoTF;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.PostProcessing;


[RequireComponent(typeof(PostProcessingBehaviour))]
public class ImageEffectOptimizer : MonoBehaviour
{
	
	private void Awake()
	{
		PostProcessingBehaviour component = base.GetComponent<PostProcessingBehaviour>();
		if (component && component.profile)
		{
			component.profile = UnityEngine.Object.Instantiate<PostProcessingProfile>(component.profile);
			this.postProcessingProfile = component.profile;
		}
		Camera component2 = base.gameObject.GetComponent<Camera>();
		component2.eventMask = 0;
		this.frostEffect = base.GetComponent<Frost>();
		this.bleedEffect = base.GetComponent<BleedBehavior>();
		this.amplifyOcclusion = base.GetComponent<AmplifyOcclusionEffect>();
		this.waterViz = base.gameObject.GetComponent<WaterViz>();
		this.waterBlur = base.gameObject.GetComponent<WaterBlurEffect>();
		this.waterBlurCeto = base.gameObject.GetComponent<UnderWaterPostEffect>();
		this.farShadowCascade = base.gameObject.GetComponent<FarShadowCascade>();
		this.SunshineCam = base.GetComponent<SunshineCamera>();
		this.SunshinePP = base.GetComponent<SunshinePostprocess>();
		this.SunshineAtmosCam = base.GetComponent<TheForestAtmosphereCamera>();
		if (Sunshine.Instance)
		{
			this.SunshineOccluders = Sunshine.Instance.Occluders;
		}
	}

	
	private void Update()
	{
		if (this.postProcessingProfile)
		{
			ColorGradingModel.Settings settings = this.postProcessingProfile.colorGrading.settings;
			float num = PlayerPreferences.GammaWorldAndDay;
			float contrast = PlayerPreferences.Contrast;
			if (Clock.Dark || LocalPlayer.IsInCaves)
			{
				num = PlayerPreferences.GammaCavesAndNight;
			}
			bool flag = LocalPlayer.Inventory != null && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Pause;
			if (flag || float.IsNaN(settings.basic.postExposure) || float.IsInfinity(settings.basic.postExposure) || Mathf.Abs(num - settings.basic.postExposure) < 0.001f)
			{
				if (!settings.basic.postExposure.Equals(num))
				{
					settings.basic.postExposure = num;
				}
			}
			else
			{
				settings.basic.postExposure = Mathf.SmoothDamp(settings.basic.postExposure, num, ref this.gammaVelocity, 3f);
			}
			if (flag || float.IsNaN(settings.basic.contrast) || float.IsInfinity(settings.basic.contrast) || Mathf.Abs(contrast - settings.basic.contrast) < 0.001f)
			{
				if (!settings.basic.contrast.Equals(contrast))
				{
					settings.basic.contrast = contrast;
				}
			}
			else
			{
				settings.basic.contrast = Mathf.SmoothDamp(settings.basic.contrast, contrast, ref this.contrastVelocity, 1f);
			}
			this.postProcessingProfile.colorGrading.settings = settings;
		}
		if (this.farShadowCascade)
		{
			bool flag2 = TheForestQualitySettings.UserSettings.FarShadowMode == TheForestQualitySettings.FarShadowModes.On && !LocalPlayer.IsInClosedArea;
			if (this.farShadowCascade.enableFarShadows != flag2)
			{
				this.farShadowCascade.enableFarShadows = flag2;
			}
		}
		if (this.postProcessingProfile && PlayerPreferences.ColorGrading >= 0 && PlayerPreferences.ColorGrading < this.AmplifyColorGradients.Length)
		{
			this.postProcessingProfile.userLut.enabled = true;
			UserLutModel.Settings settings2 = this.postProcessingProfile.userLut.settings;
			settings2.lut = this.AmplifyColorGradients[PlayerPreferences.ColorGrading];
			this.postProcessingProfile.userLut.settings = settings2;
		}
		if (this.frostEffect)
		{
			this.frostEffect.enabled = (this.frostEffect.coverage > 0f);
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
		TheForestQualitySettings.AntiAliasingTechnique antiAliasingTechnique = TheForestQualitySettings.UserSettings.AntiAliasing;
		if (SystemInfo.systemMemorySize <= 4096 || PlayerPreferences.LowMemoryMode)
		{
			antiAliasingTechnique = TheForestQualitySettings.AntiAliasingTechnique.None;
		}
		if (this.postProcessingProfile)
		{
			bool flag3 = !ForestVR.Enabled && antiAliasingTechnique != TheForestQualitySettings.AntiAliasingTechnique.None;
			this.postProcessingProfile.antialiasing.enabled = flag3;
			if (flag3)
			{
				AntialiasingModel.Settings settings3 = this.postProcessingProfile.antialiasing.settings;
				if (antiAliasingTechnique != TheForestQualitySettings.AntiAliasingTechnique.FXAA)
				{
					if (antiAliasingTechnique == TheForestQualitySettings.AntiAliasingTechnique.TAA)
					{
						settings3.method = AntialiasingModel.Method.Taa;
					}
				}
				else
				{
					settings3.method = AntialiasingModel.Method.Fxaa;
				}
				this.postProcessingProfile.antialiasing.settings = settings3;
			}
		}
		if (this.postProcessingProfile)
		{
			bool flag4 = !ForestVR.Enabled && !this.SkipMotionBlur && TheForestQualitySettings.UserSettings.MotionBlur != TheForestQualitySettings.MotionBlurQuality.None;
			this.postProcessingProfile.motionBlur.enabled = flag4;
			if (flag4)
			{
				MotionBlurModel.Settings settings4 = this.postProcessingProfile.motionBlur.settings;
				switch (TheForestQualitySettings.UserSettings.MotionBlur)
				{
				case TheForestQualitySettings.MotionBlurQuality.Low:
					settings4.sampleCount = 4;
					break;
				case TheForestQualitySettings.MotionBlurQuality.Medium:
					settings4.sampleCount = 8;
					break;
				case TheForestQualitySettings.MotionBlurQuality.High:
					settings4.sampleCount = 16;
					break;
				case TheForestQualitySettings.MotionBlurQuality.Ultra:
					settings4.sampleCount = 32;
					break;
				}
				this.postProcessingProfile.motionBlur.settings = settings4;
			}
		}
		if (this.postProcessingProfile)
		{
			bool flag5 = !ForestVR.Enabled && TheForestQualitySettings.UserSettings.screenSpaceReflection == TheForestQualitySettings.ScreenSpaceReflection.On;
			flag5 = (flag5 && LocalPlayer.IsInEndgame);
			this.postProcessingProfile.screenSpaceReflection.enabled = flag5;
		}
		if (this.postProcessingProfile)
		{
		}
		if (this.postProcessingProfile)
		{
			bool enabled = !ForestVR.Enabled && TheForestQualitySettings.UserSettings.Fg == TheForestQualitySettings.FilmGrain.Normal;
			this.postProcessingProfile.grain.enabled = enabled;
		}
		if (this.postProcessingProfile)
		{
			bool enabled2 = TheForestQualitySettings.UserSettings.CA == TheForestQualitySettings.ChromaticAberration.Normal;
			this.postProcessingProfile.chromaticAberration.enabled = enabled2;
		}
		if (this.postProcessingProfile)
		{
			bool enabled3 = TheForestQualitySettings.UserSettings.SEBloom == TheForestQualitySettings.SEBloomTechnique.Normal;
			this.postProcessingProfile.bloom.enabled = enabled3;
		}
		if (Application.isPlaying)
		{
			if (!this.SunshineAtmosCam)
			{
				this.SunshineAtmosCam = base.GetComponent<TheForestAtmosphereCamera>();
				if (!this.SunshineAtmosCam)
				{
					this.SunshineAtmosCam = base.gameObject.AddComponent<TheForestAtmosphereCamera>();
				}
			}
			this.SunshineCam.enabled = true;
			this.SunshinePP.enabled = true;
			if (Sunshine.Instance)
			{
				Sunshine.Instance.enabled = true;
				Sunshine.Instance.ScatterResolution = TheForestQualitySettings.UserSettings.ScatterResolution;
				Sunshine.Instance.ScatterSamplingQuality = TheForestQualitySettings.UserSettings.ScatterSamplingQuality;
				if (this.SunshineOccluders.value != 0)
				{
					bool flag6 = TheForestQualitySettings.UserSettings.SunshineOcclusion == TheForestQualitySettings.SunshineOcclusionOn.On || LocalPlayer.IsInCaves;
					Sunshine.Instance.Occluders = ((!flag6) ? 0 : this.SunshineOccluders.value);
				}
				else
				{
					this.SunshineOccluders = Sunshine.Instance.Occluders;
				}
			}
		}
		TheForestQualitySettings.SSAOTechnique ssaotechnique = TheForestQualitySettings.UserSettings.SSAO;
		if (SystemInfo.systemMemorySize <= 4096 || PlayerPreferences.LowMemoryMode)
		{
			ssaotechnique = TheForestQualitySettings.SSAOTechnique.Off;
		}
		if (this.amplifyOcclusion && (TheForestQualitySettings.UserSettings.SSAOType == TheForestQualitySettings.SSAOTypes.AMPLIFY || !this.postProcessingProfile))
		{
			this.amplifyOcclusion.enabled = (!ForestVR.Enabled && ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			if (this.postProcessingProfile)
			{
				this.postProcessingProfile.ambientOcclusion.enabled = false;
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
		else if (this.postProcessingProfile && (TheForestQualitySettings.UserSettings.SSAOType == TheForestQualitySettings.SSAOTypes.UNITY || !this.amplifyOcclusion))
		{
			this.postProcessingProfile.ambientOcclusion.enabled = (!ForestVR.Enabled && ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			if (this.amplifyOcclusion)
			{
				this.amplifyOcclusion.enabled = false;
			}
			AmbientOcclusionModel.Settings settings5 = this.postProcessingProfile.ambientOcclusion.settings;
			if (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Ultra)
			{
				if (ssaotechnique != TheForestQualitySettings.SSAOTechnique.High)
				{
					if (ssaotechnique == TheForestQualitySettings.SSAOTechnique.Low)
					{
						settings5.sampleCount = AmbientOcclusionModel.SampleCount.Lowest;
					}
				}
				else
				{
					settings5.sampleCount = AmbientOcclusionModel.SampleCount.Medium;
				}
			}
			else
			{
				settings5.sampleCount = AmbientOcclusionModel.SampleCount.High;
			}
			this.postProcessingProfile.ambientOcclusion.settings = settings5;
		}
		if (LocalPlayer.WaterEngine)
		{
			if (LocalPlayer.IsInClosedArea)
			{
				Scene.OceanFlat.SetActive(false);
				Scene.OceanCeto.SetActive(false);
				this.CurrentOceanQuality = (TheForestQualitySettings.OceanQualities)(-1);
			}
			else if (TheForestQualitySettings.UserSettings.OceanQuality != this.CurrentOceanQuality)
			{
				this.CurrentOceanQuality = ((!PlayerPreferences.is32bit) ? TheForestQualitySettings.UserSettings.OceanQuality : TheForestQualitySettings.OceanQualities.Flat);
				TheForestQualitySettings.OceanQualities oceanQuality = TheForestQualitySettings.UserSettings.OceanQuality;
				if (oceanQuality != TheForestQualitySettings.OceanQualities.WaveDisplacementHigh)
				{
					if (oceanQuality != TheForestQualitySettings.OceanQualities.WaveDisplacementLow)
					{
						if (oceanQuality == TheForestQualitySettings.OceanQualities.Flat)
						{
							Scene.OceanFlat.SetActive(true);
							Scene.OceanCeto.SetActive(false);
							this.waterBlurCeto.enabled = false;
						}
					}
					else
					{
						Scene.OceanFlat.SetActive(false);
						Scene.OceanCeto.SetActive(true);
						this.waterBlurCeto.enabled = true;
						if (OceanQualitySettings.Instance != null)
						{
							OceanQualitySettings.Instance.QualityChanged(CETO_QUALITY_SETTING.LOW);
						}
					}
				}
				else
				{
					Scene.OceanFlat.SetActive(false);
					Scene.OceanCeto.SetActive(true);
					this.waterBlurCeto.enabled = true;
					if (OceanQualitySettings.Instance != null)
					{
						OceanQualitySettings.Instance.QualityChanged(CETO_QUALITY_SETTING.HIGH);
					}
				}
			}
		}
	}

	
	private void LateUpdate()
	{
		if (this.waterViz && this.waterBlur)
		{
			this.waterBlur.enabled = this.waterViz.InWater;
		}
	}

	
	
	
	public bool SkipMotionBlur { get; set; }

	
	private Frost frostEffect;

	
	private BleedBehavior bleedEffect;

	
	private bool CausticsBool;

	
	private AmplifyOcclusionEffect amplifyOcclusion;

	
	private LayerMask SunshineOccluders;

	
	private SunshineCamera SunshineCam;

	
	private SunshinePostprocess SunshinePP;

	
	private TheForestAtmosphereCamera SunshineAtmosCam;

	
	private TheForestQualitySettings.OceanQualities CurrentOceanQuality = (TheForestQualitySettings.OceanQualities)(-1);

	
	private WaterViz waterViz;

	
	private WaterBlurEffect waterBlur;

	
	private UnderWaterPostEffect waterBlurCeto;

	
	private FarShadowCascade farShadowCascade;

	
	public Texture2D[] AmplifyColorGradients;

	
	private PostProcessingProfile postProcessingProfile;

	
	private float gammaVelocity;

	
	private float contrastVelocity;
}
