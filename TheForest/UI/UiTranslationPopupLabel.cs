using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiTranslationPopupLabel : UiTranslationLabel
	{
		
		public override void ApplyTranslation(string text)
		{
			if (this._translate && this._popup && this._popup.transform)
			{
				string[] array = text.Split(new char[]
				{
					'\n'
				});
				if (array.Length != this._popup.items.Count)
				{
					array = UiTranslationDatabase.EnglishValueForKey(this._key, text).Split(new char[]
					{
						'\n'
					});
				}
				if (array.Length == this._popup.items.Count)
				{
					try
					{
						int num = this._popup.items.IndexOf(this._popup.value);
						if (this._popup.items == null)
						{
							this._popup.items = new List<string>();
						}
						else
						{
							this._popup.items.Clear();
						}
						foreach (string item in array)
						{
							this._popup.items.Add(item);
						}
						if (num >= 0 && num < this._popup.items.Count && this._popup.value != this._popup.items[num])
						{
							this._popup.value = this._popup.items[num];
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
		}

		
		
		public override string Text
		{
			get
			{
				return this._popup.items.Join("\n");
			}
		}

		
		public UIPopupList _popup;
	}
}
