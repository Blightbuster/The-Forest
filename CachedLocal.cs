using System;
using UnityEngine;


internal struct CachedLocal<T> where T : Component
{
	
	public CachedLocal(GameObject gameObject)
	{
		this.go = gameObject;
		this.component = (T)((object)null);
	}

	
	
	public T Component
	{
		get
		{
			if (!this.go)
			{
				return (T)((object)null);
			}
			if (this.component)
			{
				return this.component;
			}
			this.component = this.go.GetComponentInChildren<T>();
			if (!this.component)
			{
			}
			return this.component;
		}
	}

	
	private T component;

	
	private GameObject go;
}
