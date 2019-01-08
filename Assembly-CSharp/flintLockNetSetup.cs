using System;
using UnityEngine;

public class flintLockNetSetup : MonoBehaviour
{
	private void Start()
	{
		this.netAnimator = base.transform.root.GetComponentInChildren<Animator>();
	}

	private void OnEnable()
	{
		this.doParticles = false;
	}

	private void Update()
	{
		this.nextPlayerState1 = this.netAnimator.GetNextAnimatorStateInfo(1);
		if (this.nextPlayerState1.shortNameHash == this.playerShootHash && !this.doParticles)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.smokeSpawn, this.smokePos.position, this.smokePos.rotation);
			this.doParticles = true;
			base.Invoke("resetParticles", 2f);
		}
	}

	private void resetParticles()
	{
		this.doParticles = false;
	}

	private Animator netAnimator;

	private AnimatorStateInfo nextPlayerState1;

	private int playerShootHash = Animator.StringToHash("shootFlintLock");

	public GameObject smokeSpawn;

	public Transform smokePos;

	private bool doParticles;
}
