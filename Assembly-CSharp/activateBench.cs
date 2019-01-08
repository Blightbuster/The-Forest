using System;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

public class activateBench : EntityBehaviour<IworkBenchState>
{
	private void Start()
	{
		base.enabled = false;
	}

	public override void Attached()
	{
		this.targetEntity = base.transform.parent.GetComponent<BoltEntity>();
		if (!this.targetEntity.IsOwner())
		{
			base.state.AddCallback("occupied1", new PropertyCallbackSimple(this.occupied1Changed));
			base.state.AddCallback("occupied2", new PropertyCallbackSimple(this.occupied2Changed));
		}
	}

	private void occupied1Changed()
	{
		this.occupied1 = base.state.occupied1;
	}

	private void occupied2Changed()
	{
		this.occupied2 = base.state.occupied2;
	}

	private void GrabEnter(GameObject grabber)
	{
		if (!LocalPlayer.AnimControl.onRope)
		{
			base.enabled = true;
			this.Player = grabber.transform.root.GetComponent<PlayerInventory>();
			if (Vector3.Distance(base.transform.position, LocalPlayer.Transform.position) < 4.75f || (!LocalPlayer.FpCharacter.Grounded && !this.Sitting))
			{
				this.Sheen.SetActive(false);
				this.MyPickUp.SetActive(true);
			}
			else
			{
				this.Sheen.SetActive(true);
				this.MyPickUp.SetActive(false);
			}
		}
	}

	private void GrabExit(GameObject grabber)
	{
		if (!this.Sitting && this.Player && this.Player.transform == grabber.transform.root)
		{
			base.enabled = false;
			this.Sheen.SetActive(true);
			this.MyPickUp.SetActive(false);
		}
	}

	private void Update()
	{
		bool flag = false;
		if (BoltNetwork.isRunning && this.targetEntity && ((base.state.occupied1 && this.singleSeat) || (base.state.occupied2 && base.state.occupied1)))
		{
			flag = true;
		}
		float num = Vector3.Distance(base.transform.position, LocalPlayer.Transform.position);
		if (this.checkForCloseWall() || (!this.ValidateTriggerForCoop() && !this.Sitting))
		{
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
			return;
		}
		if (flag)
		{
			this.MyPickUp.SetActive(false);
			this.Sheen.SetActive(false);
		}
		else
		{
			if (LocalPlayer.AnimControl.onRope || (LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash != this.idlehash && LocalPlayer.Animator.GetCurrentAnimatorStateInfo(0).tagHash != this.walkHash) || num > 4.75f || (!LocalPlayer.FpCharacter.Grounded && !this.Sitting))
			{
				this.MyPickUp.SetActive(false);
				this.Sheen.SetActive(true);
				return;
			}
			if (!this.Sitting)
			{
				this.Sheen.SetActive(false);
				this.MyPickUp.SetActive(true);
			}
		}
		if (!this.Sitting)
		{
			if (TheForest.Utils.Input.GetButtonDown("Take") && !flag)
			{
				this.Sitting = true;
				if (BoltNetwork.isRunning && this.targetEntity)
				{
					syncWorkBench syncWorkBench = syncWorkBench.Create(GlobalTargets.OnlyServer);
					if (base.state.occupied1 && !this.singleSeat)
					{
						if (this.targetEntity.IsOwner())
						{
							base.state.occupied2 = true;
						}
						else
						{
							syncWorkBench.occupied2 = true;
							this.occupied2 = true;
						}
						this.resetOccupied2 = true;
					}
					else if (!base.state.occupied1)
					{
						if (this.targetEntity.IsOwner())
						{
							base.state.occupied1 = true;
						}
						else
						{
							syncWorkBench.occupied1 = true;
							this.occupied1 = true;
						}
						this.resetOccupied1 = true;
					}
					syncWorkBench.target = this.targetEntity;
					syncWorkBench.Send();
					this.Player.SpecialActions.SendMessage("setSecondPosition", this.resetOccupied2);
				}
				this.Player.SpecialActions.SendMessage("useTriggerHeight", this.useTriggerHeight);
				this.Player.SpecialActions.SendMessage("SitOnBench", base.transform);
				this.Sheen.SetActive(false);
				this.MyPickUp.SetActive(false);
			}
		}
		else if ((TheForest.Utils.Input.GetButtonDown("Take") || TheForest.Utils.Input.GetButtonDown("Crouch") || TheForest.Utils.Input.GetButtonDown("Jump") || TheForest.Utils.Input.GetButtonDown("Vertical")) && this.Sitting)
		{
			this.Player.SpecialActions.SendMessage("UpFromBench");
			if (BoltNetwork.isRunning && this.targetEntity)
			{
				syncWorkBench syncWorkBench2 = syncWorkBench.Create(GlobalTargets.OnlyServer);
				if (this.resetOccupied2)
				{
					if (this.targetEntity.IsOwner())
					{
						base.state.occupied2 = false;
					}
					else
					{
						syncWorkBench2.resetOccupied2 = true;
					}
					this.resetOccupied2 = false;
				}
				else if (this.resetOccupied1)
				{
					if (this.targetEntity.IsOwner())
					{
						base.state.occupied1 = false;
					}
					else
					{
						syncWorkBench2.resetOccupied1 = true;
					}
					this.resetOccupied1 = false;
				}
				syncWorkBench2.target = this.targetEntity;
				syncWorkBench2.Send();
			}
			this.Sitting = false;
			this.Player = null;
			base.enabled = false;
		}
	}

	private bool ValidateTriggerForCoop()
	{
		if (!LocalPlayer.IsInEndgame)
		{
			return true;
		}
		for (int i = 0; i < Scene.SceneTracker.allPlayers.Count; i++)
		{
			targetStats component = Scene.SceneTracker.allPlayers[i].GetComponent<targetStats>();
			if (Scene.SceneTracker.allPlayers[i] && Scene.SceneTracker.allPlayers[i].CompareTag("PlayerNet") && component)
			{
				float sqrMagnitude = (Scene.SceneTracker.allPlayers[i].transform.position - base.transform.position).sqrMagnitude;
				if (sqrMagnitude < 2.25f)
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
		return Physics.Raycast(base.transform.position, direction, out raycastHit, direction.magnitude, this.wallMask, QueryTriggerInteraction.Ignore);
	}

	public GameObject Sheen;

	public GameObject MyPickUp;

	public bool useTriggerHeight;

	public bool singleSeat;

	private bool occupied1;

	private bool occupied2;

	private bool resetOccupied1;

	private bool resetOccupied2;

	private int wallMask = 103948289;

	private int idlehash = Animator.StringToHash("idling");

	private int walkHash = Animator.StringToHash("walking");

	public bool Sitting;

	private PlayerInventory Player;

	private BoltEntity targetEntity;

	private IworkBenchState benchState;
}
