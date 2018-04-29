using System;
using TheForest.Items.Inventory;
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
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && TheForest.Utils.Input.GetButtonAfterDelay("Take", this.delay, false) && !this.pickup && !this.pickup)
		{
			if (!BoltNetwork.isRunning)
			{
				this.BeginEndCrashSequence();
			}
			else
			{
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
		LocalPlayer.SpecialActions.SendMessage("doEndPlaneCrashRoutine", this.markTr);
		if (this.bluePulseGo)
		{
			this.bluePulseGo.SetActive(true);
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
		this.radarSfx.Stop();
	}

	
	private const float MpDelayDuration = 1f;

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public Transform markTr;

	
	public GameObject bluePulseGo;

	
	public GameObject artifactGo;

	
	public GameObject planeGo;

	
	public FMOD_StudioEventEmitter radarSfx;

	
	private Animator planeController;

	
	public bool pickup;

	
	public float delay = 0.5f;

	
	private int _previousPlayersReady;

	
	private float _mpDelay = float.MinValue;
}
