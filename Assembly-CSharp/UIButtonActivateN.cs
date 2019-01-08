using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Activate N")]
public class UIButtonActivateN : MonoBehaviour
{
	private void OnClick()
	{
		foreach (UIButtonActivateN.Target target in this._targets)
		{
			if (target.target != null)
			{
				NGUITools.SetActive(target.target, target.state);
			}
		}
	}

	public UIButtonActivateN.Target[] _targets;

	[Serializable]
	public class Target
	{
		public GameObject target;

		public bool state = true;
	}
}
