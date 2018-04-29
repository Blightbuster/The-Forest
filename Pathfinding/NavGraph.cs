using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	
	public abstract class NavGraph
	{
		
		public virtual int CountNodes()
		{
			int count = 0;
			GraphNodeDelegateCancelable del = delegate(GraphNode node)
			{
				count++;
				return true;
			};
			this.GetNodes(del);
			return count;
		}

		
		public abstract void GetNodes(GraphNodeDelegateCancelable del);

		
		public void SetMatrix(Matrix4x4 m)
		{
			this.matrix = m;
			this.inverseMatrix = m.inverse;
		}

		
		public virtual void RelocateNodes(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
		{
			Matrix4x4 inverse = oldMatrix.inverse;
			Matrix4x4 m = newMatrix * inverse;
			this.GetNodes(delegate(GraphNode node)
			{
				node.position = (Int3)m.MultiplyPoint((Vector3)node.position);
				return true;
			});
			this.SetMatrix(newMatrix);
		}

		
		public NNInfoInternal GetNearest(Vector3 position)
		{
			return this.GetNearest(position, NNConstraint.None);
		}

		
		public NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint)
		{
			return this.GetNearest(position, constraint, null);
		}

		
		public virtual NNInfoInternal GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
		{
			float maxDistSqr = (!constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistanceSqr;
			float minDist = float.PositiveInfinity;
			GraphNode minNode = null;
			float minConstDist = float.PositiveInfinity;
			GraphNode minConstNode = null;
			this.GetNodes(delegate(GraphNode node)
			{
				float sqrMagnitude = (position - (Vector3)node.position).sqrMagnitude;
				if (sqrMagnitude < minDist)
				{
					minDist = sqrMagnitude;
					minNode = node;
				}
				if (sqrMagnitude < minConstDist && sqrMagnitude < maxDistSqr && constraint.Suitable(node))
				{
					minConstDist = sqrMagnitude;
					minConstNode = node;
				}
				return true;
			});
			NNInfoInternal result = new NNInfoInternal(minNode);
			result.constrainedNode = minConstNode;
			if (minConstNode != null)
			{
				result.constClampedPosition = (Vector3)minConstNode.position;
			}
			else if (minNode != null)
			{
				result.constrainedNode = minNode;
				result.constClampedPosition = (Vector3)minNode.position;
			}
			return result;
		}

		
		public virtual NNInfoInternal GetNearestForce(Vector3 position, NNConstraint constraint)
		{
			return this.GetNearest(position, constraint);
		}

		
		public virtual void Awake()
		{
		}

		
		public virtual void OnDestroy()
		{
			this.GetNodes(delegate(GraphNode node)
			{
				node.Destroy();
				return true;
			});
		}

		
		public void ScanGraph()
		{
			if (AstarPath.OnPreScan != null)
			{
				AstarPath.OnPreScan(AstarPath.active);
			}
			if (AstarPath.OnGraphPreScan != null)
			{
				AstarPath.OnGraphPreScan(this);
			}
			IEnumerator<Progress> enumerator = this.ScanInternal().GetEnumerator();
			while (enumerator.MoveNext())
			{
			}
			if (AstarPath.OnGraphPostScan != null)
			{
				AstarPath.OnGraphPostScan(this);
			}
			if (AstarPath.OnPostScan != null)
			{
				AstarPath.OnPostScan(AstarPath.active);
			}
		}

		
		[Obsolete("Please use AstarPath.active.Scan or if you really want this.ScanInternal which has the same functionality as this method had")]
		public void Scan()
		{
			throw new Exception("This method is deprecated. Please use AstarPath.active.Scan or if you really want this.ScanInternal which has the same functionality as this method had.");
		}

		
		public abstract IEnumerable<Progress> ScanInternal();

		
		public virtual Color NodeColor(GraphNode node, PathHandler data)
		{
			GraphDebugMode debugMode = AstarPath.active.debugMode;
			Color result;
			switch (debugMode)
			{
			case GraphDebugMode.Areas:
				result = AstarColor.GetAreaColor(node.Area);
				goto IL_11F;
			case GraphDebugMode.Penalty:
				result = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (node.Penalty - AstarPath.active.debugFloor) / (AstarPath.active.debugRoof - AstarPath.active.debugFloor));
				goto IL_11F;
			case GraphDebugMode.Connections:
				result = AstarColor.NodeConnection;
				goto IL_11F;
			case GraphDebugMode.Tags:
				result = AstarColor.GetAreaColor(node.Tag);
				goto IL_11F;
			}
			if (data == null)
			{
				return AstarColor.NodeConnection;
			}
			PathNode pathNode = data.GetPathNode(node);
			float num;
			if (debugMode == GraphDebugMode.G)
			{
				num = pathNode.G;
			}
			else if (debugMode == GraphDebugMode.H)
			{
				num = pathNode.H;
			}
			else
			{
				num = pathNode.F;
			}
			result = Color.Lerp(AstarColor.ConnectionLowLerp, AstarColor.ConnectionHighLerp, (num - AstarPath.active.debugFloor) / (AstarPath.active.debugRoof - AstarPath.active.debugFloor));
			IL_11F:
			result.a *= 0.5f;
			return result;
		}

		
		public virtual void SerializeExtraInfo(GraphSerializationContext ctx)
		{
		}

		
		public virtual void DeserializeExtraInfo(GraphSerializationContext ctx)
		{
		}

		
		public virtual void PostDeserialization()
		{
		}

		
		public virtual void DeserializeSettingsCompatibility(GraphSerializationContext ctx)
		{
			this.guid = new Pathfinding.Util.Guid(ctx.reader.ReadBytes(16));
			this.initialPenalty = ctx.reader.ReadUInt32();
			this.open = ctx.reader.ReadBoolean();
			this.name = ctx.reader.ReadString();
			this.drawGizmos = ctx.reader.ReadBoolean();
			this.infoScreenOpen = ctx.reader.ReadBoolean();
			for (int i = 0; i < 4; i++)
			{
				Vector4 zero = Vector4.zero;
				for (int j = 0; j < 4; j++)
				{
					zero[j] = ctx.reader.ReadSingle();
				}
				this.matrix.SetRow(i, zero);
			}
		}

		
		public static bool InSearchTree(GraphNode node, Path path)
		{
			if (path == null || path.pathHandler == null)
			{
				return true;
			}
			PathNode pathNode = path.pathHandler.GetPathNode(node);
			return pathNode.pathID == path.pathID;
		}

		
		public virtual void OnDrawGizmos(bool drawNodes)
		{
			if (!drawNodes)
			{
				return;
			}
			PathHandler data = AstarPath.active.debugPathData;
			GraphNode node = null;
			GraphNodeDelegate drawConnection = delegate(GraphNode otherNode)
			{
				Gizmos.DrawLine((Vector3)node.position, (Vector3)otherNode.position);
			};
			this.GetNodes(delegate(GraphNode _node)
			{
				node = _node;
				Gizmos.color = this.NodeColor(node, AstarPath.active.debugPathData);
				if (AstarPath.active.showSearchTree && !NavGraph.InSearchTree(node, AstarPath.active.debugPath))
				{
					return true;
				}
				PathNode pathNode = (data == null) ? null : data.GetPathNode(node);
				if (AstarPath.active.showSearchTree && pathNode != null && pathNode.parent != null)
				{
					Gizmos.DrawLine((Vector3)node.position, (Vector3)pathNode.parent.node.position);
				}
				else
				{
					node.GetConnections(drawConnection);
				}
				return true;
			});
		}

		
		internal virtual void UnloadGizmoMeshes()
		{
		}

		
		public AstarPath active;

		
		[JsonMember]
		public Pathfinding.Util.Guid guid;

		
		[JsonMember]
		public uint initialPenalty;

		
		[JsonMember]
		public bool open;

		
		public uint graphIndex;

		
		[JsonMember]
		public string name;

		
		[JsonMember]
		public bool drawGizmos = true;

		
		[JsonMember]
		public bool infoScreenOpen;

		
		public Matrix4x4 matrix = Matrix4x4.identity;

		
		public Matrix4x4 inverseMatrix = Matrix4x4.identity;
	}
}
