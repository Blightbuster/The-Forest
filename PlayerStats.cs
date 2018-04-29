using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Bolt;
using FMOD.Studio;
using HutongGames.PlayMaker;
using Pathfinding;
using TheForest.Buildings.World;
using TheForest.Interfaces;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Items.Utils;
using TheForest.Player.Clothing;
using TheForest.Save;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using TheForest.Utils.Settings;
using TheForest.World;
using TheForest.World.Areas;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;


[DoNotSerializePublic]
public class PlayerStats : MonoBehaviour, IBurnable
{
	
	
	public float StealthFinal
	{
		get
		{
			return ((!LocalPlayer.TargetFunctions.coveredInMud) ? LocalPlayer.Stats.Stealth : Mathf.Max(10f, LocalPlayer.Stats.Stealth)) * GameSettings.Survival.StealthRatio;
		}
	}

	
	
	public bool Warmsuit
	{
		get
		{
			return this.CurrentArmorTypes[0] == PlayerStats.ArmorTypes.Warmsuit;
		}
	}

	
	
	private bool Warm
	{
		get
		{
			return this.SunWarmth || this.FireWarmth || this.IsLit || this.BuildingWarmth > 0 || this.Warmsuit;
		}
	}

	
	
	
	public Frost FrostScript { get; private set; }

	
	
	
	public bool IsBloody { get; set; }

	
	
