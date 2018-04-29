using System;
using System.Collections;
using Bolt;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	[DoNotSerializePublic]
	public class ItemSource : EntityBehaviour<IWaterSourceState>
	{
		
		private void Awake()
		{
			if (!BoltNetwork.isRunning)
			{
				base.StartCoroutine(this.DelayedAwake());
			}
			this._billboardGatherPickup.SetActive(false);
			this._billboardGatherSheen.SetActive(false);
			base.enabled = false;
		}

		
		private IEnumerator DelayedAwake()
		{
			while (LevelSerializer.IsDeserializing)
			{
				yield return null;
			}
			if (this._allowInCaves || Terrain.activeTerrain.SampleHeight(base.transform.position) < base.transform.position.y)
			{
				base.InvokeRepeating("AddRandomAmount", 300f, 300f);
			}
			yield break;
		}

		
		private void Update()
		{
			bool canGather = this.CanGather;
			if (canGather && TheForest.Utils.Input.GetButtonDown("Take"))
			{
				LocalPlayer.Sfx.PlayWhoosh();
				LocalPlayer.Inventory.AddItem(this._itemId, this._amount, false, false, null);
				this.RemoveItem((float)this._amount);
			}
			this.ToggleIcons(true);
		}

		
		private void OnDeserialized()
		{
			this.UpdateRenderers();
			this.ToggleIcons(false);
		}

		
		private void GrabEnter()
		{
			base.enabled = true;
			this.ToggleIcons(true);
		}

		
		private void GrabExit()
		{
			base.enabled = false;
			this.ToggleIcons(false);
		}

		
		public void AddRandomAmount()
		{
			if (Scene.Atmosphere.DeltaTimeOfDay > 0f && (!LocalPlayer.Inventory || Vector3.Distance(LocalPlayer.Transform.position, base.transform.position) > 150f))
			{
				int num = this._gain;
				if (num > 0)
				{
					this.AddItem((float)num);
				}
			}
		}

		
		public void AddItem(float amount)
		{
			this._amount = (int)Mathf.Min((float)this._amount + amount, this._maxAmount);
			if (BoltNetwork.isRunning && this.entity && this.entity.isAttached)
			{
				base.state.amount = (float)this._amount;
			}
			this.ToggleIcons(base.enabled);
			this.UpdateRenderers();
		}

		
		public void RemoveItem(float amount)
		{
			this._amount = (int)Mathf.Max((float)this._amount - amount, 0f);
			if (BoltNetwork.isRunning && this.entity && this.entity.isAttached)
			{
				base.state.amount = (float)this._amount;
			}
			this.ToggleIcons(base.enabled);
			this.UpdateRenderers();
		}

		
		private void UpdateRenderers()
		{
			float num = (float)this._amount / this._maxAmount;
			int num2 = Mathf.CeilToInt(num * (float)this._fillRenderers.Length);
			for (int i = 0; i < this._fillRenderers.Length; i++)
			{
				bool flag = i < num2;
				if (this._fillRenderers[i].activeSelf != flag)
				{
					this._fillRenderers[i].SetActive(flag);
				}
			}
		}

		
		private void ToggleIcons(bool pickup)
		{
			bool canGather = this.CanGather;
			if (pickup)
			{
				this._billboardGatherPickup.SetActive(canGather);
				this._billboardGatherSheen.SetActive(false);
			}
			else
			{
				this._billboardGatherPickup.SetActive(false);
				this._billboardGatherSheen.SetActive(canGather);
			}
		}

		
		public override void Attached()
		{
			if (this.entity.isOwner)
			{
				base.state.amount = (float)this._amount;
				if (this._allowInCaves || Terrain.activeTerrain.SampleHeight(base.transform.position) < base.transform.position.y)
				{
					base.InvokeRepeating("AddRandomAmount", 300f, 300f);
				}
			}
			base.state.AddCallback("amount", new PropertyCallbackSimple(this.AmountUpdatedMp));
		}

		
		private void AmountUpdatedMp()
		{
			this._amount = (int)base.state.amount;
			this.UpdateRenderers();
		}

		
		
		private bool CanGather
		{
			get
			{
				return this._amount > 0;
			}
		}

		
		[SerializeThis]
		public int _amount;

		
		public float _maxAmount = 30f;

		
		public bool _allowInCaves;

		
		public RandomRange _gain;

		
		public GameObject _billboardGatherPickup;

		
		public GameObject _billboardGatherSheen;

		
		public GameObject[] _fillRenderers;

		
		[ItemIdPicker]
		public int _itemId;
	}
}
