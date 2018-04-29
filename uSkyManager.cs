using System;
using TheForest.Utils;
using UnityEngine;
using uSky;


[ExecuteInEditMode]
[AddComponentMenu("uSky/uSky Manager")]
public class uSkyManager : MonoBehaviour
{
	
	
	
	public bool AutoApplySkybox
	{
		get
		{
			return this._AutoApplySkybox;
		}
		set
		{
			if (value && this.SkyboxMaterial && RenderSettings.skybox != this.SkyboxMaterial)
			{
				RenderSettings.skybox = this.SkyboxMaterial;
			}
			this._AutoApplySkybox = value;
		}
	}

	
	
	protected Material starMaterial
	{
		get
		{
			if (this.m_starMaterial == null)
			{
				this.m_starMaterial = new Material(Shader.Find("Hidden/uSky/Stars"));
				this.m_starMaterial.hideFlags = HideFlags.DontSave;
			}
			return this.m_starMaterial;
		}
	}

	
	protected void InitStarsMesh()
	{
		if (this.Stars == null)
		{
			this.Stars = new StarField();
		}
		this.starsMesh = this.Stars.InitializeStarfield();
		this.starsMesh.hideFlags = HideFlags.DontSave;
	}

	
	private void OnEnable()
	{
		if (this.m_sunLight == null)
		{
			this.m_sunLight = GameObject.Find("Directional Light");
		}
		if (this.EnableNightSky && this.starsMesh == null)
		{
			Debug.Log("InitMaterial Starfield");
			this.InitStarsMesh();
		}
		this.InitMaterial(this.SkyboxMaterial);
	}

	
	private void OnDisable()
	{
		if (this.starsMesh)
		{
			UnityEngine.Object.DestroyImmediate(this.starsMesh);
		}
		if (this.m_starMaterial)
		{
			UnityEngine.Object.DestroyImmediate(this.m_starMaterial);
		}
	}

	
	private void detectColorSpace()
	{
		if (this.SkyboxMaterial != null)
		{
			this.InitMaterial(this.SkyboxMaterial);
		}
	}

	
	private void Start()
	{
		this.InitPIDs();
		if (this.SkyboxMaterial != null)
		{
			this.InitMaterial(this.SkyboxMaterial);
		}
		this.AutoApplySkybox = this._AutoApplySkybox;
		if (this.EnableNightSky && this.starsMesh == null)
		{
			this.InitStarsMesh();
		}
	}

	
	
