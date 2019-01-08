using System;
using System.Collections.Generic;

namespace Pathfinding.Serialization
{
	public class GraphMeta
	{
		public Type GetGraphType(int i)
		{
			if (string.IsNullOrEmpty(this.typeNames[i]))
			{
				return null;
			}
			Type[] defaultGraphTypes = AstarData.DefaultGraphTypes;
			Type type = null;
			for (int j = 0; j < defaultGraphTypes.Length; j++)
			{
				if (defaultGraphTypes[j].FullName == this.typeNames[i])
				{
					type = defaultGraphTypes[j];
				}
			}
			if (!object.Equals(type, null))
			{
				return type;
			}
			throw new Exception("No graph of type '" + this.typeNames[i] + "' could be created, type does not exist");
		}

		public Version version;

		public int graphs;

		public List<string> guids;

		public List<string> typeNames;
	}
}
