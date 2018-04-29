using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory(ActionCategory.Movie)]
	[Tooltip("Sets the Game Object as the Audio Source associated with the Movie Texture. The Game Object must have an AudioSource Component.")]
	public class MovieTextureAudioSettings : FsmStateAction
	{
		
		public override void Reset()
		{
			this.movieTexture = null;
			this.gameObject = null;
		}

		
		public override void OnEnter()
		{
			MovieTexture movieTexture = this.movieTexture.Value as MovieTexture;
			if (movieTexture != null && this.gameObject.Value != null)
			{
				AudioSource component = this.gameObject.Value.GetComponent<AudioSource>();
				if (component != null)
				{
					component.clip = movieTexture.audioClip;
				}
			}
			base.Finish();
		}

		
		[RequiredField]
		[ObjectType(typeof(MovieTexture))]
		public FsmObject movieTexture;

		
		[RequiredField]
		[CheckForComponent(typeof(AudioSource))]
		public FsmGameObject gameObject;
	}
}
