using System;
using System.Collections.Generic;
using UnityEngine;

public class weightsRandomizer<T>
{
	public weightsRandomizer(Dictionary<T, int> weights)
	{
		this._weights = weights;
	}

	public T TakeOne()
	{
		List<KeyValuePair<T, int>> list = this.Sort(this._weights);
		int num = 0;
		foreach (KeyValuePair<T, int> keyValuePair in this._weights)
		{
			num += keyValuePair.Value;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		T key = list[list.Count - 1].Key;
		foreach (KeyValuePair<T, int> keyValuePair2 in list)
		{
			if (num2 < keyValuePair2.Value)
			{
				key = keyValuePair2.Key;
				break;
			}
			num2 -= keyValuePair2.Value;
		}
		return key;
	}

	private List<KeyValuePair<T, int>> Sort(Dictionary<T, int> weights)
	{
		List<KeyValuePair<T, int>> list = new List<KeyValuePair<T, int>>(weights);
		list.Sort((KeyValuePair<T, int> firstPair, KeyValuePair<T, int> nextPair) => firstPair.Value.CompareTo(nextPair.Value));
		return list;
	}

	private static UnityEngine.Random _random = new UnityEngine.Random();

	private Dictionary<T, int> _weights;
}
