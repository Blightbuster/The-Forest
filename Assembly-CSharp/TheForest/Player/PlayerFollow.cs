using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Player
{
	public class PlayerFollow : MonoBehaviour
	{
		private void Awake()
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				base.enabled = false;
			}
		}

		private void OnEnable()
		{
			if (this._onEnable)
			{
				this.UpdatePosition();
			}
		}

		private void LateUpdate()
		{
			if (!this._onEnable)
			{
				this.UpdatePosition();
			}
		}

		private void UpdatePosition()
		{
			if (this._useVrCamera)
			{
				Vector3 a = LocalPlayer.vrPlayerControl.VRCamera.forward;
				a.y = 0f;
				a = a.normalized;
				base.transform.position = LocalPlayer.Transform.position + a * this._forwardOffset.z;
				if (this._matchRotation)
				{
					base.transform.rotation = LocalPlayer.vrPlayerControl.VRCamera.rotation;
					base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
				}
			}
			else if (this._useVrWatch)
			{
				base.transform.position = LocalPlayer.WatchTrVR.position;
				base.transform.localPosition += this._forwardOffset;
				if (this._matchRotation)
				{
					base.transform.rotation = LocalPlayer.WatchTrVR.rotation;
				}
			}
			else if (LocalPlayer.Transform)
			{
				base.transform.position = LocalPlayer.Transform.position + LocalPlayer.Transform.rotation * this._forwardOffset;
				if (this._matchRotation)
				{
					base.transform.rotation = LocalPlayer.Transform.rotation;
				}
			}
		}

		public Vector3 _forwardOffset;

		public bool _matchRotation;

		public bool _useVrCamera;

		public bool _useVrWatch;

		public bool _onEnable;
	}
}
