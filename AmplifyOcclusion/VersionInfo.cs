using System;
using UnityEngine;

namespace AmplifyOcclusion
{
	
	[Serializable]
	public class VersionInfo
	{
		
		private VersionInfo()
		{
			this.m_major = 1;
			this.m_minor = 2;
			this.m_release = 3;
		}

		
		private VersionInfo(byte major, byte minor, byte release)
		{
			this.m_major = (int)major;
			this.m_minor = (int)minor;
			this.m_release = (int)release;
		}

		
		public static string StaticToString()
		{
			return string.Format("{0}.{1}.{2}", 1, 2, 3) + VersionInfo.StageSuffix;
		}

		
		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}", this.m_major, this.m_minor, this.m_release) + VersionInfo.StageSuffix;
		}

		
		
		public int Number
		{
			get
			{
				return this.m_major * 100 + this.m_minor * 10 + this.m_release;
			}
		}

		
		public static VersionInfo Current()
		{
			return new VersionInfo(1, 2, 3);
		}

		
		public static bool Matches(VersionInfo version)
		{
			return version.m_major == 1 && version.m_minor == 2 && 3 == version.m_release;
		}

		
		public const byte Major = 1;

		
		public const byte Minor = 2;

		
		public const byte Release = 3;

		
		private static string StageSuffix = "_dev001";

		
		[SerializeField]
		private int m_major;

		
		[SerializeField]
		private int m_minor;

		
		[SerializeField]
		private int m_release;
	}
}
