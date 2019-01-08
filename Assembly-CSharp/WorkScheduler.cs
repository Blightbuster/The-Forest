using System;
using System.Diagnostics;
using System.Threading;
using TheForest.Commons.Delegates;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Utils;
using TheForest.Utils.WorkSchedulerInterfaces;
using UnityEngine;

[ExecuteInEditMode]
public class WorkScheduler : MonoBehaviour, IWorkScheduler
{
	private void Awake()
	{
		this.xOffset = base.transform.position.x;
		this.yOffset = base.transform.position.z;
		if (WorkScheduler.Instance == null)
		{
			WorkScheduler.Instance = this;
		}
		else if (WorkScheduler.Instance != this)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		this.schedulers = new WorkSchedulerBatch[this.GridSize, this.GridSize];
		for (int i = 0; i < this.GridSize; i++)
		{
			for (int j = 0; j < this.GridSize; j++)
			{
				this.schedulers[i, j] = new WorkSchedulerBatch();
			}
		}
		this.globalScheduler = new WorkSchedulerBatch();
		if (Application.isPlaying && !this.CaveScene && base.gameObject)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		if (this.mutex == null)
		{
			this.mutex = new ManualResetEvent(false);
		}
		if (this.thread == null)
		{
			this.thread = new Thread(new ThreadStart(this.ThreadedUpdate));
			this.thread.Name = "WorkScheduler";
			this.thread.IsBackground = true;
			this.thread.Start();
		}
		this.secondaryThreadState = WorkScheduler.ThreadStates.Running;
	}

	private void OnDisable()
	{
		this.secondaryThreadState = WorkScheduler.ThreadStates.Paused;
	}

	private void Update()
	{
		if (Application.isPlaying && !WorkScheduler.Paused)
		{
			if (this.secondaryThreadUpdating && this.thread.IsAlive)
			{
				this.mainThreadUpdating = false;
			}
			else
			{
				this.stopwatch.Stop();
				WorkScheduler.CurrentTime = Time.time;
				this.DelayWithPreviousUpdate = Convert.ToSingle(this.stopwatch.ElapsedMilliseconds);
				this.mainThreadUpdating = true;
				if (this.ScaleWithFPS)
				{
					if (float.IsNaN(this.fps))
					{
						this.fps = 0f;
					}
					if (float.IsNaN(this.currentQuality))
					{
						this.currentQuality = 0f;
					}
					this.fps = Mathf.Lerp(this.fps, 1f / Time.smoothDeltaTime, 0.05f);
					float b = Mathf.Clamp(this.TargetFPS / this.fps, 0.75f, 5f);
					this.currentQuality = Mathf.Lerp(this.currentQuality, b, 0.1f);
					this.currentQuality = Mathf.Max(0.33f, this.currentQuality);
				}
				else
				{
					this.currentQuality = 1f;
				}
				float num = this.MaxMilliseconds * this.currentQuality;
				long num2 = (long)(num * (float)(Stopwatch.Frequency / 1000L));
				long num3 = num2;
				this.stopwatch.Reset();
				this.stopwatch.Start();
				this.oneShotScheduler.DoWork(num3, true);
				this.globalScheduler.DoWork(num3 / 20L, false);
				this.stopwatch.Reset();
				this.stopwatch.Start();
				if (Scene.SceneTracker && Scene.SceneTracker.allPlayers != null)
				{
					if (LocalPlayer.Transform)
					{
						this.localPlayerPosition = LocalPlayer.Transform.position;
					}
					if (LocalPlayer.Transform && Vector3.Distance(this.previousPosition, this.localPlayerPosition) > this.GridWorldSize / 2f)
					{
						WorkScheduler.ShouldDoFullCycle = true;
					}
					if (WorkScheduler.FullCycle)
					{
						this.ProcessArea(this.previousPosition, num3, true, false);
					}
					else if (BoltNetwork.isServer)
					{
						for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
						{
							this.ProcessArea(Scene.SceneTracker.allPlayers[i].transform.position, (!(Scene.SceneTracker.allPlayers[i].transform == LocalPlayer.Transform)) ? (num3 / 3L) : num3, false, false);
						}
					}
					else if (LocalPlayer.Transform)
					{
						this.ProcessArea(LocalPlayer.Transform.position, num3, false, false);
					}
					if (LocalPlayer.Transform)
					{
						this.previousPosition = LocalPlayer.Transform.position;
					}
					WorkScheduler.FullCycle = false;
				}
				this.stopwatch.Stop();
				this.mainThreadUpdating = false;
				System.Threading.ThreadState threadState = this.thread.ThreadState;
				if (!this.thread.IsAlive)
				{
					UnityEngine.Debug.Log("Restarting thread: state=" + threadState);
					this.thread = null;
					this.OnEnable();
				}
				this.stopwatch.Reset();
				this.stopwatch.Start();
			}
		}
	}

