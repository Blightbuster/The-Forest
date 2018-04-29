using System;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiTranslationOverlayWorld : MonoBehaviour
	{
		
		private void Reset()
		{
			if (base.GetComponentInParent<SurvivalBook>())
			{
				this._currentViewOption = UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook;
			}
			if (base.name.Contains("ChooseSection"))
			{
				int num = base.name.IndexOf("ChooseSection");
				this._key = "BOOK_" + base.name.Substring(num + 13).ToUpperInvariant().Trim();
				this._text = (from x in base.name.Substring(num + 13)
				select (!char.IsUpper(x)) ? x.ToString() : (" " + x)).Join(string.Empty).TrimStart(new char[]
				{
					' '
				});
				this._viewportOffset = new Vector3(-0.08f, 0f, 4f);
			}
			if (base.name.Contains("TranslationTag - "))
			{
				int num2 = base.name.IndexOf("TranslationTag - ");
				this._key = "BOOK_" + base.name.Substring(num2 + 17).ToUpperInvariant().Trim();
				this._text = (from x in base.name.Substring(num2 + 17)
				select (!char.IsUpper(x)) ? x.ToString() : (" " + x)).Join(string.Empty).TrimStart(new char[]
				{
					' '
				});
			}
		}

		
		public void OnEnable()
		{
			if (!string.IsNullOrEmpty(this._key) && (!UiTranslationDatabase.OriginalVersion || this._enableForOriginalVersion))
			{
				UiTranslationOverlaySystem.RegisterLabel(base.transform, this._key, this._textColor, this._backgroundColor, this._alignment, this._currentViewOption);
				if (this._overrideDepth || this._overrideHeight || this._currentViewOption == UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook || this._currentViewOption == UiTranslationOverlaySystem.CurrentViewOptions.AllowInInventory)
				{
					UiTranslationOverlayLabel overlayLabel = UiTranslationOverlaySystem.GetOverlayLabel(base.transform);
					if (overlayLabel)
					{
						this._ol = overlayLabel;
						if (this._overrideDepth)
						{
							if (this._currentViewOption != UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook)
							{
								this._oldDepth = overlayLabel._follow._minDepth;
								overlayLabel._follow._minDepth = this._depth;
							}
						}
						if (this._overrideHeight)
						{
							if (this._currentViewOption != UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook)
							{
								this._oldHeight = overlayLabel._follow._worldOffset.y;
								overlayLabel._follow._worldOffset.y = this._height;
							}
						}
						if (this._currentViewOption == UiTranslationOverlaySystem.CurrentViewOptions.AllowInBook || this._currentViewOption == UiTranslationOverlaySystem.CurrentViewOptions.AllowInInventory)
						{
							overlayLabel._follow._viewportOffsetBook = this._viewportOffset;
						}
					}
				}
			}
		}

		
		public void OnDrawGizmosSelected()
		{
			if (this.deployGuiText)
			{
				this.deployGuiText = false;
				TextMesh componentInParent = base.GetComponentInParent<TextMesh>();
				this._text = componentInParent.text;
				base.name = "TranslationTag - Stats - " + componentInParent.text.Replace(" ", "_").Replace("\r\n", string.Empty).Replace("\n", string.Empty).Replace(":", string.Empty);
				this._key = "BOOK_STAT_" + componentInParent.text.ToUpperInvariant().Replace(" ", "_").Replace("\r\n", string.Empty).Replace("\n", "_").Replace(":", "_");
			}
			if (this.cleanupKey)
			{
				this.cleanupKey = false;
				if (this._key.Contains("\r"))
				{
					this._key = this._key.Replace("\r", string.Empty);
				}
				if (this._text.Contains("\r\n"))
				{
					this._text = this._text.Replace("\r\n", "\n");
				}
				if (this._text.Contains("\r"))
				{
					this._text = this._text.Replace("\r", string.Empty);
				}
			}
		}

		
		public void OnDisable()
		{
			if (!string.IsNullOrEmpty(this._key))
			{
				UiTranslationOverlaySystem.UnregisterIcon(base.transform);
			}
		}

		
		public virtual void ApplyTranslation(string text, Font font)
		{
		}

		
		public string _key;

		
		[Multiline]
		public string _text;

		
		public Color _textColor;

		
		public Color _backgroundColor;

		
		public UiTranslationOverlaySystem.CurrentViewOptions _currentViewOption;

		
		public bool _overrideDepth;

		
		public float _depth;

		
		public bool _overrideHeight;

		
		public float _height;

		
		public Vector3 _viewportOffset;

		
		public bool _enableForOriginalVersion;

		
		public NGUIText.Alignment _alignment = NGUIText.Alignment.Left;

		
		public float _oldDepth;

		
		public float _oldHeight;

		
		private UiTranslationOverlayLabel _ol;

		
		public bool deployGuiText;

		
		public bool cleanupKey;
	}
}
