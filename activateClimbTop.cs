using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class activateClimbTop : MonoBehaviour
{
	
	private void Start()
	{
		base.enabled = false;
	}

	
	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.FpCharacter.Sitting && !this.PlayerIsBellowTop)
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
		if (this.checkForCloseWall())
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
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			targetStats component = Scene.SceneTracker.allPlayers[i].GetComponent<targetStats>();
			if (component && component.onRope)
			{
				float sqrMagnitude = (Scene.SceneTracker.allPlayers[i].transform.position - base.transform.position).sqrMagnitude;
				if (sqrMagnitude < 20.25f)
				{
					return false;
				}
			}
		}
		return true;
	}

	
	private bool checkForCloseWall()
	{
		Vector3 position = base.transform.position;
		position.y += 2.3f;
		Vector3 direction = LocalPlayer.MainCamTr.position - position;
		RaycastHit raycastHit;
		return Physics.Raycast(position, direction, out raycastHit, direction.magnitude, this.wallMask);
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
