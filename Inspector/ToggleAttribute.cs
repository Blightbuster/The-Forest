using System;

namespace Inspector
{
	
	public class ToggleAttribute : InspectorAttribute
	{
		
		public ToggleAttribute() : base(null)
		{
		}

		
		public ToggleAttribute(string label) : base(label)
		{
		}

		
		public bool flipped;
	}
}
