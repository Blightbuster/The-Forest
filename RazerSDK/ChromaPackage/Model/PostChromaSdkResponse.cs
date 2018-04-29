using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace RazerSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class PostChromaSdkResponse : IEquatable<PostChromaSdkResponse>
	{
		
		public PostChromaSdkResponse(string Sessionid = null, string Uri = null)
		{
			this.Sessionid = Sessionid;
			this.Uri = Uri;
		}

		
		
		
		[DataMember(Name = "sessionid")]
		[JsonProperty(PropertyName = "sessionid")]
		public string Sessionid { get; set; }

		
		
		
		[DataMember(Name = "uri")]
		[JsonProperty(PropertyName = "uri")]
		public string Uri { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class PostChromaSdkResponse {\n");
			stringBuilder.Append("  Sessionid: ").Append(this.Sessionid).Append("\n");
			stringBuilder.Append("  Uri: ").Append(this.Uri).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as PostChromaSdkResponse);
		}

		
		public bool Equals(PostChromaSdkResponse other)
		{
			return other != null && (this.Sessionid == other.Sessionid || (this.Sessionid != null && this.Sessionid.Equals(other.Sessionid))) && (this.Uri == other.Uri || (this.Uri != null && this.Uri.Equals(other.Uri)));
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Sessionid != null)
			{
				num = num * 59 + this.Sessionid.GetHashCode();
			}
			if (this.Uri != null)
			{
				num = num * 59 + this.Uri.GetHashCode();
			}
			return num;
		}
	}
}
