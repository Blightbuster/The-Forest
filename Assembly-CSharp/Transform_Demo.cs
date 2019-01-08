using System;
using System.Collections;
using UnityEngine;

public class Transform_Demo : MonoBehaviour
{
	private void Awake()
	{
		this.xform = base.transform;
	}

	private void Start()
	{
		base.StartCoroutine(this.MoveTarget());
		base.StartCoroutine(this.RotateTarget());
	}

	private IEnumerator RotateTarget()
	{
		yield return new WaitForSeconds(this.delay);
		for (;;)
		{
			this.xform.Rotate(this.rotate);
			yield return null;
		}
		yield break;
	}

	private IEnumerator MoveTarget()
	{
		for (;;)
		{
			yield return new WaitForSeconds(this.delay);
			float savedTime = Time.time;
			while (Time.time - savedTime < this.duration)
			{
				if (this.moveForward)
				{
					this.xform.Translate(Vector3.up * (Time.deltaTime * this.speed));
					this.xform.localScale = Vector3.Lerp(this.xform.localScale, this.bigScale, Time.deltaTime * 4.75f);
				}
				else
				{
					this.xform.Translate(Vector3.down * (Time.deltaTime * this.speed));
					this.xform.localScale = Vector3.Lerp(this.xform.localScale, this.smallScale, Time.deltaTime * 4.75f);
				}
				yield return null;
			}
			this.moveForward = !this.moveForward;
		}
		yield break;
	}

	public Vector3 rotate = new Vector3(0f, 3f, 0f);

	private Transform xform;

	private bool moveForward = true;

	private float speed = 5f;

	private float duration = 0.6f;

	private float delay = 1.5f;

	private Vector3 bigScale = new Vector3(2f, 2f, 2f);

	private Vector3 smallScale = new Vector3(1f, 1f, 1f);
}
