using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.ScriptControl)]
	[Tooltip("Enables/Disables a Behaviour on a GameObject. Optionally reset the Behaviour on exit - useful if you only want the Behaviour to be active while this state is active.")]
	public class EnableBehaviour : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.behaviour = null;
			this.component = null;
			this.enable = true;
			this.resetOnExit = true;
		}

		
		public override void OnEnter()
		{
			this.DoEnableBehaviour(base.Fsm.GetOwnerDefaultTarget(this.gameObject));
			base.Finish();
		}

		
		private void DoEnableBehaviour(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			if (this.component != null)
			{
				this.componentTarget = (this.component as Behaviour);
			}
			else
			{
				this.componentTarget = (go.GetComponent(this.behaviour.Value) as Behaviour);
			}
			if (this.componentTarget == null)
			{
				base.LogWarning(" " + go.name + " missing behaviour: " + this.behaviour.Value);
				return;
			}
			this.componentTarget.enabled = this.enable.Value;
		}

		
		public override void OnExit()
		{
			if (this.componentTarget == null)
			{
				return;
			}
			if (this.resetOnExit.Value)
			{
				this.componentTarget.enabled = !this.enable.Value;
			}
		}

		
		public override string ErrorCheck()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget == null || this.component != null || this.behaviour.IsNone || string.IsNullOrEmpty(this.behaviour.Value))
			{
				return null;
			}
			Behaviour x = ownerDefaultTarget.GetComponent(this.behaviour.Value) as Behaviour;
			return (!(x != null)) ? "Behaviour missing" : null;
		}

		
		[RequiredField]
		[Tooltip("The GameObject that owns the Behaviour.")]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.Behaviour)]
		[Tooltip("The name of the Behaviour to enable/disable.")]
		public FsmString behaviour;

		
		[Tooltip("Optionally drag a component directly into this field (behavior name will be ignored).")]
		public Component component;

		
		[RequiredField]
		[Tooltip("Set to True to enable, False to disable.")]
		public FsmBool enable;

		
		public FsmBool resetOnExit;

		
		private Behaviour componentTarget;
	}
}
