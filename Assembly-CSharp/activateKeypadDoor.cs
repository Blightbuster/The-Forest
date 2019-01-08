using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

public class activateKeypadDoor : MonoBehaviour
{
	private void Awake()
	{
		if (this.doorAnimator)
		{
			this.doorAnimator.CrossFade("Base Layer.closed", 0f, 0, 1f);
			this.doorAnimator.enabled = true;
		}
	}

	private void Start()
	{
		if (this.doorAnimator)
		{
			this.doorAnimator.enabled = false;
		}
		base.enabled = false;
	}

	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.AnimControl.onRope)
		{
			base.enabled = true;
		}
	}

	private void GrabExit(GameObject grabber)
	{
		base.enabled = false;
		this.ToggleIcons(false, !this.doorOpen);
	}

	private void Update()
	{
		if (!LocalPlayer.AnimControl)
		{
			return;
		}
		float num = Vector3.Distance(base.transform.position, LocalPlayer.Transform.position);
		if (LocalPlayer.AnimControl.onRope || (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash != this.idlehash && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash != this.walkHash) || num > 4.75f)
		{
			this.ToggleIcons(false, false);
			return;
		}
		if (!this.doorOpen)
		{
			if (LocalPlayer.Inventory.Owns(this._keycardId, true))
			{
				this.ToggleIcons(true, false);
				if (TheForest.Utils.Input.GetButtonDown("Take"))
				{
					if (this.sequence)
					{
						this.sequence.BeginStage(0);
					}
					else
					{
						this.DoActorAnimation();
						this.DoEnvironmentAnimation();
					}
				}
			}
			else
			{
				this.ToggleIcons(false, true);
			}
		}
		else
		{
			this.ToggleIcons(false, false);
		}
	}

	private void ToggleIcons(bool pickup, bool sheen)
	{
		if (this.Sheen.activeSelf != sheen)
		{
			this.Sheen.SetActive(sheen);
		}
		if (this.MyPickUp.activeSelf != pickup)
		{
			this.MyPickUp.SetActive(pickup);
		}
	}

	public void CloseDoor()
	{
		if (this.doorOpen)
		{
			if (this.doorAnimator)
			{
				this.doorAnimator.SetBool("open", false);
				this.doorAnimator.SetBool("close", true);
				this.doorAnimator.enabled = true;
			}
			this.onDoorClose.Invoke();
		}
	}

	public void DoActorAnimation()
	{
		LocalPlayer.SpecialActions.SendMessage("setKeycardId", this._keycardId);
		LocalPlayer.SpecialActions.SendMessage("setShortSequence", this.shortSequence);
		if (this.doorAnimator)
		{
			LocalPlayer.SpecialActions.SendMessage("setDoorAnimator", this.doorAnimator);
		}
		LocalPlayer.SpecialActions.SendMessage("openKeypadDoor", this.playerPos);
	}

	public void DoEnvironmentAnimation()
	{
		if (!this.doorOpen)
		{
			this.DoLateCompletion();
			base.Invoke("Finished", 6f);
		}
	}

	public void DoLateCompletion()
	{
		if (!this.doorOpen)
		{
			this.doorOpen = true;
			this.onDoorOpen.Invoke();
			if (this.doorAnimator)
			{
				this.doorAnimator.SetBool("close", false);
				this.doorAnimator.SetBool("open", true);
				this.doorAnimator.enabled = true;
			}
			this.ToggleIcons(false, false);
		}
	}

	private void Finished()
	{
		if (this.sequence)
		{
			this.sequence.CompleteStage(0);
		}
	}

	public void SetDoorClosed()
	{
		this.doorOpen = false;
	}

	public GameObject Sheen;

	public GameObject MyPickUp;

	public Animator doorAnimator;

	public Transform playerPos;

	private int idlehash = Animator.StringToHash("idling");

	private int walkHash = Animator.StringToHash("walking");

	[ItemIdPicker]
	public int _keycardId;

	public bool shortSequence;

	public bool doorOpen;

	public UnityEvent onDoorOpen;

	public UnityEvent onDoorClose;

	public AnimationSequence sequence;
}
