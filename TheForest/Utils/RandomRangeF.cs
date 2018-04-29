using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	[Serializable]
	public class RandomRangeF
	{
		
		public RandomRangeF(float min = 0f, float max = 0f)
		{
			this._min = min;
			this._max = max;
		}

		
		public static implicit operator float(RandomRangeF rr)
		{
			return (rr._min >= rr._max) ? rr._min : UnityEngine.Random.Range(rr._min, rr._max);
		}

		
		
		public float Average
		{
			get
			{
				return Mathf.Lerp(this._min, this._max, 0.5f);
			}
		}

		
		public override string ToString()
		{
			return (this._min == this._max) ? this._max.ToString() : (this._min + " to " + this._max);
		}

		
		public float _min;

		
		public float _max;
	}
}
