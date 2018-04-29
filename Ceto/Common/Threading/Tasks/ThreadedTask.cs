using System;
using System.Collections;
using System.Collections.Generic;
using Ceto.Common.Threading.Scheduling;

namespace Ceto.Common.Threading.Tasks
{
	
	public abstract class ThreadedTask : ICancelToken, IThreadedTask
	{
		
		protected ThreadedTask(bool isThreaded)
		{
			this.m_scheduler = null;
			this.m_isThreaded = isThreaded;
			this.m_listeners = new LinkedList<TaskListener>();
			this.m_listener = new TaskListener(this);
		}

		
		
		
		public float RunTime { get; set; }

		
		
		public bool IsThreaded
		{
			get
			{
				return this.m_isThreaded;
			}
		}

		
		
		public bool Done
		{
			get
			{
				return this.m_done;
			}
		}

		
		
		public bool Ran
		{
			get
			{
				return this.m_ran;
			}
		}

		
		
		
		public bool NoFinish
		{
			get
			{
				return this.m_noFinish;
			}
			set
			{
				this.m_noFinish = value;
			}
		}

		
		
		public bool Waiting
		{
			get
			{
				return this.m_listener.Waiting > 0;
			}
		}

		
		
		
		public bool RunOnStopWaiting
		{
			get
			{
				return this.m_runOnStopWaiting;
			}
			set
			{
				this.m_runOnStopWaiting = value;
			}
		}

		
		
		public bool Started
		{
			get
			{
				return this.m_started;
			}
		}

		
		
		public bool Cancelled
		{
			get
			{
				return this.m_cancelled;
			}
		}

		
		
		public IScheduler Scheduler
		{
			set
			{
				this.m_scheduler = value;
			}
		}

		
		
		protected LinkedList<TaskListener> Listeners
		{
			get
			{
				return this.m_listeners;
			}
		}

		
		
		protected TaskListener Listener
		{
			get
			{
				return this.m_listener;
			}
		}

		
		public virtual void Start()
		{
			this.m_started = true;
		}

		
		public virtual void Reset()
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				this.m_listeners.Clear();
				this.m_listener.Waiting = 0;
				this.m_ran = false;
				this.m_done = false;
				this.m_cancelled = false;
				this.m_started = false;
				this.RunTime = 0f;
			}
		}

		
		public abstract IEnumerator Run();

		
		protected virtual void FinishedRunning()
		{
			this.m_ran = true;
			if (this.m_noFinish)
			{
				this.m_done = true;
			}
			object @lock = this.m_lock;
			lock (@lock)
			{
				if (this.m_noFinish && !this.m_cancelled)
				{
					foreach (TaskListener taskListener in this.m_listeners)
					{
						taskListener.OnFinish();
					}
					this.m_listeners.Clear();
				}
			}
		}

		
		public virtual void End()
		{
			this.m_done = true;
			object @lock = this.m_lock;
			lock (@lock)
			{
				if (!this.m_cancelled)
				{
					foreach (TaskListener taskListener in this.m_listeners)
					{
						taskListener.OnFinish();
					}
				}
				this.m_listeners.Clear();
			}
		}

		
		public virtual void Cancel()
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				this.m_cancelled = true;
				this.m_listeners.Clear();
			}
		}

		
		public virtual void WaitOn(ThreadedTask task)
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				if (task.Cancelled)
				{
					throw new InvalidOperationException("Can not wait on a task that is cancelled");
				}
				if (task.Done)
				{
					throw new InvalidOperationException("Can not wait on a task that is already done");
				}
				if (task.IsThreaded && task.NoFinish && !this.m_isThreaded)
				{
					throw new InvalidOperationException("A non-threaded task cant wait on a threaded task with no finish");
				}
				this.m_listener.Waiting++;
				task.Listeners.AddLast(this.m_listener);
			}
		}

		
		public virtual void StopWaiting()
		{
			object @lock = this.m_lock;
			lock (@lock)
			{
				if (this.m_scheduler != null && !this.m_cancelled)
				{
					this.m_scheduler.StopWaiting(this, this.m_runOnStopWaiting);
				}
			}
		}

		
		public override string ToString()
		{
			return string.Format("[Task: isThreaded={0}, started={1}, ran={2}, done={3}, cancelled={4}]", new object[]
			{
				this.m_isThreaded,
				this.m_started,
				this.m_ran,
				this.m_done,
				this.m_cancelled
			});
		}

		
		private readonly bool m_isThreaded;

		
		private volatile bool m_done;

		
		private volatile bool m_ran;

		
		private volatile bool m_noFinish;

		
		private volatile bool m_runOnStopWaiting;

		
		private volatile bool m_started;

		
		private volatile bool m_cancelled;

		
		protected IScheduler m_scheduler;

		
		private LinkedList<TaskListener> m_listeners;

		
		private TaskListener m_listener;

		
		private readonly object m_lock = new object();
	}
}
