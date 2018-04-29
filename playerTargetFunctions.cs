using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using HutongGames.PlayMaker;
using TheForest.Utils;
using UnityEngine;


public class playerTargetFunctions : MonoBehaviour
{
	
	private void Start()
	{
		this.setup = base.GetComponent<playerScriptSetup>();
		this.animator = base.GetComponent<Animator>();
		this.playerTr = base.transform;
		this.playerRoot = base.transform.root;
		this.pmControl = this.setup.pmControl;
		this.pmRotate = this.setup.pmRotate;
		this.fsmSmashPosLook = this.pmControl.FsmVariables.GetFsmVector3("smashPosLook");
		this.fsmTreePos = this.pmRotate.FsmVariables.GetFsmVector3("treeStandPos");
		if (!base.IsInvoking("sendStealthValues") && this.setup.sceneInfo)
		{
			base.InvokeRepeating("sendStealthValues", 1f, 1f);
		}
	}

	
	private void Awake()
	{
	}

	
	private void Update()
	{
		this.bushRange = 0f;
	}

	
	public void sendPlayerAttacking()
	{
		foreach (GameObject gameObject in this.setup.sceneInfo.visibleEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setPlayerAttacking", SendMessageOptions.DontRequireReceiver);
			}
		}
		if (BoltNetwork.isRunning && BoltNetwork.isClient)
		{
			playerSwingWeapon playerSwingWeapon = playerSwingWeapon.Create(GlobalTargets.OnlyServer);
			playerSwingWeapon.Send();
		}
	}

	
	public void syncPlayerAttack(int attackType)
	{
	}

	
	private void sendStealthValues()
	{
		if (Scene.SceneTracker == null)
		{
			return;
		}
		if (this.animator.GetBool("lighterHeld") || this.animator.GetBool("flashLightHeld"))
		{
			if (Clock.Dark && !LocalPlayer.IsInCaves)
			{
				this.sendLighterRange(70f);
			}
			else
			{
				this.sendLighterRange(50f);
			}
		}
		else
		{
			this.sendLighterRange(0f);
		}
		foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setBushRange", this.bushRange, SendMessageOptions.DontRequireReceiver);
			}
		}
		float stealthFinal = LocalPlayer.Stats.StealthFinal;
		this.sendMudRange(-stealthFinal);
	}

	
	private void findAllEnemies()
	{
		this.allEnemyGo = GameObject.FindGameObjectsWithTag("enemyRoot");
	}

	
	public void sendVisionRange(float range)
	{
		float num;
		if (LocalPlayer.IsInCaves)
		{
			num = range * 0.75f;
		}
		else
		{
			num = range;
		}
		this.visionRange = range;
		foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setVisionRange", num, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void sendLighterRange(float range)
	{
		if (Scene.SceneTracker == null)
		{
			return;
		}
		float num;
		if (LocalPlayer.IsInCaves)
		{
			num = range * 0.75f;
		}
		else
		{
			num = range;
		}
		this.lighterRange = range;
		foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setLighterRange", num, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void sendMudRange(float range)
	{
		float num;
		if (LocalPlayer.IsInCaves)
		{
			num = range * 0.75f;
		}
		else
		{
			num = range;
		}
		this.mudRange = range;
		foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setMudRange", num, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void enableCrouchLayers()
	{
		this.crouchRange = -12f;
		foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setcrouchRange", this.crouchRange, SendMessageOptions.DontRequireReceiver);
				gameObject.SendMessage("setVisionLayersOn", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	public void disableCrouchLayers()
	{
		this.crouchRange = 0f;
		foreach (GameObject gameObject in this.setup.sceneInfo.closeEnemies)
		{
			if (gameObject)
			{
				gameObject.SendMessage("setcrouchRange", this.crouchRange, SendMessageOptions.DontRequireReceiver);
			}
			if (gameObject)
			{
				gameObject.SendMessage("setVisionLayersOff", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	
	private IEnumerator enableAimAtTarget()
	{
		float val = 0f;
		float initVal = this.animator.GetFloat("aimAtTarget");
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / 1f;
			val = Mathf.SmoothStep(initVal, 1f, t);
			this.animator.SetFloatReflected("aimAtTarget", val);
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator disableAimAtTarget()
	{
		float initVal = this.animator.GetFloat("aimAtTarget");
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime / 1f;
			float val = Mathf.SmoothStep(initVal, 0f, t);
			this.animator.SetFloatReflected("aimAtTarget", val);
			yield return null;
		}
		yield break;
	}

	
	private IEnumerator setSmashPosition(Vector3 pos)
	{
		Vector3 dir = (this.playerTr.position - pos).normalized;
		Vector3 smashPos = pos + dir * 1.7f;
		this.fsmSmashPosLook.Value = smashPos;
		yield return null;
		yield break;
	}

	
	private IEnumerator setTreePosition(Vector3 pos)
	{
		Vector3 newPos = new Vector3(pos.x, this.playerRoot.position.y, pos.z);
		Vector3 dir = (this.playerRoot.position - newPos).normalized;
		Vector3 treePos = newPos + dir * 3.5f;
		this.fsmTreePos.Value = treePos;
		yield return null;
		yield break;
	}

	
	private IEnumerator moveToTreeTarget(Vector3 pos)
	{
		Vector3 newPos = new Vector3(pos.x, this.playerRoot.position.y, pos.z);
		Vector3 dir = (this.playerRoot.position - newPos).normalized;
		Vector3 treePos = newPos + dir * 3.3f;
		float t = 0f;
		float initX = this.playerRoot.position.x;
		float initZ = this.playerRoot.position.z;
		while ((double)t < 1.0)
		{
			t += Time.deltaTime * 2f;
			float newX = Mathf.SmoothStep(initX, treePos.x, t);
			float newZ = Mathf.SmoothStep(initZ, treePos.z, t);
			this.playerRoot.position = new Vector3(newX, this.playerRoot.position.y, newZ);
			yield return null;
		}
		yield break;
	}

	
	private PlayMakerFSM pmControl;

	
	private PlayMakerFSM pmRotate;

	
	private FsmVector3 fsmSmashPosLook;

	
	private FsmVector3 fsmTreePos;

	
	public playerScriptSetup setup;

	
	private Animator animator;

	
	private Transform playerTr;

	
	private Transform playerRoot;

	
	public float playerDist;

	
	private float tempVal;

	
	public GameObject[] allEnemyGo;

	
	public bool coveredInMud;

	
	public List<GameObject> visibleEnemies = new List<GameObject>();

	
	public float closeVisRange;

	
	public float longVisRange;

	
	public float visionRange;

	
	public float lighterRange;

	
	public float crouchRange;

	
	public float bushRange;

	
	private float mudRange;

	
	private float setCloseVisRange;

	
	private float setLongVisRange;

	
	public List<GameObject> currentAttackers = new List<GameObject>();
}
