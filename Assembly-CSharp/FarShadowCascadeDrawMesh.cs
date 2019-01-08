using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FarShadowCascadeDrawMesh : MonoBehaviour
{
	private void Awake()
	{
		this._farCascadeProp = Shader.PropertyToID("_FarCascade");
		this._billboardShadeFadeFactorsProp = Shader.PropertyToID("_BillboardShadeFadeFactors");
		this._farCascadeMatrixProp = Shader.PropertyToID("_FarCascadeMatrix");
		this._farCascadeBlendValuesProps = Shader.PropertyToID("_FarCascadeBlendValues");
		if (!this._lightCamera)
		{
			this.Init();
		}
		if (this.TopShadowTexture)
		{
			Shader.SetGlobalTexture("_TopShadowTexture", this.TopShadowTexture);
		}
		this.InitializeMaterialInstance();
	}

	private void OnPreRender()
	{
		this.SetShadowCamera();
	}

	private void Init()
	{
		this._shadowMatrix = Matrix4x4.identity;
		this._depthShader = Shader.Find("Hidden/Far Shadow Depth Only");
		if (this._shadowCameraGO == null)
		{
			this._shadowCameraGO = new GameObject("__Far_Shadow Camera - " + base.name);
			this._shadowCameraGO.hideFlags = HideFlags.DontSave;
		}
		this._lightCamera = this._shadowCameraGO.AddComponent<Camera>();
		this._lightCamera.renderingPath = RenderingPath.Forward;
		this._lightCamera.clearFlags = CameraClearFlags.Depth;
		this._lightCamera.depthTextureMode = DepthTextureMode.None;
		this._lightCamera.useOcclusionCulling = false;
		this._lightCamera.cullingMask = this.CullingMask;
		this._lightCamera.orthographic = true;
		this._lightCamera.depth = -10f;
		this._lightCamera.aspect = 1f;
		this._lightCamera.SetReplacementShader(this._depthShader, "RenderType");
		this._lightCamera.enabled = false;
		this._lightCamera.eventMask = 0;
		this._eyeCamera = base.GetComponent<Camera>();
	}

	private void OnEnable()
	{
		this.AllocateTarget();
		this.InitializeMaterialInstance();
	}

	private void InitializeMaterialInstance()
	{
		if (this._materialInstance != null)
		{
			UnityEngine.Object.Destroy(this._materialInstance);
		}
		this._materialInstance = new Material(Shader.Find("Hidden/Far Shadow Depth Only"));
	}

	private void OnDisable()
	{
		this.ReleaseTarget();
	}

	private void OnDestroy()
	{
		if (this._lightCamera != null)
		{
			UnityEngine.Object.DestroyImmediate(this._lightCamera.gameObject);
		}
		if (this._materialInstance != null)
		{
			UnityEngine.Object.Destroy(this._materialInstance);
		}
		this._materialInstance = null;
	}

	private void OnValidate()
	{
		if (!Application.isPlaying || !this._lightCamera)
		{
			return;
		}
		this.ReleaseTarget();
		this.AllocateTarget();
	}

	private void AllocateTarget()
	{
		if (this._lightCamera)
		{
			this._shadowTexture = new RenderTexture((int)this.shadowMapSize, (int)this.shadowMapSize, 16, RenderTextureFormat.Shadowmap, RenderTextureReadWrite.Linear);
			this._shadowTexture.filterMode = FilterMode.Bilinear;
			this._shadowTexture.useMipMap = false;
			this._shadowTexture.autoGenerateMips = false;
			this._lightCamera.targetTexture = this._shadowTexture;
		}
	}

	private void ReleaseTarget()
	{
		if (this._lightCamera != null)
		{
			this._lightCamera.targetTexture = null;
		}
		if (this._shadowTexture != null)
		{
			UnityEngine.Object.DestroyImmediate(this._shadowTexture);
		}
		this._shadowTexture = null;
	}

	private void SetFarShadowMatrix(float focusRadius, float fardistance)
	{
		if (this._lightCamera == null)
		{
			return;
		}
		this._lightCamera.projectionMatrix = GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-focusRadius, focusRadius, -focusRadius, focusRadius, 0f, focusRadius * 2f), false);
		bool flag = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D9;
		bool flag2 = flag || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11;
		float num = (!flag) ? 0f : (0.5f / (float)this.shadowMapSize);
		float z = (!flag2) ? 0.5f : 1f;
		float num2 = (!flag2) ? 0.5f : 0f;
		float num3 = -this._farCascadeDepthBias;
		this._shadowSpaceMatrix.SetRow(0, new Vector4(0.5f, 0f, 0f, 0.5f + num));
		this._shadowSpaceMatrix.SetRow(1, new Vector4(0f, 0.5f, 0f, 0.5f + num));
		this._shadowSpaceMatrix.SetRow(2, new Vector4(0f, 0f, z, num2 + num3));
		this._shadowSpaceMatrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		Matrix4x4 worldToCameraMatrix = this._lightCamera.worldToCameraMatrix;
		Matrix4x4 gpuprojectionMatrix = GL.GetGPUProjectionMatrix(this._lightCamera.projectionMatrix, false);
		this._shadowMatrix = this._shadowSpaceMatrix * gpuprojectionMatrix * worldToCameraMatrix;
	}

	private void SetShadowCamera()
	{
		Light light = (!(Sunshine.Instance == null)) ? Sunshine.Instance.SunLight : null;
		Transform transform = (!(light == null)) ? light.transform : null;
		Transform transform2 = (!(this._shadowCameraGO == null)) ? this._shadowCameraGO.transform : null;
		if (this._eyeCamera == null || transform == null || transform2 == null || this._lightCamera == null)
		{
			return;
		}
		if (this._eyeCamera.renderingPath == RenderingPath.DeferredShading && this.enableFarShadows)
		{
			Shader.EnableKeyword("_USING_DEFERREDSHADING");
			transform2.rotation = Quaternion.LookRotation(transform.forward);
			SunshineMath.BoundingSphere boundingSphere = SunshineMath.FrustumBoundingSphereBinarySearch(this._eyeCamera, this._eyeCamera.nearClipPlane, this.FarCascadeDistance, true, 0f, 0.01f, 20);
			float num = SunshineMath.QuantizeValueWithoutFlicker(boundingSphere.radius, 100, this._lastBoundingRadius);
			this._lastBoundingRadius = num;
			float num2 = num * 2f;
			transform2.position = boundingSphere.origin;
			transform2.position = this._eyeCamera.transform.position - transform.forward * this.FarCascadeDistance * 0.5f;
			Vector3 vector = transform2.InverseTransformPoint(Vector3.zero);
			float step = num2 / (float)this.shadowMapSize;
			vector.x = SunshineMath.QuantizeValueWithoutFlicker(vector.x, step, this._lastLightWorldOrigin.x);
			vector.y = SunshineMath.QuantizeValueWithoutFlicker(vector.y, step, this._lastLightWorldOrigin.y);
			this._lastLightWorldOrigin = vector;
			transform2.position -= transform2.TransformPoint(vector);
			Vector3 vector2 = transform2.InverseTransformPoint(boundingSphere.origin);
			transform2.position += transform2.forward * (vector2.z - (boundingSphere.radius + this._lightCamera.nearClipPlane));
			this._lightCamera.orthographicSize = num2 * 0.5f;
			this._lightCamera.nearClipPlane = this._eyeCamera.nearClipPlane;
			this._lightCamera.farClipPlane = (boundingSphere.radius + this._lightCamera.nearClipPlane) * 2f;
			this._lightCamera.cullingMask = this.CullingMask;
			this._bbQuad = Vector3.Dot(-transform.forward, Vector3.up);
			this._biasLerp = Mathf.Abs(this._bbQuad);
			this._bbQuad = Mathf.Clamp01(Mathf.Clamp01(this._bbQuad + 0.0001f) - 0.75f) * 4f;
			this._bbBillboard = 1f - this._bbQuad;
			Shader.SetGlobalVector(this._billboardShadeFadeFactorsProp, new Vector4(this._bbBillboard, this._bbQuad, 0f, 0f));
			this._biasLerp = Mathf.Clamp01(Mathf.Abs(0.7071f - this._biasLerp) * 4f);
			this._farCascadeDepthBias = Mathf.Lerp(this.MaxFarCascadeDepthBias, this.MinFarCascadeDepthBias, this._biasLerp);
			this.SetFarShadowMatrix(boundingSphere.radius, this.FarCascadeDistance);
			this.DrawMesh();
		}
		else
		{
			Shader.DisableKeyword("_USING_DEFERREDSHADING");
		}
		float num3 = QualitySettings.shadowDistance * 0.125f;
		Shader.SetGlobalVector(this._farCascadeBlendValuesProps, new Vector4(QualitySettings.shadowDistance - num3 * 2f, num3, this.FarCascadeDistance - this.FarCascadeDistance * 0.275f));
	}

	private void DrawMesh()
	{
		if (this._shadowTexture == null || this._lightCamera == null || this._materialInstance == null || this.parent == null)
		{
			return;
		}
		GL.PushMatrix();
		Graphics.SetRenderTarget(this._shadowTexture);
		GL.LoadProjectionMatrix(this._lightCamera.projectionMatrix);
		GL.Clear(true, false, Color.black, 6000f);
		MeshFilter[] componentsInChildren = this.parent.GetComponentsInChildren<MeshFilter>();
		this._materialInstance.SetPass(0);
		foreach (MeshFilter meshFilter in componentsInChildren)
		{
			if (!(meshFilter.mesh == null))
			{
				Graphics.DrawMeshNow(meshFilter.mesh, this._matrix);
			}
		}
		GL.PopMatrix();
	}

	public bool enableFarShadows = true;

	public GameObject parent;

	private GameObject[] _targets;

	private Matrix4x4[] _targetMatrices;

	private Material _materialInstance;

	private Matrix4x4 _matrix = Matrix4x4.identity;

	public FarShadowCascadeDrawMesh.Dimension shadowMapSize = FarShadowCascadeDrawMesh.Dimension._1024;

	public float FarCascadeDistance = 200f;

	private float _farCascadeDepthBias = 0.01f;

	private float _biasLerp;

	public float MinFarCascadeDepthBias = 0.01f;

	public float MaxFarCascadeDepthBias = 0.03f;

	public int refresh = 10;

	private int c_refresh;

	public LayerMask CullingMask;

	private Camera _eyeCamera;

	private RenderTexture _shadowTexture;

	private Camera _lightCamera;

	private Matrix4x4 _shadowSpaceMatrix;

	private Matrix4x4 _shadowMatrix;

	private Shader _depthShader;

	private GameObject _shadowCameraGO;

	private float _lastBoundingRadius;

	private Vector2 _lastLightWorldOrigin = new Vector2(0f, 0f);

	public Texture TopShadowTexture;

	private float _bbQuad;

	private float _bbBillboard = 1f;

	private int _farCascadeProp;

	private int _billboardShadeFadeFactorsProp;

	private int _farCascadeMatrixProp;

	private int _farCascadeBlendValuesProps;

	public enum Dimension
	{
		_256 = 256,
		_512 = 512,
		_1024 = 1024,
		_2048 = 2048,
		_4096 = 4096
	}
}
