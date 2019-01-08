using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.TaskSystem
{
	[DoNotSerializePublic]
	[Serializable]
	public class AirBreathingCondition : ACondition
	{
		public override void Init()
		{
			this._routine = Scene.ActiveMB.StartCoroutine(this.CheckProximityRoutine());
		}

		public override void Clear()
		{
			if (this._routine != null)
			{
				Scene.ActiveMB.StopCoroutine(this._routine);
			}
			base.Clear();
		}

		public IEnumerator CheckProximityRoutine()
		{
			if (!this._done)
			{
				while (!this.IsAirBellowThreshold())
				{
					yield return YieldPresets.WaitOneSecond;
				}
				this.SetDone();
				this.Clear();
			}
			yield break;
		}

		private bool IsAirBellowThreshold()
		{
			return !(LocalPlayer.Stats == null) && LocalPlayer.Stats.AirBreathing != null && LocalPlayer.Stats.AirBreathing.CurrentAirPercent < this._threshold;
		}

		[Header("AirBreathing")]
		public float _threshold;

		private Coroutine _routine;
	}
}
