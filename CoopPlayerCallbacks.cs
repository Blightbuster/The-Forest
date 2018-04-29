using System;
using Bolt;
using FMOD.Studio;
using Steamworks;
using TheForest.Items;
using TheForest.Items.Core;
using TheForest.Items.Inventory;
using TheForest.Items.Special;
using TheForest.Items.World;
using TheForest.Player.Clothing;
using TheForest.Tools;
using TheForest.Utils;
using UdpKit;
using UniLinq;
using UnityEngine;
using UnityEngine.SceneManagement;


[BoltGlobalBehaviour]
public class CoopPlayerCallbacks : GlobalEventListener
{
	
	public override void BoltStartBegin()
	{
		CoopVoice.VoiceChannel = BoltNetwork.CreateStreamChannel(CoopPlayerCallbacks.CHANNEL_VOICE, UdpChannelMode.Unreliable, 1);
		Debug.Log("CoopPlayerCallbacks::BoltStartBegin CoopVoice.VoiceChannel:" + CoopVoice.VoiceChannel);
	}

	
	public override void BoltShutdownBegin(AddCallback registerDoneCallback)
	{
		CoopPlayerCallbacks._allTrees = null;
	}

	
	public override void StreamDataReceived(BoltConnection connection, UdpStreamData data)
	{
		int o = 0;
		BoltEntity boltEntity = BoltNetwork.FindEntity(new NetworkId(Blit.ReadU64(data.Data, ref o)));
		if (boltEntity.IsAttached())
		{
			CoopVoice component = boltEntity.GetComponent<CoopVoice>();
			if (component)
			{
				component.ReceiveVoiceData(data.Data, o);
			}
		}
	}

	
	public override void OnEvent(TakeBodyApprove evnt)
	{
		LocalPlayer.AnimControl.setMutantPickUp(evnt.Body.gameObject);
		SetCorpsePosition setCorpsePosition = SetCorpsePosition.Create(GlobalTargets.OnlyServer);
		setCorpsePosition.Corpse = evnt.Body;
		setCorpsePosition.Corpse.Freeze(false);
		setCorpsePosition.Pickup = true;
		setCorpsePosition.Send();
	}

	
	public override void OnEvent(Chop evnt)
	{
		if (evnt.Target && this.ValidateSender(evnt, SenderTypes.Any))
		{
			evnt.Target.GetComponentInChildren<chopEnemy>().triggerChop();
		}
	}

	
	public override void OnEvent(FauxWeaponHit evnt)
	{
		if (this.ValidateSender(evnt, SenderTypes.Any))
		{
			GameObject original = (GameObject)Resources.Load("CoopFauxWeapon", typeof(GameObject));
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, evnt.Position, Quaternion.identity);
			gameObject.GetComponent<CoopFauxWeapon>().Damage = evnt.Damage;
		}
	}

	
	public override void OnEvent(OpenSuitcase evnt)
	{
		if (this.ValidateSender(evnt, SenderTypes.Any))
		{
			foreach (Collider collider in Physics.OverlapSphere(evnt.Position, 0.5f))
			{
				if (collider.gameObject.CompareTag("suitCase"))
				{
					collider.SendMessage("Hit", evnt.Damage);
				}
			}
		}
	}

	
	
	public static CoopTreeId[] AllTrees
	{
		get
		{
			if (CoopPlayerCallbacks._allTrees == null)
			{
				CoopPlayerCallbacks._allTrees = (from x in UnityEngine.Object.FindObjectsOfType<CoopTreeId>()
				orderby x.Id
				select x).ToArray<CoopTreeId>();
			}
			return CoopPlayerCallbacks._allTrees;
		}
	}

	
	public static void ClearTrees()
	{
		CoopPlayerCallbacks._allTrees = null;
	}

	
	public override void OnEvent(ItemRemoveFromPlayer evnt)
	{
		if (this.ValidateSender(evnt, SenderTypes.Server))
		{
			if (evnt.ItemId == 0)
			{
				LocalPlayer.GameObject.GetComponentInChildren<LogControler>().RemoveLog(false);
			}
			else
			{
				LocalPlayer.Inventory.RemoveItem(evnt.ItemId, 1, false, true);
			}
		}
	}

	
	public override void OnEvent(DisablePickup evnt)
	{
		if (evnt.Entity && this.ValidateSender(evnt, SenderTypes.Any) && evnt.Entity.isAttached && evnt.Entity.isOwner)
		{
			evnt.Entity.SendMessage("SetPickupUsed", evnt.Num, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(TakeClothingOutfit evnt)
	{
		if (this.ValidateSender(evnt, SenderTypes.Any))
		{
			if (evnt.target == null)
			{
				return;
			}
			if (!evnt.target.isOwner)
			{
				return;
			}
			ClothingPickup componentInChildren = evnt.target.GetComponentInChildren<ClothingPickup>(true);
			if (componentInChildren == null)
			{
				return;
			}
			componentInChildren.DoBoltTaken();
		}
	}

	
	public override void OnEvent(DestroyPickUp evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.PickUpEntity)
		{
			if (evnt.PickUpEntity.isAttached)
			{
				if (evnt.PickUpEntity.isOwner)
				{
					if (evnt.PickUpEntity.StateIs<ISuitcaseState>())
					{
						if (evnt.SibblingId >= 0)
						{
							ISuitcaseState state = evnt.PickUpEntity.GetState<ISuitcaseState>();
							state.FlaresPickedUp |= 1 << evnt.SibblingId;
						}
						else
						{
							evnt.PickUpEntity.GetState<ISuitcaseState>().ClothPickedUp = true;
						}
					}
					else if (evnt.PickUpEntity.StateIs<IGardenDirtPileState>())
					{
						BoltNetwork.Destroy(evnt.PickUpEntity);
					}
					else
					{
						if (evnt.FakeDrop)
						{
							if (evnt.PickUpPlayer == LocalPlayer.Entity)
							{
								LocalPlayer.Inventory.FakeDrop(evnt.ItemId, null);
							}
							else
							{
								PlayerAddItem playerAddItem = PlayerAddItem.Create(evnt.PickUpPlayer.source);
								playerAddItem.ItemId = evnt.ItemId;
								playerAddItem.Amount = 1;
								playerAddItem.Player = evnt.PickUpPlayer;
								playerAddItem.Send();
							}
						}
						PickUp componentInChildren = evnt.PickUpEntity.GetComponentInChildren<PickUp>();
						if (componentInChildren && !componentInChildren.TryPool())
						{
							componentInChildren.CheckTrappedAnimal();
							BoltNetwork.Destroy(evnt.PickUpEntity);
						}
					}
				}
				else
				{
					DestroyPickUp destroyPickUp = DestroyPickUp.Raise(evnt.PickUpEntity.source);
					destroyPickUp.PickUpEntity = evnt.PickUpEntity;
					destroyPickUp.Send();
				}
			}
		}
		else if (evnt.PickUpPlayer.isOwner)
		{
			ItemRemoveFromPlayer itemRemoveFromPlayer = ItemRemoveFromPlayer.Create(GlobalTargets.OnlySelf);
			itemRemoveFromPlayer.ItemId = evnt.ItemId;
			itemRemoveFromPlayer.Send();
		}
		else
		{
			ItemRemoveFromPlayer itemRemoveFromPlayer2 = ItemRemoveFromPlayer.Create(evnt.PickUpPlayer.source);
			itemRemoveFromPlayer2.ItemId = evnt.ItemId;
			itemRemoveFromPlayer2.Send();
		}
	}

	
	public override void OnEvent(FmodOneShot evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (FMOD_StudioSystem.instance && evnt.EventPath != -1)
		{
			string text = CoopAudioEventDb.FindEvent(evnt.EventPath);
			if (text != null)
			{
				FMOD_StudioSystem.instance.PlayOneShot(text, evnt.Position, null);
			}
		}
	}

	
	public override void OnEvent(FmodOneShotParameter evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (FMOD_StudioSystem.instance && evnt.EventPath != -1)
		{
			string text = CoopAudioEventDb.FindEvent(evnt.EventPath);
			if (text != null)
			{
				FMOD_StudioSystem.instance.PlayOneShot(text, evnt.Position, delegate(EventInstance eventInstance)
				{
					eventInstance.setParameterValueByIndex(evnt.Index, evnt.Value);
					return true;
				});
			}
		}
	}

	
	public override void OnEvent(AdminAuthed evnt)
	{
		if (evnt.IsAdmin)
		{
			CoopSteamClientStarter.IsAdmin = true;
		}
	}

	
	public override void OnEvent(PlayerHitByEnemey evnt)
	{
		if (this.ValidateSender(evnt, SenderTypes.Server) && evnt.Target && evnt.Target.isOwner)
		{
			if (evnt.SharkHit)
			{
				LocalPlayer.GameObject.SendMessage("getHitDirection", evnt.Direction);
				LocalPlayer.GameObject.SendMessage("HitShark", evnt.Damage);
				return;
			}
			LocalPlayer.GameObject.SendMessage("hitFromEnemy", evnt.Damage);
		}
	}

	
	public override void OnEvent(ChatEvent evnt)
	{
		if (CoopPeerStarter.Dedicated && evnt.RaisedBy == BoltNetwork.server && evnt.Sender.IsZero)
		{
			TheForest.Utils.Scene.HudGui.Chatbox.AddLine(null, evnt.Message, true);
		}
		else if (this.ValidateSender(evnt, SenderTypes.Any))
		{
			TheForest.Utils.Scene.HudGui.Chatbox.AddLine(new NetworkId?(evnt.Sender), evnt.Message, false);
		}
	}

	
	public override void EntityAttached(BoltEntity arg)
	{
		if (arg.StateIs<IPlayerState>())
		{
			if (arg.isOwner)
			{
				DebugInfo.Ignore(arg);
				TheForest.Utils.Scene.HudGui.Chatbox.RegisterPlayer("You", arg.networkId);
			}
			else
			{
				arg.source.UserData = arg;
				arg.GetState<IPlayerState>().AddCallback("name", delegate
				{
					TheForest.Utils.Scene.HudGui.Chatbox.RegisterPlayer(arg.GetState<IPlayerState>().name, arg.networkId);
				});
			}
		}
	}

	
	public override void OnEvent(RequestDestroy evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Entity && evnt.Entity.isAttached && evnt.Entity.isOwner && (evnt.Entity.StateIs<ICookingState>() || evnt.Entity.GetComponentInChildren<ShelterTrigger>()))
		{
			for (int i = 0; i < TheForest.Utils.Scene.SceneTracker.allPlayerEntities.Count; i++)
			{
				if (TheForest.Utils.Scene.SceneTracker.allPlayerEntities[i].source == evnt.RaisedBy)
				{
					if (Vector3.Distance(TheForest.Utils.Scene.SceneTracker.allPlayerEntities[i].transform.position, evnt.Entity.transform.position) < 20f)
					{
						evnt.Entity.transform.parent = null;
						BoltNetwork.Destroy(evnt.Entity);
					}
					break;
				}
			}
			if (BoltNetwork.isServer && evnt.RaisedBy == null && Vector3.Distance(LocalPlayer.Transform.position, evnt.Entity.transform.position) < 9f)
			{
				evnt.Entity.transform.parent = null;
				BoltNetwork.Destroy(evnt.Entity);
			}
		}
	}

	
	public override void EntityDetached(BoltEntity arg)
	{
		if (arg.IsAttached() && arg.StateIs<IPlayerState>())
		{
			try
			{
				if (arg.source.UserData as BoltEntity == arg)
				{
					arg.source.UserData = null;
				}
				TheForest.Utils.Scene.HudGui.Chatbox.UnregisterPlayer(arg.networkId);
			}
			catch
			{
			}
		}
	}

	
	public override void Connected(BoltConnection connection)
	{
		connection.SetStreamBandwidth(40000);
	}

	
	public override void Disconnected(BoltConnection connection)
	{
		if (SteamClientDSConfig.isDedicatedClient)
		{
			return;
		}
		if (BoltNetwork.isClient && CoopClientCallbacks.OnDisconnected == null && !CoopSteamClientStarter.Retrying)
		{
			if (CoopLobby.IsInLobby)
			{
				SteamMatchmaking.LeaveLobby(CoopLobby.Instance.Info.LobbyId);
			}
			CoopPlayerCallbacks._allTrees = null;
			CoopPlayerCallbacks.WasDisconnectedFromServer = true;
			BoltLauncher.Shutdown();
			if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.EndCrash && !Application.loadedLevelName.ToLower().Contains("epilogue"))
			{
				SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
			}
		}
	}

	
	private void OnApplicationQuit()
	{
		if (BoltNetwork.isRunning)
		{
			if (BoltNetwork.isClient)
			{
				BoltNetwork.server.Disconnect();
			}
			BoltLauncher.Shutdown();
		}
	}

	
	public override void OnEvent(HitTree evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		CoopTreeId coopTreeId = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId x) => x.Id == evnt.TreeId);
		if (coopTreeId)
		{
			TreeHealth currentView = coopTreeId.GetComponent<LOD_Trees>().CurrentView;
			if (currentView)
			{
				Transform transform = currentView.transform.Find(evnt.ChunkParent);
				if (transform)
				{
					Transform transform2 = transform.Find("TreeDmg" + evnt.ChunkParent);
					if (transform2)
					{
						transform2.SendMessage("ActivateFake", evnt.ChunkId);
					}
				}
			}
		}
	}

	
	public override void OnEvent(PlayerHealed evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.HealTarget == LocalPlayer.Entity && !TheForest.Utils.Scene.Cams.DeadCam.activeSelf)
		{
			LocalPlayer.Stats.HealedMp();
		}
	}

	
	public override void OnEvent(StealItem evnt)
	{
		if (BoltNetwork.isServer && evnt.robbed != LocalPlayer.Entity)
		{
			StealItem stealItem = StealItem.Create(evnt.robbed.source);
			stealItem.thief = evnt.thief;
			stealItem.robbed = evnt.robbed;
			stealItem.Send();
		}
		else if (evnt.robbed == LocalPlayer.Entity)
		{
			if (!LocalPlayer.Inventory.IsRightHandEmpty())
			{
				ItemStorageProxy component = LocalPlayer.Inventory.RightHand._held.GetComponent<ItemStorageProxy>();
				if (component)
				{
					for (int i = 0; i < component._storage.UsedSlots.Count; i++)
					{
						PlayerAddItem playerAddItem = PlayerAddItem.Create(GlobalTargets.OnlyServer);
						playerAddItem.Player = evnt.thief;
						playerAddItem.ItemId = component._storage.UsedSlots[i]._itemId;
						playerAddItem.Amount = component._storage.UsedSlots[i]._amount;
						component._storage.UsedSlots[i]._properties.Fill(playerAddItem);
						playerAddItem.Send();
						Item item = ItemDatabase.ItemById(playerAddItem.ItemId);
						if (item.MatchType(Item.Types.Weapon))
						{
							EventRegistry.Achievements.Publish(TfEvent.Achievements.SharedWeapon, null);
						}
						else if (item.MatchType(Item.Types.Edible))
						{
							EventRegistry.Achievements.Publish(TfEvent.Achievements.SharedEdible, null);
						}
					}
					component._storage.Close();
					component._storage.UsedSlots.Clear();
					component._storage.UpdateContentVersion();
					component.CheckContentVersion();
				}
				else
				{
					PlayerAddItem playerAddItem = PlayerAddItem.Create(GlobalTargets.OnlyServer);
					playerAddItem.Player = evnt.thief;
					playerAddItem.ItemId = LocalPlayer.Inventory.RightHand._itemId;
					LocalPlayer.Inventory.RightHand.Properties.Fill(playerAddItem);
					playerAddItem.Send();
					LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, false);
				}
			}
		}
		else if (evnt.robbed.source == LocalPlayer.Entity.source)
		{
			ItemStorage component2 = evnt.robbed.GetComponent<ItemStorage>();
			if (component2)
			{
				for (int j = 0; j < component2.UsedSlots.Count; j++)
				{
					PlayerAddItem playerAddItem2 = PlayerAddItem.Create(GlobalTargets.OnlyServer);
					playerAddItem2.Player = evnt.thief;
					playerAddItem2.ItemId = component2.UsedSlots[j]._itemId;
					playerAddItem2.Amount = component2.UsedSlots[j]._amount;
					playerAddItem2.Send();
				}
				BoltNetwork.Destroy(component2.gameObject);
			}
		}
	}

	
	public override void OnEvent(PlayerAddItem evnt)
	{
		if (BoltNetwork.isServer && evnt.Player != null && evnt.Player != LocalPlayer.Entity)
		{
			PlayerAddItem playerAddItem = PlayerAddItem.Create(evnt.Player.source);
			playerAddItem.ItemId = evnt.ItemId;
			playerAddItem.Amount = evnt.Amount;
			playerAddItem.ActiveBonus = evnt.ActiveBonus;
			playerAddItem.ActiveBonusValue = evnt.ActiveBonusValue;
			playerAddItem.IntVal1 = evnt.IntVal1;
			playerAddItem.FloatVal1 = evnt.FloatVal1;
			playerAddItem.ItemPropertiesType = evnt.ItemPropertiesType;
			playerAddItem.Send();
		}
		else
		{
			int num = (evnt.Amount <= 1) ? 1 : evnt.Amount;
			if (!LocalPlayer.Inventory.AddItem(evnt.ItemId, num, false, false, ItemProperties.CreateFrom(evnt)))
			{
				for (int i = 0; i < num; i++)
				{
					LocalPlayer.Inventory.FakeDrop(evnt.ItemId, null);
				}
			}
		}
	}

	
	public override void OnEvent(Sleep evnt)
	{
		if (BoltNetwork.isClient && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Sleep)
		{
			Debug.Log("Go to sleep");
			TheForest.Utils.Scene.HudGui.MpSleepLabel.gameObject.SetActive(false);
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
			if (!evnt.Aborted)
			{
				LocalPlayer.Stats.GoToSleep();
				if (Grabber.FocusedItemGO)
				{
					ShelterTrigger component = Grabber.FocusedItemGO.GetComponent<ShelterTrigger>();
					if (component && component.BreakAfterSleep)
					{
						base.StartCoroutine(component.DelayedCollapse());
					}
					component.SendMessage("OnSleep", SendMessageOptions.DontRequireReceiver);
				}
				EventRegistry.Player.Publish(TfEvent.Slept, null);
			}
			else
			{
				LocalPlayer.Stats.GoToSleepFake();
			}
			TheForest.Utils.Scene.HudGui.Grid.repositionNow = true;
		}
	}

	
	public static bool WasDisconnectedFromServer;

	
	private static CoopTreeId[] _allTrees;

	
	public static string CHANNEL_VOICE = "Voice";
}
