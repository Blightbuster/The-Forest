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
		if (base.entity && !base.entity.isAttached)
		{
			BoltNetwork.Attach(base.entity);
		}
		yield break;
	}

	public bool destroyIfClient;
}
