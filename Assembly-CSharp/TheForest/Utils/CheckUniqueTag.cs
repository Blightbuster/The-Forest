using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class CheckUniqueTag : MonoBehaviour
	{
		private void Awake()
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag(base.tag);
			if (array.Length > 1)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
