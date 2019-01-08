using System;
using UnityEngine;

public class artifactInventorySync : MonoBehaviour
{
	private void Awake()
	{
		this._ballPropertyBlock = new MaterialPropertyBlock();
	}

	private void OnEnable()
	{
		int currentStateIndex = this.HeldArtifactController.currentStateIndex;
		Color value;
		if (currentStateIndex != 1)
		{
			if (currentStateIndex != 2)
			{
				value = this.HeldArtifactController.idleColor * Mathf.LinearToGammaSpace(0.1f);
			}
			else
			{
				value = this.HeldArtifactController.repelColor * Mathf.LinearToGammaSpace(1.8f);
			}
		}
		else
		{
			value = this.HeldArtifactController.attractColor * Mathf.LinearToGammaSpace(1.8f);
		}
		this.BallRenderer.GetPropertyBlock(this._ballPropertyBlock);
		this._ballPropertyBlock.SetColor("_EmissionColor", value);
		this.BallRenderer.SetPropertyBlock(this._ballPropertyBlock);
	}

	public artifactBallController HeldArtifactController;

	public MeshRenderer BallRenderer;

	private MaterialPropertyBlock _ballPropertyBlock;
}
