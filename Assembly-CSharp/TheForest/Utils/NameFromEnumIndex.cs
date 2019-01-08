using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class NameFromEnumIndex : PropertyAttribute
	{
		public NameFromEnumIndex(Type enumType)
		{
			this.EnumType = enumType;
		}

		public Type EnumType { get; private set; }
	}
}
