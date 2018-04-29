using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.RVO;
using UnityEngine;

namespace Pathfinding
{
	
	[RequireComponent(typeof(Seeker))]
	[AddComponentMenu("Pathfinding/AI/RichAI (3D, for navmesh)")]
	[HelpURL("http:
	public class RichAI : MonoBehaviour
	{
		
		
		public Vector3 Velocity
		{
			get
			{
				return this.velocity;
			}
		}

		
		private void Awake()
		{
			this.seeker = base.GetComponent<Seeker>();
			this.controller = base.GetComponent<CharacterController>();
			this.rvoController = base.GetComponent<RVOController>();
			this.tr = base.transform;
		}

		
		protected virtual void Start()
		{
			this.startHasRun = true;
			this.OnEnable();
		}

		
		protected virtual void OnEnable()
		{
			this.lastRepath = -9999f;
			this.waitingForPathCalc = false;
			this.canSearchPath = true;
			if (this.startHasRun)
			{
				Seeker seeker = this.seeker;
				seeker.pathCallback = (OnPathDelegate)Delegate.Combine(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
				base.StartCoroutine(this.SearchPaths());
			}
		}

		
		public void OnDisable()
		{
			if (this.seeker != null && !this.seeker.IsDone())
			{
				this.seeker.GetCurrentPath().Error();
			}
			Seeker seeker = this.seeker;
			seeker.pathCallback = (OnPathDelegate)Delegate.Remove(seeker.pathCallback, new OnPathDelegate(this.OnPathComplete));
		}

		
		public virtual void UpdatePath()
		{
			this.canSearchPath = true;
			this.waitingForPathCalc = false;
			Path currentPath = this.seeker.GetCurrentPath();
			if (currentPath != null && !this.seeker.IsDone())
			{
				currentPath.Error();
				currentPath.Claim(this);
				currentPath.Release(this, false);
			}
			this.waitingForPathCalc = true;
			this.lastRepath = Time.time;
			this.seeker.StartPath(this.tr.position, this.target.position);
		}

		
		private IEnumerator SearchPaths()
		{
			for (;;)
			{
				while (!this.repeatedlySearchPaths || this.waitingForPathCalc || !this.canSearchPath || Time.time - this.lastRepath < this.repathRate)
				{
					yield return null;
				}
				this.UpdatePath();
				yield return null;
			}
			yield break;
		}

		
		private void OnPathComplete(Path p)
		{
			this.waitingForPathCalc = false;
			p.Claim(this);
			if (p.error)
			{
				p.Release(this, false);
				return;
			}
			if (this.traversingSpecialPath)
			{
				this.delayUpdatePath = true;
			}
			else
			{
				if (this.rp == null)
				{
					this.rp = new RichPath();
				}
				this.rp.Initialize(this.seeker, p, true, this.funnelSimplification);
			}
			p.Release(this, false);
		}

		
		
		public bool TraversingSpecial
		{
			get
			{
				return this.traversingSpecialPath;
			}
		}

		
		
		public Vector3 TargetPoint
		{
			get
			{
				return this.lastTargetPoint;
			}
		}

		
		
		public bool ApproachingPartEndpoint
		{
			get
			{
				return this.lastCorner;
			}
		}

		
		
		public bool ApproachingPathEndpoint
		{
			get
			{
				return this.rp != null && this.ApproachingPartEndpoint && !this.rp.PartsLeft();
			}
		}

		
		
		public float DistanceToNextWaypoint
		{
			get
			{
				return this.distanceToWaypoint;
			}
		}

		
		private void NextPart()
		{
			this.rp.NextPart();
			this.lastCorner = false;
			if (!this.rp.PartsLeft())
			{
				this.OnTargetReached();
			}
		}

		
		protected virtual void OnTargetReached()
		{
		}

		
		protected virtual Vector3 UpdateTarget(RichFunnel fn)
		{
			this.nextCorners.Clear();
			Vector3 vector = this.tr.position;
			bool flag;
			vector = fn.Update(vector, this.nextCorners, 2, out this.lastCorner, out flag);
			if (flag && !this.waitingForPathCalc)
			{
				this.UpdatePath();
			}
			return vector;
		}

		
		private static Vector2 To2D(Vector3 v)
		{
			return new Vector2(v.x, v.z);
		}

		
		protected virtual void Update()
		{
			RichAI.deltaTime = Mathf.Min(Time.smoothDeltaTime * 2f, Time.deltaTime);
			if (this.rp != null)
			{
				RichPathPart currentPart = this.rp.GetCurrentPart();
				RichFunnel richFunnel = currentPart as RichFunnel;
				if (richFunnel != null)
				{
					Vector3 vector = this.UpdateTarget(richFunnel);
					if (Time.frameCount % 5 == 0 && this.wallForce > 0f && this.wallDist > 0f)
					{
						this.wallBuffer.Clear();
						richFunnel.FindWalls(this.wallBuffer, this.wallDist);
					}
					int num = 0;
					Vector3 vector2 = this.nextCorners[num];
					Vector3 vector3 = vector2 - vector;
					vector3.y = 0f;
					bool flag = Vector3.Dot(vector3, this.currentTargetDirection) < 0f;
					if (flag && this.nextCorners.Count - num > 1)
					{
						num++;
						vector2 = this.nextCorners[num];
					}
					if (vector2 != this.lastTargetPoint)
					{
						this.currentTargetDirection = vector2 - vector;
						this.currentTargetDirection.y = 0f;
						this.currentTargetDirection.Normalize();
						this.lastTargetPoint = vector2;
					}
					vector3 = vector2 - vector;
					vector3.y = 0f;
					Vector3 vector4 = VectorMath.Normalize(vector3, out this.distanceToWaypoint);
					bool flag2 = this.lastCorner && this.nextCorners.Count - num == 1;
					if (flag2 && this.distanceToWaypoint < 0.01f * this.maxSpeed)
					{
						this.velocity = (vector2 - vector) * 100f;
					}
					else
					{
						Vector3 a = this.CalculateWallForce(vector, vector4);
						Vector2 vector5;
						if (flag2)
						{
							vector5 = this.CalculateAccelerationToReachPoint(RichAI.To2D(vector2 - vector), Vector2.zero, RichAI.To2D(this.velocity));
							a *= Math.Min(this.distanceToWaypoint / 0.5f, 1f);
							if (this.distanceToWaypoint < this.endReachedDistance)
							{
								this.NextPart();
							}
						}
						else
						{
							Vector3 a2 = (num >= this.nextCorners.Count - 1) ? ((vector2 - vector) * 2f + vector) : this.nextCorners[num + 1];
							Vector3 v = (a2 - vector2).normalized * this.maxSpeed;
							vector5 = this.CalculateAccelerationToReachPoint(RichAI.To2D(vector2 - vector), RichAI.To2D(v), RichAI.To2D(this.velocity));
						}
						this.velocity += (new Vector3(vector5.x, 0f, vector5.y) + a * this.wallForce) * RichAI.deltaTime;
					}
					TriangleMeshNode currentNode = richFunnel.CurrentNode;
					Vector3 b;
					if (currentNode != null)
					{
						b = currentNode.ClosestPointOnNode(vector);
					}
					else
					{
						b = vector;
					}
					float magnitude = (richFunnel.exactEnd - b).magnitude;
					float num2 = this.maxSpeed;
					num2 *= Mathf.Sqrt(Mathf.Min(1f, magnitude / (this.maxSpeed * this.slowdownTime)));
					if (this.slowWhenNotFacingTarget)
					{
						float num3 = Mathf.Max((Vector3.Dot(vector4, this.tr.forward) + 0.5f) / 1.5f, 0.2f);
						num2 *= num3;
						float num4 = VectorMath.MagnitudeXZ(this.velocity);
						float y = this.velocity.y;
						this.velocity.y = 0f;
						num4 = Mathf.Min(num4, num2);
						this.velocity = Vector3.Lerp(this.velocity.normalized * num4, this.tr.forward * num4, Mathf.Clamp((!flag2) ? 1f : (this.distanceToWaypoint * 2f), 0f, 0.5f));
						this.velocity.y = y;
					}
					else
					{
						this.velocity = VectorMath.ClampMagnitudeXZ(this.velocity, num2);
					}
					this.velocity += RichAI.deltaTime * this.gravity;
					if (this.rvoController != null && this.rvoController.enabled)
					{
						Vector3 pos = vector + VectorMath.ClampMagnitudeXZ(this.velocity, magnitude);
						this.rvoController.SetTarget(pos, VectorMath.MagnitudeXZ(this.velocity), this.maxSpeed);
					}
					Vector3 vector6;
					if (this.rvoController != null && this.rvoController.enabled)
					{
						vector6 = this.rvoController.CalculateMovementDelta(vector, RichAI.deltaTime);
						vector6.y = this.velocity.y * RichAI.deltaTime;
					}
					else
					{
						vector6 = this.velocity * RichAI.deltaTime;
					}
					if (flag2)
					{
						Vector3 trotdir = Vector3.Lerp(vector6.normalized, this.currentTargetDirection, Math.Max(1f - this.distanceToWaypoint * 2f, 0f));
						this.RotateTowards(trotdir);
					}
					else
					{
						this.RotateTowards(vector6);
					}
					if (this.controller != null && this.controller.enabled)
					{
						this.tr.position = vector;
						this.controller.Move(vector6);
						vector = this.tr.position;
					}
					else
					{
						float y2 = vector.y;
						vector += vector6;
						vector = this.RaycastPosition(vector, y2);
					}
					Vector3 vector7 = richFunnel.ClampToNavmesh(vector);
					if (vector != vector7)
					{
						Vector3 vector8 = vector7 - vector;
						this.velocity -= vector8 * Vector3.Dot(vector8, this.velocity) / vector8.sqrMagnitude;
						if (this.rvoController != null && this.rvoController.enabled)
						{
							this.rvoController.SetCollisionNormal(vector8);
						}
					}
					this.tr.position = vector7;
				}
				else if (this.rvoController != null && this.rvoController.enabled)
				{
					this.rvoController.Move(Vector3.zero);
				}
				if (currentPart is RichSpecial && !this.traversingSpecialPath)
				{
					base.StartCoroutine(this.TraverseSpecial(currentPart as RichSpecial));
				}
			}
			else if (this.rvoController != null && this.rvoController.enabled)
			{
				this.rvoController.Move(Vector3.zero);
			}
			else if (!(this.controller != null) || !this.controller.enabled)
			{
				this.tr.position = this.RaycastPosition(this.tr.position, this.tr.position.y);
			}
		}

		
		private Vector2 CalculateAccelerationToReachPoint(Vector2 deltaPosition, Vector2 targetVelocity, Vector2 currentVelocity)
		{
			if (targetVelocity == Vector2.zero)
			{
				float num = 0.05f;
				float num2 = 10f;
				while (num2 - num > 0.01f)
				{
					float num3 = (num2 + num) * 0.5f;
					Vector2 a = (6f * deltaPosition - 4f * num3 * currentVelocity) / (num3 * num3);
					Vector2 a2 = 6f * (num3 * currentVelocity - 2f * deltaPosition) / (num3 * num3 * num3);
					if (a.sqrMagnitude > this.acceleration * this.acceleration || (a + a2 * num3).sqrMagnitude > this.acceleration * this.acceleration)
					{
						num = num3;
					}
					else
					{
						num2 = num3;
					}
				}
				return (6f * deltaPosition - 4f * num2 * currentVelocity) / (num2 * num2);
			}
			float magnitude = deltaPosition.magnitude;
			float magnitude2 = currentVelocity.magnitude;
			float num4;
			Vector2 a3 = VectorMath.Normalize(targetVelocity, out num4);
			return (deltaPosition - a3 * Math.Min(0.5f * magnitude * num4 / (magnitude2 + num4), this.maxSpeed * 2f)).normalized * this.acceleration;
		}

		
		private Vector3 CalculateWallForce(Vector3 position, Vector3 directionToTarget)
		{
			if (this.wallForce > 0f && this.wallDist > 0f)
			{
				float num = 0f;
				float num2 = 0f;
				for (int i = 0; i < this.wallBuffer.Count; i += 2)
				{
					Vector3 a = VectorMath.ClosestPointOnSegment(this.wallBuffer[i], this.wallBuffer[i + 1], this.tr.position);
					float sqrMagnitude = (a - position).sqrMagnitude;
					if (sqrMagnitude <= this.wallDist * this.wallDist)
					{
						Vector3 normalized = (this.wallBuffer[i + 1] - this.wallBuffer[i]).normalized;
						float num3 = Vector3.Dot(directionToTarget, normalized) * (1f - Math.Max(0f, 2f * (sqrMagnitude / (this.wallDist * this.wallDist)) - 1f));
						if (num3 > 0f)
						{
							num2 = Math.Max(num2, num3);
						}
						else
						{
							num = Math.Max(num, -num3);
						}
					}
				}
				Vector3 a2 = Vector3.Cross(Vector3.up, directionToTarget);
				return a2 * (num2 - num);
			}
			return Vector3.zero;
		}

		
		private Vector3 RaycastPosition(Vector3 position, float lasty)
		{
			if (this.raycastingForGroundPlacement)
			{
				float num = Mathf.Max(this.centerOffset, lasty - position.y + this.centerOffset);
				RaycastHit raycastHit;
				if (Physics.Raycast(position + Vector3.up * num, Vector3.down, out raycastHit, num, this.groundMask) && raycastHit.distance < num)
				{
					position = raycastHit.point;
					this.velocity.y = 0f;
				}
			}
			return position;
		}

		
		private bool RotateTowards(Vector3 trotdir)
		{
			trotdir.y = 0f;
			if (trotdir != Vector3.zero)
			{
				Quaternion rotation = this.tr.rotation;
				Vector3 eulerAngles = Quaternion.LookRotation(trotdir).eulerAngles;
				Vector3 eulerAngles2 = rotation.eulerAngles;
				eulerAngles2.y = Mathf.MoveTowardsAngle(eulerAngles2.y, eulerAngles.y, this.rotationSpeed * RichAI.deltaTime);
				this.tr.rotation = Quaternion.Euler(eulerAngles2);
				return Mathf.Abs(eulerAngles2.y - eulerAngles.y) < 5f;
			}
			return false;
		}

		
		public void OnDrawGizmos()
		{
			if (this.drawGizmos)
			{
				if (this.raycastingForGroundPlacement)
				{
					Gizmos.color = RichAI.GizmoColorRaycast;
					Gizmos.DrawLine(base.transform.position, base.transform.position + Vector3.up * this.centerOffset);
					Gizmos.DrawLine(base.transform.position + Vector3.left * 0.1f, base.transform.position + Vector3.right * 0.1f);
					Gizmos.DrawLine(base.transform.position + Vector3.back * 0.1f, base.transform.position + Vector3.forward * 0.1f);
				}
				if (this.tr != null && this.nextCorners != null)
				{
					Gizmos.color = RichAI.GizmoColorPath;
					Vector3 from = this.tr.position;
					for (int i = 0; i < this.nextCorners.Count; i++)
					{
						Gizmos.DrawLine(from, this.nextCorners[i]);
						from = this.nextCorners[i];
					}
				}
			}
		}

		
		private IEnumerator TraverseSpecial(RichSpecial rs)
		{
			this.traversingSpecialPath = true;
			this.velocity = Vector3.zero;
			AnimationLink al = rs.nodeLink as AnimationLink;
			if (al == null)
			{
				Debug.LogError("Unhandled RichSpecial");
				yield break;
			}
			while (!this.RotateTowards(rs.first.forward))
			{
				yield return null;
			}
			this.tr.parent.position = this.tr.position;
			this.tr.parent.rotation = this.tr.rotation;
			this.tr.localPosition = Vector3.zero;
			this.tr.localRotation = Quaternion.identity;
			if (rs.reverse && al.reverseAnim)
			{
				this.anim[al.clip].speed = -al.animSpeed;
				this.anim[al.clip].normalizedTime = 1f;
				this.anim.Play(al.clip);
				this.anim.Sample();
			}
			else
			{
				this.anim[al.clip].speed = al.animSpeed;
				this.anim.Rewind(al.clip);
				this.anim.Play(al.clip);
			}
			this.tr.parent.position -= this.tr.position - this.tr.parent.position;
			yield return new WaitForSeconds(Mathf.Abs(this.anim[al.clip].length / al.animSpeed));
			this.traversingSpecialPath = false;
			this.NextPart();
			if (this.delayUpdatePath)
			{
				this.delayUpdatePath = false;
				this.UpdatePath();
			}
			yield break;
		}

		
		public Transform target;

		
		public bool drawGizmos = true;

		
		public bool repeatedlySearchPaths;

		
		public float repathRate = 0.5f;

		
		public float maxSpeed = 1f;

		
		public float acceleration = 5f;

		
		public float slowdownTime = 0.5f;

		
		public float rotationSpeed = 360f;

		
		public float endReachedDistance = 0.01f;

		
		public float wallForce = 3f;

		
		public float wallDist = 1f;

		
		public Vector3 gravity = new Vector3(0f, -9.82f, 0f);

		
		public bool raycastingForGroundPlacement = true;

		
		public LayerMask groundMask = -1;

		
		public float centerOffset = 1f;

		
		public RichFunnel.FunnelSimplification funnelSimplification;

		
		public Animation anim;

		
		public bool preciseSlowdown = true;

		
		public bool slowWhenNotFacingTarget = true;

		
		private Vector3 velocity;

		
		protected RichPath rp;

		
		protected Seeker seeker;

		
		protected Transform tr;

		
		private CharacterController controller;

		
		private RVOController rvoController;

		
		private Vector3 lastTargetPoint;

		
		private Vector3 currentTargetDirection;

		
		protected bool waitingForPathCalc;

		
		protected bool canSearchPath;

		
		protected bool delayUpdatePath;

		
		protected bool traversingSpecialPath;

		
		protected bool lastCorner;

		
		private float distanceToWaypoint = 999f;

		
		protected List<Vector3> nextCorners = new List<Vector3>();

		
		protected List<Vector3> wallBuffer = new List<Vector3>();

		
		private bool startHasRun;

		
		protected float lastRepath = -9999f;

		
		private static float deltaTime;

		
		public static readonly Color GizmoColorRaycast = new Color(0.4627451f, 0.807843149f, 0.4392157f);

		
		public static readonly Color GizmoColorPath = new Color(0.03137255f, 0.305882365f, 0.7607843f);
	}
}
