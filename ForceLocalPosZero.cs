using System;
using TheForest.Utils;
using UnityEngine;


public class ForceLocalPosZero : MonoBehaviour
{
	
	private void Start()
	{
		this.tr = base.transform;
		this.animator = base.transform.GetComponent<Animator>();
		this.localPos = base.transform.localPosition;
		this.storeCurrPos = base.transform.position;
		this.pos = base.transform.position;
		this.layerMask = 69345280;
	}

	
	private void OnEnable()
	{
		if (this.target)
		{
			this.pos = this.target.position;
		}
	}

	
	private void LateUpdate()
	{
		this.s2 = this.animator.GetCurrentAnimatorStateInfo(2);
		if (this.net)
		{
			this.s0 = this.animator.GetCurrentAnimatorStateInfo(0);
			if (this.s2.tagHash == this.deathTag)
			{
				this.pos = new Vector3(this.tr.position.x, this.tr.position.y + 5f, this.tr.position.z);
				if (Physics.Raycast(this.pos, Vector3.down, out this.hit, 20f, this.layerMask))
				{
					this.nv = this.hit.normal;
					this.tr.rotation = Quaternion.Lerp(this.tr.rotation, Quaternion.LookRotation(Vector3.Cross(this.tr.right, this.nv), this.nv), Time.deltaTime * 2.5f);
				}
			}
			else if (this.s0.tagHash == this.shellRideHash)
			{
				this.pos = new Vector3(this.tr.position.x, this.tr.position.y + 1f, this.tr.position.z);
				Vector2 v = Vector3.up;
				if (Physics.Raycast(this.pos, Vector3.down, out this.hit, 2f, this.layerMask))
				{
					v = this.hit.normal;
				}
				this.tr.rotation = Quaternion.Lerp(this.tr.rotation, Quaternion.LookRotation(Vector3.Cross(this.tr.right, v), v), Time.deltaTime * 6f);
			}
			else if (this.s2.tagHash == this.getupTag)
			{
				this.tr.transform.rotation = Quaternion.Lerp(this.tr.rotation, this.tr.parent.rotation, Time.deltaTime * 2.5f);
			}
			if (this.s2.tagHash != this.deathTag && this.s2.tagHash != this.getupTag)
			{
				base.transform.localEulerAngles = Vector3.zero;
			}
			base.transform.localPosition = this.localPos;
			return;
		}
		this.pos = Vector3.SmoothDamp(this.pos, this.target.position, ref this.velRef, this.smoothTime);
		if (LocalPlayer.FpCharacter.SailingRaft && Vector3.Distance(base.transform.position, this.target.position) > 0.01f)
		{
			base.transform.position = this.target.position;
		}
		else if (Vector3.Distance(base.transform.position, this.pos) > 0.01f)
		{
			base.transform.position = this.pos;
		}
		if (this.s2.tagHash != this.deathTag && this.s2.tagHash != this.getupTag && !LocalPlayer.AnimControl.doShellRideMode && base.transform.localEulerAngles != Vector3.zero)
		{
			base.transform.localEulerAngles = Vector3.zero;
		}
	}

	
	private void ForcedUpdate()
	{
		this.pos = this.target.position;
		base.transform.position = this.pos;
	}

	
	private Animator animator;

	
	private Transform tr;

	
	private Vector3 localPos;

	
	private Vector3 pos;

	
	private Vector3 velRef;

	
	private Vector3 storeCurrPos;

	
	private int deathTag = Animator.StringToHash("death");

	
	private int getupTag = Animator.StringToHash("getup");

	
	private int shellRideHash = Animator.StringToHash("shellRide");

	
	private AnimatorStateInfo s2;

	
	private AnimatorStateInfo s0;

	
	private Vector3 nv;

	
	private RaycastHit hit;

	
	private int layerMask;

	
	public Transform target;

	
	public float smoothTime = 0.06f;

	
	public bool net;
}
