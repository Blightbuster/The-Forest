using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class animalVis : MonoBehaviour
{
	private void Start()
	{
		this.displacementDisableDistance = 45f;
		this.ai = base.transform.GetComponent<animalAI>();
		this.skin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
	}

	private void Update()
	{
		if (Time.time < this.timeOffset)
		{
			return;
		}
		this.updateVis();
		this.timeOffset = Time.time + this.updateRate;
	}

	private void updateVis()
	{
		if (this.Lod1)
		{
			if (this.ai.fsmPlayerDist.Value > this.lodDistance)
			{
				if (this.skin.sharedMesh != this.Lod1)
				{
					this.skin.sharedMesh = this.Lod1;
				}
			}
			else if (this.Lod0 && this.skin.sharedMesh != this.Lod0)
			{
				this.skin.sharedMesh = this.Lod0;
			}
		}
		if (this.ai.fsmPlayerDist.Value > this.displacementDisableDistance)
		{
			if (this.displacementGo.activeSelf)
			{
				this.displacementGo.SetActive(false);
			}
		}
		else if (!this.displacementGo.activeSelf)
		{
			this.displacementGo.SetActive(true);
		}
		if (this.ai.fsmPlayerDist.Value > this.mecanimDisableDistance)
		{
			if (this.mecanimEmitter.enabled)
			{
				this.mecanimEmitter.enabled = false;
			}
		}
		else if (!this.mecanimEmitter.enabled)
		{
			this.mecanimEmitter.enabled = true;
		}
		if (this.ai.fsmPlayerDist.Value > this.shadowsDisableDistance)
		{
			this.skin.shadowCastingMode = ShadowCastingMode.Off;
		}
		else
		{
			this.skin.shadowCastingMode = ShadowCastingMode.On;
		}
	}

	private IEnumerator fixMotionBlur()
	{
		yield return null;
		yield break;
	}

	public Mesh Lod0;

	public Mesh Lod1;

	public GameObject displacementGo;

	public MecanimEventEmitter mecanimEmitter;

	public SkinnedMeshRenderer skin;

	public animalAI ai;

	public float lodDistance = 35f;

	public float mecanimDisableDistance = 30f;

	public float displacementDisableDistance = 25f;

	public float shadowsDisableDistance = 35f;

	public float updateRate = 0.5f;

	private float timeOffset;
}
