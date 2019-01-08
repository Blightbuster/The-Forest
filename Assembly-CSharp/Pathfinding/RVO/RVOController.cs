using System;
using UnityEngine;

namespace Pathfinding.RVO
{
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_controller.php")]
	public class RVOController : MonoBehaviour
	{
		public IAgent rvoAgent { get; private set; }

		public Simulator simulator { get; private set; }

		public Vector3 position
		{
			get
			{
				return this.To3D(this.rvoAgent.Position, this.rvoAgent.ElevationCoordinate);
			}
		}

		public Vector3 velocity
		{
			get
			{
				if (Time.deltaTime > 1E-05f)
				{
					return this.CalculateMovementDelta(Time.deltaTime) / Time.deltaTime;
				}
				return Vector3.zero;
			}
		}

		public Vector3 CalculateMovementDelta(float deltaTime)
		{
			return this.To3D(Vector2.ClampMagnitude(this.rvoAgent.CalculatedTargetPoint - this.To2D(this.tr.position), this.rvoAgent.CalculatedSpeed * deltaTime), 0f);
		}

		public Vector3 CalculateMovementDelta(Vector3 position, float deltaTime)
		{
			return this.To3D(Vector2.ClampMagnitude(this.rvoAgent.CalculatedTargetPoint - this.To2D(position), this.rvoAgent.CalculatedSpeed * deltaTime), 0f);
		}

		public void SetCollisionNormal(Vector3 normal)
		{
			this.rvoAgent.SetCollisionNormal(this.To2D(normal));
		}

		public void ForceSetVelocity(Vector3 velocity)
		{
			this.rvoAgent.ForceSetVelocity(this.To2D(velocity));
		}

		private Vector2 To2D(Vector3 p)
		{
			float num;
			return this.To2D(p, out num);
		}

		private Vector2 To2D(Vector3 p, out float elevation)
		{
			if (this.movementMode == MovementMode.XY)
			{
				elevation = p.z;
				return new Vector2(p.x, p.y);
			}
			elevation = p.y;
			return new Vector2(p.x, p.z);
		}

		private Vector3 To3D(Vector2 p, float elevationCoordinate)
		{
			if (this.movementMode == MovementMode.XY)
			{
				return new Vector3(p.x, p.y, elevationCoordinate);
			}
			return new Vector3(p.x, elevationCoordinate, p.y);
		}

		public void OnDisable()
		{
			if (this.simulator == null)
			{
				return;
			}
			this.simulator.RemoveAgent(this.rvoAgent);
		}

		public void Awake()
		{
			this.tr = base.transform;
			if (RVOController.cachedSimulator == null)
			{
				RVOController.cachedSimulator = UnityEngine.Object.FindObjectOfType<RVOSimulator>();
			}
			if (RVOController.cachedSimulator == null)
			{
				Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
			}
			else
			{
				this.simulator = RVOController.cachedSimulator.GetSimulator();
			}
		}

		public void OnEnable()
		{
			if (this.simulator == null)
			{
				return;
			}
			if (this.rvoAgent != null)
			{
				this.simulator.AddAgent(this.rvoAgent);
			}
			else
			{
				float elevationCoordinate;
				Vector2 position = this.To2D(base.transform.position, out elevationCoordinate);
				this.rvoAgent = this.simulator.AddAgent(position, elevationCoordinate);
				this.rvoAgent.PreCalculationCallback = new Action(this.UpdateAgentProperties);
			}
			this.UpdateAgentProperties();
		}

		protected void UpdateAgentProperties()
		{
			this.rvoAgent.Radius = Mathf.Max(0.001f, this.radius);
			this.rvoAgent.AgentTimeHorizon = this.agentTimeHorizon;
			this.rvoAgent.ObstacleTimeHorizon = this.obstacleTimeHorizon;
			this.rvoAgent.Locked = this.locked;
			this.rvoAgent.MaxNeighbours = this.maxNeighbours;
			this.rvoAgent.DebugDraw = this.debug;
			this.rvoAgent.NeighbourDist = this.neighbourDist;
			this.rvoAgent.Layer = this.layer;
			this.rvoAgent.CollidesWith = this.collidesWith;
			this.rvoAgent.MovementMode = this.movementMode;
			this.rvoAgent.Priority = this.priority;
			float num;
			this.rvoAgent.Position = this.To2D(base.transform.position, out num);
			if (this.movementMode == MovementMode.XZ)
			{
				this.rvoAgent.Height = this.height;
				this.rvoAgent.ElevationCoordinate = num + this.center - 0.5f * this.height;
			}
			else
			{
				this.rvoAgent.Height = 1f;
				this.rvoAgent.ElevationCoordinate = 0f;
			}
		}

