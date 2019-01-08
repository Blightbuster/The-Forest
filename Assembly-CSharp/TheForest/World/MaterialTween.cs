using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.World
{
	public class MaterialTween : MonoBehaviour
	{
		private void Awake()
		{
			if (this._randTimeOffset)
			{
				this._timeOffset = UnityEngine.Random.Range(0f, 1000f);
			}
		}

		private void Update()
		{
			if (this._frequency != 0f)
			{
				float t = Time.realtimeSinceStartup * this._frequency;
				this.CalculateRawOutput(t);
				foreach (MaterialTween.Output output2 in this._output)
				{
					output2.Filter(this.RawOutput);
					switch (output2._type)
					{
					case MaterialTween.PropertyTypes.Color:
						output2.MaterialSetColor();
						break;
					case MaterialTween.PropertyTypes.Float:
						output2.MaterialSetFloat();
						break;
					case MaterialTween.PropertyTypes.LightIntensity:
						output2.SetLightIntensity();
						break;
					case MaterialTween.PropertyTypes.Message:
						output2.SendMessage();
						break;
					}
				}
			}
		}

		private void OnDestroy()
		{
			foreach (MaterialTween.Output output2 in this._output)
			{
				output2.Clear();
			}
			this._output = null;
		}

		private void CalculateRawOutput(float t)
		{
			float num = this.GetRawValue(t);
			num = Mathf.Clamp(num, this._filterMin, this._filterMax) * this._amplitude + this._offset;
			this.RawOutput = num;
		}

		private float GetRawValue(float t)
		{
			switch (this._function)
			{
			case MaterialTween.FunctionTypes.Sin:
				return this.Sin(t);
			case MaterialTween.FunctionTypes.Cos:
				return this.Cos(t);
			case MaterialTween.FunctionTypes.Rand01:
				return this.Rand(t);
			case MaterialTween.FunctionTypes.Perlin:
				return this.Perlin(t);
			default:
				return t;
			}
		}

		private float Cos(float t)
		{
			return Mathf.Sin(t + this._timeOffset);
		}

		private float Sin(float t)
		{
			return Mathf.Sin(t + this._timeOffset);
		}

		private float Rand(float t)
		{
			return UnityEngine.Random.Range(0f, 1f);
		}

		private float Perlin(float t)
		{
			return Mathf.PerlinNoise(t, this._timeOffset);
		}

		public float RawOutput { get; set; }

		[Header("Raw output")]
		public MaterialTween.FunctionTypes _function;

		public float _filterMin;

		public float _filterMax = 1f;

		public float _amplitude = 1f;

		public float _offset;

		[Header("Timing")]
		public float _frequency = 1f;

		public float _timeOffset;

		public bool _randTimeOffset;

		[Space(10f)]
		public MaterialTween.Output[] _output;

		public enum PropertyTypes
		{
			Color,
			Float,
			LightIntensity,
			Message
		}

		public enum FunctionTypes
		{
			Sin,
			Cos,
			Rand01,
			Perlin
		}

		public enum ColorFields
		{
			R,
			G,
			B,
			A
		}

		[Serializable]
		public class Output
		{
			public float Filter(float value)
			{
				this.Value = value * this._amplitude + this._offset;
				return this.Value;
			}

			public void MaterialSetFloat()
			{
				if (this._block == null)
				{
					this._block = new MaterialPropertyBlock();
				}
				Renderer renderer = (!(this._target is Renderer)) ? this._target.GetComponent<Renderer>() : ((Renderer)this._target);
				renderer.GetPropertyBlock(this._block);
				this._block.SetFloat(this._property, this.Value);
				renderer.SetPropertyBlock(this._block);
			}

			public void MaterialSetColor()
			{
				if (this._block == null)
				{
					this._block = new MaterialPropertyBlock();
				}
				Color startColor = this._startColor;
				if (this._colorFields[0])
				{
					startColor.r = this.Value;
				}
				if (this._colorFields[1])
				{
					startColor.g = this.Value;
				}
				if (this._colorFields[2])
				{
					startColor.b = this.Value;
				}
				if (this._colorFields[3])
				{
					startColor.a = this.Value;
				}
				Renderer renderer = (!(this._target is Renderer)) ? this._target.GetComponent<Renderer>() : ((Renderer)this._target);
				renderer.GetPropertyBlock(this._block);
				this._block.SetColor(this._property, startColor);
				renderer.SetPropertyBlock(this._block);
			}

			public void SetLightIntensity()
			{
				Light light = (!(this._target is Light)) ? this._target.GetComponent<Light>() : ((Light)this._target);
				light.intensity = this.Value;
			}

			public void SendMessage()
			{
				this._target.SendMessage(this._message, this.Value, SendMessageOptions.DontRequireReceiver);
			}

			public void Clear()
			{
				this._target = null;
				this._block = null;
			}

			public float Value { get; set; }

			public Component _target;

			public MaterialTween.PropertyTypes _type;

			[Header("Output value")]
			public float _amplitude = 1f;

			public float _offset;

			[Header("Type: Float")]
			public string _property;

			[Header("Type: Color")]
			[NameFromEnumIndex(typeof(MaterialTween.ColorFields))]
			public bool[] _colorFields = new bool[4];

			public Color _startColor;

			[Header("Type: Message")]
			public string _message;

			private MaterialPropertyBlock _block;
		}
	}
}
