using System;

namespace Ceto.Common.Threading.Tasks
{
	
	public class TaskListener
	{
		
		public TaskListener(ThreadedTask task)
		{
			this.m_task = task;
		}

		
		
		public ThreadedTask ListeningTask
		{
			get
			{
				return this.m_task;
			}
		}

		
		
		
		public int Waiting
		{
			get
			{
				return this.m_waiting;
			}
			set
			{
				this.m_waiting = value;
			}
		}

		
		public void OnFinish()
		{
			this.m_waiting--;
			if (this.m_waiting == 0 && !this.m_task.Cancelled)
			{
				this.m_task.StopWaiting();
			}
		}

		
		private ThreadedTask m_task;

		
		private volatile int m_waiting;
	}
}
