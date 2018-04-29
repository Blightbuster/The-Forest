using System;
using UnityEngine;


public class resetArtifactProperties : MonoBehaviour
{
	
	private void Start()
	{
		MeshRenderer component = base.transform.GetComponent<MeshRenderer>();
		Material sharedMaterial = component.sharedMaterial;
		sharedMaterial.EnableKeyword("_EMISSION");
		float value = 0f;
		Color white = Color.white;
		Color color = white * Mathf.LinearToGammaSpace(value);
		sharedMaterial.SetColor("_EmissionColor", color);
	}
}
