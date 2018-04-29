using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	[DoNotSerializePublic]
	public class OnDestroyProxyS : MonoBehaviour
	{
		
		private void OnDestroy()
		{
			if (this._todo)
			{
				this._todo.SendMessage(this._message, SendMessageOptions.DontRequireReceiver);
			}
		}

		
		private void OnDeserialized()
		{
			if (!this._todo)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		[SerializeThis]
		public GameObject _todo;

		
		public string _message = "OnDestroy";
	}
}
