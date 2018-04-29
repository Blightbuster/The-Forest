using System;
using TheForest.Buildings.Interfaces;
using UnityEngine;


public class WaterClean : MonoBehaviour
{
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player") || other.GetComponent(typeof(IWetable)))
		{
			other.SendMessage("GotClean");
		}
	}

	
	private void Burn(MasterFireSpread fire)
	{
		if (this.PutOutFires)
		{
			fire.PutOut();
		}
	}

	
	public bool PutOutFires;
}
