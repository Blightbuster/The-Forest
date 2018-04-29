using System;

namespace Serialization
{
	
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CreatorFor : Attribute
	{
		
		public CreatorFor(Type createsType)
		{
			this.CreatesType = createsType;
		}

		
		public readonly Type CreatesType;
	}
}
