using System;

namespace Serialization
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SubTypeSerializerAttribute : Attribute
	{
		public SubTypeSerializerAttribute(Type serializesType)
		{
			this.SerializesType = serializesType;
		}

		internal readonly Type SerializesType;
	}
}
