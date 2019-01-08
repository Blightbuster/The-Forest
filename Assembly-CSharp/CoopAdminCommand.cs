using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bolt;
using TheForest.Commons.Enums;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoopAdminCommand
{
	public static void Send(string command, string data)
	{
		if (CoopPeerStarter.Dedicated)
		{
			if (CoopSteamClientStarter.IsAdmin)
			{
				switch (command)
				{
				case "kick":
				case "kickbycid":
				case "ban":
				case "banbycid":
				case "unban":
				case "save":
				case "restart":
				case "shutdown":
				case "openlogs":
				case "closelogs":
				case "treeregrowmode":
				case "allowbuildingdestruction":
				case "allowenemiescreative":
				case "allowcheats":
				case "allowdebugconsole":
				{
					AdminCommand adminCommand = AdminCommand.Create(GlobalTargets.OnlyServer);
					adminCommand.Command = command;
					adminCommand.Data = data;
					adminCommand.Send();
					goto IL_193;
				}
				case "help":
					CoopAdminCommand.SendLocalMessage("Help 1/3:\r\n/kick <steamId>, /ban <steamId>, /save <slotNum>, /restart, /shutdown");
					CoopAdminCommand.SendLocalMessage("Help 2/3:\r\n/openlogs /closelogs");
					CoopAdminCommand.SendLocalMessage("Help 3/3:\r\n/treeregrowmode <on|off>, /allowbuildingdestruction <on|off>, /allowenemiescreative <on|off>");
					goto IL_193;
				}
				CoopAdminCommand.SendLocalMessage(string.Concat(new string[]
				{
					"Unknow command: '",
					command,
					" ",
					data,
					"'"
				}));
				IL_193:;
			}
			else
			{
				CoopAdminCommand.SendLocalMessage(string.Concat(new string[]
				{
					"Cannot execute command '",
					command,
					" ",
					data,
					"' (not connected as administrator)"
				}));
			}
		}
	}

	public static void Recv(string command, string data, BoltConnection source)
	{
		if (CoopPeerStarter.DedicatedHost && source.IsDedicatedServerAdmin() && command != null)
		{
			if (CoopAdminCommand.<>f__switch$map2 == null)
			{
				CoopAdminCommand.<>f__switch$map2 = new Dictionary<string, int>(15)
				{
					{
						"save",
						0
					},
					{
						"restart",
						1
					},
					{
						"shutdown",
						2
					},
					{
						"openlogs",
						3
					},
					{
						"closelogs",
						4
					},
					{
						"kick",
						5
					},
					{
						"kickbycid",
						6
					},
					{
						"ban",
						7
					},
					{
						"banbycid",
						8
					},
					{
						"unban",
						9
					},
					{
						"treeregrowmode",
						10
					},
					{
						"allowbuildingdestruction",
						11
					},
					{
						"allowenemiescreative",
						12
					},
					{
						"allowcheats",
						13
					},
					{
						"allowdebugconsole",
						13
					}
				};
			}
			int num;
			if (CoopAdminCommand.<>f__switch$map2.TryGetValue(command, out num))
			{
				switch (num)
				{
				case 0:
				{
					int slot;
					if (!string.IsNullOrEmpty(data) && int.TryParse(data, out slot))
					{
						GameSetup.SetSlot((Slots)slot);
					}
					LevelSerializer.Checkpoint();
					SaveSlotUtils.SaveHostGameGUID();
					CoopAdminCommand.SendNetworkMessage("Game saved to slot " + (int)GameSetup.Slot, source);
					break;
				}
				case 1:
					CoopAdminCommand.SendNetworkMessageAll("Server shuting down for restart, please reconnect.");
					TheForest.Utils.Scene.ActiveMB.StartCoroutine(CoopAdminCommand.ShutDownRoutine(true));
					break;
				case 2:
					CoopAdminCommand.SendNetworkMessageAll("Server is shuting down (no restart)");
					TheForest.Utils.Scene.ActiveMB.StartCoroutine(CoopAdminCommand.ShutDownRoutine(false));
					break;
				case 3:
					if (!SteamDSConfig.ShowLogs)
					{
						ConsoleWriter.Open();
						if (CoopAdminCommand.<>f__mg$cache0 == null)
						{
							CoopAdminCommand.<>f__mg$cache0 = new Application.LogCallback(CoopAdminCommand.Application_logMessageReceived);
						}
						Application.logMessageReceived += CoopAdminCommand.<>f__mg$cache0;
						SteamDSConfig.ShowLogs = true;
						CoopAdminCommand.SendNetworkMessage("Opened logs console", source);
					}
					else
					{
						CoopAdminCommand.SendNetworkMessage("Error: console already opened", source);
					}
					break;
				case 4:
					ConsoleWriter.Close();
					if (CoopAdminCommand.<>f__mg$cache1 == null)
					{
						CoopAdminCommand.<>f__mg$cache1 = new Application.LogCallback(CoopAdminCommand.Application_logMessageReceived);
					}
					Application.logMessageReceived -= CoopAdminCommand.<>f__mg$cache1;
					SteamDSConfig.ShowLogs = false;
					CoopAdminCommand.SendNetworkMessage("Closed logs console", source);
					break;
				case 5:
				{
					ulong num2;
					if (ulong.TryParse(data, out num2))
					{
						CoopAdminCommand.KickPlayer(num2, source, "ADMIN_KICKED_YOU");
					}
					else
					{
						BoltEntity entityFromName = MpPlayerList.GetEntityFromName(data, source);
						if (entityFromName)
						{
							CoopKick.KickPlayer(entityFromName, 1, "ADMIN_KICKED_YOU");
							CoopAdminCommand.SendNetworkMessage("Kicked " + data, source);
						}
						else
						{
							CoopAdminCommand.SendNetworkMessage("Invalid SteamId or Name, kick failed (data=" + data + ")", source);
						}
					}
					break;
				}
				case 6:
				{
					int connectionId;
					if (int.TryParse(data, out connectionId))
					{
						CoopAdminCommand.KickPlayerByConnectionId(connectionId, source, "ADMIN_KICKED_YOU");
					}
					break;
				}
				case 7:
				{
					ulong num2;
					if (ulong.TryParse(data, out num2))
					{
						BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(num2);
						if (entityFromSteamID)
						{
							CoopKick.BanPlayer(entityFromSteamID);
							CoopAdminCommand.SendNetworkMessage("Banned " + data, source);
						}
						else
						{
							CoopAdminCommand.SendNetworkMessage("Failed to ban " + data, source);
						}
					}
					else
					{
						BoltEntity entityFromName2 = MpPlayerList.GetEntityFromName(data, source);
						if (entityFromName2)
						{
							CoopKick.BanPlayer(entityFromName2);
							CoopAdminCommand.SendNetworkMessage("Banned " + data, source);
						}
						else
						{
							CoopAdminCommand.SendNetworkMessage("Invalid SteamId or Name, ban failed (data=" + data + ")", source);
						}
					}
					break;
				}
				case 8:
				{
					int connectionId;
					if (int.TryParse(data, out connectionId))
					{
						CoopAdminCommand.BanPlayerByConnectionId(connectionId, source, "HOST_BANNED_YOU_PERMANANTLY");
					}
					break;
				}
				case 9:
				{
					ulong num2;
					if (ulong.TryParse(data, out num2))
					{
						CoopKick.UnBanPlayer(num2);
						CoopAdminCommand.SendNetworkMessage("Unbanned " + data, source);
					}
					else if (CoopKick.UnBanPlayer(data))
					{
						CoopAdminCommand.SendNetworkMessage("Unbanned " + data, source);
					}
					else
					{
						CoopAdminCommand.SendNetworkMessage("Invalid SteamId or Name, unban failed (data=" + data + ")", source);
					}
					break;
				}
				case 10:
					if (data == "on")
					{
						SteamDSConfig.TreeRegrowMode = true;
						PlayerPreferences.SetLocalTreeRegrowth(SteamDSConfig.TreeRegrowMode);
						CoopAdminCommand.SendNetworkMessage("TreeRegrowMode set to on", source);
					}
					else if (data == "off")
					{
						SteamDSConfig.TreeRegrowMode = false;
						PlayerPreferences.SetLocalTreeRegrowth(SteamDSConfig.TreeRegrowMode);
						CoopAdminCommand.SendNetworkMessage("TreeRegrowMode set to off", source);
					}
					else
					{
						CoopAdminCommand.SendNetworkMessage("Invalid parameter (data=" + data + ")", source);
					}
					break;
				case 11:
					if (data == "on")
					{
						SteamDSConfig.AllowBuildingDestruction = true;
						PlayerPreferences.SetLocalNoDestructionMode(!SteamDSConfig.AllowBuildingDestruction);
						CoopAdminCommand.SendNetworkMessage("allowbuildingdestruction set to on", source);
					}
					else if (data == "off")
					{
						SteamDSConfig.AllowBuildingDestruction = false;
						PlayerPreferences.SetLocalNoDestructionMode(!SteamDSConfig.AllowBuildingDestruction);
						CoopAdminCommand.SendNetworkMessage("allowbuildingdestruction set to off", source);
					}
					else
					{
						CoopAdminCommand.SendNetworkMessage("Invalid parameter (data=" + data + ")", source);
					}
					break;
				case 12:
					if (data == "on")
					{
						SteamDSConfig.AllowEnemiesCreative = true;
						PlayerPreferences.SetLocalAllowEnemiesCreativeMode(SteamDSConfig.AllowEnemiesCreative);
						CoopAdminCommand.SendNetworkMessage("AllowEnemiesCreative set to on", source);
					}
					else if (data == "off")
					{
						SteamDSConfig.AllowEnemiesCreative = false;
						PlayerPreferences.SetLocalAllowEnemiesCreativeMode(SteamDSConfig.AllowEnemiesCreative);
						CoopAdminCommand.SendNetworkMessage("AllowEnemiesCreative set to off", source);
					}
					else
					{
						CoopAdminCommand.SendNetworkMessage("Invalid parameter (data=" + data + ")", source);
					}
					break;
				case 13:
					if (data == "on")
					{
						SteamDSConfig.AllowCheats = true;
						PlayerPreferences.SetAllowCheatsMode(SteamDSConfig.AllowCheats);
						CoopAdminCommand.SendNetworkMessage("allowcheats set to on", source);
					}
					else if (data == "off")
					{
						SteamDSConfig.AllowCheats = false;
						PlayerPreferences.SetAllowCheatsMode(SteamDSConfig.AllowCheats);
						CoopAdminCommand.SendNetworkMessage("allowcheats set to off", source);
					}
					else
					{
						CoopAdminCommand.SendNetworkMessage("Invalid parameter (data=" + data + ")", source);
					}
					break;
				}
			}
		}
	}

	private static void KickPlayerByConnectionId(int connectionId, BoltConnection source, string message)
	{
		CoopKick.KickPlayerByConnectionId(connectionId, 1, message);
		CoopAdminCommand.SendNetworkMessage("Kicked " + connectionId, source);
	}

	private static void BanPlayerByConnectionId(int connectionId, BoltConnection source, string message)
	{
		CoopKick.KickPlayerByConnectionId(connectionId, 0, message);
		CoopAdminCommand.SendNetworkMessage("Banned " + connectionId, source);
	}

	private static void KickPlayer(ulong steamId, BoltConnection source, string message)
	{
		CoopKick.KickPlayer(steamId, 1, message);
		CoopAdminCommand.SendNetworkMessage("Kicked " + steamId, source);
	}

	private static void SendNetworkMessageAll(string message)
	{
		ChatEvent chatEvent = ChatEvent.Create(GlobalTargets.AllClients);
		chatEvent.Message = message;
		chatEvent.Sender = CoopServerInfo.Instance.entity.networkId;
		chatEvent.Send();
	}

	private static void SendNetworkMessage(string message, BoltConnection source)
	{
		ChatEvent chatEvent = ChatEvent.Create(source);
		chatEvent.Message = message;
		chatEvent.Send();
	}

	private static void SendLocalMessage(string message)
	{
		TheForest.Utils.Scene.HudGui.Chatbox.AddLine(null, message, true);
	}

	private static IEnumerator ShutDownRoutine(bool restart)
	{
		BoltLauncher.Shutdown();
		yield return YieldPresets.WaitPointFiveSeconds;
		CoopSteamServer.Shutdown();
		CoopSteamClient.Shutdown();
		CoopTreeGrid.Clear();
		if (restart)
		{
			yield return YieldPresets.WaitPointFiveSeconds;
			SceneManager.LoadScene("SteamDedicatedBootstrapScene", LoadSceneMode.Single);
		}
		else
		{
			Application.Quit();
		}
		yield break;
	}

	public static void Application_logMessageReceived(string condition, string stackTrace, LogType type)
	{
		Console.WriteLine(string.Concat(new object[]
		{
			type,
			": ",
			condition,
			"\r\n",
			stackTrace
		}));
	}

	[CompilerGenerated]
	private static Application.LogCallback <>f__mg$cache0;

	[CompilerGenerated]
	private static Application.LogCallback <>f__mg$cache1;
}
