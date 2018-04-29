using System;
using UnityEngine;

namespace ScionEngine
{
	
	public class PostProcessParameters
	{
		
		public PostProcessParameters()
		{
			this.glareParams = default(GlareParameters);
			this.lensDirtParams = default(LensDirtParameters);
			this.cameraParams = default(CameraParameters);
			this.DoFParams = default(DepthOfFieldParameters);
			this.colorGradingParams = default(ColorGradingParameters);
			this.preCalcValues = default(PreCalcValues);
			this.commonPostProcess = default(CommonPostProcess);
		}

		
		public void Fill(ScionPostProcessBase postProcess, bool forceFill)
		{
			if (this.isFilled && !forceFill)
			{
				return;
			}
			this.isFilled = true;
			this.bloom = postProcess.bloom;
			this.lensFlare = postProcess.lensFlare;
			this.lensDirt = postProcess.lensDirt;
			this.lensDirtTexture = postProcess.lensDirtTexture;
			this.bloomTexture = null;
			this.dofTexture = null;
			this.exposure = (postProcess.cameraMode != CameraMode.Off);
			this.depthOfField = postProcess.depthOfField;
			this.halfResSource = null;
			this.glareParams.threshold = postProcess.bloomThreshold;
			this.glareParams.intensity = ScionUtility.Square(postProcess.bloomIntensity);
			this.glareParams.brightness = postProcess.bloomBrightness;
			this.glareParams.distanceMultiplier = postProcess.bloomDistanceMultiplier;
			this.glareParams.downsamples = postProcess.bloomDownsamples;
			this.lensFlareParams.ghostSamples = postProcess.lensFlareGhostSamples;
			this.lensFlareParams.ghostIntensity = postProcess.lensFlareGhostIntensity;
			this.lensFlareParams.ghostDispersal = postProcess.lensFlareGhostDispersal;
			this.lensFlareParams.ghostDistortion = postProcess.lensFlareGhostDistortion;
			this.lensFlareParams.ghostEdgeFade = postProcess.lensFlareGhostEdgeFade;
			this.lensFlareParams.haloIntensity = postProcess.lensFlareHaloIntensity;
			this.lensFlareParams.haloWidth = postProcess.lensFlareHaloWidth;
			this.lensFlareParams.haloDistortion = postProcess.lensFlareHaloDistortion;
			this.lensFlareParams.starUVScale = postProcess.lensFlareDiffractionUVScale;
			this.lensFlareParams.starTexture = postProcess.lensFlareDiffractionTexture;
			this.lensFlareParams.lensColorTexture = postProcess.lensFlareLensColorTexture;
			this.lensFlareParams.blurSamples = postProcess.lensFlareBlurSamples;
			this.lensFlareParams.blurStrength = postProcess.lensFlareBlurStrength;
			this.lensFlareParams.downsamples = postProcess.lensFlareDownsamples;
			this.lensFlareParams.threshold = postProcess.lensFlareThreshold;
			this.lensDirtParams.bloomThreshold = postProcess.lensDirtBloomThreshold;
			this.lensDirtParams.bloomEffect = ScionUtility.Square(postProcess.lensDirtBloomEffect);
			this.lensDirtParams.bloomBrightness = postProcess.lensDirtBloomBrightness;
			this.lensDirtParams.lensFlareEffect = postProcess.lensDirtLensFlareEffect;
			this.lensDirtParams.lensFlareBrightness = postProcess.lensDirtLensFlareBrightness;
			this.DoFParams.depthFocusMode = postProcess.depthFocusMode;
			this.DoFParams.maxCoCRadius = postProcess.maxCoCRadius;
			this.DoFParams.quality = postProcess.depthOfFieldSamples;
			this.DoFParams.visualizeFocalDistance = postProcess.visualizeFocalDistance;
			this.DoFParams.pointAveragePosition = postProcess.pointAveragePosition;
			this.DoFParams.pointAverageRange = postProcess.pointAverageRange;
			this.DoFParams.visualizePointFocus = postProcess.visualizePointFocus;
			this.DoFParams.depthAdaptionSpeed = postProcess.depthAdaptionSpeed;
			this.DoFParams.focalDistance = postProcess.focalDistance;
			this.DoFParams.focalRange = postProcess.focalRange;
			this.DoFParams.useTemporal = postProcess.depthOfFieldTemporalSupersampling;
			this.DoFParams.temporalBlend = postProcess.depthOfFieldTemporalBlend;
			this.DoFParams.temporalSteps = postProcess.depthOfFieldTemporalSteps;
			this.colorGradingParams.colorGradingMode = ((!(postProcess.colorGradingTex1 == null)) ? postProcess.colorGradingMode : ColorGradingMode.Off);
			this.colorGradingParams.colorGradingTex1 = postProcess.colorGradingTex1;
			this.colorGradingParams.colorGradingTex2 = postProcess.colorGradingTex2;
			this.colorGradingParams.colorGradingBlendFactor = postProcess.colorGradingBlendFactor;
			this.cameraParams.cameraMode = postProcess.cameraMode;
			this.cameraParams.fNumber = postProcess.fNumber;
			this.cameraParams.ISO = postProcess.ISO;
			this.cameraParams.shutterSpeed = postProcess.shutterSpeed;
			this.cameraParams.adaptionSpeed = postProcess.adaptionSpeed;
			this.cameraParams.minMaxExposure = postProcess.minMaxExposure;
			this.cameraParams.exposureCompensation = postProcess.exposureCompensation;
			this.commonPostProcess.grainIntensity = ((!postProcess.grain) ? 0f : postProcess.grainIntensity);
			this.commonPostProcess.vignetteIntensity = ((!postProcess.vignette) ? 0f : postProcess.vignetteIntensity);
			this.commonPostProcess.vignetteScale = postProcess.vignetteScale;
			this.commonPostProcess.vignetteColor = postProcess.vignetteColor;
			this.commonPostProcess.chromaticAberration = postProcess.chromaticAberration;
			this.commonPostProcess.chromaticAberrationDistortion = postProcess.chromaticAberrationDistortion;
			this.commonPostProcess.chromaticAberrationIntensity = postProcess.chromaticAberrationIntensity;
		}

		
		public Camera camera;

		
		public Transform cameraTransform;

		
		public bool tonemapping;

		
		public bool bloom;

		
		public bool lensFlare;

		
		public bool lensDirt;

		
		public bool exposure;

		
		public bool depthOfField;

		
		public bool isFirstRender;

		
		public int width;

		
		public int height;

		
		public int halfWidth;

		
		public int halfHeight;

		
		public RenderTexture halfResSource;

		
		public RenderTexture halfResDepth;

		
		public RenderTexture bloomTexture;

		
		public RenderTexture lensFlareTexture;

		
		public RenderTexture dofTexture;

		
		public Texture lensDirtTexture;

		
		public GlareParameters glareParams;

		
		public LensFlareParameters lensFlareParams;

		
		public LensDirtParameters lensDirtParams;

		
		public CameraParameters cameraParams;

		
		public DepthOfFieldParameters DoFParams;

		
		public ColorGradingParameters colorGradingParams;

		
		public PreCalcValues preCalcValues;

		
		public CommonPostProcess commonPostProcess;

		
		private bool isFilled;
	}
}
