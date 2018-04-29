using System;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PreClamp : MonoBehaviour
{
	
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!this.material)
		{
			this.material = new Material(this.shader);
		}
		Graphics.Blit(source, destination, this.material);
	}

	
	[HideInInspector]
	public Shader shader;

	
	private Material material;
}
