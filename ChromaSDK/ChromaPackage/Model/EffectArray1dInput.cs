using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class EffectArray1dInput : List<int?>, IEquatable<EffectArray1dInput>
	{
		
		[JsonConstructor]
		public EffectArray1dInput()
		{
		}

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class EffectArray1dInput {\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as EffectArray1dInput);
		}

		
		public bool Equals(EffectArray1dInput other)
		{
			return other == null && false;
		}

		
		public override int GetHashCode()
		{
			return 41;
		}
	}
}
