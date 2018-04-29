using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using PathologicalGames;
using Steamworks;
using TheForest.Buildings.Creation;
using TheForest.Buildings.Utils;
using TheForest.Buildings.World;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.Tools;
using TheForest.Utils;
using TheForest.World;
using UniLinq;
using UnityEngine;


[BoltGlobalBehaviour(BoltNetworkModes.Host)]
public class CoopServerCallbacks : GlobalEventListener
{
	
	public override void OnEvent(RackAdd evnt)
	{
		if (evnt.Rack && evnt.Slot >= 0)
		{
			evnt.Rack.GetState<IWeaponRackState>().Slots[evnt.Slot] = evnt.ItemId;
		}
	}

	
	public override void OnEvent(AdminCommand evnt)
	{
		CoopAdminCommand.Recv(evnt.Command, evnt.Data, evnt.RaisedBy);
	}

	
	public override void OnEvent(RackRemove evnt)
	{
		if (evnt.Rack && evnt.Slot >= 0)
		{
			evnt.Rack.GetState<IWeaponRackState>().Slots[evnt.Slot] = 0;
		}
	}

	
	public override void OnEvent(parryEnemy evnt)
	{
		if (evnt.Target)
		{
			evnt.Target.SendMessage("setEnemyParried", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(arrowFireSync evnt)
	{
		if (evnt.Target)
		{
			evnt.Target.SendMessage("enableArrowFire", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(DestroyTree evnt)
	{
		BoltEntity tree = evnt.Tree;
		if (tree && tree.isAttached)
		{
			if (tree.canFreeze)
			{
				tree.Freeze(false);
			}
			ITreeCutState state = tree.GetState<ITreeCutState>();
			if (state.State == 0)
			{
				state.State = 1;
			}
			state.Damage = 16f;
		}
	}

	
	public override void OnEvent(AddItemToDoor evnt)
	{
		if (!CoopHellDoors.Instance)
		{
			throw new Exception("could not find hell doors root object");
		}
		CoopHellDoors.Instance.entity.Freeze(false);
		if (CoopHellDoors.Instance.state.NewProperty[evnt.Door].Items[evnt.Slot] != evnt.Item)
		{
			CoopHellDoors.Instance.state.NewProperty[evnt.Door].Items[evnt.Slot] = evnt.Item;
		}
	}

	
	public override void OnEvent(Burn evnt)
	{
		if (evnt.Entity)
		{
			if (evnt.Entity.StateIs<IMutantState>())
			{
				EnemyHealth[] componentsInChildren = evnt.Entity.GetComponentsInChildren<EnemyHealth>(true);
				if (componentsInChildren.Length > 0)
				{
					componentsInChildren[0].Burn();
				}
			}
			else if (evnt.Entity.StateIs<IAnimalState>())
			{
				animalHealth[] componentsInChildren2 = evnt.Entity.GetComponentsInChildren<animalHealth>(true);
				if (componentsInChildren2.Length > 0)
				{
					componentsInChildren2[0].Burn();
				}
			}
		}
	}

	
	public override void OnEvent(Poison evnt)
	{
		if (evnt.Entity)
		{
			if (evnt.Entity.StateIs<IMutantState>())
			{
				EnemyHealth[] componentsInChildren = evnt.Entity.GetComponentsInChildren<EnemyHealth>(true);
				if (componentsInChildren.Length > 0)
				{
					componentsInChildren[0].Poison();
				}
			}
			else if (evnt.Entity.StateIs<IAnimalState>())
			{
				animalHealth[] componentsInChildren2 = evnt.Entity.GetComponentsInChildren<animalHealth>(true);
				if (componentsInChildren2.Length > 0)
				{
					componentsInChildren2[0].Poison();
				}
			}
		}
	}

	
	public override void OnEvent(AddEffigyPart evnt)
	{
		if (evnt.Effigy)
		{
			Component[] componentsInChildren = evnt.Effigy.GetComponentsInChildren(typeof(EffigyArchitect), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as EffigyArchitect).SpawnPartReal(new EffigyArchitect.Part
				{
					_itemId = evnt.ItemId,
					_position = evnt.Position,
					_rotation = evnt.Rotation
				}, true);
			}
		}
	}

	
	public override void OnEvent(LocalizedHit evnt)
	{
		if (evnt.Building)
		{
			LocalizedHitData data = default(LocalizedHitData);
			data._damage = evnt.Damage;
			data._position = evnt.Position;
			if (evnt.Chunk == -1)
			{
				evnt.Building.GetComponent<BuildingHealth>().LocalizedHitReal(data);
			}
			else
			{
				evnt.Building.GetComponent<BuildingHealth>().GetChunk(evnt.Chunk).LocalizedHitReal(data);
			}
		}
	}

	
	public override void OnEvent(PlaceTrophy evnt)
	{
		if (evnt.Maker)
		{
			TrophyMaker[] componentsInChildren = evnt.Maker.GetComponentsInChildren<TrophyMaker>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].PlaceTrophy(evnt.ItemId);
			}
		}
	}

	
	public override void OnEvent(ResetTrap evnt)
	{
		if (evnt.TargetTrap && evnt.TargetTrap.isAttached && evnt.TargetTrap.isOwner && (evnt.TargetTrap.StateIs<ITrapLargeState>() || evnt.TargetTrap.StateIs<ITrapRabbitState>()))
		{
			try
			{
				ResetTraps componentInChildren = evnt.TargetTrap.GetComponentInChildren<ResetTraps>();
				if (componentInChildren)
				{
					componentInChildren.RestoreSafe();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			PrefabIdentifier componentInParent = evnt.TargetTrap.GetComponentInParent<PrefabIdentifier>();
			BoltEntity boltEntity = BoltNetwork.Instantiate(evnt.TargetTrap.prefabId, evnt.TargetTrap.transform.position, evnt.TargetTrap.transform.rotation);
			boltEntity.GetComponent<BuildingHealth>().CopyDamageFrom(componentInParent.GetComponent<BuildingHealth>());
			TreeStructure component = componentInParent.GetComponent<TreeStructure>();
			if (component)
			{
				TreeStructure component2 = boltEntity.GetComponent<TreeStructure>();
				component2.TreeId = component.TreeId;
			}
			BoltNetwork.Destroy(evnt.TargetTrap);
		}
	}

	
	public override void OnEvent(HitCorpse evnt)
	{
		if (evnt.Entity)
		{
			NetworkArray_Integer bodyPartsDamage;
			NetworkArray_Values<int> networkArray_Values = bodyPartsDamage = evnt.Entity.GetComponentInChildren<CoopSliceAndDiceMutant>().BodyPartsDamage;
			int num;
			int index = num = evnt.BodyPartIndex;
			num = bodyPartsDamage[num];
			networkArray_Values[index] = num - evnt.Damage;
		}
	}

	
	public override void Connected(BoltConnection connection)
	{
		connection.SetCanReceiveEntities(false);
		Debug.Log("Connection:" + BoltNetwork.connections.Count<BoltConnection>());
		if (SteamDSConfig.isDedicatedServer && Mathf.Max(Scene.SceneTracker.allPlayers.Count, BoltNetwork.connections.Count<BoltConnection>()) > Mathf.Min(SteamDSConfig.ServerPlayers, SteamDSConfig.maxPlayersPerServer))
		{
			connection.Disconnect(new CoopJoinDedicatedServerFailed
			{
				Error = "Server is full"
			});
			return;
		}
		if (CoopPeerStarter.DedicatedHost)
		{
			if (!string.IsNullOrEmpty(SteamDSConfig.ServerPassword))
			{
				CoopJoinDedicatedServerToken coopJoinDedicatedServerToken = connection.ConnectToken as CoopJoinDedicatedServerToken;
				if (coopJoinDedicatedServerToken == null || coopJoinDedicatedServerToken.ServerPassword != SteamDSConfig.ServerPassword)
				{
					connection.Disconnect(new CoopJoinDedicatedServerFailed
					{
						Error = "Incorrect server password"
					});
					return;
				}
			}
			if (!string.IsNullOrEmpty(SteamDSConfig.ServerAdminPassword))
			{
				CoopJoinDedicatedServerToken coopJoinDedicatedServerToken2 = connection.ConnectToken as CoopJoinDedicatedServerToken;
				if (coopJoinDedicatedServerToken2 != null && coopJoinDedicatedServerToken2.AdminPassword == SteamDSConfig.ServerAdminPassword)
				{
					AdminAuthed adminAuthed = AdminAuthed.Create(connection);
					adminAuthed.IsAdmin = true;
					adminAuthed.Send();
				}
			}
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			CoopJoinDedicatedServerToken coopJoinDedicatedServerToken3 = connection.ConnectToken as CoopJoinDedicatedServerToken;
			bool flag = false;
			if (coopJoinDedicatedServerToken3.steamBlobToken != null && coopJoinDedicatedServerToken3.steamBlobToken.Length > 0)
			{
				CSteamID csteamID;
				flag = SteamGameServer.SendUserConnectAndAuthenticate(connection.RemoteEndPoint.Address.Packed, coopJoinDedicatedServerToken3.steamBlobToken, (uint)coopJoinDedicatedServerToken3.steamBlobToken.Length, out csteamID);
				Debug.Log(string.Concat(new object[]
				{
					"Steam auth - clientId ",
					csteamID,
					" status ",
					flag
				}));
				if (flag)
				{
					SteamDSConfig.clientConnectionInfo.Add(connection.ConnectionId, csteamID);
					if (CoopKick.IsBanned(csteamID.m_SteamID))
					{
						Debug.Log("TELL CLIENT HE WAS BANNED");
						connection.Disconnect(new CoopKickToken
						{
							Banned = true,
							KickMessage = "HOST_BANNED_YOU"
						});
						return;
					}
				}
			}
			if (!flag)
			{
				connection.Disconnect(new CoopJoinDedicatedServerFailed
				{
					Error = "No Steam Auth"
				});
				return;
			}
		}
		if (CoopHellDoors.Instance && CoopHellDoors.Instance.entity.isAttached)
		{
			CoopHellDoors.Instance.entity.Freeze(false);
		}
		SetJoiningTimeOfDay setJoiningTimeOfDay = SetJoiningTimeOfDay.Create(connection);
		setJoiningTimeOfDay.TimeOfDay = Scene.Atmosphere.TimeOfDay;
		setJoiningTimeOfDay.Send();
	}

	
	public override void Disconnected(BoltConnection connection)
	{
		if (CoopTreeGrid.TodoPlayerSweeps.Contains(connection))
		{
			CoopTreeGrid.TodoPlayerSweeps.Remove(connection);
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			bool flag = SteamDSConfig.clientConnectionInfo.ContainsKey(connection.ConnectionId);
			if (flag)
			{
				SteamGameServer.SendUserDisconnect(SteamDSConfig.clientConnectionInfo[connection.ConnectionId]);
				SteamDSConfig.clientConnectionInfo.Remove(connection.ConnectionId);
			}
		}
	}

	
	public override void OnEvent(FoundationExLocalizedHit evnt)
	{
		if (evnt.Entity)
		{
			LocalizedHitData data = default(LocalizedHitData);
			data._damage = evnt.HitDamage;
			data._position = evnt.HitPosition;
			evnt.Entity.GetComponent<FoundationArchitect>().GetChunk(evnt.Chunk).LocalizedHitReal(data);
		}
	}

	
	public override void OnEvent(FoundationExLookAtExplosion evnt)
	{
		if (evnt.Entity)
		{
			evnt.Entity.GetComponent<FoundationArchitect>().GetChunk(evnt.Chunk).LookAtExplosionReal(evnt.Position);
		}
	}

	
	public override void OnEvent(ToggleWallAddition evnt)
	{
		if (evnt.Wall)
		{
			evnt.Wall.GetComponent<WallChunkArchitect>().PerformToggleAddition();
		}
	}

	
	public override void OnEvent(ToggleWallDoor evnt)
	{
		if (evnt.Entity)
		{
			evnt.Entity.GetComponentInChildren<WallDoor>().ToggleDoorStatusAction(false);
		}
	}

	
	public override void OnEvent(PlaceWallChunk evnt)
	{
		BoltEntity boltEntity = BoltNetwork.Instantiate(evnt.prefab, evnt.token);
		if (evnt.parent)
		{
			DynamicBuilding component = evnt.parent.GetComponent<DynamicBuilding>();
			boltEntity.transform.parent = ((!component || !component._parentOverride) ? evnt.parent.transform : component._parentOverride);
		}
		LocalPlayer.Create.RefreshGrabber();
	}

	
	public override void OnEvent(DestroyWithTag evnt)
	{
		if (evnt.Entity)
		{
			DestroyOnContactWithTag componentInChildren = evnt.Entity.GetComponentInChildren<DestroyOnContactWithTag>();
			if (componentInChildren)
			{
				if (componentInChildren._destroyTarget)
				{
					foreach (object obj in componentInChildren._destroyTarget.transform)
					{
						Transform transform = (Transform)obj;
						if (transform.GetComponent<BoltEntity>())
						{
							transform.parent = null;
							BoltNetwork.Destroy(transform.gameObject);
						}
					}
				}
				componentInChildren.Perform(true);
				BoltNetwork.Destroy(evnt.Entity, new CoopDestroyTagToken());
			}
			else if ((!PlayerPreferences.NoDestruction || BoltNetwork.isServer) && evnt.Entity.StateIs<IFireState>())
			{
				BoltNetwork.Destroy(evnt.Entity);
			}
		}
	}

	
	public override void OnEvent(DamageTree evnt)
	{
		if (evnt.TreeEntity)
		{
			ITreeCutState state = evnt.TreeEntity.GetState<ITreeCutState>();
			float damage = Mathf.Max(0.1f, evnt.Damage);
			this.UpdateChunks(state, evnt.DamageIndex, damage);
		}
	}

	
	public override void OnEvent(BurnTree evnt)
	{
		if (evnt.Entity && evnt.Entity.isOwner)
		{
			ITreeCutState state = evnt.Entity.GetState<ITreeCutState>();
			state.Burning = evnt.IsBurning;
			evnt.Entity.Freeze(false);
		}
	}

	
	private void UpdateChunks(ITreeCutState state, int chunk, float damage)
	{
		switch (chunk)
		{
		case 1:
			state.Chunk1 += damage;
			break;
		case 2:
			state.Chunk2 += damage;
			break;
		case 3:
			state.Chunk3 += damage;
			break;
		case 4:
			state.Chunk4 += damage;
			break;
		}
	}

	
	public override void OnEvent(LightEffigy evnt)
	{
		if (evnt.Effigy)
		{
			evnt.Effigy.GetState<IBuildingEffigyState>().Lit = true;
		}
	}

	
	public override void OnEvent(CutTriggerActivated evnt)
	{
		if (evnt.Trap)
		{
			Component[] componentsInChildren = evnt.Trap.GetComponentsInChildren(typeof(trapTrigger), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as trapTrigger).releaseNooseTrap();
			}
			CutTriggerActivated cutTriggerActivated = CutTriggerActivated.Create(GlobalTargets.AllClients);
			cutTriggerActivated.Trap = evnt.Trap;
			cutTriggerActivated.Send();
		}
	}

	
	public override void OnEvent(doReleaseFromTrap evnt)
	{
		if (evnt.target)
		{
		}
	}

	
	public override void OnEvent(ValidSleepTime evnt)
	{
		ValidSleepTime validSleepTime = ValidSleepTime.Create(GlobalTargets.AllClients);
		validSleepTime.NextSleepTime = Scene.Clock.NextSleepTime;
		validSleepTime.Send();
	}

	
	public override void OnEvent(TriggerFishTrap evnt)
	{
		Debug.Log("TriggerFishTrap Host");
		if (evnt.Trap != null)
		{
			trapTrigger componentInChildren = evnt.Trap.GetComponentInChildren<trapTrigger>();
			if (componentInChildren)
			{
				Debug.Log("TriggerFishTrap");
				if (evnt.Fish == null || evnt.Fish == string.Empty)
				{
					Debug.Log("Call SendTrappedFishAllClients");
					componentInChildren.SendTrappedFishAllClients(string.Empty, string.Empty, Vector3.zero);
					base.StartCoroutine(componentInChildren.SetTrapFish(string.Empty, string.Empty, Vector3.zero));
				}
				else if (evnt.Fish == "request")
				{
					Debug.Log("Call SendTrappedFishToClient");
					componentInChildren.SendTrappedFishToClient(evnt.RaisedBy);
				}
				else
				{
					Debug.Log("Call SendTrappedFishAllClients");
					componentInChildren.SendTrappedFishAllClients(evnt.Fish, evnt.Spawnner, evnt.Position);
					base.StartCoroutine(componentInChildren.SetTrapFish(evnt.Fish, evnt.Spawnner, evnt.Position));
				}
			}
		}
	}

	
	public override void OnEvent(SetTrappedEnemy evnt)
	{
		if (evnt.Trap != null)
		{
			trapTrigger componentInChildren = evnt.Trap.GetComponentInChildren<trapTrigger>();
			if (componentInChildren)
			{
				componentInChildren.SendNooseRopeMP(evnt.RaisedBy);
			}
		}
	}

	
	public override void OnEvent(SetCorpsePosition evnt)
	{
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
				if (evnt.Corpse.StateIs<IDummyState>())
				{
					BoltNetwork.Destroy(evnt.Corpse);
				}
			}
			else
			{
				evnt.Corpse.SendMessage("dropFromCarry", false, SendMessageOptions.DontRequireReceiver);
				evnt.Corpse.SendMessage("setRagDollDrop", SendMessageOptions.DontRequireReceiver);
				evnt.Corpse.transform.position = evnt.Position;
				evnt.Corpse.transform.rotation = ((!(evnt.Rotation == default(Quaternion))) ? evnt.Rotation : Quaternion.identity);
				MultiHolder.GetTriggerChild(evnt.Corpse.transform).gameObject.SetActive(true);
				base.StartCoroutine(this.fixCorpsePosition(evnt.Position, evnt.Corpse.transform));
			}
		}
	}

	
	private IEnumerator fixCorpsePosition(Vector3 pos, Transform tr)
	{
		float t = 0f;
		while (t < 0.25f)
		{
			tr.position = pos;
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	public override void OnEvent(storeRagDollName evnt)
	{
		evnt.Target.SendMessage("getRagDollName", evnt.name, SendMessageOptions.DontRequireReceiver);
	}

	
	public override void OnEvent(SkinnedAnimal evnt)
	{
		evnt.Target.SendMessage("setSkinnedState", SendMessageOptions.DontRequireReceiver);
	}

	
	public override void OnEvent(SpawnCutTree evnt)
	{
		CoopTreeId coopTreeId = CoopPlayerCallbacks.AllTrees.FirstOrDefault((CoopTreeId x) => x.Id == evnt.TreeId);
		if (coopTreeId && coopTreeId.state.State == 0)
		{
			coopTreeId.state.State = 1;
			coopTreeId.entity.Freeze(false);
		}
	}

	
	public override void OnEvent(RemoveStump evnt)
	{
		if (evnt.TargetTree)
		{
			CoopTreeId component = evnt.TargetTree.GetComponent<CoopTreeId>();
			if (component && component.state.State < 4)
			{
				component.Goto_Removed();
				if (evnt.CutUpStumpPrefabId.Value > 0)
				{
					BoltNetwork.Instantiate(evnt.CutUpStumpPrefabId, evnt.Position, evnt.Rotation);
				}
			}
		}
	}

	
	public override void OnEvent(DropItem evnt)
	{
		if (evnt.PreSpawned)
		{
			evnt.PreSpawned.transform.position = evnt.Position;
			evnt.PreSpawned.transform.rotation = evnt.Rotation;
			evnt.PreSpawned.SendMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			BoltEntity boltEntity = BoltNetwork.Instantiate(evnt.PrefabId, evnt.Position, evnt.Rotation);
			if (boltEntity != null && evnt.AvoidImpacts && !boltEntity.GetComponent<flyingObjectFixerFrame>())
			{
				flyingObjectFixerFrame flyingObjectFixerFrame = boltEntity.gameObject.AddComponent<flyingObjectFixerFrame>();
			}
		}
	}

	
	public override void OnEvent(PlaceConstruction evnt)
	{
		BoltEntity boltEntity = BoltNetwork.Instantiate(evnt.PrefabId, evnt.Position, evnt.Rotation);
		if (boltEntity.GetComponent<TreeStructure>())
		{
			boltEntity.GetComponent<TreeStructure>().TreeId = evnt.TreeIndex;
		}
		if (evnt.Parent)
		{
			DynamicBuilding component = evnt.Parent.GetComponent<DynamicBuilding>();
			boltEntity.transform.parent = ((!component || !component._parentOverride) ? evnt.Parent.transform : component._parentOverride);
		}
		if (evnt.AboveGround)
		{
			FoundationArchitect component2 = boltEntity.GetComponent<FoundationArchitect>();
			if (component2)
			{
				component2._aboveGround = evnt.AboveGround;
			}
		}
		boltEntity.SendMessage("OnDeserialized", SendMessageOptions.DontRequireReceiver);
		LocalPlayer.Create.RefreshGrabber();
	}

	
	public override void OnEvent(PlaceDryingFood evnt)
	{
		BoltEntity boltEntity = BoltNetwork.Instantiate(evnt.PrefabId, evnt.Position, evnt.Rotation);
		if (evnt.Parent)
		{
			DynamicBuilding component = evnt.Parent.GetComponent<DynamicBuilding>();
			boltEntity.transform.parent = ((!component || !component._parentOverride) ? evnt.Parent.transform : component._parentOverride);
			CookStew componentInChildren = boltEntity.GetComponentInChildren<CookStew>();
			if (componentInChildren)
			{
				boltEntity.SendMessage("SetActiveBonus", (WeaponStatUpgrade.Types)evnt.DecayState, SendMessageOptions.DontRequireReceiver);
				return;
			}
			CookStew componentInChildren2 = evnt.Parent.GetComponentInChildren<CookStew>();
			if (componentInChildren2)
			{
				componentInChildren2.UpdateCookController();
			}
		}
		Cook component2 = boltEntity.GetComponent<Cook>();
		if (component2)
		{
			component2.SetDecayState((DecayingInventoryItemView.DecayStates)evnt.DecayState);
		}
	}

	
	public override void OnEvent(CutStructureHole evnt)
	{
		if (evnt.TargetStructure && !PlayerPreferences.NoDestruction)
		{
			IHoleStructure holeStructure = (IHoleStructure)evnt.TargetStructure.GetComponent(typeof(IHoleStructure));
			if (holeStructure != null)
			{
				holeStructure.AddSquareHole(evnt.Position, evnt.YRotation, evnt.Size);
				holeStructure.CreateStructure(false);
				(holeStructure as MonoBehaviour).SendMessage("OnHolePlaced", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public override void OnEvent(FireLightEvent evnt)
	{
		if (evnt.Target)
		{
			if (evnt.Target.GetComponentInChildren<Fire2>())
			{
				evnt.Target.GetComponentInChildren<Fire2>().Action_LightFire();
			}
			if (evnt.Target.GetComponentInChildren<FireStand>())
			{
				evnt.Target.GetComponentInChildren<FireStand>().LightFireMP();
			}
		}
	}

	
	public override void OnEvent(FireAddFuelEvent evnt)
	{
		if (evnt.Target)
		{
			Fire2 componentInChildren = evnt.Target.GetComponentInChildren<Fire2>();
			if (componentInChildren)
			{
				if (evnt.CanSetAlight)
				{
					componentInChildren.Burn();
				}
				evnt.Target.GetComponentInChildren<Fire2>().Action_AddFuel();
			}
			else if (evnt.Target.GetComponentInChildren<FireStand>())
			{
				evnt.Target.GetComponentInChildren<FireStand>().AddToFuelMP();
			}
		}
	}

	
	public override void OnEvent(RemoveWater evnt)
	{
		if (evnt.Entity)
		{
			WaterSource[] componentsInChildren = evnt.Entity.GetComponentsInChildren<WaterSource>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].RemoveWater(evnt.Amount);
			}
		}
	}

	
	public override void OnEvent(BreakPlank evnt)
	{
		CoopWeatherProxy.Instance.state.BreakableWalls[evnt.Index] = 1;
	}

	
	public override void OnEvent(PlaceFoundationEx evnt)
	{
		BoltEntity boltEntity = BoltNetwork.Instantiate(evnt.Prefab, evnt.Token, evnt.Position, evnt.Rotation);
		if (evnt.Parent)
		{
			boltEntity.transform.parent = evnt.Parent.transform;
		}
		LocalPlayer.Create.RefreshGrabber();
	}

	
	public override void OnEvent(GrowGarden evnt)
	{
		if (evnt.Garden)
		{
			Garden[] componentsInChildren = evnt.Garden.GetComponentsInChildren<Garden>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].PlantSeed_Real(evnt.SeedNum);
			}
		}
	}

	
	public override void OnEvent(TriggerLargeTrap evnt)
	{
		if (evnt.Trap)
		{
			evnt.Trap.GetState<ITrapLargeState>().Sprung = true;
			Component[] componentsInChildren = evnt.Trap.GetComponentsInChildren(typeof(trapTrigger), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as trapTrigger).TriggerLargeTrap(null);
			}
		}
	}

	
	public override void OnEvent(TripWire evnt)
	{
		if (evnt.WireEntity)
		{
			evnt.WireEntity.GetState<ITripWireState>().Tripped = true;
		}
	}

	
	public override void OnEvent(ClientSuitcasePush evnt)
	{
		if (evnt.Suitcase)
		{
			evnt.Suitcase.GetComponentInChildren<Rigidbody>().velocity = evnt.Direction;
			evnt.Suitcase.GetComponent<CoopSuitcase>().enabled = true;
		}
	}

	
	public override void OnEvent(ToggleDockingState evnt)
	{
		if (evnt.Entity.IsAttached())
		{
			Component[] componentsInChildren = evnt.Entity.GetComponentsInChildren(typeof(Dockable), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as Dockable).MpSendDock(evnt.DockPosition);
			}
		}
	}

	
	public override void OnEvent(PushRaft evnt)
	{
		if (evnt.Raft.IsAttached())
		{
			Component[] componentsInChildren = evnt.Raft.GetComponentsInChildren(typeof(CoopRaftPusher2), true);
			if (componentsInChildren.Length > 0)
			{
				(componentsInChildren[0] as CoopRaftPusher2).PushRaft(evnt.Direction);
			}
		}
	}

	
	public override void OnEvent(RaftGrab evnt)
	{
		if (evnt.Raft)
		{
			IRaftState state = evnt.Raft.GetState<IRaftState>();
			bool flag = state.GrabbedBy[evnt.OarId];
			bool flag2 = evnt.Player;
			if (!flag || (!flag2 && evnt.RaisedBy == state.GrabbedBy[evnt.OarId].source))
			{
				state.GrabbedBy[evnt.OarId] = evnt.Player;
				if (!flag2)
				{
					Scene.ActiveMB.StartCoroutine(evnt.Raft.GetComponent<Buoyancy>().ResetRigidbody());
				}
			}
		}
	}

	
	public override void OnEvent(RaftControl evnt)
	{
		if (evnt.Raft)
		{
			RaftPushMP component = evnt.Raft.GetComponent<RaftPushMP>();
			component.ReceivedCommand(evnt.OarId, (RaftPush.MoveDirection)evnt.Movement, evnt.Rotation);
		}
	}

	
	public override void OnEvent(CraneCommand evnt)
	{
		try
		{
			if (evnt.Crane)
			{
				ICraneState state = evnt.Crane.GetState<ICraneState>();
				evnt.Crane.Freeze(false);
				if (evnt.Sender)
				{
					if (state.GrabbedBy == null && evnt.Sender.source == evnt.RaisedBy)
					{
						state.GrabbedBy = evnt.Sender;
					}
				}
				else if (state.GrabbedBy && state.GrabbedBy.source == evnt.RaisedBy)
				{
					state.GrabbedBy = null;
				}
				if (state.GrabbedBy && state.GrabbedBy.source == evnt.RaisedBy)
				{
					CraneTrigger componentInChildren = evnt.Crane.GetComponentInChildren<CraneTrigger>();
					componentInChildren.AddRemoteInput(evnt.Direction);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	
	public override void OnEvent(SledGrab evnt)
	{
		if (evnt.Player && evnt.Sled)
		{
			IMultiHolderState state = evnt.Sled.GetState<IMultiHolderState>();
			if (!state.GrabbedBy)
			{
				state.GrabbedBy = evnt.Player;
			}
		}
	}

	
	public override void OnEvent(BreakCrateEvent evnt)
	{
		if (CoopWorldCrates.Instance)
		{
			CoopWorldCrates.Instance.state.Broken[evnt.Index] = 1;
		}
	}

	
	public override void OnEvent(PlayerHitEnemy ev)
	{
		if (!ev.Target)
		{
			return;
		}
		if (ev.Hit == 0)
		{
			return;
		}
		try
		{
			if (EnemyHealth.CurrentAttacker == null)
			{
				EnemyHealth.CurrentAttacker = ev.Target;
			}
			lb_Bird component = ev.Target.GetComponent<lb_Bird>();
			Fish componentInChildren = ev.Target.GetComponentInChildren<Fish>();
			Transform transform;
			if (componentInChildren)
			{
				transform = componentInChildren.transform;
			}
			else if (ev.Target.GetComponent<animalHealth>())
			{
				transform = ev.Target.transform;
			}
			else if (component)
			{
				transform = component.transform;
			}
			else
			{
				EnemyHealth componentInChildren2 = ev.Target.GetComponentInChildren<EnemyHealth>();
				if (componentInChildren2)
				{
					transform = componentInChildren2.transform;
				}
				else
				{
					transform = ev.Target.transform.GetChild(0);
				}
			}
			if (ev.getAttacker == 10 && ev.Weapon)
			{
				ArrowDamage componentInChildren3 = ev.Weapon.GetComponentInChildren<ArrowDamage>();
				if (componentInChildren3.Live)
				{
					arrowStickToTarget componentInChildren4 = transform.GetComponentInChildren<arrowStickToTarget>();
					Transform target = transform;
					if (componentInChildren4)
					{
						target = componentInChildren4.transform;
					}
					componentInChildren3.CheckHit(Vector3.zero, target, false, transform.GetComponent<Collider>());
				}
			}
			if (ev.explosion)
			{
				transform.SendMessage("Explosion", -1, SendMessageOptions.DontRequireReceiver);
			}
			if (ev.HitHead)
			{
				transform.SendMessage("HitHead", SendMessageOptions.DontRequireReceiver);
			}
			if (ev.getStealthAttack)
			{
				transform.SendMessage("getStealthAttack", SendMessageOptions.DontRequireReceiver);
			}
			if (ev.hitFallDown)
			{
				mutantHitReceiver componentInChildren5 = transform.GetComponentInChildren<mutantHitReceiver>();
				if (componentInChildren5)
				{
					componentInChildren5.sendHitFallDown((float)ev.Hit);
				}
			}
			else
			{
				transform.SendMessage("getAttacker", (ev.RaisedBy.UserData as BoltEntity).gameObject, SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("getAttackerType", ev.getAttackerType, SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("getAttackDirection", ev.getAttackDirection, SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("getCombo", ev.getCombo, SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("takeDamage", ev.takeDamage, SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("setSkinDamage", UnityEngine.Random.Range(0, 3), SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("ApplyAnimalSkinDamage", ev.getAttackDirection, SendMessageOptions.DontRequireReceiver);
				transform.SendMessage("HitReal", ev.Hit, SendMessageOptions.DontRequireReceiver);
				if (ev.HitAxe)
				{
					transform.SendMessage("HitAxe", SendMessageOptions.DontRequireReceiver);
				}
				if (ev.Burn)
				{
					transform.SendMessage("Burn", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		finally
		{
			EnemyHealth.CurrentAttacker = null;
		}
	}

	
	public override void OnEvent(SendMessageEvent evnt)
	{
		if (evnt.Target)
		{
			evnt.Target.gameObject.SendMessage(evnt.Message);
		}
	}

	
	public override void OnEvent(CancelBluePrint evnt)
	{
		if (evnt.BluePrint)
		{
			Craft_Structure componentInChildren = evnt.BluePrint.GetComponentInChildren<Craft_Structure>();
			if (componentInChildren)
			{
				componentInChildren.CancelBlueprintSafe();
			}
		}
	}

	
	public override void OnEvent(AddRepairMaterial evnt)
	{
		if (evnt.Building)
		{
			FoundationHealth component = evnt.Building.GetComponent<FoundationHealth>();
			BuildingHealth component2 = evnt.Building.GetComponent<BuildingHealth>();
			if (evnt.IsLog)
			{
				if (component && component.CalcMissingRepairLogs() > 0)
				{
					component.AddRepairMaterialReal(true);
				}
				else if (component2 && component2.CalcMissingRepairLogs() > 0)
				{
					component2.AddRepairMaterialReal(true);
				}
			}
			else if (component && component.CalcMissingRepairMaterial() > 0)
			{
				component.AddRepairMaterialReal(false);
			}
			else if (component2 && component2.CalcMissingRepairMaterial() > 0)
			{
				component2.AddRepairMaterialReal(false);
			}
		}
	}

	
	public override void OnEvent(ItemHolderAddItem evnt)
	{
		if (evnt.Target)
		{
			rockThrowerItemHolder componentInChildren = evnt.Target.GetComponentInChildren<rockThrowerItemHolder>();
			if (componentInChildren)
			{
				componentInChildren.AddItemMP(evnt.ContentType);
				return;
			}
			MultiThrowerItemHolder componentInChildren2 = evnt.Target.GetComponentInChildren<MultiThrowerItemHolder>();
			if (componentInChildren2)
			{
				componentInChildren2.AddItemMP(evnt.ContentType);
				return;
			}
			LogHolder componentInChildren3 = evnt.Target.GetComponentInChildren<LogHolder>();
			if (componentInChildren3)
			{
				componentInChildren3.AddItemMP(evnt.RaisedBy);
				return;
			}
			ItemHolder componentInChildren4 = evnt.Target.GetComponentInChildren<ItemHolder>();
			if (componentInChildren4)
			{
				componentInChildren4.AddItemMP(evnt.RaisedBy);
				return;
			}
			MultiHolder[] componentsInChildren = evnt.Target.GetComponentsInChildren<MultiHolder>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].AddItemMP((MultiHolder.ContentTypes)evnt.ContentType, evnt.RaisedBy);
				return;
			}
			MultiItemHolder componentInChildren5 = evnt.Target.GetComponentInChildren<MultiItemHolder>();
			if (componentInChildren5)
			{
				componentInChildren5.AddItemMP(evnt.ContentType);
				return;
			}
			MultiItemRack componentInChildren6 = evnt.Target.GetComponentInChildren<MultiItemRack>();
			if (componentInChildren6)
			{
				componentInChildren6.AddItemMP(evnt.ContentType, evnt.ContentInfo);
				return;
			}
		}
	}

	
	public override void OnEvent(TakeBody evnt)
	{
		if (evnt.Sled && evnt.Body)
		{
			MultiHolder[] componentsInChildren = evnt.Sled.GetComponentsInChildren<MultiHolder>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].TakeBodyMP(evnt.Body, evnt.RaisedBy);
			}
		}
	}

	
	public override void OnEvent(AddBody evnt)
	{
		if (evnt.Sled && evnt.Body)
		{
			MultiHolder[] componentsInChildren = evnt.Sled.GetComponentsInChildren<MultiHolder>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].AddBodyMP(evnt.Body);
			}
		}
	}

	
	public override void OnEvent(OpenSuitcase2 evnt)
	{
		if (evnt.Suitcase)
		{
			evnt.Suitcase.GetState<ISuitcaseState>().Open = true;
		}
	}

	
	public override void OnEvent(ItemHolderTakeItem evnt)
	{
		if (evnt.Target)
		{
			rockThrowerItemHolder componentInChildren = evnt.Target.GetComponentInChildren<rockThrowerItemHolder>();
			if (componentInChildren)
			{
				componentInChildren.TakeItemMP(evnt.Player, evnt.ContentType);
				return;
			}
			MultiThrowerItemHolder componentInChildren2 = evnt.Target.GetComponentInChildren<MultiThrowerItemHolder>();
			if (componentInChildren2)
			{
				componentInChildren2.TakeItemMP(evnt.Player, evnt.ContentType);
				return;
			}
			LogHolder componentInChildren3 = evnt.Target.GetComponentInChildren<LogHolder>();
			if (componentInChildren3)
			{
				componentInChildren3.TakeItemMP(evnt.Player);
				return;
			}
			ItemHolder componentInChildren4 = evnt.Target.GetComponentInChildren<ItemHolder>();
			if (componentInChildren4)
			{
				componentInChildren4.TakeItemMP(evnt.Player);
				return;
			}
			MultiItemHolder componentInChildren5 = evnt.Target.GetComponentInChildren<MultiItemHolder>();
			if (componentInChildren5)
			{
				componentInChildren5.TakeItemMP(evnt.Player, evnt.ContentType);
				return;
			}
			MultiHolder[] componentsInChildren = evnt.Target.GetComponentsInChildren<MultiHolder>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].TakeItemMP(evnt.Player, (MultiHolder.ContentTypes)evnt.ContentType);
				return;
			}
			MultiItemRack componentInChildren6 = evnt.Target.GetComponentInChildren<MultiItemRack>();
			if (componentInChildren6)
			{
				componentInChildren6.TakeItemMP(evnt.RaisedBy, evnt.ContentType, evnt.ContentValue);
				return;
			}
		}
	}

	
	public override void OnEvent(RockThrowerRemoveItem evnt)
	{
		if (evnt.Target)
		{
			rockThrowerItemHolder componentInChildren = evnt.Target.GetComponentInChildren<rockThrowerItemHolder>();
			if (componentInChildren)
			{
				componentInChildren.loadItemIntoBasket(evnt.ContentType);
				return;
			}
			MultiThrowerItemHolder componentInChildren2 = evnt.Target.GetComponentInChildren<MultiThrowerItemHolder>();
			if (componentInChildren2)
			{
				componentInChildren2.loadItemIntoBasket(evnt.ContentType);
				return;
			}
		}
	}

	
	public override void OnEvent(RockThrowerActivated evnt)
	{
		if (evnt.Target)
		{
			evnt.Target.SendMessage("disableTrigger");
		}
	}

	
	public override void OnEvent(RockThrowerDeActivated evnt)
	{
		if (evnt.Target)
		{
			evnt.Target.SendMessage("enableTrigger");
		}
	}

	
	public override void OnEvent(RockThrowerAnimate evnt)
	{
		if (evnt.Target)
		{
			coopRockThrower component = evnt.Target.GetComponent<coopRockThrower>();
			if (component)
			{
				component.setAnimator(evnt.animVar, evnt.onoff);
			}
		}
	}

	
	public override void OnEvent(RockThrowerResetAmmo evnt)
	{
		if (evnt.Target)
		{
			rockThrowerItemHolder componentInChildren = evnt.Target.GetComponentInChildren<rockThrowerItemHolder>();
			if (componentInChildren)
			{
				componentInChildren.resetBasketAmmo();
				return;
			}
			MultiThrowerItemHolder componentInChildren2 = evnt.Target.GetComponentInChildren<MultiThrowerItemHolder>();
			if (componentInChildren2)
			{
				componentInChildren2.resetBasketAmmo();
			}
		}
	}

	
	public override void OnEvent(RockThrowerLandTarget evnt)
	{
		if (evnt.Target)
		{
			rockThrowerAnimEvents componentInChildren = evnt.Target.GetComponentInChildren<rockThrowerAnimEvents>();
			if (componentInChildren)
			{
				componentInChildren.landTarget = evnt.landPos;
			}
		}
	}

	
	public override void OnEvent(ChatEvent evnt)
	{
		for (int i = 0; i < Scene.SceneTracker.allPlayerEntities.Count; i++)
		{
			if (Scene.SceneTracker.allPlayerEntities[i].source == evnt.RaisedBy)
			{
				if (Scene.SceneTracker.allPlayerEntities[i].networkId == evnt.Sender)
				{
					ChatEvent chatEvent = ChatEvent.Create(GlobalTargets.AllClients);
					chatEvent.Sender = evnt.Sender;
					chatEvent.Message = evnt.Message;
					chatEvent.Send();
				}
				return;
			}
		}
		if (BoltNetwork.isServer && evnt.RaisedBy == null)
		{
			ChatEvent chatEvent2 = ChatEvent.Create(GlobalTargets.AllClients);
			chatEvent2.Sender = evnt.Sender;
			chatEvent2.Message = evnt.Message;
			chatEvent2.Send();
		}
	}

	
	public override void OnEvent(SwapGhost evnt)
	{
		if (evnt.GhostEntity)
		{
			evnt.GhostEntity.GetComponentInChildren<Craft_Structure>().SwapToNextGhost();
		}
	}

	
	public override void OnEvent(AddIngredient evnt)
	{
		if (evnt.Construction)
		{
			evnt.Construction.GetComponentInChildren<Craft_Structure>().AddIngrendient_Actual(evnt.IngredientNum, false, evnt.RaisedBy);
		}
		else
		{
			PlayerAddItem playerAddItem = PlayerAddItem.Create(evnt.RaisedBy);
			playerAddItem.ItemId = evnt.ItemId;
			playerAddItem.Amount = 1;
			playerAddItem.Send();
		}
	}

	
	public override void OnEvent(DestroyBuilding evnt)
	{
		if (evnt.BuildingEntity && !PlayerPreferences.NoDestruction)
		{
			BoltNetwork.Destroy(evnt.BuildingEntity);
		}
	}

	
	public override void OnEvent(RemoveBuilding evnt)
	{
		if (!PlayerPreferences.NoDestruction && evnt.TargetBuilding && evnt.RaisedBy != null && evnt.RaisedBy.UserData != null && evnt.RaisedBy.UserData is BoltEntity && Vector3.Distance(evnt.TargetBuilding.transform.position, (evnt.RaisedBy.UserData as BoltEntity).transform.position) < 30f)
		{
			evnt.TargetBuilding.gameObject.AddComponent<CollapseStructure>();
		}
	}

	
	public override void OnEvent(RabbitAdd evnt)
	{
		if (evnt.Cage)
		{
			evnt.Cage.GetState<IRabbitCage>().RabbitCount++;
		}
	}

	
	public override void OnEvent(RabbitTake evnt)
	{
		if (evnt.Cage)
		{
			evnt.Cage.GetState<IRabbitCage>().RabbitCount--;
		}
	}

	
	public override void OnEvent(SpawnBunny evnt)
	{
		Transform transform = (UnityEngine.Object.Instantiate(Resources.Load<CoopRabbitReference>("CoopRabbitReference").Rabbit, evnt.Pos, Quaternion.identity) as GameObject).transform;
		if (!transform)
		{
			return;
		}
		if (transform)
		{
			transform.GetChild(0).eulerAngles = new Vector3(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		}
		if (transform)
		{
			transform.SendMessage("startUpdateSpawn");
		}
		AnimalSpawnController.AttachAnimalToNetwork(null, transform.gameObject);
	}

	
	public override void OnEvent(playerSwingWeapon evnt)
	{
		if (Scene.SceneTracker)
		{
			for (int i = 0; i < Scene.SceneTracker.visibleEnemies.Count; i++)
			{
				if (Scene.SceneTracker.visibleEnemies[i])
				{
					Scene.SceneTracker.visibleEnemies[i].SendMessage("setPlayerAttacking", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	
	public override void OnEvent(playerSyncAttack evnt)
	{
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
		if (evnt.target)
		{
			CoopPlayerClientHitPrediction component = evnt.target.GetComponent<CoopPlayerClientHitPrediction>();
			if (component)
			{
				component.setJumpEvent();
			}
		}
	}

	
	public override void OnEvent(syncTorchLight evnt)
	{
		Transform transform = PoolManager.Pools["misc"].Spawn(LocalPlayer.AnimControl.torchVisGo.transform, evnt.position, Quaternion.identity);
		torchLightSetup component = transform.GetComponent<torchLightSetup>();
		if (component)
		{
			component.sourcePos = evnt.sourcePosition;
			component.distanceToPlayer = evnt.distanceToPlayer;
		}
	}

	
	public override void OnEvent(syncWorkBench evnt)
	{
		if (evnt.target)
		{
			IworkBenchState state = evnt.target.GetState<IworkBenchState>();
			if (state != null)
			{
				if (evnt.occupied1)
				{
					state.occupied1 = true;
				}
				if (evnt.occupied2)
				{
					state.occupied2 = true;
				}
				if (evnt.resetOccupied1)
				{
					state.occupied1 = false;
				}
				if (evnt.resetOccupied2)
				{
					state.occupied2 = false;
				}
			}
		}
	}

	
	public override void OnEvent(updateMecanimRemoteState evnt)
	{
		if (evnt.Target)
		{
			CoopMecanimReplicator component = evnt.Target.GetComponent<CoopMecanimReplicator>();
			if (component)
			{
				component.ApplyHashToRemote(evnt.layer, evnt.hash, evnt.anyState, evnt.normalizedTime, false);
			}
		}
	}

	
	public override void OnEvent(deadSharkDestroy evnt)
	{
		if (evnt.target)
		{
			if (evnt.switchToRagdoll)
			{
				evnt.target.SendMessage("switchToRagdoll", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				evnt.target.SendMessage("destroyShark", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public override void OnEvent(deadSharkCutHead evnt)
	{
		if (evnt.target)
		{
			evnt.target.SendMessage("switchToCutHead", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public override void OnEvent(playerBlock evnt)
	{
		evnt.target.gameObject.SendMessage("forcePlayerBlock", SendMessageOptions.DontRequireReceiver);
	}

	
	public override void OnEvent(SetBuildingFlag evnt)
	{
		if (evnt.BuildingEntity)
		{
			HomeIcon componentInChildren = evnt.BuildingEntity.GetComponentInChildren<HomeIcon>();
			if (componentInChildren)
			{
				componentInChildren.SetMpStateFlagHost(evnt.Flag);
			}
		}
	}

	
	public override void OnEvent(RequestAnimationSequenceProxy evnt)
	{
		AnimationSequence.GetProxyFor(evnt.Position);
	}

	
	public override void OnEvent(BeginAnimationSequenceStage evnt)
	{
		evnt.Proxy.GetComponent<AnimationSequenceProxy>().BeginStage(evnt.Stage, BoltNetwork.serverTime, evnt.Actor);
	}

	
	public override void OnEvent(ProgressAnimationSequenceStage evnt)
	{
		AnimationSequenceProxy component = evnt.Proxy.GetComponent<AnimationSequenceProxy>();
		if (component && evnt.RaisedBy == component.state.Actor.source)
		{
			component.ProgressStage();
		}
	}

	
	public override void OnEvent(CompleteAnimationSequenceStage evnt)
	{
		AnimationSequenceProxy component = evnt.Proxy.GetComponent<AnimationSequenceProxy>();
		if (component && evnt.RaisedBy == component.state.Actor.source)
		{
			component.CompleteStage(evnt.Stage);
		}
	}

	
	public override void EntityDetached(BoltEntity entity)
	{
		if (entity.StateIs<IPlayerState>() && Scene.SceneTracker)
		{
			if (Scene.SceneTracker.allPlayers.Contains(entity.gameObject))
			{
				Scene.SceneTracker.allPlayers.Remove(entity.gameObject);
				if (SteamDSConfig.isDedicatedServer)
				{
					if (Scene.SceneTracker.allPlayers.Count == 0)
					{
						GameObject gameObject = new GameObject("TempFakePlayer");
						gameObject.transform.position = 10000f * Vector3.one;
						Scene.SceneTracker.allPlayers.Add(gameObject);
						SteamDSConfig.isUsingDummyPlayer = true;
						Debug.Log("One player leave dedicated server. No players at the server.");
					}
					else
					{
						Debug.Log("One player leave dedicated server. Total " + Scene.SceneTracker.allPlayers.Count + " player(s).");
					}
				}
			}
			if (Scene.SceneTracker.allPlayerEntities.Contains(entity))
			{
				Scene.SceneTracker.allPlayerEntities.Remove(entity);
				if (SteamDSConfig.isDedicatedServer && Scene.SceneTracker.allPlayerEntities.Count > 0)
				{
					this.SendMasterClientEvent();
				}
			}
		}
	}

	
	private void SendMasterClientEvent()
	{
		if (SteamDSConfig.isDedicatedServer && Scene.SceneTracker.allPlayerEntities.Count > 0)
		{
			for (int i = 0; i < Scene.SceneTracker.allPlayerEntities.Count; i++)
			{
				BoltConnection source = Scene.SceneTracker.allPlayerEntities[i].source;
				SetMasterClient setMasterClient = SetMasterClient.Create(source);
				setMasterClient.IsMaster = (i == 0);
				setMasterClient.Send();
			}
		}
	}

	
	public override void EntityReceived(BoltEntity entity)
	{
		if (entity.StateIs<IPlayerState>() && Scene.SceneTracker)
		{
			if (!Scene.SceneTracker.allPlayers.Contains(entity.gameObject))
			{
				if (SteamDSConfig.isDedicatedServer && SteamDSConfig.isUsingDummyPlayer && Scene.SceneTracker.allPlayers.Count == 1)
				{
					GameObject obj = Scene.SceneTracker.allPlayers[0];
					Scene.SceneTracker.allPlayers.Clear();
					SteamDSConfig.isUsingDummyPlayer = false;
					UnityEngine.Object.Destroy(obj);
				}
				Scene.SceneTracker.allPlayers.Add(entity.gameObject);
				if (SteamDSConfig.isDedicatedServer)
				{
					Debug.Log("New player joined dedicated server. Total " + Scene.SceneTracker.allPlayers.Count + " player(s).");
				}
				else
				{
					Debug.Log("New player joined game. Total " + Scene.SceneTracker.allPlayers.Count + " player(s).");
				}
			}
			if (!Scene.SceneTracker.allPlayerEntities.Contains(entity))
			{
				Scene.SceneTracker.allPlayerEntities.Add(entity);
				if (SteamDSConfig.isDedicatedServer && Scene.SceneTracker.allPlayerEntities.Count == 1)
				{
					this.SendMasterClientEvent();
				}
			}
		}
	}

	
	public override void OnEvent(syncGirlPickup evnt)
	{
		if (evnt.dedicatedSpawn)
		{
			base.StartCoroutine(this.dedicatedGirlToMachineSpawn(evnt.spawnPos, evnt.spawnRot));
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
			netAnimatorSetup componentInChildren2 = evnt.target.GetComponentInChildren<netAnimatorSetup>();
			if (componentInChildren2 && evnt.spawnGirl)
			{
				componentInChildren2.spawnNewGirlPickup(evnt.syncPutDownAnimation);
			}
		}
	}

	
	private IEnumerator dedicatedGirlToMachineSpawn(Vector3 pos, Quaternion rot)
	{
		GameObject spawn = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CutScene/girl_NoPickup"), pos, rot);
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		syncGirlPickup ev = syncGirlPickup.Create(GlobalTargets.Everyone);
		ev.target = spawn.GetComponent<BoltEntity>();
		ev.toMachine = true;
		ev.Send();
		yield break;
	}

	
	public override void OnEvent(stuckArrowsSync evnt)
	{
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
					gameObject = (GameObject)UnityEngine.Object.Instantiate(arrowStickToTarget.fakeArrowPickup, evnt.target.transform.position, evnt.target.transform.rotation);
				}
				else
				{
					gameObject = (GameObject)UnityEngine.Object.Instantiate(arrowStickToTarget.fakeArrowBonePickup, evnt.target.transform.position, evnt.target.transform.rotation);
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

	
	public override void OnEvent(spawnTreeDust evnt)
	{
		Prefabs.Instance.SpawnWoodChopPS(evnt.position, evnt.rotation);
	}

	
	public override void OnEvent(playerInCave evnt)
	{
		if (evnt.target)
		{
			if (evnt.inCave)
			{
				evnt.target.gameObject.SendMessage("InACave");
			}
			else if (Scene.SceneTracker.allPlayersInCave.Count == 0)
			{
				Scene.MutantControler.disableMpCaveMutants();
			}
		}
	}

	
	public override void OnEvent(syncPlayerRenderers evnt)
	{
		if (LocalPlayer.Transform)
		{
			CoopMecanimReplicator componentInChildren = LocalPlayer.Transform.GetComponentInChildren<CoopMecanimReplicator>();
			if (componentInChildren)
			{
				componentInChildren.forceStateUpdate();
			}
		}
	}

	
	public override void OnEvent(Sleep evnt)
	{
		if (SteamDSConfig.isDedicatedServer && Scene.SceneTracker.allPlayerEntities.Count > 0 && evnt.RaisedBy == Scene.SceneTracker.allPlayerEntities[0])
		{
			if (Scene.SceneTracker.allPlayerEntities.All((BoltEntity entity) => entity.GetState<IPlayerState>().CurrentView == 9))
			{
				if (!evnt.Aborted)
				{
					Scene.MutantSpawnManager.offsetSleepAmounts();
					Scene.MutantControler.startSetupFamilies();
					EventRegistry.Player.Publish(TfEvent.Slept, null);
					Scene.Atmosphere.TimeLapse();
					ShelterTrigger.CheckRegrowTrees();
				}
				Sleep sleep = Sleep.Create(GlobalTargets.AllClients);
				sleep.Aborted = evnt.Aborted;
				sleep.Send();
			}
		}
	}

	
	private IEnumerator syncLocalPlayerRenderers()
	{
		float timer = Time.time + 10f;
		while (!Scene.SceneTracker || Scene.SceneTracker.allPlayerEntities.Count == 0)
		{
			if (Time.time > timer)
			{
				yield break;
			}
			yield return null;
		}
		foreach (BoltEntity go in Scene.SceneTracker.allPlayerEntities)
		{
			if (go)
			{
				netHideDuringPlaneCrash nh = go.transform.GetComponentInChildren<netHideDuringPlaneCrash>();
				if (nh)
				{
					nh.setPlayerRenderers(true);
				}
			}
		}
		yield return null;
		yield break;
	}

	
	public override void OnEvent(setupEndBoss evnt)
	{
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
			Debug.Log("end boss  = " + evnt.bossActive);
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
		if (evnt.scaleHack && evnt.target)
		{
			base.StartCoroutine(this.scaleGirlMutant(1.25f, evnt.target.transform));
			return;
		}
		if (evnt.activateBoss && evnt.target)
		{
			creepyAnimatorControl creepyAnimatorControl = evnt.target.transform.GetComponentsInChildren<creepyAnimatorControl>(true)[0];
			creepyAnimatorControl.enabled = true;
			creepyAnimatorControl.activateGirlMutant();
			creepyAnimatorControl.animator.CrossFade("Base Layer.idle1", 0f, 0, 0f);
			return;
		}
		if (evnt.spawnBoss && !Scene.SceneTracker.endBossSpawned)
		{
			GameObject gameObject2 = GameObject.FindWithTag("endGameBossPrefab");
			if (gameObject2)
			{
				Scene.SceneTracker.endBossSpawned = true;
				setupGirlMutant component = gameObject2.GetComponent<setupGirlMutant>();
				UnityEngine.Object.Instantiate(component.realPrefab, evnt.pos, evnt.rot);
			}
			else
			{
				base.StartCoroutine(this.setupBossPrefabAsync(evnt.pos, evnt.rot));
			}
		}
	}

	
	private IEnumerator scaleGirlMutant(float amount, Transform target)
	{
		float timer = 0f;
		Vector3 newScale = new Vector3(amount, amount, amount);
		while (timer < 1f)
		{
			timer += Time.deltaTime / 3f;
			target.localScale = Vector3.Slerp(target.localScale, newScale, timer);
			yield return null;
		}
		yield return null;
		yield break;
	}

	
	private IEnumerator setupBossPrefabAsync(Vector3 pos, Quaternion rot)
	{
		Scene.SceneTracker.endBossSpawned = true;
		AsyncOperation async = Application.LoadLevelAdditiveAsync("endgame_animPrefabs");
		yield return async;
		GameObject spawnPrefab = GameObject.FindWithTag("endGameBossPrefab");
		if (spawnPrefab)
		{
			Scene.SceneTracker.endBossSpawned = true;
			setupGirlMutant sgm = spawnPrefab.GetComponent<setupGirlMutant>();
			UnityEngine.Object.Instantiate(sgm.realPrefab, pos, rot);
		}
		yield break;
	}

	
	public override void OnEvent(loadEndGamePrefabs evnt)
	{
		if (GameObject.FindWithTag("endGameBossPrefab") == null)
		{
			Application.LoadLevelAdditiveAsync(evnt.sceneName);
			Debug.Log("loading end game anim prefabs");
		}
	}

	
	public override void OnEvent(enemyRunAwayOverride evnt)
	{
		if (evnt.target && evnt.target.IsAttached() && evnt.target.gameObject.activeSelf)
		{
			evnt.target.SendMessage("setRunAwayFromPlayer");
		}
	}

	
	public override void OnEvent(debugCommand evnt)
	{
		GameObject gameObject = GameObject.Find("DebugConsole");
		if (gameObject)
		{
			if (evnt.input2.Length > 0)
			{
				if (evnt.target)
				{
					gameObject.SendMessage("setLastLocalTarget", evnt.target.transform);
				}
				gameObject.SendMessage(evnt.input, evnt.input2);
			}
			else if (evnt.target)
			{
				gameObject.SendMessage(evnt.input, evnt.target.transform);
			}
			else
			{
				gameObject.SendMessage(evnt.input, "null");
			}
		}
	}

	
	private void Update()
	{
		if (CoopServerInfo.Instance && CoopLobby.Instance != null && !CoopPeerStarter.Dedicated)
		{
			CoopLobby instance = CoopLobby.Instance;
			int num = BoltNetwork.clients.Count<BoltConnection>() + 1;
			CoopServerInfo.Instance.state.PlayerCount = num;
			instance.SetCurrentMembers(num);
		}
		if (CoopServerInfo.Instance && this.tracker.Component)
		{
			if (LocalPlayer.Entity && string.IsNullOrEmpty(CoopServerInfo.Instance.state.PlayerNames[0]))
			{
				CoopServerInfo.Instance.state.PlayerNames[0] = LocalPlayer.Entity.GetState<IPlayerState>().name + " (host)";
			}
			int num2 = Mathf.Min(CoopServerInfo.Instance.state.PlayerNames.Count<string>() - 1, this.tracker.Component.allPlayerEntities.Count);
			for (int i = 0; i < num2; i++)
			{
				if (CoopServerInfo.Instance.state.PlayerNames[i + 1] != this.tracker.Component.allPlayerEntities[i].GetState<IPlayerState>().name)
				{
					CoopServerInfo.Instance.state.PlayerNames[i + 1] = this.tracker.Component.allPlayerEntities[i].GetState<IPlayerState>().name;
				}
			}
			for (int j = this.tracker.Component.allPlayerEntities.Count + 1; j < CoopServerInfo.Instance.state.PlayerNames.Length; j++)
			{
				if (!string.IsNullOrEmpty(CoopServerInfo.Instance.state.PlayerNames[j]))
				{
					CoopServerInfo.Instance.state.PlayerNames[j] = string.Empty;
				}
			}
		}
		CoopTreeGrid.AttachTrees();
		if (this.tracker.Component)
		{
			CoopTreeGrid.AttachAdjacent(this.tracker.Component.allPlayers);
		}
	}

	
	~CoopServerCallbacks()
	{
		if (CoopPeerStarter.DedicatedHost)
		{
			CoopPeerStarter.DedicatedHost = false;
			try
			{
				SteamGameServer.LogOff();
			}
			catch (Exception)
			{
			}
			try
			{
				GameServer.Shutdown();
			}
			catch (Exception)
			{
			}
		}
	}

	
	private Vector3[] player_positions;

	
	private CachedGlobal<sceneTracker> tracker;
}
