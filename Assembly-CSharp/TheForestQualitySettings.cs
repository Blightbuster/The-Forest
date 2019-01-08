using System;
using TheForest.Graphics;
using TheForest.Utils;
using UnityEngine;

public class TheForestQualitySettings : ScriptableObject
{
	public float DrawDistanceRatio
	{
		get
		{
			switch (this.DrawDistance)
			{
			case TheForestQualitySettings.DrawDistances.Ultra:
			case TheForestQualitySettings.DrawDistances.VeryHigh:
			case TheForestQualitySettings.DrawDistances.High:
				return 1f;
			case TheForestQualitySettings.DrawDistances.Medium:
				return 0.8f;
			case TheForestQualitySettings.DrawDistances.Low:
				return 0.6f;
			case TheForestQualitySettings.DrawDistances.UltraLow:
				return 0.4f;
			default:
				return 1f;
			}
		}
	}

	public float DrawDistanceGreebleRatio
	{
		get
		{
			switch (this.DrawDistance)
			{
			case TheForestQualitySettings.DrawDistances.Ultra:
				return 2f;
			case TheForestQualitySettings.DrawDistances.VeryHigh:
				return 1.5f;
			case TheForestQualitySettings.DrawDistances.High:
			case TheForestQualitySettings.DrawDistances.Medium:
			case TheForestQualitySettings.DrawDistances.Low:
			case TheForestQualitySettings.DrawDistances.UltraLow:
				return 1f;
			default:
				return 1f;
			}
		}
	}

	public int MaterialQualityShaderLOD
	{
		get
		{
			TheForestQualitySettings.MaterialQualities materialQuality = this.MaterialQuality;
			if (materialQuality == TheForestQualitySettings.MaterialQualities.High)
			{
				return 600;
			}
			if (materialQuality == TheForestQualitySettings.MaterialQualities.Medium)
			{
				return 600;
			}
			if (materialQuality != TheForestQualitySettings.MaterialQualities.Low)
			{
				return 1000;
			}
			return 600;
		}
	}

	public float TerrainQualityPixelErrorPercentage
	{
		get
		{
			TheForestQualitySettings.TerrainQualities terrainQuality = this.TerrainQuality;
			if (terrainQuality == TheForestQualitySettings.TerrainQualities.POM)
			{
				return 16.1999989f;
			}
			if (terrainQuality == TheForestQualitySettings.TerrainQualities.PM)
			{
				return 27f;
			}
			if (terrainQuality != TheForestQualitySettings.TerrainQualities.SIMPLE)
			{
				return 0f;
			}
			return 37.8f;
		}
	}

	public TheForestQualitySettings.PostEffectsSystems PostEffectsSystem
	{
		get
		{
			if (PlayerPreferences.IsBelowDX11)
			{
				return TheForestQualitySettings.PostEffectsSystems.Legacy;
			}
			return this._postEffectSystem;
		}
		set
		{
			this._postEffectSystem = value;
		}
	}

	public TheForestQualitySettings.ShadowCascadeCount CascadeCount
	{
		get
		{
			return this._cascadeCount;
		}
		set
		{
			this._cascadeCount = value;
			TheForestQualitySettings.ShadowCascadeCount cascadeCount = this._cascadeCount;
			if (cascadeCount != TheForestQualitySettings.ShadowCascadeCount.FourCascades)
			{
				if (cascadeCount != TheForestQualitySettings.ShadowCascadeCount.TwoCascades)
				{
					QualitySettings.shadowCascades = 0;
				}
				else
				{
					QualitySettings.shadowCascades = 2;
				}
			}
			else
			{
				QualitySettings.shadowCascades = 4;
			}
			if (Scene.Atmosphere)
			{
				Scene.Atmosphere.SetShadowQualityLevel((int)this._cascadeCount);
			}
		}
	}

