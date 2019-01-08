using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GUIElement)]
	[Tooltip("Sets the Texture used by the GUITexture attached to a Game Object.")]
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

		[RequiredField]
		[CheckForComponent(typeof(GUITexture))]
		[Tooltip("The GameObject that owns the GUITexture.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("Texture to apply.")]
		public FsmTexture texture;
	}
}
