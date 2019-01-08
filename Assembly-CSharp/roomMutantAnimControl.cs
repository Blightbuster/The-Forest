using System;
using UnityEngine;

public class roomMutantAnimControl : MonoBehaviour
{
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Weapon"))
		{
			this.animator.SetBool("balloon", true);
			base.Invoke("resetBalloon", 2f);
		}
	}

	private void resetBalloon()
	{
		this.animator.SetBool("balloon", false);
	}

	private Animator animator;
}
