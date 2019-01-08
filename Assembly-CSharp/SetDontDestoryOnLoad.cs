using System;
using UnityEngine;

public class SetDontDestoryOnLoad : MonoBehaviour
{
	private void Start()
	{
		if (this.UseStart)
		{
			this.ApplyDontDestoryOnLoad();
		}
	}

	private void Awake()
	{
		if (this.UseAwake)
		{
			this.ApplyDontDestoryOnLoad();
		}
	}

	private void OnEnable()
	{
		if (this.UseEnable)
		{
			this.ApplyDontDestoryOnLoad();
		}
	}

	private void ApplyDontDestoryOnLoad()
	{
		if (base.gameObject.transform.parent != null)
		{
			base.gameObject.transform.parent = null;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public bool UseAwake;

	public bool UseStart;

	public bool UseEnable = true;
}
