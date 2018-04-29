using System;
using UnityEngine;


public class BrakeLight : MonoBehaviour
{
	
	private void Update()
	{
		base.GetComponent<Renderer>().enabled = (this.car.BrakeInput > 0f);
	}

	
	public CarController car;
}
