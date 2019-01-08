using System;
using UnityEngine;

namespace Inspector
{
	[AttributeUsage(AttributeTargets.Field)]
	public abstract class InspectorAttribute : PropertyAttribute
	{
		public InspectorAttribute() : this(null)
		{
		}

		public InspectorAttribute(string label)
		{
			this.label = label;
		}

		public readonly string label;

		public string tooltip;

		public string useProperty;

		public string visibleCheck;

		public string enabledCheck;

		public int indentLevel;
	}
}