	public float Timeline01
	{
		get
		{
			return this.Timeline / 24f;
		}
	}

	
	private void Update()
	{
		if (this.SkyUpdate && this.SkyboxMaterial != null)
		{
			this.InitMaterial(this.SkyboxMaterial);
		}
		if (this.EnableNightSky && this.starsMesh != null && this.starMaterial != null && this.SunDir.y < 0.2f && LocalPlayer.MainCam != null)
		{
			Vector3 position = LocalPlayer.MainCamTr.position;
			Graphics.DrawMesh(this.starsMesh, position, Quaternion.identity, this.starMaterial, 0);
		}
	}

	
	private void InitSun()
	{
	}

	
	private void InitPIDs()
	{
		this.SunDirPID = Shader.PropertyToID("_SunDir");
		this.Moon_wtlPID = Shader.PropertyToID("_Moon_wtl");
		this.betaRPID = Shader.PropertyToID("_betaR");
		this.betaMPID = Shader.PropertyToID("_betaM");
		this.SkyMultiplierPID = Shader.PropertyToID("_SkyMultiplier");
		this.SunSizePID = Shader.PropertyToID("_SunSize");
		this.mieConstPID = Shader.PropertyToID("_mieConst");
		this.miePhase_gPID = Shader.PropertyToID("_miePhase_g");
		this.GroundColorPID = Shader.PropertyToID("_GroundColor");
		this.NightHorizonColorPID = Shader.PropertyToID("_NightHorizonColor");
		this.NightZenithColorPID = Shader.PropertyToID("_NightZenithColor");
		this.MoonInnerCoronaPID = Shader.PropertyToID("_MoonInnerCorona");
		this.MoonOuterCoronaPID = Shader.PropertyToID("_MoonOuterCorona");
		this.MoonSizePID = Shader.PropertyToID("_MoonSize");
		this.ColorCorrectionPID = Shader.PropertyToID("_colorCorrection");
		this.OuterSpaceIntensityPID = Shader.PropertyToID("_OuterSpaceIntensity");
		this.StarIntensityPID = Shader.PropertyToID("StarIntensity");
	}

	
	private void InitMaterial(Material mat)
	{
		Vector3 zero = Vector3.zero;
		Vector3 betaR_RayleighOffset = this.betaR_RayleighOffset;
		zero.x = Mathf.Exp(-betaR_RayleighOffset.x);
		zero.y = Mathf.Exp(-betaR_RayleighOffset.y);
		zero.z = Mathf.Exp(-betaR_RayleighOffset.z);
		Vector3 skyMultiplier = this.skyMultiplier;
		Color getNightZenithColor = this.getNightZenithColor;
		Vector3 a = new Vector3(getNightZenithColor.r, getNightZenithColor.g, getNightZenithColor.b);
		a.x *= (2f - getNightZenithColor.r) * zero.x;
		a.y *= (2f - getNightZenithColor.g) * zero.y;
		a.z *= (2f - getNightZenithColor.b) * zero.z;
		Vector3 vector = Vector3.Lerp(a, Vector3.one - zero, skyMultiplier.x) * 0.75f * skyMultiplier.y;
		vector.x = Mathf.LinearToGammaSpace(vector.x);
		vector.y = Mathf.LinearToGammaSpace(vector.y);
		vector.z = Mathf.LinearToGammaSpace(vector.z);
		float a2 = Vector3.Dot(this.LinearLuminanceConst, vector);
		vector.x = Mathf.Lerp(a2, vector.x, this.ReflectionSaturation);
		vector.y = Mathf.Lerp(a2, vector.y, this.ReflectionSaturation);
		vector.z = Mathf.Lerp(a2, vector.z, this.ReflectionSaturation);
		Shader.SetGlobalVector("_SkyReflectionFinalColor", vector);
		mat.SetVector(this.SunDirPID, this.SunDir);
		mat.SetMatrix(this.Moon_wtlPID, this.getMoonMatrix);
		mat.SetVector(this.betaRPID, betaR_RayleighOffset);
		mat.SetVector(this.betaMPID, this.BetaM);
		mat.SetVector(this.SkyMultiplierPID, this.skyMultiplier);
		mat.SetFloat(this.SunSizePID, 32f / this.SunSize);
		mat.SetVector(this.mieConstPID, this.mieConst);
		mat.SetVector(this.miePhase_gPID, this.miePhase_g);
		mat.SetVector(this.GroundColorPID, this.bottomTint);
		mat.SetVector(this.NightHorizonColorPID, this.getNightHorizonColor);
		mat.SetVector(this.NightZenithColorPID, this.getNightZenithColor);
		mat.SetVector(this.MoonInnerCoronaPID, this.getMoonInnerCorona);
		mat.SetVector(this.MoonOuterCoronaPID, this.getMoonOuterCorona);
		mat.SetFloat(this.MoonSizePID, this.MoonSize);
		mat.SetVector(this.ColorCorrectionPID, this.ColorCorrection);
		mat.SetFloat(this.OuterSpaceIntensityPID, this.OuterSpaceIntensity);
		if (this.starMaterial != null)
		{
			this.starMaterial.SetFloat(this.StarIntensityPID, this.starBrightness);
		}
	}

	
	
	public Vector3 SunDir
	{
		get
		{
			return (!(this.m_sunLight != null)) ? new Vector3(0.321f, 0.766f, -0.557f) : (this.m_sunLight.transform.forward * -1f);
		}
	}

	
	