	public static void Save()
	{
		PlayerPrefs.SetInt("Quality_v016_Preset", (int)TheForestQualitySettings.UserSettings.Preset);
		PlayerPrefs.SetInt("Quality_v016_MotionBlur", (int)TheForestQualitySettings.UserSettings.MotionBlur);
		PlayerPrefs.SetInt("Quality_v016_DrawDistance", (int)TheForestQualitySettings.UserSettings.DrawDistance);
		PlayerPrefs.SetInt("Quality_v016_GrassMode", (int)TheForestQualitySettings.UserSettings.GrassMode);
		PlayerPrefs.SetFloat("Quality_v016_GrassDistance", TheForestQualitySettings.UserSettings.GrassDistance);
		PlayerPrefs.SetFloat("Quality_v016_GrassDensity", TheForestQualitySettings.UserSettings.GrassDensity);
		PlayerPrefs.SetInt("Quality_v016_SSAOType", (int)TheForestQualitySettings.UserSettings.SSAOType);
		PlayerPrefs.SetInt("Quality_v016_SSAO", (int)TheForestQualitySettings.UserSettings.SSAO);
		PlayerPrefs.SetInt("Quality_v016_SEBloom", (int)TheForestQualitySettings.UserSettings.SEBloom);
		PlayerPrefs.SetInt("Quality_v016_CA", (int)TheForestQualitySettings.UserSettings.CA);
		PlayerPrefs.SetInt("Quality_v016_Fg", (int)TheForestQualitySettings.UserSettings.Fg);
		PlayerPrefs.SetInt("Quality_v016_DofTech", (int)TheForestQualitySettings.UserSettings.DofTech);
		PlayerPrefs.SetInt("Quality_v016_ScreenSpaceReflection", (int)TheForestQualitySettings.UserSettings.screenSpaceReflection);
		PlayerPrefs.SetInt("Quality_v016_VolumetricClouds", (int)TheForestQualitySettings.UserSettings.volumetricClouds);
		PlayerPrefs.SetInt("Quality_v016_SunshineOcclusion", (int)TheForestQualitySettings.UserSettings.SunshineOcclusion);
		PlayerPrefs.SetInt("Quality_v016_VolumetricsType", (int)TheForestQualitySettings.UserSettings.VolumetricsType);
		PlayerPrefs.SetInt("Quality_v016_PostEffectsSystem", (int)TheForestQualitySettings.UserSettings.PostEffectsSystem);
		PlayerPrefs.SetInt("Quality_v016_Caustics", (int)TheForestQualitySettings.UserSettings.Caustics);
		PlayerPrefs.SetInt("Quality_v016_AntiAliasing", (int)TheForestQualitySettings.UserSettings.AntiAliasing);
		PlayerPrefs.SetInt("Quality_v016_LightmapResolution", TheForestQualitySettings.UserSettings.LightmapResolution);
		PlayerPrefs.SetFloat("Quality_v016_LightDistance", TheForestQualitySettings.UserSettings.LightDistance);
		PlayerPrefs.SetInt("Quality_v016_LightmapUpdateIntervalFrames", TheForestQualitySettings.UserSettings.LightmapUpdateIntervalFrames);
		PlayerPrefs.SetInt("Quality_v016_FarShadowMode", (int)TheForestQualitySettings.UserSettings.FarShadowMode);
		PlayerPrefs.SetInt("Quality_v016_CascadeCount", (int)TheForestQualitySettings.UserSettings.CascadeCount);
		PlayerPrefs.SetInt("Quality_v016_ShadowLevel", (int)TheForestQualitySettings.UserSettings.ShadowLevel);
		PlayerPrefs.SetInt("Quality_v016_ScatterResolution", (int)TheForestQualitySettings.UserSettings.ScatterResolution);
		PlayerPrefs.SetInt("Quality_v016_ScatterSamplingQuality", (int)TheForestQualitySettings.UserSettings.ScatterSamplingQuality);
		PlayerPrefs.SetInt("Quality_v016_TerrainQuality", (int)TheForestQualitySettings.UserSettings.TerrainQuality);
		PlayerPrefs.SetInt("Quality_v016_ReflexionMode", (int)TheForestQualitySettings.UserSettings.ReflexionMode);
		PlayerPrefs.SetInt("Quality_v016_OceanQuality", (int)TheForestQualitySettings.UserSettings.OceanQuality);
		PlayerPrefs.SetInt("Quality_v016_MaterialQuality", (int)TheForestQualitySettings.UserSettings.MaterialQuality);
		PlayerPrefs.SetInt("Quality_v016_TextureQuality", (int)TheForestQualitySettings.UserSettings.TextureQuality);
		PlayerPrefs.SetInt("Quality_v016_Saved", 1);
		PlayerPrefs.SetInt("Quality_v016_Version", 35);
		PlayerPrefs.Save();
	}

