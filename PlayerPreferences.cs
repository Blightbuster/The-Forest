using System;
using System.Collections.Generic;
using FMOD.Studio;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UniLinq;
using UnityEngine;
using UnityEngine.VR;


public class PlayerPreferences : MonoBehaviour
{
	
	
	
	public static bool CanUpdateFov
	{
		get
		{
			return PlayerPreferences._canUpdateFOV && !ForestVR.Enabled;
		}
		set
		{
			PlayerPreferences._canUpdateFOV = value;
		}
	}

	
	
	
	public static bool TreeRegrowthLocal { get; private set; }

	
	
	public static bool TreeRegrowth
	{
		get
		{
			return (!BoltNetwork.isClient) ? PlayerPreferences.TreeRegrowthLocal : PlayerPreferences.TreeRegrowthRemote;
		}
	}

	
	
	
	public static bool NoDestructionLocal { get; private set; }

	
	
	public static bool NoDestruction
	{
		get
		{
			return (!BoltNetwork.isClient) ? PlayerPreferences.NoDestructionLocal : PlayerPreferences.NoDestructionRemote;
		}
	}

	
	
	
	public static bool AllowEnemiesCreative { get; private set; }

	
	private void Awake()
	{
		PlayerPreferences.Instance = this;
		PlayerPreferences.Load();
		string operatingSystem = SystemInfo.operatingSystem;
		if (!operatingSystem.Contains("64bit"))
		{
			PlayerPreferences.is32bit = true;
			QualitySettings.masterTextureLimit = 1;
			PlayerPreferences.LowMemoryMode = true;
			if (this.TitleScene && this.Warning32Bits)
			{
				this.Warning32Bits.SetActive(true);
			}
		}
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs.Contains("-language"))
		{
			int num = commandLineArgs.IndexOf("-language");
			PlayerPrefs.SetString("Language", commandLineArgs[num + 1]);
		}
	}

	
	private void Start()
	{
		if (!SteamDSConfig.isDedicatedServer)
		{
			if (FMOD_StudioSystem.instance && FMOD_StudioSystem.instance.System != null)
			{
				UnityUtil.ERRCHECK(FMOD_StudioSystem.instance.System.getVCA("vca:/SFX", out this.SFXControl));
				UnityUtil.ERRCHECK(FMOD_StudioSystem.instance.System.getBus("bus:/Music", out this.MusicBus));
				this.GetExtraMusicBuses();
			}
			else
			{
				Debug.LogError("FMOD_StudioSystem.instance or FMOD_StudioSystem.instance.System is null, failed to initialize SFXControl & MusicBus");
			}
			UiTranslationDatabase.SetLanguage(PlayerPreferences.Language);
		}
	}

	
	private void GetExtraMusicBuses()
	{
		this.SecondaryMusicBuses = new Dictionary<string, Bus>();
		foreach (string text in this.SecondaryMusicBusPaths)
		{
			Bus value;
			if (!UnityUtil.ERRCHECK(FMOD_StudioSystem.instance.System.getBus(text, out value)))
			{
				Debug.LogError(string.Format("Failed to find secondary music bus at path \"{0}\"", text));
			}
			else
			{
				this.SecondaryMusicBuses.Add(text, value);
			}
		}
	}

	
	private void OnLevelWasLoaded(int level)
	{
		if (level == 2)
		{
			TheForestQualitySettings.UserSettings.SetTerrainQuality(TheForestQualitySettings.UserSettings.TerrainQuality);
			PlayerPreferences.ApplyValues();
		}
	}

	
	private void OnDestroy()
	{
		if (this.SFXControl != null)
		{
			UnityUtil.ERRCHECK(this.SFXControl.setFaderLevel(Mathf.Max(PlayerPreferences.Volume, 0.25f)));
		}
		if (this.MusicBus != null)
		{
			UnityUtil.ERRCHECK(this.MusicBus.setFaderLevel(Mathf.Max(PlayerPreferences.MusicVolume, 0.5f)));
		}
		if (this.SecondaryMusicBuses != null)
		{
			foreach (KeyValuePair<string, Bus> keyValuePair in this.SecondaryMusicBuses)
			{
				if (!(keyValuePair.Value == null))
				{
					UnityUtil.ERRCHECK(keyValuePair.Value.setFaderLevel(Mathf.Max(PlayerPreferences.MusicVolume, 0.5f)));
				}
			}
		}
		if (PlayerPreferences.Instance == this)
		{
			PlayerPreferences.Instance = this;
		}
	}

	
	public static void ApplyMaxFrameRate()
	{
		Application.targetFrameRate = PlayerPreferences.MaxFrameRate;
	}

	
	public static void SetLowQualityPhysics(bool lowQualityPhysics)
	{
		PlayerPreferences.LowQualityPhysics = lowQualityPhysics;
		Time.fixedDeltaTime = ((!PlayerPreferences.LowQualityPhysics) ? 0.0166666675f : 0.0333333351f);
	}

	
	public static void Load()
	{
		Debug.Log("PlayerPreferences.Load");
		if (PlayerPreferences.alreadyLoaded)
		{
			return;
		}
		PlayerPreferences.Preset = ((!CoopPeerStarter.DedicatedHost) ? PlayerPrefs.GetInt("Preset_v16", PlayerPreferences.Preset) : 5);
		PlayerPreferences.LowMemoryMode = (PlayerPrefs.GetInt("LowMemoryMode", 0) > 0);
		PlayerPreferences.MemorySafeSaveMode = (PlayerPrefs.GetInt("MemorySafeSaveMode", 0) > 0);
		PlayerPreferences.GammaWorldAndDay = PlayerPrefs.GetFloat("GammaWorldAndDay", 2f);
		PlayerPreferences.GammaCavesAndNight = PlayerPrefs.GetFloat("GammaCavesAndNight", 2f);
		PlayerPreferences.Brightness = PlayerPrefs.GetFloat("Brightness", 0.5f);
		PlayerPreferences.Volume = PlayerPrefs.GetFloat("Volume", 0.5f);
		PlayerPreferences.MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
		PlayerPreferences.MicrophoneVolume = PlayerPrefs.GetFloat("MicrophoneVolume", 5f);
		PlayerPreferences.VoiceCount = PlayerPrefs.GetInt("VoiceCount", 128);
		PlayerPreferences.MouseInvert = (PlayerPrefs.GetInt("MouseInvert", 0) > 0);
		PlayerPreferences.MouseSensitivityX = PlayerPrefs.GetFloat("MouseSensitivity", 0.5f);
		PlayerPreferences.MouseSensitivityY = PlayerPrefs.GetFloat("MouseSensitivityY", PlayerPrefs.GetFloat("MouseSensitivity", 0.5f));
		PlayerPreferences.MouseSmoothing = PlayerPrefs.GetFloat("MouseSmoothing2", 0.1f);
		PlayerPreferences.Fov = PlayerPrefs.GetFloat("Fov", 75f);
		PlayerPreferences.MaxFrameRate = PlayerPrefs.GetInt("MaxFrameRate2", -1);
		PlayerPreferences.ApplyMaxFrameRate();
		PlayerPreferences.SetLowQualityPhysics(PlayerPrefs.GetInt("LowQualityPhysics", 0) > 0);
		PlayerPreferences.Language = PlayerPrefs.GetString("Language", "English");
		PlayerPreferences.SetGhostTint(PlayerPrefs.GetInt("GhostTint", 0), PlayerPrefs.GetFloat("GhostTintOpacity", 0.0784f));
		PlayerPreferences.ColorGrading = PlayerPrefs.GetInt("ColorGrading", 0);
		PlayerPreferences.VSync = (PlayerPrefs.GetInt("VSync", QualitySettings.vSyncCount) > 0);
		PlayerPreferences.ShowHud = (PlayerPrefs.GetInt("ShowHud", 1) > 0);
		PlayerPreferences.ShowOverlayIcons = (PlayerPrefs.GetInt("ShowOverlayIcons", 1) > 0);
		PlayerPreferences.OverlayIconsGrouping = (PlayerPrefs.GetInt("OverlayIconsGroupingV2", 1) > 0);
		PlayerPreferences.ShowProjectileReticle = (PlayerPrefs.GetInt("ShowProjectileReticle", 1) > 0);
		PlayerPreferences.UseXInput = (PlayerPrefs.GetInt("UseXInputV2", 1) > 0);
		PlayerPreferences.ShowPlayerNamesMP = (PlayerPrefs.GetInt("ShowPlayerNamesMP", 1) > 0);
		PlayerPreferences.ShowStealthMeter = (PlayerPrefs.GetInt("ShowStealthMeter", 1) > 0);
		PlayerPreferences.UseCrouchToggle = (PlayerPrefs.GetInt("UseCrouchToggle", 0) > 0);
		PlayerPreferences.UseSprintToggle = (PlayerPrefs.GetInt("UseSprintToggle", 0) > 0);
		PlayerPreferences.UseGamepadRumble = (PlayerPrefs.GetInt("UseGamepadRumble", 1) > 0);
		if (!CoopPeerStarter.DedicatedHost)
		{
			PlayerPreferences.TreeRegrowthLocal = (PlayerPrefs.GetInt("TreeRegrowth", 0) > 0);
			PlayerPreferences.NoDestructionLocal = (PlayerPrefs.GetInt("NoDestruction", 0) > 0);
			PlayerPreferences.AllowEnemiesCreative = (PlayerPrefs.GetInt("AllowEnemiesCreative", 0) > 0);
		}
		PlayerPreferences.ExWallAutofill = (PlayerPrefs.GetInt("ExWallAutofill", 1) > 0);
		PlayerPreferences.ExFloorsAutofill = (PlayerPrefs.GetInt("ExFloorsAutofill", 1) > 0);
		if (!TheForestQualitySettings.Load())
		{
			TheForestQualitySettings.CopyPreset(-1);
		}
		QualitySettings.SetQualityLevel((int)TheForestQualitySettings.UserSettings.ShadowLevel);
		if (!TheForestQualitySettings.Load())
		{
			TheForestQualitySettings.CopyPreset(-1);
		}
		PlayerPreferences.ApplyValues();
		PlayerPreferences.alreadyLoaded = true;
	}

	
	public static void Save()
	{
		if (PlayerPreferences.PreventSaving)
		{
			return;
		}
		Debug.Log("Saving Preferences");
		PlayerPrefs.SetInt("Preset_v16", PlayerPreferences.Preset);
		PlayerPrefs.SetInt("LowMemoryMode", (!PlayerPreferences.LowMemoryMode) ? 0 : 1);
		PlayerPrefs.SetInt("MemorySafeSaveMode", (!PlayerPreferences.MemorySafeSaveMode) ? 0 : 1);
		PlayerPrefs.SetFloat("Brightness", PlayerPreferences.Brightness);
		PlayerPrefs.SetFloat("GammaCavesAndNight", PlayerPreferences.GammaCavesAndNight);
		PlayerPrefs.SetFloat("GammaWorldAndDay", PlayerPreferences.GammaWorldAndDay);
		PlayerPrefs.SetFloat("Volume", PlayerPreferences.Volume);
		PlayerPrefs.SetFloat("MusicVolume", PlayerPreferences.MusicVolume);
		PlayerPrefs.SetFloat("MicrophoneVolume", PlayerPreferences.MicrophoneVolume);
		PlayerPrefs.SetInt("VoiceCount", PlayerPreferences.VoiceCount);
		PlayerPrefs.SetInt("MouseInvert", (!PlayerPreferences.MouseInvert) ? 0 : 1);
		PlayerPrefs.SetFloat("MouseSensitivity", PlayerPreferences.MouseSensitivityX);
		PlayerPrefs.SetFloat("MouseSensitivityY", PlayerPreferences.MouseSensitivityY);
		PlayerPrefs.SetFloat("MouseSmoothing2", PlayerPreferences.MouseSmoothing);
		PlayerPrefs.SetFloat("Fov", PlayerPreferences.Fov);
		PlayerPrefs.SetInt("GhostTint", PlayerPreferences.GhostTintNum);
		PlayerPrefs.SetFloat("GhostTintOpacity", PlayerPreferences.GhostTintOpacity);
		PlayerPrefs.SetInt("ColorGrading", PlayerPreferences.ColorGrading);
		TheForestQualitySettings.Save();
		PlayerPrefs.SetInt("VSync", (!PlayerPreferences.VSync) ? 0 : 1);
		PlayerPrefs.SetInt("ShowHud", (!PlayerPreferences.ShowHud) ? 0 : 1);
		PlayerPrefs.SetInt("ShowOverlayIcons", (!PlayerPreferences.ShowOverlayIcons) ? 0 : 1);
		PlayerPrefs.SetInt("OverlayIconsGroupingV2", (!PlayerPreferences.OverlayIconsGrouping) ? 0 : 1);
		PlayerPrefs.SetInt("ShowProjectileReticle", (!PlayerPreferences.ShowProjectileReticle) ? 0 : 1);
		PlayerPrefs.SetInt("UseXInputV2", (!PlayerPreferences.UseXInput) ? 0 : 1);
		PlayerPrefs.SetInt("ShowPlayerNamesMP", (!PlayerPreferences.ShowPlayerNamesMP) ? 0 : 1);
		PlayerPrefs.SetInt("ShowStealthMeter", (!PlayerPreferences.ShowStealthMeter) ? 0 : 1);
		PlayerPrefs.SetInt("MaxFrameRate2", PlayerPreferences.MaxFrameRate);
		PlayerPrefs.SetInt("LowQualityPhysics", (!PlayerPreferences.LowQualityPhysics) ? 0 : 1);
		PlayerPrefs.SetString("Language", PlayerPreferences.Language);
		PlayerPrefs.SetInt("UseCrouchToggle", (!PlayerPreferences.UseCrouchToggle) ? 0 : 1);
		PlayerPrefs.SetInt("UseSprintToggle", (!PlayerPreferences.UseSprintToggle) ? 0 : 1);
		PlayerPrefs.SetInt("UseGamepadRumble", (!PlayerPreferences.UseGamepadRumble) ? 0 : 1);
		PlayerPrefs.SetInt("TreeRegrowth", (!PlayerPreferences.TreeRegrowthLocal) ? 0 : 1);
		PlayerPrefs.SetInt("NoDestruction", (!PlayerPreferences.NoDestructionLocal) ? 0 : 1);
		PlayerPrefs.SetInt("AllowEnemiesCreative", (!PlayerPreferences.AllowEnemiesCreative) ? 0 : 1);
		PlayerPrefs.Save();
	}

	
	public static void ApplyValues()
	{
		Terrain terrain = Terrain.activeTerrain;
		if (terrain)
		{
			if (terrain.detailObjectDistance != TheForestQualitySettings.UserSettings.GrassDistance)
			{
				terrain.detailObjectDistance = TheForestQualitySettings.UserSettings.GrassDistance;
			}
			if (terrain.detailObjectDensity != TheForestQualitySettings.UserSettings.GrassDensity)
			{
				terrain.detailObjectDensity = TheForestQualitySettings.UserSettings.GrassDensity;
			}
		}
		if (Scene.HudGui && Scene.HudGui.GuiCamC)
		{
			Scene.HudGui.CheckHudState();
		}
	}

	
	private void Update()
	{
		Shader.globalMaximumLOD = TheForestQualitySettings.UserSettings.MaterialQualityShaderLOD;
		if (this.activeTerrain)
		{
			this.activeTerrain.heightmapPixelError = TheForestQualitySettings.UserSettings.TerrainQualityPixelErrorPercentage;
		}
		if (AudioListener.volume != PlayerPreferences.Volume)
		{
			AudioListener.volume = PlayerPreferences.Volume;
		}
		if (this.SFXControl != null)
		{
			UnityUtil.ERRCHECK(this.SFXControl.setFaderLevel(PlayerPreferences.Volume));
		}
		if (this.MusicBus != null)
		{
			UnityUtil.ERRCHECK(this.MusicBus.setFaderLevel(PlayerPreferences.MusicVolume));
		}
		if (this.SecondaryMusicBuses != null)
		{
			foreach (KeyValuePair<string, Bus> keyValuePair in this.SecondaryMusicBuses)
			{
				if (!(keyValuePair.Value == null))
				{
					UnityUtil.ERRCHECK(keyValuePair.Value.setFaderLevel(PlayerPreferences.MusicVolume));
				}
			}
		}
		if (!this.TitleScene && PlayerPreferences.CanUpdateFov && LocalPlayer.MainCam && LocalPlayer.MainCam.fieldOfView != PlayerPreferences.Fov && !VRDevice.isPresent)
		{
			LocalPlayer.MainCam.fieldOfView = PlayerPreferences.Fov;
		}
		int num = (!PlayerPreferences.VSync) ? 0 : 1;
		if (QualitySettings.vSyncCount != num)
		{
			QualitySettings.vSyncCount = num;
		}
	}

	
	public static void SetLocalTreeRegrowth(bool onoff)
	{
		PlayerPreferences.TreeRegrowthLocal = onoff;
		EventRegistry.Game.Publish(TfEvent.RegrowModeSet, onoff);
	}

	
	public static void SetLocalNoDestructionMode(bool onoff)
	{
		PlayerPreferences.NoDestructionLocal = onoff;
		EventRegistry.Game.Publish(TfEvent.NoDestrutionModeSet, onoff);
	}

	
	public static void SetLocalAllowEnemiesCreativeMode(bool onoff)
	{
		PlayerPreferences.AllowEnemiesCreative = onoff;
		EventRegistry.Game.Publish(TfEvent.AllowEnemiesSet, onoff);
	}

	
	public static void SetGhostTint(int num, float opacity)
	{
		PlayerPreferences.GhostTintNum = Mathf.Max(num, 0);
		PlayerPreferences.GhostTintOpacity = Mathf.Clamp(opacity, 0.0784f, 0.75f);
		if (PlayerPreferences.Instance && PlayerPreferences.Instance.Prefabs)
		{
			if (PlayerPreferences.GhostTintNum >= PlayerPreferences.Instance.Prefabs.GhostTints.Length)
			{
				PlayerPreferences.GhostTintNum = 0;
			}
			Color value = PlayerPreferences.Instance.Prefabs.GhostTints[PlayerPreferences.GhostTintNum];
			PlayerPreferences.Instance.Prefabs.GetGhostClearColorMenu().SetColor("_TintColor", value);
			value.a = opacity;
			PlayerPreferences.Instance.Prefabs.GetGhostClearAlphaMenu().SetColor("_TintColor", value);
			PlayerPreferences.Instance.Prefabs.GetGhostClear().SetColor("_TintColor", value);
			PlayerPreferences.Instance.Prefabs.GetGhostClearGround().SetColor("_TintColor", value);
		}
		else if (Prefabs.Instance)
		{
			if (PlayerPreferences.GhostTintNum >= Prefabs.Instance.GhostTints.Length)
			{
				PlayerPreferences.GhostTintNum = 0;
			}
			Color value2 = Prefabs.Instance.GhostTints[PlayerPreferences.GhostTintNum];
			Prefabs.Instance.GetGhostClearColorMenu().SetColor("_TintColor", value2);
			value2.a = opacity;
			Prefabs.Instance.GetGhostClearAlphaMenu().SetColor("_TintColor", value2);
			Prefabs.Instance.GetGhostClear().SetColor("_TintColor", value2);
			Prefabs.Instance.GetGhostClearGround().SetColor("_TintColor", value2);
		}
	}

	
	private const bool RespectUnityDialogSettings = false;

	
	private const string CustomQualityPath = "/TheForestQualitySettings.dat";

	
	public bool TitleScene;

	
	public GameObject Warning32Bits;

	
	public Prefabs Prefabs;

	
	private static PlayerPreferences Instance;

	
	public static int Preset = 1;

	
	public static float Brightness = 0.5f;

	
	public static float GammaCavesAndNight = 2f;

	
	public static float GammaWorldAndDay = 2f;

	
	public const float MinGamma = 2f;

	
	public const float MaxGamma = 2.2f;

	
	public static float Volume = 0.5f;

	
	public static float MusicVolume = 1f;

	
	public static float MicrophoneVolume = 1f;

	
	public const float MicrophoneVolDef = 5f;

	
	public const float MicrophoneVolMin = 0f;

	
	public const float MicrophoneVolMax = 10f;

	
	public static int VoiceCount = 128;

	
	public static bool MouseInvert;

	
	public static float MouseSensitivityX = 0.5f;

	
	public static float MouseSensitivityY = 0.5f;

	
	public static float MouseSmoothing = 0.1f;

	
	public static float Fov = 75f;

	
	private static bool _canUpdateFOV = true;

	
	public static int MaxFrameRate = -1;

	
	public static bool LowQualityPhysics;

	
	public static string Language = "English";

	
	public static int GhostTintNum;

	
	public static float GhostTintOpacity = 0.0784f;

	
	public const float GhostTintOpacityDef = 0.0784f;

	
	public const float GhostTintOpacityMin = 0.0784f;

	
	public const float GhostTintOpacityMax = 0.75f;

	
	public static float BuildingSnapAngle = 45f;

	
	public static float BuildingSnapRange = 2f;

	
	public static int ColorGrading;

	
	public static bool VSync;

	
	public static bool ShowHud = true;

	
	public static bool ShowOverlayIcons = true;

	
	public static bool OverlayIconsGrouping;

	
	public static bool ShowProjectileReticle = true;

	
	public static bool UseXInput = true;

	
	public static bool LowMemoryMode;

	
	public static bool MemorySafeSaveMode = true;

	
	public static bool ShowPlayerNamesMP = true;

	
	public static bool ShowStealthMeter = true;

	
	public static bool UseCrouchToggle;

	
	public static bool UseSprintToggle;

	
	public static bool UseGamepadCursorSnapping;

	
	public static bool UseGamepadRumble = true;

	
	public static bool TreeRegrowthRemote;

	
	public static bool NoDestructionRemote;

	
	public static bool ExWallAutofill;

	
	public static bool ExFloorsAutofill;

	
	public static bool PreventSaving;

	
	public static bool is32bit;

	
	public Terrain activeTerrain;

	
	public RTP_LODmanager terrainLOD;

	
	private static bool alreadyLoaded;

	
	private VCA SFXControl;

	
	private Bus MusicBus;

	
	private Dictionary<string, Bus> SecondaryMusicBuses;

	
	private string[] SecondaryMusicBusPaths = new string[]
	{
		"bus:/main_menu/menu"
	};
}
