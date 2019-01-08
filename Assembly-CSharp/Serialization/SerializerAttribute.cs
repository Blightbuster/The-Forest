using System;

namespace Serialization
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SerializerAttribute : Attribute
	{
		public SerializerAttribute(Type serializesType)
		{
			this.SerializesType = serializesType;
		}

		internal readonly Type SerializesType;
	}
}
