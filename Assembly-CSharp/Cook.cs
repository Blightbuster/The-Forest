using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.Utils;
using UnityEngine;

[DoNotSerializePublic]
public class Cook : EntityBehaviour<ICookingState>
{
	private void Start()
	{
		if (!LevelSerializer.IsDeserializing)
		{
			if (DecayingInventoryItemView.LastUsed)
			{
				this.SetDecayState(DecayingInventoryItemView.LastUsed._prevState);
				DecayingInventoryItemView.LastUsed = null;
			}
			if (base.GetComponent<Collider>() is CapsuleCollider)
			{
				CapsuleCollider capsuleCollider = (CapsuleCollider)base.GetComponent<Collider>();
				Vector3 vector = base.transform.forward * capsuleCollider.height;
				Vector3 vector2 = base.transform.position;
				Vector3 vector3 = base.transform.position - vector / 2f;
				RaycastHit raycastHit;
				if (Physics.SphereCast(vector2, capsuleCollider.radius, -base.transform.up, out raycastHit, 15f, this.PlaceOnLayers))
				{
					Debug.DrawLine(vector2, raycastHit.point, Color.red, 10f);
					vector2 = raycastHit.point;
				}
				if (Physics.SphereCast(vector3, capsuleCollider.radius, -base.transform.up, out raycastHit, 15f, this.PlaceOnLayers))
				{
					Debug.DrawLine(vector3, raycastHit.point, Color.green, 10f);
					vector3 = raycastHit.point;
				}
				else
				{
					vector3 = vector2 - vector;
				}
				base.transform.position = Vector3.Lerp(vector2, vector3, 0.5f) - capsuleCollider.center;
				base.transform.LookAt(vector2);
			}
			this._startTime = Time.time;
			this._doneTime = 0f;
			if (!BoltNetwork.isRunning || (base.entity.isAttached && base.entity.isOwner))
			{
				base.CancelInvoke("CookMe");
				base.CancelInvoke("OverCooked");
				if (this._decayState < DecayingInventoryItemView.DecayStates.DriedFresh)
				{
					base.Invoke("CookMe", this._cookDuration);
				}
				else
				{
					this.CookMe();
				}
				base.Invoke("OverCooked", this._overcookDuration);
			}
			else if (BoltNetwork.isRunning && base.entity.isAttached && !base.entity.isOwner)
			{
				this.OnDecayStateUpdate();
				this.OnStatusUpdate();
			}
		}
	}

	public void SetCustomStatus(Cook.Status status)
	{
		this.CurrentStatus = status;
		if (base.entity.isAttached && base.entity.isOwner)
		{
			base.state.Status = (int)status;
		}
	}

	private void OnSerializing()
	{
		this._doneTime += Time.time - this._startTime;
		this._startTime = Time.time;
	}

