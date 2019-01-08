﻿using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class DestroyGOListOnDestroy : MonoBehaviour
	{
		private void OnDestroy()
		{
			if (this._golist != null)
			{
				for (int i = 0; i < this._golist.Length; i++)
				{
					GameObject gameObject = this._golist[i];
					if (gameObject)
					{
						UnityEngine.Object.Destroy(gameObject);
					}
				}
			}
		}

		public GameObject[] _golist;
	}
}
