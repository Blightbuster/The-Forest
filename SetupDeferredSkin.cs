using System;
using UnityEngine;


[ExecuteInEditMode]
public class SetupDeferredSkin : MonoBehaviour
{
	
	private float Smoothness2SpecularPower(float smoothness)
	{
		smoothness = 1f - smoothness;
		smoothness *= smoothness;
		smoothness = 2f / (smoothness * smoothness) - 2f;
		return smoothness;
	}

	
	private void OnEnable()
	{
	}

	
	private void OnDisable()
	{
	}

	
	private void Start()
	{
		Shader.SetGlobalTexture("_BRDFTex", this.BRDFTexture);
		Shader.SetGlobalColor("_SubColor", this.SubsurfaceColor.linear);
		Shader.SetGlobalVector("_Lux_Skin_DeepSubsurface", new Vector4(this.Power, this.Distortion, this.Scale, 0f));
	}

	
	private void Update()
	{
	}

	
	private const int k_TextureCount = 64;

	
	[Space(2f)]
	[Header("Area Lights")]
	public bool enableAreaLights;

	
	[Space(2f)]
	[Header("Skin Lighting")]
	public Texture BRDFTexture;

	
	[Space(5f)]
	public Color SubsurfaceColor = new Color(1f, 0.4f, 0.25f, 1f);

	
	public float Power = 2f;

	
	[Range(0f, 1f)]
	public float Distortion = 0.1f;

	
	public float Scale = 2f;

	
	private Texture2D[] noiseTextures;

	
	private int textureIndex;
}
