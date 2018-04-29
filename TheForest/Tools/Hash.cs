using System;
using System.Security.Cryptography;
using System.Text;

namespace TheForest.Tools
{
	
	public static class Hash
	{
		
		public static string ToMD5(string input)
		{
			MD5 md = MD5.Create();
			byte[] bytes = Encoding.ASCII.GetBytes(input);
			byte[] array = md.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		
		public static string ToSHA512(string input)
		{
			SHA512 sha = SHA512.Create();
			byte[] bytes = Encoding.ASCII.GetBytes(input);
			byte[] array = sha.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		
		public static string ToSHA256(string input)
		{
			SHA256 sha = SHA256.Create();
			byte[] bytes = Encoding.ASCII.GetBytes(input);
			byte[] array = sha.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}
	}
}
