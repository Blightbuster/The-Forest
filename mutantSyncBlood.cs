using System;
using UnityEngine;


public class mutantSyncBlood : MonoBehaviour
{
	
	private void Awake()
	{
		this.thisRenderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
		this.sourceBloodPropertyBlock = new MaterialPropertyBlock();
	}

	
	private void OnEnable()
	{
		this.sourceRenderer.GetPropertyBlock(this.sourceBloodPropertyBlock);
		this.thisRenderer.SetPropertyBlock(this.sourceBloodPropertyBlock);
		this.thisRenderer.sharedMaterial.color = this.sourceRenderer.sharedMaterial.color;
	}

	
	private MaterialPropertyBlock sourceBloodPropertyBlock;

	
	public Renderer sourceRenderer;

	
	private Renderer thisRenderer;
}