	private Matrix4x4 getMoonMatrix
	{
		get
		{
			if (this.m_moonLight == null)
			{
				this.moon_wtl = Matrix4x4.TRS(Vector3.zero, new Quaternion(-0.9238795f, 8.817204E-08f, 8.817204E-08f, 0.3826835f), Vector3.one);
			}
			else if (this.m_moonLight != null)
			{
				this.moon_wtl = this.m_moonLight.transform.worldToLocalMatrix;
				this.moon_wtl.SetColumn(2, Vector4.Scale(new Vector4(1f, 1f, 1f, -1f), this.moon_wtl.GetColumn(2)));
			}
			return this.moon_wtl;
		}
	}

	
	
	private Vector3 variableRangeWavelengths
	{
		get
		{
			return new Vector3(Mathf.Lerp(this.Wavelengths.x + 150f, this.Wavelengths.x - 150f, this.SkyTint.r), Mathf.Lerp(this.Wavelengths.y + 150f, this.Wavelengths.y - 150f, this.SkyTint.g), Mathf.Lerp(this.Wavelengths.z + 150f, this.Wavelengths.z - 150f, this.SkyTint.b));
		}
	}

	
	
	public Vector3 BetaR
	{
		get
		{
			Vector3 vector = this.variableRangeWavelengths * 1E-09f;
			Vector3 a = new Vector3(Mathf.Pow(vector.x, 4f), Mathf.Pow(vector.y, 4f), Mathf.Pow(vector.z, 4f));
			Vector3 vector2 = 7.635E+25f * a * 5.755f;
			float num = 8f * Mathf.Pow(3.14159274f, 3f) * Mathf.Pow(0.0006001896f, 2f) * 6.105f;
			return 1000f * new Vector3(num / vector2.x, num / vector2.y, num / vector2.z);
		}
	}

	
	
	private Vector3 betaR_RayleighOffset
	{
		get
		{
			return this.BetaR * Mathf.Max(0.001f, this.RayleighScattering);
		}
	}

	
	
	public Vector3 BetaM
	{
		get
		{
			return new Vector3(Mathf.Pow(this.Wavelengths.x, -0.84f), Mathf.Pow(this.Wavelengths.y, -0.84f), Mathf.Pow(this.Wavelengths.z, -0.84f));
		}
	}

	
	
	public float uMuS
	{
		get
		{
			return Mathf.Atan(Mathf.Max(this.SunDir.y, -0.1975f) * 5.35f) / 1.1f + 0.74f;
		}
	}

	
	
	public float DayTime
	{
		get
		{
			return Mathf.Min(1f, this.uMuS);
		}
	}

	
	
	public float SunsetTime
	{
		get
		{
			return Mathf.Clamp01((this.uMuS - 1f) * (2f / Mathf.Pow(this.RayleighScattering, 4f)));
		}
	}

	
	
	public float NightTime
	{
		get
		{
			return 1f - this.DayTime;
		}
	}

	
	
	public Vector3 miePhase_g
	{
		get
		{
			float num = this.SunAnisotropyFactor * this.SunAnisotropyFactor;
			float num2 = (!this.LinearSpace || !this.Tonemapping) ? 1f : 2f;
			return new Vector3(num2 * ((1f - num) / (2f + num)), 1f + num, 2f * this.SunAnisotropyFactor);
		}
	}

	
	
	public Vector3 mieConst
	{
		get
		{
			return new Vector3(1f, this.BetaR.x / this.BetaR.y, this.BetaR.x / this.BetaR.z) * 0.004f * this.MieScattering;
		}
	}

	
	
	public Vector3 skyMultiplier
	{
		get
		{
			return new Vector3(this.SunsetTime, this.Exposure * 4f * this.DayTime * Mathf.Sqrt(this.RayleighScattering), this.NightTime);
		}
	}

	
	
