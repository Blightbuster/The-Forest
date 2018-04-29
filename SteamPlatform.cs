using System;
using System.Collections.Generic;
using System.Diagnostics;
using UdpKit;


public class SteamPlatform : UdpPlatform
{
	
	public SteamPlatform()
	{
		SteamPlatform.PrecisionTimer.GetCurrentTime();
	}

	
	
	public override bool SupportsMasterServer
	{
		get
		{
			return false;
		}
	}

	
	public override UdpPlatformSocket CreateSocket()
	{
		return new SteamSocket(this);
	}

	
	public override UdpIPv4Address GetBroadcastAddress()
	{
		return UdpIPv4Address.Any;
	}

	
	public override List<UdpPlatformInterface> GetNetworkInterfaces()
	{
		return new List<UdpPlatformInterface>();
	}

	
	public override uint GetPrecisionTime()
	{
		return SteamPlatform.PrecisionTimer.GetCurrentTime();
	}

	
	public override UdpIPv4Address[] ResolveHostAddresses(string host)
	{
		return new UdpIPv4Address[0];
	}

	
	
	public override bool SupportsBroadcast
	{
		get
		{
			return false;
		}
	}

	
	private class PrecisionTimer
	{
		
		internal static uint GetCurrentTime()
		{
			long num = Stopwatch.GetTimestamp() - SteamPlatform.PrecisionTimer.start;
			double num2 = (double)num * SteamPlatform.PrecisionTimer.freq;
			return (uint)(num2 * 1000.0);
		}

		
		private static readonly long start = Stopwatch.GetTimestamp();

		
		private static readonly double freq = 1.0 / (double)Stopwatch.Frequency;
	}
}