	public static bool Load()
	{
		bool flag = TheForestQualitySettings.HasSavedSettings();
		if (flag)
		{
			int @int = PlayerPrefs.GetInt("Quality_v016_Version", 35);
			TheForestQualitySettings.UserSettings.Preset = (TheForestQualitySettings.PresetLevels)PlayerPrefs.GetInt("Quality_v016_Preset", (int)TheForestQualitySettings.UserSettings.Preset);
			int num = PlayerPrefs.GetInt("Quality_v016_CascadeCount", (int)TheForestQualitySettings.UserSettings.CascadeCount);
			int num2 = PlayerPrefs.GetInt("Quality_v016_ShadowLevel", (int)TheForestQualitySettings.UserSettings.ShadowLevel);
			if (@int < 35)
			{
				num2++;
			}
			if (PlayerPreferences.is32bit)
			{
				num = Mathf.Max(num, 1);
				num2 = Mathf.Max(num, 1);
			}
			TheForestQualitySettings.UserSettings.CascadeCount = (TheForestQualitySettings.ShadowCascadeCount)num;
			TheForestQualitySettings.UserSettings.ShadowLevel = (TheForestQualitySettings.ShadowLevels)num2;
			TheForestQualitySettings.UserSettings.FarShadowMode = (TheForestQualitySettings.FarShadowModes)PlayerPrefs.GetInt("Quality_v016_FarShadowMode", (int)TheForestQualitySettings.UserSettings.FarShadowMode);
			TheForestQualitySettings.UserSettings.MotionBlur = (TheForestQualitySettings.MotionBlurQuality)PlayerPrefs.GetInt("Quality_v016_MotionBlur", (int)TheForestQualitySettings.UserSettings.MotionBlur);
			TheForestQualitySettings.UserSettings.DrawDistance = (TheForestQualitySettings.DrawDistances)Mathf.Clamp(PlayerPrefs.GetInt("Quality_v016_DrawDistance", (int)TheForestQualitySettings.UserSettings.DrawDistance), 1, 5);
			TheForestQualitySettings.UserSettings.GrassMode = (TheForestQualitySettings.GrassModes)PlayerPrefs.GetInt("Quality_v016_GrassMode", (int)TheForestQualitySettings.UserSettings.GrassMode);
			TheForestQualitySettings.UserSettings.GrassDistance = PlayerPrefs.GetFloat("Quality_v016_GrassDistance", TheForestQualitySettings.UserSettings.GrassDistance);
			TheForestQualitySettings.UserSettings.GrassDensity = PlayerPrefs.GetFloat("Quality_v016_GrassDensity", TheForestQualitySettings.UserSettings.GrassDensity);
			TheForestQualitySettings.UserSettings.SSAOType = (TheForestQualitySettings.SSAOTypes)PlayerPrefs.GetInt("Quality_v016_SSAOType", (int)TheForestQualitySettings.UserSettings.SSAOType);
			TheForestQualitySettings.UserSettings.SSAO = (TheForestQualitySettings.SSAOTechnique)PlayerPrefs.GetInt("Quality_v016_SSAO", (int)TheForestQualitySettings.UserSettings.SSAO);
			TheForestQualitySettings.UserSettings.SEBloom = (TheForestQualitySettings.SEBloomTechnique)PlayerPrefs.GetInt("Quality_v016_SEBloom", (int)TheForestQualitySettings.UserSettings.SEBloom);
			TheForestQualitySettings.UserSettings.CA = (TheForestQualitySettings.ChromaticAberration)PlayerPrefs.GetInt("Quality_v016_CA", (int)TheForestQualitySettings.UserSettings.CA);
			TheForestQualitySettings.UserSettings.Fg = (TheForestQualitySettings.FilmGrain)PlayerPrefs.GetInt("Quality_v016_Fg", (int)TheForestQualitySettings.UserSettings.Fg);
			TheForestQualitySettings.UserSettings.DofTech = (TheForestQualitySettings.Dof)PlayerPrefs.GetInt("Quality_v016_DofTech", (int)TheForestQualitySettings.UserSettings.DofTech);
			TheForestQualitySettings.UserSettings.screenSpaceReflection = (TheForestQualitySettings.ScreenSpaceReflection)PlayerPrefs.GetInt("Quality_v016_ScreenSpaceReflection", (int)TheForestQualitySettings.UserSettings.screenSpaceReflection);
			TheForestQualitySettings.UserSettings.volumetricClouds = (TheForestQualitySettings.VolumetricClouds)PlayerPrefs.GetInt("Quality_v016_VolumetricClouds", (int)TheForestQualitySettings.UserSettings.volumetricClouds);
			TheForestQualitySettings.UserSettings.SunshineOcclusion = (TheForestQualitySettings.SunshineOcclusionOn)PlayerPrefs.GetInt("Quality_v016_SunshineOcclusion", (int)TheForestQualitySettings.UserSettings.SunshineOcclusion);
			TheForestQualitySettings.UserSettings.VolumetricsType = (TheForestQualitySettings.VolumetricsTypes)PlayerPrefs.GetInt("Quality_v016_VolumetricsType", (int)TheForestQualitySettings.UserSettings.VolumetricsType);
			TheForestQualitySettings.UserSettings.PostEffectsSystem = (TheForestQualitySettings.PostEffectsSystems)PlayerPrefs.GetInt("Quality_v016_PostEffectsSystem", (int)TheForestQualitySettings.UserSettings.PostEffectsSystem);
			TheForestQualitySettings.UserSettings.Caustics = (TheForestQualitySettings.CausticsOn)PlayerPrefs.GetInt("Quality_v016_Caustics", (int)TheForestQualitySettings.UserSettings.Caustics);
			TheForestQualitySettings.UserSettings.AntiAliasing = (TheForestQualitySettings.AntiAliasingTechnique)PlayerPrefs.GetInt("Quality_v016_AntiAliasing", (int)TheForestQualitySettings.UserSettings.AntiAliasing);
			TheForestQualitySettings.UserSettings.LightmapResolution = PlayerPrefs.GetInt("Quality_v016_LightmapResolution", TheForestQualitySettings.UserSettings.LightmapResolution);
			TheForestQualitySettings.UserSettings.LightDistance = PlayerPrefs.GetFloat("Quality_v016_LightDistance", TheForestQualitySettings.UserSettings.LightDistance);
			TheForestQualitySettings.UserSettings.LightmapUpdateIntervalFrames = PlayerPrefs.GetInt("Quality_v016_LightmapUpdateIntervalFrames", TheForestQualitySettings.UserSettings.LightmapUpdateIntervalFrames);
			TheForestQualitySettings.UserSettings.ScatterResolution = (SunshineRelativeResolutions)PlayerPrefs.GetInt("Quality_v016_ScatterResolution", (int)TheForestQualitySettings.UserSettings.ScatterResolution);
			TheForestQualitySettings.UserSettings.ScatterSamplingQuality = (SunshineScatterSamplingQualities)PlayerPrefs.GetInt("Quality_v016_ScatterSamplingQuality", (int)TheForestQualitySettings.UserSettings.ScatterSamplingQuality);
			TheForestQualitySettings.UserSettings.SetTerrainQuality((TheForestQualitySettings.TerrainQualities)PlayerPrefs.GetInt("Quality_v016_TerrainQuality", (int)TheForestQualitySettings.UserSettings.TerrainQuality));
			TheForestQualitySettings.UserSettings.ReflexionMode = (TheForestQualitySettings.ReflexionModes)PlayerPrefs.GetInt("Quality_v016_ReflexionMode", (int)TheForestQualitySettings.UserSettings.ReflexionMode);
			TheForestQualitySettings.UserSettings.OceanQuality = (TheForestQualitySettings.OceanQualities)PlayerPrefs.GetInt("Quality_v016_OceanQuality", 0);
			TheForestQualitySettings.UserSettings.SetTextureQuality((TheForestQualitySettings.TextureQualities)PlayerPrefs.GetInt("Quality_v016_TextureQuality", 0));
			TheForestQualitySettings.UserSettings.SetMaterialQuality((TheForestQualitySettings.MaterialQualities)PlayerPrefs.GetInt("Quality_v016_MaterialQuality", (int)TheForestQualitySettings.UserSettings.MaterialQuality));
		}
		return flag;
	}

