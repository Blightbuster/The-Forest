using System;
using UnityEngine;


public class OVRHapticsClip
{
	
	public OVRHapticsClip()
	{
		this.Capacity = OVRHaptics.Config.MaximumBufferSamplesCount;
		this.Samples = new byte[this.Capacity * OVRHaptics.Config.SampleSizeInBytes];
	}

	
	public OVRHapticsClip(int capacity)
	{
		this.Capacity = ((capacity < 0) ? 0 : capacity);
		this.Samples = new byte[this.Capacity * OVRHaptics.Config.SampleSizeInBytes];
	}

	
	public OVRHapticsClip(byte[] samples, int samplesCount)
	{
		this.Samples = samples;
		this.Capacity = this.Samples.Length / OVRHaptics.Config.SampleSizeInBytes;
		this.Count = ((samplesCount < 0) ? 0 : samplesCount);
	}

	
	public OVRHapticsClip(OVRHapticsClip a, OVRHapticsClip b)
	{
		int count = a.Count;
		if (b.Count > count)
		{
			count = b.Count;
		}
		this.Capacity = count;
		this.Samples = new byte[this.Capacity * OVRHaptics.Config.SampleSizeInBytes];
		int num = 0;
		while (num < a.Count || num < b.Count)
		{
			if (OVRHaptics.Config.SampleSizeInBytes == 1)
			{
				byte sample = 0;
				if (num < a.Count && num < b.Count)
				{
					sample = (byte)Mathf.Clamp((int)(a.Samples[num] + b.Samples[num]), 0, 255);
				}
				else if (num < a.Count)
				{
					sample = a.Samples[num];
				}
				else if (num < b.Count)
				{
					sample = b.Samples[num];
				}
				this.WriteSample(sample);
			}
			num++;
		}
	}

	
	public OVRHapticsClip(AudioClip audioClip, int channel = 0)
	{
		float[] array = new float[audioClip.samples * audioClip.channels];
		audioClip.GetData(array, 0);
		this.InitializeFromAudioFloatTrack(array, (double)audioClip.frequency, audioClip.channels, channel);
	}

	
	
	
	public int Count { get; private set; }

	
	
	
	public int Capacity { get; private set; }

	
	
	
	public byte[] Samples { get; private set; }

	
	public void WriteSample(byte sample)
	{
		if (this.Count >= this.Capacity)
		{
			return;
		}
		if (OVRHaptics.Config.SampleSizeInBytes == 1)
		{
			this.Samples[this.Count * OVRHaptics.Config.SampleSizeInBytes] = sample;
		}
		this.Count++;
	}

	
	public void Reset()
	{
		this.Count = 0;
	}

	
	private void InitializeFromAudioFloatTrack(float[] sourceData, double sourceFrequency, int sourceChannelCount, int sourceChannel)
	{
		double num = (sourceFrequency + 1E-06) / (double)OVRHaptics.Config.SampleRateHz;
		if (num < 1.0)
		{
			return;
		}
		int num2 = (int)num;
		double num3 = num - (double)num2;
		double num4 = 0.0;
		int num5 = sourceData.Length;
		this.Count = 0;
		this.Capacity = num5 / sourceChannelCount / num2 + 1;
		this.Samples = new byte[this.Capacity * OVRHaptics.Config.SampleSizeInBytes];
		int i = sourceChannel % sourceChannelCount;
		while (i < num5)
		{
			if (OVRHaptics.Config.SampleSizeInBytes == 1)
			{
				this.WriteSample((byte)(Mathf.Clamp01(Mathf.Abs(sourceData[i])) * 255f));
			}
			i += num2 * sourceChannelCount;
			num4 += num3;
			if ((int)num4 > 0)
			{
				i += (int)num4 * sourceChannelCount;
				num4 -= (double)((int)num4);
			}
		}
	}
}
