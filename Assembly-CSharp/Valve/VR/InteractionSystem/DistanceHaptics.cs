using System;
using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class DistanceHaptics : MonoBehaviour
	{
		private IEnumerator Start()
		{
			for (;;)
			{
				float distance = Vector3.Distance(this.firstTransform.position, this.secondTransform.position);
				SteamVR_TrackedObject trackedObject = base.GetComponentInParent<SteamVR_TrackedObject>();
				if (trackedObject)
				{
					float num = this.distanceIntensityCurve.Evaluate(distance);
					SteamVR_Controller.Input((int)trackedObject.index).TriggerHapticPulse((ushort)num, EVRButtonId.k_EButton_Axis0);
				}
				float nextPulse = this.pulseIntervalCurve.Evaluate(distance);
				yield return new WaitForSeconds(nextPulse);
			}
			yield break;
		}

		public Transform firstTransform;

		public Transform secondTransform;

		public AnimationCurve distanceIntensityCurve = AnimationCurve.Linear(0f, 800f, 1f, 800f);

		public AnimationCurve pulseIntervalCurve = AnimationCurve.Linear(0f, 0.01f, 1f, 0f);
	}
}
