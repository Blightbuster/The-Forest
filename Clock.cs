using System;
using System.Collections;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


[DoNotSerializePublic]
public class Clock : MonoBehaviour
{
	
	private void Awake()
	{
		if (BoltNetwork.isClient)
		{
			this.Atmos.TimeOfDay = 302f;
		}
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			return;
		}
		this.isValidNextSleepTime = (!BoltNetwork.isRunning || BoltNetwork.isServer);
		Clock.Temp = 30;
		Clock.Dark = false;
		Clock.InCave = false;
		Clock.Temp = UnityEngine.Random.Range(-10, 1);
		Clock.Dark = false;
	}

	
	private IEnumerator Start()
	{
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			yield break;
		}
		this.UpdatePlaneMoss();
		if (BoltNetwork.isClient)
		{
			yield return null;
			if (LocalPlayer.ActiveAreaInfo.HasActiveEndgameArea)
			{
				LocalPlayer.GameObject.SendMessage("InACave");
			}
			else if (!LocalPlayer.IsInCaves)
			{
				float y = LocalPlayer.Transform.position.y;
				float num = Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position);
				if (y < num)
				{
					float num2 = Vector3.Distance(new Vector3(Scene.SinkHoleCenter.position.x, LocalPlayer.Transform.position.y, Scene.SinkHoleCenter.position.z), LocalPlayer.Transform.position);
					if (num2 > 234f)
					{
						LocalPlayer.GameObject.SendMessage("InACave");
					}
				}
			}
		}
		yield break;
	}

	
	private IEnumerator OnDeserialized()
	{
		if (!CoopPeerStarter.DedicatedHost)
		{
			if (Clock.InCave)
			{
				LocalPlayer.ActiveAreaInfo.SetInCaves(true);
				Clock.InCave = false;
			}
			yield return null;
			if (LocalPlayer.IsInCaves)
			{
				if (LocalPlayer.Transform.position.y < Terrain.activeTerrain.SampleHeight(LocalPlayer.Transform.position) || LocalPlayer.ActiveAreaInfo.CurrentCave == CaveNames.SnowCave)
				{
					LocalPlayer.GameObject.SendMessage("InACave");
				}
				else
				{
					LocalPlayer.ActiveAreaInfo.SetInCaves(false);
					LocalPlayer.Stats.DisableCaveAudio();
				}
			}
			if (this.ElapsedGameTime < LocalPlayer.Stats.DaySurvived)
			{
				this.ElapsedGameTime = LocalPlayer.Stats.DaySurvived;
			}
			this.UpdatePlaneMoss();
		}
		yield break;
	}

	
	private void debugDay()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
	}

	
	public void RequestValidSleeptime()
	{
		if (BoltNetwork.isClient)
		{
			ValidSleepTime validSleepTime = ValidSleepTime.Create(GlobalTargets.OnlyServer);
			validSleepTime.Send();
			this.isValidNextSleepTime = true;
		}
	}

	
	private void Update()
	{
		if (!this.isValidNextSleepTime && BoltNetwork.isRunning && Scene.FinishGameLoad)
		{
			this.RequestValidSleeptime();
		}
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			return;
		}
		if (this.Atmos.TimeOfDay > 88f && this.Atmos.TimeOfDay < 270f)
		{
			this.GoDark();
		}
		else
		{
			this.GoLight();
		}
		if (this.Atmos.Sleeping && !this.Atmos.SleepUntilMorning && Scene.Clock.ElapsedGameTime - Scene.Clock.NextSleepTime > 0.3f)
		{
			this.ClockWakeUp();
		}
		if (this.Atmos.TimeOfDay > 266f && this.Atmos.TimeOfDay < 270f && !this.DaySwitch)
		{
			this.DaySwitch = true;
			this.ChangeDay();
		}
		if (this.Atmos.TimeOfDay > 270f)
		{
			this.DaySwitch = false;
		}
		if (BoltNetwork.isClient)
		{
			return;
		}
		this.ElapsedGameTime += Scene.Atmosphere.DeltaTimeOfDay;
		if (this.RainbowA.activeSelf && Clock.Dark)
		{
			this.RainbowA.SetActive(false);
		}
		if (this.RainbowA.activeSelf && this.TurnRainBowOff)
		{
			this.RainbowIntensity.a = this.RainbowIntensity.a - 0.1f * Time.deltaTime;
			this.RainbowA.GetComponent<Renderer>().material.SetColor("_TintColor", this.RainbowIntensity);
			if (this.RainbowIntensity.a <= 0f)
			{
				this.RainbowA.SetActive(false);
				this.TurnRainBowOff = false;
			}
		}
		if (this.RainbowA.activeSelf && !this.TurnRainBowOff && this.RainbowIntensity.a < this.Sun.intensity)
		{
			this.RainbowIntensity.a = this.RainbowIntensity.a + 0.1f * Time.deltaTime;
			this.RainbowA.GetComponent<Renderer>().material.SetColor("_TintColor", this.RainbowIntensity);
		}
		this.CopyPropertiesToNetwork();
	}

	
	private void resetUpdateMutantCheck()
	{
		this.updateMutantsCheck = false;
	}

	
	private void ClockWakeUp()
	{
		Debug.Log("Clock wakeup");
		this.Atmos.SleepUntilMorning = false;
		if (SteamDSConfig.isDedicatedServer)
		{
			if (this.Atmos.Sleeping)
			{
				this.Atmos.NoTimeLapse();
				float num = Scene.Clock.ElapsedGameTime - Scene.Clock.NextSleepTime;
				Scene.Clock.NextSleepTime = Scene.Clock.ElapsedGameTime + 0.95f - num;
			}
		}
		else
		{
			LocalPlayer.GameObject.SendMessage("Wake");
		}
	}

	
	private void GoDark()
	{
		if (!Clock.Dark)
		{
			Clock.Dark = true;
			Clock.Temp = UnityEngine.Random.Range(-20, 1);
			Scene.SceneTracker.Invoke("WentDark", 4f);
			this.NightTimeSfx.SetActive(true);
			this.DayTimeSfx.SetActive(false);
		}
	}

	
	private void GoLight()
	{
		if (Clock.Dark)
		{
			Clock.Dark = false;
			if (this.Atmos.SleepUntilMorning)
			{
				base.Invoke("ClockWakeUp", 0.15f);
			}
			Clock.Temp = UnityEngine.Random.Range(5, 30);
			Scene.SceneTracker.Invoke("WentLight", 4f);
			this.NightTimeSfx.SetActive(false);
			this.DayTimeSfx.SetActive(true);
		}
	}

	
	public void IsCave()
	{
		if (LocalPlayer.ActiveAreaInfo)
		{
			LocalPlayer.ActiveAreaInfo.SetInCaves(true);
		}
		if (BoltNetwork.isClient)
		{
			return;
		}
		if (Clock.Dark)
		{
			Clock.Dark = false;
		}
		else
		{
			Clock.Dark = true;
		}
	}

	
	public void IsNotCave()
	{
		if (LocalPlayer.ActiveAreaInfo)
		{
			LocalPlayer.ActiveAreaInfo.SetInCaves(false);
		}
	}

	
	private void UpdatePlaneMoss()
	{
		Prefabs.Instance.PlaneMossMaterial.SetFloat("_MossSpread", Mathf.Clamp01((float)Clock.Day / 10f) * 0.7f + 0.3f);
	}

	
	private void ChangeDay()
	{
		this.UpdatePlaneMoss();
		if (BoltNetwork.isClient)
		{
			return;
		}
		Clock.Day++;
	}

	
	private void ShowDay()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
	}

	
	private void TurnDayTextOff()
	{
		if (BoltNetwork.isClient)
		{
			return;
		}
	}

	
	public void CopyPropertiesToNetwork()
	{
		if (BoltNetwork.isServer && CoopWeatherProxy.Instance)
		{
			IWeatherState state = CoopWeatherProxy.Instance.state;
			state.Temp = Clock.Temp;
			state.Day = Clock.Day;
			if (CoopPeerStarter.DedicatedHost || (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Loading))
			{
				state.TimeOfDay = this.Atmos.TimeOfDay;
			}
			state.ElapsedGameTime = this.ElapsedGameTime;
			if (!Mathf.Approximately(state.CloudOvercastTarget, Scene.WeatherSystem.CloudOvercastTarget))
			{
				state.CloudOvercastTarget = Scene.WeatherSystem.CloudOvercastTarget;
			}
			if (!Mathf.Approximately(state.CloudOpacityScaleTarget, Scene.WeatherSystem.CloudOpacityScaleTarget))
			{
				state.CloudOpacityScaleTarget = Scene.WeatherSystem.CloudOpacityScaleTarget;
			}
			state.Raining = Scene.WeatherSystem.Raining;
			state.RainLight = Scene.RainTypes.RainLight.activeInHierarchy;
			state.RainMedium = Scene.RainTypes.RainMedium.activeInHierarchy;
			state.RainHeavy = Scene.RainTypes.RainHeavy.activeInHierarchy;
			state.Rainbow = this.RainbowA.activeInHierarchy;
			state.RainbowIntensity = this.RainbowIntensity;
			state.NightTimeSfx = this.NightTimeSfx.activeInHierarchy;
			state.DayTimeSfx = this.DayTimeSfx.activeInHierarchy;
			state.Lightning = Scene.WeatherSystem.ShouldDoLighting;
		}
	}

	
	[SerializeThis]
	public static int Day;

	
	public static int Temp = 30;

	
	public static bool Dark;

	
	[SerializeThis]
	public static bool InCave;

	
	public static bool planecrash;

	
	public bool dayOverride;

	
	[SerializeThis]
	public float ElapsedGameTime;

	
	[SerializeThis]
	public float NextSleepTime = 0.5f;

	
	public bool isValidNextSleepTime;

	
	public Transform Ocean;

	
	public Light Sun;

	
	public Light Moon;

	
	private bool updateMutantsCheck;

	
	public GameObject RainbowA;

	
	public GameObject LightningFlashGroup;

	
	public GameObject NightTimeSfx;

	
	public GameObject DayTimeSfx;

	
	public bool TurnRainBowOff;

	
	public Color RainbowIntensity;

	
	private bool DaySwitch;

	
	public TheForestAtmosphere Atmos;
}
