using System;
using UnityEngine;


public class birdNetAnimControl : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
	}

	
	private void OnAnimatorMove()
	{
		this.currState = this.animator.GetCurrentAnimatorStateInfo(0);
		this.nextState = this.animator.GetNextAnimatorStateInfo(0);
		if (this.currState.tagHash != this.flyingHash && this.nextState.tagHash != this.flyingHash && this.animator.GetBool("flying"))
		{
			this.animator.CrossFade(this.flyStateHash, 0.25f, 0);
		}
	}

	
	private Animator animator;

	
	private AnimatorStateInfo currState;

	
	private AnimatorStateInfo nextState;

	
	private int flyingHash = Animator.StringToHash("flying");

	
	private int flyStateHash = Animator.StringToHash("fly");
}
