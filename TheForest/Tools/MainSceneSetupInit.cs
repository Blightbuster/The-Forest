using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Tools
{
	
	public class MainSceneSetupInit : MonoBehaviour
	{
		
		private void Start()
		{
			if (this._sendInCaveMessage)
			{
				LocalPlayer.GameObject.SendMessage("InACave");
			}
			if (Application.loadedLevelName.ToLower().Contains("endgame"))
			{
				EventRegistry.Player.Publish(TfEvent.EnterEndgame, null);
			}
		}

		
		public bool _sendInCaveMessage = true;
	}
}
