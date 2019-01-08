using System;
using Rewired;
using UnityEngine;

namespace TheForest.Utils
{
	public class RewiredSpawner : MonoBehaviour
	{
		private void Awake()
		{
			if (Input.player == null && !CoopPeerStarter.DedicatedHost)
			{
				if (ForestVR.Enabled)
				{
					this.InstanceVRManager();
				}
				else
				{
					this.InstanceManager();
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

		private void InstanceManager()
		{
			InputManager component = this._rewiredXInputPrefab.GetComponent<InputManager>();
			bool useXInput = component.userData.ConfigVars.useXInput;
			component.userData.ConfigVars.useXInput = PlayerPreferences.UseXInput;
			GameObject obj = UnityEngine.Object.Instantiate<GameObject>(this._rewiredXInputPrefab);
			component.userData.ConfigVars.useXInput = useXInput;
			if (!PlayerPreferences.UseXInput && Input.player == null)
			{
				UnityEngine.Object.Destroy(obj);
				Debug.Log("An issue occured while instanciating rewired, trying XInput version");
				PlayerPreferences.UseXInput = true;
				obj = UnityEngine.Object.Instantiate<GameObject>(this._rewiredXInputPrefab);
			}
		}

		private void InstanceVRManager()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._rewiredVRPrefab);
		}

		public GameObject _rewiredVRPrefab;

		public GameObject _rewiredXInputPrefab;

		[Obsolete("unused XInput is disabled procedurally")]
		public GameObject _rewiredPrefab;
	}
}
