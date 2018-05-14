using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	
	public static class InstanceManager
	{
		
		public static T GetSharedInstance<T>(T source) where T : UnityEngine.Object
		{
			T t;
			if (InstanceManager.TryGetValue<T>(source, out t))
			{
				return t;
			}
			t = InstanceManager.CreateInstance<T>(source);
			InstanceManager._sharedInstances.Add(source, t);
			return t;
		}

		
		private static bool TryGetValue<T>(T source, out T result) where T : UnityEngine.Object
		{
			UnityEngine.Object @object;
			if (InstanceManager._sharedInstances.SafeCount<KeyValuePair<UnityEngine.Object, UnityEngine.Object>>() == 0 || !InstanceManager._sharedInstances.TryGetValue(source, out @object))
			{
				result = (T)((object)null);
				return false;
			}
			result = (@object as T);
			return true;
		}

		
		private static T CreateInstance<T>(T source) where T : UnityEngine.Object
		{
			return UnityEngine.Object.Instantiate<T>(source);
		}

		
		public static Dictionary<UnityEngine.Object, UnityEngine.Object> _sharedInstances = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
	}
}
