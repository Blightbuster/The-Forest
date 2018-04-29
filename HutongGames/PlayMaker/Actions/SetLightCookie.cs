using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Lights)]
	[Tooltip("Sets the Texture projected by a Light.")]
	public class SetLightCookie : ComponentAction<Light>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.lightCookie = null;
		}

		
		public override void OnEnter()
		{
			this.DoSetLightCookie();
			base.Finish();
		}

		
		private void DoSetLightCookie()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.light.cookie = this.lightCookie.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(Light))]
		public FsmOwnerDefault gameObject;

		
		public FsmTexture lightCookie;
	}
}
