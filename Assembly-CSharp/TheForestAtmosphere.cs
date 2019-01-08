using System;
using TheForest.Items.Inventory;
using TheForest.UI.Multiplayer;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using uSky;

[DoNotSerializePublic]
[ExecuteInEditMode]
[AddComponentMenu("The Forest/Atmosphere")]
public class TheForestAtmosphere : MonoBehaviour
{
	private float ForCurrentScaled
	{
		get
		{
			return (!LocalPlayer.IsInOverlookArea) ? ((!this.playerIsInSnowArea) ? this.FogCurrent : (this.FogCurrent * 0.5f)) : (this.FogCurrent * this.overlookAreaFogScale);
		}
	}

	public Color GetNoonFogColor
	{
		get
		{
			return (!ForestVR.Enabled) ? this.NoonFogColor : this.NoonFogColorVR;
		}
	}

	public float WindIntensity
	{
		get
		{
			return this.windIntensity;
		}
		private set
		{
			this.windIntensity = value;
			FMOD_StudioEventEmitter.WindIntensity = ((!LocalPlayer.IsInCaves) ? value : 0f);
		}
	}

	public Material InscatteringMaterial
	{
		get
		{
			return Sunshine.Instance.PostScatterMaterial;
		}
	}

	private void OnEnable()
	{
		if (TheForestAtmosphere.Instance == null)
		{
			TheForestAtmosphere.Instance = this;
		}
		this.clock = UnityEngine.Object.FindObjectOfType<Clock>();
		this.Randomize(true);
		this.SetShadowQualityLevel((int)TheForestQualitySettings.UserSettings.CascadeCount);
	}

	private void OnDisable()
	{
		UnityEngine.Object.DestroyImmediate(this.RTT_Inscattering);
	}

	private void OnPreCull()
	{
		this.UpdateShaderParameters(base.GetComponent<Camera>());
	}

	public void Randomize(bool force = false)
	{
		bool flag = this.isDaytime();
		bool flag2 = flag && this.LdotUp >= 0.99f;
		if ((flag2 && !this._wasNoon) || force)
		{
			this.CurrentSunsetLightColor = Color.Lerp(this.SunsetLightColor, this.SunsetLightAltColor, UnityEngine.Random.value);
		}
		this._wasDaytime = flag;
		this._wasNoon = flag2;
	}

	public void SetShadowQualityLevel(int level)
	{
		if (this.SunQualityLevels.Length > 0)
		{
			this.Sun.gameObject.SetActive(false);
			this.Sun = this.SunQualityLevels[Mathf.Min(level, this.SunQualityLevels.Length - 1)];
			this.Sun.gameObject.SetActive(true);
		}
		if (this.MoonQualityLevels.Length > 0)
		{
			this.Moon.gameObject.SetActive(false);
			this.Moon = this.MoonQualityLevels[Mathf.Min(level, this.MoonQualityLevels.Length - 1)];
			this.Moon.gameObject.SetActive(true);
		}
	}

