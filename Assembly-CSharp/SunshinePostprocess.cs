using System;
using UnityEngine;
using UnityEngine.Rendering;

public class SunshinePostprocess : MonoBehaviour
{
	private void OnEnable()
	{
		Shader shader = Shader.Find("Hidden/Post FX/Blit");
		this.blitMaterial = new Material(shader);
		this.MainTexID = Shader.PropertyToID("_MainTex");
	}

	private Camera GetCamera()
	{
		if (this._cam != null)
		{
			return this._cam;
		}
		this._cam = base.GetComponent<Camera>();
		return this._cam;
	}

	private void OnDisable()
	{
		if (this.sunshine_cb != null)
		{
			this.sunshine_cb.Clear();
			if (!Application.isPlaying)
			{
				Camera camera = this.GetCamera();
				if (camera != null)
				{
					camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, this.sunshine_cb);
				}
			}
		}
	}

	private void SetKeywordArray()
	{
		this.MYFILTER_STYLES = new string[5][];
		this.MYFILTER_STYLES[0] = new string[]
		{
			"SUNSHINE_FILTER_HARD",
			"SUNSHINE_FILTER_PCF_2x2",
			"SUNSHINE_FILTER_PCF_3x3",
			"SUNSHINE_FILTER_PCF_4x4",
			"SUNSHINE_DISABLED"
		};
		this.MYFILTER_STYLES[1] = new string[]
		{
			"SUNSHINE_DISABLED",
			"SUNSHINE_FILTER_HARD",
			"SUNSHINE_FILTER_PCF_3x3",
			"SUNSHINE_FILTER_PCF_4x4",
			"SUNSHINE_FILTER_PCF_2x2"
		};
		this.MYFILTER_STYLES[2] = new string[]
		{
			"SUNSHINE_DISABLED",
			"SUNSHINE_FILTER_HARD",
			"SUNSHINE_FILTER_PCF_3x3",
			"SUNSHINE_FILTER_PCF_4x4",
			"SUNSHINE_FILTER_PCF_2x2"
		};
		this.MYFILTER_STYLES[3] = new string[]
		{
			"SUNSHINE_DISABLED",
			"SUNSHINE_FILTER_HARD",
			"SUNSHINE_FILTER_PCF_2x2",
			"SUNSHINE_FILTER_PCF_4x4",
			"SUNSHINE_FILTER_PCF_3x3"
		};
		this.MYFILTER_STYLES[4] = new string[]
		{
			"SUNSHINE_DISABLED",
			"SUNSHINE_FILTER_HARD",
			"SUNSHINE_FILTER_PCF_2x2",
			"SUNSHINE_FILTER_PCF_3x3",
			"SUNSHINE_FILTER_PCF_4x4"
		};
	}

	private void OnPostRender()
	{
		Camera camera = this.GetCamera();
		if (camera != null)
		{
			this.rtWidth = camera.activeTexture.width;
			this.rtHeight = camera.activeTexture.height;
		}
	}

	private void OnPreCull()
	{
		Camera camera = this.GetCamera();
		if (camera == null)
		{
			return;
		}
		if (this.rtWidth < 1 || this.rtHeight < 1)
		{
			return;
		}
		if (this.sunshineCamera == null)
		{
			this.sunshineCamera = base.GetComponent<SunshineCamera>();
			if (this.sunshine_cb != null)
			{
				this.sunshine_cb.Clear();
				this.MYFILTER_STYLES = null;
			}
		}
		if (!this.sunshineCamera.GoodToGo)
		{
			if (this.sunshine_cb != null)
			{
				this.sunshine_cb.Clear();
				this.MYFILTER_STYLES = null;
			}
			return;
		}
		if (this.MYFILTER_STYLES == null)
		{
			this.SetKeywordArray();
		}
		Vector3 direction = camera.ViewportPointToRay(new Vector3(0f, 0f, 0f)).direction;
		Vector3 v = camera.ViewportPointToRay(new Vector3(1f, 0f, 0f)).direction - direction;
		Vector3 v2 = camera.ViewportPointToRay(new Vector3(0f, 1f, 0f)).direction - direction;
		Sunshine.Instance.PostScatterMaterial.SetVector("worldLightRay", Sunshine.Instance.SunLight.transform.forward);
		Sunshine.Instance.PostScatterMaterial.SetVector("worldRay", direction);
		Sunshine.Instance.PostScatterMaterial.SetVector("worldRayU", v);
		Sunshine.Instance.PostScatterMaterial.SetVector("worldRayV", v2);
		Sunshine.Instance.PostScatterMaterial.SetTexture("_ScatterRamp", Sunshine.Instance.ScatterRamp);
		this.quality = (int)(1 + Sunshine.Instance.ScatterSamplingQuality);
		Sunshine.Instance.PostScatterMaterial.SetColor("ScatterColor", Sunshine.Instance.ScatterColor);
		if (Sunshine.Instance.ScatterAnimateNoise)
		{
			this.scatterNoiseSeed += Time.deltaTime * Sunshine.Instance.ScatterAnimateNoiseSpeed;
			this.scatterNoiseSeed -= Mathf.Floor(this.scatterNoiseSeed);
		}
		Sunshine.Instance.PostScatterMaterial.SetTexture("ScatterDitherMap", Sunshine.Instance.ScatterDitherTexture);
		float value = 1f - Sunshine.Instance.ScatterExaggeration;
		float y = 1f / (Mathf.Clamp01(value) * Sunshine.Instance.LightDistance / camera.farClipPlane);
		float num = Sunshine.Instance.ScatterSky * Sunshine.Instance.ScatterIntensity;
		Sunshine.Instance.PostScatterMaterial.SetVector("ScatterIntensityVolumeSky", new Vector4(Sunshine.Instance.ScatterIntensity, y, num * 0.333f, num * 0.667f));
		int b = SunshineMath.RelativeResolutionDivisor(Sunshine.Instance.ScatterResolution);
		int2 @int = new int2(camera.pixelWidth, camera.pixelHeight) / b;
		@int.x = Mathf.Max(@int.x, 1);
		@int.y = Mathf.Max(@int.y, 1);
		Sunshine.Instance.PostScatterMaterial.SetVector("ScatterDitherData", new Vector3((float)@int.x / (float)Sunshine.Instance.ScatterDitherTexture.width, (float)@int.y / (float)Sunshine.Instance.ScatterDitherTexture.height, (!Sunshine.Instance.ScatterAnimateNoise) ? 0f : this.scatterNoiseSeed));
		Sunshine.Instance.PostBlurMaterial.SetFloat("BlurDepthTollerance", Sunshine.Instance.ScatterBlurDepthTollerance);
		if (this.sunshine_cb == null)
		{
			CommandBuffer[] commandBuffers = camera.GetCommandBuffers(CameraEvent.BeforeImageEffects);
			for (int i = 0; i < commandBuffers.Length; i++)
			{
				if (commandBuffers[i].name == "Sunshine CommandBuffer")
				{
					this.sunshine_cb = commandBuffers[i];
					break;
				}
			}
		}
		if (this.sunshine_cb == null)
		{
			this.sunshine_cb = new CommandBuffer();
			this.sunshine_cb.name = "Sunshine CommandBuffer";
			camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, this.sunshine_cb);
		}
		this.sunshine_cb.Clear();
		int nameID = Shader.PropertyToID("_SunshineScreenCopy");
		this.sunshine_cb.GetTemporaryRT(nameID, this.rtWidth, this.rtHeight, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);
		this.sunshine_cb.Blit(BuiltinRenderTextureType.CameraTarget, nameID, this.blitMaterial, 0);
		int nameID2 = Shader.PropertyToID("_STempRT");
		int nameID3 = Shader.PropertyToID("_STempPingPongRT");
		this.sunshine_cb.GetTemporaryRT(nameID2, @int.x, @int.y, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);
		this.sunshine_cb.GetTemporaryRT(nameID3, @int.x, @int.y, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default);
		this.sunshine_cb.SetGlobalTexture("_ScatterTexture", nameID2);
		this.sunshine_cb.DisableShaderKeyword(this.MYFILTER_STYLES[this.quality][0]);
		this.sunshine_cb.DisableShaderKeyword(this.MYFILTER_STYLES[this.quality][1]);
		this.sunshine_cb.DisableShaderKeyword(this.MYFILTER_STYLES[this.quality][2]);
		this.sunshine_cb.DisableShaderKeyword(this.MYFILTER_STYLES[this.quality][3]);
		this.sunshine_cb.EnableShaderKeyword(this.MYFILTER_STYLES[this.quality][4]);
		this.sunshine_cb.Blit(Texture2D.blackTexture, nameID2, Sunshine.Instance.PostScatterMaterial, 0);
		this.sunshine_cb.SetGlobalVector("BlurXY", new Vector2(1f, 0f));
		this.sunshine_cb.Blit(nameID2, nameID3, Sunshine.Instance.PostBlurMaterial, 0);
		this.sunshine_cb.SetGlobalVector("BlurXY", new Vector2(0f, 1f));
		this.sunshine_cb.Blit(nameID3, nameID2, Sunshine.Instance.PostBlurMaterial, 0);
		this.sunshine_cb.SetGlobalTexture(this.MainTexID, nameID);
		this.sunshine_cb.Blit(nameID, BuiltinRenderTextureType.CameraTarget, Sunshine.Instance.PostScatterMaterial, 1);
		this.sunshine_cb.ReleaseTemporaryRT(nameID);
		this.sunshine_cb.ReleaseTemporaryRT(nameID2);
		this.sunshine_cb.ReleaseTemporaryRT(nameID3);
	}

	public static void Blit(RenderTexture source, RenderTexture destination, Material material, int pass)
	{
		Graphics.Blit(source, destination, material, pass);
	}

	private SunshineCamera sunshineCamera;

	private CommandBuffer sunshine_cb;

	private Camera _cam;

	private Material blitMaterial;

	private float scatterNoiseSeed;

	private int quality = 3;

	private int MainTexID;

	private string[][] MYFILTER_STYLES;

	private int rtWidth = -1;

	private int rtHeight = -1;
}
