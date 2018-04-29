using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace RazerSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class ChromaSdkInputAuthor : IEquatable<ChromaSdkInputAuthor>
	{
		
		public ChromaSdkInputAuthor(string Name = null, string Contact = null)
		{
			this.Name = Name;
			this.Contact = Contact;
		}

		
		
		
		[DataMember(Name = "name")]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		
		
		
		[DataMember(Name = "contact")]
		[JsonProperty(PropertyName = "contact")]
		public string Contact { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class ChromaSdkInputAuthor {\n");
			stringBuilder.Append("  Name: ").Append(this.Name).Append("\n");
			stringBuilder.Append("  Contact: ").Append(this.Contact).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ChromaSdkInputAuthor);
		}

		
		public bool Equals(ChromaSdkInputAuthor other)
		{
			return other != null && (this.Name == other.Name || (this.Name != null && this.Name.Equals(other.Name))) && (this.Contact == other.Contact || (this.Contact != null && this.Contact.Equals(other.Contact)));
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Name != null)
			{
				num = num * 59 + this.Name.GetHashCode();
			}
			if (this.Contact != null)
			{
				num = num * 59 + this.Contact.GetHashCode();
			}
			return num;
		}
	}
}
