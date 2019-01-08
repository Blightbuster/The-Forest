﻿using System;
using UnityEngine;

[AddComponentMenu("NGUI/Widget/Event/Switch Activate")]
public class ext_SwitchActivateEvent : MonoBehaviour
{
	private void OnClick()
	{
		if (this.target != null)
		{
			NGUITools.SetActive(this.target, base.transform.parent.gameObject.GetComponent<ext_Switch>().state);
		}
	}

	public GameObject target;
}
