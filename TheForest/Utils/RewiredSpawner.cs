﻿using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class RewiredSpawner : MonoBehaviour
	{
		
		private void Awake()
		{
			if (Input.player == null && !CoopPeerStarter.DedicatedHost)
			{
				if (PlayerPreferences.UseXInput)
				{
					UnityEngine.Object.Instantiate<GameObject>(this._rewiredXInputPrefab);
				}
				else
				{
					GameObject obj = UnityEngine.Object.Instantiate<GameObject>(this._rewiredPrefab);
					if (Input.player == null)
					{
						UnityEngine.Object.Destroy(obj);
						Debug.Log("An issue occured while instanciating rewired, trying XInput version");
						PlayerPreferences.UseXInput = true;
						UnityEngine.Object.Instantiate<GameObject>(this._rewiredXInputPrefab);
					}
				}
			}
			if (!SteamManager.Initialized)
			{
				SteamManager.Reset();
			}
			if (!CoopAckChecker.Instance)
			{
				new GameObject("CoopAckChecker").AddComponent<CoopAckChecker>();
			}
		}

		
		public GameObject _rewiredXInputPrefab;

		
		public GameObject _rewiredPrefab;
	}
}
