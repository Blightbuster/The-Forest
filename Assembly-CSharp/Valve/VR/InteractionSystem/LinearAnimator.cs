using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class LinearAnimator : MonoBehaviour
	{
		private void Awake()
		{
			if (this.animator == null)
			{
				this.animator = base.GetComponent<Animator>();
			}
			this.animator.speed = 0f;
			if (this.linearMapping == null)
			{
				this.linearMapping = base.GetComponent<LinearMapping>();
			}
		}

		private void Update()
		{
			if (this.currentLinearMapping != this.linearMapping.value)
			{
				this.currentLinearMapping = this.linearMapping.value;
				this.animator.enabled = true;
				this.animator.Play(0, 0, this.currentLinearMapping);
				this.framesUnchanged = 0;
			}
			else
			{
				this.framesUnchanged++;
				if (this.framesUnchanged > 2)
				{
				}
			}
		}

		public LinearMapping linearMapping;

		public Animator animator;

		private float currentLinearMapping = float.NaN;

		private int framesUnchanged;
	}
}
