using System;
using System.Collections;
using UnityEngine;

public class OVRScreenFade : MonoBehaviour
{
	public float currentAlpha { get; private set; }

	private void Awake()
	{
		this.fadeMaterial = new Material(Shader.Find("Oculus/Unlit Transparent Color"));
	}

	public void FadeOut()
	{
		base.StartCoroutine(this.Fade(0f, 1f));
	}

	private void OnLevelFinishedLoading(int level)
	{
		base.StartCoroutine(this.Fade(1f, 0f));
	}

	private void Start()
	{
		if (this.fadeOnStart)
		{
			base.StartCoroutine(this.Fade(1f, 0f));
		}
	}

	private void OnDestroy()
	{
		if (this.fadeMaterial != null)
		{
			UnityEngine.Object.Destroy(this.fadeMaterial);
		}
	}

	public void SetUIFade(float level)
	{
		this.uiFadeAlpha = Mathf.Clamp01(level);
		this.SetMaterialAlpha();
	}

	public void SetFadeLevel(float level)
	{
		this.currentAlpha = level;
		this.SetMaterialAlpha();
	}

	private IEnumerator Fade(float startAlpha, float endAlpha)
	{
		float elapsedTime = 0f;
		while (elapsedTime < this.fadeTime)
		{
			elapsedTime += Time.deltaTime;
			this.currentAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsedTime / this.fadeTime));
			this.SetMaterialAlpha();
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	private void SetMaterialAlpha()
	{
		Color color = this.fadeColor;
		color.a = Mathf.Max(this.currentAlpha, this.uiFadeAlpha);
		this.isFading = (color.a > 0f);
		if (this.fadeMaterial != null)
		{
			this.fadeMaterial.color = color;
		}
	}

	private void OnPostRender()
	{
		if (this.isFading)
		{
			this.fadeMaterial.SetPass(0);
			GL.PushMatrix();
			GL.LoadOrtho();
			GL.Color(this.fadeMaterial.color);
			GL.Begin(7);
			GL.Vertex3(0f, 0f, -12f);
			GL.Vertex3(0f, 1f, -12f);
			GL.Vertex3(1f, 1f, -12f);
			GL.Vertex3(1f, 0f, -12f);
			GL.End();
			GL.PopMatrix();
		}
	}

	[Tooltip("Fade duration")]
	public float fadeTime = 2f;

	[Tooltip("Screen color at maximum fade")]
	public Color fadeColor = new Color(0.01f, 0.01f, 0.01f, 1f);

	public bool fadeOnStart = true;

	private float uiFadeAlpha;

	private Material fadeMaterial;

	private bool isFading;
}
