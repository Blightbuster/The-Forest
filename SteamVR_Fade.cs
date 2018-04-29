using System;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;


public class SteamVR_Fade : MonoBehaviour
{
	
	public static void Start(Color newColor, float duration, bool fadeOverlay = false)
	{
		SteamVR_Events.Fade.Send(newColor, duration, fadeOverlay);
	}

	
	public static void View(Color newColor, float duration)
	{
		CVRCompositor compositor = OpenVR.Compositor;
		if (compositor != null)
		{
			compositor.FadeToColor(duration, newColor.r, newColor.g, newColor.b, newColor.a, false);
		}
	}

	
	public void OnStartFade(Color newColor, float duration, bool fadeOverlay)
	{
		if (duration > 0f)
		{
			this.targetColor = newColor;
			this.deltaColor = (this.targetColor - this.currentColor) / duration;
		}
		else
		{
			this.currentColor = newColor;
		}
	}

	
	private void OnEnable()
	{
		if (SteamVR_Fade.fadeMaterial == null)
		{
			SteamVR_Fade.fadeMaterial = new Material(Shader.Find("Custom/SteamVR_Fade"));
			SteamVR_Fade.fadeMaterialColorID = Shader.PropertyToID("fadeColor");
		}
		SteamVR_Events.Fade.Listen(new UnityAction<Color, float, bool>(this.OnStartFade));
		SteamVR_Events.FadeReady.Send();
	}

	
	private void OnDisable()
	{
		SteamVR_Events.Fade.Remove(new UnityAction<Color, float, bool>(this.OnStartFade));
	}

	
	private void OnPostRender()
	{
		if (this.currentColor != this.targetColor)
		{
			if (Mathf.Abs(this.currentColor.a - this.targetColor.a) < Mathf.Abs(this.deltaColor.a) * Time.deltaTime)
			{
				this.currentColor = this.targetColor;
				this.deltaColor = new Color(0f, 0f, 0f, 0f);
			}
			else
			{
				this.currentColor += this.deltaColor * Time.deltaTime;
			}
			if (this.fadeOverlay)
			{
				SteamVR_Overlay instance = SteamVR_Overlay.instance;
				if (instance != null)
				{
					instance.alpha = 1f - this.currentColor.a;
				}
			}
		}
		if (this.currentColor.a > 0f && SteamVR_Fade.fadeMaterial)
		{
			SteamVR_Fade.fadeMaterial.SetColor(SteamVR_Fade.fadeMaterialColorID, this.currentColor);
			SteamVR_Fade.fadeMaterial.SetPass(0);
			GL.Begin(7);
			GL.Vertex3(-1f, -1f, 0f);
			GL.Vertex3(1f, -1f, 0f);
			GL.Vertex3(1f, 1f, 0f);
			GL.Vertex3(-1f, 1f, 0f);
			GL.End();
		}
	}

	
	private Color currentColor = new Color(0f, 0f, 0f, 0f);

	
	private Color targetColor = new Color(0f, 0f, 0f, 0f);

	
	private Color deltaColor = new Color(0f, 0f, 0f, 0f);

	
	private bool fadeOverlay;

	
	private static Material fadeMaterial;

	
	private static int fadeMaterialColorID = -1;
}
