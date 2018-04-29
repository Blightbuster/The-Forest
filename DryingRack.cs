using System;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Items;
using TheForest.Items.Inventory;
using TheForest.Items.World;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;


public class DryingRack : EntityBehaviour, IWetable
{
	
	private void Awake()
	{
		base.enabled = false;
		this.originalRotatePosition = this._rotateIcon.transform.localPosition;
	}

	
	private void Update()
	{
		if (this.CheckCurrentSlotAvailable())
		{
			int num;
			if (LocalPlayer.Inventory.RightHand != null)
			{
				num = this.foodItems.FindIndex((DryingRack.FoodItem fi) => fi._itemId == LocalPlayer.Inventory.RightHand._itemId);
			}
			else
			{
				num = -1;
			}
			int num2 = num;
			if (num2 >= 0)
			{
				this._currentFoodItemNum = num2;
			}
			if (num2 >= 0)
			{
				if (TheForest.Utils.Input.GetButtonDown("Craft"))
				{
					LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
					LocalPlayer.Sfx.PlayWhoosh();
					this.SpawnFoodPrefab(this.foodItems[num2]._cookPrefab);
					Scene.HudGui.DryingRackWidget.Shutdown();
				}
				else
				{
					Scene.HudGui.DryingRackWidget.ShowSingle(this.CurrentFoodItemId, this.Icons.transform, SideIcons.Craft);
				}
			}
			else if (this._rotateIcon)
			{
				bool flag = LocalPlayer.Inventory.Owns(this.CurrentFoodItemId, true);
				if (flag)
				{
					if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this.CurrentFoodItemId, 1, true, true))
					{
						LocalPlayer.Sfx.PlayWhoosh();
						this.SpawnFoodPrefab(this.CurrentFoodprefab);
					}
					else
					{
						Scene.HudGui.DryingRackWidget.ShowList(this.CurrentFoodItemId, this.Icons.transform, SideIcons.Craft);
					}
				}
				int num3 = this.NextFoodType();
				if (this._currentFoodItemNum != num3)
				{
					if (TheForest.Utils.Input.GetButtonDown("Rotate") || !flag)
					{
						LocalPlayer.Sfx.PlayWhoosh();
						this._currentFoodItemNum = num3;
						if (this._currentFoodItemNum > -1)
						{
							Scene.HudGui.DryingRackWidget.ShowList(this.CurrentFoodItemId, this.Icons.transform, SideIcons.Craft);
						}
					}
				}
				else if (!flag)
				{
					Scene.HudGui.DryingRackWidget.Shutdown();
				}
			}
			if (this._rotateIcon.activeSelf)
			{
				this._rotateIcon.transform.localPosition = 0.35f * this.CameraDirection + this.originalRotatePosition;
			}
		}
	}

	
	private void OnDisable()
	{
		if (Scene.HudGui)
		{
			Scene.HudGui.DryingRackWidget.Shutdown();
		}
	}

	
	public void GrabEnter()
	{
		base.enabled = (!BoltNetwork.isRunning || this.entity.isAttached);
		if (base.enabled)
		{
			this._currentFoodItemNum--;
			this._currentFoodItemNum = this.NextFoodType();
		}
	}

	
	private void GrabExit()
	{
		base.enabled = false;
		Scene.HudGui.DryingRackWidget.Shutdown();
		if (this._lastActivePickup)
		{
			this._lastActivePickup.SendMessage("GrabExit", SendMessageOptions.DontRequireReceiver);
			this._lastActivePickup = null;
		}
	}

	
	public void GotClean()
	{
		UnityEngine.Object.Destroy(this.CookHeldIcon);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	
	private Vector3 CameraDirection
	{
		get
		{
			Vector3 vector = this.dryingGrid.transform.InverseTransformPoint(LocalPlayer.Create.Grabber.transform.position);
			vector.x -= vector.x % this.gridChunkSize.x;
			vector.z -= vector.z % this.gridChunkSize.y;
			vector.x = Mathf.Clamp(vector.x, -this.dryingGrid.size.x * 0.5f + this.gridChunkSize.x, this.dryingGrid.size.x * 0.5f - this.gridChunkSize.x);
			vector.z = Mathf.Clamp(vector.z, -this.dryingGrid.size.z * 0.5f + this.gridChunkSize.y, this.dryingGrid.size.z * 0.5f - this.gridChunkSize.y);
			vector.y = this.dryingGrid.center.y + this.gridOffset.y;
			vector = this.dryingGrid.transform.TransformPoint(vector);
			return (LocalPlayer.MainCamTr.position - vector).normalized;
		}
	}

	
	private bool CheckCurrentSlotAvailable()
	{
		Vector3 vector = this.dryingGrid.transform.InverseTransformPoint(LocalPlayer.Create.Grabber.transform.position);
		vector.x -= vector.x % this.gridChunkSize.x;
		vector.z -= vector.z % this.gridChunkSize.y;
		vector.x = Mathf.Clamp(vector.x, -this.dryingGrid.size.x * 0.5f + this.gridChunkSize.x, this.dryingGrid.size.x * 0.5f - this.gridChunkSize.x);
		vector.z = Mathf.Clamp(vector.z, -this.dryingGrid.size.z * 0.5f + this.gridChunkSize.y, this.dryingGrid.size.z * 0.5f - this.gridChunkSize.y);
		vector.y = this.dryingGrid.center.y + this.gridOffset.y;
		vector = this.dryingGrid.transform.TransformPoint(vector);
		Vector3 vector2 = vector + (LocalPlayer.MainCamTr.position - vector).normalized * 0.4f + new Vector3(0f, -1f, 0f);
		float num = this.gridChunkSize.magnitude * 0.75f;
		foreach (object obj in base.transform.parent)
		{
			Transform transform = (Transform)obj;
			Cook component = transform.GetComponent<Cook>();
			if (component && Vector3.Distance(vector, transform.position) < num)
			{
				if (component.PickupTrigger && component.CurrentStatus >= Cook.Status.Cooked)
				{
					if (this._lastActivePickup != component.PickupTrigger)
					{
						if (this._lastActivePickup)
						{
							this._lastActivePickup.SendMessage("GrabExit", SendMessageOptions.DontRequireReceiver);
						}
						component.PickupTrigger.SendMessage("GrabEnter", SendMessageOptions.DontRequireReceiver);
						this._lastActivePickup = component.PickupTrigger;
					}
				}
				else if (this._lastActivePickup != component.RawPickupTrigger)
				{
					if (this._lastActivePickup)
					{
						this._lastActivePickup.SendMessage("GrabExit", SendMessageOptions.DontRequireReceiver);
					}
					component.RawPickupTrigger.SendMessage("GrabEnter", SendMessageOptions.DontRequireReceiver);
					this._lastActivePickup = component.RawPickupTrigger;
				}
				if (this._lastActivePickup && this._lastActivePickup._myPickUp)
				{
					this._lastActivePickup._myPickUp.transform.position = vector2 + this.Icons.transform.GetChild(0).localPosition;
				}
				this.Icons.transform.position = vector2;
				Scene.HudGui.DryingRackWidget.ShowSingle(component._itemId, this.Icons.transform, SideIcons.Take);
				this.Icons.gameObject.SetActive(false);
				return false;
			}
		}
		if (Time.time > this._nextAddFood)
		{
			if (this._lastActivePickup)
			{
				this._lastActivePickup.SendMessage("GrabExit", SendMessageOptions.DontRequireReceiver);
				this._lastActivePickup = null;
			}
			this.Icons.gameObject.SetActive(true);
			this.Icons.transform.position = vector2;
			return true;
		}
		Scene.HudGui.DryingRackWidget.Shutdown();
		this.Icons.gameObject.SetActive(false);
		return false;
	}

	
	private int NextFoodType()
	{
		int num = this.foodItems.Length;
		int num2 = this._currentFoodItemNum;
		while (num-- > 0)
		{
			num2 = (int)Mathf.Repeat((float)(num2 + 1), (float)this.foodItems.Length);
			if (LocalPlayer.Inventory.Owns(this.foodItems[num2]._itemId, true))
			{
				break;
			}
		}
		return num2;
	}

	
	private void SpawnFoodPrefab(Cook foodPrefab)
	{
		this._nextAddFood = Time.time + 0.25f;
		Vector3 position = this.dryingGrid.transform.InverseTransformPoint(LocalPlayer.Create.Grabber.transform.position);
		position.x -= position.x % this.gridChunkSize.x;
		position.z -= position.z % this.gridChunkSize.y;
		position.x = Mathf.Clamp(position.x, -this.dryingGrid.size.x * 0.5f + this.gridChunkSize.x, this.dryingGrid.size.x * 0.5f - this.gridChunkSize.x);
		position.z = Mathf.Clamp(position.z, -this.dryingGrid.size.z * 0.5f + this.gridChunkSize.y, this.dryingGrid.size.z * 0.5f - this.gridChunkSize.y);
		position.y = this.dryingGrid.center.y + this.gridOffset.y;
		if (BoltNetwork.isRunning)
		{
			PlaceDryingFood placeDryingFood = PlaceDryingFood.Create(GlobalTargets.OnlyServer);
			placeDryingFood.PrefabId = foodPrefab.GetComponent<BoltEntity>().prefabId;
			placeDryingFood.Position = this.dryingGrid.transform.TransformPoint(position);
			placeDryingFood.Rotation = Quaternion.identity;
			placeDryingFood.Parent = base.transform.parent.GetComponent<BoltEntity>();
			if (DecayingInventoryItemView.LastUsed)
			{
				placeDryingFood.DecayState = (int)DecayingInventoryItemView.LastUsed._prevState;
			}
			placeDryingFood.Send();
		}
		else
		{
			Cook cook = (Cook)UnityEngine.Object.Instantiate(foodPrefab, this.dryingGrid.transform.TransformPoint(position), Quaternion.identity);
			cook.transform.parent = base.transform.parent;
		}
	}

	
	
	private int CurrentFoodItemId
	{
		get
		{
			return this.foodItems[this._currentFoodItemNum]._itemId;
		}
	}

	
	
	private Cook CurrentFoodprefab
	{
		get
		{
			return this.foodItems[this._currentFoodItemNum]._cookPrefab;
		}
	}

	
	
	private GameObject CurrentFoodIcon
	{
		get
		{
			return (!this.foodItems[this._currentFoodItemNum]._icon) ? this.CookHeldIcon : this.foodItems[this._currentFoodItemNum]._icon;
		}
	}

	
	public DryingRack.FoodItem[] foodItems;

	
	public GameObject Icons;

	
	public GameObject CookHeldIcon;

	
	public GameObject _rotateIcon;

	
	public BoxCollider dryingGrid;

	
	public Vector2 gridChunkSize;

	
	public Vector3 gridOffset;

	
	private float _nextAddFood;

	
	private int _currentFoodItemNum;

	
	private PickUp _lastActivePickup;

	
	private Vector3 originalRotatePosition;

	
	[Serializable]
	public class FoodItem
	{
		
		[ItemIdPicker]
		public int _itemId;

		
		public Cook _cookPrefab;

		
		public GameObject _icon;
	}
}
