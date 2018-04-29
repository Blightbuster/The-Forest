using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class SleepTimerDisplay : MonoBehaviour
	{
		
		private void Update()
		{
			if (LocalPlayer.Stats)
			{
				this._fillSprite.fillAmount = 1f - Mathf.Abs(Scene.Clock.ElapsedGameTime - Scene.Clock.NextSleepTime);
			}
		}

		
		public UISprite _fillSprite;
	}
}
