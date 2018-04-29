using System;
using RestSharp;

namespace RazerSDK.Client
{
	
	
	public delegate Exception ExceptionFactory(string methodName, IRestResponse response);
}
