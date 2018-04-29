using System;
using UnityEngine;


public class enemyNetCollisionFixer : MonoBehaviour
{
	
	private void OnCollisionStay(Collision other)
	{
		if (other.collider.gameObject.CompareTag("enemyRoot"))
		{
			Debug.Log("in collision with " + other.gameObject.name);
			other.gameObject.SendMessage("fixEnemyPosition", base.transform.position, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("enemyRoot"))
		{
			Debug.Log("in trigger with " + other.gameObject.name);
			other.gameObject.SendMessage("fixEnemyPosition", base.transform.position, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.gameObject.CompareTag("enemyRoot"))
		{
			Debug.Log("in character hit with " + hit.gameObject.name);
		}
	}
}
