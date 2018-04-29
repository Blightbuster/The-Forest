using System;
using System.Collections.Generic;
using ChromaSDK;
using ChromaSDK.ChromaPackage.Model;
using UnityEngine;


[ExecuteInEditMode]
public class ChromaSDKAnimation1D : ChromaSDKBaseAnimation
{
	
	
	
	public List<EffectArray1dInput> Frames
	{
		get
		{
			int maxLeds = ChromaUtils.GetMaxLeds(this.Device);
			if (this._mFrames == null || this._mFrames.Length == 0 || this._mFrames[0] == null || this._mFrames[0].Colors == null || this._mFrames[0].Colors.Length != maxLeds)
			{
				this.ClearFrames();
			}
			List<EffectArray1dInput> list = new List<EffectArray1dInput>();
			for (int i = 0; i < this._mFrames.Length; i++)
			{
				ChromaSDKBaseAnimation.ColorArray colorArray = this._mFrames[i];
				int[] colors = colorArray.Colors;
				EffectArray1dInput effectArray1dInput = new EffectArray1dInput();
				foreach (int value in colors)
				{
					effectArray1dInput.Add(new int?(value));
				}
				list.Add(effectArray1dInput);
			}
			return list;
		}
		set
		{
			this._mFrames = new ChromaSDKBaseAnimation.ColorArray[value.Count];
			for (int i = 0; i < value.Count; i++)
			{
				EffectArray1dInput effectArray1dInput = value[i];
				ChromaSDKBaseAnimation.ColorArray colorArray = new ChromaSDKBaseAnimation.ColorArray();
				int[] array = new int[effectArray1dInput.Count];
				for (int j = 0; j < effectArray1dInput.Count; j++)
				{
					int value2 = effectArray1dInput[j].Value;
					array[j] = value2;
				}
				colorArray.Colors = array;
				this._mFrames[i] = colorArray;
			}
		}
	}

	
	public override List<EffectResponseId> GetEffects()
	{
		return this._mEffects;
	}

	
	public void ClearFrames()
	{
		int maxLeds = ChromaUtils.GetMaxLeds(this.Device);
		this._mFrames = new ChromaSDKBaseAnimation.ColorArray[1];
		ChromaSDKBaseAnimation.ColorArray colorArray = new ChromaSDKBaseAnimation.ColorArray();
		int[] array = new int[maxLeds];
		for (int i = 0; i < maxLeds; i++)
		{
			array[i] = 0;
		}
		colorArray.Colors = array;
		this._mFrames[0] = colorArray;
	}

	
	public void Play()
	{
		if (!this._mIsLoaded)
		{
			Debug.LogError("Play Animation has not been loaded!");
			return;
		}
		this._mOnComplete = null;
		this._mTime = DateTime.Now;
		this._mIsPlaying = true;
		this._mCurrentFrame = 0;
		if (this._mCurrentFrame < this._mEffects.Count && !this._mIsPaused && ChromaConnectionManager.Instance.Connected)
		{
			EffectResponseId effectResponseId = this._mEffects[this._mCurrentFrame];
			if (effectResponseId != null)
			{
				ChromaUtils.SetEffect(effectResponseId.Id);
			}
		}
	}

	
	public void PlayWithOnComplete(ChromaSDKAnimation1D.ChomaOnComplete1D onComplete)
	{
		if (!this._mIsLoaded)
		{
			Debug.LogError("Animation has not been loaded!");
			return;
		}
		this._mOnComplete = onComplete;
		this._mTime = DateTime.Now;
		this._mIsPlaying = true;
		this._mCurrentFrame = 0;
		if (this._mCurrentFrame < this._mEffects.Count && !this._mIsPaused && ChromaConnectionManager.Instance.Connected)
		{
			EffectResponseId effectResponseId = this._mEffects[this._mCurrentFrame];
			if (effectResponseId != null)
			{
				ChromaUtils.SetEffect(effectResponseId.Id);
			}
		}
	}

	
	public void Stop()
	{
		Debug.Log("Stop");
		this._mIsPlaying = false;
		this._mTime = DateTime.MinValue;
		this._mCurrentFrame = 0;
	}

	
	public bool IsPlaying()
	{
		return this._mIsPlaying;
	}

	
	public void Load()
	{
		if (this._mIsLoaded)
		{
			Debug.LogError("Animation has already been loaded!");
			return;
		}
		if (ChromaConnectionManager.Instance.Connected)
		{
			for (int i = 0; i < this.Frames.Count; i++)
			{
				EffectArray1dInput input = this.Frames[i];
				EffectResponseId item = ChromaUtils.CreateEffectCustom1D(this.Device, input);
				this._mEffects.Add(item);
			}
			this._mIsLoaded = true;
		}
	}

	
	public bool IsLoaded()
	{
		return this._mIsLoaded;
	}

	
	public void Reset()
	{
		this._mOnComplete = null;
		this._mIsLoaded = false;
		this._mIsPlaying = false;
		this._mTime = DateTime.MinValue;
		this._mCurrentFrame = 0;
		this._mEffects.Clear();
	}

	
	public void Unload()
	{
		if (!this._mIsLoaded)
		{
			Debug.LogError("Animation has already been unloaded!");
			return;
		}
		for (int i = 0; i < this._mEffects.Count; i++)
		{
			EffectResponseId effectResponseId = this._mEffects[i];
			if (effectResponseId != null)
			{
				ChromaUtils.RemoveEffect(effectResponseId.Id);
			}
		}
		this._mEffects.Clear();
		this._mIsLoaded = false;
	}

	
	private float GetTime(int index)
	{
		if (index >= 0 && index < this.Curve.keys.Length)
		{
			return this.Curve.keys[index].time;
		}
		return 0.033f;
	}

	
	private void OnApplicationPause(bool pause)
	{
		if (Application.isPlaying)
		{
			this._mIsPaused = pause;
		}
		else
		{
			this._mIsPaused = false;
		}
	}

	
	public override void Update()
	{
		base.Update();
		if (this._mTime == DateTime.MinValue)
		{
			return;
		}
		float num = (float)(DateTime.Now - this._mTime).TotalSeconds;
		float time = this.GetTime(this._mCurrentFrame);
		if (time < num)
		{
			this._mCurrentFrame++;
			if (this._mCurrentFrame < this._mEffects.Count)
			{
				if (!this._mIsPaused && ChromaConnectionManager.Instance.Connected)
				{
					EffectResponseId effectResponseId = this._mEffects[this._mCurrentFrame];
					if (effectResponseId != null)
					{
						ChromaUtils.SetEffect(effectResponseId.Id);
					}
				}
			}
			else
			{
				this._mIsPlaying = false;
				this._mTime = DateTime.MinValue;
				this._mCurrentFrame = 0;
				if (this._mOnComplete != null)
				{
					this._mOnComplete(this);
				}
			}
		}
	}

	
	public void RefreshCurve()
	{
		List<float> list = new List<float>();
		for (int i = 0; i < this.Curve.keys.Length; i++)
		{
			Keyframe keyframe = this.Curve.keys[i];
			float num = keyframe.time;
			if (num <= 0f)
			{
				num = 0.033f;
			}
			list.Add(num);
		}
		while (list.Count > this.Frames.Count)
		{
			int index = list.Count - 1;
			list.RemoveAt(index);
		}
		while (list.Count < this.Frames.Count)
		{
			if (list.Count == 0)
			{
				list.Add(1f);
			}
			else
			{
				int index2 = list.Count - 1;
				float item = list[index2] + 1f;
				list.Add(item);
			}
		}
		while (this.Curve.keys.Length > 0)
		{
			this.Curve.RemoveKey(0);
		}
		for (int j = 0; j < list.Count; j++)
		{
			float time = list[j];
			this.Curve.AddKey(time, 0f);
		}
	}

	
	[SerializeField]
	public ChromaDevice1DEnum Device;

	
	[SerializeField]
	private ChromaSDKBaseAnimation.ColorArray[] _mFrames;

	
	[SerializeField]
	public AnimationCurve Curve = new AnimationCurve();

	
	private ChromaSDKAnimation1D.ChomaOnComplete1D _mOnComplete;

	
	private bool _mIsLoaded;

	
	private bool _mIsPlaying;

	
	private DateTime _mTime = DateTime.MinValue;

	
	private int _mCurrentFrame;

	
	private List<EffectResponseId> _mEffects = new List<EffectResponseId>();

	
	private bool _mIsPaused;

	
	
	public delegate void ChomaOnComplete1D(ChromaSDKAnimation1D animation);
}
