using System;
using Inspector;
using Inspector.Decorations;
using UnityEngine;

namespace ScionEngine
{
	
	public abstract class ScionPostProcessBase : MonoBehaviour
	{
		
		protected bool ShowGrain()
		{
			return this.m_grain;
		}

		
		protected bool ShowVignette()
		{
			return this.m_vignette;
		}

		
		protected bool ShowChromaticAberration()
		{
			return this.m_chromaticAberration;
		}

		
		protected bool ShowBloom()
		{
			return this.bloom;
		}

		
		protected bool ShowLensFlare()
		{
			return this.m_lensFlare;
		}

		
		private bool ShowDiffractionUVScale()
		{
			return this.ShowLensFlare() && this.m_lensFlareDiffractionTexture != null;
		}

		
		protected bool ShowBlurStrength()
		{
			return this.ShowLensFlare() && this.m_lensFlareBlurSamples != LensFlareBlurSamples.Off;
		}

		
		private bool ShowLensDirtActive()
		{
			return this.ShowBloom() || this.ShowLensFlare();
		}

		
		protected bool ShowLensDirt()
		{
			return this.ShowLensDirtActive() && this.lensDirt;
		}

		
		protected bool ShowLensDirtSettings()
		{
			return this.lensDirtTexture != null && this.ShowLensDirt();
		}

		
		protected virtual bool ShowTonemapping()
		{
			return false;
		}

		
		protected bool ShowCameraMode()
		{
			return this.unityCamera.allowHDR;
		}

		
		protected bool ShowExposureComp()
		{
			return this.ShowCameraMode() && this.cameraMode != CameraMode.Off;
		}

		
		protected bool ShowExposureAdaption()
		{
			return this.ShowCameraMode() && this.cameraMode != CameraMode.Off && this.cameraMode != CameraMode.Manual;
		}

		
		protected bool ShowFocalLength()
		{
			return this.m_userControlledFocalLength;
		}

		
		protected bool ShowFNumber()
		{
			return !this.ShowCameraMode() || this.cameraMode == CameraMode.AperturePriority || this.cameraMode == CameraMode.Manual || (this.cameraMode == CameraMode.Off && this.depthOfField);
		}

		
		protected bool ShowISO()
		{
			return this.cameraMode == CameraMode.Manual;
		}

		
		protected bool ShowShutterSpeed()
		{
			return this.cameraMode == CameraMode.Manual;
		}

		
		protected bool ShowDepthOfField()
		{
			return this.depthOfField;
		}

		
		protected bool ShowPointAverage()
		{
			return this.m_depthFocusMode == DepthFocusMode.PointAverage && this.ShowDepthOfField();
		}

		
		protected bool ShowFocalDistance()
		{
			return (this.m_depthFocusMode == DepthFocusMode.ManualDistance || this.m_depthFocusMode == DepthFocusMode.ManualRange) && this.ShowDepthOfField();
		}

		
		protected bool ShowTargetTransform()
		{
			return this.m_depthFocusMode == DepthFocusMode.TargetTransform && this.ShowDepthOfField();
		}

		
		protected bool ShowFocalRange()
		{
			return this.m_depthFocusMode == DepthFocusMode.ManualRange && this.ShowDepthOfField();
		}

		
		protected bool ShowTemporalSettings()
		{
			return this.m_depthOfFieldTemporalSupersampling && this.ShowDepthOfField();
		}

		
		protected bool ShowCCTex1()
		{
			return this.colorGradingMode == ColorGradingMode.On || this.colorGradingMode == ColorGradingMode.Blend;
		}

		
		protected bool ShowCCTex2()
		{
			return this.colorGradingMode == ColorGradingMode.Blend;
		}

		
		
