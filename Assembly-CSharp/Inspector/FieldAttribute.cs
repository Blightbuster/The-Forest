using System;

namespace Inspector
{
	public class FieldAttribute : InspectorAttribute
	{
		public FieldAttribute() : base(null)
		{
		}

		public FieldAttribute(string label) : base(label)
		{
		}

		public bool allowSceneObjects = true;
	}
}