	private Vector3 bottomTint
	{
		get
		{
			float num = (!this.LinearSpace) ? 0.02f : 0.01f;
			return new Vector3(this.betaR_RayleighOffset.x / (this.m_GroundColor.r * num), this.betaR_RayleighOffset.y / (this.m_GroundColor.g * num), this.betaR_RayleighOffset.z / (this.m_GroundColor.b * num));
		}
	}

	
	
	public Vector2 ColorCorrection
	{
		get
		{
			return (!this.LinearSpace || !this.Tonemapping) ? ((!this.LinearSpace) ? Vector2.one : new Vector2(1f, 2f)) : new Vector2(0.38317f, 1.413f);
		}
	}

	
	
	public Color getNightHorizonColor
	{
		get
		{
			return this.NightHorizonColor * this.NightTime;
		}
	}

	
	
	public Color getNightZenithColor
	{
		get
		{
			return this.NightZenithColor.Evaluate(this.Timeline01) * 0.01f;
		}
	}

	
	
	private Vector4 getMoonInnerCorona
	{
		get
		{
			return new Vector4(this.MoonInnerCorona.r * this.NightTime, this.MoonInnerCorona.g * this.NightTime, this.MoonInnerCorona.b * this.NightTime, 400f / this.MoonInnerCorona.a);
		}
	}

	
	
	private Vector4 getMoonOuterCorona
	{
		get
		{
			float num = (!this.LinearSpace) ? 8f : ((!this.Tonemapping) ? 12f : 16f);
			return new Vector4(this.MoonOuterCorona.r * 0.25f * this.NightTime, this.MoonOuterCorona.g * 0.25f * this.NightTime, this.MoonOuterCorona.b * 0.25f * this.NightTime, num / this.MoonOuterCorona.a);
		}
	}

	
	
