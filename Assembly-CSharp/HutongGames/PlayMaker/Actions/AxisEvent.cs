using System;
using TheForest.Utils;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Sends events based on the direction of Input Axis (Left/Right/Up/Down...).")]
	public class AxisEvent : FsmStateAction
	{
		public override void Reset()
		{
			this.horizontalAxis = "Horizontal";
			this.verticalAxis = "Vertical";
			this.leftEvent = null;
			this.rightEvent = null;
			this.upEvent = null;
			this.downEvent = null;
			this.anyDirection = null;
			this.noDirection = null;
		}

		public override void OnUpdate()
		{
			float num = (!(this.horizontalAxis.Value != string.Empty)) ? 0f : TheForest.Utils.Input.GetAxis(this.horizontalAxis.Value);
			float num2 = (!(this.verticalAxis.Value != string.Empty)) ? 0f : TheForest.Utils.Input.GetAxis(this.verticalAxis.Value);
			float num3 = num * num + num2 * num2;
			if (num3 < 0.2f)
			{
				if (this.noDirection != null)
				{
					base.Fsm.Event(this.noDirection);
				}
				return;
			}
			float num4 = Mathf.Atan2(num2, num) * 57.29578f + 45f;
			if (num4 < 0f)
			{
				num4 += 360f;
			}
			int num5 = (int)(num4 / 90f);
			if (num5 == 0 && this.rightEvent != null)
			{
				base.Fsm.Event(this.rightEvent);
			}
			else if (num5 == 1 && this.upEvent != null)
			{
				base.Fsm.Event(this.upEvent);
			}
			else if (num5 == 2 && this.leftEvent != null)
			{
				base.Fsm.Event(this.leftEvent);
			}
			else if (num5 == 3 && this.downEvent != null)
			{
				base.Fsm.Event(this.downEvent);
			}
			else if (this.anyDirection != null)
			{
				base.Fsm.Event(this.anyDirection);
			}
		}

		[Tooltip("Horizontal axis as defined in the Input Manager")]
		public FsmString horizontalAxis;

		[Tooltip("Vertical axis as defined in the Input Manager")]
		public FsmString verticalAxis;

		[Tooltip("Event to send if input is to the left.")]
		public FsmEvent leftEvent;

		[Tooltip("Event to send if input is to the right.")]
		public FsmEvent rightEvent;

		[Tooltip("Event to send if input is to the up.")]
		public FsmEvent upEvent;

		[Tooltip("Event to send if input is to the down.")]
		public FsmEvent downEvent;

		[Tooltip("Event to send if input is in any direction.")]
		public FsmEvent anyDirection;

		[Tooltip("Event to send if no axis input (centered).")]
		public FsmEvent noDirection;
	}
}
