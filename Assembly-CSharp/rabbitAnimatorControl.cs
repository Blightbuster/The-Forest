using System;
using UnityEngine;

public class rabbitAnimatorControl : MonoBehaviour
{
	private void Awake()
	{
		this.rb = base.transform.parent.GetComponent<Rigidbody>();
		this.col = base.transform.parent.GetComponent<SphereCollider>();
		this.ai = base.transform.parent.GetComponent<animalAI>();
		this.animator = base.GetComponent<Animator>();
		this.avoidance = base.transform.GetComponentInChildren<animalAvoidance>();
		this.Tr = base.transform.parent;
		this.rotateTr = base.transform;
		this.runHash = Animator.StringToHash("run");
		this.idleHash = Animator.StringToHash("idle");
	}

	private void OnEnable()
	{
		if (!this.col)
		{
			this.col = base.transform.parent.GetComponent<SphereCollider>();
		}
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
		this.terrainPos = Terrain.activeTerrain.SampleHeight(this.Tr.position) + Terrain.activeTerrain.transform.position.y;
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
			if (this.useRB)
			{
				Vector3 position = this.Tr.position;
				position.y = this.terrainPos;
				this.rb.MovePosition(position + this.wantedDir.normalized * 0.1f);
			}
			else
			{
				this.Tr.Translate(this.wantedDir * Time.deltaTime * (this.animator.GetFloat("Speed") * 4f), Space.World);
			}
		}
	}

	private void LateUpdate()
	{
		if (this.useRB)
		{
			this.rb.velocity = Vector3.zero;
		}
	}

	private void OnAnimatorMove()
	{
		if (!this.animator)
		{
			return;
		}
		if (this.animator.enabled)
		{
			this.moveDir = this.animator.deltaPosition;
			this.animator.applyRootMotion = false;
			if (this.useRB)
			{
				Vector3 position = this.Tr.position;
				position.y = this.terrainPos;
				this.rb.MovePosition(position + this.moveDir);
				if (this.animator.deltaPosition.magnitude < 0.05f)
				{
					this.rb.velocity = Vector3.zero;
				}
			}
			else
			{
				this.Tr.position += this.moveDir;
			}
			if (Terrain.activeTerrain)
			{
				this.tx = (this.Tr.position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
				this.tz = (this.Tr.position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
				this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
				if (this.tNormal != Vector3.zero && this.tNormal.sqrMagnitude > 0f)
				{
					this.rotateTr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.rotateTr.right, this.tNormal), this.tNormal), Time.deltaTime * 10f);
				}
			}
		}
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

	private Transform rotateTr;

	private Vector3 wantedDir;

	private AnimatorStateInfo currLayerState0;

	private AnimatorStateInfo nextLayerState0;

	public float gravity;

	private Vector3 moveDir = Vector3.zero;

	public bool turnClose;

	public bool blocked;

	public Collider blockCollider;

	private float animSpeed;

	private float terrainPos;

	private float tx;

	private float tz;

	private Vector3 tNormal;

	private int layerMask2;

	private RaycastHit hit;

	private RaycastHit hit2;

	private Vector3 pos;

	private int runHash;

	private int idleHash;

	public bool useRB;
}
