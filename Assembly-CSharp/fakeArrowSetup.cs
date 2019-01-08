using System;
using Bolt;
using UnityEngine;

public class fakeArrowSetup : MonoBehaviour
{
	private void sendRemoveArrow()
	{
		if (BoltNetwork.isRunning)
		{
			if (this.entityTarget == null)
			{
				this.entityTarget = base.transform.root.GetComponent<BoltEntity>();
			}
			if (this.entityTarget)
			{
				stuckArrowsSync stuckArrowsSync = stuckArrowsSync.Create(GlobalTargets.Others);
				stuckArrowsSync.target = this.entityTarget;
				stuckArrowsSync.removeArrow = true;
				stuckArrowsSync.index = this.storedIndex;
				stuckArrowsSync.Send();
			}
		}
	}

	public Transform tip;

	public BoltEntity entityTarget;

	public int storedIndex;
}
