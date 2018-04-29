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
			if (this._disableWhenActive.Length > 0)
			{
				foreach (GameObject gameObject in this._enableWhenInactive)
				{
					gameObject.SetActive(true);
				}
			}
			this._enableWhenInactive.Clear();
		}

		
		public void OnSlotSelection(Slots slotNum)
		{
			GameSetup.SetInitType(InitTypes.Continue);
			GameSetup.SetSlot(slotNum);
			SaveSlotSelectionScreen.OnSlotSelected.Invoke();
		}

		
		public void OnCancel()
		{
			SaveSlotSelectionScreen.OnSlotCanceled.Invoke();
		}

		
		public GameObject[] _disableWhenActive;

		
		public List<GameObject> _enableWhenInactive = new List<GameObject>();

		
		public UIButton[] _saveSlotsButtons;

		
		public static UnityEvent OnSlotSelected = new UnityEvent();

		
		public static UnityEvent OnSlotCanceled = new UnityEvent();
	}
}
