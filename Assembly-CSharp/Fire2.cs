using System;
using System.Collections;
using Bolt;
using TheForest.Buildings.Interfaces;
using TheForest.Buildings.World;
using TheForest.Items;
using TheForest.Items.Craft;
using TheForest.Items.Inventory;
using TheForest.UI;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

[DoNotSerializePublic]
public class Fire2 : EntityBehaviour<IFireState>, IWetable
{
	private void Awake()
	{
		if (this.FireLight)
		{
			this._light = this.FireLight.GetComponent<Light>();
			this._baseIntensity = this._light.intensity;
		}
		if (!this.LightFadeIn && this.FlamesAll)
		{
			this.LightFadeIn = this.FlamesAll.GetComponent<LightFadeIn>();
		}
		if (this.FlamesScale)
		{
			this._scaler = this.FlamesScale.GetComponent<ParticleScaler>();
			this._baseScale = this._scaler.particleScale;
		}
		this._leafAdded = 0f;
		if (this.IconLightFire)
		{
			this.IconLightFire.SetActive(false);
		}
		base.enabled = false;
		if (!BoltNetwork.isRunning)
		{
			this.Fuel = this.FuelStart;
		}
		if (this.FuelStart == 0f)
		{
			if (this.ModelLit)
			{
				this.ModelLit.SetActive(true);
			}
			if (this.ModelUnlit)
			{
				this.ModelUnlit.SetActive(false);
			}
			if (this.ModelLit && this.MaterialBurnt)
			{
				this.ModelLit.GetComponent<Renderer>().sharedMaterial = this.MaterialBurnt;
			}
		}
		else if (this.StartsLit && !LevelSerializer.IsDeserializing && !BoltNetwork.isRunning)
		{
			this.Action_LightFire();
		}
	}

	private void Update()
	{
		this.CurrentLit = this.Lit;
		this.CurrentFuel = this.Fuel;
		this.CurrentLeafDelay = this._leafDelay;
		if (this.Lit)
		{
			this.UpdateLit();
		}
		else
		{
			this.UpdateNotLit();
		}
	}

	private void OnEnable()
	{
		FMODCommon.PreloadEvents(new string[]
		{
			this.addFuelEvent
		});
		this.hasPreloaded = true;
	}

	private void OnDisable()
	{
		if (Scene.HudGui)
		{
			this.Widget.Shutdown();
		}
		if (this.hasPreloaded)
		{
			FMODCommon.UnloadEvents(new string[]
			{
				this.addFuelEvent
			});
			this.hasPreloaded = false;
		}
	}

