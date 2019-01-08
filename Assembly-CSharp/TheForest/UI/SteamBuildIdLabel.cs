using System;
using System.Collections;
using UnityEngine;

namespace TheForest.UI
{
	public class SteamBuildIdLabel : MonoBehaviour
	{
		private IEnumerator Start()
		{
			if (!CoopPeerStarter.DedicatedHost)
			{
				while (!SteamManager.Initialized || SteamManager.BuildId == 0)
				{
					yield return null;
				}
			}
			this._label.text = SteamManager.BuildId.ToString();
			yield break;
		}

		public UILabel _label;
	}
}
