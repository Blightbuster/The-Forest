using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Web Player")]
	[Tooltip("Gets data from a url and store it in variables. See Unity WWW docs for more details.")]
	public class WWWObject : FsmStateAction
	{
		public override void Reset()
		{
			this.url = null;
			this.storeText = null;
			this.storeTexture = null;
			this.errorString = null;
			this.progress = null;
			this.isDone = null;
		}

		public override void OnEnter()
		{
			if (string.IsNullOrEmpty(this.url.Value))
			{
				base.Finish();
				return;
			}
			this.wwwObject = new WWW(this.url.Value);
		}

		public override void OnUpdate()
		{
			if (this.wwwObject == null)
			{
				this.errorString.Value = "WWW Object is Null!";
				base.Finish();
				return;
			}
			this.errorString.Value = this.wwwObject.error;
			if (!string.IsNullOrEmpty(this.wwwObject.error))
			{
				base.Finish();
				base.Fsm.Event(this.isError);
				return;
			}
			this.progress.Value = this.wwwObject.progress;
			if (this.progress.Value.Equals(1f))
			{
				this.storeText.Value = this.wwwObject.text;
				this.storeTexture.Value = this.wwwObject.texture;
				this.storeMovieTexture.Value = this.wwwObject.GetMovieTexture();
				this.errorString.Value = this.wwwObject.error;
				base.Fsm.Event((!string.IsNullOrEmpty(this.errorString.Value)) ? this.isError : this.isDone);
				base.Finish();
			}
		}

		[RequiredField]
		[Tooltip("Url to download data from.")]
		public FsmString url;

		[ActionSection("Results")]
		[UIHint(UIHint.Variable)]
		[Tooltip("Gets text from the url.")]
		public FsmString storeText;

		[UIHint(UIHint.Variable)]
		[Tooltip("Gets a Texture from the url.")]
		public FsmTexture storeTexture;

		[UIHint(UIHint.Variable)]
		[ObjectType(typeof(MovieTexture))]
		[Tooltip("Gets a Texture from the url.")]
		public FsmObject storeMovieTexture;

		[UIHint(UIHint.Variable)]
		[Tooltip("Error message if there was an error during the download.")]
		public FsmString errorString;

		[UIHint(UIHint.Variable)]
		[Tooltip("How far the download progressed (0-1).")]
		public FsmFloat progress;

		[ActionSection("Events")]
		[Tooltip("Event to send when the data has finished loading (progress = 1).")]
		public FsmEvent isDone;

		[Tooltip("Event to send if there was an error.")]
		public FsmEvent isError;

		private WWW wwwObject;
	}
}
