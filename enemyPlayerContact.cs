using System;
using TheForest.Utils;
using UnityEngine;


public class enemyPlayerContact : MonoBehaviour
{
	
	private void Start()
	{
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.setup = base.transform.root.GetComponentInChildren<mutantScriptSetup>();
		this.stats = base.transform.GetComponent<targetStats>();
		this.tr = base.transform;
		this.netCollider = base.transform.GetComponent<CapsuleCollider>();
		this.rb = base.transform.GetComponent<Rigidbody>();
		this.netAnimControl = base.transform.GetComponentInChildren<mutantNetAnimatorControl>();
		this.adjustDist = 1.8f;
		if (this.setup != null && this.setup.controller == null)
		{
			this.babyCollider = base.transform.GetComponent<SphereCollider>();
		}
	}

	
	private void adjustEnemyPosition(Vector3 target)
	{
		this.inContact = true;
		Vector3 a = target;
		a.y = this.tr.position.y;
		a = (a - this.tr.position).normalized;
		Vector3 a2 = target;
		a2.y = this.tr.position.y;
		this.tr.position = a2 + a * -(this.adjustDist + this.currRadius);
		this.oldPos = this.tr.position;
	}

	
	private void LateUpdate()
	{
		if (this.net)
		{
			return;
		}
		if (this.stats && this.stats.targetDown)
		{
			return;
		}
		if (this.net)
		{
			this.currRadius = this.netCollider.radius;
		}
		else if (this.setup.ai.creepy_baby)
		{
			this.currRadius = this.babyCollider.radius;
		}
		else
		{
			this.currRadius = this.setup.controller.radius;
		}
		float num;
		if (this.net)
		{
			num = this.netAnimControl.localPlayerDist;
		}
		else
		{
			num = this.setup.ai.mainPlayerDist;
		}
		if (num < 9f)
		{
			if (this.net)
			{
				this.closestPlayer = LocalPlayer.GameObject;
			}
			else
			{
				this.closestPlayer = Scene.SceneTracker.GetClosestPlayerFromPos(this.tr.position);
			}
			if (this.closestPlayer)
			{
				Vector3 position = this.closestPlayer.transform.position;
				position.y = this.tr.position.y;
				float num2;
				if (this.net)
				{
					num2 = this.netCollider.height;
				}
				else if (this.setup.ai.creepy_baby)
				{
					num2 = this.babyCollider.radius * 2f;
				}
				else
				{
					num2 = this.setup.controller.height;
				}
				float num3 = this.closestPlayer.transform.position.y - (this.tr.position.y + num2 / 2f);
				bool flag = false;
				if (num3 < -num2 || num3 > num2)
				{
					flag = true;
				}
				if (Vector3.Distance(this.tr.position, position) < this.adjustDist + this.currRadius && !flag)
				{
					this.adjustEnemyPosition(this.closestPlayer.transform.position);
					this.inContact = true;
				}
			}
		}
	}

	
	private CapsuleCollider netCollider;

	
	private SphereCollider babyCollider;

	
	private Transform tr;

	
	public Transform trBase;

	
	public bool inContact;

	
	private Vector3 oldPos;

	
	private mutantScriptSetup setup;

	
	private Animator animator;

	
	private targetStats stats;

	
	private mutantNetAnimatorControl netAnimControl;

	
	private Rigidbody rb;

	
	public float adjustDist = 2f;

	
	public bool net;

	
	private GameObject closestPlayer;

	
	private float currRadius;
}