	public bool IsMuddy
	{
		get
		{
			return LocalPlayer.TargetFunctions.coveredInMud;
		}
	}

	
	
	
	public bool IsRed { get; set; }

	
	
	
	public bool IsCold { get; private set; }

	
	private int GetArmorSetIndex(PlayerStats.ArmorTypes type)
	{
		switch (type)
		{
		case PlayerStats.ArmorTypes.LizardSkin:
			return 0;
		case PlayerStats.ArmorTypes.DeerSkin:
			return 1;
		default:
			if (type != PlayerStats.ArmorTypes.Warmsuit)
			{
				return -1;
			}
			return 4;
		case PlayerStats.ArmorTypes.Leaves:
			return 2;
		case PlayerStats.ArmorTypes.Bone:
			return 3;
		case PlayerStats.ArmorTypes.Creepy:
			return 5;
		}
	}

	
	private void OnSerializing()
	{
		UnityEngine.Debug.Log("Serializing PlayerStats");
	}

	
	private IEnumerator OnDeserialized()
	{
		int minArmorVis = 0;
		int armor = 0;
		for (int i = 0; i < this.CurrentArmorTypes.Length; i++)
		{
			PlayerStats.ArmorTypes armorTypes = this.CurrentArmorTypes[i];
			if (armorTypes == (PlayerStats.ArmorTypes)(-1))
			{
				armorTypes = (this.CurrentArmorTypes[i] = PlayerStats.ArmorTypes.None);
			}
			int armorSetIndex = this.GetArmorSetIndex(armorTypes);
			PlayerStats.ArmorSet armorSet = (armorSetIndex == -1) ? null : this.ArmorSets[armorSetIndex];
			if (armorSet != null)
			{
				minArmorVis = i + 1;
				if (this.CurrentArmorHP[i] == 0)
				{
					this.CurrentArmorHP[i] = armorSet.HP;
				}
				armor += armorSet.HP;
				this.ToggleArmorPiece(armorSet.ModelType, armorSet.Mat, i, true);
				this.ToggleArmorPiece(armorSet.ModelType2, armorSet.Mat2, i, true);
				ItemUtils.ApplyEffectsToStats(armorSet.Effects, true, 1);
				this.ArmorVis++;
			}
		}
		this.WarmsuitModel.SetActive(this.Warmsuit);
		if (this.ArmorVis > minArmorVis)
		{
			this.ArmorVis = minArmorVis;
		}
		this.UpdateArmorNibbles();
		this.Armor = Mathf.Clamp(armor, 0, 1000);
		this.ColdArmor = Mathf.Clamp(this.ColdArmor, 0f, 2f);
		yield return null;
		if (this.DaySurvived == -1f)
		{
			this.DaySurvived = (float)Clock.Day;
		}
		bool isStarving = this.DaySurvived >= (float)this.StarvationSettings.StartDay && this.Fullness < 0.2f && this.Starvation > 0f;
		if (isStarving)
		{
			TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(true);
			TheForest.Utils.Scene.HudGui.StomachStarvation.fillAmount = Mathf.Lerp(0.21f, 0.81f, this.Starvation);
		}
		TheForest.Utils.Scene.HudGui.Hydration.fillAmount = 1f - this.Thirst;
		this.AirBreathing.CurrentRebreatherAir = Mathf.Min(this.AirBreathing.CurrentRebreatherAir, this.AirBreathing.MaxRebreatherAirCapacity);
		this.OverridePrefabSetup();
		yield break;
	}

	
	private void Awake()
	{
		BleedBehavior.BloodAmount = 0f;
		BleedBehavior.BloodReductionRatio = 1f;
		this.explodeHash = Animator.StringToHash("explode");
		GameObject gameObject = GameObject.FindWithTag("DeadSpots");
		this.DSpots = ((!(gameObject == null)) ? gameObject.GetComponent<DeadSpotController>() : null);
		this.Hud = TheForest.Utils.Scene.HudGui;
		this.Ocean = GameObject.FindWithTag("Ocean");
		this.mutantControl = TheForest.Utils.Scene.MutantControler;
		this.sceneInfo = TheForest.Utils.Scene.SceneTracker;
		this.Player = base.gameObject.GetComponent<PlayerInventory>();
		this.camFollow = base.GetComponentInChildren<camFollowHead>();
		this.hitReaction = base.GetComponent<playerHitReactions>();
		this.Atmos = TheForest.Utils.Scene.Atmosphere;
		this.FrostScript = LocalPlayer.MainCam.GetComponent<Frost>();
		this.Tuts = LocalPlayer.Tuts;
		this.Sfx = LocalPlayer.Sfx;
		this.animator = LocalPlayer.Animator;
		this.DyingVision = LocalPlayer.MainCam.GetComponent<Grayscale>();
		this.Fullness = 1f;
		this.bloodPropertyBlock = new MaterialPropertyBlock();
		this.PlayerVariations = base.GetComponent<CoopPlayerVariations>();
		if (!LevelSerializer.IsDeserializing && BoltNetwork.isRunning)
		{
			int bodyVariationCount = this.PlayerVariations.BodyVariationCount;
			int num = UnityEngine.Random.Range(1, bodyVariationCount);
			int i;
			for (i = 0; i <= bodyVariationCount; i++)
			{
				if ((CoopServerInfo.Instance.state.UsedPlayerVariations & 1 << (num + i) % bodyVariationCount) == 0)
				{
					break;
				}
			}
			num = (num + i) % bodyVariationCount;
			this.PlayerVariation = num / 20;
			this.PlayerVariationHair = num % this.PlayerVariations.Variations[this.PlayerVariation].Hair.Length;
			LocalPlayer.Clothing.CheckInit();
			LocalPlayer.Clothing.AddClothingOutfit(ClothingItemDatabase.GetRandomOutfit(this.PlayerVariation == 0), true);
		}
		if (this.CurrentArmorTypes == null || this.CurrentArmorTypes.Length != 10)
		{
			this.CurrentArmorTypes = new PlayerStats.ArmorTypes[10];
			for (int j = 0; j < this.CurrentArmorTypes.Length; j++)
			{
				this.CurrentArmorTypes[j] = PlayerStats.ArmorTypes.None;
				if (j < this.ArmorModel.Length)
				{
					this.ArmorModel[j].SetActive(false);
				}
			}
			this.UpdateArmorNibbles();
		}
		if (this.CurrentArmorHP == null || this.CurrentArmorHP.Length != 10)
		{
			this.CurrentArmorHP = new int[10];
		}
		this.CaveDoors = GameObject.FindGameObjectsWithTag("CaveDoor");
	}

	
	private void OnDestroy()
	{
		FMODCommon.ReleaseIfValid(this.SurfaceSnapshot, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.CaveSnapshot, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.CaveReverbSnapshot, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.DyingEventInstance, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.RebreatherEventInstance, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.DrowningEventInstance, STOP_MODE.IMMEDIATE);
		FMODCommon.ReleaseIfValid(this.FireExtinguishEventInstance, STOP_MODE.IMMEDIATE);
		this.Sanity.Clear();
	}

	
	private void Start()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		this.Atmos.FogMaxHeight = 400f;
		this.pm = LocalPlayer.ScriptSetup.pmControl;
		this.pmDamage = LocalPlayer.ScriptSetup.pmDamage;
		this.fsmStamina = LocalPlayer.ScriptSetup.pmStamina.FsmVariables.GetFsmFloat("statStamina");
		this.fsmMaxStamina = LocalPlayer.ScriptSetup.pmStamina.FsmVariables.GetFsmFloat("statMaxStamina");
		base.InvokeRepeating("CheckStats", 2f, 2f);
		base.InvokeRepeating("Life", 1f, 1f);
		base.InvokeRepeating("GetTired", 2f, 2f);
		if (TheForest.Utils.Scene.Cams.SleepCam.activeSelf)
		{
			this.Health = 18f;
			this.HealthTarget = 18f;
			this.Energy = 11f;
			this.Fullness = 0f;
			this.CheckingBlood = true;
			base.Invoke("CheckBlood", 10f);
			this.IsBloody = true;
			this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
			base.Invoke("CheckArmsStart", 2f);
			TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
		}
		else
		{
			this.PlayerVariations.ResetSkinColor();
		}
		this.isExplode = false;
		this.resetSkinDamage();
		if (!this.reloadedFromRespawn)
		{
			LocalPlayer.Inventory.enabled = false;
		}
		if (FMOD_StudioSystem.instance)
		{
			this.SurfaceSnapshot = FMOD_StudioSystem.instance.GetEvent("snapshot:/Surface");
			this.CaveSnapshot = FMOD_StudioSystem.instance.GetEvent("snapshot:/Cave");
			this.CaveReverbSnapshot = FMOD_StudioSystem.instance.GetEvent("snapshot:/cave_reverb");
			this.SetInCave(LocalPlayer.IsInCaves);
			this.UpdateSnapshotPositions();
			this.DyingEventInstance = FMOD_StudioSystem.instance.GetEvent(this.DyingEvent);
			UnityUtil.ERRCHECK(this.DyingEventInstance.getParameter("health", out this.DyingHealthParameter));
			UnityUtil.ERRCHECK(this.DyingHealthParameter.setValue(this.Health));
			UnityUtil.ERRCHECK(this.DyingEventInstance.start());
			this.RebreatherEventInstance = FMOD_StudioSystem.instance.GetEvent(this.RebreatherEvent);
			if (this.RebreatherEventInstance != null)
			{
				UnityUtil.ERRCHECK(this.RebreatherEventInstance.getParameter("depth", out this.RebreatherDepthParameter));
			}
			this.DrowningEventInstance = FMOD_StudioSystem.instance.GetEvent(this.DrowningEvent);
			this.FireExtinguishEventInstance = FMOD_StudioSystem.instance.GetEvent(this.ExtinguishEvent);
			base.InvokeRepeating("UpdateSnapshotPositions", 0.5f, 0.5f);
		}
		else
		{
			UnityEngine.Debug.LogError("FMOD_StudioSystem.instance is null, could not initialize PlayerStat SFX");
		}
		if (!LevelSerializer.IsDeserializing && this.DaySurvived == -1f)
		{
			this.DaySurvived = (float)Clock.Day;
		}
		this.Skills.CalcSkills();
		this.Sanity.Initialize();
		this.PhysicalStrength.Initialize();
		this.Calories.Initialize();
	}

	
	private void OverridePrefabSetup()
	{
		this.FoodPoisoning.InfectionChance._min = 0;
		this.FoodPoisoning.InfectionChance._max = 5;
		this.FoodPoisoning.EffectModifier = 0.9f;
		this.BloodInfection.InfectionChance._min = 0;
		this.BloodInfection.InfectionChance._max = 4;
		this.BloodInfection.EffectModifier = 0.9f;
	}

	
	private static void StartIfNotPlaying(FMOD.Studio.EventInstance evt)
	{
		if (evt == null)
		{
			return;
		}
		PLAYBACK_STATE playback_STATE;
		UnityUtil.ERRCHECK(evt.getPlaybackState(out playback_STATE));
		if (playback_STATE != PLAYBACK_STATE.STARTING && playback_STATE != PLAYBACK_STATE.PLAYING)
		{
			UnityUtil.ERRCHECK(evt.start());
		}
	}

	
	private static void StopIfPlaying(FMOD.Studio.EventInstance evt)
	{
		if (evt == null)
		{
			return;
		}
		PLAYBACK_STATE playback_STATE;
		UnityUtil.ERRCHECK(evt.getPlaybackState(out playback_STATE));
		if (playback_STATE != PLAYBACK_STATE.STOPPING && playback_STATE != PLAYBACK_STATE.STOPPED)
		{
			UnityUtil.ERRCHECK(evt.stop(STOP_MODE.ALLOWFADEOUT));
		}
	}

	
	public void EnableCaveAudio()
	{
		this.SetInCave(true);
	}

	
	public void DisableCaveAudio()
	{
		this.SetInCave(false);
	}

	
	private void SetInCave(bool inCave)
	{
		if (inCave)
		{
			PlayerStats.StartIfNotPlaying(this.CaveSnapshot);
			PlayerStats.StartIfNotPlaying(this.CaveReverbSnapshot);
			PlayerStats.StopIfPlaying(this.SurfaceSnapshot);
		}
		else
		{
			PlayerStats.StartIfNotPlaying(this.SurfaceSnapshot);
			PlayerStats.StopIfPlaying(this.CaveSnapshot);
			PlayerStats.StopIfPlaying(this.CaveReverbSnapshot);
		}
	}

	
	public void setStamina(float val)
	{
		this.Stamina += val;
		this.Calories.OnFighting();
	}

	
	private void UpdateRebreatherEvent()
	{
		if (this.RebreatherEventInstance != null && this.RebreatherEventInstance.isValid())
		{
			UnityUtil.ERRCHECK(this.RebreatherEventInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			if (this.RebreatherDepthParameter != null && this.RebreatherDepthParameter.isValid())
			{
				UnityUtil.ERRCHECK(this.RebreatherDepthParameter.setValue(LocalPlayer.WaterViz.CalculateDepthParameter()));
			}
			PlayerStats.StartIfNotPlaying(this.RebreatherEventInstance);
		}
	}

	
	private void UpdateDrowningEvent()
	{
		if (this.DrowningEventInstance != null && this.DrowningEventInstance.isValid())
		{
			UnityUtil.ERRCHECK(this.DrowningEventInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			PlayerStats.StartIfNotPlaying(this.DrowningEventInstance);
		}
	}

	
	private void UpdateExtinguishEvent()
	{
		if (this.IsLit && this.FireExtinguishEventInstance != null && this.FireExtinguishEventInstance.isValid())
		{
			UnityUtil.ERRCHECK(this.FireExtinguishEventInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			PlayerStats.StartIfNotPlaying(this.FireExtinguishEventInstance);
		}
	}

	
	public bool IsInNorthColdArea()
	{
		return !(TheForest.Utils.Scene.WeatherSystem == null) && !LocalPlayer.IsInCaves && ((!TheForest.Utils.Scene.WeatherSystem.UsingSnow) ? (Mathf.Floor(base.transform.position.y) > 160f && Mathf.Floor(base.transform.position.z) < -300f) : (Mathf.Ceil(base.transform.position.y) > 160f && Mathf.Ceil(base.transform.position.z) < -300f));
	}

	
	private void Update()
	{
		if (TheForest.Utils.Scene.Atmosphere == null)
		{
			return;
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			return;
		}
		float num = LocalPlayer.Stats.DaySurvived + TheForest.Utils.Scene.Atmosphere.DeltaTimeOfDay;
		if (Mathf.FloorToInt(num) != Mathf.FloorToInt(LocalPlayer.Stats.DaySurvived))
		{
			LocalPlayer.Stats.DaySurvived = num;
			EventRegistry.Player.Publish(TfEvent.SurvivedDay, null);
		}
		else
		{
			LocalPlayer.Stats.DaySurvived = num;
		}
		LocalPlayer.ScriptSetup.targetInfo.isRed = this.IsRed;
		float b;
		if (this.coldSwitch && !LocalPlayer.AnimControl.coldOffsetBool)
		{
			b = 1f;
		}
		else
		{
			b = 0f;
		}
		this.coldFloatBlend = Mathf.Lerp(this.coldFloatBlend, b, Time.deltaTime * 10f);
		if (this.coldFloatBlend > 0.01f)
		{
			this.animator.SetFloatReflected("coldFloat", this.coldFloatBlend);
		}
		else
		{
			this.animator.SetFloatReflected("coldFloat", 0f);
		}
		if (this.Run && this.HeartRate < 170)
		{
			this.HeartRate++;
		}
		else if (!this.Run && this.HeartRate > 70)
		{
			this.HeartRate--;
		}
		if (this.Sitted)
		{
			this.Energy += 3f * Time.deltaTime;
		}
		if ((!Clock.Dark && this.IsCold && !LocalPlayer.IsInCaves && !this.IsInNorthColdArea()) || LocalPlayer.IsInEndgame)
		{
			this.SetCold(false);
			this.FrostScript.coverage = 0f;
		}
		if (this.IsInNorthColdArea() && !this.Warm)
		{
			this.SetCold(true);
		}
		if (this.ShouldDoWetColdRoll && !this.IsCold && (LocalPlayer.IsInCaves || Clock.Dark))
		{
			if (!LocalPlayer.Buoyancy.InWater)
			{
				this.ShouldDoWetColdRoll = false;
			}
			else if (LocalPlayer.IsInCaves)
			{
				if (LocalPlayer.AnimControl.swimming)
				{
					if (Time.time - this.CaveStartSwimmingTime > 12f)
					{
						this.SetCold(true);
						this.ShouldDoWetColdRoll = false;
					}
				}
				else
				{
					this.CaveStartSwimmingTime = Time.time;
				}
			}
			else if (LocalPlayer.Transform.position.y - LocalPlayer.Buoyancy.WaterLevel < 1f)
			{
				if (UnityEngine.Random.Range(0, 100) < 30)
				{
					this.SetCold(true);
				}
				this.ShouldDoWetColdRoll = false;
			}
		}
		if (this.ShouldDoGotCleanCheck)
		{
			if (!LocalPlayer.Buoyancy.InWater)
			{
				this.ShouldDoGotCleanCheck = false;
			}
			else if (LocalPlayer.ScriptSetup.hipsJnt.position.y - LocalPlayer.Buoyancy.WaterLevel < -0.5f)
			{
				this.ShouldDoGotCleanCheck = false;
				this.GotCleanReal();
			}
		}
		if (this.Health <= (float)this.GreyZoneThreshold && AudioListener.volume > 0.2f)
		{
			AudioListener.volume -= 0.1f * Time.deltaTime;
		}
		else if (AudioListener.volume < 1f)
		{
			AudioListener.volume += 0.1f * Time.deltaTime;
		}
		if (this.IsHealthInGreyZone)
		{
			this.Tuts.LowHealthTutorial();
		}
		else
		{
			this.Tuts.CloseLowHealthTutorial();
		}
		if (this.Energy < 30f)
		{
			this.Tuts.LowEnergyTutorial();
		}
		else
		{
			this.Tuts.CloseLowEnergyTutorial();
		}
		if (this.Stamina <= 10f && !this.IsTired)
		{
			base.SendMessage("PlayStaminaBreath");
			this.IsTired = true;
			this.Run = false;
		}
		if (this.Stamina > 10f && this.IsTired)
		{
			this.IsTired = false;
		}
		this.fsmStamina.Value = this.Stamina;
		this.fsmMaxStamina.Value = this.Energy;
		this.HealthResult = this.Health / 100f + (100f - this.Health) / 100f * 0.5f;
		float num2 = this.HealthTarget / 100f + (100f - this.HealthTarget) / 100f * 0.5f;
		if (this.HealthTargetResult < num2)
		{
			this.HealthTargetResult = Mathf.MoveTowards(this.HealthTargetResult, num2, 1f * Time.fixedDeltaTime);
		}
		else
		{
			this.HealthTargetResult = num2;
		}
		this.StaminaResult = this.Stamina / 100f + (100f - this.Stamina) / 100f * 0.5f;
		this.EnergyResult = this.Energy / 100f + (100f - this.Energy) / 100f * 0.5f;
		int num3 = 0;
		int num4 = 0;
		int i = 0;
		while (i < this.CurrentArmorTypes.Length)
		{
			PlayerStats.ArmorTypes armorTypes = this.CurrentArmorTypes[i];
			switch (armorTypes)
			{
			case PlayerStats.ArmorTypes.LizardSkin:
			case PlayerStats.ArmorTypes.Leaves:
			case PlayerStats.ArmorTypes.Bone:
				num3++;
				break;
			case PlayerStats.ArmorTypes.DeerSkin:
				goto IL_591;
			default:
				if (armorTypes == PlayerStats.ArmorTypes.Warmsuit)
				{
					goto IL_591;
				}
				break;
			case PlayerStats.ArmorTypes.Creepy:
				num3++;
				break;
			}
			IL_5B2:
			i++;
			continue;
			IL_591:
			num4++;
			goto IL_5B2;
		}
		this.ColdArmorResult = (float)num4 / 10f / 2f + 0.5f;
		this.ArmorResult = (float)num3 / 10f / 2f + this.ColdArmorResult;
		this.Hud.ColdArmorBar.fillAmount = this.ColdArmorResult;
		this.Hud.ArmorBar.fillAmount = this.ArmorResult;
		this.Hud.StaminaBar.fillAmount = this.StaminaResult;
		this.Hud.HealthBar.fillAmount = this.HealthResult;
		this.Hud.HealthBarTarget.fillAmount = this.HealthTargetResult;
		this.Hud.EnergyBar.fillAmount = this.EnergyResult;
		float num5 = (this.Fullness - 0.2f) / 0.8f;
		TheForest.Utils.Scene.HudGui.Stomach.fillAmount = Mathf.Lerp(0.21f, 0.81f, num5);
		if ((double)num5 < 0.5)
		{
			this.Hud.StomachOutline.SetActive(true);
			if (!this.Hud.Tut_Hungry.activeSelf)
			{
				this.Tuts.HungryTutorial();
			}
		}
		else
		{
			if (this.Hud.Tut_Hungry.activeSelf)
			{
				this.Tuts.CloseHungryTutorial();
			}
			this.Hud.StomachOutline.SetActive(false);
		}
		if (!TheForest.Utils.Scene.Atmosphere.Sleeping || this.Fullness > this.StarvationSettings.SleepingFullnessThreshold)
		{
			this.Fullness -= TheForest.Utils.Scene.Atmosphere.DeltaTimeOfDay * 1.35f;
		}
		if (!Cheats.NoSurvival)
		{
			if (this.Fullness < 0.2f)
			{
				if (this.Fullness < 0.19f)
				{
					this.Fullness = 0.19f;
				}
				if (this.DaySurvived >= (float)this.StarvationSettings.StartDay && !this.Dead && !TheForest.Utils.Scene.Atmosphere.Sleeping && LocalPlayer.Inventory.enabled)
				{
					if (!TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.activeSelf)
					{
						if (this.Starvation == 0f)
						{
							this.StarvationCurrentDuration = this.StarvationSettings.Duration;
						}
						TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(true);
					}
					this.Starvation += TheForest.Utils.Scene.Atmosphere.DeltaTimeOfDay / this.StarvationCurrentDuration;
					if (this.Starvation >= 1f)
					{
						if (!this.StarvationSettings.TakingDamage)
						{
							this.StarvationSettings.TakingDamage = true;
							LocalPlayer.Tuts.ShowStarvationTut();
						}
						this.Hit(this.StarvationSettings.Damage, true, PlayerStats.DamageType.Physical);
						TheForest.Utils.Scene.HudGui.StomachStarvationTween.ResetToBeginning();
						TheForest.Utils.Scene.HudGui.StomachStarvationTween.PlayForward();
						this.Starvation = 0f;
						this.StarvationCurrentDuration *= this.StarvationSettings.DurationDecay;
					}
					TheForest.Utils.Scene.HudGui.StomachStarvation.fillAmount = Mathf.Lerp(0.21f, 0.81f, this.Starvation);
				}
			}
			else if (this.Starvation > 0f || TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.activeSelf)
			{
				this.Starvation = 0f;
				this.StarvationCurrentDuration = this.StarvationSettings.Duration;
				this.StarvationSettings.TakingDamage = false;
				LocalPlayer.Tuts.StarvationTutOff();
				TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(false);
			}
		}
		else
		{
			this.Fullness = 1f;
			if (this.Starvation > 0f || TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.activeSelf)
			{
				this.Starvation = 0f;
				this.StarvationCurrentDuration = this.StarvationSettings.Duration;
				this.StarvationSettings.TakingDamage = false;
				TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(false);
			}
		}
		if (this.Fullness > 1f)
		{
			this.Fullness = 1f;
		}
		if (!Cheats.NoSurvival)
		{
			if (this.DaySurvived >= (float)this.ThirstSettings.StartDay && !this.Dead && LocalPlayer.Inventory.enabled)
			{
				if (this.Thirst >= 1f)
				{
					if (!TheForest.Utils.Scene.HudGui.ThirstDamageTimer.gameObject.activeSelf)
					{
						TheForest.Utils.Scene.HudGui.ThirstDamageTimer.gameObject.SetActive(true);
					}
					if (this.ThirstCurrentDuration <= 0f)
					{
						this.ThirstCurrentDuration = this.ThirstSettings.DamageDelay;
						if (!this.ThirstSettings.TakingDamage)
						{
							this.ThirstSettings.TakingDamage = true;
							LocalPlayer.Tuts.ShowThirstTut();
						}
						this.Hit(Mathf.CeilToInt((float)this.ThirstSettings.Damage * GameSettings.Survival.ThirstDamageRatio), true, PlayerStats.DamageType.Physical);
						BleedBehavior.BloodAmount += 0.6f;
						TheForest.Utils.Scene.HudGui.ThirstDamageTimerTween.ResetToBeginning();
						TheForest.Utils.Scene.HudGui.ThirstDamageTimerTween.PlayForward();
					}
					else
					{
						this.ThirstCurrentDuration -= Time.deltaTime;
						TheForest.Utils.Scene.HudGui.ThirstDamageTimer.fillAmount = 1f - this.ThirstCurrentDuration / this.ThirstSettings.DamageDelay;
					}
				}
				else if (this.Thirst < 0f)
				{
					this.Thirst = 0f;
				}
				else
				{
					if (!TheForest.Utils.Scene.Atmosphere.Sleeping || this.Thirst < this.ThirstSettings.SleepingThirstThreshold)
					{
						this.Thirst += TheForest.Utils.Scene.Atmosphere.DeltaTimeOfDay / this.ThirstSettings.Duration * GameSettings.Survival.ThirstRatio;
					}
					if (this.Thirst > this.ThirstSettings.TutorialThreshold)
					{
						LocalPlayer.Tuts.ShowThirstyTut();
						TheForest.Utils.Scene.HudGui.ThirstOutline.SetActive(true);
					}
					else
					{
						LocalPlayer.Tuts.HideThirstyTut();
						TheForest.Utils.Scene.HudGui.ThirstOutline.SetActive(false);
					}
					if (this.ThirstSettings.TakingDamage)
					{
						this.ThirstSettings.TakingDamage = false;
						LocalPlayer.Tuts.ThirstTutOff();
					}
					if (TheForest.Utils.Scene.HudGui.ThirstDamageTimer.gameObject.activeSelf)
					{
						TheForest.Utils.Scene.HudGui.ThirstDamageTimer.gameObject.SetActive(false);
					}
				}
				TheForest.Utils.Scene.HudGui.Hydration.fillAmount = 1f - this.Thirst;
			}
		}
		else if (TheForest.Utils.Scene.HudGui.Hydration.fillAmount != 1f)
		{
			TheForest.Utils.Scene.HudGui.Hydration.fillAmount = 1f;
		}
		bool flag = false;
		bool flag2 = false;
		if (LocalPlayer.WaterViz.ScreenCoverage > this.AirBreathing.ScreenCoverageThreshold && !this.Dead)
		{
			if (!TheForest.Utils.Scene.HudGui.AirReserve.gameObject.activeSelf)
			{
				TheForest.Utils.Scene.HudGui.AirReserve.gameObject.SetActive(true);
			}
			if (!this.AirBreathing.UseRebreather && this.AirBreathing.RebreatherIsEquipped && this.AirBreathing.CurrentRebreatherAir > 0f)
			{
				this.AirBreathing.UseRebreather = true;
			}
			if (this.AirBreathing.UseRebreather)
			{
				flag = true;
				this.AirBreathing.CurrentRebreatherAir -= Time.deltaTime;
				TheForest.Utils.Scene.HudGui.AirReserve.fillAmount = this.AirBreathing.CurrentRebreatherAir / this.AirBreathing.MaxRebreatherAirCapacity;
				if (this.AirBreathing.CurrentRebreatherAir < 0f)
				{
					this.AirBreathing.CurrentLungAir = 0f;
					this.AirBreathing.UseRebreather = false;
				}
				else if (this.AirBreathing.CurrentRebreatherAir < this.AirBreathing.OutOfAirWarningThreshold)
				{
					if (!TheForest.Utils.Scene.HudGui.AirReserveOutline.activeSelf)
					{
						TheForest.Utils.Scene.HudGui.AirReserveOutline.SetActive(true);
					}
				}
				else if (TheForest.Utils.Scene.HudGui.AirReserveOutline.activeSelf)
				{
					TheForest.Utils.Scene.HudGui.AirReserveOutline.SetActive(false);
				}
			}
			else
			{
				if (Time.timeScale > 0f)
				{
					if (!this.AirBreathing.CurrentLungAirTimer.IsRunning)
					{
						this.AirBreathing.CurrentLungAirTimer.Start();
					}
				}
				else if (this.AirBreathing.CurrentLungAirTimer.IsRunning)
				{
					this.AirBreathing.CurrentLungAirTimer.Stop();
				}
				if (this.AirBreathing.CurrentLungAir > this.AirBreathing.MaxLungAirCapacityFinal)
				{
					this.AirBreathing.CurrentLungAir = this.AirBreathing.MaxLungAirCapacityFinal;
				}
				if ((double)this.AirBreathing.CurrentLungAir > this.AirBreathing.CurrentLungAirTimer.Elapsed.TotalSeconds * (double)this.Skills.LungBreathingRatio)
				{
					this.Skills.TotalLungBreathingDuration += Time.deltaTime;
					TheForest.Utils.Scene.HudGui.AirReserve.fillAmount = Mathf.Lerp(TheForest.Utils.Scene.HudGui.AirReserve.fillAmount, this.AirBreathing.CurrentAirPercent, Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime));
					if (!TheForest.Utils.Scene.HudGui.AirReserveOutline.activeSelf)
					{
						TheForest.Utils.Scene.HudGui.AirReserveOutline.SetActive(true);
					}
				}
				else if (!Cheats.NoSurvival)
				{
					flag2 = true;
					this.AirBreathing.DamageCounter += (float)this.AirBreathing.Damage * Time.deltaTime;
					if (this.AirBreathing.DamageCounter >= 1f)
					{
						this.Hit((int)this.AirBreathing.DamageCounter, true, PlayerStats.DamageType.Drowning);
						this.AirBreathing.DamageCounter -= (float)((int)this.AirBreathing.DamageCounter);
					}
					if (this.Dead)
					{
						this.AirBreathing.DamageCounter = 0f;
						this.DeadTimes++;
						TheForest.Utils.Scene.HudGui.AirReserve.gameObject.SetActive(false);
						TheForest.Utils.Scene.HudGui.AirReserveOutline.SetActive(false);
					}
					else if (!TheForest.Utils.Scene.HudGui.AirReserveOutline.activeSelf)
					{
						TheForest.Utils.Scene.HudGui.AirReserveOutline.SetActive(true);
					}
				}
			}
		}
		else if (this.AirBreathing.CurrentLungAir < this.AirBreathing.MaxLungAirCapacityFinal || TheForest.Utils.Scene.HudGui.AirReserve.gameObject.activeSelf)
		{
			if (this.GaspForAirEvent.Length > 0 && FMOD_StudioSystem.instance && !this.Dead)
			{
				FMOD_StudioSystem.instance.PlayOneShot(this.GaspForAirEvent, base.transform.position, delegate(FMOD.Studio.EventInstance instance)
				{
					float value = 85f;
					if (!this.AirBreathing.UseRebreather)
					{
						value = (this.AirBreathing.CurrentLungAir - (float)this.AirBreathing.CurrentLungAirTimer.Elapsed.TotalSeconds) / this.AirBreathing.MaxLungAirCapacity * 100f;
					}
					UnityUtil.ERRCHECK(instance.setParameterValue("oxygen", value));
					return true;
				});
			}
			this.AirBreathing.DamageCounter = 0f;
			this.AirBreathing.CurrentLungAirTimer.Stop();
			this.AirBreathing.CurrentLungAirTimer.Reset();
			this.AirBreathing.CurrentLungAir = this.AirBreathing.MaxLungAirCapacityFinal;
			TheForest.Utils.Scene.HudGui.AirReserve.gameObject.SetActive(false);
			TheForest.Utils.Scene.HudGui.AirReserveOutline.SetActive(false);
		}
		if (flag)
		{
			this.UpdateRebreatherEvent();
		}
		else
		{
			PlayerStats.StopIfPlaying(this.RebreatherEventInstance);
		}
		if (flag2)
		{
			this.UpdateDrowningEvent();
		}
		else
		{
			PlayerStats.StopIfPlaying(this.DrowningEventInstance);
		}
		if (this.Energy > 100f)
		{
			this.Energy = 100f;
		}
		if (this.Energy < 10f)
		{
			this.Energy = 10f;
		}
		if (this.Health < 0f)
		{
			this.Health = 0f;
		}
		if (this.Health > 100f)
		{
			this.Health = 100f;
		}
		if (this.Health < this.HealthTarget)
		{
			this.Health = Mathf.MoveTowards(this.Health, this.HealthTarget, GameSettings.Survival.HealthRegenPerSecond * Time.deltaTime);
			TheForest.Utils.Scene.HudGui.HealthBarTarget.enabled = true;
		}
		else
		{
			TheForest.Utils.Scene.HudGui.HealthBarTarget.enabled = false;
		}
		if (this.Health < 20f)
		{
			this.Hud.HealthBarOutline.SetActive(true);
		}
		else
		{
			this.Hud.HealthBarOutline.SetActive(false);
		}
		if (this.Energy < 40f || this.IsCold)
		{
			this.Hud.EnergyBarOutline.SetActive(true);
		}
		else
		{
			this.Hud.EnergyBarOutline.SetActive(false);
		}
		if (this.Stamina < 30f)
		{
			this.Hud.StaminaBarOutline.SetActive(true);
		}
		else
		{
			this.Hud.StaminaBarOutline.SetActive(false);
		}
		if (this.Stamina < 0f)
		{
			this.Stamina = 0f;
		}
		if (this.Stamina < this.Energy)
		{
			if (!LocalPlayer.FpCharacter.running && LocalPlayer.FpCharacter.recoveringFromRun <= 0f)
			{
				this.Stamina += 6f * Time.deltaTime;
			}
			else if (LocalPlayer.FpCharacter.recoveringFromRun > 0f)
			{
				LocalPlayer.FpCharacter.recoveringFromRun -= Time.deltaTime;
			}
		}
		else
		{
			this.Stamina = this.Energy;
		}
		if (this.CheckingBlood && TheForest.Utils.Scene.SceneTracker.proxyAttackers.arrayList.Count > 0)
		{
			this.StopBloodCheck();
		}
		if (this.IsCold && !this.Warm && LocalPlayer.Inventory.enabled)
		{
			if (this.BodyTemp > 14f)
			{
				this.BodyTemp -= 1f * (1f - Mathf.Clamp01(this.ColdArmor));
			}
			if (this.FrostDamageSettings.DoDeFrost)
			{
				if (this.FrostScript.coverage > this.FrostDamageSettings.DeFrostThreshold)
				{
					this.FrostScript.coverage -= 0.0159999728f * Time.deltaTime / this.FrostDamageSettings.DeFrostDuration;
				}
				else
				{
					this.FrostDamageSettings.DoDeFrost = false;
				}
			}
			else if (this.FrostScript.coverage < 0.49f || this.ColdArmor >= 1f)
			{
				if (this.FrostScript.coverage < 0f)
				{
					this.FrostScript.coverage = 0f;
				}
				this.FrostScript.coverage += 0.01f * Time.deltaTime * (1f - Mathf.Clamp01(this.ColdArmor)) * GameSettings.Survival.FrostSpeedRatio;
				if (this.FrostScript.coverage > 0.492f)
				{
					this.FrostScript.coverage = 0.491f;
				}
			}
			else if (!Cheats.NoSurvival && TheForest.Utils.Scene.Clock.ElapsedGameTime >= (float)this.FrostDamageSettings.StartDay && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Book && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory && !LocalPlayer.AnimControl.doShellRideMode)
			{
				if (!LocalPlayer.FpCharacter.jumping && (!LocalPlayer.AnimControl.onRope || !LocalPlayer.AnimControl.VerticalMovement) && !this.IsLit && LocalPlayer.Rigidbody.velocity.sqrMagnitude < 0.3f && !this.Dead)
				{
					if (this.FrostDamageSettings.CurrentTimer >= this.FrostDamageSettings.Duration)
					{
						if (this.FrostDamageSettings.DamageChance == 0)
						{
							this.Hit((int)((float)this.FrostDamageSettings.Damage * GameSettings.Survival.FrostDamageRatio), true, PlayerStats.DamageType.Frost);
							this.FrostScript.coverage = 0.506f;
							this.FrostDamageSettings.DoDeFrost = true;
							this.FrostDamageSettings.CurrentTimer = 0f;
						}
					}
					else
					{
						this.FrostDamageSettings.CurrentTimer += Time.deltaTime * ((1f - Mathf.Clamp01(this.ColdArmor)) * 1f);
					}
				}
				else
				{
					this.FrostDamageSettings.CurrentTimer = 0f;
				}
			}
		}
		if (this.Warm)
		{
			if (this.BodyTemp < 37f)
			{
				this.BodyTemp += 1f * (1f + Mathf.Clamp01(this.ColdArmor));
			}
			if (this.FrostScript.coverage > 0f)
			{
				this.FrostScript.coverage -= 0.01f * Time.deltaTime * (1f + Mathf.Clamp01(this.ColdArmor)) * GameSettings.Survival.DefrostSpeedRatio;
				if (this.FrostScript.coverage < 0f)
				{
					this.FrostScript.coverage = 0f;
				}
			}
			else
			{
				this.FrostDamageSettings.TakingDamage = false;
			}
			this.FrostDamageSettings.CurrentTimer = 0f;
		}
		if (LocalPlayer.IsInCaves)
		{
			this.Sanity.InCave();
		}
		if (PlayerSfx.MusicPlaying)
		{
			this.Sanity.ListeningToMusic();
		}
		if (this.Sitted)
		{
			this.Sanity.SittingOnBench();
		}
		this.Calories.Refresh();
		if (this.DyingEventInstance != null && !flag2 && !this.Dead)
		{
			UnityUtil.ERRCHECK(this.DyingEventInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			UnityUtil.ERRCHECK(this.DyingHealthParameter.setValue(this.Health));
		}
		if (this.FireExtinguishEventInstance != null)
		{
			UnityUtil.ERRCHECK(this.FireExtinguishEventInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
		}
		if (Cheats.InfiniteEnergy)
		{
			this.Energy = 100f;
			this.Stamina = 100f;
		}
		if (Cheats.GodMode)
		{
			this.Health = 100f;
			this.HealthTarget = 100f;
		}
	}

	
	public static void AddEndgameCavePortal(GameObject portal)
	{
		if (!PlayerStats.EndgameCavePortals.Contains(portal))
		{
			PlayerStats.EndgameCavePortals.Add(portal);
		}
	}

	
	public static void RemoveEndgameCavePortal(GameObject portal)
	{
		PlayerStats.EndgameCavePortals.Remove(portal);
	}

	
	private void UpdateSnapshotPositions()
	{
		GameObject gameObject = null;
		float num = float.MaxValue;
		for (int i = 0; i < this.CaveDoors.Length; i++)
		{
			GameObject gameObject2 = this.CaveDoors[i];
			if (gameObject2 != null)
			{
				float sqrMagnitude = (base.transform.position - gameObject2.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					gameObject = gameObject2;
				}
			}
		}
		for (int j = 0; j < PlayerStats.EndgameCavePortals.Count; j++)
		{
			GameObject gameObject3 = PlayerStats.EndgameCavePortals[j];
			if (gameObject3 != null)
			{
				float sqrMagnitude2 = (base.transform.position - gameObject3.transform.position).sqrMagnitude;
				if (sqrMagnitude2 < num)
				{
					num = sqrMagnitude2;
					gameObject = gameObject3;
				}
			}
		}
		if (gameObject)
		{
			ATTRIBUTES_3D attributes = UnityUtil.to3DAttributes(gameObject, null);
			attributes.position = gameObject.transform.TransformPoint(0f, 0f, -0.5f).toFMODVector();
			UnityUtil.ERRCHECK(this.CaveSnapshot.set3DAttributes(attributes));
			attributes.position = gameObject.transform.TransformPoint(0f, 0f, 0.5f).toFMODVector();
			UnityUtil.ERRCHECK(this.SurfaceSnapshot.set3DAttributes(attributes));
		}
	}

	
	private void Running()
	{
		this.Tuts.CloseSprint();
		this.Run = true;
	}

	
	private void StoppedRunning()
	{
		this.Run = false;
	}

	
	private void UsedEnergy()
	{
		base.CancelInvoke("StopWaiting");
		this.Energy -= this.EnergyEx;
		if (this.Stamina > 30f)
		{
			base.Invoke("HeartOff", 1f);
		}
	}

	
	private void HeartOff()
	{
	}

	
	private void StopWaiting()
	{
	}

	
	public void UsedStick()
	{
		this.EnergyEx = 0.025f;
		this.UsedEnergy();
	}

	
	private void GotMeds()
	{
		base.SendMessage("AddedMeds");
	}

	
	private void NormalizeHealthTarget()
	{
		if (this.HealthTarget < this.Health)
		{
			this.HealthTarget = this.Health;
		}
	}

	
	public void AteMeds()
	{
		this.NormalizeHealthTarget();
		this.HealthTarget += 60f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f * 1.5f;
	}

	
	public void AteAloe()
	{
		this.NormalizeHealthTarget();
		this.HealthTarget += 6f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
	}

	
	public void UsedRock()
	{
		this.EnergyEx = 0.05f;
		this.UsedEnergy();
	}

	
	public void UsedAxe()
	{
		this.EnergyEx = 0.05f;
		this.UsedEnergy();
	}

	
	private void SetCold(bool cold)
	{
		if (cold && !this.IsCold && TheForest.Utils.Scene.SceneTracker && TheForest.Utils.Scene.SceneTracker.proxyAttackers != null && TheForest.Utils.Scene.SceneTracker.proxyAttackers.arrayList.Count < 1 && this.FrostDamageSettings.NextCheckArms < TheForest.Utils.Scene.Clock.ElapsedGameTime)
		{
			this.FrostDamageSettings.NextCheckArms = TheForest.Utils.Scene.Clock.ElapsedGameTime + 1f;
			base.StartCoroutine(this.CheckArmsRoutine());
		}
		this.IsCold = cold;
		this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
		base.StopCoroutine("SetColdRoutine");
		base.StartCoroutine("SetColdRoutine");
	}

	
	private IEnumerator SetColdRoutine()
	{
		float t = 0f;
		while (t < 4f)
		{
			this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	public void ResetFrost()
	{
		this.FrostScript.coverage = 0f;
		this.FrostDamageSettings.CurrentTimer = 0f;
	}

	
	private void GotMud()
	{
		LocalPlayer.TargetFunctions.coveredInMud = true;
		this.IsRed = false;
		this.IsBloody = false;
		this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
		base.StartCoroutine(this.CheckArmsRoutine());
		if (FMOD_StudioSystem.instance)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.ApplyMudEvent, base.transform.position, null);
		}
	}

	
	private void GotRedPaint()
	{
		this.IsRed = true;
		LocalPlayer.TargetFunctions.coveredInMud = false;
		this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
		base.StartCoroutine(this.CheckArmsRoutine());
		if (FMOD_StudioSystem.instance)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.ApplyRedPaintEvent, base.transform.position, null);
		}
	}

	
	private void GotBloody()
	{
		this.bloodDice = UnityEngine.Random.Range(0, 4);
		if (this.bloodDice == 2 && !this.CheckingBlood)
		{
			this.CheckingBlood = true;
			base.Invoke("CheckBlood", 10f);
			this.IsBloody = !this.IsRed;
			base.StopCoroutine("GotCleanRoutine");
			this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
			LocalPlayer.Inventory.BloodyWeapon();
			UnityEngine.Debug.Log("player set bloody");
		}
	}

	
	private void StopBloodCheck()
	{
		base.CancelInvoke("CheckBlood");
		base.Invoke("CheckBlood", 5f);
	}

	
	public void CheckArmor()
	{
		base.StartCoroutine(this.CheckArmsRoutine());
	}

	
	public void cancelCheckItem()
	{
		bool flag = this.checkItemRoutine != null;
		if (flag)
		{
			base.StopCoroutine(this.checkItemRoutine);
		}
	}

	
	public bool CheckItem(Item.EquipmentSlot equipmentSlot)
	{
		bool flag = this.checkItemRoutine != null;
		if (flag)
		{
			base.StopCoroutine(this.checkItemRoutine);
		}
		this.checkItemRoutine = base.StartCoroutine(this.resetCheckItem(equipmentSlot));
		return !flag;
	}

	
	private IEnumerator resetCheckItem(Item.EquipmentSlot equipmentSlot)
	{
		yield return new WaitForSeconds(6f);
		this.animator.SetBoolReflected("lookAtItemRight", false);
		this.animator.SetBoolReflected("lookAtItem", false);
		this.animator.SetBoolReflected("lookAtPhoto", false);
		yield return new WaitForSeconds(0.7f);
		if (!LocalPlayer.AnimControl.WaterBlock)
		{
			if (equipmentSlot == Item.EquipmentSlot.LeftHand)
			{
				LocalPlayer.Inventory.StashLeftHand();
			}
			else
			{
				LocalPlayer.Inventory.StashEquipedWeapon(true);
			}
		}
		else
		{
			LocalPlayer.Inventory.MemorizeItem(equipmentSlot);
		}
		this.checkItemRoutine = null;
		yield break;
	}

	
	private void CheckBlood()
	{
		if (TheForest.Utils.Scene.SceneTracker.proxyAttackers.arrayList.Count < 1)
		{
			this.Tuts.BloodyTut();
			this.CheckingBlood = false;
			base.StartCoroutine(this.CheckArmsRoutine());
		}
	}

	
	private void GotClean()
	{
		if (this.BuildingWarmth == 0)
		{
			if (LocalPlayer.IsInEndgame)
			{
				this.GotCleanReal();
			}
			else
			{
				if (!this.IsCold)
				{
					if (this.IsInNorthColdArea())
					{
						this.SetCold(true);
					}
					else
					{
						this.ShouldDoWetColdRoll = (LocalPlayer.IsInCaves || Clock.Dark);
					}
				}
				this.ShouldDoGotCleanCheck = true;
				this.CaveStartSwimmingTime = Time.time;
			}
		}
	}

	
	private void GotCleanReal()
	{
		if (this.BuildingWarmth == 0)
		{
			this.ShouldDoGotCleanCheck = false;
			if (this.IsLit && this.FireExtinguishEventInstance != null && this.FireExtinguishEventInstance.isValid())
			{
				this.UpdateExtinguishEvent();
				UnityUtil.ERRCHECK(this.FireExtinguishEventInstance.start());
			}
			this.resetSkinDamage();
			this.Player.CleanWeapon();
			this.StopBurning();
			this.Tuts.CloseBloodyTut();
			LocalPlayer.TargetFunctions.coveredInMud = false;
			this.IsBloody = false;
			if (!LocalPlayer.IsInEndgame)
			{
				this.IsRed = false;
			}
			base.StartCoroutine("GotCleanRoutine");
		}
	}

	
	private IEnumerator GotCleanRoutine()
	{
		float t = 0f;
		while (t < 5f)
		{
			this.PlayerVariations.UpdateSkinVariation(this.IsBloody, LocalPlayer.TargetFunctions.coveredInMud, this.IsRed, this.IsCold);
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	public bool GoToSleep()
	{
		if (!BoltNetwork.isClient || SteamClientDSConfig.isDSFirstClient)
		{
			for (int i = 0; i < TheForest.Utils.Scene.SceneTracker.allPlayers.Count; i++)
			{
				Transform transform = TheForest.Utils.Scene.SceneTracker.allPlayers[i].transform;
				Transform transform2 = this.mutantControl.findClosestEnemy(transform);
				if (transform2 && !LocalPlayer.IsInCaves && (LocalPlayer.ScriptSetup.targetFunctions.visibleEnemies.Count > 0 || Vector3.Distance(transform.position, transform2.transform.position) < 65f))
				{
					GraphNode node = AstarPath.active.GetNearest(transform2.transform.position, NNConstraint.Default).node;
					uint area = node.Area;
					NNConstraint nnconstraint = new NNConstraint();
					nnconstraint.constrainArea = true;
					int area2 = (int)area;
					nnconstraint.area = area2;
					GraphNode node2 = AstarPath.active.GetNearest(transform.position, nnconstraint).node;
					Vector3 a = new Vector3((float)(node2.position[0] / 1000), (float)(node2.position[1] / 1000), (float)(node2.position[2] / 1000));
					if (Vector3.Distance(a, LocalPlayer.Transform.position) < 6f)
					{
						base.StartCoroutine("setupSleepEncounter", transform2.gameObject);
						this.GoToSleepFake();
						return false;
					}
				}
			}
			TheForest.Utils.Scene.MutantSpawnManager.offsetSleepAmounts();
			TheForest.Utils.Scene.MutantControler.startSetupFamilies();
			EventRegistry.Player.Publish(TfEvent.Slept, null);
		}
		base.Invoke("TurnOffSleepCam", 3f);
		this.Tired = 0f;
		this.Atmos.TimeLapse();
		TheForest.Utils.Scene.HudGui.ToggleAllHud(false);
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(true);
		this.Energy += 100f;
		return true;
	}

	
	public void JustSave()
	{
		if (!BoltNetwork.isRunning || BoltNetwork.isServer)
		{
			SaveSlotSelectionScreen.OnSlotSelected.AddListener(new UnityAction(this.OnSaveSlotSelected));
			SaveSlotSelectionScreen.OnSlotCanceled.AddListener(new UnityAction(this.OnSaveSlotSelectionCanceled));
			LocalPlayer.Inventory.TogglePauseMenu();
			LocalPlayer.Inventory.enabled = false;
			TheForest.Utils.Scene.HudGui.SaveSlotSelectionScreen.SetActive(true);
		}
		else
		{
			TheForest.Utils.Scene.Cams.SaveCam.SetActive(true);
			base.Invoke("TurnOffSaveCam", 1f);
			PlayerSpawn.SaveMpCharacter(base.gameObject);
		}
	}

	
	public void OnSaveSlotSelected()
	{
		if (!this.Dead)
		{
			base.StartCoroutine(this.OnSaveSlotSelectedRoutine());
		}
		else
		{
			TheForest.Utils.Scene.Cams.SaveCam.SetActive(false);
		}
	}

	
	private IEnumerator OnSaveSlotSelectedRoutine()
	{
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Pause;
		LocalPlayer.Inventory.TogglePauseMenu();
		TheForest.Utils.Input.UpdateControlMapping();
		TheForest.Utils.Scene.HudGui.SaveSlotSelectionScreen.SetActive(false);
		TheForest.Utils.Scene.Cams.SaveCam.SetActive(true);
		if (!LocalPlayer.IsInOverlookArea)
		{
			if (PlayerPreferences.MemorySafeSaveMode)
			{
				TheForest.Utils.Scene.GreebleZonesManager.ForcedUnload(true);
				TheForest.Utils.Scene.GreebleZonesManager.CheckInCave();
				foreach (SceneUnloadInCave sceneUnloadInCave in TheForest.Utils.Scene.SceneLoaders)
				{
					sceneUnloadInCave.ForcedUnload(true);
					sceneUnloadInCave.CheckInCave();
				}
				yield return null;
				yield return Resources.UnloadUnusedAssets();
				yield return null;
				GC.Collect();
				yield return null;
			}
			foreach (itemConstrainToHand itemConstrainToHand in LocalPlayer.ItemSlots)
			{
				foreach (GameObject gameObject in itemConstrainToHand.Available)
				{
					if (gameObject && !gameObject.gameObject.activeSelf)
					{
						FakeParent component = gameObject.GetComponent<FakeParent>();
						if (component)
						{
							component.ReParent();
						}
					}
				}
			}
			yield return null;
			SaveSlotUtils.CreateThumbnail();
			yield return null;
			LevelSerializer.Checkpoint();
			SaveSlotUtils.SaveGameDifficulty();
			if (BoltNetwork.isRunning)
			{
				SaveSlotUtils.SaveHostGameGUID();
			}
			if (PlayerPreferences.MemorySafeSaveMode)
			{
				yield return null;
				TheForest.Utils.Scene.GreebleZonesManager.ForcedUnload(false);
				foreach (SceneUnloadInCave sceneUnloadInCave2 in TheForest.Utils.Scene.SceneLoaders)
				{
					sceneUnloadInCave2.ForcedUnload(false);
				}
				yield return YieldPresets.WaitPointThreeSeconds;
			}
			foreach (itemConstrainToHand itemConstrainToHand2 in LocalPlayer.ItemSlots)
			{
				foreach (GameObject gameObject2 in itemConstrainToHand2.Available)
				{
					if (gameObject2 && !gameObject2.gameObject.activeSelf)
					{
						FakeParent component2 = gameObject2.GetComponent<FakeParent>();
						if (component2)
						{
							component2.UnParent();
						}
					}
				}
			}
		}
		else
		{
			UnityEngine.Debug.Log("It is not possible to save the game while in the overlook area");
		}
		yield return null;
		TheForest.Utils.Scene.Cams.SaveCam.SetActive(false);
		SaveSlotSelectionScreen.OnSlotSelected.RemoveListener(new UnityAction(this.OnSaveSlotSelected));
		SaveSlotSelectionScreen.OnSlotSelected.RemoveListener(new UnityAction(this.OnSaveSlotSelectionCanceled));
		LocalPlayer.Inventory.enabled = true;
		yield break;
	}

	
	public void OnSaveSlotSelectionCanceled()
	{
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Pause;
		LocalPlayer.Inventory.TogglePauseMenu();
		TheForest.Utils.Input.UpdateControlMapping();
		LocalPlayer.Inventory.enabled = true;
		TheForest.Utils.Scene.HudGui.SaveSlotSelectionScreen.SetActive(false);
		TheForest.Utils.Scene.Cams.SaveCam.SetActive(false);
		SaveSlotSelectionScreen.OnSlotSelected.RemoveListener(new UnityAction(this.OnSaveSlotSelected));
		SaveSlotSelectionScreen.OnSlotSelected.RemoveListener(new UnityAction(this.OnSaveSlotSelectionCanceled));
	}

	
	private void TurnOffSleepCam()
	{
		TheForest.Utils.Scene.HudGui.ToggleAllHud(true);
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
	}

	
	private void TurnOffSaveCam()
	{
		TheForest.Utils.Scene.Cams.SaveCam.SetActive(false);
	}

	
	private IEnumerator setupSleepEncounter(GameObject go)
	{
		mutantTypeSetup mType = go.GetComponent<mutantTypeSetup>();
		bool search = true;
		int layerMask = 67108864;
		while (search)
		{
			Vector3 randomPoint = this.Circle2(UnityEngine.Random.Range(35f, 60f));
			randomPoint = new Vector3(base.transform.position.x + randomPoint.x, base.transform.position.y + 50f, base.transform.position.z + randomPoint.y);
			RaycastHit hit;
			if (Physics.Raycast(randomPoint, Vector3.down, out hit, 200f, layerMask) && hit.collider.CompareTag("TerrainMain"))
			{
				GraphNode node = AstarPath.active.GetNearest(hit.point).node;
				if (node.Walkable)
				{
					foreach (GameObject gameObject in mType.spawner.allMembers)
					{
						if (gameObject)
						{
							targetStats component = gameObject.GetComponent<targetStats>();
							if (component && !component.targetDown)
							{
								gameObject.transform.position = new Vector3(hit.point.x + UnityEngine.Random.Range(-7f, 7f), hit.point.y + 7f, hit.point.z + UnityEngine.Random.Range(-7f, 7f));
								gameObject.SendMessage("updateWorldTransformPosition", SendMessageOptions.DontRequireReceiver);
							}
						}
					}
					search = false;
				}
			}
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	public void GoToSleepFake()
	{
		this.Tired -= 10f;
		TheForest.Utils.Scene.HudGui.ToggleAllHud(false);
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(true);
		this.Energy += 10f;
		base.Invoke("WakeFake", (!BoltNetwork.isClient) ? 4f : 3.6f);
	}

	
	private void WakeFake()
	{
		TheForest.Utils.Scene.HudGui.ToggleAllHud(true);
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
		this.Sfx.PlayWokenByEnemies();
	}

	
	private void Wake()
	{
		if (this.Atmos.Sleeping)
		{
			this.Atmos.NoTimeLapse();
			float num = TheForest.Utils.Scene.Clock.ElapsedGameTime - TheForest.Utils.Scene.Clock.NextSleepTime;
			TheForest.Utils.Scene.Clock.NextSleepTime = TheForest.Utils.Scene.Clock.ElapsedGameTime + 0.95f - num;
			this.Sanity.OnSlept(num * 24f);
			this.FoodPoisoning.Cure();
		}
	}

	
	public void Heat()
	{
		this.FireWarmth = true;
		this.SetCold(false);
	}

	
	public void LeaveHeat()
	{
		this.FireWarmth = false;
	}

	
	private void HomeWarmth()
	{
		this.BuildingWarmth++;
		this.SetCold(false);
	}

	
	private void LeaveHomeWarmth()
	{
		this.BuildingWarmth--;
	}

	
	public void CheckStats()
	{
		if (this.IsHealthInGreyZone)
		{
			if (!this.DyingVision.enabled)
			{
				this.DyingVision.enabled = true;
				Mood.PlayerLowHealth(true);
			}
			if ((!this.Dead || !BoltNetwork.isRunning) && !this.Recharge && !this.Run)
			{
				this.Recharge = true;
				base.Invoke("RechargeHealth", 12f);
			}
		}
		else if (this.DyingVision.enabled)
		{
			this.DyingVision.enabled = false;
			Mood.PlayerLowHealth(false);
		}
	}

	
	public void HealthChange(float amount)
	{
		this.NormalizeHealthTarget();
		if (amount < 0f)
		{
			this.Health += amount;
			this.HealthTarget += amount;
		}
		else
		{
			this.HealthTarget = Mathf.Min(this.HealthTarget + amount, 100f);
		}
	}

	
	private void RechargeHealth()
	{
		if (this.IsHealthInGreyZone && !this.Dead)
		{
			this.Health = (float)(this.GreyZoneThreshold + 1);
		}
		this.Recharge = false;
	}

	
	
	public bool IsHealthInGreyZone
	{
		get
		{
			return this.Health <= (float)this.GreyZoneThreshold;
		}
	}

	
	private void Fell()
	{
		this.Health -= 200f;
		this.HealthTarget = this.Health;
		if (this.Health <= 0f)
		{
			this.Dead = true;
			this.Player.enabled = false;
			this.KillPlayer();
		}
	}

	
	private void getHitDirection(Vector3 pos)
	{
		Vector3 a = new Vector3(pos.x, base.transform.position.y, pos.z);
		Vector3 normalized = (a - base.transform.position).normalized;
		LocalPlayer.ScriptSetup.pmDamage.FsmVariables.GetFsmVector3("hitDir").Value = normalized;
		this.hitReaction.hitDir = normalized;
	}

	
	public void setCurrentAttacker(enemyWeaponMelee attacker)
	{
		this.currentAttacker = attacker;
	}

	
	public void hitFromEnemy(int getDamage)
	{
		if (this.animator.GetCurrentAnimatorStateInfo(0).tagHash == this.enterCaveHash)
		{
			return;
		}
		if (this.currentAttacker)
		{
			EventRegistry.Enemy.Publish(TfEvent.EnemyContact, this.currentAttacker.GetComponentInParent<enemyType>().Type);
		}
		bool isHealthInGreyZone = this.IsHealthInGreyZone;
		if (!isHealthInGreyZone && (float)getDamage > this.Health)
		{
			getDamage = Mathf.Clamp(getDamage, 1, Mathf.FloorToInt(this.Health) - 1);
		}
		float num = 0f;
		if (this.currentAttacker)
		{
			Vector3 vector = base.transform.InverseTransformPoint(this.currentAttacker.transform.root.position);
			num = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		}
		bool flag = this.animator.GetBool("stickBlock") && num < 60f && num > -60f;
		if (flag)
		{
			if (this.blockDamagePercent > 0f)
			{
				try
				{
					if (this.CurrentArmorTypes[this.ArmorVis] == PlayerStats.ArmorTypes.None)
					{
						int damage = Mathf.FloorToInt((float)getDamage * (this.blockDamagePercent / 6f));
						this.Hit(damage, true, PlayerStats.DamageType.Physical);
					}
				}
				catch
				{
				}
			}
			this.pm.SendEvent("blockReaction");
		}
		else
		{
			if (this.animator.GetBool("stickBlock"))
			{
				getDamage /= 2;
			}
			this.Hit(getDamage, false, PlayerStats.DamageType.Physical);
			this.setSkinDamage();
			this.pm.SendEvent("blockReaction");
		}
		if (this.currentAttacker)
		{
			if (flag)
			{
				if (this.animator.GetBool("shellHeld"))
				{
					FMODCommon.PlayOneshotNetworked(this.currentAttacker.shellBlockEvent, base.transform, FMODCommon.NetworkRole.Server);
				}
				else
				{
					FMODCommon.PlayOneshotNetworked(this.currentAttacker.blockEvent, base.transform, FMODCommon.NetworkRole.Server);
				}
			}
			else
			{
				FMODCommon.PlayOneshotNetworked(this.currentAttacker.weaponHitEvent, base.transform, FMODCommon.NetworkRole.Server);
			}
			this.currentAttacker = null;
		}
		if (LocalPlayer.AnimControl.currRaft)
		{
			RaftGrab raftGrab = RaftGrab.Create(GlobalTargets.OnlyServer);
			raftGrab.OarId = LocalPlayer.AnimControl.currRaft._oarId;
			raftGrab.Raft = LocalPlayer.AnimControl.currRaft.GetComponentInParent<BoltEntity>();
			raftGrab.Player = null;
			raftGrab.Send();
		}
		if (!isHealthInGreyZone && this.IsHealthInGreyZone && !this.Dead)
		{
			base.StartCoroutine(this.AdrenalineRush());
		}
	}

	
	public void setSkinDamage()
	{
		this.MyBody.GetPropertyBlock(this.bloodPropertyBlock);
		float num = this.bloodPropertyBlock.GetFloat("_Damage1");
		if (num < 1f)
		{
			num += 0.2f;
			this.bloodPropertyBlock.SetFloat("_Damage1", num);
			this.bloodPropertyBlock.SetFloat("_Damage2", num);
			this.bloodPropertyBlock.SetFloat("_Damage3", num);
			this.bloodPropertyBlock.SetFloat("_Damage4", num);
			LocalPlayer.Animator.SetFloatReflected("skinDamage1", num);
			LocalPlayer.Animator.SetFloatReflected("skinDamage2", num);
			LocalPlayer.Animator.SetFloatReflected("skinDamage3", num);
			LocalPlayer.Animator.SetFloatReflected("skinDamage4", num);
			this.MyBody.SetPropertyBlock(this.bloodPropertyBlock);
			for (int i = 0; i < this.clothes.Length; i++)
			{
				if (this.clothes[i] && this.clothes[i].gameObject.activeSelf)
				{
					this.clothes[i].SetPropertyBlock(this.bloodPropertyBlock);
				}
			}
		}
	}

	
	public void resetSkinDamage()
	{
		this.MyBody.GetPropertyBlock(this.bloodPropertyBlock);
		this.bloodPropertyBlock.SetFloat("_Damage1", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage2", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage3", 0f);
		this.bloodPropertyBlock.SetFloat("_Damage4", 0f);
		LocalPlayer.Animator.SetFloatReflected("skinDamage1", 0f);
		LocalPlayer.Animator.SetFloatReflected("skinDamage2", 0f);
		LocalPlayer.Animator.SetFloatReflected("skinDamage3", 0f);
		LocalPlayer.Animator.SetFloatReflected("skinDamage4", 0f);
		this.MyBody.SetPropertyBlock(this.bloodPropertyBlock);
		for (int i = 0; i < this.clothes.Length; i++)
		{
			if (this.clothes[i] && this.clothes[i].gameObject.activeSelf)
			{
				this.clothes[i].SetPropertyBlock(this.bloodPropertyBlock);
			}
		}
	}

	
	private IEnumerator AdrenalineRush()
	{
		if (this.NextAdrenalineRush < Time.realtimeSinceStartup)
		{
			this.NextAdrenalineRush = Time.realtimeSinceStartup + 120f;
			TheForest.Utils.Scene.HudGui.EnergyFlash.SetActive(true);
			float totalRegenAmount = (100f - this.Stamina) / 2f;
			float remainingRegenAmount = totalRegenAmount;
			while (remainingRegenAmount > 0f)
			{
				float regenAmount = Time.deltaTime * totalRegenAmount;
				remainingRegenAmount -= regenAmount;
				this.Stamina += regenAmount;
				if (this.Energy < this.Stamina)
				{
					this.Energy = this.Stamina;
				}
				yield return null;
			}
			TheForest.Utils.Scene.HudGui.EnergyFlash.SetActive(false);
		}
		yield break;
	}

	
	private void ToggleArmorPiece(PlayerStats.ArmorTypes modelType, Material mat, int index, bool onoff)
	{
		if (modelType != PlayerStats.ArmorTypes.None)
		{
			GameObject armorPiece = this.GetArmorPiece(modelType, index);
			if (armorPiece)
			{
				if (armorPiece.activeSelf != onoff)
				{
					armorPiece.SetActive(onoff);
				}
				if (onoff && mat)
				{
					armorPiece.GetComponent<Renderer>().sharedMaterial = mat;
				}
			}
		}
	}

	
	private GameObject GetArmorPiece(PlayerStats.ArmorTypes modelType, int index)
	{
		if (modelType == PlayerStats.ArmorTypes.Bone)
		{
			return (index >= this.BoneArmorModel.Length) ? null : this.BoneArmorModel[index];
		}
		if (modelType == PlayerStats.ArmorTypes.Creepy)
		{
			return (index >= this.CreepyArmorModel.Length) ? null : this.CreepyArmorModel[index];
		}
		if (modelType == PlayerStats.ArmorTypes.Leaves)
		{
			return (index >= this.LeafArmorModel.Length) ? null : this.LeafArmorModel[index];
		}
		if (modelType != PlayerStats.ArmorTypes.Warmsuit)
		{
			return (index >= this.ArmorModel.Length) ? null : this.ArmorModel[index];
		}
		return this.WarmsuitModel;
	}

	
	public void AddArmorVisible(PlayerStats.ArmorTypes type)
	{
		int armorSetIndex = this.GetArmorSetIndex(type);
		PlayerStats.ArmorSet armorSet = (armorSetIndex == -1) ? null : this.ArmorSets[armorSetIndex];
		if (type == PlayerStats.ArmorTypes.Warmsuit)
		{
			for (int i = 0; i < 10; i++)
			{
				int num = i;
				if (this.CurrentArmorTypes[num] != PlayerStats.ArmorTypes.None)
				{
					PlayerStats.ArmorTypes type2 = this.CurrentArmorTypes[num];
					int armorSetIndex2 = this.GetArmorSetIndex(type2);
					PlayerStats.ArmorSet armorSet2 = this.ArmorSets[armorSetIndex2];
					this.ToggleArmorPiece(armorSet2.ModelType, armorSet2.Mat, num, false);
					this.ToggleArmorPiece(armorSet2.ModelType2, armorSet2.Mat2, num, false);
					ItemUtils.ApplyEffectsToStats(armorSet2.Effects, false, 1);
					if (armorSet2.HP - this.CurrentArmorHP[num] < 4 && !this.Player.AddItem(armorSet2.ItemId, 1, false, false, null))
					{
						LocalPlayer.Inventory.FakeDrop(armorSet2.ItemId, null);
					}
					this.CurrentArmorTypes[num] = PlayerStats.ArmorTypes.None;
				}
			}
			this.ArmorVis = 0;
			this.CurrentArmorTypes[0] = PlayerStats.ArmorTypes.Warmsuit;
			this.GetArmorPiece(PlayerStats.ArmorTypes.Warmsuit, 0).SetActive(true);
			ItemUtils.ApplyEffectsToStats(armorSet.Effects, true, 1);
			this.UpdateArmorNibbles();
			LocalPlayer.Clothing.RefreshVisibleClothing();
		}
		else
		{
			if (!this.Warmsuit)
			{
				for (int j = 0; j < 10; j++)
				{
					int num2 = (int)Mathf.Repeat((float)(this.ArmorVis + j), 10f);
					if (this.CurrentArmorTypes[num2] == PlayerStats.ArmorTypes.None)
					{
						this.CurrentArmorTypes[num2] = type;
						if (armorSet != null)
						{
							this.ToggleArmorPiece(armorSet.ModelType, armorSet.Mat, num2, true);
							this.ToggleArmorPiece(armorSet.ModelType2, armorSet.Mat2, num2, true);
							this.CurrentArmorHP[num2] = armorSet.HP;
							this.ArmorVis = num2 + 1;
							ItemUtils.ApplyEffectsToStats(armorSet.Effects, true, 1);
						}
						this.UpdateArmorNibbles();
						return;
					}
				}
			}
			if (this.ArmorVis == 10)
			{
				this.ArmorVis = 0;
			}
			if (this.CurrentArmorTypes[this.ArmorVis] != PlayerStats.ArmorTypes.None)
			{
				PlayerStats.ArmorTypes type3 = this.CurrentArmorTypes[this.ArmorVis];
				int armorSetIndex3 = this.GetArmorSetIndex(type3);
				PlayerStats.ArmorSet armorSet3 = this.ArmorSets[armorSetIndex3];
				this.ToggleArmorPiece(armorSet3.ModelType, armorSet3.Mat, this.ArmorVis, false);
				this.ToggleArmorPiece(armorSet3.ModelType2, armorSet3.Mat2, this.ArmorVis, false);
				ItemUtils.ApplyEffectsToStats(armorSet3.Effects, false, 1);
				if (armorSet3.HP - this.CurrentArmorHP[this.ArmorVis] < 4 && !this.Warmsuit && !this.Player.AddItem(armorSet3.ItemId, 1, false, false, null))
				{
					LocalPlayer.Inventory.FakeDrop(armorSet3.ItemId, null);
				}
				if (this.Warmsuit)
				{
					this.CurrentArmorTypes[this.ArmorVis] = PlayerStats.ArmorTypes.None;
					LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.FullBody, false, true, false);
					LocalPlayer.Clothing.RefreshVisibleClothing();
				}
			}
			this.CurrentArmorHP[this.ArmorVis] = ((armorSet == null) ? 0 : armorSet.HP);
			this.CurrentArmorTypes[this.ArmorVis] = type;
			if (armorSet != null)
			{
				this.ToggleArmorPiece(armorSet.ModelType, armorSet.Mat, this.ArmorVis, true);
				this.ToggleArmorPiece(armorSet.ModelType2, armorSet.Mat2, this.ArmorVis, true);
				ItemUtils.ApplyEffectsToStats(armorSet.Effects, true, 1);
				this.ArmorVis++;
			}
			this.UpdateArmorNibbles();
		}
	}

	
	private void UpdateArmorNibbles()
	{
		for (int i = 0; i < TheForest.Utils.Scene.HudGui.ArmorNibbles.Length; i++)
		{
			int num = (int)Mathf.Repeat((float)(TheForest.Utils.Scene.HudGui.ArmorNibbles.Length - (i - this.ArmorVis + 1)), (float)TheForest.Utils.Scene.HudGui.ArmorNibbles.Length);
			if (this.Warmsuit)
			{
				TheForest.Utils.Scene.HudGui.ArmorNibbles[i].enabled = true;
				TheForest.Utils.Scene.HudGui.ArmorNibbles[i].color = this.GetArmorSetColor(this.CurrentArmorTypes[0]);
			}
			else if (this.CurrentArmorTypes[num] == PlayerStats.ArmorTypes.None)
			{
				TheForest.Utils.Scene.HudGui.ArmorNibbles[i].enabled = false;
			}
			else
			{
				TheForest.Utils.Scene.HudGui.ArmorNibbles[i].enabled = true;
				TheForest.Utils.Scene.HudGui.ArmorNibbles[i].color = this.GetArmorSetColor(this.CurrentArmorTypes[num]);
			}
		}
	}

	
	public Color GetArmorSetColor(PlayerStats.ArmorTypes type)
	{
		int armorSetIndex = this.GetArmorSetIndex(type);
		PlayerStats.ArmorSet armorSet = this.ArmorSets[armorSetIndex];
		return armorSet.NibbleColor;
	}

	
	public int HitArmor(int damage)
	{
		PlayerStats.ArmorTypes armorTypes = PlayerStats.ArmorTypes.LizardSkin | PlayerStats.ArmorTypes.DeerSkin | PlayerStats.ArmorTypes.Leaves | PlayerStats.ArmorTypes.Bone;
		for (int i = 0; i < this.CurrentArmorTypes.Length; i++)
		{
			int num = (int)Mathf.Repeat((float)(this.ArmorVis + i), 10f);
			if ((this.CurrentArmorTypes[num] & armorTypes) != PlayerStats.ArmorTypes.None)
			{
				this.CurrentArmorHP[num] -= damage;
				if (this.CurrentArmorHP[num] > 0)
				{
					return 0;
				}
				int armorSetIndex = this.GetArmorSetIndex(this.CurrentArmorTypes[num]);
				PlayerStats.ArmorSet armorSet = this.ArmorSets[armorSetIndex];
				ItemUtils.ApplyEffectsToStats(armorSet.Effects, false, 1);
				this.ToggleArmorPiece(armorSet.ModelType, armorSet.Mat, num, false);
				this.ToggleArmorPiece(armorSet.ModelType2, armorSet.Mat2, num, false);
				this.CurrentArmorTypes[num] = PlayerStats.ArmorTypes.None;
				this.UpdateArmorNibbles();
				if (this.CurrentArmorHP[num] == 0)
				{
					return 0;
				}
				damage = -this.CurrentArmorHP[num];
				this.CurrentArmorHP[num] = 0;
			}
		}
		return damage;
	}

	
	private void Explosion(float dist)
	{
		if (this.isExplode || LocalPlayer.AnimControl.endGameCutScene)
		{
			return;
		}
		if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash == this.enterCaveHash)
		{
			return;
		}
		if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2).tagHash == this.explodeHash)
		{
			return;
		}
		if (LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.World)
		{
			return;
		}
		if (dist < 15f)
		{
			this.isExplode = true;
			base.Invoke("resetExplosion", 2.2f);
			int num = this.HitArmor(25);
			BleedBehavior.BloodAmount += Mathf.Clamp01(3f * (float)num / this.Health) * 0.9f;
			this.HealthChange((float)(-(float)num));
			BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.75f);
			LocalPlayer.Inventory.Close();
			this.CheckDeath();
			if (this.Health < 1f)
			{
				return;
			}
			if (LocalPlayer.AnimControl.swimming)
			{
				LocalPlayer.HitReactions.enableHitState();
			}
			else
			{
				LocalPlayer.AnimControl.disconnectFromObject();
				this.pmDamage.SendEvent("toHitFall");
				this.pm.SendEvent("toHit");
				this.animator.SetIntegerReflected("knockBackInt", 1);
				this.animator.SetTriggerReflected("knockBackTrigger");
				base.Invoke("ResetHit", 1f);
			}
			this.Sfx.PlayHurtSound();
			this.CheckDeath();
		}
		else if (dist > 15f)
		{
			this.isExplode = true;
			base.Invoke("resetExplosion", 2.2f);
			this.HealthChange(-10f);
			this.CheckDeath();
			if (this.Health < 1f)
			{
				return;
			}
			if (LocalPlayer.AnimControl.swimming)
			{
				LocalPlayer.HitReactions.enableHitState();
			}
			else
			{
				this.pmDamage.SendEvent("toHit");
				this.pm.SendEvent("toHit");
				this.animator.SetIntegerReflected("knockBackInt", 0);
				this.animator.SetTriggerReflected("knockBackTrigger");
				base.Invoke("ResetHit", 1f);
			}
			this.Sfx.PlayHurtSound();
			this.CheckDeath();
		}
	}

	
	private void HitFromPlayMaker(int damage)
	{
		this.Hit(damage, false, PlayerStats.DamageType.Physical);
	}

	
	private void HitFromTrap(int damage)
	{
		if (Time.time < this.trapHitCoolDown)
		{
			return;
		}
		this.trapHitCoolDown = Time.time + 1f;
		this.Hit(damage, false, PlayerStats.DamageType.Physical);
	}

	
	public void Hit(int damage, bool ignoreArmor, PlayerStats.DamageType type = PlayerStats.DamageType.Physical)
	{
		if (LocalPlayer.Inventory.CurrentView >= PlayerInventory.PlayerViews.World)
		{
			if (!this.Dead && this.animator.GetCurrentAnimatorStateInfo(0).tagHash != this.enterCaveHash && !LocalPlayer.AnimControl.endGameCutScene && !LocalPlayer.AnimControl.upsideDown && !Clock.planecrash && !LocalPlayer.AnimControl.introCutScene)
			{
				if (UnityEngine.Random.Range(0, 4) == 0)
				{
					this.Player.SpecialItems.SendMessage("TurnLighterOff");
				}
				this.Player.SpecialActions.SendMessage("forceExitSled");
				Mood.PlayerGetHit((float)damage);
				damage = ((!ignoreArmor) ? this.HitArmor(damage) : damage);
				if (damage > 0)
				{
					BleedBehavior.BloodAmount += Mathf.Clamp01(3f * (float)damage / this.Health) * 0.9f;
					this.HealthChange((float)(-(float)damage));
					BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.75f);
				}
				if (!LocalPlayer.FpCharacter.jumpCoolDown && this.Health > 0f && (this.blockDamagePercent == 0f || !this.animator.GetBool("stickBlock")))
				{
					if (!LocalPlayer.FpCharacter.Sitting && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.cliffClimb && !LocalPlayer.AnimControl.onRaft && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Book && !LocalPlayer.Animator.GetBool("leanForward") && !LocalPlayer.Animator.GetBool("lightWeaponBool") && !LocalPlayer.AnimControl.fovAimMode && !LocalPlayer.Animator.GetBool("drawBowBool") && !LocalPlayer.AnimControl.onRockThrower && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(1).shortNameHash != this.eatMeatHash && LocalPlayer.AnimControl.currLayerState1.shortNameHash != LocalPlayer.AnimControl.reloadFlintlockHash && LocalPlayer.AnimControl.currLayerState1.shortNameHash != LocalPlayer.AnimControl.reloadFlareGunlockHash && LocalPlayer.AnimControl.currLayerState1.shortNameHash != LocalPlayer.AnimControl.flareLightHash && LocalPlayer.AnimControl.currLayerState1.shortNameHash != LocalPlayer.AnimControl.spearThrowHash && LocalPlayer.AnimControl.nextLayerState1.shortNameHash != LocalPlayer.AnimControl.spearThrowHash)
					{
						this.pmDamage.SendEvent("toHit");
						this.pm.SendEvent("toHit");
						this.animator.SetIntegerReflected("knockBackInt", 0);
						this.animator.SetTriggerReflected("knockBackTrigger");
						base.Invoke("ResetHit", 1f);
					}
					else
					{
						LocalPlayer.HitReactions.enableHitState();
					}
				}
				switch (type)
				{
				case PlayerStats.DamageType.Physical:
					this.Sfx.PlayHurtSound();
					if (this.IsBloody)
					{
						this.BloodInfection.TryGetInfected();
					}
					break;
				case PlayerStats.DamageType.Poison:
					this.Sfx.PlayEatPoison();
					this.FoodPoisoning.TryGetInfected();
					break;
				case PlayerStats.DamageType.Frost:
					this.Sfx.PlayHurtSound();
					break;
				}
				if (Cheats.GodMode)
				{
					this.Health = 100f;
				}
				this.CheckDeath();
			}
			else if (BoltNetwork.isRunning && PlayerRespawnMP.IsKillable())
			{
				PlayerRespawnMP.KillPlayer();
			}
		}
	}

	
	public void HitShark(int damage)
	{
		this.HealthChange((float)(-(float)damage));
		BleedBehavior.BloodAmount += Mathf.Clamp01(3f * (float)damage / this.Health) * 0.9f;
		BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.5f);
		if (this.Health > 0f)
		{
			this.pmDamage.SendEvent("toHit");
			this.pm.SendEvent("toHit");
			this.animator.SetIntegerReflected("knockBackInt", 0);
			this.animator.SetTriggerReflected("knockBackTrigger");
			base.Invoke("ResetHit", 1f);
			this.Sfx.PlayHurtSound();
		}
		if (this.Health < 1f)
		{
			this.DeathInWater(damage);
		}
	}

	
	private void DeathInWater(int damage = 0)
	{
		base.Invoke("KillMeFast", 7f);
		this.pm.SendEvent("toDeath");
		this.pmDamage.SendEvent("toDeath");
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		this.Player.Close();
		TheForest.Utils.Scene.HudGui.DropButton.SetActive(false);
		LocalPlayer.Inventory.enabled = false;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.FpCharacter.resetPhysicMaterial();
		LocalPlayer.AnimControl.controller.velocity = Vector3.zero;
		LocalPlayer.Create.Grabber.SendMessage("ExitMessage", SendMessageOptions.DontRequireReceiver);
		LocalPlayer.Create.Grabber.gameObject.SetActive(false);
		if (BoltNetwork.isRunning)
		{
			this.Dead = true;
			this.Player.enabled = false;
		}
		this.animator.SetBoolReflected("deathBool", true);
		this.camFollow.followAnim = true;
		BleedBehavior.BloodAmount += Mathf.Clamp01(3f * (float)damage / this.Health) * 0.9f;
		BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.5f);
		base.Invoke("ResetHit", 1f);
	}

	
	private void CheckDeath()
	{
		if (Cheats.GodMode)
		{
			return;
		}
		if (this.Health <= 0f && !this.Dead)
		{
			if (LocalPlayer.AnimControl.swimming)
			{
				this.DeathInWater(0);
				return;
			}
			this.Dead = true;
			this.Player.enabled = false;
			this.FallDownDead();
		}
	}

	
	private void FallDownDead()
	{
		float time = 4f;
		if (LocalPlayer.AnimControl.swimming)
		{
			time = 7f;
		}
		this.Player.Close();
		LocalPlayer.Animator.SetBool("jumpBool", false);
		LocalPlayer.FpCharacter.HandleLanded();
		LocalPlayer.SpecialActions.SendMessage("forceGirlReset");
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("seatedBool").Value = false;
		TheForest.Utils.Scene.SceneTracker.DisableMusic();
		TheForest.Utils.Scene.HudGui.DropButton.SetActive(false);
		LocalPlayer.Inventory.enabled = false;
		if (LocalPlayer.AnimControl.carry)
		{
			LocalPlayer.AnimControl.DropBody();
		}
		else if (LocalPlayer.Inventory.Logs.HasLogs)
		{
			LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
			LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
		}
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.FpCharacter.resetPhysicMaterial();
		LocalPlayer.AnimControl.controller.velocity = Vector3.zero;
		LocalPlayer.WaterViz.AudioOff();
		LocalPlayer.Create.Grabber.SendMessage("ExitMessage", SendMessageOptions.DontRequireReceiver);
		LocalPlayer.Create.Grabber.gameObject.SetActive(false);
		this.pmDamage.SendEvent("toDeath");
		this.pm.SendEvent("toDeath");
		if (BoltNetwork.isRunning)
		{
			this.animator.SetBoolReflected("injuredBool", true);
			this.animator.SetBoolReflected("deathBool", true);
			base.Invoke("resetInjuredBool", 0.5f);
			base.Invoke("disablePlayerControl", 1f);
		}
		else
		{
			this.animator.SetBoolReflected("deathBool", true);
			this.animator.SetTriggerReflected("deathTrigger");
		}
		this.camFollow.followAnim = true;
		base.Invoke("BlackScreen", time);
		float num = 50f;
		BleedBehavior.BloodAmount += Mathf.Clamp01(3f * num / this.Health) * 0.9f;
		BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.5f);
		base.Invoke("ResetHit", 1f);
	}

	
	private void disablePlayerControl()
	{
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.CamFollowHead.followAnim = true;
	}

	
	private void resetInjuredBool()
	{
		this.animator.SetBoolReflected("deathBool", false);
	}

	
	private void KnockOut()
	{
		BleedBehavior.BloodAmount += 1f;
		BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.5f);
		this.pmDamage.SendEvent("toHit");
		this.pm.SendEvent("toHit");
		this.animator.SetTriggerReflected("knockBackTrigger");
		base.Invoke("BlackScreen", 1f);
	}

	
	private void CutSceneBlack()
	{
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(true);
		base.Invoke("CutSceneWake", 2f);
	}

	
	private void CutSceneWake()
	{
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
		base.Invoke("CutSceneBlackToMorning", 8f);
	}

	
	private void CutSceneBlackToMorning()
	{
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(true);
		base.StartCoroutine(this.WakeFromKnockOut(false, YieldPresets.WaitFourSeconds));
	}

	
	private Transform getClosestDragMarker()
	{
		float num = float.PositiveInfinity;
		Transform result = null;
		foreach (Transform transform in TheForest.Utils.Scene.SceneTracker.dragMarkers)
		{
			float sqrMagnitude = (transform.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				result = transform;
				num = sqrMagnitude;
			}
		}
		return result;
	}

	
	public IEnumerator dragAwayCutScene()
	{
		LocalPlayer.GreebleRoot.SetActive(false);
		LocalPlayer.FpCharacter.disableToggledCrouch();
		this.Health = 28f;
		this.HealthTarget = 28f;
		this.Energy = 11f;
		this.camFollow.followAnim = true;
		TheForest.Utils.Scene.MutantControler.StartCoroutine("removeAllEnemies");
		LocalPlayer.Animator.transform.GetComponent<ForceLocalPosZero>().enabled = false;
		Transform marker = this.getClosestDragMarker();
		Vector3 pos = marker.position;
		Vector3 localPlayerPos = LocalPlayer.PlayerBase.transform.localPosition;
		if (FMOD_StudioSystem.instance)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.DragCutsceneEvent, pos, null);
		}
		LocalPlayer.Inventory.StashLeftHand();
		yield return YieldPresets.WaitThreeSeconds;
		LocalPlayer.Animator.SetBoolReflected("dragAwayBool", true);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		yield return YieldPresets.WaitForFixedUpdate;
		LocalPlayer.AnimControl.injured = true;
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.AnimControl.lockGravity = true;
		this.mutant1 = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("CutScene/player_ANIM_dragAway_MUTANT1"), marker.position, marker.rotation);
		this.mutant2 = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("CutScene/player_ANIM_dragAway_MUTANT2"), marker.position, marker.rotation);
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.AnimControl.controller.freezeRotation = false;
		LocalPlayer.CamFollowHead.followAnim = true;
		LocalPlayer.Inventory.StashLeftHand();
		LocalPlayer.Inventory.StashEquipedWeapon(false);
		BleedBehavior.BloodAmount = 1f;
		BleedBehavior.BloodReductionRatio = 1f;
		pos.y += base.transform.position.y - LocalPlayer.Animator.transform.position.y;
		base.transform.position = pos;
		base.transform.rotation = marker.rotation;
		yield return YieldPresets.WaitForFixedUpdate;
		yield return YieldPresets.WaitPointOneSeconds;
		LocalPlayer.Animator.transform.rotation = marker.transform.rotation;
		LocalPlayer.Animator.transform.position = marker.transform.position;
		this.mutant1.transform.position = marker.position;
		this.mutant1.transform.rotation = marker.rotation;
		this.mutant2.transform.position = marker.position;
		this.mutant2.transform.rotation = marker.rotation;
		this.mutant1.GetComponent<Animator>().CrossFade("Base Layer.mutantDragLeft", 0f, 0, 0.1f);
		this.mutant2.GetComponent<Animator>().CrossFade("Base Layer.mutantDragRight", 0f, 0, 0.1f);
		LocalPlayer.Animator.CrossFade("fullBodyActions.dragAway", 0f, 2, 0.1f);
		this.camFollow.followAnim = true;
		LocalPlayer.Animator.transform.rotation = marker.transform.rotation;
		LocalPlayer.Animator.transform.position = marker.transform.position;
		LocalPlayer.vrPlayerControl.centerVrSpaceAroundHead();
		LocalPlayer.vrPlayerControl.rotateVrSpaceToPlayer();
		base.StartCoroutine("forcePlayerPosToMarker", marker);
		LayerMask getCamCulling = LocalPlayer.MainCam.cullingMask;
		LocalPlayer.MainCam.cullingMask = this.dragAwayCullingMask;
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
		yield return YieldPresets.WaitTenSeconds;
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(true);
		base.StopCoroutine("forcePlayerPosToMarker");
		base.StopCoroutine("forcePlayerPosToMutant");
		UnityEngine.Object.Destroy(this.mutant1);
		UnityEngine.Object.Destroy(this.mutant2);
		Resources.UnloadUnusedAssets();
		LocalPlayer.Animator.SetBoolReflected("dragAwayBool", false);
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.Animator.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.Animator.transform.GetComponent<ForceLocalPosZero>().enabled = true;
		LocalPlayer.AnimControl.injured = false;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.AnimControl.forcePos.enabled = true;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.CamFollowHead.lockYCam = false;
		LocalPlayer.MainCam.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.Animator.SetIntegerReflected("knockBackInt", 0);
		LocalPlayer.PlayerBase.transform.localPosition = localPlayerPos;
		LocalPlayer.AnimControl.controller.freezeRotation = true;
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		LocalPlayer.FpCharacter.UnLockView();
		LocalPlayer.Inventory.enabled = true;
		yield return YieldPresets.WaitTwoSeconds;
		this.doneDragScene = true;
		base.Invoke("KillPlayer", (!BoltNetwork.isRunning) ? 0.1f : 1f);
		yield return YieldPresets.WaitPointOneSeconds;
		LocalPlayer.MainCam.cullingMask = getCamCulling;
		base.StartCoroutine(this.WakeFromKnockOut(this.Dead, YieldPresets.WaitThreeSeconds));
		this.hitReaction.disableControllerFreeze();
		yield return null;
		yield break;
	}

	
	public IEnumerator hangingInCaveCutScene()
	{
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.FpCharacter.disableToggledCrouch();
		LocalPlayer.FpCharacter.UnLockView();
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
		LocalPlayer.FpCharacter.UnLockView();
		LocalPlayer.Inventory.enabled = true;
		LocalPlayer.AnimControl.upsideDown = true;
		LocalPlayer.Create.CloseTheBook(false);
		LocalPlayer.Inventory.LastLight = LocalPlayer.Inventory.DefaultLight;
		LocalPlayer.Inventory.StashLeftHand();
		LocalPlayer.Inventory.StashEquipedWeapon(false);
		yield return YieldPresets.WaitOneSecond;
		LocalPlayer.Inventory.enabled = false;
		LocalPlayer.Transform.position = TheForest.Utils.Scene.SceneTracker.playerHangingMarkers[0].transform.position;
		LocalPlayer.Transform.rotation = TheForest.Utils.Scene.SceneTracker.playerHangingMarkers[0].transform.rotation;
		GameObject timmy = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("CutScene/axeCutScenePickup"), TheForest.Utils.Scene.SceneTracker.timmyCaveMarkers[0].transform.position, TheForest.Utils.Scene.SceneTracker.timmyCaveMarkers[0].transform.rotation);
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("seatedBool").Value = false;
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.AnimControl.lockGravity = true;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.AnimControl.upsideDown = true;
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("hangingUpsideDown").Value = true;
		LocalPlayer.Animator.SetIntegerReflected("hangingInt", 0);
		LocalPlayer.Animator.SetBoolReflected("hangingBool", true);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		LocalPlayer.MainRotator.enabled = false;
		BleedBehavior.BloodAmount = 0f;
		yield return YieldPresets.WaitPointTwoSeconds;
		GameObject rope = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("CutScene/hangingPlayerRopeGo"), LocalPlayer.PlayerBase.transform.position, LocalPlayer.PlayerBase.transform.rotation);
		rope.transform.parent = LocalPlayer.ScriptSetup.hipsJnt.transform;
		rope.transform.localPosition = new Vector3(0.04f, 3.39f, -0.03f);
		rope.transform.localEulerAngles = new Vector3(-1.08f, 179.48f, -179.87f);
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamFollowHead.followAnim = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
		yield return YieldPresets.WaitPointTwoSeconds;
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
		TheForest.Utils.Scene.Cams.CaveDeadCam.SetActive(false);
		TheForest.Utils.Scene.Cams.DeadCam.SetActive(false);
		LocalPlayer.PlayerDeadCam.SetActive(false);
		yield return YieldPresets.WaitOneSecond;
		TheForest.Utils.Scene.HudGui.ActionIconCams.enabled = true;
		LocalPlayer.Tuts.ShowLighter();
		LocalPlayer.Inventory.enabled = true;
		AnimatorStateInfo currState3 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(3);
		bool lightOn = false;
		while (!lightOn)
		{
			currState3 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(3);
			if (currState3.IsName("leftArm.lighterIdle"))
			{
				lightOn = true;
			}
			if (currState3.IsName("leftArm.flashlightIdle"))
			{
				lightOn = true;
			}
			yield return null;
		}
		this.Sfx.PlayWokenByEnemies();
		this.camFollow.followAnim = true;
		LocalPlayer.Animator.SetIntegerReflected("hangingInt", 1);
		AnimatorStateInfo currState4 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
		float hangTimer = 0f;
		bool lighterSkip = false;
		while (!lighterSkip)
		{
			if (currState4.tagHash == LocalPlayer.AnimControl.hangingHash || hangTimer > 8f)
			{
				lighterSkip = true;
			}
			hangTimer += Time.deltaTime;
			currState4 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			yield return null;
		}
		LocalPlayer.CamRotator.xOffset = 0f;
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(200f, 98f);
		hangTimer = 0f;
		while (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._planeAxeId))
		{
			yield return null;
		}
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		yield break;
	}

	
	private IEnumerator releaseFromHanging()
	{
		base.StartCoroutine("lockLayersHanging");
		LocalPlayer.AnimControl.StartCoroutine("smoothDisableLayerNew", 1);
		LocalPlayer.AnimControl.StartCoroutine("smoothDisableLayerNew", 4);
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		LocalPlayer.Inventory.StashEquipedWeapon(false);
		LocalPlayer.Animator.SetBoolReflected("hangingBool", false);
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("hangingUpsideDown").Value = false;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.FpCharacter.enabled = false;
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.CamFollowHead.followAnim = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.AnimControl.upsideDown = false;
		AnimatorStateInfo currState = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
		while (currState.tagHash != LocalPlayer.AnimControl.idleHash)
		{
			currState = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			yield return null;
		}
		base.StopCoroutine("lockLayersHanging");
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.AnimControl.upsideDown = false;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Inventory.enabled = true;
		LocalPlayer.Inventory.Equip(LocalPlayer.AnimControl._planeAxeId, false);
		LocalPlayer.GreebleRoot.SetActive(true);
		TheForest.Utils.Scene.HudGui.ShowHud(true);
		yield return YieldPresets.WaitFourSeconds;
		EventRegistry.Player.Publish(TfEvent.StoryProgress, GameStats.StoryElements.HangingScene);
		yield break;
	}

	
	public IEnumerator EndgameWakeUp()
	{
		TheForest.Utils.Scene.Cams.CaveDeadCam.SetActive(true);
		LocalPlayer.Create.CloseTheBook(false);
		LocalPlayer.Inventory.StashLeftHand();
		LocalPlayer.Inventory.StashEquipedWeapon(false);
		yield return null;
		float oldfar = 0f;
		float oldnear = 0f;
		Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
		LocalPlayer.AnimControl.lockPlayerParams();
		oldfar = LocalPlayer.MainCam.farClipPlane;
		oldnear = LocalPlayer.MainCam.nearClipPlane;
		LocalPlayer.MainCam.farClipPlane = 600f;
		LocalPlayer.MainCam.nearClipPlane = 0.1f;
		GameObject respawnBed = GameObject.Find("RespawnBed");
		Rigidbody rigidbody = LocalPlayer.Rigidbody;
		Vector3 position = respawnBed.transform.position + new Vector3(0f, 2.35f, 0f);
		LocalPlayer.Transform.position = position;
		rigidbody.position = position;
		LocalPlayer.Transform.rotation = respawnBed.transform.rotation;
		Area area = respawnBed.GetComponentInParent<Area>();
		if (area)
		{
			area.OnEnter(null);
		}
		this.FrostScript.coverage = 0f;
		this.SetCold(false);
		yield return YieldPresets.WaitThreeSeconds;
		LocalPlayer.Sfx.PlayStaminaBreath();
		this.Dead = false;
		this.DeadTimes = 0;
		this.Health = (this.HealthTarget = (float)(this.GreyZoneThreshold + 1));
		Mood.PlayerLowHealth(false);
		this.animator.SetBoolReflected("checkArms", false);
		this.animator.SetBoolReflected("wakeUp", true);
		int wakeUpHash = Animator.StringToHash("wakeUp");
		while (this.animator.GetCurrentAnimatorStateInfo(2).shortNameHash != wakeUpHash)
		{
			Rigidbody rigidbody2 = LocalPlayer.Rigidbody;
			position = respawnBed.transform.position + new Vector3(0f, 2.35f, 0f);
			LocalPlayer.Transform.position = position;
			rigidbody2.position = position;
			LocalPlayer.Transform.rotation = respawnBed.transform.rotation;
			yield return null;
		}
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.FpCharacter.enabled = false;
		float timer = Time.time + 0.3f;
		while (Time.time < timer)
		{
			Rigidbody rigidbody3 = LocalPlayer.Rigidbody;
			position = respawnBed.transform.position + new Vector3(0f, 2.35f, 0f);
			LocalPlayer.Transform.position = position;
			rigidbody3.position = position;
			LocalPlayer.Transform.rotation = respawnBed.transform.rotation;
			LocalPlayer.PlayerBase.transform.localEulerAngles = Vector3.zero;
			yield return null;
		}
		this.animator.SetBoolReflected("wakeUp", false);
		LocalPlayer.PlayerDeadCam.SetActive(false);
		TheForest.Utils.Scene.Cams.DeadCam.SetActive(false);
		TheForest.Utils.Scene.Cams.CaveDeadCam.SetActive(false);
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
		while (this.animator.GetCurrentAnimatorStateInfo(2).shortNameHash == wakeUpHash)
		{
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			yield return null;
		}
		TheForest.Utils.Scene.HudGui.ShowHud(true);
		TheForest.Utils.Scene.HudGui.MpRespawnLabel.gameObject.SetActive(false);
		TheForest.Utils.Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(false);
		LocalPlayer.MainCam.farClipPlane = oldfar;
		LocalPlayer.MainCam.nearClipPlane = oldnear;
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.AnimControl.unlockPlayerParams();
		yield break;
	}

	
	private IEnumerator forcePlayerPosToMarker(Transform m)
	{
		Animator m2 = this.mutant1.GetComponent<Animator>();
		Animator m3 = this.mutant2.GetComponent<Animator>();
		for (;;)
		{
			AnimatorStateInfo p = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			LocalPlayer.Animator.SetLayerWeight(1, 0f);
			LocalPlayer.Animator.SetLayerWeight(2, 1f);
			LocalPlayer.Animator.SetLayerWeight(3, 0f);
			LocalPlayer.Animator.SetLayerWeight(4, 0f);
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator forcePlayerPosToMutant(Transform m)
	{
		for (;;)
		{
			Vector3 setPos = new Vector3(LocalPlayer.PlayerBase.transform.position.x, m.position.y, m.position.z);
			LocalPlayer.PlayerBase.transform.position = setPos;
			LocalPlayer.Animator.SetLayerWeight(1, 0f);
			LocalPlayer.Animator.SetLayerWeight(2, 1f);
			LocalPlayer.Animator.SetLayerWeight(3, 0f);
			LocalPlayer.Animator.SetLayerWeight(4, 0f);
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator lockLayersHanging()
	{
		for (;;)
		{
			LocalPlayer.Animator.SetLayerWeight(2, 0f);
			yield return null;
		}
		yield break;
	}

	
	public void setupFirstDayConditions()
	{
		this.Health = 28f;
		this.HealthTarget = 28f;
		this.Energy = 11f;
		this.Fullness = 0f;
		this.Starvation = 0f;
		this.StarvationCurrentDuration = this.StarvationSettings.Duration * 2f;
		TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(true);
		this.Thirst = 0.35f;
		TheForest.Utils.Scene.HudGui.Hydration.fillAmount = 1f - this.Thirst;
		this.IsBloody = true;
		this.PlayerVariations.UpdateSkinVariation(true, false, false, false);
	}

	
	public IEnumerator WakeFromKnockOut(bool wasDead, WaitForSeconds timer)
	{
		yield return timer;
		LocalPlayer.Animator.SetIntegerReflected("knockBackInt", 0);
		LocalPlayer.Animator.SetTrigger("resetTrigger");
		this.Health = 28f;
		this.HealthTarget = 28f;
		this.Energy = 11f;
		if (!wasDead)
		{
			this.Fullness = 0f;
		}
		this.Starvation = 0f;
		this.StarvationCurrentDuration = this.StarvationSettings.Duration * 2f;
		TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(true);
		this.Thirst = 0.35f;
		TheForest.Utils.Scene.HudGui.Hydration.fillAmount = 1f - this.Thirst;
		this.Atmos.FogMaxHeight = 400f;
		this.Atmos.TimeOfDay = 302f;
		TheForest.Utils.Scene.Atmosphere.ForceSunRotationUpdate = true;
		base.Invoke("CheckArmsStart", 2f);
		TheForest.Utils.Scene.HudGui.ShowHud(!this.doneDragScene);
		TheForest.Utils.Scene.Cams.SleepCam.SetActive(false);
		this.camFollow.followAnim = false;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.FpCharacter.UnLockView();
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.FpCharacter.CanJump = true;
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		this.IsBloody = true;
		this.PlayerVariations.UpdateSkinVariation(true, false, false, false);
		base.Invoke("PlayWakeMusic", 3f);
		yield break;
	}

	
	public void CheckArmsStart()
	{
		base.StartCoroutine(this.CheckArmsRoutine());
	}

	
	private IEnumerator CheckArmsRoutine()
	{
		if (!LocalPlayer.Inventory.IsLeftHandEmpty())
		{
			yield break;
		}
		LocalPlayer.Animator.SetBool("checkArms", true);
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.Animator.SetBool("checkArms", false);
		yield break;
	}

	
	public void PlayWakeMusic()
	{
		this.WakeMusic.SetActive(true);
	}

	
	private void switchToLighter()
	{
		LocalPlayer.Inventory.Equip(LocalPlayer.AnimControl._lighterId, false);
		LocalPlayer.Inventory.StashLeftHand();
	}

	
	private void BlackScreen()
	{
		if (!BoltNetwork.isRunning)
		{
			this.camFollow.followAnim = false;
			TheForest.Utils.Scene.HudGui.ShowHud(false);
			TheForest.Utils.Scene.Cams.SleepCam.SetActive(true);
			if (this.DeadTimes == 0 && !LocalPlayer.IsInCaves && (!TheForest.Utils.Scene.IsInSinkhole(LocalPlayer.Transform.position) || Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position) < LocalPlayer.Transform.position.y))
			{
				base.StartCoroutine(this.dragAwayCutScene());
			}
			else
			{
				if (this.doneHangingScene || LocalPlayer.IsInEndgame)
				{
					base.StartCoroutine(this.WakeFromKnockOut(this.Dead, YieldPresets.WaitThreeSeconds));
				}
				base.Invoke("KillPlayer", (this.DeadTimes <= 0) ? 3f : 0.5f);
			}
		}
		else
		{
			base.Invoke("KillPlayer", 1f);
		}
	}

	
	public void CheckCollisionFromAbove(Collision coll)
	{
		FoundationHealth componentInParent = coll.collider.GetComponentInParent<FoundationHealth>();
		if (componentInParent && componentInParent.Collapsing)
		{
			this.Hit(1000, true, PlayerStats.DamageType.Physical);
		}
		else if (!coll.collider.GetComponentInParent<PrefabIdentifier>() && coll.collider.CompareTag("structure"))
		{
			this.Hit((int)Mathf.Clamp(coll.relativeVelocity.sqrMagnitude / 10f, 3f, 10f), false, PlayerStats.DamageType.Physical);
		}
	}

	
	private void hitFallDown()
	{
		this.pmDamage.SendEvent("toHit");
		this.pm.SendEvent("toDeath");
		LocalPlayer.CamRotator.enabled = false;
		this.animator.SetBoolReflected("deathBool", true);
		this.camFollow.followAnim = true;
		float num = 5f;
		BleedBehavior.BloodAmount += Mathf.Clamp01(3f * num / this.Health) * 0.9f;
		BleedBehavior.BloodReductionRatio = (Mathf.Clamp01(this.Health / 100f) + 0.1f) * ((!this.IsHealthInGreyZone) ? 1f : 0.5f);
		base.Invoke("ResetHit", 1f);
	}

	
	private void HitFire()
	{
		this.Hit(Mathf.RoundToInt(4f * this.Flammable * GameSettings.Survival.FireDamageRatio), false, PlayerStats.DamageType.Fire);
		if (LocalPlayer.AnimControl.skinningAnimal)
		{
			LocalPlayer.SpecialActions.SendMessage("forceSkinningReset");
		}
		LocalPlayer.Animator.SetBool("skinAnimal", false);
	}

	
	private void Infection()
	{
	}

	
	private void Bleed()
	{
		if (this.animator.GetCurrentAnimatorStateInfo(0).tagHash == this.enterCaveHash)
		{
			return;
		}
		this.HealthChange(-1f);
		this.Hit(1, true, PlayerStats.DamageType.Physical);
	}

	
	private void HealedWounds()
	{
	}

	
	private void HealedInfections()
	{
		base.CancelInvoke("Infection");
	}

	
	private void HealedBleeding()
	{
		base.CancelInvoke("Bleed");
	}

	
	private void PoisonDamage()
	{
	}

	
	public void Poison()
	{
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		this.Hit(4, true, PlayerStats.DamageType.Physical);
		if (!this.Dead)
		{
			this.BloodInfection.GetInfected();
			base.InvokeRepeating("HitPoison", 4f, UnityEngine.Random.Range(6f, 8f));
			base.Invoke("disablePoison", UnityEngine.Random.Range(50f, 80f));
			Mood.PlayerGetPoisonned(true);
		}
	}

	
	private void disablePoison()
	{
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		Mood.PlayerGetPoisonned(false);
	}

	
	private void HitPoison()
	{
		int min = 2;
		int max = 4;
		int damage = UnityEngine.Random.Range(min, max);
		this.Hit(damage, true, PlayerStats.DamageType.Physical);
	}

	
	private void Burn()
	{
		if (this.animator.GetCurrentAnimatorStateInfo(0).tagHash == this.enterCaveHash || LocalPlayer.AnimControl.swimming)
		{
			return;
		}
		if (!this.IsLit && !this.Dead && LocalPlayer.WaterViz.ScreenCoverage < 0.5f)
		{
			LocalPlayer.AnimControl.onFire = true;
			this.IsLit = true;
			this.PlayerFlames.SetActive(true);
			base.InvokeRepeating("HitFire", 0f, 3f);
			base.Invoke("StopBurning", 10f);
		}
	}

	
	private void StopBurning()
	{
		LocalPlayer.AnimControl.onFire = false;
		this.PlayerFlames.SetActive(false);
		base.CancelInvoke("HitFire");
		if (this.IsLit)
		{
			this.IsLit = false;
			CapsuleCollider component = base.GetComponent<CapsuleCollider>();
			component.enabled = false;
			component.enabled = true;
		}
	}

	
	private void ResetHit()
	{
	}

	
	private void resetExplosion()
	{
		this.isExplode = false;
	}

	
	public void KillMeFast()
	{
		if (!BoltNetwork.isRunning)
		{
			TheForest.Utils.Scene.HudGui.ShowHud(false);
			TheForest.Utils.Scene.Cams.DeadCam.SetActive(true);
			LocalPlayer.PlayerDeadCam.SetActive(true);
			LocalPlayer.AnimControl.enabled = false;
			if (LocalPlayer.AnimControl.swimming)
			{
				LocalPlayer.Animator.CrossFade("fullBodyActions.swimDeath", 0f, 2, 1f);
			}
			else
			{
				LocalPlayer.Animator.CrossFade("fullBodyActions.deathFallForward", 0f, 2, 1f);
			}
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			base.Invoke("GameOver", 6f);
		}
		else
		{
			this.KillPlayer();
		}
	}

	
	private void KillPlayer()
	{
		base.CancelInvoke("HitPoison");
		base.CancelInvoke("disablePoison");
		base.CancelInvoke("HitFire");
		base.CancelInvoke("Bleed");
		base.CancelInvoke("Infection");
		base.CancelInvoke("CheckBlood");
		this.DeadTimes++;
		LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
		LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.LeftHand);
		LocalPlayer.Inventory.StashEquipedWeapon(false);
		LocalPlayer.Inventory.StashLeftHand();
		LocalPlayer.Animator.SetBoolReflected("deathBool", false);
		LocalPlayer.Create.CancelPlace();
		LocalPlayer.Tuts.CloseAll();
		this.OnSaveSlotSelectionCanceled();
		this.Player.TurnOffLastLight();
		this.Player.TurnOffLastUtility(Item.EquipmentSlot.LeftHand);
		if (this.Player.CurrentView == PlayerInventory.PlayerViews.Pause)
		{
			this.Player.TogglePauseMenu();
		}
		this.Player.Close();
		this.StopBurning();
		BleedBehavior.BloodReductionRatio = 3f;
		if (!BoltNetwork.isRunning)
		{
			if (LocalPlayer.IsInEndgame && LocalPlayer.Stats.IsFightingBoss)
			{
				base.StartCoroutine(this.EndgameWakeUp());
			}
			else if (this.DeadTimes > 1)
			{
				TheForest.Utils.Scene.HudGui.ShowHud(false);
				TheForest.Utils.Scene.Cams.DeadCam.SetActive(true);
				LocalPlayer.PlayerDeadCam.SetActive(true);
				if (Cheats.PermaDeath)
				{
					PlayerPrefsFile.DeleteKey("__RESUME__", true);
					PlayerPrefsFile.DeleteKey("__RESUME__prev", true);
					PlayerPrefsFile.DeleteKey("info", true);
					PlayerPrefsFile.DeleteKey("thumb.png", true);
				}
				base.Invoke("GameOver", 6f);
			}
			else
			{
				if (LocalPlayer.IsInEndgame)
				{
					GameObject gameObject = GameObject.FindWithTag("EndgameLoader");
					if (gameObject)
					{
						SceneLoadTrigger component = gameObject.GetComponent<SceneLoadTrigger>();
						component.ForceUnload();
					}
					EventRegistry.Player.Publish(TfEvent.ExitOverlookArea, null);
					EventRegistry.Player.Publish(TfEvent.ExitEndgame, null);
				}
				TerrainCollider component2 = Terrain.activeTerrain.GetComponent<TerrainCollider>();
				SphereCollider component3 = base.transform.GetComponent<SphereCollider>();
				CapsuleCollider component4 = base.transform.GetComponent<CapsuleCollider>();
				if (component2.enabled && component3.enabled)
				{
					Physics.IgnoreCollision(component2, component3, true);
				}
				if (component2.enabled && component4.enabled)
				{
					Physics.IgnoreCollision(component2, component4, true);
				}
				Rigidbody rigidbody = LocalPlayer.Rigidbody;
				Vector3 position = this.DSpots.DeadSpots[UnityEngine.Random.Range(0, this.DSpots.DeadSpots.Length)].position;
				LocalPlayer.Transform.position = position;
				rigidbody.position = position;
				LocalPlayer.FpCharacter.enabled = true;
				LocalPlayer.Create.Grabber.gameObject.SetActive(true);
				TheForest.Utils.Scene.Cams.CaveDeadCam.SetActive(true);
				this.camFollow.followAnim = false;
				this.hitReaction.Invoke("disableControllerFreeze", 4f);
				LocalPlayer.ActiveAreaInfo.SetCurrentCave(CaveNames.Cave02);
				this.InACave();
				this.Player.StashLeftHand();
				this.Player.StashEquipedWeapon(false);
				base.StartCoroutine(this.WakeInCave());
				this.FrostScript.coverage = 0f;
				this.SetCold(false);
				this.Atmos.TimeOfDay = 1f;
				TheForest.Utils.Scene.Atmosphere.ForceSunRotationUpdate = true;
				this.Starvation = 0f;
				this.StarvationCurrentDuration = this.StarvationSettings.Duration * 2f;
				TheForest.Utils.Scene.HudGui.StomachStarvation.gameObject.SetActive(true);
				this.Thirst = 0.35f;
				TheForest.Utils.Scene.HudGui.Hydration.fillAmount = 1f - this.Thirst;
			}
		}
		else
		{
			if (LocalPlayer.AnimControl.swimming)
			{
				PlayerRespawnMP.KillPlayer();
				return;
			}
			PlayerRespawnMP.enableRespawnTimer();
			if (this.DeadTimes > 1)
			{
				PlayerRespawnMP.KillPlayer();
			}
		}
	}

	
	public void HealedMp()
	{
		this.Dead = false;
		this.DeadTimes = 0;
		this.Health = (this.HealthTarget = (float)(this.GreyZoneThreshold + 1));
		Mood.PlayerLowHealth(false);
		LocalPlayer.CamFollowHead.followAnim = true;
		this.animator.SetBoolReflected("injuredBool", false);
		LocalPlayer.Sfx.PlayStaminaBreath();
		LocalPlayer.Create.Grabber.gameObject.SetActive(true);
		LocalPlayer.CamFollowHead.followAnim = true;
		base.StartCoroutine("resetPlayerFromHeal");
	}

	
	private IEnumerator resetPlayerFromHeal()
	{
		PlayerRespawnMP.offsetDeathTime();
		TheForest.Utils.Scene.HudGui.ShowHud(true);
		TheForest.Utils.Scene.Cams.DeadCam.SetActive(false);
		LocalPlayer.PlayerDeadCam.SetActive(false);
		TheForest.Utils.Scene.HudGui.MpRespawnLabel.gameObject.SetActive(false);
		TheForest.Utils.Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(false);
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
		yield return YieldPresets.WaitOneSecond;
		AnimatorStateInfo state2 = this.animator.GetCurrentAnimatorStateInfo(2);
		while (state2.tagHash == this.getupHash)
		{
			state2 = this.animator.GetCurrentAnimatorStateInfo(2);
			this.animator.SetLayerWeightReflected(4, 0f);
			yield return null;
		}
		UnityEngine.Debug.Log("inventory enabled 3928");
		LocalPlayer.Inventory.enabled = true;
		this.hitReaction.disableControllerFreeze();
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			LocalPlayer.FpCharacter.MovementLocked = false;
		}
		PlayerRespawnMP.Cancel();
		yield break;
	}

	
	private IEnumerator WakeInCave()
	{
		yield return YieldPresets.WaitThreeSeconds;
		TerrainCollider tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
		Physics.IgnoreCollision(tc, LocalPlayer.Transform.GetComponent<SphereCollider>(), true);
		Physics.IgnoreCollision(tc, LocalPlayer.Transform.GetComponent<CapsuleCollider>(), true);
		LocalPlayer.ScriptSetup.sceneInfo.EnableMusic();
		for (int i = 0; i < TheForest.Utils.Scene.SceneTracker.caveEntrances.Count; i++)
		{
			TheForest.Utils.Scene.SceneTracker.caveEntrances[i].disableCaveBlack();
		}
		if (this.Ocean)
		{
			this.Ocean.SetActive(false);
		}
		this.Dead = false;
		this.Player.enabled = true;
		this.pm.SendEvent("toResetPlayer");
		this.animator.SetTriggerReflected("resetTrigger");
		base.StartCoroutine(this.hangingInCaveCutScene());
		yield break;
	}

	
	private void GameOver()
	{
		TheForest.Utils.Scene.Cams.DeadCam.SetActive(false);
		LocalPlayer.PlayerDeadCam.SetActive(false);
		SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
	}

	
	private void GetTired()
	{
		if (this.Player.Logs.Amount == 1)
		{
			this.LogWeight = 0.15f;
		}
		else if (this.Player.Logs.Amount == 2)
		{
			this.LogWeight = 0.25f;
		}
		else
		{
			this.LogWeight = 0f;
		}
		this.Energy -= this.LogWeight;
		if (this.Fullness < 0.2f)
		{
			this.Energy -= 0.5f;
		}
		if (this.IsCold)
		{
			this.Energy -= 0.1f + 0.1f * (1f - this.ColdArmor);
		}
		else
		{
			this.Energy -= 0.1f;
		}
	}

	
	private void CallBird()
	{
		this.pm.SendEvent("toOnHand");
	}

	
	private void EnableMpCaveMutants()
	{
		this.mutantControl.SendMessage("enableMpCaveMutants");
	}

	
	public void IgnoreCollisionWithTerrain(bool onoff)
	{
		TerrainCollider terrainCollider = null;
		if (Terrain.activeTerrain)
		{
			terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
		}
		SphereCollider component = base.transform.GetComponent<SphereCollider>();
		CapsuleCollider component2 = base.transform.GetComponent<CapsuleCollider>();
		if (!terrainCollider || !component || !component2)
		{
			return;
		}
		if (component.enabled)
		{
			Physics.IgnoreCollision(terrainCollider, component, onoff);
		}
		if (component2.enabled)
		{
			Physics.IgnoreCollision(terrainCollider, component2, onoff);
		}
	}

	
	public void InACave()
	{
		TheForest.Utils.Scene.Clock.IsCave();
		if (this.Ocean)
		{
			this.Ocean.SetActive(false);
		}
		this.SetInCave(true);
		if (!this.sceneInfo.allPlayersInCave.Contains(base.gameObject))
		{
			this.sceneInfo.allPlayersInCave.Add(base.gameObject);
		}
		if (BoltNetwork.isServer)
		{
			if (this.mutantControl.gameObject.activeSelf)
			{
				this.EnableMpCaveMutants();
			}
			else
			{
				base.Invoke("EnableMpCaveMutants", 10f);
			}
		}
		else if (SteamClientDSConfig.isDedicatedClient)
		{
			playerInCave playerInCave = playerInCave.Create(GlobalTargets.OnlyServer);
			playerInCave.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
			playerInCave.inCave = true;
			playerInCave.Send();
		}
		else if (this.mutantControl && this.mutantControl.gameObject.activeSelf)
		{
			this.mutantControl.StartCoroutine("updateCaveSpawns");
			base.Invoke("doRemoveWorldMutants", 30f);
			this.delayedMutantSpawnCheck = true;
		}
		this.IgnoreCollisionWithTerrain(true);
		CaveTriggers.CheckPlayersInCave();
	}

	
	private void doRemoveWorldMutants()
	{
		TheForest.Utils.Scene.MutantControler.StartCoroutine("removeWorldMutants");
		this.delayedMutantSpawnCheck = false;
	}

	
	public void NotInACave()
	{
		TheForest.Utils.Scene.Clock.IsNotCave();
		if (this.Ocean)
		{
			this.Ocean.SetActive(true);
		}
		this.SetInCave(false);
		if (this.sceneInfo.allPlayersInCave.Contains(base.gameObject))
		{
			this.sceneInfo.allPlayersInCave.Remove(base.gameObject);
		}
		if (BoltNetwork.isServer)
		{
			if (this.sceneInfo.allPlayersInCave.Count == 0)
			{
				this.mutantControl.disableMpCaveMutants();
			}
		}
		else if (SteamClientDSConfig.isDedicatedClient)
		{
			playerInCave playerInCave = playerInCave.Create(GlobalTargets.OnlyServer);
			playerInCave.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
			playerInCave.inCave = false;
			playerInCave.Send();
		}
		else if (this.mutantControl)
		{
			if (this.delayedMutantSpawnCheck)
			{
				this.mutantControl.StartCoroutine("removeCaveMutants");
				base.CancelInvoke("doRemoveWorldMutants");
			}
			else
			{
				this.mutantControl.startSetupFamilies();
			}
		}
		this.IgnoreCollisionWithTerrain(false);
		CaveTriggers.CheckPlayersInCave();
	}

	
	private void Life()
	{
		if (TheForest.Utils.Scene.WeatherSystem == null)
		{
			return;
		}
		if (!LocalPlayer.IsInCaves && TheForest.Utils.Scene.WeatherSystem.Raining && !this.Warm && Clock.Dark)
		{
			this.SetCold(true);
		}
		if (this.Warm)
		{
			this.Tuts.CloseColdTut();
			if (this.FrostScript.coverage <= 0f && this.IsCold)
			{
				this.SetCold(false);
			}
			this.coldSwitch = false;
		}
		if ((this.IsCold || this.IsInNorthColdArea()) && !LocalPlayer.IsInEndgame && this.IsCold && !this.Warm)
		{
			this.Tuts.ColdTut();
			this.coldSwitch = true;
		}
		if (!Clock.Dark && !TheForest.Utils.Scene.WeatherSystem.Raining && !LocalPlayer.IsInCaves && !this.IsInNorthColdArea())
		{
			if (!this.SunWarmth)
			{
				base.Invoke("WarmDay", 10f);
			}
		}
		else
		{
			this.SunWarmth = false;
		}
		this.Skills.CalcSkills();
		this.FoodPoisoning.TryAutoHeal();
		this.BloodInfection.TryAutoHeal();
	}

	
	private void WarmDay()
	{
		this.SunWarmth = true;
	}

	
	private void Infected()
	{
	}

	
	private void GetHealth()
	{
		base.SendMessage("PlayWhoosh");
		this.Health = (this.HealthTarget = 60f);
		this.CheckStats();
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
	}

	
	public void Drink()
	{
		this.Energy += 80f;
		this.Hunger -= 100;
		base.SendMessage("PlayDrink");
		this.CheckStats();
	}

	
	public void DrinkBooze()
	{
		this.Energy += 80f;
		base.SendMessage("PlayDrink");
		this.CheckStats();
	}

	
	private void DrinkLake()
	{
		base.SendMessage("PlayDrink");
		this.CheckStats();
	}

	
	public void AteChocBar()
	{
		this.Fullness += 0.2f * GameSettings.Survival.FullnessMealReplenishmentRatio;
		this.HealthChange(20f * GameSettings.Survival.FullnessMealReplenishmentRatio);
		this.Energy += 80f * GameSettings.Survival.EnergyMealReplenishmentRatio;
		base.SendMessage("PlayEat");
		BleedBehavior.BloodReductionRatio = this.Health / 100f * 1.25f;
	}

	
	public void SitDown()
	{
		this.Sitted = true;
	}

	
	public void StandUp()
	{
		this.Sitted = false;
	}

	
	private void AteMealRabbit()
	{
		this.Fullness += 0.9f;
		this.HealthChange(20f);
		this.Energy += 80f;
		base.SendMessage("PlayEatMeat");
		BleedBehavior.BloodReductionRatio = this.Health / 100f * 1.25f;
	}

	
	private void AteMushroomLibertyCap()
	{
		this.Tuts.ShowMushroomPage();
		this.Fullness += 0.02f;
		this.HealthChange(5f);
		this.Energy += 4f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.SendMessage("PlayEat");
	}

	
	private void AteMushroomChant()
	{
		this.Tuts.ShowMushroomPage();
		this.Fullness += 0.02f;
		this.HealthChange(5f);
		this.Energy += 4f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.SendMessage("PlayEat");
	}

	
	private void AteMushroomDeer()
	{
		this.Tuts.ShowMushroomPage();
		this.Fullness += 0.02f;
		this.HealthChange(4f);
		this.Energy += 3f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.SendMessage("PlayEat");
	}

	
	private void AteMushroomAman()
	{
		this.Tuts.ShowMushroomPage();
		this.Fullness += 0.02f;
		this.Energy += 1f;
		base.SendMessage("PlayEat");
		this.PoisonMe();
	}

	
	private void AteMushroomPuffMush()
	{
		this.Tuts.ShowMushroomPage();
		this.Fullness += 0.02f;
		this.HealthChange(2f);
		this.Energy += 2f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.SendMessage("PlayEat");
	}

	
	private void AteMushroomJack()
	{
		this.Tuts.ShowMushroomPage();
		this.Fullness += 0.02f;
		this.HealthChange(3f);
		this.Energy += 3f;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.SendMessage("PlayEat");
	}

	
	private void AteBlueBerry()
	{
		this.Fullness += 0.02f;
		this.HealthChange(1f);
		this.Energy += 4f;
		base.SendMessage("PlayEat");
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
	}

	
	private void AteTwinBerry()
	{
		this.Fullness += 0.02f;
		base.Invoke("HitFood", 1.5f);
	}

	
	public void HitFood()
	{
		this.Hit(2 * GameSettings.Survival.HitFoodDamageRatio, true, PlayerStats.DamageType.Poison);
	}

	
	public void HitFoodDelayed(int damage)
	{
		base.StartCoroutine(this.HitFoodRoutine(damage));
	}

	
	private IEnumerator HitFoodRoutine(int damage)
	{
		yield return YieldPresets.WaitTwoSeconds;
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		this.Hit(damage * GameSettings.Survival.HitFoodDamageRatio, true, PlayerStats.DamageType.Poison);
		yield break;
	}

	
	public void HitWaterDelayed(int damage)
	{
		base.StartCoroutine(this.HitWaterRoutine(damage));
	}

	
	private IEnumerator HitWaterRoutine(int damage)
	{
		yield return YieldPresets.WaitTwoSeconds;
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		this.Hit(Mathf.CeilToInt((float)damage * GameSettings.Survival.PolutedWaterDamageRatio), true, PlayerStats.DamageType.Poison);
		yield break;
	}

	
	private void AtePlaneFood()
	{
		this.Fullness += 0.45f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessMealReplenishmentRatio;
		this.HealthChange(5f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.HealthMealReplenishmentRatio);
		this.Energy += 30f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyMealReplenishmentRatio;
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.SendMessage("PlayEat");
		this.Sanity.OnAteFreshFood();
	}

	
	public void AteCustom(float fullness, float health, float energy, bool isFresh, bool isMeat, bool isLimb, int calories)
	{
		if (isLimb)
		{
			this.Fullness += fullness * 0.5f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessLimbReplenishmentRatio;
			this.HealthChange(health * this.FoodPoisoning.EffectRatio * GameSettings.Survival.HealthLimbReplenishmentRatio);
			this.Energy += energy * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyLimbReplenishmentRatio;
			BleedBehavior.BloodReductionRatio = this.Health / 100f;
			base.Invoke("HitFood", 2f);
			this.Sanity.OnCannibalism();
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteLimb, null);
		}
		else
		{
			this.Fullness += fullness * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessMealReplenishmentRatio;
			this.HealthChange(health * this.FoodPoisoning.EffectRatio * GameSettings.Survival.HealthMealReplenishmentRatio);
			this.Energy += energy * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyMealReplenishmentRatio;
			BleedBehavior.BloodReductionRatio = this.Health / 100f * 1.25f;
			if (isFresh && isMeat)
			{
				this.Sanity.OnAteFreshFood();
			}
		}
		if (isMeat)
		{
			this.Calories.OnAteFood(calories);
			base.Invoke("sendPlayEatMeat", 0.5f);
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteMeat, null);
		}
	}

	
	public void AteFreshMeat(bool isLimb, float size, int calories)
	{
		if (!isLimb)
		{
			this.Fullness += 0.8f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.FullnessMealReplenishmentRatio;
			this.HealthChange(20f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.HealthMealReplenishmentRatio);
			this.Energy += 80f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.EnergyMealReplenishmentRatio;
			BleedBehavior.BloodReductionRatio = this.Health / 100f * 1.25f;
			this.Sanity.OnAteFreshFood();
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteMeat, null);
		}
		else
		{
			this.Fullness += 0.4f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessLimbReplenishmentRatio;
			this.HealthChange(20f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.HealthLimbReplenishmentRatio);
			this.Energy += 80f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyLimbReplenishmentRatio;
			base.Invoke("HitFood", 2f);
			BleedBehavior.BloodReductionRatio = this.Health / 100f;
			this.Sanity.OnCannibalism();
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteLimb, null);
		}
		this.Calories.OnAteFood(calories);
		base.Invoke("sendPlayEatMeat", 0.5f);
	}

	
	public void AteEdibleMeat(bool isLimb, float size, int calories)
	{
		if (!isLimb)
		{
			this.Fullness += 0.5f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.FullnessMealReplenishmentRatio;
			this.HealthChange(0f);
			this.Energy += 40f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.EnergyMealReplenishmentRatio;
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteMeat, null);
		}
		else
		{
			this.Fullness += 0.25f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessLimbReplenishmentRatio;
			this.HealthChange(0f * this.FoodPoisoning.EffectRatio);
			this.Energy += 40f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyLimbReplenishmentRatio;
			base.Invoke("HitFood", 2f);
			this.Sanity.OnCannibalism();
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteLimb, null);
		}
		this.Calories.OnAteFood(calories);
		BleedBehavior.BloodReductionRatio = this.Health / 100f;
		base.Invoke("sendPlayEatMeat", 0.5f);
	}

	
	public void AteSpoiltMeat(bool isLimb, float size, int calories)
	{
		if (!isLimb)
		{
			this.Fullness += 0.35f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.FullnessMealReplenishmentRatio;
			this.HealthChange(0f * this.FoodPoisoning.EffectRatio * size);
			this.Energy += 20f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.EnergyMealReplenishmentRatio;
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteMeat, null);
		}
		else
		{
			this.Fullness += 0.175f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessLimbReplenishmentRatio;
			this.HealthChange(0f * this.FoodPoisoning.EffectRatio);
			this.Energy += 20f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyLimbReplenishmentRatio;
			this.Sanity.OnCannibalism();
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteLimb, null);
		}
		this.Calories.OnAteFood(calories);
		BleedBehavior.BloodReductionRatio = this.Health / 100f * 0.75f;
		base.Invoke("sendPlayEatMeat", 0.5f);
		base.Invoke("HitFood", 2f);
	}

	
	public void AteBurnt(bool isLimb, float size, int calories)
	{
		if (!isLimb)
		{
			this.Fullness += 0.15f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.FullnessMealReplenishmentRatio;
			this.HealthChange(0f * this.FoodPoisoning.EffectRatio * size);
			this.Energy += 10f * this.FoodPoisoning.EffectRatio * size * GameSettings.Survival.EnergyMealReplenishmentRatio;
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteMeat, null);
		}
		else
		{
			this.Fullness += 0.075f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.FullnessLimbReplenishmentRatio;
			this.HealthChange(0f * this.FoodPoisoning.EffectRatio);
			this.Energy += 10f * this.FoodPoisoning.EffectRatio * GameSettings.Survival.EnergyLimbReplenishmentRatio;
			base.Invoke("HitFood", 2f);
			this.Sanity.OnCannibalism();
			EventRegistry.Achievements.Publish(TfEvent.Achievements.AteLimb, null);
		}
		this.Calories.OnAteFood(calories);
		BleedBehavior.BloodReductionRatio = this.Health / 100f * 0.5f;
		base.Invoke("sendPlayEatMeat", 0.5f);
	}

	
	private void sendPlayEatMeat()
	{
		base.SendMessage("PlayEatMeat");
	}

	
	private void SpiderBite()
	{
		this.PoisonMe();
	}

	
	public void PoisonMe()
	{
		this.Hit(Mathf.CeilToInt(2f * GameSettings.Survival.PoisonDamageRatio), true, PlayerStats.DamageType.Physical);
	}

	
	public void playerLoadedFromRespawn()
	{
		this.reloadedFromRespawn = true;
	}

	
	private Vector2 Circle2(float radius)
	{
		Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
		insideUnitCircle.Normalize();
		return insideUnitCircle * radius;
	}

	
	private IEnumerator ResetStamina()
	{
		base.SendMessage("PlayStaminaBreath");
		if (this.Tired < 40f)
		{
			yield return YieldPresets.WaitFifteenSeconds;
		}
		else if (this.Tired > 40f)
		{
			yield return YieldPresets.WaitThirtySeconds;
		}
		base.SendMessage("StopStaminaBreath");
		if (this.Tired < 40f)
		{
			base.SendMessage("ResetTiredValue");
		}
		else
		{
			base.SendMessage("ResetTiredValueSleepy");
		}
		yield break;
	}

	
	public void UseRebreather(bool onoff)
	{
		this.AirBreathing.UseRebreather = onoff;
		this.AirBreathing.RebreatherIsEquipped = onoff;
		if (this.AirBreathing.UseRebreather)
		{
			if (this.AirBreathing.CurrentRebreatherAir < this.AirBreathing.CurrentLungAir)
			{
				this.AirBreathing.CurrentRebreatherAir = this.AirBreathing.CurrentLungAir;
			}
		}
		else if (this.AirBreathing.CurrentLungAir < this.AirBreathing.CurrentRebreatherAir)
		{
			this.AirBreathing.CurrentLungAir = Mathf.Min(this.AirBreathing.CurrentRebreatherAir, this.AirBreathing.MaxLungAirCapacityFinal);
		}
	}

	
	public void equipCutSceneAxe()
	{
		LocalPlayer.Inventory.AddItem(LocalPlayer.AnimControl._planeAxeId, 1, false, false, null);
		LocalPlayer.Inventory.Equip(LocalPlayer.AnimControl._planeAxeId, false);
	}

	
	
	public bool IsBurning
	{
		get
		{
			return this.IsLit;
		}
	}

	
	
	
	public bool IsFightingBoss { get; set; }

	
	[Header("Vitals")]
	public float BodyTemp = 37f;

	
	public int HeartRate = 70;

	
	public int GreyZoneThreshold = 10;

	
	[SerializeThis]
	public float Stamina = 100f;

	
	[SerializeThis]
	public float Health;

	
	[SerializeThis]
	public float HealthTarget;

	
	[SerializeThis]
	public float Energy = 100f;

	
	[SerializeThis]
	public int Armor;

	
	[SerializeThis]
	public PlayerStats.ArmorTypes[] CurrentArmorTypes;

	
	[SerializeThis]
	private int[] CurrentArmorHP;

	
	[SerializeThis]
	public int ArmorVis;

	
	[Range(0f, 1f)]
	[SerializeThis]
	public float ColdArmor;

	
	[SerializeThis]
	public float BatteryCharge = 100f;

	
	[SerializeThis]
	public float Stealth;

	
	[SerializeThis]
	public float SoundRangeDampFactor = 1f;

	
	[SerializeThis]
	public float Flammable = 1f;

	
	[SerializeThis]
	public float DaySurvived = -1f;

	
	[SerializeThis]
	public float NextSleepTime = 0.5f;

	
	public bool Dead;

	
	[SerializeThis]
	private bool doneHangingScene;

	
	[Header("Survival")]
	[SerializeThis]
	public float Fullness;

	
	[SerializeThis]
	public float Thirst;

	
	[SerializeThis]
	public float Starvation;

	
	[SerializeThis]
	public float StarvationCurrentDuration = 180f;

	
	public float ThirstCurrentDuration = 10f;

	
	public PlayerStats.StarvationSettingsData StarvationSettings;

	
	public PlayerStats.ThirstSettingsData ThirstSettings;

	
	public PlayerStats.FrostSettingsData FrostDamageSettings;

	
	[SerializeThis]
	public PlayerStats.AirBreathingData AirBreathing;

	
	[SerializeThis]
	public PlayerStats.FuelData Fuel;

	
	public PlayerStats.CarriedWeightData CarriedWeight;

	
	[SerializeThis]
	public PlayerStats.hairsprayFuelData hairSprayFuel;

	
	[SerializeThis]
	public PlayerStats.SkillData Skills;

	
	[SerializeThis]
	public PlayerStats.SanityData Sanity;

	
	[SerializeThis]
	public PlayerStats.PhysicalStrengthData PhysicalStrength;

	
	[SerializeThis]
	public PlayerStats.CaloriesSystemData Calories;

	
	[SerializeThis]
	public PlayerStats.InfectionData FoodPoisoning;

	
	[SerializeThis]
	public PlayerStats.InfectionData BloodInfection;

	
	[SerializeThis]
	public int PedometerSteps;

	
	[Header("Armor")]
	public PlayerStats.ArmorSet[] ArmorSets;

	
	public GameObject[] ArmorModel;

	
	public GameObject[] LeafArmorModel;

	
	public GameObject[] BoneArmorModel;

	
	public GameObject[] CreepyArmorModel;

	
	public GameObject WarmsuitModel;

	
	[Header("Variations")]
	[Space(20f)]
	public Renderer MyBody;

	
	public Renderer[] clothes;

	
	[Header("Effects")]
	public GameObject WakeMusic;

	
	public GameObject PlayerFlames;

	
	[Header("Rumble")]
	public float HitRumbleStrength = 1f;

	
	public float HitRumbleDuration = 0.1f;

	
	[Header("FMOD")]
	public string GaspForAirEvent;

	
	public string RebreatherEvent;

	
	public string DrowningEvent;

	
	public string DyingEvent;

	
	public string DragCutsceneEvent;

	
	public string ExtinguishEvent;

	
	public string ApplyRedPaintEvent;

	
	public string ApplyMudEvent;

	
	private PlayerInventory Player;

	
	private int bloodDice;

	
	private mutantController mutantControl;

	
	private sceneTracker sceneInfo;

	
	private bool ShouldCheckArmor;

	
	private bool IsLit;

	
	private bool gotControlRefs;

	
	private bool Run;

	
	private bool Asleep;

	
	private bool IsTired;

	
	private bool Cold;

	
	private float CaveStartSwimmingTime;

	
	private bool ShouldDoWetColdRoll;

	
	private bool ShouldDoGotCleanCheck;

	
	private bool SunWarmth;

	
	private bool FireWarmth;

	
	private int BuildingWarmth;

	
	private bool Recharge;

	
	private bool CheckingBlood;

	
	private bool Sitted;

	
	private bool isExplode;

	
	private bool doneDragScene;

	
	private bool gotCutSceneAxe;

	
	private float trapHitCoolDown;

	
	private int Hunger = 3000;

	
	private int InfectionChance;

	
	private int BleedChance;

	
	private int Ate;

	
	private int ColdAmt;

	
	private Color HealthCurrentColor;

	
	private Color StaminaCurrentColor;

	
	private Color EnergyBackingCurrentColor;

	
	private Color EnergyCurrentColor;

	
	private float NextAdrenalineRush;

	
	private float ArmorResult;

	
	private float ColdArmorResult;

	
	private float HealthResult;

	
	private float HealthTargetResult;

	
	private float StaminaResult;

	
	private float EnergyResult;

	
	private float Tired;

	
	private float EnergyEx;

	
	private int DeadTimes;

	
	private float EnergyIconTemp;

	
	private float coldFloatBlend;

	
	public bool coldSwitch;

	
	private bool reloadedFromRespawn;

	
	public LayerMask dragAwayCullingMask;

	
	private TheForestAtmosphere Atmos;

	
	private PlayerTuts Tuts;

	
	private PlayerSfx Sfx;

	
	private GameObject Ocean;

	
	private enemyWeaponMelee currentAttacker;

	
	private Grayscale DyingVision;

	
	private PlayMakerFSM pmDamage;

	
	private PlayMakerFSM pmControl;

	
	private PlayMakerFSM pm;

	
	private Animator animator;

	
	private playerHitReactions hitReaction;

	
	private camFollowHead camFollow;

	
	private FsmFloat fsmStamina;

	
	private FsmFloat fsmMaxStamina;

	
	private HudGui Hud;

	
	private DeadSpotController DSpots;

	
	private FMOD.Studio.EventInstance SurfaceSnapshot;

	
	private FMOD.Studio.EventInstance CaveSnapshot;

	
	private FMOD.Studio.EventInstance CaveReverbSnapshot;

	
	private FMOD.Studio.EventInstance DyingEventInstance;

	
	private ParameterInstance DyingHealthParameter;

	
	private FMOD.Studio.EventInstance RebreatherEventInstance;

	
	private ParameterInstance RebreatherDepthParameter;

	
	private FMOD.Studio.EventInstance DrowningEventInstance;

	
	private FMOD.Studio.EventInstance FireExtinguishEventInstance;

	
	private GameObject[] CaveDoors;

	
	public GameObject sticksGreeble;

	
	private static List<GameObject> EndgameCavePortals = new List<GameObject>();

	
	private float LogWeight;

	
	private int explodeHash;

	
	private int getupHash = Animator.StringToHash("getup");

	
	private int enterCaveHash = Animator.StringToHash("enterCave");

	
	private int eatMeatHash = Animator.StringToHash("eatMeat");

	
	public float blockDamagePercent;

	
	private MaterialPropertyBlock bloodPropertyBlock;

	
	private CoopPlayerVariations PlayerVariations;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariation;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariationBody;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariationTShirtType;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariationTShirtMat;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariationPantsType;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariationPantsMat;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerVariationHair;

	
	[SerializeThis]
	[HideInInspector]
	public PlayerCloting PlayerVariationExtras;

	
	[SerializeThis]
	[HideInInspector]
	public int PlayerClothingVariation;

	
	private const int ArmorSlots = 10;

	
	private Coroutine checkItemRoutine;

	
	private GameObject mutant1;

	
	private GameObject mutant2;

	
	private bool delayedMutantSpawnCheck;

	
	public enum DamageType
	{
		
		Physical,
		
		Poison,
		
		Drowning,
		
		Fire,
		
		Frost
	}

	
	[Flags]
	public enum ArmorTypes
	{
		
		None = 0,
		
		LizardSkin = 1,
		
		DeerSkin = 2,
		
		Leaves = 4,
		
		Bone = 8,
		
		Creepy = 9,
		
		Warmsuit = 16
	}

	
	[Serializable]
	public class ArmorSet
	{
		
		public PlayerStats.ArmorTypes Type;

		
		public PlayerStats.ArmorTypes ModelType = PlayerStats.ArmorTypes.LizardSkin;

		
		public Material Mat;

		
		public PlayerStats.ArmorTypes ModelType2;

		
		public Material Mat2;

		
		public Color NibbleColor;

		
		[ItemIdPicker(Item.Types.Edible)]
		public int ItemId;

		
		public int HP;

		
		[NameFromProperty("_type", 0)]
		public StatEffect[] Effects;
	}

	
	[Serializable]
	public class StarvationSettingsData
	{
		
		public int StartDay;

		
		public int Damage = 10;

		
		[UnityEngine.Tooltip("Duration in game time days")]
		public float Duration = 2f;

		
		public float DurationDecay = 0.3f;

		
		public bool TakingDamage;

		
		public float SleepingFullnessThreshold = 0.4f;
	}

	
	[Serializable]
	public class ThirstSettingsData
	{
		
		public int StartDay;

		
		[UnityEngine.Tooltip("Duration in game time days")]
		public float Duration = 1f;

		
		public int Damage = 2;

		
		[UnityEngine.Tooltip("Duration in real life seconds")]
		public float DamageDelay = 8f;

		
		public float TutorialThreshold;

		
		public bool TakingDamage;

		
		public float SleepingThirstThreshold = 0.6f;
	}

	
	[Serializable]
	public class FrostSettingsData
	{
		
		public int StartDay;

		
		[UnityEngine.Tooltip("Duration standing still at max cold before damage kicks in")]
		public float Duration = 8f;

		
		[UnityEngine.Tooltip("Current time in seconds that player has been standing still at max cold")]
		public float CurrentTimer;

		
		[UnityEngine.Tooltip("Total damage per game day")]
		public int Damage = 5;

		
		public RandomRange DamageChance = new RandomRange
		{
			_min = 0,
			_max = 15
		};

		
		public bool TakingDamage;

		
		[UnityEngine.Tooltip("Cold value at which de-frost stops and returns to normal cold routine")]
		public float DeFrostThreshold = 0.492f;

		
		[UnityEngine.Tooltip("Time it takes for full screen frost to fade off after taking cold damage")]
		public float DeFrostDuration = 1f;

		
		public bool DoDeFrost;

		
		[UnityEngine.Tooltip("in game day time")]
		public float NextCheckArms;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class AirBreathingData
	{
		
		
		public float MaxLungAirCapacityFinal
		{
			get
			{
				return this.MaxLungAirCapacity * GameSettings.Survival.LungMaxCapacityRatio;
			}
		}

		
		
		public float CurrentAirPercent
		{
			get
			{
				return (this.CurrentLungAir - (float)this.CurrentLungAirTimer.Elapsed.TotalSeconds * LocalPlayer.Stats.Skills.LungBreathingRatio) / this.MaxLungAirCapacity;
			}
		}

		
		
		
		public float DamageCounter { get; set; }

		
		[UnityEngine.Tooltip("The % of screen covered by water before air breathing countdown starts")]
		public float ScreenCoverageThreshold = 0.5f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		public float MaxLungAirCapacity = 25f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		public float CurrentLungAir = 25f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		public float MaxRebreatherAirCapacity = 300f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		[SerializeThis]
		public float CurrentRebreatherAir;

		
		public Stopwatch CurrentLungAirTimer = new Stopwatch();

		
		public bool UseRebreather;

		
		public bool RebreatherIsEquipped;

		
		[UnityEngine.Tooltip("In real life seconds")]
		public float OutOfAirWarningThreshold = 30f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		public int Damage = 10;

		
		public bool TakingDamage;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class hairsprayFuelData
	{
		
		
		
		[SerializeThis]
		public float CurrentFuel
		{
			get
			{
				return this._currentFuel;
			}
			set
			{
				this._currentFuel = ((!Cheats.UnlimitedHairspray) ? value : this.MaxFuelCapacity);
			}
		}

		
		[UnityEngine.Tooltip("In real life seconds")]
		public float MaxFuelCapacity = 30f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		private float _currentFuel = 30f;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class FuelData
	{
		
		[UnityEngine.Tooltip("In real life seconds")]
		public float MaxFuelCapacity = 120f;

		
		[UnityEngine.Tooltip("In real life seconds")]
		[SerializeThis]
		public float CurrentFuel = 120f;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class CarriedWeightData
	{
		
		
		public float MaxWeight
		{
			get
			{
				return 1f;
			}
		}

		
		public float CurrentWeight;

		
		public string WeightUnit = "lb";

		
		public float WeightToUnitRatio = 120f;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class PhysicalStrengthData
	{
		
		
		public float CurrentStrengthScaled
		{
			get
			{
				return this.CurrentStrength * LocalPlayer.Stats.BloodInfection.EffectRatio;
			}
		}

		
		
		public float CurrentStrengthUnscaled
		{
			get
			{
				return this.CurrentStrength;
			}
		}

		
		
		public float PreviousStrengthUnscaled
		{
			get
			{
				return this.PreviousStrength;
			}
		}

		
		
		public float CurrentWeightScaled
		{
			get
			{
				return this.CurrentWeight;
			}
		}

		
		public void Initialize()
		{
			if (LocalPlayer.Stats.DaySurvived < 0.01f)
			{
				this.CurrentStrength = this.StartStrength;
				this.PreviousStrength = this.StartStrength;
				this.CurrentWeight = this.BaseWeight;
			}
			else if (this.CurrentStrength < this.MinStrength)
			{
				this.CurrentStrength = this.MinStrength;
				this.PreviousStrength = this.MinStrength;
			}
			this.DailyStartStrength = this.CurrentStrength;
			this.DailyStartWeight = this.CurrentWeight;
			this.LastStrengthUpdateDay = Mathf.FloorToInt(LocalPlayer.Stats.DaySurvived * 100f);
		}

		
		public void StrengthChange(float value)
		{
			this.PreviousStrength = this.CurrentStrength;
			this.CurrentStrength = Mathf.Clamp(this.CurrentStrength + value, Mathf.Max(this.MinStrength, this.DailyStartStrength - 2f), Mathf.Min(this.MaxStrength, this.DailyStartStrength + 2f));
		}

		
		public void WeightChange(float value)
		{
			this.CurrentWeight = Mathf.Clamp(this.CurrentWeight + value, Mathf.Max(this.MinWeight, this.DailyStartWeight - 2f), Mathf.Min(this.MaxWeight, this.DailyStartWeight + 2f));
		}

		
		[Header("Strength")]
		public float StartStrength = 20f;

		
		public float MinStrength = 10f;

		
		public float MaxStrength = 99f;

		
		[Header("Weight")]
		public float BaseWeight = 205f;

		
		public float MinWeight = 155f;

		
		public float MaxWeight = 305f;

		
		[Header("Runtime data")]
		[SerializeThis]
		public int LastStrengthUpdateDay;

		
		[SerializeThis]
		public float CurrentStrength;

		
		[SerializeThis]
		public float PreviousStrength;

		
		[SerializeThis]
		public float CurrentWeight;

		
		[SerializeThis]
		public float CurrentCutDownTreeCount;

		
		[SerializeThis]
		public float CurrentCaloriesBurntCount;

		
		[SerializeThis]
		public float LastMealTime;

		
		[SerializeThis]
		public bool IsHungry;

		
		[SerializeThis]
		public float GoodNutritionPoints;

		
		[SerializeThis]
		public float OvereatingPoints;

		
		[SerializeThis]
		public float UndereatingPoints;

		
		[SerializeThis]
		public float DailyStartStrength;

		
		[SerializeThis]
		public float DailyStartWeight;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class CaloriesSystemData
	{
		
		public void Initialize()
		{
			if (LocalPlayer.Stats.DaySurvived < 0.01f)
			{
			}
			this.NextResolutionTime = this.GetNextResolutionTime();
		}

		
		public void Refresh()
		{
			if (Time.timeScale > 0f)
			{
				if (GameSetup.IsHardSurvivalMode)
				{
					if (LocalPlayer.FpCharacter.jumping && !LocalPlayer.FpCharacter.Sitting)
					{
						this.CaloriesBurntChange(this.JumpingCaloriesPerSecond * Time.deltaTime);
					}
					if (LocalPlayer.FpCharacter.swimming && LocalPlayer.Stats.Stamina > 0f && LocalPlayer.Rigidbody.velocity.sqrMagnitude > 1f)
					{
						this.CaloriesBurntChange((this.SwimmingCaloriesPerSecond + ((!LocalPlayer.FpCharacter.running) ? 0f : this.RunningCaloriesPerSecond)) * Time.deltaTime);
					}
					else if (LocalPlayer.FpCharacter.running && LocalPlayer.Stats.Stamina > 0f && LocalPlayer.Rigidbody.velocity.sqrMagnitude > 1f)
					{
						this.CaloriesBurntChange((this.RunningCaloriesPerSecond + (float)LocalPlayer.Inventory.Logs.Amount * 1.5f * this.CarryLogCaloriesPerSecond) * Time.deltaTime);
					}
					else if (LocalPlayer.FpCharacter.Grounded && LocalPlayer.Rigidbody.velocity.sqrMagnitude > 0.3f)
					{
						this.CaloriesBurntChange((this.WalkingCaloriesPerSecond + (float)LocalPlayer.Inventory.Logs.Amount * this.CarryLogCaloriesPerSecond) * Time.deltaTime);
					}
					else if (LocalPlayer.FpCharacter.Grounded)
					{
						this.CaloriesBurntChange(this.StationnaryCaloriesPerSecond * Time.deltaTime);
					}
				}
				if (this.NextResolutionTime == -1)
				{
					this.NextResolutionTime = this.GetNextResolutionTime();
				}
				else if (FMOD_StudioEventEmitter.HoursSinceMidnight > (float)this.NextResolutionTime)
				{
					this.NextResolutionTime = this.GetNextResolutionTime();
					this.Resolve();
				}
			}
		}

		
		public int GetRawExcessCalories()
		{
			return Mathf.RoundToInt(this.CurrentCaloriesEatenCount - this.CurrentCaloriesBurntCount);
		}

		
		public int GetExcessCaloriesFinal()
		{
			return this.GetRawExcessCalories();
		}

		
		public int GetStrengthTrend()
		{
			return this.GetRawExcessCalories();
		}

		
		public int GetWeightTrend()
		{
			return this.GetExcessCaloriesFinal();
		}

		
		public void OnFighting()
		{
			if (GameSetup.IsHardSurvivalMode)
			{
				this.CaloriesBurntChange(this.FightActionCalories);
				this.StrengthChange(this.FightActionStrength * Mathf.Sign((float)this.GetRawExcessCalories()));
			}
			else
			{
				this.StrengthChange(this.FightActionStrength);
			}
		}

		
		public void OnAteFood(int calories)
		{
			this.CaloriesEatenChange((float)calories);
		}

		
		public void OnAttacked()
		{
			if ((LocalPlayer.ScriptSetup.treeHit.atStump || LocalPlayer.ScriptSetup.treeHit.atTree) && (LocalPlayer.Animator.GetBool("axeHeld") || LocalPlayer.Animator.GetBool("chainSawHeld")))
			{
				this.CaloriesBurntChange(1f);
			}
		}

		
		private void CaloriesEatenChange(float value)
		{
			if (GameSetup.IsHardSurvivalMode)
			{
				this.CurrentCaloriesEatenCount += value;
			}
		}

		
		private void CaloriesBurntChange(float value)
		{
			if (GameSetup.IsHardSurvivalMode)
			{
				this.CurrentCaloriesBurntCount += value;
			}
		}

		
		private void WeightChange(float value)
		{
			LocalPlayer.Stats.PhysicalStrength.WeightChange(value);
		}

		
		private void StrengthChange(float value)
		{
			LocalPlayer.Stats.PhysicalStrength.StrengthChange(value);
		}

		
		public float TimeToNextResolution()
		{
			if (FMOD_StudioEventEmitter.HoursSinceMidnight < 6f)
			{
				return (float)this.NextResolutionTime - FMOD_StudioEventEmitter.HoursSinceMidnight;
			}
			return 24f - FMOD_StudioEventEmitter.HoursSinceMidnight + 6f;
		}

		
		public float SecondsToNextResolution()
		{
			float num = this.TimeToNextResolution();
			float num2 = 2160f;
			float num3 = num2 / 24f;
			return num * num3;
		}

		
		private int GetNextResolutionTime()
		{
			if (FMOD_StudioEventEmitter.HoursSinceMidnight < 6f)
			{
				return 6;
			}
			return -1;
		}

		
		private void Resolve()
		{
			if (GameSetup.IsHardSurvivalMode)
			{
				int excessCaloriesFinal = this.GetExcessCaloriesFinal();
				if (excessCaloriesFinal > 0)
				{
					this.WeightChange((float)excessCaloriesFinal * this.WeightGainPerExcessCalory);
				}
				else
				{
					this.WeightChange((float)excessCaloriesFinal * this.WeightLossPerMissingCalory);
				}
				this.CurrentCaloriesBurntCount = 0f;
				this.CurrentCaloriesEatenCount = 0f;
			}
			LocalPlayer.Stats.PhysicalStrength.DailyStartStrength = LocalPlayer.Stats.PhysicalStrength.CurrentStrength;
			LocalPlayer.Stats.PhysicalStrength.DailyStartWeight = LocalPlayer.Stats.PhysicalStrength.CurrentWeight;
		}

		
		[Header("Calories")]
		public float FightActionCalories = 7f;

		
		public float JumpingCaloriesPerSecond = 2f;

		
		public float RunningCaloriesPerSecond = 1f;

		
		public float WalkingCaloriesPerSecond = 0.5f;

		
		public float StationnaryCaloriesPerSecond = 0.06f;

		
		public float SwimmingCaloriesPerSecond = 1.5f;

		
		public float CarryLogCaloriesPerSecond = 0.5f;

		
		[Header("Weight")]
		public float WeightGainPerExcessCalory = 0.00015f;

		
		public float WeightLossPerMissingCalory = 0.00015f;

		
		public float StrengthLossPerMissingCalory = 0.0001f;

		
		[Header("Strength")]
		public float StrengthGainPerEatenCalory = 0.001f;

		
		public float FightActionStrength = 0.0047f;

		
		[Header("Runtime data")]
		[SerializeThis]
		public float CurrentCaloriesBurntCount;

		
		[SerializeThis]
		public float CurrentCaloriesEatenCount;

		
		private int NextResolutionTime;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class SkillData
	{
		
		public void CalcSkills()
		{
			this.AthleticismSkill = Mathf.FloorToInt(this.TotalRunDuration / this.RunSkillLevelDuration) + Mathf.FloorToInt(this.TotalLungBreathingDuration / this.BreathingSkillLevelDuration);
			if (LocalPlayer.Stats.PhysicalStrength.CurrentWeightScaled > this.OverweightThreshold)
			{
				float num = LocalPlayer.Stats.PhysicalStrength.CurrentWeightScaled - this.OverweightThreshold;
				float num2 = LocalPlayer.Stats.PhysicalStrength.MaxWeight - this.OverweightThreshold;
				this.AthleticismSkill = Mathf.FloorToInt((float)this.AthleticismSkill * Mathf.Lerp(1f, this.OverweightMaxRatio, num / num2));
			}
			this.RunStaminaRatio = Mathf.Max(1f - (float)this.AthleticismSkill * this.RunSkillLevelBonus, 0.5f);
			this.LungBreathingRatio = Mathf.Max(1f - (float)this.AthleticismSkill * this.BreathingSkillLevelBonus, 0.5f);
		}

		
		
		
		public float RunStaminaRatio { get; private set; }

		
		
		
		public float LungBreathingRatio { get; private set; }

		
		
		public float AthleticismSkillLevelProgressApprox
		{
			get
			{
				return Mathf.Clamp01((this.TotalRunDuration / this.RunSkillLevelDuration + this.TotalLungBreathingDuration / this.BreathingSkillLevelDuration) / 10f) * 79f + 20f;
			}
		}

		
		
		public int AthleticismSkillLevel
		{
			get
			{
				return this.AthleticismSkill;
			}
		}

		
		[UnityEngine.Tooltip("In real life seconds")]
		public float RunSkillLevelDuration = 7200f;

		
		[UnityEngine.Tooltip("0.1 = 10% less stamina cost")]
		public float RunSkillLevelBonus = 0.1f;

		
		[UnityEngine.Tooltip("In real life seconds (of underwater lung breathing)")]
		public float BreathingSkillLevelDuration = 1800f;

		
		[UnityEngine.Tooltip("0.1 = 10% more underwater lunq breathing duration")]
		public float BreathingSkillLevelBonus = 0.1f;

		
		public float OverweightThreshold = 210f;

		
		public float OverweightMaxRatio = 0.4f;

		
		[SerializeThis]
		public float TotalRunDuration;

		
		[SerializeThis]
		public float TotalLungBreathingDuration;

		
		private int AthleticismSkill;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class SanityData
	{
		
		public void Initialize()
		{
			EventRegistry.Enemy.Subscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.OnEnemyKilled));
			EventRegistry.Enemy.Subscribe(TfEvent.CutLimb, new EventRegistry.SubscriberCallback(this.OnCutLimbOff));
		}

		
		public void Clear()
		{
			EventRegistry.Enemy.Unsubscribe(TfEvent.KilledEnemy, new EventRegistry.SubscriberCallback(this.OnEnemyKilled));
			EventRegistry.Enemy.Unsubscribe(TfEvent.CutLimb, new EventRegistry.SubscriberCallback(this.OnCutLimbOff));
		}

		
		public void OnEnemyKilled(object o)
		{
			this.SanityChange(this.SanityPerKill);
		}

		
		public void OnCutLimbOff(object o)
		{
			this.SanityChange(this.SanityPerLimbCutOff);
		}

		
		public void OnCannibalism()
		{
			this.SanityChange(this.SanityPerCannibalism);
		}

		
		public void InCave()
		{
			this.SanityChange(this.SanityPerSecondInCave * Time.deltaTime);
		}

		
		public void ListeningToMusic()
		{
			this.SanityChange(this.SanityPerSecondOfMusic * Time.deltaTime);
		}

		
		public void SittingOnBench()
		{
			if (LocalPlayer.Stats.Energy < 100f)
			{
				this.SanityChange(this.SanityPerSecondSittedOnBench * Time.deltaTime);
			}
		}

		
		public void OnSlept(float hours)
		{
			this.SanityChange(this.SanityPerInGameHourOfSleep * hours);
		}

		
		public void OnAteFreshFood()
		{
			this.SanityChange(this.SanityPerFreshFoodEaten);
		}

		
		internal void SanityChange(float value)
		{
			this.CurrentSanity = Mathf.Clamp(this.CurrentSanity + value, 0f, 100f);
			if (this.CurrentSanity < 50f && !LocalPlayer.SavedData.ReachedLowSanityThreshold)
			{
				LocalPlayer.SavedData.ReachedLowSanityThreshold.SetValue(true);
				LocalPlayer.Tuts.ShowNewBuildingsAvailableTut();
			}
		}

		
		[SerializeThis]
		public float CurrentSanity = 100f;

		
		public float SanityPerKill = -0.5f;

		
		public float SanityPerLimbCutOff = -1f;

		
		public float SanityPerCannibalism = -3f;

		
		public float SanityPerSecondInCave = -0.001f;

		
		public float SanityPerSecondOfMusic = 0.01f;

		
		public float SanityPerSecondSittedOnBench = 0.15f;

		
		public float SanityPerInGameHourOfSleep = 0.75f;

		
		public float SanityPerFreshFoodEaten = 1f;
	}

	
	[DoNotSerializePublic]
	[Serializable]
	public class InfectionData
	{
		
		
		public float EffectRatio
		{
			get
			{
				return (!this.Infected) ? 1f : (this.EffectModifier * GameSettings.Survival.InfectionEffectRatio);
			}
		}

		
		public void GetInfected()
		{
			this.Infected = true;
			this.InfectedTime = LocalPlayer.Stats.DaySurvived;
			GameStats.Infected.Invoke();
		}

		
		public void TryGetInfected()
		{
			if (Mathf.FloorToInt((float)this.InfectionChance / GameSettings.Survival.InfectionChance) == 0)
			{
				this.Infected = true;
				this.InfectedTime = LocalPlayer.Stats.DaySurvived;
				GameStats.Infected.Invoke();
			}
		}

		
		public void TryAutoHeal()
		{
			if (this.Infected && LocalPlayer.Stats.DaySurvived > this.InfectedTime + this.AutoHealDelay)
			{
				this.Infected = false;
			}
		}

		
		public void Cure()
		{
			LocalPlayer.Stats.CancelInvoke("HitPoison");
			LocalPlayer.Stats.CancelInvoke("disablePoison");
			this.Infected = false;
		}

		
		[SerializeThis]
		public bool Infected;

		
		public RandomRange InfectionChance = new RandomRange();

		
		[UnityEngine.Tooltip("In game day time")]
		public float AutoHealDelay = float.MaxValue;

		
		public float EffectModifier = 1f;

		
		[SerializeThis]
		private float InfectedTime;
	}
}
