using System;
using UnityEngine;

namespace AssetBundles
{
	public class Utility
	{
		public static string GetPlatformName()
		{
			return "AssetBundles";
		}

		private static string GetPlatformForAssetBundles(RuntimePlatform platform)
		{
			switch (platform)
			{
			case RuntimePlatform.OSXPlayer:
				return "OSX";
			case RuntimePlatform.WindowsPlayer:
				return "Windows";
			case RuntimePlatform.OSXWebPlayer:
			case RuntimePlatform.WindowsWebPlayer:
				return "WebPlayer";
			default:
				if (platform != RuntimePlatform.WebGLPlayer)
				{
					return null;
				}
				return "WebGL";
			case RuntimePlatform.IPhonePlayer:
				return "iOS";
			case RuntimePlatform.Android:
				return "Android";
			}
		}

		public const string AssetBundlesOutputPath = "AssetBundles";
	}
}
