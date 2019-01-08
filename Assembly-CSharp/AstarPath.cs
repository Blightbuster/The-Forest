using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Pathfinding;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Pathfinding/Pathfinder")]
[HelpURL("http://arongranberg.com/astar/docs/class_astar_path.php")]
public class AstarPath : MonoBehaviour
{
	private AstarPath()
	{
		this.pathProcessor = new PathProcessor(this, this.pathReturnQueue, 0, true);
		this.pathReturnQueue = new PathReturnQueue(this);
		this.workItems = new WorkItemProcessor(this);
		this.graphUpdates = new GraphUpdateProcessor(this);
		this.graphUpdates.OnGraphsUpdated += delegate
		{
			if (AstarPath.OnGraphsUpdated != null)
			{
				AstarPath.OnGraphsUpdated(this);
			}
		};
	}

	public static Version Version
	{
		get
		{
			return new Version(3, 8, 4);
		}
	}

	[Obsolete]
	public Type[] graphTypes
	{
		get
		{
			return this.astarData.graphTypes;
		}
	}

	public NavGraph[] graphs
	{
		get
		{
			if (this.astarData == null)
			{
				this.astarData = new AstarData();
			}
			return this.astarData.graphs;
		}
		set
		{
			if (this.astarData == null)
			{
				this.astarData = new AstarData();
			}
			this.astarData.graphs = value;
		}
	}

	public float maxNearestNodeDistanceSqr
	{
		get
		{
			return this.maxNearestNodeDistance * this.maxNearestNodeDistance;
		}
	}

	[Obsolete("This field has been renamed to 'batchGraphUpdates'")]
	public bool limitGraphUpdates
	{
		get
		{
			return this.batchGraphUpdates;
		}
		set
		{
			this.batchGraphUpdates = value;
		}
	}

	[Obsolete("This field has been renamed to 'graphUpdateBatchingInterval'")]
	public float maxGraphUpdateFreq
	{
		get
		{
			return this.graphUpdateBatchingInterval;
		}
		set
		{
			this.graphUpdateBatchingInterval = value;
		}
	}

	public float lastScanTime { get; private set; }

	public PathHandler debugPathData
	{
		get
		{
			if (this.debugPath == null)
			{
				return null;
			}
			return this.debugPath.pathHandler;
		}
	}

	public bool isScanning { get; private set; }

	public int NumParallelThreads
	{
		get
		{
			return this.pathProcessor.NumThreads;
		}
	}

	public bool IsUsingMultithreading
	{
		get
		{
			return this.pathProcessor.IsUsingMultithreading;
		}
	}

	[Obsolete("Fixed grammar, use IsAnyGraphUpdateQueued instead")]
	public bool IsAnyGraphUpdatesQueued
	{
		get
		{
			return this.IsAnyGraphUpdateQueued;
		}
	}

	public bool IsAnyGraphUpdateQueued
	{
		get
		{
			return this.graphUpdates.IsAnyGraphUpdateQueued;
		}
	}

	public bool IsAnyGraphUpdateInProgress
	{
		get
		{
			return this.graphUpdates.IsAnyGraphUpdateInProgress;
		}
	}

	public bool IsAnyWorkItemInProgress
	{
		get
		{
			return this.workItems.workItemsInProgress;
		}
	}

	public string[] GetTagNames()
	{
		if (this.tagNames == null || this.tagNames.Length != 32)
		{
			this.tagNames = new string[32];
			for (int i = 0; i < this.tagNames.Length; i++)
			{
				this.tagNames[i] = string.Empty + i;
			}
			this.tagNames[0] = "Basic Ground";
		}
		return this.tagNames;
	}

	public static string[] FindTagNames()
	{
		if (AstarPath.active != null)
		{
			return AstarPath.active.GetTagNames();
		}
		AstarPath astarPath = UnityEngine.Object.FindObjectOfType<AstarPath>();
		if (astarPath != null)
		{
			AstarPath.active = astarPath;
			return astarPath.GetTagNames();
		}
		return new string[]
		{
			"There is no AstarPath component in the scene"
		};
	}

