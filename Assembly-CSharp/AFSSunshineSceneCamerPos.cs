using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class AFSSunshineSceneCamerPos : MonoBehaviour
{
	private void Awake()
	{
		this.afsSceneCameraPosProp = Shader.PropertyToID("_AFSSceneCameraPos");
		this.afsSunshineBillboardLightDirectionProp = Shader.PropertyToID("_AFSSunshineBillboardLightDirection");
		this.sunShineScatterColorProp = Shader.PropertyToID("_SunShineScatterColor");
		this.sunShineDustVolumeScaleProp = Shader.PropertyToID("_SunShineDustVolumeScale");
		this.sunShineScatterIntensityProp = Shader.PropertyToID("_SunShineScatterIntensity");
		this.sunShineSunLightProp = Shader.PropertyToID("_SunShineSunLight");
		this.sunShineSunDirProp = Shader.PropertyToID("_SunShineSunDir");
	}

	private void OnPreCull()
	{
		Shader.SetGlobalVector(this.afsSceneCameraPosProp, base.transform.position);
		if (Sunshine.Instance)
		{
			Shader.SetGlobalVector(this.afsSunshineBillboardLightDirectionProp, Sunshine.Instance.SunLight.transform.forward);
			Shader.SetGlobalVector(this.sunShineScatterColorProp, Sunshine.Instance.ScatterColor.linear);
			float scatterExaggeration = Sunshine.Instance.ScatterExaggeration;
			float value = 1f / (Mathf.Clamp01(scatterExaggeration) * Sunshine.Instance.LightDistance / base.GetComponent<Camera>().farClipPlane);
			Shader.SetGlobalFloat(this.sunShineDustVolumeScaleProp, value);
			Shader.SetGlobalFloat(this.sunShineScatterIntensityProp, Sunshine.Instance.ScatterIntensity);
			this.SunShine_Sun_Col = Sunshine.Instance.SunLight.color.linear * Sunshine.Instance.SunLight.intensity;
			Shader.SetGlobalVector(this.sunShineSunLightProp, this.SunShine_Sun_Col);
			Shader.SetGlobalVector(this.sunShineSunDirProp, Sunshine.Instance.SunLight.transform.forward * -1f);
		}
	}

	public Color SunShine_Sun_Col;

	private int afsSceneCameraPosProp;

	private int afsSunshineBillboardLightDirectionProp;

	private int sunShineScatterColorProp;

	private int sunShineDustVolumeScaleProp;

	private int sunShineScatterIntensityProp;

	private int sunShineSunLightProp;

	private int sunShineSunDirProp;
}
