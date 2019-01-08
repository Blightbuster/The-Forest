using System;

namespace Inspector.Decorations
{
	public class SpaceAttribute : DecorationAttribute
	{
		public SpaceAttribute(int order, int height) : base(order)
		{
			this.height = height;
		}

		public readonly int height;
	}
}
