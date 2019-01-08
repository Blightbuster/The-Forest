using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Pathfinding
{
	internal class PathProcessor
	{
		public PathProcessor(AstarPath astar, PathReturnQueue returnQueue, int processors, bool multithreaded)
		{
			this.astar = astar;
			this.returnQueue = returnQueue;
			if (processors < 0)
			{
				throw new ArgumentOutOfRangeException("processors");
			}
			if (!multithreaded && processors != 1)
			{
				throw new Exception("Only a single non-multithreaded processor is allowed");
			}
			this.queue = new ThreadControlQueue(processors);
			this.threadInfos = new PathThreadInfo[processors];
			for (int i = 0; i < processors; i++)
			{
				this.threadInfos[i] = new PathThreadInfo(i, astar, new PathHandler(i, processors));
			}
			if (multithreaded)
			{
				this.threads = new Thread[processors];
				for (int j = 0; j < processors; j++)
				{
					int threadIndex = j;
					Thread thread = new Thread(delegate
					{
						this.CalculatePathsThreaded(this.threadInfos[threadIndex]);
					});
					thread.Name = "Pathfinding Thread " + j;
					thread.IsBackground = true;
					this.threads[j] = thread;
					thread.Start();
				}
			}
			else
			{
				this.threadCoroutine = this.CalculatePaths(this.threadInfos[0]);
			}
		}

		public event Action<Path> OnPathPreSearch;

		public event Action<Path> OnPathPostSearch;

		public int NumThreads
		{
			get
			{
				return this.threadInfos.Length;
			}
		}

		public bool IsUsingMultithreading
		{
			get
			{
				return this.threads != null;
			}
		}

		public void BlockUntilPathQueueBlocked()
		{
			this.queue.Block();
			if (!Application.isPlaying)
			{
				return;
			}
			while (!this.queue.AllReceiversBlocked)
			{
				if (this.IsUsingMultithreading)
				{
					Thread.Sleep(1);
				}
				else
				{
					this.TickNonMultithreaded();
				}
			}
		}

		public void TickNonMultithreaded()
		{
			if (this.threadCoroutine != null)
			{
				try
				{
					this.threadCoroutine.MoveNext();
				}
				catch (Exception ex)
				{
					this.threadCoroutine = null;
					if (!(ex is ThreadControlQueue.QueueTerminationException))
					{
						Debug.LogException(ex);
						Debug.LogError("Unhandled exception during pathfinding. Terminating.");
						this.queue.TerminateReceivers();
						try
						{
							this.queue.PopNoBlock(false);
						}
						catch
						{
						}
					}
				}
			}
		}

		public void JoinThreads()
		{
			if (this.threads != null)
			{
				for (int i = 0; i < this.threads.Length; i++)
				{
					if (!this.threads[i].Join(50))
					{
						Debug.LogError("Could not terminate pathfinding thread[" + i + "] in 50ms, trying Thread.Abort");
						this.threads[i].Abort();
					}
				}
			}
		}

		public void AbortThreads()
		{
			if (this.threads == null)
			{
				return;
			}
			for (int i = 0; i < this.threads.Length; i++)
			{
				if (this.threads[i] != null && this.threads[i].IsAlive)
				{
					this.threads[i].Abort();
				}
			}
		}

		public int GetNewNodeIndex()
		{
			return (this.nodeIndexPool.Count <= 0) ? this.nextNodeIndex++ : this.nodeIndexPool.Pop();
		}

		public void InitializeNode(GraphNode node)
		{
			if (!this.queue.AllReceiversBlocked)
			{
				throw new Exception("Trying to initialize a node when it is not safe to initialize any nodes. Must be done during a graph update. See http://arongranberg.com/astar/docs/graph-updates.php#direct");
			}
			for (int i = 0; i < this.threadInfos.Length; i++)
			{
				this.threadInfos[i].runData.InitializeNode(node);
			}
		}

		public void DestroyNode(GraphNode node)
		{
			if (node.NodeIndex == -1)
			{
				return;
			}
			this.nodeIndexPool.Push(node.NodeIndex);
			for (int i = 0; i < this.threadInfos.Length; i++)
			{
				this.threadInfos[i].runData.DestroyNode(node);
			}
		}

		private void CalculatePathsThreaded(PathThreadInfo threadInfo)
		{
			try
			{
				PathHandler runData = threadInfo.runData;
				if (runData.nodes == null)
				{
					throw new NullReferenceException("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\nthreadInfo is an argument to the thread functions");
				}
				long num = (long)(this.astar.maxFrameTime * 10000f);
				long num2 = DateTime.UtcNow.Ticks + num;
				for (;;)
				{
					Path path = this.queue.Pop();
					num = (long)(this.astar.maxFrameTime * 10000f);
					path.PrepareBase(runData);
					path.AdvanceState(PathState.Processing);
					if (this.OnPathPreSearch != null)
					{
						this.OnPathPreSearch(path);
					}
					long ticks = DateTime.UtcNow.Ticks;
					long num3 = 0L;
					path.Prepare();
					if (!path.IsDone())
					{
						this.astar.debugPath = path;
						path.Initialize();
						while (!path.IsDone())
						{
							path.CalculateStep(num2);
							path.searchIterations++;
							if (path.IsDone())
							{
								break;
							}
							num3 += DateTime.UtcNow.Ticks - ticks;
							Thread.Sleep(0);
							ticks = DateTime.UtcNow.Ticks;
							num2 = ticks + num;
							if (this.queue.IsTerminating)
							{
								path.Error();
							}
						}
						num3 += DateTime.UtcNow.Ticks - ticks;
						path.duration = (float)num3 * 0.0001f;
					}
					path.Cleanup();
					if (path.immediateCallback != null)
					{
						path.immediateCallback(path);
					}
					if (this.OnPathPostSearch != null)
					{
						this.OnPathPostSearch(path);
					}
					this.returnQueue.Enqueue(path);
					path.AdvanceState(PathState.ReturnQueue);
					if (DateTime.UtcNow.Ticks > num2)
					{
						Thread.Sleep(1);
						num2 = DateTime.UtcNow.Ticks + num;
					}
				}
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is ThreadControlQueue.QueueTerminationException)
				{
					if (this.astar.logPathResults == PathLog.Heavy)
					{
						Debug.LogWarning("Shutting down pathfinding thread #" + threadInfo.threadIndex);
					}
					return;
				}
				Debug.LogException(ex);
				Debug.LogError("Unhandled exception during pathfinding. Terminating.");
				this.queue.TerminateReceivers();
			}
			Debug.LogError("Error : This part should never be reached.");
			this.queue.ReceiverTerminated();
		}

		private IEnumerator CalculatePaths(PathThreadInfo threadInfo)
		{
			int numPaths = 0;
			PathHandler runData = threadInfo.runData;
			if (runData.nodes == null)
			{
				throw new NullReferenceException("NodeRuns must be assigned to the threadInfo.runData.nodes field before threads are started\nthreadInfo is an argument to the thread functions");
			}
			long maxTicks = (long)(this.astar.maxFrameTime * 10000f);
			long targetTick = DateTime.UtcNow.Ticks + maxTicks;
			for (;;)
			{
				Path p = null;
				bool blockedBefore = false;
				while (p == null)
				{
					try
					{
						p = this.queue.PopNoBlock(blockedBefore);
						blockedBefore |= (p == null);
					}
					catch (ThreadControlQueue.QueueTerminationException)
					{
						yield break;
					}
					if (p == null)
					{
						yield return null;
					}
				}
				maxTicks = (long)(this.astar.maxFrameTime * 10000f);
				p.PrepareBase(runData);
				p.AdvanceState(PathState.Processing);
				Action<Path> tmpOnPathPreSearch = this.OnPathPreSearch;
				if (tmpOnPathPreSearch != null)
				{
					tmpOnPathPreSearch(p);
				}
				numPaths++;
				long startTicks = DateTime.UtcNow.Ticks;
				long totalTicks = 0L;
				p.Prepare();
				if (!p.IsDone())
				{
					this.astar.debugPath = p;
					p.Initialize();
					while (!p.IsDone())
					{
						p.CalculateStep(targetTick);
						p.searchIterations++;
						if (p.IsDone())
						{
							break;
						}
						totalTicks += DateTime.UtcNow.Ticks - startTicks;
						yield return null;
						startTicks = DateTime.UtcNow.Ticks;
						if (this.queue.IsTerminating)
						{
							p.Error();
						}
						targetTick = DateTime.UtcNow.Ticks + maxTicks;
					}
					totalTicks += DateTime.UtcNow.Ticks - startTicks;
					p.duration = (float)totalTicks * 0.0001f;
				}
				p.Cleanup();
				OnPathDelegate tmpImmediateCallback = p.immediateCallback;
				if (tmpImmediateCallback != null)
				{
					tmpImmediateCallback(p);
				}
				Action<Path> tmpOnPathPostSearch = this.OnPathPostSearch;
				if (tmpOnPathPostSearch != null)
				{
					tmpOnPathPostSearch(p);
				}
				this.returnQueue.Enqueue(p);
				p.AdvanceState(PathState.ReturnQueue);
				if (DateTime.UtcNow.Ticks > targetTick)
				{
					yield return null;
					targetTick = DateTime.UtcNow.Ticks + maxTicks;
					numPaths = 0;
				}
			}
			yield break;
		}

		public readonly ThreadControlQueue queue;

		private readonly AstarPath astar;

		private readonly PathReturnQueue returnQueue;

		private readonly PathThreadInfo[] threadInfos;

		private readonly Thread[] threads;

		private IEnumerator threadCoroutine;

		private int nextNodeIndex = 1;

		private readonly Stack<int> nodeIndexPool = new Stack<int>();
	}
}
