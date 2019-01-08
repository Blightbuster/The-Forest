using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.UI
{
	public class UiIconSlotToken : MonoBehaviour
	{
		public void Register(UiIconSlot icon, bool priority)
		{
			if (!this._activeIcons.Contains(icon))
			{
				if (priority)
				{
					this._activeIcons.Insert(0, icon);
				}
				else
				{
					this._activeIcons.Add(icon);
				}
			}
			this.RefreshActiveFilters();
		}

		public void Unregister(UiIconSlot icon)
		{
			if (this._activeIcons.Contains(icon))
			{
				this._activeIcons.Remove(icon);
			}
			this.RefreshActiveFilters();
		}

		private void RefreshActiveFilters()
		{
			for (int i = this._activeIcons.Count - 1; i >= 0; i--)
			{
				if (!this._activeIcons[i] || !this._activeIcons[i]._filterGo)
				{
					this._activeIcons.RemoveAt(i);
				}
				else if (this._activeIcons[i]._filterGo.activeSelf != (i == 0))
				{
					this._activeIcons[i]._filterGo.SetActive(i == 0);
				}
			}
		}

		public List<UiIconSlot> _activeIcons = new List<UiIconSlot>();
	}
}
