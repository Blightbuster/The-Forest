using System;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.UI;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;

public class CoopWeatherProxy : EntityBehaviour<IWeatherState>
{
	public static CoopWeatherProxy Instance { get; private set; }

	public static float JoiningTimeOfDay { get; set; }

	public Clock Clock
	{
		get
		{
			if (!this._clock)
			{
				this._clock = UnityEngine.Object.FindObjectOfType<Clock>();
			}
			return this._clock;
		}
	}

	private void Awake()
	{
		base.enabled = false;
	}

	private void ReceivedTimeOfDay()
	{
		if (base.state.TimeOfDay > 0f && base.state.ElapsedGameTime > 0f && LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView > PlayerInventory.PlayerViews.Loading)
		{
			this.Clock.Atmos.TimeOfDay = base.state.TimeOfDay;
			Scene.Clock.ElapsedGameTime = base.state.ElapsedGameTime;
			Scene.Atmosphere.ForceSunRotationUpdate = true;
			base.enabled = true;
			base.state.RemoveCallback("TimeOfDay", new PropertyCallbackSimple(this.ReceivedTimeOfDay));
		}
	}

	public override void Attached()
	{
		CoopWeatherProxy.Instance = this;
		base.state.AddCallback("BreakableWalls[]", new PropertyCallbackSimple(this.BreakableWallsChanged));
		if (!base.entity.isOwner)
		{
			IWeatherState s = base.state;
			s.AddCallback("TimeOfDay", new PropertyCallbackSimple(this.ReceivedTimeOfDay));
			s.AddCallback("Temp", this.IfClock(delegate
			{
				Clock.Temp = s.Temp;
			}));
			s.AddCallback("Day", this.IfClock(delegate
			{
				Clock.Day = s.Day;
			}));
			Scene.Clock.ElapsedGameTime = base.state.ElapsedGameTime;
			s.AddCallback("CloudOvercastTarget", new PropertyCallbackSimple(this.UpdateCloudTargetValues));
			s.AddCallback("CloudOpacityScaleTarget", new PropertyCallbackSimple(this.UpdateCloudTargetValues));
			s.AddCallback("Raining", this.IfClock(delegate
			{
				if (s.Raining)
				{
					Scene.WeatherSystem.Raining = true;
				}
				else
				{
					Scene.WeatherSystem.Raining = false;
					LocalPlayer.Sfx.PlayAfterStorm();
				}
			}));
			s.AddCallback("RainLight", new PropertyCallbackSimple(this.UpdateRainLight));
			s.AddCallback("RainMedium", new PropertyCallbackSimple(this.UpdateRainMedium));
			s.AddCallback("RainHeavy", new PropertyCallbackSimple(this.UpdateRainHeavy));
			s.AddCallback("DayTimeSfx", this.IfClock(delegate
			{
				this.Clock.DayTimeSfx.SetActive(s.DayTimeSfx);
			}));
			s.AddCallback("NightTimeSfx", this.IfClock(delegate
			{
				this.Clock.NightTimeSfx.SetActive(s.NightTimeSfx);
			}));
			s.AddCallback("Lightning", this.IfClock(delegate
			{
				if (!Scene.WeatherSystem.UsingSnow || !s.Lightning)
				{
					if (!FMOD_StudioSystem.ForceFmodOff && FMOD_StudioSystem.instance && s.Lightning)
					{
						FMOD_StudioSystem.instance.PlayOneShot(Scene.WeatherSystem.ThunderEventPath, this.Clock.transform.position, null);
					}
					this.Clock.LightningFlashGroup.SetActive(s.Lightning);
				}
			}));
			s.AddCallback("Rainbow", this.IfClock(delegate
			{
				this.Clock.RainbowA.SetActive(s.Rainbow);
			}));
			s.AddCallback("RainbowIntensity", this.IfClock(delegate
			{
				this.Clock.RainbowA.GetComponent<Renderer>().material.SetColor("_TintColor", s.RainbowIntensity);
			}));
			base.state.AddCallback("ExplodeCaches[]", new PropertyCallbackSimple(this.ExplodeCachesChanged));
			this.LoadClientExplodeCaches();
		}
	}

