using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Sets the Audio Clip played by the AudioSource component on a Game Object.")]
	[ActionCategory(ActionCategory.Audio)]
	public class SetAudioClip : ComponentAction<AudioSource>
	{
		
		public override void Reset()
		{
			this.gameObject = null;
			this.audioClip = null;
		}

		
		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (base.UpdateCache(ownerDefaultTarget))
			{
				base.audio.clip = (this.audioClip.Value as AudioClip);
			}
			base.Finish();
		}

		
		[RequiredField]
		[Tooltip("The GameObject with the AudioSource component.")]
		[CheckForComponent(typeof(AudioSource))]
		public FsmOwnerDefault gameObject;

		
		[Tooltip("The AudioClip to set.")]
		[ObjectType(typeof(AudioClip))]
		public FsmObject audioClip;
	}
}
