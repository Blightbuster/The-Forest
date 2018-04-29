using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Rigid bodies start sleeping when they come to rest. This action wakes up all rigid bodies in the scene. E.g., if you Set Gravity and want objects at rest to respond.")]
	public class WakeAllRigidBodies : FsmStateAction
	{
		
		public override void Reset()
		{
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.bodies = (UnityEngine.Object.FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[]);
			this.DoWakeAll();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoWakeAll();
		}

		
		private void DoWakeAll()
		{
			this.bodies = (UnityEngine.Object.FindObjectsOfType(typeof(Rigidbody)) as Rigidbody[]);
			if (this.bodies != null)
			{
				foreach (Rigidbody rigidbody in this.bodies)
				{
					rigidbody.WakeUp();
				}
			}
		}

		
		public bool everyFrame;

		
		private Rigidbody[] bodies;
	}
}
