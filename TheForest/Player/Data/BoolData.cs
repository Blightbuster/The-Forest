using System;
using UnityEngine;

namespace TheForest.Player.Data
{
	
	public class BoolData : MonoBehaviour
	{
		
		public void SetValue(bool value)
		{
			this._value = value;
		}

		
		public static implicit operator bool(BoolData bd)
		{
			return bd._value;
		}

		
		[SerializeThis]
		private bool _value;
	}
}
