using System;
using System.Collections;
using Bolt;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Items.World
{
	[AddComponentMenu("Items/World/Battery Based Light")]
	public class BatteryBasedLight : EntityBehaviour<IPlayerState>
	{
		private void Awake()
		{
			this._vis = base.transform.root.GetComponent<netPlayerVis>();
			this.SetColor(this._torchBaseColor);
		}

		private void OnEnable()
		{
			if (!BoltNetwork.isRunning || (BoltNetwork.isRunning && base.entity && base.entity.isAttached))
			{
				if (!BoltNetwork.isRunning || base.entity.isOwner)
				{
					base.StartCoroutine(this.DelayedLightOn());
					this._doingStash = false;
				}
				else
				{
					this.SetEnabled(base.state.BatteryTorchEnabled);
				}
			}
		}

		private void OnDisable()
		{
			this.SetEnabled(false);
			if (this._manageDynamicShadows)
			{
				if (CoopPeerStarter.DedicatedHost || LocalPlayer.Transform == null || Scene.SceneTracker == null)
				{
					return;
				}
				if (Scene.SceneTracker.activePlayerLights.Contains(base.gameObject))
				{
					Scene.SceneTracker.activePlayerLights.Remove(base.gameObject);
				}
			}
			if (LocalPlayer.Animator)
			{
				LocalPlayer.Animator.SetBool("noBattery", false);
			}
			this._animCoolDown = 0f;
			base.CancelInvoke("resetBatteryBool");
			this._doingStash = false;
			base.StopAllCoroutines();
		}

		private void Update()
		{
			if (!BoltNetwork.isRunning || (BoltNetwork.isRunning && base.entity && base.entity.isAttached && base.entity.isOwner))
			{
				LocalPlayer.Stats.BatteryCharge -= this._batterieCostPerSecond * Time.deltaTime;
				if (LocalPlayer.Stats.BatteryCharge > 50f)
				{
					this.SetIntensity(this._highBatteryIntensity);
				}
				else if (LocalPlayer.Stats.BatteryCharge < 20f)
				{
					if (LocalPlayer.Stats.BatteryCharge < 10f)
					{
						if (LocalPlayer.Stats.BatteryCharge < 5f)
						{
							if (LocalPlayer.Stats.BatteryCharge < 3f && Time.time > this._animCoolDown && !this._skipNoBatteryRoutine)
							{
								LocalPlayer.Animator.SetBool("noBattery", true);
								this._animCoolDown = Time.time + (float)UnityEngine.Random.Range(30, 60);
								base.Invoke("resetBatteryBool", 1.5f);
							}
							if (LocalPlayer.Stats.BatteryCharge <= 0f)
							{
								LocalPlayer.Stats.BatteryCharge = 0f;
								if (this._skipNoBatteryRoutine)
								{
									this.SetEnabled(false);
								}
								else
								{
									this.TorchLowerLightEvenMore();
									if (!this._doingStash)
									{
										base.StartCoroutine("stashNoBatteryRoutine");
									}
									this._doingStash = true;
								}
							}
							else
							{
								this.SetEnabled(true);
							}
						}
						else
						{
							this.TorchLowerLightMore();
							this.SetEnabled(true);
						}
					}
					else
					{
						this.TorchLowerLight();
						this.SetEnabled(true);
					}
				}
				if (BoltNetwork.isRunning)
				{
					base.state.BatteryTorchIntensity = this._mainLight.intensity;
					base.state.BatteryTorchEnabled = this._mainLight.enabled;
					base.state.BatteryTorchColor = this._mainLight.color;
				}
			}
			if (BoltNetwork.isRunning && base.entity && base.entity.isAttached && !base.entity.isOwner)
			{
				this.SetEnabled(base.state.BatteryTorchEnabled);
			}
			if (ForestVR.Enabled)
			{
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._mainLight, LightShadows.None);
			}
			else if (this._manageDynamicShadows)
			{
				this.manageShadows();
			}
			else
			{
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._mainLight, LightShadows.Hard);
			}
		}

		private IEnumerator DelayedLightOn()
		{
			this.SetEnabled(false);
			yield return new WaitForSeconds(this._delayBeforeLight);
			this.SetEnabled(true);
			yield break;
		}

		private void GotBloody()
		{
			this.SetColor(this._torchBloodyColor);
		}

		private void GotClean()
		{
			this.SetColor(this._torchBaseColor);
		}

		private void manageShadows()
		{
			if (CoopPeerStarter.DedicatedHost || LocalPlayer.Transform == null || Scene.SceneTracker == null)
			{
				return;
			}
			if ((Scene.SceneTracker.activePlayerLights.Count < 3 || Scene.SceneTracker.activePlayerLights.Contains(base.gameObject)) && this._vis.localplayerDist < 60f)
			{
				if (!Scene.SceneTracker.activePlayerLights.Contains(base.gameObject))
				{
					Scene.SceneTracker.activePlayerLights.Add(base.gameObject);
				}
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._mainLight, LightShadows.Hard);
			}
			else
			{
				if (Scene.SceneTracker.activePlayerLights.Contains(base.gameObject))
				{
					Scene.SceneTracker.activePlayerLights.Remove(base.gameObject);
				}
				TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._mainLight, LightShadows.None);
			}
		}

		private void TorchLowerLight()
		{
			this.SetIntensity(this._lowBatteryIntensity1);
		}

		private void TorchLowerLightMore()
		{
			this.SetIntensity(this._lowBatteryIntensity2);
		}

		private void TorchLowerLightEvenMore()
		{
			this.SetIntensity(this._lowBatteryIntensity3);
		}

		public void SetEnabled(bool enabled)
		{
			this._mainLight.enabled = enabled;
			if (this._fillLight)
			{
				this._fillLight.enabled = enabled;
			}
		}

		public void SetIntensity(float intensity)
		{
			this._mainLight.intensity = intensity;
			float num = intensity / 2f;
			if (intensity < 0.3f)
			{
				num = intensity / 3f;
			}
			if (num > 0.5f)
			{
				num = 0.5f;
			}
			if (this._fillLight)
			{
				this._fillLight.intensity = num;
			}
		}

		public void SetColor(Color color)
		{
			this._mainLight.color = color;
		}

		private void resetBatteryBool()
		{
			LocalPlayer.Animator.SetBool("noBattery", false);
		}

		private IEnumerator stashNoBatteryRoutine()
		{
			float clamp = 1f;
			RandomRangeF _offIntensity = new RandomRangeF(0.3f, 0.5f);
			float t = 0f;
			while (t < 1f)
			{
				_offIntensity = new RandomRangeF(0.3f, 0.5f);
				float result = _offIntensity;
				result *= clamp;
				this.SetIntensity(result);
				clamp = Mathf.Lerp(1f, 0f, t);
				t += Time.deltaTime / 2f;
				yield return null;
			}
			LocalPlayer.Inventory.StashLeftHand();
			yield break;
		}

		public Light _mainLight;

		public Light _fillLight;

		public Color _torchBaseColor;

		public Color _torchBloodyColor;

		public float _batterieCostPerSecond = 0.2f;

		public float _delayBeforeLight = 0.5f;

		public float _highBatteryIntensity = 0.45f;

		public RandomRangeF _lowBatteryIntensity1 = new RandomRangeF(0.4f, 0.3f);

		public RandomRangeF _lowBatteryIntensity2 = new RandomRangeF(0.35f, 0.2f);

		public RandomRangeF _lowBatteryIntensity3 = new RandomRangeF(0.1f, 0.01f);

		public bool _skipNoBatteryRoutine;

		public bool _manageDynamicShadows;

		private netPlayerVis _vis;

		private float _animCoolDown;

		private float _boolResetTimer;

		private bool _doingStash;
	}
}
