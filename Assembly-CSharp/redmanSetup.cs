﻿using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class redmanSetup : MonoBehaviour
{
	private void Start()
	{
		this.Tr = base.transform;
		this.animator = base.GetComponent<Animator>();
		this.LayerMask = 69214208;
	}

	private void OnAnimatorIK()
	{
		if (!this.animator)
		{
			return;
		}
		if (!this.useIk)
		{
			this.animator.SetLookAtWeight(0f, 0.5f, 1f, 0f, 0f);
			return;
		}
		this.currState = this.animator.GetCurrentAnimatorStateInfo(0);
		this.animator.SetLookAtPosition(LocalPlayer.Transform.position);
		if (this.currState.tagHash == this.watchingTag)
		{
			this.animator.SetLookAtWeight(1f, 0.5f, 1f, 0f, 0.5f);
		}
		else
		{
			if (!this.doneLook)
			{
				base.StartCoroutine("smoothDisableIk");
				this.doneLook = true;
			}
			this.animator.SetLookAtWeight(this.val, 0.5f, 1f, 0f, 0.5f);
		}
	}

	public IEnumerator smoothDisableIk()
	{
		float t = 0f;
		this.val = 1f;
		while (t < 1f)
		{
			t += Time.deltaTime / 2.5f;
			this.val = Mathf.Lerp(1f, 0f, t);
			yield return null;
		}
		yield break;
	}

	private void LateUpdate()
	{
		if (!this.alignHeightToTerrain)
		{
			return;
		}
		this.pos = new Vector3(this.Tr.position.x, this.Tr.position.y + 3f, this.Tr.position.z);
		if (Physics.Raycast(this.pos, Vector3.down, out this.hit, 10f, this.LayerMask))
		{
			this.Tr.position = new Vector3(this.Tr.position.x, this.hit.point.y, this.Tr.position.z);
			if (this.rotateToTerrain)
			{
				this.Tr.rotation = Quaternion.LookRotation(Vector3.Cross(base.transform.right, this.hit.normal), this.hit.normal);
			}
		}
	}

	private Animator animator;

	private AnimatorStateInfo currState;

	public bool useIk;

	public bool rotateToTerrain;

	public bool alignHeightToTerrain;

	private int watchingTag = Animator.StringToHash("watching");

	private bool doneLook;

	private float val;

	private Vector3 pos;

	private Transform Tr;

	private RaycastHit hit;

	private int LayerMask;
}