		public void SetTarget(Vector3 pos, float speed, float maxSpeed)
		{
			this.rvoAgent.SetTarget(this.To2D(pos), speed, maxSpeed);
			if (this.lockWhenNotMoving)
			{
				this.locked = (speed < 0.001f);
			}
		}

		public void Move(Vector3 vel)
		{
			Vector2 b = this.To2D(vel);
			float magnitude = b.magnitude;
			this.rvoAgent.SetTarget(this.To2D(this.tr.position) + b, magnitude, magnitude);
			if (this.lockWhenNotMoving)
			{
				this.locked = (magnitude < 0.001f);
			}
		}

		public void Teleport(Vector3 pos)
		{
			this.tr.position = pos;
		}

		public void Update()
		{
		}

		private static void DrawCircle(Vector3 p, float radius, float a0, float a1)
		{
			while (a0 > a1)
			{
				a0 -= 6.28318548f;
			}
			Vector3 b = new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
			int num = 0;
			while ((float)num <= 40f)
			{
				Vector3 vector = new Vector3(Mathf.Cos(Mathf.Lerp(a0, a1, (float)num / 40f)) * radius, 0f, Mathf.Sin(Mathf.Lerp(a0, a1, (float)num / 40f)) * radius);
				Gizmos.DrawLine(p + b, p + vector);
				b = vector;
				num++;
			}
		}

		private static void DrawCylinder(Vector3 p, Vector3 up, float height, float radius)
		{
			Vector3 normalized = Vector3.Cross(up, Vector3.one).normalized;
			Gizmos.matrix = Matrix4x4.TRS(p, Quaternion.LookRotation(normalized, up), new Vector3(radius, height, radius));
			RVOController.DrawCircle(new Vector2(0f, 0f), 1f, 0f, 6.28318548f);
			if (height > 0f)
			{
				RVOController.DrawCircle(new Vector2(0f, 1f), 1f, 0f, 6.28318548f);
				Gizmos.DrawLine(new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f));
				Gizmos.DrawLine(new Vector3(-1f, 0f, 0f), new Vector3(-1f, 1f, 0f));
				Gizmos.DrawLine(new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f));
				Gizmos.DrawLine(new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, -1f));
			}
		}

		private void OnDrawGizmos()
		{
			if (this.locked)
			{
				Gizmos.color = RVOController.GizmoColor * 0.5f;
			}
			else
			{
				Gizmos.color = RVOController.GizmoColor;
			}
			if (this.movementMode == MovementMode.XY)
			{
				RVOController.DrawCylinder(base.transform.position, Vector3.forward, 0f, this.radius);
			}
			else
			{
				RVOController.DrawCylinder(base.transform.position + this.To3D(Vector2.zero, this.center - this.height * 0.5f), this.To3D(Vector2.zero, 1f), this.height, this.radius);
			}
		}

		private void OnDrawGizmosSelected()
		{
		}

		public MovementMode movementMode;

		[Tooltip("Radius of the agent")]
		public float radius = 5f;

		[Tooltip("Height of the agent. In world units")]
		public float height = 1f;

		[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quailty is not the best")]
		public bool locked;

		[Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
		public bool lockWhenNotMoving = true;

		[Tooltip("How far in the time to look for collisions with other agents")]
		public float agentTimeHorizon = 2f;

		public float obstacleTimeHorizon = 2f;

		[Tooltip("Maximum distance to other agents to take them into account for collisions.\nDecreasing this value can lead to better performance, increasing it can lead to better quality of the simulation")]
		public float neighbourDist = 10f;

		[Tooltip("Max number of other agents to take into account.\nA smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
		public int maxNeighbours = 10;

		public RVOLayer layer = RVOLayer.DefaultAgent;

		[AstarEnumFlag]
		public RVOLayer collidesWith = (RVOLayer)(-1);

		[HideInInspector]
		public float wallAvoidForce = 1f;

		[HideInInspector]
		public float wallAvoidFalloff = 1f;

		[Tooltip("How strongly other agents will avoid this agent")]
		[Range(0f, 1f)]
		public float priority = 0.5f;

		[Tooltip("Center of the agent relative to the pivot point of this game object")]
		public float center;

		private Transform tr;

		public bool debug;

		private static RVOSimulator cachedSimulator;

		private static readonly Color GizmoColor = new Color(0.9411765f, 0.8352941f, 0.117647059f);
	}
}
