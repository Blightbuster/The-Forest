using System;
using System.Collections;
using TheForest.Utils.AssetBundle;
using UnityEngine;

namespace TheForest.Player
{
	
	[DoNotSerializePublic]
	[RequireComponent(typeof(GreebleZone))]
	public class PassengerViewZone : MonoBehaviour
	{
		
		private void Awake()
		{
			base.GetComponent<GreebleZone>().OnSpawned = new Action<int, GameObject>(this.OnPassengerSpawned);
		}

		
		public void OnPassengerSpawned(int index, GameObject go)
		{
			if (index >= 0 && index < this._passengerIds.Length)
			{
				BundledPrefabLoader component = go.GetComponent<BundledPrefabLoader>();
				if (component)
				{
					base.StartCoroutine(this.SetupPassenger(index, component));
				}
				else
				{
					PassengerView component2 = go.GetComponent<PassengerView>();
					if (component2)
					{
						component2._id = this._passengerIds[index];
						go.GetComponent<Collider>().enabled = true;
					}
				}
			}
		}

		
		private IEnumerator SetupPassenger(int index, BundledPrefabLoader loader)
		{
			while (loader && !loader.Instance && loader.Loading)
			{
				yield return null;
			}
			if (loader && loader.Instance)
			{
				PassengerView pv = loader.Instance.GetComponent<PassengerView>();
				if (pv)
				{
					pv._id = this._passengerIds[index];
					loader.Instance.GetComponent<Collider>().enabled = true;
				}
			}
			yield break;
		}

		
		public int _zoneId;

		
		public int[] _passengerIds;
	}
}
