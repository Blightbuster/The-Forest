using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using TheForest.World.Areas;
using UniLinq;
using UnityEngine;


public class ShelterTrigger : MonoBehaviour
{
	
	private void Awake()
	{
		if (this.SleepIcon)
		{
			this.SleepIcon.transform.parent = base.transform;
			this.SleepIcon.SetActive(false);
		}
		if (this.SaveIcon)
		{
			this.SaveIcon.transform.parent = base.transform;
			this.SaveIcon.SetActive(false);
		}
		base.enabled = false;
	}

	
	private void Update()
	{
		if (this.playerDoingAction())
		{
			this.SleepIcon.SetActive(false);
			Scene.HudGui.SleepDelayIcon.gameObject.SetActive(false);
			return;
		}
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World && !this.playerDoingAction())
		{
			if (this.CanSleep)
			{
				if (this.SleepIcon)
				{
					if (!this.SleepIcon.activeSelf)
					{
						this.SleepIcon.SetActive(true);
						Scene.HudGui.SleepDelayIcon.gameObject.SetActive(false);
					}
					if (TheForest.Utils.Input.GetButtonAfterDelay("RestKey", 0.5f, false))
					{
						if (!BoltNetwork.isRunning)
						{
							if (LocalPlayer.Stats.GoToSleep())
							{
								ShelterTrigger.CheckRegrowTrees();
								if (this.BreakAfterSleep)
								{
									base.StartCoroutine(this.DelayedCollapse());
								}
								base.SendMessage("OnSleep", SendMessageOptions.DontRequireReceiver);
							}
						}
						else
						{
							this.ResetMpSleepDelay();
							this._previousPlayersReady = -1;
							this.SleepIcon.SetActive(false);
							Scene.HudGui.SleepDelayIcon.gameObject.SetActive(false);
							Scene.HudGui.MpSleepLabel.gameObject.SetActive(true);
							LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Sleep;
						}
					}
				}
			}
			else if (this.SleepIcon.activeSelf || !Scene.HudGui.SleepDelayIcon.gameObject.activeSelf)
			{
				this.SleepIcon.SetActive(false);
				Scene.HudGui.SleepDelayIcon._target = this.SleepIcon.transform;
				Scene.HudGui.SleepDelayIcon.gameObject.SetActive(true);
			}
		}
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Sleep)
		{
			if (TheForest.Utils.Input.GetButtonDown("Esc"))
			{
				LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
				Scene.HudGui.MpSleepLabel.gameObject.SetActive(false);
			}
			else
			{
				int num = 0;
				for (int i = 0; i < Scene.SceneTracker.allPlayerEntities.Count; i++)
				{
					if (Scene.SceneTracker.allPlayerEntities[i].GetState<IPlayerState>().CurrentView == 9)
					{
						num++;
					}
				}
				if (this._previousPlayersReady != num)
				{
					this.ResetMpSleepDelay();
					this._previousPlayersReady = num;
					Scene.HudGui.MpSleepLabel.text = num + 1 + "/" + (Scene.SceneTracker.allPlayerEntities.Count + 1);
				}
				if ((BoltNetwork.isServer || SteamClientDSConfig.isDSFirstClient) && num == Scene.SceneTracker.allPlayerEntities.Count)
				{
					if (this._mpSleepDelay > 0f)
					{
						this._mpSleepDelay -= Time.deltaTime;
					}
					else
					{
						base.SendMessage("OnSleep", SendMessageOptions.DontRequireReceiver);
						this.ResetMpSleepDelay();
						Sleep sleep = Sleep.Create((!SteamClientDSConfig.isDSFirstClient) ? GlobalTargets.AllClients : GlobalTargets.OnlyServer);
						if (LocalPlayer.Stats.GoToSleep())
						{
							if (!SteamClientDSConfig.isDSFirstClient)
							{
								ShelterTrigger.CheckRegrowTrees();
							}
							if (this.BreakAfterSleep)
							{
								base.StartCoroutine(this.DelayedCollapse());
							}
						}
						else
						{
							sleep.Aborted = true;
						}
						sleep.Send();
						LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
						Scene.HudGui.MpSleepLabel.gameObject.SetActive(false);
						Scene.HudGui.Grid.repositionNow = true;
					}
				}
			}
		}
		if (this.PlayerCanSave() && TheForest.Utils.Input.GetButtonAfterDelay("Save", 0.5f, false))
		{
			LocalPlayer.Stats.JustSave();
		}
	}

	
	private void ResetMpSleepDelay()
	{
		this._mpSleepDelay = 1f;
	}

	
	private void GrabExit()
	{
		if (this.SleepIcon)
		{
			this.SleepIcon.SetActive(false);
			Scene.HudGui.SleepDelayIcon.gameObject.SetActive(false);
		}
		if (this.SaveIcon)
		{
			this.SaveIcon.SetActive(false);
		}
		base.enabled = false;
		if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Sleep)
		{
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
			Scene.HudGui.MpSleepLabel.gameObject.SetActive(false);
		}
	}

	
	private void GrabEnter()
	{
		if (this.SleepIcon && !BoltNetwork.isRunning && !this.playerDoingAction())
		{
			if (this.CanSleep)
			{
				this.SleepIcon.SetActive(true);
			}
			else
			{
				Scene.HudGui.SleepDelayIcon.gameObject.SetActive(true);
				Scene.HudGui.SleepDelayIcon._target = this.SleepIcon.transform;
			}
		}
		if ((!LocalPlayer.IsInEndgame || (LocalPlayer.SurfaceDetector.Surface == UnderfootSurfaceDetector.SurfaceType.Rock && Area.IsActiveAreaTheStartArea())) && !this.playerDoingAction() && this.SaveIcon)
		{
			this.SaveIcon.SetActive(true);
		}
		base.enabled = true;
	}

	
	private bool playerDoingAction()
	{
		return LocalPlayer.AnimControl.useRootMotion;
	}

	
	private void OnDestroy()
	{
		if (Scene.HudGui && this.SleepIcon && Scene.HudGui.SleepDelayIcon._target == this.SleepIcon.transform)
		{
			Scene.HudGui.SleepDelayIcon.gameObject.SetActive(false);
		}
	}

	
	
	private bool CanSleep
	{
		get
		{
			return Scene.Clock.ElapsedGameTime > Scene.Clock.NextSleepTime;
		}
	}

	
	public IEnumerator DelayedCollapse()
	{
		base.enabled = false;
		base.GetComponent<Collider>().enabled = false;
		yield return YieldPresets.WaitTwoSeconds;
		int childCount = base.transform.parent.childCount;
		for (int i = childCount - 1; i >= 0; i--)
		{
			if (base.transform.parent.GetChild(i) != base.transform)
			{
				UnityEngine.Object.Destroy(base.transform.parent.GetChild(i).gameObject);
			}
		}
		LocalPlayer.Sfx.PlayStructureFall(base.gameObject, 0.01f);
		UnityEngine.Object.Instantiate<GameObject>(Prefabs.Instance.DestroyedLeafShelter, base.transform.position, base.transform.rotation);
		yield return YieldPresets.WaitOneSecond;
		if (this && base.transform)
		{
			if (!BoltNetwork.isClient)
			{
				if (!BoltNetwork.isRunning)
				{
					UnityEngine.Object.Destroy(base.transform.parent.gameObject);
				}
				else
				{
					BoltNetwork.Destroy(base.transform.parent.gameObject);
				}
			}
			else
			{
				RequestDestroy requestDestroy = RequestDestroy.Create(GlobalTargets.OnlyServer);
				requestDestroy.Entity = base.transform.parent.GetComponent<BoltEntity>();
				requestDestroy.Send();
			}
		}
		yield break;
	}

	
	public static void CheckRegrowTrees()
	{
		if (PlayerPreferences.TreeRegrowth)
		{
			LOD_Trees[] source = UnityEngine.Object.FindObjectsOfType<LOD_Trees>();
			List<LOD_Trees> list = (from t in source
			where !t.enabled && t.CurrentView == null
			select t).ToList<LOD_Trees>();
			if (list != null && list.Count > 0)
			{
				TreeLodGrid treeLodGrid = UnityEngine.Object.FindObjectOfType<TreeLodGrid>();
				int count = list.Count;
				int num = count / 10 + 2;
				float num2 = Mathf.Max((float)count / (float)num, 1f);
				int num3 = 0;
				for (float num4 = 0f; num4 < (float)count; num4 += num2)
				{
					int index = (int)num4;
					if (BoltNetwork.isRunning)
					{
						CoopTreeId component = list[index].GetComponent<CoopTreeId>();
						if (component)
						{
							component.RegrowTree();
						}
						list[index].DontSpawn = false;
					}
					list[index].enabled = true;
					list[index].RefreshLODs();
					if (treeLodGrid)
					{
						treeLodGrid.RegisterTreeRegrowth(list[index].transform.position);
					}
					IEnumerator enumerator = list[index].transform.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							object obj = enumerator.Current;
							Transform transform = (Transform)obj;
							LOD_Stump component2 = transform.GetComponent<LOD_Stump>();
							if (component2)
							{
								component2.DespawnCurrent();
								component2.CurrentView = null;
							}
							UnityEngine.Object.Destroy(transform.gameObject);
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
					num3++;
				}
				if (num3 != 0 && BoltNetwork.isRunning)
				{
					CoopTreeGrid.SweepGrid();
				}
				Debug.Log(string.Concat(new object[]
				{
					"Tree regrowth: ",
					num3,
					"/",
					count
				}));
			}
		}
	}

	
	private bool PlayerCanSave()
	{
		bool flag = LocalPlayer.IsInEndgame && (LocalPlayer.SurfaceDetector.Surface != UnderfootSurfaceDetector.SurfaceType.Rock || !Area.IsActiveAreaTheStartArea());
		return !LocalPlayer.FpCharacter.SailingRaft && !flag;
	}

	
	public GameObject SleepIcon;

	
	public GameObject SaveIcon;

	
	public bool BreakAfterSleep;

	
	private int _previousPlayersReady;

	
	private float _mpSleepDelay = float.MinValue;

	
	private const float MpSleepDelayDuration = 1f;
}