	internal ushort GetNextPathID()
	{
		if (this.nextFreePathID == 0)
		{
			this.nextFreePathID += 1;
			UnityEngine.Debug.Log("65K cleanup (this message is harmless, it just means you have searched a lot of paths)");
			if (AstarPath.On65KOverflow != null)
			{
				Action on65KOverflow = AstarPath.On65KOverflow;
				AstarPath.On65KOverflow = null;
				on65KOverflow();
			}
		}
		ushort result;
		this.nextFreePathID = (result = this.nextFreePathID) + 1;
		return result;
	}

	private void RecalculateDebugLimits()
	{
		this.debugFloor = float.PositiveInfinity;
		this.debugRoof = float.NegativeInfinity;
		for (int i = 0; i < this.graphs.Length; i++)
		{
			if (this.graphs[i] != null && this.graphs[i].drawGizmos)
			{
				this.graphs[i].GetNodes(delegate(GraphNode node)
				{
					if (!this.showSearchTree || this.debugPathData == null || NavGraph.InSearchTree(node, this.debugPath))
					{
						PathNode pathNode = (this.debugPathData == null) ? null : this.debugPathData.GetPathNode(node);
						if (pathNode != null || this.debugMode == GraphDebugMode.Penalty)
						{
							switch (this.debugMode)
							{
							case GraphDebugMode.G:
								this.debugFloor = Mathf.Min(this.debugFloor, pathNode.G);
								this.debugRoof = Mathf.Max(this.debugRoof, pathNode.G);
								break;
							case GraphDebugMode.H:
								this.debugFloor = Mathf.Min(this.debugFloor, pathNode.H);
								this.debugRoof = Mathf.Max(this.debugRoof, pathNode.H);
								break;
							case GraphDebugMode.F:
								this.debugFloor = Mathf.Min(this.debugFloor, pathNode.F);
								this.debugRoof = Mathf.Max(this.debugRoof, pathNode.F);
								break;
							case GraphDebugMode.Penalty:
								this.debugFloor = Mathf.Min(this.debugFloor, node.Penalty);
								this.debugRoof = Mathf.Max(this.debugRoof, node.Penalty);
								break;
							}
						}
					}
					return true;
				});
			}
		}
		if (float.IsInfinity(this.debugFloor))
		{
			this.debugFloor = 0f;
			this.debugRoof = 1f;
		}
		if (this.debugRoof - this.debugFloor < 1f)
		{
			this.debugRoof += 1f;
		}
	}

	private void OnDrawGizmos()
	{
		if (this.isScanning)
		{
			return;
		}
		if (AstarPath.active == null)
		{
			AstarPath.active = this;
		}
		else if (AstarPath.active != this)
		{
			return;
		}
		if (this.graphs == null)
		{
			return;
		}
		if (this.workItems.workItemsInProgress)
		{
			return;
		}
		if (this.showNavGraphs && !this.manualDebugFloorRoof)
		{
			this.RecalculateDebugLimits();
		}
		for (int i = 0; i < this.graphs.Length; i++)
		{
			if (this.graphs[i] != null && this.graphs[i].drawGizmos)
			{
				this.graphs[i].OnDrawGizmos(this.showNavGraphs);
			}
		}
		if (this.showNavGraphs)
		{
			this.euclideanEmbedding.OnDrawGizmos();
			if (this.showUnwalkableNodes)
			{
				Gizmos.color = AstarColor.UnwalkableNode;
				GraphNodeDelegateCancelable del = new GraphNodeDelegateCancelable(this.DrawUnwalkableNode);
				for (int j = 0; j < this.graphs.Length; j++)
				{
					if (this.graphs[j] != null && this.graphs[j].drawGizmos)
					{
						this.graphs[j].GetNodes(del);
					}
				}
			}
		}
		if (this.OnDrawGizmosCallback != null)
		{
			this.OnDrawGizmosCallback();
		}
	}

	private bool DrawUnwalkableNode(GraphNode node)
	{
		if (!node.Walkable)
		{
			Gizmos.DrawCube((Vector3)node.position, Vector3.one * this.unwalkableNodeDebugSize);
		}
		return true;
	}

	internal void Log(string s)
	{
		if (object.ReferenceEquals(AstarPath.active, null))
		{
			UnityEngine.Debug.Log("No AstarPath object was found : " + s);
			return;
		}
		if (AstarPath.active.logPathResults != PathLog.None && AstarPath.active.logPathResults != PathLog.OnlyErrors)
		{
			UnityEngine.Debug.Log(s);
		}
	}

