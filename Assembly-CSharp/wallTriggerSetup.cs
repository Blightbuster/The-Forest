using System;
using UnityEngine;

public class wallTriggerSetup : MonoBehaviour
{
	private void Start()
	{
		this.animator = base.transform.root.GetComponentInChildren<Animator>();
		this.speedVal = 0.095f;
	}

	private void OnTriggerStay(Collider other)
	{
		if (!other.isTrigger && !other.gameObject.CompareTag("enemyBlocker") && !other.gameObject.CompareTag("enemyRoot") && !other.gameObject.CompareTag("Player"))
		{
			this.atWall = true;
		}
	}

	private void Update()
	{
		float num = 0f;
		this.smoothVal = Mathf.SmoothDamp(this.smoothVal, this.val, ref num, this.speedVal);
		this.animator.SetFloatReflected("weaponClipBlend", this.smoothVal);
	}

	private void FixedUpdate()
	{
		if (this.atWall)
		{
			this.val = 10f;
			this.atWallCheck = true;
		}
		else
		{
			this.val = 0f;
			this.atWallCheck = false;
		}
		this.atWall = false;
	}

	public Animator animator;

	private bool atWall;

	public bool atWallCheck;

	private float val;

	private float smoothVal;

	public float speedVal = 0.11f;
}
