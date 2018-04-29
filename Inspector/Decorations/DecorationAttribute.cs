using System;

namespace Inspector.Decorations
{
	
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public abstract class DecorationAttribute : Attribute
	{
		
		public DecorationAttribute(int order)
		{
			this.order = order;
		}

		
		public readonly int order;

		
		public string visibleCheck;
	}
}
