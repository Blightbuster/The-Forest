using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends events based on Touch Phases. Optionally filter by a fingerID.")]
	public class TouchEvent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.fingerId = new FsmInt
			{
				UseVariable = true
			};
			this.storeFingerId = null;
		}

		
		public override void OnUpdate()
		{
			if (Input.touchCount > 0)
			{
				foreach (Touch touch in Input.touches)
				{
					if ((this.fingerId.IsNone || touch.fingerId == this.fingerId.Value) && touch.phase == this.touchPhase)
					{
						this.storeFingerId.Value = touch.fingerId;
						base.Fsm.Event(this.sendEvent);
					}
				}
			}
		}

		
		public FsmInt fingerId;

		
		public TouchPhase touchPhase;

		
		public FsmEvent sendEvent;

		
		[UIHint(UIHint.Variable)]
		public FsmInt storeFingerId;
	}
}
