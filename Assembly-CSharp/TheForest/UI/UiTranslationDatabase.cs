using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using TheForest.Tools;
using UniLinq;
using UnityEngine;

namespace TheForest.UI
{
	public class UiTranslationDatabase : ScriptableObject
	{
		private static UiTranslationDatabase Instance
		{
			get
			{
				if (!UiTranslationDatabase._instance)
				{
					UiTranslationDatabase._instance = (Resources.Load("Translation/UiTranslationDatabase") as UiTranslationDatabase);
					UiTranslationDatabase._instance._availableTranslations = null;
					UiTranslationDatabase._instance._language = "English";
					UiTranslationDatabase._instance.Data = null;
					UiTranslationDatabase._instance.DataEnglish = null;
					UiTranslationDatabase.OriginalVersion = true;
					UiTranslationDatabase.GetAvailableTranslations();
					UiTranslationDatabase.InitializeDefaultData();
				}
				return UiTranslationDatabase._instance;
			}
		}

		public static bool OriginalVersion { get; private set; }

		public static string TranslationsPath
		{
			get
			{
				return Application.dataPath + "/../lang/";
			}
		}

		private string Filename
		{
			get
			{
				foreach (UiTranslationDatabase.Language language in this._builtInLanguages)
				{
					if (language._name.Equals(this._language))
					{
						return UiTranslationDatabase.TranslationsPath + language._data.name + ".json";
					}
				}
				return UiTranslationDatabase.TranslationsPath + this._language + ".json";
			}
		}

		public Dictionary<string, string> DataEnglish { get; private set; }

		public Dictionary<string, string> Data { get; private set; }

		public Dictionary<string, string> DataCaps { get; private set; }

		public Font CurrentLanguageFont { get; private set; }

		public static Texture[] GetAvailableTranslationsIcons()
		{
			UiTranslationDatabase.GetAvailableTranslations();
			return UiTranslationDatabase.Instance._availableTranslationsIcons;
		}

		public static string[] GetAvailableTranslations()
		{
			if (UiTranslationDatabase.Instance._availableTranslations != null && UiTranslationDatabase.Instance._availableTranslations.Length != 0 && UiTranslationDatabase.Instance._availableTranslationsIcons != null)
			{
				if (UiTranslationDatabase.Instance._availableTranslationsIcons.Length == UiTranslationDatabase.Instance._availableTranslations.Length)
				{
					goto IL_392;
				}
			}
			try
			{
				int num = -1;
				bool flag = false;
				List<string> list = new List<string>();
				List<Texture> list2 = new List<Texture>();
				if (!Directory.Exists(UiTranslationDatabase.TranslationsPath))
				{
					Directory.CreateDirectory(UiTranslationDatabase.TranslationsPath);
				}
				if (!File.Exists(UiTranslationDatabase.TranslationsPath + "version.txt") || !int.TryParse(File.ReadAllText(UiTranslationDatabase.TranslationsPath + "version.txt"), out num) || num != UiTranslationDatabase.Instance._version)
				{
					flag = true;
				}
				List<string> list3 = (from f in Directory.GetFiles(UiTranslationDatabase.TranslationsPath, "*.json", SearchOption.TopDirectoryOnly)
				select f.Substring(f.LastIndexOf('/') + 1).Replace(".json", string.Empty)).ToList<string>();
				foreach (UiTranslationDatabase.Language language in UiTranslationDatabase.Instance._builtInLanguages)
				{
					string text = language._data.name.Replace(".json", string.Empty);
					string text2 = language._name;
					if (string.IsNullOrEmpty(text2))
					{
						text2 = text;
					}
					if (flag || !list3.Contains(text))
					{
						File.WriteAllText(UiTranslationDatabase.TranslationsPath + language._data.name + ".json", language._data.text);
					}
					if (list3.Contains(text))
					{
						list3.Remove(text);
					}
					if (!list.Contains(text2))
					{
						list.Add(text2);
						list2.Add(language._icon);
					}
				}
				foreach (string item in list3)
				{
					if (!list.Contains(item))
					{
						list.Add(item);
						list2.Add(null);
					}
				}
				GUIText guitext = new GameObject("TestFont").AddComponent<GUIText>();
				guitext.text = string.Empty;
				for (int j = list.Count - 1; j >= 0; j--)
				{
					int num2 = list[j].IndexOf(' ');
					if (num2 <= 0)
					{
						num2 = list[j].Length;
					}
					if (num2 > 0)
					{
						guitext.text = list[j].Substring(0, num2);
						if (guitext.GetScreenRect().width < 10f)
						{
							Debug.LogWarning("[Translation] Missing font, removing " + list[j]);
							list.RemoveAt(j);
							list2.RemoveAt(j);
						}
					}
					else
					{
						Debug.LogWarning("[Translation] Invalid translation name, removing " + list[j]);
						list.RemoveAt(j);
						list2.RemoveAt(j);
					}
				}
				UnityEngine.Object.DestroyImmediate(guitext.gameObject);
				UiTranslationDatabase.Instance._availableTranslations = list.ToArray();
				UiTranslationDatabase.Instance._availableTranslationsIcons = list2.ToArray();
				UiTranslationDatabase.Instance._language = UiTranslationDatabase.Instance._language;
				File.WriteAllText(UiTranslationDatabase.TranslationsPath + "version.txt", UiTranslationDatabase.Instance._version.ToString());
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			IL_392:
			return UiTranslationDatabase.Instance._availableTranslations;
		}

		public static void SaveTranslations()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (UiTranslationDatabase.Node node in UiTranslationDatabase.Instance._nodes)
			{
				foreach (UiTranslationDatabase.NodeKeyValue nodeKeyValue in node._keyValues)
				{
					if (!dictionary.ContainsKey(nodeKeyValue._key))
					{
						dictionary[nodeKeyValue._key] = nodeKeyValue._value;
					}
				}
			}
			string contents = JsonMapper.ToJson(dictionary, true);
			string text = Application.dataPath + "/Resources/Translation/";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllText(text + "English.json", contents);
			UiTranslationDatabase.Instance._version++;
		}