	public static bool HasSavedSettings()
	{
		return PlayerPrefs.HasKey("Quality_v016_Saved");
	}

	public static TheForestQualitySettings CurrentPreset
	{
		get
		{
			return TheForestQualitySettings.GetPreset(QualitySettings.GetQualityLevel());
		}
	}

	internal static void SetUnityQualityFromShadowLevel(TheForestQualitySettings.ShadowLevels shadowLevel)
	{
		QualitySettings.SetQualityLevel(TheForestQualitySettings.ConvertToQualityIndex(shadowLevel));
		TheForestQualitySettings.UpdateUnityQualityShadowResolution(shadowLevel);
	}

	public static void UpdateUnityQualityShadowResolution(TheForestQualitySettings.ShadowLevels shadowLevel)
	{
		switch (shadowLevel)
		{
		case TheForestQualitySettings.ShadowLevels.VeryHigh:
			QualitySettings.shadowResolution = ShadowResolution.High;
			break;
		case TheForestQualitySettings.ShadowLevels.High:
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			break;
		case TheForestQualitySettings.ShadowLevels.Medium:
		case TheForestQualitySettings.ShadowLevels.Low:
		case TheForestQualitySettings.ShadowLevels.Fastest:
		case TheForestQualitySettings.ShadowLevels.UltraLow:
			QualitySettings.shadowResolution = ShadowResolution.Low;
			break;
		}
	}

