using System;

namespace ChromaSDK.Client
{
	
	public class ApiException : Exception
	{
		
		public ApiException()
		{
		}

		
		public ApiException(int errorCode, string message) : base(message)
		{
			this.ErrorCode = errorCode;
		}

		
		public ApiException(int errorCode, string message, object errorContent) : base(message)
		{
			this.ErrorCode = errorCode;
			this.ErrorContent = errorContent;
		}

		
		
		
		public int ErrorCode { get; set; }

		
		
		
		public object ErrorContent { get; private set; }
	}
}
