using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding
{
	public abstract class Path
	{
		public PathHandler pathHandler { get; private set; }

		public PathCompleteState CompleteState
		{
			get
			{
				return this.pathCompleteState;
			}
			protected set
			{
				this.pathCompleteState = value;
			}
		}

		public bool error
		{
			get
			{
				return this.CompleteState == PathCompleteState.Error;
			}
		}

		public string errorLog
		{
			get
			{
				return this._errorLog;
			}
		}

		public DateTime callTime { get; private set; }

		[Obsolete("Has been renamed to 'pooled' to use more widely underestood terminology")]
		internal bool recycled
		{
			get
			{
				return this.pooled;
			}
			set
			{
				this.pooled = value;
			}
		}

		public ushort pathID { get; private set; }

		public int[] tagPenalties
		{
			get
			{
				return this.manualTagPenalties;
			}
			set
			{
				if (value == null || value.Length != 32)
				{
					this.manualTagPenalties = null;
					this.internalTagPenalties = Path.ZeroTagPenalties;
				}
				else
				{
					this.manualTagPenalties = value;
					this.internalTagPenalties = value;
				}
			}
		}

		public virtual bool FloodingPath
		{
			get
			{
				return false;
			}
		}

		public float GetTotalLength()
		{
			if (this.vectorPath == null)
			{
				return float.PositiveInfinity;
			}
			float num = 0f;
			for (int i = 0; i < this.vectorPath.Count - 1; i++)
			{
				num += Vector3.Distance(this.vectorPath[i], this.vectorPath[i + 1]);
			}
			return num;
		}

		public IEnumerator WaitForPath()
		{
			if (this.GetState() == PathState.Created)
			{
				throw new InvalidOperationException("This path has not been started yet");
			}
			while (this.GetState() != PathState.Returned)
			{
				yield return null;
			}
			yield break;
		}

		public uint CalculateHScore(GraphNode node)
		{
			Heuristic heuristic = this.heuristic;
			uint num;
			if (heuristic == Heuristic.Euclidean)
			{
				num = (uint)((float)(this.GetHTarget() - node.position).costMagnitude * this.heuristicScale);
				if (this.hTargetNode != null)
				{
					num = Math.Max(num, AstarPath.active.euclideanEmbedding.GetHeuristic(node.NodeIndex, this.hTargetNode.NodeIndex));
				}
				return num;
			}
			if (heuristic == Heuristic.Manhattan)
			{
				Int3 position = node.position;
				num = (uint)((float)(Math.Abs(this.hTarget.x - position.x) + Math.Abs(this.hTarget.y - position.y) + Math.Abs(this.hTarget.z - position.z)) * this.heuristicScale);
				if (this.hTargetNode != null)
				{
					num = Math.Max(num, AstarPath.active.euclideanEmbedding.GetHeuristic(node.NodeIndex, this.hTargetNode.NodeIndex));
				}
				return num;
			}
			if (heuristic != Heuristic.DiagonalManhattan)
			{
				return 0u;
			}
			Int3 @int = this.GetHTarget() - node.position;
			@int.x = Math.Abs(@int.x);
			@int.y = Math.Abs(@int.y);
			@int.z = Math.Abs(@int.z);
			int num2 = Math.Min(@int.x, @int.z);
			int num3 = Math.Max(@int.x, @int.z);
			num = (uint)((float)(14 * num2 / 10 + (num3 - num2) + @int.y) * this.heuristicScale);
			if (this.hTargetNode != null)
			{
				num = Math.Max(num, AstarPath.active.euclideanEmbedding.GetHeuristic(node.NodeIndex, this.hTargetNode.NodeIndex));
			}
			return num;
		}

		public uint GetTagPenalty(int tag)
		{
			return (uint)this.internalTagPenalties[tag];
		}

		public Int3 GetHTarget()
		{
			return this.hTarget;
		}

		public bool CanTraverse(GraphNode node)
		{
			return node.Walkable && (this.enabledTags >> (int)node.Tag & 1) != 0;
		}

		public uint GetTraversalCost(GraphNode node)
		{
			return this.GetTagPenalty((int)node.Tag) + node.Penalty;
		}

		public virtual uint GetConnectionSpecialCost(GraphNode a, GraphNode b, uint currentCost)
		{
			return currentCost;
		}

		public bool IsDone()
		{
			return this.CompleteState != PathCompleteState.NotCalculated;
		}

		public void AdvanceState(PathState s)
		{
			object obj = this.stateLock;
			lock (obj)
			{
				this.state = (PathState)Math.Max((int)this.state, (int)s);
			}
		}

		public PathState GetState()
		{
			return this.state;
		}

		[Conditional("DISABLED")]
		internal void LogError(string msg)
		{
			if (AstarPath.active.logPathResults != PathLog.None)
			{
				this._errorLog += msg;
			}
			if (AstarPath.active.logPathResults != PathLog.None && AstarPath.active.logPathResults != PathLog.InGame)
			{
				UnityEngine.Debug.LogWarning(msg);
			}
		}

		internal void ForceLogError(string msg)
		{
			this.Error();
			this._errorLog += msg;
			UnityEngine.Debug.LogError(msg);
		}

		internal void Log(string msg)
		{
			if (AstarPath.active.logPathResults != PathLog.None)
			{
				this._errorLog += msg;
			}
		}

		public void Error()
		{
			this.CompleteState = PathCompleteState.Error;
		}

		private void ErrorCheck()
		{
			if (!this.hasBeenReset)
			{
				throw new Exception("The path has never been reset. Use pooling API or call Reset() after creating the path with the default constructor.");
			}
			if (this.pooled)
			{
				throw new Exception("The path is currently in a path pool. Are you sending the path for calculation twice?");
			}
			if (this.pathHandler == null)
			{
				throw new Exception("Field pathHandler is not set. Please report this bug.");
			}
			if (this.GetState() > PathState.Processing)
			{
				throw new Exception("This path has already been processed. Do not request a path with the same path object twice.");
			}
		}

		public virtual void OnEnterPool()
		{
			if (this.vectorPath != null)
			{
				ListPool<Vector3>.Release(this.vectorPath);
			}
			if (this.path != null)
			{
				ListPool<GraphNode>.Release(this.path);
			}
			this.vectorPath = null;
			this.path = null;
		}

		public virtual void Reset()
		{
			if (object.ReferenceEquals(AstarPath.active, null))
			{
				throw new NullReferenceException("No AstarPath object found in the scene. Make sure there is one or do not create paths in Awake");
			}
			this.hasBeenReset = true;
			this.state = PathState.Created;
			this.releasedNotSilent = false;
			this.pathHandler = null;
			this.callback = null;
			this._errorLog = string.Empty;
			this.pathCompleteState = PathCompleteState.NotCalculated;
			this.path = ListPool<GraphNode>.Claim();
			this.vectorPath = ListPool<Vector3>.Claim();
			this.currentR = null;
			this.duration = 0f;
			this.searchIterations = 0;
			this.searchedNodes = 0;
			this.nnConstraint = PathNNConstraint.Default;
			this.next = null;
			this.heuristic = AstarPath.active.heuristic;
			this.heuristicScale = AstarPath.active.heuristicScale;
			this.enabledTags = -1;
			this.tagPenalties = null;
			this.callTime = DateTime.UtcNow;
			this.pathID = AstarPath.active.GetNextPathID();
			this.hTarget = Int3.zero;
			this.hTargetNode = null;
		}

		protected bool HasExceededTime(int searchedNodes, long targetTime)
		{
			return DateTime.UtcNow.Ticks >= targetTime;
		}

		public void Claim(object o)
		{
			if (object.ReferenceEquals(o, null))
			{
				throw new ArgumentNullException("o");
			}
			for (int i = 0; i < this.claimed.Count; i++)
			{
				if (object.ReferenceEquals(this.claimed[i], o))
				{
					throw new ArgumentException("You have already claimed the path with that object (" + o + "). Are you claiming the path with the same object twice?");
				}
			}
			this.claimed.Add(o);
		}

		[Obsolete("Use Release(o, true) instead")]
		public void ReleaseSilent(object o)
		{
			this.Release(o, true);
		}

		public void Release(object o, bool silent = false)
		{
			if (o == null)
			{
				throw new ArgumentNullException("o");
			}
			for (int i = 0; i < this.claimed.Count; i++)
			{
				if (object.ReferenceEquals(this.claimed[i], o))
				{
					this.claimed.RemoveAt(i);
					if (!silent)
					{
						this.releasedNotSilent = true;
					}
					if (this.claimed.Count == 0 && this.releasedNotSilent)
					{
						PathPool.Pool(this);
					}
					return;
				}
			}
			if (this.claimed.Count == 0)
			{
				throw new ArgumentException("You are releasing a path which is not claimed at all (most likely it has been pooled already). Are you releasing the path with the same object (" + o + ") twice?\nCheck out the documentation on path pooling for help.");
			}
			throw new ArgumentException("You are releasing a path which has not been claimed with this object (" + o + "). Are you releasing the path with the same object twice?\nCheck out the documentation on path pooling for help.");
		}

		protected virtual void Trace(PathNode from)
		{
			PathNode pathNode = from;
			int num = 0;
			while (pathNode != null)
			{
				pathNode = pathNode.parent;
				num++;
				if (num > 2048)
				{
					UnityEngine.Debug.LogWarning("Infinite loop? >2048 node path. Remove this message if you really have that long paths (Path.cs, Trace method)");
					break;
				}
			}
			if (this.path.Capacity < num)
			{
				this.path.Capacity = num;
			}
			if (this.vectorPath.Capacity < num)
			{
				this.vectorPath.Capacity = num;
			}
			pathNode = from;
			for (int i = 0; i < num; i++)
			{
				this.path.Add(pathNode.node);
				pathNode = pathNode.parent;
			}
			int num2 = num / 2;
			for (int j = 0; j < num2; j++)
			{
				GraphNode value = this.path[j];
				this.path[j] = this.path[num - j - 1];
				this.path[num - j - 1] = value;
			}
			for (int k = 0; k < num; k++)
			{
				this.vectorPath.Add((Vector3)this.path[k].position);
			}
		}

		protected void DebugStringPrefix(PathLog logMode, StringBuilder text)
		{
			text.Append((!this.error) ? "Path Completed : " : "Path Failed : ");
			text.Append("Computation Time ");
			text.Append(this.duration.ToString((logMode != PathLog.Heavy) ? "0.00 ms " : "0.000 ms "));
			text.Append("Searched Nodes ").Append(this.searchedNodes);
			if (!this.error)
			{
				text.Append(" Path Length ");
				text.Append((this.path != null) ? this.path.Count.ToString() : "Null");
				if (logMode == PathLog.Heavy)
				{
					text.Append("\nSearch Iterations ").Append(this.searchIterations);
				}
			}
		}

		protected void DebugStringSuffix(PathLog logMode, StringBuilder text)
		{
			if (this.error)
			{
				text.Append("\nError: ").Append(this.errorLog);
			}
			if (logMode == PathLog.Heavy && !AstarPath.active.IsUsingMultithreading)
			{
				text.Append("\nCallback references ");
				if (this.callback != null)
				{
					text.Append(this.callback.Target.GetType().FullName).AppendLine();
				}
				else
				{
					text.AppendLine("NULL");
				}
			}
			text.Append("\nPath Number ").Append(this.pathID).Append(" (unique id)");
		}

		public virtual string DebugString(PathLog logMode)
		{
			if (logMode == PathLog.None || (!this.error && logMode == PathLog.OnlyErrors))
			{
				return string.Empty;
			}
			StringBuilder debugStringBuilder = this.pathHandler.DebugStringBuilder;
			debugStringBuilder.Length = 0;
			this.DebugStringPrefix(logMode, debugStringBuilder);
			this.DebugStringSuffix(logMode, debugStringBuilder);
			return debugStringBuilder.ToString();
		}

		public virtual void ReturnPath()
		{
			if (this.callback != null)
			{
				this.callback(this);
			}
		}

		internal void PrepareBase(PathHandler pathHandler)
		{
			if (pathHandler.PathID > this.pathID)
			{
				pathHandler.ClearPathIDs();
			}
			this.pathHandler = pathHandler;
			pathHandler.InitializeForPath(this);
			if (this.internalTagPenalties == null || this.internalTagPenalties.Length != 32)
			{
				this.internalTagPenalties = Path.ZeroTagPenalties;
			}
			try
			{
				this.ErrorCheck();
			}
			catch (Exception ex)
			{
				this.ForceLogError(string.Concat(new object[]
				{
					"Exception in path ",
					this.pathID,
					"\n",
					ex
				}));
			}
		}

		public abstract void Prepare();

		public virtual void Cleanup()
		{
		}

		public abstract void Initialize();

		public abstract void CalculateStep(long targetTick);

		public OnPathDelegate callback;

		public OnPathDelegate immediateCallback;

		private PathState state;

		private object stateLock = new object();

		private PathCompleteState pathCompleteState;

		private string _errorLog = string.Empty;

		public List<GraphNode> path;

		public List<Vector3> vectorPath;

		protected float maxFrameTime;

		protected PathNode currentR;

		public float duration;

		public int searchIterations;

		public int searchedNodes;

		internal bool pooled;

		protected bool hasBeenReset;

		public NNConstraint nnConstraint = PathNNConstraint.Default;

		internal Path next;

		public Heuristic heuristic;

		public float heuristicScale = 1f;

		protected GraphNode hTargetNode;

		protected Int3 hTarget;

		public int enabledTags = -1;

		private static readonly int[] ZeroTagPenalties = new int[32];

		protected int[] internalTagPenalties;

		protected int[] manualTagPenalties;

		private List<object> claimed = new List<object>();

		private bool releasedNotSilent;
	}
}
