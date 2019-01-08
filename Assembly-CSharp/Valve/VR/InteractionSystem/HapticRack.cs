using System;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	[RequireComponent(typeof(Interactable))]
	public class HapticRack : MonoBehaviour
	{
		private void Awake()
		{
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
		}

		private void OnHandHoverBegin(Hand hand)
		{
			this.hand = hand;
		}

		private void OnHandHoverEnd(Hand hand)
		{
			this.hand = null;
		}

		private void Update()
		{
			int num = Mathf.RoundToInt(this.linearMapping.value * (float)this.teethCount - 0.5f);
			if (num != this.previousToothIndex)
			{
				this.Pulse();
				this.previousToothIndex = num;
			}
		}

		private void Pulse()
		{
			if (this.hand && this.hand.controller != null && this.hand.GetStandardInteractionButton())
			{
				ushort durationMicroSec = (ushort)UnityEngine.Random.Range(this.minimumPulseDuration, this.maximumPulseDuration + 1);
				this.hand.controller.TriggerHapticPulse(durationMicroSec, EVRButtonId.k_EButton_Axis0);
				this.onPulse.Invoke();
			}
		}

		[Tooltip("The linear mapping driving the haptic rack")]
		public LinearMapping linearMapping;

		[Tooltip("The number of haptic pulses evenly distributed along the mapping")]
		public int teethCount = 128;

		[Tooltip("Minimum duration of the haptic pulse")]
		public int minimumPulseDuration = 500;

		[Tooltip("Maximum duration of the haptic pulse")]
		public int maximumPulseDuration = 900;

		[Tooltip("This event is triggered every time a haptic pulse is made")]
		public UnityEvent onPulse;

		private Hand hand;

		private int previousToothIndex = -1;
	}
}
