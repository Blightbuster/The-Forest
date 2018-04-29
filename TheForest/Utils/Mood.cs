using System;
using Rewired;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class Mood : MonoBehaviour
	{
		
		private void Awake()
		{
			Mood.Instance = this;
		}

		
		private void Update()
		{
			if (this._constantVibration && Input.IsGamePad && PlayerPreferences.UseGamepadRumble)
			{
				foreach (Rewired.Joystick joystick in Input.player.controllers.Joysticks)
				{
					if (joystick.supportsVibration)
					{
						joystick.SetVibration(this._leftMotorForce, this._rightMotorForce);
					}
				}
				this._constantVibrationCurrent = true;
			}
			else if (this._constantVibrationCurrent)
			{
				this._constantVibrationCurrent = false;
				this.TurnOffVibration();
			}
			if (this._stopFlashTime != 0f && this._stopFlashTime <= Time.realtimeSinceStartup && Input.DS4 != null)
			{
				Input.DS4.StopLightFlash();
			}
		}

		
		private void OnDestroy()
		{
			if (Mood.Instance == this)
			{
				Mood.Instance = null;
			}
		}

		
		public static void HitRumble()
		{
			if (!LocalPlayer.Inventory.IsRightHandEmpty())
			{
				Mood.OneShotVibratation1f(LocalPlayer.Inventory.RightHand.ItemCache._hitRumbleStrength, LocalPlayer.Inventory.RightHand.ItemCache._hitRumbleDuration);
			}
		}

		
		public static void OneShotVibratation1f(float force, float duration = 0.1f)
		{
			Mood.OneShotVibratation2f(force, force, duration);
		}

		
		public static void OneShotVibratation2f(float leftForce, float rightForce, float duration = 0.1f)
		{
			if (Input.IsGamePad && PlayerPreferences.UseGamepadRumble && !Mood.Instance._constantVibration)
			{
				foreach (Rewired.Joystick joystick in Input.player.controllers.Joysticks)
				{
					if (joystick.supportsVibration)
					{
						joystick.SetVibration(leftForce, rightForce, duration, duration);
					}
				}
			}
		}

		
		public static void BeginConstantVibratation(float leftForce, float rightForce)
		{
			Mood.Instance._leftMotorForce = leftForce;
			Mood.Instance._rightMotorForce = rightForce;
			Mood.Instance._constantVibration = true;
		}

		
		public static void StopConstantVibratation()
		{
			Mood.Instance._constantVibration = false;
		}

		
		public static void PlayerGetHit(float damage)
		{
			Mood.OneShotVibratation1f(LocalPlayer.Stats.HitRumbleStrength * (damage / 100f), LocalPlayer.Stats.HitRumbleDuration);
			if (Input.DS4 != null)
			{
				Input.DS4.SetLightColor(Color.red);
				Input.DS4.SetLightFlash(1f, 1f);
				Mood.Instance._stopFlashTime = Time.realtimeSinceStartup + 1f;
			}
		}

		
		public static void PlayerLowHealth(bool on)
		{
			if (Input.DS4 != null)
			{
				if (on)
				{
					Input.DS4.SetLightColor(Color.red);
					Input.DS4.SetLightFlash(0.5f, 1f);
				}
				else
				{
					Input.DS4.StopLightFlash();
				}
			}
		}

		
		public static void PlayerGetPoisonned(bool on)
		{
			if (Input.DS4 != null)
			{
				Input.DS4.SetLightColor((!on) ? Color.blue : Color.green);
			}
		}

		
		private void TurnOffVibration()
		{
			foreach (Rewired.Joystick joystick in Input.player.controllers.Joysticks)
			{
				if (joystick.supportsVibration)
				{
					joystick.StopVibration();
				}
			}
		}

		
		public float _leftMotorForce;

		
		public float _rightMotorForce;

		
		private static Mood Instance;

		
		private bool _constantVibration;

		
		private bool _constantVibrationCurrent;

		
		private float _stopFlashTime;
	}
}