	private IEnumerator OnDeserialized()
	{
		if (base.GetComponent<EmptyObjectIdentifier>())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			yield break;
		}
		if (BoltNetwork.isRunning)
		{
			while (!base.entity.isAttached)
			{
				yield return null;
			}
		}
		if (!base.transform.parent && this._cookDuration * 10f < this._overcookDuration)
		{
			DryingRack[] array = UnityEngine.Object.FindObjectsOfType<DryingRack>();
			foreach (DryingRack dryingRack in array)
			{
				Vector3 position = base.transform.position;
				position.y = dryingRack.dryingGrid.bounds.center.y;
				if (dryingRack.dryingGrid.bounds.Contains(position))
				{
					base.transform.parent = dryingRack.transform.parent;
					if (BoltNetwork.isRunning && base.entity.isOwner && !base.state.ParentHack)
					{
						base.state.ParentHack = base.transform.parent.GetComponent<BoltEntity>();
					}
					break;
				}
			}
			if (!base.transform.parent)
			{
				if (this.PickupTrigger)
				{
					this.PickupTrigger.GetComponent<Collider>().enabled = true;
				}
				if (this.RawPickupTrigger)
				{
					this.RawPickupTrigger.GetComponent<Collider>().enabled = true;
				}
			}
		}
		this.SetDecayState(this._decayState);
		base.CancelInvoke("CookMe");
		base.CancelInvoke("OverCooked");
		if (this._overcookDuration < this._doneTime)
		{
			this.OverCooked();
		}
		else if (this._cookDuration < this._doneTime)
		{
			this.CookMe();
		}
		while (!Scene.FinishGameLoad)
		{
			yield return null;
		}
		this._startTime = Time.time;
		if (this._cookDuration > this._doneTime)
		{
			base.Invoke("CookMe", this._cookDuration - this._doneTime);
		}
		if (this._overcookDuration > this._doneTime)
		{
			if (this._cookDuration < this._doneTime)
			{
			}
			base.Invoke("OverCooked", this._overcookDuration - this._doneTime);
		}
		yield break;
	}

	public void SetDecayState(DecayingInventoryItemView.DecayStates decayState)
	{
		this._decayState = decayState;
		if (this.PickupTrigger)
		{
			if (this._decayState < DecayingInventoryItemView.DecayStates.DriedFresh)
			{
				this.PickupTrigger._state = this._decayState + 3;
			}
			else
			{
				this.PickupTrigger._state = this._decayState;
			}
		}
		if (this.RawPickupTrigger)
		{
			this.RawPickupTrigger._state = this._decayState;
		}
		if (LocalPlayer.Inventory && LocalPlayer.Inventory.InventoryItemViewsCache[this._itemId][0] is DecayingInventoryItemView)
		{
			this._targetRenderer.sharedMaterial = ((DecayingInventoryItemView)LocalPlayer.Inventory.InventoryItemViewsCache[this._itemId][0]).GetMaterialForState(this._decayState);
		}
		if (BoltNetwork.isRunning && base.entity.isAttached && base.entity.isOwner)
		{
			base.state.DecayState = (int)this._decayState;
		}
	}

	private void CancelCook()
	{
		if (this.CurrentStatus == Cook.Status.Cooking && (!BoltNetwork.isRunning || !base.entity.isAttached || base.entity.isOwner))
		{
			Item item = ItemDatabase.ItemById(this._itemId);
			Transform transform = UnityEngine.Object.Instantiate<Transform>((BoltNetwork.isRunning && item._pickupPrefabMP) ? item._pickupPrefabMP : item._pickupPrefab, this._targetRenderer.transform.position, this._targetRenderer.transform.rotation);
			if (BoltNetwork.isRunning)
			{
				BoltNetwork.Attach(transform.gameObject);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void CookMe()
	{
		GameStats.CookedFood.Invoke();
		if (this.EatTrigger)
		{
			this.EatTrigger.gameObject.SetActive(true);
		}
		if (this.RawPickupTrigger && this.EatTrigger)
		{
			this.RawPickupTrigger.gameObject.SetActive(false);
		}
		if (this.Cooked)
		{
			this._targetRenderer.sharedMaterial = this.Cooked;
		}
		if (this._billboardSheen)
		{
			this._billboardSheen.SetActive(true);
		}
		this.CurrentStatus = Cook.Status.Cooked;
		if (base.entity.isAttached && base.entity.isOwner)
		{
			base.state.Status = 1;
		}
		base.BroadcastMessage("Cooked", SendMessageOptions.DontRequireReceiver);
	}

	private void OverCooked()
	{
		GameStats.BurntFood.Invoke();
		if (this.EatTrigger)
		{
			this.EatTrigger.gameObject.SetActive(true);
			this.EatTrigger.Burnt = true;
		}
		if (this.RawPickupTrigger)
		{
			this.RawPickupTrigger.gameObject.SetActive(false);
		}
		if (this.Burnt)
		{
			this._targetRenderer.sharedMaterial = this.Burnt;
		}
		if (this.DissolvedPrefab || this._destroyIfNotDisolve)
		{
			this.DisolveBurnt();
		}
		this.CurrentStatus = Cook.Status.Burnt;
		if (base.entity.isAttached && base.entity.isOwner)
		{
			base.state.Status = 2;
		}
	}

	private void DisolveBurnt()
	{
		if (!BoltNetwork.isRunning || (base.entity.isAttached && base.entity.isOwner))
		{
			if (this.DissolvedPrefab)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.DissolvedPrefab, base.transform.position, base.transform.rotation);
				if (this._scaleDisolve)
				{
					gameObject.transform.localScale.Scale(this._targetRenderer.transform.localScale);
				}
				if (BoltNetwork.isRunning && gameObject.GetComponent<BoltEntity>())
				{
					BoltNetwork.Attach(gameObject);
				}
			}
			if (BoltNetwork.isRunning)
			{
				BoltNetwork.Destroy(base.gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public override void Attached()
	{
		if (!base.entity.isOwner)
		{
			if (!base.state.ParentHack)
			{
				base.state.AddCallback("ParentHack", new PropertyCallbackSimple(this.OnParentUpdate));
			}
			else
			{
				this.OnParentUpdate();
			}
			base.state.AddCallback("Status", new PropertyCallbackSimple(this.OnStatusUpdate));
			base.state.AddCallback("DecayState", new PropertyCallbackSimple(this.OnDecayStateUpdate));
			this.OnDecayStateUpdate();
			this.OnStatusUpdate();
		}
		else
		{
			if (!base.state.ParentHack)
			{
				base.state.AddCallback("ParentHack", new PropertyCallbackSimple(this.OnParentUpdate));
			}
			else
			{
				this.OnParentUpdate();
			}
			if (this._decayState != (DecayingInventoryItemView.DecayStates)base.state.DecayState)
			{
				base.state.DecayState = (int)this._decayState;
			}
		}
	}

	private void OnParentUpdate()
	{
		base.StartCoroutine(this.OnParentUpdateRoutine());
	}

	private IEnumerator OnParentUpdateRoutine()
	{
		int failsafe = 90;
		while (!base.state.ParentHack)
		{
			int num;
			failsafe = (num = failsafe) - 1;
			if (num <= 0)
			{
				break;
			}
			yield return YieldPresets.WaitOneSecond;
		}
		base.transform.parent = base.state.ParentHack.transform;
		yield break;
	}

	private void OnStatusUpdate()
	{
		Cook.Status status = (Cook.Status)base.state.Status;
		if (status != Cook.Status.Cooked)
		{
			if (status != Cook.Status.Burnt)
			{
				if (status == Cook.Status.CookingNoWater)
				{
					base.SendMessage("SetActiveBonus", (WeaponStatUpgrade.Types)(-1), SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				this.OverCooked();
			}
		}
		else
		{
			this.CookMe();
		}
	}

	private void OnDecayStateUpdate()
	{
		if (base.state.DecayState != 0)
		{
			this.SetDecayState((DecayingInventoryItemView.DecayStates)base.state.DecayState);
		}
	}

	public Cook.Status CurrentStatus { get; private set; }

	public LayerMask PlaceOnLayers;

	public Material Cooked;

	public Material Burnt;

	public EatCooked EatTrigger;

	public DecayingPickUp RawPickupTrigger;

	public DecayingPickUp PickupTrigger;

	public GameObject DissolvedPrefab;

	public Renderer _targetRenderer;

	public GameObject _billboardSheen;

	public GameObject _billboardPickup;

	public float _cookDuration = 25f;

	public float _overcookDuration = 60f;

	public float _dissolveDelay = 2f;

	public bool _destroyIfNotDisolve;

	public bool _scaleDisolve = true;

	[SerializeThis]
	public DecayingInventoryItemView.DecayStates _decayState;

	[ItemIdPicker]
	public int _itemId;

	private float _startTime;

	[SerializeThis]
	private float _doneTime;

	public enum Status
	{
		Cooking,
		Cooked,
		Burnt,
		CookingNoWater
	}
}
