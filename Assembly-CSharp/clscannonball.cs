using System;
using UnityEngine;

public class clscannonball : MonoBehaviour
{
	private void OnCollisionEnter()
	{
		if (this.vargamenabled && this.varcannon != null)
		{
			this.varcannon.metresetactor();
		}
		this.vargamenabled = false;
	}

	public bool vargamenabled = true;

	public clscannon varcannon;
}
