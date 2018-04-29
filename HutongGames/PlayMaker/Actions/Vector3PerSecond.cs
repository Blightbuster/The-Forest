using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Multiplies a Vector3 variable by Time.deltaTime. Useful for frame rate independent motion.")]
	[ActionCategory(ActionCategory.Vector3)]
	public class Vector3PerSecond : FsmStateAction
	{
		
		public override void Reset()
		{
			this.vector3Variable = null;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.vector3Variable.Value = this.vector3Variable.Value * Time.deltaTime;
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.vector3Variable.Value = this.vector3Variable.Value * Time.deltaTime;
		}

		
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmVector3 vector3Variable;

		
		public bool everyFrame;
	}
}
