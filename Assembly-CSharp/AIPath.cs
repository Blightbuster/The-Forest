using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AIPath (3D)")]
[HelpURL("http://arongranberg.com/astar/docs/class_a_i_path.php")]
public class AIPath : MonoBehaviour
{
	public bool TargetReached
	{
		get
		{
			return this.targetReached;
		}
	}

	protected virtual void Awake()
	{
		this.seeker = base.GetComponent<Seeker>();
		this.tr = base.transform;
		this.controller = base.GetComponent<CharacterController>();
		this.rvoController = base.GetComponent<RVOController>();
		this.rigid = base.GetComponent<Rigidbody>();
	}

	protected virtual void Start()
	{
		this.startHasRun = true;
		this.OnEnable();
	}

	protected virtual void OnEnable()
	{
		this.lastRepath = -9999f;
		this.canSearchAgain = true;
		this.lastFoundWaypointPosition = this.GetFeetPosition();
		if (this.startHasRun)
		{
			Seeker seeker = this.seeker;
			seeker.pathCallback = (OnPathDelegate)Delegate.Combine(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
			base.StartCoroutine(this.RepeatTrySearchPath());
		}
	}

	public void OnDisable()
	{
		if (this.seeker != null && !this.seeker.IsDone())
		{
			this.seeker.GetCurrentPath().Error();
		}
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = null;
		Seeker seeker = this.seeker;
		seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
	}

	protected IEnumerator RepeatTrySearchPath()
	{
		for (;;)
		{
			float v = this.TrySearchPath();
			yield return new WaitForSeconds(v);
		}
		yield break;
	}

	public float TrySearchPath()
	{
		if (Time.time - this.lastRepath >= this.repathRate && this.canSearchAgain && this.canSearch && this.target != null)
		{
			this.SearchPath();
			return this.repathRate;
		}
		float num = this.repathRate - (Time.time - this.lastRepath);
		return (num >= 0f) ? num : 0f;
	}

	public virtual void SearchPath()
	{
		if (this.target == null)
		{
			throw new InvalidOperationException("Target is null");
		}
		this.lastRepath = Time.time;
		Vector3 position = this.target.position;
		this.canSearchAgain = false;
		this.seeker.StartPath(this.GetFeetPosition(), position);
	}

	public virtual void OnTargetReached()
	{
	}

	public virtual void OnPathComplete(Path _p)
	{
		ABPath abpath = _p as ABPath;
		if (abpath == null)
		{
			throw new Exception("This function only handles ABPaths, do not use special path types");
		}
		this.canSearchAgain = true;
		abpath.Claim(this);
		if (abpath.error)
		{
			abpath.Release(this, false);
			return;
		}
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = abpath;
		this.currentWaypointIndex = 0;
		this.targetReached = false;
		if (this.closestOnPathCheck)
		{
			Vector3 vector = (Time.time - this.lastFoundWaypointTime >= 0.3f) ? abpath.originalStartPoint : this.lastFoundWaypointPosition;
			Vector3 feetPosition = this.GetFeetPosition();
			Vector3 vector2 = feetPosition - vector;
			float magnitude = vector2.magnitude;
			vector2 /= magnitude;
			int num = (int)(magnitude / this.pickNextWaypointDist);
			for (int i = 0; i <= num; i++)
			{
				this.CalculateVelocity(vector);
				vector += vector2;
			}
		}
	}

	public virtual Vector3 GetFeetPosition()
	{
		if (this.rvoController != null)
		{
			return this.tr.position - Vector3.up * this.rvoController.height * 0.5f;
		}
		if (this.controller != null)
		{
			return this.tr.position - Vector3.up * this.controller.height * 0.5f;
		}
		return this.tr.position;
	}

	public virtual void Update()
	{
		if (!this.canMove)
		{
			return;
		}
		Vector3 vector = this.CalculateVelocity(this.GetFeetPosition());
		this.RotateTowards(this.targetDirection);
		if (this.rvoController != null)
		{
			this.rvoController.Move(vector);
			vector = this.rvoController.velocity;
		}
		if (this.controller != null)
		{
			this.controller.SimpleMove(vector);
		}
		else if (this.rigid != null)
		{
			this.rigid.AddForce(vector);
		}
		else
		{
			this.tr.Translate(vector * Time.deltaTime, Space.World);
		}
	}

	protected float XZSqrMagnitude(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	private static Vector2 To2D(Vector3 p)
	{
		return new Vector2(p.x, p.z);
	}

	protected Vector3 CalculateVelocity(Vector3 currentPosition)
	{
		if (this.path == null || this.path.vectorPath == null || this.path.vectorPath.Count == 0)
		{
			return Vector3.zero;
		}
		List<Vector3> vectorPath = this.path.vectorPath;
		if (vectorPath.Count == 1)
		{
			vectorPath.Insert(0, currentPosition);
		}
		if (this.currentWaypointIndex >= vectorPath.Count)
		{
			this.currentWaypointIndex = vectorPath.Count - 1;
		}
		if (this.currentWaypointIndex <= 1)
		{
			this.currentWaypointIndex = 1;
		}
		while (this.currentWaypointIndex < vectorPath.Count - 1)
		{
			float num = VectorMath.SqrDistanceXZ(vectorPath[this.currentWaypointIndex], currentPosition);
			if (num >= this.pickNextWaypointDist * this.pickNextWaypointDist)
			{
				break;
			}
			this.lastFoundWaypointPosition = currentPosition;
			this.lastFoundWaypointTime = Time.time;
			this.currentWaypointIndex++;
		}
		Vector3 vector = vectorPath[this.currentWaypointIndex - 1];
		Vector3 vector2 = vectorPath[this.currentWaypointIndex];
		float num2 = VectorMath.LineCircleIntersectionFactor(AIPath.To2D(currentPosition), AIPath.To2D(vector), AIPath.To2D(vector2), this.pickNextWaypointDist);
		num2 = Mathf.Clamp01(num2);
		Vector3 a = Vector3.Lerp(vector, vector2, num2);
		Vector3 vector3 = a - currentPosition;
		vector3.y = 0f;
		float magnitude = vector3.magnitude;
		float num3 = (this.slowdownDistance <= 0f) ? 1f : Mathf.Clamp01(magnitude / this.slowdownDistance);
		this.targetDirection = vector3;
		this.targetPoint = a;
		if (this.currentWaypointIndex == vectorPath.Count - 1 && magnitude <= this.endReachedDistance)
		{
			if (!this.targetReached)
			{
				this.targetReached = true;
				this.OnTargetReached();
			}
			return Vector3.zero;
		}
		Vector3 forward = this.tr.forward;
		float a2 = Vector3.Dot(vector3.normalized, forward);
		float num4 = this.speed * Mathf.Max(a2, this.minMoveScale) * num3;
		if (Time.deltaTime > 0f)
		{
			num4 = Mathf.Clamp(num4, 0f, magnitude / (Time.deltaTime * 2f));
		}
		return forward * num4;
	}

	protected virtual void RotateTowards(Vector3 dir)
	{
		if (dir == Vector3.zero)
		{
			return;
		}
		Quaternion quaternion = this.tr.rotation;
		Quaternion b = Quaternion.LookRotation(dir);
		Vector3 eulerAngles = Quaternion.Slerp(quaternion, b, this.turningSpeed * Time.deltaTime).eulerAngles;
		eulerAngles.z = 0f;
		eulerAngles.x = 0f;
		quaternion = Quaternion.Euler(eulerAngles);
		this.tr.rotation = quaternion;
	}

	public float repathRate = 0.5f;

	public Transform target;

	public bool canSearch = true;

	public bool canMove = true;

	public float speed = 3f;

	public float turningSpeed = 5f;

	public float slowdownDistance = 0.6f;

	public float pickNextWaypointDist = 2f;

	public float endReachedDistance = 0.2f;

	public bool closestOnPathCheck = true;

	protected float minMoveScale = 0.05f;

	protected Seeker seeker;

	protected Transform tr;

	protected float lastRepath = -9999f;

	protected Path path;

	protected CharacterController controller;

	protected RVOController rvoController;

	protected Rigidbody rigid;

	protected int currentWaypointIndex;

	protected bool targetReached;

	protected bool canSearchAgain = true;

	protected Vector3 lastFoundWaypointPosition;

	protected float lastFoundWaypointTime = -9999f;

	private bool startHasRun;

	protected Vector3 targetPoint;

	protected Vector3 targetDirection;
}
