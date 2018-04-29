using System;
using UnityEngine;

namespace TheForest.Items
{
	
	public class FieldReset : PropertyAttribute
	{
		
		public FieldReset(object value)
		{
			this.Value = value;
			this.HasValue2 = false;
		}

		
		public FieldReset(object value, object value2)
		{
			this.Value = value;
			this.Value2 = value2;
			this.HasValue2 = true;
		}

		
		
		
		public object Value { get; set; }

		
		
		
		public object Value2 { get; set; }

		
		
		
		public bool HasValue2 { get; set; }
	}
}
