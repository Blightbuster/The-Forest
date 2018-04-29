using System;
using System.Collections;
using ModAPI;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UltimateCheatmenu;
using UnityEngine;

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
			}
			base.StartCoroutine(this.WatchCleanInRain());
		}

		
		private void __Update__Original()
		{
			this.CheckInCave();
			if (this.State == WeatherSystem.States.GrowingClouds || (BoltNetwork.isClient && this.CloudOvercastCurrentValue < this.CloudOvercastTargetValue))
			{
				this.GrowClouds();
			}
			else if (this.State == WeatherSystem.States.ReducingClouds || (BoltNetwork.isClient && this.CloudOvercastCurrentValue > this.CloudOvercastTargetValue))
			{
				this.ReduceClouds();
			}
			else if (!CoopPeerStarter.DedicatedHost)
			{
				this.CheckSnowArea();
			}
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

		
		private void __TryRain__Original()
		{
			if (!this.Raining)
			{
				if (this.RainDice >= 2 && this.RainDice <= 6)
				{
					this.RainStopRolls = 0;
					this.State = WeatherSystem.States.GrowingClouds;
					if (this.RainDice == 2)
					{
						this.CloudOvercastTargetValue = UnityEngine.Random.Range(0.55f, 0.7f);
						this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(1f, 1.05f);
					}
					else if (this.RainDice == 3)
					{
						this.CloudOvercastTargetValue = UnityEngine.Random.Range(0.7f, 0.85f);
						this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(1.05f, 1.12f);
					}
					else if (this.RainDice == 4)
					{
						this.CloudOvercastTargetValue = UnityEngine.Random.Range(0.85f, 1f);
						this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(1.12f, 1.2f);
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
				this.CloudOvercastTargetValue = 0f;
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
				switch (this.RainDice)
				{
				case 2:
					this.TurnOn(WeatherSystem.RainTypes.Light);
					break;
				case 3:
					this.TurnOn(WeatherSystem.RainTypes.Medium);
					break;
				case 4:
					this.TurnOn(WeatherSystem.RainTypes.Heavy);
					break;
				}
			}
			else
			{
				this.State = WeatherSystem.States.Idle;
			}
		}

		
		public void GrowClouds()
		{
			this.CloudOpacityScaleCurrentValue = Mathf.SmoothDamp(this.CloudOpacityScaleCurrentValue, this.CloudOpacityScaleTargetValue * 1.1f, ref this.CloudOpacityScaleVelocity, this.CloudSmoothTime * 3f / 4f);
			if (this.CloudOpacityScaleCurrentValue < this.CloudOpacityScaleTargetValue)
			{
				this.CloudOvercastMat.SetFloat("CloudOpacityScale", this.CloudOpacityScaleCurrentValue);
			}
			this.CloudOvercastCurrentValue = Mathf.SmoothDamp(this.CloudOvercastCurrentValue, this.CloudOvercastTargetValue * 1.1f, ref this.CloudOvercastVelocity, this.CloudSmoothTime);
			if (this.CloudOvercastCurrentValue < this.CloudOvercastTargetValue)
			{
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastCurrentValue);
			}
			else
			{
				this.CloudOpacityScaleVelocity = 0f;
				this.CloudOvercastVelocity = 0f;
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
			this.CloudOvercastCurrentValue = Mathf.SmoothDamp(this.CloudOvercastCurrentValue, (this.CloudOvercastTargetValue != 0f) ? (this.CloudOvercastTargetValue * 0.8f) : -0.2f, ref this.CloudOvercastVelocity, this.CloudSmoothTime);
			if (this.CloudOvercastCurrentValue > this.CloudOvercastTargetValue)
			{
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastCurrentValue);
			}
			else
			{
				this.CloudOpacityScaleVelocity = 0f;
				this.CloudOvercastVelocity = 0f;
				this.CloudOvercastMat.SetFloat("OvercastAmount", this.CloudOvercastTargetValue);
				if (!BoltNetwork.isClient)
				{
					this.State = WeatherSystem.States.Idle;
				}
			}
		}

		
		public void CheckSnowArea()
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
				Scene.RainFollowGO.SetActive(false);
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

		
		
		public float CloudCovergage
		{
			get
			{
				return this.CloudOvercastCurrentValue;
			}
		}

		
		
		
		public bool UsingSnow { get; set; }

		
		
		
		public bool ShouldDoLighting { get; private set; }

		
		private void TryRain()
		{
			try
			{
				if (UCheatmenu.FreezeWeather)
				{
					return;
				}
				this.__TryRain__Original();
			}
			catch (Exception ex)
			{
				Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
				this.__TryRain__Original();
			}
		}

		
		private void Update()
		{
			try
			{
				if (this.ResetCloudTime > 0f)
				{
					this.ResetCloudTime -= Time.deltaTime;
					if (this.ResetCloudTime <= 0f)
					{
						this.CloudSmoothTime = 20f;
					}
				}
				if (UCheatmenu.ForceWeather >= 0)
				{
					this.AllOff();
					Scene.RainFollowGO.SetActive(true);
					Scene.RainTypes.SnowConstant.SetActive(false);
					if (UCheatmenu.ForceWeather == 1)
					{
						this.TurnOn(WeatherSystem.RainTypes.Light);
						this.CloudSmoothTime = 1f;
					}
					if (UCheatmenu.ForceWeather == 2)
					{
						this.TurnOn(WeatherSystem.RainTypes.Medium);
						this.CloudSmoothTime = 1f;
					}
					if (UCheatmenu.ForceWeather == 3)
					{
						this.TurnOn(WeatherSystem.RainTypes.Heavy);
						this.CloudSmoothTime = 1f;
					}
					if (UCheatmenu.ForceWeather == 4)
					{
						this.RainDiceStop = 1;
						this.GrowClouds();
						this.ReduceClouds();
						this.CloudOvercastTargetValue = UnityEngine.Random.Range(0.55f, 1f);
						this.CloudOpacityScaleTargetValue = UnityEngine.Random.Range(1f, 1.2f);
					}
					if (UCheatmenu.ForceWeather == 5)
					{
						this.State = WeatherSystem.States.Raining;
						Scene.RainTypes.SnowLight.SetActive(true);
						this.CloudSmoothTime = 1f;
					}
					if (UCheatmenu.ForceWeather == 6)
					{
						this.State = WeatherSystem.States.Raining;
						Scene.RainTypes.SnowMedium.SetActive(true);
						this.CloudSmoothTime = 1f;
					}
					if (UCheatmenu.ForceWeather == 7)
					{
						this.State = WeatherSystem.States.Raining;
						Scene.RainTypes.SnowHeavy.SetActive(true);
						this.CloudSmoothTime = 1f;
					}
					UCheatmenu.ForceWeather = -1;
				}
				this.__Update__Original();
			}
			catch (Exception ex)
			{
				Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
				this.__Update__Original();
			}
		}

		
		public Material CloudOvercastMat;

		
		public string ThunderEventPath;

		
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

		
		private WeatherSystem.States State;

		
		private WeatherSystem.RainTypes CurrentType;

		
		protected float ResetCloudTime;

		
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
