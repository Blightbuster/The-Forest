using System;
using UnityEngine;

namespace ScionEngine
{
	
	public class Bloom
	{
		
		public Bloom()
		{
			this.m_bloomMat = new Material(Shader.Find("Hidden/ScionBloom"));
			this.m_bloomMat.hideFlags = HideFlags.HideAndDontSave;
		}

		
		public void ReleaseResources()
		{
			if (this.m_bloomMat != null)
			{
				UnityEngine.Object.Destroy(this.m_bloomMat);
				this.m_bloomMat = null;
			}
		}

		
		public bool PlatformCompatibility()
		{
			return Shader.Find("Hidden/ScionBloom").isSupported;
		}

		
		public RenderTexture TryGetSmallGlareTexture(int minimumReqPixels, out int numSearches)
		{
			numSearches = 1;
			for (int i = this.numDownsamples - 1; i >= 0; i--)
			{
				int num = (this.m_glareTextures[i].width <= this.m_glareTextures[i].height) ? this.m_glareTextures[i].width : this.m_glareTextures[i].height;
				if (num >= minimumReqPixels)
				{
					return this.m_glareTextures[i];
				}
				numSearches++;
			}
			return null;
		}

		
		public float GetEnergyNormalizer(int forNumDownsamples)
		{
			float num = 1f;
			for (int i = forNumDownsamples - 1; i > 0; i--)
			{
				num = num * this.distanceMultiplier + 1f;
			}
			return 1f / num;
		}

		
		public void EndOfFrameCleanup()
		{
			if (this.m_glareTextures == null)
			{
				return;
			}
			for (int i = 0; i < this.numDownsamples; i++)
			{
				RenderTexture.ReleaseTemporary(this.m_glareTextures[i]);
				this.m_glareTextures[i] = null;
			}
		}

		
		public RenderTexture GetGlareTexture(int downsampleIndex)
		{
			return this.m_glareTextures[downsampleIndex];
		}

		
		public void RunUpsamplingChain(RenderTexture halfResSource)
		{
			for (int i = this.numDownsamples - 1; i > 1; i--)
			{
				Graphics.Blit(this.m_glareTextures[i], this.m_glareTextures[i - 1], this.m_bloomMat, 1);
			}
			this.m_bloomMat.SetTexture("_HalfResSource", halfResSource);
			Graphics.Blit(this.m_glareTextures[1], this.m_glareTextures[0], this.m_bloomMat, 2);
		}

		
		public void RunDownsamplingChain(RenderTexture halfResSource, int numDownsamples, float distanceMultiplier)
		{
			this.distanceMultiplier = distanceMultiplier;
			if (numDownsamples != this.numDownsamples)
			{
				this.numDownsamples = numDownsamples;
				this.m_glareTextures = new RenderTexture[numDownsamples];
			}
			halfResSource.filterMode = FilterMode.Bilinear;
			RenderTextureFormat format = halfResSource.format;
			int num = halfResSource.width;
			int num2 = halfResSource.height;
			for (int i = 0; i < numDownsamples; i++)
			{
				this.m_glareTextures[i] = RenderTexture.GetTemporary(num, num2, 0, format);
				this.m_glareTextures[i].filterMode = FilterMode.Bilinear;
				this.m_glareTextures[i].wrapMode = TextureWrapMode.Clamp;
				num /= 2;
				num2 /= 2;
			}
			halfResSource.filterMode = FilterMode.Bilinear;
			RenderTexture source = halfResSource;
			this.m_bloomMat.SetFloat("_GlareDistanceMultiplier", distanceMultiplier);
			for (int j = 1; j < numDownsamples; j++)
			{
				Graphics.Blit(source, this.m_glareTextures[j], this.m_bloomMat, 0);
				source = this.m_glareTextures[j];
			}
		}

		
		private Material m_bloomMat;

		
		private RenderTexture[] m_glareTextures;

		
		private int numDownsamples = -1;

		
		private float distanceMultiplier;
	}
}
