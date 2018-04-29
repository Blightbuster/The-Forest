using System;
using System.Collections;
using Bolt;
using ModAPI;
using TheForest.Utils;
using UltimateCheatmenu;
using UnityEngine;

namespace TheForest.Items.World
{
	
	[AddComponentMenu("Items/World/Battery Based Light")]
	public class BatteryBasedLight : EntityBehaviour<IPlayerState>
	{
		
		private void Awake()
		{
			this.SetColor(this._torchBaseColor);
		}

		
		private void OnEnable()
		{
			if (!BoltNetwork.isRunning || (BoltNetwork.isRunning && this.entity && this.entity.isAttached))
			{
				if (!BoltNetwork.isRunning || this.entity.isOwner)
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
			if (LocalPlayer.Animator)
			{
				LocalPlayer.Animator.SetBool("noBattery", false);
			}
			this._animCoolDown = 0f;
			base.CancelInvoke("resetBatteryBool");
			this._doingStash = false;
			base.StopAllCoroutines();
		}

		
		private void __Update__Original()
		{
			if (!BoltNetwork.isRunning || (BoltNetwork.isRunning && this.entity && this.entity.isAttached && this.entity.isOwner))
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
			TheForestQualitySettings.UserSettings.ApplyQualitySetting(this._mainLight, LightShadows.Hard);
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

		
		private void Update()
		{
			try
			{
				if (!BoltNetwork.isRunning || (BoltNetwork.isRunning && base.entity.isAttached && base.entity.isOwner))
				{
					if (UCheatmenu.TorchToggle)
					{
						LocalPlayer.Stats.BatteryCharge = 100f;
						Color color = new Color((float)UCheatmenu.TorchR, (float)UCheatmenu.TorchG, (float)UCheatmenu.TorchB);
						if (!BoltNetwork.isRunning || (BoltNetwork.isRunning && base.entity.isAttached && base.entity.isOwner))
						{
							this.SetIntensity(Convert.ToSingle(UCheatmenu.TorchI));
							this.SetColor(color);
							if (BoltNetwork.isRunning)
							{
								base.state.BatteryTorchIntensity = UCheatmenu.TorchI;
								base.state.BatteryTorchEnabled = this._mainLight.enabled;
								base.state.BatteryTorchColor = color;
								return;
							}
						}
					}
					else
					{
						Color color2 = new Color(1f, 1f, 1f);
						this.SetColor(color2);
						LocalPlayer.Stats.BatteryCharge -= this._batterieCostPerSecond * Time.deltaTime;
						if (LocalPlayer.Stats.BatteryCharge > 50f)
						{
							this.SetIntensity(0.45f);
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
										if (this._skipNoBatteryRoutine)
										{
											this.SetEnabled(false);
										}
										else
										{
											if (!this._doingStash)
											{
												base.StartCoroutine("stashNoBatteryRoutine");
											}
											this._doingStash = true;
										}
									}
									else
									{
										this.TorchLowerLightEvenMore();
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
				}
			}
			catch (Exception ex)
			{
				Log.Write("Exception thrown: " + ex.ToString(), "UltimateCheatmenu");
				this.__Update__Original();
			}
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

		
		private float _animCoolDown;

		
		private float _boolResetTimer;

		
		private bool _doingStash;
	}
}
