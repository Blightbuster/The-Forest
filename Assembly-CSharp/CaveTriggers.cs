using System;
using System.Collections;
using System.Collections.Generic;
using TheForest.Utils;
using UnityEngine;

public class CaveTriggers : MonoBehaviour
{
	private void Awake()
	{
		this.caveManager = base.transform.GetComponent<caveEntranceManager>();
		base.enabled = false;
	}

	private void Update()
	{
		for (int i = this.playersEnteringTrigger.Count - 1; i >= 0; i--)
		{
			Transform transform = this.playersEnteringTrigger[i];
			if (transform == null)
			{
				this.playersEnteringTrigger.RemoveAt(i);
				return;
			}
			Vector3 position = base.transform.InverseTransformPoint(transform.position);
			position.x = 0f;
			position.y = 0f;
			Vector3 a = base.transform.TransformPoint(position);
			float num = Vector3.Distance(a, base.transform.position);
			float num2 = 4.9f;
			if (this.climbEntrance)
			{
				num2 = 0.3f;
			}
			if (num < num2)
			{
				this.playersEnteringTrigger.RemoveAt(i);
				this.playersExitingTrigger.Add(transform);
				if (transform == LocalPlayer.Transform && position.z < 0f)
				{
					LocalPlayer.ActiveAreaInfo.SetCurrentCave(this.ForwardCaveNum);
					if (this.climbEntrance && this.caveManager)
					{
						this.caveManager.StartCoroutine("disableCaveBlackRoutine");
					}
				}
				if (!this.IsOutsideArea)
				{
					if (position.z < 0f)
					{
						transform.gameObject.SendMessage("InACave", SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (transform == LocalPlayer.Transform)
				{
					LocalPlayer.Stats.IgnoreCollisionWithTerrain(true);
				}
			}
			else if (Vector3.Distance(transform.position, base.transform.position) > 7f)
			{
				this.playersEnteringTrigger.RemoveAt(i);
			}
		}
		for (int j = this.playersExitingTrigger.Count - 1; j >= 0; j--)
		{
			Transform transform2 = this.playersExitingTrigger[j];
			if (transform2 == null)
			{
				this.playersExitingTrigger.RemoveAt(j);
				return;
			}
			Vector3 position2 = base.transform.InverseTransformPoint(transform2.position);
			position2.x = 0f;
			position2.y = 0f;
			Vector3 a2 = base.transform.TransformPoint(position2);
			float num3 = Vector3.Distance(a2, base.transform.position);
			bool flag = false;
			if (transform2 == LocalPlayer.Transform && this.climbEntrance && num3 > 2f)
			{
				flag = true;
			}
			if (num3 > 6.48f || flag)
			{
				if (flag)
				{
					if ((double)num3 > 6.48)
					{
						this.playersExitingTrigger.RemoveAt(j);
					}
				}
				else
				{
					this.playersExitingTrigger.RemoveAt(j);
				}
				if (transform2 == LocalPlayer.Transform && position2.z < 0f)
				{
					LocalPlayer.ActiveAreaInfo.SetCurrentCave(this.BackwardCaveNum);
				}
				if (!this.IsOutsideArea && position2.z < 0f)
				{
					if (this.climbEntrance && this.caveManager && transform2 == LocalPlayer.Transform)
					{
						this.caveManager.StartCoroutine("enableCaveBlackRoutine");
					}
					transform2.SendMessage("NotInACave", SendMessageOptions.DontRequireReceiver);
				}
				if (transform2 == LocalPlayer.Transform && position2.z < 0f)
				{
					LocalPlayer.Stats.IgnoreCollisionWithTerrain(false);
				}
			}
		}
		if (this.playersEnteringTrigger.Count == 0 && this.playersExitingTrigger.Count == 0)
		{
			base.enabled = false;
		}
		CaveTriggers.CheckPlayersInCave();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!this.IsEntryControlled && other.gameObject.tag.StartsWith("Player") && !this.playersEnteringTrigger.Contains(other.transform) && !this.playersExitingTrigger.Contains(other.transform))
		{
			base.transform.InverseTransformPoint(other.transform.position).y = 0f;
			base.enabled = true;
			this.playersEnteringTrigger.Add(other.transform);
		}
		if (other.gameObject.CompareTag("PlayerNet") && !this.playersEnteringTrigger.Contains(other.transform) && !this.playersExitingTrigger.Contains(other.transform))
		{
			base.transform.InverseTransformPoint(other.transform.position).y = 0f;
			base.enabled = true;
			this.playersEnteringTrigger.Add(other.transform);
		}
		if (this.IsOutsideArea && (other.gameObject.CompareTag("Multisled") || other.gameObject.CompareTag("enemyRoot")))
		{
			other.SendMessage("killEnemyInSinkHole", SendMessageOptions.DontRequireReceiver);
			TerrainCollider terrainCollider = null;
			if (Terrain.activeTerrain)
			{
				terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();
			}
			if (!terrainCollider)
			{
				return;
			}
			Physics.IgnoreCollision(terrainCollider, other, true);
		}
	}

	private IEnumerator CaveDoorRoutine()
	{
		Vector3 localPosition = base.transform.InverseTransformPoint(LocalPlayer.Transform.position);
		bool enteringCave = localPosition.z < 0f;
		localPosition.z *= ((!enteringCave) ? -1.5f : -1.25f);
		Scene.HudGui.ShowHud(false);
		Scene.HudGui.Loading._cam.SetActive(true);
		Scene.HudGui.Loading._message.SetActive(false);
		Scene.HudGui.Loading._backgroundTween.PlayForward();
		yield return YieldPresets.WaitPointFiveSeconds;
		LocalPlayer.GameObject.SendMessage((!enteringCave) ? "NotInACave" : "InACave", SendMessageOptions.DontRequireReceiver);
		if (!LocalPlayer.AnimControl.onRope)
		{
			LocalPlayer.Transform.position = base.transform.TransformPoint(localPosition);
		}
		yield return YieldPresets.WaitPointFiveSeconds;
		Scene.HudGui.Loading._backgroundTween.PlayReverse();
		yield return YieldPresets.WaitPointFiveSeconds;
		Scene.HudGui.Loading._cam.SetActive(false);
		Scene.HudGui.Loading._message.SetActive(false);
		Scene.HudGui.CheckHudState();
		yield break;
	}

	public static void CheckPlayersInCave()
	{
		if (Scene.CaveGrounds != null)
		{
			bool flag = LocalPlayer.IsInCaves || Scene.SceneTracker.allPlayersInCave.Count > 0;
			if (!flag && LocalPlayer.Transform)
			{
				float num = Vector3.Distance(new Vector3(Scene.SinkHoleCenter.position.x, LocalPlayer.Transform.position.y, Scene.SinkHoleCenter.position.z), LocalPlayer.Transform.position);
				flag = (num < 190f || (num < 230f && LocalPlayer.Transform.position.y < Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position)));
			}
			for (int i = 0; i < Scene.CaveGrounds.Length; i++)
			{
				GameObject gameObject = Scene.CaveGrounds[i];
				if (gameObject && gameObject.activeSelf != flag)
				{
					gameObject.SetActive(flag);
				}
			}
		}
	}

	private caveEntranceManager caveManager;

	public bool IsEntryControlled;

	public bool IsOutsideArea;

	public bool IsInnerArea;

	public CaveNames ForwardCaveNum = CaveNames.NotInCaves;

	public CaveNames BackwardCaveNum = CaveNames.NotInCaves;

	public List<Transform> playersEnteringTrigger = new List<Transform>();

	public List<Transform> playersExitingTrigger = new List<Transform>();

	public bool climbEntrance;

	private int climbOutHash = Animator.StringToHash("climbToIdle");
}
