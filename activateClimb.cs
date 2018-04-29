using System;
using System.Collections;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class activateClimb : MonoBehaviour
{
	
	private void Start()
	{
		base.enabled = false;
	}

	
	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.FpCharacter.Sitting)
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
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			targetStats component = Scene.SceneTracker.allPlayers[i].GetComponent<targetStats>();
			if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet") && component)
			{
				float sqrMagnitude = (Scene.SceneTracker.allPlayers[i].transform.position - base.transform.position).sqrMagnitude;
				if (component.onRope)
				{
					if (sqrMagnitude < 6.25f)
					{
						return false;
					}
				}
				else if (sqrMagnitude < 2.25f)
				{
					return false;
				}
			}
		}
		return true;
	}

	
	private bool checkForCloseWall()
	{
		Vector3 direction = LocalPlayer.MainCamTr.position - base.transform.position;
		RaycastHit raycastHit;
		return Physics.Raycast(base.transform.position, direction, out raycastHit, direction.magnitude, this.wallMask);
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

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
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
