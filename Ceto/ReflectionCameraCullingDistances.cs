using System;
using UnityEngine;

namespace Ceto
{
	
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Ceto/Camera/ReflectionCameraCullingDistances")]
	public class ReflectionCameraCullingDistances : MonoBehaviour
	{
		
		private void Start()
		{
			this.m_camera = base.GetComponent<Camera>();
		}

		
		private void Update()
		{
			if (Ocean.Instance == null || this.distances.Length != 32)
			{
				return;
			}
			CameraData cameraData = Ocean.Instance.FindCameraData(this.m_camera);
			if (cameraData.reflection == null)
			{
				return;
			}
			Camera cam = cameraData.reflection.cam;
			cam.layerCullDistances = this.distances;
			cam.layerCullSpherical = this.sphericalCulling;
		}

		
		public bool sphericalCulling = true;

		
		public float[] distances = new float[32];

		
		private Camera m_camera;
	}
}
