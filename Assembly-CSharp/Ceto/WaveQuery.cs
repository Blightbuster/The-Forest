﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ceto
{
	public class WaveQuery
	{
		public WaveQuery()
		{
			this.posX = 0f;
			this.posZ = 0f;
			this.minError = 0.1f;
			bool[] array = new bool[4];
			array[0] = true;
			array[1] = true;
			this.sampleSpectrum = array;
			this.sampleOverlay = true;
			this.mode = QUERY_MODE.POSITION;
			this.result.displacement = new Vector3[4];
		}

		public WaveQuery(Vector3 worldPos)
		{
			this.posX = worldPos.x;
			this.posZ = worldPos.z;
			this.minError = 0.1f;
			bool[] array = new bool[4];
			array[0] = true;
			array[1] = true;
			this.sampleSpectrum = array;
			this.sampleOverlay = true;
			this.mode = QUERY_MODE.POSITION;
			this.result.displacement = new Vector3[4];
		}

		public WaveQuery(float x, float z)
		{
			this.posX = x;
			this.posZ = z;
			this.minError = 0.1f;
			bool[] array = new bool[4];
			array[0] = true;
			array[1] = true;
			this.sampleSpectrum = array;
			this.sampleOverlay = true;
			this.mode = QUERY_MODE.POSITION;
			this.result.displacement = new Vector3[4];
		}

		public static readonly float MIN_ERROR = 0.01f;

		public static readonly int MAX_ITERATIONS = 20;

		public float minError;

		public float posX;

		public float posZ;

		public readonly bool[] sampleSpectrum;

		public bool sampleOverlay;

		public bool overrideIgnoreQuerys;

		public QUERY_MODE mode;

		public int tag;

		public WaveQuery.WaveQueryResult result;

		public struct WaveQueryResult
		{
			public void Clear()
			{
				this.height = 0f;
				this.overlayHeight = 0f;
				this.displacementX = 0f;
				this.displacementZ = 0f;
				this.iterations = 0;
				this.error = 0f;
				this.isClipped = false;
				this.overlays = null;
				this.displacement[0].x = 0f;
				this.displacement[0].y = 0f;
				this.displacement[0].z = 0f;
				this.displacement[1].x = 0f;
				this.displacement[1].y = 0f;
				this.displacement[1].z = 0f;
				this.displacement[2].x = 0f;
				this.displacement[2].y = 0f;
				this.displacement[2].z = 0f;
				this.displacement[3].x = 0f;
				this.displacement[3].y = 0f;
				this.displacement[3].z = 0f;
			}

			public float height;

			public float displacementX;

			public float displacementZ;

			public float overlayHeight;

			public Vector3[] displacement;

			public int iterations;

			public float error;

			public bool isClipped;

			public IEnumerable<QueryableOverlayResult> overlays;
		}
	}
}
