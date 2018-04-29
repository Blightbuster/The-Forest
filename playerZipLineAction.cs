using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class playerZipLineAction : MonoBehaviour
{
	
	private void EnterZipLine(Transform trn)
	{
		this._currentZipLine = trn;
		this._onZipLine = true;
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		LocalPlayer.Animator.SetBoolReflected("zipLineAttach", true);
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = false;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(78f, 105f);
		LocalPlayer.Rigidbody.useGravity = false;
		LocalPlayer.Inventory.enabled = false;
		Vector3 center = trn.GetComponent<BoxCollider>().bounds.center;
		center.y = LocalPlayer.Transform.position.y;
		LocalPlayer.GameObject.transform.position = center;
		LocalPlayer.GameObject.transform.rotation = trn.rotation;
		LocalPlayer.GameObject.transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
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
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		base.StartCoroutine("StickToZipLine", trn);
	}

	
	private IEnumerator StickToZipLine(Transform trn)
	{
		Vector3 fixLocalPos = new Vector3(0f, -2.344841f, 0f);
		LocalPlayer.Rigidbody.velocity = Vector3.zero;
		this._onRopeAttachPos = trn.GetComponent<BoxCollider>().bounds.center;
		this._onRopeAttachPos.y = this._onRopeAttachPos.y - 2.5f;
		LocalPlayer.GameObject.transform.position = this._onRopeAttachPos;
		while (LocalPlayer.AnimControl.currLayerState0.shortNameHash != this._zipIdleHash)
		{
			if (LocalPlayer.AnimControl.currLayerState0.shortNameHash == this._idleToZipHash && LocalPlayer.AnimControl.currLayerState0.normalizedTime > 0.39f)
			{
				break;
			}
			LocalPlayer.Rigidbody.velocity = Vector3.zero;
			LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
			LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			yield return null;
		}
		this._planeAxeHeld.SetActive(true);
		LocalPlayer.CamFollowHead.StartCoroutine("startZipLineShake");
		float disConnectTimer = Time.time + 1.3f;
		for (;;)
		{
			this._planeAxeHeld.SetActive(true);
			if (trn == null)
			{
				break;
			}
			if (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != this._idleToZipHash && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash != this._zipIdleHash)
			{
				goto IL_422;
			}
			if (!this._fixPlayerPosition)
			{
				LocalPlayer.GameObject.transform.position = this._onRopeAttachPos;
				this._fixPlayerPosition = true;
				LocalPlayer.Rigidbody.velocity = Vector3.zero;
			}
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			LocalPlayer.FpCharacter.capsule.height = 3.3f;
			LocalPlayer.FpCharacter.capsule.center = new Vector3(0f, 0.25f, 0f);
			LocalPlayer.Rigidbody.AddForce(trn.forward * 10f, ForceMode.Acceleration);
			if (Time.time > disConnectTimer && this.CheckLowVelocity())
			{
				goto Block_7;
			}
			Vector3 fixPos = trn.InverseTransformPoint(LocalPlayer.Transform.position);
			fixPos.x = 0f;
			fixPos.y = -2.5f;
			LocalPlayer.Transform.position = trn.TransformPoint(fixPos);
			LocalPlayer.PlayerBase.transform.localPosition = fixLocalPos;
			if (LocalPlayer.Rigidbody.velocity.magnitude > 50f)
			{
				Vector3 newVelocity = LocalPlayer.Rigidbody.velocity.normalized;
				newVelocity *= 50f;
				LocalPlayer.Rigidbody.velocity = newVelocity;
			}
			LocalPlayer.Sfx.SetZiplineLoop(true);
			if (this.CheckCloseToGround())
			{
				goto Block_9;
			}
			if (TheForest.Utils.Input.GetButtonDown("Take") || TheForest.Utils.Input.GetButtonDown("Jump"))
			{
				goto IL_450;
			}
			yield return new WaitForFixedUpdate();
		}
		this.ExitZipLine();
		yield break;
		Block_7:
		this.ExitZipLine();
		yield break;
		Block_9:
		this.ExitZipLine();
		yield break;
		IL_422:
		this.ExitZipLine();
		yield break;
		IL_450:
		this.ExitZipLine();
		yield break;
		yield break;
	}

	
	public void ExitZipLine()
	{
		if (LocalPlayer.Rigidbody.velocity.magnitude > 10f)
		{
			base.StartCoroutine(this.PreserveExitVelocity((!this._currentZipLine) ? LocalPlayer.Transform.forward : this._currentZipLine.forward));
		}
		this._currentZipLine = null;
		this._onZipLine = false;
		LocalPlayer.Sfx.SetZiplineLoop(false);
		LocalPlayer.Sfx.PlayZiplineExit();
		LocalPlayer.FpCharacter.StopCoroutine("startJumpTimer");
		LocalPlayer.FpCharacter.StartCoroutine("startJumpTimer");
		base.StopCoroutine("StickToZipLine");
		this._fixPlayerPosition = false;
		this._planeAxeHeld.SetActive(false);
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		LocalPlayer.Animator.SetBoolReflected("zipLineAttach", false);
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
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
		LocalPlayer.FpCharacter.capsule.height = LocalPlayer.FpCharacter.originalHeight;
		LocalPlayer.FpCharacter.capsule.center = new Vector3(0f, 0f, 0f);
		if (LocalPlayer.Inventory.IsRightHandEmpty())
		{
			LocalPlayer.Inventory.EquipPreviousWeaponDelayed();
		}
	}

	
	private bool CheckCloseToGround()
	{
		RaycastHit raycastHit;
		return Physics.Raycast(LocalPlayer.Transform.position, Vector3.down, out raycastHit, 10f, this._collideMask) && (raycastHit.distance < 1.5f && !raycastHit.collider.isTrigger);
	}

	
	private bool CheckLowVelocity()
	{
		float num = (LocalPlayer.Transform.position - this.prevPos).magnitude * 10f;
		this.prevPos = LocalPlayer.Transform.position;
		return num < 0.8f;
	}

	
	private IEnumerator PreserveExitVelocity(Vector3 dir)
	{
		this._forceDir = dir;
		float t = 0f;
		float force = 1f;
		LocalPlayer.FpCharacter._doingExitVelocity = true;
		while (t < 1f)
		{
			force = Mathf.Lerp(force, 0f, t);
			this._forceDir *= force;
			t += Time.deltaTime;
			yield return new WaitForFixedUpdate();
		}
		LocalPlayer.FpCharacter._doingExitVelocity = false;
		yield break;
	}

	
	public GameObject _planeAxeHeld;

	
	public LayerMask _collideMask;

	
	public Transform _currentZipLine;

	
	private Vector3 _onRopeAttachPos;

	
	private float _onRopeOffset;

	
	private bool _fixPlayerPosition;

	
	public Vector3 _forceDir;

	
	public bool _onZipLine;

	
	private Vector3 prevPos;

	
	private int _idleToZipHash = Animator.StringToHash("idleToZip");

	
	private int _zipIdleHash = Animator.StringToHash("zipIdle");
}
