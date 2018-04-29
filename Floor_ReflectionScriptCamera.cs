using System;
using System.Collections.Generic;
using UnityEngine;


public class Floor_ReflectionScriptCamera : MonoBehaviour
{
	
	public void Start()
	{
		this.initialReflectionTextures = new Texture2D[this.reflectiveMaterials.Length];
		for (int i = 0; i < this.reflectiveMaterials.Length; i++)
		{
			this.initialReflectionTextures[i] = this.reflectiveMaterials[i].GetTexture(this.reflectionSampler);
		}
	}

	
	public void OnDisable()
	{
		if (this.initialReflectionTextures == null)
		{
			return;
		}
		for (int i = 0; i < this.reflectiveMaterials.Length; i++)
		{
			this.reflectiveMaterials[i].SetTexture(this.reflectionSampler, this.initialReflectionTextures[i]);
		}
	}

	
	private Camera CreateReflectionCameraFor(Camera cam)
	{
		string text = base.gameObject.name + "Reflection" + cam.name;
		Debug.Log("AngryBots: created internal reflection camera " + text);
		GameObject gameObject = GameObject.Find(text);
		if (!gameObject)
		{
			gameObject = new GameObject(text, new Type[]
			{
				typeof(Camera)
			});
		}
		if (!gameObject.GetComponent(typeof(Camera)))
		{
			gameObject.AddComponent(typeof(Camera));
		}
		Camera component = gameObject.GetComponent<Camera>();
		component.backgroundColor = this.clearColor;
		component.clearFlags = CameraClearFlags.Color;
		this.SetStandardCameraParameter(component, this.reflectionMask);
		if (!component.targetTexture)
		{
			component.targetTexture = this.CreateTextureFor(cam);
		}
		return component;
	}

	
	public void HighQuality()
	{
		this.highQuality = true;
	}

	
	private void SetStandardCameraParameter(Camera cam, LayerMask mask)
	{
		cam.backgroundColor = Color.black;
		cam.enabled = false;
		cam.cullingMask = this.reflectionMask;
	}

	
	private RenderTexture CreateTextureFor(Camera cam)
	{
		RenderTextureFormat format = RenderTextureFormat.RGB565;
		if (!SystemInfo.SupportsRenderTextureFormat(format))
		{
			format = RenderTextureFormat.Default;
		}
		float num = (!this.highQuality) ? 0.5f : 0.75f;
		return new RenderTexture(Mathf.FloorToInt((float)cam.pixelWidth * num), Mathf.FloorToInt((float)cam.pixelHeight * num), 24, format)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	
	public void RenderHelpCameras(Camera currentCam)
	{
		if (this.helperCameras == null)
		{
			this.helperCameras = new Dictionary<Camera, bool>();
		}
		if (!this.helperCameras.ContainsKey(currentCam))
		{
			this.helperCameras.Add(currentCam, false);
		}
		if (this.helperCameras[currentCam])
		{
			return;
		}
		if (!this.reflectionCamera)
		{
			this.reflectionCamera = this.CreateReflectionCameraFor(currentCam);
			foreach (Material material in this.reflectiveMaterials)
			{
				material.SetTexture(this.reflectionSampler, this.reflectionCamera.targetTexture);
			}
		}
		this.RenderReflectionFor(currentCam, this.reflectionCamera);
		this.helperCameras[currentCam] = true;
	}

	
	public void LateUpdate()
	{
		Transform transform = null;
		float num = float.PositiveInfinity;
		Vector3 position = Camera.main.transform.position;
		foreach (Transform transform2 in this.reflectiveObjects)
		{
			if (transform2.GetComponent<Renderer>().isVisible)
			{
				float sqrMagnitude = (position - transform2.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					transform = transform2;
				}
			}
		}
		if (!transform)
		{
			return;
		}
		this.ObjectBeingRendered(transform, Camera.main);
		if (this.helperCameras != null)
		{
			this.helperCameras.Clear();
		}
	}

	
	private void ObjectBeingRendered(Transform tr, Camera currentCam)
	{
		if (null == tr)
		{
			return;
		}
		this.reflectiveSurfaceHeight = tr;
		this.RenderHelpCameras(currentCam);
	}

	
	private void RenderReflectionFor(Camera cam, Camera reflectCamera)
	{
		if (!reflectCamera)
		{
			return;
		}
		this.SaneCameraSettings(reflectCamera);
		reflectCamera.backgroundColor = this.clearColor;
		GL.invertCulling = false;
		Transform transform = this.reflectiveSurfaceHeight;
		Vector3 eulerAngles = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(-eulerAngles.x, eulerAngles.y, eulerAngles.z);
		reflectCamera.transform.position = cam.transform.position;
		Vector3 position = transform.transform.position;
		position.y = transform.position.y;
		Vector3 up = transform.transform.up;
		float w = -Vector3.Dot(up, position) - this.clipPlaneOffset;
		Vector4 plane = new Vector4(up.x, up.y, up.z, w);
		Matrix4x4 matrix4x = Matrix4x4.zero;
		matrix4x = Floor_ReflectionScriptCamera.CalculateReflectionMatrix(matrix4x, plane);
		this.oldpos = cam.transform.position;
		Vector3 position2 = matrix4x.MultiplyPoint(this.oldpos);
		reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * matrix4x;
		Vector4 clipPlane = this.CameraSpacePlane(reflectCamera, position, up, 1f);
		Matrix4x4 matrix4x2 = cam.projectionMatrix;
		matrix4x2 = Floor_ReflectionScriptCamera.CalculateObliqueMatrix(matrix4x2, clipPlane);
		reflectCamera.projectionMatrix = matrix4x2;
		reflectCamera.transform.position = position2;
		Vector3 eulerAngles2 = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(-eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
		reflectCamera.RenderWithShader(this.replacementShader, "Reflection");
		GL.invertCulling = false;
	}

	
	private void SaneCameraSettings(Camera helperCam)
	{
		helperCam.depthTextureMode = DepthTextureMode.None;
		helperCam.backgroundColor = Color.black;
		helperCam.clearFlags = CameraClearFlags.Color;
		helperCam.renderingPath = RenderingPath.Forward;
	}

	
	private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(Floor_ReflectionScriptCamera.sgn(clipPlane.x), Floor_ReflectionScriptCamera.sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
		return projection;
	}

	
	private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
		return reflectionMat;
	}

	
	private static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	
	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 v = pos + normal * this.clipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(v);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, -Vector3.Dot(lhs, rhs));
	}

	
	public Transform[] reflectiveObjects;

	
	public LayerMask reflectionMask;

	
	public Material[] reflectiveMaterials;

	
	private Transform reflectiveSurfaceHeight;

	
	public Shader replacementShader;

	
	private bool highQuality;

	
	public Color clearColor = Color.black;

	
	public string reflectionSampler = "_ReflectionTex";

	
	public float clipPlaneOffset = 0.07f;

	
	private Vector3 oldpos = Vector3.zero;

	
	private Camera reflectionCamera;

	
	private Dictionary<Camera, bool> helperCameras;

	
	private Texture[] initialReflectionTextures;
}
