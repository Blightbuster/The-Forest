using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Effects)]
	[Tooltip("Turns a Game Object on/off in a regular repeating pattern.")]
	public class Blink : ComponentAction<Renderer>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.timeOff = 0.5f;
			this.timeOn = 0.5f;
			this.rendererOnly = true;
			this.startOn = false;
			this.realTime = false;
		}

		
		public override void OnEnter()
		{
			this.startTime = FsmTime.RealtimeSinceStartup;
			this.timer = 0f;
			this.UpdateBlinkState(this.startOn.Value);
		}

		
		public override void OnUpdate()
		{
			if (this.realTime)
			{
				this.timer = FsmTime.RealtimeSinceStartup - this.startTime;
			}
			else
			{
				this.timer += Time.deltaTime;
			}
			if (this.blinkOn && this.timer > this.timeOn.Value)
			{
				this.UpdateBlinkState(false);
			}
			if (!this.blinkOn && this.timer > this.timeOff.Value)
			{
				this.UpdateBlinkState(true);
			}
		}

		
		private void UpdateBlinkState(bool state)
		{
			GameObject gameObject = (this.gameObject.OwnerOption != OwnerDefaultOption.UseOwner) ? this.gameObject.GameObject.Value : base.Owner;
			if (gameObject == null)
			{
				return;
			}
			if (this.rendererOnly)
			{
				if (base.UpdateCache(gameObject))
				{
					base.renderer.enabled = state;
				}
			}
			else
			{
				gameObject.SetActive(state);
			}
			this.blinkOn = state;
			this.startTime = FsmTime.RealtimeSinceStartup;
			this.timer = 0f;
		}

		
		[Tooltip("The GameObject to blink on/off.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[HasFloatSlider(0f, 5f)]
		[Tooltip("Time to stay off in seconds.")]
		public FsmFloat timeOff;

		
		[Tooltip("Time to stay on in seconds.")]
		[HasFloatSlider(0f, 5f)]
		public FsmFloat timeOn;

		
		[Tooltip("Should the object start in the active/visible state?")]
		public FsmBool startOn;

		
		[Tooltip("Only effect the renderer, keeping other components active.")]
		public bool rendererOnly;

		
		[Tooltip("Ignore TimeScale. Useful if the game is paused.")]
		public bool realTime;

		
		private float startTime;

		
		private float timer;

		
		private bool blinkOn;
	}
}
