using System;
using UnityEngine;

public class birdOnFingerController : MonoBehaviour
{
	private void Start()
	{
		this._skin = base.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < this._skin.Length; i++)
		{
			this._skin[i].enabled = false;
		}
		this._enableVisTimer = Time.time + 2f;
	}

	private void Update()
	{
		if (Time.time > this._enableVisTimer)
		{
			for (int i = 0; i < this._skin.Length; i++)
			{
				this._skin[i].enabled = true;
			}
			base.enabled = false;
		}
	}

	private SkinnedMeshRenderer[] _skin;

	private float _enableVisTimer = 2f;
}
