using System;
using UnityEngine;


public class mutantSimpleOptimize : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.GetComponent<mutantScriptSetup>();
	}

	
	private void doOptimize()
	{
		if (!this.setup.ai.awayFromPlayer && !this.onBool)
		{
			this.onBool = true;
			foreach (GameObject gameObject in this.disableList)
			{
				gameObject.SetActive(true);
			}
		}
		else if (this.setup.ai.awayFromPlayer && this.onBool)
		{
			this.onBool = false;
			foreach (GameObject gameObject2 in this.disableList)
			{
				gameObject2.SetActive(false);
			}
		}
	}

	
	public GameObject[] disableList;

	
	private mutantScriptSetup setup;

	
	private bool onBool;
}
