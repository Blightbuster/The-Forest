using System;
using UnityEngine;

namespace TheForest.UI
{
	public class UiTranslationTextMesh : MonoBehaviour
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

		public void SetText(string text)
		{
			this._label.text = text;
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

		public TextMesh _label;

		public string _key;
	}
}
