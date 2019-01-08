using System;
using UnityEngine;
using UnityEngine.Events;

namespace VolumetricClouds3
{
	public class CloudsEvent : MonoBehaviour
	{
		private void Update()
		{
			this.UpdateMaterialInfo();
			this.CheckEvent();
		}

		private void CheckEvent()
		{
			foreach (CloudsEvent.CloudEvent cloudEvent in this.events)
			{
				if (cloudEvent.monitoredTransform == null)
				{
					Debug.LogError(cloudEvent.eventName + " doesn't have a transform to monitor, fill in the transform or delete the event");
				}
				else
				{
					float num = this.SampleClouds(cloudEvent.monitoredTransform.position);
					if (cloudEvent.IsInside && num < this.eventThreshold)
					{
						cloudEvent.OnExit.Invoke();
						cloudEvent.IsInside = false;
					}
					else if (!cloudEvent.IsInside && num > this.eventThreshold)
					{
						cloudEvent.OnEnter.Invoke();
						cloudEvent.IsInside = true;
					}
				}
			}
		}

		private float CloudCenterBounds()
		{
			return this.matInfo._CloudTransform.x;
		}

		private float CloudUpperBounds()
		{
			return this.CloudCenterBounds() + this.matInfo._CloudTransform.y;
		}

		private float SampleClouds(Vector3 samplePos)
		{
			Vector2 a;
			float value;
			if (this.matInfo._Spherical)
			{
				Vector3 vector = samplePos - this.matInfo._CloudSpherePosition;
				a = new Vector2(vector.x, vector.z) * Mathf.Pow((this.CloudUpperBounds() - Mathf.Abs(vector.y)) / this.CloudUpperBounds(), this.matInfo._SphereHorizonStretch);
				value = Vector3.Distance(samplePos, this.matInfo._CloudSpherePosition + Vector3.Normalize(samplePos - this.matInfo._CloudSpherePosition) * this.CloudCenterBounds()) / this.matInfo._CloudTransform.y;
			}
			else
			{
				a = new Vector2(samplePos.x, samplePos.z);
				value = Mathf.Abs(this.CloudCenterBounds(this.matInfo._CloudTransform) - samplePos.y) / this.matInfo._CloudTransform.y;
			}
			float num = 1f - Mathf.Clamp01(value);
			Vector2 a2 = this.matInfo._Time.y * 0.001f * new Vector2(this.matInfo._WindDirection.x, this.matInfo._WindDirection.z);
			Vector2 a3 = (a + new Vector2(this.matInfo._CloudTransform.z, this.matInfo._CloudTransform.w)) / this.matInfo._Tiling;
			Vector2 vector2 = a3 + a2 * this.matInfo._TimeMult;
			Vector2 vector3 = a3 + a2 * this.matInfo._TimeMultSecondLayer + new Vector2(0f, 0.5f);
			float a4 = this.matInfo._PerlinNormalMap.GetPixelBilinear(vector2.x, vector2.y).a;
			float a5 = this.matInfo._PerlinNormalMap.GetPixelBilinear(vector3.x, vector3.y).a;
			float num2 = (a4 + this.matInfo.density) * num - a5;
			return Mathf.Clamp01(num2 * this.matInfo._Alpha);
		}

		private float CloudCenterBounds(Vector4 _CloudTransform)
		{
			return _CloudTransform.x;
		}

		private void UpdateMaterialInfo()
		{
			if (this.matInfo == null)
			{
				Debug.LogError("Cloud Material is null, fill in the field with a proper material and reactivate the script");
				base.enabled = false;
			}
			this.matInfo._Spherical = (this.cloudMaterial.GetInt("_SphereMapped") == 1);
			this.matInfo._CloudSpherePosition = this.cloudMaterial.GetVector("_CloudSpherePosition");
			this.matInfo._SphereHorizonStretch = this.cloudMaterial.GetFloat("_SphereHorizonStretch");
			this.matInfo._WindDirection = this.cloudMaterial.GetVector("_WindDirection");
			this.matInfo._CloudTransform = this.cloudMaterial.GetVector("_CloudTransform");
			this.matInfo._Tiling = this.cloudMaterial.GetFloat("_Tiling");
			this.matInfo._TimeMult = this.cloudMaterial.GetFloat("_TimeMult");
			this.matInfo._TimeMultSecondLayer = this.cloudMaterial.GetFloat("_TimeMultSecondLayer");
			this.matInfo._Alpha = this.cloudMaterial.GetFloat("_Alpha");
			this.matInfo.density = this.cloudMaterial.GetFloat("_Density");
			float time = Time.time;
			this.matInfo._Time = new Vector4(time / 20f, time, time * 2f, time * 3f);
			this.matInfo._PerlinNormalMap = (Texture2D)this.cloudMaterial.GetTexture("_PerlinNormalMap");
		}

		public Material cloudMaterial;

		[Header("Event")]
		public float eventThreshold = 0.5f;

		public CloudsEvent.CloudEvent[] events = new CloudsEvent.CloudEvent[1];

		[Header("Debug")]
		public bool showDebugCubes = true;

		public int debugCubeCount = 20;

		public float debugCubeSize = 1f;

		public bool showEventTrigger;

		private CloudsEvent.MaterialInfo matInfo = new CloudsEvent.MaterialInfo();

		[Serializable]
		public class CloudEvent
		{
			public string eventName = "EventName";

			public Transform monitoredTransform;

			public bool IsInside;

			public UnityEvent OnEnter;

			public UnityEvent OnExit;
		}

		private class MaterialInfo
		{
			public bool _Spherical;

			public Vector3 _CloudSpherePosition;

			public float _SphereHorizonStretch;

			public Vector4 _WindDirection;

			public Vector4 _CloudTransform;

			public float _Tiling;

			public float _TimeMult;

			public float _TimeMultSecondLayer;

			public float _Alpha;

			public float density;

			public Vector4 _Time;

			public Texture2D _PerlinNormalMap;
		}
	}
}
