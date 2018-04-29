using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.Utils;
using UnityEngine;


[DoNotSerializePublic]
public class PlaneCrashController : MonoBehaviour, IPlaneCrashController
{
	
	private void Start()
	{
		Scene.PlaneCrash = this;
	}

	
	public void DisablePlaneCrash()
	{
		this.ShowCrash = false;
		if (Scene.TriggerCutScene)
		{
			Scene.TriggerCutScene.StopSounds();
		}
		UnityEngine.Object.Destroy(Scene.TriggerCutScene.transform.parent.gameObject);
	}

	
	public IEnumerator InitPlaneCrashSequence()
	{
		while (Scene.LoadSave)
		{
			yield return null;
		}
		if (!this.Crashed && this.ShowCrash)
		{
			this.Crashed = true;
			if (Scene.PlaneCrashAnimGO && Scene.PlaneCrashAnimGO.activeSelf)
			{
				Scene.TriggerCutScene.StartCoroutine("beginPlaneCrash");
			}
		}
		yield break;
	}

	
	private void OnDeserialized()
	{
		if (SteamDSConfig.isDedicatedServer)
		{
			this.setupCrashedPlane();
		}
		else
		{
			base.Invoke("setupCrashedPlane", 0.3f);
		}
	}

	
	private void OnSerializing()
	{
		if (!this.spawnedHullPrefab)
		{
			this.spawnedHullPrefab = GameObject.FindGameObjectWithTag("planeCrash");
		}
		if (Vector3.Distance(this.planePosition, Vector3.zero) < 1f)
		{
			if (!this.savePos)
			{
				this.savePos = GameObject.FindGameObjectWithTag("savePlanePos").transform;
			}
			if (Vector3.Distance(this.savePos.position, this.spawnedHullPrefab.transform.position) > 0.1f)
			{
				this.savePos.position = this.spawnedHullPrefab.transform.position;
				this.savePos.rotation = this.spawnedHullPrefab.transform.rotation;
			}
			this.planePosition = this.savePos.position;
			this.planeRotation = this.savePos.rotation;
		}
	}

	
	private void setupCrashedPlane()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("savePlanePos");
		if (Vector3.Distance(this.planePosition, Vector3.zero) > 1f)
		{
			if (!gameObject)
			{
				gameObject = new GameObject("PlanePos");
				gameObject.transform.position = this.planePosition;
				gameObject.transform.rotation = this.planeRotation;
			}
			else if (gameObject.GetComponent<EmptyObjectIdentifier>())
			{
				gameObject.transform.position = this.planePosition;
				gameObject.transform.rotation = this.planeRotation;
			}
		}
		if (gameObject)
		{
			this.setupCrashedPlane(gameObject.transform);
		}
	}

	
	public void SetupCrashedPlane_MP()
	{
		Transform transform = new GameObject("MPHostPlanePos").transform;
		transform.position = CoopServerInfo.Instance.state.PlanePosition;
		transform.rotation = CoopServerInfo.Instance.state.PlaneRotation;
		this.setupCrashedPlane(transform);
	}

	
	private void setupCrashedPlane(Transform t)
	{
		if (Scene.PlaneCrashAnimGO)
		{
			UnityEngine.Object.Destroy(Scene.PlaneCrashAnimGO);
		}
		this.savePos = t;
		if (this.savePos)
		{
			base.Invoke("loadCrashPlane", 0.15f);
			Debug.Log("fake plane loaded");
		}
	}

	
	private void loadCrashPlane()
	{
		this.spawnedHullPrefab = UnityEngine.Object.Instantiate<GameObject>(this.savedHullPrefab, this.savePos.position, this.savePos.rotation);
		if (!CoopPeerStarter.DedicatedHost && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash)
		{
			LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.Loading;
		}
		this.fakePlaneActive = true;
		Scene.PlaneGreebles.transform.position = this.savePos.position;
		Scene.PlaneGreebles.transform.rotation = this.savePos.rotation;
		Scene.PlaneGreebles.SetActive(true);
		if (!BoltNetwork.isRunning || BoltNetwork.isClient)
		{
		}
	}

	
	public GameObject savedHullPrefab;

	
	public GameObject spawnedHullPrefab;

	
	[HideInInspector]
	public Transform savePos;

	
	[SerializeThis]
	public bool Crashed;

	
	public bool ShowCrash;

	
	public bool fakePlaneActive;

	
	public bool doneHidePlayer;

	
	[SerializeThis]
	private Vector3 planePosition = Vector3.zero;

	
	[SerializeThis]
	private Quaternion planeRotation;
}
