using System;

namespace Inspector.Decorations
{
	public class LineSeparatorAttribute : DecorationAttribute
	{
		public LineSeparatorAttribute(int order) : base(order)
		{
		}

		public float padding = 60f;
	}
}
