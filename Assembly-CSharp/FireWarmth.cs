using System;
using TheForest.Utils;
using UnityEngine;

public class FireWarmth : MonoBehaviour
{
	private void OnEnable()
	{
		Collider component = base.GetComponent<Collider>();
		if (component)
		{
			UnityEngine.Object.Destroy(component);
		}
		if (!base.IsInvoking("PlayerWarmthCheck"))
		{
			base.InvokeRepeating("PlayerWarmthCheck", 2f, 2f);
		}
	}

	private void OnDisable()
	{
		base.CancelInvoke("PlayerWarmthCheck");
		if (LocalPlayer.Stats)
		{
			LocalPlayer.Stats.LeaveHeat();
		}
		this.inHeat = false;
	}

	private void PlayerWarmthCheck()
	{
		if (!LocalPlayer.Transform)
		{
			return;
		}
		float sqrMagnitude = (LocalPlayer.Transform.position - base.transform.position).sqrMagnitude;
		if (sqrMagnitude < this.WarmthTriggerDistance)
		{
			this.inHeat = true;
			LocalPlayer.Stats.Heat();
		}
		else if (this.inHeat)
		{
			LocalPlayer.Stats.LeaveHeat();
			this.inHeat = false;
		}
	}

	private void OnDestroy()
	{
		base.CancelInvoke("PlayerWarmthCheck");
		if (LocalPlayer.Stats)
		{
			LocalPlayer.Stats.LeaveHeat();
		}
	}

	private float WarmthTriggerDistance = 64f;

	private bool inHeat;
}
