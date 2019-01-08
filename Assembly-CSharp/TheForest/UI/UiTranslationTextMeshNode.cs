using System;
using System.Collections.Generic;
using TheForest.Tools;
using TheForest.Utils;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	public class UiTranslationTextMeshNode : MonoBehaviour
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
				result = this._translationKeys.ToDictionary((UiTranslationTextMeshNode.TranslationData tk) => tk._key, (UiTranslationTextMeshNode.TranslationData tk) => tk._labels[0]._label.text);
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
				foreach (UiTranslationTextMeshNode.TranslationData translationData2 in this._translationKeys)
				{
					if (translationData.ContainsKey(translationData2._key) && !translationData2._keepAlways)
					{
						translationData2.ApplyTranslation(translationData[translationData2._key], (capsTranslationData == null) ? null : capsTranslationData[translationData2._key]);
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

		public void RegisterDynamicLabel(UiTranslationTextMesh label)
		{
			if (!this._dynamicLabel.Contains(label))
			{
				this._dynamicLabel.Add(label);
			}
		}

		public void UnregisterDynamicLabel(UiTranslationTextMesh label)
		{
			if (this._dynamicLabel.Contains(label))
			{
				this._dynamicLabel.Remove(label);
			}
		}

		public UiTranslationDatabase _db;

		public string _name;

		[NameFromProperty("_key", 0)]
		public List<UiTranslationTextMeshNode.TranslationData> _translationKeys;

		private List<UiTranslationTextMesh> _dynamicLabel = new List<UiTranslationTextMesh>();

		[Serializable]
		public class TranslationData
		{
			public void ApplyTranslation(string text, string capsText)
			{
				foreach (UiTranslationTextMesh uiTranslationTextMesh in this._labels)
				{
					if (uiTranslationTextMesh)
					{
						if (uiTranslationTextMesh._caps && !string.IsNullOrEmpty(capsText))
						{
							uiTranslationTextMesh.ApplyTranslation(capsText);
						}
						else
						{
							uiTranslationTextMesh.ApplyTranslation(text);
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
				if (obj is UiTranslationTextMeshNode.TranslationData)
				{
					return this._key == (obj as UiTranslationTextMeshNode.TranslationData)._key && this._caps == (obj as UiTranslationTextMeshNode.TranslationData)._caps;
				}
				return base.Equals(obj);
			}

			public UiTranslationTextMesh[] _labels;

			public string _key;

			public bool _caps;

			public bool _keepAlways;
		}

		private class CompareTranslationData : IComparer<UiTranslationTextMeshNode.TranslationData>
		{
			public int Compare(UiTranslationTextMeshNode.TranslationData x, UiTranslationTextMeshNode.TranslationData y)
			{
				return string.Compare(x._key, y._key);
			}
		}
	}
}