	private float starBrightness
	{
		get
		{
			float num = (!this.LinearSpace) ? 1.5f : 0.5f;
			return this.StarIntensity * this.NightTime * num;
		}
	}

	
	[Tooltip("Update of the sky calculations in each frame.")]
	public bool SkyUpdate = true;

	
	[Range(0f, 24f)]
	[Tooltip("This value controls the light vertically. It represents sunrise/day and sunset/night time( Rotation X )")]
	public float Timeline = 17f;

	
	[Range(-180f, 180f)]
	[Tooltip("This value controls the light horizionally.( Rotation Y )")]
	public float Longitude;

	
	[Space(10f)]
	[Tooltip("This value sets the brightness of the sky.(for day time only)")]
	[Range(0f, 5f)]
	public float Exposure = 1f;

	
	[Range(0f, 5f)]
	[Tooltip("Rayleigh scattering is caused by particles in the atmosphere (up to 8 km). It produces typical earth-like sky colors (reddish/yellowish colors at sun set, and the like).")]
	public float RayleighScattering = 1f;

	
	[Range(0f, 5f)]
	[Tooltip("Mie scattering is caused by aerosols in the lower atmosphere (up to 1.2 km). It is for haze and halos around the sun on foggy days.")]
	public float MieScattering = 1f;

	
	[Range(0f, 0.9995f)]
	[Tooltip("The anisotropy factor controls the sun's appearance in the sky.The closer this value gets to 1.0, the sharper and smaller the sun spot will be. Higher values cause more fuzzy and bigger sun spots.")]
	public float SunAnisotropyFactor = 0.76f;

	
	[Range(0.001f, 10f)]
	[Tooltip("Size of the sun spot in the sky")]
	public float SunSize = 1f;

	
	[Tooltip("It is visible spectrum light waves. Tweaking these values will shift the colors of the resulting gradients and produce different kinds of atmospheres.")]
	public Vector3 Wavelengths = new Vector3(680f, 550f, 440f);

	
	[Tooltip("It is wavelength dependent. Tweaking these values will shift the colors of sky color.")]
	public Color SkyTint = new Color(0.5f, 0.5f, 0.5f, 1f);

	
	[Tooltip("It is the bottom half color of the skybox")]
	public Color m_GroundColor = new Color(0.369f, 0.349f, 0.341f, 1f);

	
	[Tooltip("It is a Directional Light from the scene, it represents Sun Ligthing")]
	public GameObject m_sunLight;

	
	[Space(10f)]
	[Tooltip("Toggle the Night Sky On and Off")]
	public bool EnableNightSky = true;

	
	[Tooltip("The zenith color of the night sky gradient. (Top of the night sky)")]
	public Gradient NightZenithColor = new Gradient
	{
		colorKeys = new GradientColorKey[]
		{
			new GradientColorKey(new Color32(50, 71, 99, byte.MaxValue), 0.225f),
			new GradientColorKey(new Color32(74, 107, 148, byte.MaxValue), 0.25f),
			new GradientColorKey(new Color32(74, 107, 148, byte.MaxValue), 0.75f),
			new GradientColorKey(new Color32(50, 71, 99, byte.MaxValue), 0.775f)
		},
		alphaKeys = new GradientAlphaKey[]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	
	[Tooltip("The horizon color of the night sky gradient.")]
	public Color NightHorizonColor = new Color(0.43f, 0.47f, 0.5f, 1f);

	
	[Range(0f, 5f)]
	[Tooltip("This controls the intensity of the Stars field in night sky.")]
	public float StarIntensity = 1f;

	
	[Range(0f, 2f)]
	[Tooltip("This controls the intensity of the Outer Space Cubemap in night sky.")]
	public float OuterSpaceIntensity = 0.25f;

	
	[Tooltip("The color of the moon's inner corona. This Alpha value controls the size and blurriness corona.")]
	public Color MoonInnerCorona = new Color(1f, 1f, 1f, 0.5f);

	
	[Tooltip("The color of the moon's outer corona. This Alpha value controls the size and blurriness corona.")]
	public Color MoonOuterCorona = new Color(0.25f, 0.39f, 0.5f, 0.5f);

	
	[Range(0f, 1f)]
	[Tooltip("This controls the moon texture size in the night sky.")]
	public float MoonSize = 0.15f;

	
	[Tooltip("It is additional Directional Light from the scene, it represents Moon Ligthing.")]
	public GameObject m_moonLight;

	
	[Tooltip("It is the uSkybox Material of the uSky.")]
	public Material SkyboxMaterial;

	
	[SerializeField]
	[Tooltip("It will automatically assign the current skybox material to Render Settings.")]
	private bool _AutoApplySkybox = true;

	
	[HideInInspector]
	[SerializeField]
	public bool LinearSpace;

	
	[Tooltip("Toggle it if the Main Camera is using HDR mode and Tonemapping image effect.")]
	public bool Tonemapping;

	
	[Range(0f, 1f)]
	public float ReflectionSaturation = 0.25f;

	
	private Vector3 euler;

	
	private Matrix4x4 moon_wtl;

	
	private StarField Stars;

	
	private Mesh starsMesh;

	
	private int SunDirPID;

	
	private int Moon_wtlPID;

	
	private int betaRPID;

	
	private int betaMPID;

	
	private int SkyMultiplierPID;

	
	private int SunSizePID;

	
	private int mieConstPID;

	
	private int miePhase_gPID;

	
	private int GroundColorPID;

	
	private int NightHorizonColorPID;

	
	private int NightZenithColorPID;

	
	private int MoonInnerCoronaPID;

	
	private int MoonOuterCoronaPID;

	
	private int MoonSizePID;

	
	private int ColorCorrectionPID;

	
	private int OuterSpaceIntensityPID;

	
	private int StarIntensityPID;

	
	private Vector3 LinearLuminanceConst = new Vector3(0.0396819152f, 0.4580218f, 0.00609653955f);

	
	private Material m_starMaterial;
}
