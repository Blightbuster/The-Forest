using System;
using UnityEngine;

namespace TheForest.Tools
{
	public class SendMessageTo : MonoBehaviour
	{
		private void Start()
		{
			this._target.SendMessage(this._message);
		}

		public GameObject _target;

		public string _message;
	}
}
