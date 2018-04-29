using System;
using UnityEngine;


public class AfsWindFake : MonoBehaviour
{
	
	private void Awake()
	{
		this.afsSyncFrequencies();
		this.afsLightingSettings();
		this.afsUpdateWind();
	}

	
	public void Update()
	{
		this.afsUpdateWind();
	}

	
	private void afsSyncFrequencies()
	{
		this.temp_WindFrequency = this.WindFrequency;
		this.temp_WindJitterFrequency = this.WindJitterFrequencyForGrassshader;
		this.domainTime_Wind = 0f;
		this.domainTime_2ndBending = 0f;
		this.domainTime_Grass = 0f;
	}

	
	private void afsLightingSettings()
	{
		if (this.DirectionalLightReference != null)
		{
			this.DirectlightDir = this.DirectionalLightReference.transform.forward;
			if (this.Sunlight == null)
			{
				this.Sunlight = this.DirectionalLightReference.GetComponent<Light>();
			}
			if (!this.Sunlight.enabled)
			{
				Shader.SetGlobalVector("_AfsDirectSunDir", new Vector4(0f, 0f, 0f, 0f));
			}
			else
			{
				this.SunLightCol = new Vector3(this.Sunlight.color.r, this.Sunlight.color.g, this.Sunlight.color.b) * this.Sunlight.intensity;
				this.SunLuminance = Vector3.Dot(this.SunLightCol, new Vector3(0.22f, 0.707f, 0.071f));
				Shader.SetGlobalVector("_AfsDirectSunDir", new Vector4(this.DirectlightDir.x, this.DirectlightDir.y, this.DirectlightDir.z, this.SunLuminance));
				Shader.SetGlobalVector("_AfsDirectSunCol", this.SunLightCol);
			}
		}
		else
		{
			Shader.SetGlobalVector("_AfsDirectSunDir", new Vector4(0f, 0f, 0f, 0f));
			Shader.SetGlobalVector("_AfsDirectSunCol", new Vector3(0f, 0f, 0f));
		}
		Shader.SetGlobalVector("_AfsSpecFade", new Vector4(this.AfsSpecFade.x, this.AfsSpecFade.y, 1f, 1f));
	}

	
	private void afsUpdateWind()
	{
		if (this.SyncWindDir)
		{
			this.Wind = new Vector4(base.transform.forward.x, base.transform.forward.y, base.transform.forward.z, this.Wind.w);
		}
		this.TempWind = this.Wind;
		this.TempWind.w = this.TempWind.w * 2f;
		Shader.SetGlobalVector("_Wind", this.TempWind);
		Shader.SetGlobalVector("_WindStrengthTrees", new Vector2(this.TempWind.w * this.WindMuliplierForTreeShaderPrimary, this.TempWind.w * this.WindMuliplierForTreeShaderSecondary));
		Shader.SetGlobalFloat("_WindFrequency", this.WindFrequency);
		this.domainTime_Wind += this.temp_WindFrequency * Time.deltaTime * 2f;
		this.domainTime_2ndBending += Time.deltaTime;
		Shader.SetGlobalVector("_AfsTimeFrequency", new Vector4(this.domainTime_Wind, this.domainTime_2ndBending, 0.375f * (1f + this.temp_WindFrequency * this.LeafTurbulenceFoliageShader), 0.193f * (1f + this.temp_WindFrequency * this.LeafTurbulenceFoliageShader)));
		this.temp_WindFrequency = Mathf.MoveTowards(this.temp_WindFrequency, this.WindFrequency, this.freqSpeed);
		this.SinusWave = Mathf.Sin(this.domainTime_Wind);
		this.TriangleWaves = this.SmoothTriangleWave(new Vector4(this.SinusWave, this.SinusWave * 0.8f, 0f, 0f));
		this.Oscillation = this.TriangleWaves.x + this.TriangleWaves.y * this.TriangleWaves.y;
		this.Oscillation = (this.Oscillation + 8f) * 0.125f * this.TempWind.w;
		this.TempWind.x = this.TempWind.x * this.Oscillation;
		this.TempWind.z = this.TempWind.z * this.Oscillation;
		Shader.SetGlobalFloat("_AfsWaveSize", 0.5f / this.WaveSizeFoliageShader);
		float num = this.WindJitterScaleForGrassshaderCurve.Evaluate(this.TempWind.w);
		Shader.SetGlobalFloat("_AfsWindJitterScale", this.WindJitterScaleForGrassshader * (10f * num));
		Shader.SetGlobalVector("_AfsGrassWind", this.TempWind * this.WindMultiplierForGrassshader);
		this.domainTime_Grass += this.temp_WindJitterFrequency * Time.deltaTime;
		this.temp_WindJitterFrequency = Mathf.MoveTowards(this.temp_WindJitterFrequency, this.WindJitterFrequencyForGrassshader, this.freqSpeed);
		this.GrassWindSpeed = this.domainTime_Grass * 0.1f;
		Shader.SetGlobalVector("_AfsWaveAndDistance", new Vector4(this.GrassWindSpeed, this.WaveSizeForGrassshader, this.TempWind.w, this.DetailDistanceForGrassShader * this.DetailDistanceForGrassShader));
		Shader.SetGlobalVector("_AFSWindMuliplier", this.WindMuliplierForTreeShader);
	}

	
	private float CubicSmooth(float x)
	{
		return x * x * (3f - 2f * x);
	}

	
	private float TriangleWave(float x)
	{
		return Mathf.Abs((x + 0.5f) % 1f * 2f - 1f);
	}

	
	private float SmoothTriangleWave(float x)
	{
		return this.CubicSmooth(this.TriangleWave(x));
	}

	
	private Vector4 CubicSmooth(Vector4 x)
	{
		x = Vector4.Scale(x, x);
		x = Vector4.Scale(x, new Vector4(3f, 3f, 3f, 3f) - 2f * x);
		return x;
	}

	
	private Vector4 TriangleWave(Vector4 x)
	{
		x = (x + new Vector4(0.5f, 0.5f, 0.5f, 0.5f)) * 2f - new Vector4(1f, 1f, 1f, 1f);
		return this.AbsVecFour(x);
	}

	
	private Vector4 SmoothTriangleWave(Vector4 x)
	{
		return this.CubicSmooth(this.TriangleWave(x));
	}

	
	private Vector4 FracVecFour(Vector4 a)
	{
		a.x %= 1f;
		a.y %= 1f;
		a.z %= 1f;
		a.w %= 1f;
		return a;
	}

	
	private Vector4 AbsVecFour(Vector4 a)
	{
		a.x = Mathf.Abs(a.x);
		a.y = Mathf.Abs(a.y);
		a.z = Mathf.Abs(a.z);
		a.w = Mathf.Abs(a.w);
		return a;
	}

	
	private const float TwoPI = 6.28318548f;

	
	public bool isLinear;

	
	public GameObject DirectionalLightReference;

	
	private Vector3 DirectlightDir;

	
	private Light Sunlight;

	
	private Vector3 SunLightCol;

	
	private float SunLuminance;

	
	public bool GrassApproxTrans;

	
	[Range(0f, 2f)]
	public float GrassTransStrength = 0.5f;

	
	public Vector4 Wind = new Vector4(0.85f, 0.075f, 0.4f, 0.5f);

	
	[Range(0.01f, 2f)]
	public float WindFrequency = 0.25f;

	
	[Range(0.1f, 25f)]
	public float WaveSizeFoliageShader = 10f;

	
	[Range(0f, 10f)]
	public float LeafTurbulenceFoliageShader = 4f;

	
	[Range(0.01f, 1f)]
	public float WindMultiplierForGrassshader = 1f;

	
	[Range(0f, 10f)]
	public float WaveSizeForGrassshader = 1f;

	
	[Range(0f, 1f)]
	public float WindJitterFrequencyForGrassshader = 0.25f;

	
	[Range(0f, 1f)]
	public float WindJitterScaleForGrassshader = 0.15f;

	
	[Range(0f, 100f)]
	public Vector2 AfsSpecFade = new Vector2(30f, 10f);

	
	public AnimationCurve WindJitterScaleForGrassshaderCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 1f),
		new Keyframe(4f, 0.2f)
	});

	
	public bool SyncWindDir;

	
	[Range(0f, 10f)]
	public float WindMuliplierForTreeShaderPrimary = 1f;

	
	[Range(0f, 10f)]
	public float WindMuliplierForTreeShaderSecondary = 1f;

	
	public Vector4 WindMuliplierForTreeShader = new Vector4(1f, 1f, 1f, 1f);

	
	private float temp_WindFrequency = 0.25f;

	
	private float temp_WindJitterFrequency = 0.25f;

	
	private float freqSpeed = 0.05f;

	
	private float domainTime_Wind;

	
	private float domainTime_2ndBending;

	
	private float domainTime_Grass;

	
	public float DetailDistanceForGrassShader = 80f;

	
	private Vector4 TempWind;

	
	private float GrassWindSpeed;

	
	private float SinusWave;

	
	private Vector4 TriangleWaves;

	
	private float Oscillation;
}