		protected Camera unityCamera
		{
			get
			{
				if (this.m_camera == null)
				{
					this.m_camera = base.GetComponent<Camera>();
				}
				return this.m_camera;
			}
		}

		
		
		
		public CameraMode cameraMode
		{
			get
			{
				return this.m_cameraMode;
			}
			set
			{
				this.m_cameraMode = value;
				this.postProcessParams.cameraParams.cameraMode = value;
				this.postProcessParams.exposure = (value != CameraMode.Off);
			}
		}

		
		
		
		public bool bloom
		{
			get
			{
				return this.m_bloom;
			}
			set
			{
				this.m_bloom = value;
				this.postProcessParams.bloom = value;
			}
		}

		
		
		
		public bool lensFlare
		{
			get
			{
				return this.m_lensFlare;
			}
			set
			{
				this.m_lensFlare = value;
				this.postProcessParams.lensFlare = value;
			}
		}

		
		
		
		public bool lensDirt
		{
			get
			{
				return this.m_lensDirt;
			}
			set
			{
				this.m_lensDirt = value;
				this.postProcessParams.lensDirt = value;
			}
		}

		
		
		
		public Texture lensDirtTexture
		{
			get
			{
				return this.m_lensDirtTexture;
			}
			set
			{
				this.m_lensDirtTexture = value;
				this.postProcessParams.lensDirtTexture = value;
			}
		}

		
		
		
		public bool depthOfField
		{
			get
			{
				return this.m_depthOfField;
			}
			set
			{
				this.m_depthOfField = value;
				this.postProcessParams.depthOfField = value;
				this.PlatformCompatibility();
			}
		}

		
		
		
		public float bloomThreshold
		{
			get
			{
				return this.m_bloomThreshold;
			}
			set
			{
				value = Mathf.Max(value, 0f);
				this.m_bloomThreshold = value;
				this.postProcessParams.glareParams.threshold = value;
			}
		}

		
		
		
		public float bloomIntensity
		{
			get
			{
				return this.m_bloomIntensity;
			}
			set
			{
				this.m_bloomIntensity = value;
				this.postProcessParams.glareParams.intensity = ScionUtility.Square(value);
			}
		}

		
		
		
		public float bloomBrightness
		{
			get
			{
				return this.m_bloomBrightness;
			}
			set
			{
				this.m_bloomBrightness = value;
				this.postProcessParams.glareParams.brightness = value;
			}
		}

		
		
		
		public float bloomDistanceMultiplier
		{
			get
			{
				return this.m_bloomDistanceMultiplier;
			}
			set
			{
				this.m_bloomDistanceMultiplier = value;
				this.postProcessParams.glareParams.distanceMultiplier = value;
			}
		}

		
		
		
		public int bloomDownsamples
		{
			get
			{
				return this.m_bloomDownsamples;
			}
			set
			{
				this.m_bloomDownsamples = value;
				this.postProcessParams.glareParams.downsamples = value;
			}
		}

		
		
		
		public LensFlareGhostSamples lensFlareGhostSamples
		{
			get
			{
				return this.m_lensFlareGhostSamples;
			}
			set
			{
				this.m_lensFlareGhostSamples = value;
				this.postProcessParams.lensFlareParams.ghostSamples = value;
			}
		}

		
		
		
		public float lensFlareGhostIntensity
		{
			get
			{
				return this.m_lensFlareGhostIntensity;
			}
			set
			{
				this.m_lensFlareGhostIntensity = value;
				this.postProcessParams.lensFlareParams.ghostIntensity = value;
			}
		}

		
		
		
		public float lensFlareGhostDispersal
		{
			get
			{
				return this.m_lensFlareGhostDispersal;
			}
			set
			{
				this.m_lensFlareGhostDispersal = value;
				this.postProcessParams.lensFlareParams.ghostDispersal = value;
			}
		}

		
		
		
		public float lensFlareGhostDistortion
		{
			get
			{
				return this.m_lensFlareGhostDistortion;
			}
			set
			{
				this.m_lensFlareGhostDistortion = value;
				this.postProcessParams.lensFlareParams.ghostDistortion = value;
			}
		}

		
		
		
		public float lensFlareGhostEdgeFade
		{
			get
			{
				return this.m_lensFlareGhostEdgeFade;
			}
			set
			{
				this.m_lensFlareGhostEdgeFade = value;
				this.postProcessParams.lensFlareParams.ghostEdgeFade = value;
			}
		}

		
		
		
		public float lensFlareHaloIntensity
		{
			get
			{
				return this.m_lensFlareHaloIntensity;
			}
			set
			{
				this.m_lensFlareHaloIntensity = value;
				this.postProcessParams.lensFlareParams.haloIntensity = value;
			}
		}

		
		
		
		public float lensFlareHaloWidth
		{
			get
			{
				return this.m_lensFlareHaloWidth;
			}
			set
			{
				this.m_lensFlareHaloWidth = value;
				this.postProcessParams.lensFlareParams.haloWidth = value;
			}
		}

		
		
		
		public float lensFlareHaloDistortion
		{
			get
			{
				return this.m_lensFlareHaloDistortion;
			}
			set
			{
				this.m_lensFlareHaloDistortion = value;
				this.postProcessParams.lensFlareParams.haloDistortion = value;
			}
		}

		
		
		
		public float lensFlareDiffractionUVScale
		{
			get
			{
				return this.m_lensFlareDiffractionUVScale;
			}
			set
			{
				this.m_lensFlareDiffractionUVScale = value;
				this.postProcessParams.lensFlareParams.starUVScale = value;
			}
		}

		
		
		
		public Texture2D lensFlareDiffractionTexture
		{
			get
			{
				return this.m_lensFlareDiffractionTexture;
			}
			set
			{
				this.m_lensFlareDiffractionTexture = value;
				this.postProcessParams.lensFlareParams.starTexture = value;
			}
		}

		
		
		
		public LensFlareBlurSamples lensFlareBlurSamples
		{
			get
			{
				return this.m_lensFlareBlurSamples;
			}
			set
			{
				this.m_lensFlareBlurSamples = value;
				this.postProcessParams.lensFlareParams.blurSamples = value;
			}
		}

		
		
		
		public float lensFlareBlurStrength
		{
			get
			{
				return this.m_lensFlareBlurStrength;
			}
			set
			{
				this.m_lensFlareBlurStrength = value;
				this.postProcessParams.lensFlareParams.blurStrength = value;
			}
		}

		
		
		
		public int lensFlareDownsamples
		{
			get
			{
				return this.m_lensFlareDownsamples;
			}
			set
			{
				this.m_lensFlareDownsamples = value;
				this.postProcessParams.lensFlareParams.downsamples = value;
			}
		}

		
		
		
		public float lensFlareThreshold
		{
			get
			{
				return this.m_lensFlareThreshold;
			}
			set
			{
				value = Mathf.Max(value, 0f);
				this.m_lensFlareThreshold = value;
				this.postProcessParams.lensFlareParams.threshold = value;
			}
		}

		
		
		
		public Texture2D lensFlareLensColorTexture
		{
			get
			{
				return this.m_lensFlareLensColorTexture;
			}
			set
			{
				this.m_lensFlareLensColorTexture = value;
				this.postProcessParams.lensFlareParams.lensColorTexture = value;
			}
		}

		
		
		
		public float lensDirtBloomThreshold
		{
			get
			{
				return this.m_lensDirtBloomThreshold;
			}
			set
			{
				value = Mathf.Max(value, 0f);
				this.m_lensDirtBloomThreshold = value;
				this.postProcessParams.lensDirtParams.bloomThreshold = value;
			}
		}

		
		
		
		public float lensDirtBloomEffect
		{
			get
			{
				return this.m_lensDirtBloomEffect;
			}
			set
			{
				this.m_lensDirtBloomEffect = value;
				this.postProcessParams.lensDirtParams.bloomEffect = ScionUtility.Square(value);
			}
		}

		
		
		
		public float lensDirtBloomBrightness
		{
			get
			{
				return this.m_lensDirtBloomBrightness;
			}
			set
			{
				this.m_lensDirtBloomBrightness = value;
				this.postProcessParams.lensDirtParams.bloomBrightness = value;
			}
		}

		
		
		
		public float lensDirtLensFlareEffect
		{
			get
			{
				return this.m_lensDirtLensFlareEffect;
			}
			set
			{
				this.m_lensDirtLensFlareEffect = value;
				this.postProcessParams.lensDirtParams.lensFlareEffect = value;
			}
		}

		
		
		
		public float lensDirtLensFlareBrightness
		{
			get
			{
				return this.m_lensDirtLensFlareBrightness;
			}
			set
			{
				this.m_lensDirtLensFlareBrightness = value;
				this.postProcessParams.lensDirtParams.lensFlareBrightness = value;
			}
		}

		
		
		
		public TonemappingMode tonemappingMode
		{
			get
			{
				return this.m_tonemappingMode;
			}
			set
			{
				this.m_tonemappingMode = value;
			}
		}

		
		
		
		public float whitePoint
		{
			get
			{
				return this.m_whitePoint;
			}
			set
			{
				this.m_whitePoint = value;
				this.postProcessParams.commonPostProcess.whitePoint = value;
			}
		}

		
		
		
		public LayerMask exclusionMask
		{
			get
			{
				return this.m_exclusionMask;
			}
			set
			{
				this.m_exclusionMask = value;
				this.postProcessParams.DoFParams.depthOfFieldMask = value;
			}
		}

		
		
		
		public DepthFocusMode depthFocusMode
		{
			get
			{
				return this.m_depthFocusMode;
			}
			set
			{
				this.m_depthFocusMode = value;
				this.postProcessParams.DoFParams.depthFocusMode = value;
				this.focalDistance = this.focalDistance;
			}
		}

		
		
		
		public bool visualizeFocalDistance
		{
			get
			{
				return this.m_visualizeFocalDistance;
			}
			set
			{
				this.m_visualizeFocalDistance = value;
				this.postProcessParams.DoFParams.visualizeFocalDistance = value;
			}
		}

		
		
		
		public Transform depthOfFieldTargetTransform
		{
			get
			{
				return this.m_depthOfFieldTargetTransform;
			}
			set
			{
				this.m_depthOfFieldTargetTransform = value;
			}
		}

		
		
		
		public float maxCoCRadius
		{
			get
			{
				return this.m_maxCoCRadius;
			}
			set
			{
				this.m_maxCoCRadius = value;
				this.postProcessParams.DoFParams.maxCoCRadius = value;
			}
		}

		
		
		
		public bool depthOfFieldTemporalSupersampling
		{
			get
			{
				return this.m_depthOfFieldTemporalSupersampling;
			}
			set
			{
				this.m_depthOfFieldTemporalSupersampling = value;
				this.postProcessParams.DoFParams.useTemporal = value;
			}
		}

		
		
		
		public float depthOfFieldTemporalBlend
		{
			get
			{
				return this.m_depthOfFieldTemporalBlend;
			}
			set
			{
				this.m_depthOfFieldTemporalBlend = value;
				this.postProcessParams.DoFParams.temporalBlend = value;
			}
		}

		
		
		
		public int depthOfFieldTemporalSteps
		{
			get
			{
				return this.m_depthOfFieldTemporalSteps;
			}
			set
			{
				this.m_depthOfFieldTemporalSteps = value;
				this.postProcessParams.DoFParams.temporalSteps = value;
			}
		}

		
		
		
		public DepthOfFieldSamples depthOfFieldSamples
		{
			get
			{
				return this.m_depthOfFieldSamples;
			}
			set
			{
				this.m_depthOfFieldSamples = value;
				this.postProcessParams.DoFParams.quality = value;
			}
		}

		
		
		
		public Vector2 pointAveragePosition
		{
			get
			{
				return this.m_pointAveragePosition;
			}
			set
			{
				this.m_pointAveragePosition = value;
				this.postProcessParams.DoFParams.pointAveragePosition = value;
			}
		}

		
		
		
		public float pointAverageRange
		{
			get
			{
				return this.m_pointAverageRange;
			}
			set
			{
				this.m_pointAverageRange = value;
				this.postProcessParams.DoFParams.pointAverageRange = value;
			}
		}

		
		
		
		public bool visualizePointFocus
		{
			get
			{
				return this.m_visualizePointFocus;
			}
			set
			{
				this.m_visualizePointFocus = value;
				this.postProcessParams.DoFParams.visualizePointFocus = value;
			}
		}

		
		
		
		public float depthAdaptionSpeed
		{
			get
			{
				return this.m_depthAdaptionSpeed;
			}
			set
			{
				this.m_depthAdaptionSpeed = value;
				this.postProcessParams.DoFParams.depthAdaptionSpeed = value;
			}
		}

		
		
		
		public float focalDistance
		{
			get
			{
				return this.m_focalDistance;
			}
			set
			{
				this.m_focalDistance = value;
				this.postProcessParams.DoFParams.focalDistance = value;
			}
		}

		
		
		
		public float focalRange
		{
			get
			{
				return this.m_focalRange;
			}
			set
			{
				this.m_focalRange = value;
				this.postProcessParams.DoFParams.focalRange = value;
			}
		}

		
		
		
		public ColorGradingMode colorGradingMode
		{
			get
			{
				return this.m_colorGradingMode;
			}
			set
			{
				this.m_colorGradingMode = value;
				this.postProcessParams.colorGradingParams.colorGradingMode = ((!(this.colorGradingTex1 == null)) ? value : ColorGradingMode.Off);
			}
		}

		
		
		
		public Texture2D colorGradingTex1
		{
			get
			{
				return this.m_colorGradingTex1;
			}
			set
			{
				this.m_colorGradingTex1 = value;
				this.postProcessParams.colorGradingParams.colorGradingTex1 = value;
				this.colorGradingMode = this.colorGradingMode;
			}
		}

		
		
		
		public Texture2D colorGradingTex2
		{
			get
			{
				return this.m_colorGradingTex2;
			}
			set
			{
				this.m_colorGradingTex2 = value;
				this.postProcessParams.colorGradingParams.colorGradingTex2 = value;
			}
		}

		
		
		
		public float colorGradingBlendFactor
		{
			get
			{
				return this.m_colorGradingBlendFactor;
			}
			set
			{
				float colorGradingBlendFactor = Mathf.Clamp01(value);
				this.m_colorGradingBlendFactor = colorGradingBlendFactor;
				this.postProcessParams.colorGradingParams.colorGradingBlendFactor = colorGradingBlendFactor;
			}
		}

		
		
		
		public bool userControlledFocalLength
		{
			get
			{
				return this.m_userControlledFocalLength;
			}
			set
			{
				this.m_userControlledFocalLength = value;
				if (value)
				{
					this.focalLength = ScionUtility.GetFocalLength(Mathf.Tan(this.m_camera.fieldOfView * 0.5f * 0.0174532924f)) * 1000f;
				}
			}
		}

		
		
		
		public float focalLength
		{
			get
			{
				return this.m_focalLength;
			}
			set
			{
				this.m_focalLength = value;
				this.postProcessParams.cameraParams.focalLength = value;
				this.m_camera.fieldOfView = ScionUtility.GetFieldOfView(this.m_focalLength * 0.001f);
			}
		}

		
		
		
		public float fNumber
		{
			get
			{
				return this.m_fNumber;
			}
			set
			{
				this.m_fNumber = value;
				this.postProcessParams.cameraParams.fNumber = value;
			}
		}

		
		
		
		public float ISO
		{
			get
			{
				return this.m_ISO;
			}
			set
			{
				this.m_ISO = value;
				this.postProcessParams.cameraParams.ISO = value;
			}
		}

		
		
		
		public float shutterSpeed
		{
			get
			{
				return this.m_shutterSpeed;
			}
			set
			{
				this.m_shutterSpeed = value;
				this.postProcessParams.cameraParams.shutterSpeed = value;
			}
		}

		
		
		
		public float adaptionSpeed
		{
			get
			{
				return this.m_adaptionSpeed;
			}
			set
			{
				this.m_adaptionSpeed = value;
				this.postProcessParams.cameraParams.adaptionSpeed = value;
			}
		}

		
		
		
		public Vector2 minMaxExposure
		{
			get
			{
				return this.m_minMaxExposure;
			}
			set
			{
				this.m_minMaxExposure = value;
				this.postProcessParams.cameraParams.minMaxExposure = value;
			}
		}

		
		
		
		public float exposureCompensation
		{
			get
			{
				return this.m_exposureCompensation;
			}
			set
			{
				this.m_exposureCompensation = value;
				this.postProcessParams.cameraParams.exposureCompensation = value;
			}
		}

		
		
		
		public bool grain
		{
			get
			{
				return this.m_grain;
			}
			set
			{
				this.m_grain = value;
				this.postProcessParams.commonPostProcess.grainIntensity = ((!this.m_grain) ? 0f : this.grainIntensity);
			}
		}

		
		
		
		public float grainIntensity
		{
			get
			{
				return this.m_grainIntensity;
			}
			set
			{
				this.m_grainIntensity = value;
				this.postProcessParams.commonPostProcess.grainIntensity = ((!this.m_grain) ? 0f : this.grainIntensity);
			}
		}

		
		
		
		public bool vignette
		{
			get
			{
				return this.m_vignette;
			}
			set
			{
				this.m_vignette = value;
				this.postProcessParams.commonPostProcess.vignetteIntensity = ((!this.m_vignette) ? 0f : this.vignetteIntensity);
			}
		}

		
		
		
		public float vignetteIntensity
		{
			get
			{
				return this.m_vignetteIntensity;
			}
			set
			{
				this.m_vignetteIntensity = value;
				this.postProcessParams.commonPostProcess.vignetteIntensity = ((!this.m_vignette) ? 0f : this.vignetteIntensity);
			}
		}

		
		
		
		public float vignetteScale
		{
			get
			{
				return this.m_vignetteScale;
			}
			set
			{
				this.m_vignetteScale = value;
				this.postProcessParams.commonPostProcess.vignetteScale = value;
			}
		}

		
		
		
		public Color vignetteColor
		{
			get
			{
				return this.m_vignetteColor;
			}
			set
			{
				this.m_vignetteColor = value;
				this.postProcessParams.commonPostProcess.vignetteColor = value;
			}
		}

		
		
		
		public bool chromaticAberration
		{
			get
			{
				return this.m_chromaticAberration;
			}
			set
			{
				this.m_chromaticAberration = value;
				this.postProcessParams.commonPostProcess.chromaticAberration = value;
			}
		}

		
		
		
		public float chromaticAberrationDistortion
		{
			get
			{
				return this.m_chromaticAberrationDistortion;
			}
			set
			{
				this.m_chromaticAberrationDistortion = value;
				this.postProcessParams.commonPostProcess.chromaticAberrationDistortion = value;
			}
		}

		
		
		
		public float chromaticAberrationIntensity
		{
			get
			{
				return this.m_chromaticAberrationIntensity;
			}
			set
			{
				this.m_chromaticAberrationIntensity = value;
				this.postProcessParams.commonPostProcess.chromaticAberrationIntensity = value;
			}
		}

		
		protected void OnEnable()
		{
			this.m_camera = base.GetComponent<Camera>();
			this.m_cameraTransform = this.unityCamera.transform;
			this.m_bloomClass = new Bloom();
			this.m_lensFlareClass = new ScionLensFlare();
			this.m_combinationPass = new CombinationPass();
			this.m_downsampling = new Downsampling();
			this.m_virtualCamera = new VirtualCamera();
			this.m_depthOfFieldClass = new DepthOfField();
			this.m_scionDebug = new ScionDebug();
			this.m_isFirstRender = true;
			if (!this.PlatformCompatibility())
			{
				base.enabled = false;
			}
		}

		
		protected void OnDisable()
		{
			if (this.m_bloomClass != null)
			{
				this.m_bloomClass.ReleaseResources();
			}
			if (this.m_combinationPass != null)
			{
				this.m_combinationPass.ReleaseResources();
			}
			if (this.m_depthOfFieldClass != null)
			{
				this.m_depthOfFieldClass.ReleaseResources();
			}
			if (this.m_downsampling != null)
			{
				this.m_downsampling.ReleaseResources();
			}
			if (this.m_lensFlareClass != null)
			{
				this.m_lensFlareClass.ReleaseResources();
			}
			if (this.m_virtualCamera != null)
			{
				this.m_virtualCamera.ReleaseResources();
			}
		}

		
		protected virtual void InitializePostProcessParams()
		{
			this.postProcessParams.Fill(this, this.forceFillParams);
			this.forceFillParams = false;
		}

		
		protected void OnPreRender()
		{
			this.InitializePostProcessParams();
			this.unityCamera.depthTextureMode |= DepthTextureMode.Depth;
		}

		
		protected bool PlatformCompatibility()
		{
			if (!SystemInfo.supportsImageEffects)
			{
				Debug.LogWarning("Image Effects are not supported on this platform");
				return false;
			}
			if (!this.m_bloomClass.PlatformCompatibility())
			{
				Debug.LogWarning("Bloom shader not supported on this platform");
				return false;
			}
			if (!this.m_lensFlareClass.PlatformCompatibility())
			{
				Debug.LogWarning("Lens flare shader not supported on this platform");
				return false;
			}
			if (!this.m_combinationPass.PlatformCompatibility())
			{
				Debug.LogWarning("Combination shader not supported on this platform");
				return false;
			}
			if (!this.m_virtualCamera.PlatformCompatibility())
			{
				Debug.LogWarning("Virtual camera shader not supported on this platform");
				return false;
			}
			return this.m_depthOfFieldClass.PlatformCompatibility() || !this.depthOfField;
		}

		
		protected void SetupPostProcessParameters(PostProcessParameters postProcessParams, RenderTexture source)
		{
			this.focalDistance = ((this.focalDistance >= this.unityCamera.nearClipPlane + 0.01f) ? this.focalDistance : (this.unityCamera.nearClipPlane + 0.01f));
			postProcessParams.camera = this.unityCamera;
			postProcessParams.cameraTransform = this.m_cameraTransform;
			postProcessParams.halfResSource = null;
			postProcessParams.width = source.width;
			postProcessParams.height = source.height;
			postProcessParams.halfWidth = source.width / 2;
			postProcessParams.halfHeight = source.height / 2;
			if (this.prevCamFoV != this.unityCamera.fieldOfView || postProcessParams.preCalcValues.tanHalfFoV == 0f)
			{
				postProcessParams.preCalcValues.tanHalfFoV = Mathf.Tan(this.unityCamera.fieldOfView * 0.5f * 0.0174532924f);
				this.prevCamFoV = this.unityCamera.fieldOfView;
			}
			postProcessParams.DoFParams.useMedianFilter = true;
			if (!this.userControlledFocalLength)
			{
				postProcessParams.cameraParams.focalLength = ScionUtility.GetFocalLength(postProcessParams.preCalcValues.tanHalfFoV);
			}
			else
			{
				postProcessParams.cameraParams.focalLength = this.focalLength * 0.001f;
			}
			postProcessParams.cameraParams.apertureDiameter = ScionUtility.ComputeApertureDiameter(this.fNumber, postProcessParams.cameraParams.focalLength);
			postProcessParams.cameraParams.fieldOfView = this.unityCamera.fieldOfView;
			postProcessParams.cameraParams.aspect = this.unityCamera.aspect;
			postProcessParams.cameraParams.nearPlane = this.unityCamera.nearClipPlane;
			postProcessParams.cameraParams.farPlane = this.unityCamera.farClipPlane;
			if (this.m_isFirstRender)
			{
				postProcessParams.cameraParams.previousViewProjection = this.unityCamera.projectionMatrix * this.unityCamera.worldToCameraMatrix;
			}
			postProcessParams.isFirstRender = this.m_isFirstRender;
			this.m_isFirstRender = false;
			if (this.m_depthFocusMode == DepthFocusMode.TargetTransform)
			{
				if (this.depthOfFieldTargetTransform == null)
				{
					postProcessParams.DoFParams.focalDistance = 10f;
					return;
				}
				Vector3 lhs = this.depthOfFieldTargetTransform.position - this.m_cameraTransform.position;
				float focalDistance = Mathf.Max(Vector3.Dot(lhs, this.m_cameraTransform.forward), this.unityCamera.nearClipPlane + 0.01f);
				postProcessParams.DoFParams.focalDistance = focalDistance;
			}
		}

		
		protected void SetGlobalParameters(PostProcessParameters postProcessParams)
		{
			Vector4 value = default(Vector4);
			value.x = postProcessParams.cameraParams.nearPlane;
			value.y = postProcessParams.cameraParams.farPlane;
			value.z = 1f / value.y;
			value.w = value.x * value.z;
			Shader.SetGlobalVector("_ScionNearFarParams", value);
			Shader.SetGlobalVector("_ScionResolutionParameters1", new Vector4
			{
				x = (float)postProcessParams.halfWidth,
				y = (float)postProcessParams.halfHeight,
				z = (float)postProcessParams.width,
				w = (float)postProcessParams.height
			});
			Shader.SetGlobalVector("_ScionResolutionParameters2", new Vector4
			{
				x = 1f / (float)postProcessParams.halfWidth,
				y = 1f / (float)postProcessParams.halfHeight,
				z = 1f / (float)postProcessParams.width,
				w = 1f / (float)postProcessParams.height
			});
			Shader.SetGlobalVector("_ScionCameraParams1", new Vector4
			{
				x = postProcessParams.cameraParams.apertureDiameter,
				y = postProcessParams.cameraParams.focalLength,
				z = postProcessParams.cameraParams.aspect,
				w = 1f / postProcessParams.cameraParams.aspect
			});
			Shader.SetGlobalVector("_ScionCameraPosition", new Vector4
			{
				x = postProcessParams.cameraTransform.position.x,
				y = postProcessParams.cameraTransform.position.y,
				z = postProcessParams.cameraTransform.position.z
			});
			float value2 = 0f;
			if (SystemInfo.graphicsDeviceVersion.StartsWith("Direct") && this.unityCamera.actualRenderingPath == RenderingPath.Forward && QualitySettings.antiAliasing > 0)
			{
				value2 = 1f;
			}
			Shader.SetGlobalFloat("_ScionForwardMSAAFix", value2);
		}

		
		protected virtual void SetShaderKeyWords(PostProcessParameters postProcessParams)
		{
			if (postProcessParams.cameraParams.cameraMode == CameraMode.Off || postProcessParams.cameraParams.cameraMode == CameraMode.Manual)
			{
				ShaderSettings.ExposureSettings.SetIndex(1);
			}
			else
			{
				ShaderSettings.ExposureSettings.SetIndex(0);
			}
			switch (postProcessParams.DoFParams.depthFocusMode)
			{
			case DepthFocusMode.ManualDistance:
				ShaderSettings.DepthFocusSettings.SetIndex(0);
				break;
			case DepthFocusMode.ManualRange:
				ShaderSettings.DepthFocusSettings.SetIndex(1);
				break;
			case DepthFocusMode.TargetTransform:
				ShaderSettings.DepthFocusSettings.SetIndex(0);
				break;
			case DepthFocusMode.PointAverage:
				ShaderSettings.DepthFocusSettings.SetIndex(2);
				break;
			}
			if (postProcessParams.DoFParams.depthOfFieldMask != 0)
			{
				ShaderSettings.DepthOfFieldMaskSettings.SetIndex(0);
			}
			else
			{
				ShaderSettings.DepthOfFieldMaskSettings.Disable();
			}
			if (postProcessParams.lensFlare)
			{
				ShaderSettings.LensFlareSettings.SetIndex(0);
			}
			else
			{
				ShaderSettings.LensFlareSettings.Disable();
			}
			if (postProcessParams.colorGradingParams.colorGradingMode == ColorGradingMode.Off || postProcessParams.colorGradingParams.colorGradingTex1 == null)
			{
				ShaderSettings.ColorGradingSettings.Disable();
			}
			else
			{
				if (postProcessParams.colorGradingParams.colorGradingMode == ColorGradingMode.On)
				{
					ShaderSettings.ColorGradingSettings.SetIndex(0);
				}
				if (postProcessParams.colorGradingParams.colorGradingMode == ColorGradingMode.Blend)
				{
					ShaderSettings.ColorGradingSettings.SetIndex(1);
				}
			}
			if (postProcessParams.commonPostProcess.chromaticAberration)
			{
				ShaderSettings.ChromaticAberrationSettings.SetIndex(0);
			}
			else
			{
				ShaderSettings.ChromaticAberrationSettings.Disable();
			}
		}

		
		private void Start()
		{
			this.prevRenderWasHDR = this.unityCamera.allowHDR;
		}

		
		private void UpdateParameters()
		{
			this.postProcessParams.DoFParams.depthOfFieldMask = this.exclusionMask;
			if (!this.unityCamera.allowHDR)
			{
				this.postProcessParams.cameraParams.cameraMode = CameraMode.Off;
				this.postProcessParams.exposure = false;
			}
			else
			{
				if (this.prevRenderWasHDR != this.unityCamera.allowHDR)
				{
					this.m_isFirstRender = true;
					this.prevRenderWasHDR = this.unityCamera.allowHDR;
				}
				this.postProcessParams.cameraParams.cameraMode = this.m_cameraMode;
				this.postProcessParams.exposure = (this.m_cameraMode != CameraMode.Off);
			}
		}

		
		protected virtual void OnRenderImage(RenderTexture source, RenderTexture dest)
		{
			ScionPostProcessBase.ActiveDebug = this.m_scionDebug;
			this.UpdateParameters();
			this.SetupPostProcessParameters(this.postProcessParams, source);
			this.SetGlobalParameters(this.postProcessParams);
			this.SetShaderKeyWords(this.postProcessParams);
			this.PerformPostProcessing(source, dest, this.postProcessParams);
			this.StoreViewProjectionMatrix();
			ScionPostProcessBase.ActiveDebug = null;
		}

		
		private void StoreViewProjectionMatrix()
		{
			this.postProcessParams.cameraParams.previousViewProjection = this.unityCamera.projectionMatrix * this.unityCamera.worldToCameraMatrix;
		}

		
		protected void PerformPostProcessing(RenderTexture source, RenderTexture dest, PostProcessParameters postProcessParams)
		{
			if (postProcessParams.depthOfField)
			{
				source = this.DepthOfFieldStep(postProcessParams, source);
			}
			postProcessParams.halfResSource = this.m_downsampling.DownsampleFireflyRemoving(source);
			bool flag = false;
			if (postProcessParams.bloom || postProcessParams.lensFlare)
			{
				int a = (!postProcessParams.bloom) ? 0 : postProcessParams.glareParams.downsamples;
				int b = (!postProcessParams.lensFlare) ? 0 : postProcessParams.lensFlareParams.downsamples;
				int numDownsamples = Mathf.Max(a, b);
				this.m_bloomClass.RunDownsamplingChain(postProcessParams.halfResSource, numDownsamples, postProcessParams.glareParams.distanceMultiplier);
				flag = true;
			}
			if (postProcessParams.lensFlare)
			{
				this.m_virtualCamera.BindVirtualCameraTextures(this.m_lensFlareClass.m_lensFlareMat);
				int downsamples = postProcessParams.lensFlareParams.downsamples;
				RenderTexture downsampledScene;
				if (downsamples > 1)
				{
					downsampledScene = this.m_bloomClass.GetGlareTexture(downsamples - 1);
				}
				else if (downsamples == 1)
				{
					downsampledScene = postProcessParams.halfResSource;
				}
				else
				{
					downsampledScene = source;
				}
				postProcessParams.lensFlareTexture = this.m_lensFlareClass.RenderLensFlare(downsampledScene, postProcessParams.lensFlareParams, source.width);
			}
			if (flag && postProcessParams.exposure)
			{
				int num;
				RenderTexture renderTexture = this.m_bloomClass.TryGetSmallGlareTexture(100, out num);
				if (renderTexture == null)
				{
					renderTexture = postProcessParams.halfResSource;
				}
				this.m_virtualCamera.CalculateVirtualCamera(postProcessParams.cameraParams, renderTexture, (float)postProcessParams.halfWidth, postProcessParams.preCalcValues.tanHalfFoV, postProcessParams.DoFParams.focalDistance, postProcessParams.isFirstRender);
			}
			else if (postProcessParams.exposure)
			{
				this.m_virtualCamera.CalculateVirtualCamera(postProcessParams.cameraParams, postProcessParams.halfResSource, (float)postProcessParams.halfWidth, postProcessParams.preCalcValues.tanHalfFoV, postProcessParams.DoFParams.focalDistance, postProcessParams.isFirstRender);
			}
			if (postProcessParams.bloom)
			{
				this.m_bloomClass.RunUpsamplingChain(postProcessParams.halfResSource);
				postProcessParams.bloomTexture = this.m_bloomClass.GetGlareTexture(0);
				postProcessParams.glareParams.bloomNormalizationTerm = this.m_bloomClass.GetEnergyNormalizer(postProcessParams.glareParams.downsamples);
			}
			this.m_combinationPass.Combine(source, dest, postProcessParams, this.m_virtualCamera);
			this.m_scionDebug.VisualizeDebug(dest);
			RenderTexture.ReleaseTemporary(postProcessParams.halfResSource);
			RenderTexture.ReleaseTemporary(postProcessParams.dofTexture);
			this.m_bloomClass.EndOfFrameCleanup();
			this.m_virtualCamera.EndOfFrameCleanup();
			this.m_depthOfFieldClass.EndOfFrameCleanup();
			if (postProcessParams.lensFlare)
			{
				RenderTexture.ReleaseTemporary(postProcessParams.lensFlareTexture);
			}
			if (postProcessParams.depthOfField)
			{
				RenderTexture.ReleaseTemporary(source);
			}
		}

		
		protected RenderTexture DepthOfFieldStep(PostProcessParameters postProcessParams, RenderTexture source)
		{
			RenderTexture downsampledClrDepth = this.m_downsampling.DownsampleForDepthOfField(source);
			RenderTexture renderTexture = null;
			if (postProcessParams.DoFParams.depthOfFieldMask != 0)
			{
				renderTexture = this.m_depthOfFieldClass.RenderExclusionMask(postProcessParams.width, postProcessParams.height, postProcessParams.camera, postProcessParams.cameraTransform, postProcessParams.DoFParams.depthOfFieldMask);
				RenderTexture renderTexture2 = this.m_downsampling.DownsampleMinFilter(source.width, source.height, renderTexture);
				RenderTexture.ReleaseTemporary(renderTexture);
				renderTexture = renderTexture2;
			}
			source = this.m_depthOfFieldClass.RenderDepthOfField(postProcessParams, source, downsampledClrDepth, this.m_virtualCamera, renderTexture);
			RenderTexture.ReleaseTemporary(postProcessParams.halfResSource);
			if (renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			return source;
		}

		
		[Inspector.Decorations.Header(0, "Grain")]
		[Toggle("Active", useProperty = "grain", tooltip = "Determines if grain is used")]
		[SerializeField]
		protected bool m_grain = true;

		
		[Slider("Intensity", useProperty = "grainIntensity", visibleCheck = "ShowGrain", minValue = 0f, maxValue = 0.4f, tooltip = "How strong the grain effect is")]
		[SerializeField]
		protected float m_grainIntensity = 0.1f;

		
		[Inspector.Decorations.Header(0, "Vignette")]
		[Toggle("Active", useProperty = "vignette", tooltip = "Determines if vignette is used")]
		[SerializeField]
		protected bool m_vignette = true;

		
		[Slider("Intensity", useProperty = "vignetteIntensity", visibleCheck = "ShowVignette", minValue = 0f, maxValue = 1f, tooltip = "How strong the vignette effect is")]
		[SerializeField]
		protected float m_vignetteIntensity = 0.7f;

		
		[Slider("Scale", useProperty = "vignetteScale", visibleCheck = "ShowVignette", minValue = 0f, maxValue = 1f, tooltip = "How much of the screen is affected")]
		[SerializeField]
		protected float m_vignetteScale = 0.7f;

		
		[Field("Color", useProperty = "vignetteColor", visibleCheck = "ShowVignette", tooltip = "What color the vignette effect has")]
		[SerializeField]
		protected Color m_vignetteColor = Color.black;

		
		[Inspector.Decorations.Header(0, "Chromatic Aberration")]
		[Toggle("Active", useProperty = "chromaticAberration", tooltip = "Determines if chromatic aberration is used")]
		[SerializeField]
		protected bool m_chromaticAberration = true;

		
		[Slider("Distortion Scale", useProperty = "chromaticAberrationDistortion", visibleCheck = "ShowChromaticAberration", minValue = 0f, maxValue = 1f, tooltip = "How much of the screen is affected")]
		[SerializeField]
		protected float m_chromaticAberrationDistortion = 0.7f;

		
		[Slider("Intensity", useProperty = "chromaticAberrationIntensity", visibleCheck = "ShowChromaticAberration", minValue = 0f, maxValue = 20f, tooltip = "How strong the distortion effect is")]
		[SerializeField]
		protected float m_chromaticAberrationIntensity = 10f;

		
		[Inspector.Decorations.Header(0, "Bloom")]
		[Toggle("Active", useProperty = "bloom", tooltip = "Determines if bloom is used")]
		[SerializeField]
		protected bool m_bloom = true;

		
		[Field("Threshold", useProperty = "bloomThreshold", visibleCheck = "ShowBloom", tooltip = "This effect will not become visible until a pixel is this bright")]
		[SerializeField]
		protected float m_bloomThreshold;

		
		[Slider("Intensity", useProperty = "bloomIntensity", visibleCheck = "ShowBloom", minValue = 0f, maxValue = 1f, tooltip = "How strong the bloom effect is")]
		[SerializeField]
		protected float m_bloomIntensity = 0.35f;

		
		[Slider("Brightness", useProperty = "bloomBrightness", visibleCheck = "ShowBloom", minValue = 1f, maxValue = 8f, tooltip = "How bright the bloom effect is")]
		[SerializeField]
		protected float m_bloomBrightness = 1.2f;

		
		[Slider("Range", useProperty = "bloomDistanceMultiplier", visibleCheck = "ShowBloom", minValue = 0.25f, maxValue = 1.25f, tooltip = "Modifies the range of the bloom")]
		[SerializeField]
		protected float m_bloomDistanceMultiplier = 1f;

		
		[Slider("Downsamples", useProperty = "bloomDownsamples", visibleCheck = "ShowBloom", minValue = 3f, maxValue = 9f, tooltip = "Number of downsamples")]
		[SerializeField]
		protected int m_bloomDownsamples = 7;

		
		[Inspector.Decorations.Header(0, "Lens Flare")]
		[Toggle("Active", useProperty = "lensFlare", tooltip = "Determines if lens flares are used")]
		[SerializeField]
		protected bool m_lensFlare = true;

		
		[Field("Ghost Samples", useProperty = "lensFlareGhostSamples", visibleCheck = "ShowLensFlare", tooltip = "The number of samples used for ghosting")]
		[SerializeField]
		protected LensFlareGhostSamples m_lensFlareGhostSamples = LensFlareGhostSamples.x3;

		
		[Slider("Ghost Intensity", useProperty = "lensFlareGhostIntensity", visibleCheck = "ShowLensFlare", minValue = 0f, maxValue = 2f, tooltip = "The intensity of the lens ghosts")]
		[SerializeField]
		protected float m_lensFlareGhostIntensity = 0.1f;

		
		[Slider("Ghost Dispersal", useProperty = "lensFlareGhostDispersal", visibleCheck = "ShowLensFlare", minValue = 0.01f, maxValue = 1f, tooltip = "How spread out the ghost samples are")]
		[SerializeField]
		protected float m_lensFlareGhostDispersal = 0.2f;

		
		[Slider("Ghost Distortion", useProperty = "lensFlareGhostDistortion", visibleCheck = "ShowLensFlare", minValue = 0.01f, maxValue = 1f, tooltip = "How much chromatic abberation is applied to the ghosts")]
		[SerializeField]
		protected float m_lensFlareGhostDistortion = 0.1f;

		
		[Slider("Ghost Edge Fade", useProperty = "lensFlareGhostEdgeFade", visibleCheck = "ShowLensFlare", minValue = 0f, maxValue = 1.5f, tooltip = "High values mean bright pixels on the border of the screen will not cause ghosting")]
		[SerializeField]
		protected float m_lensFlareGhostEdgeFade = 1f;

		
		[Field("Lens Color", useProperty = "lensFlareLensColorTexture", visibleCheck = "ShowLensFlare", tooltip = "A radial color texture for the lens flare effects")]
		[SerializeField]
		protected Texture2D m_lensFlareLensColorTexture;

		
		[Slider("Halo Intensity", useProperty = "lensFlareHaloIntensity", visibleCheck = "ShowLensFlare", minValue = 0f, maxValue = 2f, tooltip = "The intensity of the lens flare halo")]
		[SerializeField]
		protected float m_lensFlareHaloIntensity = 0.1f;

		
		[Slider("Halo Width", useProperty = "lensFlareHaloWidth", visibleCheck = "ShowLensFlare", minValue = 0f, maxValue = 0.8f, tooltip = "How far from the center of the screen the halo will appear")]
		[SerializeField]
		protected float m_lensFlareHaloWidth = 0.3f;

		
		[Slider("Halo Distortion", useProperty = "lensFlareHaloDistortion", visibleCheck = "ShowLensFlare", minValue = 0f, maxValue = 1f, tooltip = "How much chromatic abberation is applied to the halo")]
		[SerializeField]
		protected float m_lensFlareHaloDistortion = 0.5f;

		
		[Field("Diffraction Texture", useProperty = "lensFlareDiffractionTexture", visibleCheck = "ShowLensFlare", tooltip = "A rotating texture that the lens flare is multiplied by")]
		[SerializeField]
		protected Texture2D m_lensFlareDiffractionTexture;

		
		[Slider("Diffraction UV Scale", useProperty = "lensFlareDiffractionUVScale", visibleCheck = "ShowDiffractionUVScale", minValue = 0.5f, maxValue = 0.9f, tooltip = "Scales the diffraction texture so that it can rotate without the corners ending up outside the texture")]
		[SerializeField]
		protected float m_lensFlareDiffractionUVScale = 0.8f;

		
		[Field("Blur Samples", useProperty = "lensFlareBlurSamples", visibleCheck = "ShowLensFlare", tooltip = "How many samples are used to blur the resulting lens flare texture")]
		[SerializeField]
		protected LensFlareBlurSamples m_lensFlareBlurSamples = LensFlareBlurSamples.x4;

		
		[Slider("Blur Strength", useProperty = "lensFlareBlurStrength", visibleCheck = "ShowBlurStrength", minValue = 0f, maxValue = 1f, tooltip = "How spread out the blur samples are")]
		[SerializeField]
		protected float m_lensFlareBlurStrength = 0.5f;

		
		[Slider("Downsamples", useProperty = "lensFlareDownsamples", visibleCheck = "ShowLensFlare", minValue = 1f, maxValue = 3f, tooltip = "How many times the lens flares are downsampled. Higher values have better performance")]
		[SerializeField]
		protected int m_lensFlareDownsamples = 2;

		
		[Field("Threshold", useProperty = "lensFlareThreshold", visibleCheck = "ShowLensFlare", tooltip = "This effect will not become visible until a pixel is this bright")]
		[SerializeField]
		protected float m_lensFlareThreshold;

		
		[Inspector.Decorations.Header(0, "Lens Dirt", visibleCheck = "ShowLensDirtActive")]
		[Toggle("Active", useProperty = "lensDirt", visibleCheck = "ShowLensDirtActive", tooltip = "Determines if lens dirt is used")]
		[SerializeField]
		protected bool m_lensDirt;

		
		[Field("Dirt Texture", useProperty = "lensDirtTexture", visibleCheck = "ShowLensDirt", tooltip = "The texture used as lens dirt")]
		[SerializeField]
		protected Texture m_lensDirtTexture;

		
		[Field("Bloom Threshold", useProperty = "lensDirtBloomThreshold", visibleCheck = "ShowLensDirtSettings", tooltip = "This effect will not become visible until a pixel is this bright")]
		[SerializeField]
		protected float m_lensDirtBloomThreshold;

		
		[Slider("Bloom Effect", useProperty = "lensDirtBloomEffect", visibleCheck = "ShowLensDirtSettings", minValue = 0f, maxValue = 1f, tooltip = "How strong the lens dirt effect is")]
		[SerializeField]
		protected float m_lensDirtBloomEffect = 0.3f;

		
		[Slider("Bloom Brightness Scale", useProperty = "lensDirtBloomBrightness", visibleCheck = "ShowLensDirtSettings", minValue = 0.25f, maxValue = 2f, tooltip = "How bright the lens dirt effect is")]
		[SerializeField]
		protected float m_lensDirtBloomBrightness = 1.2f;

		
		[Slider("Lens Flare Effect", useProperty = "lensDirtLensFlareEffect", visibleCheck = "ShowLensDirtSettings", minValue = 0f, maxValue = 1f, tooltip = "How modulated the lens flare is by the dirt texture")]
		[SerializeField]
		protected float m_lensDirtLensFlareEffect = 1f;

		
		[Slider("Lens Flare Brightness Scale", useProperty = "lensDirtLensFlareBrightness", visibleCheck = "ShowLensDirtSettings", minValue = 0.25f, maxValue = 2f, tooltip = "How bright the lens dirt effect is")]
		[SerializeField]
		protected float m_lensDirtLensFlareBrightness = 1f;

		
		[Inspector.Decorations.Header(0, "Tonemapping", visibleCheck = "ShowTonemapping")]
		[Field("Mode", useProperty = "tonemappingMode", visibleCheck = "ShowTonemapping", tooltip = "What type of tonemapping algorithm is used")]
		[SerializeField]
		protected TonemappingMode m_tonemappingMode = TonemappingMode.Filmic;

		
		[Slider("White Point", useProperty = "whitePoint", visibleCheck = "ShowTonemapping", minValue = 0.5f, maxValue = 20f, tooltip = "At what intensity pixels will become white")]
		[SerializeField]
		protected float m_whitePoint = 7f;

		
		[Inspector.Decorations.Header(0, "Camera Mode")]
		[Field("Camera Mode", useProperty = "cameraMode", visibleCheck = "ShowCameraMode", tooltip = "What camera mode is used")]
		[SerializeField]
		protected CameraMode m_cameraMode = CameraMode.AutoPriority;

		
		[Slider("F Number", useProperty = "fNumber", visibleCheck = "ShowFNumber", minValue = 1.4f, maxValue = 22f, tooltip = "The F number of the camera")]
		[SerializeField]
		protected float m_fNumber = 4f;

		
		[Slider("ISO", useProperty = "ISO", visibleCheck = "ShowISO", minValue = 100f, maxValue = 6400f, tooltip = "The ISO setting of the camera")]
		[SerializeField]
		protected float m_ISO = 100f;

		
		[Slider("Shutter Speed", useProperty = "shutterSpeed", visibleCheck = "ShowShutterSpeed", minValue = 0.00025f, maxValue = 0.0333333351f, tooltip = "The shutted speed of the camera")]
		[SerializeField]
		protected float m_shutterSpeed = 0.01f;

		
		[Toggle("Manual Focal Length", useProperty = "userControlledFocalLength", tooltip = "If false the focal length will be derived from the camera's field of view. Enabled this will drive the field of view")]
		[SerializeField]
		protected bool m_userControlledFocalLength;

		
		[Slider("Focal Length", useProperty = "focalLength", visibleCheck = "ShowFocalLength", minValue = 10f, maxValue = 250f, tooltip = "The focal length of the camera in millimeters")]
		[SerializeField]
		protected float m_focalLength = 15f;

		
		[Inspector.Decorations.Header(0, "Exposure Settings")]
		[Slider("Exposure Compensation", useProperty = "exposureCompensation", visibleCheck = "ShowExposureComp", minValue = -8f, maxValue = 8f, tooltip = "Allows you to manually compensate towards the desired exposure")]
		[SerializeField]
		protected float m_exposureCompensation;

		
		[MinMaxSlider("Min Max Exposure", -16f, 16f, useProperty = "minMaxExposure")]
		[SerializeField]
		protected Vector2 m_minMaxExposure = new Vector2(-16f, 16f);

		
		[Slider("Adaption Speed", useProperty = "adaptionSpeed", visibleCheck = "ShowExposureAdaption", minValue = 0.1f, maxValue = 8f, tooltip = "How fast the exposure is allowed to change")]
		[SerializeField]
		protected float m_adaptionSpeed = 1f;

		
		[Inspector.Decorations.Header(0, "Depth of Field")]
		[Toggle("Active", useProperty = "depthOfField", tooltip = "Determines if depth of field is used")]
		[SerializeField]
		protected bool m_depthOfField = true;

		
		[Tooltip("Excludes layers from the depth of field")]
		[SerializeField]
		protected LayerMask m_exclusionMask;

		
		[Slider("Max Radius", useProperty = "maxCoCRadius", visibleCheck = "ShowDepthOfField", minValue = 8f, maxValue = 20f, tooltip = "The maximum radius the blur can be. Lower values will have less artifacts. Set this as high as you can without seeing artifacts")]
		[SerializeField]
		protected float m_maxCoCRadius = 20f;

		
		[Toggle("Temporal Supersampling", useProperty = "depthOfFieldTemporalSupersampling", visibleCheck = "ShowDepthOfField", tooltip = "If active the Depth of Field will be temporally supersampled. This improves quality but can cause an unsharp result")]
		[SerializeField]
		protected bool m_depthOfFieldTemporalSupersampling = true;

		
		[Slider("Temporal Blend", useProperty = "depthOfFieldTemporalBlend", visibleCheck = "ShowTemporalSettings", minValue = 0.8f, maxValue = 0.95f, tooltip = "How much of the final result is influenced by the temporal supersampling")]
		[SerializeField]
		protected float m_depthOfFieldTemporalBlend = 0.9f;

		
		[Slider("Temporal Steps", useProperty = "depthOfFieldTemporalSteps", visibleCheck = "ShowTemporalSettings", minValue = 4f, maxValue = 10f, tooltip = "How many different sample offsets the temporal supersampling uses before looping around")]
		[SerializeField]
		protected int m_depthOfFieldTemporalSteps = 8;

		
		[Field("Sample Count", useProperty = "depthOfFieldSamples", visibleCheck = "ShowDepthOfField", tooltip = "How many samples the algorithm does")]
		[SerializeField]
		protected DepthOfFieldSamples m_depthOfFieldSamples;

		
		[Field("Depth Focus Mode", useProperty = "depthFocusMode", visibleCheck = "ShowDepthOfField", tooltip = "How the depth focus point is chosen")]
		[SerializeField]
		protected DepthFocusMode m_depthFocusMode = DepthFocusMode.PointAverage;

		
		[Toggle("Visualize Focal Distance", useProperty = "visualizeFocalDistance", visibleCheck = "ShowDepthOfField", tooltip = "Visualizes the areas that are in and out of focus")]
		[SerializeField]
		protected bool m_visualizeFocalDistance;

		
		[Field("Target Transform", useProperty = "depthOfFieldTargetTransform", visibleCheck = "ShowTargetTransform", tooltip = "Will autofocus on the depth from this camera to the target transform")]
		[SerializeField]
		protected Transform m_depthOfFieldTargetTransform;

		
		[Field("Point Center", useProperty = "pointAveragePosition", visibleCheck = "ShowPointAverage", tooltip = "Where the center of focus is on the screen. [0,0] is the bottom left corner and [1,1] is the top right")]
		[SerializeField]
		protected Vector2 m_pointAveragePosition = new Vector2(0.5f, 0.5f);

		
		[Inspector.Decorations.Space(0, 1)]
		[Slider("Point Range", useProperty = "pointAverageRange", visibleCheck = "ShowPointAverage", minValue = 0.01f, maxValue = 0.2f, tooltip = "How far the point average calculation reaches")]
		[SerializeField]
		protected float m_pointAverageRange = 0.1f;

		
		[Toggle("Visualize Point Focus", useProperty = "visualizePointFocus", visibleCheck = "ShowPointAverage", tooltip = "Show the area of influence on the main screen for visualizaiton")]
		[SerializeField]
		protected bool m_visualizePointFocus;

		
		[Slider("Adaption Speed", useProperty = "depthAdaptionSpeed", visibleCheck = "ShowPointAverage", minValue = 1f, maxValue = 30f, tooltip = "Dictates how fast the focal distance changes")]
		[SerializeField]
		protected float m_depthAdaptionSpeed = 15f;

		
		[Field("Focal Distance", useProperty = "focalDistance", visibleCheck = "ShowFocalDistance", tooltip = "The focal distance in meters")]
		[SerializeField]
		protected float m_focalDistance = 10f;

		
		[Slider("Depth Range", useProperty = "focalRange", visibleCheck = "ShowFocalRange", minValue = 0f, maxValue = 50f, tooltip = "The length of the range that is 100% in focus")]
		[SerializeField]
		protected float m_focalRange = 10f;

		
		[Inspector.Decorations.Header(0, "Color Correction")]
		[Field("Mode", useProperty = "colorGradingMode", tooltip = "Which color correction mode is currently active")]
		[SerializeField]
		protected ColorGradingMode m_colorGradingMode;

		
		[Field("Lookup Texture", useProperty = "colorGradingTex1", visibleCheck = "ShowCCTex1", tooltip = "The lookup texture used for color correction")]
		[SerializeField]
		protected Texture2D m_colorGradingTex1;

		
		[Field("Blend Lookup Texture", useProperty = "colorGradingTex2", visibleCheck = "ShowCCTex2", tooltip = "The lookup texture blended in as the blend factor increases")]
		[SerializeField]
		protected Texture2D m_colorGradingTex2;

		
		[Slider("Blend Factor", useProperty = "colorGradingBlendFactor", visibleCheck = "ShowCCTex2", minValue = 0f, maxValue = 1f, tooltip = "Interpolates between the original color correction texture and the blend target color correction texture")]
		[SerializeField]
		protected float m_colorGradingBlendFactor;

		
		private Camera m_camera;

		
		[HideInInspector]
		[SerializeField]
		private bool forceFillParams = true;

		
		protected bool m_isFirstRender = true;

		
		protected float prevCamFoV;

		
		protected Transform m_cameraTransform;

		
		protected Bloom m_bloomClass;

		
		protected ScionLensFlare m_lensFlareClass;

		
		protected VirtualCamera m_virtualCamera;

		
		protected CombinationPass m_combinationPass;

		
		protected Downsampling m_downsampling;

		
		protected DepthOfField m_depthOfFieldClass;

		
		protected PostProcessParameters postProcessParams = new PostProcessParameters();

		
		protected ScionDebug m_scionDebug;

		
		public static ScionDebug ActiveDebug;

		
		private bool prevRenderWasHDR;
	}
}
