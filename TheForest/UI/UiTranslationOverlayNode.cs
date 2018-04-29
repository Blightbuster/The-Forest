using System;
using System.Collections.Generic;
using TheForest.Tools;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiTranslationOverlayNode : MonoBehaviour
	{
		
		private void Start()
		{
			EventRegistry.Game.Subscribe(TfEvent.LanguageSet, new EventRegistry.SubscriberCallback(this.OnLanguageChange));
			this.OnLanguageChange(null);
		}

		
		private void OnDestroy()
		{
			EventRegistry.Game.Unsubscribe(TfEvent.LanguageSet, new EventRegistry.SubscriberCallback(this.OnLanguageChange));
		}

		
		public void OnLanguageChange(object o)
		{
			this.ImportTranslationData(this._db.Data, this._db.CurrentLanguageFont);
		}

		
		public Dictionary<string, string> ExportTranslationData()
		{
			Dictionary<string, string> result;
			try
			{
				result = this._translationKeys.ToDictionary((UiTranslationOverlayNode.TranslationData tk) => tk._key, (UiTranslationOverlayNode.TranslationData tk) => tk._worldObjects[0]._text);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				result = null;
			}
			return result;
		}

		
		public void ImportTranslationData(Dictionary<string, string> translationData, Font font)
		{
			if (translationData != null)
			{
				foreach (UiTranslationOverlayNode.TranslationData translationData2 in this._translationKeys)
				{
					if (translationData.ContainsKey(translationData2._key))
					{
						translationData2.ApplyTranslation(translationData[translationData2._key], font);
					}
				}
			}
		}

		
		public UiTranslationDatabase _db;

		
		public string _name;

		
		[NameFromProperty("_key", 0)]
		public List<UiTranslationOverlayNode.TranslationData> _translationKeys;

		
		[Serializable]
		public class TranslationData
		{
			
			public void ApplyTranslation(string text, Font font)
			{
				foreach (UiTranslationOverlayWorld uiTranslationOverlayWorld in this._worldObjects)
				{
					if (uiTranslationOverlayWorld)
					{
						uiTranslationOverlayWorld.ApplyTranslation(text, font);
					}
				}
			}

			
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			
			public override bool Equals(object obj)
			{
				if (obj is UiTranslationOverlayNode.TranslationData)
				{
					return this._key == (obj as UiTranslationOverlayNode.TranslationData)._key;
				}
				return base.Equals(obj);
			}

			
			public UiTranslationOverlayWorld[] _worldObjects;

			
			public string _key;
		}

		
		private class CompareTranslationData : IComparer<UiTranslationOverlayNode.TranslationData>
		{
			
			public int Compare(UiTranslationOverlayNode.TranslationData x, UiTranslationOverlayNode.TranslationData y)
			{
				return string.Compare(x._key, y._key);
			}
		}
	}
}
