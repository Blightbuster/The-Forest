using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Audio)]
	[Tooltip("Mute/unmute the Audio Clip played by an Audio Source component on a Game Object.")]
	public class AudioMute : FsmStateAction
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.mute = false;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (ownerDefaultTarget != null)
			{
				AudioSource component = ownerDefaultTarget.GetComponent<AudioSource>();
				if (component != null)
				{
					component.mute = this.mute.Value;
				}
			}
			base.Finish();
		}

		
		[RequiredField]
		[CheckForComponent(typeof(AudioSource))]
		[Tooltip("The GameObject with an Audio Source component.")]
		public FsmOwnerDefault gameObject;

		
		[RequiredField]
		[Tooltip("Check to mute, uncheck to unmute.")]
		public FsmBool mute;
	}
}
