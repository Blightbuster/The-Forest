using System;

namespace Inspector.Decorations
{
	public class HeaderAttribute : DecorationAttribute
	{
		public HeaderAttribute(int order, string header) : base(order)
		{
			this.header = header;
		}

		public readonly string header;
	}
}
