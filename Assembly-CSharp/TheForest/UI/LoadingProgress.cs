using System;
using UnityEngine;

namespace TheForest.UI
{
	public class LoadingProgress : MonoBehaviour
	{
		public static float Progress { get; set; }

		public static void ResetVisuals()
		{
			LoadingProgress._forceReset = true;
		}

		private void Awake()
		{
			this.Update();
			this._lastProgressStep = LoadingProgress.Progress;
			this._lastTargetProgress = LoadingProgress.Progress;
		}

		private void Update()
		{
			if (LoadingProgress._forceReset)
			{
				this._lastProgressStep = 0f;
				this._lastTargetProgress = 0f;
				LoadingProgress.Progress = 0f;
				LoadingProgress._forceReset = false;
			}
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

		private static bool _forceReset;
	}
}
