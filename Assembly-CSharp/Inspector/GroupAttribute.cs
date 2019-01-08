using System;

namespace Inspector
{
	public class GroupAttribute : InspectorAttribute
	{
		public GroupAttribute() : base(null)
		{
		}

		public GroupAttribute(string label) : base(label)
		{
		}

		public bool drawFoldout = true;
	}
}
