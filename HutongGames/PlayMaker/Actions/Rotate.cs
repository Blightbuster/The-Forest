using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Rotates a Game Object around each Axis. Use a Vector3 Variable and/or XYZ components. To leave any axis unchanged, set variable to 'None'.")]
	[ActionCategory(ActionCategory.Transform)]
	public class Rotate : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.vector = null;
			this.xAngle = new FsmFloat
			{
				UseVariable = true
			};
			this.yAngle = new FsmFloat
			{
				UseVariable = true
			};
			this.zAngle = new FsmFloat
			{
				UseVariable = true
			};
			this.space = Space.Self;
			this.perSecond = false;
			this.everyFrame = true;
			this.lateUpdate = false;
			this.fixedUpdate = false;
		}

		
		public override void Awake()
		{
			base.Fsm.HandleFixedUpdate = true;
		}

		
		public override void OnEnter()
		{
			if (!this.everyFrame && !this.lateUpdate && !this.fixedUpdate)
			{
				this.DoRotate();
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			if (!this.lateUpdate && !this.fixedUpdate)
			{
				this.DoRotate();
			}
		}

		
		public override void OnLateUpdate()
		{
			if (this.lateUpdate)
			{
				this.DoRotate();
			}
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnFixedUpdate()
		{
			if (this.fixedUpdate)
			{
				this.DoRotate();
			}
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		private void DoRotate()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null)
			{
				return;
			}
			Vector3 vector = (!this.vector.IsNone) ? this.vector.Value : new Vector3(this.xAngle.Value, this.yAngle.Value, this.zAngle.Value);
			if (!this.xAngle.IsNone)
			{
				vector.x = this.xAngle.Value;
			}
			if (!this.yAngle.IsNone)
			{
				vector.y = this.yAngle.Value;
			}
			if (!this.zAngle.IsNone)
			{
				vector.z = this.zAngle.Value;
			}
			if (!this.perSecond)
			{
				ownerDefaultTarget.transform.Rotate(vector, this.space);
			}
			else
			{
				ownerDefaultTarget.transform.Rotate(vector * Time.deltaTime, this.space);
			}
		}

		
		[Tooltip("The game object to rotate.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("A rotation vector. NOTE: You can override individual axis below.")]
		[UIHint(UIHint.Variable)]
		public FsmVector3 vector;

		
		[Tooltip("Rotation around x axis.")]
		public FsmFloat xAngle;

		
		[Tooltip("Rotation around y axis.")]
		public FsmFloat yAngle;

		
		[Tooltip("Rotation around z axis.")]
		public FsmFloat zAngle;

		
		[Tooltip("Rotate in local or world space.")]
		public Space space;

		
		[Tooltip("Rotate over one second")]
		public bool perSecond;

		
		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		
		[Tooltip("Perform the rotation in LateUpdate. This is useful if you want to override the rotation of objects that are animated or otherwise rotated in Update.")]
		public bool lateUpdate;

		
		[Tooltip("Perform the rotation in FixedUpdate. This is useful when working with rigid bodies and physics.")]
		public bool fixedUpdate;
	}
}
