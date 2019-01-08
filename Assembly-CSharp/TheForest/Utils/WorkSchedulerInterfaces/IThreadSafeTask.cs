using System;

namespace TheForest.Utils.WorkSchedulerInterfaces
{
	public interface IThreadSafeTask
	{
		bool ShouldDoMainThreadRefresh { get; set; }

		void ThreadedRefresh();

		void MainThreadRefresh();
	}
}
