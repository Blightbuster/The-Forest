using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Items.World
{
	public class VendingMachine : MonoBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
			this.ShowIcons(false);
		}

		private void Update()
		{
			if (!this._spawning && LocalPlayer.Inventory.AmountOf(this._inputItemId, false) >= this._inputAmount)
			{
				this.ShowIcons(true);
				if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this._inputItemId, this._inputAmount, false, true))
				{
					LocalPlayer.Sfx.PlayItemCustomSfx(this._inputItemId, true);
					this.ShowIcons(false);
					this._onInputItemAdded.Invoke();
					this._currentInputAmount += this._inputAmount;
					if (this._currentInputAmount >= this._inputAmount)
					{
						this._currentInputAmount = 0;
						this._spawning = true;
						base.Invoke("DoSpawn", this._outputSpawnDelay);
					}
				}
			}
		}

		private void GrabEnter()
		{
			base.enabled = true;
		}

		private void GrabExit()
		{
			base.enabled = false;
			this.ShowIcons(false);
		}

		private void ShowIcons(bool pickup)
		{
			if (this._sheenIcon.activeSelf == pickup)
			{
				this._sheenIcon.SetActive(!pickup);
			}
			if (this._pickupIcon.activeSelf != pickup)
			{
				this._pickupIcon.SetActive(pickup);
			}
		}

		public void DoSpawn()
		{
			this._spawning = false;
			this._onOuputItemSpawned.Invoke();
			UnityEngine.Object.Instantiate<GameObject>(ItemDatabase.ItemById(this._outputItemId)._pickupPrefab.gameObject, this._outputSpawnPosition.position, this._outputSpawnPosition.rotation);
		}

		[ItemIdPicker]
		public int _inputItemId;

		public int _inputAmount = 1;

		[ItemIdPicker]
		public int _outputItemId;

		public float _outputSpawnDelay = 1f;

		public Transform _outputSpawnPosition;

		public GameObject _pickupIcon;

		public GameObject _sheenIcon;

		public UnityEvent _onInputItemAdded;

		public UnityEvent _onOuputItemSpawned;

		private bool _spawning;

		private int _currentInputAmount;
	}
}
