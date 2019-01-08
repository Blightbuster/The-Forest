using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class InCaveToggleGo : MonoBehaviour
	{
		private void Start()
		{
			this.Reset();
		}

		private void Update()
		{
			if (LocalPlayer.IsInCaves != this._inCavesGo.activeSelf)
			{
				this.Reset();
			}
		}

		private void Reset()
		{
			this._inCavesGo.SetActive(LocalPlayer.IsInCaves);
			if (this._otherGo)
			{
				this._otherGo.SetActive(!LocalPlayer.IsInCaves);
			}
		}

		public GameObject _inCavesGo;

		public GameObject _otherGo;
	}
}
