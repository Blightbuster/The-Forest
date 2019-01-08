using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Grabber : MonoBehaviour
{
	public static Collider FocusedItem { get; private set; }

	public static GameObject FocusedItemGO { get; private set; }

	public static bool IsFocused
	{
		get
		{
			return Grabber.FocusedItem && Grabber.FocusedItem.enabled && Grabber.FocusedItemGO.activeInHierarchy;
		}
	}

	private void Awake()
	{
		Grabber.FocusedItem = null;
		Grabber.FocusedItemGO = null;
		Grabber.Filter = null;
		this.collider = base.GetComponent<Collider>();
		this.PickupLayer = LayerMask.NameToLayer("PickUp");
		this.ResetDefaultMessages();
	}

	public void ResetDefaultMessages()
	{
		this.ValidateCollider = new Func<Collider, bool>(this.IsValid);
		this.OnEnter = new Action(this.EnterMessage);
		this.OnExit = new Action(this.ExitMessage);
	}

	public bool IsValid(Collider other)
	{
		return other.isTrigger && other.gameObject.layer == this.PickupLayer && Grabber.FocusedItem != other && (Grabber.Filter == null || other.gameObject.Equals(Grabber.Filter));
	}

	public void EnterMessage()
	{
		if (Grabber.FocusedItem)
		{
			Grabber.FocusedItem.SendMessage("GrabEnter", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void ExitMessage()
	{
		if (Grabber.FocusedItem)
		{
			Grabber.FocusedItem.SendMessage("GrabExit", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Clear()
	{
		Grabber.FocusedItem = null;
		Grabber.FocusedItemGO = null;
		Grabber.wasFocused = false;
	}

	public void RefreshCollider()
	{
		if (!this.colliderWasRefreshed)
		{
			this.colliderWasRefreshed = true;
			this.collider.enabled = false;
			this.collider.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		this.physicsUpdate = true;
	}

	private void Update()
	{
		if (Grabber.wasFocused && !Grabber.IsFocused)
		{
			this.OnExit();
			this.Clear();
			this.RefreshCollider();
		}
		else if (Grabber.wasFocused && ((LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World && (LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.Sleep || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlayerList)) || LocalPlayer.Create.CreateMode))
		{
			this.OnExit();
			this.Clear();
			this.RefreshCollider();
			this.busyLock = true;
		}
		else if (this.busyLock)
		{
			this.RefreshCollider();
			this.busyLock = false;
		}
		if (this.physicsUpdate)
		{
			if (!this.triggerStayUpdate && Grabber.IsFocused)
			{
				this.OnExit();
				this.Clear();
				this.RefreshCollider();
			}
			this.physicsUpdate = false;
			this.triggerStayUpdate = false;
		}
		if (this.colliderWasRefreshed)
		{
			if (!this.readyToResetColliderWasRefreshed)
			{
				this.readyToResetColliderWasRefreshed = true;
			}
			else
			{
				this.colliderWasRefreshed = false;
				this.readyToResetColliderWasRefreshed = false;
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (this.ValidateCollider(other))
		{
			if (!Grabber.IsFocused || Vector3.Distance(base.transform.position, other.transform.position) < Vector3.Distance(base.transform.position, Grabber.FocusedItem.transform.position))
			{
				if (Grabber.IsFocused)
				{
					this.OnExit();
				}
				Grabber.FocusedItem = other;
				Grabber.FocusedItemGO = other.gameObject;
				this.OnEnter();
				Grabber.wasFocused = true;
				this.triggerStayUpdate = true;
			}
		}
		else if (Grabber.FocusedItem == other)
		{
			this.triggerStayUpdate = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == this.PickupLayer && other == Grabber.FocusedItem)
		{
			this.OnExit();
			this.Clear();
			this.RefreshCollider();
		}
	}

	public static void SetFilter(GameObject filter)
	{
		Grabber.Filter = filter;
		if (Grabber.FocusedItemGO != filter)
		{
			if (Grabber.FocusedItem)
			{
				Grabber.FocusedItem.SendMessage("GrabExit", LocalPlayer.Create.Grabber.gameObject, SendMessageOptions.DontRequireReceiver);
				Grabber.FocusedItem = null;
			}
			Grabber.FocusedItemGO = null;
		}
	}

	private static bool wasFocused;

	public static GameObject Filter;

	private int PickupLayer = -1;

	private bool busyLock;

	private bool colliderWasRefreshed;

	private bool readyToResetColliderWasRefreshed;

	private Collider collider;

	private bool physicsUpdate;

	private bool triggerStayUpdate;

	public Func<Collider, bool> ValidateCollider;

	public Action OnEnter;

	public Action OnExit;
}
