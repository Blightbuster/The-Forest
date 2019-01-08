using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class PlayerHangGliderAction : MonoBehaviour
{
	private void Start()
	{
		this.smoothGliderOffset = this.startGliderOffset;
		base.enabled = false;
	}

	private void OnEnable()
	{
		this.wasFlying = false;
		this.startFlyingDelay = 0f;
	}

	private void Update()
	{
		if (!LocalPlayer.AnimControl.holdingGlider)
		{
			return;
		}
		if ((TheForest.Utils.Input.GetButtonDown("Drop") && !ForestVR.Enabled) || (TheForest.Utils.Input.GetButtonDown("AltFire") && ForestVR.Enabled) || LocalPlayer.AnimControl.onRope || LocalPlayer.AnimControl.onRaft || LocalPlayer.AnimControl.useRootMotion || LocalPlayer.AnimControl.enteringACave || LocalPlayer.AnimControl.doSledPushMode || LocalPlayer.AnimControl.sitting || LocalPlayer.AnimControl.onRockThrower || LocalPlayer.Animator.GetBool("craneAttach") || MenuMain.exiting || MenuMain.exitingToMenu || this.isOutsideWorldBounds())
		{
			this.StopFlyingGlider();
			this.DropGlider(false);
		}
		if (LocalPlayer.Inventory.Logs.HasLogs)
		{
			LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
			if (LocalPlayer.Inventory.Logs.HasLogs)
			{
				LocalPlayer.Inventory.Logs.PutDown(false, true, false, null);
			}
		}
		if (!LocalPlayer.FpCharacter.Grounded && LocalPlayer.AnimControl.holdingGlider)
		{
			this.startFlyingDelay += Time.deltaTime;
			if (this.startFlyingDelay > 0.9f)
			{
				this.FlyWithGlider();
				if (LocalPlayer.AnimControl.swimming)
				{
					this.StopFlyingGlider();
					this.DropGlider(false);
				}
				this.wasFlying = true;
			}
		}
		else if (this.wasFlying)
		{
			this.wasFlying = false;
			this.StopFlyingGlider();
			this.startFlyingDelay = 0f;
		}
		else
		{
			this.startFlyingDelay = 0f;
		}
		if (ForestVR.Enabled)
		{
			if (LocalPlayer.AnimControl.holdingGlider)
			{
				this.gliderHeld.SetActive(LocalPlayer.CurrentView != PlayerInventory.PlayerViews.Inventory);
			}
			Vector3 zero = Vector3.zero;
			if (LocalPlayer.AnimControl.flyingGlider)
			{
				zero.z = 35f;
			}
			this.smoothGliderOffset = Vector3.Lerp(this.smoothGliderOffset, zero, Time.deltaTime);
			this.gliderHeld.transform.localEulerAngles = this.smoothGliderOffset;
		}
	}

	private void pickupGlider()
	{
		base.enabled = true;
		LocalPlayer.Inventory.MemorizeItem(Item.EquipmentSlot.RightHand);
		LocalPlayer.Inventory.StashEquipedWeapon(false);
		LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.RightHand);
		LocalPlayer.Animator.SetBool("hangGliderHeld", true);
		LocalPlayer.AnimControl.holdingGlider = true;
		this.gliderHeld.SetActive(true);
		if (ForestVR.Enabled)
		{
			this.gliderHeld.transform.parent = this.VRGliderTr;
			this.gliderHeld.transform.localPosition = Vector3.zero;
			this.gliderHeld.transform.localEulerAngles = Vector3.zero;
		}
	}

	private void DropGlider(bool ignoreCurrentVelocity = false)
	{
		if (!LocalPlayer.AnimControl.holdingGlider)
		{
			return;
		}
		if (LocalPlayer.AnimControl.flyingGlider)
		{
			this.StopFlyingGlider();
		}
		LocalPlayer.Animator.SetBool("hangGliderHeld", false);
		LocalPlayer.Animator.SetBool("hangGliderFlying", false);
		LocalPlayer.AnimControl.holdingGlider = false;
		LocalPlayer.AnimControl.flyingGlider = false;
		this.gliderHeld.SetActive(false);
		LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.RightHand);
		LocalPlayer.Inventory.EquipPreviousWeaponDelayed();
		Vector3 vector = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * 3f;
		if (!LocalPlayer.FpCharacter.Grounded)
		{
			vector = this.gliderHeld.transform.position;
		}
		if (ignoreCurrentVelocity)
		{
			vector = LocalPlayer.Transform.position + Vector3.up * 3f;
		}
		if (BoltNetwork.isClient)
		{
			PickupGlider pickupGlider = PickupGlider.Create(GlobalTargets.OnlyServer);
			pickupGlider.dropGlider = true;
			pickupGlider.targetEntity = this.gliderSpawnPickup.GetComponent<BoltEntity>();
			pickupGlider.gliderId = this.gliderSpawnPickup.GetComponent<BoltEntity>().prefabId;
			pickupGlider.dropPos = vector;
			pickupGlider.dropRot = this.gliderHeld.transform.rotation;
			pickupGlider.targetVelocity = LocalPlayer.Rigidbody.velocity * 100f;
			pickupGlider.Send();
		}
		else
		{
			this.SpawnDroppedGlider(vector, ignoreCurrentVelocity);
		}
		base.enabled = false;
	}

	private void FlyWithGlider()
	{
		LocalPlayer.AnimControl.playerCollider.radius = 1f;
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		LocalPlayer.AnimControl.flyingGlider = true;
		LocalPlayer.AnimControl.StartCoroutine(LocalPlayer.AnimControl.smoothEnableLayerNew(2));
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.rotationRange = new Vector2(155f, 90f);
		LocalPlayer.Animator.SetBool("hangGliderFlying", true);
		LocalPlayer.Animator.SetBoolReflected("clampSpine", true);
		base.CancelInvoke("enableFallDamage");
		LocalPlayer.Sfx.SetHangGliderLoop(true);
		LocalPlayer.AnimControl.disconnectFromGlider = true;
	}

	private void StopFlyingGlider()
	{
		if (!LocalPlayer.AnimControl.flyingGlider)
		{
			return;
		}
		LocalPlayer.AnimControl.playerCollider.radius = LocalPlayer.AnimControl.playerColliderRadius;
		LocalPlayer.AnimControl.StartCoroutine(LocalPlayer.AnimControl.smoothDisableLayerNew(2));
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.AnimControl.flyingGlider = false;
		LocalPlayer.Animator.SetBool("hangGliderFlying", false);
		LocalPlayer.Animator.SetBoolReflected("clampSpine", false);
		LocalPlayer.Sfx.SetHangGliderLoop(false);
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		base.Invoke("enableFallDamage", 0.25f);
	}

	private void SpawnDroppedGlider(Vector3 pos, bool safeSpawn)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.gliderSpawnPickup, pos, this.gliderHeld.transform.rotation);
		Rigidbody component = gameObject.GetComponent<Rigidbody>();
		if (!LocalPlayer.FpCharacter.Grounded && !safeSpawn)
		{
			component.AddForce(LocalPlayer.Rigidbody.velocity * 100f, ForceMode.Force);
		}
		else
		{
			base.StartCoroutine(this.forceHighDragOnSpawn(component));
		}
	}

	private IEnumerator forceHighDragOnSpawn(Rigidbody rb)
	{
		if (rb)
		{
			rb.angularDrag = 15f;
			rb.drag = 15f;
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		if (rb)
		{
			rb.angularDrag = 0.05f;
			rb.drag = 0.05f;
		}
		yield break;
	}

	private void enableFallDamage()
	{
		LocalPlayer.AnimControl.disconnectFromGlider = false;
	}

	private bool isOutsideWorldBounds()
	{
		Vector3 position = LocalPlayer.Transform.position;
		return position.x > 2200f || position.x < -2200f || position.z > 2200f || position.z < -2200f;
	}

	public GameObject gliderHeld;

	public GameObject gliderSpawnPickup;

	public Transform VRGliderTr;

	private bool wasFlying;

	private float startFlyingDelay;

	private Vector3 startGliderOffset;

	private Vector3 smoothGliderOffset;
}
