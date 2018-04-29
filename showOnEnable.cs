using System;
using UnityEngine;


public class showOnEnable : MonoBehaviour
{
	
	private void Awake()
	{
		if (this.go && this.allowedAction <= showOnEnable.Actions.EnabledOnly && this.go.activeSelf != this.invert)
		{
			this.go.SetActive(this.invert);
		}
	}

	
	private void OnEnable()
	{
		if (this.go && this.allowedAction <= showOnEnable.Actions.EnabledOnly && this.go.activeSelf != !this.invert)
		{
			this.go.SetActive(!this.invert);
		}
	}

	
	private void OnDisable()
	{
		if (this.go && this.allowedAction != showOnEnable.Actions.EnabledOnly && this.go.activeSelf != this.invert)
		{
			this.go.SetActive(this.invert);
		}
	}

	
	public showOnEnable.Actions allowedAction;

	
	public bool invert;

	
	public GameObject go;

	
	public enum Actions
	{
		
		All,
		
		EnabledOnly,
		
		DisabledOnly
	}
}
