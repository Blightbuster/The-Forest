using System;
using System.Collections.Generic;
using System.IO;

namespace AssemblyCSharp
{
	
	public class PluginManager
	{
		
		public PluginManager()
		{
			if (Directory.Exists("Plugins"))
			{
				string[] files = Directory.GetFiles("Plugins", "*.dll");
				foreach (string fileName in files)
				{
					this._plugins.Push(new Plugin(fileName));
				}
			}
		}

		
		
		public Plugin[] Plugins
		{
			get
			{
				return this._plugins.ToArray();
			}
		}

		
		private const string _dir = "Plugins";

		
		private Stack<Plugin> _plugins = new Stack<Plugin>();
	}
}
