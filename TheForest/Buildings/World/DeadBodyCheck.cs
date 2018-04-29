using System;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[AddComponentMenu("Buildings/World/Dead Body Check")]
	[DoNotSerializePublic]
	public class DeadBodyCheck : MonoBehaviour
	{
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.GetComponent<mutantPickUp>())
			{
				this._receiver.SendMessage("DeadBodyEnteredArea");
			}
		}

		
		public GameObject _receiver;
	}
}
