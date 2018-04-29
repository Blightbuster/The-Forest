using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	
	public static class GeoHashHelper<T> where T : Component
	{
		
		public static T GetFromHash(long hash)
		{
			T result = (T)((object)null);
			if (GeoHashHelper<T>.hashes == null || !GeoHashHelper<T>.hashes.TryGetValue(hash, out result))
			{
				T[] array = UnityEngine.Object.FindObjectsOfType<T>();
				if (GeoHashHelper<T>.hashes == null)
				{
					GeoHashHelper<T>.hashes = new Dictionary<long, T>(array.Length);
					GeoHash.helpers.Add(GeoHashHelper<T>.hashes);
				}
				foreach (T value in array)
				{
					GeoHashHelper<T>.hashes[value.transform.ToGeoHash()] = value;
				}
				GeoHashHelper<T>.hashes.TryGetValue(hash, out result);
			}
			return result;
		}

		
		private static Dictionary<long, T> hashes;
	}
}
