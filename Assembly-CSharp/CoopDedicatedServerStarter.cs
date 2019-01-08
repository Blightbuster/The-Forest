using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Steamworks;
using TheForest.Utils;
using UdpKit;
using UnityEngine;

public class CoopDedicatedServerStarter : MonoBehaviour
{
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this.loadAsync.gameObject);
		SteamDSConfig.initialized = false;
		Debug.Log("DS configurations tests: Start tests");
		if (this.CheckNetworkConfiguration())
		{
			Debug.Log("DS configurations tests: Host pass tests");
		}
		else
		{
			Debug.Log("DS configurations tests: Host fail network configurations tests. Please check error log and your configuration.");
		}
	}

	private void Start()
	{
		if (!SteamDSConfig.isDedicatedServer && !SteamManager.Initialized)
		{
			SteamManager.Reset();
		}
		SteamDSConfig.MapName = GameSetup.Difficulty.ToString();
		SteamDSConfig.manager.SetStart(this.loadAsync);
		bool flag = GameServer.Init(0u, SteamDSConfig.ServerSteamPort, SteamDSConfig.ServerGamePort, SteamDSConfig.ServerQueryPort, SteamDSConfig.ServerAuthMode, SteamDSConfig.ServerVersion);
		if (flag)
		{
			Debug.Log("GameServer init success. Port: " + SteamDSConfig.ServerGamePort);
			if (CoopDedicatedServerStarter.<>f__mg$cache0 == null)
			{
				CoopDedicatedServerStarter.<>f__mg$cache0 = new SteamAPIWarningMessageHook_t(CoopDedicatedServerStarter.SteamAPIDebugTextHook);
			}
			SteamGameServerUtils.SetWarningMessageHook(CoopDedicatedServerStarter.<>f__mg$cache0);
			SteamGameServer.SetModDir("theforestDS");
			SteamGameServer.SetProduct(SteamDSConfig.ProductName);
			SteamGameServer.SetGameDescription(SteamDSConfig.ProductDescription);
			SteamGameServer.SetServerName(SteamDSConfig.ServerName);
			SteamGameServer.SetDedicatedServer(true);
			if (string.IsNullOrEmpty(SteamDSConfig.ServerSteamAccount))
			{
				Debug.Log("Set a LogOnAnonymous");
				SteamGameServer.LogOnAnonymous();
			}
			else
			{
				Debug.Log("Set a Logon");
				SteamGameServer.LogOn(SteamDSConfig.ServerSteamAccount);
			}
			SteamGameServer.EnableHeartbeats(true);
			SteamDSConfig.initialized = true;
		}
		else
		{
			Debug.LogError("GameServer.InitSafe failed");
			CoopDedicatedServerStarter.ShutDown();
		}
	}

	private static void ShutDown()
	{
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

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		if (nSeverity == 0)
		{
			Debug.Log(pchDebugText);
		}
		else
		{
			Debug.LogWarning(pchDebugText);
		}
	}

	private bool isMachineNetAvailable
	{
		get
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}
	}

	private string getMachineIp()
	{
		if (NetworkInterface.GetIsNetworkAvailable())
		{
			foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (networkInterface.OperationalStatus == OperationalStatus.Up)
				{
					IPInterfaceProperties ipproperties = networkInterface.GetIPProperties();
					if (ipproperties.GatewayAddresses.FirstOrDefault<GatewayIPAddressInformation>() != null)
					{
						foreach (UnicastIPAddressInformation unicastIPAddressInformation in ipproperties.UnicastAddresses)
						{
							if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
							{
								return unicastIPAddressInformation.Address.ToString();
							}
						}
					}
				}
			}
		}
		return string.Empty;
	}

	private bool CheckPorts(IPAddress ip, int[] ports)
	{
		bool result = true;
		foreach (int port in ports)
		{
			try
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				socket.Connect(ip, port);
				if (socket.Connected)
				{
					byte[] bytes = Encoding.ASCII.GetBytes("test string");
					IPEndPoint remote_end = new IPEndPoint(ip, port);
					try
					{
						socket.SendTo(bytes, remote_end);
					}
					catch (Exception ex)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"UDP Port ",
							port.ToString(),
							" seens to be blocked (",
							ex.Message,
							")"
						}));
					}
				}
				socket.Close();
			}
			catch (SocketException ex2)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"UDP Port ",
					port.ToString(),
					" seens to be blocked (",
					ex2.Message,
					":",
					ex2.ErrorCode,
					")"
				}));
				result = false;
			}
		}
		return result;
	}

	private bool CheckNetworkConfiguration()
	{
		bool result = true;
		if (!this.isMachineNetAvailable)
		{
			Debug.LogError("Your host network seens to be disconnected");
			result = false;
		}
		UdpIPv4Address udpIPv4Address = UdpIPv4Address.Any;
		bool flag = true;
		try
		{
			udpIPv4Address = UdpIPv4Address.Parse(SteamDSConfig.ServerAddress);
		}
		catch
		{
			Debug.LogError("You configure a invalid ip address: " + SteamDSConfig.ServerAddress);
			flag = false;
			result = false;
		}
		if (flag)
		{
			string machineIp = this.getMachineIp();
			bool flag2 = true;
			if (machineIp != string.Empty)
			{
				if (machineIp.ToLower().Trim() != SteamDSConfig.ServerAddress.ToLower().Trim())
				{
					Debug.LogError("You configure ip address as " + SteamDSConfig.ServerAddress + " but your host IP seens to be " + machineIp);
					result = false;
				}
			}
			else
			{
				Debug.LogError("Unable to check you host IP address");
				result = false;
				flag2 = false;
			}
			if (flag2 && !this.CheckPorts(IPAddress.Parse(machineIp), new int[]
			{
				(int)SteamDSConfig.ServerGamePort,
				(int)SteamDSConfig.ServerQueryPort,
				(int)SteamDSConfig.ServerSteamPort
			}))
			{
				result = false;
			}
		}
		return result;
	}

	public LoadAsync loadAsync;

	private static CoopSteamServerStarter serverStarter;

	[CompilerGenerated]
	private static SteamAPIWarningMessageHook_t <>f__mg$cache0;
}
