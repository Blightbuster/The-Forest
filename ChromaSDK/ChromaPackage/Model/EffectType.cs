using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ChromaSDK.ChromaPackage.Model
{
	
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EffectType
	{
		
		[EnumMember(Value = "CHROMA_NONE")]
		CHROMA_NONE,
		
		[EnumMember(Value = "CHROMA_WAVE")]
		CHROMA_WAVE,
		
		[EnumMember(Value = "CHROMA_SPECTRUMCYCLING")]
		CHROMA_SPECTRUMCYCLING,
		
		[EnumMember(Value = "CHROMA_BREATHING")]
		CHROMA_BREATHING,
		
		[EnumMember(Value = "CHROMA_BLINKING")]
		CHROMA_BLINKING,
		
		[EnumMember(Value = "CHROMA_REACTIVE")]
		CHROMA_REACTIVE,
		
		[EnumMember(Value = "CHROMA_STATIC")]
		CHROMA_STATIC,
		
		[EnumMember(Value = "CHROMA_CUSTOM")]
		CHROMA_CUSTOM,
		
		[EnumMember(Value = "CHROMA_CUSTOM2")]
		CHROMA_CUSTOM2,
		
		[EnumMember(Value = "CHROMA_RESERVED")]
		CHROMA_RESERVED,
		
		[EnumMember(Value = "CHROMA_INVALID")]
		CHROMA_INVALID
	}
}
