using System;
using UnityEngine;


public class Manifest : MonoBehaviour
{
	
	private void CrossOff()
	{
		this.PassengersFound++;
		this.Mark[this.PassengersFound - 1].SetActive(true);
	}

	
	public GameObject[] Mark;

	
	private int PassengersFound;
}
