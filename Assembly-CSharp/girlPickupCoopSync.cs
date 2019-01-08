using System;
using System.Collections;
using UnityEngine;

public class girlPickupCoopSync : MonoBehaviour
{
	private void Start()
	{
		this.animator = base.transform.GetComponent<Animator>();
	}

	public void setGirlToMachine()
	{
		this.animator.enabled = true;
		this.animator.CrossFade("Base Layer.girlToMachine", 0f, 0, 0f);
	}

	public void setGirlPickupAnimation(Transform localPlayer)
	{
		this.animator.enabled = true;
		if (localPlayer)
		{
			base.StartCoroutine(this.syncPickupAnimation(localPlayer));
		}
	}

	public void setGirlPutDownAnimation(Transform localPlayer)
	{
		if (!this.animator)
		{
			this.animator = base.transform.GetComponent<Animator>();
		}
		this.animator.enabled = true;
		this.animator.CrossFade("Base Layer.putDownGirl", 0f, 0, 0f);
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			renderer.enabled = false;
		}
		if (localPlayer)
		{
			base.StartCoroutine(this.syncPutDownAnimation(localPlayer));
		}
	}

	private IEnumerator syncPickupAnimation(Transform player)
	{
		this.playerAnimator = player.GetComponentInChildren<Animator>();
		if (!this.playerAnimator)
		{
			yield break;
		}
		AnimatorStateInfo playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
		this.rootTr.position = this.playerAnimator.transform.position;
		this.rootTr.rotation = this.playerAnimator.transform.rotation;
		yield return YieldPresets.WaitPointTwoFiveSeconds;
		float timer = Time.time + 2f;
		while (playerState.shortNameHash != this.idleToGirlPickupHash)
		{
			if (Time.time > timer)
			{
				yield break;
			}
			if (player == null || this.playerAnimator == null || !this.playerAnimator.enabled)
			{
				yield break;
			}
			playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
			yield return null;
		}
		while (playerState.shortNameHash == this.idleToGirlPickupHash)
		{
			yield return null;
			if (player == null || this.playerAnimator == null || !this.playerAnimator.enabled)
			{
				yield break;
			}
			playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
			this.animator.Play(this.girlPickupHash, 0, playerState.normalizedTime);
		}
		this.animator.CrossFade(this.girlPickupHash, 0f, 0, 1f);
		yield return null;
		this.animator.enabled = false;
		yield break;
	}

	private IEnumerator syncPutDownAnimation(Transform player)
	{
		this.playerAnimator = player.GetComponentInChildren<Animator>();
		if (!this.playerAnimator)
		{
			yield break;
		}
		AnimatorStateInfo playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
		this.rootTr.position = this.playerAnimator.transform.position;
		this.rootTr.rotation = this.playerAnimator.transform.rotation;
		float timer = Time.time + 2f;
		while (playerState.shortNameHash != this.putDownGirlHash)
		{
			if (Time.time > timer)
			{
				yield break;
			}
			if (player == null || this.playerAnimator == null || !this.playerAnimator.enabled)
			{
				yield break;
			}
			playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
			yield return null;
		}
		Renderer[] ren = base.transform.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer renderer in ren)
		{
			renderer.enabled = true;
		}
		playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
		this.animator.Play(this.putDownGirlHash, 0, playerState.normalizedTime);
		while (playerState.shortNameHash == this.putDownGirlHash)
		{
			yield return null;
			if (player == null || this.playerAnimator == null || !this.playerAnimator.enabled)
			{
				yield break;
			}
			playerState = this.playerAnimator.GetCurrentAnimatorStateInfo(2);
			this.animator.Play(this.putDownGirlHash, 0, playerState.normalizedTime);
		}
		this.animator.CrossFade(this.putDownGirlHash, 0f, 0, 1f);
		yield return null;
		this.animator.enabled = false;
		yield break;
	}

	public void disablePickupTrigger()
	{
		if (this.triggerGo)
		{
			this.triggerGo.SetActive(false);
		}
	}

	public void enablePickupTrigger()
	{
		if (this.triggerGo)
		{
			this.triggerGo.SetActive(true);
			this.triggerGo.SendMessage("resetPickup", SendMessageOptions.DontRequireReceiver);
			this.playerAnimator = null;
			base.StartCoroutine("resetGirlAnimation");
		}
	}

	private IEnumerator resetGirlAnimation()
	{
		base.StopCoroutine("syncPickupAnimation");
		this.animator.CrossFade("Base Layer.girlPickup", 0f, 0, 0f);
		yield return YieldPresets.WaitForEndOfFrame;
		this.animator.enabled = false;
		yield break;
	}

	public Transform rootTr;

	private Animator animator;

	public GameObject triggerGo;

	private Animator playerAnimator;

	private int girlPickupHash = Animator.StringToHash("girlPickup");

	private int idleToGirlPickupHash = Animator.StringToHash("idleToGirlPickup");

	private int putDownGirlHash = Animator.StringToHash("putDownGirl");
}
