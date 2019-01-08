using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

public class playerEnterCaveAction : MonoBehaviour
{
	private void Start()
	{
	}

	private void setLightingSwitch(bool b)
	{
		this.ignoreLighting = b;
	}

	private void setGoodbyeTimmyState(bool t)
	{
		this.timmyCutscene = t;
	}

	private void setIgnoreOcean(bool s)
	{
		this.ignoreOceanTriggersOnEntry = s;
	}

	private void enterCave(GameObject go)
	{
		base.StartCoroutine(this.doCave(go, true));
	}

	private void exitCave(GameObject go)
	{
		base.StartCoroutine(this.doCave(go, false));
	}

	public IEnumerator doCave(GameObject posGo, bool enter)
	{
		LocalPlayer.AnimControl.enteringACave = true;
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
		LocalPlayer.Animator.SetBoolReflected("stickBlock", false);
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(0f, 0f);
		LocalPlayer.CamRotator.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.AnimControl.playerCollider.isTrigger = true;
		LocalPlayer.AnimControl.playerHeadCollider.isTrigger = true;
		LocalPlayer.Rigidbody.velocity = Vector3.zero;
		LocalPlayer.vrPlayerControl.useGhostMode = true;
		LocalPlayer.vrPlayerControl.gameObject.SendMessage("useSteppedGhostMode");
		if (!ForestVR.Enabled)
		{
			if (this.swimCave)
			{
				LocalPlayer.CamFollowHead.smoothLock = true;
			}
			LocalPlayer.CamFollowHead.followAnim = true;
		}
		LocalPlayer.AnimControl.lockGravity = true;
		LocalPlayer.FpCharacter.disableToggledCrouch();
		if (!this.swimCave)
		{
			LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
			LocalPlayer.Inventory.StashEquipedWeapon(false);
		}
		if (this.swimCave && this.ignoreOceanTriggersOnEntry)
		{
			LocalPlayer.Buoyancy.ignoreOceanTriggers = enter;
		}
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.AnimControl.onRope = true;
		Vector3 standPos = posGo.transform.position;
		standPos.y += LocalPlayer.AnimControl.playerCollider.height / 2f;
		LocalPlayer.Transform.parent = posGo.transform;
		LocalPlayer.Transform.localPosition = new Vector3(0f, 0.01945f, 0f);
		LocalPlayer.Transform.localEulerAngles = Vector3.zero;
		if (this.swimCave)
		{
			LocalPlayer.Animator.SetBool("swimmingBool", true);
		}
		if (enter)
		{
			LocalPlayer.Animator.SetIntegerReflected("enterCaveInt", 1);
		}
		else
		{
			LocalPlayer.Animator.SetIntegerReflected("enterCaveInt", 2);
		}
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		if (this.swimCave)
		{
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
		}
		else
		{
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		}
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		if (this.swimCave)
		{
			base.StartCoroutine(LocalPlayer.AnimControl.smoothDisableLayerNew(4));
		}
		else
		{
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		}
		if (this.timmyCutscene)
		{
			LocalPlayer.Animator.SetBool("goodbyeTimmy", true);
		}
		base.Invoke("resetCaveParams", 2.4f);
		if (this.swimCave)
		{
			float timer = Time.time + 2.5f;
			while (Time.time < timer)
			{
				LocalPlayer.Animator.SetBool("swimmingBool", true);
				yield return null;
			}
		}
		else
		{
			yield return YieldPresets.WaitOneSecond;
		}
		Scene.HudGui.LoadingCavesInfo.SetActive(true);
		Scene.HudGui.LoadingCavesFill.fillAmount = 0f;
		yield return YieldPresets.WaitPointFiveSeconds;
		if (!this.ignoreLighting)
		{
			LocalPlayer.GameObject.SendMessage((!enter) ? "NotInACave" : "InACave", SendMessageOptions.DontRequireReceiver);
		}
		bool hasLog = false;
		bool hasLog2 = false;
		this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo layer2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
		do
		{
			yield return null;
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			layer2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
		}
		while (this.layer0.tagHash != this.enterCaveHash && layer2.tagHash != this.enterCaveHash);
		while (this.layer0.tagHash == this.enterCaveHash || layer2.tagHash == this.enterCaveHash)
		{
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.Rigidbody.isKinematic = true;
			LocalPlayer.AnimControl.useRootMotion = true;
			LocalPlayer.AnimControl.onRope = true;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.Transform.rotation = posGo.transform.rotation;
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			this.layer0 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0);
			layer2 = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			if (LocalPlayer.Inventory.Logs.HasLogs)
			{
				if (LocalPlayer.ScriptSetup.heldLog1.activeSelf)
				{
					hasLog = true;
					LocalPlayer.ScriptSetup.heldLog1.SetActive(false);
				}
				if (LocalPlayer.ScriptSetup.heldLog2.activeSelf)
				{
					hasLog2 = true;
					LocalPlayer.ScriptSetup.heldLog2.SetActive(false);
				}
			}
			if (Scene.HudGui.LoadingCavesFill.fillAmount < 1f)
			{
				Scene.HudGui.LoadingCavesFill.fillAmount = Mathf.Max(0.1f, (this.layer0.normalizedTime - 0.25f) / 0.45f);
				if (Scene.HudGui.LoadingCavesFill.fillAmount >= 1f)
				{
					Scene.HudGui.LoadingCavesInfo.SetActive(false);
					Scene.HudGui.Grid.repositionNow = true;
				}
			}
			if (this.timmyCutscene)
			{
				LocalPlayer.AnimControl.enteringACave = false;
				LocalPlayer.SpecialActions.SendMessage("goodbyeTimmyRoutine");
				yield break;
			}
			yield return null;
		}
		Scene.HudGui.LoadingCavesInfo.SetActive(false);
		Scene.HudGui.Grid.repositionNow = true;
		if (!this.swimCave || this.ignoreOceanTriggersOnEntry)
		{
		}
		if (!this.swimCave)
		{
			if (hasLog)
			{
				LocalPlayer.ScriptSetup.heldLog1.SetActive(true);
			}
			if (hasLog2)
			{
				LocalPlayer.ScriptSetup.heldLog2.SetActive(true);
			}
		}
		LocalPlayer.AnimControl.playerCollider.isTrigger = false;
		LocalPlayer.AnimControl.playerHeadCollider.isTrigger = false;
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		if (!this.swimCave)
		{
			LocalPlayer.ScriptSetup.pmControl.SendEvent("toReset2");
		}
		LocalPlayer.Transform.parent = Scene.SceneTracker.transform;
		LocalPlayer.Transform.parent = null;
		LocalPlayer.Transform.localScale = new Vector3(1f, 1f, 1f);
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.FpCharacter.enabled = true;
		if (this.swimCave)
		{
			LocalPlayer.MainRotator.resetOriginalRotation = true;
		}
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.AnimControl.controller.useGravity = true;
		LocalPlayer.AnimControl.controller.isKinematic = false;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.AnimControl.enteringACave = false;
		LocalPlayer.vrPlayerControl.useGhostMode = false;
		if (!this.swimCave)
		{
			LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		}
		yield break;
	}

	private void resetCaveParams()
	{
		LocalPlayer.Animator.SetIntegerReflected("enterCaveInt", 0);
	}

	private void setSwimCave(bool onoff)
	{
		this.swimCave = onoff;
	}

	private AnimatorStateInfo layer0;

	private int enterCaveHash = Animator.StringToHash("enterCave");

	private int enterSwimCaveHash = Animator.StringToHash("enterSwimCave");

	private int exitSwimCaveHash = Animator.StringToHash("exitSwimCave");

	private bool ignoreLighting;

	private bool swimCave;

	private bool timmyCutscene;

	private bool ignoreOceanTriggersOnEntry;
}
