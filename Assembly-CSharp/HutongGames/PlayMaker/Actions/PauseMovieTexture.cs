using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Movie)]
	[Tooltip("Pauses a Movie Texture.")]
	public class PauseMovieTexture : FsmStateAction
	{
		public override void Reset()
		{
			this.movieTexture = null;
		}

		public override void OnEnter()
		{
			MovieTexture movieTexture = this.movieTexture.Value as MovieTexture;
			if (movieTexture != null)
			{
				movieTexture.Pause();
			}
			base.Finish();
		}

		[RequiredField]
		[ObjectType(typeof(MovieTexture))]
		public FsmObject movieTexture;
	}
}
