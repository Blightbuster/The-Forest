using System;
using System.Collections.Generic;
using Pathfinding.RVO;
using UnityEngine;

namespace Pathfinding.Examples
{
	
	[HelpURL("http:
	public class RVOExampleAgent : MonoBehaviour
	{
		
		public void Awake()
		{
			this.seeker = base.GetComponent<Seeker>();
		}

		
		public void Start()
		{
			this.SetTarget(-base.transform.position);
			this.controller = base.GetComponent<RVOController>();
		}

		
		public void SetTarget(Vector3 target)
		{
			this.target = target;
			this.RecalculatePath();
		}

		
		public void SetColor(Color col)
		{
			if (this.rends == null)
			{
				this.rends = base.GetComponentsInChildren<MeshRenderer>();
			}
			foreach (MeshRenderer meshRenderer in this.rends)
			{
				Color color = meshRenderer.material.GetColor("_TintColor");
				AnimationCurve curve = AnimationCurve.Linear(0f, color.r, 1f, col.r);
				AnimationCurve curve2 = AnimationCurve.Linear(0f, color.g, 1f, col.g);
				AnimationCurve curve3 = AnimationCurve.Linear(0f, color.b, 1f, col.b);
				AnimationClip animationClip = new AnimationClip();
				animationClip.legacy = true;
				animationClip.SetCurve(string.Empty, typeof(Material), "_TintColor.r", curve);
				animationClip.SetCurve(string.Empty, typeof(Material), "_TintColor.g", curve2);
				animationClip.SetCurve(string.Empty, typeof(Material), "_TintColor.b", curve3);
				Animation animation = meshRenderer.gameObject.GetComponent<Animation>();
				if (animation == null)
				{
					animation = meshRenderer.gameObject.AddComponent<Animation>();
				}
				animationClip.wrapMode = WrapMode.Once;
				animation.AddClip(animationClip, "ColorAnim");
				animation.Play("ColorAnim");
			}
		}

		
		public void RecalculatePath()
		{
			this.canSearchAgain = false;
			this.nextRepath = Time.time + this.repathRate * (UnityEngine.Random.value + 0.5f);
			this.seeker.StartPath(base.transform.position, this.target, new OnPathDelegate(this.OnPathComplete));
		}

		
		public void OnPathComplete(Path _p)
		{
			ABPath abpath = _p as ABPath;
			this.canSearchAgain = true;
			if (this.path != null)
			{
				this.path.Release(this, false);
			}
			this.path = abpath;
			abpath.Claim(this);
			if (abpath.error)
			{
				this.wp = 0;
				this.vectorPath = null;
				return;
			}
			Vector3 originalStartPoint = abpath.originalStartPoint;
			Vector3 position = base.transform.position;
			originalStartPoint.y = position.y;
			float magnitude = (position - originalStartPoint).magnitude;
			this.wp = 0;
			this.vectorPath = abpath.vectorPath;
			for (float num = 0f; num <= magnitude; num += this.moveNextDist * 0.6f)
			{
				this.wp--;
				Vector3 a = originalStartPoint + (position - originalStartPoint) * num;
				Vector3 b;
				do
				{
					this.wp++;
					b = this.vectorPath[this.wp];
					b.y = a.y;
				}
				while ((a - b).sqrMagnitude < this.moveNextDist * this.moveNextDist && this.wp != this.vectorPath.Count - 1);
			}
		}

		
		public void Update()
		{
			if (Time.time >= this.nextRepath && this.canSearchAgain)
			{
				this.RecalculatePath();
			}
			Vector3 vector = base.transform.position;
			if (this.vectorPath != null && this.vectorPath.Count != 0)
			{
				Vector3 vector2 = this.vectorPath[this.wp];
				vector2.y = vector.y;
				while ((vector - vector2).sqrMagnitude < this.moveNextDist * this.moveNextDist && this.wp != this.vectorPath.Count - 1)
				{
					this.wp++;
					vector2 = this.vectorPath[this.wp];
					vector2.y = vector.y;
				}
				if (this.wp == this.vectorPath.Count - 1)
				{
					float speed = Mathf.Clamp01((vector2 - vector).magnitude / this.slowdownDistance) * this.maxSpeed;
					this.controller.SetTarget(vector2, speed, this.maxSpeed);
				}
				else
				{
					Vector3 vector3 = vector2 - vector;
					this.controller.Move(vector3.normalized * this.maxSpeed);
				}
			}
			else
			{
				this.controller.SetTarget(vector, this.maxSpeed, this.maxSpeed);
			}
			Vector3 vector4 = this.controller.CalculateMovementDelta(Time.deltaTime);
			vector += vector4;
			if (Time.deltaTime > 0f && vector4.magnitude / Time.deltaTime > 0.01f)
			{
				Quaternion rotation = base.transform.rotation;
				Quaternion b = Quaternion.LookRotation(vector4);
				base.transform.rotation = Quaternion.Slerp(rotation, b, Time.deltaTime * 5f);
			}
			RaycastHit raycastHit;
			if (Physics.Raycast(vector + Vector3.up, Vector3.down, out raycastHit, 2f, this.groundMask))
			{
				vector.y = raycastHit.point.y;
			}
			base.transform.position = vector;
		}

		
		public float repathRate = 1f;

		
		private float nextRepath;

		
		private Vector3 target;

		
		private bool canSearchAgain = true;

		
		private RVOController controller;

		
		public float maxSpeed = 10f;

		
		private Path path;

		
		private List<Vector3> vectorPath;

		
		private int wp;

		
		public float moveNextDist = 1f;

		
		public float slowdownDistance = 1f;

		
		public LayerMask groundMask;

		
		private Seeker seeker;

		
		private MeshRenderer[] rends;
	}
}
