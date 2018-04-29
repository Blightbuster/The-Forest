using System;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	
	public class CookStew : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
		}

		
		private void Update()
		{
			bool flag = LocalPlayer.Inventory.Owns(this.CurrentFoodItemId, true);
			if (flag)
			{
				if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this.CurrentFoodItemId, 1, true, true))
				{
					LocalPlayer.Sfx.PlayItemCustomSfx(this.CurrentFoodItemId, true);
					this.SpawnFoodPrefab(this.CurrentFoodprefab);
				}
				else
				{
					Scene.HudGui.StewWidget.ShowList(this.CurrentFoodItemId, this._iconTr, SideIcons.Craft);
				}
			}
			int num = this.NextFoodType();
			if (this._currentFoodItemNum != num)
			{
				if (TheForest.Utils.Input.GetButtonDown("Rotate") || !flag)
				{
					LocalPlayer.Sfx.PlayWhoosh();
					this._currentFoodItemNum = num;
					if (this._currentFoodItemNum > -1)
					{
						Scene.HudGui.StewWidget.ShowList(this.CurrentFoodItemId, this._iconTr, SideIcons.Craft);
					}
				}
			}
			else if (!flag)
			{
				Scene.HudGui.StewWidget.Shutdown();
			}
		}

		
		private void OnDisable()
		{
			if (Scene.HudGui)
			{
				Scene.HudGui.StewWidget.Shutdown();
			}
		}

		
		private void GrabEnter()
		{
			int num;
			if (!LocalPlayer.Inventory.IsRightHandEmpty())
			{
				num = this._stewItems.FindIndex((CookStew.StewItem fi) => fi._itemId == LocalPlayer.Inventory.RightHand._itemId);
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
			else
			{
				this._currentFoodItemNum--;
				this.ToggleNextFood();
			}
			base.enabled = true;
		}

		
		private void GrabExit()
		{
			Scene.HudGui.StewWidget.Shutdown();
			base.enabled = false;
		}

		
		public void SetNoWater()
		{
			this._dryCooking = true;
			this._eatController._hydration = 0f;
			this._cookController.SetCustomStatus(Cook.Status.CookingNoWater);
		}

		
		private int NextFoodType()
		{
			int num = this._stewItems.Length;
			int num2 = this._currentFoodItemNum;
			while (num-- > 0)
			{
				num2++;
				num2 = (int)Mathf.Repeat((float)num2, (float)this._stewItems.Length);
				if (LocalPlayer.Inventory.Owns(this._stewItems[num2]._itemId, true))
				{
					break;
				}
			}
			return num2;
		}

		
		private void ToggleNextFood()
		{
			int num = this.NextFoodType();
			if (num != this._currentFoodItemNum)
			{
				this._currentFoodItemNum = num;
			}
		}

		
		private void SpawnFoodPrefab(GameObject prefab)
		{
			Vector3 position = base.transform.position + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.15f, 0f), UnityEngine.Random.Range(-0.2f, 0.2f));
			if (!BoltNetwork.isRunning)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, Quaternion.identity);
				gameObject.transform.parent = base.transform.parent;
				this.UpdateCookController();
			}
			else
			{
				PlaceDryingFood placeDryingFood = PlaceDryingFood.Create(GlobalTargets.OnlyServer);
				placeDryingFood.PrefabId = prefab.GetComponent<BoltEntity>().prefabId;
				placeDryingFood.Position = position;
				placeDryingFood.Rotation = Quaternion.Euler((float)UnityEngine.Random.Range(-15, 15), (float)UnityEngine.Random.Range(0, 359), (float)UnityEngine.Random.Range(-15, 15));
				placeDryingFood.Parent = base.transform.parent.GetComponent<BoltEntity>();
				if (DecayingInventoryItemView.LastUsed)
				{
					placeDryingFood.DecayState = (int)DecayingInventoryItemView.LastUsed._prevState;
				}
				placeDryingFood.Send();
			}
		}

		
		public void UpdateCookController()
		{
			this._cookController._cookDuration = 30f;
			this._cookController.CancelInvoke("CookMe");
			this._cookController.CancelInvoke("OverCooked");
			this._cookController.Invoke("CookMe", this._cookController._cookDuration);
			this._cookController.Invoke("OverCooked", (float)((!this._dryCooking) ? 600 : 60));
			this._cookController.EatTrigger = this._eatController;
		}

		
		
		private CookStew.StewItem CurrentStewItem
		{
			get
			{
				return this._stewItems[this._currentFoodItemNum];
			}
		}

		
		
		private int CurrentFoodItemId
		{
			get
			{
				return this._stewItems[this._currentFoodItemNum]._itemId;
			}
		}

		
		
		private GameObject CurrentFoodprefab
		{
			get
			{
				return this._stewItems[this._currentFoodItemNum]._prefab;
			}
		}

		
		public CookStew.StewItem[] _stewItems;

		
		public Transform _iconTr;

		
		public Cook _cookController;

		
		public EatStew _eatController;

		
		public bool _dryCooking;

		
		private int _currentFoodItemNum = -2;

		
		[Serializable]
		public class StewItem
		{
			
			[ItemIdPicker]
			public int _itemId;

			
			public GameObject _prefab;
		}
	}
}
