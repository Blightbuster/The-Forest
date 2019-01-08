using System;
using UnityEngine;

public class amplifyDisableHook : MonoBehaviour
{
	private void Start()
	{
		this.ai = base.transform.GetComponentInParent<mutantAI>();
	}

	private void Update()
	{
		this.dist = this.ai.mainPlayerDist;
	}

	private mutantAI ai;

	public bool skipUpdate;

	public float dist;
}
