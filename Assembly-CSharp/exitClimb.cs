using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class exitClimb : MonoBehaviour
{
	private void Start()
	{
		this.collider = base.transform.GetComponent<BoxCollider>();
		this.collider.enabled = true;
		this.triggerCoolDown = false;
		float num = Terrain.activeTerrain.SampleHeight(base.transform.position) + Terrain.activeTerrain.transform.position.y;
		if ((this.WoodenPlanksGo == null && !LocalPlayer.IsInEndgame && this.climbType == exitClimb.Types.ropeClimb && base.transform.position.y - num < 2.5f) || this.forceAddLedge)
		{
			this.SpawnWoodenPlatform();
		}
	}

	public void SpawnWoodenPlatform()
	{
		this.WoodenPlanksGo = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("climbingPlanksPrefab"), base.transform.position, base.transform.rotation);
		this.WoodenPlanksGo.transform.parent = base.transform;
		this.WoodenPlanksGo.transform.localPosition = new Vector3(-0.135f, 0.158f, 0.131f);
		this.WoodenPlanksGo.transform.localEulerAngles = new Vector3(0f, -1.525f, 0f);
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Player") && !this.triggerCoolDown)
		{
			this.triggerCoolDown = true;
			this.Player = other.transform.root.GetComponent<PlayerInventory>();
			if (this.climbType == exitClimb.Types.ropeClimb)
			{
				this.Player.SpecialActions.SendMessage("exitClimbRopeTop", base.transform);
			}
			if (this.climbType == exitClimb.Types.wallClimb)
			{
				this.Player.SpecialActions.SendMessage("exitClimbWallTop", base.transform);
			}
			base.Invoke("resetCoolDown", 0.1f);
		}
	}

	private void resetCoolDown()
	{
		this.triggerCoolDown = false;
	}

	private void OnDestroy()
	{
		if (this.WoodenPlanksGo)
		{
			UnityEngine.Object.Destroy(this.WoodenPlanksGo);
		}
	}

	private IEnumerator pulseCollider()
	{
		for (;;)
		{
			this.collider.enabled = true;
			yield return YieldPresets.WaitPointOneSeconds;
			this.collider.enabled = false;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	public GameObject WoodenPlanksGo;

	private PlayerInventory Player;

	private BoxCollider collider;

	private bool triggerCoolDown;

	public bool forceAddLedge;

	public exitClimb.Types climbType;

	public enum Types
	{
		ropeClimb,
		wallClimb
	}
}
