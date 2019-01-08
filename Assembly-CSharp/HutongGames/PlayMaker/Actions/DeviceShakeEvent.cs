using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends an Event when the mobile device is shaken.")]
	public class DeviceShakeEvent : FsmStateAction
	{
		public override void Reset()
		{
			this.shakeThreshold = 3f;
			this.sendEvent = null;
		}

		public override void OnUpdate()
		{
			if (Input.acceleration.sqrMagnitude > this.shakeThreshold.Value * this.shakeThreshold.Value)
			{
				base.Fsm.Event(this.sendEvent);
			}
		}

		[RequiredField]
		[Tooltip("Amount of acceleration required to trigger the event. Higher numbers require a harder shake.")]
		public FsmFloat shakeThreshold;

		[RequiredField]
		[Tooltip("Event to send when Shake Threshold is exceded.")]
		public FsmEvent sendEvent;
	}
}
