using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Lights)]
	[Tooltip("Sets the strength of the shadows cast by a Light.")]
	public class SetShadowStrength : ComponentAction<Light>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.shadowStrength = 0.8f;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetShadowStrength();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetShadowStrength();
		}

		
		private void DoSetShadowStrength()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.light.shadowStrength = this.shadowStrength.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Light))]
		public FsmOwnerDefault gameObject;

		
		public FsmFloat shadowStrength;

		
		public bool everyFrame;
	}
}
