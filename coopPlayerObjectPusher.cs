using System;
using UnityEngine;


public class coopPlayerObjectPusher : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void OnTriggerEnter(Collider other)
	{
		Rigidbody component = other.GetComponent<Rigidbody>();
		if (component)
		{
			Vector3 position = base.transform.position;
			position.y = other.transform.position.y;
			bool flag = false;
			if (other.GetComponent<CoopLogStopper>())
			{
				component.isKinematic = false;
				flag = true;
			}
			if (other.GetComponent<enableNitrogenRelay>() || flag)
			{
				component.AddExplosionForce(this.force, position, 10f);
				Debug.Log("adding force to " + other.gameObject.name);
			}
		}
	}

	
	public float force = 4000f;
}
