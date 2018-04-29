using System;
using UnityEngine;

namespace ScionEngine
{
	
	public struct DepthOfFieldParameters
	{
		
		public LayerMask depthOfFieldMask;

		
		public bool useMedianFilter;

		
		public DepthOfFieldSamples quality;

		
		public DepthFocusMode depthFocusMode;

		
		public bool visualizeFocalDistance;

		
		public float maxCoCRadius;

		
		public Vector2 pointAveragePosition;

		
		public float pointAverageRange;

		
		public bool visualizePointFocus;

		
		public float depthAdaptionSpeed;

		
		public float focalDistance;

		
		public float focalRange;

		
		public bool useTemporal;

		
		public float temporalBlend;

		
		public int temporalSteps;
	}
}
