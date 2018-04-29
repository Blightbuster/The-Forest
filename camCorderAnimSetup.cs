using System;
using TheForest.Utils;
using UnityEngine;


public class camCorderAnimSetup : MonoBehaviour
{
	
	private void Start()
	{
		this._animator = base.transform.GetComponent<Animator>();
		if (!this._net)
		{
			this._playerAnimator = LocalPlayer.Animator;
		}
	}

	
	private void OnEnable()
	{
		if (!this._animator)
		{
			this._animator = base.transform.GetComponent<Animator>();
		}
		if (!this._playerAnimator && !this._net)
		{
			this._playerAnimator = LocalPlayer.Animator;
		}
		if (this._playerAnimator.GetCurrentAnimatorStateInfo(1).shortNameHash == this._idleHash)
		{
			this._animator.CrossFade("Base Layer.camCorderIdle", 0f, 0, this._playerAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime);
		}
		else if (this._playerAnimator.GetCurrentAnimatorStateInfo(1).shortNameHash == this._toIdle0Hash)
		{
			this._animator.CrossFade("Base Layer.toIdle", 0f, 0, this._playerAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime);
		}
		else if (this._playerAnimator.GetCurrentAnimatorStateInfo(1).shortNameHash != this._idleHash && this._playerAnimator.GetCurrentAnimatorStateInfo(1).shortNameHash != this._toIdle0Hash)
		{
			this._animator.CrossFade("Base Layer.toCamCorderIdle", 0f, 0, 0f);
			this._animator.SetBool("closeCamCorder", false);
		}
	}

	
	private void Update()
	{
		AnimatorStateInfo currentAnimatorStateInfo = this._playerAnimator.GetCurrentAnimatorStateInfo(1);
		if (this._playerAnimator.GetNextAnimatorStateInfo(1).shortNameHash == this._toIdle0Hash || this._playerAnimator.GetCurrentAnimatorStateInfo(1).shortNameHash == this._toIdle0Hash)
		{
			this._animator.SetBool("closeCamCorder", true);
		}
		else if (this._playerAnimator.GetCurrentAnimatorStateInfo(1).shortNameHash == this._toIdleHash)
		{
			this._animator.SetBool("closeCamCorder", false);
			this._animator.Play(this._toIdleHash, 0, this._playerAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime);
		}
		if (currentAnimatorStateInfo.shortNameHash == this._toIdle0Hash)
		{
			this._animator.Play("Base Layer.toIdle", 0, currentAnimatorStateInfo.normalizedTime);
		}
	}

	
	public bool _net;

	
	private Animator _animator;

	
	public Animator _playerAnimator;

	
	private int _toIdle0Hash = Animator.StringToHash("toCamCorderIdle 0");

	
	private int _toIdleHash = Animator.StringToHash("toCamCorderIdle");

	
	private int _idleHash = Animator.StringToHash("camCorderIdle");
}
