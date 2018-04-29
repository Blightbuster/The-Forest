using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
	
	public class ArcheryTarget : MonoBehaviour
	{
		
		private void ApplyDamage()
		{
			this.OnDamageTaken();
		}

		
		private void FireExposure()
		{
			this.OnDamageTaken();
		}

		
		private void OnDamageTaken()
		{
			if (this.targetEnabled)
			{
				this.onTakeDamage.Invoke();
				base.StartCoroutine(this.FallDown());
				if (this.onceOnly)
				{
					this.targetEnabled = false;
				}
			}
		}

		
		private IEnumerator FallDown()
		{
			if (this.baseTransform)
			{
				Quaternion startingRot = this.baseTransform.rotation;
				float startTime = Time.time;
				float rotLerp = 0f;
				while (rotLerp < 1f)
				{
					rotLerp = Util.RemapNumberClamped(Time.time, startTime, startTime + this.fallTime, 0f, 1f);
					this.baseTransform.rotation = Quaternion.Lerp(startingRot, this.fallenDownTransform.rotation, rotLerp);
					yield return null;
				}
			}
			yield return null;
			yield break;
		}

		
		public UnityEvent onTakeDamage;

		
		public bool onceOnly;

		
		public Transform targetCenter;

		
		public Transform baseTransform;

		
		public Transform fallenDownTransform;

		
		public float fallTime = 0.5f;

		
		private const float targetRadius = 0.25f;

		
		private bool targetEnabled = true;
	}
}
