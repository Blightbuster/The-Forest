using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class HostNotRespondingAlert : MonoBehaviour
	{
		
		
		
		public static bool Responding { get; set; }

		
		private void Awake()
		{
			HostNotRespondingAlert.Responding = true;
			if (BoltNetwork.isClient)
			{
				CoopClientCallbacks.ServerNotResponding = (Action)Delegate.Combine(CoopClientCallbacks.ServerNotResponding, new Action(this.OnHostNotResponding));
				CoopClientCallbacks.ServerIsResponding = (Action)Delegate.Combine(CoopClientCallbacks.ServerIsResponding, new Action(this.OnHostResponding));
				this._ui.SetActive(false);
			}
			else
			{
				UnityEngine.Object.Destroy(this._ui);
				UnityEngine.Object.Destroy(this);
			}
		}

		
		private void OnDestroy()
		{
			CoopClientCallbacks.ServerNotResponding = (Action)Delegate.Remove(CoopClientCallbacks.ServerNotResponding, new Action(this.OnHostNotResponding));
			CoopClientCallbacks.ServerIsResponding = (Action)Delegate.Remove(CoopClientCallbacks.ServerIsResponding, new Action(this.OnHostResponding));
		}

		
		private void OnHostNotResponding()
		{
			HostNotRespondingAlert.Responding = false;
			this._ui.SetActive(true);
		}

		
		private void OnHostResponding()
		{
			HostNotRespondingAlert.Responding = true;
			this._ui.SetActive(false);
		}

		
		public GameObject _ui;
	}
}
