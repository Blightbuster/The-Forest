using System;

namespace Serialization
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SerializationPriorityAttribute : Attribute
	{
		public SerializationPriorityAttribute(int priority)
		{
			this.Priority = priority;
		}

		public readonly int Priority;
	}
}
