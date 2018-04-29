using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Connect a joint to a game object.")]
	[ActionCategory(ActionCategory.Physics)]
	public class SetJointConnectedBody : FsmStateAction
	{
		
		public override void Reset()
		{
			this.joint = null;
			this.rigidBody = null;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.joint);
			if (ownerDefaultTarget != null)
			{
				Joint component = ownerDefaultTarget.GetComponent<Joint>();
				if (component != null)
				{
					component.connectedBody = ((!(this.rigidBody.Value == null)) ? this.rigidBody.Value.GetComponent<Rigidbody>() : null);
				}
			}
			base.Finish();
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Joint))]
		[Tooltip("The joint to connect. Requires a Joint component.")]
		public FsmOwnerDefault joint;

		
		[CheckForComponent(typeof(Rigidbody))]
		[Tooltip("The game object to connect to the Joint. Set to none to connect the Joint to the world.")]
		public FsmGameObject rigidBody;
	}
}
