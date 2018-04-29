using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Destroys a Component of an Object.")]
	public class DestroyComponent : FsmStateAction
	{
		
		public override void Reset()
		{
			this.aComponent = null;
			this.gameObject = null;
			this.component = null;
		}

		
		public override void OnEnter()
		{
			this.DoDestroyComponent((this.gameObject.OwnerOption != OwnerDefaultOption.UseOwner) ? this.gameObject.GameObject.Value : base.Owner);
			base.Finish();
		}

		
		private void DoDestroyComponent(GameObject go)
		{
			this.aComponent = go.GetComponent(this.component.Value);
			if (this.aComponent == null)
			{
				this.LogError("No such component: " + this.component.Value);
			}
			else
			{
				UnityEngine.Object.Destroy(this.aComponent);
			}
		}

		
		[Tooltip("The GameObject that owns the Component.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[UIHint(UIHint.ScriptComponent)]
		[Tooltip("The name of the Component to destroy.")]
		[RequiredField]
		public FsmString component;

		
		private Component aComponent;
	}
}
