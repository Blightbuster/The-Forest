using System;
using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

public class lizardAnimatorControl : MonoBehaviour
{
	private void Awake()
	{
		this.rb = base.transform.GetComponent<Rigidbody>();
		this.col = base.transform.GetComponent<SphereCollider>();
		this.ai = base.GetComponent<animalAI>();
		this.animator = base.GetComponent<Animator>();
		this.avoidance = base.transform.GetComponentInChildren<animalAvoidance>();
		this.Tr = base.transform;
	}

	private void OnEnable()
	{
		if (!this.col)
		{
			this.col = base.transform.parent.GetComponent<SphereCollider>();
		}
		if (!this.animator)
		{
			this.animator = base.transform.GetComponent<Animator>();
		}
		this.animator.SetBool("Tree", false);
		this.animator.SetBool("trapped", false);
		this.animator.speed = UnityEngine.Random.Range(1.1f, 1.2f);
	}

	private void Start()
	{
		this.fsmTreeBool = this.ai.playMaker.FsmVariables.GetFsmBool("treeBool");
	}

	private void checkVis()
	{
		if (!this.animator.enabled)
		{
			this.ai.playMaker.SendEvent("notVisible");
		}
		else
		{
			this.ai.playMaker.SendEvent("visible");
		}
	}

	private void Update()
	{
		if (!this.animator.GetBool("Tree"))
		{
			this.terrainPos = Terrain.activeTerrain.SampleHeight(this.Tr.position) + Terrain.activeTerrain.transform.position.y;
			this.col.enabled = true;
		}
		else
		{
			this.terrainPos = this.Tr.position.y;
			this.col.enabled = false;
		}
		if (!this.animator.enabled && this.ai.doMove)
		{
			Quaternion quaternion = Quaternion.identity;
			this.wantedDir = this.ai.wantedDir;
			Vector3 vector = this.ai.wantedDir;
			vector.y = 0f;
			if (vector != Vector3.zero)
			{
				quaternion = Quaternion.LookRotation(vector, Vector3.up);
			}
			if (this.useRB)
			{
				Vector3 position = this.Tr.position;
				position.y = this.terrainPos;
				this.rb.MovePosition(position + this.wantedDir.normalized * 0.1f);
				this.rb.MoveRotation(quaternion);
			}
			else
			{
				this.Tr.Translate(this.wantedDir * Time.deltaTime * (this.animator.GetFloat("Speed") * 4f), Space.World);
				this.Tr.rotation = quaternion;
			}
		}
	}

	private void LateUpdate()
	{
		if (this.useRB)
		{
			this.rb.velocity = Vector3.zero;
			this.rb.angularVelocity = Vector3.zero;
		}
	}

	private void OnAnimatorMove()
	{
		if (this.animator.enabled)
		{
			this.moveDir = this.animator.deltaPosition;
			this.animator.applyRootMotion = true;
			if (this.useRB)
			{
				Vector3 position = this.Tr.position;
				position.y = this.terrainPos;
				this.rb.MovePosition(position + this.moveDir * this.speedMultiplyer);
				if (this.animator.deltaPosition.magnitude < 0.05f)
				{
					this.rb.velocity = Vector3.zero;
					this.rb.angularVelocity = Vector3.zero;
				}
			}
			else
			{
				this.Tr.position += this.moveDir;
			}
			if (!this.animator.GetBool("Tree"))
			{
				this.tx = (this.Tr.position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
				this.tz = (this.Tr.position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
				this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
				if (this.useRB)
				{
					this.rb.MoveRotation(Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(base.transform.right, this.tNormal), this.tNormal), Time.fixedDeltaTime * 10f));
				}
				else
				{
					this.Tr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(base.transform.right, this.tNormal), this.tNormal), Time.deltaTime * 10f);
				}
			}
			else
			{
				if (this.useRB)
				{
					this.rb.MoveRotation(Quaternion.Lerp(this.Tr.rotation, Quaternion.LookRotation(Vector3.Cross(base.transform.right, Vector3.up), Vector3.up), Time.fixedDeltaTime * 4f));
				}
				this.Tr.rotation = Quaternion.Lerp(this.Tr.rotation, Quaternion.LookRotation(Vector3.Cross(base.transform.right, Vector3.up), Vector3.up), Time.fixedDeltaTime * 4f);
			}
		}
	}

	private IEnumerator moveToTreeTarget(Vector3 pos)
	{
		yield return YieldPresets.WaitTwoSeconds;
		yield return YieldPresets.WaitPointFiveSeconds;
		float t = 0f;
		float initX = this.Tr.position.x;
		float initZ = this.Tr.position.z;
		while ((double)t < 1.0)
		{
			t += Time.deltaTime / 5f;
			float newX = Mathf.Lerp(initX, pos.x, t);
			float newZ = Mathf.Lerp(initZ, pos.z, t);
			this.Tr.position = new Vector3(newX, base.transform.position.y, newZ);
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

	private Rigidbody rb;

	private SphereCollider col;

	private Animator animator;

	private animalAI ai;

	private animalAvoidance avoidance;

	private Transform Tr;

	private Vector3 wantedDir;

	private AnimatorStateInfo currLayerState0;

	private AnimatorStateInfo nextLayerState0;

	public float gravity;

	private Vector3 moveDir = Vector3.zero;

	private float terrainPos;

	private float tx;

	private float tz;

	private Vector3 tNormal;

	public bool blocked;

	public Collider blockCollider;

	public float speedMultiplyer = 1.25f;

	private float animSpeed;

	private FsmBool fsmTreeBool;

	private RaycastHit hit;

	private Vector3 pos;

	public bool useRB;
}
