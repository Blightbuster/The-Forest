using System;
using System.Collections.Generic;
using ChromaSDK;
using ChromaSDK.ChromaPackage.Model;
using UnityEngine;


[ExecuteInEditMode]
[Serializable]
public class ChromaSDKAnimation2D : ChromaSDKBaseAnimation
{
	
	
	
	public List<EffectArray2dInput> Frames
	{
		get
		{
			int maxRow = ChromaUtils.GetMaxRow(this.Device);
			int maxColumn = ChromaUtils.GetMaxColumn(this.Device);
			if (this._mFrames == null || this._mFrames.Length == 0 || this._mFrames[0] == null || this._mFrames[0].Colors == null || this._mFrames[0].Colors.Length != maxRow || this._mFrames[0].Colors[0].Colors == null || this._mFrames[0].Colors[0].Colors.Length != maxColumn)
			{
				this.ClearFrames();
			}
			List<EffectArray2dInput> list = new List<EffectArray2dInput>();
			for (int i = 0; i < this._mFrames.Length; i++)
			{
				ChromaSDKAnimation2D.ColorFrame2D colorFrame2D = this._mFrames[i];
				ChromaSDKBaseAnimation.ColorArray[] colors = colorFrame2D.Colors;
				EffectArray2dInput effectArray2dInput = new EffectArray2dInput();
				for (int j = 0; j < colors.Length; j++)
				{
					int[] colors2 = colors[j].Colors;
					List<int> list2 = new List<int>();
					foreach (int item in colors2)
					{
						list2.Add(item);
					}
					effectArray2dInput.Add(list2);
				}
				list.Add(effectArray2dInput);
			}
			return list;
		}
		set
		{
			this._mFrames = new ChromaSDKAnimation2D.ColorFrame2D[value.Count];
			for (int i = 0; i < value.Count; i++)
			{
				EffectArray2dInput effectArray2dInput = value[i];
				ChromaSDKAnimation2D.ColorFrame2D colorFrame2D = new ChromaSDKAnimation2D.ColorFrame2D();
				ChromaSDKBaseAnimation.ColorArray[] array = new ChromaSDKBaseAnimation.ColorArray[effectArray2dInput.Count];
				for (int j = 0; j < effectArray2dInput.Count; j++)
				{
					List<int> list = effectArray2dInput[j];
					array[j] = new ChromaSDKBaseAnimation.ColorArray();
					int[] array2 = new int[list.Count];
					for (int k = 0; k < list.Count; k++)
					{
						int num = list[k];
						array2[k] = num;
					}
					array[j].Colors = array2;
				}
				colorFrame2D.Colors = array;
				this._mFrames[i] = colorFrame2D;
			}
		}
	}

	
	public override List<EffectResponseId> GetEffects()
	{
		return this._mEffects;
	}

	
	public void ClearFrames()
	{
		int maxRow = ChromaUtils.GetMaxRow(this.Device);
		int maxColumn = ChromaUtils.GetMaxColumn(this.Device);
		this._mFrames = new ChromaSDKAnimation2D.ColorFrame2D[1];
		ChromaSDKAnimation2D.ColorFrame2D colorFrame2D = new ChromaSDKAnimation2D.ColorFrame2D();
		ChromaSDKBaseAnimation.ColorArray[] array = new ChromaSDKBaseAnimation.ColorArray[maxRow];
		for (int i = 0; i < maxRow; i++)
		{
			array[i] = new ChromaSDKBaseAnimation.ColorArray();
			int[] array2 = new int[maxColumn];
			for (int j = 0; j < maxColumn; j++)
			{
				array2[j] = 0;
			}
			array[i].Colors = array2;
		}
		colorFrame2D.Colors = array;
		this._mFrames[0] = colorFrame2D;
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

	
	public void PlayWithOnComplete(ChromaSDKAnimation2D.ChomaOnComplete2D onComplete)
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
		this._mIsPlaying = false;
		this._mTime = DateTime.MinValue;
		this._mCurrentFrame = 0;
	}

	
	public bool IsPlaying()
	{
		return this._mIsPlaying;
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
				EffectArray2dInput input = this.Frames[i];
				EffectResponseId item = ChromaUtils.CreateEffectCustom2D(this.Device, input);
				this._mEffects.Add(item);
			}
			this._mIsLoaded = true;
		}
	}

	
	public bool IsLoaded()
	{
		return this._mIsLoaded;
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
	public ChromaDevice2DEnum Device;

	
	[SerializeField]
	private ChromaSDKAnimation2D.ColorFrame2D[] _mFrames;

	
	[SerializeField]
	public AnimationCurve Curve = new AnimationCurve();

	
	private ChromaSDKAnimation2D.ChomaOnComplete2D _mOnComplete;

	
	private bool _mIsLoaded;

	
	private bool _mIsPlaying;

	
	private DateTime _mTime = DateTime.MinValue;

	
	private int _mCurrentFrame;

	
	private List<EffectResponseId> _mEffects = new List<EffectResponseId>();

	
	private bool _mIsPaused;

	
	[Serializable]
	public class ColorFrame2D
	{
		
		[SerializeField]
		public ChromaSDKBaseAnimation.ColorArray[] Colors;
	}

	
	
	public delegate void ChomaOnComplete2D(ChromaSDKAnimation2D animation);
}
