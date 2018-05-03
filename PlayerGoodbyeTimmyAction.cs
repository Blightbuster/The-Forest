using System;
using System.Collections;
using TheForest.Items;
using TheForest.Player;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;


public class PlayerGoodbyeTimmyAction : MonoBehaviour
{
	
	private IEnumerator goodbyeTimmyRoutine()
	{
		LocalPlayer.AnimControl.endGameCutScene = true;
		this.setTimeAndWeather();
		this.lighterFlameGo.SetActive(false);
		bool doPlane = false;
		bool doPhoto = false;
		bool doLighterFlame = false;
		bool doBurn = false;
		bool doBurnAudio = false;
		bool doMusic = false;
		float musicTimer = 0f;
		AnimatorStateInfo state0 = LocalPlayer.AnimControl.currLayerState0;
		LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.Chest, false, true, false);
		LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.LeftHand, false, true, false);
		LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
		bool storeRed = LocalPlayer.ScriptSetup.targetInfo.isRed;
		LocalPlayer.ScriptSetup.targetInfo.isRed = true;
		while (state0.tagHash == this.enterCaveHash || state0.shortNameHash == this.goodbyeTimmyHash)
		{
			state0 = LocalPlayer.AnimControl.currLayerState0;
			LocalPlayer.Inventory.LockEquipmentSlot(Item.EquipmentSlot.LeftHand);
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.Rigidbody.isKinematic = true;
			LocalPlayer.AnimControl.useRootMotion = true;
			LocalPlayer.AnimControl.onRope = true;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
			Vector3 hidePos = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * -100f + LocalPlayer.Transform.up * 100f;
			for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (Scene.SceneTracker.allPlayers[i] != null && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
				{
					Scene.SceneTracker.allPlayers[i].transform.position = hidePos;
				}
			}
			if (LocalPlayer.AnimControl.currLayerState0.shortNameHash == this.goodbyeTimmyHash)
			{
				LocalPlayer.Animator.SetBool("goodbyeTimmy", false);
				Scene.HudGui.LoadingCavesInfo.SetActive(false);
				Scene.HudGui.Grid.repositionNow = true;
				musicTimer += Time.deltaTime;
				if (musicTimer > 1f && !doMusic)
				{
					FMODCommon.PlayOneshot("event:/endgame/cinematics/cine_timmy_goodbye_plane", base.transform);
					doMusic = true;
				}
				if (state0.normalizedTime > 0.115f && !doPlane)
				{
					this.SpawnOverHeadPlane();
					doPlane = true;
				}
				if (state0.normalizedTime > 0.4f && !doPhoto)
				{
					if (this.heldTimmyPhotoGo != null)
					{
						LocalPlayer.Inventory.Equip(LocalPlayer.AnimControl._timmyPhotoId, false);
					}
					doPhoto = true;
				}
				if (state0.normalizedTime > 0.57f)
				{
					this.heldLighterGo.SetActive(true);
				}
				if (state0.normalizedTime > 0.635f && !doLighterFlame)
				{
					this.lighterFlameGo.SetActive(true);
					doLighterFlame = true;
				}
				if (state0.normalizedTime > 0.64f && !doBurn)
				{
					Debug.Log("doing burn photo!");
					if (this.heldTimmyPhotoGo)
					{
						this.heldTimmyPhotoGo.SendMessage("burnPhotoRoutine");
					}
					doBurn = true;
				}
				if (state0.normalizedTime > 0.78f && !doBurnAudio)
				{
					FMODCommon.PlayOneshot("event:/endgame/cinematics/cine_timmy_goodbye_burn_photo", this.heldTimmyPhotoGo.transform);
					doBurnAudio = true;
				}
				if (state0.normalizedTime > 0.144f && Physics.Raycast(LocalPlayer.Transform.position, Vector3.down, 10f, this.mask, QueryTriggerInteraction.Ignore))
				{
					Vector3 point = this.Hit.point;
					point.y += this.Hit.distance;
					LocalPlayer.Transform.position = point;
				}
			}
			yield return null;
		}
		LocalPlayer.ScriptSetup.targetInfo.isRed = storeRed;
		LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
		LocalPlayer.AnimControl.endGameCutScene = false;
		LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, true, true);
		LocalPlayer.Inventory.UnlockEquipmentSlot(Item.EquipmentSlot.LeftHand);
		LocalPlayer.Inventory.RemoveItem(LocalPlayer.AnimControl._timmyPhotoId, 1, false, true);
		LocalPlayer.Animator.SetBool("goodbyeTimmy", false);
		LocalPlayer.AnimControl.playerCollider.isTrigger = false;
		LocalPlayer.AnimControl.playerHeadCollider.isTrigger = false;
		LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("noControl").Value = false;
		LocalPlayer.Transform.parent = Scene.SceneTracker.transform;
		LocalPlayer.Transform.parent = null;
		LocalPlayer.Transform.localScale = new Vector3(1f, 1f, 1f);
		LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.AnimControl.onRope = false;
		LocalPlayer.AnimControl.lockGravity = false;
		LocalPlayer.AnimControl.controller.useGravity = true;
		LocalPlayer.AnimControl.controller.isKinematic = false;
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.Animator.SetLayerWeightReflected(4, 1f);
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		if (!LocalPlayer.SavedData.ExitedEndgame)
		{
			yield return YieldPresets.WaitThreeSeconds;
			LocalPlayer.SavedData.ExitedEndgame.SetValue(true);
			LocalPlayer.SpecialItems.GetComponentInChildren<SurvivalBookTodo>().StrikeThroughAll();
			LocalPlayer.Tuts.ShowNewBuildingsAvailableTut();
		}
		yield break;
	}

	
	private void SpawnOverHeadPlane()
	{
		Vector3 vector = new Vector3(-1497f, 850f, -1384f);
		Vector3 vector2 = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * 400f;
		vector2.y = vector.y;
		vector2 = (vector2 - vector).normalized;
		Debug.DrawLine(vector, vector2, Color.red, 20f);
		GameObject plane = UnityEngine.Object.Instantiate<GameObject>(this.planeGo, vector, Quaternion.LookRotation(vector2));
		base.StartCoroutine(this.overheadPlaneMove(plane, vector2));
	}

	
	private IEnumerator overheadPlaneMove(GameObject plane, Vector3 dir)
	{
		Vector3 midPoint = new Vector3(0f, 1400f, 0f);
		for (;;)
		{
			if (LocalPlayer.GameObject)
			{
				plane.transform.position = plane.transform.position + dir * 1.4f;
				if (Vector3.Distance(midPoint, plane.transform.position) > 5500f)
				{
					break;
				}
			}
			yield return new WaitForFixedUpdate();
		}
		UnityEngine.Object.Destroy(plane);
		yield break;
		yield break;
	}

	
	private void setTimeAndWeather()
	{
		if (Scene.SceneTracker.goodbyeTimmyWeatherDone)
		{
			return;
		}
		Debug.Log("setting time and weather");
		Scene.RainTypes.CaveFilter.SetActive(true);
		Scene.RainFollowGO.SetActive(true);
		Scene.Atmosphere.ForceSunRotationUpdate = true;
		Scene.Atmosphere.TimeOfDay = 320f;
		Scene.Atmosphere.Visibility = 1500f;
		Scene.WeatherSystem.TurnOn(WeatherSystem.RainTypes.Heavy);
		Scene.WeatherSystem.GrowClouds();
		Scene.SceneTracker.goodbyeTimmyWeatherDone = true;
		base.Invoke("resetForceSunUpdate", 5f);
	}

	
	private void resetForceSunUpdate()
	{
		Scene.Atmosphere.ForceSunRotationUpdate = false;
	}

	
	public GameObject heldTimmyPhotoGo;

	
	public GameObject heldLighterGo;

	
	public GameObject lighterFlameGo;

	
	public GameObject planeGo;

	
	private int enterCaveHash = Animator.StringToHash("enterCave");

	
	private int goodbyeTimmyHash = Animator.StringToHash("goodbyeTimmy");

	
	private bool doPlane;

	
	private bool doPhoto;

	
	private bool doLighter;

	
	public LayerMask mask;

	
	private RaycastHit Hit;
}
