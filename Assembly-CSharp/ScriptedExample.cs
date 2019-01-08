using System;
using System.Collections;
using PathologicalGames;
using UnityEngine;

public class ScriptedExample : MonoBehaviour
{
	private void Awake()
	{
		this.xform = base.transform;
		this.xformCns = base.gameObject.AddComponent<TransformConstraint>();
		this.xformCns.noTargetMode = UnityConstraints.NO_TARGET_OPTIONS.SetByScript;
		this.xformCns.constrainRotation = false;
		this.lookCns = base.gameObject.AddComponent<SmoothLookAtConstraint>();
		this.lookCns.noTargetMode = UnityConstraints.NO_TARGET_OPTIONS.SetByScript;
		this.lookCns.pointAxis = Vector3.up;
		this.lookCns.upAxis = Vector3.forward;
		this.lookCns.speed = this.turnSpeed;
		base.StartCoroutine(this.LookAtRandom());
		base.StartCoroutine(this.MoveRandom());
	}

	private IEnumerator MoveRandom()
	{
		yield return new WaitForSeconds(this.newDirectionInterval + 0.001f);
		for (;;)
		{
			yield return null;
			Vector3 targetDirection = this.lookCns.position - this.xform.position;
			Vector3 moveVect = targetDirection.normalized * this.moveSpeed * 0.1f;
			this.xformCns.position = this.xform.position + moveVect;
			Debug.DrawRay(this.xform.position, this.xform.up * 2f, Color.grey);
			Debug.DrawRay(this.xform.position, targetDirection.normalized * 2f, Color.green);
		}
		yield break;
	}

	private IEnumerator LookAtRandom()
	{
		for (;;)
		{
			yield return new WaitForSeconds(this.newDirectionInterval);
			Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * 100f;
			this.lookCns.position = randomPosition + this.xform.position;
		}
		yield break;
	}

	public float moveSpeed = 1f;

	public float turnSpeed = 1f;

	public float newDirectionInterval = 3f;

	private SmoothLookAtConstraint lookCns;

	private TransformConstraint xformCns;

	private Transform xform;
}
