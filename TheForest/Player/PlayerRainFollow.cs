using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	
	public class PlayerRainFollow : MonoBehaviour
	{
		
		private void Awake()
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				base.enabled = false;
			}
		}

		
		private void LateUpdate()
		{
			if (!Scene.RainTypes)
			{
				return;
			}
			if (LocalPlayer.Transform && (Scene.WeatherSystem.Raining || Scene.WeatherSystem.UsingSnow || Scene.RainTypes.bubbles.activeSelf) && Time.deltaTime > 0f)
			{
				if (!this._target)
				{
					this._target = LocalPlayer.Transform.Find("RainTargetGo");
					this._lastYRot = LocalPlayer.Rigidbody.rotation.eulerAngles.y;
					if (!this._target)
					{
						return;
					}
				}
				float num = LocalPlayer.Rigidbody.velocity.magnitude;
				if (float.IsNaN(num))
				{
					num = 0f;
				}
				if (Vector3.Dot(LocalPlayer.Rigidbody.velocity, LocalPlayer.Transform.forward) < 0f)
				{
					num = (num + this._target.localPosition.z / 2f) * -1f;
				}
				this._velocityOffset = num * this._velocityOffsetRatio;
				this._newYRot = LocalPlayer.Rigidbody.rotation.eulerAngles.y;
				this._angularVelocity = (this._newYRot - this._lastYRot) / Time.deltaTime;
				this._lastYRot = this._newYRot;
				this._angularVelocityDir = LocalPlayer.Transform.forward.RotateY(this._angularVelocity * this._angularVelocityOffsetRatio);
				base.transform.position = LocalPlayer.Rigidbody.position + this._angularVelocityDir * (this._velocityOffset + this._target.localPosition.z);
				base.transform.rotation = LocalPlayer.Rigidbody.rotation;
			}
		}

		
		public float _velocityOffsetRatio = 2f;

		
		public float _angularVelocityOffsetRatio = 2f;

		
		private Transform _target;

		
		private float _newYRot;

		
		private float _lastYRot;

		
		private float _angularVelocity;

		
		private float _velocityOffset;

		
		private Vector3 _angularVelocityDir;
	}
}
