using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Character)]
	[Tooltip("Tests if a Character Controller on a Game Object was touching the ground during the last move.")]
	public class ControllerIsGrounded : FsmStateAction
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
			this.DoControllerIsGrounded();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoControllerIsGrounded();
		}

		
		private void DoControllerIsGrounded()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			if (ownerDefaultTarget != this.previousGo)
			{
				this.controller = ownerDefaultTarget.GetComponent<CharacterController>();
				this.previousGo = ownerDefaultTarget;
			}
			if (this.controller == null)
			{
				return;
			}
			bool isGrounded = this.controller.isGrounded;
			this.storeResult.Value = isGrounded;
			base.Fsm.Event((!isGrounded) ? this.falseEvent : this.trueEvent);
		}

		
		[CheckForComponent(typeof(CharacterController))]
		[RequiredField]
		[Tooltip("The GameObject to check.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Event to send if touching the ground.")]
		public FsmEvent trueEvent;

		
		[Tooltip("Event to send if not touching the ground.")]
		public FsmEvent falseEvent;

		
		[Tooltip("Sore the result in a bool variable.")]
		[UIHint(UIHint.Variable)]
		public FsmBool storeResult;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		
		private GameObject previousGo;

		
		private CharacterController controller;
	}
}
