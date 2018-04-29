using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class NameFromProperty : PropertyAttribute
	{
		
		public NameFromProperty(string nameProperty, int extraHeight = 0)
		{
			this.NameProperty = nameProperty;
			this.ExtraHeight = extraHeight;
		}

		
		
		
		public string NameProperty { get; private set; }

		
		
		
		public int ExtraHeight { get; private set; }
	}
}
