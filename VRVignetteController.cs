using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.PostProcessing;


public class VRVignetteController : MonoBehaviour
{
	
	private void Awake()
	{
		this._vignetteModel = this.PostProcessingProfile.vignette;
	}

	
	private void Update()
	{
		if (ForestVR.Enabled)
		{
			if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.World)
			{
				this.SetVignetteIntensity(0f);
			}
			else if (LocalPlayer.AnimControl.useRootMotion || LocalPlayer.CamFollowHead.flying || LocalPlayer.CamFollowHead.followAnim)
			{
				this.SetVignetteIntensity(this.AnimationIntensity);
			}
			else
			{
				float magnitude = LocalPlayer.Rigidbody.velocity.magnitude;
				this.SetVignetteIntensity(this.CalculateSpeedIntensity(magnitude, this.SpeedDivisor, this.MaxIntensity));
			}
		}
	}

	
	private float CalculateSpeedIntensity(float input, float divisor, float clamp)
	{
		return Mathf.Clamp(input / divisor, 0f, clamp);
	}

	
	public void SetVignetteIntensity(float intensity)
	{
		this._intensity = Mathf.Lerp(this._intensity, intensity, this.Smoothness);
		VignetteModel.Settings settings = this._vignetteModel.settings;
		settings.intensity = this._intensity;
		this._vignetteModel.settings = settings;
	}

	
	[Header("Input")]
	public float AnimationIntensity = 0.8f;

	
	public float MaxIntensity = 1f;

	
	public float SpeedDivisor = 2.5f;

	
	[Tooltip("Lerp value -- 1 = instant, lesser values = slower")]
	[Range(0.01f, 1f)]
	public float Smoothness = 0.1f;

	
	[Header("Output")]
	public PostProcessingProfile PostProcessingProfile;

	
	public VignetteModel _vignetteModel;

	
	private float _intensity;
}
