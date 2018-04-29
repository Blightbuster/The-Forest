using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	
	public class ConstantPath : Path
	{
		
		
		public override bool FloodingPath
		{
			get
			{
				return true;
			}
		}

		
		public static ConstantPath Construct(Vector3 start, int maxGScore, OnPathDelegate callback = null)
		{
			ConstantPath path = PathPool.GetPath<ConstantPath>();
			path.Setup(start, maxGScore, callback);
			return path;
		}

		
		protected void Setup(Vector3 start, int maxGScore, OnPathDelegate callback)
		{
			this.callback = callback;
			this.startPoint = start;
			this.originalStartPoint = this.startPoint;
			this.endingCondition = new EndingConditionDistance(this, maxGScore);
		}

		
		public override void OnEnterPool()
		{
			base.OnEnterPool();
			if (this.allNodes != null)
			{
				ListPool<GraphNode>.Release(this.allNodes);
			}
		}

		
		public override void Reset()
		{
			base.Reset();
			this.allNodes = ListPool<GraphNode>.Claim();
			this.endingCondition = null;
			this.originalStartPoint = Vector3.zero;
			this.startPoint = Vector3.zero;
			this.startNode = null;
			this.heuristic = Heuristic.None;
		}

		
		public override void Prepare()
		{
			this.nnConstraint.tags = this.enabledTags;
			this.startNode = AstarPath.active.GetNearest(this.startPoint, this.nnConstraint).node;
			if (this.startNode == null)
			{
				base.Error();
				return;
			}
		}

		
		public override void Initialize()
		{
			PathNode pathNode = base.pathHandler.GetPathNode(this.startNode);
			pathNode.node = this.startNode;
			pathNode.pathID = base.pathHandler.PathID;
			pathNode.parent = null;
			pathNode.cost = 0u;
			pathNode.G = base.GetTraversalCost(this.startNode);
			pathNode.H = base.CalculateHScore(this.startNode);
			this.startNode.Open(this, pathNode, base.pathHandler);
			this.searchedNodes++;
			pathNode.flag1 = true;
			this.allNodes.Add(this.startNode);
			if (base.pathHandler.heap.isEmpty)
			{
				base.CompleteState = PathCompleteState.Complete;
				return;
			}
			this.currentR = base.pathHandler.heap.Remove();
		}

		
		public override void Cleanup()
		{
			int count = this.allNodes.Count;
			for (int i = 0; i < count; i++)
			{
				base.pathHandler.GetPathNode(this.allNodes[i]).flag1 = false;
			}
		}

		
		public override void CalculateStep(long targetTick)
		{
			int num = 0;
			while (base.CompleteState == PathCompleteState.NotCalculated)
			{
				this.searchedNodes++;
				if (this.endingCondition.TargetFound(this.currentR))
				{
					base.CompleteState = PathCompleteState.Complete;
					break;
				}
				if (!this.currentR.flag1)
				{
					this.allNodes.Add(this.currentR.node);
					this.currentR.flag1 = true;
				}
				this.currentR.node.Open(this, this.currentR, base.pathHandler);
				if (base.pathHandler.heap.isEmpty)
				{
					base.CompleteState = PathCompleteState.Complete;
					break;
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
		}

		
		public GraphNode startNode;

		
		public Vector3 startPoint;

		
		public Vector3 originalStartPoint;

		
		public List<GraphNode> allNodes;

		
		public PathEndingCondition endingCondition;
	}
}
