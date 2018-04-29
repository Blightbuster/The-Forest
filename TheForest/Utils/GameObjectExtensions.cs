using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public static class GameObjectExtensions
	{
		
		public static void SetActiveSafe(this GameObject[] objects, bool activeValue)
		{
			if (objects == null)
			{
				return;
			}
			foreach (GameObject gameObject in objects)
			{
				if (!(gameObject == null))
				{
					gameObject.SetActive(activeValue);
				}
			}
		}

		
		public static string SafeName(this GameObject myObject, string defaultResult = null)
		{
			if (myObject == null)
			{
				return defaultResult;
			}
			return myObject.name;
		}

		
		public static void SetActiveSelfSafe(this Component target, bool activeValue)
		{
			if (target == null || target.gameObject == null)
			{
				return;
			}
			if (target.gameObject.activeSelf == activeValue)
			{
				return;
			}
			target.gameObject.SetActive(activeValue);
		}

		
		public static bool IsNull(this UnityEngine.Object target)
		{
			return target == null;
		}

		
		public static GameObject GetRootGameObject(this GameObject target)
		{
			if (target == null || target.transform.root == null)
			{
				return null;
			}
			return target.transform.root.gameObject;
		}

		
		public static GameObject GetRootGameObject(this Component target)
		{
			if (target == null || target.transform.root == null)
			{
				return null;
			}
			return target.transform.root.gameObject;
		}

		
		public static GameObject GetRootGameObject(this Transform target)
		{
			if (target == null || target.transform.root == null)
			{
				return null;
			}
			return target.transform.root.gameObject;
		}
	}
}
