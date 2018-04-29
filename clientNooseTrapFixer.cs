using System;
using UnityEngine;


public class clientNooseTrapFixer : MonoBehaviour
{
	
	private void Start()
	{
		if (!BoltNetwork.isClient)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (BoltNetwork.isClient && other.gameObject.CompareTag("enemyRoot"))
		{
			other.SendMessage("setClientTrigger", base.gameObject, SendMessageOptions.DontRequireReceiver);
			other.SendMessage("setClientNoosePivot", this.nooseFootPivot, SendMessageOptions.DontRequireReceiver);
		}
		if (other.gameObject.CompareTag("dummyExplode"))
		{
			other.transform.parent.SendMessage("setClientDummyInNooseTrap", this.nooseFootPivot, SendMessageOptions.DontRequireReceiver);
		}
	}

	
	public Transform nooseFootPivot;

	
	public GameObject sprungRopeGo;

	
	public GameObject looseRopeGo;

	
	public GameObject sprungNooseJoint;

	
	private Transform dummyInTrap;

	
	private Animator dummyAnimator;
}
