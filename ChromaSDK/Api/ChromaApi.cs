using System;
using System.Collections.Generic;
using System.Net;
using ChromaSDK.ChromaPackage.Model;
using ChromaSDK.Client;
using RestSharp;
using UnityEngine;

namespace ChromaSDK.Api
{
	
	public class ChromaApi : IChromaApi
	{
		
		public ChromaApi(ApiClient apiClient = null)
		{
			if (apiClient == null)
			{
				this.ApiClient = Configuration.DefaultApiClient;
			}
			else
			{
				this.ApiClient = apiClient;
			}
		}

		
		public ChromaApi(string basePath)
		{
			this.ApiClient = new ApiClient(basePath);
		}

		
		public void SetBasePath(string basePath)
		{
			this.ApiClient.BasePath = basePath;
		}

		
		public string GetBasePath(string basePath)
		{
			return this.ApiClient.BasePath;
		}

		
		
		
		public ApiClient ApiClient { get; set; }

		
		public DeleteChromaSdkResponse DeleteChromaSdk()
		{
			string text = "/";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.DELETE, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling DeleteChromaSdk: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling DeleteChromaSdk: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (DeleteChromaSdkResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(DeleteChromaSdkResponse), restResponse.Headers);
		}

		
		public void Heartbeat()
		{
			string text = "/heartbeat";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling Heartbeat: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling Heartbeat: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
		}

		
		public EffectResponseId PostChromaLink(EffectInput data)
		{
			string text = "/chromalink";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLink: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLink: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostChromaLinkCustom(EffectArray1dInput data)
		{
			string text = "/chromalink/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLinkCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLinkCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostChromaLinkNone()
		{
			string text = "/chromalink/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLinkNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLinkNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostChromaLinkStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PostChromaLinkStatic");
			}
			string text = "/chromalink/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLinkStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaLinkStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostHeadset(EffectInput data)
		{
			string text = "/headset";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadset: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadset: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostHeadsetCustom(EffectArray1dInput data)
		{
			string text = "/headset/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadsetCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadsetCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostHeadsetNone()
		{
			string text = "/headset/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadsetNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadsetNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostHeadsetStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PostHeadsetStatic");
			}
			string text = "/headset/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadsetStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostHeadsetStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeyboard(EffectInput data)
		{
			string text = "/keyboard";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboard: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboard: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeyboardCustom(EffectArray2dInput data)
		{
			string text = "/keyboard/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboardCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboardCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeyboardNone()
		{
			string text = "/keyboard/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboardNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboardNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeyboardStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PostKeyboardStatic");
			}
			string text = "/keyboard/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboardStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeyboardStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeypad(EffectInput data)
		{
			string text = "/keypad";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypad: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypad: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeypadCustom(EffectArray2dInput data)
		{
			string text = "/keypad/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypadCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypadCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeypadNone()
		{
			string text = "/keypad/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypadNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypadNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostKeypadStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PostKeypadStatic");
			}
			string text = "/keypad/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypadStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostKeypadStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMouse(EffectInput data)
		{
			string text = "/mouse";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouse: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouse: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMouseCustom(EffectArray2dInput data)
		{
			string text = "/mouse/custom2";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouseCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouseCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMouseNone()
		{
			string text = "/mouse/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouseNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouseNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMouseStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PostMouseStatic");
			}
			string text = "/mouse/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouseStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMouseStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMousepad(EffectInput data)
		{
			string text = "/mousepad";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepad: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepad: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMousepadCustom(EffectArray1dInput data)
		{
			string text = "/mousepad/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepadCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepadCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMousepadNone()
		{
			string text = "/mousepad/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepadNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepadNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponseId PostMousepadStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PostMousepadStatic");
			}
			string text = "/mousepad/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepadStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostMousepadStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponseId)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponseId), restResponse.Headers);
		}

		
		public EffectResponse PutChromaLink(EffectInput data)
		{
			string text = "/chromalink";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLink: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLink: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutChromaLinkCustom(EffectArray1dInput data)
		{
			string text = "/chromalink/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLinkCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLinkCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutChromaLinkNone()
		{
			string text = "/chromalink/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLinkNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLinkNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutChromaLinkStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PutChromaLinkStatic");
			}
			string text = "/chromalink/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLinkStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutChromaLinkStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectIdentifierResponse PutEffect(EffectIdentifierInput data)
		{
			string text = "/effect";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutEffect: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutEffect: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectIdentifierResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectIdentifierResponse), restResponse.Headers);
		}

		
		public EffectResponse PutHeadset(EffectInput data)
		{
			string text = "/headset";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadset: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadset: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutHeadsetCustom(EffectArray1dInput data)
		{
			string text = "/headset/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadsetCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadsetCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutHeadsetNone()
		{
			string text = "/headset/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadsetNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadsetNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutHeadsetStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PutHeadsetStatic");
			}
			string text = "/headset/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadsetStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutHeadsetStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeyboard(EffectInput data)
		{
			string text = "/keyboard";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboard: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboard: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeyboardCustom(EffectArray2dInput data)
		{
			string text = "/keyboard/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string text2 = this.ApiClient.Serialize(data);
			Debug.Log(text2);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, text2, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboardCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboardCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeyboardNone()
		{
			string text = "/keyboard/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboardNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboardNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeyboardStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PutKeyboardStatic");
			}
			string text = "/keyboard/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboardStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeyboardStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeypad(EffectInput data)
		{
			string text = "/keypad";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypad: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypad: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeypadCustom(EffectArray2dInput data)
		{
			string text = "/keypad/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypadCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypadCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeypadNone()
		{
			string text = "/keypad/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypadNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypadNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutKeypadStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PutKeypadStatic");
			}
			string text = "/keypad/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypadStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutKeypadStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMouse(EffectInput data)
		{
			string text = "/mouse";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouse: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouse: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMouseCustom(EffectArray2dInput data)
		{
			string text = "/mouse/custom2";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouseCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouseCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMouseNone()
		{
			string text = "/mouse/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouseNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouseNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMouseStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PutMouseStatic");
			}
			string text = "/mouse/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouseStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMouseStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMousepad(EffectInput data)
		{
			string text = "/mousepad";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepad: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepad: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMousepadCustom(EffectArray1dInput data)
		{
			string text = "/mousepad/custom";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepadCustom: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepadCustom: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMousepadNone()
		{
			string text = "/mousepad/none";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = null;
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepadNone: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepadNone: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectResponse PutMousepadStatic(int? color)
		{
			if (color == null)
			{
				throw new ApiException(400, "Missing required parameter 'color' when calling PutMousepadStatic");
			}
			string text = "/mousepad/static";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(color);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepadStatic: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PutMousepadStatic: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectResponse), restResponse.Headers);
		}

		
		public EffectIdentifierResponse RemoveEffect(EffectIdentifierInput data)
		{
			string text = "/effect/remove";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(data);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.PUT, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling RemoveEffect: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling RemoveEffect: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (EffectIdentifierResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(EffectIdentifierResponse), restResponse.Headers);
		}
	}
}
