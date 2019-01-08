using System;
using UnityEngine;

public class Tutorial5 : MonoBehaviour
{
	public void SetDurationToCurrentProgress()
	{
		UITweener[] componentsInChildren = base.GetComponentsInChildren<UITweener>();
		foreach (UITweener uitweener in componentsInChildren)
		{
			uitweener.duration = Mathf.Lerp(2f, 0.5f, UIProgressBar.current.value);
		}
	}
}