	private void LateUpdate()
	{
		this.mutex.Set();
	}

	private void OnDestroy()
	{
		this.secondaryThreadState = WorkScheduler.ThreadStates.Closing;
		this.Clear();
	}

	private static void CheckConfig()
	{
		if (WorkScheduler.Instance == null && !WorkScheduler.configWarningIssued && Application.isPlaying)
		{
			UnityEngine.Debug.LogWarning("No Work Scheduler found, please add one. (From the Menu: GameObject/The Forest/Work Scheduler)");
			WorkScheduler.configWarningIssued = true;
			WorkScheduler.AddWorkScheduler();
		}
	}

	public int getTokenForPosition(Vector3 position)
	{
		int num = Mathf.Clamp(this.WorldToGridX(position.x), 0, this.GridSize - 1);
		int num2 = Mathf.Clamp(this.WorldToGridY(position.z), 0, this.GridSize - 1);
		return num * this.GridSize + num2;
	}

	public static bool CheckTokenForPosition(Vector3 position, int token)
	{
		return WorkScheduler.Instance.getTokenForPosition(position) == token;
	}

	public int register(WsTask task, Vector3 position, bool force)
	{
		int num = Mathf.Clamp(this.WorldToGridX(position.x), 0, this.GridSize - 1);
		int num2 = Mathf.Clamp(this.WorldToGridY(position.z), 0, this.GridSize - 1);
		this.schedulers[num, num2].Register(task, force);
		return num * this.GridSize + num2;
	}

	public static int Register(WsTask task, Vector3 position, bool force = false)
	{
		WorkScheduler.CheckConfig();
		return WorkScheduler.Instance.register(task, position, force);
	}

	public int register(IThreadSafeTask tfTask, Vector3 position, bool force)
	{
		int num = Mathf.Clamp(this.WorldToGridX(position.x), 0, this.GridSize - 1);
		int num2 = Mathf.Clamp(this.WorldToGridY(position.z), 0, this.GridSize - 1);
		this.schedulers[num, num2].Register(tfTask, force);
		return num * this.GridSize + num2;
	}

	public static int Register(IThreadSafeTask tfTask, Vector3 position, bool force = false)
	{
		WorkScheduler.CheckConfig();
		return WorkScheduler.Instance.register(tfTask, position, force);
	}

	public void unregister(WsTask task, int token)
	{
		int num = Mathf.Clamp(token / this.GridSize, 0, this.GridSize - 1);
		int num2 = Mathf.Clamp(token - num * this.GridSize, 0, this.GridSize - 1);
		this.schedulers[num, num2].Unregister(task);
	}

	public static void Unregister(WsTask task, int token)
	{
		if (WorkScheduler.Instance != null)
		{
			WorkScheduler.Instance.unregister(task, token);
		}
	}

	public void unregister(IThreadSafeTask tfTask, int token)
	{
		int num = Mathf.Clamp(token / this.GridSize, 0, this.GridSize - 1);
		int num2 = Mathf.Clamp(token - num * this.GridSize, 0, this.GridSize - 1);
		this.schedulers[num, num2].Unregister(tfTask);
	}

	public static void Unregister(IThreadSafeTask tfTask, int token)
	{
		if (WorkScheduler.Instance != null)
		{
			WorkScheduler.Instance.unregister(tfTask, token);
		}
	}

	public void registerGlobal(WsTask task, bool force)
	{
		this.globalScheduler.Register(task, force);
	}

	public static void RegisterGlobal(WsTask task, bool force = false)
	{
		WorkScheduler.CheckConfig();
		WorkScheduler.Instance.registerGlobal(task, force);
	}

	public void unregisterGlobal(WsTask task)
	{
		this.globalScheduler.Unregister(task);
	}

	public static void UnregisterGlobal(WsTask task)
	{
		if (WorkScheduler.Instance != null)
		{
			WorkScheduler.Instance.unregisterGlobal(task);
		}
	}

