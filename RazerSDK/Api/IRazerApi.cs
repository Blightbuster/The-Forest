using System;
using RazerSDK.ChromaPackage.Model;

namespace RazerSDK.Api
{
	
	public interface IRazerApi
	{
		
		PostChromaSdkResponse PostChromaSdk(ChromaSdkInput baseInput);
	}
}
