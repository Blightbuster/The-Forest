using System;
using UnityEngine;

public class tortoiseAnimatorControl : MonoBehaviour
{
	private void Awake()
	{
		this.ai = base.transform.parent.GetComponent<animalAI>();
		this.animator = base.GetComponent<Animator>();
		this.Tr = base.transform.parent;
		this.rotateTr = base.transform;
		this.layer = 26;
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
			this.Tr.Translate(this.wantedDir * Time.deltaTime, Space.World);
		}
	}

	private void OnAnimatorMove()
	{
		if (this.animator.enabled)
		{
			this.moveDir = this.animator.deltaPosition;
			this.Tr.position += this.moveDir;
			this.terrainPos = Terrain.activeTerrain.SampleHeight(this.Tr.position) + Terrain.activeTerrain.transform.position.y;
			this.Tr.position = new Vector3(this.Tr.position.x, this.terrainPos, this.Tr.position.z);
			this.tx = (this.Tr.position.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x;
			this.tz = (this.Tr.position.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z;
			this.tNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(this.tx, this.tz);
			this.rotateTr.rotation = Quaternion.Lerp(this.animator.rootRotation, Quaternion.LookRotation(Vector3.Cross(this.rotateTr.right, this.tNormal), this.tNormal), Time.deltaTime * 10f);
		}
	}

	private Animator animator;

	private animalAI ai;

	private Transform Tr;

	private Transform rotateTr;

	private Vector3 wantedDir;

	private AnimatorStateInfo currLayerState0;

	private AnimatorStateInfo nextLayerState0;

	public float gravity;

	private Vector3 moveDir = Vector3.zero;

	public bool turnClose;

	private int layer;

	private int layerMask2;

	private float terrainPos;

	private RaycastHit hit;

	private RaycastHit hit2;

	private Vector3 pos;

	private float tx;

	private float tz;

	private Vector3 tNormal;
}
