using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[Tooltip("Plays a Movie Texture. Use the Movie Texture in a Material, or in the GUI.")]
	[ActionCategory(ActionCategory.Movie)]
	public class PlayMovieTexture : FsmStateAction
	{
		
		public override void Reset()
		{
			this.movieTexture = null;
			this.loop = false;
		}

		
		public override void OnEnter()
		{
			MovieTexture movieTexture = this.movieTexture.Value as MovieTexture;
			if (movieTexture != null)
			{
				movieTexture.loop = this.loop.Value;
				movieTexture.Play();
			}
			base.Finish();
		}

		
		[RequiredField]
		[ObjectType(typeof(MovieTexture))]
		public FsmObject movieTexture;

		
		public FsmBool loop;
	}
}
