using System;
using UnityEngine;
using UnityEngine.VR;


public class OVRSceneSampleController : MonoBehaviour
{
	
	private void Awake()
	{
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRMainMenu: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRMainMenu: More then 1 OVRCameraRig attached.");
		}
		else
		{
			this.cameraController = componentsInChildren[0];
		}
		OVRPlayerController[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<OVRPlayerController>();
		if (componentsInChildren2.Length == 0)
		{
			Debug.LogWarning("OVRMainMenu: No OVRPlayerController attached.");
		}
		else if (componentsInChildren2.Length > 1)
		{
			Debug.LogWarning("OVRMainMenu: More then 1 OVRPlayerController attached.");
		}
		else
		{
			this.playerController = componentsInChildren2[0];
		}
	}

	
	private void Start()
	{
		if (!Application.isEditor)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		if (this.cameraController != null)
		{
			this.gridCube = base.gameObject.AddComponent<OVRGridCube>();
			this.gridCube.SetOVRCameraController(ref this.cameraController);
		}
	}

	
	private void Update()
	{
		this.UpdateRecenterPose();
		this.UpdateVisionMode();
		if (this.playerController != null)
		{
			this.UpdateSpeedAndRotationScaleMultiplier();
		}
		if (Input.GetKeyDown(KeyCode.F11))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			VRSettings.showDeviceView = !VRSettings.showDeviceView;
		}
		if (Input.GetKeyDown(this.quitKey))
		{
			Application.Quit();
		}
	}

	
	private void UpdateVisionMode()
	{
		if (Input.GetKeyDown(KeyCode.F2))
		{
			this.visionMode ^= this.visionMode;
			OVRManager.tracker.isEnabled = this.visionMode;
		}
	}

	
	private void UpdateSpeedAndRotationScaleMultiplier()
	{
		float num = 0f;
		this.playerController.GetMoveScaleMultiplier(ref num);
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			num -= this.speedRotationIncrement;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			num += this.speedRotationIncrement;
		}
		this.playerController.SetMoveScaleMultiplier(num);
		float num2 = 0f;
		this.playerController.GetRotationScaleMultiplier(ref num2);
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			num2 -= this.speedRotationIncrement;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			num2 += this.speedRotationIncrement;
		}
		this.playerController.SetRotationScaleMultiplier(num2);
	}

	
	private void UpdateRecenterPose()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			OVRManager.display.RecenterPose();
		}
	}

	
	public KeyCode quitKey = KeyCode.Escape;

	
	public Texture fadeInTexture;

	
	public float speedRotationIncrement = 0.05f;

	
	private OVRPlayerController playerController;

	
	private OVRCameraRig cameraController;

	
	public string layerName = "Default";

	
	private bool visionMode = true;

	
	private OVRGridCube gridCube;
}
