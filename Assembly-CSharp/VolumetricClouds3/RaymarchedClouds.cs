using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VolumetricClouds3
{
	[ExecuteInEditMode]
	public class RaymarchedClouds : MonoBehaviour
	{
		private void Start()
		{
			if (this.materialUsed != null)
			{
				this.ValidateRenderPlane();
			}
			else if (this.materialUsed == null && Application.isPlaying)
			{
				Debug.LogError("Object " + base.gameObject.name + " has a RaymarchedClouds attached to it but its specified material is null");
			}
		}

		private void OnEnable()
		{
			RaymarchedClouds.instances.Add(this);
			if (RaymarchedClouds.instances.Count >= 1)
			{
				this.pointLights = RaymarchedClouds.instances[0].pointLights;
			}
			this.lastPointLights = this.pointLights;
			if (this.plane)
			{
				this.plane.SetActive(true);
			}
			if (this.shadowPlane)
			{
				this.shadowPlane.SetActive(true);
			}
		}

		private void OnDisable()
		{
			RaymarchedClouds.instances.Remove(this);
			if (this.plane)
			{
				this.plane.SetActive(false);
			}
			if (this.shadowPlane)
			{
				this.shadowPlane.SetActive(false);
			}
		}

		private void OnDestroy()
		{
			this.Clear();
		}

		private void Clear()
		{
			this.DestroySafe(this.plane);
			this.DestroySafe(this.shadowPlane);
			this.DestroySafe(this.editorCamScript);
		}

		private void LateUpdate()
		{
		}

		private void OnPreCull()
		{
			this.ValidatePointLights();
			if (this.materialUsed == null)
			{
				return;
			}
			this.PrepareRendering();
		}

		private void OnPostRender()
		{
			if (this.hidePlane && !this.IsEditorCamera())
			{
				if (this.mRendererClouds)
				{
					this.mRendererClouds.enabled = false;
				}
				if (this.splitShadowRendering && this.mRendererShadow)
				{
					this.mRendererShadow.enabled = false;
				}
			}
		}

		private void OnDrawGizmos()
		{
			this.ValidatePointLights();
			if (this.materialUsed == null)
			{
				return;
			}
			this.ValidateEditorCam();
			if (base.enabled)
			{
				this.PrepareRendering();
			}
		}

		private void PrepareRendering()
		{
			this.ValidateRenderPlane();
			this.SetupShaderVariables();
		}

		private void ValidateRenderPlane()
		{
			if (this.materialUsed == null)
			{
				return;
			}
			if (this.thisCam == null)
			{
				this.thisCam = base.GetComponent<Camera>();
			}
			this.LazyValidatePlane(ref this.plane, ref this.mRendererClouds, ref this.materialUsed, "VC2_RaymarchedCloudPlane");
			if (this.splitShadowRendering)
			{
				this.ValidateSplitShadow();
			}
			else if (this.shadowPlane != null)
			{
				this.DestroySafe(this.shadowPlane);
			}
			this.FitPlane(ref this.plane, ref this.shadowPlane);
		}

		private void SetupShaderVariables()
		{
			if (this.pointLights != null && this.pointLights.Length > 0)
			{
				Light[] array = this.ComputeVisibleLightToShader(this.pointLights, this.thisCam);
				this.mRendererClouds.sharedMaterial.SetInt("_VisibleLightCount", array.Length);
				if (array.Length != 0)
				{
					Vector4[] array2 = new Vector4[array.Length];
					Vector4[] array3 = new Vector4[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = this.LightToSphere(array[i]);
						array3[i] = array[i].color * array[i].intensity;
					}
					this.mRendererClouds.sharedMaterial.SetVectorArray("_VisiblePointLights", RaymarchedClouds.emptyCleanupArray);
					this.mRendererClouds.sharedMaterial.SetVectorArray("_VisiblePointLightsColor", RaymarchedClouds.emptyCleanupArray);
					this.mRendererClouds.sharedMaterial.SetVectorArray("_VisiblePointLights", array2);
					this.mRendererClouds.sharedMaterial.SetVectorArray("_VisiblePointLightsColor", array3);
				}
			}
			this.mRendererClouds.enabled = true;
			if (this.splitShadowRendering)
			{
				this.mRendererShadow.enabled = true;
				if (this.materialShadows && this.syncMaterial)
				{
					this.materialUsed.DisableKeyword("_RENDERSHADOWS");
					this.materialUsed.DisableKeyword("_RENDERSHADOWSONLY");
					this.materialUsed.SetInt("_RenderShadowsOnly", 0);
					this.materialUsed.SetInt("_SSShadows", 0);
					this.materialShadows.CopyPropertiesFromMaterial(this.materialUsed);
					this.materialShadows.SetInt("_RenderShadowsOnly", 1);
					this.materialShadows.SetInt("_SSShadows", 1);
					this.materialShadows.EnableKeyword("_RENDERSHADOWS");
					this.materialShadows.EnableKeyword("_RENDERSHADOWSONLY");
				}
			}
		}

		private void ValidatePointLights()
		{
			if (this.pointLights != this.lastPointLights)
			{
				this.lastPointLights = this.pointLights;
				for (int i = 0; i < RaymarchedClouds.instances.Count; i++)
				{
					RaymarchedClouds.instances[i].UpdateLocalLights(this.pointLights);
				}
			}
		}

		private Vector4 LightToSphere(Light light)
		{
			return new Vector4(light.transform.position.x, light.transform.position.y, light.transform.position.z, light.range);
		}

		private Light[] ComputeVisibleLightToShader(Light[] lights, Camera cam)
		{
			List<Light> list = new List<Light>();
			for (int i = 0; i < lights.Length; i++)
			{
				if (lights[i].type == LightType.Point)
				{
					if (lights[i].enabled)
					{
						Vector3 position = lights[i].transform.position;
						float range = lights[i].range;
						if (this.IsPointLightLooselyInsideFrustum(position, range, cam))
						{
							list.Add(lights[i]);
						}
					}
				}
				else
				{
					Debug.LogError("Volumetric Clouds : " + lights[i].type + " is not supported.");
				}
			}
			return list.ToArray();
		}

		private bool IsPointLightLooselyInsideFrustum(Vector3 pos, float radius, Camera cam)
		{
			Vector3 a = cam.transform.position + (pos - cam.transform.position).magnitude * cam.transform.forward;
			Vector3 vector = a - pos;
			Vector3 a2 = pos + vector.normalized * Mathf.Min(radius, vector.magnitude);
			return Vector3.Angle(cam.transform.forward, (a2 - cam.transform.position).normalized) < cam.fieldOfView;
		}

		public void UpdateLocalLights(Light[] newPointLights)
		{
			this.pointLights = newPointLights;
			this.lastPointLights = this.pointLights;
		}

		private void ValidateSplitShadow()
		{
			if (this.materialShadows)
			{
				this.LazyValidatePlane(ref this.shadowPlane, ref this.mRendererShadow, ref this.materialShadows, "VC2_RaymarchedShadowsPlane");
			}
		}

		private void ValidateEditorCam()
		{
			if (this.editorCamScript == null && this.attachToEditorCam)
			{
				if (Camera.current.name == "SceneCamera")
				{
					this.editorCamScript = Camera.current.gameObject.GetComponent<RaymarchedClouds>();
					if (this.editorCamScript == null)
					{
						this.editorCamScript = Camera.current.gameObject.AddComponent<RaymarchedClouds>();
					}
					this.editorCamScript.materialUsed = this.materialUsed;
					this.editorCamScript.materialShadows = this.materialShadows;
					this.editorCamScript.planeOffset = this.planeOffset;
					this.editorCamScript.splitShadowRendering = this.splitShadowRendering;
					this.editorCamScript.gameObject.hideFlags &= ~HideFlags.NotEditable;
				}
			}
			else if (this.editorCamScript != null)
			{
				if (!this.attachToEditorCam)
				{
					this.DestroySafe(this.editorCamScript);
				}
				else if (this.editorCamScript.materialUsed != this.materialUsed)
				{
					this.editorCamScript.materialUsed = this.materialUsed;
				}
			}
		}

		private void LazyValidatePlane(ref GameObject planeGO, ref MeshRenderer planeRenderer, ref Material material, string defaultName)
		{
			if (planeGO == null)
			{
				this.CreatePlane(ref planeGO, defaultName);
			}
			if (planeRenderer == null)
			{
				planeRenderer = planeGO.GetComponent<MeshRenderer>();
			}
			if (planeRenderer.sharedMaterial != material)
			{
				planeRenderer.sharedMaterial = material;
			}
			if (this.hidePlane)
			{
				planeGO.hideFlags = HideFlags.HideAndDontSave;
			}
			else if (!this.hidePlane)
			{
				planeGO.hideFlags = HideFlags.None;
			}
		}

		private void CreatePlane(ref GameObject planeGO, string defaultName)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
			planeGO = gameObject;
			planeGO.name = defaultName;
			planeGO.SetActive(base.enabled);
			this.DestroySafe(planeGO.GetComponent<MeshCollider>());
			this.DestroySafe(planeGO.GetComponent<Rigidbody>());
			MeshRenderer component = planeGO.GetComponent<MeshRenderer>();
			component.lightProbeUsage = LightProbeUsage.Off;
			component.shadowCastingMode = ShadowCastingMode.Off;
			component.reflectionProbeUsage = ReflectionProbeUsage.Off;
		}

		private void FitPlane(ref GameObject planeGO, ref GameObject shadowGO)
		{
			float num = this.planeOffset;
			float num2 = this.thisCam.nearClipPlane + num;
			float num3 = (!this.thisCam.orthographic) ? (Mathf.Tan(this.thisCam.fieldOfView * 0.5f * 0.0174532924f) * 2f * num2) : (this.thisCam.orthographicSize * 2f);
			float num4 = num3 * this.thisCam.aspect;
			planeGO.transform.SetParent(this.thisCam.transform);
			planeGO.transform.localPosition = new Vector3(0f, 0f, num2);
			planeGO.transform.localRotation = Quaternion.identity;
			planeGO.transform.localScale = new Vector3(num4 + num4 * num, num3 + num3 * num, 1f);
			if (shadowGO != null)
			{
				shadowGO.transform.SetParent(planeGO.transform.parent);
				shadowGO.transform.localPosition = planeGO.transform.localPosition + Vector3.forward * this.shadowRenderOffset;
				shadowGO.transform.localRotation = planeGO.transform.localRotation;
				shadowGO.transform.localScale = planeGO.transform.localScale + new Vector3(num4 * this.shadowRenderOffset, num3 * this.shadowRenderOffset, 0f);
			}
		}

		public bool IsEditorCamera()
		{
			return base.gameObject.name == "SceneCamera";
		}

		private void DestroySafe(UnityEngine.Object o)
		{
			if (o == null)
			{
				return;
			}
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(o);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(o);
			}
		}

		[Space(20f)]
		public Material materialUsed;

		[Header("Plane")]
		public bool hidePlane = true;

		public float planeOffset = 0.01f;

		[Header("Editor")]
		public bool attachToEditorCam;

		public RaymarchedClouds editorCamScript;

		[Header("Render Lights")]
		public Light[] pointLights;

		private Light[] lastPointLights;

		private static Vector4[] emptyCleanupArray = new Vector4[64];

		[Header("Split Shadow Rendering")]
		public bool splitShadowRendering;

		public Material materialShadows;

		public bool syncMaterial;

		public float shadowRenderOffset = 0.01f;

		[Space(10f)]
		public bool activateHandleDrawing;

		[SerializeField]
		[HideInInspector]
		private GameObject shadowPlane;

		[SerializeField]
		[HideInInspector]
		private GameObject plane;

		private MeshRenderer mRendererShadow;

		private MeshRenderer mRendererClouds;

		private Camera thisCam;

		private static List<RaymarchedClouds> instances = new List<RaymarchedClouds>();
	}
}
