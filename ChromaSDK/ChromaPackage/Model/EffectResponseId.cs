using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class EffectResponseId : IEquatable<EffectResponseId>
	{
		
		[JsonConstructor]
		protected EffectResponseId()
		{
		}

		
		public EffectResponseId(string Id = null, int? Result = null)
		{
			if (Result == null)
			{
				throw new Exception("Result is a required property for EffectResponseId and cannot be null");
			}
			this.Result = Result;
			this.Id = Id;
		}

		
		
		
		[JsonProperty(PropertyName = "id")]
		[DataMember(Name = "id")]
		public string Id { get; set; }

		
		
		
		[DataMember(Name = "result")]
		[JsonProperty(PropertyName = "result")]
		public int? Result { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class EffectResponseId {\n");
			stringBuilder.Append("  Id: ").Append(this.Id).Append("\n");
			stringBuilder.Append("  Result: ").Append(this.Result).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as EffectResponseId);
		}

		
		public bool Equals(EffectResponseId other)
		{
			if (other == null)
			{
				return false;
			}
			bool result3;
			if (this.Id == other.Id || (this.Id != null && this.Id.Equals(other.Id)))
			{
				int? result = this.Result;
				int valueOrDefault = result.GetValueOrDefault();
				int? result2 = other.Result;
				result3 = ((valueOrDefault == result2.GetValueOrDefault() && result != null == (result2 != null)) || (this.Result != null && this.Result.Equals(other.Result)));
			}
			else
			{
				result3 = false;
			}
			return result3;
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Id != null)
			{
				num = num * 59 + this.Id.GetHashCode();
			}
			if (this.Result != null)
			{
				num = num * 59 + this.Result.GetHashCode();
			}
			return num;
		}
	}
}
