using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Tools
{
	
	public class PlayerInEndgameTester : MonoBehaviour
	{
		
		public void DoPositionningTest()
		{
			if (LocalPlayer.IsInEndgame)
			{
				this._callback.Invoke();
			}
		}

		
		public UnityEvent _callback;
	}
}