	internal static int ConvertToQualityIndex(TheForestQualitySettings.ShadowLevels shadowLevel)
	{
		int result;
		switch (shadowLevel)
		{
		case TheForestQualitySettings.ShadowLevels.VeryHigh:
		case TheForestQualitySettings.ShadowLevels.High:
			result = 0;
			break;
		case TheForestQualitySettings.ShadowLevels.Medium:
			result = 1;
			break;
		case TheForestQualitySettings.ShadowLevels.Low:
			result = 2;
			break;
		case TheForestQualitySettings.ShadowLevels.Fastest:
			result = 3;
			break;
		case TheForestQualitySettings.ShadowLevels.UltraLow:
			result = 4;
			break;
		default:
			result = 1;
			break;
		}
		return result;
	}

	public static TheForestQualitySettings UserSettings
	{
		get
		{
			if (!TheForestQualitySettings.userSettings)
			{
				TheForestQualitySettings.userSettings = ScriptableObject.CreateInstance<TheForestQualitySettings>();
				TheForestQualitySettings.CopyPreset(-1);
			}
			return TheForestQualitySettings.userSettings;
		}
	}

	public static TheForestQualitySettings GetPreset(int level = -1)
	{
		int num = QualitySettings.names.Length;
		level = Mathf.Clamp(level, 0, num - 1);
		if (TheForestQualitySettings.Presets == null || level >= num)
		{
			TheForestQualitySettings.Presets = new TheForestQualitySettings[num];
		}
		if (TheForestQualitySettings.Presets[level] == null)
		{
			TheForestQualitySettings.Presets[level] = (Resources.Load(string.Format("Quality/{0}", level), typeof(TheForestQualitySettings)) as TheForestQualitySettings);
			if (!TheForestQualitySettings.Presets[level])
			{
				Debug.LogWarning(string.Format("Quality Settings Missing.  Please create Assets/Resources/{0}.asset", level));
				TheForestQualitySettings.Presets[level] = ScriptableObject.CreateInstance<TheForestQualitySettings>();
			}
		}
		return TheForestQualitySettings.Presets[level];
	}

	public static void CopyPreset(int level = -1)
	{
		if (level < 0)
		{
			level = PlayerPreferences.Preset;
		}
		TheForestQualitySettings.Copy(TheForestQualitySettings.GetPreset(level), TheForestQualitySettings.UserSettings);
	}

