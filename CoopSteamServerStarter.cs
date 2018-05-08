using System;
using System.Collections;
using BoltInternal;
using Steamworks;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UnityEngine;


internal class CoopSteamServerStarter : CoopPeerStarter
{
	
	private IEnumerator Start()
	{
		PlayerPrefs.Save();
		CoopTreeGrid.Init();
		LoadSave.OnGameStart += this.OnGameStart;
		yield return CoopPeerStarter.PrefabDbResource;
		if (!CoopPeerStarter.Dedicated)
		{
			CoopLobby.Instance.SetServer(SteamGameServer.GetSteamID());
		}
		BoltConfig config = base.GetConfig();
		config.serverConnectionLimit = ((!CoopPeerStarter.Dedicated) ? (CoopLobby.Instance.Info.MemberLimit - 1) : SteamDSConfig.ServerPlayers);
		if (CoopPeerStarter.Dedicated)
		{
			BoltLauncher.SetUdpPlatform(new DotNetPlatform());
			BoltLauncher.StartServer(SteamDSConfig.EndPoint, config);
		}
		else
		{
			BoltLauncher.StartServer(SteamGameServer.GetSteamID().ToEndPoint(), config);
		}
		try
		{
			BoltNetwork.AddGlobalEventListener(CoopAckChecker.Instance);
		}
		catch
		{
		}
		yield break;
	}

	
	public override void Connected(BoltConnection connection)
	{
		connection.SetCanReceiveEntities(false);
		if (CoopKick.IsBanned(connection.RemoteEndPoint.SteamId))
		{
			connection.Disconnect(new CoopKickToken
			{
				Banned = true,
				KickMessage = "HOST_BANNED_YOU_PERMANANTLY"
			});
		}
		else
		{
			CoopServerInfo.Instance.entity.Freeze(false);
		}
	}

	
	public override void BoltStartDone()
	{
		BoltNetwork.AddGlobalEventListener(CoopAckChecker.Instance);
		if (GameSetup.IsSavedGame && !LevelSerializer.CanResume)
		{
			Debug.Log(string.Concat(new object[]
			{
				"Will force game to new because GameSetup.IsSavedGame",
				GameSetup.IsSavedGame,
				" LevelSerializer.CanResume ",
				LevelSerializer.CanResume
			}));
			GameSetup.SetInitType(InitTypes.New);
		}
		if (GameSetup.IsSavedGame)
		{
			AnimalSpawnController.lastUpdate = Time.realtimeSinceStartup + 60f;
		}
		base.BoltSetup();
		BoltNetwork.Instantiate(BoltPrefabs.CoopServerInfo).transform.position = new Vector3(0f, 0f, 0f);
		ICoopServerInfo state = CoopServerInfo.Instance.state;
		this.mapState = CoopPeerStarter.MapState.Begin;
	}

	
	private void OnGameStart()
	{
		try
		{
			AnimalSpawnController.lastUpdate = Time.realtimeSinceStartup + 30f;
			try
			{
				BoltNetwork.UpdateSceneObjectsLookup();
			}
			catch (Exception)
			{
			}
			CoopTreeGrid.Update(BoltNetwork.SceneObjects);
			this.AttachBuildings();
		}
		finally
		{
			LoadSave.OnGameStart -= this.OnGameStart;
		}
	}

	
	public static void AttachBuildingBoltEntity(BoltEntity entity)
	{
		if (!BoltNetwork.isServer)
		{
			return;
		}
		if (!CoopSteamServerStarter.SaveIsLoading)
		{
			return;
		}
		if (!entity)
		{
			return;
		}
		if (entity.IsAttached())
		{
			return;
		}
		BoltEntitySettingsModifier boltEntitySettingsModifier = entity.ModifySettings();
		BridgeArchitect component = entity.GetComponent<BridgeArchitect>();
		ZiplineArchitect component2 = entity.GetComponent<ZiplineArchitect>();
		GardenArchitect component3 = entity.GetComponent<GardenArchitect>();
		CraneArchitect component4 = entity.GetComponent<CraneArchitect>();
		if (boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IFireState)
		{
			BoltNetwork.Attach(entity);
		}
		else if (boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IRaftState)
		{
			BoltNetwork.Attach(entity);
			if (entity && entity.isAttached && entity.StateIs<IRaftState>())
			{
				entity.GetState<IRaftState>().IsReal = true;
			}
		}
		else if (boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IMultiHolderState)
		{
			BoltNetwork.Attach(entity);
			if (entity && entity.isAttached && entity.StateIs<IMultiHolderState>())
			{
				entity.GetState<IMultiHolderState>().IsReal = true;
				MultiHolder[] componentsInChildren = entity.GetComponentsInChildren<MultiHolder>(true);
				if (componentsInChildren.Length > 0)
				{
					componentsInChildren[0]._contentActual = componentsInChildren[0]._contentAmount;
					componentsInChildren[0]._contentTypeActual = componentsInChildren[0]._content;
				}
			}
		}
		else if (component)
		{
			BoltNetwork.Attach(entity, component.CustomToken);
		}
		else if (component2)
		{
			BoltNetwork.Attach(entity, component2.CustomToken);
		}
		else if (component3)
		{
			BoltNetwork.Attach(entity, component3.CustomToken);
		}
		else if (component4)
		{
			BoltNetwork.Attach(entity, component4.CustomToken);
		}
		else if (boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IFoundationState || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IBuildingState || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IRabbitCage || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.ITreeBuildingState || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.ITrapLargeState || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IBuildingDestructibleState || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IWallChunkBuildingState)
		{
			CoopBuildingEx component5 = entity.GetComponent<CoopBuildingEx>();
			WallChunkArchitect component6 = entity.GetComponent<WallChunkArchitect>();
			if (component6)
			{
				BoltNetwork.Attach(entity, component6.CustomToken);
			}
			else if (component5)
			{
				CoopConstructionExToken coopConstructionExToken = new CoopConstructionExToken();
				coopConstructionExToken.Architects = new CoopConstructionExToken.ArchitectData[component5.Architects.Length];
				for (int i = 0; i < component5.Architects.Length; i++)
				{
					coopConstructionExToken.Architects[i].PointsCount = (component5.Architects[i] as ICoopStructure).MultiPointsCount;
					coopConstructionExToken.Architects[i].PointsPositions = (component5.Architects[i] as ICoopStructure).MultiPointsPositions.ToArray();
					coopConstructionExToken.Architects[i].CustomToken = (component5.Architects[i] as ICoopStructure).CustomToken;
					if (component5.Architects[i] is FoundationArchitect)
					{
						coopConstructionExToken.Architects[i].AboveGround = ((FoundationArchitect)component5.Architects[i])._aboveGround;
					}
					if (component5.Architects[i] is RoofArchitect && (component5.Architects[i] as RoofArchitect).CurrentSupport != null)
					{
						coopConstructionExToken.Architects[i].Support = ((component5.Architects[i] as RoofArchitect).CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
					}
					if (component5.Architects[i] is FloorArchitect && (component5.Architects[i] as FloorArchitect).CurrentSupport != null)
					{
						coopConstructionExToken.Architects[i].Support = ((component5.Architects[i] as FloorArchitect).CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
					}
					CoopSteamServerStarter.AttachBuildingBoltEntity(coopConstructionExToken.Architects[i].Support);
				}
				BoltNetwork.Attach(entity, coopConstructionExToken);
			}
			else
			{
				BoltNetwork.Attach(entity);
			}
		}
		else if (boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IConstructionState || boltEntitySettingsModifier.serializerId == StateSerializerTypeIds.IWallChunkConstructionState)
		{
			CoopConstructionEx component7 = entity.GetComponent<CoopConstructionEx>();
			WallChunkArchitect component8 = entity.GetComponent<WallChunkArchitect>();
			if (component8)
			{
				BoltNetwork.Attach(entity, component8.CustomToken);
			}
			else if (component7)
			{
				CoopConstructionExToken coopConstructionExToken2 = new CoopConstructionExToken();
				coopConstructionExToken2.Architects = new CoopConstructionExToken.ArchitectData[component7.Architects.Length];
				for (int j = 0; j < component7.Architects.Length; j++)
				{
					coopConstructionExToken2.Architects[j].PointsCount = (component7.Architects[j] as ICoopStructure).MultiPointsCount;
					coopConstructionExToken2.Architects[j].PointsPositions = (component7.Architects[j] as ICoopStructure).MultiPointsPositions.ToArray();
					coopConstructionExToken2.Architects[j].CustomToken = (component7.Architects[j] as ICoopStructure).CustomToken;
					if (component7.Architects[j] is FoundationArchitect)
					{
						coopConstructionExToken2.Architects[j].AboveGround = ((FoundationArchitect)component7.Architects[j])._aboveGround;
					}
					if (component7.Architects[j] is RoofArchitect && (component7.Architects[j] as RoofArchitect).CurrentSupport != null)
					{
						coopConstructionExToken2.Architects[j].Support = ((component7.Architects[j] as RoofArchitect).CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
					}
					if (component7.Architects[j] is FloorArchitect && (component7.Architects[j] as FloorArchitect).CurrentSupport != null)
					{
						coopConstructionExToken2.Architects[j].Support = ((component7.Architects[j] as FloorArchitect).CurrentSupport as MonoBehaviour).GetComponent<BoltEntity>();
					}
					CoopSteamServerStarter.AttachBuildingBoltEntity(coopConstructionExToken2.Architects[j].Support);
				}
				BoltNetwork.Attach(entity, coopConstructionExToken2);
			}
			else
			{
				BoltNetwork.Attach(entity);
			}
		}
	}

	
	private void AttachBuildings()
	{
		CoopSteamServerStarter.SaveIsLoading = true;
		try
		{
			foreach (BoltEntity entity in UnityEngine.Object.FindObjectsOfType<BoltEntity>())
			{
				CoopSteamServerStarter.AttachBuildingBoltEntity(entity);
			}
		}
		finally
		{
			CoopSteamServerStarter.SaveIsLoading = false;
		}
	}

	
	protected override void OnLoadingDone()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			SteamDSConfig.StartAutoSaveMode();
		}
		BoltEntity boltEntity = BoltNetwork.Instantiate(BoltPrefabs.CoopBuildMission);
		boltEntity.transform.position = new Vector3(0f, 100f, 0f);
		BoltEntity boltEntity2 = BoltNetwork.Instantiate(BoltPrefabs.CoopWeatherProxy);
		boltEntity2.GetState<IWeatherState>().TimeOfDay = -1f;
		boltEntity2.transform.position = new Vector3(0f, 200f, 0f);
		if (CoopPeerStarter.DedicatedHost)
		{
			Console.WriteLine("Dedicated Server Running");
			Console.WriteLine("Address: " + SteamDSConfig.EndPoint);
			Console.WriteLine("Max Players: " + SteamDSConfig.ServerPlayers);
			Console.WriteLine("Save Interval: " + SteamDSConfig.GameAutoSaveIntervalMinutes + " minutes");
		}
	}

	
	public static bool SaveIsLoading;
}
