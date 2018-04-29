using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.UI
{
	
	public class ToggleUIPopupListSelection : MonoBehaviour
	{
		
		private void OnClick()
		{
			List<string> items = this._target.items;
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

		
		public UIPopupList _target;

		
		public ToggleUIPopupListSelection.Directions _direction;

		
		public enum Directions
		{
			
			Previous,
			
			Next
		}
	}
}
