using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace TheForest.Player
{
	public class PassengerDatabase : ScriptableObject
	{
		public static PassengerDatabase Instance { get; private set; }

		public void OnEnable()
		{
			base.hideFlags = HideFlags.None;
			if (PassengerDatabase.Instance == null)
			{
				PassengerDatabase.Instance = this;
				this._passengersCache = this._passengers.ToDictionary((Passenger p) => p._id);
			}
		}

		public int GetPassengerNum(int passengerId)
		{
			return passengerId - 1;
		}

		public List<Passenger> _passengers = new List<Passenger>();

		[HideInInspector]
		public int _passengersZonesMaxID;

		private Dictionary<int, Passenger> _passengersCache;
	}
}
