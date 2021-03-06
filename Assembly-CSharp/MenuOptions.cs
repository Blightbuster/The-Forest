﻿using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.UI;
using TheForest.Utils;
using UniLinq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.ImageEffects;

public class MenuOptions : MonoBehaviour
{
	private void BeginIgnoreEvents()
	{
		this.ignoreCounter++;
	}

	private void EndIgnoreEvents()
	{
		this.ignoreCounter--;
	}

	private bool IgnoreEvents
	{
		get
		{
			return this.ignoreCounter > 0;
		}
	}

	private void OnDisable()
	{
		if (this.shouldSave)
		{
			this.Save();
		}
		if (LoadAsync.Scenery)
		{
			Blur blur = (!LoadAsync.Scenery) ? null : LoadAsync.Scenery.GetComponentInChildren<Blur>();
			if (blur)
			{
				blur.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		this.shouldSave = true;
		this.ShowMenuWidget(GameSetup.IsCreativeGame, this.AllowEnemies, true);
		this.RefreshOptionalWidgets();
		if (LoadAsync.Scenery)
		{
			Blur blur = (!LoadAsync.Scenery) ? null : LoadAsync.Scenery.GetComponentInChildren<Blur>();
			if (blur)
			{
				blur.enabled = true;
			}
		}
		if (ForestVR.Enabled)
		{
			this.AudioDrivers.items.Clear();
			int num = 0;
			foreach (string rawAudioDriverName in PlayerPreferences.GetAudioDrivers())
			{
				string displayName = this.ProcessAudioDriverName(rawAudioDriverName);
				int num2 = this.AudioDrivers.items.Count((string ad) => ad.StartsWith(displayName));
				if (num2 > 0)
				{
					displayName = displayName + " " + num2;
				}
				this.AudioDrivers.AddItem(displayName, num++);
			}
		}
	}

	private string ProcessAudioDriverName(string rawAudioDriverName)
	{
		try
		{
			if (rawAudioDriverName.Contains('('))
			{
				int num = rawAudioDriverName.IndexOf('(') + 1;
				int num2 = rawAudioDriverName.IndexOf(')');
				int num3 = rawAudioDriverName.LastIndexOf(' ', num2);
				if (num3 > num && num2 - num > 23 && num3 - num > 12)
				{
					num2 = num3;
				}
				return rawAudioDriverName.Substring(num, Mathf.Clamp(num2 - num, 0, 23));
			}
		}
		catch
		{
		}
		return rawAudioDriverName;
	}

	private void RefreshOptionalWidgets()
	{
		this.ShowMenuWidget(!PlayerPreferences.IsBelowDX11, this.PostEffectsSystem, true);
		bool flag = TheForestQualitySettings.UserSettings.PostEffectsSystem == TheForestQualitySettings.PostEffectsSystems.Legacy;
		this.ShowMenuWidget(!flag, this.GammaWorldAndDay, true);
		this.ShowMenuWidget(!flag, this.GammaCavesAndNight, true);
		this.ShowMenuWidget(!flag, this.Contrast, true);
		this.ShowMenuWidget(ForestVR.Enabled, this.TurnSnap, true);
		this.ShowMenuWidget(ForestVR.Enabled, this.MoveDarkening, true);
		this.ShowMenuWidget(ForestVR.Enabled, this.VRAntiAliasing, true);
		bool showValue = false;
		this.ShowMenuWidget(showValue, this.GrassMode, true);
	}

	private void ShowMenuWidget(bool showValue, UIWidgetContainer widgetTarget, bool reconnectUiKeyNav)
	{
		if (widgetTarget == null)
		{
			return;
		}
		if (widgetTarget.transform.parent.gameObject.activeSelf == showValue)
		{
			return;
		}
		widgetTarget.transform.parent.gameObject.SetActive(showValue);
		if (!reconnectUiKeyNav)
		{
			return;
		}
		MenuOptions.ReconnectMenuButton(widgetTarget, !showValue);
	}

	private static void ReconnectMenuButton(UIWidgetContainer buttonTarget, bool breakConnection)
	{
		UIKeyNavigation component = buttonTarget.GetComponent<UIKeyNavigation>();
		if (component == null)
		{
			return;
		}
		GameObject onUp = component.onUp;
		GameObject onDown = component.onDown;
		UIKeyNavigation uikeyNavigation = null;
		UIKeyNavigation uikeyNavigation2 = null;
		if (onUp != null)
		{
			uikeyNavigation = onUp.GetComponent<UIKeyNavigation>();
		}
		if (onDown != null)
		{
			uikeyNavigation2 = onDown.GetComponent<UIKeyNavigation>();
		}
		if (uikeyNavigation == null || uikeyNavigation2 == null)
		{
			return;
		}
		if (breakConnection)
		{
			if (uikeyNavigation.onDown != buttonTarget.gameObject || uikeyNavigation2.onUp != buttonTarget.gameObject)
			{
				return;
			}
			uikeyNavigation.onDown = onDown;
			uikeyNavigation2.onUp = onUp;
		}
		else
		{
			uikeyNavigation.onDown = buttonTarget.gameObject;
			uikeyNavigation2.onUp = buttonTarget.gameObject;
		}
	}

	private static string ConvertToPercentageValue(UIPopupList popup, int value)
	{
		foreach (string text in popup.items)
		{
			int num = MenuOptions.ConvertPercentToInt(text, int.MaxValue);
			if (num == value)
			{
				return text;
			}
		}
		return popup.value;
	}

	private static int ConvertPercentToInt(string percent, int defaultResult)
	{
		int num;
		return (!int.TryParse(percent.Replace('%', ' '), out num)) ? defaultResult : num;
	}

	private void Update()
	{
		TheForest.Utils.Input.SetState(InputState.Menu, true);
		if (this.SSAO)
		{
			if (PlayerPreferences.LowMemoryMode)
			{
				this.SSAO.enabled = false;
			}
			else
			{
				this.SSAO.enabled = true;
			}
		}
		this.RefreshOptionalWidgets();
	}

	private void Awake()
	{
		if (this.Resolution)
		{
			Resolution[] resolutions = Screen.resolutions;
			this.Resolution.items = new List<string>();
			foreach (Resolution resolution in resolutions)
			{
				string item = string.Format("{0}x{1}", resolution.width, resolution.height);
				if (!this.Resolution.items.Contains(item))
				{
					this.Resolution.items.Add(item);
				}
			}
			Debug.Log(string.Format("[Options] Found resolutions:\n{0}", this.Resolution.items.Join("\n")));
		}
		if (this.Language)
		{
			this.Language.items = new List<string>();
			foreach (string item2 in UiTranslationDatabase.GetAvailableTranslations())
			{
				this.Language.items.Add(item2);
			}
		}
		if (this.Preset)
		{
			EventDelegate.Add(this.Preset.onChange, new EventDelegate.Callback(this.OnChangePreset));
		}
		if (this.Antialias)
		{
			EventDelegate.Add(this.Antialias.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Shadows)
		{
			EventDelegate.Add(this.Shadows.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.FarShadows)
		{
			EventDelegate.Add(this.FarShadows.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Scatter)
		{
			EventDelegate.Add(this.Scatter.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SSAOType)
		{
			EventDelegate.Add(this.SSAOType.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SSAO)
		{
			EventDelegate.Add(this.SSAO.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Bloom)
		{
			EventDelegate.Add(this.Bloom.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.FilmGrain)
		{
			EventDelegate.Add(this.FilmGrain.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ChromaticAberation)
		{
			EventDelegate.Add(this.ChromaticAberation.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Dof)
		{
			EventDelegate.Add(this.Dof.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ScreenSpaceReflection)
		{
			EventDelegate.Add(this.ScreenSpaceReflection.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SunshineOcclusion)
		{
			EventDelegate.Add(this.SunshineOcclusion.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumetricsType)
		{
			EventDelegate.Add(this.VolumetricsType.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumetricClouds)
		{
			EventDelegate.Add(this.VolumetricClouds.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.PostEffectsSystem)
		{
			EventDelegate.Add(this.PostEffectsSystem.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MotionBlur)
		{
			EventDelegate.Add(this.MotionBlur.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MaterialQuality)
		{
			EventDelegate.Add(this.MaterialQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Grass)
		{
			EventDelegate.Add(this.Grass.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GrassMode)
		{
			EventDelegate.Add(this.GrassMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GrassD)
		{
			EventDelegate.Add(this.GrassD.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ColorGrading)
		{
			EventDelegate.Add(this.ColorGrading.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Resolution)
		{
			EventDelegate.Add(this.Resolution.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.DrawDistance)
		{
			EventDelegate.Add(this.DrawDistance.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.TerrainQuality)
		{
			EventDelegate.Add(this.TerrainQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.TextureQuality)
		{
			EventDelegate.Add(this.TextureQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MaxFrameRate)
		{
			EventDelegate.Add(this.MaxFrameRate.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.LowQualityPhysics)
		{
			EventDelegate.Add(this.LowQualityPhysics.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ReflexionMode)
		{
			EventDelegate.Add(this.ReflexionMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.OceanQuality)
		{
			EventDelegate.Add(this.OceanQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GhostTint)
		{
			EventDelegate.Add(this.GhostTint.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GhostTintOpacity)
		{
			EventDelegate.Add(this.GhostTintOpacity.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseInvert)
		{
			EventDelegate.Add(this.MouseInvert.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseSensitivityX)
		{
			EventDelegate.Add(this.MouseSensitivityX.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseSensitivityY)
		{
			EventDelegate.Add(this.MouseSensitivityY.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseSmoothing)
		{
			EventDelegate.Add(this.MouseSmoothing.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Fov)
		{
			EventDelegate.Add(this.Fov.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Fullscreen)
		{
			EventDelegate.Add(this.Fullscreen.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Language)
		{
			EventDelegate.Add(this.Language.onChange, new EventDelegate.Callback(this.OnLanguageChange));
		}
		if (this.VSync)
		{
			EventDelegate.Add(this.VSync.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowHud)
		{
			EventDelegate.Add(this.ShowHud.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowOverlayIcons)
		{
			EventDelegate.Add(this.ShowOverlayIcons.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.OverlayIconsGrouping)
		{
			EventDelegate.Add(this.OverlayIconsGrouping.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowProjectileReticle)
		{
			EventDelegate.Add(this.ShowProjectileReticle.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowPlayerNamesMP)
		{
			EventDelegate.Add(this.ShowPlayerNamesMP.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowStealthMeter)
		{
			EventDelegate.Add(this.ShowStealthMeter.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.UseXInput)
		{
			EventDelegate.Add(this.UseXInput.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.CrouchMode)
		{
			EventDelegate.Add(this.CrouchMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SprintMode)
		{
			EventDelegate.Add(this.SprintMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GamepadCursorMode)
		{
			EventDelegate.Add(this.GamepadCursorMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GamepadRumbleMode)
		{
			EventDelegate.Add(this.GamepadRumbleMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.RegrowMode)
		{
			EventDelegate.Add(this.RegrowMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.IronForestMode)
		{
			EventDelegate.Add(this.IronForestMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.AllowCheats)
		{
			EventDelegate.Add(this.AllowCheats.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.AllowEnemies)
		{
			EventDelegate.Add(this.AllowEnemies.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.LowMemoryMode)
		{
			EventDelegate.Add(this.LowMemoryMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GammaWorldAndDay)
		{
			EventDelegate.Add(this.GammaWorldAndDay.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GammaCavesAndNight)
		{
			EventDelegate.Add(this.GammaCavesAndNight.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Contrast)
		{
			EventDelegate.Add(this.Contrast.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.AudioDrivers)
		{
			EventDelegate.Add(this.AudioDrivers.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Volume)
		{
			EventDelegate.Add(this.Volume.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumeMusic)
		{
			EventDelegate.Add(this.VolumeMusic.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumeMicrophone)
		{
			EventDelegate.Add(this.VolumeMicrophone.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VoiceCount)
		{
			EventDelegate.Add(this.VoiceCount.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.TurnSnap)
		{
			EventDelegate.Add(this.TurnSnap.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MoveDarkening)
		{
			EventDelegate.Add(this.MoveDarkening.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRAntiAliasing)
		{
			EventDelegate.Add(this.VRAntiAliasing.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRUsePhysicalCrouching)
		{
			EventDelegate.Add(this.VRUsePhysicalCrouching.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRForwardMovement)
		{
			EventDelegate.Add(this.VRForwardMovement.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRAutoRun)
		{
			EventDelegate.Add(this.VRAutoRun.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRUseRightHandedBow)
		{
			EventDelegate.Add(this.VRUseRightHandedBow.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRUseRightHandedWeapon)
		{
			EventDelegate.Add(this.VRUseRightHandedWeapon.onChange, new EventDelegate.Callback(this.OnChange));
		}
		this.BeginIgnoreEvents();
	}

	private IEnumerator Start()
	{
		yield return YieldPresets.WaitForEndOfFrame;
		if (this.Language)
		{
			this.Language.value = ((!this.Language.items.Contains(PlayerPreferences.Language)) ? "ENGLISH" : PlayerPreferences.Language);
		}
		this.EndIgnoreEvents();
		if (this.Preset)
		{
			if (TheForestQualitySettings.IsCustomized)
			{
				this.Preset.value = this.GetPopupItem(this.Preset, 0);
			}
			else
			{
				this.Preset.value = this.GetPopupItem(this.Preset, this.PresetToUiIndex(PlayerPreferences.Preset));
			}
		}
		else
		{
			this.OnChangePreset();
		}
		yield break;
	}

	private void OnDestroy()
	{
		if (this.Preset)
		{
			EventDelegate.Remove(this.Preset.onChange, new EventDelegate.Callback(this.OnChangePreset));
		}
		if (this.Antialias)
		{
			EventDelegate.Remove(this.Antialias.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Shadows)
		{
			EventDelegate.Remove(this.Shadows.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.FarShadows)
		{
			EventDelegate.Remove(this.FarShadows.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SSAOType)
		{
			EventDelegate.Remove(this.SSAOType.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SSAO)
		{
			EventDelegate.Remove(this.SSAO.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Scatter)
		{
			EventDelegate.Remove(this.Scatter.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Bloom)
		{
			EventDelegate.Remove(this.Bloom.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.FilmGrain)
		{
			EventDelegate.Remove(this.FilmGrain.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ChromaticAberation)
		{
			EventDelegate.Remove(this.ChromaticAberation.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Dof)
		{
			EventDelegate.Remove(this.Dof.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ScreenSpaceReflection)
		{
			EventDelegate.Remove(this.ScreenSpaceReflection.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SunshineOcclusion)
		{
			EventDelegate.Remove(this.SunshineOcclusion.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumetricsType)
		{
			EventDelegate.Remove(this.VolumetricsType.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumetricClouds)
		{
			EventDelegate.Remove(this.VolumetricClouds.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.PostEffectsSystem)
		{
			EventDelegate.Remove(this.PostEffectsSystem.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MotionBlur)
		{
			EventDelegate.Remove(this.MotionBlur.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MaterialQuality)
		{
			EventDelegate.Remove(this.MaterialQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Grass)
		{
			EventDelegate.Remove(this.Grass.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GrassMode)
		{
			EventDelegate.Remove(this.GrassMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GrassD)
		{
			EventDelegate.Remove(this.GrassD.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ColorGrading)
		{
			EventDelegate.Remove(this.ColorGrading.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Resolution)
		{
			EventDelegate.Remove(this.Resolution.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.DrawDistance)
		{
			EventDelegate.Remove(this.DrawDistance.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.TerrainQuality)
		{
			EventDelegate.Remove(this.TerrainQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.TextureQuality)
		{
			EventDelegate.Remove(this.TextureQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MaxFrameRate)
		{
			EventDelegate.Remove(this.MaxFrameRate.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.LowQualityPhysics)
		{
			EventDelegate.Remove(this.LowQualityPhysics.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ReflexionMode)
		{
			EventDelegate.Remove(this.ReflexionMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.OceanQuality)
		{
			EventDelegate.Remove(this.OceanQuality.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GhostTint)
		{
			EventDelegate.Remove(this.GhostTint.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GhostTintOpacity)
		{
			EventDelegate.Remove(this.GhostTintOpacity.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseInvert)
		{
			EventDelegate.Remove(this.MouseInvert.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseSensitivityX)
		{
			EventDelegate.Remove(this.MouseSensitivityX.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseSensitivityY)
		{
			EventDelegate.Remove(this.MouseSensitivityY.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MouseSmoothing)
		{
			EventDelegate.Remove(this.MouseSmoothing.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Fov)
		{
			EventDelegate.Remove(this.Fov.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Fullscreen)
		{
			EventDelegate.Remove(this.Fullscreen.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Language)
		{
			EventDelegate.Remove(this.Language.onChange, new EventDelegate.Callback(this.OnLanguageChange));
		}
		if (this.VSync)
		{
			EventDelegate.Remove(this.VSync.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowHud)
		{
			EventDelegate.Remove(this.ShowHud.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowOverlayIcons)
		{
			EventDelegate.Remove(this.ShowOverlayIcons.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.OverlayIconsGrouping)
		{
			EventDelegate.Remove(this.OverlayIconsGrouping.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowProjectileReticle)
		{
			EventDelegate.Remove(this.ShowProjectileReticle.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowPlayerNamesMP)
		{
			EventDelegate.Remove(this.ShowPlayerNamesMP.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.ShowStealthMeter)
		{
			EventDelegate.Remove(this.ShowStealthMeter.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.UseXInput)
		{
			EventDelegate.Remove(this.UseXInput.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.CrouchMode)
		{
			EventDelegate.Remove(this.CrouchMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.SprintMode)
		{
			EventDelegate.Remove(this.SprintMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GamepadCursorMode)
		{
			EventDelegate.Remove(this.GamepadCursorMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GamepadRumbleMode)
		{
			EventDelegate.Remove(this.GamepadRumbleMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.RegrowMode)
		{
			EventDelegate.Remove(this.RegrowMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.IronForestMode)
		{
			EventDelegate.Remove(this.IronForestMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.AllowCheats)
		{
			EventDelegate.Remove(this.AllowCheats.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.AllowEnemies)
		{
			EventDelegate.Remove(this.AllowEnemies.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.LowMemoryMode)
		{
			EventDelegate.Remove(this.LowMemoryMode.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GammaWorldAndDay)
		{
			EventDelegate.Remove(this.GammaWorldAndDay.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.GammaCavesAndNight)
		{
			EventDelegate.Remove(this.GammaCavesAndNight.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Contrast)
		{
			EventDelegate.Remove(this.Contrast.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.AudioDrivers)
		{
			EventDelegate.Remove(this.AudioDrivers.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.Volume)
		{
			EventDelegate.Remove(this.Volume.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumeMusic)
		{
			EventDelegate.Remove(this.VolumeMusic.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VolumeMicrophone)
		{
			EventDelegate.Remove(this.VolumeMicrophone.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VoiceCount)
		{
			EventDelegate.Remove(this.VoiceCount.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.TurnSnap)
		{
			EventDelegate.Remove(this.TurnSnap.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.MoveDarkening)
		{
			EventDelegate.Remove(this.MoveDarkening.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRAntiAliasing)
		{
			EventDelegate.Remove(this.VRAntiAliasing.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRUsePhysicalCrouching)
		{
			EventDelegate.Remove(this.VRUsePhysicalCrouching.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRForwardMovement)
		{
			EventDelegate.Remove(this.VRForwardMovement.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRAutoRun)
		{
			EventDelegate.Remove(this.VRAutoRun.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRUseRightHandedBow)
		{
			EventDelegate.Remove(this.VRUseRightHandedBow.onChange, new EventDelegate.Callback(this.OnChange));
		}
		if (this.VRUseRightHandedWeapon)
		{
			EventDelegate.Remove(this.VRUseRightHandedWeapon.onChange, new EventDelegate.Callback(this.OnChange));
		}
	}

	private bool OnOff(UIPopupList list)
	{
		return list.items.IndexOf(list.value) == 0;
	}

	private bool OffOn(UIPopupList list)
	{
		return list.items.IndexOf(list.value) == 1;
	}

	private int PopupIndex(UIPopupList list)
	{
		return (!list) ? 0 : list.items.IndexOf(list.value);
	}

	private int PresetToUiIndex(int preset)
	{
		return (preset != 5) ? (preset + 1) : 0;
	}

	private int UiIndexToPreset(int uiIndex)
	{
		return (uiIndex != 0) ? (uiIndex - 1) : 5;
	}

	private void OnChangePreset()
	{
		if (this.IgnoreEvents)
		{
			return;
		}
		this.BeginIgnoreEvents();
		int num = 2;
		bool flag = false;
		if (this.Preset)
		{
			num = (PlayerPreferences.Preset = this.UiIndexToPreset(this.PopupIndex(this.Preset)));
			if (num < 0)
			{
				return;
			}
			flag = (num == this.Preset.items.Count - 1);
			TheForestQualitySettings.UserSettings.Preset = (TheForestQualitySettings.PresetLevels)num;
			if (!flag)
			{
				TheForestQualitySettings.CopyPreset(num);
				TheForestQualitySettings.SetUnityQualityFromShadowLevel(TheForestQualitySettings.UserSettings.ShadowLevel);
				TheForestQualitySettings.CopyPreset(num);
			}
		}
		try
		{
			if (flag)
			{
				if (this.Shadows)
				{
					int num2 = (int)TheForestQualitySettings.UserSettings.ShadowLevel;
					if (num2 >= 0)
					{
						if (PlayerPreferences.is32bit)
						{
							num2 = Mathf.Max(num2, 1);
						}
						this.Shadows.value = this.GetPopupItem(this.Shadows, num2);
						TheForestQualitySettings.SetUnityQualityFromShadowLevel(TheForestQualitySettings.UserSettings.ShadowLevel);
						if (PlayerPreferences.is32bit)
						{
							TheForestQualitySettings.UserSettings.SetTextureQuality(TheForestQualitySettings.UserSettings.TextureQuality);
						}
					}
				}
				if (this.Scatter)
				{
					int scatterLevel = TheForestQualitySettings.GetScatterLevel();
					if (scatterLevel >= 0)
					{
						this.Scatter.value = this.GetPopupItem(this.Scatter, scatterLevel);
					}
				}
				if (this.Grass)
				{
					int grassLevel = TheForestQualitySettings.GetGrassLevel();
					if (grassLevel >= 0)
					{
						this.Grass.value = this.GetPopupItem(this.Grass, grassLevel);
					}
				}
				if (this.GrassMode)
				{
					this.GrassMode.value = this.GetPopupItem(this.GrassMode, (int)TheForestQualitySettings.UserSettings.GrassMode);
				}
				if (this.GrassD)
				{
					int grassLevelDensity = TheForestQualitySettings.GetGrassLevelDensity();
					if (grassLevelDensity >= 0)
					{
						this.GrassD.value = this.GetPopupItem(this.GrassD, grassLevelDensity);
					}
				}
			}
			else
			{
				int num3 = (int)TheForestQualitySettings.UserSettings.ShadowLevel;
				if (PlayerPreferences.is32bit)
				{
					num3 = Mathf.Max(num3, 1);
				}
				if (this.Shadows)
				{
					this.Shadows.value = this.GetPopupItem(this.Shadows, num3);
				}
				if (this.Scatter)
				{
					this.Scatter.value = this.GetPopupItem(this.Scatter, num);
				}
				if (this.Grass)
				{
					this.Grass.value = this.GetPopupItem(this.Grass, num);
				}
				if (this.GrassMode)
				{
					this.GrassMode.value = this.GetPopupItem(this.GrassMode, (int)TheForestQualitySettings.UserSettings.GrassMode);
				}
				if (this.GrassD)
				{
					this.GrassD.value = this.GetPopupItem(this.GrassD, num);
				}
			}
			if (PlayerPreferences.IsBelowDX11)
			{
				TheForestQualitySettings.UserSettings.PostEffectsSystem = TheForestQualitySettings.PostEffectsSystems.Legacy;
			}
		}
		catch
		{
		}
		try
		{
			this.FarShadows.value = this.GetPopupItem(this.FarShadows, (int)TheForestQualitySettings.UserSettings.FarShadowMode);
		}
		catch
		{
		}
		try
		{
			this.Antialias.value = this.GetPopupItem(this.Antialias, (int)TheForestQualitySettings.UserSettings.AntiAliasing);
		}
		catch
		{
		}
		try
		{
			this.SSAOType.value = this.GetPopupItem(this.SSAOType, (int)TheForestQualitySettings.UserSettings.SSAOType);
		}
		catch
		{
		}
		try
		{
			this.SSAO.value = this.GetPopupItem(this.SSAO, (int)TheForestQualitySettings.UserSettings.SSAO);
		}
		catch
		{
		}
		try
		{
			this.Bloom.value = this.GetPopupItem(this.Bloom, (int)TheForestQualitySettings.UserSettings.SEBloom);
		}
		catch
		{
		}
		try
		{
			this.FilmGrain.value = this.GetPopupItem(this.FilmGrain, (int)TheForestQualitySettings.UserSettings.Fg);
		}
		catch
		{
		}
		try
		{
			this.ChromaticAberation.value = this.GetPopupItem(this.ChromaticAberation, (int)TheForestQualitySettings.UserSettings.CA);
		}
		catch
		{
		}
		try
		{
			this.Dof.value = this.GetPopupItem(this.Dof, (int)TheForestQualitySettings.UserSettings.DofTech);
		}
		catch
		{
		}
		try
		{
			this.ScreenSpaceReflection.value = this.GetPopupItem(this.ScreenSpaceReflection, (int)TheForestQualitySettings.UserSettings.screenSpaceReflection);
		}
		catch
		{
		}
		try
		{
			this.PostEffectsSystem.value = this.GetPopupItem(this.PostEffectsSystem, (int)TheForestQualitySettings.UserSettings.PostEffectsSystem);
		}
		catch
		{
		}
		try
		{
			this.SunshineOcclusion.value = this.GetPopupItem(this.SunshineOcclusion, (int)TheForestQualitySettings.UserSettings.SunshineOcclusion);
		}
		catch
		{
		}
		try
		{
			this.VolumetricsType.value = this.GetPopupItem(this.VolumetricsType, (int)TheForestQualitySettings.UserSettings.VolumetricsType);
		}
		catch
		{
		}
		try
		{
			this.VolumetricClouds.value = this.GetPopupItem(this.VolumetricClouds, (int)TheForestQualitySettings.UserSettings.volumetricClouds);
		}
		catch
		{
		}
		try
		{
			this.MaterialQuality.value = this.GetPopupItem(this.MaterialQuality, (int)TheForestQualitySettings.UserSettings.MaterialQuality);
		}
		catch
		{
		}
		try
		{
			this.TerrainQuality.value = this.GetPopupItem(this.TerrainQuality, (int)TheForestQualitySettings.UserSettings.TerrainQuality);
		}
		catch
		{
		}
		try
		{
			this.ReflexionMode.value = this.GetPopupItem(this.ReflexionMode, (int)TheForestQualitySettings.UserSettings.ReflexionMode);
		}
		catch
		{
		}
		try
		{
			this.OceanQuality.value = ((!PlayerPreferences.is32bit) ? this.GetPopupItem(this.OceanQuality, (int)TheForestQualitySettings.UserSettings.OceanQuality) : this.GetPopupItem(this.OceanQuality, 2));
		}
		catch
		{
		}
		try
		{
			this.MotionBlur.value = this.GetPopupItem(this.MotionBlur, (int)TheForestQualitySettings.UserSettings.MotionBlur);
		}
		catch
		{
		}
		try
		{
			this.DrawDistance.value = this.GetPopupItem(this.DrawDistance, TheForestQualitySettings.UserSettings.DrawDistance - TheForestQualitySettings.DrawDistances.VeryHigh);
		}
		catch
		{
		}
		try
		{
			this.Resolution.value = string.Format("{0}x{1}", Screen.width, Screen.height);
		}
		catch
		{
		}
		try
		{
			this.TextureQuality.value = this.GetPopupItem(this.TextureQuality, (int)TheForestQualitySettings.UserSettings.TextureQuality);
		}
		catch
		{
		}
		try
		{
			this.GhostTint.value = this.GetPopupItem(this.GhostTint, PlayerPreferences.GhostTintNum);
		}
		catch
		{
		}
		try
		{
			this.GhostTintOpacity.value = Mathf.InverseLerp(0.0784f, 0.75f, PlayerPreferences.GhostTintOpacity);
		}
		catch
		{
		}
		try
		{
			this.MaxFrameRate.value = this.GetPopupItem(this.MaxFrameRate, (PlayerPreferences.MaxFrameRate != -1) ? 0 : 1);
		}
		catch
		{
		}
		try
		{
			this.LowQualityPhysics.value = this.GetPopupItem(this.LowQualityPhysics, (!PlayerPreferences.LowQualityPhysics) ? 0 : 1);
		}
		catch
		{
		}
		try
		{
			this.VSync.value = this.GetPopupItem(this.VSync, (!PlayerPreferences.VSync) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.ShowHud.value = this.GetPopupItem(this.ShowHud, (!PlayerPreferences.ShowHud) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.ShowOverlayIcons.value = this.GetPopupItem(this.ShowOverlayIcons, (!PlayerPreferences.ShowOverlayIcons) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.OverlayIconsGrouping.value = this.GetPopupItem(this.OverlayIconsGrouping, (!PlayerPreferences.OverlayIconsGrouping) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.ShowProjectileReticle.value = this.GetPopupItem(this.ShowProjectileReticle, (!PlayerPreferences.ShowProjectileReticle) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.ShowPlayerNamesMP.value = this.GetPopupItem(this.ShowPlayerNamesMP, (!PlayerPreferences.ShowPlayerNamesMP) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.ShowStealthMeter.value = this.GetPopupItem(this.ShowStealthMeter, (!PlayerPreferences.ShowStealthMeter) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.UseXInput.value = this.GetPopupItem(this.UseXInput, (!PlayerPreferences.UseXInput) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.LowMemoryMode.value = PlayerPreferences.LowMemoryMode;
		}
		catch
		{
		}
		try
		{
			this.Fullscreen.value = this.GetPopupItem(this.Fullscreen, (!Screen.fullScreen) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.ColorGrading.value = this.GetPopupItem(this.ColorGrading, PlayerPreferences.ColorGrading);
		}
		catch
		{
		}
		try
		{
			this.MouseInvert.value = this.GetPopupItem(this.MouseInvert, (!PlayerPreferences.MouseInvert) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.MouseSensitivityX.value = PlayerPreferences.MouseSensitivityX;
		}
		catch
		{
		}
		try
		{
			this.MouseSensitivityY.value = PlayerPreferences.MouseSensitivityY;
		}
		catch
		{
		}
		try
		{
			this.MouseSmoothing.value = PlayerPreferences.MouseSmoothing;
		}
		catch
		{
		}
		try
		{
			this.CrouchMode.value = this.GetPopupItem(this.CrouchMode, PlayerPreferences.UseCrouchToggle ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.SprintMode.value = this.GetPopupItem(this.SprintMode, PlayerPreferences.UseSprintToggle ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.GamepadCursorMode.value = this.GetPopupItem(this.GamepadCursorMode, (!PlayerPreferences.UseGamepadCursorSnapping) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.GamepadRumbleMode.value = this.GetPopupItem(this.GamepadRumbleMode, (!PlayerPreferences.UseGamepadRumble) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.RegrowMode.value = this.GetPopupItem(this.RegrowMode, (!PlayerPreferences.TreeRegrowthLocal) ? 0 : 1);
		}
		catch
		{
		}
		try
		{
			this.IronForestMode.value = this.GetPopupItem(this.IronForestMode, PlayerPreferences.NoDestructionLocal ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.AllowCheats.value = this.GetPopupItem(this.AllowCheats, (!PlayerPreferences.CheatsAllowed) ? 1 : 0);
		}
		catch
		{
		}
		if (GameSetup.IsCreativeGame)
		{
			try
			{
				this.AllowEnemies.value = this.GetPopupItem(this.AllowEnemies, (!PlayerPreferences.AllowEnemiesCreative) ? 1 : 0);
			}
			catch
			{
			}
		}
		try
		{
			this.Fov.value = (PlayerPreferences.Fov - 60f) / 35f;
		}
		catch
		{
		}
		try
		{
			this.GammaWorldAndDay.value = Mathf.InverseLerp(1.5f, 2.3f, PlayerPreferences.GammaWorldAndDay);
		}
		catch
		{
		}
		try
		{
			this.GammaCavesAndNight.value = Mathf.InverseLerp(2f, 2.5f, PlayerPreferences.GammaCavesAndNight);
		}
		catch
		{
		}
		try
		{
			this.Contrast.value = Mathf.InverseLerp(0.9f, 1.15f, PlayerPreferences.Contrast);
		}
		catch
		{
		}
		if (ForestVR.Enabled)
		{
			try
			{
				this.AudioDrivers.value = this.GetPopupItemByData(this.AudioDrivers, PlayerPreferences.AudioDriverNum);
			}
			catch
			{
			}
		}
		try
		{
			this.Volume.value = PlayerPreferences.Volume;
		}
		catch
		{
		}
		try
		{
			this.VolumeMusic.value = PlayerPreferences.MusicVolume;
		}
		catch
		{
		}
		try
		{
			this.VolumeMicrophone.value = Mathf.InverseLerp(0f, 10f, PlayerPreferences.MicrophoneVolume);
		}
		catch
		{
		}
		try
		{
			this.VoiceCount.value = PlayerPreferences.VoiceCount.ToString();
		}
		catch
		{
		}
		try
		{
			this.TurnSnap.value = PlayerPreferences.VRTurnSnap.ToString() + "%";
		}
		catch
		{
		}
		try
		{
			this.MoveDarkening.value = this.GetPopupItem(this.MoveDarkening, (int)PlayerPreferences.VRMoveDarkening);
		}
		catch
		{
		}
		try
		{
			this.VRAntiAliasing.value = this.GetPopupItem(this.VRAntiAliasing, (int)PlayerPreferences.VRAntiAliasing);
		}
		catch
		{
		}
		try
		{
			this.VRUsePhysicalCrouching.value = this.GetPopupItem(this.VRUsePhysicalCrouching, (!PlayerPreferences.VRUsePhysicalCrouching) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.VRForwardMovement.value = this.GetPopupItem(this.VRForwardMovement, (int)PlayerPreferences.VRForwardMovement);
		}
		catch
		{
		}
		try
		{
			this.VRAutoRun.value = this.GetPopupItem(this.VRAutoRun, (!PlayerPreferences.VRAutoRun) ? 1 : 0);
		}
		catch
		{
		}
		try
		{
			this.VRUseRightHandedBow.value = PlayerPreferences.VRUseRightHandedBow;
		}
		catch
		{
		}
		try
		{
			this.VRUseRightHandedWeapon.value = PlayerPreferences.VRUseRightHandedWeapon;
		}
		catch
		{
		}
		PlayerPreferences.ApplyValues();
		this.EndIgnoreEvents();
	}

	private void OnLanguageChange()
	{
		if (this.IgnoreEvents)
		{
			return;
		}
		string value = this.Language.value;
		this.BeginIgnoreEvents();
		UiTranslationDatabase.SetLanguage(value);
		this.EndIgnoreEvents();
	}

	private void CopySettingsFromGUI()
	{
		if (this.Shadows)
		{
			int num = this.PopupIndex(this.Shadows);
			if (PlayerPreferences.is32bit)
			{
				num = Mathf.Max(num, 1);
			}
			TheForestQualitySettings.ShadowLevels shadowLevels = (TheForestQualitySettings.ShadowLevels)num;
			int level = TheForestQualitySettings.ConvertToQualityIndex(shadowLevels);
			TheForestQualitySettings.SetUnityQualityFromShadowLevel(shadowLevels);
			TheForestQualitySettings.UserSettings.CascadeCount = TheForestQualitySettings.GetPreset(level).CascadeCount;
			TheForestQualitySettings.UserSettings.LightmapResolution = TheForestQualitySettings.GetPreset(level).LightmapResolution;
			TheForestQualitySettings.UserSettings.LightDistance = TheForestQualitySettings.GetPreset(level).LightDistance;
			TheForestQualitySettings.UserSettings.LightmapUpdateIntervalFrames = TheForestQualitySettings.GetPreset(level).LightmapUpdateIntervalFrames;
			TheForestQualitySettings.UserSettings.ShadowLevel = shadowLevels;
		}
		if (this.FarShadows)
		{
			TheForestQualitySettings.UserSettings.FarShadowMode = (TheForestQualitySettings.FarShadowModes)this.PopupIndex(this.FarShadows);
		}
		if (this.Preset)
		{
			PlayerPreferences.Preset = this.UiIndexToPreset(this.PopupIndex(this.Preset));
			TheForestQualitySettings.UserSettings.Preset = (TheForestQualitySettings.PresetLevels)PlayerPreferences.Preset;
		}
		if (this.Antialias)
		{
			TheForestQualitySettings.UserSettings.AntiAliasing = (TheForestQualitySettings.AntiAliasingTechnique)this.PopupIndex(this.Antialias);
		}
		if (this.Scatter)
		{
			int level2 = this.PopupIndex(this.Scatter);
			TheForestQualitySettings.UserSettings.ScatterResolution = TheForestQualitySettings.GetPreset(level2).ScatterResolution;
			TheForestQualitySettings.UserSettings.ScatterSamplingQuality = TheForestQualitySettings.GetPreset(level2).ScatterSamplingQuality;
		}
		if (this.SSAOType)
		{
			TheForestQualitySettings.UserSettings.SSAOType = (TheForestQualitySettings.SSAOTypes)this.PopupIndex(this.SSAOType);
		}
		if (this.SSAO)
		{
			TheForestQualitySettings.UserSettings.SSAO = (TheForestQualitySettings.SSAOTechnique)this.PopupIndex(this.SSAO);
		}
		if (this.Bloom)
		{
			TheForestQualitySettings.UserSettings.SEBloom = (TheForestQualitySettings.SEBloomTechnique)this.PopupIndex(this.Bloom);
		}
		if (this.FilmGrain)
		{
			TheForestQualitySettings.UserSettings.Fg = (TheForestQualitySettings.FilmGrain)this.PopupIndex(this.FilmGrain);
		}
		if (this.ChromaticAberation)
		{
			TheForestQualitySettings.UserSettings.CA = (TheForestQualitySettings.ChromaticAberration)this.PopupIndex(this.ChromaticAberation);
		}
		if (this.Dof)
		{
			TheForestQualitySettings.UserSettings.DofTech = (TheForestQualitySettings.Dof)this.PopupIndex(this.Dof);
		}
		if (this.ScreenSpaceReflection)
		{
			TheForestQualitySettings.UserSettings.screenSpaceReflection = (TheForestQualitySettings.ScreenSpaceReflection)this.PopupIndex(this.ScreenSpaceReflection);
		}
		if (this.SunshineOcclusion)
		{
			TheForestQualitySettings.UserSettings.SunshineOcclusion = (TheForestQualitySettings.SunshineOcclusionOn)this.PopupIndex(this.SunshineOcclusion);
		}
		if (this.VolumetricsType)
		{
			TheForestQualitySettings.UserSettings.VolumetricsType = (TheForestQualitySettings.VolumetricsTypes)this.PopupIndex(this.VolumetricsType);
		}
		if (this.VolumetricClouds)
		{
			TheForestQualitySettings.UserSettings.volumetricClouds = (TheForestQualitySettings.VolumetricClouds)this.PopupIndex(this.VolumetricClouds);
		}
		if (this.PostEffectsSystem)
		{
			TheForestQualitySettings.UserSettings.PostEffectsSystem = (TheForestQualitySettings.PostEffectsSystems)this.PopupIndex(this.PostEffectsSystem);
		}
		if (this.TerrainQuality)
		{
			TheForestQualitySettings.UserSettings.SetTerrainQuality((TheForestQualitySettings.TerrainQualities)this.PopupIndex(this.TerrainQuality));
		}
		if (this.TextureQuality)
		{
			TheForestQualitySettings.UserSettings.SetTextureQuality((TheForestQualitySettings.TextureQualities)this.PopupIndex(this.TextureQuality));
		}
		if (this.MaterialQuality)
		{
			TheForestQualitySettings.UserSettings.SetMaterialQuality((TheForestQualitySettings.MaterialQualities)this.PopupIndex(this.MaterialQuality));
		}
		if (this.ReflexionMode)
		{
			TheForestQualitySettings.UserSettings.ReflexionMode = (TheForestQualitySettings.ReflexionModes)this.PopupIndex(this.ReflexionMode);
		}
		if (!PlayerPreferences.is32bit && this.OceanQuality)
		{
			TheForestQualitySettings.UserSettings.OceanQuality = (TheForestQualitySettings.OceanQualities)this.PopupIndex(this.OceanQuality);
		}
		if (this.MotionBlur)
		{
			TheForestQualitySettings.UserSettings.MotionBlur = (TheForestQualitySettings.MotionBlurQuality)this.PopupIndex(this.MotionBlur);
		}
		if (this.DrawDistance)
		{
			TheForestQualitySettings.UserSettings.DrawDistance = this.PopupIndex(this.DrawDistance) + TheForestQualitySettings.DrawDistances.VeryHigh;
		}
		if (this.Grass)
		{
			TheForestQualitySettings.UserSettings.GrassDistance = TheForestQualitySettings.GetPreset(this.PopupIndex(this.Grass)).GrassDistance;
		}
		if (this.GrassMode)
		{
			TheForestQualitySettings.UserSettings.GrassMode = (TheForestQualitySettings.GrassModes)this.PopupIndex(this.GrassMode);
		}
		if (this.GrassD)
		{
			TheForestQualitySettings.UserSettings.GrassDensity = TheForestQualitySettings.GetPreset(this.PopupIndex(this.GrassD)).GrassDensity;
		}
		if (this.GhostTint)
		{
			PlayerPreferences.SetGhostTint(this.PopupIndex(this.GhostTint), Mathf.Lerp(0.0784f, 0.75f, this.GhostTintOpacity.value));
		}
		if (this.ColorGrading)
		{
			PlayerPreferences.ColorGrading = this.PopupIndex(this.ColorGrading);
		}
		if (this.MouseInvert)
		{
			PlayerPreferences.MouseInvert = this.OnOff(this.MouseInvert);
		}
		if (this.MouseSensitivityX)
		{
			PlayerPreferences.MouseSensitivityX = this.MouseSensitivityX.value;
		}
		if (this.MouseSensitivityY)
		{
			PlayerPreferences.MouseSensitivityY = this.MouseSensitivityY.value;
		}
		if (this.MouseSmoothing)
		{
			PlayerPreferences.MouseSmoothing = this.MouseSmoothing.value;
		}
		if (this.Fov)
		{
			PlayerPreferences.Fov = this.Fov.value * 35f + 60f;
		}
		if (this.VSync)
		{
			bool flag = this.OnOff(this.VSync);
			if (flag != PlayerPreferences.VSync)
			{
				PlayerPreferences.VSync = flag;
			}
		}
		if (this.ShowHud)
		{
			PlayerPreferences.ShowHud = this.OnOff(this.ShowHud);
		}
		if (this.ShowOverlayIcons)
		{
			PlayerPreferences.ShowOverlayIcons = this.OnOff(this.ShowOverlayIcons);
		}
		if (this.OverlayIconsGrouping)
		{
			PlayerPreferences.OverlayIconsGrouping = this.OnOff(this.OverlayIconsGrouping);
		}
		if (this.ShowProjectileReticle)
		{
			PlayerPreferences.ShowProjectileReticle = this.OnOff(this.ShowProjectileReticle);
		}
		if (this.ShowPlayerNamesMP)
		{
			PlayerPreferences.ShowPlayerNamesMP = this.OnOff(this.ShowPlayerNamesMP);
		}
		if (this.ShowStealthMeter)
		{
			PlayerPreferences.ShowStealthMeter = this.OnOff(this.ShowStealthMeter);
		}
		if (this.CrouchMode)
		{
			PlayerPreferences.UseCrouchToggle = this.OffOn(this.CrouchMode);
		}
		if (this.SprintMode)
		{
			PlayerPreferences.UseSprintToggle = this.OffOn(this.SprintMode);
		}
		if (this.GamepadCursorMode)
		{
			PlayerPreferences.UseGamepadCursorSnapping = this.OnOff(this.GamepadCursorMode);
		}
		if (this.GamepadRumbleMode)
		{
			PlayerPreferences.UseGamepadRumble = this.OnOff(this.GamepadRumbleMode);
		}
		if (this.RegrowMode)
		{
			PlayerPreferences.SetLocalTreeRegrowth(this.PopupIndex(this.RegrowMode) == 1);
		}
		if (this.IronForestMode)
		{
			PlayerPreferences.SetLocalNoDestructionMode(!this.OnOff(this.IronForestMode));
		}
		if (this.AllowCheats)
		{
			PlayerPreferences.SetAllowCheatsMode(this.OnOff(this.AllowCheats));
		}
		if (this.TurnSnap)
		{
			int.TryParse(this.TurnSnap.value.Replace("%", string.Empty), out PlayerPreferences.VRTurnSnap);
		}
		if (this.MoveDarkening)
		{
			PlayerPreferences.VRMoveDarkening = (PlayerPreferences.VRMoveDarkeningTypes)this.PopupIndex(this.MoveDarkening);
		}
		if (this.VRAntiAliasing)
		{
			PlayerPreferences.VRAntiAliasing = (PlayerPreferences.VRAntiAliasingTypes)this.PopupIndex(this.VRAntiAliasing);
		}
		if (this.VRUsePhysicalCrouching)
		{
			PlayerPreferences.VRUsePhysicalCrouching = this.OnOff(this.VRUsePhysicalCrouching);
		}
		if (this.VRForwardMovement)
		{
			PlayerPreferences.VRForwardMovement = (PlayerPreferences.VRForwardDirectionTypes)this.PopupIndex(this.VRForwardMovement);
		}
		if (this.VRAutoRun)
		{
			PlayerPreferences.VRAutoRun = this.OnOff(this.VRAutoRun);
		}
		if (this.VRUseRightHandedBow)
		{
			PlayerPreferences.VRUseRightHandedBow = this.VRUseRightHandedBow.value;
		}
		if (this.VRUseRightHandedWeapon)
		{
			PlayerPreferences.VRUseRightHandedWeapon = this.VRUseRightHandedWeapon.value;
		}
		if (GameSetup.IsCreativeGame && this.AllowEnemies)
		{
			PlayerPreferences.SetLocalAllowEnemiesCreativeMode(this.OnOff(this.AllowEnemies));
		}
		if (this.UseXInput)
		{
			PlayerPreferences.UseXInput = this.OnOff(this.UseXInput);
		}
		if (this.LowMemoryMode)
		{
			PlayerPreferences.LowMemoryMode = this.LowMemoryMode.value;
		}
		if (this.Fullscreen && this.Resolution)
		{
			bool flag2 = this.OnOff(this.Fullscreen);
			if (!string.IsNullOrEmpty(this.Resolution.value))
			{
				int width = Screen.width;
				int height = Screen.height;
				string[] array = this.Resolution.value.Split(new char[]
				{
					'x'
				});
				if (array.Length == 2)
				{
					int.TryParse(array[0], out width);
					int.TryParse(array[1], out height);
				}
				if (width != Screen.width || height != Screen.height || flag2 != Screen.fullScreen)
				{
					Screen.SetResolution(width, height, flag2);
					TweenTransform componentInParent = base.GetComponentInParent<TweenTransform>();
					if (componentInParent)
					{
						componentInParent.enabled = true;
					}
				}
			}
		}
		if (this.MaxFrameRate)
		{
			PlayerPreferences.MaxFrameRate = ((this.PopupIndex(this.MaxFrameRate) != 0) ? -1 : 60);
			PlayerPreferences.ApplyMaxFrameRate();
		}
		if (this.LowQualityPhysics)
		{
			PlayerPreferences.SetLowQualityPhysics(this.OffOn(this.LowQualityPhysics));
		}
		if (this.GammaCavesAndNight)
		{
			PlayerPreferences.GammaCavesAndNight = Mathf.Lerp(2f, 2.5f, this.GammaCavesAndNight.value);
		}
		if (this.GammaWorldAndDay)
		{
			PlayerPreferences.GammaWorldAndDay = Mathf.Lerp(1.5f, 2.3f, this.GammaWorldAndDay.value);
		}
		if (this.Contrast)
		{
			PlayerPreferences.Contrast = Mathf.Lerp(0.9f, 1.15f, this.Contrast.value);
		}
		if (ForestVR.Enabled && this.AudioDrivers)
		{
			PlayerPreferences.SetAudioDriver(this.AudioDrivers.value, (int)this.AudioDrivers.data);
		}
		if (this.Volume)
		{
			PlayerPreferences.Volume = this.Volume.value;
		}
		if (this.VolumeMusic)
		{
			PlayerPreferences.MusicVolume = this.VolumeMusic.value;
		}
		if (this.VolumeMicrophone)
		{
			PlayerPreferences.MicrophoneVolume = Mathf.Lerp(0f, 10f, this.VolumeMicrophone.value);
		}
		if (this.VoiceCount)
		{
			int.TryParse(this.VoiceCount.value, out PlayerPreferences.VoiceCount);
		}
		PlayerPreferences.ApplyValues();
		this.BeginIgnoreEvents();
		if (this.Preset)
		{
			if (TheForestQualitySettings.IsCustomized)
			{
				this.Preset.value = this.GetPopupItem(this.Preset, 0);
			}
			else
			{
				this.Preset.value = this.GetPopupItem(this.Preset, this.PresetToUiIndex(PlayerPreferences.Preset));
			}
		}
		this.EndIgnoreEvents();
	}

	private string GetPopupItem(UIPopupList popup, int item)
	{
		return popup.items[Mathf.Clamp(item, 0, popup.items.Count - 1)];
	}

	private string GetPopupItemByData(UIPopupList popup, object data)
	{
		return this.GetPopupItem(popup, popup.itemData.IndexOf(data));
	}

	private void OnChange()
	{
		if (this.IgnoreEvents)
		{
			return;
		}
		this.CopySettingsFromGUI();
	}

	public void Save()
	{
		this.shouldSave = false;
		PlayerPreferences.Save();
	}

	public UIPopupList Preset;

	public UIPopupList MaterialQuality;

	public UIPopupList Antialias;

	public UIPopupList Gi;

	public UIPopupList Bloom;

	public UIPopupList FilmGrain;

	public UIPopupList ChromaticAberation;

	public UIPopupList Dof;

	public UIPopupList ScreenSpaceReflection;

	public UIPopupList SunshineOcclusion;

	public UIPopupList VolumetricsType;

	public UIPopupList VolumetricClouds;

	public UIPopupList Shadows;

	public UIPopupList FarShadows;

	public UIPopupList Scatter;

	public UIPopupList SSAOType;

	public UIPopupList SSAO;

	public UIPopupList MotionBlur;

	[FormerlySerializedAs("GrassType")]
	public UIPopupList GrassMode;

	public UIPopupList Grass;

	public UIPopupList GrassD;

	public UIPopupList ColorGrading;

	public UIPopupList Resolution;

	public UIPopupList PostEffectsSystem;

	public UIPopupList DrawDistance;

	public UIPopupList TerrainQuality;

	public UIPopupList TextureQuality;

	public UIPopupList MaxFrameRate;

	public UIPopupList LowQualityPhysics;

	public UIPopupList ReflexionMode;

	public UIPopupList OceanQuality;

	public UIPopupList GhostTint;

	public UISlider GhostTintOpacity;

	public UIPopupList MouseInvert;

	public UISlider MouseSensitivityX;

	public UISlider MouseSensitivityY;

	public UISlider MouseSmoothing;

	public UIPopupList Fullscreen;

	public UIPopupList VSync;

	public UIPopupList Language;

	public UIPopupList ShowHud;

	public UIPopupList ShowOverlayIcons;

	public UIPopupList OverlayIconsGrouping;

	public UIPopupList ShowProjectileReticle;

	public UIPopupList ShowPlayerNamesMP;

	public UIPopupList ShowStealthMeter;

	public UIPopupList UseXInput;

	public UIPopupList CrouchMode;

	public UIPopupList SprintMode;

	public UIPopupList GamepadCursorMode;

	public UIPopupList GamepadRumbleMode;

	public UIPopupList RegrowMode;

	public UIPopupList IronForestMode;

	public UIPopupList AllowCheats;

	public UIPopupList AllowEnemies;

	public UIToggle LowMemoryMode;

	public UISlider GammaWorldAndDay;

	public UISlider GammaCavesAndNight;

	public UISlider Contrast;

	public UIPopupList AudioDrivers;

	public UISlider Volume;

	public UISlider VolumeMusic;

	public UISlider VolumeMicrophone;

	public UIPopupList VoiceCount;

	public UISlider Fov;

	[Header("VR")]
	public UIPopupList TurnSnap;

	public UIPopupList MoveDarkening;

	public UIPopupList VRAntiAliasing;

	public UIPopupList VRUsePhysicalCrouching;

	public UIPopupList VRForwardMovement;

	public UIPopupList VRAutoRun;

	public UIToggle VRUseRightHandedBow;

	public UIToggle VRUseRightHandedWeapon;

	private int ignoreCounter;

	private bool shouldSave;
}
