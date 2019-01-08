using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace TheForest.Tools
{
	public class DllModLoader : MonoBehaviour
	{
		private void Awake()
		{
			if (Directory.Exists("./DllMods"))
			{
				DirectoryInfo directoryInfo = new DirectoryInfo("./DllMods");
				foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories))
				{
					this.LoadDllMod(fileInfo.FullName);
				}
			}
		}

		private void LoadDllMod(string filename)
		{
			try
			{
				Debug.Log("Loading DllMod: '" + filename + "'");
				byte[] rawAssembly = File.ReadAllBytes(filename);
				Assembly assembly = Assembly.Load(rawAssembly);
				Type[] exportedTypes = assembly.GetExportedTypes();
				foreach (Type type in exportedTypes)
				{
					MethodInfo[] methods = type.GetMethods();
					foreach (MethodInfo methodInfo in methods)
					{
						if (methodInfo.Name.StartsWith("__") && methodInfo.IsStatic)
						{
							Debug.Log(string.Concat(new string[]
							{
								"$> Executing ",
								assembly.FullName,
								".",
								type.Name,
								".",
								methodInfo.Name,
								" from ",
								filename
							}));
							methodInfo.Invoke(null, null);
						}
					}
				}
				Debug.Log("$> LoadDllMod done on file: '" + filename + "'");
			}
			catch (Exception ex)
			{
				Debug.Log(string.Concat(new object[]
				{
					"$> LoadDllMod error on file: '",
					filename,
					"':\n",
					ex
				}));
			}
		}
	}
}
