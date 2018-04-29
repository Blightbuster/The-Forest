using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class EffectIdentifierResponse : IEquatable<EffectIdentifierResponse>
	{
		
		public EffectIdentifierResponse(int? Result = null, List<EffectIdentifierResponseResults> Results = null)
		{
			this.Result = Result;
			this.Results = Results;
		}

		
		
		
		[JsonProperty(PropertyName = "result")]
		[DataMember(Name = "result")]
		public int? Result { get; set; }

		
		
		
		[DataMember(Name = "results")]
		[JsonProperty(PropertyName = "results")]
		public List<EffectIdentifierResponseResults> Results { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class EffectIdentifierResponse {\n");
			stringBuilder.Append("  Result: ").Append(this.Result).Append("\n");
			stringBuilder.Append("  Results: ").Append(this.Results).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as EffectIdentifierResponse);
		}

		
		public bool Equals(EffectIdentifierResponse other)
		{
			if (other == null)
			{
				return false;
			}
			int? result = this.Result;
			int valueOrDefault = result.GetValueOrDefault();
			int? result2 = other.Result;
			return ((valueOrDefault == result2.GetValueOrDefault() && result != null == (result2 != null)) || (this.Result != null && this.Result.Equals(other.Result))) && (this.Results == other.Results || (this.Results != null && this.Results.SequenceEqual(other.Results)));
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Result != null)
			{
				num = num * 59 + this.Result.GetHashCode();
			}
			if (this.Results != null)
			{
				num = num * 59 + this.Results.GetHashCode();
			}
			return num;
		}
	}
}
