using System;
using System.Collections;
using UnityEngine;

public class artifactGlowSetup : MonoBehaviour
{
	private void Start()
	{
		base.StartCoroutine(this.ArtifactGlowRoutine());
	}

	private IEnumerator ArtifactGlowRoutine()
	{
		MeshRenderer mr = this.artifactGo.GetComponent<MeshRenderer>();
		Material mat = mr.sharedMaterial;
		mat.EnableKeyword("_EMISSION");
		float emission = 0f;
		Color baseColor = Color.white;
		float t = 0f;
		while (t < 1f)
		{
			emission = Mathf.Lerp(0f, 1f, t);
			t += Time.deltaTime / 8f;
			Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
			mat.SetColor("_EmissionColor", finalColor);
			yield return null;
		}
		yield return null;
		yield break;
	}

	public GameObject artifactGo;
}