	public void UpdateShaderParameters(Camera camera)
	{
		this.CAMERA_NEAR = camera.nearClipPlane;
		this.CAMERA_FAR = camera.farClipPlane;
		this.CAMERA_FOV = camera.fieldOfView;
		this.CAMERA_ASPECT_RATIO = camera.aspect;
		Matrix4x4 identity = Matrix4x4.identity;
		float num = this.CAMERA_FOV * 0.5f;
		Vector3 b = camera.transform.right * this.CAMERA_NEAR * Mathf.Tan(num * 0.0174532924f) * this.CAMERA_ASPECT_RATIO;
		Vector3 b2 = camera.transform.up * this.CAMERA_NEAR * Mathf.Tan(num * 0.0174532924f);
		Vector3 vector = camera.transform.forward * this.CAMERA_NEAR - b + b2;
		float d = vector.magnitude * this.CAMERA_FAR / this.CAMERA_NEAR;
		vector.Normalize();
		vector *= d;
		Vector3 vector2 = camera.transform.forward * this.CAMERA_NEAR + b + b2;
		vector2.Normalize();
		vector2 *= d;
		Vector3 vector3 = camera.transform.forward * this.CAMERA_NEAR + b - b2;
		vector3.Normalize();
		vector3 *= d;
		Vector3 vector4 = camera.transform.forward * this.CAMERA_NEAR - b - b2;
		vector4.Normalize();
		vector4 *= d;
		identity.SetRow(0, vector);
		identity.SetRow(1, vector2);
		identity.SetRow(2, vector3);
		identity.SetRow(3, vector4);
		Vector3[] array = new Vector3[4];
		Vector3[] array2 = new Vector3[4];
		camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Left, array);
		camera.CalculateFrustumCorners(new Rect(0f, 0f, 1f, 1f), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Right, array2);
		for (int i = 0; i < 4; i++)
		{
			array[i] = camera.transform.TransformVector(array[i]);
			array2[i] = camera.transform.TransformVector(array2[i]);
		}
		Matrix4x4 identity2 = Matrix4x4.identity;
		identity2.SetRow(0, array[1]);
		identity2.SetRow(1, array[2]);
		identity2.SetRow(2, array[3]);
		identity2.SetRow(3, array[0]);
		Matrix4x4 identity3 = Matrix4x4.identity;
		identity3.SetRow(0, array2[1]);
		identity3.SetRow(1, array2[2]);
		identity3.SetRow(2, array2[3]);
		identity3.SetRow(3, array2[0]);
		if (this.AtmosphereMesh != null)
		{
			if (Sunshine.Instance.Ready)
			{
				this.InscatteringMaterial.SetMatrix("_FrustumCornersWS", identity);
				this.InscatteringMaterial.SetMatrix("_FrustumCornersWSLeftEye", identity2);
				this.InscatteringMaterial.SetMatrix("_FrustumCornersWSRightEye", identity3);
				this.InscatteringMaterial.SetVector("_CameraWS", camera.transform.position);
				this.InscatteringMaterial.SetFloat("InscatteringIntensity", this.InscatteringIntensity);
				this.InscatteringMaterial.SetFloat("InscatteringDistribution", this.InscatteringDistribution);
				this.InscatteringMaterial.SetFloat("FogMinHeight", this.FogMinHeight);
				this.InscatteringMaterial.SetFloat("FogMaxHeight", this.FogMaxHeight);
			}
			Shader.SetGlobalFloat("FogStartDistance", this.FogStartDistance);
			Shader.SetGlobalFloat("Visibility", this.Visibility);
			Shader.SetGlobalColor("_SkylightColor", TheForestAtmosphere.AmbientSheen);
			Shader.SetGlobalVector("_CameraAtmosphere", camera.transform.position);
			Shader.SetGlobalFloat("_InscatteringDistribution", this.InscatteringDistribution);
			Shader.SetGlobalFloat("_InscatteringIntensity", this.InscatteringIntensity);
			Shader.SetGlobalFloat("_FogMinHeight", this.FogMinHeight);
			Shader.SetGlobalFloat("_FogMaxHeight", this.FogMaxHeight);
			Shader.SetGlobalFloat("_FogIntensity", this.FogMaxHeight);
		}
	}

	private void Awake()
	{
		this.afs = GameObject.FindWithTag("Afs").GetComponent<SetupAdvancedFoliageShader>();
		this.WindIntensity = 0f;
		this.UpdateFoliageShaderWind();
		this.windGustIntervalTimer = UnityEngine.Random.Range(15f, 120f);
		this.OceanH = GameObject.FindWithTag("OceanHeight").transform;
		base.InvokeRepeating("ChangeFogAmount", 500f, 600f);
		base.InvokeRepeating("ChangeMoonLight", 10f, 10f);
		this.DeltaTimeOfDay = 0.0;
	}

	private bool LightningActive
	{
		get
		{
			return this.clock && this.clock.LightningFlashGroup.activeSelf;
		}
	}

	private float TimeOfDayDiff(float from, float to)
	{
		return ((from >= to) ? (to + 360f) : to) - from;
	}

	private float LerpToTimeOfDay(float from, float to, float alpha)
	{
		return Mathf.Lerp(from, (from >= to) ? (to + 360f) : to, alpha) % 360f;
	}

	private Quaternion SunRotationAtTimeOfDay(float timeOfDay)
	{
		return Quaternion.Euler(new Vector3(this.Lx, this.Ly, 0f)) * Quaternion.Euler(0f, timeOfDay, 0f);
	}

	private void Update()
	{
		if (LocalPlayer.Stats)
		{
			this.playerIsInSnowArea = LocalPlayer.Stats.IsInNorthColdArea();
		}
		Shader.SetGlobalColor("_AmbientSkyColor", RenderSettings.ambientSkyColor.linear * RenderSettings.ambientIntensity);
		bool flag = false;
		if (!CoopPeerStarter.DedicatedHost && LocalPlayer.Transform)
		{
			bool flag2 = false;
			if (Scene.SceneTracker.caveEntrances.Count > 0 && LocalPlayer.IsInCaves && !LocalPlayer.IsInEndgame)
			{
				for (int i = Scene.SceneTracker.caveEntrances.Count - 1; i >= 0; i--)
				{
					if (Scene.SceneTracker.caveEntrances[i] != null && (LocalPlayer.Transform.position - Scene.SceneTracker.caveEntrances[i].transform.position).sqrMagnitude < 2500f)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			if (this.Sun.gameObject.activeSelf)
			{
				this.Sun.gameObject.SetActive(false);
			}
			if (this.Moon.gameObject.activeSelf)
			{
				this.Moon.gameObject.SetActive(false);
			}
		}
		else
		{
			if (!this.Sun.gameObject.activeSelf)
			{
				this.Sun.gameObject.SetActive(true);
			}
			if (!this.Moon.gameObject.activeSelf)
			{
				this.Moon.gameObject.SetActive(true);
			}
		}
		this.Randomize(false);
		this.caveAmt = Mathf.Lerp(this.caveAmt, (!LocalPlayer.IsInClosedArea) ? 0f : 1f, Time.deltaTime);
		Shader.SetGlobalFloat("CaveAmount", this.caveAmt);
		Shader.SetGlobalFloat("_ForestCaveSetting", Mathf.Lerp(1f, -1f, this.caveAmt));
		if (this.MoonLightIntensity < this.MoonBrightness)
		{
			this.MoonLightIntensity += 0.01f * Time.deltaTime;
		}
		else if (this.MoonLightIntensity > this.MoonBrightness)
		{
			this.MoonLightIntensity -= 0.01f * Time.deltaTime;
		}
		if (LocalPlayer.IsInOutsideWorld)
		{
			if (LocalPlayer.IsInOverlookArea)
			{
				this.FogMinHeight = this.OceanH.position.y + 1450f;
				this.Visibility = this.ForCurrentScaled;
			}
			else
			{
				this.FogMinHeight = this.OceanH.position.y;
			}
		}
		bool flag3 = LocalPlayer.AnimControl != null && LocalPlayer.AnimControl.endGameCutScene;
		bool flag4 = LocalPlayer.AnimControl != null && LocalPlayer.AnimControl.exitingACave;
		if (Application.isPlaying && !this.overrideVisibility && (CoopPeerStarter.DedicatedHost || !flag3))
		{
			if (this.Visibility < this.ForCurrentScaled)
			{
				this.Visibility += 1f;
			}
			else if (this.Visibility > this.ForCurrentScaled)
			{
				this.Visibility -= 1f;
			}
		}
		if (Application.isPlaying && (CoopPeerStarter.DedicatedHost || LocalPlayer.IsInClosedArea || flag4))
		{
			float b = this.CaveAmbientIntensity;
			float b2 = 0f;
			float b3 = 0f;
			Color b4 = Color.black;
			Color b5 = this.CaveSkyColor;
			Color b6 = this.CaveEquatorColor;
			Color b7 = this.CaveGroundColor;
			Color b8 = this.CaveAddLight1;
			Vector3 b9 = this.CaveAddLight1Dir;
			Color b10 = this.CaveAddLight2;
			Vector3 b11 = this.CaveAddLight2Dir;
			if (Application.isPlaying && !CoopPeerStarter.DedicatedHost && flag4)
			{
				if (this.isDaytime())
				{
					b = Mathf.Lerp(this.SunsetAmbientIntensity, this.DayAmbientIntensity, this.LdotUp);
					b2 = Mathf.Lerp(this.SunsetSunIntensity, this.DaySunIntensity, this.LdotUp);
					b3 = Mathf.Lerp(this.SunsetSunBounceIntensity, this.DaySunBounceIntensity, this.LdotUp);
					b4 = Color.Lerp(this.SunsetBounceColor * this.SunsetSunBounceIntensity, this.DayBounceColor * this.DaySunBounceIntensity, this.LdotUp);
					b5 = Color.Lerp(this.SunsetColor * this.SunsetAmbientIntensity, this.DayColor * this.DayAmbientIntensity, this.LdotUp);
					b6 = Color.Lerp(this.SunsetColorEq * this.SunsetAmbientIntensity, this.DayColorEq * this.DayAmbientIntensity, this.LdotUp);
					b7 = Color.Lerp(this.SunsetColorGround * this.SunsetAmbientIntensity, this.DayColorGround * this.DayAmbientIntensity, this.LdotUp);
					b8 = Color.Lerp(this.SunsetAddLight1, this.NoonAddLight1, this.LdotUp);
					b9 = Vector3.Lerp(this.SunsetAddLight1Dir, this.NoonAddLight1Dir, this.LdotUp);
					b10 = Color.Lerp(this.SunsetAddLight2, this.NoonAddLight2, this.LdotUp);
					b11 = Vector3.Lerp(this.SunsetAddLight2Dir, this.NoonAddLight2Dir, this.LdotUp);
				}
				else
				{
					b = Mathf.Lerp(this.SunsetAmbientIntensity, this.NightAmbientIntensity, this.LdotDown);
					b2 = Mathf.Lerp(this.SunsetSunIntensity, this.NightSunIntensity, this.LdotDown);
					b3 = Mathf.Lerp(this.SunsetSunBounceIntensity, this.NightSunBounceIntensity, this.LdotDown);
					b4 = Color.Lerp(this.SunsetBounceColor * this.SunsetSunBounceIntensity, this.NightBounceColor * this.NightSunBounceIntensity, this.LdotDown);
					b5 = Color.Lerp(this.SunsetColor * this.SunsetAmbientIntensity, this.NightColor * this.NightAmbientIntensity, this.LdotDown);
					b6 = Color.Lerp(this.SunsetColorEq * this.SunsetAmbientIntensity, this.NightColorEq * this.NightAmbientIntensity, this.LdotDown);
					b7 = Color.Lerp(this.SunsetColorGround * this.SunsetAmbientIntensity, this.NightColorGround * this.NightAmbientIntensity, this.LdotDown);
					b8 = Color.Lerp(this.SunsetAddLight1, this.NightAddLight1, this.LdotDown);
					b9 = Vector3.Lerp(this.SunsetAddLight1Dir, this.NightAddLight1Dir, this.LdotDown);
					b10 = Color.Lerp(this.SunsetAddLight2, this.NightAddLight2, this.LdotDown);
					b11 = Vector3.Lerp(this.SunsetAddLight2Dir, this.NightAddLight2Dir, this.LdotDown);
				}
			}
			TheForestAtmosphere.ReflectionAmount = 0.2f;
			float num = 0.5f;
			if (Application.isPlaying && !CoopPeerStarter.DedicatedHost && flag4)
			{
				num = 1f;
			}
			this.temp_AmbientIntensity = Mathf.Lerp(this.SunsetAmbientIntensity, b, Time.deltaTime * num);
			this.temp_SunIntensity = Mathf.Lerp(this.temp_SunIntensity, b2, Time.deltaTime * num);
			this.temp_SunBounceIntensity = Mathf.Lerp(this.temp_SunBounceIntensity, b3, Time.deltaTime * num);
			this.temp_BounceColor = Color.Lerp(this.temp_BounceColor, b4, Time.deltaTime * num);
			this.temp_SkyColor = Color.Lerp(this.temp_SkyColor, b5, Time.deltaTime * num);
			this.temp_EquatorColor = Color.Lerp(this.temp_EquatorColor, b6, Time.deltaTime * num);
			this.temp_GroundColor = Color.Lerp(this.temp_GroundColor, b7, Time.deltaTime * num);
			this.temp_AddLight1 = Color.Lerp(this.SunsetAddLight1, b8, Time.deltaTime * num);
			this.temp_AddLight1Dir = Vector3.Lerp(this.SunsetAddLight1Dir, b9, Time.deltaTime * num);
			this.temp_AddLight2 = Color.Lerp(this.SunsetAddLight2, b10, Time.deltaTime * num);
			this.temp_AddLight2Dir = Vector3.Lerp(this.SunsetAddLight2Dir, b11, Time.deltaTime * num);
			this.SetSHAmbientLighting();
		}
		if (this.Sun == null)
		{
			Debug.Log("Set the main light");
		}
		else
		{
			this.L = -this.Sun.transform.forward;
			DistanceCloud.sunlightVector = this.L;
			this.LdotUp = Mathf.Clamp01(Vector3.Dot(this.L, Vector3.up));
			this.LdotDown = Mathf.Clamp01(Vector3.Dot(this.L, Vector3.down));
			float num2 = Mathf.Cos(this.dayNightTransitionAngle * 0.0174532924f);
			float num3 = 1f / (1f - num2);
			float num4 = Mathf.Clamp01((this.LdotUp - num2) * num3);
			float num5 = 1f - Mathf.Pow(1f - num4, 4f);
			Color color = Color.Lerp(this.CurrentSunsetLightColor, this.NoonLightColor, num5);
			if (num5 <= 0f)
			{
				float num6 = Mathf.Abs(this.LdotUp - num2) * (1f / num2);
				float t = Mathf.Clamp01(num6 * 1.5f);
				color = Color.Lerp(this.CurrentSunsetLightColor, this.Moon.color, t);
			}
			color.r = Mathf.Pow(color.r, 1f);
			color.g = Mathf.Pow(color.g, 1f);
			color.b = Mathf.Pow(color.b, 1f);
			this.Sun.color = color;
			this.FogColor = Color.Lerp(Color.Lerp(new Color(0f, 0f, 0f, 0f), this.GetNoonFogColor, Mathf.Clamp01(this.LdotUp)), new Color(0f, 0f, 0f, 0f), Mathf.Clamp01(this.LdotDown));
			if (Sunshine.Instance.Ready)
			{
				Sunshine.Instance.ScatterColor = this.FogColor;
			}
			if (LocalPlayer.IsInClosedArea || this.overrideVisibility)
			{
				this.FogColor = new Color(0f, 0f, 0f, 0f);
			}
			this.CurrentFogColor = this.FogColor;
			this.Vanishing = Mathf.Lerp(Mathf.Lerp(0f, 1f, Mathf.Clamp01(10f * this.LdotUp)), 1f, Mathf.Clamp01(10f * this.LdotDown));
			if (Sunshine.Instance.Ready)
			{
				Sunshine.Instance.PostScatterMaterial.SetFloat("Vanishing", this.Vanishing);
			}
			this.sunE = 10000f * Mathf.Max(0f, 1f - Mathf.Exp(-((this.cutoffAngle - Mathf.Acos(this.LdotUp)) / this.steepness)));
			if (this.Animate && !BoltNetwork.isClient)
			{
				if (!SteamDSConfig.isDedicatedServer || Scene.SceneTracker.allPlayerEntities.Count != 0)
				{
					float num7 = Time.deltaTime * this.RotationSpeed * TheForestAtmosphere.GameTimeScale;
					if (this.Sleeping)
					{
						this.TimeOfDay += num7 * 600f;
						this.DeltaTimeOfDay = (double)(num7 * 600f / 360f);
					}
					else if (this.Moon.enabled)
					{
						this.TimeOfDay += num7 * 2f;
						this.DeltaTimeOfDay = (double)(num7 * 2f / 360f);
					}
					else
					{
						this.TimeOfDay += num7;
						this.DeltaTimeOfDay = (double)(num7 / 360f);
					}
					this.TimeOfDay %= 360f;
					FMOD_StudioEventEmitter.HoursSinceMidnight = (this.TimeOfDay + 180f) % 360f * 0.06666667f;
				}
				else
				{
					this.DeltaTimeOfDay = 0.0;
				}
			}
			PlayerInventory.PlayerViews playerViews = (!LocalPlayer.Inventory.IsNull()) ? LocalPlayer.Inventory.CurrentView : PlayerInventory.PlayerViews.Loading;
			bool flag5 = LocalPlayer.Inventory.IsNull() || this.Sleeping || (!LocalPlayer.Inventory.enabled && !ChatBox.IsChatOpen) || playerViews < PlayerInventory.PlayerViews.World || (playerViews > PlayerInventory.PlayerViews.Inventory && playerViews < PlayerInventory.PlayerViews.Sleep && playerViews != PlayerInventory.PlayerViews.Book);
			bool flag6 = LocalPlayer.MainCamTr.IsNull() || Vector3.Distance(LocalPlayer.MainCamTr.forward, this.PrevCameraForward) > 0.01f;
			bool flag7 = LocalPlayer.Rigidbody.IsNull() || LocalPlayer.Rigidbody.velocity.sqrMagnitude > 0.5f || ChatBox.IsChatOpen || playerViews == PlayerInventory.PlayerViews.Book;
			if (this.TimeOfDay > 70f && this.TimeOfDay < 280f)
			{
				this.CatchUpTimeOfDay = true;
			}
			if (BoltNetwork.isClient && playerViews >= PlayerInventory.PlayerViews.Loading && (!CoopWeatherProxy.Instance || !CoopWeatherProxy.Instance.enabled))
			{
				this.TimeOfDay = CoopWeatherProxy.JoiningTimeOfDay;
				this.ForceSunRotationUpdate = true;
			}
			float num8;
			if (this.LdotUp < 0.5f)
			{
				num8 = Mathf.Clamp(this.DayCloudAlphaValue * this.LdotUp, this.NightCloudAlphaValue, 1f);
			}
			else
			{
				num8 = this.DayCloudAlphaValue;
			}
			if (Application.isPlaying && Scene.WeatherSystem.vClouds && !Mathf.Approximately(num8, Scene.WeatherSystem.vClouds.materialUsed.GetColor("_ShadowColor").a))
			{
				this.smoothTargetCloudAlpha = Mathf.Lerp(this.smoothTargetCloudAlpha, num8, Time.deltaTime * 0.2f);
				Color color2 = Scene.WeatherSystem.vClouds.materialUsed.GetColor("_ShadowColor");
				color2.a = this.smoothTargetCloudAlpha;
				Scene.WeatherSystem.vClouds.materialUsed.SetColor("_ShadowColor", color2);
			}
			float num9 = this.TimeOfDay;
			if (LocalPlayer.IsInOverlookArea)
			{
				num9 = 20f;
				this.ForceSunRotationUpdate = true;
			}
			if (this.OverrideLightingTimeOfDay)
			{
				num9 = this.LightingTimeOfDayOverrideValue;
				this.ForceSunRotationUpdate = true;
			}
			if (flag5 || this.ForceSunRotationUpdate || Clock.Dark)
			{
				this.DelayedTimeOfDayAlpha = 1f;
				this.DelayedTimeOfDay = num9;
				this.Sun.transform.rotation = this.SunRotationAtTimeOfDay(this.DelayedTimeOfDay);
				this.ForceSunRotationUpdate = false;
				this.CatchUpTimeOfDay = false;
			}
			else if (this.CatchUpTimeOfDay)
			{
				if (this.DelayedTimeOfDayAlpha < 0f)
				{
					this.DelayedTimeOfDayAlpha = 0f;
					this.MovementStartTimeOfDay = this.DelayedTimeOfDay;
				}
				this.DelayedTimeOfDayAlpha = Mathf.Clamp01(this.DelayedTimeOfDayAlpha + Time.deltaTime / 5f);
				float num10 = MathEx.Easing.EaseInOutQuad(this.DelayedTimeOfDayAlpha, 0f, 1f, 1f);
				if (num10 >= 1f)
				{
					num10 = 1f;
					this.CatchUpTimeOfDay = false;
				}
				this.DelayedTimeOfDay = this.LerpToTimeOfDay(this.MovementStartTimeOfDay, num9, num10);
				this.Sun.transform.rotation = this.SunRotationAtTimeOfDay(this.DelayedTimeOfDay);
			}
			else if (flag7 || flag6)
			{
				if (this.DelayedTimeOfDayAlpha < 0f)
				{
					this.DelayedTimeOfDayAlpha = 0f;
					this.MovementStartTimeOfDay = this.DelayedTimeOfDay;
				}
				this.DelayedTimeOfDayAlpha = Mathf.Clamp01(this.DelayedTimeOfDayAlpha + Time.deltaTime / 5f);
				float alpha = MathEx.Easing.EaseInQuad(this.DelayedTimeOfDayAlpha, 0f, 1f, 1f);
				this.DelayedTimeOfDay = this.LerpToTimeOfDay(this.MovementStartTimeOfDay, num9, alpha);
				this.Sun.transform.rotation = this.SunRotationAtTimeOfDay(this.DelayedTimeOfDay);
				if (flag6 && !LocalPlayer.MainCamTr.IsNull())
				{
					this.PrevCameraForward = LocalPlayer.MainCamTr.forward;
				}
			}
			else
			{
				this.DelayedTimeOfDayAlpha = -1f;
				float num11;
				if (this.DelayedTimeOfDay <= num9)
				{
					num11 = num9 - this.DelayedTimeOfDay;
				}
				else
				{
					num11 = (num9 + 360f - this.DelayedTimeOfDay) % 360f;
				}
				if (num11 > 5f)
				{
					this.CatchUpTimeOfDay = true;
				}
			}
			if (this.Sleeping)
			{
				this.ForceSunRotationUpdate = true;
			}
		}
		if (this.Moon == null)
		{
			Debug.Log("Set the moon");
		}
		else
		{
			this.Moon.transform.rotation = Quaternion.Euler(new Vector3(this.Lx - 180f, this.Ly, 0f)) * Quaternion.Euler(0f, -this.TimeOfDay, 0f);
		}
		if (Application.isPlaying)
		{
			Vector3 vector = (!Clock.Dark) ? Scene.Atmosphere.Sun.transform.forward : Scene.Atmosphere.Moon.transform.forward;
			vector.y = 0f;
			float num12 = 1f + (LOD_Manager.TreeOcclusionBonusRatio - 1f) * this.OCCLUSION_MULT;
			num12 = Mathf.Clamp(num12, 1f, 4f);
			float num13 = LOD_Manager.Instance.Trees.BaseRanges[2] + this.SHADOW_BASE_DISTANCE;
			int num14 = Mathf.FloorToInt(num13 * num12);
			if (this.startShadowCoolDown < 5f)
			{
				this.startShadowCoolDown += Time.deltaTime;
				num14 = 300;
			}
			switch (TheForestQualitySettings.UserSettings.ShadowLevel)
			{
			case TheForestQualitySettings.ShadowLevels.VeryHigh:
				num14 = num14;
				goto IL_12C3;
			case TheForestQualitySettings.ShadowLevels.High:
				num14 = num14;
				goto IL_12C3;
			case TheForestQualitySettings.ShadowLevels.Medium:
				num14 = num14;
				goto IL_12C3;
			case TheForestQualitySettings.ShadowLevels.Low:
				num14 = 130;
				goto IL_12C3;
			case TheForestQualitySettings.ShadowLevels.Fastest:
				num14 = 100;
				goto IL_12C3;
			}
			num14 = 60;
			IL_12C3:
			if (this.LowerShadowsAtExtremeLightAngles)
			{
				QualitySettings.shadowDistance = Mathf.Lerp(1f, 0.35f, (vector.magnitude - 0.9f) * 10f) * (float)num14;
			}
			else
			{
				QualitySettings.shadowDistance = (float)num14;
			}
			this.DebugShadowDist = QualitySettings.shadowDistance;
		}
		if (this.isDaytime())
		{
			if (!this.Sun.enabled)
			{
				this.Sun.color = this.Moon.color;
				this.Sun.enabled = true;
				this.Moon.enabled = false;
				this.afs.DirectionalLightReference = this.Sun.gameObject;
			}
			if (LocalPlayer.IsInOutsideWorld)
			{
				TheForestAtmosphere.Ambient = Color.Lerp(this.SunsetColor, this.DayColor, this.LdotUp);
				TheForestAtmosphere.AmbientSheen = Color.Lerp(this.SunsetColor, this.DayColor, this.LdotUp);
				this.Sun.intensity = (TheForestAtmosphere.ReflectionAmount = this.SunLightIntensity * Mathf.Clamp01((1f - Mathf.Pow(1f - this.LdotUp, 2f)) * 1.1f - 0.1f) * 0.5f);
				if (Scene.WeatherSystem)
				{
					this.Sun.intensity *= Mathf.Lerp(1f, 0.625f, Scene.WeatherSystem.CloudCovergage);
				}
				this.atmosphereGain = 1f;
				if (Application.isPlaying && !CoopPeerStarter.DedicatedHost && !flag4)
				{
					this.temp_AmbientIntensity = Mathf.Lerp(this.SunsetAmbientIntensity, this.DayAmbientIntensity, this.LdotUp);
					this.temp_SunIntensity = Mathf.Lerp(this.SunsetSunIntensity, this.DaySunIntensity, this.LdotUp);
					this.temp_SunBounceIntensity = Mathf.Lerp(this.SunsetSunBounceIntensity, this.DaySunBounceIntensity, this.LdotUp);
					this.temp_BounceColor = Color.Lerp(this.SunsetBounceColor * this.SunsetSunBounceIntensity, this.DayBounceColor * this.DaySunBounceIntensity, this.LdotUp);
					this.temp_SkyColor = Color.Lerp(this.SunsetColor * this.SunsetAmbientIntensity, this.DayColor * this.DayAmbientIntensity, this.LdotUp);
					this.temp_EquatorColor = Color.Lerp(this.SunsetColorEq * this.SunsetAmbientIntensity, this.DayColorEq * this.DayAmbientIntensity, this.LdotUp);
					this.temp_GroundColor = Color.Lerp(this.SunsetColorGround * this.SunsetAmbientIntensity, this.DayColorGround * this.DayAmbientIntensity, this.LdotUp);
					this.temp_AddLight1 = Color.Lerp(this.SunsetAddLight1, this.NoonAddLight1, this.LdotUp);
					this.temp_AddLight1Dir = Vector3.Lerp(this.SunsetAddLight1Dir, this.NoonAddLight1Dir, this.LdotUp);
					this.temp_AddLight2 = Color.Lerp(this.SunsetAddLight2, this.NoonAddLight2, this.LdotUp);
					this.temp_AddLight2Dir = Vector3.Lerp(this.SunsetAddLight2Dir, this.NoonAddLight2Dir, this.LdotUp);
					this.SetSHAmbientLighting();
				}
			}
			else
			{
				TheForestAtmosphere.AmbientSheen = Color.black;
			}
			Sunshine.Instance.SunLight = this.Sun;
			if (Sunshine.Instance.Ready)
			{
				this.InscatteringMaterial.SetVector("L", this.L);
				Sunshine.Instance.PostScatterMaterial.SetVector("InscatteringColor", this.Sun.color * this.Sun.intensity);
				Shader.SetGlobalVector("_LightVecL", this.L);
				Shader.SetGlobalVector("_InscatteringColor", this.Sun.color * this.Sun.intensity);
			}
		}
		else
		{
			if (!this.Moon.enabled)
			{
				this.Moon.enabled = true;
				this.afs.DirectionalLightReference = this.Moon.gameObject;
				this.Sun.enabled = false;
			}
			Sunshine.Instance.SunLight = this.Moon;
			if (!CoopPeerStarter.DedicatedHost && LocalPlayer.IsInOutsideWorld)
			{
				this.Moon.intensity = this.MoonLightIntensity * (1f - Mathf.Pow(1f - this.LdotDown, 2f));
				this.atmosphereGain = Mathf.Lerp(1f, 0.01f, Mathf.Clamp01(15f * this.LdotDown));
				TheForestAtmosphere.ReflectionAmount = this.Moon.intensity / 3f;
				TheForestAtmosphere.Ambient = Color.Lerp(this.SunsetColor, this.NightColor, this.LdotDown);
				TheForestAtmosphere.AmbientSheen = Color.Lerp(this.SunsetColor, this.NightColor, this.LdotDown);
				if (Application.isPlaying && !flag4)
				{
					this.temp_AmbientIntensity = Mathf.Lerp(this.SunsetAmbientIntensity, this.NightAmbientIntensity, this.LdotDown);
					this.temp_SunIntensity = Mathf.Lerp(this.SunsetSunIntensity, this.NightSunIntensity, this.LdotDown);
					this.temp_SunBounceIntensity = Mathf.Lerp(this.SunsetSunBounceIntensity, this.NightSunBounceIntensity, this.LdotDown);
					this.temp_BounceColor = Color.Lerp(this.SunsetBounceColor * this.SunsetSunBounceIntensity, this.NightBounceColor * this.NightSunBounceIntensity, this.LdotDown);
					this.temp_SkyColor = Color.Lerp(this.SunsetColor * this.SunsetAmbientIntensity, this.NightColor * this.NightAmbientIntensity, this.LdotDown);
					this.temp_EquatorColor = Color.Lerp(this.SunsetColorEq * this.SunsetAmbientIntensity, this.NightColorEq * this.NightAmbientIntensity, this.LdotDown);
					this.temp_GroundColor = Color.Lerp(this.SunsetColorGround * this.SunsetAmbientIntensity, this.NightColorGround * this.NightAmbientIntensity, this.LdotDown);
					this.temp_AddLight1 = Color.Lerp(this.SunsetAddLight1, this.NightAddLight1, this.LdotDown);
					this.temp_AddLight1Dir = Vector3.Lerp(this.SunsetAddLight1Dir, this.NightAddLight1Dir, this.LdotDown);
					this.temp_AddLight2 = Color.Lerp(this.SunsetAddLight2, this.NightAddLight2, this.LdotDown);
					this.temp_AddLight2Dir = Vector3.Lerp(this.SunsetAddLight2Dir, this.NightAddLight2Dir, this.LdotDown);
					this.SetSHAmbientLighting();
				}
			}
			else
			{
				TheForestAtmosphere.AmbientSheen = Color.black;
			}
			if (Sunshine.Instance.Ready)
			{
				this.InscatteringMaterial.SetVector("L", -this.L);
				Sunshine.Instance.PostScatterMaterial.SetVector("InscatteringColor", this.Moon.color * this.Moon.intensity / 2f);
				Shader.SetGlobalVector("_LightVecL", -this.L);
				Shader.SetGlobalVector("_InscatteringColor", this.Moon.color * this.Moon.intensity / 2f);
			}
		}
		Shader.SetGlobalColor("_FogColor", this.CurrentFogColor);
		this.UpdateWind();
	}

	private void SetSHAmbientLighting()
	{
		this.AmbientSHLighting.Clear();
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, 1f, 0f), this.temp_SkyColor, 1f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(1f, 0f, 0f), this.temp_EquatorColor, 1f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(-1f, 0f, 0f), this.temp_EquatorColor, 1f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, 0f, 1f), this.temp_EquatorColor, 1f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, 0f, -1f), this.temp_EquatorColor, 1f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, -1f, 0f), this.temp_GroundColor, 1f);
		this.AmbientSHLighting.AddDirectionalLight(this.temp_AddLight1Dir, this.temp_AddLight1, 1f);
		this.AmbientSHLighting.AddDirectionalLight(this.temp_AddLight2Dir, this.temp_AddLight2, 1f);
		float intensity;
		if (this.LdotUp > 0f)
		{
			intensity = Mathf.Clamp01(this.LdotUp / 0.34f);
		}
		else if (this.LdotDown > 0f)
		{
			intensity = Mathf.Clamp01(this.LdotDown / 0.34f);
		}
		else
		{
			intensity = 0f;
		}
		Light component = Sunshine.Instance.SunLight.GetComponent<Light>();
		this.temp_AmbientSunColor = component.color.linear * this.temp_SunIntensity;
		this.AmbientSHLighting.AddDirectionalLight(component.transform.forward * -1f, this.temp_AmbientSunColor, intensity);
		this.AmbientSHLighting.AddDirectionalLight(component.transform.forward, component.color.linear * this.temp_BounceColor.linear, intensity);
		RenderSettings.ambientProbe = this.AmbientSHLighting;
	}

	private void SetSHAmbientLighting_New()
	{
		this.AmbientSHLighting.Clear();
		Vector3 vector = new Vector3(0.612372458f, 0.5f, 0.612372458f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, 1f, 0f), this.temp_SkyColor, 0.428571433f);
		Color color = Color.Lerp(this.temp_SkyColor, this.temp_EquatorColor, 0.5f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(-vector.x, vector.y, -vector.z), color, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(vector.x, vector.y, -vector.z), color, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(-vector.x, vector.y, vector.z), color, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(vector.x, vector.y, vector.z), color, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(1f, 0f, 0f), this.temp_EquatorColor, 0.142857149f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(-1f, 0f, 0f), this.temp_EquatorColor, 0.142857149f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, 0f, 1f), this.temp_EquatorColor, 0.142857149f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, 0f, -1f), this.temp_EquatorColor, 0.142857149f);
		Color color2 = Color.Lerp(this.temp_EquatorColor, this.temp_GroundColor, 0.5f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(-vector.x, -vector.y, -vector.z), color2, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(vector.x, -vector.y, -vector.z), color2, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(-vector.x, -vector.y, vector.z), color2, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(vector.x, -vector.y, vector.z), color2, 0.2857143f);
		this.AmbientSHLighting.AddDirectionalLight(new Vector3(0f, -1f, 0f), this.temp_GroundColor, 0.428571433f);
		this.AmbientSHLighting.AddDirectionalLight(this.temp_AddLight1Dir, this.temp_AddLight1, 1f);
		this.AmbientSHLighting.AddDirectionalLight(this.temp_AddLight2Dir, this.temp_AddLight2, 1f);
		float intensity;
		if (this.LdotUp > 0f)
		{
			intensity = Mathf.Clamp01(this.LdotUp / 0.34f);
		}
		else if (this.LdotDown > 0f)
		{
			intensity = Mathf.Clamp01(this.LdotDown / 0.34f);
		}
		else
		{
			intensity = 0f;
		}
		Light component = Sunshine.Instance.SunLight.GetComponent<Light>();
		this.temp_AmbientSunColor = component.color.linear * this.temp_SunIntensity;
		this.AmbientSHLighting.AddDirectionalLight(component.transform.forward * -1f, this.temp_AmbientSunColor, intensity);
		this.AmbientSHLighting.AddDirectionalLight(component.transform.forward, component.color.linear * this.temp_BounceColor.linear, intensity);
		RenderSettings.ambientProbe = this.AmbientSHLighting;
	}

	private void ChangeMoonLight()
	{
		this.MoonBrightness = UnityEngine.Random.Range(0.65f, 1f);
	}

	public bool isDaytime()
	{
		bool result = false;
		if (this.LdotUp > 0f)
		{
			result = true;
		}
		if (this.LdotDown > 0f)
		{
			result = false;
		}
		return result;
	}

	private void ChangeFogAmount()
	{
		if (LocalPlayer.IsInEndgame)
		{
			this.FogCurrent = 900f;
		}
		else if (LocalPlayer.IsInCaves)
		{
			this.FogCurrent = 3000f;
		}
		else
		{
			this.FogCurrent = (float)UnityEngine.Random.Range(700, 2000);
		}
	}

	private void ChangeColors()
	{
	}

	public void TimeLapse()
	{
		Scene.Clock.NextSleepTime = Scene.Clock.ElapsedGameTime;
		this.Sleeping = true;
		if (Clock.Dark)
		{
			this.SleepUntilMorning = true;
		}
		Debug.Log("Sleeping " + ((!this.SleepUntilMorning) ? ("8 hours (sleeping at daytime: " + FMOD_StudioEventEmitter.HoursSinceMidnight + ")") : ("until morning (sleeping at dark: " + FMOD_StudioEventEmitter.HoursSinceMidnight + ")")));
	}

	public void NoTimeLapse()
	{
		this.Sleeping = false;
		this.SleepUntilMorning = false;
	}

	private void UpdateFoliageShaderWind()
	{
		this.afs.Wind.w = this.WindIntensity * this.WindIntensity * 1.5f;
	}

	private static float MoveTowardsTarget(float current, float target, float delta)
	{
		if (current < target)
		{
			return Math.Min(current + delta * Time.deltaTime, target);
		}
		if (current > target)
		{
			return Math.Max(current - delta * Time.deltaTime, target);
		}
		return current;
	}

	private void UpdateWind()
	{
		this.windBase = TheForestAtmosphere.MoveTowardsTarget(this.windBase, this.windBaseTarget, this.windBaseDelta);
		this.windGust = TheForestAtmosphere.MoveTowardsTarget(this.windGust, this.windGustTarget, this.windGustDelta);
		if (this.windGustTarget > 0f && this.windGust == this.windGustTarget)
		{
			this.windGustTarget = 0f;
			this.windGustDelta = 1f / UnityEngine.Random.Range(5f, 30f);
		}
		this.windBaseHoldTimer -= Time.deltaTime;
		if (this.windBaseHoldTimer <= 0f)
		{
			this.windBaseTarget = UnityEngine.Random.Range(0f, 0.4f);
			this.windBaseDelta = 1f / UnityEngine.Random.Range(60f, 120f);
			this.windBaseHoldTimer = UnityEngine.Random.Range(60f, 600f);
		}
		this.windGustIntervalTimer -= Time.deltaTime;
		if (this.windGustIntervalTimer <= 0f)
		{
			this.windGustTarget = UnityEngine.Random.Range(0.2f, 0.7f);
			this.windGustDelta = 1f / UnityEngine.Random.Range(5f, 30f);
			this.windGustIntervalTimer = UnityEngine.Random.Range(15f, 120f);
		}
		float num;
		if (LocalPlayer.Transform)
		{
			num = Mathf.Lerp(1f, 2.5f, (LocalPlayer.Transform.position.y - 150f) / 150f);
		}
		else
		{
			num = 1f;
		}
		if (this.debugWind)
		{
			this.windIntensity = this.debugWindIntensity;
		}
		else
		{
			this.WindIntensity = this.windBase * num + this.windGust;
		}
		this.UpdateFoliageShaderWind();
		if (FMOD_Listener.IsDebugEnabled && UnityEngine.Input.GetKeyDown(KeyCode.RightControl))
		{
			this.showDebug = !this.showDebug;
		}
	}

	public static TheForestAtmosphere Instance;

	private Transform OceanH;

	public static Color Ambient;

	public static Color AmbientSheen;

	public static Color CurrentLightColor;

	public static float ReflectionAmount;

	public float FogCurrent = 150f;

	public float overlookAreaFogScale = 1.2f;

	public bool Sleeping;

	public bool SleepUntilMorning;

	private float MoonBrightness = 0.16f;

	public float StarsIntensity;

	public float Horizon = 1000f;

	public float mieDirectionalG = 0.75f;

	public bool LowerShadowsAtExtremeLightAngles = true;

	public bool OverrideLightingTimeOfDay;

	public float LightingTimeOfDayOverrideValue;

	public Light Sun;

	public Light Moon;

	public Light[] SunQualityLevels;

	public Light[] MoonQualityLevels;

	private Vector3 L;

	public Color FogColor = Color.grey;

	public Color NoonFogColor = Color.grey;

	public Color SunsetFogColor = Color.grey;

	public Color NoonFogColorVR = Color.grey;

	public Color NightCloudColor;

	public float reileighCoefficient = 0.053f;

	public float mieCoefficient = 1f;

	public float Molecules = 2.55f;

	public float turbidity = 1f;

	public float dayNightTransitionAngle = 80f;

	public bool FoldoutFog = true;

	public bool FoldoutSky = true;

	public bool FoldoutGalileo = true;

	public bool FoldoutAmbient = true;

	public SetupAdvancedFoliageShader afs;

	private float OCCLUSION_MULT = 1f;

	private float SHADOW_BASE_DISTANCE = 15f;

	private float startShadowCoolDown;

	public float DebugShadowDist;

	private float windIntensity;

	public float ShapeIntensity = 5f;

	public float ShapeSize = 4000f;

	public float ShapeHardness = 5f;

	public Color DayColor = Color.white;

	public Color DayColorEq = Color.white;

	public Color DayColorGround = Color.white;

	public Color SunsetColor = Color.black;

	public Color SunsetColorEq = Color.black;

	public Color SunsetColorGround = Color.black;

	public Color NightColor = Color.blue;

	public Color NightColorEq = Color.blue;

	public Color NightColorGround = Color.blue;

	public Color NightAltColor = Color.blue;

	public Color CaveSkyColor = Color.black;

	public Color CaveEquatorColor = Color.black;

	public Color CaveGroundColor = Color.black;

	[Range(0f, 2f)]
	public float CaveAmbientIntensity = 0.5f;

	public Color DayBounceColor = Color.black;

	public Color SunsetBounceColor = Color.black;

	public Color NightBounceColor = Color.black;

	[Range(0f, 2f)]
	public float DayAmbientIntensity = 0.5f;

	[Range(0f, 2f)]
	public float DaySunIntensity = 0.5f;

	[Range(0f, 2f)]
	public float DaySunBounceIntensity = 0.5f;

	[Range(0f, 2f)]
	public float SunsetAmbientIntensity = 0.5f;

	[Range(0f, 2f)]
	public float SunsetSunIntensity = 0.5f;

	[Range(0f, 2f)]
	public float SunsetSunBounceIntensity = 0.5f;

	[Range(0f, 2f)]
	public float NightAmbientIntensity = 0.5f;

	[Range(0f, 2f)]
	public float NightSunIntensity = 0.5f;

	[Range(0f, 2f)]
	public float NightSunBounceIntensity = 0.5f;

	public SphericalHarmonicsL2 AmbientSHLighting;

	public Color temp_SkyColor;

	public Color temp_EquatorColor;

	public Color temp_GroundColor;

	public Color temp_AmbientSunColor;

	public Color temp_BounceColor;

	public float temp_AmbientIntensity;

	public float temp_SunIntensity;

	public float temp_SunBounceIntensity;

	public Color NoonAddLight1 = Color.black;

	public Color NoonAddLight2 = Color.black;

	public Color SunsetAddLight1 = Color.black;

	public Color SunsetAddLight2 = Color.black;

	public Color NightAddLight1 = Color.black;

	public Color NightAddLight2 = Color.black;

	public Color CaveAddLight1 = Color.black;

	public Color CaveAddLight2 = Color.black;

	public Vector3 NoonAddLight1Dir = new Vector3(0f, 1f, 0f);

	public Vector3 NoonAddLight2Dir = new Vector3(0f, 1f, 0f);

	public Vector3 SunsetAddLight1Dir = new Vector3(0f, 1f, 0f);

	public Vector3 SunsetAddLight2Dir = new Vector3(0f, 1f, 0f);

	public Vector3 NightAddLight1Dir = new Vector3(0f, 1f, 0f);

	public Vector3 NightAddLight2Dir = new Vector3(0f, 1f, 0f);

	public Vector3 CaveAddLight1Dir = new Vector3(0f, 1f, 0f);

	public Vector3 CaveAddLight2Dir = new Vector3(0f, 1f, 0f);

	public float NoonAddLight1Intensity;

	public float NoonAddLight2Intensity;

	public float SunsetAddLight1Intensity;

	public float SunsetAddLight2Intensity;

	public float NightAddLight1Intensity;

	public float NightAddLight2Intensity;

	public float CaveAddLight1Intensity;

	public float CaveAddLight2Intensity;

	public Color temp_AddLight1 = Color.black;

	public Color temp_AddLight2 = Color.black;

	public Vector3 temp_AddLight1Dir;

	public Vector3 temp_AddLight2Dir;

	public float Lx;

	public float Ly;

	public float Lz;

	public float LdotUp;

	public float LdotDown;

	public float SunLightIntensity = 1f;

	public float MoonLightIntensity = 0.2f;

	public Color NoonLightColor = Color.white;

	public Color SunsetLightColor = Color.white;

	public Color SunsetLightAltColor = Color.white;

	public Color CurrentSunsetLightColor = Color.white;

	public GameObject AtmosphereMesh;

	public float RotationSpeed = 45f;

	[SerializeThis]
	public float TimeOfDay;

	private float DelayedTimeOfDay;

	private float DelayedTimeOfDayAlpha;

	private float MovementStartTimeOfDay;

	private bool CatchUpTimeOfDay;

	public double DeltaTimeOfDay;

	public static float GameTimeScale = 1f;

	public bool ForceSunRotationUpdate;

	private float ForcedSunRotationDelay = 30f;

	private float ForcedSunRotationDuration = 24f;

	private float NextForcedSunRotation;

	private Vector3 PrevCameraForward;

	public bool Animate;

	public float ScatterIntensity = 0.65f;

	public float ScatterExaggeration = 0.1f;

	public Color CurrentFogColor;

	public Color NightFogColor;

	public Color CurrentAverageSkyColor;

	private float CAMERA_NEAR = 0.5f;

	private float CAMERA_FAR = 50f;

	private float CAMERA_FOV = 60f;

	private float CAMERA_ASPECT_RATIO = 1.333333f;

	public Texture2D Inscattering;

	public RenderTexture RTT_Inscattering;

	private RenderTextureFormat RTT_Format;

	public float FogMinHeight;

	public float FogMaxHeight = 250f;

	public float FogStartDistance;

	public float Visibility = 100f;

	public float InscatteringIntensity = 1f;

	public float InscatteringDistribution = 0.5f;

	public float Vanishing = 1f;

	public bool ShowInfo;

	private float cutoffAngle = 1.61107683f;

	private float steepness = 1.5f;

	private float sunE;

	private const float n = 1.0003f;

	private const float pn = 0.035f;

	public bool overrideVisibility;

	private Clock clock;

	public float DayCloudAlphaValue = 0.32f;

	public float NightCloudAlphaValue = 0.01f;

	private float smoothTargetCloudAlpha;

	private bool _wasNoon;

	private bool _wasDaytime;

	private float caveAmt;

	private bool sunPopHack;

	private float sunPopIntensityHack;

	private Color sunPopColorHack = Color.black;

	private bool moonPopHack;

	private float moonPopIntensityHack;

	private Color moonPopColorHack = Color.black;

	private float atmosphereGain = 1f;

	public float LightningIntensity = 5f;

	private bool playerIsInSnowArea;

	private float windBase;

	private float windBaseTarget;

	private float windBaseDelta;

	private float windBaseHoldTimer;

	private float windGust;

	private float windGustTarget;

	private float windGustDelta;

	private float windGustIntervalTimer;

	private const float ONE_MINUTE = 60f;

	private const float WIND_BASE_MINIMUM = 0f;

	private const float WIND_BASE_MAXIMUM = 0.4f;

	private const float WIND_BASE_RAMP_TIME_MINIMUM = 60f;

	private const float WIND_BASE_RAMP_TIME_MAXIMUM = 120f;

	private const float WIND_BASE_HOLD_TIME_MINIMUM = 60f;

	private const float WIND_BASE_HOLD_TIME_MAXIMUM = 600f;

	private const float WIND_GUST_MINIMUM = 0.2f;

	private const float WIND_GUST_MAXIMUM = 0.7f;

	private const float WIND_GUST_RAMP_TIME_MINIMUM = 5f;

	private const float WIND_GUST_RAMP_TIME_MAXIMUM = 30f;

	private const float WIND_GUST_INTERVAL_MINIMUM = 15f;

	private const float WIND_GUST_INTERVAL_MAXIMUM = 120f;

	private const float WIND_HEIGHT_MIN = 150f;

	private const float WIND_HEIGHT_MAX = 300f;

	private const float WIND_MAXHEIGHT_RATIO = 2.5f;

	public float debugWindIntensity;

	public bool debugWind;

	private bool showDebug;
}
