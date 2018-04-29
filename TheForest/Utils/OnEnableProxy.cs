using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	[DoNotSerializePublic]
	public class OnEnableProxy : MonoBehaviour
	{
		
		private void OnEnable()
		{
			if (!Mathf.Approximately(this._delay, 0f))
			{
				base.Invoke("SendMessageToTarget", this._delay);
			}
			else
			{
				this.SendMessageToTarget();
			}
		}

		
		private void SendMessageToTarget()
		{
			this._todo.SendMessage(this._message, SendMessageOptions.DontRequireReceiver);
		}

		
		public MonoBehaviour _todo;

		
		public string _message = "OnEnable";

		
		public float _delay;
	}
}
