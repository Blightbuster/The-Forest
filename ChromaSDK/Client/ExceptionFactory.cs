using System;
using RestSharp;

namespace ChromaSDK.Client
{
	
	
	public delegate Exception ExceptionFactory(string methodName, IRestResponse response);
}