	private void OnSerializing()
	{
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			this._lit = base.state.Lit;
		}
	}

	private IEnumerator OnDeserialized()
	{
		if (base.GetComponent<EmptyObjectIdentifier>())
		{
			UnityEngine.Object.Destroy(base.gameObject);
			yield break;
		}
		if (this._lit)
		{
			if (BoltNetwork.isRunning)
			{
				while (!base.entity.isAttached)
				{
					yield return null;
				}
			}
			if (this._scaler)
			{
				this._scaler.Awake();
				this._scaler.ResetParticleScale(Mathf.Max(Mathf.Clamp01(this.Fuel / (this.FuelMax * 0.5f)), 0.01f));
			}
			if (!BoltNetwork.isRunning)
			{
				this.On();
			}
			else if (BoltNetwork.isClient)
			{
				base.enabled = false;
				this._lit = false;
			}
		}
		yield break;
	}

	private void GrabEnter()
	{
		if (!BoltNetwork.isRunning || base.entity.isAttached)
		{
			base.enabled = true;
			this._proximity = true;
			if (!this.Lit && this.IconLightFire)
			{
				this.PositionIcon(this.IconAddLeaves);
				this.Widget.ShowSingle(LocalPlayer.Inventory.DefaultLight._itemId, this.IconAddLeaves.transform, SideIcons.Take);
			}
		}
	}

	private void GrabExit()
	{
		if (!this.Lit)
		{
			base.CancelInvoke();
		}
		base.enabled = this.Lit;
		this._proximity = false;
		this.Widget.Shutdown();
		if (this.IconLightFire)
		{
			this.IconLightFire.SetActive(false);
		}
	}

	public void Burn()
	{
		if (!this.Lit)
		{
			this.SetAlight();
		}
	}

	public void GotClean()
	{
		UnityEngine.Object.Destroy(this.IconLightFire);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void PositionIcon(GameObject icon)
	{
		if (this.IconOffset > 0f)
		{
			icon.transform.localPosition = base.transform.InverseTransformPoint(LocalPlayer.Transform.position).normalized * this.IconOffset;
		}
	}

	private void UpdateNotLit()
	{
		if (this._proximity)
		{
			this.PositionIcon(this.IconAddLeaves);
			if (TheForest.Utils.Input.GetButtonAfterDelay("Take", 0.5f, false) && this.Widget.gameObject.activeSelf)
			{
				LocalPlayer.Inventory.SpecialItems.SendMessage("LightTheFire");
				this.Widget.Shutdown();
				base.Invoke("BringBackLightFireIcon", 3f);
			}
		}
	}

	private void UpdateLit()
	{
		if (!BoltNetwork.isClient)
		{
			if (Scene.WeatherSystem.Raining)
			{
				this.Fuel -= this.DrainRateRaining * Time.deltaTime;
			}
			else
			{
				this.Fuel -= this.DrainRate * Time.deltaTime;
			}
		}
		float num = Mathf.Clamp01(this.Fuel / (this.FuelMax * 0.5f));
		if (this._leafAdded > Time.time)
		{
			Vector2 a = new Vector2(0f, this.LeafAddedExtraScale);
			Vector2 b = new Vector2(this.LeafAddedExtraScale, 0f);
			Vector2 vector = Vector2.Lerp(a, b, Mathf.Clamp01((this._leafAdded - Time.time) / 2f));
			Vector2 a2 = new Vector2(0f, this.LeafAddedExtraIntensity);
			Vector2 b2 = new Vector2(this.LeafAddedExtraIntensity, 0f);
			Vector2 vector2 = Vector2.Lerp(a2, b2, Mathf.Clamp01((this._leafAdded - Time.time) / 2f));
			if (this._leafAdded - Time.time < 1f)
			{
				float num2 = this._baseIntensity * num + vector2.x;
				if (this.LightFadeIn)
				{
					this.LightFadeIn.TargetIntensity = num2;
					if (!this.LightFadeIn.ControllingLight)
					{
						this._light.intensity = num2;
					}
				}
				else
				{
					this._light.intensity = num2;
				}
				if (this._scaler)
				{
					this._scaler.particleScale = Mathf.Max(Mathf.Min(num, this._baseScale), 0.01f) + vector.x;
				}
			}
			else
			{
				float num3 = this._baseIntensity * num + vector2.y;
				if (this.LightFadeIn)
				{
					this.LightFadeIn.TargetIntensity = num3;
					if (!this.LightFadeIn.ControllingLight)
					{
						this._light.intensity = num3;
					}
				}
				else
				{
					this._light.intensity = num3;
				}
				if (this._scaler)
				{
					this._scaler.particleScale = Mathf.Max(Mathf.Min(num, this._baseScale), 0.01f) + vector.y;
				}
			}
		}
		else
		{
			float num4 = this._baseIntensity * num;
			if (this.LightFadeIn)
			{
				this.LightFadeIn.TargetIntensity = num4;
				if (!this.LightFadeIn.ControllingLight)
				{
					this._light.intensity = num4;
				}
			}
			else
			{
				this._light.intensity = num4;
			}
			if (this._scaler)
			{
				this._scaler.particleScale = Mathf.Max(Mathf.Min(num, this._baseScale), 0.01f);
			}
		}
		if (BoltNetwork.isClient || this.Fuel > 0.01f)
		{
			if (this._proximity && this.UpdateCooking())
			{
				this.UpdateFueling(true);
			}
			return;
		}
		base.transform.parent.BroadcastMessage("CancelCook", SendMessageOptions.DontRequireReceiver);
		if (!this.InfinitePutOutChances && this._putOutChance-- <= 0)
		{
			this.DestroyFire();
			if (this._proximity)
			{
				this._proximity = false;
				this.Widget.Shutdown();
			}
			return;
		}
		this.Off();
		if (this._proximity)
		{
			if (this.CookingDisabled)
			{
				Scene.HudGui.FireStandWidget.ShowSingle(LocalPlayer.Inventory.DefaultLight._itemId, this.IconAddLeaves.transform, SideIcons.Take);
			}
			else
			{
				this.Widget.ShowSingle(LocalPlayer.Inventory.DefaultLight._itemId, this.IconAddLeaves.transform, SideIcons.Take);
			}
		}
	}

	private void UpdateFueling(bool legacy)
	{
		bool flag = this._leafDelay + this.LeafDelayTime < Time.time && this.HasFuel;
		this.PositionIcon(this.IconAddLeaves);
		if (legacy && this.CookingDisabled)
		{
			int num = this.NextFoodType();
			if (this._currentFoodItemNum != num && (TheForest.Utils.Input.GetButtonDown("Rotate") || !this.HasFuel))
			{
				LocalPlayer.Sfx.PlayWhoosh();
				this._currentFoodItemNum = num;
			}
		}
		if (flag && this._proximity)
		{
			this.Widget.ShowList(this.FuelItemId, this.IconAddLeaves.transform, SideIcons.Craft);
		}
		else
		{
			this.Widget.Shutdown();
		}
		if (flag && TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this.FuelItemId, 1, false, true))
		{
			LocalPlayer.Sfx.PlayItemCustomSfx(this.FuelItemId, true);
			this._leafDelay = Time.time;
			if (BoltNetwork.isRunning)
			{
				FireAddFuelEvent fireAddFuelEvent = FireAddFuelEvent.Create(GlobalTargets.Everyone);
				fireAddFuelEvent.Target = base.entity;
				fireAddFuelEvent.Send();
			}
			else
			{
				this.Action_AddFuel();
			}
		}
	}

	private bool UpdateCooking()
	{
		if (this.CookingDisabled)
		{
			return true;
		}
		int num;
		if (!LocalPlayer.Inventory.IsRightHandEmpty())
		{
			num = this.foodItems.FindIndex((Fire2.FoodItem fi) => fi._itemId == LocalPlayer.Inventory.RightHand._itemId);
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
				LocalPlayer.Sfx.PlayItemCustomSfx(this.foodItems[num2]._itemId, true);
				DecayingInventoryItemView.DecayStates decayStateValue = DecayingInventoryItemView.DecayStates.Fresh;
				if (DecayingInventoryItemView.LastUsed)
				{
					decayStateValue = DecayingInventoryItemView.LastUsed._prevState;
					DecayingInventoryItemView.LastUsed = null;
				}
				this.SpawnFoodPrefab(this.foodItems[num2]._cookPrefab, false, decayStateValue);
				this.Widget.Shutdown();
			}
			else
			{
				this.Widget.ShowSingle(this.CurrentFoodItemId, this.CurrentFoodIcon.transform, SideIcons.Craft);
			}
		}
		else if (!LocalPlayer.Inventory.IsRightHandEmpty() && LocalPlayer.Inventory.RightHand._itemId == this.waterCleaningItem._itemId)
		{
			this.PositionIcon(this.IconCookHeld);
			if (TheForest.Utils.Input.GetButtonDown("Craft"))
			{
				WeaponStatUpgrade.Types activeBonus = LocalPlayer.Inventory.RightHand.ActiveBonus;
				LocalPlayer.Inventory.RightHand.ActiveBonus = (WeaponStatUpgrade.Types)(-1);
				LocalPlayer.Inventory.UnequipItemAtSlot(Item.EquipmentSlot.RightHand, false, false, true);
				LocalPlayer.Sfx.PlayItemCustomSfx(this.waterCleaningItem._itemId, true);
				this.SpawnFoodPrefab(this.waterCleaningItem._cookPrefab, true, (DecayingInventoryItemView.DecayStates)activeBonus);
				this.Widget.Shutdown();
			}
			else
			{
				this.Widget.ShowSingle(this.waterCleaningItem._itemId, this.IconAddLeaves.transform, SideIcons.Craft);
			}
		}
		else if (this.RotateIcon)
		{
			bool flag;
			if (this._currentFoodItemNum < 0)
			{
				flag = this.HasFuel;
				this.UpdateFueling(false);
			}
			else
			{
				flag = LocalPlayer.Inventory.Owns(this.CurrentFoodItemId, true);
				if (flag)
				{
					if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this.CurrentFoodItemId, 1, true, true))
					{
						LocalPlayer.Sfx.PlayItemCustomSfx(this.CurrentFoodItemId, true);
						DecayingInventoryItemView.DecayStates decayStateValue2 = DecayingInventoryItemView.DecayStates.Fresh;
						if (DecayingInventoryItemView.LastUsed)
						{
							decayStateValue2 = DecayingInventoryItemView.LastUsed._prevState;
							DecayingInventoryItemView.LastUsed = null;
						}
						this.SpawnFoodPrefab(this.CurrentFoodprefab, false, decayStateValue2);
					}
					else
					{
						this.PositionIcon(this.IconAddLeaves);
						this.Widget.ShowList(this.CurrentFoodItemId, this.IconAddLeaves.transform, SideIcons.Craft);
					}
				}
			}
			int num3 = this.NextFoodType();
			if (this._currentFoodItemNum != num3)
			{
				this.PositionIcon(this.RotateIcon);
				if (TheForest.Utils.Input.GetButtonDown("Rotate") || !flag)
				{
					LocalPlayer.Sfx.PlayWhoosh();
					this._currentFoodItemNum = num3;
					if (this._currentFoodItemNum > -1)
					{
						this.Widget.ShowList(this.CurrentFoodItemId, this.IconAddLeaves.transform, SideIcons.Craft);
					}
				}
			}
			else if (!flag)
			{
				this.Widget.Shutdown();
			}
			return false;
		}
		return !this.RotateIcon;
	}

	private int NextFoodType()
	{
		int num = this.foodItems.Length + 2;
		int num2 = this._currentFoodItemNum;
		while (num-- > 0)
		{
			num2++;
			if (num2 < 0)
			{
				if (LocalPlayer.Inventory.Owns(this.LeafItemId, false))
				{
					break;
				}
			}
			else if (num2 < this.foodItems.Length)
			{
				num2 = (int)Mathf.Repeat((float)num2, (float)this.foodItems.Length);
				if (!this.CookingDisabled && LocalPlayer.Inventory.Owns(this.foodItems[num2]._itemId, true))
				{
					break;
				}
			}
			else
			{
				num2 = -2;
				if (LocalPlayer.Inventory.Owns(this.CashItemId, false))
				{
					break;
				}
			}
		}
		return num2;
	}

	private void SpawnFoodPrefab(Cook foodPrefab, bool center, DecayingInventoryItemView.DecayStates decayStateValue)
	{
		if (this.CookingDisabled)
		{
			return;
		}
		Vector3 position = base.transform.position + new Vector3((!center) ? UnityEngine.Random.Range(-0.2f, 0.2f) : 0f, 1.25f, (!center) ? UnityEngine.Random.Range(-0.2f, 0.2f) : 0f);
		if (!BoltNetwork.isRunning)
		{
			Cook cook = UnityEngine.Object.Instantiate<Cook>(foodPrefab, position, Quaternion.identity);
			cook.transform.parent = base.transform.parent;
			cook.transform.Rotate(0f, (float)UnityEngine.Random.Range(0, 359), 0f);
			if (decayStateValue > DecayingInventoryItemView.DecayStates.None && decayStateValue <= DecayingInventoryItemView.DecayStates.DriedSpoilt)
			{
				cook.SetDecayState(decayStateValue);
			}
			else
			{
				cook.SendMessage("SetActiveBonus", (WeaponStatUpgrade.Types)decayStateValue, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			PlaceDryingFood placeDryingFood = PlaceDryingFood.Create(GlobalTargets.OnlyServer);
			placeDryingFood.PrefabId = foodPrefab.GetComponent<BoltEntity>().prefabId;
			placeDryingFood.Position = position;
			placeDryingFood.Rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 359), 0f);
			placeDryingFood.Parent = base.transform.parent.GetComponent<BoltEntity>();
			placeDryingFood.DecayState = (int)decayStateValue;
			placeDryingFood.Send();
		}
	}

	private void DestroyFire()
	{
		if (this.ModelLit && this.MaterialBurnt)
		{
			this.ModelLit.GetComponent<Renderer>().sharedMaterial = this.MaterialBurnt;
		}
		if (BoltNetwork.isRunning && base.entity)
		{
			DestroyWithTag destroyWithTag = DestroyWithTag.Create(GlobalTargets.OnlyServer);
			destroyWithTag.Entity = base.entity;
			destroyWithTag.Send();
		}
		else
		{
			this.FlamesAll.BroadcastMessage("PerformDestroy", false, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void BringBackLightFireIcon()
	{
		if (this._proximity && !this.Lit)
		{
			this.PositionIcon(this.IconAddLeaves);
			this.Widget.ShowSingle(LocalPlayer.Inventory.DefaultLight._itemId, this.IconAddLeaves.transform, SideIcons.Take);
		}
	}

	private void receiveLightFire()
	{
		if (base.enabled)
		{
			this.SetAlight();
		}
	}

	private void SetAlight()
	{
		if (BoltNetwork.isRunning)
		{
			FireLightEvent fireLightEvent = FireLightEvent.Create(GlobalTargets.OnlyServer);
			fireLightEvent.Target = base.entity;
			fireLightEvent.Send();
		}
		else
		{
			this.Action_LightFire();
		}
	}

	public void Action_LightFire()
	{
		if (BoltNetwork.isClient)
		{
			Debug.LogError("Action_LightFire: Should never be called on the client!");
			return;
		}
		if (!this.Lit)
		{
			if (this.Fuel < 5f)
			{
				this.Fuel = 10f;
				if (this._scaler)
				{
					this._scaler.Awake();
					this._scaler.ResetParticleScale(Mathf.Clamp01(this.Fuel / (this.FuelMax * 0.5f)));
				}
			}
			FMODCommon.PlayOneshotNetworked(this.addFuelEvent, base.transform, FMODCommon.NetworkRole.Any);
			this.On();
		}
	}

	public void Action_AddFuel()
	{
		this.Fuel += (float)UnityEngine.Random.Range(10, 30);
		this.AddFuel_Complete();
	}

	public void AddFuel_Complete()
	{
		this._leafAdded = Time.time + 2f;
		FMODCommon.PlayOneshot(this.addFuelEvent, base.transform);
	}

	private void Action_EnableFireDamage()
	{
		if (this.Lit && this.FireDamage)
		{
			this.FireDamage.SetActive(true);
		}
	}

	private void On()
	{
		base.CancelInvoke();
		bool flag = true;
		base.enabled = flag;
		this.Lit = flag;
		this.SetFlames(true);
		if (this._light)
		{
			this._light.intensity = 0f;
		}
		if (this.IconLightFire)
		{
			this.IconLightFire.SetActive(false);
		}
		if (this.ModelLit)
		{
			this.ModelLit.SetActive(true);
			if (this.MaterialAlight)
			{
				this.ModelLit.GetComponent<Renderer>().sharedMaterial = this.MaterialAlight;
			}
		}
		if (this.ModelUnlit)
		{
			this.ModelUnlit.SetActive(false);
		}
		base.Invoke("Action_EnableFireDamage", 2f);
		Collider component = base.GetComponent<Collider>();
		if (component)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}

	private void Off()
	{
		bool flag = false;
		base.enabled = flag;
		this.Lit = (this.CurrentLit = flag);
		this.SetFlames(false);
		if (this.FireDamage)
		{
			this.FireDamage.SetActive(false);
		}
		if (this.ModelLit && this.MaterialBurnt)
		{
			this.ModelLit.GetComponent<Renderer>().sharedMaterial = this.MaterialBurnt;
		}
		Collider component = base.GetComponent<Collider>();
		if (component)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}

	private void SetFlames(bool active)
	{
		if (this.FlamesAll)
		{
			this.FlamesAll.SetActive(active);
		}
		if (this.FearTrigger)
		{
			this.FearTrigger.SetActive(active);
		}
	}

	public override void Attached()
	{
		IFireState state = base.state;
		state.OnFuelAdded = (Action)Delegate.Combine(state.OnFuelAdded, new Action(this.AddFuel_Complete));
		base.state.AddCallback("Lit", delegate
		{
			if (BoltNetwork.isClient)
			{
				if (base.state.Lit)
				{
					this.On();
				}
				else
				{
					base.transform.parent.BroadcastMessage("CancelCook", SendMessageOptions.DontRequireReceiver);
					this.Off();
				}
			}
		});
		if (base.entity.isOwner)
		{
			base.state.Fuel = this.FuelStart;
			base.state.Lit = this._lit;
			if (this._lit)
			{
				this.On();
			}
			else if (this.StartsLit)
			{
				this.Action_LightFire();
			}
		}
	}

	public override void Detached()
	{
		if (base.entity.detachToken is CoopDestroyTagToken)
		{
			this._tag.Perform(true);
		}
	}

	public bool Lit
	{
		get
		{
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached)
			{
				return base.state.Lit;
			}
			return this._lit;
		}
		set
		{
			if (BoltNetwork.isRunning)
			{
				base.state.Lit = value;
			}
			else
			{
				this._lit = value;
			}
		}
	}

	private float Fuel
	{
		get
		{
			if (BoltNetwork.isRunning && base.entity.isAttached)
			{
				return base.state.Fuel;
			}
			return this._fuel;
		}
		set
		{
			value = Mathf.Clamp(value, 0.0001f, this.FuelMax);
			if (BoltNetwork.isRunning)
			{
				base.state.Fuel = value;
			}
			else
			{
				this._fuel = value;
			}
		}
	}

	private int FuelItemId
	{
		get
		{
			return (this._currentFoodItemNum != -2) ? this.LeafItemId : this.CashItemId;
		}
	}

	private bool HasFuel
	{
		get
		{
			return LocalPlayer.Inventory.Owns(this.FuelItemId, false);
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
			return (!this.foodItems[this._currentFoodItemNum]._icon) ? this.IconCookHeld : this.foodItems[this._currentFoodItemNum]._icon;
		}
	}

	private ItemListWidget Widget
	{
		get
		{
			return (!this.CookingDisabled) ? Scene.HudGui.FireWidget : Scene.HudGui.FireStandWidget;
		}
	}

	public Fire2.FoodItem[] foodItems;

	public Fire2.FoodItem waterCleaningItem;

	[ItemIdPicker]
	public int CashItemId = 38;

	[ItemIdPicker]
	public int LeafItemId;

	[Header("Info")]
	public bool CurrentLit;

	public float CurrentFuel;

	public float CurrentLeafDelay;

	public bool isBonfire;

	[Header("Settings")]
	public bool InfinitePutOutChances;

	public bool CookingDisabled;

	public bool StartsLit;

	public float FuelStart = 120f;

	public float FuelMax = 120f;

	public float DrainRate;

	public float DrainRateRaining;

	public float LeafDelayTime = 2f;

	public float LeafAddedExtraScale = 1f;

	public float LeafAddedExtraIntensity = 1f;

	public float IconOffset;

	[Header("References")]
	public Material MaterialBurnt;

	public Material MaterialAlight;

	public GameObject RotateIcon;

	public GameObject ModelLit;

	public GameObject ModelUnlit;

	public GameObject IconCookHeld;

	public GameObject IconAddLeaves;

	public GameObject IconLightFire;

	public GameObject FireLight;

	public GameObject FireDamage;

	public GameObject FlamesAll;

	public GameObject FlamesScale;

	public GameObject FearTrigger;

	public LightFadeIn LightFadeIn;

	[Header("FMOD")]
	public string addFuelEvent;

	private bool hasPreloaded;

	[SerializeThis]
	private bool _lit;

	private bool _proximity;

	private int _currentFoodItemNum = -2;

	[SerializeThis]
	private float _fuel;

	private float _leafDelay;

	private float _leafAdded;

	[SerializeThis]
	private int _putOutChance = 2;

	private Light _light;

	private ParticleScaler _scaler;

	[SerializeField]
	private DestroyOnContactWithTag _tag;

	private float _baseScale;

	private float _baseIntensity;

	[Serializable]
	public class FoodItem
	{
		[ItemIdPicker]
		public int _itemId;

		public Cook _cookPrefab;

		public GameObject _icon;
	}
}
