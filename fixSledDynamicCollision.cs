using System;
using UnityEngine;


public class fixSledDynamicCollision : MonoBehaviour
{
	
	private void OnEnable()
	{
		this.fixSledCollision();
	}

	
	private void fixSledCollision()
	{
		Collider component = Terrain.activeTerrain.GetComponent<Collider>();
		if (component && component.enabled && component.gameObject.activeSelf)
		{
			Physics.IgnoreCollision(base.transform.GetComponent<Collider>(), component, true);
		}
	}
}
