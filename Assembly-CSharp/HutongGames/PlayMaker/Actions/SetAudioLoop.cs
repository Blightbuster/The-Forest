using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Audio)]
	[Tooltip("Sets looping on the AudioSource component on a Game Object.")]
	public class SetAudioLoop : ComponentAction<AudioSource>
	{
		public override void Reset()
		{
			this.gameObject = null;
			this.loop = false;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.audio.loop = this.loop.Value;
			}
			base.Finish();
		}

		[RequiredField]
		[CheckForComponent(typeof(AudioSource))]
		public FsmOwnerDefault gameObject;

		public FsmBool loop;
	}
}
