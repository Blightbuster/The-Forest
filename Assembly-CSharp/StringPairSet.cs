using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StringPairSet", menuName = "General/StringPairSet", order = 1)]
public class StringPairSet : ScriptableObject
{
	public List<StringPairSet.StringPair> Items = new List<StringPairSet.StringPair>();

	[Serializable]
	public class StringPair
	{
		public string Key;

		public string Value;
	}
}
