using System;
using System.Collections;
using Bolt;
using FMOD.Studio;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class PlayerSfx : EntityEventListener<IPlayerState>
{
	
	private string[] AllEventPaths()
	{
		return new string[]
		{
			this.PlantRustleEvent,
			this.StructureBreakEvent,
			this.StructureFallEvent,
			this.SplashEvent,
			this.WhooshEvent,
			this.PageTurnEvent,
			this.FlareEvent,
			this.FlareDryFireEvent,
			this.ShootFlintLockEvent,
			this.ThrowEvent,
			this.PickUpEvent,
			this.PutDownEvent,
			this.PlaceGhostEvent,
			this.EatSoftEvent,
			this.EatMeatEvent,
			this.EatPoisonEvent,
			this.EatMedsEvent,
			this.PutOnClothingEvent,
			this.PutOnArmorEvent,
			this.PutOnStealthArmorEvent,
			this.DrinkEvent,
			this.DrinkFromWaterSourceEvent,
			this.StaminaBreathEvent,
			this.LighterSparkEvent,
			this.HammerEvent,
			this.HurtEvent,
			this.JumpEvent,
			this.OpenInventoryEvent,
			this.CloseInventoryEvent,
			this.visWarningEvent,
			this.bowSnapEvent,
			this.bowDrawEvent,
			this.MusicInjuredEvent,
			this.CoinEvent,
			this.TorchLightOnEvent,
			this.TorchLightOffEvent,
			this.TwinkleEvent,
			this.RemovalEvent,
			this.FindBodyTwinkleEvent,
			this.WalkyTalkyEvent,
			this.WokenByEnemiesEvent,
			this.SightedByEnemyEvent,
			this.DigEvent,
			this.PencilTickEvent,
			this.TaskCompletedEvent,
			this.BreakWoodEvent,
			this.UpgradeSuccessEvent,
			this.CraneLoopEvent,
			this.ZiplineLoopEvent,
			this.ZiplineExitEvent,
			this.CamcorderRewindLoopEvent,
			this.KillRabbitEvent
		};
	}

	
	private void Awake()
	{
		if (this.Remote)
		{
			return;
		}
		this.Buoyancy = base.GetComponent<Buoyancy>();
	}

	
	private void Start()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			foreach (string path in this.AllEventPaths())
			{
				FMOD_StudioSystem.PreloadEvent(path);
			}
			if (!this.Remote)
			{
				this.ZiplineLoopInstance = FMOD_StudioSystem.instance.GetEvent(this.ZiplineLoopEvent);
				if (this.ZiplineLoopInstance != null)
				{
					this.ZiplineLoopInstance.getParameter("speed", out this.ZiplineLoopSpeedParameter);
				}
				this.CraneLoopInstance = FMOD_StudioSystem.instance.GetEvent(this.CraneLoopEvent);
				this.TurtleShellSledLoopInstance = FMOD_StudioSystem.instance.GetEvent(this.TurtleShellSledLoopEvent);
				if (this.TurtleShellSledLoopInstance != null)
				{
					this.TurtleShellSledLoopInstance.getParameter("speed", out this.TurtleShellSledLoopSpeedParameter);
					this.TurtleShellSledLoopInstance.getParameter("air", out this.TurtleShellSledLoopAirParameter);
					this.TurtleShellSledLoopInstance.getParameter("snow", out this.TurtleShellSledLoopSnowParameter);
				}
			}
		}
		else
		{
			base.enabled = false;
		}
		if (this.Remote)
		{
			base.enabled = false;
			return;
		}
		this.prevPosition = base.GetComponent<Rigidbody>().position;
	}

	
	private static void Set3DAttributes(EventInstance evt, ATTRIBUTES_3D attributes)
	{
		if (evt != null && evt.isValid())
		{
			UnityUtil.ERRCHECK(evt.set3DAttributes(attributes));
		}
	}

	
	private void Update()
	{
		if (this.Remote)
		{
			return;
		}
		PLAYBACK_STATE playback_STATE = PLAYBACK_STATE.STOPPED;
		if (this.musicTrack != null)
		{
			UnityUtil.ERRCHECK(this.musicTrack.getPlaybackState(out playback_STATE));
		}
		if (playback_STATE != PLAYBACK_STATE.STOPPED)
		{
			PlayerSfx.MusicPlaying = true;
		}
		else
		{
			PlayerSfx.MusicPlaying = false;
		}
		if (base.transform.hasChanged)
		{
			base.transform.hasChanged = false;
			ATTRIBUTES_3D attributes = UnityUtil.to3DAttributes(this.SfxPlayer, null);
			PlayerSfx.Set3DAttributes(this.staminaBreathInstance, attributes);
			PlayerSfx.Set3DAttributes(this.walkyTalkyInstance, attributes);
			PlayerSfx.Set3DAttributes(this.musicTrack, attributes);
			PlayerSfx.Set3DAttributes(this.TurtleShellSledLoopInstance, attributes);
			ATTRIBUTES_3D attributes2 = this.SfxPlayer.transform.position.to3DAttributes();
			PlayerSfx.Set3DAttributes(this.afterStormInstance, attributes2);
			PlayerSfx.Set3DAttributes(this.onOceanInstance, attributes2);
		}
		if (this.afterStormInstance != null && !this.afterStormInstance.isValid())
		{
			this.afterStormInstance = null;
		}
		this.sledOnTerrain = base.GetComponent<FirstPersonCharacter>().terrainContact;
		if (this.sledIsPlaying)
		{
			UnityUtil.ERRCHECK(this.TurtleShellSledLoopSpeedParameter.setValue(LocalPlayer.Rigidbody.velocity.sqrMagnitude / 200f));
		}
		if (this.sledIsPlaying && LocalPlayer.FpCharacter.Grounded)
		{
			UnityUtil.ERRCHECK(this.TurtleShellSledLoopAirParameter.setValue(0f));
		}
		else if (this.sledIsPlaying && !LocalPlayer.FpCharacter.Grounded)
		{
			UnityUtil.ERRCHECK(this.TurtleShellSledLoopAirParameter.setValue(1f));
		}
		if (this.sledIsPlaying && LocalPlayer.FpCharacter.inSnow)
		{
			UnityUtil.ERRCHECK(this.TurtleShellSledLoopSnowParameter.setValue(1f));
		}
		else if (this.sledIsPlaying && !LocalPlayer.FpCharacter.inSnow)
		{
			UnityUtil.ERRCHECK(this.TurtleShellSledLoopSnowParameter.setValue(0f));
		}
		Vector3 vector = (base.GetComponent<Rigidbody>().position - this.prevPosition) / Time.deltaTime;
		this.prevPosition = base.GetComponent<Rigidbody>().position;
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.z);
		this.flatVelocity = vector2.magnitude;
		if (!this.Buoyancy.InWater)
		{
			this.immersed = false;
		}
		else if (!this.immersed && !LocalPlayer.FpCharacter.Grounded && LocalPlayer.FpCharacter.IsAboveWaistDeep())
		{
			this.immersed = true;
			float num = Mathf.Clamp(this.SplashSpeedMaximum - this.SplashSpeedMinimum, 0f, this.SplashSpeedMaximum);
			float num2 = (this.Buoyancy.LastWaterEnterSpeed - this.SplashSpeedMinimum) / num;
			if (num2 >= 0f)
			{
				EventInstance @event = FMOD_StudioSystem.instance.GetEvent(this.SplashEvent);
				UnityUtil.ERRCHECK(@event.set3DAttributes(UnityUtil.to3DAttributes(this.SfxPlayer, null)));
				UnityUtil.ERRCHECK(@event.setParameterValue("speed", Mathf.Clamp01(num2)));
				UnityUtil.ERRCHECK(@event.start());
				UnityUtil.ERRCHECK(@event.release());
			}
		}
		if (this.Buoyancy.InWater && this.Buoyancy.IsOcean)
		{
			if (this.onOceanInstance == null)
			{
				this.onOceanInstance = FMODCommon.PlayOneshot("event:/ambient/water/on_ocean", base.transform);
			}
		}
		else if (this.onOceanInstance != null)
		{
			UnityUtil.ERRCHECK(this.onOceanInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.onOceanInstance.release());
			this.onOceanInstance = null;
		}
	}

	
	private void OnDestroy()
	{
		if (this.onOceanInstance != null)
		{
			UnityUtil.ERRCHECK(this.onOceanInstance.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.onOceanInstance.release());
			this.onOceanInstance = null;
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (this.Remote)
		{
			return;
		}
		if (other.gameObject.CompareTag("SmallTree"))
		{
			this.PlayPlantRustle();
		}
	}

	
	public void PlayStructureBreak(GameObject emiter, float size)
	{
		EventInstance @event = FMOD_StudioSystem.instance.GetEvent(this.StructureBreakEvent);
		UnityUtil.ERRCHECK(@event.set3DAttributes(UnityUtil.to3DAttributes(emiter, null)));
		UnityUtil.ERRCHECK(@event.setParameterValue("size", Mathf.Clamp01(size)));
		UnityUtil.ERRCHECK(@event.start());
		UnityUtil.ERRCHECK(@event.release());
	}

	
	public void PlayStructureFall(GameObject emiter, float size)
	{
		EventInstance @event = FMOD_StudioSystem.instance.GetEvent(this.StructureFallEvent);
		UnityUtil.ERRCHECK(@event.set3DAttributes(UnityUtil.to3DAttributes(emiter, null)));
		UnityUtil.ERRCHECK(@event.setParameterValue("size", Mathf.Clamp01(size)));
		UnityUtil.ERRCHECK(@event.start());
		UnityUtil.ERRCHECK(@event.release());
	}

	
	public void PlayPlantRustle()
	{
		if (this.plantRustleEnabled && FMOD_StudioSystem.instance)
		{
			EventInstance @event = FMOD_StudioSystem.instance.GetEvent(this.PlantRustleEvent);
			UnityUtil.ERRCHECK(@event.set3DAttributes(UnityUtil.to3DAttributes(this.SfxRustle, null)));
			UnityUtil.ERRCHECK(@event.setParameterValue("speed", LocalPlayer.FpCharacter.CalculateSpeedParameter(this.flatVelocity)));
			UnityUtil.ERRCHECK(@event.start());
			UnityUtil.ERRCHECK(@event.release());
			this.plantRustleEnabled = false;
			base.Invoke("EnablePlantRustle", 0.2f);
		}
	}

	
	private void EnablePlantRustle()
	{
		this.plantRustleEnabled = true;
	}

	
	public EventInstance PlayEvent(string path, GameObject gameObject)
	{
		return this.PlayEvent(path, gameObject.transform.position);
	}

	
	public EventInstance PlayEvent(string path, Vector3 position)
	{
		if (LocalPlayer.CurrentView > PlayerInventory.PlayerViews.Loading)
		{
			if ((!this.Remote && base.entity && base.entity.isAttached) || CoopPeerStarter.DedicatedHost)
			{
				FmodOneShot fmodOneShot = FmodOneShot.Create(GlobalTargets.Others, ReliabilityModes.Unreliable);
				fmodOneShot.EventPath = CoopAudioEventDb.FindId(path);
				fmodOneShot.Position = position;
				fmodOneShot.Send();
			}
			return (!FMOD_StudioSystem.instance || CoopPeerStarter.DedicatedHost) ? null : FMOD_StudioSystem.instance.PlayOneShot(path, position, null);
		}
		return null;
	}

	
	private Vector3 GetItemSfxPosition()
	{
		return (!Grabber.FocusedItem) ? (LocalPlayer.Transform.position + LocalPlayer.MainCamTr.forward) : Grabber.FocusedItem.transform.position;
	}

	
	public bool PlayItemCustomSfx(int itemId, bool fallback = true)
	{
		return this.PlayItemCustomSfx(ItemDatabase.ItemById(itemId), this.GetItemSfxPosition(), fallback);
	}

	
	public bool PlayItemCustomSfx(int itemId, Vector3 position, bool fallback = true)
	{
		return this.PlayItemCustomSfx(ItemDatabase.ItemById(itemId), position, fallback);
	}

	
	public bool PlayItemCustomSfx(Item item, bool fallback = true)
	{
		return this.PlayItemCustomSfx(item, this.GetItemSfxPosition(), fallback);
	}

	
	public bool PlayItemCustomSfx(Item item, Vector3 position, bool fallback = true)
	{
		if (!string.IsNullOrEmpty(item._customSfxEvent))
		{
			this.PlayEvent(item._customSfxEvent, position);
			return true;
		}
		if (fallback)
		{
			this.PlayWhoosh();
		}
		return false;
	}

	
	public void PlayTorchOn()
	{
		this.PlayEvent(this.TorchLightOnEvent, this.SfxPlayer);
	}

	
	public void PlayTorchOff()
	{
		this.PlayEvent(this.TorchLightOffEvent, this.SfxPlayer);
	}

	
	public void PlayCoinsSfx()
	{
		this.PlayEvent(this.CoinEvent, this.SfxPlayer);
	}

	
	public void PlayShootFlareSfx()
	{
		this.PlayEvent(this.FlareEvent, this.SfxPlayer);
	}

	
	public void PlayShootFlintLockSfx()
	{
		this.PlayEvent(this.ShootFlintLockEvent, this.SfxPlayer);
	}

	
	public void PlayDryFlareFireSfx()
	{
		this.PlayEvent(this.FlareDryFireEvent, this.SfxPlayer);
	}

	
	public void PlayPutOnArmorSfx()
	{
		this.PlayEvent(this.PutOnArmorEvent, this.SfxPlayer);
	}

	
	public void PlayPutOnClothingSfx()
	{
		this.PlayEvent(this.PutOnClothingEvent, this.SfxPlayer);
	}

	
	public void PlayPutOnStealthArmorSfx()
	{
		this.PlayEvent(this.PutOnStealthArmorEvent, this.SfxPlayer);
	}

	
	public void PlayPickUp()
	{
		this.PlayEvent(this.PickUpEvent, this.SfxPlayer);
	}

	
	public void PlayWhoosh()
	{
		this.PlayEvent(this.WhooshEvent, this.SfxPlayer);
	}

	
	public void PlayWhoosh(Vector3 position)
	{
		this.PlayEvent(this.WhooshEvent, position);
	}

	
	public void PlayThrow()
	{
		this.PlayEvent(this.ThrowEvent, this.SfxPlayer);
	}

	
	public void PlayPutDown(GameObject location)
	{
		this.PlayEvent(this.PutDownEvent, location);
	}

	
	public void PlayPlaceGhost()
	{
		this.PlayEvent(this.PlaceGhostEvent, this.SfxPlayer);
	}

	
	public void PlayTwinkle()
	{
		this.PlayEvent(this.TwinkleEvent, this.SfxGUI);
	}

	
	public void PlayInventorySound(Item.SFXCommands command)
	{
		string path = null;
		switch (command)
		{
		case Item.SFXCommands.None:
			return;
		case Item.SFXCommands.PlayPlantRustle:
			path = this.PlantRustleEvent;
			break;
		case Item.SFXCommands.PlayTorchOn:
			path = this.TorchLightOnEvent;
			break;
		case Item.SFXCommands.PlayTorchOff:
			path = this.TorchLightOffEvent;
			break;
		case Item.SFXCommands.PlayCoinsSfx:
			path = this.CoinEvent;
			break;
		case Item.SFXCommands.PlayShootFlareSfx:
			path = this.FlareEvent;
			break;
		case Item.SFXCommands.PlayPickUp:
			path = this.PickUpEvent;
			break;
		case Item.SFXCommands.PlayWhoosh:
			path = this.WhooshEvent;
			break;
		case Item.SFXCommands.PlayTwinkle:
			path = this.TwinkleEvent;
			break;
		case Item.SFXCommands.PlayRemove:
			path = this.RemovalEvent;
			break;
		case Item.SFXCommands.PlayEat:
			path = this.EatSoftEvent;
			break;
		case Item.SFXCommands.PlayDrink:
			path = this.DrinkEvent;
			break;
		case Item.SFXCommands.PlayHurtSound:
			path = this.HurtEvent;
			break;
		case Item.SFXCommands.PlayHammer:
			path = this.HammerEvent;
			break;
		case Item.SFXCommands.PlayFindBodyTwinkle:
			path = this.FindBodyTwinkleEvent;
			break;
		case Item.SFXCommands.PlayColdSfx:
		case Item.SFXCommands.StopColdSfx:
		case Item.SFXCommands.PlayCough:
		case Item.SFXCommands.PlayKillRabbit:
		case Item.SFXCommands.playVisWarning:
		case Item.SFXCommands.stopVisWarning:
		case Item.SFXCommands.PlayMusicTrack:
		case Item.SFXCommands.StopMusic:
		case Item.SFXCommands.PlayInjured:
		case Item.SFXCommands.StopPlaying:
		case Item.SFXCommands.StartWalkyTalky:
		case Item.SFXCommands.StopWalkyTalky:
			base.SendMessage(command.ToString());
			return;
		case Item.SFXCommands.PlayStaminaBreath:
			path = this.StaminaBreathEvent;
			break;
		case Item.SFXCommands.PlayLighterSound:
			path = this.LighterSparkEvent;
			break;
		case Item.SFXCommands.PlayTurnPage:
			path = this.PageTurnEvent;
			break;
		case Item.SFXCommands.PlayOpenInventory:
			path = this.OpenInventoryEvent;
			break;
		case Item.SFXCommands.PlayCloseInventory:
			path = this.CloseInventoryEvent;
			break;
		case Item.SFXCommands.PlayBowSnap:
			path = this.bowSnapEvent;
			break;
		case Item.SFXCommands.PlayBowDraw:
			path = this.bowDrawEvent;
			break;
		case Item.SFXCommands.PlayDryFlareFireSfx:
			path = this.FlareDryFireEvent;
			break;
		case Item.SFXCommands.PlayEatMeds:
			path = this.EatMedsEvent;
			break;
		case Item.SFXCommands.PlayShootFlintLockSfx:
			path = this.ShootFlintLockEvent;
			break;
		case Item.SFXCommands.PlayPutOnArmorSfx:
			path = this.PutOnArmorEvent;
			break;
		case Item.SFXCommands.PlayPutOnStealthArmorSfx:
			path = this.PutOnStealthArmorEvent;
			break;
		case Item.SFXCommands.PlayPutOnClothingSfx:
			path = this.PutOnClothingEvent;
			break;
		}
		FMODCommon.PlayOneshot(path, this.SfxGUI.transform);
	}

	
	private static Vector3 FindClosestPoint(GameObject gameObject, Vector3 position)
	{
		Vector3 vector = gameObject.transform.position;
		float num = Vector3.SqrMagnitude(vector - position);
		foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
		{
			Vector3 vector2 = collider.ClosestPointOnBounds(position);
			float num2 = Vector3.SqrMagnitude(vector2 - position);
			if (num2 < num)
			{
				vector = vector2;
				num = num2;
			}
		}
		return vector;
	}

	
	public void PlayBuildingComplete(GameObject building, bool networkSync = false)
	{
		Vector3 position = PlayerSfx.FindClosestPoint(building, base.transform.position);
		if (networkSync)
		{
			this.PlayEvent(this.TwinkleEvent, position);
		}
		else if (FMOD_StudioSystem.instance)
		{
			FMOD_StudioSystem.instance.PlayOneShot(this.TwinkleEvent, position, null);
		}
	}

	
	public void PlayBuildingRepair(GameObject building)
	{
		if (FMOD_StudioSystem.instance)
		{
			Vector3 position = PlayerSfx.FindClosestPoint(building, base.transform.position);
			FMOD_StudioSystem.instance.PlayOneShot(this.HammerEvent, position, null);
		}
	}

	
	public void PlayRemove()
	{
		this.PlayEvent(this.RemovalEvent, this.SfxGUI);
	}

	
	public override void OnEvent(SfxEat evnt)
	{
		this.PlayEat();
	}

	
	public void PlayEat()
	{
		this.PlayEvent(this.EatSoftEvent, this.SfxPlayer);
	}

	
	public void PlayEatPoison()
	{
		this.PlayEvent(this.EatPoisonEvent, this.SfxPlayer);
	}

	
	public void PlayEatMeat()
	{
		this.PlayEvent(this.EatMeatEvent, this.SfxPlayer);
	}

	
	public void PlayEatMeds()
	{
		this.PlayEvent(this.EatMedsEvent, this.SfxPlayer);
	}

	
	public override void OnEvent(SfxDrink evnt)
	{
		this.PlayDrink();
	}

	
	public void PlayDrink()
	{
		this.PlayEvent(this.DrinkEvent, this.SfxPlayer);
	}

	
	public void PlayDrinkFromWaterSource()
	{
		this.PlayEvent(this.DrinkFromWaterSourceEvent, this.SfxPlayer);
	}

	
	public override void OnEvent(SfxHurt evnt)
	{
		this.PlayHurtSound();
	}

	
	public void PlayHurtSound()
	{
		this.PlayEvent(this.HurtEvent, this.SfxPlayer);
	}

	
	public void PlayJumpSound()
	{
		if (this.jumpEnabled)
		{
			this.PlayEvent(this.JumpEvent, this.SfxPlayer);
			this.jumpEnabled = false;
			base.Invoke("EnableJump", 0.333333343f);
		}
	}

	
	private void EnableJump()
	{
		this.jumpEnabled = true;
	}

	
	public override void OnEvent(SfxHammer evnt)
	{
		this.PlayHammer();
	}

	
	public void PlayHammer()
	{
		this.PlayEvent(this.HammerEvent, this.SfxPlayer);
	}

	
	public void PlayFindBodyTwinkle()
	{
		this.PlayEvent(this.FindBodyTwinkleEvent, this.SfxGUI);
	}

	
	public void PlayColdSfx()
	{
	}

	
	public void StopColdSfx()
	{
	}

	
	public void PlayCough()
	{
	}

	
	public void PlayStaminaBreath()
	{
		PLAYBACK_STATE playback_STATE = PLAYBACK_STATE.STOPPED;
		if (this.staminaBreathInstance != null && this.staminaBreathInstance.isValid())
		{
			UnityUtil.ERRCHECK(this.staminaBreathInstance.getPlaybackState(out playback_STATE));
		}
		if (playback_STATE == PLAYBACK_STATE.STOPPED)
		{
			this.staminaBreathInstance = this.PlayEvent(this.StaminaBreathEvent, this.SfxPlayer);
		}
	}

	
	public void PlayLighterSound()
	{
		this.PlayEvent(this.LighterSparkEvent, this.SfxPlayer);
	}

	
	public void PlayKillRabbit()
	{
		this.PlayEvent(this.KillRabbitEvent, LocalPlayer.Inventory.RightHand._held);
	}

	
	public void PlayTurnPage()
	{
		this.PlayEvent(this.PageTurnEvent, this.SfxPlayer);
	}

	
	public void PlayOpenInventory()
	{
		this.PlayEvent(this.OpenInventoryEvent, this.SfxPlayer);
	}

	
	public void PlayCloseInventory()
	{
		this.PlayEvent(this.CloseInventoryEvent, this.SfxPlayer);
	}

	
	public IEnumerator playVisWarning()
	{
		if (this.visWarningInstance == null)
		{
			this.visWarningInstance = this.PlayEvent(this.visWarningEvent, this.SfxPlayer);
		}
		yield return null;
		yield break;
	}

	
	public IEnumerator stopVisWarning()
	{
		yield return YieldPresets.WaitPointOneSeconds;
		base.StopCoroutine("playVisWarning");
		if (this.visWarningInstance != null)
		{
			UnityUtil.ERRCHECK(this.visWarningInstance.stop(STOP_MODE.ALLOWFADEOUT));
			this.visWarningInstance = null;
		}
		yield return null;
		yield break;
	}

	
	public void PlayBowSnap()
	{
		this.PlayEvent(this.bowSnapEvent, this.SfxPlayer);
	}

	
	public void PlayBowDraw()
	{
		this.PlayEvent(this.bowDrawEvent, this.SfxPlayer);
	}

	
	public EventInstance InstantiateMusicTrack(int trackNum)
	{
		return FMOD_StudioSystem.instance.GetEvent(this.MusicTrackPaths[trackNum]);
	}

	
	public void PlayMusicTrack(int trackNum)
	{
		this.StopMusic();
		this.musicTrack = this.InstantiateMusicTrack(trackNum);
		UnityUtil.ERRCHECK(this.musicTrack.set3DAttributes(UnityUtil.to3DAttributes(this.SfxPlayer, null)));
		UnityUtil.ERRCHECK(this.musicTrack.start());
	}

	
	public void StopMusic()
	{
		if (this.musicTrack != null)
		{
			UnityUtil.ERRCHECK(this.musicTrack.stop(STOP_MODE.ALLOWFADEOUT));
			UnityUtil.ERRCHECK(this.musicTrack.release());
			this.musicTrack = null;
		}
	}

	
	public void PlayInjured()
	{
		if (!this.Playing)
		{
			this.DeathMusic.SetActive(true);
			base.Invoke("StopPlaying", 6f);
			this.Playing = true;
		}
	}

	
	public void StopPlaying()
	{
		this.DeathMusic.SetActive(false);
		this.Playing = false;
	}

	
	public EventInstance RelinquishMusicTrack()
	{
		EventInstance result = this.musicTrack;
		this.musicTrack = null;
		return result;
	}

	
	public void SetMusicTrack(EventInstance evt)
	{
		this.StopMusic();
		this.musicTrack = evt;
		UnityUtil.ERRCHECK(this.musicTrack.set3DAttributes(UnityUtil.to3DAttributes(this.SfxPlayer, null)));
	}

	
	public void StartWalkyTalky()
	{
		this.walkyTalkyInstance = FMOD_StudioSystem.instance.GetEvent(this.WalkyTalkyEvent);
		UnityUtil.ERRCHECK(this.walkyTalkyInstance.set3DAttributes(UnityUtil.to3DAttributes(this.SfxPlayer, null)));
		UnityUtil.ERRCHECK(this.walkyTalkyInstance.start());
	}

	
	public void StopWalkyTalky()
	{
		if (this.walkyTalkyInstance != null)
		{
			CueInstance cueInstance = null;
			UnityUtil.ERRCHECK(this.walkyTalkyInstance.getCue("KeyOff", out cueInstance));
			UnityUtil.ERRCHECK(cueInstance.trigger());
			UnityUtil.ERRCHECK(this.walkyTalkyInstance.release());
			this.walkyTalkyInstance = null;
		}
	}

	
	public void PlayWokenByEnemies()
	{
		this.PlayEvent(this.WokenByEnemiesEvent, this.SfxPlayer);
	}

	
	public void PlaySightedByEnemy(GameObject location)
	{
		if (this.sightedByEnemyEnabled)
		{
			this.PlayEvent(this.SightedByEnemyEvent, location);
			this.sightedByEnemyEnabled = false;
			base.Invoke("EnableSightedByEnemy", 5f);
		}
	}

	
	public void PlayDigDirtPile(GameObject dirtPile)
	{
		this.PlayEvent(this.DigEvent, dirtPile);
	}

	
	public void PlayTaskAvailable()
	{
		this.PlayEvent(this.PencilTickEvent, this.SfxPlayer);
	}

	
	public void PlayTaskCompleted()
	{
		this.PlayEvent(this.TaskCompletedEvent, this.SfxPlayer);
	}

	
	public void PlayBreakWood(GameObject wood)
	{
		this.PlayEvent(this.BreakWoodEvent, wood);
	}

	
	public void PlayUpgradeSuccess(GameObject upgradeView)
	{
		this.PlayEvent(this.UpgradeSuccessEvent, upgradeView);
	}

	
	public void PlayAfterStorm()
	{
		FMODCommon.ReleaseIfValid(this.afterStormInstance, STOP_MODE.ALLOWFADEOUT);
		this.afterStormInstance = FMODCommon.PlayOneshot("event:/ambient/amb_streamed/after_storm", base.transform);
	}

	
	public void SetCraneLoop(bool onOff, GameObject source)
	{
		if (this.CraneLoopInstance != null)
		{
			PLAYBACK_STATE state;
			UnityUtil.ERRCHECK(this.CraneLoopInstance.getPlaybackState(out state));
			if (onOff)
			{
				this.SyncLoopingEvent(this.CraneLoopEvent, source.transform.position);
				UnityUtil.ERRCHECK(this.CraneLoopInstance.set3DAttributes(UnityUtil.to3DAttributes(source, null)));
				if (!state.isPlaying())
				{
					UnityUtil.ERRCHECK(this.CraneLoopInstance.start());
				}
			}
			else
			{
				this.SyncLoopingEvent(string.Empty, Vector3.zero);
				if (state.isPlaying())
				{
					UnityUtil.ERRCHECK(this.CraneLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
				}
			}
		}
	}

	
	public void SetZiplineLoop(bool onOff)
	{
		PLAYBACK_STATE state;
		UnityUtil.ERRCHECK(this.ZiplineLoopInstance.getPlaybackState(out state));
		if (onOff)
		{
			UnityUtil.ERRCHECK(this.ZiplineLoopInstance.set3DAttributes(UnityUtil.to3DAttributes(base.gameObject, null)));
			UnityUtil.ERRCHECK(this.ZiplineLoopSpeedParameter.setValue(LocalPlayer.Rigidbody.velocity.sqrMagnitude / 2500f));
			this.SyncLoopingEvent(this.ZiplineLoopEvent, base.transform.position);
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.ZiplineLoopInstance.start());
			}
		}
		else
		{
			this.SyncLoopingEvent(string.Empty, Vector3.zero);
			if (state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.ZiplineLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
			}
		}
	}

	
	public void PlayZiplineExit()
	{
		this.PlayEvent(this.ZiplineExitEvent, LocalPlayer.MainCamTr.gameObject);
	}

	
	public void PlayCamcorderLoop(Transform target, bool onOff, string eventPath)
	{
		PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		if (this.CamcorderLoopInstance == null && onOff)
		{
			this.CamcorderLoopInstance = FMOD_StudioSystem.instance.GetEvent(eventPath);
		}
		if (this.CamcorderLoopInstance != null)
		{
			UnityUtil.ERRCHECK(this.CamcorderLoopInstance.getPlaybackState(out state));
		}
		if (onOff)
		{
			UnityUtil.ERRCHECK(this.CamcorderLoopInstance.set3DAttributes(target.to3DAttributes()));
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.CamcorderLoopInstance.start());
			}
		}
		else
		{
			if (state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.CamcorderLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
			}
			this.CamcorderLoopInstance = null;
		}
	}

	
	public void PlayCamcorderRewindLoop(Transform target, bool onOff)
	{
		PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
		if (this.CamcorderRewindLoopInstance == null && onOff && this.CamcorderRewindLoopEvent != null)
		{
			this.CamcorderRewindLoopInstance = FMOD_StudioSystem.instance.GetEvent(this.CamcorderRewindLoopEvent);
		}
		if (this.CamcorderRewindLoopInstance != null)
		{
			UnityUtil.ERRCHECK(this.CamcorderRewindLoopInstance.getPlaybackState(out state));
		}
		if (onOff)
		{
			UnityUtil.ERRCHECK(this.CamcorderRewindLoopInstance.set3DAttributes(target.to3DAttributes()));
			this.SyncLoopingEvent(this.CamcorderRewindLoopEvent, target.position);
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.CamcorderRewindLoopInstance.start());
			}
		}
		else
		{
			this.SyncLoopingEvent(string.Empty, Vector3.zero);
			if (state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.CamcorderRewindLoopInstance.stop(STOP_MODE.IMMEDIATE));
			}
			this.CamcorderRewindLoopInstance = null;
		}
	}

	
	public void PlayPushRaft(bool onWater, GameObject raft)
	{
		this.PlayEvent((!onWater) ? this.PushRaftOnGround : this.PushRaftOnWater, raft);
	}

	
	public void SetTurtleSledLoop(bool onOff)
	{
		PLAYBACK_STATE state;
		UnityUtil.ERRCHECK(this.TurtleShellSledLoopInstance.getPlaybackState(out state));
		if (onOff)
		{
			UnityUtil.ERRCHECK(this.TurtleShellSledLoopInstance.set3DAttributes(UnityUtil.to3DAttributes(this.SfxPlayer, null)));
			this.SyncLoopingEvent(this.TurtleShellSledLoopEvent, base.transform.position);
			if (!state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.TurtleShellSledLoopInstance.start());
				this.sledIsPlaying = true;
				this.TurtleShellSledLoopSpeedParameter.getValue(out this.p);
			}
		}
		else
		{
			this.SyncLoopingEvent(string.Empty, Vector3.zero);
			if (state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.TurtleShellSledLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
				this.sledIsPlaying = false;
			}
		}
	}

	
	private void EnableSightedByEnemy()
	{
		this.sightedByEnemyEnabled = true;
	}

	
	private void DieTrap(int type)
	{
		if (type == 3)
		{
			FMODCommon.PlayOneshotNetworked("event:/player/player_vox/jump_vox", base.transform, FMODCommon.NetworkRole.Server);
			FMODCommon.PlayOneshot("event:/player/foley/body_fall", base.transform.position, FMODCommon.NetworkRole.Server, new object[]
			{
				"fall",
				0.8f
			});
		}
	}

	
	public override void Attached()
	{
		if (!SteamDSConfig.isDedicatedServer)
		{
			if (!base.entity.isOwner)
			{
				base.state.AddCallback("LoopingEventPath", new PropertyCallbackSimple(this.OnLoopingEventPathUpdate));
				base.state.AddCallback("LoopingEventPosition", new PropertyCallbackSimple(this.OnLoopingEventPathUpdate));
			}
			else
			{
				base.state.LoopingEventPath = -1;
			}
		}
	}

	
	private void OnLoopingEventPathUpdate()
	{
		bool flag = base.state.LoopingEventPath != -1;
		string text = CoopAudioEventDb.FindEvent(base.state.LoopingEventPath);
		if (flag)
		{
			if (this.remoteLoopInstance == null && !string.IsNullOrEmpty(text) && this.remoteLoopEvent != text)
			{
				this.remoteLoopInstance = FMOD_StudioSystem.instance.GetEvent(text);
				UnityUtil.ERRCHECK(this.remoteLoopInstance.set3DAttributes(base.state.LoopingEventPosition.to3DAttributes()));
				this.remoteLoopLastUpdateTime = Time.time;
			}
			if (this.remoteLoopInstance != null)
			{
				this.remoteLoopEvent = text;
				PLAYBACK_STATE state;
				UnityUtil.ERRCHECK(this.remoteLoopInstance.getPlaybackState(out state));
				ATTRIBUTES_3D attributes_3D;
				UnityUtil.ERRCHECK(this.remoteLoopInstance.get3DAttributes(out attributes_3D));
				Vector3 vector = attributes_3D.position.toUnityVector();
				Vector3 vector2 = Vector3.SmoothDamp(vector, base.state.LoopingEventPosition, ref this.remoteLoopVelocity, 0.2f);
				UnityUtil.ERRCHECK(this.remoteLoopInstance.set3DAttributes(vector2.to3DAttributes()));
				if (this.remoteLoopEvent == this.ZiplineLoopEvent)
				{
					ParameterInstance parameterInstance;
					this.remoteLoopInstance.getParameter("speed", out parameterInstance);
					UnityUtil.ERRCHECK(parameterInstance.setValue(Vector3.Distance(vector, vector2) / (Time.time - this.remoteLoopLastUpdateTime) / 50f));
				}
				if (!state.isPlaying())
				{
					UnityUtil.ERRCHECK(this.remoteLoopInstance.start());
				}
				this.remoteLoopLastUpdateTime = Time.time;
			}
		}
		else if (this.remoteLoopInstance != null)
		{
			PLAYBACK_STATE state;
			UnityUtil.ERRCHECK(this.remoteLoopInstance.getPlaybackState(out state));
			if (state.isPlaying())
			{
				UnityUtil.ERRCHECK(this.remoteLoopInstance.stop(STOP_MODE.ALLOWFADEOUT));
				this.remoteLoopInstance = null;
			}
			this.remoteLoopEvent = null;
		}
	}

	
	private void SyncLoopingEvent(string path, Vector3 position)
	{
		if ((!this.Remote && base.entity && base.entity.isAttached) || CoopPeerStarter.DedicatedHost)
		{
			base.state.LoopingEventPosition = position;
			base.state.LoopingEventPath = CoopAudioEventDb.FindId(path);
		}
	}

	
	private bool Playing;

	
	public bool Remote;

	
	public GameObject SfxPlayer;

	
	public GameObject SfxGUI;

	
	public GameObject SfxRustle;

	
	public GameObject MPCas;

	
	public GameObject DeathMusic;

	
	public static bool MusicPlaying;

	
	private bool sledIsPlaying;

	
	private bool sledOnTerrain;

	
	private bool sledOnSnow;

	
	public float p;

	
	public string PlantRustleEvent;

	
	public string SplashEvent;

	
	public string StructureBreakEvent;

	
	public string StructureFallEvent;

	
	public float SplashSpeedMinimum = 10f;

	
	public float SplashSpeedMaximum = 30f;

	
	public string WhooshEvent;

	
	public string PageTurnEvent;

	
	public string FlareEvent;

	
	public string FlareDryFireEvent;

	
	public string ShootFlintLockEvent;

	
	public string ThrowEvent;

	
	public string PickUpEvent;

	
	public string PutDownEvent;

	
	public string PlaceGhostEvent;

	
	public string EatSoftEvent;

	
	public string EatMeatEvent;

	
	public string EatPoisonEvent;

	
	public string EatMedsEvent;

	
	public string PutOnClothingEvent;

	
	public string PutOnArmorEvent;

	
	public string PutOnStealthArmorEvent;

	
	public string DrinkEvent;

	
	public string DrinkFromWaterSourceEvent;

	
	public string StaminaBreathEvent;

	
	public string LighterSparkEvent;

	
	public string HammerEvent;

	
	public string HurtEvent;

	
	public string JumpEvent;

	
	public string OpenInventoryEvent;

	
	public string CloseInventoryEvent;

	
	public string visWarningEvent;

	
	public string bowSnapEvent;

	
	public string bowDrawEvent;

	
	public string MusicInjuredEvent;

	
	public string CoinEvent;

	
	public string TorchLightOnEvent;

	
	public string TorchLightOffEvent;

	
	public string TwinkleEvent;

	
	public string RemovalEvent;

	
	public string FindBodyTwinkleEvent;

	
	public string WalkyTalkyEvent;

	
	public string WokenByEnemiesEvent;

	
	public string SightedByEnemyEvent;

	
	public string DigEvent;

	
	public string PencilTickEvent;

	
	public string TaskCompletedEvent;

	
	public string BreakWoodEvent;

	
	public string UpgradeSuccessEvent;

	
	public string CraneLoopEvent;

	
	public string ZiplineLoopEvent;

	
	public string ZiplineExitEvent;

	
	public string PushRaftOnGround;

	
	public string PushRaftOnWater;

	
	public string CamcorderRewindLoopEvent;

	
	public string KillRabbitEvent;

	
	public string TurtleShellSledLoopEvent;

	
	public string[] MusicTrackPaths;

	
	private Buoyancy Buoyancy;

	
	private EventInstance CraneLoopInstance;

	
	private EventInstance ZiplineLoopInstance;

	
	private ParameterInstance ZiplineLoopSpeedParameter;

	
	private EventInstance CamcorderLoopInstance;

	
	private EventInstance CamcorderRewindLoopInstance;

	
	private EventInstance TurtleShellSledLoopInstance;

	
	private ParameterInstance TurtleShellSledLoopSpeedParameter;

	
	private ParameterInstance TurtleShellSledLoopAirParameter;

	
	private ParameterInstance TurtleShellSledLoopSnowParameter;

	
	private EventInstance musicTrack;

	
	private EventInstance staminaBreathInstance;

	
	private EventInstance visWarningInstance;

	
	private EventInstance walkyTalkyInstance;

	
	private EventInstance afterStormInstance;

	
	private EventInstance onOceanInstance;

	
	private bool plantRustleEnabled = true;

	
	private bool jumpEnabled = true;

	
	private bool sightedByEnemyEnabled = true;

	
	private Vector3 prevPosition;

	
	private float flatVelocity;

	
	private bool immersed;

	
	private float remoteLoopLastUpdateTime;

	
	private string remoteLoopEvent;

	
	private EventInstance remoteLoopInstance;

	
	private Vector3 remoteLoopVelocity;
}
