using System;
using UnityEngine;

namespace ScionEngine
{
	
	public struct LensFlareParameters
	{
		
		public float threshold;

		
		public LensFlareGhostSamples ghostSamples;

		
		public float ghostIntensity;

		
		public float ghostDispersal;

		
		public float ghostDistortion;

		
		public float ghostEdgeFade;

		
		public float haloIntensity;

		
		public float haloWidth;

		
		public float haloDistortion;

		
		public float starUVScale;

		
		public Texture2D starTexture;

		
		public Texture2D lensColorTexture;

		
		public LensFlareBlurSamples blurSamples;

		
		public float blurStrength;

		
		public int downsamples;
	}
}
