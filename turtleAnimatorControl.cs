using System;
using UnityEngine;


public class turtleAnimatorControl : MonoBehaviour
{
	
	private void Start()
	{
		this.rb = base.GetComponent<Rigidbody>();
		this.avoidance = base.transform.GetComponentInChildren<turtleAvoidance>();
		GameObject gameObject = GameObject.FindWithTag("OceanHeight");
		if (gameObject)
		{
			this.ocean = gameObject.transform;
		}
		this.ai = base.GetComponent<animalAI>();
		this.animator = base.GetComponent<Animator>();
		this.Tr = base.transform;
		base.InvokeRepeating("randomDepth", 1f, UnityEngine.Random.Range(8f, 15f));
		this.swimMoveOffset = Vector3.zero;
		this.layer = 26;
		this.layerMask = 1 << this.layer;
	}

	
	private void Update()
	{
		if (!this.animator.enabled && this.ai.doMove)
		{
			this.rot = Quaternion.identity;
			this.wantedDir = this.ai.wantedDir;
			if (this.ai.wantedDir != Vector3.zero)
			{
				Vector3 forward = this.ai.wantedDir;
				forward.y = 0f;
				this.rot = Quaternion.LookRotation(forward, Vector3.up);
			}
			this.Tr.rotation = this.rot;
			this.swimMoveOffset += this.wantedDir * Time.deltaTime;
		}
		if (this.isSwimming)
		{
			this.swimMoveOffset += this.Tr.forward * this.swimSpeed * 0.1f;
		}
		else
		{
			this.tx = (this.Tr.position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
			this.tz = (this.Tr.position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
			this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
			this.Tr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.Tr.right, this.tNormal), this.tNormal), Time.deltaTime * 10f);
		}
	}

	
	private void enableSwimMove()
	{
		this.isSwimming = true;
	}

	
	private void randomDepth()
	{
	}

	
	private void LateUpdate()
	{
		this.rb.velocity = Vector3.zero;
	}

	
	private void OnAnimatorMove()
	{
		if (this.animator)
		{
			this.moveDir = this.animator.deltaPosition;
			Vector3 vector = this.Tr.position;
			if (this.blocked && this.blockedCollider)
			{
				Vector3 vector2 = this.avoidance.transform.InverseTransformPoint(this.blockedCollider.transform.position);
				Vector3 normalized = (vector - this.blockedCollider.transform.position).normalized;
				Vector3 vector3 = Vector3.Cross(normalized, Vector3.up);
				this.animator.applyRootMotion = false;
				if (vector2.x >= 0f)
				{
					if (vector2.x > 0f)
					{
					}
				}
				vector += normalized * this.animSpeed * Time.deltaTime;
			}
			else
			{
				vector += this.moveDir;
				this.animator.applyRootMotion = true;
			}
			if (!this.ocean)
			{
				return;
			}
			this.oceanHeight = vector.y - this.ocean.position.y;
			this.terrainPosY = Terrain.activeTerrain.SampleHeight(vector) + Terrain.activeTerrain.transform.position.y;
			if (this.oceanHeight > -0.9f)
			{
				if (this.terrainPosY != vector.y)
				{
					this.terrainPosY = Mathf.Lerp(vector.y, this.terrainPosY, Time.deltaTime);
					vector.y = this.terrainPosY;
				}
				this.animator.SetBoolReflected("swimming", false);
				this.isSwimming = false;
			}
			else
			{
				this.oceanDepth = this.ocean.position.y - this.terrainPosY;
				if (this.oceanDepth > 0.5f)
				{
					this.terrainPosY = Mathf.Lerp(vector.y, this.terrainPosY + this.oceanDepth / 2f - 0.3f, Time.deltaTime * this.depthSpeed);
					vector.y = this.terrainPosY;
				}
				this.animator.SetBoolReflected("swimming", true);
				this.ai.fsmRotateSpeed.Value = 0.6f;
				base.Invoke("enableSwimMove", 1f);
			}
			this.rb.velocity = Vector3.zero;
			this.rb.MovePosition(vector + this.swimMoveOffset);
			this.swimMoveOffset = Vector3.zero;
		}
	}

	
	private turtleAvoidance avoidance;

	
	private Animator animator;

	
	private animalAI ai;

	
	private Transform Tr;

	
	private Vector3 wantedDir;

	
	private Rigidbody rb;

	
	private AnimatorStateInfo currLayerState0;

	
	private AnimatorStateInfo nextLayerState0;

	
	public float gravity;

	
	private Vector3 moveDir = Vector3.zero;

	
	private float terrainPosY;

	
	private Quaternion rot;

	
	private Transform ocean;

	
	private float oceanHeight;

	
	public float oceanDepth;

	
	public float swimSpeed;

	
	public float depthSpeed;

	
	private bool isSwimming;

	
	public bool blocked;

	
	public Collider blockedCollider;

	
	public float animSpeed = 0.5f;

	
	private int layer;

	
	private int layerMask;

	
	private RaycastHit hit;

	
	private Vector3 pos;

	
	private Vector3 tNormal;

	
	private Vector3 swimMoveOffset;

	
	private float tx;

	
	private float tz;
}
