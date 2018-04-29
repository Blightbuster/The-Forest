using System;
using System.Collections;
using Bolt;

namespace TheForest.Utils
{
	
	public class AnimationSequenceProxy : EntityBehaviour<IAnimationSequenceState>
	{
		
		
		
		public AnimationSequence Sequence { get; private set; }

		
		private IEnumerator Start()
		{
			while (!base.entity || !base.entity.isAttached)
			{
				yield return null;
			}
			if (base.entity.isFrozen)
			{
				base.entity.Freeze(false);
			}
			yield break;
		}

		
		public void BeginStage(int stage, float startTime, BoltEntity actor)
		{
			base.entity.Freeze(false);
			base.state.Progress = 0;
			base.state.Completed = false;
			base.state.Stage = stage + 1;
			base.state.StartTime = startTime;
			base.state.Actor = null;
			base.state.Actor = actor;
		}

		
		public void ProgressStage()
		{
			base.entity.Freeze(false);
			base.state.Progress++;
		}

		
		public void CompleteStage(int stage)
		{
			if (base.state.Stage == stage + 1)
			{
				base.state.Completed = true;
			}
		}

		
		public void SetSequence(AnimationSequence sequence)
		{
			if (!this.Sequence && base.entity.isAttached)
			{
				this.Sequence = sequence;
				if (this.Sequence)
				{
					if (base.state.Stage > 0)
					{
						for (int i = 1; i < base.state.Stage; i++)
						{
							this.Sequence.RunStageCompleted(i - 1);
						}
						if (base.state.Completed)
						{
							this.Sequence.RunStageCompleted(base.state.Stage - 1);
						}
						else
						{
							this.Sequence.RunStageProgress(base.state.Stage - 1);
						}
					}
					this.Sequence.Proxy = this;
				}
			}
		}

		
		private void CheckSequence()
		{
			if (!this.Sequence)
			{
				this.SetSequence(GeoHashHelper<AnimationSequence>.GetFromHash(base.transform.ToGeoHash(), Lookup.Auto));
			}
		}

		
		public override void Attached()
		{
			this.CheckSequence();
			base.state.AddCallback("Actor", new PropertyCallbackSimple(this.OnStageUpdated));
			base.state.AddCallback("Progress", new PropertyCallbackSimple(this.OnStageProgress));
			base.state.AddCallback("Completed", new PropertyCallbackSimple(this.OnStageCompleted));
		}

		
		private void OnStageUpdated()
		{
			this.CheckSequence();
			if (this.Sequence && base.state.Stage > 0 && base.state.StartTime > 0f)
			{
				if (base.state.Completed)
				{
					this.OnStageCompleted();
				}
				else
				{
					this.Sequence.RunStage(base.state.Stage - 1, BoltNetwork.serverTime - base.state.StartTime, base.state.Actor == LocalPlayer.Entity);
				}
			}
		}

		
		private void OnStageProgress()
		{
			this.CheckSequence();
			if (this.Sequence)
			{
				this.Sequence.RunStageProgress(base.state.Stage - 1);
			}
		}

		
		private void OnStageCompleted()
		{
			this.CheckSequence();
			if (this.Sequence && base.state.Completed)
			{
				this.Sequence.RunStageCompleted(base.state.Stage - 1);
			}
		}
	}
}
