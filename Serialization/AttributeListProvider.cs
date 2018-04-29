using System;

namespace Serialization
{
	
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class AttributeListProvider : Attribute
	{
		
		public AttributeListProvider(Type attributeListType)
		{
			this.AttributeListType = attributeListType;
		}

		
		public readonly Type AttributeListType;
	}
}