	private void LogPathResults(Path p)
	{
		if (this.logPathResults == PathLog.None || (this.logPathResults == PathLog.OnlyErrors && !p.error))
		{
			return;
		}
		string message = p.DebugString(this.logPathResults);
		if (this.logPathResults == PathLog.InGame)
		{
			this.inGameDebugPath = message;
		}
		else
		{
			UnityEngine.Debug.Log(message);
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (!this.isScanning)
		{
			this.PerformBlockingActions(false, true);
		}
		this.pathProcessor.TickNonMultithreaded();
		this.pathReturnQueue.ReturnPaths(true);
	}

	private void PerformBlockingActions(bool force = false, bool unblockOnComplete = true)
	{
		if (this.pathProcessor.queue.AllReceiversBlocked)
		{
			this.pathReturnQueue.ReturnPaths(false);
			if (AstarPath.OnThreadSafeCallback != null)
			{
				Action onThreadSafeCallback = AstarPath.OnThreadSafeCallback;
				AstarPath.OnThreadSafeCallback = null;
				onThreadSafeCallback();
			}
			if (this.pathProcessor.queue.AllReceiversBlocked && this.workItems.ProcessWorkItems(force))
			{
				this.workItemsQueued = false;
				if (unblockOnComplete)
				{
					if (this.euclideanEmbedding.dirty)
					{
						this.euclideanEmbedding.RecalculateCosts();
					}
					this.pathProcessor.queue.Unblock();
				}
			}
		}
	}

	[Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void QueueWorkItemFloodFill()
	{
		throw new Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	[Obsolete("This method has been moved. Use the method on the context object that can be sent with work item delegates instead")]
	public void EnsureValidFloodFill()
	{
		throw new Exception("This method has been moved. Use the method on the context object that can be sent with work item delegates instead");
	}

	public void AddWorkItem(AstarWorkItem itm)
	{
		this.workItems.AddWorkItem(itm);
		if (!this.workItemsQueued)
		{
			this.workItemsQueued = true;
			if (!this.isScanning)
			{
				this.InterruptPathfinding();
			}
		}
	}

	public void QueueGraphUpdates()
	{
		if (!this.graphUpdatesWorkItemAdded)
		{
			this.graphUpdatesWorkItemAdded = true;
			AstarWorkItem workItem = this.graphUpdates.GetWorkItem();
			this.AddWorkItem(new AstarWorkItem(delegate
			{
				this.graphUpdatesWorkItemAdded = false;
				this.lastGraphUpdate = Time.realtimeSinceStartup;
				this.debugPath = null;
				workItem.init();
			}, workItem.update));
		}
	}

	private IEnumerator DelayedGraphUpdate()
	{
		this.graphUpdateRoutineRunning = true;
		yield return new WaitForSeconds(this.graphUpdateBatchingInterval - (Time.realtimeSinceStartup - this.lastGraphUpdate));
		this.QueueGraphUpdates();
		this.graphUpdateRoutineRunning = false;
		yield break;
	}

	public void UpdateGraphs(Bounds bounds, float t)
	{
		this.UpdateGraphs(new GraphUpdateObject(bounds), t);
	}

	public void UpdateGraphs(GraphUpdateObject ob, float t)
	{
		base.StartCoroutine(this.UpdateGraphsInteral(ob, t));
	}

	private IEnumerator UpdateGraphsInteral(GraphUpdateObject ob, float t)
	{
		yield return new WaitForSeconds(t);
		this.UpdateGraphs(ob);
		yield break;
	}

	public void UpdateGraphs(Bounds bounds)
	{
		this.UpdateGraphs(new GraphUpdateObject(bounds));
	}

	public void UpdateGraphs(GraphUpdateObject ob)
	{
		this.graphUpdates.UpdateGraphs(ob);
		if (this.batchGraphUpdates && Time.realtimeSinceStartup - this.lastGraphUpdate < this.graphUpdateBatchingInterval)
		{
			if (!this.graphUpdateRoutineRunning)
			{
				base.StartCoroutine(this.DelayedGraphUpdate());
			}
		}
		else
		{
			this.QueueGraphUpdates();
		}
	}

	public void FlushGraphUpdates()
	{
		if (this.IsAnyGraphUpdateQueued)
		{
			this.QueueGraphUpdates();
			this.FlushWorkItems();
		}
	}

	public void FlushWorkItems()
	{
		this.FlushWorkItemsInternal(true);
	}

	[Obsolete("Use FlushWorkItems() instead. Use FlushWorkItemsInternal if you really need to")]
	public void FlushWorkItems(bool unblockOnComplete, bool block)
	{
		this.BlockUntilPathQueueBlocked();
		this.PerformBlockingActions(block, unblockOnComplete);
	}

	internal void FlushWorkItemsInternal(bool unblockOnComplete)
	{
		this.BlockUntilPathQueueBlocked();
		this.PerformBlockingActions(true, unblockOnComplete);
	}

	public void FlushThreadSafeCallbacks()
	{
		if (AstarPath.OnThreadSafeCallback != null)
		{
			this.BlockUntilPathQueueBlocked();
			this.PerformBlockingActions(false, true);
		}
	}

	public static int CalculateThreadCount(ThreadCount count)
	{
		if (count != ThreadCount.AutomaticLowLoad && count != ThreadCount.AutomaticHighLoad)
		{
			return (int)count;
		}
		int num = Mathf.Max(1, SystemInfo.processorCount);
		int num2 = SystemInfo.systemMemorySize;
		if (num2 <= 0)
		{
			UnityEngine.Debug.LogError("Machine reporting that is has <= 0 bytes of RAM. This is definitely not true, assuming 1 GiB");
			num2 = 1024;
		}
		if (num <= 1)
		{
			return 0;
		}
		if (num2 <= 512)
		{
			return 0;
		}
		if (count == ThreadCount.AutomaticHighLoad)
		{
			if (num2 <= 1024)
			{
				num = Math.Min(num, 2);
			}
		}
		else
		{
			num /= 2;
			num = Mathf.Max(1, num);
			if (num2 <= 1024)
			{
				num = Math.Min(num, 2);
			}
			num = Math.Min(num, 6);
		}
		return num;
	}

	private void Awake()
	{
		AstarPath.active = this;
		base.useGUILayout = false;
		if (!Application.isPlaying)
		{
			return;
		}
		if (AstarPath.OnAwakeSettings != null)
		{
			AstarPath.OnAwakeSettings();
		}
		this.InitializePathProcessor();
		this.InitializeProfiler();
		this.SetUpReferences();
		this.InitializeAstarData();
		this.FlushWorkItems();
		this.euclideanEmbedding.dirty = true;
		if (this.scanOnStartup && (!this.astarData.cacheStartup || this.astarData.file_cachedStartup == null))
		{
			UnityEngine.Debug.Log("doing scan");
			this.Scan();
		}
	}

	private void InitializePathProcessor()
	{
		int num = AstarPath.CalculateThreadCount(this.threadCount);
		int processors = Mathf.Max(num, 1);
		bool flag = num > 0;
		this.pathProcessor = new PathProcessor(this, this.pathReturnQueue, processors, flag);
		this.pathProcessor.OnPathPreSearch += delegate(Path path)
		{
			OnPathDelegate onPathPreSearch = AstarPath.OnPathPreSearch;
			if (onPathPreSearch != null)
			{
				onPathPreSearch(path);
			}
		};
		this.pathProcessor.OnPathPostSearch += delegate(Path path)
		{
			this.LogPathResults(path);
			OnPathDelegate onPathPostSearch = AstarPath.OnPathPostSearch;
			if (onPathPostSearch != null)
			{
				onPathPostSearch(path);
			}
		};
		if (flag)
		{
			this.graphUpdates.EnableMultithreading();
		}
	}

	internal void VerifyIntegrity()
	{
		if (AstarPath.active != this)
		{
			throw new Exception("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
		}
		if (this.astarData == null)
		{
			throw new NullReferenceException("AstarData is null... Astar not set up correctly?");
		}
		if (this.astarData.graphs == null)
		{
			this.astarData.graphs = new NavGraph[0];
		}
	}

	public void SetUpReferences()
	{
		AstarPath.active = this;
		if (this.astarData == null)
		{
			this.astarData = new AstarData();
		}
		if (this.colorSettings == null)
		{
			this.colorSettings = new AstarColor();
		}
		this.colorSettings.OnEnable();
	}

	private void InitializeProfiler()
	{
	}

	private void InitializeAstarData()
	{
		this.astarData.FindGraphTypes();
		this.astarData.Awake();
		this.astarData.UpdateShortcuts();
		for (int i = 0; i < this.astarData.graphs.Length; i++)
		{
			if (this.astarData.graphs[i] != null)
			{
				this.astarData.graphs[i].Awake();
			}
		}
	}

	private void OnDisable()
	{
		if (this.OnUnloadGizmoMeshes != null)
		{
			this.OnUnloadGizmoMeshes();
		}
	}

	private void OnDestroy()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");
		}
		if (AstarPath.active != this)
		{
			return;
		}
		this.BlockUntilPathQueueBlocked();
		this.euclideanEmbedding.dirty = false;
		this.FlushWorkItemsInternal(false);
		this.pathProcessor.queue.TerminateReceivers();
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Processing Possible Work Items");
		}
		this.graphUpdates.DisableMultithreading();
		this.pathProcessor.JoinThreads();
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Returning Paths");
		}
		this.pathReturnQueue.ReturnPaths(false);
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Destroying Graphs");
		}
		this.astarData.OnDestroy();
		if (this.logPathResults == PathLog.Heavy)
		{
			UnityEngine.Debug.Log("Cleaning up variables");
		}
		this.OnDrawGizmosCallback = null;
		AstarPath.OnAwakeSettings = null;
		AstarPath.OnGraphPreScan = null;
		AstarPath.OnGraphPostScan = null;
		AstarPath.OnPathPreSearch = null;
		AstarPath.OnPathPostSearch = null;
		AstarPath.OnPreScan = null;
		AstarPath.OnPostScan = null;
		AstarPath.OnLatePostScan = null;
		AstarPath.On65KOverflow = null;
		AstarPath.OnGraphsUpdated = null;
		AstarPath.OnThreadSafeCallback = null;
		AstarPath.active = null;
	}

	public void FloodFill(GraphNode seed)
	{
		this.graphUpdates.FloodFill(seed);
	}

	public void FloodFill(GraphNode seed, uint area)
	{
		this.graphUpdates.FloodFill(seed, area);
	}

	[ContextMenu("Flood Fill Graphs")]
	public void FloodFill()
	{
		this.graphUpdates.FloodFill();
		this.workItems.OnFloodFill();
	}

	internal int GetNewNodeIndex()
	{
		return this.pathProcessor.GetNewNodeIndex();
	}

	internal void InitializeNode(GraphNode node)
	{
		this.pathProcessor.InitializeNode(node);
	}

	internal void DestroyNode(GraphNode node)
	{
		this.pathProcessor.DestroyNode(node);
	}

	public void BlockUntilPathQueueBlocked()
	{
		this.pathProcessor.BlockUntilPathQueueBlocked();
	}

	public void Scan()
	{
		foreach (Progress progress in this.ScanAsync())
		{
		}
	}

	[Obsolete("ScanLoop is now named ScanAsync and is an IEnumerable<Progress>. Use foreach to iterate over the progress insead")]
	public void ScanLoop(OnScanStatus statusCallback)
	{
		foreach (Progress progress in this.ScanAsync())
		{
			statusCallback(progress);
		}
	}

	public IEnumerable<Progress> ScanAsync()
	{
		if (this.graphs == null)
		{
			yield break;
		}
		this.isScanning = true;
		this.euclideanEmbedding.dirty = false;
		this.VerifyIntegrity();
		this.BlockUntilPathQueueBlocked();
		this.pathReturnQueue.ReturnPaths(false);
		this.BlockUntilPathQueueBlocked();
		if (!Application.isPlaying)
		{
			GraphModifier.FindAllModifiers();
			RelevantGraphSurface.FindAllGraphSurfaces();
		}
		RelevantGraphSurface.UpdateAllPositions();
		this.astarData.UpdateShortcuts();
		yield return new Progress(0.05f, "Pre processing graphs");
		if (AstarPath.OnPreScan != null)
		{
			AstarPath.OnPreScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PreScan);
		Stopwatch watch = Stopwatch.StartNew();
		for (int j = 0; j < this.graphs.Length; j++)
		{
			if (this.graphs[j] != null)
			{
				this.graphs[j].GetNodes(delegate(GraphNode node)
				{
					node.Destroy();
					return true;
				});
			}
		}
		for (int i = 0; i < this.graphs.Length; i++)
		{
			if (this.graphs[i] != null)
			{
				float minp = Mathf.Lerp(0.1f, 0.8f, (float)i / (float)this.graphs.Length);
				float maxp = Mathf.Lerp(0.1f, 0.8f, ((float)i + 0.95f) / (float)this.graphs.Length);
				string progressDescriptionPrefix = string.Concat(new object[]
				{
					"Scanning graph ",
					i + 1,
					" of ",
					this.graphs.Length,
					" - "
				});
				foreach (Progress progress in this.ScanGraph(this.graphs[i]))
				{
					yield return new Progress(Mathf.Lerp(minp, maxp, progress.progress), progressDescriptionPrefix + progress.description);
				}
			}
		}
		yield return new Progress(0.8f, "Post processing graphs");
		if (AstarPath.OnPostScan != null)
		{
			AstarPath.OnPostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.PostScan);
		try
		{
			this.FlushWorkItemsInternal(false);
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		yield return new Progress(0.9f, "Computing areas");
		this.FloodFill();
		this.VerifyIntegrity();
		yield return new Progress(0.95f, "Late post processing");
		this.isScanning = false;
		if (AstarPath.OnLatePostScan != null)
		{
			AstarPath.OnLatePostScan(this);
		}
		GraphModifier.TriggerEvent(GraphModifier.EventType.LatePostScan);
		this.euclideanEmbedding.dirty = true;
		this.euclideanEmbedding.RecalculatePivots();
		this.PerformBlockingActions(true, true);
		watch.Stop();
		this.lastScanTime = (float)watch.Elapsed.TotalSeconds;
		GC.Collect();
		this.Log("Scanning - Process took " + (this.lastScanTime * 1000f).ToString("0") + " ms to complete");
		yield break;
	}

	private IEnumerable<Progress> ScanGraph(NavGraph graph)
	{
		if (AstarPath.OnGraphPreScan != null)
		{
			yield return new Progress(0f, "Pre processing");
			AstarPath.OnGraphPreScan(graph);
		}
		yield return new Progress(0f, string.Empty);
		foreach (Progress p in graph.ScanInternal())
		{
			yield return new Progress(Mathf.Lerp(0f, 0.95f, p.progress), p.description);
		}
		yield return new Progress(0.95f, "Assigning graph indices");
		graph.GetNodes(delegate(GraphNode node)
		{
			node.GraphIndex = graph.graphIndex;
			return true;
		});
		if (AstarPath.OnGraphPostScan != null)
		{
			yield return new Progress(0.99f, "Post processing");
			AstarPath.OnGraphPostScan(graph);
		}
		yield break;
	}

	public static void WaitForPath(Path p)
	{
		if (AstarPath.active == null)
		{
			throw new Exception("Pathfinding is not correctly initialized in this scene (yet?). AstarPath.active is null.\nDo not call this function in Awake");
		}
		if (p == null)
		{
			throw new ArgumentNullException("Path must not be null");
		}
		if (AstarPath.active.pathProcessor.queue.IsTerminating)
		{
			return;
		}
		if (p.GetState() == PathState.Created)
		{
			throw new Exception("The specified path has not been started yet.");
		}
		AstarPath.waitForPathDepth++;
		if (AstarPath.waitForPathDepth == 5)
		{
			UnityEngine.Debug.LogError("You are calling the WaitForPath function recursively (maybe from a path callback). Please don't do this.");
		}
		if (p.GetState() < PathState.ReturnQueue)
		{
			if (AstarPath.active.IsUsingMultithreading)
			{
				while (p.GetState() < PathState.ReturnQueue)
				{
					if (AstarPath.active.pathProcessor.queue.IsTerminating)
					{
						AstarPath.waitForPathDepth--;
						throw new Exception("Pathfinding Threads seems to have crashed.");
					}
					Thread.Sleep(1);
					AstarPath.active.PerformBlockingActions(false, true);
				}
			}
			else
			{
				while (p.GetState() < PathState.ReturnQueue)
				{
					if (AstarPath.active.pathProcessor.queue.IsEmpty && p.GetState() != PathState.Processing)
					{
						AstarPath.waitForPathDepth--;
						throw new Exception("Critical error. Path Queue is empty but the path state is '" + p.GetState() + "'");
					}
					AstarPath.active.pathProcessor.TickNonMultithreaded();
					AstarPath.active.PerformBlockingActions(false, true);
				}
			}
		}
		AstarPath.active.pathReturnQueue.ReturnPaths(false);
		AstarPath.waitForPathDepth--;
	}

	[Obsolete("The threadSafe parameter has been deprecated")]
	public static void RegisterSafeUpdate(Action callback, bool threadSafe)
	{
		AstarPath.RegisterSafeUpdate(callback);
	}

	public static void RegisterSafeUpdate(Action callback)
	{
		if (callback == null || !Application.isPlaying)
		{
			return;
		}
		if (AstarPath.active.pathProcessor.queue.AllReceiversBlocked)
		{
			AstarPath.active.pathProcessor.queue.Lock();
			try
			{
				if (AstarPath.active.pathProcessor.queue.AllReceiversBlocked)
				{
					callback();
					return;
				}
			}
			finally
			{
				AstarPath.active.pathProcessor.queue.Unlock();
			}
		}
		object obj = AstarPath.safeUpdateLock;
		lock (obj)
		{
			AstarPath.OnThreadSafeCallback = (Action)Delegate.Combine(AstarPath.OnThreadSafeCallback, callback);
		}
		AstarPath.active.pathProcessor.queue.Block();
	}

	private void InterruptPathfinding()
	{
		this.pathProcessor.queue.Block();
	}

	public static void StartPath(Path p, bool pushToFront = false)
	{
		AstarPath astarPath = AstarPath.active;
		if (object.ReferenceEquals(astarPath, null))
		{
			UnityEngine.Debug.LogError("There is no AstarPath object in the scene or it has not been initialized yet");
			return;
		}
		if (p.GetState() != PathState.Created)
		{
			throw new Exception(string.Concat(new object[]
			{
				"The path has an invalid state. Expected ",
				PathState.Created,
				" found ",
				p.GetState(),
				"\nMake sure you are not requesting the same path twice"
			}));
		}
		if (astarPath.pathProcessor.queue.IsTerminating)
		{
			p.Error();
			return;
		}
		if (astarPath.graphs == null || astarPath.graphs.Length == 0)
		{
			UnityEngine.Debug.LogError("There are no graphs in the scene");
			p.Error();
			UnityEngine.Debug.LogError(p.errorLog);
			return;
		}
		p.Claim(astarPath);
		p.AdvanceState(PathState.PathQueue);
		if (pushToFront)
		{
			astarPath.pathProcessor.queue.PushFront(p);
		}
		else
		{
			astarPath.pathProcessor.queue.Push(p);
		}
	}

	private void OnApplicationQuit()
	{
		this.OnDestroy();
		this.pathProcessor.AbortThreads();
	}

	public NNInfo GetNearest(Vector3 position)
	{
		return this.GetNearest(position, NNConstraint.None);
	}

	public NNInfo GetNearest(Vector3 position, NNConstraint constraint)
	{
		return this.GetNearest(position, constraint, null);
	}

	public NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		NavGraph[] graphs = this.graphs;
		float num = float.PositiveInfinity;
		NNInfoInternal internalInfo = default(NNInfoInternal);
		int num2 = -1;
		if (graphs != null)
		{
			for (int i = 0; i < graphs.Length; i++)
			{
				NavGraph navGraph = graphs[i];
				if (navGraph != null && constraint.SuitableGraph(i, navGraph))
				{
					NNInfoInternal nninfoInternal;
					if (this.fullGetNearestSearch)
					{
						nninfoInternal = navGraph.GetNearestForce(position, constraint);
					}
					else
					{
						nninfoInternal = navGraph.GetNearest(position, constraint);
					}
					if (nninfoInternal.node != null)
					{
						float magnitude = (nninfoInternal.clampedPosition - position).magnitude;
						if (this.prioritizeGraphs && magnitude < this.prioritizeGraphsLimit)
						{
							internalInfo = nninfoInternal;
							num2 = i;
							break;
						}
						if (magnitude < num)
						{
							num = magnitude;
							internalInfo = nninfoInternal;
							num2 = i;
						}
					}
				}
			}
		}
		if (num2 == -1)
		{
			return default(NNInfo);
		}
		if (internalInfo.constrainedNode != null)
		{
			internalInfo.node = internalInfo.constrainedNode;
			internalInfo.clampedPosition = internalInfo.constClampedPosition;
		}
		if (!this.fullGetNearestSearch && internalInfo.node != null && !constraint.Suitable(internalInfo.node))
		{
			NNInfoInternal nearestForce = graphs[num2].GetNearestForce(position, constraint);
			if (nearestForce.node != null)
			{
				internalInfo = nearestForce;
			}
		}
		if (!constraint.Suitable(internalInfo.node) || (constraint.constrainDistance && (internalInfo.clampedPosition - position).sqrMagnitude > this.maxNearestNodeDistanceSqr))
		{
			return default(NNInfo);
		}
		return new NNInfo(internalInfo);
	}

	public GraphNode GetNearest(Ray ray)
	{
		if (this.graphs == null)
		{
			return null;
		}
		float minDist = float.PositiveInfinity;
		GraphNode nearestNode = null;
		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;
		for (int i = 0; i < this.graphs.Length; i++)
		{
			NavGraph navGraph = this.graphs[i];
			navGraph.GetNodes(delegate(GraphNode node)
			{
				Vector3 vector = (Vector3)node.position;
				Vector3 a = lineOrigin + Vector3.Dot(vector - lineOrigin, lineDirection) * lineDirection;
				float num = Mathf.Abs(a.x - vector.x);
				num *= num;
				if (num > minDist)
				{
					return true;
				}
				num = Mathf.Abs(a.z - vector.z);
				num *= num;
				if (num > minDist)
				{
					return true;
				}
				float sqrMagnitude = (a - vector).sqrMagnitude;
				if (sqrMagnitude < minDist)
				{
					minDist = sqrMagnitude;
					nearestNode = node;
				}
				return true;
			});
		}
		return nearestNode;
	}

	public static readonly AstarPath.AstarDistribution Distribution = AstarPath.AstarDistribution.WebsiteDownload;

	public static readonly string Branch = "rvo_fix_Pro";

	public AstarData astarData;

	public static AstarPath active;

	public bool showNavGraphs = true;

	public bool showUnwalkableNodes = true;

	public GraphDebugMode debugMode;

	public float debugFloor;

	public float debugRoof = 20000f;

	public bool manualDebugFloorRoof;

	public bool showSearchTree;

	public float unwalkableNodeDebugSize = 0.3f;

	public PathLog logPathResults = PathLog.Normal;

	public float maxNearestNodeDistance = 100f;

	public bool scanOnStartup = true;

	public bool fullGetNearestSearch;

	public bool prioritizeGraphs;

	public float prioritizeGraphsLimit = 1f;

	public AstarColor colorSettings;

	[SerializeField]
	protected string[] tagNames;

	public Heuristic heuristic = Heuristic.Euclidean;

	public float heuristicScale = 1f;

	public ThreadCount threadCount;

	public float maxFrameTime = 1f;

	[Obsolete("Minimum area size is mostly obsolete since the limit has been raised significantly, and the edge cases are handled automatically")]
	public int minAreaSize;

	public bool batchGraphUpdates;

	public float graphUpdateBatchingInterval = 0.2f;

	[NonSerialized]
	public Path debugPath;

	private string inGameDebugPath;

	public static Action OnAwakeSettings;

	public static OnGraphDelegate OnGraphPreScan;

	public static OnGraphDelegate OnGraphPostScan;

	public static OnPathDelegate OnPathPreSearch;

	public static OnPathDelegate OnPathPostSearch;

	public static OnScanDelegate OnPreScan;

	public static OnScanDelegate OnPostScan;

	public static OnScanDelegate OnLatePostScan;

	public static OnScanDelegate OnGraphsUpdated;

	public static Action On65KOverflow;

	private static Action OnThreadSafeCallback;

	public Action OnDrawGizmosCallback;

	public Action OnUnloadGizmoMeshes;

	[Obsolete]
	public Action OnGraphsWillBeUpdated;

	[Obsolete]
	public Action OnGraphsWillBeUpdated2;

	private readonly GraphUpdateProcessor graphUpdates;

	private readonly WorkItemProcessor workItems;

	private PathProcessor pathProcessor;

	private bool graphUpdateRoutineRunning;

	private bool graphUpdatesWorkItemAdded;

	private float lastGraphUpdate = -9999f;

	private bool workItemsQueued;

	private readonly PathReturnQueue pathReturnQueue;

	public EuclideanEmbedding euclideanEmbedding = new EuclideanEmbedding();

	public bool showGraphs;

	private static readonly object safeUpdateLock = new object();

	private ushort nextFreePathID = 1;

	private static int waitForPathDepth = 0;

	public enum AstarDistribution
	{
		WebsiteDownload,
		AssetStore
	}
}
