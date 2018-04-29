using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RestSharp.Contrib
{
	
	internal class HttpEncoder
	{
		
		
		private static IDictionary<string, char> Entities
		{
			get
			{
				object obj = HttpEncoder.entitiesLock;
				IDictionary<string, char> result;
				lock (obj)
				{
					if (HttpEncoder.entities == null)
					{
						HttpEncoder.InitEntities();
					}
					result = HttpEncoder.entities;
				}
				return result;
			}
		}

		
		
		public static HttpEncoder Current
		{
			get
			{
				return HttpEncoder.currentEncoder;
			}
		}

		
		
		public static HttpEncoder Default
		{
			get
			{
				return HttpEncoder.defaultEncoder;
			}
		}

		
		internal static void HeaderNameValueEncode(string headerName, string headerValue, out string encodedHeaderName, out string encodedHeaderValue)
		{
			encodedHeaderName = ((!string.IsNullOrEmpty(headerName)) ? HttpEncoder.EncodeHeaderString(headerName) : headerName);
			encodedHeaderValue = ((!string.IsNullOrEmpty(headerValue)) ? HttpEncoder.EncodeHeaderString(headerValue) : headerValue);
		}

		
		private static void StringBuilderAppend(string s, ref StringBuilder sb)
		{
			if (sb == null)
			{
				sb = new StringBuilder(s);
			}
			else
			{
				sb.Append(s);
			}
		}

		
		private static string EncodeHeaderString(string input)
		{
			StringBuilder stringBuilder = null;
			foreach (char c in input)
			{
				if ((c < ' ' && c != '\t') || c == '\u007f')
				{
					HttpEncoder.StringBuilderAppend(string.Format("%{0:x2}", (int)c), ref stringBuilder);
				}
			}
			return (stringBuilder == null) ? input : stringBuilder.ToString();
		}

		
		internal static string UrlPathEncode(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			MemoryStream memoryStream = new MemoryStream();
			int length = value.Length;
			for (int i = 0; i < length; i++)
			{
				HttpEncoder.UrlPathEncodeChar(value[i], memoryStream);
			}
			byte[] array = memoryStream.ToArray();
			return Encoding.ASCII.GetString(array, 0, array.Length);
		}

		
		internal static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			int num = bytes.Length;
			if (num == 0)
			{
				return new byte[0];
			}
			if (offset < 0 || offset >= num)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (count < 0 || count > num - offset)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			MemoryStream memoryStream = new MemoryStream(count);
			int num2 = offset + count;
			for (int i = offset; i < num2; i++)
			{
				HttpEncoder.UrlEncodeChar((char)bytes[i], memoryStream, false);
			}
			return memoryStream.ToArray();
		}

		
		internal static string HtmlEncode(string s)
		{
			if (s == null)
			{
				return null;
			}
			if (s.Length == 0)
			{
				return string.Empty;
			}
			bool flag = false;
			foreach (char c in s)
			{
				if (c == '&' || c == '"' || c == '<' || c == '>' || c > '\u009f')
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return s;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int length = s.Length;
			for (int j = 0; j < length; j++)
			{
				char c2 = s[j];
				switch (c2)
				{
				case '<':
					stringBuilder.Append("&lt;");
					break;
				default:
					switch (c2)
					{
					case '＜':
						stringBuilder.Append("&#65308;");
						break;
					default:
						if (c2 != '"')
						{
							if (c2 != '&')
							{
								char c3 = s[j];
								if (c3 > '\u009f' && c3 < 'Ā')
								{
									stringBuilder.Append("&#");
									StringBuilder stringBuilder2 = stringBuilder;
									int num = (int)c3;
									stringBuilder2.Append(num.ToString(Helpers.InvariantCulture));
									stringBuilder.Append(";");
								}
								else
								{
									stringBuilder.Append(c3);
								}
							}
							else
							{
								stringBuilder.Append("&amp;");
							}
						}
						else
						{
							stringBuilder.Append("&quot;");
						}
						break;
					case '＞':
						stringBuilder.Append("&#65310;");
						break;
					}
					break;
				case '>':
					stringBuilder.Append("&gt;");
					break;
				}
			}
			return stringBuilder.ToString();
		}

		
		internal static string HtmlAttributeEncode(string s)
		{
			if (s == null)
			{
				return null;
			}
			if (s.Length == 0)
			{
				return string.Empty;
			}
			bool flag = false;
			foreach (char c in s)
			{
				if (c == '&' || c == '"' || c == '<')
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return s;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int length = s.Length;
			for (int j = 0; j < length; j++)
			{
				char c2 = s[j];
				if (c2 != '"')
				{
					if (c2 != '&')
					{
						if (c2 != '<')
						{
							stringBuilder.Append(s[j]);
						}
						else
						{
							stringBuilder.Append("&lt;");
						}
					}
					else
					{
						stringBuilder.Append("&amp;");
					}
				}
				else
				{
					stringBuilder.Append("&quot;");
				}
			}
			return stringBuilder.ToString();
		}

		
		internal static string HtmlDecode(string s)
		{
			if (s == null)
			{
				return null;
			}
			if (s.Length == 0)
			{
				return string.Empty;
			}
			if (s.IndexOf('&') == -1)
			{
				return s;
			}
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			int length = s.Length;
			int num = 0;
			int num2 = 0;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < length; i++)
			{
				char c = s[i];
				if (num == 0)
				{
					if (c == '&')
					{
						stringBuilder.Append(c);
						num = 1;
					}
					else
					{
						stringBuilder2.Append(c);
					}
				}
				else if (c == '&')
				{
					num = 1;
					if (flag2)
					{
						stringBuilder.Append(num2.ToString(Helpers.InvariantCulture));
						flag2 = false;
					}
					stringBuilder2.Append(stringBuilder);
					stringBuilder.Length = 0;
					stringBuilder.Append('&');
				}
				else
				{
					switch (num)
					{
					case 1:
						if (c == ';')
						{
							num = 0;
							stringBuilder2.Append(stringBuilder);
							stringBuilder2.Append(c);
							stringBuilder.Length = 0;
						}
						else
						{
							num2 = 0;
							flag = false;
							num = ((c == '#') ? 3 : 2);
							stringBuilder.Append(c);
						}
						break;
					case 2:
						stringBuilder.Append(c);
						if (c == ';')
						{
							string text = stringBuilder.ToString();
							if (text.Length > 1 && HttpEncoder.Entities.ContainsKey(text.Substring(1, text.Length - 2)))
							{
								text = HttpEncoder.Entities[text.Substring(1, text.Length - 2)].ToString();
							}
							stringBuilder2.Append(text);
							num = 0;
							stringBuilder.Length = 0;
						}
						break;
					case 3:
						if (c == ';')
						{
							if (num2 > 65535)
							{
								stringBuilder2.Append("&#");
								stringBuilder2.Append(num2.ToString(Helpers.InvariantCulture));
								stringBuilder2.Append(";");
							}
							else
							{
								stringBuilder2.Append((char)num2);
							}
							num = 0;
							stringBuilder.Length = 0;
							flag2 = false;
						}
						else if (flag && HttpEncoder.IsHexDigit(c))
						{
							num2 = num2 * 16 + HttpEncoder.FromHex(c);
							flag2 = true;
						}
						else if (char.IsDigit(c))
						{
							num2 = num2 * 10 + (int)(c - '0');
							flag2 = true;
						}
						else if (num2 == 0 && (c == 'x' || c == 'X'))
						{
							flag = true;
						}
						else
						{
							num = 2;
							if (flag2)
							{
								stringBuilder.Append(num2.ToString(Helpers.InvariantCulture));
								flag2 = false;
							}
							stringBuilder.Append(c);
						}
						break;
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder2.Append(stringBuilder);
			}
			else if (flag2)
			{
				stringBuilder2.Append(num2.ToString(Helpers.InvariantCulture));
			}
			return stringBuilder2.ToString();
		}

		
		internal static int FromHex(char c)
		{
			return int.Parse(Convert.ToString(c), NumberStyles.HexNumber);
		}

		
		internal static bool IsHexDigit(char c)
		{
			string text = "0123456789abcdefABCDEF";
			return text.IndexOf(c) >= 0;
		}

		
		internal static bool NotEncoded(char c)
		{
			return c == '!' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_';
		}

		
		internal static void UrlEncodeChar(char c, Stream result, bool isUnicode)
		{
			if (c > 'ÿ')
			{
				result.WriteByte(37);
				result.WriteByte(117);
				int num = (int)(c >> 12);
				result.WriteByte((byte)HttpEncoder.hexChars[num]);
				num = (int)(c >> 8 & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[num]);
				num = (int)(c >> 4 & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[num]);
				num = (int)(c & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[num]);
				return;
			}
			if (c > ' ' && HttpEncoder.NotEncoded(c))
			{
				result.WriteByte((byte)c);
				return;
			}
			if (c == ' ')
			{
				result.WriteByte(43);
				return;
			}
			if (c < '0' || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || c > 'z')
			{
				if (isUnicode && c > '\u007f')
				{
					result.WriteByte(37);
					result.WriteByte(117);
					result.WriteByte(48);
					result.WriteByte(48);
				}
				else
				{
					result.WriteByte(37);
				}
				int num2 = (int)(c >> 4);
				result.WriteByte((byte)HttpEncoder.hexChars[num2]);
				num2 = (int)(c & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[num2]);
			}
			else
			{
				result.WriteByte((byte)c);
			}
		}

		
		internal static void UrlPathEncodeChar(char c, Stream result)
		{
			if (c < '!' || c > '~')
			{
				byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
				for (int i = 0; i < bytes.Length; i++)
				{
					result.WriteByte(37);
					int num = bytes[i] >> 4;
					result.WriteByte((byte)HttpEncoder.hexChars[num]);
					num = (int)(bytes[i] & 15);
					result.WriteByte((byte)HttpEncoder.hexChars[num]);
				}
			}
			else if (c == ' ')
			{
				result.WriteByte(37);
				result.WriteByte(50);
				result.WriteByte(48);
			}
			else
			{
				result.WriteByte((byte)c);
			}
		}

		
		private static void InitEntities()
		{
			HttpEncoder.entities = new SortedDictionary<string, char>(StringComparer.Ordinal)
			{
				{
					"nbsp",
					'\u00a0'
				},
				{
					"iexcl",
					'¡'
				},
				{
					"cent",
					'¢'
				},
				{
					"pound",
					'£'
				},
				{
					"curren",
					'¤'
				},
				{
					"yen",
					'¥'
				},
				{
					"brvbar",
					'¦'
				},
				{
					"sect",
					'§'
				},
				{
					"uml",
					'¨'
				},
				{
					"copy",
					'©'
				},
				{
					"ordf",
					'ª'
				},
				{
					"laquo",
					'«'
				},
				{
					"not",
					'¬'
				},
				{
					"shy",
					'­'
				},
				{
					"reg",
					'®'
				},
				{
					"macr",
					'¯'
				},
				{
					"deg",
					'°'
				},
				{
					"plusmn",
					'±'
				},
				{
					"sup2",
					'²'
				},
				{
					"sup3",
					'³'
				},
				{
					"acute",
					'´'
				},
				{
					"micro",
					'µ'
				},
				{
					"para",
					'¶'
				},
				{
					"middot",
					'·'
				},
				{
					"cedil",
					'¸'
				},
				{
					"sup1",
					'¹'
				},
				{
					"ordm",
					'º'
				},
				{
					"raquo",
					'»'
				},
				{
					"frac14",
					'¼'
				},
				{
					"frac12",
					'½'
				},
				{
					"frac34",
					'¾'
				},
				{
					"iquest",
					'¿'
				},
				{
					"Agrave",
					'À'
				},
				{
					"Aacute",
					'Á'
				},
				{
					"Acirc",
					'Â'
				},
				{
					"Atilde",
					'Ã'
				},
				{
					"Auml",
					'Ä'
				},
				{
					"Aring",
					'Å'
				},
				{
					"AElig",
					'Æ'
				},
				{
					"Ccedil",
					'Ç'
				},
				{
					"Egrave",
					'È'
				},
				{
					"Eacute",
					'É'
				},
				{
					"Ecirc",
					'Ê'
				},
				{
					"Euml",
					'Ë'
				},
				{
					"Igrave",
					'Ì'
				},
				{
					"Iacute",
					'Í'
				},
				{
					"Icirc",
					'Î'
				},
				{
					"Iuml",
					'Ï'
				},
				{
					"ETH",
					'Ð'
				},
				{
					"Ntilde",
					'Ñ'
				},
				{
					"Ograve",
					'Ò'
				},
				{
					"Oacute",
					'Ó'
				},
				{
					"Ocirc",
					'Ô'
				},
				{
					"Otilde",
					'Õ'
				},
				{
					"Ouml",
					'Ö'
				},
				{
					"times",
					'×'
				},
				{
					"Oslash",
					'Ø'
				},
				{
					"Ugrave",
					'Ù'
				},
				{
					"Uacute",
					'Ú'
				},
				{
					"Ucirc",
					'Û'
				},
				{
					"Uuml",
					'Ü'
				},
				{
					"Yacute",
					'Ý'
				},
				{
					"THORN",
					'Þ'
				},
				{
					"szlig",
					'ß'
				},
				{
					"agrave",
					'à'
				},
				{
					"aacute",
					'á'
				},
				{
					"acirc",
					'â'
				},
				{
					"atilde",
					'ã'
				},
				{
					"auml",
					'ä'
				},
				{
					"aring",
					'å'
				},
				{
					"aelig",
					'æ'
				},
				{
					"ccedil",
					'ç'
				},
				{
					"egrave",
					'è'
				},
				{
					"eacute",
					'é'
				},
				{
					"ecirc",
					'ê'
				},
				{
					"euml",
					'ë'
				},
				{
					"igrave",
					'ì'
				},
				{
					"iacute",
					'í'
				},
				{
					"icirc",
					'î'
				},
				{
					"iuml",
					'ï'
				},
				{
					"eth",
					'ð'
				},
				{
					"ntilde",
					'ñ'
				},
				{
					"ograve",
					'ò'
				},
				{
					"oacute",
					'ó'
				},
				{
					"ocirc",
					'ô'
				},
				{
					"otilde",
					'õ'
				},
				{
					"ouml",
					'ö'
				},
				{
					"divide",
					'÷'
				},
				{
					"oslash",
					'ø'
				},
				{
					"ugrave",
					'ù'
				},
				{
					"uacute",
					'ú'
				},
				{
					"ucirc",
					'û'
				},
				{
					"uuml",
					'ü'
				},
				{
					"yacute",
					'ý'
				},
				{
					"thorn",
					'þ'
				},
				{
					"yuml",
					'ÿ'
				},
				{
					"fnof",
					'ƒ'
				},
				{
					"Alpha",
					'Α'
				},
				{
					"Beta",
					'Β'
				},
				{
					"Gamma",
					'Γ'
				},
				{
					"Delta",
					'Δ'
				},
				{
					"Epsilon",
					'Ε'
				},
				{
					"Zeta",
					'Ζ'
				},
				{
					"Eta",
					'Η'
				},
				{
					"Theta",
					'Θ'
				},
				{
					"Iota",
					'Ι'
				},
				{
					"Kappa",
					'Κ'
				},
				{
					"Lambda",
					'Λ'
				},
				{
					"Mu",
					'Μ'
				},
				{
					"Nu",
					'Ν'
				},
				{
					"Xi",
					'Ξ'
				},
				{
					"Omicron",
					'Ο'
				},
				{
					"Pi",
					'Π'
				},
				{
					"Rho",
					'Ρ'
				},
				{
					"Sigma",
					'Σ'
				},
				{
					"Tau",
					'Τ'
				},
				{
					"Upsilon",
					'Υ'
				},
				{
					"Phi",
					'Φ'
				},
				{
					"Chi",
					'Χ'
				},
				{
					"Psi",
					'Ψ'
				},
				{
					"Omega",
					'Ω'
				},
				{
					"alpha",
					'α'
				},
				{
					"beta",
					'β'
				},
				{
					"gamma",
					'γ'
				},
				{
					"delta",
					'δ'
				},
				{
					"epsilon",
					'ε'
				},
				{
					"zeta",
					'ζ'
				},
				{
					"eta",
					'η'
				},
				{
					"theta",
					'θ'
				},
				{
					"iota",
					'ι'
				},
				{
					"kappa",
					'κ'
				},
				{
					"lambda",
					'λ'
				},
				{
					"mu",
					'μ'
				},
				{
					"nu",
					'ν'
				},
				{
					"xi",
					'ξ'
				},
				{
					"omicron",
					'ο'
				},
				{
					"pi",
					'π'
				},
				{
					"rho",
					'ρ'
				},
				{
					"sigmaf",
					'ς'
				},
				{
					"sigma",
					'σ'
				},
				{
					"tau",
					'τ'
				},
				{
					"upsilon",
					'υ'
				},
				{
					"phi",
					'φ'
				},
				{
					"chi",
					'χ'
				},
				{
					"psi",
					'ψ'
				},
				{
					"omega",
					'ω'
				},
				{
					"thetasym",
					'ϑ'
				},
				{
					"upsih",
					'ϒ'
				},
				{
					"piv",
					'ϖ'
				},
				{
					"bull",
					'•'
				},
				{
					"hellip",
					'…'
				},
				{
					"prime",
					'′'
				},
				{
					"Prime",
					'″'
				},
				{
					"oline",
					'‾'
				},
				{
					"frasl",
					'⁄'
				},
				{
					"weierp",
					'℘'
				},
				{
					"image",
					'ℑ'
				},
				{
					"real",
					'ℜ'
				},
				{
					"trade",
					'™'
				},
				{
					"alefsym",
					'ℵ'
				},
				{
					"larr",
					'←'
				},
				{
					"uarr",
					'↑'
				},
				{
					"rarr",
					'→'
				},
				{
					"darr",
					'↓'
				},
				{
					"harr",
					'↔'
				},
				{
					"crarr",
					'↵'
				},
				{
					"lArr",
					'⇐'
				},
				{
					"uArr",
					'⇑'
				},
				{
					"rArr",
					'⇒'
				},
				{
					"dArr",
					'⇓'
				},
				{
					"hArr",
					'⇔'
				},
				{
					"forall",
					'∀'
				},
				{
					"part",
					'∂'
				},
				{
					"exist",
					'∃'
				},
				{
					"empty",
					'∅'
				},
				{
					"nabla",
					'∇'
				},
				{
					"isin",
					'∈'
				},
				{
					"notin",
					'∉'
				},
				{
					"ni",
					'∋'
				},
				{
					"prod",
					'∏'
				},
				{
					"sum",
					'∑'
				},
				{
					"minus",
					'−'
				},
				{
					"lowast",
					'∗'
				},
				{
					"radic",
					'√'
				},
				{
					"prop",
					'∝'
				},
				{
					"infin",
					'∞'
				},
				{
					"ang",
					'∠'
				},
				{
					"and",
					'∧'
				},
				{
					"or",
					'∨'
				},
				{
					"cap",
					'∩'
				},
				{
					"cup",
					'∪'
				},
				{
					"int",
					'∫'
				},
				{
					"there4",
					'∴'
				},
				{
					"sim",
					'∼'
				},
				{
					"cong",
					'≅'
				},
				{
					"asymp",
					'≈'
				},
				{
					"ne",
					'≠'
				},
				{
					"equiv",
					'≡'
				},
				{
					"le",
					'≤'
				},
				{
					"ge",
					'≥'
				},
				{
					"sub",
					'⊂'
				},
				{
					"sup",
					'⊃'
				},
				{
					"nsub",
					'⊄'
				},
				{
					"sube",
					'⊆'
				},
				{
					"supe",
					'⊇'
				},
				{
					"oplus",
					'⊕'
				},
				{
					"otimes",
					'⊗'
				},
				{
					"perp",
					'⊥'
				},
				{
					"sdot",
					'⋅'
				},
				{
					"lceil",
					'⌈'
				},
				{
					"rceil",
					'⌉'
				},
				{
					"lfloor",
					'⌊'
				},
				{
					"rfloor",
					'⌋'
				},
				{
					"lang",
					'〈'
				},
				{
					"rang",
					'〉'
				},
				{
					"loz",
					'◊'
				},
				{
					"spades",
					'♠'
				},
				{
					"clubs",
					'♣'
				},
				{
					"hearts",
					'♥'
				},
				{
					"diams",
					'♦'
				},
				{
					"quot",
					'"'
				},
				{
					"amp",
					'&'
				},
				{
					"lt",
					'<'
				},
				{
					"gt",
					'>'
				},
				{
					"OElig",
					'Œ'
				},
				{
					"oelig",
					'œ'
				},
				{
					"Scaron",
					'Š'
				},
				{
					"scaron",
					'š'
				},
				{
					"Yuml",
					'Ÿ'
				},
				{
					"circ",
					'ˆ'
				},
				{
					"tilde",
					'˜'
				},
				{
					"ensp",
					'\u2002'
				},
				{
					"emsp",
					'\u2003'
				},
				{
					"thinsp",
					'\u2009'
				},
				{
					"zwnj",
					'‌'
				},
				{
					"zwj",
					'‍'
				},
				{
					"lrm",
					'‎'
				},
				{
					"rlm",
					'‏'
				},
				{
					"ndash",
					'–'
				},
				{
					"mdash",
					'—'
				},
				{
					"lsquo",
					'‘'
				},
				{
					"rsquo",
					'’'
				},
				{
					"sbquo",
					'‚'
				},
				{
					"ldquo",
					'“'
				},
				{
					"rdquo",
					'”'
				},
				{
					"bdquo",
					'„'
				},
				{
					"dagger",
					'†'
				},
				{
					"Dagger",
					'‡'
				},
				{
					"permil",
					'‰'
				},
				{
					"lsaquo",
					'‹'
				},
				{
					"rsaquo",
					'›'
				},
				{
					"euro",
					'€'
				}
			};
		}

		
		private static readonly char[] hexChars = "0123456789abcdef".ToCharArray();

		
		private static readonly object entitiesLock = new object();

		
		private static SortedDictionary<string, char> entities;

		
		private static readonly HttpEncoder defaultEncoder = new HttpEncoder();

		
		private static readonly HttpEncoder currentEncoder = HttpEncoder.defaultEncoder;
	}
}
