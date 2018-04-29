using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Logic)]
	[Tooltip("Tests if a Game Object is visible.")]
	public class GameObjectIsVisible : ComponentAction<Renderer>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.trueEvent = null;
			this.falseEvent = null;
			this.storeResult = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoIsVisible();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoIsVisible();
		}

		
		private void DoIsVisible()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				bool isVisible = base.renderer.isVisible;
				this.storeResult.Value = isVisible;
				base.Fsm.Event((!isVisible) ? this.falseEvent : this.trueEvent);
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Renderer))]
		[Tooltip("The GameObject to test.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Event to send if the GameObject is visible.")]
		public FsmEvent trueEvent;

		
		[Tooltip("Event to send if the GameObject is NOT visible.")]
		public FsmEvent falseEvent;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a bool variable.")]
		public FsmBool storeResult;

		
		public bool everyFrame;
	}
}
