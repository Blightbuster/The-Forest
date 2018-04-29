using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the Texture used by the GUITexture attached to a Game Object.")]
	[ActionCategory(ActionCategory.GUIElement)]
	public class SetGUITexture : ComponentAction<GUITexture>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.texture = null;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.guiTexture.texture = this.texture.Value;
			}
			base.Finish();
		}

		
		[CheckForComponent(typeof(GUITexture))]
		[Tooltip("The GameObject that owns the GUITexture.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("Texture to apply.")]
		public FsmTexture texture;
	}
}
