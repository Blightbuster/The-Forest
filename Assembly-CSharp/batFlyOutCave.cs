using System;
using System.Collections;
using UnityEngine;

public class batFlyOutCave : MonoBehaviour
{
	private void Awake()
	{
		this.anim = base.GetComponent<Animator>();
	}

	private IEnumerator doBats()
	{
		if (this.bat1)
		{
			this.anim.SetBool("bat0", true);
		}
		if (this.bat2)
		{
			this.anim.SetBool("bat1", true);
		}
		if (this.bat3)
		{
			this.anim.SetBool("bat2", true);
		}
		if (this.bat4)
		{
			this.anim.SetBool("bat3", true);
		}
		if (this.bat5)
		{
			this.anim.SetBool("bat4", true);
		}
		if (this.bat6)
		{
			this.anim.SetBool("bat5", true);
		}
		if (this.bat7)
		{
			this.anim.SetBool("bat6", true);
		}
		if (this.bat8)
		{
			this.anim.SetBool("bat7", true);
		}
		if (this.bat9)
		{
			this.anim.SetBool("bat8", true);
		}
		if (this.bat10)
		{
			this.anim.SetBool("bat9", true);
		}
		if (this.bat11)
		{
			this.anim.SetBool("bat10", true);
		}
		if (this.bat12)
		{
			this.anim.SetBool("bat11", true);
		}
		if (this.bat13)
		{
			this.anim.SetBool("bat12", true);
		}
		if (this.bat14)
		{
			this.anim.SetBool("bat13", true);
		}
		if (this.bat15)
		{
			this.anim.SetBool("bat14", true);
		}
		if (this.bat16)
		{
			this.anim.SetBool("bat15", true);
		}
		yield return YieldPresets.WaitTwoSeconds;
		this.anim.SetBool("bat0", false);
		this.anim.SetBool("bat1", false);
		this.anim.SetBool("bat2", false);
		this.anim.SetBool("bat3", false);
		this.anim.SetBool("bat4", false);
		this.anim.SetBool("bat5", false);
		this.anim.SetBool("bat6", false);
		this.anim.SetBool("bat7", false);
		this.anim.SetBool("bat8", false);
		this.anim.SetBool("bat9", false);
		this.anim.SetBool("bat10", false);
		this.anim.SetBool("bat11", false);
		this.anim.SetBool("bat12", false);
		this.anim.SetBool("bat13", false);
		this.anim.SetBool("bat14", false);
		this.anim.SetBool("bat15", false);
		yield break;
	}

	public bool bat1;

	public bool bat2;

	public bool bat3;

	public bool bat4;

	public bool bat5;

	public bool bat6;

	public bool bat7;

	public bool bat8;

	public bool bat9;

	public bool bat10;

	public bool bat11;

	public bool bat12;

	public bool bat13;

	public bool bat14;

	public bool bat15;

	public bool bat16;

	private Animator anim;
}
