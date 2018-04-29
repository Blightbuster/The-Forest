using System;
using TheForest.UI;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class ActionTriggerEvent : MonoBehaviour
	{
		
		private void Awake()
		{
			this._actionName = this._action.ToString();
		}

		
		public void OnEnable()
		{
			if (!this._noIcon && this._action != InputMappingIcons.Actions.None && (!this._gamepadOnlyIcon || Input.IsGamePad))
			{
				ActionIconSystem.RegisterIcon(base.transform, this._action, this._sideIcon, ActionIconSystem.CurrentViewOptions.AllowInBook, false, false);
			}
		}

		
		public void OnDisable()
		{
			if (this._action != InputMappingIcons.Actions.None)
			{
				ActionIconSystem.UnregisterIcon(base.transform, false, false);
			}
		}

		
		private void Update()
		{
			if (!this._gamepadOnly || Input.IsGamePad)
			{
				if (this._isAxis)
				{
					float num = Input.GetAxis(this._actionName);
					if (Mathf.Abs(num) > this._axisDeadZone)
					{
						num *= this._axisSpeed;
						if (this._vector2AxisX)
						{
							if (this._targetOverride)
							{
								this._targetOverride.SendMessage(this._event, new Vector2(num, 0f), SendMessageOptions.DontRequireReceiver);
							}
							else
							{
								base.SendMessage(this._event, new Vector2(num, 0f), SendMessageOptions.DontRequireReceiver);
							}
						}
						else if (this._vector2AxisY)
						{
							if (this._targetOverride)
							{
								this._targetOverride.SendMessage(this._event, new Vector2(0f, num), SendMessageOptions.DontRequireReceiver);
							}
							else
							{
								base.SendMessage(this._event, new Vector2(0f, num), SendMessageOptions.DontRequireReceiver);
							}
						}
						else if (this._targetOverride)
						{
							this._targetOverride.SendMessage(this._event, num, SendMessageOptions.DontRequireReceiver);
						}
						else
						{
							base.SendMessage(this._event, num, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
				else if (Input.GetButtonDown(this._actionName))
				{
					this.SendButton();
				}
			}
		}

		
		public void SendButton()
		{
			if (this._targetOverride)
			{
				this._targetOverride.SendMessage(this._event, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.SendMessage(this._event, SendMessageOptions.DontRequireReceiver);
			}
		}

		
		public GameObject _targetOverride;

		
		public InputMappingIcons.Actions _action;

		
		public ActionIcon.SideIconTypes _sideIcon;

		
		public string _event = "OnClick";

		
		public bool _gamepadOnly;

		
		public bool _gamepadOnlyIcon = true;

		
		public bool _noIcon;

		
		public bool _isAxis;

		
		public bool _vector2AxisX;

		
		public bool _vector2AxisY;

		
		public float _axisSpeed = 0.1f;

		
		public float _axisDeadZone = 0.05f;

		
		private string _actionName;
	}
}
