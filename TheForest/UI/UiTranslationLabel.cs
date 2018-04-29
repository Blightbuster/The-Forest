using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiTranslationLabel : MonoBehaviour
	{
		
		public virtual void ApplyTranslation(string text)
		{
			try
			{
				if (this._translate)
				{
					this._label.text = text;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		
		
		public virtual string Text
		{
			get
			{
				return this._label.text;
			}
		}

		
		public bool _translate = true;

		
		public bool _caps;

		
		public UILabel _label;

		
		public string _key;
	}
}
