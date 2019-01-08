using System;
using UnityEngine;

public class animateMeat : MonoBehaviour
{
	private void Start()
	{
		this.skin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		this.animator = base.transform.GetComponent<Animator>();
		base.gameObject.SetActive(false);
	}

	private void playMeat(float t)
	{
		this.animator.CrossFade("Base Layer.eatMeatCycle", 0f, 0, t);
	}

	private void setMeatMaterial(bool burnt)
	{
		if (burnt)
		{
			this.skin.sharedMaterial = this.burntMat;
		}
		else
		{
			this.skin.sharedMaterial = this.meatMat;
		}
	}

	public Animator animator;

	private SkinnedMeshRenderer skin;

	public Material meatMat;

	public Material burntMat;
}
