using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using VolumetricClouds3;

namespace TheForest.World
{
	[DoNotSerializePublic]
	public class WeatherSystem : MonoBehaviour
	{
		private void Awake()
		{
			this.ResetClouds();
			if (!BoltNetwork.isClient)
			{
				base.InvokeRepeating("RainChance", 150f, 60f);
				base.InvokeRepeating("RandomClouds", 30f, 30f);
			}
			this.Weather = base.transform.parent.GetComponentInChildren<WeatherPresets>();
			if (LocalPlayer.MainCamTr)
			{
				this.vClouds = LocalPlayer.MainCamTr.GetComponent<RaymarchedClouds>();
			}
			base.StartCoroutine(this.WatchCleanInRain());
		}

		private void Start()
		{
			this.startSunColorMultiplier = this.CloudOvercastMat.GetFloat("SunColorMultiplier");
			if (LocalPlayer.MainCamTr)
			{
				this.vClouds = LocalPlayer.MainCamTr.GetComponent<RaymarchedClouds>();
			}
		}

		private void Update()
		{
			if (LocalPlayer.AnimControl && !LocalPlayer.AnimControl.endGameCutScene)
			{
				this.CheckInCave();
			}
			if (BoltNetwork.isClient)
			{
				this.UpdateClientClouds();
				if (LocalPlayer.AnimControl && !LocalPlayer.AnimControl.endGameCutScene)
				{
					this.UpdateRainForSnowArea();
				}
				return;
			}
			if (this.State == WeatherSystem.States.GrowingClouds || (BoltNetwork.isClient && this.VCloudCoverageCurrentValue < this.VCloudCoverageTargetValue))
			{
				this.GrowClouds();
			}
			else if (this.State == WeatherSystem.States.ReducingClouds || (BoltNetwork.isClient && this.VCloudCoverageCurrentValue > this.VCloudCoverageTargetValue))
			{
				this.ReduceClouds();
			}
			else if (!CoopPeerStarter.DedicatedHost && LocalPlayer.AnimControl && !LocalPlayer.AnimControl.endGameCutScene)
			{
				this.UpdateRainForSnowArea();
				if (this.State == WeatherSystem.States.Idle && !this.UsingSnow)
				{
					this.UpdateRandomClouds();
				}
			}
			this.CloudOvercastMat.SetFloat("SkyColorMultiplier", 0.01f);
		}

		public void ForceRain(int rainDice)
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			this.RainDice = rainDice;
			this.RainDiceStop = ((rainDice >= 6) ? 2 : 1);
			this.TryRain();
		}

		private void RandomClouds()
		{
			if (this.State == WeatherSystem.States.Idle)
			{
				this.CloudAlphaSaturationTargetValue = UnityEngine.Random.Range(2.2f, 8f);
				this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(0.2f, 1.3f);
				this.VCloudCoverageTargetValue = UnityEngine.Random.Range(-0.5f, 0.2f);
			}
		}

		private void UpdateClientClouds()
		{
			if (this.vClouds == null && LocalPlayer.MainCamTr)
			{
				this.vClouds = LocalPlayer.MainCamTr.GetComponent<RaymarchedClouds>();
			}
			if (this.vClouds && this.vClouds.enabled && !Mathf.Approximately(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue))
			{
				this.VCloudCoverageCurrentValue = this.vClouds.materialUsed.GetFloat("_Coverage");
				this.VCloudCoverageCurrentValue = Mathf.SmoothDamp(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue, ref this.VCloudCoverageVelocity, this.CloudSmoothTime * 0.65f);
				this.vClouds.materialUsed.SetFloat("_Coverage", this.VCloudCoverageCurrentValue);
			}
		}

