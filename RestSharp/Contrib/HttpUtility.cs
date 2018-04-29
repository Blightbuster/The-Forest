using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using RestSharp.Extensions;

namespace RestSharp.Contrib
{
	
	public sealed class HttpUtility
	{
		
		public static void HtmlAttributeEncode(string s, TextWriter output)
		{
			if (output == null)
			{
				throw new NullReferenceException(".NET emulation");
			}
			output.Write(HttpEncoder.HtmlAttributeEncode(s));
		}

		
		public static string HtmlAttributeEncode(string s)
		{
			return HttpEncoder.HtmlAttributeEncode(s);
		}

		
		public static string UrlDecode(string str)
		{
			return HttpUtility.UrlDecode(str, Encoding.UTF8);
		}

		
		private static char[] GetChars(MemoryStream b, Encoding e)
		{
			return e.GetChars(b.ReadAsBytes(), 0, (int)b.Length);
		}

		
		private static void WriteCharBytes(IList buf, char ch, Encoding e)
		{
			if (ch > 'ÿ')
			{
				foreach (byte b in e.GetBytes(new char[]
				{
					ch
				}))
				{
					buf.Add(b);
				}
			}
			else
			{
				buf.Add((byte)ch);
			}
		}

		
		public static string UrlDecode(string s, Encoding e)
		{
			if (s == null)
			{
				return null;
			}
			if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
			{
				return s;
			}
			if (e == null)
			{
				e = Encoding.UTF8;
			}
			long num = (long)s.Length;
			List<byte> list = new List<byte>();
			int num2 = 0;
			while ((long)num2 < num)
			{
				char c = s[num2];
				if (c == '%' && (long)(num2 + 2) < num && s[num2 + 1] != '%')
				{
					int @char;
					if (s[num2 + 1] == 'u' && (long)(num2 + 5) < num)
					{
						@char = HttpUtility.GetChar(s, num2 + 2, 4);
						if (@char != -1)
						{
							HttpUtility.WriteCharBytes(list, (char)@char, e);
							num2 += 5;
						}
						else
						{
							HttpUtility.WriteCharBytes(list, '%', e);
						}
					}
					else if ((@char = HttpUtility.GetChar(s, num2 + 1, 2)) != -1)
					{
						HttpUtility.WriteCharBytes(list, (char)@char, e);
						num2 += 2;
					}
					else
					{
						HttpUtility.WriteCharBytes(list, '%', e);
					}
				}
				else if (c == '+')
				{
					HttpUtility.WriteCharBytes(list, ' ', e);
				}
				else
				{
					HttpUtility.WriteCharBytes(list, c, e);
				}
				num2++;
			}
			byte[] array = list.ToArray();
			return e.GetString(array, 0, array.Length);
		}

		
		public static string UrlDecode(byte[] bytes, Encoding e)
		{
			if (bytes == null)
			{
				return null;
			}
			return HttpUtility.UrlDecode(bytes, 0, bytes.Length, e);
		}

		
		private static int GetInt(byte b)
		{
			char c = (char)b;
			if (c >= '0' && c <= '9')
			{
				return (int)(c - '0');
			}
			if (c >= 'a' && c <= 'f')
			{
				return (int)(c - 'a' + '\n');
			}
			if (c >= 'A' && c <= 'F')
			{
				return (int)(c - 'A' + '\n');
			}
			return -1;
		}

		
		private static int GetChar(byte[] bytes, int offset, int length)
		{
			int num = 0;
			int num2 = length + offset;
			for (int i = offset; i < num2; i++)
			{
				int @int = HttpUtility.GetInt(bytes[i]);
				if (@int == -1)
				{
					return -1;
				}
				num = (num << 4) + @int;
			}
			return num;
		}

		
		private static int GetChar(string str, int offset, int length)
		{
			int num = 0;
			int num2 = length + offset;
			for (int i = offset; i < num2; i++)
			{
				char c = str[i];
				if (c > '\u007f')
				{
					return -1;
				}
				int @int = HttpUtility.GetInt((byte)c);
				if (@int == -1)
				{
					return -1;
				}
				num = (num << 4) + @int;
			}
			return num;
		}

		
		public static string UrlDecode(byte[] bytes, int offset, int count, Encoding e)
		{
			if (bytes == null)
			{
				return null;
			}
			if (count == 0)
			{
				return string.Empty;
			}
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			if (offset < 0 || offset > bytes.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (count < 0 || offset + count > bytes.Length)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			StringBuilder stringBuilder = new StringBuilder();
			MemoryStream memoryStream = new MemoryStream();
			int num = count + offset;
			int i = offset;
			while (i < num)
			{
				if (bytes[i] != 37 || i + 2 >= count || bytes[i + 1] == 37)
				{
					goto IL_11C;
				}
				if (bytes[i + 1] == 117 && i + 5 < num)
				{
					if (memoryStream.Length > 0L)
					{
						stringBuilder.Append(HttpUtility.GetChars(memoryStream, e));
						memoryStream.SetLength(0L);
					}
					int @char = HttpUtility.GetChar(bytes, i + 2, 4);
					if (@char == -1)
					{
						goto IL_11C;
					}
					stringBuilder.Append((char)@char);
					i += 5;
				}
				else
				{
					int @char;
					if ((@char = HttpUtility.GetChar(bytes, i + 1, 2)) == -1)
					{
						goto IL_11C;
					}
					memoryStream.WriteByte((byte)@char);
					i += 2;
				}
				IL_162:
				i++;
				continue;
				IL_11C:
				if (memoryStream.Length > 0L)
				{
					stringBuilder.Append(HttpUtility.GetChars(memoryStream, e));
					memoryStream.SetLength(0L);
				}
				if (bytes[i] == 43)
				{
					stringBuilder.Append(' ');
					goto IL_162;
				}
				stringBuilder.Append((char)bytes[i]);
				goto IL_162;
			}
			if (memoryStream.Length > 0L)
			{
				stringBuilder.Append(HttpUtility.GetChars(memoryStream, e));
			}
			return stringBuilder.ToString();
		}

		
		public static byte[] UrlDecodeToBytes(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}
			return HttpUtility.UrlDecodeToBytes(bytes, 0, bytes.Length);
		}

		
		public static byte[] UrlDecodeToBytes(string str)
		{
			return HttpUtility.UrlDecodeToBytes(str, Encoding.UTF8);
		}

		
		public static byte[] UrlDecodeToBytes(string str, Encoding e)
		{
			if (str == null)
			{
				return null;
			}
			if (e == null)
			{
				throw new ArgumentNullException("e");
			}
			return HttpUtility.UrlDecodeToBytes(e.GetBytes(str));
		}

		
		public static byte[] UrlDecodeToBytes(byte[] bytes, int offset, int count)
		{
			if (bytes == null)
			{
				return null;
			}
			if (count == 0)
			{
				return new byte[0];
			}
			int num = bytes.Length;
			if (offset < 0 || offset >= num)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (count < 0 || offset > num - count)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			MemoryStream memoryStream = new MemoryStream();
			int num2 = offset + count;
			for (int i = offset; i < num2; i++)
			{
				char c = (char)bytes[i];
				if (c == '+')
				{
					c = ' ';
				}
				else if (c == '%' && i < num2 - 2)
				{
					int @char = HttpUtility.GetChar(bytes, i + 1, 2);
					if (@char != -1)
					{
						c = (char)@char;
						i += 2;
					}
				}
				memoryStream.WriteByte((byte)c);
			}
			return memoryStream.ToArray();
		}

		
		public static string UrlEncode(string str)
		{
			return HttpUtility.UrlEncode(str, Encoding.UTF8);
		}

		
		public static string UrlEncode(string s, Encoding enc)
		{
			if (s == null)
			{
				return null;
			}
			if (s == string.Empty)
			{
				return string.Empty;
			}
			bool flag = false;
			int length = s.Length;
			for (int i = 0; i < length; i++)
			{
				char c = s[i];
				if (c < '0' || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || c > 'z')
				{
					if (!HttpEncoder.NotEncoded(c))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return s;
			}
			byte[] bytes = new byte[enc.GetMaxByteCount(s.Length)];
			int bytes2 = enc.GetBytes(s, 0, s.Length, bytes, 0);
			byte[] array = HttpUtility.UrlEncodeToBytes(bytes, 0, bytes2);
			return Encoding.ASCII.GetString(array, 0, array.Length);
		}

		
		public static string UrlEncode(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}
			if (bytes.Length == 0)
			{
				return string.Empty;
			}
			byte[] array = HttpUtility.UrlEncodeToBytes(bytes, 0, bytes.Length);
			return Encoding.ASCII.GetString(array, 0, array.Length);
		}

		
		public static string UrlEncode(byte[] bytes, int offset, int count)
		{
			if (bytes == null)
			{
				return null;
			}
			if (bytes.Length == 0)
			{
				return string.Empty;
			}
			byte[] array = HttpUtility.UrlEncodeToBytes(bytes, offset, count);
			return Encoding.ASCII.GetString(array, 0, array.Length);
		}

		
		public static byte[] UrlEncodeToBytes(string str)
		{
			return HttpUtility.UrlEncodeToBytes(str, Encoding.UTF8);
		}

		
		public static byte[] UrlEncodeToBytes(string str, Encoding e)
		{
			if (str == null)
			{
				return null;
			}
			if (str.Length == 0)
			{
				return new byte[0];
			}
			byte[] bytes = e.GetBytes(str);
			return HttpUtility.UrlEncodeToBytes(bytes, 0, bytes.Length);
		}

		
		public static byte[] UrlEncodeToBytes(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}
			if (bytes.Length == 0)
			{
				return new byte[0];
			}
			return HttpUtility.UrlEncodeToBytes(bytes, 0, bytes.Length);
		}

		
		public static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
		{
			if (bytes == null)
			{
				return null;
			}
			return HttpEncoder.UrlEncodeToBytes(bytes, offset, count);
		}

		
		public static string UrlEncodeUnicode(string str)
		{
			if (str == null)
			{
				return null;
			}
			byte[] array = HttpUtility.UrlEncodeUnicodeToBytes(str);
			return Encoding.ASCII.GetString(array, 0, array.Length);
		}

		
		public static byte[] UrlEncodeUnicodeToBytes(string str)
		{
			if (str == null)
			{
				return null;
			}
			if (str.Length == 0)
			{
				return new byte[0];
			}
			MemoryStream memoryStream = new MemoryStream(str.Length);
			foreach (char c in str)
			{
				HttpEncoder.UrlEncodeChar(c, memoryStream, true);
			}
			return memoryStream.ToArray();
		}

		
		public static string HtmlDecode(string s)
		{
			return HttpEncoder.HtmlDecode(s);
		}

		
		public static void HtmlDecode(string s, TextWriter output)
		{
			if (output == null)
			{
				throw new NullReferenceException(".NET emulation");
			}
			if (!string.IsNullOrEmpty(s))
			{
				output.Write(HttpEncoder.HtmlDecode(s));
			}
		}

		
		public static string HtmlEncode(string s)
		{
			return HttpEncoder.HtmlEncode(s);
		}

		
		public static void HtmlEncode(string s, TextWriter output)
		{
			if (output == null)
			{
				throw new NullReferenceException(".NET emulation");
			}
			if (!string.IsNullOrEmpty(s))
			{
				output.Write(HttpEncoder.HtmlEncode(s));
			}
		}

		
		public static string UrlPathEncode(string s)
		{
			return HttpEncoder.UrlPathEncode(s);
		}

		
		public static NameValueCollection ParseQueryString(string query)
		{
			return HttpUtility.ParseQueryString(query, Encoding.UTF8);
		}

		
		public static NameValueCollection ParseQueryString(string query, Encoding encoding)
		{
			if (query == null)
			{
				throw new ArgumentNullException("query");
			}
			if (encoding == null)
			{
				throw new ArgumentNullException("encoding");
			}
			if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
			{
				return new NameValueCollection();
			}
			if (query[0] == '?')
			{
				query = query.Substring(1);
			}
			NameValueCollection result = new HttpUtility.HttpQsCollection();
			HttpUtility.ParseQueryString(query, encoding, result);
			return result;
		}

		
		internal static void ParseQueryString(string query, Encoding encoding, NameValueCollection result)
		{
			if (query.Length == 0)
			{
				return;
			}
			string text = HttpUtility.HtmlDecode(query);
			int length = text.Length;
			int i = 0;
			bool flag = true;
			while (i <= length)
			{
				int num = -1;
				int num2 = -1;
				for (int j = i; j < length; j++)
				{
					if (num == -1 && text[j] == '=')
					{
						num = j + 1;
					}
					else if (text[j] == '&')
					{
						num2 = j;
						break;
					}
				}
				if (flag)
				{
					flag = false;
					if (text[i] == '?')
					{
						i++;
					}
				}
				string name;
				if (num == -1)
				{
					name = null;
					num = i;
				}
				else
				{
					name = HttpUtility.UrlDecode(text.Substring(i, num - i - 1), encoding);
				}
				if (num2 < 0)
				{
					i = -1;
					num2 = text.Length;
				}
				else
				{
					i = num2 + 1;
				}
				string val = HttpUtility.UrlDecode(text.Substring(num, num2 - num), encoding);
				result.Add(name, val);
				if (i == -1)
				{
					break;
				}
			}
		}

		
		private sealed class HttpQsCollection : NameValueCollection
		{
			
			public override string ToString()
			{
				int count = this.Count;
				if (count == 0)
				{
					return string.Empty;
				}
				StringBuilder stringBuilder = new StringBuilder();
				string[] allKeys = this.AllKeys;
				for (int i = 0; i < count; i++)
				{
					stringBuilder.AppendFormat("{0}={1}&", allKeys[i], base[allKeys[i]]);
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Length--;
				}
				return stringBuilder.ToString();
			}
		}
	}
}
