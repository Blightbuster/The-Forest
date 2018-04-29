using System;
using System.Collections;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Tools;
using TheForest.Utils;
using UnityEngine;


public class activateEndCrash : MonoBehaviour
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	private void GrabEnter()
	{
		if (this.pickup)
		{
			base.enabled = false;
			return;
		}
		base.enabled = true;
		if (this.Sheen)
		{
			this.Sheen.SetActive(false);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(true);
		}
	}

	
	private void GrabExit()
	{
		if (this.pickup)
		{
			base.enabled = false;
			return;
		}
		if (!this._altEnding)
		{
			this.activateScreen.SetActive(true);
		}
		this.confirmGo.SetActive(false);
		this.activateGo.SetActive(true);
		this.confirm = false;
		if (this.Sheen)
		{
			this.Sheen.SetActive(true);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(false);
		}
		base.enabled = false;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.EndCrash)
		{
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
			Scene.HudGui.MpEndCrashLabel.gameObject.SetActive(false);
			SteamClientDSConfig.IsClientAtWorld = true;
		}
	}

	
	private void Update()
	{
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && TheForest.Utils.Input.GetButtonAfterDelay("Take", this.delay, false))
		{
			if (!this.confirm)
			{
				if (!this._altEnding)
				{
					this.activateScreen.SetActive(false);
				}
				else
				{
					this.activateGo.SetActive(false);
				}
				this.confirmGo.SetActive(true);
				if (this.timmyPhotoRoutine == null && !this._altEnding)
				{
					this.timmyPhotoRoutine = base.StartCoroutine(this.equipTimmyPhotoRoutine());
				}
				this.confirm = true;
			}
			else if (!BoltNetwork.isRunning)
			{
				if (!this._altEnding)
				{
					this.activateScreen.SetActive(true);
				}
				this.activateGo.SetActive(true);
				this.confirmGo.SetActive(false);
				this.BeginEndCrashSequence();
			}
			else
			{
				if (!this._altEnding)
				{
					this.activateScreen.SetActive(true);
				}
				this.activateGo.SetActive(true);
				this.confirmGo.SetActive(false);
				this.ResetMpDelay();
				this._previousPlayersReady = -1;
				if (this.Sheen)
				{
					this.Sheen.SetActive(false);
				}
				if (this.MyPickUp)
				{
					this.MyPickUp.SetActive(false);
				}
				Scene.HudGui.MpEndCrashLabel.gameObject.SetActive(true);
				LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.EndCrash;
			}
		}
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.EndCrash)
		{
			if (TheForest.Utils.Input.GetButtonDown("Esc"))
			{
				LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
				Scene.HudGui.MpEndCrashLabel.gameObject.SetActive(false);
				SteamClientDSConfig.IsClientAtWorld = true;
				this.activateGo.SetActive(true);
				this.confirmGo.SetActive(false);
			}
			else
			{
				int num = 0;
				for (int i = 0; i < Scene.SceneTracker.allPlayerEntities.Count; i++)
				{
					if (Scene.SceneTracker.allPlayerEntities[i].GetState<IPlayerState>().CurrentView == 10)
					{
						num++;
					}
				}
				if (this._previousPlayersReady != num)
				{
					this.ResetMpDelay();
					this._previousPlayersReady = num;
					Scene.HudGui.MpEndCrashLabel.text = num + 1 + "/" + (Scene.SceneTracker.allPlayerEntities.Count + 1);
				}
				if (num == Scene.SceneTracker.allPlayerEntities.Count)
				{
					if (this._mpDelay > 0f)
					{
						this._mpDelay -= Time.deltaTime;
					}
					else
					{
						this.ResetMpDelay();
						this.BeginEndCrashSequence();
						Scene.HudGui.MpEndCrashLabel.gameObject.SetActive(false);
						Scene.HudGui.Grid.repositionNow = true;
						base.GetComponent<Collider>().enabled = false;
					}
				}
			}
		}
	}

	
	private void ResetMpDelay()
	{
		this._mpDelay = 1f;
	}

	
	private void BeginEndCrashSequence()
	{
		LocalPlayer.SpecialActions.SendMessage("setPlaneGo", this.planeGo);
		LocalPlayer.SpecialActions.SendMessage("setSecondArtifactGo", this.artifactGo);
		LocalPlayer.SpecialActions.SendMessage("setEndCrashScript", this);
		if (this._altEnding)
		{
			EventRegistry.Endgame.Publish(TfEvent.Endgame.Shutdown2ndArtifact, null);
			LocalPlayer.SpecialActions.SendMessage("doShutDownRoutine", this.markTr);
			LocalPlayer.SpecialActions.SendMessage("setAltAnim", this.altAnim);
			LocalPlayer.SpecialActions.SendMessage("setElevatorDoor", this.elevatorDoorGo);
			LocalPlayer.SpecialActions.SendMessage("setActivateScreen", this.activateScreen);
			if (this.activateScreen)
			{
				this.activateScreen.SetActive(false);
			}
			this.otherTriggerGo.SetActive(false);
		}
		else
		{
			LocalPlayer.SpecialActions.SendMessage("doEndPlaneCrashRoutine", this.markTr);
			if (this.bluePulseGo)
			{
				this.bluePulseGo.SetActive(true);
			}
			this.otherTriggerGo.SetActive(false);
		}
		this.pickup = true;
		if (this.Sheen)
		{
			this.Sheen.SetActive(false);
		}
		if (this.MyPickUp)
		{
			this.MyPickUp.SetActive(false);
		}
		base.enabled = false;
		base.Invoke("DisableRadarSfx", 20f);
	}

	
	private void DisableRadarSfx()
	{
		if (this.radarSfx)
		{
			this.radarSfx.Stop();
		}
	}

	
	private IEnumerator equipTimmyPhotoRoutine()
	{
		yield return YieldPresets.WaitPointSevenSeconds;
		LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
		LocalPlayer.Inventory.Equip(LocalPlayer.AnimControl._timmyPhotoId, false);
		yield return YieldPresets.WaitOneSecond;
		float dist = 0f;
		while (dist < 5f)
		{
			dist = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
			if (!LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._timmyPhotoId))
			{
				break;
			}
			yield return null;
		}
		if (LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, LocalPlayer.AnimControl._timmyPhotoId))
		{
			LocalPlayer.Inventory.EquipPreviousWeapon(true);
		}
		this.timmyPhotoRoutine = null;
		yield break;
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public GameObject confirmGo;

	
	public GameObject activateGo;

	
	public GameObject shutdownActivatedGo;

	
	public GameObject activateScreen;

	
	public Transform markTr;

	
	public GameObject bluePulseGo;

	
	public GameObject artifactGo;

	
	public GameObject elevatorDoorGo;

	
	public GameObject otherTriggerGo;

	
	public AnimationClip altAnim;

	
	public GameObject RedLight;

	
	public GameObject planeGo;

	
	public FMOD_StudioEventEmitter radarSfx;

	
	private Animator planeController;

	
	public bool pickup;

	
	public bool confirm;

	
	public bool _altEnding;

	
	public float delay = 0.5f;

	
	private int _previousPlayersReady;

	
	private float _mpDelay = float.MinValue;

	
	private const float MpDelayDuration = 1f;

	
	private Coroutine timmyPhotoRoutine;
}
