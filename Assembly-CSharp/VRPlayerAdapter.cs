using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;

public class VRPlayerAdapter : MonoBehaviour
{
	private void Awake()
	{
		this.TheatreController = base.transform.GetComponent<VRTheatreController>();
	}

	private void Start()
	{
		this.ConvertToVR();
	}

	private void ConvertToVR()
	{
		if (ForestVR.Enabled)
		{
			Debug.LogWarning("ENABLING VR");
			for (int i = 0; i < this.MainCamera.transform.childCount; i++)
			{
				this.MainCamera.transform.GetChild(i).SetParent(this.cameraChildOffsetTranform, false);
			}
			this.mouthPiece.transform.parent = this.VREyeCamera.transform;
			this.mouthPiece.transform.localEulerAngles = Vector3.zero;
			this.mouthPiece.transform.localPosition = new Vector3(0f, -1.78f, 0.008f);
			AudioListener componentInChildren = this.MainCamera.GetComponentInChildren<AudioListener>(true);
			if (componentInChildren)
			{
				UnityEngine.Object.Destroy(componentInChildren);
			}
			FMOD_Listener componentInChildren2 = this.MainCamera.GetComponentInChildren<FMOD_Listener>(true);
			if (componentInChildren2)
			{
				UnityEngine.Object.Destroy(componentInChildren2);
			}
			this.MainCamera.SetActive(false);
			this.VRCameraRig.SetActive(true);
			this.Player.GetComponent<FirstPersonHeadBob>().enabled = false;
			this.Player.GetComponent<SimpleMouseRotator>().enabled = false;
			camFollowHead componentInChildren3 = this.PlayerBase.GetComponentInChildren<camFollowHead>(true);
			foreach (GameObject gameObject in this.PlayerMeshesToDisable)
			{
				SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
				{
					skinnedMeshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
				}
			}
			CoopArmorReplicator component = LocalPlayer.Transform.GetComponent<CoopArmorReplicator>();
			foreach (GameObject gameObject2 in component.Bones)
			{
				MeshRenderer component2 = gameObject2.GetComponent<MeshRenderer>();
				if (component2)
				{
					component2.enabled = false;
				}
			}
			this.VRPlayerHands.gameObject.SetActive(true);
			this.VRPlayerHands.shadowCastingMode = ShadowCastingMode.On;
			this.defaultPlayerArms.gameObject.SetActive(false);
			LocalPlayer.ScriptSetup.pmControl.FsmVariables.GetFsmBool("vrBool").Value = true;
			this.JointsOffsetTransform.parent = LocalPlayer.vrPlayerControl.VROffsetTransform;
			this.heldLogsParentTransform.parent = LocalPlayer.vrAdapter.VREyeCamera.transform;
			this.heldLogsParentTransform.localPosition = new Vector3(0.053f, -0.409f, -0.0181f);
			this.heldLogsParentTransform.localEulerAngles = new Vector3(9.855f, -1.221f, 0.205f);
			this.grassDisplacementGo.SetActive(false);
			this.heldDeadBodyGo.transform.localPosition = new Vector3(1.22f, -2.99f, 0.49f);
		}
		else
		{
			this.VRWatchParentGo.SetActive(false);
			this.HeightScaleGo.SetActive(false);
		}
	}

	public VRTheatreController TheatreController;

	public GameObject MainCamera;

	public GameObject VRCameraRig;

	public GameObject VREyeCamera;

	public GameObject Player;

	public GameObject PlayerBase;

	public GameObject cameraChildren;

	public GameObject mouthPiece;

	public GameObject RightWeaponOffset;

	public GameObject VRWatchGo;

	public GameObject VRWatchParentGo;

	public GameObject[] tempHands;

	public GameObject grassDisplacementGo;

	public GameObject heldDeadBodyGo;

	public GameObject HeightScaleGo;

	public GameObject HeadHeightScaleGo;

	public Transform overShoulderCamPos;

	public Transform dragAwayCamPos;

	public Transform behindCamPos;

	public Transform behindClientCamPos;

	public Transform JointsOffsetTransform;

	public Transform heldLogsParentTransform;

	public Transform cameraChildOffsetTranform;

	public bool BindCamHead;

	public bool BindNewHeight;

	public bool BindScale;

	public GameObject[] PlayerMeshesToDisable;

	public Renderer PlayerHead1;

	public Renderer VRPlayerHands;

	public Renderer defaultPlayerArms;

	public Renderer[] PlayerShadowOnly;

	public LayerMask OptionsMenuLayerMask;
}
