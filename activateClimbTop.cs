using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class activateClimbTop : MonoBehaviour
{
	
	private void Start()
	{
		if (base.transform.parent)
		{
			this._bottomClimb = base.transform.parent.GetComponentInChildren<activateClimb>(true);
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
		if (!LocalPlayer.FpCharacter.Sitting && !this.PlayerIsBellowTop && !this.CheckShortRope())
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
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
			base.enabled = false;
		}
	}

	
	private void Update()
	{
		if (LocalPlayer.FpCharacter.Sitting)
		{
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
			base.enabled = false;
			return;
		}
		if (this.checkForCloseWall() || LocalPlayer.AnimControl.onRope || this.CheckShortRope())
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
		if (TheForest.Utils.Input.GetButtonDown("Take") && !LocalPlayer.FpCharacter.Sitting && !LocalPlayer.AnimControl.onRope && !LocalPlayer.AnimControl.knockedDown)
		{
			if (this.climbType == activateClimbTop.Types.ropeClimb)
			{
				LocalPlayer.SpecialActions.SendMessage("enterClimbRopeTop", base.transform);
			}
			if (this.climbType == activateClimbTop.Types.wallClimb && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
			{
				LocalPlayer.SpecialActions.SendMessage("enterClimbWallTop", base.transform);
			}
			base.StartCoroutine(this.validatePlayerActivate());
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
		}
	}

	
	private void enableExitTop()
	{
		this.exitTop.SetActive(true);
	}

	
	
	private bool PlayerIsBellowTop
	{
		get
		{
			return LocalPlayer.Transform.position.y + 2f < base.transform.position.y;
		}
	}

	
	private bool validateTriggerForCoop()
	{
		return true;
	}

	
	private bool checkForCloseWall()
	{
		Vector3 position = base.transform.position;
		position.y += 2.3f;
		Vector3 direction = LocalPlayer.MainCamTr.position - position;
		RaycastHit raycastHit;
		return Physics.Raycast(position, direction, out raycastHit, direction.magnitude, this.wallMask, QueryTriggerInteraction.Ignore);
	}

	
	private bool CheckShortRope()
	{
		if (this._bottomClimb)
		{
			float num = Vector3.Distance(base.transform.position, this._bottomClimb.transform.position);
			if (num < this.closeTriggerThreshhold)
			{
				return true;
			}
		}
		return false;
	}

	
	private IEnumerator validatePlayerActivate()
	{
		float t = 0f;
		while (t < 1f)
		{
			if (!this.validateTriggerForCoop())
			{
				LocalPlayer.SpecialActions.SendMessage("resetClimbRopeTop");
				yield break;
			}
			t += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public GameObject exitTop;

	
	public float closeTriggerThreshhold = 3f;

	
	private activateClimb _bottomClimb;

	
	public activateClimbTop.Types climbType;

	
	private int wallMask = 103948289;

	
	[ItemIdPicker]
	public int _itemId;

	
	public enum Types
	{
		
		ropeClimb,
		
		wallClimb
	}
}
