using System;
using System.Collections;
using PathologicalGames;
using TheForest.Buildings.Creation;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;

public class activateZipLine : MonoBehaviour
{
	private void Start()
	{
		base.enabled = false;
	}

	private bool playerCanGrabZip()
	{
		return !CoopPeerStarter.DedicatedHost && !LocalPlayer.AnimControl.onRope && !LocalPlayer.FpCharacter.Diving && !LocalPlayer.FpCharacter.drinking && !LocalPlayer.AnimControl.skinningAnimal;
	}

	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.FpCharacter.Sitting)
		{
			base.enabled = true;
			this.ToggleIcons((!LocalPlayer.Inventory.Logs.HasLogs) ? activateZipLine.ZiplineIcons.PlayerPickup : activateZipLine.ZiplineIcons.LogPickup);
			if (LocalPlayer.Inventory.Logs.HasLogs)
			{
				LocalPlayer.Inventory.DontShowDrop = true;
			}
		}
	}

	private void GrabExit(GameObject grabber)
	{
		if (base.enabled)
		{
			base.enabled = false;
			this.ToggleIcons(activateZipLine.ZiplineIcons.PlayerSheen);
			LocalPlayer.Inventory.DontShowDrop = false;
		}
	}

	private void Update()
	{
		if (LocalPlayer.Inventory.Logs.HasLogs)
		{
			this.PutLogOnZiplineUpdate();
		}
		else
		{
			this.GrabZiplineUpdate();
		}
	}

	private void PutLogOnZiplineUpdate()
	{
		this.ToggleIcons(activateZipLine.ZiplineIcons.LogPickup);
		if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.Logs.PutDown(false, false, true, null))
		{
			GameObject gameObject;
			if (!BoltNetwork.isRunning)
			{
				gameObject = PoolManager.Pools["misc"].Spawn(Prefabs.Instance.ZiplineLog, base.transform.position + base.transform.forward * this.logSpawnDistance, base.transform.rotation).gameObject;
			}
			else
			{
				gameObject = BoltNetwork.Instantiate(Prefabs.Instance.ZiplineLog.gameObject, base.transform.position + base.transform.forward * this.logSpawnDistance, base.transform.rotation).gameObject;
			}
			if (gameObject)
			{
				ZiplineTransportation component = gameObject.GetComponent<ZiplineTransportation>();
				component._targetTr = base.GetComponentInParent<ZiplineArchitect>()._exitTrigger;
			}
			this.ToggleIcons(activateZipLine.ZiplineIcons.None);
		}
	}

	private void GrabZiplineUpdate()
	{
		if (!this.playerCanGrabZip())
		{
			this.ToggleIcons(activateZipLine.ZiplineIcons.None);
			return;
		}
		if (LocalPlayer.FpCharacter.Sitting)
		{
			this.ToggleIcons(activateZipLine.ZiplineIcons.PlayerSheen);
			base.enabled = false;
			return;
		}
		if (!this.validateTriggerForCoop())
		{
			this.ToggleIcons(activateZipLine.ZiplineIcons.PlayerSheen);
			return;
		}
		this.ToggleIcons(activateZipLine.ZiplineIcons.PlayerPickup);
		if (LocalPlayer.FpCharacter.PushingSled)
		{
			return;
		}
		if (TheForest.Utils.Input.GetButtonDown("Take") && !LocalPlayer.FpCharacter.Sitting && !LocalPlayer.AnimControl.onRope)
		{
			LocalPlayer.SpecialActions.SendMessage("EnterZipLine", base.transform);
			this.ToggleIcons(activateZipLine.ZiplineIcons.None);
		}
	}

	private bool validateTriggerForCoop()
	{
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			targetStats component = Scene.SceneTracker.allPlayers[i].GetComponent<targetStats>();
			if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet") && component && component.onRope)
			{
				float sqrMagnitude = (Scene.SceneTracker.allPlayers[i].transform.position - base.transform.position).sqrMagnitude;
				if (sqrMagnitude < 6.25f)
				{
					return false;
				}
			}
		}
		return true;
	}

	private IEnumerator validatePlayerActivate()
	{
		float t = 0f;
		while (t < 1f)
		{
			if (!this.validateTriggerForCoop())
			{
				LocalPlayer.SpecialActions.SendMessage("fixClimbingPosition");
				yield break;
			}
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	private void ToggleIcons(activateZipLine.ZiplineIcons icon)
	{
		if (this.Sheen.activeSelf != (icon == activateZipLine.ZiplineIcons.PlayerSheen))
		{
			this.Sheen.SetActive(icon == activateZipLine.ZiplineIcons.PlayerSheen);
		}
		if (this.MyPickUp.activeSelf != (icon == activateZipLine.ZiplineIcons.PlayerPickup))
		{
			this.MyPickUp.SetActive(icon == activateZipLine.ZiplineIcons.PlayerPickup);
		}
		if (this.LogPickUp.activeSelf != (icon == activateZipLine.ZiplineIcons.LogPickup))
		{
			this.LogPickUp.SetActive(icon == activateZipLine.ZiplineIcons.LogPickup);
		}
	}

	public GameObject Sheen;

	public GameObject MyPickUp;

	public GameObject LogPickUp;

	public global::Types climbType;

	public float logSpawnDistance = 2f;

	public enum ZiplineIcons
	{
		None,
		PlayerSheen,
		PlayerPickup,
		LogSheen,
		LogPickup
	}
}
