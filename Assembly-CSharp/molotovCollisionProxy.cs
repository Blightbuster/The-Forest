using System;
using UnityEngine;

public class molotovCollisionProxy : MonoBehaviour
{
	private void Start()
	{
		this.mol = base.transform.GetComponentInChildren<Molotov>();
	}

	private void OnCollisionEnter(Collision col)
	{
		if (!this.mol)
		{
			this.mol = base.transform.GetComponentInChildren<Molotov>();
		}
		if (this.mol)
		{
			this.mol.OnCollisionEnter(col);
		}
	}

	private Molotov mol;
}
