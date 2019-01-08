using System;
using System.Collections;
using Pathfinding;
using TheForest.Utils;
using UnityEngine;

public class wormWalkerController : MonoBehaviour
{
	private void Start()
	{
		this.thisSkin = base.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		if (this.thisSkin && !this.DebugMovement)
		{
			this.thisSkin.enabled = false;
		}
		this.animator = base.transform.GetComponentInChildren<Animator>();
		this.rootTr = base.transform;
		this.controller = base.transform.GetComponent<CharacterController>();
		this.attachControl = base.transform.GetComponent<wormAttachController>();
		this.seeker = base.transform.GetComponentInChildren<Seeker>();
		this.dripWormTimer = Time.time + 7f;
		this.detachFormTimer = Time.time + 20f;
	}

	private void setHiveController(wormHiveController set)
	{
		this.hiveController = set;
	}

	private void Update()
	{
		if (!this.DebugMovement)
		{
			this.UpdateAttachPoints();
		}
		if (Time.time > this.dripWormTimer)
		{
			this.DetachRandomWorm();
			this.dripWormTimer = Time.time + (float)UnityEngine.Random.Range(3, 7);
		}
		this.playerDist = Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position);
		if (this.currentAttachedWorms > 25 || this.DebugMovement)
		{
			this.animator.SetBool("wakeUp", true);
		}
		if (this.playerDist > 15f && this.animator.GetBool("wakeUp"))
		{
			this.MoveTowardTarget();
			this.animator.SetBool("walk", true);
			this.detachFormTimer = Time.time + 5f;
		}
		else
		{
			if (Time.time > this.detachFormTimer)
			{
				base.StartCoroutine(this.DetachAllWormsDelayedUp(0));
				base.StartCoroutine(this.DetachAllWormsDelayedDown(0));
				this.delayedDetach = true;
				this.attachControl.canAttach = false;
			}
			this.animator.SetBool("walk", false);
		}
	}

	private void UpdateAttachPoints()
	{
		this.currentAttachedWorms = 0;
		for (int i = 0; i < this.attachControl.AttachPoints.Length; i++)
		{
			bool flag = false;
			wormAttachPoints wormAttachPoints = this.attachControl.AttachPoints[i];
			foreach (Transform x in wormAttachPoints.AttachedWorm)
			{
				if (x != null)
				{
					this.currentAttachedWorms++;
					flag = true;
				}
			}
			if (i > 0 && flag)
			{
				bool flag2 = false;
				int num = i - 1;
				foreach (Transform x2 in this.attachControl.AttachPoints[num].AttachedWorm)
				{
					if (x2 != null)
					{
						flag2 = true;
					}
				}
				if (!flag2 && !this.delayedDetach)
				{
					base.StartCoroutine(this.DetachAllWormsDelayedUp(i));
					base.StartCoroutine(this.DetachAllWormsDelayedDown(i));
					this.delayedDetach = true;
					this.attachControl.canAttach = false;
					break;
				}
			}
		}
		if (this.currentAttachedWorms == 0 && this.delayedDetach)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void MoveTowardTarget()
	{
		this.target = Scene.SceneTracker.GetClosestPlayerFromPos(base.transform.position).transform.position;
		this.moveDir = this.animator.deltaPosition;
		this.moveDir.y = this.moveDir.y - this.gravity * Time.deltaTime;
		if (Time.time > this.nextPathTimer)
		{
			this.generatePathToTarget();
			this.nextPathTimer = Time.time + 1f;
		}
		this.UpdatePathTarget();
		this.controller.Move(this.moveDir);
		this.rootSmoothLookAtDir(this.targetWaypoint, 2f);
	}

	private void DetachRandomWorm()
	{
		bool flag = false;
		for (int i = 0; i < this.attachControl.AttachPoints.Length; i++)
		{
			i = UnityEngine.Random.Range(0, this.attachControl.AttachPoints.Length);
			wormAttachPoints wormAttachPoints = this.attachControl.AttachPoints[i];
			for (int j = 0; j < wormAttachPoints.AttachedWorm.Length; j++)
			{
				if (wormAttachPoints.AttachedWorm[j] != null)
				{
					wormAttachPoints.AttachedWorm[j].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					wormAttachPoints.AttachedWorm[j] = null;
					if (wormAttachPoints.currentEmptySlot > 0)
					{
						wormAttachPoints.currentEmptySlot--;
					}
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	private void DetachAllWorms()
	{
		foreach (wormAttachPoints wormAttachPoints in this.attachControl.AttachPoints)
		{
			for (int j = 0; j < wormAttachPoints.AttachedWorm.Length; j++)
			{
				if (wormAttachPoints.AttachedWorm[j] != null)
				{
					wormAttachPoints.AttachedWorm[j].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					wormAttachPoints.AttachedWorm[j] = null;
				}
			}
			wormAttachPoints.currentEmptySlot = 0;
		}
		float num = Vector3.Distance(LocalPlayer.Transform.position, base.transform.position);
		if (num < 26f)
		{
			LocalPlayer.HitReactions.enableFootShake(num, 0.3f);
		}
		if (this.hiveController)
		{
			this.hiveController.spawnFormCoolDown = Time.time + 4f;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator DetachAllWormsDelayedUp(int startSegment)
	{
		for (int index = startSegment; index < this.attachControl.AttachPoints.Length; index++)
		{
			wormAttachPoints t = this.attachControl.AttachPoints[index];
			for (int i = 0; i < t.AttachedWorm.Length; i++)
			{
				if (t.AttachedWorm[i] != null)
				{
					t.AttachedWorm[i].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					t.AttachedWorm[i] = null;
				}
			}
			t.currentEmptySlot = 0;
			yield return YieldPresets.WaitPointOneSeconds;
		}
		yield break;
	}

	private IEnumerator DetachAllWormsDelayedDown(int startSegment)
	{
		for (int index = startSegment; index > -1; index--)
		{
			wormAttachPoints t = this.attachControl.AttachPoints[index];
			for (int i = 0; i < t.AttachedWorm.Length; i++)
			{
				if (t.AttachedWorm[i] != null)
				{
					t.AttachedWorm[i].SendMessage("Detach", SendMessageOptions.DontRequireReceiver);
					t.AttachedWorm[i] = null;
				}
			}
			t.currentEmptySlot = 0;
			yield return YieldPresets.WaitPointOneSeconds;
		}
		yield break;
	}

	public virtual void SearchPath()
	{
		this.seeker.StartPath(base.transform.position, this.target, new OnPathDelegate(this.OnPathComplete));
	}

	public void OnPathComplete(Path p)
	{
		if (!p.error)
		{
			if (this.path != null)
			{
				this.path.Release(this, false);
			}
			this.path = p;
			this.currentWaypoint = 1;
			this.path.Claim(this);
		}
		else
		{
			Debug.Log("no path to target");
		}
	}

	private void generatePathToTarget()
	{
		this.SearchPath();
	}

	private void UpdatePathTarget()
	{
		if (this.path == null)
		{
			return;
		}
		if (this.currentWaypoint < this.path.vectorPath.Count - 1)
		{
			Vector3 a = this.path.vectorPath[this.currentWaypoint];
			a.y = base.transform.position.y;
			float num = this.XZSqrMagnitude(a, base.transform.position);
			if (num < this.nextWaypointDistance * this.nextWaypointDistance)
			{
				this.currentWaypoint++;
			}
			this.targetWaypoint = this.path.vectorPath[this.currentWaypoint];
			this.targetWaypoint.y = base.transform.position.y;
		}
		else
		{
			this.targetWaypoint = this.target;
		}
	}

	protected float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	private void rootSmoothLookAtDir(Vector3 lookAtPos, float speed)
	{
		lookAtPos.y = base.transform.position.y;
		Vector3 vector = lookAtPos - base.transform.position;
		Quaternion quaternion = base.transform.rotation;
		if (vector != Vector3.zero && vector.sqrMagnitude > 0f)
		{
			this.desiredRotation = Quaternion.LookRotation(vector, Vector3.up);
		}
		quaternion = Quaternion.Slerp(quaternion, this.desiredRotation, speed * Time.deltaTime);
		this.rootTr.rotation = quaternion;
	}

	public wormHiveController hiveController;

	private wormAttachController attachControl;

	private Animator animator;

	private SkinnedMeshRenderer thisSkin;

	public int currentAttachedWorms;

	private Quaternion desiredRotation;

	private float dripWormTimer;

	private float detachFormTimer;

	private CharacterController controller;

	private Transform rootTr;

	public Transform pathTarget;

	public Seeker seeker;

	public Path path;

	private float nextPathTimer;

	public float speed = 5f;

	public float gravity = 10f;

	public float nextWaypointDistance = 3f;

	public int currentWaypoint;

	public bool cansearch;

	public bool isPath;

	public Vector3 target;

	private Vector3 moveDir;

	public float playerDist;

	public bool delayedDetach;

	public bool DebugMovement;

	public Vector3 targetWaypoint;
}
