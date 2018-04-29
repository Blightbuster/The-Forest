using System;
using UnityEngine;

namespace TheForest.UI
{
	
	[AddComponentMenu("UI/Numeric Option Value Label")]
	public class NumericOptionValueLabel : MonoBehaviour
	{
		
		private void Reset()
		{
			this._label = base.GetComponent<UILabel>();
		}

		
		private void LateUpdate()
		{
			switch (this._option)
			{
			case NumericOptionValueLabel.Option.FoV:
				this.ShowFloat(PlayerPreferences.Fov);
				break;
			case NumericOptionValueLabel.Option.SfxVolume:
				this.ShowFloat(PlayerPreferences.Volume);
				break;
			case NumericOptionValueLabel.Option.MusicVolume:
				this.ShowFloat(PlayerPreferences.MusicVolume);
				break;
			case NumericOptionValueLabel.Option.MouseSensitivityX:
				this.ShowFloat(PlayerPreferences.MouseSensitivityX);
				break;
			case NumericOptionValueLabel.Option.GhostOpacity:
				this.ShowFloat(PlayerPreferences.GhostTintOpacity);
				break;
			case NumericOptionValueLabel.Option.MouseSensitivityY:
				this.ShowFloat(PlayerPreferences.MouseSensitivityY);
				break;
			case NumericOptionValueLabel.Option.MouseSmoothing:
				this.ShowFloat(PlayerPreferences.MouseSmoothing);
				break;
			case NumericOptionValueLabel.Option.GammaWorld:
				this.ShowFloat(PlayerPreferences.GammaWorldAndDay);
				break;
			case NumericOptionValueLabel.Option.GammaCaves:
				this.ShowFloat(PlayerPreferences.GammaCavesAndNight);
				break;
			case NumericOptionValueLabel.Option.MicrophoneVolume:
				this.ShowFloat(PlayerPreferences.MicrophoneVolume);
				break;
			}
		}

		
		private void ShowFloat(float value)
		{
			if (!Mathf.Approximately(value, this._displayedValue))
			{
				this._displayedValue = value;
				switch (this._format)
				{
				case NumericOptionValueLabel.DisplayFormat.Raw:
					this._label.text = value.ToString();
					break;
				case NumericOptionValueLabel.DisplayFormat.Rounded:
					this._label.text = string.Format("{0:0}", value);
					break;
				case NumericOptionValueLabel.DisplayFormat.Percent:
					this._label.text = string.Format("{0:P0}", value);
					break;
				case NumericOptionValueLabel.DisplayFormat.Rounded2:
					this._label.text = string.Format("{0:0.00}", value);
					break;
				}
			}
		}

		
		public UILabel _label;

		
		public NumericOptionValueLabel.Option _option;

		
		public NumericOptionValueLabel.DisplayFormat _format;

		
		private float _displayedValue = -1f;

		
		public enum Option
		{
			
			FoV,
			
			SfxVolume,
			
			MusicVolume,
			
			MouseSensitivityX,
			
			GhostOpacity,
			
			MouseSensitivityY,
			
			MouseSmoothing,
			
			GammaWorld,
			
			GammaCaves,
			
			MicrophoneVolume
		}

		
		public enum DisplayFormat
		{
			
			Raw,
			
			Rounded,
			
			Percent,
			
			Rounded2
		}
	}
}
