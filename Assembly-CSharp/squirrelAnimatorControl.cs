﻿using System;
using System.Collections;
using UnityEngine;

public class squirrelAnimatorControl : MonoBehaviour
{
	private void Awake()
	{
		this.ai = base.transform.parent.GetComponent<animalAI>();
		this.animator = base.GetComponent<Animator>();
		this.avoidance = base.GetComponentInChildren<animalAvoidance>();
		this.Tr = base.transform.parent;
		this.rotateTr = base.transform;
	}

	private void Start()
	{
		this.ai.playMaker.FsmVariables.GetFsmGameObject("animatorGO").Value = base.gameObject;
		this.ai.pmMove.FsmVariables.GetFsmGameObject("animatorGO").Value = base.gameObject;
		this.ai.animatorTr = base.transform;
		base.Invoke("callChangeIdle", (float)UnityEngine.Random.Range(0, 3));
		this.ai.playMaker.FsmVariables.GetFsmInt("HashOnTree").Value = Animator.StringToHash("onTree");
	}

	private void checkVis()
	{
		if (!this.animator.enabled)
		{
			this.ai.playMaker.SendEvent("notVisible");
		}
	}

	private void Update()
	{
		if (!this.animator.enabled && this.ai.doMove)
		{
			Quaternion rotation = Quaternion.identity;
			this.wantedDir = this.ai.wantedDir;
			Vector3 vector = this.ai.wantedDir;
			vector.y = 0f;
			if (vector != Vector3.zero)
			{
				rotation = Quaternion.LookRotation(vector, Vector3.up);
			}
			this.rotateTr.rotation = rotation;
			this.Tr.Translate(this.wantedDir * Time.deltaTime * (this.animator.GetFloat("Speed") * 4f), Space.World);
		}
	}

	private void OnAnimatorMove()
	{
		if (this.animator.enabled)
		{
			this.moveDir = this.animator.deltaPosition;
			if (this.blocked)
			{
				Vector3 currNormal = this.avoidance.currNormal;
				currNormal.y = 0f;
				currNormal.Normalize();
				if (currNormal.sqrMagnitude > 0.03f)
				{
					Vector3 vector = this.avoidance.transform.InverseTransformPoint(this.avoidance.currPoint);
					Vector3 a = Vector3.Cross(currNormal, Vector3.up);
					Debug.DrawRay(this.avoidance.currPoint, a * 5f, Color.blue);
					this.animator.applyRootMotion = false;
					float d = 0f;
					if (vector.z < 0f)
					{
						d = 1f;
					}
					else if (vector.z > 0f)
					{
						d = -1f;
					}
					this.animSpeed = this.animator.GetFloat("Speed") * 5f;
					this.animSpeed = Mathf.Clamp(this.animSpeed, 0.5f, 6f);
					Vector3 b = this.Tr.position + a * d;
					Debug.DrawRay(this.Tr.position, a * d * 5f, Color.red);
					this.Tr.position = Vector3.Slerp(this.Tr.position, b, this.animSpeed * Time.deltaTime);
				}
			}
			else
			{
				this.animator.applyRootMotion = true;
				this.Tr.position += this.moveDir;
			}
			if (!this.animator.GetBool("treeClimb"))
			{
				this.terrainPos = Terrain.activeTerrain.SampleHeight(this.Tr.position) + Terrain.activeTerrain.transform.position.y;
				this.Tr.position = new Vector3(this.Tr.position.x, this.terrainPos, this.Tr.position.z);
				this.tx = (this.Tr.position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
				this.tz = (this.Tr.position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
				this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
				this.rotateTr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.rotateTr.right, this.tNormal), this.tNormal), Time.deltaTime * 10f);
			}
			else
			{
				this.rotateTr.rotation = Quaternion.Lerp(this.rotateTr.rotation, Quaternion.LookRotation(Vector3.Cross(this.rotateTr.right, Vector3.up), Vector3.up), Time.deltaTime * 4f);
			}
		}
	}

	private void callChangeIdle()
	{
		base.StartCoroutine("changeIdleValue");
	}

	private IEnumerator changeIdleValue()
	{
		float val = 0f;
		float initVal = this.animator.GetFloat("idleFloat1");
		float side = initVal;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			if (side > 0.5f)
			{
				val = Mathf.SmoothStep(initVal, 0f, t);
			}
			else
			{
				val = Mathf.SmoothStep(initVal, 1f, t);
			}
			this.animator.SetFloatReflected("idleFloat1", val);
			yield return null;
		}
		base.Invoke("callChangeIdle", UnityEngine.Random.Range(3f, 7f));
		yield break;
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
		base.CancelInvoke("callChangeIdle");
	}

	private IEnumerator moveToTreeTarget(Vector3 pos)
	{
		float t = 0f;
		float initX = this.Tr.position.x;
		float initZ = this.Tr.position.z;
		while ((double)t < 1.0)
		{
			t += Time.deltaTime * 1.5f;
			float newX = Mathf.Lerp(initX, pos.x, t);
			float newZ = Mathf.Lerp(initZ, pos.z, t);
			this.Tr.position = new Vector3(newX, this.Tr.position.y, newZ);
			yield return null;
		}
		yield break;
	}

	private void enableBlocked()
	{
		this.blocked = true;
	}

	private void disableBlocked()
	{
		this.blocked = false;
	}

	private Animator animator;

	private animalAI ai;

	private animalAvoidance avoidance;

	private Transform Tr;

	private Transform rotateTr;

	private Vector3 wantedDir;

	private AnimatorStateInfo currLayerState0;

	private AnimatorStateInfo nextLayerState0;

	public float gravity;

	private Vector3 moveDir = Vector3.zero;

	public bool turnClose;

	private float terrainPos;

	private Vector3 tNormal;

	private float tx;

	private float tz;

	public bool blocked;

	public Collider blockCollider;

	private float animSpeed;

	private RaycastHit hit;

	private RaycastHit hit2;

	private Vector3 pos;
}
