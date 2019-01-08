using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;

public class VRTheatreController : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			return;
		}
		if (this.SourceCamera == null)
		{
			this.SourceCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		}
		if (this.VignetteController == null)
		{
			this.VignetteController = LocalPlayer.GameObject.GetComponent<VRVignetteController>();
		}
		this.TheatreObject = Scene.SceneTracker.TheatreGo;
		this.TheatrePosTransform = this.TheatreObject.transform.Find("TheatreCamPos");
		this.DefaultLayerMask = LocalPlayer.MainCam.cullingMask;
		this.SwitchToGameMode();
	}

	private void Update()
	{
		if (ForestVR.Enabled)
		{
			if (LocalPlayer.vrPlayerControl.useGhostMode)
			{
				this.SwitchToTheatreMode();
				this.comfortMode = true;
			}
			else if (this.comfortMode)
			{
				this.SwitchToGameMode();
				this.comfortMode = false;
			}
			if (this.ghostFromTheatreMode || this.ghostFromIkDisable)
			{
				LocalPlayer.Animator.SetBoolReflected("ghostMode", true);
			}
			else
			{
				LocalPlayer.Animator.SetBoolReflected("ghostMode", false);
			}
		}
	}

	public void SwitchToTheatreMode()
	{
		if (!this.theatreOn)
		{
			float num = LocalPlayer.vrPlayerControl.VRCameraRig.localPosition.y;
			if (PlayerPreferences.VRUsePhysicalCrouching)
			{
				num = 0f;
			}
			this.ghostFromTheatreMode = true;
			LocalPlayer.vrAdapter.JointsOffsetTransform.parent = LocalPlayer.PlayerBase.transform;
			LocalPlayer.vrAdapter.JointsOffsetTransform.GetComponent<invertRotateConstrain>().enabled = false;
			LocalPlayer.vrAdapter.JointsOffsetTransform.localEulerAngles = Vector3.zero;
			LocalPlayer.vrAdapter.JointsOffsetTransform.localPosition = Vector3.zero;
			LocalPlayer.vrAdapter.VRCameraRig.transform.localPosition = new Vector3(LocalPlayer.vrAdapter.VRCameraRig.transform.localPosition.x, this.offsetRigHeight + num, LocalPlayer.vrAdapter.VRCameraRig.transform.localPosition.z);
			if (!this.steppedMode)
			{
				this.storeVrOffsetPos = LocalPlayer.vrPlayerControl.VROffsetTransform.localPosition;
				LocalPlayer.vrPlayerControl.VROffsetTransform.parent = null;
				LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles = new Vector3(0f, LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles.y, 0f);
			}
			this.TheatreObject.transform.position = LocalPlayer.PlayerBase.transform.position;
			this.TheatreObject.transform.rotation = Quaternion.LookRotation(LocalPlayer.vrPlayerControl.VRCamera.forward, Vector3.up);
			this.TheatreObject.transform.localEulerAngles = new Vector3(0f, this.TheatreObject.transform.localEulerAngles.y, 0f);
			foreach (GameObject gameObject in LocalPlayer.vrAdapter.PlayerMeshesToDisable)
			{
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(true);
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.shadowCastingMode = ShadowCastingMode.On;
					renderer.gameObject.layer = 18;
				}
			}
			foreach (Renderer renderer2 in LocalPlayer.vrAdapter.PlayerShadowOnly)
			{
				renderer2.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
			}
			LocalPlayer.vrAdapter.defaultPlayerArms.gameObject.SetActive(true);
			LocalPlayer.vrAdapter.VRPlayerHands.gameObject.SetActive(false);
			this._cameraOffset = LocalPlayer.vrPlayerControl.VRCamera.position;
			this._cameraOffset.y = LocalPlayer.vrPlayerControl.VROffsetTransform.position.y;
			this._cameraOffset -= LocalPlayer.vrPlayerControl.VROffsetTransform.position;
			this.theatreOn = true;
		}
		if (this.vrPos1 != null)
		{
			if ((this.steppedMode && Vector3.Distance(LocalPlayer.vrPlayerControl.VROffsetTransform.position, this.vrPos1.position) < this.SteppedModeUpdateDistance) || LocalPlayer.AnimControl.currLayerState0.tagHash == LocalPlayer.AnimControl.climbOutHash)
			{
				return;
			}
			LocalPlayer.vrPlayerControl.VROffsetTransform.position = this.vrPos1.position - this._cameraOffset;
			LocalPlayer.vrPlayerControl.VROffsetTransform.rotation = this.vrPos1.rotation;
		}
		else if (this.useCustomPosition)
		{
			LocalPlayer.vrPlayerControl.VROffsetTransform.position = this.customStandPos - this._cameraOffset;
		}
	}

	public void SwitchToGameMode()
	{
		if (this.theatreOn)
		{
			this.useCustomPosition = false;
			this.steppedMode = false;
			LocalPlayer.MainCam.cullingMask = this.DefaultLayerMask;
			this.ghostFromTheatreMode = false;
			LocalPlayer.vrPlayerControl.VROffsetTransform.parent = LocalPlayer.CamFollowHead.transform;
			LocalPlayer.vrPlayerControl.VROffsetTransform.localPosition = new Vector3(0f, -4.27549f, -0.206f);
			LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles = Vector3.zero;
			Vector3 position = LocalPlayer.vrPlayerControl.VRCamera.position;
			position.y = LocalPlayer.vrPlayerControl.VRCameraOrigin.position.y;
			Vector3 b = position - LocalPlayer.vrPlayerControl.VRCameraOrigin.position;
			LocalPlayer.vrPlayerControl.VRCameraRig.position = LocalPlayer.vrPlayerControl.VRCameraRig.position - b;
			if (PlayerPreferences.VRUsePhysicalCrouching)
			{
				LocalPlayer.vrAdapter.VRCameraRig.transform.localPosition = new Vector3(LocalPlayer.vrAdapter.VRCameraRig.transform.localPosition.x, this.offsetRigHeight, LocalPlayer.vrAdapter.VRCameraRig.transform.localPosition.z);
			}
			this.SourceCamera.forceIntoRenderTexture = false;
			this.SourceCamera.targetTexture = null;
			this.SourceCamera.gameObject.SetActive(false);
			this.TheatreObject.SetActive(false);
			foreach (GameObject gameObject in LocalPlayer.vrAdapter.PlayerMeshesToDisable)
			{
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(true);
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				}
			}
			LocalPlayer.vrAdapter.defaultPlayerArms.gameObject.SetActive(false);
			LocalPlayer.vrAdapter.VRPlayerHands.gameObject.SetActive(true);
			LocalPlayer.vrAdapter.VRPlayerHands.shadowCastingMode = ShadowCastingMode.On;
			LocalPlayer.vrAdapter.PlayerHead1.gameObject.layer = 27;
			LocalPlayer.vrAdapter.JointsOffsetTransform.parent = LocalPlayer.vrPlayerControl.VROffsetTransform;
			LocalPlayer.vrAdapter.JointsOffsetTransform.localPosition = Vector3.zero;
			LocalPlayer.vrAdapter.JointsOffsetTransform.GetComponent<invertRotateConstrain>().enabled = true;
			this.vrPos1 = null;
			this.vrPos2 = null;
			this.theatreOn = false;
		}
	}

	public void setCustomGhostPosition(Vector3 pos)
	{
		this.customStandPos = pos;
		this.useCustomPosition = true;
	}

	private void setVrStandPos1(Transform tr)
	{
		this.vrPos1 = tr;
	}

	private void setVrStandPos2(Transform tr)
	{
		this.vrPos2 = tr;
	}

	private void goToNextVrPos()
	{
		if (this.vrPos2 != null)
		{
			LocalPlayer.vrPlayerControl.VROffsetTransform.position = this.vrPos2.position;
		}
	}

	private void useSteppedGhostMode()
	{
		this.steppedMode = true;
		this.storeVrOffsetPos = LocalPlayer.vrPlayerControl.VROffsetTransform.localPosition;
		LocalPlayer.vrPlayerControl.VROffsetTransform.parent = null;
		LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles = new Vector3(0f, LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles.y, 0f);
	}

	public Camera SourceCamera;

	public GameObject TheatreObject;

	public RenderTexture TheatreRT;

	public Transform TheatrePosTransform;

	public Transform InventoryBone;

	public Transform vrPos1;

	public Transform vrPos2;

	public float SteppedModeUpdateDistance = 5f;

	public LayerMask TheatreCullingMask;

	public LayerMask DefaultLayerMask;

	public LayerMask OptionsMenuLayerMask;

	public VRVignetteController VignetteController;

	private Vector3 storeVrOffsetPos;

	public bool useCustomPosition;

	private Vector3 customStandPos;

	public float offsetRigHeight = 0.2f;

	public bool ghostFromIkDisable;

	public bool ghostFromTheatreMode;

	private Vector3 _cameraOffset;

	public bool theatreOn;

	private bool steppedMode;

	public bool comfortMode;
}
