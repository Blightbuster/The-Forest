﻿using System;

namespace Inspector
{
	public class SliderAttribute : InspectorAttribute
	{
		public SliderAttribute() : base(null)
		{
		}

		public SliderAttribute(string label) : base(label)
		{
		}

		public SliderAttribute(string label, float minValue, float maxValue) : base(label)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
		}

		public float minValue;

		public float maxValue;
	}
}
