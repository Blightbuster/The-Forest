using System;
using System.Collections;
using TheForest.Items.Inventory;
using UnityEngine;


public class exitClimb : MonoBehaviour
{
	
	private void Start()
	{
		this.collider = base.transform.GetComponent<BoxCollider>();
		this.collider.enabled = true;
		this.triggerCoolDown = false;
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

	
	private PlayerInventory Player;

	
	private BoxCollider collider;

	
	private bool triggerCoolDown;

	
	public exitClimb.Types climbType;

	
	public enum Types
	{
		
		ropeClimb,
		
		wallClimb
	}
}
