using System;
using Steamworks;
using UdpKit;


public static class CoopSteamUtils
{
	
	public static UdpEndPoint ToEndPoint(this CSteamID id)
	{
		return new UdpEndPoint(new UdpSteamID(id.m_SteamID));
	}

	
	public static UdpEndPoint ToEndPoint(this ulong steamId)
	{
		return new UdpEndPoint(new UdpSteamID(steamId));
	}
}
