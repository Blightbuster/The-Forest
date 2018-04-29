using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	[DoNotSerializePublic]
	public class OnDestroyProxy : MonoBehaviour
	{
		
		private void OnDestroy()
		{
			if (this._todo)
			{
				this._todo.SendMessage(this._message, SendMessageOptions.DontRequireReceiver);
			}
		}

		
		public MonoBehaviour _todo;

		
		public string _message = "OnDestroy";
	}
}
