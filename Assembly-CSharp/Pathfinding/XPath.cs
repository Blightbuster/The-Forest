using System;
using UnityEngine;

namespace Pathfinding
{
	public class XPath : ABPath
	{
		public new static XPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
		{
			XPath path = PathPool.GetPath<XPath>();
			path.Setup(start, end, callback);
			path.endingCondition = new ABPathEndingCondition(path);
			return path;
		}

		public override void Reset()
		{
			base.Reset();
			this.endingCondition = null;
		}

		protected override bool EndPointGridGraphSpecialCase(GraphNode endNode)
		{
			return false;
		}

		protected override void CompletePathIfStartIsValidTarget()
		{
			PathNode pathNode = base.pathHandler.GetPathNode(this.startNode);
			if (this.endingCondition.TargetFound(pathNode))
			{
				this.ChangeEndNode(this.startNode);
				this.Trace(pathNode);
				base.CompleteState = PathCompleteState.Complete;
			}
		}

		private void ChangeEndNode(GraphNode target)
		{
			if (this.endNode != null && this.endNode != this.startNode)
			{
				PathNode pathNode = base.pathHandler.GetPathNode(this.endNode);
				PathNode pathNode2 = pathNode;
				bool flag = false;
				pathNode.flag2 = flag;
				pathNode2.flag1 = flag;
			}
			this.endNode = target;
			this.endPoint = (Vector3)target.position;
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
				this.ChangeEndNode(this.currentR.node);
				this.Trace(this.currentR);
			}
		}

		public PathEndingCondition endingCondition;
	}
}
