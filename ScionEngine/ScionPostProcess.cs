using System;
using UnityEngine;

namespace ScionEngine
{
	
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Scion Post Process")]
	[RequireComponent(typeof(Camera))]
	public class ScionPostProcess : ScionPostProcessBase
	{
		
		protected override bool ShowTonemapping()
		{
			return true;
		}

		
		protected override void SetShaderKeyWords(PostProcessParameters postProcessParams)
		{
			base.SetShaderKeyWords(postProcessParams);
			switch (this.m_tonemappingMode)
			{
			case TonemappingMode.Reinhard:
				ShaderSettings.TonemappingSettings.SetIndex(0);
				break;
			case TonemappingMode.LumaReinhard:
				ShaderSettings.TonemappingSettings.SetIndex(1);
				break;
			case TonemappingMode.Filmic:
				ShaderSettings.TonemappingSettings.SetIndex(2);
				break;
			case TonemappingMode.Photographic:
				ShaderSettings.TonemappingSettings.SetIndex(3);
				break;
			}
		}

		
		protected override void InitializePostProcessParams()
		{
			base.InitializePostProcessParams();
			this.postProcessParams.tonemapping = true;
			this.postProcessParams.commonPostProcess.whitePoint = base.whitePoint;
		}

		
		[ImageEffectTransformsToLDR]
		protected override void OnRenderImage(RenderTexture source, RenderTexture dest)
		{
			base.OnRenderImage(source, dest);
		}
	}
}
