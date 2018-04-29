using System;
using UnityEngine;


public class FishMovements : MonoBehaviour
{
	
	private void Start()
	{
		this.startingPoint = base.transform.position;
		this.rndShift = this.startingPoint + this.maxDist * UnityEngine.Random.insideUnitSphere;
		this.rndShift.y = this.startingPoint.y;
	}

	
	private void Update()
	{
		if (Vector3.Distance(base.transform.position, this.rndShift) > 0.1f)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, this.rndShift, Time.deltaTime);
			Quaternion to = Quaternion.LookRotation(this.rndShift - base.transform.position);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, to, Time.deltaTime);
		}
		else
		{
			this.rndShift = this.startingPoint + this.maxDist * UnityEngine.Random.insideUnitSphere;
			this.rndShift.y = this.startingPoint.y;
		}
	}

	
	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("Collision");
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			this.rndShift = this.maxDist * contactPoint.normal;
			this.rndShift.y = this.startingPoint.y;
		}
	}

	
	public Vector3 startingPoint;

	
	public float maxDist;

	
	public Vector3 rndShift;
}
