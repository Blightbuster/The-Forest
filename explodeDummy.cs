using System;
using UnityEngine;


public class explodeDummy : MonoBehaviour
{
	
	public void Explosion(float dist)
	{
		if (!BoltNetwork.isClient)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.explodedGo, base.transform.position, base.transform.rotation);
			UnityEngine.Object.Destroy(this.removeGo);
		}
	}

	
	public GameObject explodedGo;

	
	public GameObject removeGo;
}
