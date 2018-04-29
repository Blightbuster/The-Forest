using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class EffectInput : IEquatable<EffectInput>
	{
		
		[JsonConstructor]
		protected EffectInput()
		{
		}

		
		public EffectInput(EffectType Effect = EffectType.CHROMA_NONE, EffectInputParam Param = null)
		{
			this.Effect = Effect;
			this.Param = Param;
		}

		
		
		
		[DataMember(Name = "effect")]
		[JsonProperty(PropertyName = "effect")]
		public EffectType Effect { get; set; }

		
		
		
		[JsonProperty(PropertyName = "param")]
		[DataMember(Name = "param")]
		public EffectInputParam Param { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class EffectInput {\n");
			stringBuilder.Append("  Effect: ").Append(this.Effect).Append("\n");
			stringBuilder.Append("  Param: ").Append(this.Param).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as EffectInput);
		}

		
		public bool Equals(EffectInput other)
		{
			return other != null && (this.Effect == other.Effect || this.Effect.Equals(other.Effect)) && (this.Param == other.Param || (this.Param != null && this.Param.Equals(other.Param)));
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			num = num * 59 + this.Effect.GetHashCode();
			if (this.Param != null)
			{
				num = num * 59 + this.Param.GetHashCode();
			}
			return num;
		}
	}
}
