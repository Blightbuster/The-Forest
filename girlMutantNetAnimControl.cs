using System;
using UnityEngine;


public class girlMutantNetAnimControl : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void Update()
	{
		this.animator.enabled = true;
		this.animator.applyRootMotion = false;
	}

	
	public Animator animator;
}
