using System;
using UnityEngine;

public class net_bookController : MonoBehaviour
{
	private void Start()
	{
		if (this.bookHeld)
		{
			this.bookAnim = this.bookHeld.GetComponent<Animator>();
		}
	}

	private void Update()
	{
		if (!this.bookHeld)
		{
			return;
		}
		this.currState1 = this.animator.GetCurrentAnimatorStateInfo(1);
		this.nextState1 = this.animator.GetNextAnimatorStateInfo(1);
		if (this.currState1.shortNameHash == this.idleToBookHash || this.currState1.shortNameHash == this.bookIdleHash || this.currState1.shortNameHash == this.bookToIdleHash)
		{
			if (this.currState1.shortNameHash == this.bookToIdleHash || this.nextState1.shortNameHash == this.bookToIdleHash)
			{
				this.bookAnim.SetBool("bookHeld", false);
				this.bookOpen = true;
			}
			else if (!this.bookOpen)
			{
				this.bookHeld.SetActive(true);
				this.bookAnim.CrossFade("Base Layer.toBookIdle", 0f, 0, 0f);
				this.bookAnim.SetBool("bookHeld", true);
				this.bookOpen = true;
			}
		}
		else
		{
			this.bookHeld.SetActive(false);
			this.bookOpen = false;
		}
	}

	public Animator animator;

	public GameObject bookHeld;

	private Animator bookAnim;

	private AnimatorStateInfo currState1;

	private AnimatorStateInfo nextState1;

	private bool bookOpen;

	private int idleToBookHash = Animator.StringToHash("idleToBookIdle");

	private int bookIdleHash = Animator.StringToHash("bookIdle");

	private int bookToIdleHash = Animator.StringToHash("bookIdleToIdle");
}
