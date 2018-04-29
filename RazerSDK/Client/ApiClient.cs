using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Contrib;
using RestSharp.Extensions;

namespace RazerSDK.Client
{
	
	public class ApiClient
	{
		
		public ApiClient(string basePath = "http:
		{
			this.BasePath = basePath;
			this.RestClient = new RestClient(this.BasePath);
			this.RestClient.Timeout = 1000;
		}

		
		
		
		public string BasePath { get; set; }

		
		
		
		public RestClient RestClient { get; set; }

		
		
		public Dictionary<string, string> DefaultHeader
		{
			get
			{
				return this._defaultHeaderMap;
			}
		}

		
		public object CallApi(string path, Method method, Dictionary<string, string> queryParams, string postBody, Dictionary<string, string> headerParams, Dictionary<string, string> formParams, Dictionary<string, FileParameter> fileParams, string[] authSettings)
		{
			RestRequest restRequest = new RestRequest(path, method);
			this.UpdateParamsForAuth(queryParams, headerParams, authSettings);
			foreach (KeyValuePair<string, string> keyValuePair in this._defaultHeaderMap)
			{
				restRequest.AddHeader(keyValuePair.Key, keyValuePair.Value);
			}
			foreach (KeyValuePair<string, string> keyValuePair2 in headerParams)
			{
				restRequest.AddHeader(keyValuePair2.Key, keyValuePair2.Value);
			}
			foreach (KeyValuePair<string, string> keyValuePair3 in queryParams)
			{
				restRequest.AddParameter(keyValuePair3.Key, keyValuePair3.Value, ParameterType.GetOrPost);
			}
			foreach (KeyValuePair<string, string> keyValuePair4 in formParams)
			{
				restRequest.AddParameter(keyValuePair4.Key, keyValuePair4.Value, ParameterType.GetOrPost);
			}
			foreach (KeyValuePair<string, FileParameter> keyValuePair5 in fileParams)
			{
				restRequest.AddFile(keyValuePair5.Value.Name, keyValuePair5.Value.Writer, keyValuePair5.Value.FileName, keyValuePair5.Value.ContentType);
			}
			if (postBody != null)
			{
				restRequest.AddParameter("application/json", postBody, ParameterType.RequestBody);
			}
			return this.RestClient.Execute(restRequest);
		}

		
		public void AddDefaultHeader(string key, string value)
		{
			this._defaultHeaderMap.Add(key, value);
		}

		
		public string EscapeString(string str)
		{
			return HttpUtility.UrlEncode(str);
		}

		
		public FileParameter ParameterToFile(string name, Stream stream)
		{
			if (stream is FileStream)
			{
				return FileParameter.Create(name, stream.ReadAsBytes(), Path.GetFileName(((FileStream)stream).Name));
			}
			return FileParameter.Create(name, stream.ReadAsBytes(), "no_file_name_provided");
		}

		
		public string ParameterToString(object obj)
		{
			if (obj is DateTime)
			{
				return ((DateTime)obj).ToString(Configuration.DateTimeFormat);
			}
			if (obj is List<string>)
			{
				return string.Join(",", (obj as List<string>).ToArray());
			}
			return Convert.ToString(obj);
		}

		
		public object Deserialize(string content, Type type, IList<Parameter> headers = null)
		{
			if (type == typeof(object))
			{
				return content;
			}
			if (type == typeof(Stream))
			{
				string text = (!string.IsNullOrEmpty(Configuration.TempFolderPath)) ? Configuration.TempFolderPath : Path.GetTempPath();
				string path = text + Guid.NewGuid();
				if (headers != null)
				{
					Regex regex = new Regex("Content-Disposition:.*filename=['\"]?([^'\"\\s]+)['\"]?$");
					Match match = regex.Match(headers.ToString());
					if (match.Success)
					{
						path = text + match.Value.Replace("\"", string.Empty).Replace("'", string.Empty);
					}
				}
				File.WriteAllText(path, content);
				return new FileStream(path, FileMode.Open);
			}
			if (type.Name.StartsWith("System.Nullable`1[[System.DateTime"))
			{
				return DateTime.Parse(content, null, DateTimeStyles.RoundtripKind);
			}
			if (type == typeof(string) || type.Name.StartsWith("System.Nullable"))
			{
				return ApiClient.ConvertType(content, type);
			}
			object result;
			try
			{
				result = JsonConvert.DeserializeObject(content, type);
			}
			catch (IOException ex)
			{
				throw new ApiException(500, ex.Message);
			}
			return result;
		}

		
		public string Serialize(object obj)
		{
			string result;
			try
			{
				result = ((obj == null) ? null : JsonConvert.SerializeObject(obj));
			}
			catch (Exception ex)
			{
				throw new ApiException(500, ex.Message);
			}
			return result;
		}

		
		public string GetApiKeyWithPrefix(string apiKeyIdentifier)
		{
			string empty = string.Empty;
			Configuration.ApiKey.TryGetValue(apiKeyIdentifier, out empty);
			string empty2 = string.Empty;
			if (Configuration.ApiKeyPrefix.TryGetValue(apiKeyIdentifier, out empty2))
			{
				return empty2 + " " + empty;
			}
			return empty;
		}

		
		public void UpdateParamsForAuth(Dictionary<string, string> queryParams, Dictionary<string, string> headerParams, string[] authSettings)
		{
			if (authSettings == null || authSettings.Length == 0)
			{
				return;
			}
			foreach (string text in authSettings)
			{
				string text2 = text;
				if (text2 != null)
				{
					if (ApiClient.<>f__switch$map3 == null)
					{
						ApiClient.<>f__switch$map3 = new Dictionary<string, int>(0);
					}
					int num;
					if (ApiClient.<>f__switch$map3.TryGetValue(text2, out num))
					{
					}
				}
			}
		}

		
		public static string Base64Encode(string text)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			return Convert.ToBase64String(bytes);
		}

		
		public static object ConvertType(object source, Type dest)
		{
			return Convert.ChangeType(source, dest);
		}

		
		private readonly Dictionary<string, string> _defaultHeaderMap = new Dictionary<string, string>();
	}
}
