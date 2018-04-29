using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.UI
{
	
	public class PedometerUI : MonoBehaviour
	{
		
		private void LateUpdate()
		{
			this._amount.text = LocalPlayer.Stats.PedometerSteps.ToString();
			this._heart.text = LocalPlayer.Stats.HeartRate.ToString();
			this._body.text = LocalPlayer.Stats.BodyTemp.ToString();
			this._temp.text = Clock.Temp.ToString();
		}

		
		public UILabel _amount;

		
		public UILabel _heart;

		
		public UILabel _body;

		
		public UILabel _temp;
	}
}
