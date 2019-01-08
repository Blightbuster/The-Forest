using System;
using UnityEngine;

namespace TheForest.Utils
{
	[DoNotSerializePublic]
	public class OnDisableProxy : MonoBehaviour
	{
		private void OnDisable()
		{
			this._todo.SendMessage(this._message, SendMessageOptions.DontRequireReceiver);
		}

		public MonoBehaviour _todo;

		public string _message = "OnDisable";
	}
}
