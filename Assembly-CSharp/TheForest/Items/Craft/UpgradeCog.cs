using System;
using System.Collections.Generic;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.Craft
{
	[AddComponentMenu("Items/Craft/Upgrade Cog")]
	public class UpgradeCog : MonoBehaviour
	{
		public void Awake()
		{
			if (this._supportedItemsCache == null)
			{
				this._supportedItemsCache = new Dictionary<int, UpgradeCogItems>();
				foreach (UpgradeCogItems upgradeCogItems in this._supportedItems)
				{
					this._supportedItemsCache[upgradeCogItems._itemId] = upgradeCogItems;
				}
				this._craftingCog = base.GetComponent<CraftingCog>();
				base.enabled = false;
			}
		}

		private void Update()
		{
			float num = (!this._fastForward) ? (Time.realtimeSinceStartup - this._stateStartTime) : float.MaxValue;
			switch (this._state)
			{
			case UpgradeCog.States.Idle:
				Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
				Scene.HudGui.ManualUpgradingInfo.SetActive(false);
				this._disableWhenActive.SetActive(true);
				this._craftingCog.enabled = true;
				base.enabled = false;
				break;
			case UpgradeCog.States.MoveUp:
				num = MathEx.Easing.EaseInOutQuad(num, 0f, 1f, this._moveUpDuration);
				this._craftView.transform.position = Vector3.Slerp(this._downPosition, this._upPositionV, num);
				this._craftView.transform.rotation = Quaternion.Slerp(this._downRotation, this._upRotation, num);
				if (Mathf.Approximately(num, 1f))
				{
					this._stateStartTime = Time.realtimeSinceStartup;
					Scene.HudGui.ManualUpgradingInfo.SetActive(true);
					this._failCount = 0;
					this._state = ((!this._autoPlace) ? UpgradeCog.States.Aiming : UpgradeCog.States.Applying);
				}
				break;
			case UpgradeCog.States.Aiming:
				if (!this._currentReceiver)
				{
					this.NextIngredient();
				}
				else if (this._implantsDone < this._totalIngredientImplants)
				{
					float num2 = TheForest.Utils.Input.GetAxis("Vertical") * (float)((!PlayerPreferences.MouseInvert) ? 1 : -1);
					float num3 = -TheForest.Utils.Input.GetAxis("Horizontal") * (float)((!PlayerPreferences.MouseInvert) ? 1 : -1);
					if (Mathf.Abs(num2) > 0.05f || Mathf.Abs(num3) > 0.05f)
					{
						if (Mathf.Abs(num2) > 0.05f)
						{
							this._craftView.transform.RotateAround(this._currentReceiver._centerAndRotationAxisUpDirection.position, LocalPlayer.InventoryCam.transform.right, Time.unscaledDeltaTime * 150f * num2);
						}
						if (Mathf.Abs(num3) > 0.05f)
						{
							this._craftView.transform.RotateAround(this._currentReceiver._centerAndRotationAxisUpDirection.position, LocalPlayer.InventoryCam.transform.up, Time.unscaledDeltaTime * 150f * num3);
						}
						this.ThrowUpgradePart(true);
					}
					if (this.ThrowUpgradePart(true) && TheForest.Utils.Input.GetButtonDown("Fire1"))
					{
						Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
						this._state = UpgradeCog.States.Applying;
					}
					if (TheForest.Utils.Input.GetButtonDown("AltFire"))
					{
						Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
						Scene.HudGui.ManualUpgradingInfo.SetActive(false);
						this._stateStartTime = Time.realtimeSinceStartup;
						this._state = UpgradeCog.States.MoveDown;
					}
				}
				else if (!this.TryPayMoreIngredients())
				{
					this.NextIngredient();
				}
				break;
			case UpgradeCog.States.Applying:
				if (this._implantsDone < this._totalIngredientImplants)
				{
					LocalPlayer.Sfx.PlayWhoosh();
					if (this.ThrowUpgradePart(false))
					{
						this._failCount = 0;
						if (this._autoPlace)
						{
							bool flag = Vector3.Dot(this._currentReceiver._centerAndRotationAxisUpDirection.right, this._currentUpgradePartTr.position - this._currentReceiver._centerAndRotationAxisUpDirection.position) < 0f;
							if (this._rotateAngle > 60f)
							{
								this._currentUpgradePartTr.gameObject.SetActive(false);
								if (!flag)
								{
									this._rotateAngle = -this._rotateAngle;
								}
								this._rotateAxis = this._currentReceiver._centerAndRotationAxisUpDirection.up;
								this._stateStartTime = 0f;
								this._state = UpgradeCog.States.Rotating;
								break;
							}
						}
						this._stateStartTime = Time.realtimeSinceStartup;
						this._state = UpgradeCog.States.Implanting;
					}
					else if (++this._failCount + this._implantsDone > this._totalIngredientImplants)
					{
						this.NextIngredient();
					}
					else if (!this._autoPlace)
					{
						this._state = UpgradeCog.States.Aiming;
					}
				}
				else
				{
					this.NextIngredient();
				}
				break;
			case UpgradeCog.States.Rotating:
			{
				float num4 = (!this._fastForward) ? (Time.unscaledDeltaTime / this._rotationDuration) : 1f;
				this._craftView.transform.RotateAround(this._currentReceiver._centerAndRotationAxisUpDirection.position, this._rotateAxis, this._rotateAngle * num4);
				this._circlePatternRotation *= Quaternion.AngleAxis(this._rotateAngle * num4, this._rotateAxis);
				this._stateStartTime += Time.unscaledDeltaTime;
				if (this._stateStartTime >= this._rotationDuration || this._fastForward)
				{
					if (this._autoPlace)
					{
						this._currentUpgradePartTr.gameObject.SetActive(true);
					}
					this._failCount = 0;
					this._stateStartTime = Time.realtimeSinceStartup;
					this._state = ((!this._autoPlace) ? UpgradeCog.States.Aiming : UpgradeCog.States.Implanting);
				}
				break;
			}
			case UpgradeCog.States.Implanting:
				num = Mathf.Clamp01(num / this._singleImplantDuration);
				this._currentUpgradePartTr.localPosition = Vector3.Slerp(this._upgradePartStartPosition, this._upgradePartTargetPosition, num);
				if (Mathf.Approximately(num, 1f))
				{
					this._currentUpgradePartTr.localPosition = this._upgradePartTargetPosition;
					this._currentReceiver.AddView(this._currentIngredientItemId, this._currentUpgradePartTr);
					this._implantsDone++;
					if (this.SupportedItemsCache[this._currentIngredientItemId]._amount == 1 || this._implantsDone % this.SupportedItemsCache[this._currentIngredientItemId]._amount == 1)
					{
						this.ApplyBonus();
						Scene.HudGui.ShowUpgradesDistribution(this._recipe._productItemID, this._recipe._ingredients[1]._itemID, 1);
					}
					this._state = ((!this._autoPlace) ? UpgradeCog.States.Aiming : UpgradeCog.States.Applying);
					LocalPlayer.Sfx.PlayUpgradeSuccess(this._currentUpgradePartTr.gameObject);
				}
				break;
			case UpgradeCog.States.MoveDown:
				num = MathEx.Easing.EaseInOutQuad(num, 0f, 1f, this._moveUpDuration);
				this._craftView.transform.position = Vector3.Slerp(this._upPositionV, this._downPosition, num);
				this._craftView.transform.rotation = Quaternion.Slerp(this._upRotation, this._downRotation, num);
				if (Mathf.Approximately(num, 1f))
				{
					this.Shutdown();
				}
				break;
			}
		}

		public void Shutdown()
		{
			if (this.SupportedItemsCache[this._currentIngredientItemId]._amount > 0)
			{
				float num = (float)((this._totalIngredientImplants - this._implantsDone) / this.SupportedItemsCache[this._currentIngredientItemId]._amount);
				for (int i = 1; i < this._recipe._ingredients.Length; i++)
				{
					this._craftingCog.Add(this._recipe._ingredients[i]._itemID, Mathf.FloorToInt((float)this._recipe._ingredients[i]._amount * num), null);
				}
			}
			if (this._currentReceiver != null)
			{
				if (this._disableMainColliderAfter)
				{
					this._currentReceiver._mainCollider.enabled = false;
					this._disableMainColliderAfter = false;
				}
				if (this._disableFilterColliderAfter)
				{
					this._currentReceiver._filterCollider.enabled = false;
					this._disableFilterColliderAfter = false;
				}
				this._currentReceiver = null;
			}
			this._currentReceiver = null;
			this._craftView.transform.position = Vector3.Slerp(this._upPositionV, this._downPosition, 1f);
			this._craftView.transform.rotation = Quaternion.Slerp(this._upRotation, this._downRotation, 1f);
			this._state = UpgradeCog.States.Idle;
			this._disableWhenActive.SetActive(true);
			base.enabled = false;
			this._craftingCog.enabled = true;
			this._craftingCog.CheckForValidRecipe();
			Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
			Scene.HudGui.ManualUpgradingInfo.SetActive(false);
			Scene.HudGui.HideUpgradesDistribution();
		}

		public void ApplyUpgradeRecipe(InventoryItemView craftView, Receipe receipe, int amount)
		{
			this._currentIngredientIndex = 0;
			this._recipe = receipe;
			this._craftView = craftView;
			this._currentReceiver = null;
			this._receivers = this._craftView.GetAllComponentsInChildren<UpgradeViewReceiver>();
			Transform parent = craftView.transform.parent;
			Transform child = craftView.transform.GetChild(0);
			this._downPosition = this._craftView.transform.position;
			this._downRotation = this._craftView.transform.rotation;
			child.parent = this._upPosition;
			craftView.transform.parent = child;
			child.localPosition = Vector3.zero;
			child.localRotation = Quaternion.identity;
			this._upRotation = this._craftView.transform.rotation;
			this._upPositionV = craftView.transform.position;
			craftView.transform.parent = parent;
			child.parent = craftView.transform;
			child.SetAsFirstSibling();
			craftView.transform.position = this._downPosition;
			craftView.transform.rotation = this._downRotation;
			this._state = UpgradeCog.States.MoveUp;
			this._stateStartTime = Time.realtimeSinceStartup;
			this._totalUpgrades = amount;
			this._implantsDone = 0;
			this._totalIngredientImplants = 0;
			this._craftingCog.enabled = false;
			this._disableWhenActive.SetActive(false);
			base.enabled = true;
			Scene.HudGui.ShowValidCraftingRecipes(null);
			Scene.HudGui.ShowUpgradesDistribution(this._recipe._productItemID, this._recipe._ingredients[1]._itemID, 1);
		}

		private bool TryPayMoreIngredients()
		{
			for (int i = 1; i < this._recipe._ingredients.Length; i++)
			{
				if (LocalPlayer.Inventory.AmountOf(this._recipe._ingredients[i]._itemID, false) < this._recipe._ingredients[i]._amount)
				{
					return false;
				}
			}
			int maxUpgradesAmount = ItemDatabase.ItemById(this._recipe._productItemID)._maxUpgradesAmount;
			int amountOfUpgrades = LocalPlayer.Inventory.GetAmountOfUpgrades(this._recipe._productItemID);
			if (amountOfUpgrades < maxUpgradesAmount)
			{
				for (int j = 1; j < this._recipe._ingredients.Length; j++)
				{
					LocalPlayer.Inventory.RemoveItem(this._recipe._ingredients[j]._itemID, this._recipe._ingredients[j]._amount, false, true);
				}
				this._totalUpgrades++;
				this._totalIngredientImplants = this._totalUpgrades * this._recipe._ingredients[this._currentIngredientIndex]._amount * this.SupportedItemsCache[this._currentIngredientItemId]._amount;
				return true;
			}
			return false;
		}

		private void NextIngredient()
		{
			if (this._currentReceiver != null)
			{
				if (this._disableMainColliderAfter)
				{
					this._currentReceiver._mainCollider.enabled = false;
					this._disableMainColliderAfter = false;
				}
				if (this._disableFilterColliderAfter)
				{
					this._currentReceiver._filterCollider.enabled = false;
					this._disableFilterColliderAfter = false;
				}
				this._currentReceiver = null;
			}
			if (++this._currentIngredientIndex < this._recipe._ingredients.Length)
			{
				if (this._supportedItemsCache.ContainsKey(this._recipe._ingredients[this._currentIngredientIndex]._itemID))
				{
					this._currentIngredientItemId = this._recipe._ingredients[this._currentIngredientIndex]._itemID;
					foreach (UpgradeViewReceiver upgradeViewReceiver in this._receivers)
					{
						foreach (int num in upgradeViewReceiver._acceptedItemIds)
						{
							if (num == this._currentIngredientItemId)
							{
								this._currentReceiver = upgradeViewReceiver;
								break;
							}
						}
						if (this._currentReceiver != null)
						{
							break;
						}
					}
					if (this._currentReceiver != null)
					{
						this._implantsDone = 0;
						this._totalIngredientImplants = this._totalUpgrades * this._recipe._ingredients[this._currentIngredientIndex]._amount * this.SupportedItemsCache[this._currentIngredientItemId]._amount;
						int amountOfUpgrades = this._craftingCog._inventory.GetAmountOfUpgrades(this._recipe._productItemID, this._currentIngredientItemId);
						if (this._totalIngredientImplants + amountOfUpgrades >= this.SupportedItemsCache[this._currentIngredientItemId]._maxViewsPerItem)
						{
							this._totalIngredientImplants = this.SupportedItemsCache[this._currentIngredientItemId]._maxViewsPerItem - amountOfUpgrades;
						}
						this._stateStartTime = Time.realtimeSinceStartup;
						if (!this._currentReceiver._mainCollider.enabled)
						{
							this._currentReceiver._mainCollider.enabled = true;
							this._disableMainColliderAfter = true;
						}
						if (!this._currentReceiver._filterCollider.enabled)
						{
							this._currentReceiver._filterCollider.enabled = true;
							this._disableFilterColliderAfter = true;
						}
						if (this._currentReceiver._filterCollider is CapsuleCollider)
						{
							CapsuleCollider capsuleCollider = (CapsuleCollider)this._currentReceiver._filterCollider;
							this._circlePatternCapsuleFilterPivot = this.GetCapsuleAxis(this._currentReceiver, capsuleCollider) * ((float)UnityEngine.Random.Range(-1, 1) * (capsuleCollider.height / 2f)) + capsuleCollider.center;
						}
						this._circlePatternRotation = UnityEngine.Random.rotation;
					}
					else
					{
						Debug.Log(string.Concat(new string[]
						{
							"No upgrade receiver for '",
							ItemDatabase.ItemById(this._recipe._ingredients[this._currentIngredientIndex]._itemID)._name,
							"' found on '",
							this._craftView.name,
							"' (for '",
							this._recipe._name,
							"')"
						}));
					}
				}
				else
				{
					Debug.Log(string.Concat(new string[]
					{
						"Unsupported item used in upgrade receipe : '",
						this._recipe._name,
						"', UpgradeCog is missing view for '",
						ItemDatabase.ItemById(this._recipe._ingredients[this._currentIngredientIndex]._itemID)._name,
						"'"
					}));
				}
			}
			else
			{
				this._stateStartTime = Time.realtimeSinceStartup;
				this._state = UpgradeCog.States.MoveDown;
			}
		}

		private void SpawnDebug(Vector3 outterPos, Vector3 dir, Color col)
		{
			if (!this._spawnDebug)
			{
				return;
			}
			if (GameObject.Find("DebugUpgrades"))
			{
				UnityEngine.Object.Destroy(GameObject.Find("DebugUpgrades"));
			}
			GameObject gameObject = new GameObject("DebugUpgrades");
			gameObject.transform.position = outterPos;
			gameObject.layer = 23;
			GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject2.transform.position = outterPos;
			gameObject2.transform.localScale *= 0.16f;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.name = "outter";
			gameObject2.GetComponent<Renderer>().material.color = col;
			gameObject2.layer = 23;
			UnityEngine.Object.Destroy(gameObject2.GetComponent<Collider>());
			GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject3.transform.position = outterPos + dir;
			gameObject3.transform.localScale *= 0.1f;
			gameObject3.transform.parent = gameObject.transform;
			gameObject3.name = "outter + dir";
			gameObject3.GetComponent<Renderer>().material.color = col / 2f;
			gameObject3.layer = 23;
			UnityEngine.Object.Destroy(gameObject3.GetComponent<Collider>());
			GameObject gameObject4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			gameObject4.transform.position = outterPos + dir / 2f;
			gameObject4.transform.LookAt(outterPos + dir);
			gameObject4.transform.LookAt(gameObject4.transform.position + gameObject4.transform.up);
			gameObject4.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
			gameObject4.transform.parent = gameObject.transform;
			gameObject4.name = "outter to dir";
			gameObject4.GetComponent<Renderer>().material.color = col;
			gameObject4.layer = 23;
			UnityEngine.Object.Destroy(gameObject4.GetComponent<Collider>());
			if (this._breakAfterSpawnDebug)
			{
				Debug.Break();
			}
		}

		private bool ThrowUpgradePart(bool preview)
		{
			if (this._supportedItemsCache[this._currentIngredientItemId]._pattern == UpgradeCog.Patterns.NoView)
			{
				Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
				return false;
			}
			Ray ray;
			if (this._autoPlace)
			{
				Vector3 vector2;
				Vector3 vector;
				if (this._currentReceiver._filterCollider is CapsuleCollider)
				{
					vector = this.OuterPositionFromCapsule(out vector2);
					Debug.Log(string.Concat(new object[]
					{
						"Capsule outterPosition=",
						vector,
						" direction=",
						vector2
					}));
					Debug.DrawRay(vector, vector2, Color.blue, 10f, true);
					this.SpawnDebug(vector, vector2, Color.blue);
				}
				else
				{
					vector = this.OuterPositionFromSphere(out vector2);
					Debug.DrawRay(vector, vector2, Color.red, 10f, true);
					this.SpawnDebug(vector, vector2, Color.red);
				}
				ray = new Ray(vector, vector2.normalized);
			}
			else
			{
				ray = LocalPlayer.InventoryCam.ScreenPointToRay(TheForest.Utils.Input.mousePosition);
			}
			RaycastHit raycastHit;
			if (Physics.SphereCast(ray, 0.1f, out raycastHit, 100f, 1 << this._currentReceiver._mainCollider.gameObject.layer, QueryTriggerInteraction.Ignore) && raycastHit.collider == this._currentReceiver._mainCollider)
			{
				if (preview)
				{
					Scene.HudGui.UpgradePlacementGizmo.position = raycastHit.point;
					Scene.HudGui.UpgradePlacementGizmo.rotation = Quaternion.LookRotation(raycastHit.normal);
					Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(true);
				}
				else
				{
					Vector3 vector = raycastHit.point + raycastHit.normal * 2f;
					Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
					this._currentUpgradePartTr = UnityEngine.Object.Instantiate<Transform>(this._supportedItemsCache[this._currentIngredientItemId]._prefab, vector, Quaternion.LookRotation(raycastHit.normal));
					if (this._supportedItemsCache[this._currentIngredientItemId]._pointDownwards)
					{
						this._currentUpgradePartTr.forward = -this._currentReceiver._centerAndRotationAxisUpDirection.up;
					}
					this._currentUpgradePartTr.parent = this._currentReceiver.transform;
					this._upgradePartStartPosition = this._currentReceiver.transform.InverseTransformPoint(vector);
					this._upgradePartTargetPosition = this._currentReceiver.transform.InverseTransformPoint(raycastHit.point);
					if (this._autoPlace)
					{
						this._rotateAngle = Vector3.Angle(Vector3.up, vector - raycastHit.point);
					}
				}
				return true;
			}
			Scene.HudGui.UpgradePlacementGizmo.gameObject.SetActive(false);
			return false;
		}

		private Vector3 GetOutterPosition()
		{
			return LocalPlayer.InventoryCam.transform.position + Quaternion.Euler(this._upgradeRayOriginAngles) * Vector3.forward;
		}

		private Vector3 OuterPositionFromSphere(out Vector3 direction)
		{
			float magnitude = Vector3.Scale(this._currentReceiver._filterCollider.bounds.extents, this._currentReceiver.transform.localScale).magnitude;
			Vector3 center = this._currentReceiver._filterCollider.bounds.center;
			Vector3 a;
			if (this._supportedItemsCache[this._currentIngredientItemId]._pattern == UpgradeCog.Patterns.FullRandom)
			{
				a = UnityEngine.Random.onUnitSphere * magnitude;
			}
			else
			{
				float f;
				if (this._supportedItemsCache[this._currentIngredientItemId]._pattern == UpgradeCog.Patterns.Circle)
				{
					f = (float)(this._implantsDone + this._failCount) / (float)this._totalIngredientImplants * 2f * 3.14159274f;
				}
				else
				{
					f = (float)(this._implantsDone + this._failCount) * this._supportedItemsCache[this._currentIngredientItemId]._alignedPatternExtents * 2f * 3.14159274f;
				}
				a = this._circlePatternRotation * new Vector3(Mathf.Cos(f) * magnitude, Mathf.Sin(f) * magnitude, 0f);
			}
			direction = Vector3.Scale(-a.normalized, new Vector3(UnityEngine.Random.Range(0.98f, 1.02f), UnityEngine.Random.Range(0.98f, 1.02f), UnityEngine.Random.Range(0.98f, 1.02f)));
			return a + center;
		}

		private Vector3 OuterPositionFromCapsule(out Vector3 direction)
		{
			CapsuleCollider capsuleCollider = (CapsuleCollider)this._currentReceiver._filterCollider;
			Vector3 vector;
			if (this._supportedItemsCache[this._currentIngredientItemId]._pattern == UpgradeCog.Patterns.FullRandom)
			{
				Vector3 capsuleAxis = this.GetCapsuleAxis(this._currentReceiver, capsuleCollider);
				vector = capsuleAxis * ((float)UnityEngine.Random.Range(-1, 1) * (capsuleCollider.height / 2f)) + capsuleCollider.center;
				direction = Vector3.Scale(UnityEngine.Random.onUnitSphere, Vector3.one - capsuleAxis).normalized * -capsuleCollider.radius;
				vector -= direction;
			}
			else
			{
				vector = this._circlePatternCapsuleFilterPivot;
				vector += this._circlePatternRotation * new Vector3(Mathf.Cos((float)this._implantsDone / (float)this._totalUpgrades * 2f * 3.14159274f) * capsuleCollider.radius, Mathf.Sin((float)this._implantsDone / (float)this._totalUpgrades * 2f * 3.14159274f) * capsuleCollider.radius, 0f);
				direction = this._circlePatternCapsuleFilterPivot - vector;
			}
			return vector + this._currentReceiver.transform.position;
		}

		private Vector3 GetCapsuleAxis(UpgradeViewReceiver receiver, CapsuleCollider cc)
		{
			switch (cc.direction)
			{
			case 0:
				return receiver.transform.right;
			case 1:
				return receiver.transform.up;
			}
			return receiver.transform.forward;
		}

		private void ApplyBonus()
		{
			this._craftingCog.ApplyWeaponStatsUpgrades(this._recipe._productItemID, this._currentIngredientItemId, this.SupportedItemsCache[this._currentIngredientItemId]._weaponStatUpgrades, false, 1, null);
			this._craftingCog._inventory.AddUpgradeToCounter(this._recipe._productItemID, this._currentIngredientItemId, 1);
		}

		public Dictionary<int, UpgradeCogItems> SupportedItemsCache
		{
			get
			{
				return this._supportedItemsCache;
			}
		}

		public List<UpgradeCogItems> _supportedItems;

		public float _moveUpDuration;

		public float _moveDownDuration;

		public float _singleImplantDuration;

		public float _rotationDuration;

		public Transform _upPosition;

		public GameObject _disableWhenActive;

		public bool _autoPlace = true;

		private Dictionary<int, UpgradeCogItems> _supportedItemsCache;

		private UpgradeCog.States _state;

		private CraftingCog _craftingCog;

		private Receipe _recipe;

		private InventoryItemView _craftView;

		private UpgradeViewReceiver[] _receivers;

		private UpgradeViewReceiver _currentReceiver;

		private int _currentIngredientIndex;

		private int _currentIngredientItemId;

		private int _totalUpgrades;

		private int _totalIngredientImplants;

		private int _implantsDone;

		private bool _disableMainColliderAfter;

		private bool _disableFilterColliderAfter;

		private int _failCount;

		private Vector3 _upPositionV;

		private Quaternion _upRotation;

		private Vector3 _downPosition;

		private Quaternion _downRotation;

		private Vector3 _rotateAxis;

		private float _rotateAngle;

		private float _stateStartTime;

		private Transform _currentUpgradePartTr;

		private Vector3 _upgradeRayOriginAngles;

		private Vector3 _upgradePartStartPosition;

		private Vector3 _upgradePartTargetPosition;

		private Quaternion _circlePatternRotation;

		private Vector3 _circlePatternCapsuleFilterPivot;

		private bool _fastForward;

		public bool _spawnDebug;

		public bool _breakAfterSpawnDebug;

		public enum States
		{
			Idle,
			MoveUp,
			Aiming,
			Applying,
			Rotating,
			Implanting,
			MoveDown
		}

		public enum Patterns
		{
			FullRandom,
			Circle,
			Aligned,
			NoView
		}
	}
}
