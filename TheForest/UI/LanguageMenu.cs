using System;
using System.Collections;
using UnityEngine;

namespace TheForest.UI
{
	
	public class LanguageMenu : MonoBehaviour
	{
		
		private void Awake()
		{
			if (this.Language)
			{
				this.Language.Clear();
				string[] availableTranslations = UiTranslationDatabase.GetAvailableTranslations();
				Texture[] availableTranslationsIcons = UiTranslationDatabase.GetAvailableTranslationsIcons();
				for (int i = 0; i < availableTranslations.Length; i++)
				{
					this.Language.AddItem(availableTranslations[i], availableTranslationsIcons[i], null);
				}
				EventDelegate.Add(this.Language.onChange, new EventDelegate.Callback(this.OnLanguageChange));
			}
			this.BeginIgnoreEvents();
		}

		
		private IEnumerator Start()
		{
			yield return YieldPresets.WaitForEndOfFrame;
			this.Language.value = ((!this.Language.items.Contains(PlayerPreferences.Language)) ? "ENGLISH" : PlayerPreferences.Language);
			this.EndIgnoreEvents();
			yield break;
		}

		
		private void OnDestroy()
		{
			if (this.Language)
			{
				EventDelegate.Remove(this.Language.onChange, new EventDelegate.Callback(this.OnLanguageChange));
			}
		}

		
		private void BeginIgnoreEvents()
		{
			this.ignoreCounter++;
		}

		
		private void EndIgnoreEvents()
		{
			this.ignoreCounter--;
		}

		
		private void OnLanguageChange()
		{
			if (this.IgnoreEvents)
			{
				return;
			}
			string value = this.Language.value;
			this.BeginIgnoreEvents();
			UiTranslationDatabase.SetLanguage(value);
			this.EndIgnoreEvents();
		}

		
		
		private bool IgnoreEvents
		{
			get
			{
				return this.ignoreCounter > 0;
			}
		}

		
		public UIPopupListIcon Language;

		
		private int ignoreCounter;
	}
}
