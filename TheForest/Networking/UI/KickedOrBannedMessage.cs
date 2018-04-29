using System;
using UnityEngine;

namespace TheForest.Networking.UI
{
	
	public class KickedOrBannedMessage : MonoBehaviour
	{
		
		private void Awake()
		{
			if (!string.IsNullOrEmpty(SteamClientConfig.KickMessage))
			{
				Debug.Log("MP Kick Message: " + SteamClientConfig.KickMessage);
				this._label.text = SteamClientConfig.KickMessage;
				base.Invoke("Open", 0.05f);
			}
		}

		
		public void Open()
		{
			this._root.SetActive(true);
		}

		
		public void Close()
		{
			this._root.SetActive(false);
			SteamClientConfig.KickMessage = string.Empty;
		}

		
		public GameObject _root;

		
		public UILabel _label;
	}
}
