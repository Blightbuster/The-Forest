using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Steamworks;
using TheForest.Buildings.World;
using TheForest.Tools;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;


[BoltGlobalBehaviour(BoltNetworkModes.Client)]
public class CoopClientCallbacks : GlobalEventListener
{
	
	private void Start()
	{
		base.StartCoroutine("ServerResponseTest");
	}

	
	public override void OnEvent(CreepHitPlayer evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Server))
		{
			return;
		}
		LocalPlayer.GameObject.SendMessage("Explosion", evnt.damage, SendMessageOptions.DontRequireReceiver);
		if (evnt.Type == 11)
		{
			EventRegistry.Enemy.Publish(TfEvent.EnemyContact, EnemyType.creepyFat);
		}
	}

	
	private IEnumerator ServerResponseTest()
	{
		for (;;)
		{
			yield return null;
			if (BoltNetwork.isRunning && BoltNetwork.isClient && BoltNetwork.server != null)
			{
				if (BoltNetwork.server.DejitterFrames < -300)
				{
					try
					{
						CoopClientCallbacks.invokedServerNotResponding = true;
						if (CoopClientCallbacks.ServerNotResponding != null)
						{
							CoopClientCallbacks.ServerNotResponding();
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				else if (CoopClientCallbacks.invokedServerNotResponding)
				{
					CoopClientCallbacks.invokedServerNotResponding = false;
					if (CoopClientCallbacks.ServerIsResponding != null)
					{
						CoopClientCallbacks.ServerIsResponding();
					}
				}
			}
		}
		yield break;
	}

	
	private CoopComponentDisabler InitCCD(BoltEntity entity)
	{
		CoopComponentDisabler coopComponentDisabler = entity.GetComponent<CoopComponentDisabler>();
		if (!coopComponentDisabler)
		{
			coopComponentDisabler = entity.gameObject.AddComponent<CoopComponentDisabler>();
		}
		return coopComponentDisabler;
	}

	
	public override void Connected(BoltConnection connection)
	{
		SteamClientConfig.Banned = false;
		SteamClientConfig.KickMessage = string.Empty;
	}

	
	public override void Disconnected(BoltConnection connection)
	{
		CoopKickToken coopKickToken = connection.DisconnectToken as CoopKickToken;
		if (coopKickToken != null)
		{
			SteamClientConfig.Banned = coopKickToken.Banned;
			SteamClientConfig.KickMessage = UiTranslationDatabase.TranslateKey(coopKickToken.KickMessage, "Kicked/Banned", false);
			CoopSteamClientStarter coopSteamClientStarter = UnityEngine.Object.FindObjectOfType<CoopSteamClientStarter>();
			if (coopSteamClientStarter)
			{
				coopSteamClientStarter.CancelInvoke("OnDisconnected");
			}
			CoopClientCallbacks.OnDisconnected = null;
			if (SteamClientDSConfig.isDedicatedClient)
			{
				BoltLauncher.Shutdown();
				UnityEngine.Object.Destroy(base.gameObject);
				SteamUser.TerminateGameConnection(SteamClientDSConfig.EndPoint.Address.Packed, SteamClientDSConfig.EndPoint.Port);
				this.ReturnToTitle();
			}
			else if (!TheForest.Utils.Scene.FinishGameLoad)
			{
				this.ReturnToTitle();
			}
		}
		else
		{
			SteamClientConfig.Banned = false;
			SteamClientConfig.KickMessage = string.Empty;
			if (SteamClientDSConfig.isDedicatedClient)
			{
				CoopJoinDedicatedServerFailed coopJoinDedicatedServerFailed = connection.DisconnectToken as CoopJoinDedicatedServerFailed;
				if (coopJoinDedicatedServerFailed != null)
				{
					CoopClientCallbacks.OnDisconnected = null;
					CoopPlayerCallbacks.WasDisconnectedFromServer = true;
					CoopSteamClientStarter coopSteamClientStarter2 = UnityEngine.Object.FindObjectOfType<CoopSteamClientStarter>();
					if (coopSteamClientStarter2)
					{
						coopSteamClientStarter2._connectionAttempts = 3;
						coopSteamClientStarter2.CancelInvoke("OnDisconnected");
						UnityEngine.Object.Destroy(coopSteamClientStarter2.gameObject);
					}
					BoltLauncher.Shutdown();
					SteamClientConfig.KickMessage = "Incorrect password";
					UnityEngine.Object.Destroy(base.gameObject);
					this.ReturnToTitle();
				}
			}
			if (CoopClientCallbacks.OnDisconnected != null)
			{
				CoopClientCallbacks.OnDisconnected();
			}
		}
		if (SteamClientDSConfig.isDedicatedClient)
		{
			SteamUser.TerminateGameConnection(SteamClientDSConfig.EndPoint.Address.Packed, SteamClientDSConfig.EndPoint.Port);
			this.ReturnToTitle();
		}
	}

	
	private void ReturnToTitle()
	{
		if (!SceneManager.GetSceneByName("TitleScene").isLoaded)
		{
			SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
		}
	}

	
	public override void OnEvent(ServerStatusInfo evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		Debug.Log("Receive server status code: " + evnt.Status);
		SteamDSConfig.ReceiveServerStatus(evnt.Status);
	}

	
	public override void OnEvent(SetMasterClient evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		Debug.Log("Setting this client. Is master? " + evnt.IsMaster);
		SteamClientDSConfig.isDSFirstClient = evnt.IsMaster;
	}

	
	public override void OnEvent(ItemHolderTakeItem evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			Component[] componentsInChildren = evnt.Target.GetComponentsInChildren(typeof(MultiHolder), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as MultiHolder).TakeItemMP(null, (MultiHolder.ContentTypes)evnt.ContentType);
			}
		}
	}

	
	public override void OnEvent(PerformRepairBuilding evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Building)
		{
			evnt.Building.GetComponentInChildren<BuildingHealth>().RespawnBuilding();
		}
	}

	
	public override void OnEvent(ItemHolderAddItem evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			Component[] componentsInChildren = evnt.Target.GetComponentsInChildren(typeof(MultiHolder), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as MultiHolder).AddItemMP((MultiHolder.ContentTypes)evnt.ContentType, evnt.RaisedBy);
			}
		}
	}

	
	public override void OnEvent(FireAddFuelEvent evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target && evnt.Target.GetComponentInChildren<Fire2>())
		{
			evnt.Target.GetComponentInChildren<Fire2>().AddFuel_Complete();
		}
	}

	
	public override void EntityFrozen(BoltEntity entity)
	{
		if (entity.StateIs<IMutantState>() || entity.StateIs<IAnimalState>())
		{
			if (entity.StateIs<IMutantFemaleDummyState>())
			{
				return;
			}
			if (entity.StateIs<IMutantMaleDummyState>())
			{
				return;
			}
			if (!entity.transform.GetComponent<CoopMutantDummy>())
			{
				CoopComponentDisabler coopComponentDisabler = this.InitCCD(entity);
				coopComponentDisabler.DisableComponents();
			}
		}
	}

	
	public override void EntityThawed(BoltEntity entity)
	{
		if (entity.StateIs<IMutantState>() || entity.StateIs<IAnimalState>())
		{
			if (entity.StateIs<IMutantFemaleDummyState>())
			{
				return;
			}
			if (entity.StateIs<IMutantMaleDummyState>())
			{
				return;
			}
			if (!entity.transform.GetComponent<CoopMutantDummy>())
			{
				CoopComponentDisabler coopComponentDisabler = this.InitCCD(entity);
				coopComponentDisabler.EnableComponents();
			}
		}
	}

	
	public override void OnEvent(DroppedBody evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			if (evnt.dropFromTrap)
			{
				evnt.Target.SendMessage("releaseFromSpikeTrap", SendMessageOptions.DontRequireReceiver);
				return;
			}
			evnt.Target.SendMessage("clientDrop", evnt.rot, SendMessageOptions.DontRequireReceiver);
			evnt.Target.SendMessage("setRagDollDrop", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(ragdollActivate evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			evnt.Target.SendMessage("enableNetRagDoll", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(storeRagDollName evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		evnt.Target.SendMessage("getRagDollName", evnt.name, SendMessageOptions.DontRequireReceiver);
	}

	
	public override void OnEvent(mutantPoison evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.target != null)
		{
			evnt.target.SendMessage("setSkinColor", evnt.skinColor, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(CutTriggerActivated evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Trap)
		{
			Component[] componentsInChildren = evnt.Trap.GetComponentsInChildren(typeof(trapTrigger), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as trapTrigger).releaseNooseTrap();
			}
		}
	}

	
	public override void OnEvent(ValidSleepTime evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		TheForest.Utils.Scene.Clock.NextSleepTime = evnt.NextSleepTime;
	}

	
	public override void OnEvent(TriggerFishTrap evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		Debug.Log("TriggerFishTrap client");
		if (evnt.Trap != null)
		{
			trapTrigger componentInChildren = evnt.Trap.GetComponentInChildren<trapTrigger>();
			if (componentInChildren)
			{
				if (evnt.Fish == null || evnt.Fish == string.Empty)
				{
					base.StartCoroutine(componentInChildren.SetTrapFish(string.Empty, string.Empty, Vector3.zero));
				}
				else
				{
					Debug.Log("Call SetTrapFish");
					base.StartCoroutine(componentInChildren.SetTrapFish(evnt.Fish, evnt.Spawnner, evnt.Position));
				}
			}
		}
	}

	
	public override void OnEvent(SetTrappedEnemy evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Enemy != null && evnt.Trap != null)
		{
			mutantScriptSetup componentInChildren = evnt.Enemy.gameObject.transform.root.GetComponentInChildren<mutantScriptSetup>();
			if (componentInChildren)
			{
				trapTrigger componentInChildren2 = evnt.Trap.GetComponentInChildren<trapTrigger>();
				if (componentInChildren2)
				{
					componentInChildren2.SetNooseRope(componentInChildren);
				}
			}
			else
			{
				CoopMutantDummy componentInChildren3 = evnt.Enemy.gameObject.transform.root.GetComponentInChildren<CoopMutantDummy>();
				if (componentInChildren3)
				{
					trapTrigger componentInChildren4 = evnt.Trap.GetComponentInChildren<trapTrigger>();
					if (componentInChildren4)
					{
						componentInChildren4.SetNooseRope(componentInChildren3);
					}
				}
			}
		}
	}

	
	public override void OnEvent(SetCorpsePosition evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Corpse)
		{
			if (evnt.Corpse.transform.parent != null)
			{
				evnt.Corpse.gameObject.SendMessageUpwards("releaseNooseTrapMP", SendMessageOptions.DontRequireReceiver);
			}
			evnt.Corpse.transform.parent = null;
			evnt.Corpse.Freeze(false);
			if (evnt.Pickup)
			{
				evnt.Corpse.SendMessage("sendResetRagDoll", SendMessageOptions.DontRequireReceiver);
				evnt.Corpse.transform.position = new Vector3(4096f, 4096f, 4096f);
			}
			else if (evnt.Destroy)
			{
				BoltNetwork.Destroy(evnt.Corpse);
			}
			else
			{
				evnt.Corpse.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
				evnt.Corpse.SendMessage("setRagDollDrop", SendMessageOptions.DontRequireReceiver);
				evnt.Corpse.transform.position = evnt.Position;
				evnt.Corpse.transform.rotation = ((!(evnt.Rotation == default(Quaternion))) ? evnt.Rotation : Quaternion.identity);
				MultiHolder.GetTriggerChild(evnt.Corpse.transform).gameObject.SetActive(true);
			}
		}
	}

	
	public override void OnEvent(RockThrowerActivated evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			evnt.Target.SendMessage("disableTrigger");
		}
	}

	
	public override void OnEvent(RockThrowerDeActivated evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			evnt.Target.SendMessage("enableTrigger");
		}
	}

	
	public override void OnEvent(RockThrowerAnimate evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			coopRockThrower component = evnt.Target.GetComponent<coopRockThrower>();
			if (component)
			{
				component.setAnimator(evnt.animVar, evnt.onoff);
			}
		}
	}

	
	public override void OnEvent(playerBlock evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		evnt.target.gameObject.SendMessage("forcePlayerBlock", SendMessageOptions.DontRequireReceiver);
	}

	
	public override void OnEvent(SkinnedAnimal evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		evnt.Target.SendMessage("setSkinnedState", SendMessageOptions.DontRequireReceiver);
	}

	
	public override void OnEvent(syncPlayerRenderers evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (LocalPlayer.Transform)
		{
			CoopMecanimReplicator componentInChildren = LocalPlayer.Transform.GetComponentInChildren<CoopMecanimReplicator>();
			if (componentInChildren)
			{
				componentInChildren.forceStateUpdate();
			}
		}
	}

	
	private IEnumerator syncLocalPlayerRenderers()
	{
		float timer = Time.time + 10f;
		yield break;
	}

	
	public override void OnEvent(playerSyncAttack evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.target)
		{
			CoopPlayerClientHitPrediction component = evnt.target.GetComponent<CoopPlayerClientHitPrediction>();
			if (component)
			{
				component.setAttackEvent(evnt.stickAttack, evnt.axeAttack, evnt.rockAttack);
			}
		}
	}

	
	public override void OnEvent(playerSyncJump evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.target)
		{
			CoopPlayerClientHitPrediction component = evnt.target.GetComponent<CoopPlayerClientHitPrediction>();
			if (component)
			{
				component.setJumpEvent();
			}
		}
	}

	
	public override void OnEvent(updateMecanimRemoteState evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			CoopMecanimReplicator component = evnt.Target.GetComponent<CoopMecanimReplicator>();
			if (!component)
			{
				component = evnt.Target.transform.GetChild(0).GetComponent<CoopMecanimReplicator>();
			}
			if (component)
			{
				component.ApplyHashToRemote(evnt.layer, evnt.hash, evnt.anyState, evnt.normalizedTime, false);
			}
		}
	}

	
	public override void OnEvent(syncGirlPickup evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.target)
		{
			if (evnt.destroyPickup)
			{
				BoltNetwork.Destroy(evnt.target);
				return;
			}
			girlPickupCoopSync componentInChildren = evnt.target.GetComponentInChildren<girlPickupCoopSync>();
			if (componentInChildren)
			{
				if (evnt.syncPutDownAnimation)
				{
					componentInChildren.setGirlPutDownAnimation(evnt.playerTarget.transform);
				}
				if (evnt.syncPickupAnimation)
				{
					componentInChildren.setGirlPickupAnimation(evnt.playerTarget.transform);
				}
				if (evnt.toMachine)
				{
					componentInChildren.setGirlToMachine();
				}
				if (evnt.disableTrigger)
				{
					componentInChildren.disablePickupTrigger();
				}
				if (evnt.enableTrigger)
				{
					componentInChildren.enablePickupTrigger();
				}
			}
		}
	}

	
	public override void OnEvent(syncAnimalState evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.target)
		{
			CoopAnimal componentInChildren = evnt.target.GetComponentInChildren<CoopAnimal>();
			if (componentInChildren)
			{
				componentInChildren.setOnSnow(evnt.onSnow);
			}
		}
	}

	
	public override void OnEvent(spawnTreeDust evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		Prefabs.Instance.SpawnWoodChopPS(evnt.position, evnt.rotation);
	}

	
	public override void OnEvent(setupEndBoss evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.bossActive > 0)
		{
			if (evnt.bossActive == 1)
			{
				LocalPlayer.Stats.IsFightingBoss = true;
			}
			else if (evnt.bossActive == 2)
			{
				LocalPlayer.Stats.IsFightingBoss = false;
			}
			return;
		}
		if (evnt.disableBossTrigger)
		{
			GameObject gameObject = GameObject.FindWithTag("endGameBossPrefab");
			if (gameObject)
			{
				SphereCollider componentInChildren = gameObject.GetComponentInChildren<SphereCollider>();
				if (componentInChildren)
				{
					componentInChildren.enabled = false;
				}
			}
			return;
		}
		if (evnt.spawnBoss)
		{
			TheForest.Utils.Scene.SceneTracker.endBossSpawned = true;
		}
	}

	
	public override void OnEvent(arrowFireSync evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Target)
		{
			evnt.Target.SendMessage("enableArrowFire", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(stuckArrowsSync evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.target)
		{
			arrowStickToTarget arrowStickToTarget = evnt.target.GetComponent<arrowStickToTarget>();
			if (!arrowStickToTarget)
			{
				arrowStickToTarget = evnt.target.GetComponentInChildren<arrowStickToTarget>();
			}
			if (arrowStickToTarget)
			{
				if (evnt.removeArrow)
				{
					int num = 0;
					foreach (KeyValuePair<Transform, int> keyValuePair in arrowStickToTarget.stuckArrows)
					{
						if (num == evnt.index && keyValuePair.Key)
						{
							UnityEngine.Object.Destroy(keyValuePair.Key.gameObject);
						}
						num++;
					}
					return;
				}
				int type = evnt.type;
				GameObject gameObject;
				if (type != 1)
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(arrowStickToTarget.fakeArrowPickup, evnt.target.transform.position, evnt.target.transform.rotation);
				}
				else
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(arrowStickToTarget.fakeArrowBonePickup, evnt.target.transform.position, evnt.target.transform.rotation);
				}
				gameObject.transform.parent = arrowStickToTarget.stickToJoints[evnt.index];
				gameObject.transform.localPosition = evnt.pos;
				gameObject.transform.localRotation = evnt.rot;
				arrowStickToTarget.stuckArrows.Add(gameObject.transform, evnt.index);
				fakeArrowSetup component = gameObject.GetComponent<fakeArrowSetup>();
				if (component && BoltNetwork.isRunning)
				{
					component.storedIndex = arrowStickToTarget.stuckArrows.Count - 1;
					component.entityTarget = evnt.target;
				}
			}
		}
	}

	
	public override void OnEvent(HitCorpse evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		if (evnt.Entity && evnt.creepyCorpse)
		{
			DamageCorpseSimple componentInChildren = evnt.Entity.GetComponentInChildren<DamageCorpseSimple>();
			if (componentInChildren)
			{
				componentInChildren.DoLocalCut(evnt.Damage);
			}
		}
	}

	
	public override void OnEvent(SetJoiningTimeOfDay evnt)
	{
		if (!this.ValidateSender(evnt, SenderTypes.Any))
		{
			return;
		}
		Debug.Log("Setting joining time of day to " + evnt.TimeOfDay);
		CoopWeatherProxy.JoiningTimeOfDay = evnt.TimeOfDay;
		if (TheForest.Utils.Scene.Atmosphere)
		{
			TheForest.Utils.Scene.Atmosphere.TimeOfDay = evnt.TimeOfDay;
		}
	}

	
	~CoopClientCallbacks()
	{
		if (SteamClientDSConfig.isDedicatedClient)
		{
			SteamUser.TerminateGameConnection(SteamClientDSConfig.EndPoint.Address.Packed, SteamClientDSConfig.EndPoint.Port);
		}
	}

	
	private static bool invokedServerNotResponding;

	
	public static Action OnDisconnected;

	
	public static Action ServerNotResponding;

	
	public static Action ServerIsResponding;
}
