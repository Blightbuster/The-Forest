using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	public static class GeoHash
	{
		public static void ClearAll()
		{
			GeoHash.helpers.ForEach(delegate(IDictionary h)
			{
				h.Clear();
			});
		}

		public static long ToGeoHash(this Transform tr)
		{
			return (long)(Mathf.RoundToInt(tr.position.x * 10f) + Mathf.RoundToInt(tr.position.y * 10f) * 1000000) + (long)Mathf.RoundToInt(tr.position.z * 10f) * 1000000000000L;
		}

		public static long ToGeoHash(Vector3 position)
		{
			return (long)(Mathf.RoundToInt(position.x * 10f) + Mathf.RoundToInt(position.y * 10f) * 1000000) + (long)Mathf.RoundToInt(position.z * 10f) * 1000000000000L;
		}

		public static List<IDictionary> helpers = new List<IDictionary>();
	}
}
