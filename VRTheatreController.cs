using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;


public class VRTheatreController : MonoBehaviour
{
	
	private void Start()
	{
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
		}
	}

	
	public void SwitchToTheatreMode()
	{
		if (!this.theatreOn)
		{
			this.SourceCamera.forceIntoRenderTexture = true;
			this.SourceCamera.targetTexture = this.TheatreRT;
			this.SourceCamera.gameObject.SetActive(true);
			this.storeVrOffsetPos = LocalPlayer.vrPlayerControl.VROffsetTransform.localPosition;
			LocalPlayer.vrPlayerControl.VROffsetTransform.parent = null;
			LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles = new Vector3(0f, LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles.y, 0f);
			this.TheatreObject.transform.position = LocalPlayer.PlayerBase.transform.position;
			this.TheatreObject.transform.rotation = Quaternion.LookRotation(LocalPlayer.vrPlayerControl.VRCamera.forward, Vector3.up);
			this.TheatreObject.transform.localEulerAngles = new Vector3(0f, this.TheatreObject.transform.localEulerAngles.y, 0f);
			IEnumerator enumerator = LocalPlayer.vrAdapter.PlayerMeshesToDisable.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					SkinnedMeshRenderer component = transform.GetComponent<SkinnedMeshRenderer>();
					if (component)
					{
						component.shadowCastingMode = ShadowCastingMode.On;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			LocalPlayer.vrAdapter.defaultPlayerArms.gameObject.SetActive(true);
			LocalPlayer.vrAdapter.VRPlayerHands.gameObject.SetActive(false);
			if (!LocalPlayer.AnimControl.skinningAnimal && !LocalPlayer.AnimControl.endGameCutScene)
			{
				LocalPlayer.vrPlayerControl.VRCameraRig.position = this.TheatrePosTransform.position;
			}
			if (this.vrPos1 != null)
			{
				LocalPlayer.vrPlayerControl.VRCameraRig.position = this.vrPos1.position;
			}
			else if (this.useCustomPosition)
			{
				LocalPlayer.vrPlayerControl.VRCameraRig.position = this.customStandPos;
			}
			this.theatreOn = true;
		}
	}

	
	public void SwitchToGameMode()
	{
		if (this.theatreOn)
		{
			LocalPlayer.MainCam.cullingMask = this.DefaultLayerMask;
			LocalPlayer.vrPlayerControl.VROffsetTransform.parent = LocalPlayer.CamFollowHead.transform;
			LocalPlayer.vrPlayerControl.VROffsetTransform.localPosition = this.storeVrOffsetPos;
			LocalPlayer.vrPlayerControl.VROffsetTransform.localEulerAngles = Vector3.zero;
			this.SourceCamera.forceIntoRenderTexture = false;
			this.SourceCamera.targetTexture = null;
			this.SourceCamera.gameObject.SetActive(false);
			this.TheatreObject.SetActive(false);
			IEnumerator enumerator = LocalPlayer.vrAdapter.PlayerMeshesToDisable.transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					SkinnedMeshRenderer component = transform.GetComponent<SkinnedMeshRenderer>();
					if (component)
					{
						component.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			LocalPlayer.vrAdapter.defaultPlayerArms.gameObject.SetActive(false);
			LocalPlayer.vrAdapter.VRPlayerHands.gameObject.SetActive(true);
			LocalPlayer.vrAdapter.VRPlayerHands.shadowCastingMode = ShadowCastingMode.On;
			LocalPlayer.vrAdapter.PlayerHead1.gameObject.layer = 27;
			this.useCustomPosition = false;
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
			LocalPlayer.vrPlayerControl.VRCameraRig.position = this.vrPos2.position;
		}
	}

	
	public Camera SourceCamera;

	
	public GameObject TheatreObject;

	
	public RenderTexture TheatreRT;

	
	public Transform TheatrePosTransform;

	
	public Transform InventoryBone;

	
	public Transform vrPos1;

	
	public Transform vrPos2;

	
	public LayerMask TheatreCullingMask;

	
	private LayerMask DefaultLayerMask;

	
	public VRVignetteController VignetteController;

	
	private Vector3 storeVrOffsetPos;

	
	public bool useCustomPosition;

	
	private Vector3 customStandPos;

	
	private bool theatreOn;

	
	private bool comfortMode;
}
