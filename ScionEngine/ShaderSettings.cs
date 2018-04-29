using System;
using UnityEngine;

namespace ScionEngine
{
	
	public static class ShaderSettings
	{
		
		public const string ExposureAutoKW = "SC_EXPOSURE_AUTO";

		
		public const string ExposureManualKW = "SC_EXPOSURE_MANUAL";

		
		private static readonly string[] ExposureKeywords = new string[]
		{
			"SC_EXPOSURE_AUTO",
			"SC_EXPOSURE_MANUAL"
		};

		
		public static ShaderSettings.IndexOption ExposureSettings = new ShaderSettings.IndexOption(ShaderSettings.ExposureKeywords);

		
		public const string DepthFocusManualKW = "SC_DOF_FOCUS_MANUAL";

		
		public const string DepthFocusManualRangeKW = "SC_DOF_FOCUS_RANGE";

		
		public const string DepthFocusCenterKW = "SC_DOF_FOCUS_CENTER";

		
		private static readonly string[] DepthFocusKeywords = new string[]
		{
			"SC_DOF_FOCUS_MANUAL",
			"SC_DOF_FOCUS_RANGE",
			"SC_DOF_FOCUS_CENTER"
		};

		
		public static ShaderSettings.IndexOption DepthFocusSettings = new ShaderSettings.IndexOption(ShaderSettings.DepthFocusKeywords);

		
		public const string DepthOfFieldMaskOnKW = "SC_DOF_MASK_ON";

		
		private static readonly string[] DepthOfFieldMaskKeywords = new string[]
		{
			"SC_DOF_MASK_ON"
		};

		
		public static ShaderSettings.IndexOption DepthOfFieldMaskSettings = new ShaderSettings.IndexOption(ShaderSettings.DepthOfFieldMaskKeywords);

		
		public const string ChromaticAberrationOnKW = "SC_CHROMATIC_ABERRATION_ON";

		
		private static readonly string[] ChromaticAberrationKeywords = new string[]
		{
			"SC_CHROMATIC_ABERRATION_ON"
		};

		
		public static ShaderSettings.IndexOption ChromaticAberrationSettings = new ShaderSettings.IndexOption(ShaderSettings.ChromaticAberrationKeywords);

		
		public const string LensFlareOnKW = "SC_LENS_FLARE_ON";

		
		private static readonly string[] LensFlareKeywords = new string[]
		{
			"SC_LENS_FLARE_ON"
		};

		
		public static ShaderSettings.IndexOption LensFlareSettings = new ShaderSettings.IndexOption(ShaderSettings.LensFlareKeywords);

		
		public const string TonemappingReinhardKW = "SC_TONEMAPPING_REINHARD";

		
		public const string TonemappingLumaReinhardKW = "SC_TONEMAPPING_LUMAREINHARD";

		
		public const string TonemappingFilmicKW = "SC_TONEMAPPING_FILMIC";

		
		public const string TonemappingPhotographicKW = "SC_TONEMAPPING_PHOTOGRAPHIC";

		
		private static readonly string[] TonemappingKeywords = new string[]
		{
			"SC_TONEMAPPING_REINHARD",
			"SC_TONEMAPPING_LUMAREINHARD",
			"SC_TONEMAPPING_FILMIC",
			"SC_TONEMAPPING_PHOTOGRAPHIC"
		};

		
		public static ShaderSettings.IndexOption TonemappingSettings = new ShaderSettings.IndexOption(ShaderSettings.TonemappingKeywords);

		
		public const string ColorGradingOn1TexKW = "SC_COLOR_CORRECTION_1_TEX";

		
		public const string ColorGradingOn2TexKW = "SC_COLOR_CORRECTION_2_TEX";

		
		private static readonly string[] ColorGradingKeywords = new string[]
		{
			"SC_COLOR_CORRECTION_1_TEX",
			"SC_COLOR_CORRECTION_2_TEX"
		};

		
		public static ShaderSettings.IndexOption ColorGradingSettings = new ShaderSettings.IndexOption(ShaderSettings.ColorGradingKeywords);

		
		public class IndexOption
		{
			
			public IndexOption(string[] _keywords)
			{
				this.keywords = _keywords;
				this.curValue = -1;
			}

			
			public void SetIndex(int index)
			{
				if (index != this.curValue)
				{
					this.SetKeyword(index);
					this.curValue = index;
				}
			}

			
			public void Disable()
			{
				for (int i = 0; i < this.keywords.Length; i++)
				{
					Shader.DisableKeyword(this.keywords[i]);
				}
				this.curValue = -1;
			}

			
			public bool IsActive(int index)
			{
				return this.curValue == index;
			}

			
			public bool IsActive(string keyword)
			{
				for (int i = 0; i < this.keywords.Length; i++)
				{
					if (keyword == this.keywords[i])
					{
						return this.IsActive(i);
					}
				}
				return false;
			}

			
			private void SetKeyword(int index)
			{
				for (int i = 0; i < this.keywords.Length; i++)
				{
					if (i == index)
					{
						Shader.EnableKeyword(this.keywords[i]);
					}
					else
					{
						Shader.DisableKeyword(this.keywords[i]);
					}
				}
			}

			
			private void DisableKeyword(int index)
			{
				Shader.DisableKeyword(this.keywords[index]);
			}

			
			private int curValue;

			
			private string[] keywords;
		}
	}
}
