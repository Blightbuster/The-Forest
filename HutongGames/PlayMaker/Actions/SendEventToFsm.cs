using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Obsolete("This action is obsolete; use Send Event with Event Target instead.")]
	[Tooltip("Sends an Event to another Fsm after an optional delay. Specify an Fsm Name or use the first Fsm on the object.")]
	[ActionCategory(ActionCategory.StateMachine)]
	public class SendEventToFsm : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.fsmName = null;
			this.sendEvent = null;
			this.delay = null;
			this.requireReceiver = false;
		}

		
		public override void OnEnter()
		{
			this.go = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (this.go == null)
			{
				base.Finish();
				return;
			}
			PlayMakerFSM gameObjectFsm = ActionHelpers.GetGameObjectFsm(this.go, this.fsmName.Value);
			if (gameObjectFsm == null)
			{
				if (this.requireReceiver)
				{
					this.LogError("GameObject doesn't have FsmComponent: " + this.go.name + " " + this.fsmName.Value);
				}
				return;
			}
			if ((double)this.delay.Value < 0.001)
			{
				gameObjectFsm.Fsm.Event(this.sendEvent.Value);
				base.Finish();
			}
			else
			{
				this.delayedEvent = gameObjectFsm.Fsm.DelayedEvent(FsmEvent.GetFsmEvent(this.sendEvent.Value), this.delay.Value);
			}
		}

		
		public override void OnUpdate()
		{
			if (DelayedEvent.WasSent(this.delayedEvent))
			{
				base.Finish();
			}
		}

		
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Optional name of Fsm on Game Object")]
		[UIHint(UIHint.FsmName)]
		public FsmString fsmName;

		
		[RequiredField]
		[UIHint(UIHint.FsmEvent)]
		public FsmString sendEvent;

		
		[HasFloatSlider(0f, 10f)]
		public FsmFloat delay;

		
		private bool requireReceiver;

		
		private GameObject go;

		
		private DelayedEvent delayedEvent;
	}
}
