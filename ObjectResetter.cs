using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectResetter : MonoBehaviour
{
	
	private void Start()
	{
		this.originalStructure = new List<Transform>(base.GetComponentsInChildren<Transform>());
		this.originalPosition = base.transform.position;
		this.originalRotation = base.transform.rotation;
	}

	
	public void DelayedReset(float delay)
	{
		base.StartCoroutine(this.ResetCoroutine(delay));
	}

	
	private IEnumerator ResetCoroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		foreach (Transform t in base.GetComponentsInChildren<Transform>())
		{
			if (!this.originalStructure.Contains(t))
			{
				t.parent = null;
			}
		}
		base.transform.position = this.originalPosition;
		base.transform.rotation = this.originalRotation;
		if (base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().velocity = Vector3.zero;
			base.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
		base.SendMessage("Reset");
		yield break;
	}

	
	private Vector3 originalPosition;

	
	private Quaternion originalRotation;

	
	private List<Transform> originalStructure;
}
