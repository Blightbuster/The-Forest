using System;
using System.Collections.Generic;
using System.Reflection;


public static class FieldSerializer
{
	
	public static void SerializeFields(Dictionary<string, object> storage, object obj, params string[] names)
	{
		Type type = obj.GetType();
		foreach (string text in names)
		{
			FieldInfo field = type.GetField(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField);
			if (field != null)
			{
				storage[text] = field.GetValue(obj);
			}
		}
	}

	
	public static void DeserializeFields(Dictionary<string, object> storage, object obj)
	{
		Type type = obj.GetType();
		foreach (KeyValuePair<string, object> keyValuePair in storage)
		{
			FieldInfo field = type.GetField(keyValuePair.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField);
			if (field != null)
			{
				field.SetValue(obj, keyValuePair.Value);
			}
		}
	}
}
