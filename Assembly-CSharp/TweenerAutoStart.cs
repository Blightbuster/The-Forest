using System;
using UnityEngine;

public class TweenerAutoStart : MonoBehaviour
{
	public void OnEnable()
	{
		if (this.Targets == null)
		{
			return;
		}
		foreach (UITweener uitweener in this.Targets)
		{
			uitweener.enabled = false;
			uitweener.ResetToBeginning();
			uitweener.enabled = true;
		}
	}

	public UITweener[] Targets;
}
