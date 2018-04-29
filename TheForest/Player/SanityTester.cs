using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	
	public class SanityTester : MonoBehaviour
	{
		
		private void OnEnable()
		{
			bool flag = LocalPlayer.SavedData.ReachedLowSanityThreshold;
			if (this._bellow && this._bellow.activeSelf != flag)
			{
				this._bellow.SetActive(flag);
			}
			if (this._above && this._above.activeSelf != !flag)
			{
				this._above.SetActive(!flag);
			}
		}

		
		public GameObject _bellow;

		
		public GameObject _above;
	}
}
