using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.World
{
	public class SwitchLightGroupTrigger : MonoBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
		}

		private void Update()
		{
			switch (this._state)
			{
			case SwitchLightGroupTrigger.States.Off:
				if (TheForest.Utils.Input.GetButtonDown("Take") || this._turnOnTest)
				{
					this._turnOnTest = false;
					this._state = SwitchLightGroupTrigger.States.TurningOn;
					this.ToggleIcons(false, false);
				}
				break;
			case SwitchLightGroupTrigger.States.On:
				if (TheForest.Utils.Input.GetButtonDown("Take") || this._turnOffTest)
				{
					this._turnOffTest = false;
					this._state = SwitchLightGroupTrigger.States.TurningOff;
					this.ToggleIcons(false, false);
				}
				break;
			case SwitchLightGroupTrigger.States.TurningOff:
			case SwitchLightGroupTrigger.States.TurningOn:
				this.BatchLights();
				if (this._currentLight >= this._lights.Length)
				{
					this._currentLight = 0;
					this._state = ((this._state != SwitchLightGroupTrigger.States.TurningOn) ? SwitchLightGroupTrigger.States.Off : SwitchLightGroupTrigger.States.On);
					this.ToggleIcons(!this._grabbed, this._grabbed);
					base.enabled = this._grabbed;
				}
				break;
			}
		}

		private void GrabEnter()
		{
			base.enabled = true;
			this._grabbed = true;
			if (this._state <= SwitchLightGroupTrigger.States.On)
			{
				this.ToggleIcons(false, true);
			}
		}

		private void GrabExit()
		{
			base.enabled = false;
			this._grabbed = false;
			if (this._state <= SwitchLightGroupTrigger.States.On)
			{
				this.ToggleIcons(true, false);
			}
		}

		private void ToggleIcons(bool showSheen, bool showPickup)
		{
			if (this._sheenIcon)
			{
				this._sheenIcon.SetActive(showSheen);
			}
			if (this._pickupIcon)
			{
				this._pickupIcon.SetActive(showPickup);
			}
		}

		private void BatchLights()
		{
			if (this._nextBatchTime <= Time.time && this._currentLight < this._lights.Length)
			{
				this._nextBatchTime = Time.time + this._batchInterval;
				bool flag = this._state == SwitchLightGroupTrigger.States.TurningOn;
				int num = Mathf.Min(this._currentLight + Mathf.Max((this._batchInterval != 0f) ? 1 : int.MaxValue, this._lightsPerBatch), this._lights.Length);
				for (int i = this._currentLight; i < num; i++)
				{
					if (this._lights[i]._rootGo && this._lights[i]._rootGo.activeSelf != flag)
					{
						this._lights[i]._rootGo.SetActive(flag);
					}
					if (this._lights[i]._light)
					{
						this._lights[i]._light.enabled = flag;
					}
					if (this._lights[i]._renderer)
					{
						this._lights[i]._renderer.sharedMaterial = ((!flag) ? this._turnedOffMat : this._turnedOnMat);
					}
				}
				this._currentLight += ((this._lightsPerBatch != 0) ? this._lightsPerBatch : this._lights.Length);
			}
		}

		public SwitchLightGroupTrigger.LightObject[] _lights;

		public Material _turnedOnMat;

		public Material _turnedOffMat;

		public float _batchInterval;

		public int _lightsPerBatch;

		public GameObject _sheenIcon;

		public GameObject _pickupIcon;

		[Header("Testing")]
		public bool _turnOnTest;

		public bool _turnOffTest;

		private float _nextBatchTime;

		private bool _grabbed;

		private int _currentLight;

		private SwitchLightGroupTrigger.States _state;

		[Serializable]
		public class LightObject
		{
			public GameObject _rootGo;

			public Light _light;

			public Renderer _renderer;
		}

		public enum States
		{
			Off,
			On,
			TurningOff,
			TurningOn
		}
	}
}
