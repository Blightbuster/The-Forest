using System;
using System.Collections.Generic;
using TheForest.Tools;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiTranslationNode : MonoBehaviour
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
			this.ImportTranslationData(this._db.Data, this._db.DataCaps);
		}

		
		public Dictionary<string, string> ExportTranslationData()
		{
			Dictionary<string, string> result;
			try
			{
				result = this._translationKeys.ToDictionary((UiTranslationNode.TranslationData tk) => tk._key, (UiTranslationNode.TranslationData tk) => tk._labelTr[0].Text);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				result = null;
			}
			return result;
		}

		
		public void ImportTranslationData(Dictionary<string, string> translationData, Dictionary<string, string> capsTranslationData)
		{
			if (translationData != null)
			{
				foreach (UiTranslationNode.TranslationData translationData2 in this._translationKeys)
				{
					if (translationData.ContainsKey(translationData2._key))
					{
						translationData2.ApplyTranslation(translationData[translationData2._key], (!translationData2._caps || capsTranslationData == null) ? null : capsTranslationData[translationData2._key]);
					}
				}
			}
		}

		
		public UiTranslationDatabase _db;

		
		public string _name;

		
		[NameFromProperty("_key", 0)]
		public List<UiTranslationNode.TranslationData> _translationKeys;

		
		[Serializable]
		public class TranslationData
		{
			
			public void ApplyTranslation(string text, string capsText)
			{
				foreach (UiTranslationLabel uiTranslationLabel in this._labelTr)
				{
					if (uiTranslationLabel)
					{
						if (uiTranslationLabel._caps && !string.IsNullOrEmpty(capsText))
						{
							uiTranslationLabel.ApplyTranslation(capsText);
						}
						else
						{
							uiTranslationLabel.ApplyTranslation(text);
						}
					}
				}
			}

			
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			
			public override bool Equals(object obj)
			{
				if (obj is UiTranslationNode.TranslationData)
				{
					return this._key == (obj as UiTranslationNode.TranslationData)._key && this._caps == (obj as UiTranslationNode.TranslationData)._caps;
				}
				return base.Equals(obj);
			}

			
			public bool Match(UiTranslationLabel obj)
			{
				return this._key == obj._key;
			}

			
			public UiTranslationLabel[] _labelTr;

			
			public string _key;

			
			public bool _caps;
		}

		
		private class CompareTranslationData : IComparer<UiTranslationNode.TranslationData>
		{
			
			public int Compare(UiTranslationNode.TranslationData x, UiTranslationNode.TranslationData y)
			{
				return string.Compare(x._key, y._key);
			}
		}
	}
}
