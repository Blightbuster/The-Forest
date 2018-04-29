using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RazerSDK.Client
{
	
	public class Configuration
	{
		
		
		
		public static string Username { get; set; }

		
		
		
		public static string Password { get; set; }

		
		
		
		public static string TempFolderPath
		{
			get
			{
				return Configuration._tempFolderPath;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Configuration._tempFolderPath = value;
					return;
				}
				if (!Directory.Exists(value))
				{
					Directory.CreateDirectory(value);
				}
				if (value[value.Length - 1] == Path.DirectorySeparatorChar)
				{
					Configuration._tempFolderPath = value;
				}
				else
				{
					Configuration._tempFolderPath = value + Path.DirectorySeparatorChar;
				}
			}
		}

		
		
		
		public static string DateTimeFormat
		{
			get
			{
				return Configuration._dateTimeFormat;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Configuration._dateTimeFormat = "o";
					return;
				}
				Configuration._dateTimeFormat = value;
			}
		}

		
		public static string ToDebugReport()
		{
			string text = "C# SDK (RazerSDK) Debug Report:\n";
			string text2 = text;
			text = string.Concat(new object[]
			{
				text2,
				"    OS: ",
				Environment.OSVersion,
				"\n"
			});
			text = text + "    .NET Framework Version: " + (from x in Assembly.GetExecutingAssembly().GetReferencedAssemblies()
			where x.Name == "System.Core"
			select x).First<AssemblyName>().Version.ToString() + "\n";
			text += "    Version of the API: 0.0.1\n";
			return text + "    SDK Package Version: 1.0.0\n";
		}

		
		public const string Version = "1.0.0";

		
		private const string ISO8601_DATETIME_FORMAT = "o";

		
		public static ApiClient DefaultApiClient = new ApiClient("http:

		
		public static Dictionary<string, string> ApiKey = new Dictionary<string, string>();

		
		public static Dictionary<string, string> ApiKeyPrefix = new Dictionary<string, string>();

		
		private static string _tempFolderPath = Path.GetTempPath();

		
		private static string _dateTimeFormat = "o";
	}
}
