using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

public class playerCraneClimbAction : MonoBehaviour
{
	private void Start()
	{
		this._speedDamp = 0.7f;
		this._attachOffset = 0.73f;
	}

	private void EnterCraneClimb(Transform trn)
	{
		this._onCrane = true;
		this._triggerTr = trn;
		this._input = 0.1f;
		if (!LocalPlayer.Inventory.IsRightHandEmpty())
		{
			if (!LocalPlayer.Inventory.RightHand.IsHeldOnly)
			{
				LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
			}
			LocalPlayer.Inventory.StashEquipedWeapon(false);
		}
		else if (LocalPlayer.Inventory.Logs.HasLogs)
		{
			LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
			if (LocalPlayer.Inventory.Logs.HasLogs)
			{
				LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
			}
		}
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = true;
		LocalPlayer.Inventory.enabled = false;
		LocalPlayer.Rigidbody.velocity = Vector3.zero;
		LocalPlayer.CamRotator.rotationRange = new Vector2(78f, 105f);
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		Vector3 position = LocalPlayer.Transform.position;
		Vector3 vector = trn.position;
		vector.y = LocalPlayer.Transform.position.y;
		vector = (LocalPlayer.Transform.position - vector).normalized * this._attachOffset;
		Vector3 vector2 = vector;
		vector += trn.position;
		LocalPlayer.Transform.position = vector + trn.position;
		LocalPlayer.Transform.position = new Vector3(LocalPlayer.Transform.position.x, position.y, LocalPlayer.Transform.position.z);
		LocalPlayer.Transform.rotation = Quaternion.LookRotation(-vector2, Vector3.up);
		LocalPlayer.Animator.SetBool("craneAttach", true);
		base.StartCoroutine(this.StickToCrane(vector, vector2));
	}

	private void ExitCraneClimb()
	{
		this._onCrane = false;
		base.StopCoroutine("StickToCrane");
		LocalPlayer.Animator.SetFloatReflected("pullCraneSpeed", 0.1f);
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.Animator.SetBool("craneAttach", false);
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.Rigidbody.useGravity = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.Inventory.enabled = true;
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		if (LocalPlayer.Inventory.IsRightHandEmpty())
		{
			LocalPlayer.Inventory.EquipPreviousWeapon(false);
		}
		this._triggerTr = null;
	}

	private IEnumerator RefreshPlayerCollisionRoutine()
	{
		LocalPlayer.AnimControl.playerCollider.enabled = false;
		yield return YieldPresets.WaitForFixedUpdate;
		LocalPlayer.AnimControl.playerCollider.enabled = true;
		yield break;
	}

	private IEnumerator StickToCrane(Vector3 pos, Vector3 dir)
	{
		while (this._onCrane)
		{
			if (this._triggerTr == null)
			{
				this.ExitCraneClimb();
				yield break;
			}
			LocalPlayer.Rigidbody.velocity = Vector3.zero;
			LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			this._input = Mathf.Lerp(this._input, TheForest.Utils.Input.GetAxis("Vertical"), Time.deltaTime * 15f);
			LocalPlayer.Animator.SetFloatReflected("pullCraneSpeed", this._input * this._speedDamp);
			LocalPlayer.Transform.position = new Vector3(pos.x, LocalPlayer.Transform.position.y, pos.z);
			LocalPlayer.Transform.rotation = Quaternion.LookRotation(-dir, Vector3.up);
			yield return null;
		}
		yield break;
	}

	private void ForceCraneReset()
	{
		if (this._triggerTr)
		{
			this._triggerTr.SendMessage("UnlockPlayer");
		}
	}

	private float _speedDamp = 0.7f;

	private float _attachOffset = 0.73f;

	private Transform _triggerTr;

	private int _pullCraneLoopHash = Animator.StringToHash("pullCraneLoop");

	private bool _onCrane;

	private float _input;
}
