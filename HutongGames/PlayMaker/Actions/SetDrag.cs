using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[HelpUrl("http:
	[ActionCategory(ActionCategory.Physics)]
	[Tooltip("Sets the Drag of a Game Object's Rigid Body.")]
	public class SetDrag : ComponentAction<Rigidbody>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.drag = 1f;
		}

		
		public override void OnEnter()
		{
			this.DoSetDrag();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetDrag();
		}

		
		private void DoSetDrag()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.rigidbody.drag = this.drag.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Rigidbody))]
		public FsmOwnerDefault gameObject;

		
		[HasFloatSlider(0f, 10f)]
		[RequiredField]
		public FsmFloat drag;

		
		[Tooltip("Repeat every frame. Typically this would be set to True.")]
		public bool everyFrame;
	}
}
