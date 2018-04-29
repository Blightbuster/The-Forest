using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class activateClimb : MonoBehaviour
{
	
	private void Start()
	{
		if (base.transform.parent)
		{
			this._topClimb = base.transform.parent.GetComponentInChildren<activateClimbTop>(true);
		}
		if (this.CheckShortRope())
		{
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
		}
		base.enabled = false;
	}

	
	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.FpCharacter.Sitting && !this.CheckShortRope())
		{
			base.enabled = true;
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(true);
		}
	}

	
	private void GrabExit(GameObject grabber)
	{
		if (base.enabled)
		{
			base.enabled = false;
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
		}
	}

	
	private void Update()
	{
		float num = Vector3.Distance(LocalPlayer.MainCamTr.position, base.transform.position);
		if (LocalPlayer.FpCharacter.Sitting || num > 6.1f)
		{
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
			return;
		}
		if (this.checkForCloseWall() || this.CheckShortRope())
		{
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
			return;
		}
		if (!this.validateTriggerForCoop())
		{
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
			return;
		}
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
		if (LocalPlayer.FpCharacter.PushingSled)
		{
			return;
		}
		if (TheForest.Utils.Input.GetButtonDown("Take") && !LocalPlayer.FpCharacter.Sitting && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.knockedDown)
		{
			if (this.climbType == activateClimb.Types.ropeClimb)
			{
				LocalPlayer.SpecialActions.SendMessage("enterClimbRope", base.transform);
			}
			else if (this.climbType == activateClimb.Types.wallClimb && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
			{
				LocalPlayer.SpecialActions.SendMessage("enterClimbWall", base.transform);
			}
			base.StartCoroutine(this.validatePlayerActivate());
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
		}
	}

	
	private bool validateTriggerForCoop()
	{
		return true;
	}

	
	private bool checkForCloseWall()
	{
		Vector3 direction = LocalPlayer.MainCamTr.position - base.transform.position;
		RaycastHit raycastHit;
		return Physics.Raycast(base.transform.position, direction, out raycastHit, direction.magnitude, this.wallMask, QueryTriggerInteraction.Ignore);
	}

	
	private bool CheckShortRope()
	{
		if (this._topClimb)
		{
			float num = Vector3.Distance(base.transform.position, this._topClimb.transform.position);
			if (num < this.closeTriggerThreshhold)
			{
				return true;
			}
		}
		return false;
	}

	
	private IEnumerator validatePlayerActivate()
	{
		yield break;
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	private activateClimbTop _topClimb;

	
	public float closeTriggerThreshhold = 3f;

	
	public activateClimb.Types climbType;

	
	private int wallMask = 103948289;

	
	[ItemIdPicker]
	public int _itemId;

	
	public enum Types
	{
		
		ropeClimb,
		
		wallClimb,
		
		cliffClimb
	}
}
