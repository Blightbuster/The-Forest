using System;
using UnityEngine;

namespace Pathfinding
{
	public class RandomPath : ABPath
	{
		public RandomPath()
		{
		}

		[Obsolete("This constructor is obsolete. Please use the pooling API and the Construct methods")]
		public RandomPath(Vector3 start, int length, OnPathDelegate callback = null)
		{
			throw new Exception("This constructor is obsolete. Please use the pooling API and the Setup methods");
		}

		public override bool FloodingPath
		{
			get
			{
				return true;
			}
		}

		protected override bool hasEndPoint
		{
			get
			{
				return false;
			}
		}

		public override void Reset()
		{
			base.Reset();
			this.searchLength = 5000;
			this.spread = 5000;
			this.aimStrength = 0f;
			this.chosenNodeR = null;
			this.maxGScoreNodeR = null;
			this.maxGScore = 0;
			this.aim = Vector3.zero;
			this.nodesEvaluatedRep = 0;
		}

		public static RandomPath Construct(Vector3 start, int length, OnPathDelegate callback = null)
		{
			RandomPath path = PathPool.GetPath<RandomPath>();
			path.Setup(start, length, callback);
			return path;
		}

		protected RandomPath Setup(Vector3 start, int length, OnPathDelegate callback)
		{
			this.callback = callback;
			this.searchLength = length;
			this.originalStartPoint = start;
			this.originalEndPoint = Vector3.zero;
			this.startPoint = start;
			this.endPoint = Vector3.zero;
			this.startIntPoint = (Int3)start;
			return this;
		}

		public override void ReturnPath()
		{
			if (this.path != null && this.path.Count > 0)
			{
				this.endNode = this.path[this.path.Count - 1];
				this.endPoint = (Vector3)this.endNode.position;
				this.originalEndPoint = this.endPoint;
				this.hTarget = this.endNode.position;
			}
			if (this.callback != null)
			{
				this.callback(this);
			}
		}

		public override void Prepare()
		{
			this.nnConstraint.tags = this.enabledTags;
			NNInfo nearest = AstarPath.active.GetNearest(this.startPoint, this.nnConstraint, this.startHint);
			this.startPoint = nearest.position;
			this.endPoint = this.startPoint;
			this.startIntPoint = (Int3)this.startPoint;
			this.hTarget = (Int3)this.aim;
			this.startNode = nearest.node;
			this.endNode = this.startNode;
			if (this.startNode == null || this.endNode == null)
			{
				base.Error();
				return;
			}
			if (!this.startNode.Walkable)
			{
				base.Error();
				return;
			}
			this.heuristicScale = this.aimStrength;
		}

		public override void Initialize()
		{
			PathNode pathNode = base.pathHandler.GetPathNode(this.startNode);
			pathNode.node = this.startNode;
			if (this.searchLength + this.spread <= 0)
			{
				base.CompleteState = PathCompleteState.Complete;
				this.Trace(pathNode);
				return;
			}
			pathNode.pathID = base.pathID;
			pathNode.parent = null;
			pathNode.cost = 0u;
			pathNode.G = base.GetTraversalCost(this.startNode);
			pathNode.H = base.CalculateHScore(this.startNode);
			this.startNode.Open(this, pathNode, base.pathHandler);
			this.searchedNodes++;
			if (base.pathHandler.heap.isEmpty)
			{
				base.Error();
				return;
			}
			this.currentR = base.pathHandler.heap.Remove();
		}

		public override void CalculateStep(long targetTick)
		{
			int num = 0;
			while (base.CompleteState == PathCompleteState.NotCalculated)
			{
				this.searchedNodes++;
				if ((ulong)this.currentR.G >= (ulong)((long)this.searchLength))
				{
					if ((ulong)this.currentR.G > (ulong)((long)(this.searchLength + this.spread)))
					{
						if (this.chosenNodeR == null)
						{
							this.chosenNodeR = this.currentR;
						}
						base.CompleteState = PathCompleteState.Complete;
						break;
					}
					this.nodesEvaluatedRep++;
					if (this.rnd.NextDouble() <= (double)(1f / (float)this.nodesEvaluatedRep))
					{
						this.chosenNodeR = this.currentR;
					}
				}
				else if ((ulong)this.currentR.G > (ulong)((long)this.maxGScore))
				{
					this.maxGScore = (int)this.currentR.G;
					this.maxGScoreNodeR = this.currentR;
				}
				this.currentR.node.Open(this, this.currentR, base.pathHandler);
				if (base.pathHandler.heap.isEmpty)
				{
					if (this.chosenNodeR != null)
					{
						base.CompleteState = PathCompleteState.Complete;
					}
					else if (this.maxGScoreNodeR != null)
					{
						this.chosenNodeR = this.maxGScoreNodeR;
						base.CompleteState = PathCompleteState.Complete;
					}
					else
					{
						base.Error();
					}
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
			if (base.CompleteState == PathCompleteState.Complete)
			{
				this.Trace(this.chosenNodeR);
			}
		}

		public int searchLength;

		public int spread = 5000;

		public float aimStrength;

		private PathNode chosenNodeR;

		private PathNode maxGScoreNodeR;

		private int maxGScore;

		public Vector3 aim;

		private int nodesEvaluatedRep;

		private readonly System.Random rnd = new System.Random();
	}
}
