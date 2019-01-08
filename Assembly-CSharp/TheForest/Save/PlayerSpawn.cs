using System;
using System.Collections;
using System.IO;
using Serialization;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.Save
{
	public class PlayerSpawn : MonoBehaviour
	{
		private void Awake()
		{
			if (this._playerEditor)
			{
				UnityEngine.Object.DestroyImmediate(this._playerEditor);
			}
			if (!CoopPeerStarter.DedicatedHost)
			{
				if ((!BoltNetwork.isRunning || !BoltNetwork.isClient || !PlayerSpawn.LoadSavedCharacter || !this.LoadMpCharacter()) && !LevelSerializer.IsDeserializing)
				{
					this._player = UnityEngine.Object.Instantiate<GameObject>(this._playerPrefab, base.transform.position, base.transform.rotation);
					this._player.name = "player";
					this._player.GetComponent<playerAiInfo>().enabled = false;
					base.Invoke("InitPlayer", 0.1f);
					if (this._planeCrash)
					{
						base.StartCoroutine(Scene.PlaneCrash.InitPlaneCrashSequence());
					}
				}
			}
			else
			{
				GameObject gameObject = new GameObject("DummyLocalPlayer");
				LocalPlayer localPlayer = gameObject.AddComponent<LocalPlayer>();
				LocalPlayer.Transform = gameObject.transform;
				LocalPlayer.GameObject = gameObject;
			}
			PlayerSpawn.LoadSavedCharacter = false;
		}

		public static string GetClientSaveFileName()
		{
			if (CoopPeerStarter.Dedicated)
			{
				return (!string.IsNullOrEmpty(SteamClientDSConfig.Guid)) ? SteamClientDSConfig.Guid : "0";
			}
			return (CoopLobby.Instance == null) ? string.Empty : CoopLobby.Instance.Info.Guid;
		}

		public static void SaveMpCharacter(GameObject playerGO)
		{
			byte[] data = playerGO.SaveObjectTree();
			data.WriteToFile(SaveSlotUtils.GetMpClientLocalPath() + PlayerSpawn.GetClientSaveFileName());
		}

		public static void DeleteMpCharacter()
		{
			bool flag = PlayerSpawn.HasMPCharacterSave();
			if (flag)
			{
				File.Delete(SaveSlotUtils.GetMpClientLocalPath() + PlayerSpawn.GetClientSaveFileName());
				Debug.Log("Deleted MP client local save");
			}
		}

		public static bool HasMPCharacterSave()
		{
			string mpClientLocalPath = SaveSlotUtils.GetMpClientLocalPath();
			string clientSaveFileName = PlayerSpawn.GetClientSaveFileName();
			return File.Exists(mpClientLocalPath + clientSaveFileName);
		}

		private bool LoadMpCharacter()
		{
			bool flag = PlayerSpawn.HasMPCharacterSave();
			if (flag)
			{
				Scene.ActiveMB.StartCoroutine(this.LoadMpCharacterDelayed());
				Scene.PlaneCrash.SetupCrashedPlane_MP();
				return true;
			}
			return false;
		}

		private void InitPlayer()
		{
			this._player.GetComponent<playerAiInfo>().enabled = true;
		}

		private IEnumerator LoadMpCharacterDelayed()
		{
			yield return null;
			if (!SaveGameManager.Instance.requiredObjects.Contains(this._playerPrefab))
			{
				SaveGameManager.Instance.requiredObjects = new UnityEngine.Object[]
				{
					this._playerPrefab
				};
				LevelSerializer.InitPrefabList();
			}
			PlayerSpawn.LoadingSavedCharacter = true;
			File.ReadAllBytes(SaveSlotUtils.GetMpClientLocalPath() + PlayerSpawn.GetClientSaveFileName()).LoadObjectTree(null);
			PlayerSpawn.LoadingSavedCharacter = false;
			Debug.Log("Client player loaded from local file");
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.Rigidbody.isKinematic = true;
			yield break;
		}

		public GameObject _playerEditor;

		public GameObject _playerPrefab;

		public bool _planeCrash = true;

		private GameObject _player;

		public static string MpCharacterSaveFileName = "MPCharacterSave";

		public static bool LoadSavedCharacter;

		public static bool LoadingSavedCharacter;
	}
}