	private void BreakableWallsChanged()
	{
		if (CoopWoodPlanks.Instance)
		{
			for (int i = 0; i < CoopWoodPlanks.Instance.Planks.Length; i++)
			{
				if (base.state.BreakableWalls[i] == 1 && CoopWoodPlanks.Instance.Planks[i])
				{
					CoopWoodPlanks.Instance.Planks[i].CutDown();
				}
			}
		}
	}

	private void LoadClientExplodeCaches()
	{
		BreakStoneSimple[] array = UnityEngine.Object.FindObjectsOfType<BreakStoneSimple>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ClientExplodeCheck(true);
		}
	}

	private void ExplodeCachesChanged()
	{
		BreakStoneSimple[] array = UnityEngine.Object.FindObjectsOfType<BreakStoneSimple>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ClientExplodeCheck(false);
		}
	}

	private void Update()
	{
		if (this.Clock && BoltNetwork.isClient && HostNotRespondingAlert.Responding)
		{
			bool flag = base.state.TimeOfDay < 0f;
			if (flag)
			{
				this.Clock.Atmos.TimeOfDay = 302f;
				this.lastWasNegative = true;
			}
			else if (this.lastWasNegative || Scene.Atmosphere.ForceSunRotationUpdate)
			{
				this.Clock.Atmos.TimeOfDay = base.state.TimeOfDay;
				Scene.Atmosphere.ForceSunRotationUpdate = true;
				this.lastWasNegative = false;
			}
			else
			{
				this.Clock.Atmos.TimeOfDay = Mathf.LerpAngle(this.Clock.Atmos.TimeOfDay, base.state.TimeOfDay, Time.deltaTime * 10f) % 360f;
				this.lastWasNegative = false;
			}
			Scene.Atmosphere.DeltaTimeOfDay = (double)(base.state.ElapsedGameTime - Scene.Clock.ElapsedGameTime);
			Scene.Clock.ElapsedGameTime = base.state.ElapsedGameTime;
		}
	}

	private void UpdateCloudTargetValues()
	{
		if (Scene.WeatherSystem)
		{
			Scene.WeatherSystem.CloudOvercastTarget = base.state.CloudOvercastTarget;
			Scene.WeatherSystem.CloudOpacityScaleTarget = base.state.CloudOpacityScaleTarget;
		}
	}

	private void UpdateRainLight()
	{
		if (Scene.RainTypes)
		{
			if (base.state.RainLight)
			{
				Scene.WeatherSystem.TurnOn(WeatherSystem.RainTypes.Light);
			}
			else
			{
				LocalPlayer.MudGreeble.gameObject.SetActive(true);
				Scene.WeatherSystem.AllOff();
			}
		}
	}

	private void UpdateRainMedium()
	{
		if (Scene.RainTypes)
		{
			if (base.state.RainMedium)
			{
				Scene.WeatherSystem.TurnOn(WeatherSystem.RainTypes.Medium);
			}
			else
			{
				LocalPlayer.MudGreeble.gameObject.SetActive(true);
				Scene.WeatherSystem.AllOff();
			}
		}
	}

	private void UpdateRainHeavy()
	{
		if (Scene.RainTypes)
		{
			if (base.state.RainHeavy)
			{
				Scene.WeatherSystem.TurnOn(WeatherSystem.RainTypes.Heavy);
			}
			else
			{
				LocalPlayer.MudGreeble.gameObject.SetActive(true);
				Scene.WeatherSystem.AllOff();
			}
		}
	}

	private PropertyCallbackSimple IfClock(Action action)
	{
		return delegate
		{
			if (this.Clock)
			{
				action();
			}
		};
	}

	public override void Detached()
	{
		if (CoopWeatherProxy.Instance == this)
		{
			CoopWeatherProxy.Instance = null;
		}
	}

	private void OnDestroy()
	{
		if (CoopWeatherProxy.Instance == this)
		{
			CoopWeatherProxy.Instance = null;
		}
	}

	private Clock _clock;

	private bool lastWasNegative;
}
