using System;
using UnityEngine;

namespace TheForest.Tools
{
	public class BroadcastTo : MonoBehaviour
	{
		public void Start()
		{
			this._target.BroadcastMessage(this._message, SendMessageOptions.DontRequireReceiver);
		}

		public GameObject _target;

		public string _message;
	}
}
