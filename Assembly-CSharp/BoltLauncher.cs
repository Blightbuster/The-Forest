﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BoltInternal;
using UdpKit;
using UnityEngine;

public static class BoltLauncher
{
	public static void StartSinglePlayer()
	{
		BoltLauncher.StartSinglePlayer(BoltRuntimeSettings.instance.GetConfigCopy());
	}

	public static void StartSinglePlayer(BoltConfig config)
	{
		BoltLauncher.SetUdpPlatform(new NullPlatform());
		BoltLauncher.Initialize(BoltNetworkModes.Host, UdpEndPoint.Any, config);
	}

	public static void StartServer()
	{
		BoltLauncher.StartServer(UdpEndPoint.Any);
	}

	public static void StartServer(int port)
	{
		if (port >= 0 && port <= 65535)
		{
			BoltLauncher.StartServer(new UdpEndPoint(UdpIPv4Address.Any, (ushort)port));
			return;
		}
		throw new ArgumentOutOfRangeException(string.Format("'port' must be >= 0 and <= {0}", ushort.MaxValue));
	}

	public static void StartServer(BoltConfig config)
	{
		BoltLauncher.StartServer(UdpEndPoint.Any, config);
	}

	public static void StartServer(UdpEndPoint endpoint)
	{
		BoltLauncher.StartServer(endpoint, BoltRuntimeSettings.instance.GetConfigCopy());
	}

	public static void StartServer(UdpEndPoint endpoint, BoltConfig config)
	{
		BoltLauncher.Initialize(BoltNetworkModes.Host, endpoint, config);
	}

	public static void StartClient()
	{
		BoltLauncher.StartClient(UdpEndPoint.Any);
	}

	public static void StartClient(BoltConfig config)
	{
		BoltLauncher.StartClient(UdpEndPoint.Any, config);
	}

	public static void StartClient(UdpEndPoint endpoint)
	{
		BoltLauncher.StartClient(endpoint, BoltRuntimeSettings.instance.GetConfigCopy());
	}

	public static void StartClient(UdpEndPoint endpoint, BoltConfig config)
	{
		BoltLauncher.Initialize(BoltNetworkModes.Client, endpoint, config);
	}

	public static void StartClient(int port)
	{
		if (port >= 0 && port <= 65535)
		{
			BoltLauncher.StartClient(new UdpEndPoint(UdpIPv4Address.Any, (ushort)port));
			return;
		}
		throw new ArgumentOutOfRangeException(string.Format("'port' must be >= 0 and <= {0}", ushort.MaxValue));
	}

	public static void Shutdown()
	{
		if (!BoltNetwork.isRunning)
		{
			return;
		}
		Debug.Log("Bolt shutdown");
		BoltNetworkInternal.__Shutdown();
	}

	private static void Initialize(BoltNetworkModes modes, UdpEndPoint endpoint, BoltConfig config)
	{
		BoltNetworkInternal.DebugDrawer = new UnityDebugDrawer();
		BoltNetworkInternal.UsingUnityPro = true;
		if (BoltLauncher.<>f__mg$cache0 == null)
		{
			BoltLauncher.<>f__mg$cache0 = new Func<int, string>(BoltLauncher.GetSceneName);
		}
		BoltNetworkInternal.GetSceneName = BoltLauncher.<>f__mg$cache0;
		if (BoltLauncher.<>f__mg$cache1 == null)
		{
			BoltLauncher.<>f__mg$cache1 = new Func<string, int>(BoltLauncher.GetSceneIndex);
		}
		BoltNetworkInternal.GetSceneIndex = BoltLauncher.<>f__mg$cache1;
		if (BoltLauncher.<>f__mg$cache2 == null)
		{
			BoltLauncher.<>f__mg$cache2 = new Func<List<STuple<BoltGlobalBehaviourAttribute, Type>>>(BoltLauncher.GetGlobalBehaviourTypes);
		}
		BoltNetworkInternal.GetGlobalBehaviourTypes = BoltLauncher.<>f__mg$cache2;
		if (BoltLauncher.<>f__mg$cache3 == null)
		{
			BoltLauncher.<>f__mg$cache3 = new Action(BoltNetworkInternal_User.EnvironmentSetup);
		}
		BoltNetworkInternal.EnvironmentSetup = BoltLauncher.<>f__mg$cache3;
		if (BoltLauncher.<>f__mg$cache4 == null)
		{
			BoltLauncher.<>f__mg$cache4 = new Action(BoltNetworkInternal_User.EnvironmentReset);
		}
		BoltNetworkInternal.EnvironmentReset = BoltLauncher.<>f__mg$cache4;
		BoltNetworkInternal.__Initialize(modes, endpoint, config, BoltLauncher.CreateUdpPlatform(), null);
	}

	private static int GetSceneIndex(string name)
	{
		return BoltScenes_Internal.GetSceneIndex(name);
	}

	private static string GetSceneName(int index)
	{
		return BoltScenes_Internal.GetSceneName(index);
	}

	public static List<STuple<BoltGlobalBehaviourAttribute, Type>> GetGlobalBehaviourTypes()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		List<STuple<BoltGlobalBehaviourAttribute, Type>> list = new List<STuple<BoltGlobalBehaviourAttribute, Type>>();
		try
		{
			foreach (Type type in executingAssembly.GetTypes())
			{
				if (typeof(MonoBehaviour).IsAssignableFrom(type))
				{
					BoltGlobalBehaviourAttribute[] array = (BoltGlobalBehaviourAttribute[])type.GetCustomAttributes(typeof(BoltGlobalBehaviourAttribute), false);
					if (array.Length == 1)
					{
						list.Add(new STuple<BoltGlobalBehaviourAttribute, Type>(array[0], type));
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	public static void SetUdpPlatform(UdpPlatform platform)
	{
		BoltLauncher.UserAssignedPlatform = platform;
	}

	public static UdpPlatform CreateUdpPlatform()
	{
		if (BoltLauncher.UserAssignedPlatform != null)
		{
			return BoltLauncher.UserAssignedPlatform;
		}
		return new DotNetPlatform();
	}

	private static UdpPlatform UserAssignedPlatform;

	[CompilerGenerated]
	private static Func<int, string> <>f__mg$cache0;

	[CompilerGenerated]
	private static Func<string, int> <>f__mg$cache1;

	[CompilerGenerated]
	private static Func<List<STuple<BoltGlobalBehaviourAttribute, Type>>> <>f__mg$cache2;

	[CompilerGenerated]
	private static Action <>f__mg$cache3;

	[CompilerGenerated]
	private static Action <>f__mg$cache4;
}