		private static void InitializeDefaultData()
		{
			string language = UiTranslationDatabase.Instance._language;
			UiTranslationDatabase.Instance._language = "English";
			try
			{
				string filename = UiTranslationDatabase.Instance.Filename;
				Dictionary<string, string> source;
				if (Path.IsPathRooted(filename))
				{
					if (!File.Exists(filename))
					{
						Debug.LogError("Missing lang file: " + filename);
						return;
					}
					source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(filename));
				}
				else
				{
					string path = Application.dataPath + "/../" + UiTranslationDatabase.TranslationsPath;
					if (!Directory.Exists(path))
					{
						Debug.LogError("Missing lang file: " + filename);
						return;
					}
					source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(Application.dataPath + "/../" + filename));
				}
				UiTranslationDatabase.Instance.DataEnglish = (from d in source
				where !string.IsNullOrEmpty(d.Value)
				select d).ToDictionary((KeyValuePair<string, string> i) => i.Key, (KeyValuePair<string, string> i) => i.Value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				UiTranslationDatabase.Instance._language = language;
			}
		}

		public static void SetLanguage(string lang)
		{
			UiTranslationDatabase.GetAvailableTranslations();
			if (string.Equals(lang, "TRANSLATIONDEBUG", StringComparison.InvariantCultureIgnoreCase))
			{
				UiTranslationDatabase.Instance.Data = (from d in UiTranslationDatabase.Instance.DataEnglish
				where !string.IsNullOrEmpty(d.Value)
				select d).ToDictionary((KeyValuePair<string, string> i) => i.Key, (KeyValuePair<string, string> i) => "{" + i.Key + "}");
				UiTranslationDatabase.Instance.DataCaps = (from d in UiTranslationDatabase.Instance.DataEnglish
				where !string.IsNullOrEmpty(d.Value)
				select d).ToDictionary((KeyValuePair<string, string> i) => i.Key, (KeyValuePair<string, string> i) => "{{" + i.Key + "}}");
				EventRegistry.Game.Publish(TfEvent.LanguageSet, null);
				return;
			}
			if (!UiTranslationDatabase.Instance._availableTranslations.Contains(lang))
			{
				Debug.LogError("Unknown lang: " + lang + " fallback to English");
				lang = "English";
			}
			if (lang != UiTranslationDatabase.Instance._language || UiTranslationDatabase.Instance.Data == null)
			{
				string language = UiTranslationDatabase.Instance._language;
				UiTranslationDatabase.Instance._language = lang;
				try
				{
					string filename = UiTranslationDatabase.Instance.Filename;
					Dictionary<string, string> source;
					if (Path.IsPathRooted(filename))
					{
						if (!File.Exists(filename))
						{
							Debug.LogError("Missing lang file: " + filename);
							return;
						}
						source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(filename));
					}
					else
					{
						string path = Application.dataPath + "/../" + UiTranslationDatabase.TranslationsPath;
						if (!Directory.Exists(path))
						{
							Debug.LogError("Missing lang file: " + filename);
							return;
						}
						source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(Application.dataPath + "/../" + filename));
					}
					UiTranslationDatabase.Instance.Data = (from d in source
					where !string.IsNullOrEmpty(d.Value)
					select d).ToDictionary((KeyValuePair<string, string> i) => i.Key, (KeyValuePair<string, string> i) => i.Value);
					UiTranslationDatabase.Instance.DataCaps = new Dictionary<string, string>();
					foreach (KeyValuePair<string, string> keyValuePair in UiTranslationDatabase.Instance.DataEnglish)
					{
						if (!UiTranslationDatabase.Instance.Data.ContainsKey(keyValuePair.Key))
						{
							UiTranslationDatabase.Instance.Data.Add(keyValuePair.Key, keyValuePair.Value);
						}
						if (!UiTranslationDatabase.Instance.DataCaps.ContainsKey(keyValuePair.Key))
						{
							UiTranslationDatabase.Instance.DataCaps.Add(keyValuePair.Key, UiTranslationDatabase.Instance.Data[keyValuePair.Key].ToUpper());
						}
					}
					PlayerPreferences.Language = lang;
					PlayerPreferences.Save();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					UiTranslationDatabase.Instance._language = language;
					return;
				}
				UiTranslationDatabase.OriginalVersion = (UiTranslationDatabase.Instance._language == "English");
				EventRegistry.Game.Publish(TfEvent.LanguageSet, null);
			}
		}

		public static bool HasKey(string key)
		{
			return UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.Data != null && UiTranslationDatabase.Instance.Data.ContainsKey(key);
		}

		public static string TranslateKey(string key, string defaultValue, bool caps = false)
		{
			caps = true;
			if (caps)
			{
				string text;
				if (UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.DataCaps != null && UiTranslationDatabase.Instance.DataCaps.TryGetValue(key, out text) && !string.IsNullOrEmpty(text))
				{
					return text;
				}
				return defaultValue;
			}
			else
			{
				string text;
				if (UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.Data != null && UiTranslationDatabase.Instance.Data.TryGetValue(key, out text) && !string.IsNullOrEmpty(text))
				{
					return text;
				}
				return defaultValue;
			}
		}

		public static string EnglishValueForKey(string key, string defaultValue)
		{
			string text;
			if (UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.DataEnglish != null && UiTranslationDatabase.Instance.DataEnglish.TryGetValue(key, out text) && !string.IsNullOrEmpty(text))
			{
				return text;
			}
			return defaultValue;
		}

		public const string DatabasePath = "Translation/UiTranslationDatabase";

		private const string DefaultLanguage = "English";

		public int _version;

		public UiTranslationDatabase.Language[] _builtInLanguages;

		public List<UiTranslationDatabase.Node> _nodes;

		[SerializeField]
		private string[] _availableTranslations;

		[SerializeField]
		private Texture[] _availableTranslationsIcons;

		[SerializeField]
		private string _language = "English";

		private static UiTranslationDatabase _instance;

		[Serializable]
		public class Language
		{
			public TextAsset _data;

			public Texture _icon;

			public string _name;

			public string _customFont;

			public bool _roman;
		}

		[Serializable]
		public class Node
		{
			public string _name;

			public List<UiTranslationDatabase.NodeKeyValue> _keyValues;
		}

		[Serializable]
		public class NodeKeyValue
		{
			public string _key;

			public string _value;
		}
	}
}
