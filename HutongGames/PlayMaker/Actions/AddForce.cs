using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Adds a force to a Game Object. Use Vector3 variable and/or Float variables for each axis.")]
	public class AddForce : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.atPosition = new FsmVector3
			{
				UseVariable = true
			};
			this.vector = null;
			this.x = new FsmFloat
			{
				UseVariable = true
			};
			this.y = new FsmFloat
			{
				UseVariable = true
			};
			this.z = new FsmFloat
			{
				UseVariable = true
			};
			this.space = Space.World;
			this.forceMode = ForceMode.Force;
			this.everyFrame = false;
		}

		
		public override void Awake()
		{
			base.Fsm.HandleFixedUpdate = true;
		}

		
		public override void OnEnter()
		{
			this.DoAddForce();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnFixedUpdate()
		{
			this.DoAddForce();
		}

		
		private void DoAddForce()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (!base.UpdateCache(ownerDefaultTarget))
			{
				return;
			}
			Vector3 force = (!this.vector.IsNone) ? this.vector.Value : new Vector3(this.x.Value, this.y.Value, this.z.Value);
			if (!this.x.IsNone)
			{
				force.x = this.x.Value;
			}
			if (!this.y.IsNone)
			{
				force.y = this.y.Value;
			}
			if (!this.z.IsNone)
			{
				force.z = this.z.Value;
			}
			if (this.space == Space.World)
			{
				if (!this.atPosition.IsNone)
				{
					base.rigidbody.AddForceAtPosition(force, this.atPosition.Value, this.forceMode);
				}
				else
				{
					base.rigidbody.AddForce(force, this.forceMode);
				}
			}
			else
			{
				base.rigidbody.AddRelativeForce(force, this.forceMode);
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		[Tooltip("The GameObject to apply the force to.")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("Optionally apply the force at a position on the object. This will also add some torque. The position is often returned from MousePick or GetCollisionInfo actions.")]
		public FsmVector3 atPosition;

		
		[UIHint(UIHint.Variable)]
		[Tooltip("A Vector3 force to add. Optionally override any axis with the X, Y, Z parameters.")]
		public FsmVector3 vector;

		
		[Tooltip("Force along the X axis. To leave unchanged, set to 'None'.")]
		public FsmFloat x;

		
		[Tooltip("Force along the Y axis. To leave unchanged, set to 'None'.")]
		public FsmFloat y;

		
		[Tooltip("Force along the Z axis. To leave unchanged, set to 'None'.")]
		public FsmFloat z;

		
		[Tooltip("Apply the force in world or local space.")]
		public Space space;

		
		[Tooltip("The type of force to apply. See Unity Physics docs.")]
		public ForceMode forceMode;

		
		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;
	}
}
