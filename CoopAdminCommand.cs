using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using TheForest.Commons.Enums;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;


public static class CoopAdminCommand
{
	
	public static void Send(string command, string data)
	{
		if (CoopPeerStarter.Dedicated)
		{
			if (CoopSteamClientStarter.IsAdmin)
			{
				if (command != null)
				{
					if (CoopAdminCommand.<>f__switch$map4 == null)
					{
						CoopAdminCommand.<>f__switch$map4 = new Dictionary<string, int>(12)
						{
							{
								"kick",
								0
							},
							{
								"ban",
								0
							},
							{
								"unban",
								0
							},
							{
								"save",
								0
							},
							{
								"restart",
								0
							},
							{
								"shutdown",
								0
							},
							{
								"openlogs",
								0
							},
							{
								"closelogs",
								0
							},
							{
								"treeregrowmode",
								0
							},
							{
								"allowbuildingdestruction",
								0
							},
							{
								"allowenemiescreative",
								0
							},
							{
								"help",
								1
							}
						};
					}
					int num;
					if (CoopAdminCommand.<>f__switch$map4.TryGetValue(command, out num))
					{
						if (num == 0)
						{
							AdminCommand adminCommand = AdminCommand.Create(GlobalTargets.OnlyServer);
							adminCommand.Command = command;
							adminCommand.Data = data;
							adminCommand.Send();
							goto IL_160;
						}
						if (num == 1)
						{
							CoopAdminCommand.SendLocalMessage("Help 1/3:\r\n/kick <steamId>, /ban <steamId>, /save <slotNum>, /restart, /shutdown");
							CoopAdminCommand.SendLocalMessage("Help 2/3:\r\n/openlogs /closelogs");
							CoopAdminCommand.SendLocalMessage("Help 3/3:\r\n/treeregrowmode <on|off>, /allowbuildingdestruction <on|off>, /allowenemiescreative <on|off>");
							goto IL_160;
						}
					}
				}
				CoopAdminCommand.SendLocalMessage(string.Concat(new string[]
				{
					"Unknow command: '",
					command,
					" ",
					data,
					"'"
				}));
				IL_160:;
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
			if (CoopAdminCommand.<>f__switch$map5 == null)
			{
				CoopAdminCommand.<>f__switch$map5 = new Dictionary<string, int>(11)
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
						"ban",
						6
					},
					{
						"unban",
						7
					},
					{
						"treeregrowmode",
						8
					},
					{
						"allowbuildingdestruction",
						9
					},
					{
						"allowenemiescreative",
						10
					}
				};
			}
			int num;
			if (CoopAdminCommand.<>f__switch$map5.TryGetValue(command, out num))
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
					Scene.ActiveMB.StartCoroutine(CoopAdminCommand.ShutDownRoutine(true));
					break;
				case 2:
					CoopAdminCommand.SendNetworkMessageAll("Server is shuting down (no restart)");
					Scene.ActiveMB.StartCoroutine(CoopAdminCommand.ShutDownRoutine(false));
					break;
				case 3:
					if (!SteamDSConfig.ShowLogs)
					{
						ConsoleWriter.Open();
						Application.logMessageReceived += CoopAdminCommand.Application_logMessageReceived;
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
					Application.logMessageReceived -= CoopAdminCommand.Application_logMessageReceived;
					SteamDSConfig.ShowLogs = false;
					CoopAdminCommand.SendNetworkMessage("Closed logs console", source);
					break;
				case 5:
				{
					ulong num2;
					if (ulong.TryParse(data, out num2))
					{
						BoltEntity entityFromSteamID = MpPlayerList.GetEntityFromSteamID(num2);
						if (entityFromSteamID)
						{
							CoopKick.KickPlayer(entityFromSteamID, 1, "ADMIN_KICKED_YOU");
							CoopAdminCommand.SendNetworkMessage("Kicked " + data, source);
						}
						else
						{
							CoopAdminCommand.SendNetworkMessage("Failed to kick " + data, source);
						}
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
					ulong num2;
					if (ulong.TryParse(data, out num2))
					{
						BoltEntity entityFromSteamID2 = MpPlayerList.GetEntityFromSteamID(num2);
						if (entityFromSteamID2)
						{
							CoopKick.BanPlayer(entityFromSteamID2);
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
				case 7:
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
				case 8:
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
				case 9:
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
				case 10:
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
				}
			}
		}
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
		Scene.HudGui.Chatbox.AddLine(null, message, true);
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
			Application.LoadLevel("SteamDedicatedBootstrapScene");
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
}
