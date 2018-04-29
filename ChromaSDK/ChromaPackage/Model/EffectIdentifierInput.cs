using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class EffectIdentifierInput : IEquatable<EffectIdentifierInput>
	{
		
		public EffectIdentifierInput(string Id = null, List<string> Ids = null)
		{
			this.Id = Id;
			this.Ids = Ids;
		}

		
		
		
		[DataMember(Name = "id")]
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		
		
		
		[DataMember(Name = "ids")]
		[JsonProperty(PropertyName = "ids")]
		public List<string> Ids { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class EffectIdentifierInput {\n");
			stringBuilder.Append("  Id: ").Append(this.Id).Append("\n");
			stringBuilder.Append("  Ids: ").Append(this.Ids).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as EffectIdentifierInput);
		}

		
		public bool Equals(EffectIdentifierInput other)
		{
			return other != null && (this.Id == other.Id || (this.Id != null && this.Id.Equals(other.Id))) && (this.Ids == other.Ids || (this.Ids != null && this.Ids.SequenceEqual(other.Ids)));
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Id != null)
			{
				num = num * 59 + this.Id.GetHashCode();
			}
			if (this.Ids != null)
			{
				num = num * 59 + this.Ids.GetHashCode();
			}
			return num;
		}
	}
}
