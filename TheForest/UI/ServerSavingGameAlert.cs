using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ServerSavingGameAlert : MonoBehaviour
	{
		
		private void Awake()
		{
			if (BoltNetwork.isClient && SteamClientDSConfig.isDedicatedClient)
			{
				this._ui.SetActive(false);
			}
			else
			{
				UnityEngine.Object.Destroy(this._ui);
				UnityEngine.Object.Destroy(this);
			}
		}

		
		private void Update()
		{
			if (BoltNetwork.isClient && SteamClientDSConfig.isDedicatedClient)
			{
				if (SteamDSConfig.IsServerSaving && !this._ui.activeSelf)
				{
					this._ui.SetActive(true);
				}
				else if (!SteamDSConfig.IsServerSaving && this._ui.activeSelf)
				{
					this._ui.SetActive(false);
				}
			}
		}

		
		public GameObject _ui;
	}
}
