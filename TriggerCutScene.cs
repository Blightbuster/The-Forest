using System;
using System.Collections;
using FMOD.Studio;
using PathologicalGames;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


public class TriggerCutScene : MonoBehaviour
{
	
	
	
	public bool skipOpening { get; private set; }

	
	private void Awake()
	{
		this.forwardHullConstrain = this.FrontOfPlane.GetComponentInChildren<TransformConstraint>().gameObject;
		debrisPart2[] componentsInChildren = this.debrisInterior1.GetComponentsInChildren<debrisPart2>(true);
		if (componentsInChildren[0])
		{
			this.debrisInterior2 = componentsInChildren[0].gameObject;
		}
		this.timmySleepGo = this.playerSeatGo.transform.parent.Find("planecrash_ANIM_timmyIdle").gameObject;
		this.clientCutScenePlayerGo = this.playerPosGo.GetComponentInChildren<setupClientPlaneCrash>().gameObject;
		Transform[] componentsInChildren2 = this.playerPosGo.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren2)
		{
			if (transform.name.Contains("clientAislePos"))
			{
				this.clientAisleCamPos1 = transform;
			}
			if (transform.name.Contains("clientStartPos1"))
			{
				this.clientSeatPos1 = transform;
			}
			if (transform.name.Contains("clientStartPos2"))
			{
				this.clientSeatPos2 = transform;
			}
			if (transform.name.Contains("clientStartPos3"))
			{
				this.clientSeatPos3 = transform;
			}
			if (transform.name.Contains("clientStartPos4"))
			{
				this.clientSeatPos4 = transform;
			}
			if (transform.name.Contains("clientStartPos5"))
			{
				this.clientSeatPos5 = transform;
			}
			if (transform.name.Contains("clientStartPos6"))
			{
				this.clientSeatPos6 = transform;
			}
			if (transform.name.Contains("clientStartPos7"))
			{
				this.clientSeatPos7 = transform;
			}
			if (transform.name.Contains("clientStartPos8"))
			{
				this.clientSeatPos8 = transform;
			}
			if (transform.name.Contains("playerOnFloorHeadGeo"))
			{
				this.playerOnFloorHeadGeo = transform.gameObject;
			}
			if (transform.name.Contains("CLIENT_playerOnPlane"))
			{
				this.clientPlayerOnPlaneGo = transform.gameObject;
			}
			if (transform.name.Contains("clientSeat"))
			{
				this.clientStartCamPos = transform;
			}
		}
		Transform[] componentsInChildren3 = this.playerSeatGo.GetComponentsInChildren<Transform>();
		foreach (Transform transform2 in componentsInChildren3)
		{
			if (transform2.name.Contains("book_ANIM_base"))
			{
				this.survivalBookGo = transform2.gameObject;
			}
			if (transform2.name.Contains("iconGrp"))
			{
				this.iconGo = transform2.gameObject;
			}
		}
		this.crawl = this.PlaneReal.GetComponentInChildren<setupPlayerCrawl>();
		this.pEvents = base.transform.root.GetComponent<planeEvents>();
		this.passengersScript = this.Passengers.GetComponentsInChildren<setupPassengers>();
		if (LevelSerializer.IsDeserializing || !Scene.PlaneCrash || !Scene.PlaneCrash.ShowCrash)
		{
			base.enabled = false;
		}
		else if (CoopPeerStarter.DedicatedHost)
		{
			base.Invoke("DedicatedServerPlaneInit", 0.25f);
		}
	}

	
	private void DedicatedServerPlaneInit()
	{
		this.planeController.setPlanePosition();
		this.FinalizePlanePosition();
		this.disablePlaneCrash();
		this.disablePlaneAnim();
		UnityEngine.Object.Destroy(this);
	}

	
	public IEnumerator beginPlaneCrash()
	{
		this.preloadEvents = new string[]
		{
			"event:/ambient/plane_start/body_fly_past",
			"event:/ambient/plane_start/fall_forward",
			"event:/ambient/plane_start/flight_attendant",
			"event:/ambient/plane_start/hit_ground",
			"event:/ambient/plane_start/hit_screen",
			"event:/ambient/plane_start/take_timmy",
			"event:/ambient/plane_start/notebook_plane_sequence"
		};
		FMODCommon.PreloadEvents(this.preloadEvents);
		this.hasPreloadedEvents = true;
		this.clientCutScenePlayerGo.SetActive(false);
		if (BoltNetwork.isClient)
		{
			this.iconGo.SetActive(false);
		}
		Scene.HudGui.ShowHud(false);
		if (this.IconsAndTextMain)
		{
			this.IconsAndTextMain.SetActive(false);
		}
		this.SpaceTut.SetActive(true);
		Clock.planecrash = true;
		this.disabled = false;
		this.skipOpening = false;
		Scene.Atmosphere.TimeOfDay = 302f;
		this.Atm.LightingTimeOfDayOverrideValue = 180f;
		this.Atm.OverrideLightingTimeOfDay = true;
		this.Atm.FogMaxHeight = 1500f;
		this.Hud.enabled = false;
		LocalPlayer.Inventory.enabled = false;
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.PlaneCrash;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.FpCharacter.Locked = true;
		LocalPlayer.MainRotator.enabled = false;
		this.storeCameraDamp = LocalPlayer.CamRotator.dampingTime;
		LocalPlayer.CamRotator.dampingTime = 0f;
		this.planeController.beginPlaneCrash();
		if (BoltNetwork.isClient)
		{
			base.StartCoroutine("lockPlayerPosition");
		}
		if (!this.crawl)
		{
			this.crawl = this.PlaneReal.GetComponentInChildren<setupPlayerCrawl>();
		}
		this.crawl.gameObject.SetActive(false);
		this.timmySleepGo.SetActive(true);
		this.pmTrigger.SendEvent("begin");
		LocalPlayer.MainCam.SendMessage("GuiOff");
		if (BoltNetwork.isServer)
		{
			LocalPlayer.Transform.position += new Vector3(2f, 0f, 0f);
		}
		yield return new WaitForSeconds(0.1f);
		this.setupClouds();
		yield return new WaitForSeconds(0.4f);
		base.StartCoroutine(this.enablePlayerControl());
		yield return new WaitForSeconds(0.7f);
		this.finishedInitialLoad = true;
		this.PlaySounds();
		this.getBook = false;
		while (!this.getBook)
		{
			if (BoltNetwork.isClient)
			{
				break;
			}
			yield return null;
		}
		if (BoltNetwork.isClient)
		{
			this.clientPlayerOnPlaneGo.GetComponent<Animator>().SetFloat("clientBlend", 1f);
			this.clientPlayerOnPlaneGo.GetComponent<Animator>().SetBool("client", true);
		}
		FMODCommon.PlayOneshot("event:/ambient/plane_start/notebook_plane_sequence", this.playerSeatPos);
		this.playerSeatGo.GetComponent<Animator>().SetBool("toBook", true);
		this.pmTrigger.SendEvent("continue");
		base.Invoke("resetBook", 1f);
		this.planeController.setupCameraShake();
		yield return new WaitForSeconds(9f);
		this.playerSeatGo.GetComponent<playerPlaneControl>().book.SetActive(false);
		yield return new WaitForSeconds(9.8f);
		base.StartCoroutine(this.moveCloudsRoutine(this._stormClouds.transform));
		this.ScreenFlight.SetActive(false);
		this.ScreenFlightTimmy.SetActive(false);
		this.ScreenFlightClient.SetActive(false);
		this.ScreenCrash.SetActive(true);
		this.ScreenCrashTimmy.SetActive(true);
		this.ScreenCrashClient.SetActive(true);
		this.planeController.startCrashCameraShake();
		this.ShowDamage();
		yield return new WaitForSeconds(25f);
		this.SmokeOn();
		yield break;
	}

	
	private IEnumerator moveCloudsRoutine(Transform tr)
	{
		float t = 0f;
		while (t < 1f)
		{
			Vector3 localPos = tr.localPosition;
			localPos.y += 0.0005f;
			tr.localPosition = localPos;
			t += Time.deltaTime / 10f;
			yield return null;
		}
		if (tr)
		{
			UnityEngine.Object.Destroy(tr.gameObject);
		}
		yield break;
	}

	
	private void resetBook()
	{
		this.playerSeatGo.GetComponent<Animator>().SetBool("toBook", false);
	}

	
	private IEnumerator enablePlayerControl()
	{
		yield break;
	}

	
	public IEnumerator forceCameraOnPlane()
	{
		float timer = 0f;
		while (timer < 1f)
		{
			Transform seatPos = this.planeController.seatPos;
			if (BoltNetwork.isClient)
			{
				seatPos = this.clientStartCamPos;
			}
			LocalPlayer.Transform.rotation = seatPos.rotation;
			LocalPlayer.FpCharacter.LockView(true);
			if (!Scene.HudGui.PauseMenu.activeInHierarchy)
			{
				TheForest.Utils.Input.LockMouse();
			}
			LocalPlayer.CamFollowHead.enableMouseControl(false);
			LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
			LocalPlayer.FpCharacter.enabled = false;
			LocalPlayer.CamRotator.enabled = false;
			LocalPlayer.MainRotator.enabled = false;
			LocalPlayer.Transform.eulerAngles = new Vector3(0f, LocalPlayer.Transform.eulerAngles.y, 0f);
			LocalPlayer.MainCamTr.rotation = seatPos.rotation;
			LocalPlayer.MainCamTr.eulerAngles = new Vector3(0f, LocalPlayer.MainCamTr.eulerAngles.y, 0f);
			timer += Time.deltaTime / 1.5f;
			yield return null;
		}
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.MainRotator.enabled = true;
		yield return null;
		LocalPlayer.CamFollowHead.enableMouseControl(true);
		LocalPlayer.FpCharacter.UnLockView();
		yield break;
	}

	
	private void Update()
	{
		if (!this.skipOpening && Clock.planecrash)
		{
			if (!Scene.HudGui.PauseMenu.activeSelf && (TheForest.Utils.Input.GetButtonDown("Jump") || TriggerCutScene.FastStart))
			{
				this.SpaceTut.SetActive(false);
				this.LightsFlight.SetActive(false);
				this.pmTrigger.SendEvent("toSkipOpening");
				this.skipOpening = true;
			}
		}
		else if (!Scene.HudGui.PauseMenu.activeInHierarchy)
		{
			TheForest.Utils.Input.LockMouse();
		}
	}

	
	private void setupClouds()
	{
		this.clouds.transform.position = this.mainHull.position;
		this.clouds.transform.rotation = this.mainHull.rotation;
		this.clouds.SetActive(true);
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("CutScene/PlaneClouds"), this.clouds.transform.position, this.clouds.transform.rotation);
		gameObject.transform.parent = this.clouds.transform;
		base.StartCoroutine("moveClouds");
	}

	
	private IEnumerator moveClouds()
	{
		for (;;)
		{
			this.clouds.transform.position += this.clouds.transform.forward * -10f * Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	private void enableMoBlur()
	{
	}

	
	private void playAttendantDialogue()
	{
		this.attendantDialogueEvent = FMODCommon.PlayOneshot("event:/ambient/plane_start/flight_attendant", this.playerSeatPos);
	}

	
	private void enableAnimation()
	{
		if (this.debrisInterior1)
		{
			this.debrisInterior1.GetComponentInChildren<Animator>().SetBool("startCrash", true);
		}
		this.planeAnim.SetBoolReflected("begin", true);
		this.StopEventEmitter(this.SFX_inplane);
		if (this.timmySleepGo)
		{
			this.timmySleepGo.GetComponent<Animator>().SetBool("crashBegin", true);
		}
		if (this.playerSeatGo)
		{
			this.playerSeatGo.GetComponent<Animator>().SetBool("crashBegin", true);
		}
		if (BoltNetwork.isClient)
		{
			this.clientPlayerOnPlaneGo.GetComponent<Animator>().SetBool("crashBegin", true);
		}
		foreach (setupPassengers setupPassengers in this.passengersScript)
		{
			if (setupPassengers)
			{
				setupPassengers.Invoke("triggerFall1", UnityEngine.Random.Range(0.1f, 0.4f));
				setupPassengers.Invoke("triggerFlyBack", 11.5f);
				setupPassengers.Invoke("triggerFrontSeats", 5.2f);
			}
		}
		this.pEvents.fallForward1();
	}

	
	private void PlaySounds()
	{
		this.StartEventEmitter(this.SFX_inplane);
		this.StartEventEmitter(this.SFX_Music);
	}

	
	private void StartEventEmitter(GameObject parent)
	{
		parent.GetComponent<FMOD_StudioEventEmitter>().Play();
	}

	
	private void StopEventEmitter(GameObject parent)
	{
		parent.GetComponent<FMOD_StudioEventEmitter>().Stop();
	}

	
	public void StopSounds()
	{
		this.pEvents.stopFMODEvents();
		if (base.transform && base.transform.parent)
		{
			foreach (FMOD_AnimationEventHandler fmod_AnimationEventHandler in base.transform.parent.GetComponentsInChildren<FMOD_AnimationEventHandler>())
			{
				fmod_AnimationEventHandler.enabled = false;
			}
		}
		FMODCommon.ReleaseIfValid(this.attendantDialogueEvent, STOP_MODE.ALLOWFADEOUT);
		this.attendantDialogueEvent = null;
		this.StopEventEmitter(this.SFX_inplane);
		this.StopEventEmitter(this.SFX_TakeTimmy);
		PlaneCrashAudioState.Disable();
	}

	
	private void ShowDamage()
	{
		if (!this.skipOpening)
		{
			this.GlassAndStuff.SetActive(true);
		}
	}

	
	private void SmokeOn()
	{
		this.Smoke.SetActive(true);
	}

	
	private void GroundOn()
	{
		if (this.GroundElements)
		{
			this.GroundElements.SetActive(true);
		}
	}

	
	private IEnumerator startTimmyCutscene()
	{
		this.clientPlayerOnPlaneGo.SetActive(false);
		this.disablePlaneAnim();
		this.ShowEnemies();
		if (BoltNetwork.isClient)
		{
			this.clientCutScenePlayerGo.SetActive(true);
			this.playerOnFloorHeadGeo.SetActive(true);
		}
		else
		{
			this.playerOnFloorHeadGeo.SetActive(false);
		}
		if (this.crawl)
		{
			this.crawl.gameObject.SetActive(true);
		}
		LocalPlayer.CamFollowHead.enableAisle();
		yield return new WaitForSeconds(3f);
		if (BoltNetwork.isRunning && !CoopPeerStarter.DedicatedHost)
		{
			base.StartCoroutine("hideOtherNetPlayers");
		}
		this.startPlayerCrawl();
		yield break;
	}

	
	private void startPlayerCrawl()
	{
		if (this.crawl)
		{
			this.crawl.transform.SendMessage("startCrawl", SendMessageOptions.DontRequireReceiver);
		}
	}

	
	private void ShowEnemies()
	{
		this.LightsFlight.SetActive(false);
		this.GlassAndStuff.SetActive(false);
		this.PlaneInterior.material = this.PlaneCrashOff;
		this.LightsCrash.SetActive(true);
		this.Mutant = UnityEngine.Object.Instantiate<GameObject>(this.MutantOnPlaneReal, this.MutantOnPlane.transform.position, this.MutantOnPlane.transform.rotation);
		this.Mutant.transform.localScale = this.MutantOnPlane.transform.localScale;
		this.Mutant.transform.parent = this.PlaneReal.transform;
		if (this.timmySeat)
		{
			UnityEngine.Object.Destroy(this.timmySeat);
		}
		if (this.timmySleepGo)
		{
			UnityEngine.Object.Destroy(this.timmySleepGo);
		}
		if (this.playerSeatGo)
		{
			UnityEngine.Object.Destroy(this.playerSeatGo);
		}
		if (this.Passengers)
		{
			UnityEngine.Object.Destroy(this.Passengers);
		}
		if (this.debrisInterior1)
		{
			UnityEngine.Object.Destroy(this.debrisInterior1);
		}
		Resources.UnloadUnusedAssets();
		UnityEngine.Object.Destroy(this.playerSeatGo);
		base.Invoke("PlayTimmySounds", 1f);
		this.Hull.OnCrash();
	}

	
	private void PlayTimmySounds()
	{
		this.StartEventEmitter(this.SFX_TakeTimmy);
	}

	
	private void FinalizePlanePosition()
	{
		if (Scene.TriggerCutScene && Scene.TriggerCutScene.planeController)
		{
			Scene.TriggerCutScene.planeController.enabled = false;
		}
		if (BoltNetwork.isClient)
		{
			this.planeController.transform.position = CoopServerInfo.Instance.state.PlanePosition;
			this.planeController.transform.rotation = CoopServerInfo.Instance.state.PlaneRotation;
		}
		else
		{
			Transform transform = PlaneCrashLocations.finalPositions[PlaneCrashLocations.crashSite].transform;
			this.planeController.transform.position = transform.position;
			this.planeController.transform.rotation = transform.rotation;
		}
		base.Invoke("FixLoaders", 1f);
		foreach (GameObject gameObject in this.enableAfterCrash)
		{
			if (gameObject)
			{
				gameObject.SetActive(true);
			}
		}
		Scene.PlaneGreebles.transform.position = this.planeController.transform.position;
		Scene.PlaneGreebles.transform.rotation = this.planeController.transform.rotation;
		Scene.PlaneGreebles.SetActive(true);
	}

	
	private void FixLoaders()
	{
		if (this.planeController.Loaders != null)
		{
			foreach (GameObject gameObject in this.planeController.Loaders)
			{
				gameObject.SendMessage("CheckWsToken", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private IEnumerator CleanUp()
	{
		this.GlassAndStuff.SetActive(false);
		UnityEngine.Object.Destroy(this.ScreenFlight);
		UnityEngine.Object.Destroy(this.ScreenFlightTimmy);
		UnityEngine.Object.Destroy(this.ScreenCrash);
		UnityEngine.Object.Destroy(this.ScreenCrashTimmy);
		UnityEngine.Object.Destroy(this.ScreenCrashClient);
		UnityEngine.Object.Destroy(this.ScreenFlightClient);
		Animator[] anims = this.playerPosGo.GetComponentsInChildren<Animator>(true);
		foreach (Animator animator in anims)
		{
			UnityEngine.Object.Destroy(animator.gameObject);
		}
		base.StopAllCoroutines();
		this.FinalizePlanePosition();
		this.PlaneInterior.material = this.PlaneCrashOff;
		this.SpaceTut.SetActive(false);
		Scene.HudGui.ShowHud(true);
		if (this.IconsAndTextMain)
		{
			this.IconsAndTextMain.SetActive(true);
		}
		this.skipOpening = true;
		this.StopSounds();
		base.StopCoroutine("moveClouds");
		this.Stewardess.SetActive(true);
		this.planeController.StartCoroutine("setAisleCam2");
		if (this.Clouds)
		{
			UnityEngine.Object.Destroy(this.Clouds);
		}
		if (this.FakeBody)
		{
			UnityEngine.Object.Destroy(this.FakeBody);
		}
		this.Hull.OnCrash();
		if (this.LightsCrash)
		{
			UnityEngine.Object.Destroy(this.LightsCrash);
		}
		if (this.timmySeat)
		{
			UnityEngine.Object.Destroy(this.timmySeat);
		}
		if (this.playerSeatGo)
		{
			UnityEngine.Object.Destroy(this.playerSeatGo.transform.parent.gameObject);
		}
		if (this.timmySleepGo)
		{
			UnityEngine.Object.Destroy(this.timmySleepGo);
		}
		if (this.clientCutScenePlayerGo)
		{
			UnityEngine.Object.Destroy(this.clientCutScenePlayerGo);
		}
		if (this.clientPlayerOnPlaneGo)
		{
			UnityEngine.Object.Destroy(this.clientPlayerOnPlaneGo);
		}
		setupPlayerCrawl crawl = this.PlaneReal.GetComponentInChildren<setupPlayerCrawl>();
		if (crawl)
		{
			crawl.gameObject.SetActive(false);
		}
		if (this._stormClouds)
		{
			UnityEngine.Object.Destroy(this._stormClouds);
		}
		this.GroundOn();
		base.enabled = false;
		this.Hud.enabled = true;
		if (this.debrisInterior1)
		{
			UnityEngine.Object.Destroy(this.debrisInterior1);
		}
		if (this.Mutant)
		{
			UnityEngine.Object.Destroy(this.Mutant);
		}
		if (this.Passengers)
		{
			UnityEngine.Object.Destroy(this.Passengers);
		}
		if (this.Smoke)
		{
			UnityEngine.Object.Destroy(this.Smoke);
		}
		if (this.clouds)
		{
			UnityEngine.Object.Destroy(this.clouds);
		}
		this.MutantOnPlaneReal = null;
		this.Passengers = null;
		this.debrisInterior1 = null;
		this.playerPosGo = null;
		this.timmySeatGo = null;
		this.timmySleepGo = null;
		this.playerSeatGo = null;
		this.StopEventEmitter(this.SFX_Music);
		this.PlaneAfterSound.SetActive(true);
		Resources.UnloadUnusedAssets();
		base.StopCoroutine("hideOtherNetPlayers");
		base.StartCoroutine("startPlayerInPlane");
		yield return new WaitForSeconds(34f);
		this.TurnOffPM();
		yield break;
	}

	
	private IEnumerator startPlayerInPlane()
	{
		base.StopCoroutine("beginPlaneCrash");
		LocalPlayer.FpCharacter.allowFallDamage = false;
		LocalPlayer.AnimControl.introCutScene = true;
		if (BoltNetwork.isClient)
		{
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.AnimControl.playerCollider.enabled = false;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			base.StopCoroutine("lockPlayerPosition");
			LocalPlayer.Transform.position = this.startPlayerPos;
		}
		LocalPlayer.Stats.setupFirstDayConditions();
		LocalPlayer.ScriptSetup.weaponRight.gameObject.SetActive(false);
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.WakingUp;
		yield return YieldPresets.WaitPointFiveSeconds;
		int clientNumber = 0;
		Transform standPos;
		if (BoltNetwork.isClient)
		{
			for (int i = 0; i < Scene.SceneTracker.allClients.Count; i++)
			{
				if (base.gameObject == Scene.SceneTracker.allClients[i])
				{
					clientNumber = i;
				}
			}
			if (clientNumber == 0)
			{
				standPos = this.clientSeatPos1;
			}
			else if (clientNumber == 1)
			{
				standPos = this.clientSeatPos2;
			}
			else if (clientNumber == 2)
			{
				standPos = this.clientSeatPos3;
			}
			else if (clientNumber == 3)
			{
				standPos = this.clientSeatPos4;
			}
			else if (clientNumber == 4)
			{
				standPos = this.clientSeatPos5;
			}
			else if (clientNumber == 5)
			{
				standPos = this.clientSeatPos6;
			}
			else if (clientNumber == 6)
			{
				standPos = this.clientSeatPos7;
			}
			else if (clientNumber == 7)
			{
				standPos = this.clientSeatPos8;
			}
			else
			{
				standPos = this.clientSeatPos1;
			}
			if (clientNumber < 5)
			{
				LocalPlayer.Animator.SetBool("client", true);
			}
			else
			{
				LocalPlayer.Animator.SetBool("client", false);
			}
		}
		else
		{
			standPos = this.PlaneReal.GetComponentInChildren<standUpId>().transform;
		}
		LocalPlayer.Transform.position = standPos.position;
		LocalPlayer.Transform.rotation = standPos.rotation;
		this.disablePlaneCrash();
		yield return YieldPresets.WaitOneSecond;
		this.planeController.doNavMesh();
		Scene.Atmosphere.FogMaxHeight = 400f;
		if (!BoltNetwork.isRunning || Scene.SceneTracker.allPlayers.Count <= 1)
		{
			Scene.Atmosphere.TimeOfDay = 302f;
		}
		Scene.Atmosphere.ForceSunRotationUpdate = true;
		Scene.Atmosphere.OverrideLightingTimeOfDay = false;
		LocalPlayer.CamRotator.dampingTime = this.storeCameraDamp;
		LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(false);
		LocalPlayer.Stats.setupFirstDayConditions();
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		LocalPlayer.MainCam.SendMessage("GuiOn");
		LocalPlayer.AnimControl.introCutScene = true;
		LocalPlayer.AnimControl.useRootMotion = true;
		LocalPlayer.CamFollowHead.enableMouseControl(false);
		LocalPlayer.MainCamTr.localEulerAngles = Vector3.zero;
		LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
		LocalPlayer.FpCharacter.enabled = false;
		LocalPlayer.FpCharacter.capsule.center = new Vector3(LocalPlayer.FpCharacter.capsule.center.x, LocalPlayer.FpCharacter.originalYPos, LocalPlayer.FpCharacter.capsule.center.z);
		if (BoltNetwork.isClient && clientNumber < 5)
		{
			LocalPlayer.AnimControl.lockGravity = true;
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.AnimControl.playerCollider.enabled = false;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = false;
			LocalPlayer.Transform.position = standPos.position;
			LocalPlayer.Transform.rotation = standPos.rotation;
		}
		else
		{
			LocalPlayer.AnimControl.playerCollider.enabled = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
		}
		LocalPlayer.CamRotator.enabled = false;
		LocalPlayer.MainRotator.enabled = false;
		LocalPlayer.CamFollowHead.disableFlying();
		LocalPlayer.CamFollowHead.followAnim = true;
		LocalPlayer.Animator.SetLayerWeightReflected(1, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(3, 0f);
		LocalPlayer.Animator.SetLayerWeightReflected(4, 0f);
		LocalPlayer.Transform.position = new Vector3(LocalPlayer.Transform.position.x, LocalPlayer.Transform.position.y + 0.36f, LocalPlayer.Transform.position.z);
		LocalPlayer.AnimControl.animEvents.introCutScene = true;
		LocalPlayer.Transform.position = standPos.position;
		LocalPlayer.Transform.rotation = standPos.rotation;
		float storeFov = PlayerPreferences.Fov;
		PlayerPreferences.Fov = 76f;
		if (ForestVR.Enabled)
		{
			LocalPlayer.vrPlayerControl.VRCameraRig.localEulerAngles = Vector3.zero;
		}
		else if (!TriggerCutScene.FastStart)
		{
			LocalPlayer.Animator.SetBoolReflected("introStandBool", true);
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		if (clientNumber > 5)
		{
			LocalPlayer.Animator.SetBoolReflected("client", false);
		}
		Scene.Cams.SleepCam.SetActive(false);
		base.Invoke("resetStandBool", 2.5f);
		base.StartCoroutine(this.resetFov(storeFov));
		LocalPlayer.Transform.SendMessage("enableMpRenderers");
		if (!ForestVR.Enabled)
		{
			if (BoltNetwork.isClient)
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 2.5f));
			}
			else
			{
				yield return YieldPresets.WaitOnePointThreeSeconds;
			}
		}
		else
		{
			LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		}
		int standupHash = Animator.StringToHash("standup");
		AnimatorStateInfo state = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
		while (state.IsTag("getup"))
		{
			state = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
			yield return null;
		}
		LocalPlayer.ScriptSetup.forceLocalPos.enabled = true;
		LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 999f);
		LocalPlayer.CamRotator.rotationRange = new Vector2(LocalPlayer.FpCharacter.minCamRotationRange, 0f);
		LocalPlayer.ScriptSetup.weaponRight.gameObject.SetActive(true);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = Vector3.zero;
		LocalPlayer.FpCharacter.UnLockView();
		LocalPlayer.FpCharacter.enabled = true;
		LocalPlayer.MainRotator.resetOriginalRotation = true;
		LocalPlayer.CamRotator.resetOriginalRotation = true;
		LocalPlayer.CamFollowHead.followAnim = false;
		LocalPlayer.CamRotator.enabled = true;
		LocalPlayer.MainRotator.enabled = true;
		LocalPlayer.FpCharacter.allowFallDamage = true;
		if (BoltNetwork.isClient && clientNumber < 5)
		{
			LocalPlayer.AnimControl.lockGravity = false;
			LocalPlayer.AnimControl.playerCollider.enabled = true;
			LocalPlayer.AnimControl.playerHeadCollider.enabled = true;
			LocalPlayer.Rigidbody.useGravity = true;
		}
		LocalPlayer.AnimControl.useRootMotion = false;
		LocalPlayer.FpCharacter.CanJump = true;
		LocalPlayer.Animator.SetLayerWeightReflected(1, 1f);
		LocalPlayer.Animator.SetLayerWeightReflected(2, 0f);
		LocalPlayer.Inventory.enabled = true;
		LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
		LocalPlayer.ScriptSetup.bodyCollisionGo.SetActive(true);
		LocalPlayer.CamFollowHead.stopAllCameraShake();
		if (ForestVR.Enabled)
		{
			LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
		}
		if (BoltNetwork.isClient)
		{
			LocalPlayer.Animator.SetBool("client", true);
		}
		yield break;
	}

	
	private IEnumerator holdClientPosition(Transform standPos)
	{
		AnimatorStateInfo state = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
		yield return YieldPresets.WaitOneSecond;
		while (state.IsTag("getup"))
		{
			yield return new WaitForEndOfFrame();
			Vector3 pos = LocalPlayer.Transform.position;
			pos.y = standPos.position.y;
			LocalPlayer.Transform.position = pos;
			state = LocalPlayer.Animator.GetCurrentAnimatorStateInfo(2);
		}
		yield break;
	}

	
	private IEnumerator resetFov(float set)
	{
		yield return new WaitForSeconds(5.3f);
		float t = 0f;
		float startVal = PlayerPreferences.Fov;
		float val = startVal;
		while (t < 1f)
		{
			t += Time.deltaTime / 2f;
			val = Mathf.SmoothStep(startVal, set, t);
			PlayerPreferences.Fov = val;
			yield return null;
		}
		yield break;
	}

	
	private void resetStandBool()
	{
		LocalPlayer.Animator.SetBoolReflected("introStandBool", false);
	}

	
	private void TurnOffPM()
	{
		this.PMGui.SetActive(false);
	}

	
	private void skipOpeningAnimation()
	{
		this.skipOpening = true;
		base.StopCoroutine("beginPlaneCrash");
		base.StopCoroutine("moveClouds");
		base.StopCoroutine("startTimmyCutscene");
		base.Invoke("TurnOffPM", 20f);
		base.CancelInvoke("SmokeOn");
		base.CancelInvoke("GroundOn");
		base.CancelInvoke("PlaySounds");
		base.CancelInvoke("ShowDamage");
		this.ShowDamage();
		this.StopSounds();
		base.Invoke("GroundOn", 2f);
		this.enableAnimation();
		this.planeAnim.speed = 60f;
		this.planeController.skipPlaneCrash();
		this.GlassAndStuff.SetActive(false);
		this.LightsFlight.SetActive(false);
		this.PlaneInterior.material = this.PlaneCrashOff;
		this.FrontOfPlane.SetActive(false);
	}

	
	private void disablePlaneCrash()
	{
		WorkScheduler.ShouldDoFullCycle = true;
		Scene.WorkScheduler.gameObject.SetActive(true);
		Clock.planecrash = false;
		Scene.MutantControler.startSetupFamilies();
		Scene.HudGui.ShowHud(true);
		if (this.hasPreloadedEvents)
		{
			FMODCommon.UnloadEvents(this.preloadEvents);
		}
		base.transform.parent = null;
	}

	
	private void doNavMesh()
	{
		this.disablePlaneAnim();
		this.planeController.doNavMesh();
	}

	
	private void disablePlaneAnim()
	{
		if (!this.disabled)
		{
			this.planeController.transform.parent = null;
			UnityEngine.Object.Instantiate<GameObject>(this.testSavePos, this.planeController.transform.position, this.planeController.transform.rotation);
			this.planeAnim.enabled = false;
			this.planeController.enabled = false;
			this.disabled = true;
		}
	}

	
	private IEnumerator lockPlayerPosition()
	{
		this.startPlayerPos = LocalPlayer.Transform.position;
		Vector3 grabPos = new Vector3(-1681f, 116f, 1631f);
		for (;;)
		{
			LocalPlayer.Transform.position = grabPos;
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator hideOtherNetPlayers()
	{
		Vector3 grabPos = new Vector3(-1681f, 200f, 1631f);
		for (;;)
		{
			for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
			{
				if (Scene.SceneTracker.allPlayers[i] != null && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet"))
				{
					Scene.SceneTracker.allPlayers[i].transform.position = grabPos;
				}
			}
			yield return null;
		}
		yield break;
	}

	
	public GameObject ScreenFlight;

	
	public GameObject ScreenFlightTimmy;

	
	public GameObject ScreenCrash;

	
	public GameObject ScreenCrashTimmy;

	
	public GameObject ScreenFlightClient;

	
	public GameObject ScreenCrashClient;

	
	public GameObject PMGui;

	
	public GameObject IconsAndTextMain;

	
	public PlayMakerFSM pmTrigger;

	
	public GameObject SpaceTut;

	
	public TheForestAtmosphere Atm;

	
	public planeCrashHeight planeController;

	
	private planeEvents pEvents;

	
	private setupPassengers[] passengersScript;

	
	private setupPlayerCrawl crawl;

	
	public Animator planeAnim;

	
	public GameObject testSavePos;

	
	public GameObject savedCrashPos;

	
	public CrashClearing Hull;

	
	public GameObject MutantOnPlane;

	
	public GameObject Passengers;

	
	public GameObject debrisInterior1;

	
	public GameObject debrisInterior2;

	
	public GameObject playerPosGo;

	
	public GameObject[] enableAfterCrash;

	
	public GameObject MutantOnPlaneReal;

	
	private GameObject Mutant;

	
	public Transform playerSeatPos;

	
	public Transform playerSeatPosVR;

	
	public Transform clientSeatPos1;

	
	public Transform clientSeatPos2;

	
	public Transform clientSeatPos3;

	
	public Transform clientSeatPos4;

	
	public Transform clientSeatPos5;

	
	public Transform clientSeatPos6;

	
	public Transform clientSeatPos7;

	
	public Transform clientSeatPos8;

	
	public Transform clientAisleCamPos1;

	
	public Transform clientStartCamPos;

	
	public Transform playerPlanePosition;

	
	public Transform playerAislePosition;

	
	public GameObject timmySeatGo;

	
	public GameObject clientCutScenePlayerGo;

	
	public GameObject clientPlayerOnPlaneGo;

	
	private GameObject cutSceneCharactersGo;

	
	private GameObject survivalBookGo;

	
	private GameObject iconGo;

	
	private GameObject playerOnFloorHeadGeo;

	
	private GameObject timmySeat;

	
	public GameObject PlaneReal;

	
	public GameObject timmySleepGo;

	
	public GameObject playerSeatGo;

	
	public int SmokeTime;

	
	public GameObject SFX_inplane;

	
	public GameObject SFX_TakeTimmy;

	
	public GameObject SFX_Music;

	
	public GameObject Smoke;

	
	public int GroundElementsTime;

	
	public GameObject GroundElements;

	
	public GameObject GlassAndStuff;

	
	public GameObject Clouds;

	
	public GameObject LightsCrash;

	
	public GameObject LightsFlight;

	
	public GameObject FrontOfPlane;

	
	public GameObject clouds;

	
	public Transform mainHull;

	
	public Camera Hud;

	
	public GameObject FakeBody;

	
	public GameObject Stewardess;

	
	public Material PlaneCrashOff;

	
	public Renderer PlaneInterior;

	
	public Material FlashingSign;

	
	public Material cloudsMaterial;

	
	public GameObject PlaneAfterSound;

	
	public GameObject _stormClouds;

	
	private string[] preloadEvents;

	
	private bool hasPreloadedEvents;

	
	public bool getBook;

	
	public bool fakePlaneActive;

	
	private float storeCameraDamp;

	
	private Vector3 startPlayerPos;

	
	private GameObject forwardHullConstrain;

	
	private int standUpHash = Animator.StringToHash("standup");

	
	private Vector3 storeCamLocalPos;

	
	private bool disabled;

	
	private float prevMaxVel;

	
	private bool finishedInitialLoad;

	
	private EventInstance attendantDialogueEvent;

	
	public static bool FastStart;
}
