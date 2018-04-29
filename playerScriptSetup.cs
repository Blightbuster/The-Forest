using System;
using TheForest.Utils;
using UnityEngine;


public class playerScriptSetup : MonoBehaviour
{
	
	private void Awake()
	{
		this.stats = base.transform.parent.GetComponent<PlayerStats>();
		this.targetInfo = base.transform.parent.GetComponent<targetStats>();
		this.sceneInfo = Scene.SceneTracker;
		playerScriptSetup.playerTransform = base.transform;
		this.targetFunctions = base.GetComponent<playerTargetFunctions>();
		this.events = base.transform.GetComponent<animEventsManager>();
		this.forceLocalPos = base.transform.GetComponentInChildren<ForceLocalPosZero>();
		this.treeHit = base.transform.GetComponentInChildren<treeHitTrigger>();
		this.hashName = Animator.StringToHash(base.transform.root.name);
		PlayMakerFSM[] components = base.transform.GetComponents<PlayMakerFSM>();
		foreach (PlayMakerFSM playMakerFSM in components)
		{
			if (playMakerFSM.FsmName == "rotatePlayerFSM")
			{
				this.pmRotate = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "controlFSM")
			{
				this.pmControl = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "targetManagerFSM")
			{
				this.pmTarget = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "staminaFSM")
			{
				this.pmStamina = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "noiseDetectFSM")
			{
				this.pmNoise = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "damageFSM")
			{
				this.pmDamage = playMakerFSM;
			}
			if (playMakerFSM.FsmName == "tempBlockManagerFSM")
			{
				this.pmBlock = playMakerFSM;
			}
		}
		Transform[] componentsInChildren = base.transform.root.gameObject.GetComponentsInChildren<Transform>(true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == "char_Spine")
			{
				this.spine1 = transform;
			}
			if (transform.name == "char_Spine1")
			{
				this.spine2 = transform;
			}
			if (transform.name == "char_Spine2")
			{
				this.spine3 = transform;
			}
			if (transform.name == "MainCamNew")
			{
				this.playerCam = transform;
			}
			if (transform.name == "OVRCameraController")
			{
				this.OvrCam = transform;
			}
			if (transform.name == "LookObject")
			{
				this.camParent = transform;
			}
			if (transform.name == "char_Head1")
			{
				this.headJnt = transform;
			}
			if (transform.name == "player_BASE")
			{
				this.playerBase = transform.gameObject;
			}
			if (transform.name == "lookAtGo")
			{
				this.lookAtTr = transform;
			}
			if (transform.name == "playerHitDetect")
			{
				this.bodyCollisionGo = transform.gameObject;
			}
			if (transform.name == "char_RightHandWeapon")
			{
				this.weaponRight = transform;
			}
			if (transform.name == "char_LeftHand1")
			{
				this.leftHand = transform;
			}
			if (transform.name == "char_LeftArm")
			{
				this.leftArm = transform;
			}
			if (transform.name == "char_RightArm")
			{
				this.rightArm = transform;
			}
			if (transform.name == "char_Neck")
			{
				this.neckJnt = transform;
			}
			if (transform.name == "char_Hips")
			{
				this.hipsJnt = transform;
			}
			if (transform.name == "leftHandHeld")
			{
				this.leftHandHeld = transform;
			}
			if (transform.name == "rightHandHeld")
			{
				this.rightHandHeld = transform;
			}
			if (transform.name == "char_RightHand")
			{
				this.rightHandWrist = transform;
			}
			if (transform.name == "char_LeftHand")
			{
				this.leftHandWrist = transform;
			}
			if (transform.name == "smallBird_ANIM_landOnFinger")
			{
				this.smallBirdGo = transform.gameObject;
			}
			if (transform.name == "soundDetectGo")
			{
				this.soundDetectGo = transform.gameObject;
			}
			if (transform.name == "enemyBlocker")
			{
				this.enemyBlockerGo = transform.gameObject;
			}
			if (transform.name == "LogHeld1")
			{
				this.heldLog1 = transform.gameObject;
			}
			if (transform.name == "LogHeld2")
			{
				this.heldLog2 = transform.gameObject;
			}
		}
	}

	
	private void Start()
	{
		this.pmRotate.FsmVariables.GetFsmGameObject("lookAtGO").Value = this.lookAtTr.gameObject;
		this.pmControl.FsmVariables.GetFsmGameObject("smallBirdGO").Value = this.smallBirdGo;
		this.pmDamage.FsmVariables.GetFsmGameObject("bodyCollision").Value = this.bodyCollisionGo;
		this.pmControl.FsmVariables.GetFsmGameObject("playerRoot").Value = base.transform.root.gameObject;
		this.pmControl.FsmVariables.GetFsmGameObject("camGo").Value = LocalPlayer.MainCam.gameObject;
		if (this.inventoryGo)
		{
			this.pmControl.FsmVariables.GetFsmGameObject("inventoryGo").Value = this.inventoryGo;
		}
		if (!ForestVR.Prototype && !this.sceneInfo.allPlayers.Contains(base.transform.parent.gameObject))
		{
			this.sceneInfo.allPlayers.Add(base.transform.parent.gameObject);
		}
		base.Invoke("checkPlaneCrash", 1.5f);
		base.Invoke("unParentPlayerBase", 1.5f);
	}

	
	private void unParentPlayerBase()
	{
	}

	
	private void checkPlaneCrash()
	{
		GameObject gameObject = GameObject.FindWithTag("planeCrash");
		if (gameObject == null)
		{
			gameObject = new GameObject();
			gameObject.transform.position = base.transform.position;
			gameObject.tag = "planeCrash";
		}
	}

	
	private void OnDisable()
	{
		if (base.transform.parent && this.sceneInfo.allPlayers.Contains(base.transform.parent.gameObject))
		{
			this.sceneInfo.allPlayers.Remove(base.transform.parent.gameObject);
		}
	}

	
	public playerTargetFunctions targetFunctions;

	
	public sceneTracker sceneInfo;

	
	public SimpleMouseRotator mainRotator;

	
	public PlayerStats stats;

	
	public targetStats targetInfo;

	
	public animEventsManager events;

	
	public ForceLocalPosZero forceLocalPos;

	
	public treeHitTrigger treeHit;

	
	public PlayMakerFSM pmRotate;

	
	public PlayMakerFSM pmControl;

	
	public PlayMakerFSM pmTarget;

	
	public PlayMakerFSM pmStamina;

	
	public PlayMakerFSM pmNoise;

	
	public PlayMakerFSM pmDamage;

	
	public PlayMakerFSM pmBlock;

	
	public static Transform playerTransform;

	
	public Transform weaponRight;

	
	public Transform constrainTr;

	
	public Transform lookAtTr;

	
	public Transform spine1;

	
	public Transform spine2;

	
	public Transform spine3;

	
	public Transform playerCam;

	
	public Transform OvrCam;

	
	public Transform camParent;

	
	public Transform hipsJnt;

	
	public Transform headJnt;

	
	public Transform neckJnt;

	
	public Transform leftHand;

	
	public Transform leftHandHeld;

	
	public Transform rightHandHeld;

	
	public Transform leftHandWrist;

	
	public Transform leftArm;

	
	public Transform rightArm;

	
	public Transform rightHandWrist;

	
	public GameObject weaponCube;

	
	public GameObject playerBase;

	
	public GameObject smallBirdGo;

	
	public GameObject threatRangeGo;

	
	public GameObject inventoryGo;

	
	public GameObject bodyCollisionGo;

	
	public GameObject soundDetectGo;

	
	public GameObject enemyBlockerGo;

	
	public GameObject heldLog1;

	
	public GameObject heldLog2;

	
	public GameObject heldBow;

	
	public GameObject heldBowTip;

	
	public GameObject heldModernBow;

	
	public GameObject heldModernBowTip;

	
	public int hashName;
}
