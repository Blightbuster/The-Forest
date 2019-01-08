using System;
using System.Collections;
using UnityEngine;

public class animalAnimatorControl : MonoBehaviour
{
	private void Awake()
	{
		this.rb = base.transform.parent.GetComponent<Rigidbody>();
		this.col = base.transform.parent.GetComponent<SphereCollider>();
		this.ai = base.transform.parent.GetComponent<animalAI>();
		this.animator = base.GetComponent<Animator>();
		this.waterChecker = base.transform.GetComponentInChildren<inWaterChecker>();
		this.Tr = base.transform.parent;
		this.rotateTr = base.transform;
		if (!this.hit)
		{
			this.hit = base.transform.GetComponentInChildren<animalHitReceiver>();
		}
		if (this.boar)
		{
			this.animator.speed = 1.5f;
		}
	}

	private void Start()
	{
		this.smoothPos = this.Tr.position;
	}

	private void OnEnable()
	{
		this.smoothPos = this.Tr.position;
		if (!this.col)
		{
			this.col = base.transform.parent.GetComponent<SphereCollider>();
		}
		if (!this.animator)
		{
			this.animator = base.transform.GetComponent<Animator>();
		}
		this.smoothPos = this.Tr.position;
		this.fixPositionDelay = Time.time + 0.5f;
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
		if (this.typeLizard || this.typeRaccoon)
		{
			if (!this.animator.GetBool("treeClimb"))
			{
				this.terrainPos = Terrain.activeTerrain.SampleHeight(this.rb.position) + Terrain.activeTerrain.transform.position.y;
			}
			else
			{
				this.terrainPos = this.Tr.position.y;
			}
		}
		else
		{
			this.terrainPos = Terrain.activeTerrain.SampleHeight(this.rb.position) + Terrain.activeTerrain.transform.position.y;
		}
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
			if (this.useRB && Time.time > this.fixPositionDelay)
			{
				if (this.spineCentre)
				{
					Vector3 center = this.Tr.InverseTransformPoint(this.spineCentre.transform.position);
					this.col.center = center;
				}
				Vector3 position = this.Tr.position;
				position.y = this.terrainPos;
				this.rb.MovePosition(position + this.wantedDir.normalized * 0.1f);
			}
			else
			{
				this.Tr.Translate(this.wantedDir * Time.deltaTime * (this.animator.GetFloat("Speed") * this.offScreenSpeed), Space.World);
			}
		}
	}

	private void LateUpdate()
	{
		this.rb.velocity = Vector3.zero;
		if (this.animator.enabled && Time.time > this.fixPositionDelay)
		{
			this.smoothPos = Vector3.Lerp(this.smoothPos, this.Tr.position, Time.deltaTime * 25f);
			this.rotateTr.position = this.smoothPos;
		}
		else
		{
			this.smoothPos = this.Tr.position;
			this.rotateTr.position = this.smoothPos;
		}
	}

	private void OnAnimatorMove()
	{
		if (this.animator.enabled)
		{
			this.moveDir = this.animator.deltaPosition;
			if (this.moveDir.magnitude > 3f)
			{
				this.moveDir = this.moveDir.normalized * 3f;
			}
			if ((this.typeRaccoon || this.typeLizard) && this.animator.GetBool("treeClimb"))
			{
				this.rotateTr.rotation = Quaternion.Lerp(this.rotateTr.rotation, Quaternion.LookRotation(Vector3.Cross(this.rotateTr.right, Vector3.up), Vector3.up), Time.deltaTime * 4f);
			}
			else
			{
				if (this.typeCrocodile)
				{
					if (this.waterChecker.swimming)
					{
						this.rotateToTerrainSpeed = 0.2f;
						float y = this.Tr.position.y;
						this.terrainPos = Mathf.Lerp(y, this.waterChecker.waterHeight, Time.deltaTime * this.rotateToTerrainSpeed);
					}
					else
					{
						this.terrainPos = Terrain.activeTerrain.SampleHeight(this.Tr.position) + Terrain.activeTerrain.transform.position.y;
						this.rotateToTerrainSpeed = 3f;
					}
				}
				this.tx = (this.Tr.position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
				this.tz = (this.Tr.position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
				if (this.typeCrocodile && this.waterChecker.swimming)
				{
					this.tNormal = Vector3.up;
				}
				else
				{
					this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
				}
				if (this.tNormal != Vector3.zero && this.tNormal.sqrMagnitude > 0f)
				{
					this.rotateTr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.rotateTr.right, this.tNormal), this.tNormal), Time.fixedDeltaTime * this.rotateToTerrainSpeed);
				}
			}
			if (this.useRB && Time.time > this.fixPositionDelay)
			{
				if (this.spineCentre)
				{
					Vector3 center = this.Tr.InverseTransformPoint(this.spineCentre.transform.position);
					this.col.center = center;
				}
				Vector3 position = this.rb.position;
				position.y = this.terrainPos;
				this.rb.MovePosition(position + this.moveDir);
				if (this.animator.deltaPosition.magnitude < 0.05f)
				{
					this.rb.velocity = Vector3.zero;
				}
			}
			else
			{
				Vector3 position2 = this.Tr.position + this.moveDir;
				position2.y = this.terrainPos;
				this.Tr.position = position2;
			}
		}
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

	private IEnumerator lizardMoveToTreeTarget(Vector3 pos)
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

	private void enableRbMovement()
	{
		this.useRB = true;
		if (this.rb.isKinematic)
		{
			this.rb.isKinematic = false;
		}
		if (this.rb.IsSleeping())
		{
			this.rb.WakeUp();
		}
		if (!this.col.enabled)
		{
			if (this.typeLizard || this.typeRaccoon)
			{
				if (!this.animator.GetBool("treeClimb"))
				{
					this.col.enabled = true;
				}
			}
			else
			{
				this.col.enabled = true;
			}
			if (this.hit)
			{
				this.hit.disableBodyCollisions();
			}
		}
	}

	private void disableRbMovement()
	{
		if (this.col.enabled)
		{
			this.col.enabled = false;
		}
		if (!this.rb.isKinematic)
		{
			this.rb.isKinematic = true;
		}
		this.rb.Sleep();
		this.useRB = false;
	}

	private Rigidbody rb;

	private SphereCollider col;

	public Animator animator;

	private animalAI ai;

	public animalHitReceiver hit;

	private inWaterChecker waterChecker;

	private Transform Tr;

	private Transform rotateTr;

	private Vector3 wantedDir;

	public Transform spineCentre;

	private AnimatorStateInfo currLayerState0;

	private AnimatorStateInfo nextLayerState0;

	public float gravity = 16f;

	private Vector3 moveDir = Vector3.zero;

	private float terrainPos;

	private float tx;

	private float tz;

	private Vector3 tNormal;

	private Vector3 pos;

	private Vector3 prevPos;

	private Vector3 smoothPos;

	public bool useRB;

	public bool typeCrocodile;

	public bool typeRaccoon;

	public bool typeLizard;

	public bool boar;

	public float speedMultiplyer = 1.25f;

	public float offScreenSpeed = 4f;

	public float rotateToTerrainSpeed = 10f;

	private int idleHash = Animator.StringToHash("idle");

	private float fixPositionDelay;
}
