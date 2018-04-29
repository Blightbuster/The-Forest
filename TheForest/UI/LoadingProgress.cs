using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class LoadingProgress : MonoBehaviour
	{
		
		private void Awake()
		{
			this._lastProgressStep = LoadingProgress.Progress;
			this._lastTargetProgress = LoadingProgress.Progress;
			this.Update();
		}

		
		private void Update()
		{
			LoadingProgress.Progress = Mathf.Clamp01(LoadingProgress.Progress);
			this._alpha += Time.fixedDeltaTime * 3f;
			float num = Mathf.Lerp(this._lastProgressStep, LoadingProgress.Progress, this._alpha);
			if (LevelSerializer.LevelLoadingOperation != null)
			{
				LoadingProgress.Progress = LevelSerializer.LevelLoadingOperation.progress * 0.5f;
			}
			if (!Mathf.Approximately(this._lastTargetProgress, LoadingProgress.Progress))
			{
				this._alpha = 0f;
				this._lastProgressStep = num;
				this._lastTargetProgress = LoadingProgress.Progress;
			}
			if (Mathf.Approximately(num, LoadingProgress.Progress))
			{
				this._lastProgressStep = LoadingProgress.Progress;
				this._alpha = 0f;
			}
			this._progressBar.localPosition = Vector3.Lerp(this._from, this._to, num);
		}

		
		public Transform _progressBar;

		
		public Vector3 _from;

		
		public Vector3 _to;

		
		private float _lastProgressStep;

		
		private float _lastTargetProgress;

		
		private float _alpha;

		
		public static float Progress;
	}
}
