using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	
	public class VideoStreaming : MonoBehaviour
	{
		
		private void OnDisable()
		{
			this.Stop();
			this.Clear();
		}

		
		public void Play(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				this._lastPlayed = path;
				base.StartCoroutine(this.PlayRoutine(path));
			}
		}

		
		public void Stop()
		{
			if (this._movie)
			{
				this._onStop.Invoke();
				Debug.Log("Video stream stopped");
				this._movie.Stop();
			}
			this._streamingFromResources = false;
		}

		
		public void Clear()
		{
			if (this._movie)
			{
				Debug.Log("Video stream cleared");
				this._onClear.Invoke();
				this._targetMaterial.SetTexture("_EmissionMap", null);
				this._movie.Stop();
				this._movie = null;
			}
			this._streamingFromResources = false;
		}

		
		private IEnumerator PlayRoutine(string path)
		{
			Debug.Log("Begin video stream routine: " + path);
			this._onBeginRead.Invoke();
			ResourceRequest request = Resources.LoadAsync<MovieTexture>(path);
			this._streamingFromResources = true;
			this._movie = (MovieTexture)request.asset;
			this._targetMaterial.SetTexture("_EmissionMap", this._movie);
			while (!this._movie.isReadyToPlay)
			{
				yield return null;
				if (!this._movie)
				{
					yield break;
				}
			}
			this._streamingFromResources = false;
			Debug.Log("Video stream routine done, now playing: " + path);
			this._onPlay.Invoke();
			this._movie.Play();
			while (this._movie && this._movie.isPlaying)
			{
				yield return null;
			}
			this.Stop();
			yield break;
		}

		
		
		public bool IsPlaying
		{
			get
			{
				return this._movie && this._movie.isPlaying;
			}
		}

		
		
		public bool IsReading
		{
			get
			{
				return this._streamingFromResources || this.IsPlaying;
			}
		}

		
		public Material _targetMaterial;

		
		public Texture _defaultTexture;

		
		public UnityEvent _onBeginRead;

		
		public UnityEvent _onPlay;

		
		public UnityEvent _onStop;

		
		public UnityEvent _onClear;

		
		private string _lastPlayed;

		
		private bool _streamingFromResources;

		
		private MovieTexture _movie;
	}
}
