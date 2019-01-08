using System;
using Steamworks;

public static class SteamConfig
{
	public static AppId_t AppId
	{
		get
		{
			return new AppId_t(242760u);
		}
	}

	public static int BuildId;

	public static string BetaName;
}
