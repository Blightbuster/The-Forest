using System;
using System.Collections;
using PathologicalGames;
using TheForest.Items;
using TheForest.Items.Core;
using TheForest.Items.Inventory;
using TheForest.Networking;
using TheForest.Player;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

namespace TheForest.Save
{
	
	[DoNotSerializePublic]
	public class PlayerRespawnMP : MonoBehaviour
	{
		
		private void Awake()
		{
			PlayerRespawnMP.Instance = this;
			base.enabled = false;
		}

		
		private void Update()
		{
			if (!Scene.Cams.DeadCam.activeSelf)
			{
				if (!Scene.HudGui.MpRespawnLabel.gameObject.activeSelf && this._deathTime + this._minDownDuration < Time.time)
				{
					Scene.HudGui.MpRespawnLabel.gameObject.SetActive(true);
				}
				else if ((TheForest.Utils.Input.GetButtonDown("Take") && Scene.HudGui.MpRespawnMaxTimer.gameObject.activeSelf) || this._deathTime + this._maxDownDuration < Time.time)
				{
					Debug.Log("killed player");
					PlayerRespawnMP.KillPlayer();
				}
			}
			else if (this._manualRespawnTime + this._respawnDuration < Time.time)
			{
				this.Respawn();
			}
			Scene.HudGui.MpRespawnMaxTimer.fillAmount = Mathf.Clamp01((Time.time - this._deathTime) / this._maxDownDuration);
		}

		
		private void OnDestroy()
		{
			if (PlayerRespawnMP.Instance == this)
			{
				PlayerRespawnMP.Instance = null;
			}
		}

		
		public static void TakeDownPlayer()
		{
			Scene.HudGui.MpRespawnMaxTimer.fillAmount = 0f;
			Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(true);
			Scene.SceneTracker.allPlayers.Remove(LocalPlayer.GameObject);
			LocalPlayer.Create.CancelPlace();
			LocalPlayer.Create.CloseTheBook(false);
			LocalPlayer.Inventory.Close();
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Death;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.enabled = false;
			LocalPlayer.GameObject.GetComponent<Rigidbody>().isKinematic = true;
			LocalPlayer.GameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
			PlayerRespawnMP.Instance._deathTime = Time.time;
			PlayerRespawnMP.Instance.enabled = true;
		}

		
		public static void offsetDeathTime()
		{
			PlayerRespawnMP.Instance._deathTime = Time.time;
		}

		
		public static void enableRespawnTimer()
		{
			Scene.HudGui.MpRespawnMaxTimer.fillAmount = 0f;
			Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(true);
			Scene.SceneTracker.allPlayers.Remove(LocalPlayer.GameObject);
			LocalPlayer.Create.CancelPlace();
			LocalPlayer.Create.CloseTheBook(false);
			LocalPlayer.Inventory.Close();
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Death;
			PlayerRespawnMP.Instance._deathTime = Time.time;
			PlayerRespawnMP.Instance.enabled = true;
			Scene.HudGui.Chatbox.ForceRefreshInput();
		}

		
		public static bool IsKillable()
		{
			return PlayerRespawnMP.Instance._deathTime + 0.5f < Time.time && PlayerRespawnMP.Instance.enabled;
		}

		
		public static void KillPlayer()
		{
			if (LocalPlayer.IsInEndgame && LocalPlayer.Stats.IsFightingBoss)
			{
				LocalPlayer.PlayerDeadCam.SetActive(false);
				Scene.HudGui.ShowHud(true);
				Scene.HudGui.MpRespawnLabel.gameObject.SetActive(false);
				Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(false);
				if (!Scene.SceneTracker.allPlayers.Contains(LocalPlayer.GameObject))
				{
					Scene.SceneTracker.allPlayers.Add(LocalPlayer.GameObject);
				}
				Scene.HudGui.Chatbox.ForceRefreshInput();
				PlayerRespawnMP.Instance.enabled = false;
				Scene.ActiveMB.StartCoroutine(LocalPlayer.Stats.EndgameWakeUp());
			}
			else if (!Scene.Cams.DeadCam.activeSelf)
			{
				if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Pause)
				{
					Scene.HudGui.ShowHud(true);
					PlayerRespawnMP.Instance._bringUpPauseMenu = true;
				}
				else
				{
					PlayerRespawnMP.Instance._bringUpPauseMenu = false;
				}
				LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.WakingUp;
				Scene.Cams.DeadCam.SetActive(true);
				Scene.HudGui.ShowHud(false);
				Scene.HudGui.PauseMenu.SetActive(false);
				LocalPlayer.PlayerDeadCam.SetActive(true);
				Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(false);
				Scene.HudGui.MpRespawnLabel.gameObject.SetActive(false);
				Scene.ActiveMB.StartCoroutine(Scene.HudGui.Chatbox.Close());
				LocalPlayer.Animator.SetBool("deathFromInjuredBool", true);
				PlayerRespawnMP.Instance._manualRespawnTime = Time.time;
				PlayerRespawnMP.Instance.enabled = true;
			}
		}

		
		public static void Cancel()
		{
			Scene.HudGui.ShowHud(true);
			Scene.Cams.DeadCam.SetActive(false);
			LocalPlayer.PlayerDeadCam.SetActive(false);
			Scene.HudGui.MpRespawnLabel.gameObject.SetActive(false);
			Scene.HudGui.MpRespawnMaxTimer.gameObject.SetActive(false);
			if (!Scene.SceneTracker.allPlayers.Contains(LocalPlayer.GameObject))
			{
				Scene.SceneTracker.allPlayers.Add(LocalPlayer.GameObject);
			}
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
			LocalPlayer.Inventory.enabled = true;
			LocalPlayer.FpCharacter.enabled = true;
			LocalPlayer.CamFollowHead.followAnim = false;
			LocalPlayer.CamRotator.resetOriginalRotation = true;
			LocalPlayer.MainRotator.enabled = true;
			LocalPlayer.CamRotator.enabled = true;
			LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
			LocalPlayer.MainCamTr.localEulerAngles = Vector3.zero;
			LocalPlayer.Transform.eulerAngles = new Vector3(0f, LocalPlayer.Transform.eulerAngles.y, 0f);
			LocalPlayer.PlayerBase.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			LocalPlayer.AnimControl.StartCoroutine("smoothEnableLayerNew", 4);
			LocalPlayer.ScriptSetup.pmControl.SendEvent("toResetPlayer");
			LocalPlayer.GameObject.SendMessage("GotBloody");
			LocalPlayer.GameObject.GetComponent<Rigidbody>().isKinematic = false;
			if (LocalPlayer.Stats.Fullness < 0.35f)
			{
				LocalPlayer.Stats.Fullness = 0.35f;
			}
			if (LocalPlayer.Stats.Thirst > 0.35f)
			{
				LocalPlayer.Stats.Thirst = 0.35f;
			}
			PlayerRespawnMP.Instance.enabled = false;
			Scene.HudGui.Chatbox.ForceRefreshInput();
		}

		
		private void forceAnimSpineReset()
		{
			LocalPlayer.Animator.SetLayerWeight(4, 1f);
		}

		
		private void Respawn()
		{
			Debug.Log("LocalPlayer -> Respawn");
			if (LocalPlayer.Stats.Dead)
			{
				if (LocalPlayer.IsInEndgame)
				{
					GameObject gameObject = GameObject.FindWithTag("EndgameLoader");
					if (gameObject)
					{
						SceneLoadTrigger component = gameObject.GetComponent<SceneLoadTrigger>();
						component.ForceUnload();
					}
				}
				EventRegistry.Player.Publish(TfEvent.ExitOverlookArea, null);
				EventRegistry.Player.Publish(TfEvent.ExitEndgame, null);
				LocalPlayer.GameObject.SendMessage("NotInACave");
				PlayerInventory inventory = LocalPlayer.Inventory;
				AchievementsManager achievements = LocalPlayer.Achievements;
				string name = LocalPlayer.Entity.GetState<IPlayerState>().name;
				LocalPlayer.Inventory.HideAllEquiped(false, false);
				LocalPlayer.Inventory.enabled = false;
				if (Scene.SceneTracker.allPlayers.Contains(LocalPlayer.GameObject))
				{
					Scene.SceneTracker.allPlayers.Remove(LocalPlayer.GameObject);
				}
				if (Scene.SceneTracker.allPlayerEntities.Contains(LocalPlayer.Entity))
				{
					Scene.SceneTracker.allPlayerEntities.Remove(LocalPlayer.Entity);
				}
				BoltNetwork.Detach(LocalPlayer.Entity);
				GameObject gameObject2 = LocalPlayer.GameObject;
				BoltEntity entity = LocalPlayer.Entity;
				gameObject2.name = "player Corpse - " + name;
				gameObject2.tag = "Untagged";
				LocalPlayer.MainCamTr.parent = LocalPlayer.Transform;
				LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Loot;
				if (LocalPlayer.AnimControl.swimming)
				{
					LocalPlayer.Rigidbody.useGravity = true;
				}
				for (int i = gameObject2.transform.childCount - 1; i >= 0; i--)
				{
					Transform child = gameObject2.transform.GetChild(i);
					UnityEngine.Object.Destroy(child.gameObject);
				}
				Component[] components = gameObject2.GetComponents(typeof(MonoBehaviour));
				foreach (Component component2 in components)
				{
					if (!(component2 is BoltEntity))
					{
						UnityEngine.Object.DestroyImmediate(component2);
					}
				}
				Transform transform = base.transform;
				GameObject gameObject3 = (GameObject)UnityEngine.Object.Instantiate(Prefabs.Instance.PlayerPrefab, transform.position, transform.rotation);
				gameObject3.transform.localEulerAngles = new Vector3(0f, gameObject3.transform.localEulerAngles.y, 0f);
				gameObject3.name = Prefabs.Instance.PlayerPrefab.name;
				LocalPlayer.Inventory.SetQuickSelectItemIds(inventory.QuickSelectItemIds);
				LocalPlayer.Achievements.Clone(achievements);
				LocalPlayer.FpCharacter.UnLockView();
				LocalPlayer.CamFollowHead.enableMouseControl(false);
				LocalPlayer.MainCamTr.localEulerAngles = Vector3.zero;
				LocalPlayer.MainRotator.enabled = true;
				LocalPlayer.CamRotator.enabled = true;
				LocalPlayer.Stats.Health = 28f;
				LocalPlayer.Stats.HealthTarget = 28f;
				LocalPlayer.Stats.Energy = 100f;
				LocalPlayer.Stats.Fullness = 0.35f;
				LocalPlayer.Stats.Thirst = 0.35f;
				LocalPlayer.Stats.Invoke("CheckArmsStart", 2f);
				LocalPlayer.Stats.Invoke("PlayWakeMusic", 0.5f);
				LocalPlayer.Tuts.CloseColdTut();
				Scene.RainFollowGO.GetComponent<SmoothTransformConstraint>().target = LocalPlayer.Transform;
				gameObject3.SetActive(true);
				CoopUtils.AttachLocalPlayer(gameObject3, name);
				Scene.SceneTracker.allPlayers.Add(LocalPlayer.GameObject);
				LocalPlayer.GreebleRoot.SetActive(true);
				LocalPlayer.Inventory.enabled = true;
				LocalPlayer.Transform.SendMessage("enableMpRenderers");
				LocalPlayer.Transform.SendMessage("playerLoadedFromRespawn");
				StealItemTrigger stealItemTrigger = (StealItemTrigger)UnityEngine.Object.Instantiate(Prefabs.Instance.DeadBackpackPrefab, gameObject2.transform.position, gameObject2.transform.rotation);
				stealItemTrigger._entity = entity;
				stealItemTrigger.transform.parent = gameObject2.transform;
				gameObject2.AddComponent<DeathMPTut>();
				ItemStorage cis = gameObject2.AddComponent<ItemStorage>();
				for (int k = inventory._possessedItems.Count - 1; k >= 0; k--)
				{
					InventoryItem inventoryItem = inventory._possessedItems[k];
					if (!LocalPlayer.Inventory.Owns(inventoryItem._itemId, false))
					{
						if (inventoryItem.MaxAmount == 1)
						{
							InventoryItemView inventoryItemView = inventory.InventoryItemViewsCache[inventoryItem._itemId][0];
							this.AddItemToStorage(inventoryItem._itemId, inventoryItem._amount, cis, inventoryItemView.Properties);
						}
						else if (inventoryItem.MaxAmount > 0 && inventoryItem.MaxAmount < 2147483647)
						{
							while (inventory.Owns(inventoryItem._itemId, false))
							{
								InventoryItemView inventoryItemView2 = inventory.InventoryItemViewsCache[inventoryItem._itemId][0];
								inventory.SortInventoryViewsByBonus(inventoryItemView2, inventoryItemView2.Properties.ActiveBonus, true);
								int amount = inventory.AmountOfItemWithBonus(inventoryItem._itemId, inventoryItemView2.Properties.ActiveBonus);
								inventory.RemoveItem(inventoryItem._itemId, amount, true, false);
								this.AddItemToStorage(inventoryItem._itemId, amount, cis, inventoryItemView2.Properties);
							}
						}
						else
						{
							this.AddItemToStorage(inventoryItem._itemId, inventoryItem._amount, cis, null);
						}
					}
				}
				for (int l = 0; l < inventory.EquipmentSlots.Length; l++)
				{
					InventoryItemView inventoryItemView3 = inventory.EquipmentSlots[l];
					if (inventoryItemView3 && inventoryItemView3._itemId > 0)
					{
						this.AddItemToStorage(inventoryItemView3._itemId, 1, cis, inventoryItemView3.Properties);
					}
				}
				animalAI[] array2 = UnityEngine.Object.FindObjectsOfType<animalAI>();
				foreach (animalAI animalAI in array2)
				{
					animalAI.SendMessage("updatePlayerTargets");
				}
				mutantAI[] array4 = UnityEngine.Object.FindObjectsOfType<mutantAI>();
				foreach (mutantAI mutantAI in array4)
				{
					mutantAI.SendMessage("updatePlayerTargets");
				}
				Fish[] array6 = UnityEngine.Object.FindObjectsOfType<Fish>();
				mutantScriptSetup[] array7 = UnityEngine.Object.FindObjectsOfType<mutantScriptSetup>();
				foreach (mutantScriptSetup mutantScriptSetup in array7)
				{
					mutantScriptSetup.setupPlayer();
					mutantScriptSetup.search.refreshCurrentTarget();
				}
				LocalPlayer.Transform.SendMessage("enableMpRenderers");
				Terrain.activeTerrain.GetComponent<Collider>().enabled = true;
				Scene.Clock.IsNotCave();
			}
			PlayerRespawnMP.Cancel();
			if (this._bringUpPauseMenu)
			{
				this._bringUpPauseMenu = false;
				LocalPlayer.Inventory.TogglePauseMenu();
			}
		}

		
		private void AddItemToStorage(int itemId, int amount, ItemStorage cis, ItemProperties properties = null)
		{
			if (ItemDatabase.IsItemidValid(itemId))
			{
				Item item = ItemDatabase.ItemById(itemId);
				if (item != null)
				{
					if (!item.MatchType(Item.Types.Story))
					{
						cis.Add(itemId, amount, properties);
					}
					else if (amount > 0)
					{
						base.StartCoroutine(this.DelayedAddItem(itemId, amount));
					}
				}
				else
				{
					Debug.LogError("Item not found while creating loot backpack id=" + itemId);
				}
			}
		}

		
		private IEnumerator DelayedAddItem(int itemId, int amount)
		{
			yield return YieldPresets.WaitPointFiveSeconds;
			LocalPlayer.Inventory.AddItem(itemId, amount, true, true, null);
			yield break;
		}

		
		private void SetLayerRecursively(Transform tr, LayerMask layer)
		{
			tr.gameObject.layer = layer;
			foreach (object obj in tr)
			{
				Transform tr2 = (Transform)obj;
				this.SetLayerRecursively(tr2, layer);
			}
		}

		
		
		
		public static PlayerRespawnMP Instance
		{
			get
			{
				if (!PlayerRespawnMP._instance)
				{
					GameObject gameObject = new GameObject("PlayerPlanePosition");
					GameObject gameObject2 = GameObject.Find("Hull(Clone)") ?? GameObject.Find("Hull");
					gameObject.transform.parent = gameObject2.transform;
					gameObject.transform.localPosition = new Vector3(0f, 0.0335f, 0f);
					PlayerRespawnMP._instance = gameObject.AddComponent<PlayerRespawnMP>();
				}
				return PlayerRespawnMP._instance;
			}
			set
			{
				PlayerRespawnMP._instance = value;
			}
		}

		
		public float _minDownDuration = 5f;

		
		public float _respawnDuration = 5f;

		
		public float _maxDownDuration = 120f;

		
		public bool _blockDeathInput;

		
		private GameObject _lastCorpse;

		
		private float _deathTime;

		
		private float _manualRespawnTime;

		
		private bool _bringUpPauseMenu;

		
		private static PlayerRespawnMP _instance;
	}
}
