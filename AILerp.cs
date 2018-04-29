using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;


[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/AI/AISimpleLerp (2D,3D generic)")]
[HelpURL("http:
public class AILerp : MonoBehaviour
{
	
	
	
	public bool targetReached { get; private set; }

	
	protected virtual void Awake()
	{
		this.tr = base.transform;
		this.seeker = base.GetComponent<Seeker>();
		this.seeker.startEndModifier.adjustStartPoint = (() => this.tr.position);
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
		this.ForceSearchPath();
	}

	
	public virtual void ForceSearchPath()
	{
		if (this.target == null)
		{
			throw new InvalidOperationException("Target is null");
		}
		this.lastRepath = Time.time;
		Vector3 position = this.target.position;
		Vector3 start = this.GetFeetPosition();
		if (this.path != null && this.path.vectorPath.Count > 1)
		{
			start = this.path.vectorPath[this.currentWaypointIndex];
		}
		this.canSearchAgain = false;
		this.seeker.StartPath(start, position);
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
		if (this.interpolatePathSwitches)
		{
			this.ConfigurePathSwitchInterpolation();
		}
		if (this.path != null)
		{
			this.path.Release(this, false);
		}
		this.path = abpath;
		if (this.path.vectorPath != null && this.path.vectorPath.Count == 1)
		{
			this.path.vectorPath.Insert(0, this.GetFeetPosition());
		}
		this.targetReached = false;
		this.ConfigureNewPath();
	}

	
	protected virtual void ConfigurePathSwitchInterpolation()
	{
		bool flag = this.path != null && this.path.vectorPath != null && this.path.vectorPath.Count > 1;
		bool flag2 = false;
		if (flag)
		{
			flag2 = (this.currentWaypointIndex == this.path.vectorPath.Count - 1 && this.distanceAlongSegment >= (this.path.vectorPath[this.path.vectorPath.Count - 1] - this.path.vectorPath[this.path.vectorPath.Count - 2]).magnitude);
		}
		if (flag && !flag2)
		{
			List<Vector3> vectorPath = this.path.vectorPath;
			this.currentWaypointIndex = Mathf.Clamp(this.currentWaypointIndex, 1, vectorPath.Count - 1);
			Vector3 vector = vectorPath[this.currentWaypointIndex] - vectorPath[this.currentWaypointIndex - 1];
			float magnitude = vector.magnitude;
			float num = magnitude * Mathf.Clamp01(1f - this.distanceAlongSegment);
			for (int i = this.currentWaypointIndex; i < vectorPath.Count - 1; i++)
			{
				num += (vectorPath[i + 1] - vectorPath[i]).magnitude;
			}
			this.previousMovementOrigin = this.GetFeetPosition();
			this.previousMovementDirection = vector.normalized * num;
			this.previousMovementStartTime = Time.time;
		}
		else
		{
			this.previousMovementOrigin = Vector3.zero;
			this.previousMovementDirection = Vector3.zero;
			this.previousMovementStartTime = -9999f;
		}
	}

	
	public virtual Vector3 GetFeetPosition()
	{
		return this.tr.position;
	}

	
	protected virtual void ConfigureNewPath()
	{
		List<Vector3> vectorPath = this.path.vectorPath;
		Vector3 feetPosition = this.GetFeetPosition();
		float num = 0f;
		float num2 = float.PositiveInfinity;
		Vector3 vector = Vector3.zero;
		int num3 = 1;
		for (int i = 0; i < vectorPath.Count - 1; i++)
		{
			float num4 = VectorMath.ClosestPointOnLineFactor(vectorPath[i], vectorPath[i + 1], feetPosition);
			num4 = Mathf.Clamp01(num4);
			Vector3 b = Vector3.Lerp(vectorPath[i], vectorPath[i + 1], num4);
			float sqrMagnitude = (feetPosition - b).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				num2 = sqrMagnitude;
				vector = vectorPath[i + 1] - vectorPath[i];
				num = num4 * vector.magnitude;
				num3 = i + 1;
			}
		}
		this.currentWaypointIndex = num3;
		this.distanceAlongSegment = num;
		if (this.interpolatePathSwitches && this.switchPathInterpolationSpeed > 0.01f)
		{
			float num5 = Mathf.Max(-Vector3.Dot(this.previousMovementDirection.normalized, vector.normalized), 0f);
			this.distanceAlongSegment -= this.speed * num5 * (1f / this.switchPathInterpolationSpeed);
		}
	}

	
	protected virtual void Update()
	{
		if (this.canMove)
		{
			Vector3 vector;
			Vector3 position = this.CalculateNextPosition(out vector);
			if (this.enableRotation && vector != Vector3.zero)
			{
				if (this.rotationIn2D)
				{
					float b = Mathf.Atan2(vector.x, -vector.y) * 57.29578f + 180f;
					Vector3 eulerAngles = this.tr.eulerAngles;
					eulerAngles.z = Mathf.LerpAngle(eulerAngles.z, b, Time.deltaTime * this.rotationSpeed);
					this.tr.eulerAngles = eulerAngles;
				}
				else
				{
					Quaternion rotation = this.tr.rotation;
					Quaternion b2 = Quaternion.LookRotation(vector);
					this.tr.rotation = Quaternion.Slerp(rotation, b2, Time.deltaTime * this.rotationSpeed);
				}
			}
			this.tr.position = position;
		}
	}

	
	protected virtual Vector3 CalculateNextPosition(out Vector3 direction)
	{
		if (this.path == null || this.path.vectorPath == null || this.path.vectorPath.Count == 0)
		{
			direction = Vector3.zero;
			return this.tr.position;
		}
		List<Vector3> vectorPath = this.path.vectorPath;
		this.currentWaypointIndex = Mathf.Clamp(this.currentWaypointIndex, 1, vectorPath.Count - 1);
		Vector3 vector = vectorPath[this.currentWaypointIndex] - vectorPath[this.currentWaypointIndex - 1];
		float num = vector.magnitude;
		this.distanceAlongSegment += Time.deltaTime * this.speed;
		if (this.distanceAlongSegment >= num && this.currentWaypointIndex < vectorPath.Count - 1)
		{
			float num2 = this.distanceAlongSegment - num;
			Vector3 vector2;
			float magnitude;
			for (;;)
			{
				this.currentWaypointIndex++;
				vector2 = vectorPath[this.currentWaypointIndex] - vectorPath[this.currentWaypointIndex - 1];
				magnitude = vector2.magnitude;
				if (num2 <= magnitude || this.currentWaypointIndex == vectorPath.Count - 1)
				{
					break;
				}
				num2 -= magnitude;
			}
			vector = vector2;
			num = magnitude;
			this.distanceAlongSegment = num2;
		}
		if (this.distanceAlongSegment >= num && this.currentWaypointIndex == vectorPath.Count - 1)
		{
			if (!this.targetReached)
			{
				this.OnTargetReached();
			}
			this.targetReached = true;
		}
		Vector3 vector3 = vector * Mathf.Clamp01((num <= 0f) ? 1f : (this.distanceAlongSegment / num)) + vectorPath[this.currentWaypointIndex - 1];
		direction = vector;
		if (this.interpolatePathSwitches)
		{
			Vector3 a = this.previousMovementOrigin + Vector3.ClampMagnitude(this.previousMovementDirection, this.speed * (Time.time - this.previousMovementStartTime));
			return Vector3.Lerp(a, vector3, this.switchPathInterpolationSpeed * (Time.time - this.previousMovementStartTime));
		}
		return vector3;
	}

	
	public float repathRate = 0.5f;

	
	public Transform target;

	
	public bool canSearch = true;

	
	public bool canMove = true;

	
	public float speed = 3f;

	
	public bool enableRotation = true;

	
	public bool rotationIn2D;

	
	public float rotationSpeed = 10f;

	
	public bool interpolatePathSwitches = true;

	
	public float switchPathInterpolationSpeed = 5f;

	
	protected Seeker seeker;

	
	protected Transform tr;

	
	protected float lastRepath = -9999f;

	
	protected ABPath path;

	
	protected int currentWaypointIndex;

	
	protected float distanceAlongSegment;

	
	protected bool canSearchAgain = true;

	
	protected Vector3 previousMovementOrigin;

	
	protected Vector3 previousMovementDirection;

	
	protected float previousMovementStartTime = -9999f;

	
	private bool startHasRun;
}
