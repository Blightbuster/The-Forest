using System;
using TheForest.Tools;
using UnityEngine;


public class StarLocationTrigger : MonoBehaviour
{
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			EventRegistry.Player.Publish(TfEvent.FoundStarLocation, this.MyInt);
		}
	}

	
	public int MyInt;
}
