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
					UiTranslationDatabase.LoadEnglishData();
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
				return UiTranslationDatabase.TranslationsPath + this._language + ".json";
			}
		}

		
		
		
		public Dictionary<string, string> DataEnglish { get; private set; }

		
		
		
		public Dictionary<string, string> Data { get; private set; }

		
		
		
		public Dictionary<string, string> DataCaps { get; private set; }

		
		
		
		public Font CurrentLanguageFont { get; private set; }

		
		public static string[] GetAvailableTranslations()
		{
			if (UiTranslationDatabase.Instance._availableTranslations != null)
			{
				if (UiTranslationDatabase.Instance._availableTranslations.Length != 0)
				{
					goto IL_24F;
				}
			}
			try
			{
				int num = -1;
				bool flag = false;
				if (!Directory.Exists(UiTranslationDatabase.TranslationsPath))
				{
					Directory.CreateDirectory(UiTranslationDatabase.TranslationsPath);
				}
				if (!File.Exists(UiTranslationDatabase.TranslationsPath + "version.txt") || !int.TryParse(File.ReadAllText(UiTranslationDatabase.TranslationsPath + "version.txt"), out num) || num != UiTranslationDatabase.Instance._version)
				{
					flag = true;
				}
				List<string> list = (from f in Directory.GetFiles(UiTranslationDatabase.TranslationsPath, "*.json", SearchOption.TopDirectoryOnly)
				select f.Substring(f.LastIndexOf('/') + 1).Replace(".json", string.Empty)).ToList<string>();
				foreach (UiTranslationDatabase.Language language in UiTranslationDatabase.Instance._builtInLanguages)
				{
					string item = language._data.name.Replace(".json", string.Empty);
					if (flag || !list.Contains(item))
					{
						File.WriteAllText(UiTranslationDatabase.TranslationsPath + language._data.name + ".json", language._data.text);
						if (!list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
				GUIText guitext = new GameObject("TestFont").AddComponent<GUIText>();
				guitext.text = string.Empty;
				for (int j = list.Count - 1; j >= 0; j--)
				{
					int num2 = list[j].IndexOf(' ');
					if (num2 > 0)
					{
						guitext.text = list[j].Substring(0, num2);
						if (guitext.GetScreenRect().width < 10f)
						{
							Debug.Log("Missing font, removing " + list[j]);
							list.RemoveAt(j);
						}
					}
				}
				UnityEngine.Object.DestroyImmediate(guitext.gameObject);
				UiTranslationDatabase.Instance._availableTranslations = list.ToArray();
				File.WriteAllText(UiTranslationDatabase.TranslationsPath + "version.txt", UiTranslationDatabase.Instance._version.ToString());
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			IL_24F:
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

		
		private static void LoadEnglishData()
		{
			string language = UiTranslationDatabase.Instance._language;
			UiTranslationDatabase.Instance._language = "English";
			try
			{
				Dictionary<string, string> source;
				if (Path.IsPathRooted(UiTranslationDatabase.Instance.Filename))
				{
					if (!File.Exists(UiTranslationDatabase.Instance.Filename))
					{
						Debug.LogError("Missing lang file: " + UiTranslationDatabase.Instance.Filename);
						return;
					}
					source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(UiTranslationDatabase.Instance.Filename));
				}
				else
				{
					string path = Application.dataPath + "/../" + UiTranslationDatabase.TranslationsPath;
					if (!Directory.Exists(path))
					{
						Debug.LogError("Missing lang file: " + UiTranslationDatabase.Instance.Filename);
						return;
					}
					source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(Application.dataPath + "/../" + UiTranslationDatabase.Instance.Filename));
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
			if (UiTranslationDatabase.Instance._availableTranslations.Contains(lang))
			{
				if (lang != UiTranslationDatabase.Instance._language || UiTranslationDatabase.Instance.Data == null)
				{
					string language = UiTranslationDatabase.Instance._language;
					UiTranslationDatabase.Instance._language = lang;
					try
					{
						bool flag = UiTranslationDatabase.Instance._builtInLanguages.Any((UiTranslationDatabase.Language bil) => bil._roman && bil._data.name.Contains(lang));
						Dictionary<string, string> source;
						if (Path.IsPathRooted(UiTranslationDatabase.Instance.Filename))
						{
							if (!File.Exists(UiTranslationDatabase.Instance.Filename))
							{
								Debug.LogError("Missing lang file: " + UiTranslationDatabase.Instance.Filename);
								return;
							}
							source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(UiTranslationDatabase.Instance.Filename));
						}
						else
						{
							string path = Application.dataPath + "/../" + UiTranslationDatabase.TranslationsPath;
							if (!Directory.Exists(path))
							{
								Debug.LogError("Missing lang file: " + UiTranslationDatabase.Instance.Filename);
								return;
							}
							source = JsonMapper.ToObject<Dictionary<string, string>>(File.ReadAllText(Application.dataPath + "/../" + UiTranslationDatabase.Instance.Filename));
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
			else
			{
				Debug.LogError("Unknown lang: " + lang);
			}
		}

		
		public static bool HasKey(string key)
		{
			return UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.Data != null && UiTranslationDatabase.Instance.Data.ContainsKey(key);
		}

		
		public static string TranslateKey(string key, string defaultValue, bool caps = false)
		{
			string text;
			if (caps && UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.DataCaps != null && UiTranslationDatabase.Instance.DataCaps.TryGetValue(key, out text) && !string.IsNullOrEmpty(text))
			{
				return text;
			}
			if (UiTranslationDatabase.Instance && UiTranslationDatabase.Instance.Data != null && UiTranslationDatabase.Instance.Data.TryGetValue(key, out text) && !string.IsNullOrEmpty(text))
			{
				return text;
			}
			return defaultValue;
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

		
		public int _version;

		
		public UiTranslationDatabase.Language[] _builtInLanguages;

		
		public List<UiTranslationDatabase.Node> _nodes;

		
		private string[] _availableTranslations;

		
		private string _language = "English";

		
		private static UiTranslationDatabase _instance;

		
		[Serializable]
		public class Language
		{
			
			public TextAsset _data;

			
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
