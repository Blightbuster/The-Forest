using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace RazerSDK.ChromaPackage.Model
{
	
	[DataContract]
	public class ChromaSdkInput : IEquatable<ChromaSdkInput>
	{
		
		public ChromaSdkInput(string Title = null, string Description = null, ChromaSdkInputAuthor Author = null, List<string> DeviceSupported = null, string Category = null)
		{
			this.Title = Title;
			this.Description = Description;
			this.Author = Author;
			this.DeviceSupported = DeviceSupported;
			this.Category = Category;
		}

		
		
		
		[DataMember(Name = "title")]
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		
		
		
		[DataMember(Name = "description")]
		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; }

		
		
		
		[DataMember(Name = "author")]
		[JsonProperty(PropertyName = "author")]
		public ChromaSdkInputAuthor Author { get; set; }

		
		
		
		[DataMember(Name = "device_supported")]
		[JsonProperty(PropertyName = "device_supported")]
		public List<string> DeviceSupported { get; set; }

		
		
		
		[JsonProperty(PropertyName = "category")]
		[DataMember(Name = "category")]
		public string Category { get; set; }

		
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("class ChromaSdkInput {\n");
			stringBuilder.Append("  Title: ").Append(this.Title).Append("\n");
			stringBuilder.Append("  Description: ").Append(this.Description).Append("\n");
			stringBuilder.Append("  Author: ").Append(this.Author).Append("\n");
			stringBuilder.Append("  DeviceSupported: ").Append(this.DeviceSupported).Append("\n");
			stringBuilder.Append("  Category: ").Append(this.Category).Append("\n");
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		
		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ChromaSdkInput);
		}

		
		public bool Equals(ChromaSdkInput other)
		{
			return other != null && ((this.Title == other.Title || (this.Title != null && this.Title.Equals(other.Title))) && (this.Description == other.Description || (this.Description != null && this.Description.Equals(other.Description))) && (this.Author == other.Author || (this.Author != null && this.Author.Equals(other.Author))) && (this.DeviceSupported == other.DeviceSupported || (this.DeviceSupported != null && this.DeviceSupported.SequenceEqual(other.DeviceSupported)))) && (this.Category == other.Category || (this.Category != null && this.Category.Equals(other.Category)));
		}

		
		public override int GetHashCode()
		{
			int num = 41;
			if (this.Title != null)
			{
				num = num * 59 + this.Title.GetHashCode();
			}
			if (this.Description != null)
			{
				num = num * 59 + this.Description.GetHashCode();
			}
			if (this.Author != null)
			{
				num = num * 59 + this.Author.GetHashCode();
			}
			if (this.DeviceSupported != null)
			{
				num = num * 59 + this.DeviceSupported.GetHashCode();
			}
			if (this.Category != null)
			{
				num = num * 59 + this.Category.GetHashCode();
			}
			return num;
		}
	}
}
