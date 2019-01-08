using System;
using System.Collections;
using Bolt;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	public class AnimationSequence : MonoBehaviour
	{
		public AnimationSequenceProxy Proxy { get; set; }

		public bool IsActor
		{
			get
			{
				return this._isActor;
			}
		}

		private void Awake()
		{
			base.StartCoroutine(this.DelayedAwake());
		}

		public void BeginStage(int stage)
		{
			if (this.Proxy)
			{
				BeginAnimationSequenceStage beginAnimationSequenceStage = BeginAnimationSequenceStage.Create(GlobalTargets.OnlyServer);
				beginAnimationSequenceStage.Stage = stage;
				beginAnimationSequenceStage.Actor = LocalPlayer.Entity;
				beginAnimationSequenceStage.Proxy = this.Proxy.entity;
				beginAnimationSequenceStage.Send();
			}
			else
			{
				this.RunStage(stage, 0f, true);
			}
		}

		public void TickProgressStage()
		{
			if (this.Proxy && this._isActor)
			{
				ProgressAnimationSequenceStage progressAnimationSequenceStage = ProgressAnimationSequenceStage.Create(GlobalTargets.OnlyServer);
				progressAnimationSequenceStage.Proxy = this.Proxy.entity;
				progressAnimationSequenceStage.Send();
			}
		}

		public void CompleteStage(int stage)
		{
			if (this._isActor)
			{
				if (this.Proxy)
				{
					if (!this._stages[stage]._completed)
					{
						CompleteAnimationSequenceStage completeAnimationSequenceStage = CompleteAnimationSequenceStage.Create(GlobalTargets.OnlyServer);
						completeAnimationSequenceStage.Stage = stage;
						completeAnimationSequenceStage.Proxy = this.Proxy.entity;
						completeAnimationSequenceStage.Send();
					}
				}
				else
				{
					this.RunStageCompleted(stage);
				}
			}
		}

		public void RunStage(int stage, float elapsedTime, bool isActor)
		{
			this._stage = stage;
			this._progress = 0;
			this._isActor = isActor;
			this._stages[this._stage]._stageStartCallbacks.Invoke();
			if (this._isActor)
			{
				this._stages[this._stage]._stagePlayerActions.Invoke();
			}
			else
			{
				this._stages[this._stage]._spectatorsActions.Invoke();
			}
			this._stages[this._stage]._stageEnvironmentActions.Invoke();
		}

		public void RunStageProgress(int stage)
		{
			this._progress++;
			if (!this._isActor)
			{
				this._stages[stage]._spectatorsProgressActions.Invoke();
			}
		}

		public void RunStageCompletedMp(int stage)
		{
			if (this.Proxy)
			{
				this.Proxy.BeginStage(stage, 0f, null);
				this.Proxy.CompleteStage(stage);
			}
			else
			{
				this.RunStageCompleted(stage);
			}
		}

		public void RunStageCompleted(int stage)
		{
			GlobalDataSaver.SetInt(string.Concat(new object[]
			{
				base.transform.ToGeoHash(),
				"_",
				stage,
				"_completed"
			}), 1);
			for (int i = 0; i <= stage; i++)
			{
				if (!this._stages[i]._completed)
				{
					this._stages[i]._completed = true;
					if (!this._isActor)
					{
						this._stages[i]._spectatorsCompletedActions.Invoke();
					}
				}
			}
			if (this._stage == stage && this._isActor)
			{
				this._isActor = false;
			}
		}

		public static AnimationSequenceProxy GetProxyFor(Vector3 position)
		{
			AnimationSequenceProxy animationSequenceProxy;
			if (BoltNetwork.isRunning)
			{
				animationSequenceProxy = GeoHashHelper<AnimationSequenceProxy>.GetFromHash(GeoHash.ToGeoHash(position), Lookup.Auto);
				if (!animationSequenceProxy)
				{
					if (BoltNetwork.isClient)
					{
						RequestAnimationSequenceProxy requestAnimationSequenceProxy = RequestAnimationSequenceProxy.Create(GlobalTargets.OnlyServer);
						requestAnimationSequenceProxy.Position = position;
						requestAnimationSequenceProxy.Send();
					}
					else
					{
						animationSequenceProxy = UnityEngine.Object.Instantiate<AnimationSequenceProxy>(Prefabs.Instance.AnimationSequenceProxy, position, Quaternion.identity);
					}
				}
				else
				{
					animationSequenceProxy.entity.Freeze(false);
				}
			}
			else
			{
				animationSequenceProxy = null;
			}
			return animationSequenceProxy;
		}

		private IEnumerator DelayedAwake()
		{
			while (!Prefabs.Instance)
			{
				yield return null;
			}
			try
			{
				this.Proxy = AnimationSequence.GetProxyFor(base.transform.position);
				if (this.Proxy)
				{
					this.Proxy.SetSequence(this);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			if (GameSetup.IsSavedGame && !BoltNetwork.isClient)
			{
				while (LevelSerializer.IsDeserializing || (BoltNetwork.isRunning && !this.Proxy))
				{
					yield return null;
				}
				yield return null;
				for (int i = 0; i <= this._stages.Length; i++)
				{
					bool flag = GlobalDataSaver.GetInt(string.Concat(new object[]
					{
						base.transform.ToGeoHash(),
						"_",
						i,
						"_completed"
					}), 0) == 1;
					if (flag)
					{
						this.RunStageCompletedMp(i);
					}
				}
			}
			yield break;
		}

		public AnimationSequence.Stage[] _stages;

		private bool _isActor;

		private int _stage = -1;

		private int _progress = -1;

		[Serializable]
		public class Stage
		{
			[Header("Common")]
			public UnityEvent _stageStartCallbacks;

			public UnityEvent _stageEnvironmentActions;

			[Header("Actor")]
			public UnityEvent _stagePlayerActions;

			[Header("Spectators")]
			public UnityEvent _spectatorsActions;

			public UnityEvent _spectatorsProgressActions;

			public UnityEvent _spectatorsCompletedActions;

			[Header("Status")]
			public bool _completed;
		}
	}
}
