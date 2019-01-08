using System;
using System.Collections;
using UnityEngine;

public class MoveTargetDemo : MonoBehaviour
{
	private void Start()
	{
		this.xform = base.transform;
		base.StartCoroutine(this.MoveTarget());
	}

	private IEnumerator MoveTarget()
	{
		yield return new WaitForSeconds(this.delay);
		float savedTime = Time.time;
		while (Time.time - savedTime < this.duration)
		{
			if (this.moveForward)
			{
				this.xform.Translate(Vector3.forward * (Time.deltaTime * this.speed));
			}
			else
			{
				this.xform.Translate(Vector3.back * (Time.deltaTime * this.speed));
			}
			yield return null;
		}
		this.moveForward = !this.moveForward;
		base.StartCoroutine(this.MoveTarget());
		yield break;
	}

	private Transform xform;

	private bool moveForward = true;

	private float speed = 20f;

	private float duration = 0.6f;

	private float delay = 3f;
}
