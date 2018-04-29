using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	
	[JsonOptIn]
	public class PointGraph : NavGraph, IUpdatableGraph
	{
		
		
		
		public int nodeCount { get; private set; }

		
		public override int CountNodes()
		{
			return this.nodeCount;
		}

		
		public override void GetNodes(GraphNodeDelegateCancelable del)
		{
			if (this.nodes == null)
			{
				return;
			}
			int nodeCount = this.nodeCount;
			int num = 0;
			while (num < nodeCount && del(this.nodes[num]))
			{
				num++;
			}
		}

		
		public override NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
		{
			return this.GetNearestForce(position, null);
		}

		
		public override NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			if (this.nodes == null)
			{
				return default(NNInfoInternal);
			}
			if (this.optimizeForSparseGraph)
			{
				return new NNInfoInternal(this.lookupTree.GetNearest((Int3)position, constraint));
			}
			float num = (constraint != null && !constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistanceSqr;
			NNInfoInternal result = new NNInfoInternal(null);
			float num2 = float.PositiveInfinity;
			float num3 = float.PositiveInfinity;
			for (int i = 0; i < this.nodeCount; i++)
			{
				PointNode pointNode = this.nodes[i];
				float sqrMagnitude = (position - (Vector3)pointNode.position).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					result.node = pointNode;
				}
				if (sqrMagnitude < num3 && sqrMagnitude < num && (constraint == null || constraint.Suitable(pointNode)))
				{
					num3 = sqrMagnitude;
					result.constrainedNode = pointNode;
				}
			}
			result.UpdateInfo();
			return result;
		}

		
		public PointNode AddNode(Int3 position)
		{
			return this.AddNode<PointNode>(new PointNode(this.active), position);
		}

		
		public T AddNode<T>(T node, Int3 position) where T : PointNode
		{
			if (this.nodes == null || this.nodeCount == this.nodes.Length)
			{
				PointNode[] array = new PointNode[(this.nodes == null) ? 4 : Math.Max(this.nodes.Length + 4, this.nodes.Length * 2)];
				for (int i = 0; i < this.nodeCount; i++)
				{
					array[i] = this.nodes[i];
				}
				this.nodes = array;
			}
			node.SetPosition(position);
			node.GraphIndex = this.graphIndex;
			node.Walkable = true;
			this.nodes[this.nodeCount] = node;
			this.nodeCount++;
			this.AddToLookup(node);
			return node;
		}

		
		protected static int CountChildren(Transform tr)
		{
			int num = 0;
			foreach (object obj in tr)
			{
				Transform tr2 = (Transform)obj;
				num++;
				num += PointGraph.CountChildren(tr2);
			}
			return num;
		}

		
		protected void AddChildren(ref int c, Transform tr)
		{
			foreach (object obj in tr)
			{
				Transform transform = (Transform)obj;
				this.nodes[c].SetPosition((Int3)transform.position);
				this.nodes[c].Walkable = true;
				this.nodes[c].gameObject = transform.gameObject;
				c++;
				this.AddChildren(ref c, transform);
			}
		}

		
		public void RebuildNodeLookup()
		{
			if (!this.optimizeForSparseGraph || this.nodes == null)
			{
				this.lookupTree = new PointKDTree();
			}
			else
			{
				this.lookupTree.Rebuild(this.nodes, 0, this.nodeCount);
			}
		}

		
		private void AddToLookup(PointNode node)
		{
			this.lookupTree.Add(node);
		}

		
		public override IEnumerable<Progress> ScanInternal()
		{
			yield return new Progress(0f, "Searching for GameObjects");
			if (this.root == null)
			{
				GameObject[] gos = (this.searchTag == null) ? null : GameObject.FindGameObjectsWithTag(this.searchTag);
				if (gos == null)
				{
					this.nodes = new PointNode[0];
					this.nodeCount = 0;
					yield break;
				}
				yield return new Progress(0.1f, "Creating nodes");
				this.nodes = new PointNode[gos.Length];
				this.nodeCount = this.nodes.Length;
				for (int i = 0; i < this.nodes.Length; i++)
				{
					this.nodes[i] = new PointNode(this.active);
				}
				for (int j = 0; j < gos.Length; j++)
				{
					this.nodes[j].SetPosition((Int3)gos[j].transform.position);
					this.nodes[j].Walkable = true;
					this.nodes[j].gameObject = gos[j].gameObject;
				}
			}
			else if (!this.recursive)
			{
				this.nodes = new PointNode[this.root.childCount];
				this.nodeCount = this.nodes.Length;
				for (int k = 0; k < this.nodes.Length; k++)
				{
					this.nodes[k] = new PointNode(this.active);
				}
				int c = 0;
				foreach (object obj in this.root)
				{
					Transform child = (Transform)obj;
					this.nodes[c].SetPosition((Int3)child.position);
					this.nodes[c].Walkable = true;
					this.nodes[c].gameObject = child.gameObject;
					c++;
				}
			}
			else
			{
				this.nodes = new PointNode[PointGraph.CountChildren(this.root)];
				this.nodeCount = this.nodes.Length;
				for (int l = 0; l < this.nodes.Length; l++)
				{
					this.nodes[l] = new PointNode(this.active);
				}
				int startID = 0;
				this.AddChildren(ref startID, this.root);
			}
			if (this.optimizeForSparseGraph)
			{
				yield return new Progress(0.15f, "Building node lookup");
				this.RebuildNodeLookup();
			}
			if (this.maxDistance >= 0f)
			{
				List<PointNode> connections = new List<PointNode>();
				List<uint> costs = new List<uint>();
				List<GraphNode> candidateConnections = new List<GraphNode>();
				long maxPossibleSqrRange;
				if (this.maxDistance == 0f && (this.limits.x == 0f || this.limits.y == 0f || this.limits.z == 0f))
				{
					maxPossibleSqrRange = long.MaxValue;
				}
				else
				{
					maxPossibleSqrRange = (long)(Mathf.Max(this.limits.x, Mathf.Max(this.limits.y, Mathf.Max(this.limits.z, this.maxDistance))) * 1000f) + 1L;
					maxPossibleSqrRange *= maxPossibleSqrRange;
				}
				for (int m = 0; m < this.nodes.Length; m++)
				{
					if (m % 512 == 0)
					{
						yield return new Progress(Mathf.Lerp(0.15f, 1f, (float)m / (float)this.nodes.Length), "Connecting nodes");
					}
					connections.Clear();
					costs.Clear();
					PointNode node = this.nodes[m];
					if (this.optimizeForSparseGraph)
					{
						candidateConnections.Clear();
						this.lookupTree.GetInRange(node.position, maxPossibleSqrRange, candidateConnections);
						Console.WriteLine(m + " " + candidateConnections.Count);
						for (int n = 0; n < candidateConnections.Count; n++)
						{
							PointNode other = candidateConnections[n] as PointNode;
							float dist;
							if (other != node && this.IsValidConnection(node, other, out dist))
							{
								connections.Add(other);
								costs.Add((uint)Mathf.RoundToInt(dist * 1000f));
							}
						}
					}
					else
					{
						for (int j2 = 0; j2 < this.nodes.Length; j2++)
						{
							if (m != j2)
							{
								PointNode other2 = this.nodes[j2];
								float dist2;
								if (this.IsValidConnection(node, other2, out dist2))
								{
									connections.Add(other2);
									costs.Add((uint)Mathf.RoundToInt(dist2 * 1000f));
								}
							}
						}
					}
					node.connections = connections.ToArray();
					node.connectionCosts = costs.ToArray();
				}
			}
			yield break;
		}

		
		public virtual bool IsValidConnection(GraphNode a, GraphNode b, out float dist)
		{
			dist = 0f;
			if (!a.Walkable || !b.Walkable)
			{
				return false;
			}
			Vector3 vector = (Vector3)(b.position - a.position);
			if ((!Mathf.Approximately(this.limits.x, 0f) && Mathf.Abs(vector.x) > this.limits.x) || (!Mathf.Approximately(this.limits.y, 0f) && Mathf.Abs(vector.y) > this.limits.y) || (!Mathf.Approximately(this.limits.z, 0f) && Mathf.Abs(vector.z) > this.limits.z))
			{
				return false;
			}
			dist = vector.magnitude;
			if (this.maxDistance != 0f && dist >= this.maxDistance)
			{
				return false;
			}
			if (!this.raycast)
			{
				return true;
			}
			Ray ray = new Ray((Vector3)a.position, vector);
			Ray ray2 = new Ray((Vector3)b.position, -vector);
			if (this.use2DPhysics)
			{
				if (this.thickRaycast)
				{
					return !Physics2D.CircleCast(ray.origin, this.thickRaycastRadius, ray.direction, dist, this.mask) && !Physics2D.CircleCast(ray2.origin, this.thickRaycastRadius, ray2.direction, dist, this.mask);
				}
				return !Physics2D.Linecast((Vector3)a.position, (Vector3)b.position, this.mask) && !Physics2D.Linecast((Vector3)b.position, (Vector3)a.position, this.mask);
			}
			else
			{
				if (this.thickRaycast)
				{
					return !Physics.SphereCast(ray, this.thickRaycastRadius, dist, this.mask) && !Physics.SphereCast(ray2, this.thickRaycastRadius, dist, this.mask);
				}
				return !Physics.Linecast((Vector3)a.position, (Vector3)b.position, this.mask) && !Physics.Linecast((Vector3)b.position, (Vector3)a.position, this.mask);
			}
		}

		
		public GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o)
		{
			return GraphUpdateThreading.UnityThread;
		}

		
		public void UpdateAreaInit(GraphUpdateObject o)
		{
		}

		
		public void UpdateAreaPost(GraphUpdateObject o)
		{
		}

		
		public void UpdateArea(GraphUpdateObject guo)
		{
			if (this.nodes == null)
			{
				return;
			}
			for (int i = 0; i < this.nodeCount; i++)
			{
				if (guo.bounds.Contains((Vector3)this.nodes[i].position))
				{
					guo.WillUpdateNode(this.nodes[i]);
					guo.Apply(this.nodes[i]);
				}
			}
			if (guo.updatePhysics)
			{
				Bounds bounds = guo.bounds;
				if (this.thickRaycast)
				{
					bounds.Expand(this.thickRaycastRadius * 2f);
				}
				List<GraphNode> list = ListPool<GraphNode>.Claim();
				List<uint> list2 = ListPool<uint>.Claim();
				for (int j = 0; j < this.nodeCount; j++)
				{
					PointNode pointNode = this.nodes[j];
					Vector3 a = (Vector3)pointNode.position;
					List<GraphNode> list3 = null;
					List<uint> list4 = null;
					for (int k = 0; k < this.nodeCount; k++)
					{
						if (k != j)
						{
							Vector3 b = (Vector3)this.nodes[k].position;
							if (VectorMath.SegmentIntersectsBounds(bounds, a, b))
							{
								PointNode pointNode2 = this.nodes[k];
								bool flag = pointNode.ContainsConnection(pointNode2);
								float num;
								bool flag2 = this.IsValidConnection(pointNode, pointNode2, out num);
								if (!flag && flag2)
								{
									if (list3 == null)
									{
										list.Clear();
										list2.Clear();
										list3 = list;
										list4 = list2;
										list3.AddRange(pointNode.connections);
										list4.AddRange(pointNode.connectionCosts);
									}
									uint item = (uint)Mathf.RoundToInt(num * 1000f);
									list3.Add(pointNode2);
									list4.Add(item);
								}
								else if (flag && !flag2)
								{
									if (list3 == null)
									{
										list.Clear();
										list2.Clear();
										list3 = list;
										list4 = list2;
										list3.AddRange(pointNode.connections);
										list4.AddRange(pointNode.connectionCosts);
									}
									int num2 = list3.IndexOf(pointNode2);
									if (num2 != -1)
									{
										list3.RemoveAt(num2);
										list4.RemoveAt(num2);
									}
								}
							}
						}
					}
					if (list3 != null)
					{
						pointNode.connections = list3.ToArray();
						pointNode.connectionCosts = list4.ToArray();
					}
				}
				ListPool<GraphNode>.Release(list);
				ListPool<uint>.Release(list2);
			}
		}

		
		public override void PostDeserialization()
		{
			this.RebuildNodeLookup();
		}

		
		public override void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
		{
			base.RelocateNodes(oldMatrix, newMatrix);
			this.RebuildNodeLookup();
		}

		
		public override void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			base.DeserializeSettingsCompatibility(ctx);
			this.root = (ctx.DeserializeUnityObject() as Transform);
			this.searchTag = ctx.reader.ReadString();
			this.maxDistance = ctx.reader.ReadSingle();
			this.limits = ctx.DeserializeVector3();
			this.raycast = ctx.reader.ReadBoolean();
			this.use2DPhysics = ctx.reader.ReadBoolean();
			this.thickRaycast = ctx.reader.ReadBoolean();
			this.thickRaycastRadius = ctx.reader.ReadSingle();
			this.recursive = ctx.reader.ReadBoolean();
			ctx.reader.ReadBoolean();
			this.mask = ctx.reader.ReadInt32();
			this.optimizeForSparseGraph = ctx.reader.ReadBoolean();
			ctx.reader.ReadBoolean();
		}

		
		public override void SerializeExtraInfo(GraphSerializationContext ctx)
		{
			if (this.nodes == null)
			{
				ctx.writer.Write(-1);
			}
			ctx.writer.Write(this.nodeCount);
			for (int i = 0; i < this.nodeCount; i++)
			{
				if (this.nodes[i] == null)
				{
					ctx.writer.Write(-1);
				}
				else
				{
					ctx.writer.Write(0);
					this.nodes[i].SerializeNode(ctx);
				}
			}
		}

		
		public override void DeserializeExtraInfo(GraphSerializationContext ctx)
		{
			int num = ctx.reader.ReadInt32();
			if (num == -1)
			{
				this.nodes = null;
				return;
			}
			this.nodes = new PointNode[num];
			this.nodeCount = num;
			for (int i = 0; i < this.nodes.Length; i++)
			{
				if (ctx.reader.ReadInt32() != -1)
				{
					this.nodes[i] = new PointNode(this.active);
					this.nodes[i].DeserializeNode(ctx);
				}
			}
		}

		
		[JsonMember]
		public Transform root;

		
		[JsonMember]
		public string searchTag;

		
		[JsonMember]
		public float maxDistance;

		
		[JsonMember]
		public Vector3 limits;

		
		[JsonMember]
		public bool raycast = true;

		
		[JsonMember]
		public bool use2DPhysics;

		
		[JsonMember]
		public bool thickRaycast;

		
		[JsonMember]
		public float thickRaycastRadius = 1f;

		
		[JsonMember]
		public bool recursive = true;

		
		[JsonMember]
		public LayerMask mask;

		
		[JsonMember]
		public bool optimizeForSparseGraph;

		
		private PointKDTree lookupTree = new PointKDTree();

		
		public PointNode[] nodes;
	}
}
