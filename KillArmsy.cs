using System;
using UnityEngine;


public class KillArmsy : MonoBehaviour
{
	
	private void Explosion(float dist)
	{
		UnityEngine.Object.Instantiate<GameObject>(this.RagDoll, base.transform.parent.position, base.transform.parent.rotation);
		UnityEngine.Object.Destroy(base.transform.parent.gameObject);
	}

	
	public GameObject RagDoll;
}