	public static void ToggleTryCatchWork(bool onoff)
	{
		if (onoff)
		{
			WorkSchedulerBatch[,] array = WorkScheduler.Instance.schedulers;
			int length = array.GetLength(0);
			int length2 = array.GetLength(1);
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					WorkSchedulerBatch workSchedulerBatch = array[i, j];
					workSchedulerBatch.DoWork = new WorkSchedulerBatch.DoWorkDelegate(workSchedulerBatch.DoWorkTryCatch);
				}
			}
		}
		else
		{
			WorkSchedulerBatch[,] array2 = WorkScheduler.Instance.schedulers;
			int length3 = array2.GetLength(0);
			int length4 = array2.GetLength(1);
			for (int k = 0; k < length3; k++)
			{
				for (int l = 0; l < length4; l++)
				{
					WorkSchedulerBatch workSchedulerBatch2 = array2[k, l];
					workSchedulerBatch2.DoWork = new WorkSchedulerBatch.DoWorkDelegate(workSchedulerBatch2.DoWorkNoTry);
				}
			}
		}
	}

	public void registerOneShot(WsTask task)
	{
		this.oneShotScheduler.Register(task, true);
	}

	public static void RegisterOneShot(WsTask task)
	{
		if (WorkScheduler.Instance != null)
		{
			WorkScheduler.Instance.registerOneShot(task);
		}
	}

	public void Toggle(bool onOff)
	{
		if (base.gameObject.activeSelf != onOff)
		{
			base.gameObject.SetActive(onOff);
		}
	}

	public static void ClearInstance()
	{
		WorkScheduler.Instance.Clear();
	}

	public void Clear()
	{
		if (this.schedulers != null)
		{
			for (int i = 0; i < this.GridSize; i++)
			{
				for (int j = 0; j < this.GridSize; j++)
				{
					this.schedulers[i, j].Clear();
				}
			}
		}
		if (this.globalScheduler != null)
		{
			this.globalScheduler.Clear();
		}
		if (this.oneShotScheduler != null)
		{
			this.oneShotScheduler.Clear();
		}
	}

	private void ThreadedUpdate()
	{
		Stopwatch stopwatch = new Stopwatch();
		while (this.secondaryThreadState < WorkScheduler.ThreadStates.Closing)
		{
			this.mutex.WaitOne();
			stopwatch.Reset();
			stopwatch.Start();
			if (this.secondaryThreadState == WorkScheduler.ThreadStates.Running && !this.mainThreadUpdating && !WorkSchedulerBatch.MainThreadUpdating)
			{
				this.currentThreadFrame++;
				this.secondaryThreadUpdating = true;
				float num = this.MaxMilliseconds * this.currentQuality;
				long num2 = (long)(num * (float)(Stopwatch.Frequency / 1000L));
				long num3 = 1000000000L;
				if (WorkScheduler.ShouldDoFullCycle)
				{
					WorkScheduler.ShouldDoFullCycle = false;
					this.ProcessArea(this.previousPosition, num3, true, true);
					this.ThreadedUpdateFullCycleDuration = Convert.ToSingle(stopwatch.ElapsedMilliseconds);
					WorkScheduler.FullCycle = true;
				}
				else if (BoltNetwork.isServer)
				{
					for (int i = 0; i < Scene.SceneTracker.allPlayerPositions.Count; i++)
					{
						this.ProcessArea(Scene.SceneTracker.allPlayerPositions[i], (!(Scene.SceneTracker.allPlayers[i] == LocalPlayer.GameObject)) ? num2 : num3, false, true);
					}
				}
				else if (LocalPlayer.Transform)
				{
					this.ProcessArea(this.localPlayerPosition, num3, false, true);
				}
				this.secondaryThreadUpdating = false;
			}
			stopwatch.Stop();
			this.ThreadedUpdateDuration = Convert.ToSingle(stopwatch.ElapsedMilliseconds);
			this.mutex.Reset();
		}
	}

	private void ProcessArea(Vector3 position, long remainingTicks, bool swipe, bool threadSafeOnly = false)
	{
		if (!threadSafeOnly)
		{
			this.currentFrame = Time.frameCount;
		}
		int num = 0;
		int num2 = 0;
		this.xPlayer = this.WorldToGridXRounded(position.x);
		this.yPlayer = this.WorldToGridYRounded(position.z);
		this.xMin = -2 - Mathf.Min(this.xPlayer - 2, 0);
		this.yMin = -2 - Mathf.Min(this.yPlayer - 2, 0);
		this.xMax = 2 - Mathf.Max(this.xPlayer + 2 - (this.GridSize - 1), 0);
		this.yMax = 2 - Mathf.Max(this.yPlayer + 2 - (this.GridSize - 1), 0);
		long maxTicks = remainingTicks * 3L / 4L / 5L;
		long maxTicks2 = remainingTicks / 4L / 12L;
		if (swipe)
		{
			this.xPlayer = 0;
			this.yPlayer = 0;
			this.xMin = 0;
			this.yMin = 0;
			this.xMax = this.GridSize - 1;
			this.yMax = this.GridSize - 1;
			if (!threadSafeOnly)
			{
				WorkScheduler.FullCycle = true;
			}
		}
		if (!threadSafeOnly)
		{
		}
		for (int i = this.xMin; i <= this.xMax; i++)
		{
			for (int j = this.yMin; j <= this.yMax; j++)
			{
				WorkSchedulerBatch workSchedulerBatch = this.schedulers[this.xPlayer + i, this.yPlayer + j];
				if (i > -2 && i < 1 && j > -2 && j < 1)
				{
					if (!threadSafeOnly)
					{
						if (workSchedulerBatch.LastNearRefreshFrame != this.currentFrame)
						{
							workSchedulerBatch.LastNearRefreshFrame = this.currentFrame;
							num += workSchedulerBatch.DoWork(maxTicks, false);
						}
					}
					else if (workSchedulerBatch.LastThreadRefreshFrame != this.currentThreadFrame)
					{
						workSchedulerBatch.LastThreadRefreshFrame = this.currentThreadFrame;
						workSchedulerBatch.DoThreadSafeWorkTryCatch(false);
					}
				}
				else if (!threadSafeOnly)
				{
					if (workSchedulerBatch.LastNearRefreshFrame != this.currentFrame && workSchedulerBatch.LastFarRefreshFrame != this.currentFrame)
					{
						workSchedulerBatch.LastFarRefreshFrame = this.currentFrame;
						num2 += workSchedulerBatch.DoWork(maxTicks2, false);
					}
				}
				else if (workSchedulerBatch.LastThreadRefreshFrame != this.currentThreadFrame)
				{
					workSchedulerBatch.LastThreadRefreshFrame = this.currentThreadFrame;
					workSchedulerBatch.DoThreadSafeWorkTryCatch(false);
				}
			}
		}
		if (!threadSafeOnly)
		{
		}
	}

	private int WorldToGridX(float xPosition)
	{
		return Mathf.FloorToInt((xPosition - this.xOffset) / this.GridWorldSize);
	}

	private int WorldToGridY(float zPosition)
	{
		return Mathf.FloorToInt((zPosition - this.yOffset) / this.GridWorldSize);
	}

	private int WorldToGridXRounded(float xPosition)
	{
		return Mathf.RoundToInt((xPosition - this.xOffset) / this.GridWorldSize);
	}

	private int WorldToGridYRounded(float zPosition)
	{
		return Mathf.RoundToInt((zPosition - this.yOffset) / this.GridWorldSize);
	}

	private static void AddWorkScheduler()
	{
		if (GameObject.Find("Work Scheduler") == null)
		{
			GameObject gameObject = new GameObject("Work Scheduler");
			gameObject.AddComponent<WorkScheduler>();
		}
		else
		{
			UnityEngine.Debug.Log("Work Scheduler already added. :)");
		}
	}

	public bool CaveScene;

	[Range(0.01f, 2f)]
	public float MaxMilliseconds = 1f;

	public bool ScaleWithFPS;

	[Range(30f, 120f)]
	public float TargetFPS = 30f;

	public int GridSize = 9;

	public float GridWorldSize = 100f;

	[Header("Stats")]
	public float DelayWithPreviousUpdate;

	public float ThreadedUpdateDuration;

	public float ThreadedUpdateFullCycleDuration;

	[Header("Gizmos")]
	public bool ShowGrid = true;

	private WorkSchedulerBatch[,] schedulers;

	private WorkSchedulerBatch globalScheduler;

	private WorkSchedulerBatch oneShotScheduler = new WorkSchedulerBatch();

	private Stopwatch stopwatch = new Stopwatch();

	private float fps = 60f;

	private float currentQuality;

	private float xOffset;

	private float yOffset;

	private int xPlayer;

	private int yPlayer;

	private int xMin;

	private int yMin;

	private int xMax;

	private int yMax;

	private int currentFrame;

	private int currentThreadFrame;

	private Vector3 previousPosition;

	private WorkScheduler.ThreadStates secondaryThreadState;

	private Thread thread;

	private bool mainThreadUpdating;

	private bool secondaryThreadUpdating;

	private Vector3 localPlayerPosition;

	private ManualResetEvent mutex;

	private static WorkScheduler Instance;

	private static bool configWarningIssued;

	public static bool Paused;

	public static bool ShouldDoFullCycle;

	public static bool FullCycle;

	public static float CurrentTime;

	public enum ThreadStates
	{
		Idle,
		Running,
		Paused,
		Closing
	}
}
