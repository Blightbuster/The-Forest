using System;
using System.Collections;
using UnityEngine;

public class CustomActiveValueGreebleGOList : CustomActiveValueGreeble
{
	private void OnDisable()
	{
		if (base.Data != null && !base.gameObject.activeSelf)
		{
			byte b = 0;
			for (int i = 0; i < this._targets.Length; i++)
			{
				GameObject gameObject = this._targets[i];
				if (!gameObject)
				{
					b |= (byte)(1 << i);
				}
				else if (!gameObject.activeSelf)
				{
					b |= (byte)(1 << i);
					gameObject.SetActive(true);
				}
			}
			base.Data._instancesState[base.Index] = b;
			base.Data = null;
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.OnEnableRoutine());
	}

	private IEnumerator OnEnableRoutine()
	{
		yield return null;
		if (base.Data != null)
		{
			this.InitFromData();
		}
		yield break;
	}

	private void OnDestroy()
	{
		if (base.Data != null)
		{
			base.Data._instancesState[base.Index] = byte.MaxValue;
			base.Data = null;
		}
	}

	private void SetLodBase(LOD_Base lodbase)
	{
		CustomActiveValueGreeble component = lodbase.GetComponent<CustomActiveValueGreeble>();
		if (component.Data != null && component.Index < component.Data._instancesState.Length)
		{
			base.Data = component.Data;
			base.Index = component.Index;
			this.InitFromData();
		}
	}

	private void InitFromData()
	{
		bool flag = base.Data._instancesState[base.Index] < 252;
		for (int i = 0; i < this._targets.Length; i++)
		{
			bool flag2 = !flag || ((int)base.Data._instancesState[base.Index] & 1 << i) == 0;
			if (this._targets[i] && flag2 != this._targets[i].activeSelf)
			{
				this._targets[i].SetActive(flag2);
			}
		}
	}

	public GameObject[] _targets;
}
