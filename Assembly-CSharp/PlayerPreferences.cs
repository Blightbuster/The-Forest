using System;
using System.Collections.Generic;
using System.Text;
using FMOD;
using FMOD.Studio;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using TheForest.World;
using UniLinq;
using UnityEngine;
using UnityEngine.VR;

public class PlayerPreferences : MonoBehaviour
{
	public static bool VRUseCameraDirectedForwardMovement
	{
		get
		{
			return PlayerPreferences.VRForwardMovement == PlayerPreferences.VRForwardDirectionTypes.CAMERA;
		}
	}

	public static bool VRUseControllerDirectedForwardMovement
	{
		get
		{
			return PlayerPreferences.VRForwardMovement == PlayerPreferences.VRForwardDirectionTypes.CONTROLLER;
		}
	}

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

	public static bool CheatsAllowed { get; private set; }

	public static bool IsBelowDX11
	{
		get
		{
			bool? isBelowDX = PlayerPreferences._isBelowDX11;
			if (isBelowDX == null)
			{
				PlayerPreferences._isBelowDX11 = new bool?(SystemInfo.graphicsShaderLevel < 50 || !SystemInfo.supportsComputeShaders);
			}
			bool? isBelowDX2 = PlayerPreferences._isBelowDX11;
			return isBelowDX2.Value;
		}
	}

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
				UnityEngine.Debug.LogError("FMOD_StudioSystem.instance or FMOD_StudioSystem.instance.System is null, failed to initialize SFXControl & MusicBus");
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
				UnityEngine.Debug.LogError(string.Format("Failed to find secondary music bus at path \"{0}\"", text));
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
		UnityEngine.Debug.Log("PlayerPreferences.Load");
		if (PlayerPreferences.alreadyLoaded)
		{
			return;
		}
		PlayerPreferences.Preset = PlayerPreferences.GetPreset();
		PlayerPreferences.LowMemoryMode = (PlayerPrefs.GetInt("LowMemoryMode", 0) > 0);
		PlayerPreferences.MemorySafeSaveMode = (PlayerPrefs.GetInt("MemorySafeSaveMode", 0) > 0);
		PlayerPreferences.GammaWorldAndDay = PlayerPrefs.GetFloat("GammaWorldAndDay", 2f);
		PlayerPreferences.GammaCavesAndNight = PlayerPrefs.GetFloat("GammaCavesAndNight", 2f);
		PlayerPreferences.Contrast = PlayerPrefs.GetFloat("Contrast", 1f);
		if (ForestVR.Enabled)
		{
			PlayerPreferences.SetAudioDriver(PlayerPrefs.GetString((!ForestVR.Enabled) ? "AudioDriver" : "AudioDriverVR", string.Empty), -1);
		}
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
			PlayerPreferences.CheatsAllowed = (PlayerPrefs.GetInt("CheatsAllowed", 0) > 0);
		}
		PlayerPreferences.ExWallAutofill = (PlayerPrefs.GetInt("ExWallAutofill", 1) > 0);
		PlayerPreferences.ExFloorsAutofill = (PlayerPrefs.GetInt("ExFloorsAutofill", 1) > 0);
		PlayerPreferences.VRTurnSnap = PlayerPrefs.GetInt("VRTurnSnap", PlayerPreferences.VRTurnSnap);
		PlayerPreferences.VRMoveDarkening = (PlayerPreferences.VRMoveDarkeningTypes)PlayerPrefs.GetInt("VRMoveDarkening", (int)PlayerPreferences.VRMoveDarkening);
		PlayerPreferences.VRAntiAliasing = (PlayerPreferences.VRAntiAliasingTypes)PlayerPrefs.GetInt("VRAntiAliasing", (int)PlayerPreferences.VRAntiAliasing);
		PlayerPreferences.VRUsePhysicalCrouching = (PlayerPrefs.GetInt("VRUsePhysicalCrouching", (!PlayerPreferences.VRUsePhysicalCrouching) ? 0 : 1) > 0);
		PlayerPreferences.VRForwardMovement = (PlayerPreferences.VRForwardDirectionTypes)PlayerPrefs.GetInt("VRForwardMovement", (int)PlayerPreferences.VRForwardMovement);
		PlayerPreferences.VRAutoRun = (PlayerPrefs.GetInt("VRAutoRun", (!PlayerPreferences.VRAutoRun) ? 0 : 1) > 0);
		PlayerPreferences.VRUseRightHandedBow = (PlayerPrefs.GetInt("VRUseRightHandedBow", (!PlayerPreferences.VRUseRightHandedBow) ? 0 : 1) > 0);
		PlayerPreferences.VRUseRightHandedWeapon = (PlayerPrefs.GetInt("VRUseRightHandedWeapon", (!PlayerPreferences.VRUseRightHandedWeapon) ? 0 : 1) > 0);
		if (!TheForestQualitySettings.Load())
		{
			TheForestQualitySettings.CopyPreset(-1);
		}
		TheForestQualitySettings.SetUnityQualityFromShadowLevel(TheForestQualitySettings.UserSettings.ShadowLevel);
		if (!TheForestQualitySettings.Load())
		{
			TheForestQualitySettings.CopyPreset(-1);
		}
		PlayerPreferences.ApplyValues();
		PlayerPreferences.alreadyLoaded = true;
	}

	private static int GetPreset()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			return 5;
		}
		int @int = PlayerPrefs.GetInt("Preset_v16", PlayerPreferences.Preset);
		if (@int == 5 && !TheForestQualitySettings.HasSavedSettings())
		{
			return PlayerPreferences.Preset;
		}
		return @int;
	}

	public static void Save()
	{
		if (PlayerPreferences.PreventSaving)
		{
			return;
		}
		UnityEngine.Debug.Log("Saving Preferences");
		PlayerPrefs.SetInt("Preset_v16", PlayerPreferences.Preset);
		PlayerPrefs.SetInt("LowMemoryMode", (!PlayerPreferences.LowMemoryMode) ? 0 : 1);
		PlayerPrefs.SetInt("MemorySafeSaveMode", (!PlayerPreferences.MemorySafeSaveMode) ? 0 : 1);
		PlayerPrefs.SetFloat("GammaCavesAndNight", PlayerPreferences.GammaCavesAndNight);
		PlayerPrefs.SetFloat("GammaWorldAndDay", PlayerPreferences.GammaWorldAndDay);
		PlayerPrefs.SetFloat("Contrast", PlayerPreferences.Contrast);
		if (ForestVR.Enabled)
		{
			PlayerPrefs.SetString((!ForestVR.Enabled) ? "AudioDriver" : "AudioDriverVR", PlayerPreferences.AudioDriver);
		}
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
		PlayerPrefs.SetInt("CheatsAllowed", (!PlayerPreferences.CheatsAllowed) ? 0 : 1);
		PlayerPrefs.SetInt("AllowEnemiesCreative", (!PlayerPreferences.AllowEnemiesCreative) ? 0 : 1);
		PlayerPrefs.SetInt("VRTurnSnap", PlayerPreferences.VRTurnSnap);
		PlayerPrefs.SetInt("VRMoveDarkening", (int)PlayerPreferences.VRMoveDarkening);
		PlayerPrefs.SetInt("VRAntiAliasing", (int)PlayerPreferences.VRAntiAliasing);
		PlayerPrefs.SetInt("VRUsePhysicalCrouching", (!PlayerPreferences.VRUsePhysicalCrouching) ? 0 : 1);
		PlayerPrefs.SetInt("VRForwardMovement", (int)PlayerPreferences.VRForwardMovement);
		PlayerPrefs.SetInt("VRAutoRun", (!PlayerPreferences.VRAutoRun) ? 0 : 1);
		PlayerPrefs.SetInt("VRUseRightHandedBow", (!PlayerPreferences.VRUseRightHandedBow) ? 0 : 1);
		PlayerPrefs.SetInt("VRUseRightHandedWeapon", (!PlayerPreferences.VRUseRightHandedWeapon) ? 0 : 1);
		PlayerPrefs.Save();
	}

	public static void ApplyValues()
	{
		GrassModeManager instance = GrassModeManager.GetInstance();
		if (instance != null)
		{
			instance.RefreshSettings();
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
			if (ForestVR.Enabled)
			{
				this.activeTerrain.heightmapPixelError = 200f;
			}
			else
			{
				this.activeTerrain.heightmapPixelError = TheForestQualitySettings.UserSettings.TerrainQualityPixelErrorPercentage;
			}
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

	public static void SetAudioDriver(string audioDriver, int driverNum = -1)
	{
		FMOD.System system = null;
		FMOD_StudioSystem.instance.System.getLowLevelSystem(out system);
		int num;
		system.getDriver(out num);
		if (driverNum != -1)
		{
			if (num != driverNum)
			{
				int num2 = 0;
				foreach (string audioDriver2 in PlayerPreferences.GetAudioDrivers())
				{
					if (num2 == driverNum)
					{
						PlayerPreferences.AudioDriver = audioDriver2;
						PlayerPreferences.AudioDriverNum = num2;
						system.setDriver(num2);
						return;
					}
					num2++;
				}
			}
		}
		else if (!string.IsNullOrEmpty(audioDriver) && PlayerPreferences.AudioDriver != audioDriver)
		{
			int num3 = 0;
			foreach (string a in PlayerPreferences.GetAudioDrivers())
			{
				if (a == audioDriver)
				{
					PlayerPreferences.AudioDriver = audioDriver;
					PlayerPreferences.AudioDriverNum = num3;
					system.setDriver(num3);
					return;
				}
				num3++;
			}
		}
		if (string.IsNullOrEmpty(audioDriver) && driverNum == -1)
		{
			if (ForestVR.Enabled)
			{
				int num4 = 0;
				foreach (string text in PlayerPreferences.GetAudioDrivers())
				{
					if (text.ToLower().Contains("rift"))
					{
						system.setDriver(num4);
						PlayerPreferences.AudioDriver = text;
						PlayerPreferences.AudioDriverNum = num4;
						UnityEngine.Debug.Log("[VR] Setting " + text + " as audio driver");
						break;
					}
					if (text.ToLower().Contains("htc vive"))
					{
						system.setDriver(num4);
						PlayerPreferences.AudioDriver = text;
						PlayerPreferences.AudioDriverNum = num4;
						UnityEngine.Debug.Log("[VR] Setting " + text + " as audio driver");
						break;
					}
					num4++;
				}
			}
			else
			{
				string audioDriver3 = PlayerPreferences.GetAudioDrivers().Skip(num).First<string>();
				PlayerPreferences.AudioDriver = audioDriver3;
				PlayerPreferences.AudioDriverNum = num;
			}
		}
	}

	public static IEnumerable<string> GetAudioDrivers()
	{
		FMOD.System sys = null;
		FMOD_StudioSystem.instance.System.getLowLevelSystem(out sys);
		int numDrivers;
		sys.getNumDrivers(out numDrivers);
		StringBuilder name = new StringBuilder(200);
		for (int i = 0; i < numDrivers; i++)
		{
			Guid guid;
			int systemRate;
			SPEAKERMODE speakerMode;
			int speakerModesChannel;
			sys.getDriverInfo(i, name, 200, out guid, out systemRate, out speakerMode, out speakerModesChannel);
			yield return name.ToString();
			name.Length = 0;
			name.Capacity = 1;
			name.Capacity = 200;
		}
		yield break;
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

	public static void SetAllowCheatsMode(bool onoff)
	{
		PlayerPreferences.CheatsAllowed = onoff;
		EventRegistry.Game.Publish(TfEvent.CheatAllowedSet, onoff);
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
			if (ForestVR.Enabled)
			{
				PlayerPreferences.Instance.Prefabs.GetGhostClear().SetFloat("_SheenIntensity", 0f);
				PlayerPreferences.Instance.Prefabs.GetGhostClearGround().SetFloat("_SheenIntensity", 0f);
			}
			else
			{
				PlayerPreferences.Instance.Prefabs.GetGhostClear().SetFloat("_SheenIntensity", 0.05f);
				PlayerPreferences.Instance.Prefabs.GetGhostClearGround().SetFloat("_SheenIntensity", 0.05f);
			}
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
			if (ForestVR.Enabled)
			{
				Prefabs.Instance.GetGhostClear().SetFloat("_SheenIntensity", 0f);
				Prefabs.Instance.GetGhostClearGround().SetFloat("_SheenIntensity", 0f);
			}
			else
			{
				Prefabs.Instance.GetGhostClear().SetFloat("_SheenIntensity", 0.05f);
				Prefabs.Instance.GetGhostClearGround().SetFloat("_SheenIntensity", 0.05f);
			}
		}
	}

	private const bool RespectUnityDialogSettings = false;

	private const string CustomQualityPath = "/TheForestQualitySettings.dat";

	public bool TitleScene;

	public GameObject Warning32Bits;

	public Prefabs Prefabs;

	private static PlayerPreferences Instance;

	public static int Preset = 1;

	public static float GammaCavesAndNight = 2f;

	public static float GammaWorldAndDay = 2f;

	public const float GammaMin = 1.5f;

	public const float GammaMax = 2.3f;

	public const float GammaDefault = 2f;

	public const float GammaCavesMin = 2f;

	public const float GammaCavesMax = 2.5f;

	public const float GammaCavesDefault = 2f;

	public static float Contrast = 1f;

	public const float ContrastMin = 0.9f;

	public const float ContrastMax = 1.15f;

	public const float ContrastDefault = 1f;

	public static string AudioDriver = string.Empty;

	public static int AudioDriverNum = -1;

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

	public static bool AllowAsync = true;

	public static int VRTurnSnap = 30;

	public static PlayerPreferences.VRMoveDarkeningTypes VRMoveDarkening = PlayerPreferences.VRMoveDarkeningTypes.MEDIUM;

	public static PlayerPreferences.VRAntiAliasingTypes VRAntiAliasing = PlayerPreferences.VRAntiAliasingTypes.FXAA;

	public static PlayerPreferences.VRForwardDirectionTypes VRForwardMovement;

	public static bool VRUsePhysicalCrouching;

	public static bool VRAutoRun;

	public static bool VRUseRightHandedBow = true;

	public static bool VRUseRightHandedWeapon = true;

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

	private static bool? _isBelowDX11;

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

	public enum VRMoveDarkeningTypes
	{
		OFF,
		LOW,
		MEDIUM,
		HIGH
	}

	public enum VRAntiAliasingTypes
	{
		OFF,
		FXAA,
		SMAA
	}

	public enum VRForwardDirectionTypes
	{
		CAMERA,
		CONTROLLER,
		PLAYER
	}
}
