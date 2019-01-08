using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	public static class GeoHashHelper<T> where T : Component
	{
		public static T GetFromHash(long hash, Lookup lookupMode = Lookup.Auto)
		{
			T t = (T)((object)null);
			if ((GeoHashHelper<T>.hashes == null || !GeoHashHelper<T>.hashes.TryGetValue(hash, out t) || !t) && lookupMode == Lookup.Auto)
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
				GeoHashHelper<T>.hashes.TryGetValue(hash, out t);
			}
			return t;
		}

		public static void Register(T component)
		{
			if (GeoHashHelper<T>.hashes == null)
			{
				GeoHashHelper<T>.hashes = new Dictionary<long, T>(5);
			}
			GeoHashHelper<T>.hashes[component.transform.ToGeoHash()] = component;
		}

		public static void Unregister(T component)
		{
			if (GeoHashHelper<T>.hashes != null)
			{
				long key = component.transform.ToGeoHash();
				if (GeoHashHelper<T>.hashes.ContainsKey(key))
				{
					GeoHashHelper<T>.hashes.Remove(key);
				}
			}
		}

		private static Dictionary<long, T> hashes;
	}
}
