using System;
using AmplifyMotion;
using Ceto;
using CetoTF;
using ScionEngine;
using Smaa;
using TheForest.Utils;
using UnityEngine;
using UnityStandardAssets.ImageEffects;


[ExecuteInEditMode]
public class ImageEffectOptimizer : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.MyCamera = base.gameObject.GetComponent<Camera>();
		this.MyCamera.eventMask = 0;
		this.frostEffect = base.GetComponent<Frost>();
		this.ScionStuff = base.GetComponent<ScionPostProcess>();
		this.bleedEffect = base.GetComponent<BleedBehavior>();
		this.aa_FXAA = base.GetComponent<Antialiasing>();
		this.aa_SMAA = base.GetComponent<SMAA>();
		this.amplifyMotion = base.GetComponent<AmplifyMotionEffect>();
		this.sessao = base.GetComponent<SESSAO>();
		this.hbao = base.GetComponent<HBAO>();
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
		if (this.farShadowCascade)
		{
			bool flag = TheForestQualitySettings.UserSettings.FarShadowMode == TheForestQualitySettings.FarShadowModes.On && !LocalPlayer.IsInClosedArea;
			if (this.farShadowCascade.enableFarShadows != flag)
			{
				this.farShadowCascade.enableFarShadows = flag;
			}
		}
		if (PlayerPreferences.ColorGrading >= 0 && PlayerPreferences.ColorGrading < this.AmplifyColorGradients.Length)
		{
			this.ScionStuff.colorGradingTex1 = this.AmplifyColorGradients[PlayerPreferences.ColorGrading];
		}
		if (LocalPlayer.Inventory && LocalPlayer.Inventory.enabled)
		{
			float num = (!LocalPlayer.Stats.IsInNorthColdArea()) ? 0.5f : 1.75f;
			if (!Mathf.Approximately(this.ScionStuff.exposureCompensation, num))
			{
				this.ScionStuff.exposureCompensation = Mathf.Lerp(this.ScionStuff.exposureCompensation, num, Time.deltaTime);
			}
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
		if (this.aa_FXAA)
		{
			this.aa_FXAA.enabled = (antiAliasingTechnique == TheForestQualitySettings.AntiAliasingTechnique.FXAA);
		}
		if (this.aa_SMAA)
		{
			this.aa_SMAA.enabled = (antiAliasingTechnique == TheForestQualitySettings.AntiAliasingTechnique.SMAA);
		}
		if (this.amplifyMotion)
		{
			this.amplifyMotion.enabled = (!this.SkipMotionBlur && TheForestQualitySettings.UserSettings.MotionBlur != TheForestQualitySettings.MotionBlurQuality.None);
			switch (TheForestQualitySettings.UserSettings.MotionBlur)
			{
			case TheForestQualitySettings.MotionBlurQuality.Low:
				this.amplifyMotion.QualityLevel = Quality.Mobile;
				this.amplifyMotion.QualitySteps = 2;
				break;
			case TheForestQualitySettings.MotionBlurQuality.Medium:
				this.amplifyMotion.QualityLevel = Quality.Standard;
				this.amplifyMotion.QualitySteps = 2;
				break;
			case TheForestQualitySettings.MotionBlurQuality.High:
				this.amplifyMotion.QualityLevel = Quality.Standard;
				this.amplifyMotion.QualitySteps = 3;
				break;
			case TheForestQualitySettings.MotionBlurQuality.Ultra:
				this.amplifyMotion.QualityLevel = Quality.SoftEdge_SM3;
				this.amplifyMotion.QualitySteps = 3;
				break;
			}
		}
		if (this.MyCamera)
		{
			this.MyCamera.renderingPath = RenderingPath.DeferredLighting;
			switch (TheForestQualitySettings.UserSettings.MyRenderType)
			{
			case TheForestQualitySettings.RendererType.Deferred:
				this.MyCamera.renderingPath = RenderingPath.DeferredShading;
				break;
			case TheForestQualitySettings.RendererType.LegacyDeferred:
				this.MyCamera.renderingPath = RenderingPath.DeferredLighting;
				break;
			case TheForestQualitySettings.RendererType.Forward:
				this.MyCamera.renderingPath = RenderingPath.Forward;
				break;
			}
		}
		if (PlayerPreferences.is32bit)
		{
			this.ScionStuff.bloomDownsamples = 4;
		}
		TheForestQualitySettings.SEBloomTechnique sebloom = TheForestQualitySettings.UserSettings.SEBloom;
		if (sebloom != TheForestQualitySettings.SEBloomTechnique.Normal)
		{
			if (sebloom == TheForestQualitySettings.SEBloomTechnique.None)
			{
				this.ScionStuff.bloom = false;
			}
		}
		else
		{
			this.ScionStuff.bloom = true;
		}
		TheForestQualitySettings.ChromaticAberration ca = TheForestQualitySettings.UserSettings.CA;
		if (ca != TheForestQualitySettings.ChromaticAberration.Normal)
		{
			if (ca == TheForestQualitySettings.ChromaticAberration.None)
			{
				this.ScionStuff.chromaticAberration = false;
			}
		}
		else
		{
			this.ScionStuff.chromaticAberration = true;
		}
		TheForestQualitySettings.FilmGrain fg = TheForestQualitySettings.UserSettings.Fg;
		if (fg != TheForestQualitySettings.FilmGrain.Normal)
		{
			if (fg == TheForestQualitySettings.FilmGrain.None)
			{
				this.ScionStuff.grainIntensity = 0f;
			}
		}
		else
		{
			this.ScionStuff.grainIntensity = 0.07f;
		}
		TheForestQualitySettings.Dof dofTech = TheForestQualitySettings.UserSettings.DofTech;
		if (dofTech != TheForestQualitySettings.Dof.Normal)
		{
			if (dofTech == TheForestQualitySettings.Dof.None)
			{
				if (this.ScionStuff.depthOfField)
				{
					this.ScionStuff.depthOfField = false;
				}
			}
		}
		else if (!this.ScionStuff.depthOfField)
		{
			this.ScionStuff.depthOfField = true;
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
					bool flag2 = TheForestQualitySettings.UserSettings.SunshineOcclusion == TheForestQualitySettings.SunshineOcclusionOn.On || LocalPlayer.IsInCaves;
					Sunshine.Instance.Occluders = ((!flag2) ? 0 : this.SunshineOccluders.value);
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
		if (this.sessao && (TheForestQualitySettings.UserSettings.SSAOType == TheForestQualitySettings.SSAOTypes.SESSAO || !this.hbao))
		{
			this.sessao.enabled = (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			if (this.hbao)
			{
				this.hbao.enabled = false;
			}
			switch (ssaotechnique)
			{
			case TheForestQualitySettings.SSAOTechnique.Ultra:
				this.sessao.halfSampling = false;
				break;
			case TheForestQualitySettings.SSAOTechnique.High:
			case TheForestQualitySettings.SSAOTechnique.Low:
				this.sessao.halfSampling = true;
				break;
			}
		}
		else if (this.hbao && (TheForestQualitySettings.UserSettings.SSAOType == TheForestQualitySettings.SSAOTypes.HBAO || !this.sessao))
		{
			this.hbao.enabled = (ssaotechnique != TheForestQualitySettings.SSAOTechnique.Off);
			if (this.sessao)
			{
				this.sessao.enabled = false;
			}
			switch (ssaotechnique)
			{
			case TheForestQualitySettings.SSAOTechnique.Ultra:
				if (this.hbao.generalSettings.quality != HBAO.Quality.Highest)
				{
					this.hbao.ApplyPreset(HBAO.Preset.HighestQuality);
				}
				break;
			case TheForestQualitySettings.SSAOTechnique.High:
				if (this.hbao.generalSettings.quality != HBAO.Quality.Medium)
				{
					this.hbao.ApplyPreset(HBAO.Preset.FastestPerformance);
				}
				break;
			case TheForestQualitySettings.SSAOTechnique.Low:
				if (this.hbao.generalSettings.quality != HBAO.Quality.Lowest)
				{
					this.hbao.ApplyPreset(HBAO.Preset.FastestPerformance);
				}
				break;
			}
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
				switch (TheForestQualitySettings.UserSettings.OceanQuality)
				{
				case TheForestQualitySettings.OceanQualities.WaveDisplacementHigh:
					Scene.OceanFlat.SetActive(false);
					Scene.OceanCeto.SetActive(true);
					this.waterBlurCeto.enabled = true;
					if (OceanQualitySettings.Instance != null)
					{
						OceanQualitySettings.Instance.QualityChanged(CETO_QUALITY_SETTING.HIGH);
					}
					break;
				case TheForestQualitySettings.OceanQualities.WaveDisplacementLow:
					Scene.OceanFlat.SetActive(false);
					Scene.OceanCeto.SetActive(true);
					this.waterBlurCeto.enabled = true;
					if (OceanQualitySettings.Instance != null)
					{
						OceanQualitySettings.Instance.QualityChanged(CETO_QUALITY_SETTING.LOW);
					}
					break;
				case TheForestQualitySettings.OceanQualities.Flat:
					Scene.OceanFlat.SetActive(true);
					Scene.OceanCeto.SetActive(false);
					this.waterBlurCeto.enabled = false;
					break;
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

	
	private Camera MyCamera;

	
	private Frost frostEffect;

	
	private BleedBehavior bleedEffect;

	
	private ScionPostProcess ScionStuff;

	
	private Antialiasing aa_FXAA;

	
	private SMAA aa_SMAA;

	
	private AmplifyMotionEffect amplifyMotion;

	
	private bool CausticsBool;

	
	private SESSAO sessao;

	
	private HBAO hbao;

	
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

	
	private float gammaVelocity;
}
