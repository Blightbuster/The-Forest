using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TheForest.Commons.Enums;
using UnityEngine;

namespace TheForest.Utils
{
	public static class SaveSlotUtils
	{
		public static string UserId { get; private set; }

		public static void SetUserId(string userId)
		{
			SaveSlotUtils.UserId = userId;
		}

		public static string GetUserPath()
		{
			if (SaveSlotUtils.UserId == null)
			{
				try
				{
					SaveSlotUtils.UserId = ((!SteamDSConfig.isDedicated) ? SteamUser.GetSteamID().ToString() : "ds");
				}
				catch (Exception ex)
				{
				}
				finally
				{
					if (string.IsNullOrEmpty(SaveSlotUtils.UserId))
					{
						SaveSlotUtils.UserId = "0";
					}
				}
			}
			return Application.persistentDataPath + "/" + SaveSlotUtils.UserId + "/";
		}

		public static string GetLocalSlotPath()
		{
			return SaveSlotUtils.GetLocalSlotPath(GameSetup.Slot);
		}

		public static string GetLocalSlotPath(Slots slot)
		{
			return SaveSlotUtils.GetLocalSlotPath(GameSetup.Mode, slot);
		}

		public static string GetLocalSlotPath(PlayerModes mode, Slots slot)
		{
			return string.Concat(new object[]
			{
				SaveSlotUtils.GetUserPath(),
				mode,
				"/",
				slot,
				"/"
			});
		}

		public static string GetMpClientLocalPath()
		{
			return SaveSlotUtils.GetUserPath() + PlayerModes.Multiplayer + "/cs/";
		}

		public static string GetCloudSlotPath()
		{
			return SaveSlotUtils.GetCloudSlotPath(GameSetup.Slot);
		}

		public static string GetCloudSlotPath(Slots slot)
		{
			return SaveSlotUtils.GetCloudSlotPath(GameSetup.Mode, slot);
		}

		public static string GetCloudSlotPath(PlayerModes mode, Slots slot)
		{
			return string.Concat(new object[]
			{
				mode,
				"_",
				slot,
				"_"
			});
		}

		public static bool HasLocalFile(Slots slot, string filename)
		{
			string localSlotPath = SaveSlotUtils.GetLocalSlotPath(slot);
			return File.Exists(localSlotPath + filename);
		}

		private static void DeleteDirectory(string target_dir)
		{
			string[] files = Directory.GetFiles(target_dir);
			string[] directories = Directory.GetDirectories(target_dir);
			foreach (string path in files)
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
			foreach (string target_dir2 in directories)
			{
				SaveSlotUtils.DeleteDirectory(target_dir2);
			}
			Directory.Delete(target_dir, false);
		}

		public static void DeleteSlot(PlayerModes mode, Slots slot)
		{
			string localSlotPath = SaveSlotUtils.GetLocalSlotPath(mode, slot);
			if (Directory.Exists(localSlotPath))
			{
				SaveSlotUtils.DeleteDirectory(localSlotPath);
			}
			string[] array = CoopSteamCloud.ListFiles();
			string cloudSlotPath = SaveSlotUtils.GetCloudSlotPath(mode, slot);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].StartsWith(cloudSlotPath))
				{
					CoopSteamCloud.CloudDelete(array[i]);
				}
			}
		}

		public static void CreateThumbnail()
		{
			string localSlotPath = SaveSlotUtils.GetLocalSlotPath();
			if (!Directory.Exists(localSlotPath))
			{
				Directory.CreateDirectory(localSlotPath);
			}
			string path = localSlotPath + "thumb.png";
			RenderTexture targetTexture = LocalPlayer.MainCam.targetTexture;
			RenderTexture active = RenderTexture.active;
			int num = 250;
			int num2 = 175;
			RenderTexture renderTexture = new RenderTexture(num, num2, 24);
			LocalPlayer.MainCam.targetTexture = renderTexture;
			Texture2D texture2D = new Texture2D(num, num2, TextureFormat.RGB24, false);
			Vector3 b = LocalPlayer.MainCamTr.forward * 3f / 4f;
			LocalPlayer.MainCamTr.position += b;
			LocalPlayer.MainCam.Render();
			LocalPlayer.MainCamTr.position -= b;
			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)num, (float)num2), 0, 0);
			LocalPlayer.MainCam.targetTexture = targetTexture;
			RenderTexture.active = active;
			UnityEngine.Object.Destroy(renderTexture);
			byte[] array = texture2D.EncodeToPNG();
			File.WriteAllBytes(path, array);
			CoopSteamCloud.CloudSave(SaveSlotUtils.GetCloudSlotPath() + "thumb.png", array);
		}

		public static void SaveHostGameGUID()
		{
			if (CoopLobby.Instance != null && CoopLobby.Instance.Info != null)
			{
				PlayerPrefsFile.SetString("guid", CoopLobby.Instance.Info.Guid, true);
			}
			else
			{
				bool flag = !string.IsNullOrEmpty(CoopLobby.HostGuid);
				Debug.LogError("Failed to store host GUID (lobby no longer exists) fallback=" + flag);
				if (flag)
				{
					PlayerPrefsFile.SetString("guid", CoopLobby.HostGuid, true);
				}
			}
		}

		public static void LoadHostGameGUID()
		{
			string text = PlayerPrefsFile.GetString("guid", null, true);
			if (string.IsNullOrEmpty(text))
			{
				text = Guid.NewGuid().ToString();
			}
			CoopLobby.Instance.SetGuid(text);
		}

		public static void SaveGameDifficulty()
		{
			PlayerPrefsFile.SetString("difficulty", (!GameSetup.IsCreativeGame) ? GameSetup.Difficulty.ToString() : "Creative", true);
		}

		public static HashSet<string> GetPreviouslyPlayedServers()
		{
			HashSet<string> hashSet = new HashSet<string>();
			string mpClientLocalPath = SaveSlotUtils.GetMpClientLocalPath();
			if (!Directory.Exists(mpClientLocalPath))
			{
				Directory.CreateDirectory(mpClientLocalPath);
			}
			foreach (string path in Directory.GetFiles(mpClientLocalPath))
			{
				hashSet.Add(Path.GetFileName(path));
			}
			return hashSet;
		}
	}
}
