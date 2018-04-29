using System;
using System.Collections;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class playerPlaceArtifactAction : MonoBehaviour
{
	
	private IEnumerator placeArtifactRoutine(Transform heldArtifact)
	{
		if (this.doingPlaceArtifact || this.spawnedArtifactInWorld != null)
		{
			yield break;
		}
		this.doingPlaceArtifact = true;
		LocalPlayer.Transform.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.AnimControl.knockedDown = true;
		LocalPlayer.CamFollowHead.lockYCam = true;
		LocalPlayer.Animator.SetInteger("artifactState", 2);
		heldArtifact.SendMessage("enableArtifactOpen");
		LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
		LocalPlayer.AnimControl.StartCoroutine("smoothEnableLayerNew", 2);
		LocalPlayer.AnimControl.StartCoroutine("smoothDisableLayerNew", 4);
		yield return YieldPresets.WaitOneSecond;
		while (LocalPlayer.AnimControl.fullBodyState2.shortNameHash == this.artifactPutDownHash)
		{
			LocalPlayer.Animator.SetInteger("artifactState", 0);
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.CamRotator.enabled = false;
			LocalPlayer.AnimControl.useRootMotion = true;
			LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
			LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
			if (LocalPlayer.AnimControl.fullBodyState2.normalizedTime > 0.615f && this.spawnedArtifactInWorld == null)
			{
				float @float = heldArtifact.GetComponent<Animator>().GetFloat("spinSpeed");
				base.StartCoroutine(this.spawnPlacedPrefab(heldArtifact));
			}
			yield return null;
		}
		if (this.spawnedArtifactInWorld == null)
		{
			heldArtifact.SendMessage("resetOpen");
		}
		LocalPlayer.AnimControl.StartCoroutine("smoothEnableLayerNew", 4);
		LocalPlayer.AnimControl.StartCoroutine("smoothDisableLayerNew", 2);
		LocalPlayer.AnimControl.knockedDown = false;
		LocalPlayer.CamFollowHead.lockYCam = false;
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.FpCharacter.CanJump = true;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.CamRotator.stopInput = false;
		if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Pause)
		{
			LocalPlayer.FpCharacter.Locked = false;
		}
		LocalPlayer.CamFollowHead.smoothLock = false;
		LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		this.doingPlaceArtifact = false;
		yield return null;
		yield break;
	}

	
	private IEnumerator spawnPlacedPrefab(Transform currentArtifact)
	{
		Vector3 spawnPos = currentArtifact.position;
		Vector3 castPos = spawnPos;
		castPos.y += 3f;
		RaycastHit hit;
		if (Physics.Raycast(castPos, Vector3.down, out hit, 10f, this.mask, QueryTriggerInteraction.Ignore))
		{
			spawnPos = hit.point;
			spawnPos.y += 1.5f;
		}
		spawnPos.y += 0.15f;
		this.spawnedArtifactInWorld = UnityEngine.Object.Instantiate<GameObject>(this.placedPrefab, spawnPos, Quaternion.LookRotation(Vector3.down));
		yield return this.spawnedArtifactInWorld;
		artifactBallController controller = currentArtifact.GetComponent<artifactBallController>();
		artifactBallPlacedController abpc = this.spawnedArtifactInWorld.transform.GetComponent<artifactBallPlacedController>();
		if (abpc)
		{
			abpc.setSpinSpeed(controller.spin);
			abpc.setArtifactState(controller.currentStateIndex);
		}
		LocalPlayer.Inventory.RemoveItem(this._artifactId, 1, false, false);
		base.Invoke("equipPreviousWeapon", 1f);
		yield break;
	}

	
	private void equipPreviousWeapon()
	{
		LocalPlayer.Inventory.EquipPreviousWeaponDelayed();
	}

	
	private void setHeldArtifactState(int set)
	{
		if (this.heldArtifactGo)
		{
			artifactBallController component = this.heldArtifactGo.GetComponent<artifactBallController>();
			if (component)
			{
				component.forceArtifactState(set);
			}
		}
	}

	
	public GameObject placedPrefab;

	
	public GameObject heldArtifactGo;

	
	public GameObject spawnedArtifactInWorld;

	
	private bool doingPlaceArtifact;

	
	private float currentSpinSpeed;

	
	private int mask = 103948289;

	
	[ItemIdPicker]
	public int _artifactId;

	
	private int artifactPutDownHash = Animator.StringToHash("artifactPutDown");
}
