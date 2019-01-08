using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GUIElement)]
	[Tooltip("Sets the Text used by the GUIText Component attached to a Game Object.")]
	public class SetGUIText : ComponentAction<GUIText>
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.text = string.Empty;
		}

		public override void OnEnter()
		{
			this.DoSetGUIText();
			if (!this.everyFrame)
			{
				base.Finish();
			}
		}

		public override void OnUpdate()
		{
			this.DoSetGUIText();
		}

		private void DoSetGUIText()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.guiText.text = this.text.Value;
			}
		}

		[RequiredField]
		[CheckForComponent(typeof(GUIText))]
		public FsmOwnerDefault gameObject;

		public FsmString text;

		public bool everyFrame;
	}
}
