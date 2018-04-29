using System;
using UnityEngine;


public class playerEpilogueEvents : MonoBehaviour
{
	
	private void Start()
	{
		this.mainCam.fieldOfView = 62f;
	}

	
	private void triggerCurtains()
	{
		this.curtainsAnimator.enabled = true;
	}

	
	private void doCameraGuy()
	{
		this.cameraGuyAnimator.enabled = true;
	}

	
	private void doMomentOfHope()
	{
		this.playerAnimator.Play(this.momentHash, 0, 0f);
		this.timmyAnimator.Play(this.momentHash, 0, 0f);
		this.axe.SetActive(false);
	}

	
	public Animator cameraGuyAnimator;

	
	public Animator curtainsAnimator;

	
	public Camera mainCam;

	
	public Animator playerAnimator;

	
	public Animator timmyAnimator;

	
	public GameObject axe;

	
	private int momentHash = Animator.StringToHash("moment");
}
