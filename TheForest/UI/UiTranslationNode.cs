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
					if (capsTranslationData.ContainsKey(translationData2._key))
					{
						translationData2.ApplyTranslation(translationData[translationData2._key], capsTranslationData[translationData2._key]);
					}
				}
				for (int i = this._dynamicLabel.Count - 1; i >= 0; i--)
				{
					if (this._dynamicLabel[i])
					{
						if (translationData.ContainsKey(this._dynamicLabel[i]._key))
						{
							this._dynamicLabel[i].ApplyTranslation((capsTranslationData == null) ? translationData[this._dynamicLabel[i]._key] : capsTranslationData[this._dynamicLabel[i]._key]);
						}
					}
					else
					{
						this._dynamicLabel.RemoveAt(i);
					}
				}
			}
		}

		
		public void RegisterDynamicLabel(UiTranslationLabel label)
		{
			if (!this._dynamicLabel.Contains(label))
			{
				this._dynamicLabel.Add(label);
			}
		}

		
		public void UnregisterDynamicLabel(UiTranslationLabel label)
		{
			if (this._dynamicLabel.Contains(label))
			{
				this._dynamicLabel.Remove(label);
			}
		}

		
		public UiTranslationDatabase _db;

		
		public string _name;

		
		[NameFromProperty("_key", 0)]
		public List<UiTranslationNode.TranslationData> _translationKeys;

		
		private List<UiTranslationLabel> _dynamicLabel = new List<UiTranslationLabel>();

		
		[Serializable]
		public class TranslationData
		{
			
			public void ApplyTranslation(string text, string capsText)
			{
				foreach (UiTranslationLabel uiTranslationLabel in this._labelTr)
				{
					if (uiTranslationLabel)
					{
						if (!string.IsNullOrEmpty(capsText))
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
