using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the Color of the GUITexture attached to a Game Object.")]
	[ActionCategory(ActionCategory.GUIElement)]
	public class SetGUITextureColor : ComponentAction<GUITexture>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.color = Color.white;
			this.everyFrame = false;
		}

		
		public override void OnEnter()
		{
			this.DoSetGUITextureColor();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		
		public override void OnUpdate()
		{
			this.DoSetGUITextureColor();
		}

		
		private void DoSetGUITextureColor()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.guiTexture.color = this.color.Value;
			}
		}

		
		[RequiredField]
		[CheckForComponent(typeof(GUITexture))]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		public FsmColor color;

		
		public bool everyFrame;
	}
}
