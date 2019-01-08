using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class LinearAnimation : MonoBehaviour
	{
		private void Awake()
		{
			if (this.animation == null)
			{
				this.animation = base.GetComponent<Animation>();
			}
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
			this.animation.playAutomatically = true;
			this.animState = this.animation[this.animation.clip.name];
			this.animState.wrapMode = WrapMode.PingPong;
			this.animState.speed = 0f;
			this.animLength = this.animState.length;
		}

		private void Update()
		{
			float value = this.linearMapping.value;
			if (value != this.lastValue)
			{
				this.animState.time = value / this.animLength;
			}
			this.lastValue = value;
		}

		public LinearMapping linearMapping;

		public Animation animation;

		private AnimationState animState;

		private float animLength;

		private float lastValue;
	}
}
