using System;

namespace Serialization
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Specialist : Attribute
	{
		public Specialist(Type type)
		{
			this.Type = type;
		}

		public readonly Type Type;
	}
}
