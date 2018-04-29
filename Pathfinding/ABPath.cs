using System;
using System.Text;
using UnityEngine;

namespace Pathfinding
{
	
	public class ABPath : Path
	{
		
		
		protected virtual bool hasEndPoint
		{
			get
			{
				return true;
			}
		}

		
		public static ABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
		{
			ABPath path = PathPool.GetPath<ABPath>();
			path.Setup(start, end, callback);
			return path;
		}

		
		protected void Setup(Vector3 start, Vector3 end, OnPathDelegate callbackDelegate)
		{
			this.callback = callbackDelegate;
			this.UpdateStartEnd(start, end);
		}

		
		protected void UpdateStartEnd(Vector3 start, Vector3 end)
		{
			this.originalStartPoint = start;
			this.originalEndPoint = end;
			this.startPoint = start;
			this.endPoint = end;
			this.startIntPoint = (Int3)start;
			this.hTarget = (Int3)end;
		}

		
		public override uint GetConnectionSpecialCost(GraphNode a, GraphNode b, uint currentCost)
		{
			if (this.startNode != null && this.endNode != null)
			{
				if (a == this.startNode)
				{
					return (uint)((double)(this.startIntPoint - ((b != this.endNode) ? b.position : this.hTarget)).costMagnitude * (currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (b == this.startNode)
				{
					return (uint)((double)(this.startIntPoint - ((a != this.endNode) ? a.position : this.hTarget)).costMagnitude * (currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (a == this.endNode)
				{
					return (uint)((double)(this.hTarget - b.position).costMagnitude * (currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (b == this.endNode)
				{
					return (uint)((double)(this.hTarget - a.position).costMagnitude * (currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
			}
			else
			{
				if (a == this.startNode)
				{
					return (uint)((double)(this.startIntPoint - b.position).costMagnitude * (currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
				if (b == this.startNode)
				{
					return (uint)((double)(this.startIntPoint - a.position).costMagnitude * (currentCost * 1.0 / (double)(a.position - b.position).costMagnitude));
				}
			}
			return currentCost;
		}

		
		public override void Reset()
		{
			base.Reset();
			this.startNode = null;
			this.endNode = null;
			this.startHint = null;
			this.endHint = null;
			this.originalStartPoint = Vector3.zero;
			this.originalEndPoint = Vector3.zero;
			this.startPoint = Vector3.zero;
			this.endPoint = Vector3.zero;
			this.calculatePartial = false;
			this.partialBestTarget = null;
			this.startIntPoint = default(Int3);
			this.hTarget = default(Int3);
			this.endNodeCosts = null;
			this.gridSpecialCaseNode = null;
		}

		
		protected virtual bool EndPointGridGraphSpecialCase(GraphNode closestWalkableEndNode)
		{
			GridNode gridNode = closestWalkableEndNode as GridNode;
			if (gridNode != null)
			{
				GridGraph gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);
				GridNode gridNode2 = AstarPath.active.GetNearest(this.originalEndPoint, NNConstraint.None, this.endHint).node as GridNode;
				if (gridNode != gridNode2 && gridNode2 != null && gridNode.GraphIndex == gridNode2.GraphIndex)
				{
					int num = gridNode.NodeInGridIndex % gridGraph.width;
					int num2 = gridNode.NodeInGridIndex / gridGraph.width;
					int num3 = gridNode2.NodeInGridIndex % gridGraph.width;
					int num4 = gridNode2.NodeInGridIndex / gridGraph.width;
					bool flag = false;
					switch (gridGraph.neighbours)
					{
					case NumNeighbours.Four:
						if ((num == num3 && Math.Abs(num2 - num4) == 1) || (num2 == num4 && Math.Abs(num - num3) == 1))
						{
							flag = true;
						}
						break;
					case NumNeighbours.Eight:
						if (Math.Abs(num - num3) <= 1 && Math.Abs(num2 - num4) <= 1)
						{
							flag = true;
						}
						break;
					case NumNeighbours.Six:
						for (int i = 0; i < 6; i++)
						{
							int num5 = num3 + gridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
							int num6 = num4 + gridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
							if (num == num5 && num2 == num6)
							{
								flag = true;
								break;
							}
						}
						break;
					default:
						throw new Exception("Unhandled NumNeighbours");
					}
					if (flag)
					{
						this.SetFlagOnSurroundingGridNodes(gridNode2, 1, true);
						this.endPoint = (Vector3)gridNode2.position;
						this.hTarget = gridNode2.position;
						this.endNode = gridNode2;
						this.hTargetNode = this.endNode;
						this.gridSpecialCaseNode = gridNode2;
						return true;
					}
				}
			}
			return false;
		}

		
		private void SetFlagOnSurroundingGridNodes(GridNode gridNode, int flag, bool flagState)
		{
			GridGraph gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);
			int num = (gridGraph.neighbours != NumNeighbours.Four) ? ((gridGraph.neighbours != NumNeighbours.Eight) ? 6 : 8) : 4;
			int num2 = gridNode.NodeInGridIndex % gridGraph.width;
			int num3 = gridNode.NodeInGridIndex / gridGraph.width;
			if (flag != 1 && flag != 2)
			{
				throw new ArgumentOutOfRangeException("flag");
			}
			for (int i = 0; i < num; i++)
			{
				int num4;
				int num5;
				if (gridGraph.neighbours == NumNeighbours.Six)
				{
					num4 = num2 + gridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
					num5 = num3 + gridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
				}
				else
				{
					num4 = num2 + gridGraph.neighbourXOffsets[i];
					num5 = num3 + gridGraph.neighbourZOffsets[i];
				}
				if (num4 >= 0 && num5 >= 0 && num4 < gridGraph.width && num5 < gridGraph.depth)
				{
					GridNode node = gridGraph.nodes[num5 * gridGraph.width + num4];
					PathNode pathNode = base.pathHandler.GetPathNode(node);
					if (flag == 1)
					{
						pathNode.flag1 = flagState;
					}
					else
					{
						pathNode.flag2 = flagState;
					}
				}
			}
		}

		
		public override void Prepare()
		{
			this.nnConstraint.tags = this.enabledTags;
			NNInfo nearest = AstarPath.active.GetNearest(this.startPoint, this.nnConstraint, this.startHint);
			PathNNConstraint pathNNConstraint = this.nnConstraint as PathNNConstraint;
			if (pathNNConstraint != null)
			{
				pathNNConstraint.SetStart(nearest.node);
			}
			this.startPoint = nearest.position;
			this.startIntPoint = (Int3)this.startPoint;
			this.startNode = nearest.node;
			if (this.startNode == null)
			{
				base.Error();
				return;
			}
			if (!this.startNode.Walkable)
			{
				base.Error();
				return;
			}
			if (this.hasEndPoint)
			{
				NNInfo nearest2 = AstarPath.active.GetNearest(this.endPoint, this.nnConstraint, this.endHint);
				this.endPoint = nearest2.position;
				this.endNode = nearest2.node;
				if (this.startNode == null && this.endNode == null)
				{
					base.Error();
					return;
				}
				if (this.endNode == null)
				{
					base.Error();
					return;
				}
				if (!this.endNode.Walkable)
				{
					base.Error();
					return;
				}
				if (this.startNode.Area != this.endNode.Area)
				{
					base.Error();
					return;
				}
				if (!this.EndPointGridGraphSpecialCase(nearest2.node))
				{
					this.hTarget = (Int3)this.endPoint;
					this.hTargetNode = this.endNode;
					base.pathHandler.GetPathNode(this.endNode).flag1 = true;
				}
			}
		}

		
		protected virtual void CompletePathIfStartIsValidTarget()
		{
			if (this.hasEndPoint && base.pathHandler.GetPathNode(this.startNode).flag1)
			{
				this.CompleteWith(this.startNode);
				this.Trace(base.pathHandler.GetPathNode(this.startNode));
			}
		}

		
		public override void Initialize()
		{
			if (this.startNode != null)
			{
				base.pathHandler.GetPathNode(this.startNode).flag2 = true;
			}
			if (this.endNode != null)
			{
				base.pathHandler.GetPathNode(this.endNode).flag2 = true;
			}
			PathNode pathNode = base.pathHandler.GetPathNode(this.startNode);
			pathNode.node = this.startNode;
			pathNode.pathID = base.pathHandler.PathID;
			pathNode.parent = null;
			pathNode.cost = 0u;
			pathNode.G = base.GetTraversalCost(this.startNode);
			pathNode.H = base.CalculateHScore(this.startNode);
			this.CompletePathIfStartIsValidTarget();
			if (base.CompleteState == PathCompleteState.Complete)
			{
				return;
			}
			this.startNode.Open(this, pathNode, base.pathHandler);
			this.searchedNodes++;
			this.partialBestTarget = pathNode;
			if (base.pathHandler.heap.isEmpty)
			{
				if (!this.calculatePartial)
				{
					base.Error();
					return;
				}
				base.CompleteState = PathCompleteState.Partial;
				this.Trace(this.partialBestTarget);
			}
			this.currentR = base.pathHandler.heap.Remove();
		}

		
		public override void Cleanup()
		{
			if (this.startNode != null)
			{
				PathNode pathNode = base.pathHandler.GetPathNode(this.startNode);
				pathNode.flag1 = false;
				pathNode.flag2 = false;
			}
			if (this.endNode != null)
			{
				PathNode pathNode2 = base.pathHandler.GetPathNode(this.endNode);
				pathNode2.flag1 = false;
				pathNode2.flag2 = false;
			}
			if (this.gridSpecialCaseNode != null)
			{
				PathNode pathNode3 = base.pathHandler.GetPathNode(this.gridSpecialCaseNode);
				pathNode3.flag1 = false;
				pathNode3.flag2 = false;
				this.SetFlagOnSurroundingGridNodes(this.gridSpecialCaseNode, 1, false);
				this.SetFlagOnSurroundingGridNodes(this.gridSpecialCaseNode, 2, false);
			}
		}

		
		private void CompleteWith(GraphNode node)
		{
			if (this.endNode != node)
			{
				GridNode gridNode = node as GridNode;
				if (gridNode == null)
				{
					throw new Exception("Some path is not cleaning up the flag1 field. This is a bug.");
				}
				this.endPoint = gridNode.ClosestPointOnNode(this.originalEndPoint);
				this.endNode = node;
			}
			base.CompleteState = PathCompleteState.Complete;
		}

		
		public override void CalculateStep(long targetTick)
		{
			int num = 0;
			while (base.CompleteState == PathCompleteState.NotCalculated)
			{
				this.searchedNodes++;
				if (this.currentR.flag1)
				{
					this.CompleteWith(this.currentR.node);
					break;
				}
				if (this.currentR.H < this.partialBestTarget.H)
				{
					this.partialBestTarget = this.currentR;
				}
				this.currentR.node.Open(this, this.currentR, base.pathHandler);
				if (base.pathHandler.heap.isEmpty)
				{
					base.Error();
					return;
				}
				this.currentR = base.pathHandler.heap.Remove();
				if (num > 500)
				{
					if (DateTime.UtcNow.Ticks >= targetTick)
					{
						return;
					}
					num = 0;
					if (this.searchedNodes > 1000000)
					{
						throw new Exception("Probable infinite loop. Over 1,000,000 nodes searched");
					}
				}
				num++;
			}
			if (base.CompleteState == PathCompleteState.Complete)
			{
				this.Trace(this.currentR);
			}
			else if (this.calculatePartial && this.partialBestTarget != null)
			{
				base.CompleteState = PathCompleteState.Partial;
				this.Trace(this.partialBestTarget);
			}
		}

		
		public void ResetCosts(Path p)
		{
		}

		
		public override string DebugString(PathLog logMode)
		{
			if (logMode == PathLog.None || (!base.error && logMode == PathLog.OnlyErrors))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			base.DebugStringPrefix(logMode, stringBuilder);
			if (!base.error && logMode == PathLog.Heavy)
			{
				stringBuilder.Append("\nSearch Iterations " + this.searchIterations);
				if (this.hasEndPoint && this.endNode != null)
				{
					PathNode pathNode = base.pathHandler.GetPathNode(this.endNode);
					stringBuilder.Append("\nEnd Node\n\tG: ");
					stringBuilder.Append(pathNode.G);
					stringBuilder.Append("\n\tH: ");
					stringBuilder.Append(pathNode.H);
					stringBuilder.Append("\n\tF: ");
					stringBuilder.Append(pathNode.F);
					stringBuilder.Append("\n\tPoint: ");
					StringBuilder stringBuilder2 = stringBuilder;
					Vector3 vector = this.endPoint;
					stringBuilder2.Append(vector.ToString());
					stringBuilder.Append("\n\tGraph: ");
					stringBuilder.Append(this.endNode.GraphIndex);
				}
				stringBuilder.Append("\nStart Node");
				stringBuilder.Append("\n\tPoint: ");
				StringBuilder stringBuilder3 = stringBuilder;
				Vector3 vector2 = this.startPoint;
				stringBuilder3.Append(vector2.ToString());
				stringBuilder.Append("\n\tGraph: ");
				if (this.startNode != null)
				{
					stringBuilder.Append(this.startNode.GraphIndex);
				}
				else
				{
					stringBuilder.Append("< null startNode >");
				}
			}
			base.DebugStringSuffix(logMode, stringBuilder);
			return stringBuilder.ToString();
		}

		
		public Vector3 GetMovementVector(Vector3 point)
		{
			if (this.vectorPath == null || this.vectorPath.Count == 0)
			{
				return Vector3.zero;
			}
			if (this.vectorPath.Count == 1)
			{
				return this.vectorPath[0] - point;
			}
			float num = float.PositiveInfinity;
			int num2 = 0;
			for (int i = 0; i < this.vectorPath.Count - 1; i++)
			{
				Vector3 a = VectorMath.ClosestPointOnSegment(this.vectorPath[i], this.vectorPath[i + 1], point);
				float sqrMagnitude = (a - point).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					num2 = i;
				}
			}
			return this.vectorPath[num2 + 1] - point;
		}

		
		public bool recalcStartEndCosts = true;

		
		public GraphNode startNode;

		
		public GraphNode endNode;

		
		public GraphNode startHint;

		
		public GraphNode endHint;

		
		public Vector3 originalStartPoint;

		
		public Vector3 originalEndPoint;

		
		public Vector3 startPoint;

		
		public Vector3 endPoint;

		
		public Int3 startIntPoint;

		
		public bool calculatePartial;

		
		protected PathNode partialBestTarget;

		
		protected int[] endNodeCosts;

		
		private GridNode gridSpecialCaseNode;
	}
}
