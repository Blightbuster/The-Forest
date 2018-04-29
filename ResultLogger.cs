using System;
using System.Collections;
using System.Text;
using UnityEngine;


public class ResultLogger : UnityEngine.Object
{
	
	public static void logObject(object result)
	{
		if (result.GetType() == typeof(ArrayList))
		{
			ResultLogger.logArraylist((ArrayList)result);
		}
		else if (result.GetType() == typeof(Hashtable))
		{
			ResultLogger.logHashtable((Hashtable)result);
		}
		else
		{
			Debug.Log("result is not a hashtable or arraylist");
		}
	}

	
	public static void logArraylist(ArrayList result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		IEnumerator enumerator = result.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Hashtable item = (Hashtable)obj;
				ResultLogger.addHashtableToString(stringBuilder, item);
				stringBuilder.Append("\n--------------------\n");
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		Debug.Log(stringBuilder.ToString());
	}

	
	public static void logHashtable(Hashtable result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		ResultLogger.addHashtableToString(stringBuilder, result);
		Debug.Log(stringBuilder.ToString());
	}

	
	public static void addHashtableToString(StringBuilder builder, Hashtable item)
	{
		IDictionaryEnumerator enumerator = item.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				if (dictionaryEntry.Value is Hashtable)
				{
					builder.AppendFormat("{0}: ", dictionaryEntry.Key);
					ResultLogger.addHashtableToString(builder, (Hashtable)dictionaryEntry.Value);
				}
				else if (dictionaryEntry.Value is ArrayList)
				{
					builder.AppendFormat("{0}: ", dictionaryEntry.Key);
					ResultLogger.addArraylistToString(builder, (ArrayList)dictionaryEntry.Value);
				}
				else
				{
					builder.AppendFormat("{0}: {1}\n", dictionaryEntry.Key, dictionaryEntry.Value);
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	
	public static void addArraylistToString(StringBuilder builder, ArrayList result)
	{
		IEnumerator enumerator = result.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				if (obj is Hashtable)
				{
					ResultLogger.addHashtableToString(builder, (Hashtable)obj);
				}
				else if (obj is ArrayList)
				{
					ResultLogger.addArraylistToString(builder, (ArrayList)obj);
				}
				builder.Append("\n--------------------\n");
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		Debug.Log(builder.ToString());
	}
}
