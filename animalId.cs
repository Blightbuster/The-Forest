using System;
using UnityEngine;


public class animalId : MonoBehaviour
{
	
	private void Start()
	{
		if (this.setTagToAnimalCollide)
		{
			base.gameObject.tag = "animalCollide";
			base.gameObject.layer = 9;
		}
	}

	
	public bool setTagToAnimalCollide;

	
	public bool isCroc;

	
	public bool isTurtle;
}