	public static void Copy(TheForestQualitySettings from, TheForestQualitySettings to)
	{
		to.Preset = from.Preset;
		to.AntiAliasing = from.AntiAliasing;
		if (PlayerPreferences.is32bit)
		{
			to.CascadeCount = (TheForestQualitySettings.ShadowCascadeCount)Mathf.Max((int)from.CascadeCount, 1);
		}
		else
		{
			to.CascadeCount = from.CascadeCount;
		}
		to.ShadowLevel = from.ShadowLevel;
		to.FarShadowMode = from.FarShadowMode;
		to.LightDistance = from.LightDistance;
		to.LightmapResolution = from.LightmapResolution;
		to.LightmapUpdateIntervalFrames = from.LightmapUpdateIntervalFrames;
		to.MotionBlur = from.MotionBlur;
		to.ScatterResolution = from.ScatterResolution;
		to.ScatterSamplingQuality = from.ScatterSamplingQuality;
		to.SSAOType = from.SSAOType;
		to.SSAO = from.SSAO;
		to.PostEffectsSystem = from.PostEffectsSystem;
		to.SEBloom = from.SEBloom;
		to.Fg = from.Fg;
		to.CA = from.CA;
		to.DofTech = from.DofTech;
		to.screenSpaceReflection = from.screenSpaceReflection;
		to.volumetricClouds = from.volumetricClouds;
		to.SunshineOcclusion = from.SunshineOcclusion;
		to.VolumetricsType = from.VolumetricsType;
		to.Caustics = from.Caustics;
		to.SkyLighting = from.SkyLighting;
		to.SetTerrainQuality(from.TerrainQuality);
		to.DrawDistance = from.DrawDistance;
		to.GrassMode = from.GrassMode;
		to.GrassDistance = from.GrassDistance;
		to.GrassDensity = from.GrassDensity;
		to.ReflexionMode = from.ReflexionMode;
		to.OceanQuality = from.OceanQuality;
		to.SetTextureQuality(from.TextureQuality);
		to.SetMaterialQuality(from.MaterialQuality);
	}

	public static int GetScatterLevel()
	{
		int num = QualitySettings.names.Length;
		for (int i = 0; i < num; i++)
		{
			TheForestQualitySettings preset = TheForestQualitySettings.GetPreset(i);
			if (preset.ScatterResolution == TheForestQualitySettings.UserSettings.ScatterResolution && preset.ScatterSamplingQuality == TheForestQualitySettings.UserSettings.ScatterSamplingQuality)
			{
				return i;
			}
		}
		return -1;
	}

	public static TheForestQualitySettings.GrassModes GetGrassMode()
	{
		if (PlayerPreferences.IsBelowDX11)
		{
			return TheForestQualitySettings.GrassModes.CPU;
		}
		return TheForestQualitySettings.UserSettings.GrassMode;
	}

	public static int GetGrassLevel()
	{
		int num = QualitySettings.names.Length;
		for (int i = 0; i < num; i++)
		{
			TheForestQualitySettings preset = TheForestQualitySettings.GetPreset(i);
			if (preset.GrassDistance == TheForestQualitySettings.UserSettings.GrassDistance)
			{
				return i;
			}
		}
		return -1;
	}

