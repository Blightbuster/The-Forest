using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheForest.Commons.Delegates;
using TheForest.Utils.WorkSchedulerInterfaces;

public class WorkSchedulerBatch
{
	public WorkSchedulerBatch()
	{
		this.DoWork = new WorkSchedulerBatch.DoWorkDelegate(this.DoWorkTryCatch);
	}

	public int LastThreadRefreshFrame { get; set; }

	public int LastNearRefreshFrame { get; set; }

	public int LastFarRefreshFrame { get; set; }

	public int WorkUnityCount
	{
		get
		{
			return this.tasks.Count;
		}
	}

	public bool Contains(WsTask task)
	{
		return this.tasks.Contains(task);
	}

	public void Register(WsTask task, bool force = false)
	{
		if (force || !this.tasks.Contains(task))
		{
			this.tasks.Add(task);
		}
	}

	public void Register(IThreadSafeTask tfTask, bool force = false)
	{
		WorkSchedulerBatch.MainThreadUpdating = true;
		if (force || !this.tfTasks.Contains(tfTask))
		{
			this.tfTasks.Add(tfTask);
		}
		WorkSchedulerBatch.MainThreadUpdating = false;
	}

	public void Unregister(WsTask task)
	{
		if (this.tasks.Count > 0)
		{
			int num = this.tasks.LastIndexOf(task);
			if (num >= 0)
			{
				this.tasks.RemoveAt(num);
			}
		}
	}

	public void Unregister(IThreadSafeTask tfTask)
	{
		WorkSchedulerBatch.MainThreadUpdating = true;
		if (this.tfTasks.Count > 0)
		{
			int num = this.tfTasks.LastIndexOf(tfTask);
			if (num >= 0)
			{
				this.tfTasks.RemoveAt(num);
			}
		}
		if (tfTask.ShouldDoMainThreadRefresh)
		{
			tfTask.ShouldDoMainThreadRefresh = false;
			if (this.tfTasksChanged.Count > 0)
			{
				int num2 = this.tfTasksChanged.LastIndexOf(tfTask);
				if (num2 >= 0)
				{
					this.tfTasksChanged.RemoveAt(num2);
				}
			}
		}
		WorkSchedulerBatch.MainThreadUpdating = false;
	}

	public int DoWorkTryCatch(long maxTicks, bool autoUnregister = false)
	{
		long num = maxTicks * 3L;
		int count = this.tfTasksChanged.Count;
		int num2 = 0;
		this.stopwatch.Reset();
		this.stopwatch.Start();
		while (num2 < count && (this.stopwatch.ElapsedTicks < num || WorkScheduler.FullCycle))
		{
			if (this.tfTasksChanged.Count == 0)
			{
				break;
			}
			IThreadSafeTask threadSafeTask = this.tfTasksChanged[0];
			try
			{
				threadSafeTask.ShouldDoMainThreadRefresh = false;
				threadSafeTask.MainThreadRefresh();
			}
			catch
			{
			}
			num2++;
			if (autoUnregister)
			{
				this.Unregister(threadSafeTask);
			}
			else
			{
				this.tfTasksChanged.RemoveAt(0);
			}
		}
		count = this.tasks.Count;
		num2 = 0;
		this.stopwatch.Stop();
		this.stopwatch.Reset();
		this.stopwatch.Start();
		while (num2 < count && (this.stopwatch.ElapsedTicks < maxTicks || WorkScheduler.FullCycle))
		{
			int count2 = this.tasks.Count;
			if (this.iterator < 0)
			{
				this.iterator += count2;
			}
			this.iterator = (this.iterator + count2) % count2;
			try
			{
				this.tasks[this.iterator]();
			}
			catch
			{
			}
			num2++;
			if (autoUnregister)
			{
				this.tasks.RemoveAt(this.iterator);
			}
			this.iterator--;
		}
		this.stopwatch.Stop();
		return num2;
	}

	public int DoWorkNoTry(long maxTicks, bool autoUnregister = false)
	{
		int count = this.tfTasksChanged.Count;
		long num = maxTicks * (long)(3 + count / 10);
		int num2 = 0;
		this.stopwatch.Stop();
		this.stopwatch.Reset();
		this.stopwatch.Start();
		while (num2 < count && (this.stopwatch.ElapsedTicks < num || WorkScheduler.FullCycle))
		{
			if (this.tfTasksChanged.Count == 0)
			{
				break;
			}
			IThreadSafeTask threadSafeTask = this.tfTasksChanged[0];
			try
			{
				threadSafeTask.ShouldDoMainThreadRefresh = false;
				threadSafeTask.MainThreadRefresh();
			}
			catch
			{
			}
			num2++;
			if (autoUnregister)
			{
				this.Unregister(threadSafeTask);
			}
			else
			{
				this.tfTasksChanged.Remove(threadSafeTask);
			}
		}
		count = this.tasks.Count;
		num2 = 0;
		this.stopwatch.Stop();
		this.stopwatch.Reset();
		this.stopwatch.Start();
		while (num2 < count && (this.stopwatch.ElapsedTicks < maxTicks || WorkScheduler.FullCycle))
		{
			int count2 = this.tasks.Count;
			if (this.iterator < 0)
			{
				this.iterator += count2;
			}
			this.iterator = (this.iterator + count2) % count2;
			this.tasks[this.iterator]();
			num2++;
			if (autoUnregister)
			{
				this.tasks.RemoveAt(this.iterator);
			}
			this.iterator--;
		}
		this.stopwatch.Stop();
		return num2;
	}

	public void DoThreadSafeWorkTryCatch(bool autoUnregister = false)
	{
		int i = this.tfTasks.Count - 1;
		while (i >= 0)
		{
			IThreadSafeTask threadSafeTask = this.tfTasks[i];
			try
			{
				bool shouldDoMainThreadRefresh = threadSafeTask.ShouldDoMainThreadRefresh;
				threadSafeTask.ThreadedRefresh();
				if (shouldDoMainThreadRefresh != threadSafeTask.ShouldDoMainThreadRefresh)
				{
					if (shouldDoMainThreadRefresh)
					{
						this.tfTasksChanged.Remove(threadSafeTask);
					}
					else
					{
						this.tfTasksChanged.Add(threadSafeTask);
					}
				}
			}
			catch
			{
			}
			i--;
			if (autoUnregister)
			{
				this.tfTasks.RemoveAt(this.iterator);
			}
		}
	}

	public void Clear()
	{
		this.tasks.Clear();
		this.tfTasks.Clear();
		this.tfTasksChanged.Clear();
	}

	public static bool MainThreadUpdating;

	private List<WsTask> tasks = new List<WsTask>();

	private List<IThreadSafeTask> tfTasks = new List<IThreadSafeTask>();

	private List<IThreadSafeTask> tfTasksChanged = new List<IThreadSafeTask>(50);

	private int iterator;

	private Stopwatch stopwatch = new Stopwatch();

	public WorkSchedulerBatch.DoWorkDelegate DoWork;

	public delegate int DoWorkDelegate(long maxTicks, bool autoUnregister = false);
}
