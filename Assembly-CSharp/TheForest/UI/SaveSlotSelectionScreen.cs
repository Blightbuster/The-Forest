using System;
using System.Collections.Generic;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.UI
{
	public class SaveSlotSelectionScreen : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this._disableWhenActive.Length > 0)
			{
				foreach (GameObject gameObject in this._disableWhenActive)
				{
					if (gameObject.activeSelf)
					{
						this._enableWhenInactive.Add(gameObject);
						gameObject.SetActive(false);
					}
				}
			}
		}

		private void Update()
		{
			if (Cheats.PermaDeath && GameSetup.Init == InitTypes.Continue)
			{
				for (int i = 0; i < this._saveSlotsButtons.Length; i++)
				{
					this._saveSlotsButtons[i].isEnabled = (i + Slots.Slot1 == GameSetup.Slot);
				}
			}
		}

		private void OnDisable()
		{
			if (!this._confirmPanel.activeSelf)
			{
				if (this._disableWhenActive.Length > 0)
				{
					foreach (GameObject gameObject in this._enableWhenInactive)
					{
						gameObject.SetActive(true);
					}
				}
				this._enableWhenInactive.Clear();
			}
		}

		public void OnSlotSelection(Slots slotNum)
		{
			this._pendingConfirmationSlot = slotNum;
			if ((slotNum == GameSetup.Slot && SaveSlotUtils.UserId == GameSetup.SaveUserId && GameSetup.Init != InitTypes.New) || !SaveSlotUtils.HasLocalFile(slotNum, "__RESUME__"))
			{
				this.OnSlotConfirmed();
			}
			else
			{
				this._confirmPanel.SetActive(true);
				base.gameObject.SetActive(false);
			}
		}

		public void OnCancelConfirm()
		{
			this._confirmPanel.SetActive(false);
			base.gameObject.SetActive(true);
		}

		public void OnSlotConfirmed()
		{
			if (this._confirmPanel.activeSelf)
			{
				this._confirmPanel.SetActive(false);
			}
			this.OnDisable();
			GameSetup.SetInitType(InitTypes.Continue);
			GameSetup.SetSlot(this._pendingConfirmationSlot);
			SaveSlotSelectionScreen.OnSlotSelected.Invoke();
		}

		public void OnCancel()
		{
			SaveSlotSelectionScreen.OnSlotCanceled.Invoke();
		}

		public GameObject[] _disableWhenActive;

		public List<GameObject> _enableWhenInactive = new List<GameObject>();

		public UIButton[] _saveSlotsButtons;

		public GameObject _confirmPanel;

		private Slots _pendingConfirmationSlot;

		public static UnityEvent OnSlotSelected = new UnityEvent();

		public static UnityEvent OnSlotCanceled = new UnityEvent();
	}
}
