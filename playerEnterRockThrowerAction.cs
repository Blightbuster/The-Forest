using System;
using System.Collections;
using TheForest.Buildings.World;
using TheForest.Player;
using TheForest.Utils;
using UnityEngine;


public class playerEnterRockThrowerAction : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void setCurrentTrigger(GameObject go)
	{
		this.currentTriggerGo = go;
	}

	
	private void setCurrentThrower(GameObject go)
	{
		this.currentThrowerGo = go;
		this.am = go.GetComponentInChildren<rockThrowerAnimEvents>();
	}

	
	private void setCurrentLever(GameObject go)
	{
		this.currentLever = go;
	}

	
	private void enterRockThrower(GameObject go)
	{
		base.StartCoroutine(this.doThrower(go));
	}

	
	private void LateUpdate()
	{
		if (this.currentThrowerGo == null)
		{
			return;
		}
		this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
		if (this.layer0.tagHash == this.enterThrowerHash)
		{
			if (!this.loadCheck)
			{
				if (BoltNetwork.isRunning)
				{
					if (this.ih)
					{
						this.ih.sendAnimVars(0, true);
					}
				}
				else
				{
					this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("load", true);
				}
				this.loadCheck = true;
			}
			Animator component = this.currentThrowerGo.GetComponent<Animator>();
			AnimatorStateInfo currentAnimatorStateInfo = component.GetCurrentAnimatorStateInfo(0);
			component.Play(this.loadHash, 0, this.layer0.normalizedTime);
		}
		else
		{
			this.loadCheck = false;
		}
	}

	
	public IEnumerator doThrower(GameObject posGo)
	{
		this.ih = this.currentThrowerGo.transform.parent.GetComponentInChildren<MultiThrowerItemHolder>();
		if (BoltNetwork.isRunning)
		{
			this.ih.disableTriggerMP();
		}
		else
		{
			this.currentTriggerGo.SetActive(false);
		}
		LocalPlayer.ScriptSetup.pmControl.enabled = false;
		LocalPlayer.Animator.SetBoolReflected("stickBlock", false);
		LocalPlayer.Animator.SetBoolReflected("blockColdBool", true);
		LocalPlayer.Animator.SetFloatReflected("crouch", 0f);
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.Rigidbody.isKinematic = true;
		LocalPlayer.Rigidbody.useGravity = false;
		LocalPlayer.Rigidbody.velocity = Vector3.zero;
		LocalPlayer.CamRotator.rotationRange = new Vector2(70f, 60f);
		LocalPlayer.CamRotator.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.Create.Grabber.gameObject.SetActive(false);
		LocalPlayer.AnimControl.lockGravity = true;
		LocalPlayer.AnimControl.onRockThrower = true;
		LocalPlayer.Inventory.HideAllEquiped(false, false);
		Vector3 standPos = posGo.transform.position;
		standPos.y += LocalPlayer.AnimControl.playerCollider.height / 2f;
		LocalPlayer.Transform.position = posGo.transform.position;
		LocalPlayer.Transform.rotation = posGo.transform.rotation;
		LocalPlayer.Animator.SetBoolReflected("setThrowerBool", true);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
		base.Invoke("resetThrowerParams", 1f);
		yield return YieldPresets.WaitPointTwoSeconds;
		this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
		this.nextlayer0 = LocalPlayer.Animator.GetNextAnimatorStateInfo(0);
		bool waitForLoad = false;
		while (!waitForLoad)
		{
			if (this.currentThrowerGo == null || posGo == null)
			{
				this.exitThrower();
				base.StopCoroutine("doThrower");
				yield break;
			}
			if (this.layer0.tagHash == this.throwerHash)
			{
				waitForLoad = true;
			}
			if (this.layer0.tagHash == this.enterThrowerHash)
			{
				waitForLoad = true;
			}
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			yield return null;
		}
		if (BoltNetwork.isRunning)
		{
			if (this.ih)
			{
				this.ih.sendAnimVars(0, true);
			}
		}
		else if (this.currentThrowerGo)
		{
			this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("load", true);
		}
		while (this.layer0.tagHash != this.throwerHash)
		{
			if (this.currentThrowerGo == null || posGo == null)
			{
				this.exitThrower();
				base.StopCoroutine("doThrower");
				yield break;
			}
			LocalPlayer.Transform.position = posGo.transform.position;
			LocalPlayer.Transform.rotation = posGo.transform.rotation;
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			yield return null;
		}
		this.am.throwPos.gameObject.SetActive(true);
		int currAmmo = this.ih._anim.ammoCount;
		currAmmo = 3 - currAmmo;
		for (int x = 0; x < currAmmo; x++)
		{
			this.loadAmmo();
			yield return YieldPresets.WaitPointOneSeconds;
		}
		while (LocalPlayer.AnimControl.onRockThrower)
		{
			if (this.currentThrowerGo == null || posGo == null)
			{
				this.exitThrower();
				base.StopCoroutine("doThrower");
				yield break;
			}
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			LocalPlayer.Transform.position = posGo.transform.position;
			LocalPlayer.Transform.rotation = posGo.transform.rotation;
			LocalPlayer.Rigidbody.isKinematic = true;
			LocalPlayer.Rigidbody.useGravity = false;
			if (TheForest.Utils.Input.GetButtonDown("Take"))
			{
				this.exitThrower();
				base.StopCoroutine("doThrower");
				yield break;
			}
			if (TheForest.Utils.Input.GetButtonDown("Fire1"))
			{
				if (BoltNetwork.isClient)
				{
					this.ih.sendLandTarget();
				}
				else
				{
					this.am.landTarget = this.am.throwPos.GetComponent<rockThrowerAimingReticle>()._currentLandTarget;
				}
				LocalPlayer.Animator.SetBoolReflected("releaseThrowerBool", true);
				FMODCommon.PlayOneshotNetworked("event:/traps/catapult_release", base.transform, FMODCommon.NetworkRole.Any);
				if (BoltNetwork.isRunning)
				{
					this.ih.sendAnimVars(1, true);
				}
				else
				{
					this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("release", true);
				}
				base.StartCoroutine("offsetLeverRotation");
				yield return YieldPresets.WaitOneSecond;
				this.ih.sendResetAmmoMP();
				LocalPlayer.Animator.SetBoolReflected("releaseThrowerBool", false);
				if (BoltNetwork.isRunning)
				{
					if (this.ih)
					{
						this.ih.sendAnimVars(0, false);
						this.ih.sendAnimVars(1, false);
					}
				}
				else if (this.currentThrowerGo)
				{
					this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("release", false);
					this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("load", false);
				}
				this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
				this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
				while (this.layer0.tagHash != this.throwerHash)
				{
					if (this.currentThrowerGo == null || posGo == null)
					{
						this.exitThrower();
						base.StopCoroutine("doThrower");
						yield break;
					}
					LocalPlayer.Transform.position = posGo.transform.position;
					LocalPlayer.Transform.rotation = posGo.transform.rotation;
					this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
					yield return null;
				}
				currAmmo = this.ih._anim.ammoCount;
				currAmmo = 3 - currAmmo;
				for (int x2 = 0; x2 < currAmmo; x2++)
				{
					this.loadAmmo();
					yield return YieldPresets.WaitPointOneSeconds;
				}
			}
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			if (!this.fixLeverRotation && this.layer0.tagHash == this.throwerHash)
			{
				this.currentLever.transform.localEulerAngles = new Vector3(0f, LocalPlayer.AnimControl.headCamY / 2f, 0f);
			}
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator offsetLeverRotation()
	{
		this.fixLeverRotation = true;
		yield return YieldPresets.WaitPointTwoSeconds;
		float val = this.currentLever.transform.localEulerAngles.y * -1f;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * 6f;
			this.currentLever.transform.localRotation = Quaternion.Lerp(this.currentLever.transform.localRotation, Quaternion.Euler(this.currentLever.transform.localRotation.x, val, this.currentLever.transform.localRotation.z), t);
			yield return null;
		}
		yield return YieldPresets.WaitOneSecond;
		t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / 2f;
			this.currentLever.transform.localRotation = Quaternion.Lerp(this.currentLever.transform.localRotation, Quaternion.Euler(this.currentLever.transform.localRotation.x, 0f, this.currentLever.transform.localRotation.z), t);
			yield return null;
		}
		this.fixLeverRotation = false;
		yield break;
	}

	
	private void exitThrower()
	{
		base.StopCoroutine("offsetLeverRotation");
		this.fixLeverRotation = false;
		if (this.currentLever)
		{
			this.currentLever.transform.localEulerAngles = Vector3.zero;
			this.am.throwPos.gameObject.SetActive(false);
			this.currentLever.transform.localRotation = Quaternion.identity;
		}
		if (BoltNetwork.isRunning)
		{
			if (this.ih)
			{
				this.ih.sendAnimVars(0, false);
			}
		}
		else if (this.currentThrowerGo)
		{
			this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("load", false);
		}
		LocalPlayer.Animator.SetBoolReflected("blockColdBool", false);
		FMODCommon.PlayOneshotNetworked("event:/traps/catapult_unload", base.transform, FMODCommon.NetworkRole.Any);
		if (BoltNetwork.isRunning)
		{
			if (this.ih)
			{
				this.ih.Invoke("enableTriggerMP", 1f);
			}
		}
		else
		{
			base.Invoke("enableTrigger", 1f);
		}
		this.currentThrowerGo = null;
		this.currentLever = null;
		LocalPlayer.ScriptSetup.pmControl.enabled = true;
		LocalPlayer.ScriptSetup.pmControl.SendEvent("toReset2");
		LocalPlayer.Transform.parent = null;
		LocalPlayer.Transform.localScale = new Vector3(1f, 1f, 1f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.Create.Grabber.gameObject.SetActive(true);
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.AnimControl.controller.useGravity = true;
		LocalPlayer.AnimControl.controller.isKinematic = false;
		LocalPlayer.Inventory.ShowAllEquiped(true);
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.AnimControl.onRockThrower = false;
		LocalPlayer.Rigidbody.isKinematic = false;
		LocalPlayer.Rigidbody.useGravity = true;
		LocalPlayer.Animator.SetBoolReflected("setThrowerBool", false);
		LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
	}

	
	private void enableTrigger()
	{
		if (this.currentTriggerGo)
		{
			this.currentTriggerGo.SetActive(true);
		}
	}

	
	private void resetThrowerParams()
	{
	}

	
	private void resetRelease()
	{
		LocalPlayer.Animator.SetBoolReflected("releaseThrowerBool", false);
		if (BoltNetwork.isRunning)
		{
			if (this.ih)
			{
				this.ih.sendAnimVars(1, false);
			}
		}
		else if (this.currentThrowerGo)
		{
			this.currentThrowerGo.GetComponent<Animator>().SetBoolReflected("release", false);
		}
	}

	
	private void loadAmmo()
	{
		this.ih.forceRemoveItem();
	}

	
	private MultiThrowerItemHolder ih;

	
	private rockThrowerAimingReticle ar;

	
	private rockThrowerAnimEvents am;

	
	public GameObject currentTriggerGo;

	
	public GameObject currentThrowerGo;

	
	public GameObject currentLever;

	
	public GameObject aimReticleGo;

	
	private AnimatorStateInfo layer0;

	
	private AnimatorStateInfo nextlayer0;

	
	private int throwerHash = Animator.StringToHash("thrower");

	
	private int enterThrowerHash = Animator.StringToHash("enterThrower");

	
	private int loadHash = Animator.StringToHash("load");

	
	private bool fixLeverRotation;

	
	public string sfxString;

	
	private bool loadCheck;
}
