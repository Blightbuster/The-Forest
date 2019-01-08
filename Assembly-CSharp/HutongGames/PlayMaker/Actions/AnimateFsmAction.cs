﻿using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Animate base action - DON'T USE IT!")]
	public abstract class AnimateFsmAction : FsmStateAction
	{
		public override void Reset()
		{
			this.finishEvent = null;
			this.realTime = false;
			this.time = new FsmFloat
			{
				UseVariable = true
			};
			this.speed = new FsmFloat
			{
				UseVariable = true
			};
			this.delay = new FsmFloat
			{
				UseVariable = true
			};
			this.ignoreCurveOffset = new FsmBool
			{
				Value = true
			};
			this.resultFloats = new float[0];
			this.fromFloats = new float[0];
			this.toFloats = new float[0];
			this.endTimes = new float[0];
			this.keyOffsets = new float[0];
			this.curves = new AnimationCurve[0];
			this.finishAction = false;
			this.start = false;
		}

		public override void OnEnter()
		{
			this.startTime = FsmTime.RealtimeSinceStartup;
			this.lastTime = FsmTime.RealtimeSinceStartup - this.startTime;
			this.deltaTime = 0f;
			this.currentTime = 0f;
			this.isRunning = false;
			this.finishAction = false;
			this.looping = false;
			this.delayTime = ((!this.delay.IsNone) ? (this.delayTime = this.delay.Value) : 0f);
			this.start = true;
		}

		protected void Init()
		{
			this.endTimes = new float[this.curves.Length];
			this.keyOffsets = new float[this.curves.Length];
			this.largestEndTime = 0f;
			for (int i = 0; i < this.curves.Length; i++)
			{
				if (this.curves[i] != null && this.curves[i].keys.Length > 0)
				{
					this.keyOffsets[i] = ((this.curves[i].keys.Length <= 0) ? 0f : ((!this.time.IsNone) ? (this.time.Value / this.curves[i].keys[this.curves[i].length - 1].time * this.curves[i].keys[0].time) : this.curves[i].keys[0].time));
					this.currentTime = ((!this.ignoreCurveOffset.IsNone) ? ((!this.ignoreCurveOffset.Value) ? 0f : this.keyOffsets[i]) : 0f);
					if (!this.time.IsNone)
					{
						this.endTimes[i] = this.time.Value;
					}
					else
					{
						this.endTimes[i] = this.curves[i].keys[this.curves[i].length - 1].time;
					}
					if (this.largestEndTime < this.endTimes[i])
					{
						this.largestEndTime = this.endTimes[i];
					}
					if (!this.looping)
					{
						this.looping = ActionHelpers.IsLoopingWrapMode(this.curves[i].postWrapMode);
					}
				}
				else
				{
					this.endTimes[i] = -1f;
				}
			}
			for (int j = 0; j < this.curves.Length; j++)
			{
				if (this.largestEndTime > 0f && this.endTimes[j] == -1f)
				{
					this.endTimes[j] = this.largestEndTime;
				}
				else if (this.largestEndTime == 0f && this.endTimes[j] == -1f)
				{
					if (this.time.IsNone)
					{
						this.endTimes[j] = 1f;
					}
					else
					{
						this.endTimes[j] = this.time.Value;
					}
				}
			}
		}

		public override void OnUpdate()
		{
			if (!this.isRunning && this.start)
			{
				if (this.delayTime >= 0f)
				{
					if (this.realTime)
					{
						this.deltaTime = FsmTime.RealtimeSinceStartup - this.startTime - this.lastTime;
						this.lastTime = FsmTime.RealtimeSinceStartup - this.startTime;
						this.delayTime -= this.deltaTime;
					}
					else
					{
						this.delayTime -= Time.deltaTime;
					}
				}
				else
				{
					this.isRunning = true;
					this.start = false;
				}
			}
			if (this.isRunning)
			{
				if (this.realTime)
				{
					this.deltaTime = FsmTime.RealtimeSinceStartup - this.startTime - this.lastTime;
					this.lastTime = FsmTime.RealtimeSinceStartup - this.startTime;
					if (!this.speed.IsNone)
					{
						this.currentTime += this.deltaTime * this.speed.Value;
					}
					else
					{
						this.currentTime += this.deltaTime;
					}
				}
				else if (!this.speed.IsNone)
				{
					this.currentTime += Time.deltaTime * this.speed.Value;
				}
				else
				{
					this.currentTime += Time.deltaTime;
				}
				for (int i = 0; i < this.curves.Length; i++)
				{
					if (this.curves[i] != null && this.curves[i].keys.Length > 0)
					{
						if (this.calculations[i] != AnimateFsmAction.Calculation.None)
						{
							switch (this.calculations[i])
							{
							case AnimateFsmAction.Calculation.SetValue:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time);
								}
								else
								{
									this.resultFloats[i] = this.curves[i].Evaluate(this.currentTime);
								}
								break;
							case AnimateFsmAction.Calculation.AddToValue:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = this.fromFloats[i] + this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time);
								}
								else
								{
									this.resultFloats[i] = this.fromFloats[i] + this.curves[i].Evaluate(this.currentTime);
								}
								break;
							case AnimateFsmAction.Calculation.SubtractFromValue:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = this.fromFloats[i] - this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time);
								}
								else
								{
									this.resultFloats[i] = this.fromFloats[i] - this.curves[i].Evaluate(this.currentTime);
								}
								break;
							case AnimateFsmAction.Calculation.SubtractValueFromCurve:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time) - this.fromFloats[i];
								}
								else
								{
									this.resultFloats[i] = this.curves[i].Evaluate(this.currentTime) - this.fromFloats[i];
								}
								break;
							case AnimateFsmAction.Calculation.MultiplyValue:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time) * this.fromFloats[i];
								}
								else
								{
									this.resultFloats[i] = this.curves[i].Evaluate(this.currentTime) * this.fromFloats[i];
								}
								break;
							case AnimateFsmAction.Calculation.DivideValue:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = ((this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time) == 0f) ? float.MaxValue : (this.fromFloats[i] / this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time)));
								}
								else
								{
									this.resultFloats[i] = ((this.curves[i].Evaluate(this.currentTime) == 0f) ? float.MaxValue : (this.fromFloats[i] / this.curves[i].Evaluate(this.currentTime)));
								}
								break;
							case AnimateFsmAction.Calculation.DivideCurveByValue:
								if (!this.time.IsNone)
								{
									this.resultFloats[i] = ((this.fromFloats[i] == 0f) ? float.MaxValue : (this.curves[i].Evaluate(this.currentTime / this.time.Value * this.curves[i].keys[this.curves[i].length - 1].time) / this.fromFloats[i]));
								}
								else
								{
									this.resultFloats[i] = ((this.fromFloats[i] == 0f) ? float.MaxValue : (this.curves[i].Evaluate(this.currentTime) / this.fromFloats[i]));
								}
								break;
							}
						}
						else
						{
							this.resultFloats[i] = this.fromFloats[i];
						}
					}
					else
					{
						this.resultFloats[i] = this.fromFloats[i];
					}
				}
				if (this.isRunning && !this.looping)
				{
					this.finishAction = true;
					for (int j = 0; j < this.endTimes.Length; j++)
					{
						if (this.currentTime < this.endTimes[j])
						{
							this.finishAction = false;
						}
					}
					this.isRunning = !this.finishAction;
				}
			}
		}

		[Tooltip("Define time to use your curve scaled to be stretched or shrinked.")]
		public FsmFloat time;

		[Tooltip("If you define speed, your animation will be speeded up or slowed down.")]
		public FsmFloat speed;

		[Tooltip("Delayed animimation start.")]
		public FsmFloat delay;

		[Tooltip("Animation curve start from any time. If IgnoreCurveOffset is true the animation starts right after the state become entered.")]
		public FsmBool ignoreCurveOffset;

		[Tooltip("Optionally send an Event when the animation finishes.")]
		public FsmEvent finishEvent;

		[Tooltip("Ignore TimeScale. Useful if the game is paused.")]
		public bool realTime;

		private float startTime;

		private float currentTime;

		private float[] endTimes;

		private float lastTime;

		private float deltaTime;

		private float delayTime;

		private float[] keyOffsets;

		protected AnimationCurve[] curves;

		protected AnimateFsmAction.Calculation[] calculations;

		protected float[] resultFloats;

		protected float[] fromFloats;

		protected float[] toFloats;

		protected bool finishAction;

		protected bool isRunning;

		protected bool looping;

		private bool start;

		private float largestEndTime;

		public enum Calculation
		{
			None,
			SetValue,
			AddToValue,
			SubtractFromValue,
			SubtractValueFromCurve,
			MultiplyValue,
			DivideValue,
			DivideCurveByValue
		}
	}
}
