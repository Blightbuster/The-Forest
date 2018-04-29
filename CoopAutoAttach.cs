using System;
using System.Collections;
using Bolt;


public class CoopAutoAttach : EntityBehaviour
{
	
	private void OnEnable()
	{
		if (BoltNetwork.isRunning && !base.transform.root.CompareTag("CaveProps"))
		{
			base.StartCoroutine(this.DelayedOnEnable());
		}
	}

	
	private IEnumerator DelayedOnEnable()
	{
		yield return null;
		if (this.entity && !this.entity.isAttached)
		{
			BoltNetwork.Attach(this.entity);
		}
		yield break;
	}

	
	public bool destroyIfClient;
}
