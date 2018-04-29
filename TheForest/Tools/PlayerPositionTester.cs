using System;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Tools
{
	
	public class PlayerPositionTester : MonoBehaviour
	{
		
		public void DoPositionningTest()
		{
			switch (this._test)
			{
			case PlayerPositionTester.Tests.Proximity:
				this.ProximityTest();
				break;
			case PlayerPositionTester.Tests.InFront:
				this.InFrontTest();
				break;
			case PlayerPositionTester.Tests.Behind:
				this.BehindTest();
				break;
			}
		}

		
		private void ProximityTest()
		{
			Transform transform = (!this._playerTrOverride) ? LocalPlayer.Transform : this._playerTrOverride;
			if (Vector3.Distance(transform.position, this._testTarget.position) < this._proximityRange)
			{
				this._callback.Invoke();
			}
		}

		
		private void InFrontTest()
		{
			if (this.Dot() > 0f)
			{
				this._callback.Invoke();
			}
		}

		
		private void BehindTest()
		{
			if (this.Dot() < 0f)
			{
				this._callback.Invoke();
			}
		}

		
		private float Dot()
		{
			Transform transform = (!this._playerTrOverride) ? LocalPlayer.Transform : this._playerTrOverride;
			return Vector3.Dot(this._testTarget.forward, transform.position - this._testTarget.position);
		}

		
		public PlayerPositionTester.Tests _test;

		
		public float _proximityRange = 0.5f;

		
		public Transform _playerTrOverride;

		
		public Transform _testTarget;

		
		public UnityEvent _callback;

		
		public enum Tests
		{
			
			Proximity,
			
			InFront,
			
			Behind
		}
	}
}
