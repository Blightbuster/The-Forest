using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Valve.VR.InteractionSystem
{
	
	public class TeleportArc : MonoBehaviour
	{
		
		private void Start()
		{
			this.arcTimeOffset = Time.time;
		}

		
		private void Update()
		{
			if (this.thickness != this.prevThickness || this.segmentCount != this.prevSegmentCount)
			{
				this.CreateLineRendererObjects();
				this.prevThickness = this.thickness;
				this.prevSegmentCount = this.segmentCount;
			}
		}

		
		private void CreateLineRendererObjects()
		{
			if (this.arcObjectsTransfrom != null)
			{
				UnityEngine.Object.Destroy(this.arcObjectsTransfrom.gameObject);
			}
			GameObject gameObject = new GameObject("ArcObjects");
			this.arcObjectsTransfrom = gameObject.transform;
			this.arcObjectsTransfrom.SetParent(base.transform);
			this.lineRenderers = new LineRenderer[this.segmentCount];
			for (int i = 0; i < this.segmentCount; i++)
			{
				GameObject gameObject2 = new GameObject("LineRenderer_" + i);
				gameObject2.transform.SetParent(this.arcObjectsTransfrom);
				this.lineRenderers[i] = gameObject2.AddComponent<LineRenderer>();
				this.lineRenderers[i].receiveShadows = false;
				this.lineRenderers[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
				this.lineRenderers[i].lightProbeUsage = LightProbeUsage.Off;
				this.lineRenderers[i].shadowCastingMode = ShadowCastingMode.Off;
				this.lineRenderers[i].material = this.material;
				this.lineRenderers[i].startWidth = this.thickness;
				this.lineRenderers[i].endWidth = this.thickness;
				this.lineRenderers[i].enabled = false;
			}
		}

		
		public void SetArcData(Vector3 position, Vector3 velocity, bool gravity, bool pointerAtBadAngle)
		{
			this.startPos = position;
			this.projectileVelocity = velocity;
			this.useGravity = gravity;
			if (this.arcInvalid && !pointerAtBadAngle)
			{
				this.arcTimeOffset = Time.time;
			}
			this.arcInvalid = pointerAtBadAngle;
		}

		
		public void Show()
		{
			this.showArc = true;
			if (this.lineRenderers == null)
			{
				this.CreateLineRendererObjects();
			}
		}

		
		public void Hide()
		{
			if (this.showArc)
			{
				this.HideLineSegments(0, this.segmentCount);
			}
			this.showArc = false;
		}

		
		public bool DrawArc(out RaycastHit hitInfo)
		{
			float num = this.arcDuration / (float)this.segmentCount;
			float num2 = (Time.time - this.arcTimeOffset) * this.arcSpeed;
			if (num2 > num + this.segmentBreak)
			{
				this.arcTimeOffset = Time.time;
				num2 = 0f;
			}
			float num3 = num2;
			float num4 = this.FindProjectileCollision(out hitInfo);
			if (this.arcInvalid)
			{
				this.lineRenderers[0].enabled = true;
				this.lineRenderers[0].SetPosition(0, this.GetArcPositionAtTime(0f));
				this.lineRenderers[0].SetPosition(1, this.GetArcPositionAtTime((num4 >= num) ? num : num4));
				this.HideLineSegments(1, this.segmentCount);
			}
			else
			{
				int num5 = 0;
				if (num3 > this.segmentBreak)
				{
					float num6 = num2 - this.segmentBreak;
					if (num4 < num6)
					{
						num6 = num4;
					}
					this.DrawArcSegment(0, 0f, num6);
					num5 = 1;
				}
				bool flag = false;
				int i = 0;
				if (num3 < num4)
				{
					for (i = num5; i < this.segmentCount; i++)
					{
						float num7 = num3 + num;
						if (num7 >= this.arcDuration)
						{
							num7 = this.arcDuration;
							flag = true;
						}
						if (num7 >= num4)
						{
							num7 = num4;
							flag = true;
						}
						this.DrawArcSegment(i, num3, num7);
						num3 += num + this.segmentBreak;
						if (flag || num3 >= this.arcDuration || num3 >= num4)
						{
							break;
						}
					}
				}
				else
				{
					i--;
				}
				this.HideLineSegments(i + 1, this.segmentCount);
			}
			return num4 != float.MaxValue;
		}

		
		private void DrawArcSegment(int index, float startTime, float endTime)
		{
			this.lineRenderers[index].enabled = true;
			this.lineRenderers[index].SetPosition(0, this.GetArcPositionAtTime(startTime));
			this.lineRenderers[index].SetPosition(1, this.GetArcPositionAtTime(endTime));
		}

		
		public void SetColor(Color color)
		{
			for (int i = 0; i < this.segmentCount; i++)
			{
				this.lineRenderers[i].startColor = color;
				this.lineRenderers[i].endColor = color;
			}
		}

		
		private float FindProjectileCollision(out RaycastHit hitInfo)
		{
			float num = this.arcDuration / (float)this.segmentCount;
			float num2 = 0f;
			hitInfo = default(RaycastHit);
			Vector3 vector = this.GetArcPositionAtTime(num2);
			for (int i = 0; i < this.segmentCount; i++)
			{
				float num3 = num2 + num;
				Vector3 arcPositionAtTime = this.GetArcPositionAtTime(num3);
				if (Physics.Linecast(vector, arcPositionAtTime, out hitInfo, this.traceLayerMask) && hitInfo.collider.GetComponent<IgnoreTeleportTrace>() == null)
				{
					Util.DrawCross(hitInfo.point, Color.red, 0.5f);
					float num4 = Vector3.Distance(vector, arcPositionAtTime);
					return num2 + num * (hitInfo.distance / num4);
				}
				num2 = num3;
				vector = arcPositionAtTime;
			}
			return float.MaxValue;
		}

		
		public Vector3 GetArcPositionAtTime(float time)
		{
			Vector3 a = (!this.useGravity) ? Vector3.zero : Physics.gravity;
			return this.startPos + (this.projectileVelocity * time + 0.5f * time * time * a);
		}

		
		private void HideLineSegments(int startSegment, int endSegment)
		{
			if (this.lineRenderers != null)
			{
				for (int i = startSegment; i < endSegment; i++)
				{
					this.lineRenderers[i].enabled = false;
				}
			}
		}

		
		public int segmentCount = 60;

		
		public float thickness = 0.01f;

		
		[Tooltip("The amount of time in seconds to predict the motion of the projectile.")]
		public float arcDuration = 3f;

		
		[Tooltip("The amount of time in seconds between each segment of the projectile.")]
		public float segmentBreak = 0.025f;

		
		[Tooltip("The speed at which the line segments of the arc move.")]
		public float arcSpeed = 0.2f;

		
		public Material material;

		
		[HideInInspector]
		public int traceLayerMask;

		
		private LineRenderer[] lineRenderers;

		
		private float arcTimeOffset;

		
		private float prevThickness;

		
		private int prevSegmentCount;

		
		private bool showArc = true;

		
		private Vector3 startPos;

		
		private Vector3 projectileVelocity;

		
		private bool useGravity = true;

		
		private Transform arcObjectsTransfrom;

		
		private bool arcInvalid;
	}
}