		private void UpdateRandomClouds()
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			if (this.vClouds == null && LocalPlayer.MainCamTr)
			{
				this.vClouds = LocalPlayer.MainCamTr.GetComponent<RaymarchedClouds>();
			}
			if (this.vClouds && this.vClouds.enabled && !Mathf.Approximately(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue))
			{
				this.VCloudCoverageCurrentValue = this.vClouds.materialUsed.GetFloat("_Coverage");
				this.VCloudCoverageCurrentValue = Mathf.SmoothDamp(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue, ref this.VCloudCoverageVelocity, this.CloudSmoothTime * 0.65f);
				this.vClouds.materialUsed.SetFloat("_Coverage", this.VCloudCoverageCurrentValue);
			}
			if (!Mathf.Approximately(this.CloudOpacityScaleCurrentValue, this.CloudOpacityScaleTargetValue))
			{
				this.CloudOpacityScaleCurrentValue = this.CloudOvercastMat.GetFloat("CloudOpacityScale");
				this.CloudOpacityScaleCurrentValue = Mathf.SmoothDamp(this.CloudOpacityScaleCurrentValue, this.CloudOpacityScaleTargetValue * 1.1f, ref this.CloudOpacityScaleVelocity, this.CloudSmoothTime * 0.65f);
				this.CloudOvercastMat.SetFloat("CloudOpacityScale", this.CloudOpacityScaleCurrentValue);
			}
			if (!Mathf.Approximately(this.CloudAlphaSaturationCurrentValue, this.CloudAlphaSaturationTargetValue))
			{
				this.CloudAlphaSaturationCurrentValue = this.CloudOvercastMat.GetFloat("AlphaSaturation");
				this.CloudAlphaSaturationCurrentValue = Mathf.SmoothDamp(this.CloudAlphaSaturationCurrentValue, this.CloudAlphaSaturationTargetValue * 1.1f, ref this.CloudAlphaSaturationVelocity, this.CloudSmoothTime * 0.65f);
				this.CloudOvercastMat.SetFloat("AlphaSaturation", this.CloudAlphaSaturationCurrentValue);
			}
		}

		private void RainChance()
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			if (this.State != WeatherSystem.States.GrowingClouds)
			{
				if (RainEffigy.RainAdd == 0)
				{
					this.RainDice = UnityEngine.Random.Range(1, this.RainRollBase * 4);
				}
				else if (RainEffigy.RainAdd == 1)
				{
					this.RainDice = UnityEngine.Random.Range(1, this.RainRollBase * 3);
				}
				else if (RainEffigy.RainAdd == 2)
				{
					this.RainDice = UnityEngine.Random.Range(1, this.RainRollBase * 2);
				}
				else if (RainEffigy.RainAdd == 3)
				{
					this.RainDice = UnityEngine.Random.Range(1, this.RainRollBase);
				}
				else
				{
					this.RainDice = UnityEngine.Random.Range(1, 16);
				}
				this.RainDiceStop = UnityEngine.Random.Range(1, 9);
				base.Invoke("TryRain", 5f);
			}
		}

		private void TryRain()
		{
			if (!this.Raining)
			{
				if (this.RainDice >= 2 && this.RainDice <= 6)
				{
					this.RainStopRolls = 0;
					this.State = WeatherSystem.States.GrowingClouds;
					if (this.RainDice == 2)
					{
						this.setRandomWeather(0);
						this.VCloudCoverageTargetValue = 0.42f;
					}
					else if (this.RainDice == 3)
					{
						this.setRandomWeather(1);
						this.VCloudCoverageTargetValue = 0.52f;
					}
					else if (this.RainDice == 4)
					{
						this.setRandomWeather(2);
						this.VCloudCoverageTargetValue = 0.6f;
					}
					else
					{
						this.CloudOvercastTargetValue = UnityEngine.Random.Range(0.55f, 1f);
						this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(1f, 1.2f);
					}
				}
			}
			else if ((this.RainDiceStop == 2 || this.RainStopRolls >= this.MaxRainDuration) && this.RainStopRolls >= this.MinRainDuration)
			{
				this.RainStopRolls = 0;
				if (!this.UsingSnow && LocalPlayer.MudGreeble)
				{
					LocalPlayer.MudGreeble.gameObject.SetActive(true);
				}
				this.RainDiceStop = 1;
				this.State = WeatherSystem.States.ReducingClouds;
				if (LocalPlayer.Sfx)
				{
					LocalPlayer.Sfx.PlayAfterStorm();
				}
				this.CloudOvercastTargetValue = 0.1f;
				this.CloudAlphaSaturationTargetValue = 2.2f;
				this.CloudSkyColorMultiplyerTargetValue = 0f;
				this.VCloudCoverageTargetValue = -0.2f;
				this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(0.8f, 1f);
				this.AllOff();
				if (!Clock.Dark)
				{
					Scene.Clock.RainbowA.SetActive(true);
					base.Invoke("RainBowOff", 60f);
				}
				this.LastRainTime = Scene.Clock.ElapsedGameTime;
			}
			else
			{
				this.RainStopRolls++;
			}
		}

		private void DoRain(bool doLighting)
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			this.RainStopRolls = 0;
			if (this.RainDice >= 2 && this.RainDice <= 4)
			{
				this.LastRainTime = Scene.Clock.ElapsedGameTime;
				if (doLighting)
				{
					this.Lightning();
				}
				int rainDice = this.RainDice;
				if (rainDice != 2)
				{
					if (rainDice != 3)
					{
						if (rainDice == 4)
						{
							this.TurnOn(WeatherSystem.RainTypes.Heavy);
						}
					}
					else
					{
						this.TurnOn(WeatherSystem.RainTypes.Medium);
					}
				}
				else
				{
					this.TurnOn(WeatherSystem.RainTypes.Light);
				}
			}
			else
			{
				this.State = WeatherSystem.States.Idle;
			}
		}

		public void GrowClouds()
		{
			this.CloudOpacityScaleCurrentValue = this.CloudOvercastMat.GetFloat("CloudOpacityScale");
			this.CloudOpacityScaleCurrentValue = Mathf.SmoothDamp(this.CloudOpacityScaleCurrentValue, this.CloudOpacityScaleTargetValue * 1.1f, ref this.CloudOpacityScaleVelocity, this.CloudSmoothTime * 3f / 4f);
			if (!Mathf.Approximately(this.CloudOpacityScaleCurrentValue, this.CloudOpacityScaleTargetValue))
			{
				this.CloudOvercastMat.SetFloat("CloudOpacityScale", this.CloudOpacityScaleCurrentValue);
			}
			this.CloudAlphaSaturationCurrentValue = this.CloudOvercastMat.GetFloat("AlphaSaturation");
			this.CloudAlphaSaturationCurrentValue = Mathf.SmoothDamp(this.CloudAlphaSaturationCurrentValue, this.CloudAlphaSaturationTargetValue * 1.1f, ref this.CloudAlphaSaturationVelocity, this.CloudSmoothTime);
			if (!Mathf.Approximately(this.CloudAlphaSaturationCurrentValue, this.CloudAlphaSaturationTargetValue))
			{
				this.CloudOvercastMat.SetFloat("AlphaSaturation", this.CloudAlphaSaturationCurrentValue);
			}
			if (this.vClouds && this.vClouds.enabled && !Mathf.Approximately(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue))
			{
				this.VCloudCoverageCurrentValue = this.vClouds.materialUsed.GetFloat("_Coverage");
				this.VCloudCoverageCurrentValue = Mathf.SmoothDamp(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue, ref this.VCloudCoverageVelocity, this.CloudSmoothTime * 3f / 4f);
				this.vClouds.materialUsed.SetFloat("_Coverage", this.VCloudCoverageCurrentValue);
			}
			this.CloudOvercastCurrentValue = this.CloudOvercastMat.GetFloat("OvercastAmount");
			this.CloudOvercastCurrentValue = Mathf.SmoothDamp(this.CloudOvercastCurrentValue, this.CloudOvercastTargetValue * 1.1f, ref this.CloudOvercastVelocity, this.CloudSmoothTime);
			if (this.CloudOvercastCurrentValue < this.CloudOvercastTargetValue)
			{
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastCurrentValue);
			}
			else
			{
				this.CloudOpacityScaleVelocity = 0f;
				this.CloudOvercastVelocity = 0f;
				this.CloudAlphaSaturationVelocity = 0f;
				this.CloudSkyColorMultiplyerVelocity = 0f;
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastTargetValue);
				this.DoRain(true);
			}
		}

		public void ReduceClouds()
		{
			this.CloudOpacityScaleCurrentValue = Mathf.SmoothDamp(this.CloudOpacityScaleCurrentValue, this.CloudOpacityScaleTargetValue * 0.9f, ref this.CloudOpacityScaleVelocity, this.CloudSmoothTime * 3f / 4f);
			if (this.CloudOpacityScaleCurrentValue > this.CloudOpacityScaleTargetValue)
			{
				this.CloudOvercastMat.SetFloat("CloudOpacityScale", this.CloudOpacityScaleCurrentValue);
			}
			this.CloudAlphaSaturationCurrentValue = Mathf.SmoothDamp(this.CloudAlphaSaturationCurrentValue, this.CloudAlphaSaturationTargetValue * 0.9f, ref this.CloudAlphaSaturationVelocity, this.CloudSmoothTime);
			if (!Mathf.Approximately(this.CloudAlphaSaturationCurrentValue, this.CloudAlphaSaturationTargetValue))
			{
				this.CloudOvercastMat.SetFloat("AlphaSaturation", this.CloudAlphaSaturationCurrentValue);
			}
			if (this.vClouds && this.vClouds.enabled && !Mathf.Approximately(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue))
			{
				this.VCloudCoverageCurrentValue = this.vClouds.materialUsed.GetFloat("_Coverage");
				this.VCloudCoverageCurrentValue = Mathf.SmoothDamp(this.VCloudCoverageCurrentValue, this.VCloudCoverageTargetValue, ref this.VCloudCoverageVelocity, this.CloudSmoothTime * 3f / 4f);
				this.vClouds.materialUsed.SetFloat("_Coverage", this.VCloudCoverageCurrentValue);
			}
			this.CloudOvercastCurrentValue = Mathf.SmoothDamp(this.CloudOvercastCurrentValue, (this.CloudOvercastTargetValue != 0f) ? (this.CloudOvercastTargetValue * 0.8f) : -0.2f, ref this.CloudOvercastVelocity, this.CloudSmoothTime);
			if (this.CloudOvercastCurrentValue > this.CloudOvercastTargetValue)
			{
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastCurrentValue);
			}
			else
			{
				this.CloudOpacityScaleVelocity = 0f;
				this.CloudOvercastVelocity = 0f;
				this.CloudAlphaSaturationVelocity = 0f;
				this.CloudSkyColorMultiplyerVelocity = 0f;
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastTargetValue);
				if (!BoltNetwork.isClient)
				{
					this.State = WeatherSystem.States.Idle;
				}
			}
		}

		public void UpdateRainForSnowArea()
		{
			if (this.State == WeatherSystem.States.Raining)
			{
				if (this.UsingSnow != (LocalPlayer.Stats.IsInNorthColdArea() && LocalPlayer.IsInOutsideWorld))
				{
					this.AllOff();
					this.TurnOn(this.CurrentType);
				}
			}
			else if (LocalPlayer.Stats && this.UsingSnow != (LocalPlayer.Stats.IsInNorthColdArea() && LocalPlayer.IsInOutsideWorld))
			{
				this.AllOff();
			}
		}

		public void CheckInCave()
		{
			if (LocalPlayer.IsInCaves || (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.PlaneCrash))
			{
				if (Scene.RainTypes.CaveFilter.activeSelf)
				{
					Scene.RainTypes.CaveFilter.SetActive(false);
				}
			}
			else if (!Scene.RainTypes.CaveFilter.activeSelf)
			{
				Scene.RainTypes.CaveFilter.SetActive(true);
			}
		}

		private void Lightning()
		{
			this.ShouldDoLighting = true;
			base.Invoke("LightningOff", 0.5f);
			if (BoltNetwork.isClient || this.UsingSnow || (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView < PlayerInventory.PlayerViews.PlaneCrash))
			{
				return;
			}
			if (LocalPlayer.IsInOutsideWorld)
			{
				if (FMOD_StudioSystem.instance)
				{
					FMOD_StudioSystem.instance.PlayOneShot(this.ThunderEventPath, base.transform.position, null);
				}
				Scene.Clock.LightningFlashGroup.SetActive(true);
			}
		}

		public void AllOff()
		{
			this.CurrentType = WeatherSystem.RainTypes.None;
			Scene.RainTypes.RainLight.SetActive(false);
			Scene.RainTypes.RainMedium.SetActive(false);
			Scene.RainTypes.RainHeavy.SetActive(false);
			Scene.RainTypes.SnowLight.SetActive(false);
			Scene.RainTypes.SnowMedium.SetActive(false);
			Scene.RainTypes.SnowHeavy.SetActive(false);
			this.UsingSnow = (LocalPlayer.Stats && LocalPlayer.Stats.IsInNorthColdArea() && LocalPlayer.IsInOutsideWorld);
			if (this.UsingSnow)
			{
				Scene.RainFollowGO.SetActive(true);
				Scene.RainTypes.SnowConstant.SetActive(true);
			}
			else
			{
				Scene.RainTypes.SnowConstant.SetActive(false);
			}
		}

		public void TurnOn(WeatherSystem.RainTypes type)
		{
			this.CurrentType = type;
			this.State = WeatherSystem.States.Raining;
			this.UsingSnow = (LocalPlayer.Stats && LocalPlayer.Stats.IsInNorthColdArea() && LocalPlayer.IsInOutsideWorld);
			Scene.RainFollowGO.SetActive(true);
			Scene.RainTypes.SnowConstant.SetActive(false);
			switch (type)
			{
			case WeatherSystem.RainTypes.Light:
				if (this.UsingSnow)
				{
					Scene.RainTypes.SnowLight.SetActive(true);
				}
				else
				{
					Scene.RainTypes.RainLight.SetActive(true);
				}
				break;
			case WeatherSystem.RainTypes.Medium:
				if (this.UsingSnow)
				{
					Scene.RainTypes.SnowMedium.SetActive(true);
				}
				else
				{
					Scene.RainTypes.RainMedium.SetActive(true);
				}
				break;
			case WeatherSystem.RainTypes.Heavy:
				if (this.UsingSnow)
				{
					Scene.RainTypes.SnowHeavy.SetActive(true);
				}
				else
				{
					Scene.RainTypes.RainHeavy.SetActive(true);
				}
				break;
			default:
				this.AllOff();
				break;
			}
		}

		private void LightningOff()
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			this.ShouldDoLighting = false;
			Scene.Clock.LightningFlashGroup.SetActive(false);
		}

		private void RainBowOff()
		{
			if (BoltNetwork.isClient)
			{
				return;
			}
			Scene.Clock.TurnRainBowOff = true;
		}

		private void ResetClouds()
		{
			if (this.CloudOvercastMat)
			{
				this.CloudOvercastMat.SetFloat("CloudOpacityScale", 1f);
				this.CloudOvercastMat.SetFloat("OvercastAmount", 0f);
				this.CloudOvercastMat.SetFloat("SunColorMultiplier", 0.99f);
				this.CloudOvercastMat.SetFloat("SkyColorMultiplier", 0.01f);
			}
			if (this.vClouds)
			{
				this.vClouds.materialUsed.SetFloat("_Coverage", 0f);
			}
		}

		private IEnumerator WatchCleanInRain()
		{
			WaitForSeconds wait = YieldPresets.WaitFourSeconds;
			for (;;)
			{
				yield return wait;
				if (this.State == WeatherSystem.States.Raining && !this.UsingSnow && !this.UsingSnow && !LocalPlayer.IsInCaves)
				{
					LocalPlayer.GameObject.SendMessage("GotClean", SendMessageOptions.DontRequireReceiver);
					LocalPlayer.GameObject.SendMessage("GotCleanReal", SendMessageOptions.DontRequireReceiver);
				}
			}
			yield break;
		}

		private void setRandomWeather(int val)
		{
			if (val > this.Weather.WeatherConfigPresets.Length)
			{
				Debug.LogError("cloud preset value out of range!");
				return;
			}
			this.CloudOvercastTargetValue = this.Weather.WeatherConfigPresets[val].OvercastAmount;
			this.CloudOpacityScaleTargetValue = this.Weather.WeatherConfigPresets[val].CloudOpacityScale;
			this.CloudSkyColorMultiplyerTargetValue = this.Weather.WeatherConfigPresets[val].SkyColorMultiplyer;
			this.CloudAlphaSaturationTargetValue = this.Weather.WeatherConfigPresets[val].AlphaSaturation;
		}

		public bool Raining
		{
			get
			{
				return this.State == WeatherSystem.States.Raining;
			}
			set
			{
				this.State = ((!value) ? WeatherSystem.States.Idle : WeatherSystem.States.Raining);
			}
		}

		public float CloudOvercastCurrent
		{
			get
			{
				return this.CloudOvercastCurrentValue;
			}
		}

		public float CloudOvercastTarget
		{
			get
			{
				return this.CloudOvercastTargetValue;
			}
			set
			{
				this.CloudOvercastTargetValue = value;
			}
		}

		public float CloudOpacityScaleCurrent
		{
			get
			{
				return this.CloudOpacityScaleCurrentValue;
			}
			set
			{
				this.CloudOpacityScaleCurrentValue = value;
			}
		}

		public float CloudOpacityScaleTarget
		{
			get
			{
				return this.CloudOpacityScaleTargetValue;
			}
			set
			{
				this.CloudOpacityScaleTargetValue = value;
			}
		}

		public float CloudAlphaSaturationTarget
		{
			get
			{
				return this.CloudAlphaSaturationTargetValue;
			}
			set
			{
				this.CloudAlphaSaturationTargetValue = value;
			}
		}

		public float CloudSkyColorMultiplyerTarget
		{
			get
			{
				return this.CloudSkyColorMultiplyerTargetValue;
			}
			set
			{
				this.CloudSkyColorMultiplyerTargetValue = value;
			}
		}

		public float CloudCovergage
		{
			get
			{
				return this.CloudOvercastCurrentValue;
			}
		}

		public bool UsingSnow { get; set; }

		public bool ShouldDoLighting { get; private set; }

		public Material CloudOvercastMat;

		public string ThunderEventPath;

		public RaymarchedClouds vClouds;

		[SerializeThis]
		public float LastRainTime;

		public float CloudSmoothTime = 20f;

		public int RainRollBase = 30;

		public int MinRainDuration = 1;

		public int MaxRainDuration = 9;

		private int RainDice;

		private int RainDiceStop;

		private int RainStopRolls;

		private float CloudOvercastCurrentValue;

		private float CloudOvercastTargetValue;

		private float CloudOvercastVelocity;

		private float CloudOpacityScaleCurrentValue = 1f;

		private float CloudOpacityScaleTargetValue = 1f;

		private float CloudOpacityScaleVelocity;

		private float CloudAlphaSaturationCurrentValue;

		private float CloudAlphaSaturationTargetValue = 1f;

		private float CloudAlphaSaturationVelocity;

		private float CloudSkyColorMultiplyerCurrentValue;

		private float CloudSkyColorMultiplyerTargetValue = 1f;

		private float CloudSkyColorMultiplyerVelocity;

		public float VCloudCoverageCurrentValue;

		public float VCloudCoverageTargetValue;

		private float VCloudCoverageVelocity;

		public float startSunColorMultiplier;

		private WeatherSystem.States State;

		private WeatherSystem.RainTypes CurrentType;

		private WeatherPresets Weather;

		public enum States
		{
			Idle,
			GrowingClouds,
			ReducingClouds,
			Raining
		}

		public enum RainTypes
		{
			None,
			Light,
			Medium,
			Heavy
		}
	}
}
