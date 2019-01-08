using System;
using System.IO;
using System.Reflection;

namespace AssemblyCSharp
{
	public class Plugin
	{
		public Plugin(string fileName)
		{
			try
			{
				this.PluginName = Path.GetFileName(fileName);
				Assembly assembly = Assembly.LoadFrom(fileName);
			}
			catch (Exception exception)
			{
				this.Exception = exception;
			}
		}

		public Exception Exception { get; private set; }

		public string PluginName { get; private set; }

		public bool Loaded
		{
			get
			{
				return this.Exception == null;
			}
		}
	}
}
