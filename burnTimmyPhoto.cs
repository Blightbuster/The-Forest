using System;
using System.Collections;
using UnityEngine;


public class burnTimmyPhoto : MonoBehaviour
{
	
	private void Start()
	{
		this._photoBlock = new MaterialPropertyBlock();
		this.targetRenderers = this.burnTarget.GetComponentsInChildren<SkinnedMeshRenderer>();
	}

	
	private void OnEnable()
	{
		this._photoBlock = new MaterialPropertyBlock();
		SkinnedMeshRenderer componentInChildren = this.burnTarget.GetComponentInChildren<SkinnedMeshRenderer>();
		componentInChildren.GetPropertyBlock(this._photoBlock);
		this._photoBlock.SetFloat("_BurnAmount", 0f);
		componentInChildren.SetPropertyBlock(this._photoBlock);
	}

	
	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	
	private IEnumerator burnPhotoRoutine()
	{
		float t = 0.35f;
		while (t < 1f)
		{
			for (int i = 0; i < this.targetRenderers.Length; i++)
			{
				this.targetRenderers[i].GetPropertyBlock(this._photoBlock);
				this._photoBlock.SetFloat("_BurnAmount", t);
				this.targetRenderers[i].SetPropertyBlock(this._photoBlock);
			}
			if (t > 0.45f)
			{
				this.burnTarget.GetComponent<Animator>().enabled = true;
			}
			t += Time.deltaTime / 78f;
			yield return null;
		}
		yield break;
	}

	
	private MaterialPropertyBlock _photoBlock;

	
	public GameObject burnTarget;

	
	private SkinnedMeshRenderer[] targetRenderers;
}
