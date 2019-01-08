﻿using System;
using UnityEngine;

namespace TheForest.Graphics
{
	[ExecuteInEditMode]
	[AddComponentMenu("The Forest/Graphics/Floating")]
	public class Floating : MonoBehaviour
	{
		private float DeltaTime
		{
			get
			{
				float num = 0f;
				if (Application.isPlaying)
				{
					num = Time.timeSinceLevelLoad;
				}
				if (this.timeSinceLevelLoad == -1f)
				{
					this.timeSinceLevelLoad = num;
				}
				float result = num - this.timeSinceLevelLoad;
				this.timeSinceLevelLoad = num;
				return result;
			}
		}

		private void Update()
		{
			Water water = WaterEngine.WaterAt(base.transform.position);
			if (water)
			{
				Vector3 position = base.transform.position;
				if (this.useHeight)
				{
					float b = water.HeightAt(base.transform.position);
					position.y = Mathf.Lerp(position.y, b, this.DeltaTime / this.smoothTime);
					base.transform.position = position;
				}
				if (this.useNormal)
				{
					base.transform.up = Vector3.Lerp(base.transform.up, water.NormalAt(position), this.DeltaTime / this.smoothTime);
				}
			}
		}

		private void OnDrawGizmos()
		{
			this.Update();
		}

		[Header("Base")]
		[Range(0.01f, 1f)]
		public float smoothTime = 1f;

		public bool useHeight = true;

		public bool useNormal = true;

		private float timeSinceLevelLoad = -1f;
	}
}
