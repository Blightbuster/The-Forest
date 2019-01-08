using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Tests if a Game Object's Rigid Body is sleeping.")]
	public class IsSleeping : ComponentAction<Rigidbody>
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.trueEvent = null;
			this.falseEvent = null;
			this.store = null;
			this.everyFrame = false;
		}

		public override void OnEnter()
		{
			this.DoIsSleeping();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoIsSleeping();
		}

		private void DoIsSleeping()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				bool flag = base.rigidbody.IsSleeping();
				this.store.Value = flag;
				base.Fsm.Event((!flag) ? this.falseEvent : this.trueEvent);
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		public FsmOwnerDefault gameObject;

		public FsmEvent trueEvent;

		public FsmEvent falseEvent;

		[UIHint(UIHint.Variable)]
		public FsmBool store;

		public bool everyFrame;
	}
}
