using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Gets the Collision Flags from a Character Controller on a Game Object. Collision flags give you a broad overview of where the character collided with any other object.")]
	[ActionCategory(ActionCategory.Character)]
	public class GetControllerCollisionFlags : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.isGrounded = null;
			this.none = null;
			this.sides = null;
			this.above = null;
			this.below = null;
		}

		
		public override void OnUpdate()
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
			if (this.controller != null)
			{
				this.isGrounded.Value = this.controller.isGrounded;
				FsmBool fsmBool = this.none;
				CollisionFlags collisionFlags = this.controller.collisionFlags;
				fsmBool.Value = false;
				this.sides.Value = ((this.controller.collisionFlags & CollisionFlags.Sides) != CollisionFlags.None);
				this.above.Value = ((this.controller.collisionFlags & CollisionFlags.Above) != CollisionFlags.None);
				this.below.Value = ((this.controller.collisionFlags & CollisionFlags.Below) != CollisionFlags.None);
			}
		}

		
		[CheckForComponent(typeof(CharacterController))]
		[RequiredField]
		[Tooltip("The GameObject with a Character Controller component.")]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("True if the Character Controller capsule is on the ground")]
		[UIHint(UIHint.Variable)]
		public FsmBool isGrounded;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("True if no collisions in last move.")]
		public FsmBool none;

		
		[Tooltip("True if the Character Controller capsule was hit on the sides.")]
		[UIHint(UIHint.Variable)]
		public FsmBool sides;

		
		[Tooltip("True if the Character Controller capsule was hit from above.")]
		[UIHint(UIHint.Variable)]
		public FsmBool above;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("True if the Character Controller capsule was hit from below.")]
		public FsmBool below;

		
		private GameObject previousGo;

		
		private CharacterController controller;
	}
}
