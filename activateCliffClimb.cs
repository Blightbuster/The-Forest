using System;
using System.Collections;
using TheForest.Items;
using TheForest.Player.Actions;
using TheForest.Utils;
using UnityEngine;


public class activateCliffClimb : MonoBehaviour
{
	
	private void Start()
	{
		this.layerMask = 67117056;
		this.MyPickUp.SetActive(false);
		base.StopAllCoroutines();
	}

	
	private void OnEnable()
	{
		base.StartCoroutine("scanForCliff");
	}

	
	private void OnDisable()
	{
		base.StopAllCoroutines();
		this.MyPickUp.SetActive(false);
		this.allowClimb = false;
	}

	
	private IEnumerator scanForCliff()
	{
		yield return YieldPresets.WaitForEndOfFrame;
		if (!this.cliffAction)
		{
			this.cliffAction = LocalPlayer.SpecialActions.GetComponent<PlayerClimbCliffAction>();
		}
		while (true && base.gameObject != null)
		{
			if (Physics.Raycast(LocalPlayer.MainCamTr.position, LocalPlayer.MainCamTr.forward, out this.hit, 5f, this.layerMask) && !LocalPlayer.FpCharacter.Sitting)
			{
				notClimbable nc = this.hit.transform.GetComponent<notClimbable>();
				if (!nc)
				{
					if (!this.hit.collider.gameObject.CompareTag("climbWall") && LocalPlayer.IsInCaves)
					{
						this.allowClimb = false;
						this.MyPickUp.SetActive(false);
						this.MyPickUp.transform.parent = base.transform;
					}
					else if (this.hit.collider.gameObject.layer == 13)
					{
						this.MyPickUp.transform.parent = null;
						if (this.cliffAction.doingClimb)
						{
							this.MyPickUp.SetActive(false);
						}
						else
						{
							this.MyPickUp.SetActive(true);
						}
						Vector3 iconPos = this.hit.point;
						iconPos += LocalPlayer.MainCamTr.forward * -1.5f;
						iconPos.y -= 1f;
						this.MyPickUp.transform.position = Vector3.Lerp(this.MyPickUp.transform.position, iconPos, Time.deltaTime * 20f);
						this.allowClimb = true;
					}
				}
			}
			else
			{
				this.allowClimb = false;
				this.MyPickUp.SetActive(false);
				this.MyPickUp.transform.parent = base.transform;
			}
			if (!this.activateCoolDown)
			{
				if (TheForest.Utils.Input.GetButtonDown("Take") && this.allowClimb && !LocalPlayer.Animator.GetBool("deathBool") && Physics.Raycast(LocalPlayer.Transform.position, LocalPlayer.Transform.forward, out this.hit, 5f, this.layerMask) && !LocalPlayer.AnimControl.cliffClimb)
				{
					if (!this.cliffAction.doingClimb && LocalPlayer.Inventory.HasInSlot(Item.EquipmentSlot.RightHand, this._itemId))
					{
						LocalPlayer.SpecialActions.SendMessage("setEnterClimbPos", this.hit.point);
						LocalPlayer.SpecialActions.SendMessage("enterClimbCliff", base.transform);
						LocalPlayer.AnimControl.cliffEnterNormal = this.hit.normal;
						LocalPlayer.AnimControl.cliffEnterPos = this.hit.point;
						this.activateCoolDown = true;
						base.Invoke("resetCoolDown", 2.2f);
					}
				}
				else if (TheForest.Utils.Input.GetButtonDown("Take") && LocalPlayer.AnimControl.allowCliffReset)
				{
					if (!this.cliffAction)
					{
						this.cliffAction = LocalPlayer.SpecialActions.GetComponent<PlayerClimbCliffAction>();
					}
					if (this.cliffAction.doingClimb)
					{
						LocalPlayer.SpecialActions.SendMessage("exitClimbCliffGround");
					}
				}
			}
			if (LocalPlayer.Animator.GetBool("deathBool"))
			{
				if (!this.cliffAction)
				{
					this.cliffAction = LocalPlayer.SpecialActions.GetComponent<PlayerClimbCliffAction>();
				}
				if (this.cliffAction.doingClimb)
				{
					LocalPlayer.SpecialActions.SendMessage("exitClimbCliffGround");
				}
				base.StopAllCoroutines();
			}
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	
	private void resetCoolDown()
	{
		this.activateCoolDown = false;
	}

	
	public GameObject MyPickUp;

	
	private PlayerClimbCliffAction cliffAction;

	
	public global::Types climbType;

	
	private RaycastHit hit;

	
	private int layerMask;

	
	public bool allowClimb;

	
	public bool doingClimb;

	
	public bool activateCoolDown;

	
	[ItemIdPicker]
	public int _itemId;
}