	public static int GetGrassLevelDensity()
	{
		int num = QualitySettings.names.Length;
		for (int i = 0; i < num; i++)
		{
			TheForestQualitySettings preset = TheForestQualitySettings.GetPreset(i);
			if (preset.GrassDensity == TheForestQualitySettings.UserSettings.GrassDensity)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsCustomized
	{
		get
		{
			TheForestQualitySettings theForestQualitySettings = TheForestQualitySettings.UserSettings;
			TheForestQualitySettings preset = TheForestQualitySettings.GetPreset((int)theForestQualitySettings.Preset);
			return theForestQualitySettings.ShadowLevel != preset.ShadowLevel || theForestQualitySettings.AntiAliasing != preset.AntiAliasing || theForestQualitySettings.CascadeCount != preset.CascadeCount || theForestQualitySettings.FarShadowMode != preset.FarShadowMode || theForestQualitySettings.GrassMode != preset.GrassMode || !Mathf.Approximately(theForestQualitySettings.GrassDistance, preset.GrassDistance) || !Mathf.Approximately(theForestQualitySettings.GrassDensity, preset.GrassDensity) || !Mathf.Approximately(theForestQualitySettings.LightDistance, preset.LightDistance) || theForestQualitySettings.LightmapResolution != preset.LightmapResolution || theForestQualitySettings.LightmapUpdateIntervalFrames != preset.LightmapUpdateIntervalFrames || theForestQualitySettings.MotionBlur != preset.MotionBlur || theForestQualitySettings.ScatterResolution != preset.ScatterResolution || theForestQualitySettings.ScatterSamplingQuality != preset.ScatterSamplingQuality || theForestQualitySettings.SSAOType != preset.SSAOType || theForestQualitySettings.SSAO != preset.SSAO || theForestQualitySettings.PostEffectsSystem != preset.PostEffectsSystem || theForestQualitySettings.SEBloom != preset.SEBloom || theForestQualitySettings.Fg != preset.Fg || theForestQualitySettings.CA != preset.CA || theForestQualitySettings.DofTech != preset.DofTech || theForestQualitySettings.screenSpaceReflection != preset.screenSpaceReflection || theForestQualitySettings.volumetricClouds != preset.volumetricClouds || theForestQualitySettings.SunshineOcclusion != preset.SunshineOcclusion || theForestQualitySettings.VolumetricsType != preset.VolumetricsType || theForestQualitySettings.Caustics != preset.Caustics || theForestQualitySettings.SkyLighting != preset.SkyLighting || theForestQualitySettings.TerrainQuality != preset.TerrainQuality || theForestQualitySettings.DrawDistance != preset.DrawDistance || theForestQualitySettings.ReflexionMode != preset.ReflexionMode || theForestQualitySettings.OceanQuality != preset.OceanQuality || theForestQualitySettings.TextureQuality != preset.TextureQuality || theForestQualitySettings.MaterialQuality != preset.MaterialQuality;
		}
	}

	public void SetTextureQuality(TheForestQualitySettings.TextureQualities value)
	{
		if (PlayerPreferences.is32bit)
		{
			this.TextureQuality = TheForestQualitySettings.TextureQualities.QuaterRes;
		}
		else
		{
			this.TextureQuality = value;
		}
		if (QualitySettings.masterTextureLimit != (int)this.TextureQuality)
		{
			QualitySettings.masterTextureLimit = (int)this.TextureQuality;
			RTP_LODmanager.FreshTextures = false;
		}
	}

	public void SetMaterialQuality(TheForestQualitySettings.MaterialQualities value)
	{
		this.MaterialQuality = value;
		if (Application.isPlaying && WaterEngine.Lakes != null)
		{
			for (int i = 0; i < WaterEngine.Lakes.Count; i++)
			{
				WaterEngine.Lakes[i].InitMaterial();
			}
		}
		if (value == TheForestQualitySettings.MaterialQualities.Low)
		{
			Shader.EnableKeyword("SIMPLE_SHADING");
		}
		else
		{
			Shader.DisableKeyword("SIMPLE_SHADING");
		}
	}

	public void SetTerrainQuality(TheForestQualitySettings.TerrainQualities value)
	{
		this.TerrainQuality = value;
		Terrain activeTerrain = Terrain.activeTerrain;
		if (activeTerrain && activeTerrain.materialTemplate)
		{
			Material materialTemplate = activeTerrain.materialTemplate;
			if (value == TheForestQualitySettings.TerrainQualities.POM)
			{
				materialTemplate.EnableKeyword("_EXTRUSIONMODE_POM");
				materialTemplate.DisableKeyword("_EXTRUSIONMODE_PM");
			}
			else
			{
				materialTemplate.EnableKeyword("_EXTRUSIONMODE_PM");
				materialTemplate.DisableKeyword("_EXTRUSIONMODE_POM");
			}
		}
	}

	public void ApplyQualitySetting(Light light, LightShadows defaultValue)
	{
		LightShadows lightShadows = (this.ShadowLevel >= TheForestQualitySettings.ShadowLevels.Low) ? LightShadows.None : defaultValue;
		if (light.shadows != lightShadows)
		{
			light.shadows = lightShadows;
		}
	}

	public TheForestQualitySettings.DrawDistances DrawDistance = TheForestQualitySettings.DrawDistances.Medium;

	public TheForestQualitySettings.PresetLevels Preset = TheForestQualitySettings.PresetLevels.Medium;

	public TheForestQualitySettings.MotionBlurQuality MotionBlur = TheForestQualitySettings.MotionBlurQuality.High;

	public float GrassDistance = 50f;

	public TheForestQualitySettings.GrassModes GrassMode = TheForestQualitySettings.GrassModes.GPU;

	public float GrassDensity = 0.5f;

	public TheForestQualitySettings.SSAOTypes SSAOType;

	public TheForestQualitySettings.SSAOTechnique SSAO = TheForestQualitySettings.SSAOTechnique.High;

	public TheForestQualitySettings.SEBloomTechnique SEBloom;

	public TheForestQualitySettings.ChromaticAberration CA;

	public TheForestQualitySettings.FilmGrain Fg;

	public TheForestQualitySettings.Dof DofTech = TheForestQualitySettings.Dof.None;

	public TheForestQualitySettings.ScreenSpaceReflection screenSpaceReflection;

	public TheForestQualitySettings.VolumetricClouds volumetricClouds;

	public TheForestQualitySettings.SunshineOcclusionOn SunshineOcclusion;

	public TheForestQualitySettings.VolumetricsTypes VolumetricsType;

	public TheForestQualitySettings.SkyLightingOn SkyLighting = TheForestQualitySettings.SkyLightingOn.Off;

	private TheForestQualitySettings.PostEffectsSystems _postEffectSystem = TheForestQualitySettings.PostEffectsSystems.DX11;

	public TheForestQualitySettings.CausticsOn Caustics;

	public TheForestQualitySettings.AntiAliasingTechnique AntiAliasing = TheForestQualitySettings.AntiAliasingTechnique.TAA;

	public int LightmapResolution = 4096;

	public float LightDistance = 91f;

	public int LightmapUpdateIntervalFrames = 1;

	public TheForestQualitySettings.FarShadowModes FarShadowMode = TheForestQualitySettings.FarShadowModes.Off;

	public TheForestQualitySettings.TextureQualities TextureQuality;

	public TheForestQualitySettings.ShadowLevels ShadowLevel;

	[SerializeField]
	private TheForestQualitySettings.ShadowCascadeCount _cascadeCount;

	public SunshineRelativeResolutions ScatterResolution = SunshineRelativeResolutions.Half;

	public SunshineScatterSamplingQualities ScatterSamplingQuality = SunshineScatterSamplingQualities.High;

	public TheForestQualitySettings.TerrainQualities TerrainQuality;

	public TheForestQualitySettings.ReflexionModes ReflexionMode;

	public TheForestQualitySettings.OceanQualities OceanQuality;

	public TheForestQualitySettings.MaterialQualities MaterialQuality;

	private const string Prefix = "Quality_v016_";

	private static TheForestQualitySettings[] Presets;

	private static TheForestQualitySettings userSettings;

	public enum TerrainQualities
	{
		POM,
		PM,
		SIMPLE
	}

	public enum MaterialQualities
	{
		High,
		Medium,
		Low
	}

	public enum DrawDistances
	{
		Ultra,
		VeryHigh,
		High,
		Medium,
		Low,
		UltraLow
	}

	public enum SSAOTypes
	{
		AMPLIFY,
		UNITY
	}

	public enum SSAOTechnique
	{
		Ultra,
		High,
		Low,
		Off
	}

	public enum SEBloomTechnique
	{
		Normal,
		None
	}

	public enum ChromaticAberration
	{
		Normal,
		None
	}

	public enum FilmGrain
	{
		Normal,
		None
	}

	public enum ScreenSpaceReflection
	{
		On,
		Off
	}

	public enum VolumetricClouds
	{
		On,
		Off
	}

	public enum Dof
	{
		Normal,
		None
	}

	public enum GiOn
	{
		On,
		Off
	}

	public enum SkyLightingOn
	{
		On,
		Off
	}

	public enum CausticsOn
	{
		On,
		Off
	}

	public enum AntiAliasingTechnique
	{
		None,
		FXAA,
		TAA
	}

	public enum MotionBlurQuality
	{
		None,
		Low,
		Medium,
		High,
		Ultra
	}

	public enum FarShadowModes
	{
		On,
		Off
	}

	public enum ShadowLevels
	{
		VeryHigh,
		High,
		Medium,
		Low,
		Fastest,
		UltraLow
	}

	public enum ShadowCascadeCount
	{
		FourCascades,
		TwoCascades,
		NoCascades_low,
		NoCascades_vlow,
		NoCascades_fastest,
		NoCascades_laptop
	}

	public enum ReflexionModes
	{
		Complex,
		Simple,
		Off
	}

	public enum SunshineOcclusionOn
	{
		On,
		Off
	}

	public enum VolumetricsTypes
	{
		Hx,
		Sunshine
	}

	public enum PostEffectsSystems
	{
		Legacy,
		DX11
	}

	public enum OceanQualities
	{
		WaveDisplacementHigh,
		WaveDisplacementLow,
		Flat
	}

	public enum TextureQualities
	{
		FullRes,
		HalfRes,
		QuaterRes
	}

	public enum GrassModes
	{
		CPU,
		GPU
	}

	[Serializable]
	public enum PresetLevels
	{
		High,
		Medium,
		Low,
		Fastest,
		UltraLow,
		Custom,
		Ps4 = -1,
		DS = -2
	}
}
