using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class EffectInputParam : IEquatable<EffectInputParam>
	{
		
		public EffectInputParam(int? Color = null, int? Color1 = null, int? Color2 = null, int? Direction = null, int? Duration = null, int? Type = null)
		{
			this.Color = Color;
			this.Color1 = Color1;
			this.Color2 = Color2;
			this.Direction = Direction;
			this.Duration = Duration;
			this.Type = Type;
		}

		
		
		
		[DataMember(Name = "color")]
		[JsonProperty(PropertyName = "color")]
		public int? Color { get; set; }

		
		
		
		[DataMember(Name = "color1")]
		[JsonProperty(PropertyName = "color1")]
		public int? Color1 { get; set; }

		
		
		
		[JsonProperty(PropertyName = "color2")]
		[DataMember(Name = "color2")]
		public int? Color2 { get; set; }

		
		
		
		[DataMember(Name = "direction")]
		[JsonProperty(PropertyName = "direction")]
		public int? Direction { get; set; }

		
		
		
		[DataMember(Name = "duration")]
		[JsonProperty(PropertyName = "duration")]
		public int? Duration { get; set; }

		
		
		
		[JsonProperty(PropertyName = "type")]
		[DataMember(Name = "type")]
		public int? Type { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class EffectInputParam {\n");
			stringBuilder.Append("  Color: ").Append(this.Color).Append("\n");
			stringBuilder.Append("  Color1: ").Append(this.Color1).Append("\n");
			stringBuilder.Append("  Color2: ").Append(this.Color2).Append("\n");
			stringBuilder.Append("  Direction: ").Append(this.Direction).Append("\n");
			stringBuilder.Append("  Duration: ").Append(this.Duration).Append("\n");
			stringBuilder.Append("  Type: ").Append(this.Type).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as EffectInputParam);
		}

		
		public bool Equals(EffectInputParam other)
		{
			if (other == null)
			{
				return false;
			}
			int? color = this.Color;
			int valueOrDefault = color.GetValueOrDefault();
			int? color2 = other.Color;
			if ((valueOrDefault == color2.GetValueOrDefault() && color != null == (color2 != null)) || (this.Color != null && this.Color.Equals(other.Color)))
			{
				int? color3 = this.Color1;
				int valueOrDefault2 = color3.GetValueOrDefault();
				int? color4 = other.Color1;
				if ((valueOrDefault2 == color4.GetValueOrDefault() && color3 != null == (color4 != null)) || (this.Color1 != null && this.Color1.Equals(other.Color1)))
				{
					int? color5 = this.Color2;
					int valueOrDefault3 = color5.GetValueOrDefault();
					int? color6 = other.Color2;
					if ((valueOrDefault3 == color6.GetValueOrDefault() && color5 != null == (color6 != null)) || (this.Color2 != null && this.Color2.Equals(other.Color2)))
					{
						int? direction = this.Direction;
						int valueOrDefault4 = direction.GetValueOrDefault();
						int? direction2 = other.Direction;
						if ((valueOrDefault4 == direction2.GetValueOrDefault() && direction != null == (direction2 != null)) || (this.Direction != null && this.Direction.Equals(other.Direction)))
						{
							int? duration = this.Duration;
							int valueOrDefault5 = duration.GetValueOrDefault();
							int? duration2 = other.Duration;
							if ((valueOrDefault5 == duration2.GetValueOrDefault() && duration != null == (duration2 != null)) || (this.Duration != null && this.Duration.Equals(other.Duration)))
							{
								int? type = this.Type;
								int valueOrDefault6 = type.GetValueOrDefault();
								int? type2 = other.Type;
								return (valueOrDefault6 == type2.GetValueOrDefault() && type != null == (type2 != null)) || (this.Type != null && this.Type.Equals(other.Type));
							}
						}
					}
				}
			}
			return false;
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Color != null)
			{
				num = num * 59 + this.Color.GetHashCode();
			}
			if (this.Color1 != null)
			{
				num = num * 59 + this.Color1.GetHashCode();
			}
			if (this.Color2 != null)
			{
				num = num * 59 + this.Color2.GetHashCode();
			}
			if (this.Direction != null)
			{
				num = num * 59 + this.Direction.GetHashCode();
			}
			if (this.Duration != null)
			{
				num = num * 59 + this.Duration.GetHashCode();
			}
			if (this.Type != null)
			{
				num = num * 59 + this.Type.GetHashCode();
			}
			return num;
		}
	}
}
