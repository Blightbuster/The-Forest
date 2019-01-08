using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class camFollowHead : MonoBehaviour
{
	private void Awake()
	{
		this.anim = base.GetComponent<Animation>();
		this.anim["turbulanceLight"].wrapMode = WrapMode.Loop;
		this.anim["turbulanceLight"].speed = 1.2f;
		this.anim["camShakePlane"].wrapMode = WrapMode.Loop;
		this.anim["camShakePlane"].speed = 2.4f;
		this.anim["camShakePlaneStart"].wrapMode = WrapMode.Once;
		this.anim["camShakePlaneStart"].speed = 1.7f;
		this.anim["camShakePlaneGround"].wrapMode = WrapMode.Loop;
		this.anim["camShakePlaneGround"].speed = 3.5f;
		this.anim["noShake"].wrapMode = WrapMode.Loop;
		this.anim["camShakeFall"].wrapMode = WrapMode.Loop;
		this.anim["camShakeFall"].speed = 2.5f;
		this.anim["camShakeZipline"].wrapMode = WrapMode.Loop;
		this.anim["camShakeZipline"].speed = 2.7f;
		this.anim["turbulanceLight"].layer = 1;
		this.anim["camShakePlaneGround"].layer = 1;
		this.anim["camShakePlane"].layer = 1;
		this.anim["noShake"].layer = 1;
		this.anim["camShakeFall"].layer = 1;
		this.anim["camShakeZipline"].layer = 1;
		this.setup = base.transform.root.GetComponentInChildren<playerScriptSetup>();
		this.thisTr = base.transform;
		this.startCamPos = this.thisTr.localPosition;
		this.mouse1 = base.GetComponentInChildren<SimpleMouseRotator>();
		this.mouse2 = base.transform.root.GetComponent<SimpleMouseRotator>();
		this.camTr = this.mouse1.transform;
	}

	private void Start()
	{
		this.offsetY = 0f;
		this.offsetZ = -0.05f;
		if (Scene.TriggerCutScene)
		{
			if (Scene.TriggerCutScene.playerPlanePosition)
			{
				GameObject gameObject = Scene.TriggerCutScene.playerPlanePosition.gameObject;
				if (gameObject)
				{
					this.planePos = gameObject.transform;
				}
			}
			if (Scene.TriggerCutScene.playerAislePosition)
			{
				GameObject gameObject2 = Scene.TriggerCutScene.playerAislePosition.gameObject;
				if (ForestVR.Enabled)
				{
					if (BoltNetwork.isClient)
					{
						gameObject2 = Scene.TriggerCutScene.ClientAislePositionVR.gameObject;
					}
					else
					{
						gameObject2 = Scene.TriggerCutScene.playerAislePositionVR.gameObject;
					}
				}
				if (gameObject2)
				{
					this.aislePos = gameObject2.transform;
				}
			}
		}
		if (this.setup.playerCam)
		{
			this.playerCamPos = this.setup.playerCam.localPosition;
		}
		if (this.setup.OvrCam)
		{
			this.ovrCamPos = this.setup.OvrCam.localPosition;
		}
		base.Invoke("checkPlayerControl", 1f);
	}

	private void OnEnable()
	{
		base.Invoke("checkPlayerControl", 1f);
	}

	private void checkPlayerControl()
	{
		if (base.enabled)
		{
			if (!Clock.planecrash)
			{
				LocalPlayer.CamRotator.enabled = true;
			}
			LocalPlayer.MainRotator.enabled = true;
		}
	}

	private void enableFollowAnim()
	{
		this.followAnim = true;
	}

	private void disableFollowAnim()
	{
		this.followAnim = false;
	}

	public IEnumerator preCrashCameraShake()
	{
		if (ForestVR.Enabled)
		{
			yield break;
		}
		yield return YieldPresets.WaitTwoSeconds;
		yield return YieldPresets.WaitTwoSeconds;
		this.anim.Play("noShake", PlayMode.StopAll);
		this.anim.CrossFade("turbulanceLight", 20f, PlayMode.StopSameLayer);
		yield break;
	}

	public IEnumerator planeCameraShake()
	{
		if (ForestVR.Enabled)
		{
			yield break;
		}
		this.anim.Stop("turbulanceLight");
		this.anim.Play("camShakePlaneStart", PlayMode.StopAll);
		yield return YieldPresets.WaitTwoSeconds;
		this.anim.Play("noShake", PlayMode.StopAll);
		this.anim.CrossFade("camShakePlane", 5f, PlayMode.StopSameLayer);
		yield return YieldPresets.WaitSeventeenSeconds;
		this.anim.Play("camShakePlaneGround", PlayMode.StopAll);
		yield return YieldPresets.WaitThreeSeconds;
		this.anim.CrossFade("noShake", 4f, PlayMode.StopSameLayer);
		yield return YieldPresets.WaitFourSeconds;
		this.anim.Stop("noShake");
		this.anim.Stop("camShakePlaneGround");
		yield break;
	}

	public void stopAllCameraShake()
	{
		this.anim.Stop("camShakePlaneStart");
		this.anim.Stop("camShakePlane");
		this.anim.Stop("camShakePlaneGround");
		this.anim.Stop("camShakeFall");
		this.anim.Stop("noShake");
		this.anim.Stop("turbulanceLight");
		this.anim.Stop("camShakeZipline");
	}

	public IEnumerator startFallingShake()
	{
		if (ForestVR.Enabled)
		{
			yield break;
		}
		this.anim.Play("noShake", PlayMode.StopAll);
		this.anim.CrossFade("camShakeFall", 1.3f, PlayMode.StopSameLayer);
		yield return YieldPresets.WaitTwoSeconds;
		yield break;
	}

	public IEnumerator startZipLineShake()
	{
		if (ForestVR.Enabled)
		{
			yield break;
		}
		this.anim.Play("noShake", PlayMode.StopAll);
		this.anim.CrossFade("camShakeZipline", 4f, PlayMode.StopSameLayer);
		yield return YieldPresets.WaitTwoSeconds;
		yield break;
	}

	private void Update()
	{
		this.doCamFollow();
	}

	private void LateUpdate()
	{
		this.updateCamPosition();
	}

	public void updateCamPosition()
	{
		if (this.flying)
		{
			if (!this.seatPos)
			{
				if (BoltNetwork.isClient && !ForestVR.Enabled)
				{
					this.seatPos = Scene.TriggerCutScene.clientStartCamPos;
				}
				else if (ForestVR.Enabled)
				{
					if (BoltNetwork.isClient)
					{
						this.seatPos = Scene.TriggerCutScene.ClientSeatPosVR;
					}
					else
					{
						this.seatPos = Scene.TriggerCutScene.playerSeatPosVR;
					}
				}
				else
				{
					this.seatPos = Scene.TriggerCutScene.playerSeatPos;
				}
			}
			if (this.seatPos)
			{
				this.thisTr.position = this.seatPos.position;
			}
			if (!this.shakeBlock)
			{
				this.shakeBlock = true;
			}
		}
		else if (this.aisle)
		{
			if (BoltNetwork.isClient && !ForestVR.Enabled)
			{
				if (Scene.TriggerCutScene.clientAisleCamPos1)
				{
					this.thisTr.position = Scene.TriggerCutScene.clientAisleCamPos1.position;
				}
			}
			else if (this.aislePos)
			{
				this.thisTr.position = this.aislePos.position;
			}
			if (!this.shakeBlock)
			{
				this.shakeBlock = true;
			}
		}
		else
		{
			this.pos = this.headJnt.position;
			if (this.vrAllowFollow())
			{
				this.thisTr.position = this.pos;
				if (this.setup.playerCam)
				{
					this.setup.playerCam.localPosition = new Vector3(this.playerCamPos.x, this.playerCamPos.y + this.offsetY, this.playerCamPos.z + this.offsetZ);
				}
			}
			else
			{
				this.thisTr.localPosition = this.startCamPos;
			}
		}
		this.doCamFollow();
		if (!this.flying && !this.followAnim && !this.lockXCam && !this.lockYCam && !this.smoothLock && !this.smoothUnLock && !LocalPlayer.AnimControl.useRootMotion && !LocalPlayer.AnimControl.endGameCutScene && !LocalPlayer.AnimControl.upsideDown && !LocalPlayer.AnimControl.onRope && !this.anim.isPlaying)
		{
			base.transform.localRotation = Quaternion.identity;
		}
	}

	private bool vrAllowFollow()
	{
		return !ForestVR.Enabled || (ForestVR.Enabled && (LocalPlayer.CamFollowHead.followAnim || LocalPlayer.AnimControl.endGameCutScene));
	}

	private void doCamFollow()
	{
		if (this.vrAllowFollow())
		{
			if (this.followAnim || this.lockYCam)
			{
				if (this.smoothLock)
				{
					Quaternion quaternion = this.camTr.rotation;
					quaternion = Quaternion.Slerp(quaternion, this.headJnt.rotation, Time.deltaTime * 5f);
					this.camTr.rotation = quaternion;
					if (this.camTr.rotation == this.headJnt.rotation)
					{
						this.smoothLock = false;
					}
				}
				else if (!this.smoothUnLock)
				{
					this.storePrevRot = false;
					this.thisTr.rotation = this.headJnt.rotation;
				}
				if (!this.smoothLock && !this.smoothUnLock)
				{
					this.thisTr.rotation = this.headJnt.rotation;
					this.camTr.localEulerAngles = new Vector3(0f, 0f, 0f);
				}
			}
			if (this.smoothUnLock)
			{
				this.thisTr.localRotation = Quaternion.Slerp(this.thisTr.localRotation, this.thisTr.parent.localRotation, Time.deltaTime * 2f);
				if (this.thisTr.localEulerAngles == Vector3.zero)
				{
					LocalPlayer.CamRotator.resetOriginalRotation = true;
					this.smoothUnLock = false;
					this.followAnim = false;
					LocalPlayer.CamRotator.enabled = true;
				}
			}
		}
	}

	public void disableFlying()
	{
		this.anim.Stop("camShakePlane");
		this.anim.Stop("lightTurbulance");
		this.flying = false;
		this.aisle = false;
		LocalPlayer.Transform.position = this.planePos.position;
		LocalPlayer.Transform.eulerAngles = new Vector3(LocalPlayer.Transform.eulerAngles.x, this.planePos.eulerAngles.y, LocalPlayer.Transform.eulerAngles.z);
		LocalPlayer.CamFollowHead.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
	}

	public void enableMouseControl(bool on)
	{
		if (!ForestVR.Enabled)
		{
			if (on)
			{
				this.mouse1.enabled = true;
			}
			else
			{
				this.mouse1.enabled = false;
				this.mouse2.enabled = false;
			}
		}
	}

	public void enableAisle()
	{
		this.anim.Stop("camShakePlane");
		this.flying = false;
		this.shakeBlock = false;
		this.aisle = true;
		if (BoltNetwork.isClient)
		{
			LocalPlayer.CamRotator.rotationRange = new Vector2(45f, 45f);
			LocalPlayer.MainRotator.enabled = false;
		}
		else
		{
			LocalPlayer.MainRotator.rotationRange = new Vector2(0f, 70f);
			LocalPlayer.CamRotator.rotationRange = new Vector2(95f, 0f);
		}
	}

	private TriggerCutScene cutScene;

	private GameObject planeCrashHull;

	public Transform headJnt;

	public float damp;

	private Transform thisTr;

	public Transform camTr;

	public Transform planePos;

	public Transform seatPos;

	public Transform aislePos;

	public Transform aislePosVR;

	public Transform clientPos;

	private playerScriptSetup setup;

	private playerHitReactions reactions;

	public SimpleMouseRotator mouse1;

	public SimpleMouseRotator mouse2;

	public bool oculus;

	public bool flying;

	public bool aisle;

	public bool shakeBlock;

	public bool followAnim;

	private Vector3 playerCamPos;

	private Vector3 ovrCamPos;

	private Quaternion prevRotation;

	private Vector3 startCamPos;

	public float offsetY;

	public float offsetZ;

	public bool lockXCam;

	public bool lockYCam;

	public bool smoothLock;

	public bool smoothUnLock;

	private bool storePrevRot;

	private Animation anim;

	private bool reparent;

	private Vector3 pos;

	public float introWeight = 0.1f;
}
