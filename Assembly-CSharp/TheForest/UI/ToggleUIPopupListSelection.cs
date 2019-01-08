using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.UI
{
	public class ToggleUIPopupListSelection : MonoBehaviour
	{
		private void Awake()
		{
			EventDelegate.Add(this._target.onChange, new EventDelegate.Callback(this.RefreshState));
		}

		private void OnEnable()
		{
			this._active = base.GetComponent<UIButton>().isEnabled;
			this.RefreshState();
		}

		private void OnClick()
		{
			if (this._active)
			{
				List<string> items = this._target.items;
				if (items.Count > 0)
				{
					string value = this._target.value;
					int num = items.IndexOf(value);
					if (num == -1)
					{
						num = 0;
					}
					num += ((this._direction != ToggleUIPopupListSelection.Directions.Next) ? -1 : 1);
					num = (int)Mathf.Repeat((float)num, (float)items.Count);
					this._target.value = items[num];
				}
			}
		}

		private void RefreshState()
		{
			List<string> items = this._target.items;
			string value = this._target.value;
			int num = items.IndexOf(value);
			if (this._direction == ToggleUIPopupListSelection.Directions.Previous)
			{
				if (num == 0)
				{
					if (this._active)
					{
						this._active = false;
						base.GetComponent<UIButton>().isEnabled = false;
					}
				}
				else if (!this._active)
				{
					this._active = true;
					base.GetComponent<UIButton>().isEnabled = true;
				}
			}
			else if (this._direction == ToggleUIPopupListSelection.Directions.Next)
			{
				if (num + 1 >= items.Count)
				{
					if (this._active)
					{
						this._active = false;
						base.GetComponent<UIButton>().isEnabled = false;
					}
				}
				else if (!this._active)
				{
					this._active = true;
					base.GetComponent<UIButton>().isEnabled = true;
				}
			}
		}

		private void RefreshSiblingsState()
		{
			IEnumerator enumerator = base.transform.parent.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					ToggleUIPopupListSelection component = transform.GetComponent<ToggleUIPopupListSelection>();
					if (component)
					{
						component.RefreshState();
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public UIPopupList _target;

		public ToggleUIPopupListSelection.Directions _direction;

		private bool _active;

		public enum Directions
		{
			Previous,
			Next
		}
	}
}
