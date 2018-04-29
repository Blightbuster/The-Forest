using System;
using System.Collections.Generic;
using System.Net;
using RazerSDK.ChromaPackage.Model;
using RazerSDK.Client;
using RestSharp;

namespace RazerSDK.Api
{
	
	public class RazerApi : IRazerApi
	{
		
		public RazerApi(ApiClient apiClient = null)
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

		
		public RazerApi(string basePath)
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

		
		public PostChromaSdkResponse PostChromaSdk(ChromaSdkInput baseInput)
		{
			string text = "/chromasdk";
			text = text.Replace("{format}", "json");
			Dictionary<string, string> queryParams = new Dictionary<string, string>();
			Dictionary<string, string> headerParams = new Dictionary<string, string>();
			Dictionary<string, string> formParams = new Dictionary<string, string>();
			Dictionary<string, FileParameter> fileParams = new Dictionary<string, FileParameter>();
			string postBody = this.ApiClient.Serialize(baseInput);
			string[] authSettings = new string[0];
			IRestResponse restResponse = (IRestResponse)this.ApiClient.CallApi(text, Method.POST, queryParams, postBody, headerParams, formParams, fileParams, authSettings);
			if (restResponse.StatusCode >= HttpStatusCode.BadRequest)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaSdk: " + restResponse.Content, restResponse.Content);
			}
			if (restResponse.StatusCode == (HttpStatusCode)0)
			{
				throw new ApiException((int)restResponse.StatusCode, "Error calling PostChromaSdk: " + restResponse.ErrorMessage, restResponse.ErrorMessage);
			}
			return (PostChromaSdkResponse)this.ApiClient.Deserialize(restResponse.Content, typeof(PostChromaSdkResponse), restResponse.Headers);
		}
	}
}
