using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	public class GamepadSelectedControl : MonoBehaviour
	{
		public static GamepadSelectedControl Instance { get; private set; }

		private void Awake()
		{
			GamepadSelectedControl.Instance = this;
			this._gamepadPopupInput = (base.GetComponent<GamepadInputToSelectedPopup>() ?? base.gameObject.AddComponent<GamepadInputToSelectedPopup>());
		}

		private void Update()
		{
			if (TheForest.Utils.Input.IsGamePad && !VirtualCursor.Instance.OverridingPosition && !ForestVR.Enabled)
			{
				if (UICamera.controller != null)
				{
					if (!UICamera.controller.current && !this._currentTarget && this._lastFirstSelectControl)
					{
						this._lastFirstSelectControl.SelectFirstControl();
					}
					if (UICamera.controller.current != this._currentTarget)
					{
						this._speed = Mathf.Max(this._speed, 0.1f);
						this._currentTarget = ((!UICamera.controller.current || UICamera.controller.current.GetComponent<UIPanel>()) ? null : UICamera.controller.current);
						if (this._currentTarget)
						{
							this._currentTargetCol = this._currentTarget.GetComponent<Collider>();
							this._cursor.gameObject.SetActive(true);
							if (this._buttonActionIcon)
							{
								bool flag = this._currentTarget.GetComponent<UIButton>() || this._currentTarget.GetComponent<UIInput>();
								bool flag2 = this._currentTarget.GetComponent<UIToggle>();
								bool flag3 = this._currentTarget.GetComponent<UISlider>();
								bool flag4 = this._currentTarget.GetComponent<ToggleUIPopupListSelection>() || this._currentTarget.GetComponent<UIPopupList>();
								this._gamepadPopupInput.CurrentTarget = null;
								if (this._buttonActionIcon.activeSelf)
								{
									this._buttonActionIcon.SetActive(false);
								}
								if (this._sliderActionIcon.activeSelf)
								{
									this._sliderActionIcon.SetActive(false);
								}
								if (flag4)
								{
									base.StopAllCoroutines();
									base.StartCoroutine("RepositionPopupIcon");
								}
								else if (flag3)
								{
									base.StopAllCoroutines();
									base.StartCoroutine("RepositionSliderIcon");
								}
								else if (flag)
								{
									base.StopAllCoroutines();
									if (this._showButtonIcon)
									{
										base.StartCoroutine("RepositionButtonIcon");
									}
								}
								else if (flag2)
								{
									base.StopAllCoroutines();
									base.StartCoroutine("RepositionCheckboxIcon");
								}
							}
						}
						else if (this._cursor.gameObject.activeSelf)
						{
							this._gamepadPopupInput.CurrentTarget = null;
							this._cursor.gameObject.SetActive(false);
						}
					}
				}
			}
			else if (this._cursor.gameObject.activeSelf)
			{
				this._gamepadPopupInput.CurrentTarget = null;
				this._cursor.gameObject.SetActive(false);
			}
			if (this._currentTarget)
			{
				if (this._currentTargetCol)
				{
					if (this._currentTargetCol is BoxCollider)
					{
						this._cursor.position = this._currentTarget.transform.TransformPoint(((BoxCollider)this._currentTargetCol).center) + this._offset;
					}
					else if (this._currentTargetCol is SphereCollider)
					{
						this._cursor.position = this._currentTarget.transform.TransformPoint(((SphereCollider)this._currentTargetCol).center) + this._offset;
					}
				}
				else
				{
					this._cursor.position = this._currentTarget.transform.position + this._offset;
				}
				if (this._forceRefresh)
				{
					this._forceRefresh = false;
					this._currentTarget = null;
				}
			}
		}

		private void OnDestroy()
		{
			if (GamepadSelectedControl.Instance == this)
			{
				GamepadSelectedControl.Instance = null;
			}
		}

		public void ForceRefreshDelayed()
		{
			base.Invoke("ForceRefresh", 0.1f);
		}

		public void ForceRefresh()
		{
			this._forceRefresh = true;
		}

		private IEnumerator RepositionButtonIcon()
		{
			yield return null;
			while (TweenTransform.ActiveTransformTweener > 0)
			{
				yield return null;
			}
			yield return null;
			yield return null;
			Vector3 position = this._buttonActionIcon.transform.localPosition;
			if (this._currentTarget == null)
			{
				yield break;
			}
			if (this._currentTarget.GetComponentInChildren<LoadSaveSlotInfo>())
			{
				position.x = 210f;
				position.y = -140f;
			}
			else
			{
				GamepadInputIconPosition componentInParent = this._currentTarget.GetComponentInParent<GamepadInputIconPosition>();
				if (componentInParent)
				{
					if (!componentInParent._showIcon)
					{
						if (this._buttonActionIcon.activeSelf)
						{
							this._buttonActionIcon.SetActive(false);
						}
						yield break;
					}
					position.x = this._sliderActionIcon.transform.parent.InverseTransformPoint(componentInParent._positionTarget.position).x;
				}
				else
				{
					position.x = (this._currentTarget.GetComponentInChildren<UILabel>() ?? this._currentTarget.GetComponentInChildren<UIWidget>()).CalculateBounds(this._buttonActionIcon.transform.parent).max.x + 60f;
				}
				position.y = 22f;
			}
			if (!this._buttonActionIcon.activeSelf)
			{
				this._buttonActionIcon.SetActive(true);
			}
			this._buttonActionIcon.transform.localPosition = position;
			yield break;
		}

		private IEnumerator RepositionSliderIcon()
		{
			yield return null;
			while (TweenTransform.ActiveTransformTweener > 0)
			{
				yield return null;
			}
			if (this._sliderActionIcon.activeSelf != this._showSliderIcon)
			{
				this._sliderActionIcon.SetActive(this._showSliderIcon);
			}
			if (this._showSliderIcon)
			{
				Vector3 localPosition = this._sliderActionIcon.transform.localPosition;
				GamepadInputIconPosition componentInParent = this._currentTarget.GetComponentInParent<GamepadInputIconPosition>();
				if (componentInParent)
				{
					localPosition.x = this._sliderActionIcon.transform.parent.InverseTransformPoint(componentInParent._positionTarget.position).x;
				}
				else
				{
					localPosition.x = this._currentTarget.GetComponentInChildren<UISprite>().CalculateBounds(this._sliderActionIcon.transform.parent).max.x + 175f;
				}
				this._sliderActionIcon.transform.localPosition = localPosition;
			}
			yield break;
		}

		private IEnumerator RepositionPopupIcon()
		{
			yield return null;
			while (TweenTransform.ActiveTransformTweener > 0)
			{
				yield return null;
			}
			if (this._sliderActionIcon.activeSelf != this._showPopupIcon)
			{
				this._sliderActionIcon.SetActive(this._showPopupIcon);
			}
			ToggleUIPopupListSelection proxy = this._currentTarget.GetComponent<ToggleUIPopupListSelection>();
			Vector3 position = this._sliderActionIcon.transform.localPosition;
			GameObject widget;
			if (proxy)
			{
				widget = proxy._target.gameObject;
			}
			else
			{
				widget = this._currentTarget.gameObject;
			}
			GamepadInputToSelectedPopup gamepadPopupInput = base.GetComponent<GamepadInputToSelectedPopup>() ?? base.gameObject.AddComponent<GamepadInputToSelectedPopup>();
			gamepadPopupInput.CurrentTarget = widget;
			gamepadPopupInput.ForwardButton = widget.transform.parent.Find("Fwd").gameObject;
			gamepadPopupInput.BackwardButton = widget.transform.parent.Find("Bwd").gameObject;
			if (this._showPopupIcon)
			{
				GamepadInputIconPosition componentInParent = this._currentTarget.GetComponentInParent<GamepadInputIconPosition>();
				if (componentInParent)
				{
					position.x = this._sliderActionIcon.transform.parent.InverseTransformPoint(componentInParent._positionTarget.position).x;
				}
				else
				{
					position.x = this._sliderActionIcon.transform.parent.InverseTransformPoint(widget.transform.position).x + 325f;
				}
				this._sliderActionIcon.transform.localPosition = position;
			}
			yield break;
		}

		private IEnumerator RepositionCheckboxIcon()
		{
			yield return null;
			while (TweenTransform.ActiveTransformTweener > 0)
			{
				yield return null;
			}
			if (this._buttonActionIcon.activeSelf != this._showCheckboxIcon)
			{
				this._buttonActionIcon.SetActive(this._showCheckboxIcon);
			}
			if (this._showCheckboxIcon)
			{
				Vector3 localPosition = this._buttonActionIcon.transform.localPosition;
				GamepadInputIconPosition componentInParent = this._currentTarget.GetComponentInParent<GamepadInputIconPosition>();
				if (componentInParent)
				{
					localPosition.x = this._buttonActionIcon.transform.parent.InverseTransformPoint(componentInParent._positionTarget.position).x;
				}
				else
				{
					UILabel componentInChildren = this._currentTarget.transform.parent.GetComponentInChildren<UILabel>();
					if (componentInChildren)
					{
						localPosition.x = componentInChildren.CalculateBounds(this._buttonActionIcon.transform.parent).max.x + 100f;
					}
					else
					{
						localPosition.x = this._currentTarget.GetComponentInChildren<UISprite>().CalculateBounds(this._buttonActionIcon.transform.parent).max.x + 175f;
					}
				}
				this._buttonActionIcon.transform.localPosition = localPosition;
			}
			yield break;
		}

		public Transform _cursor;

		public GameObject _buttonActionIcon;

		public GameObject _sliderActionIcon;

		public Vector3 _offset = new Vector3(0f, -0.025f, 0f);

		public bool _showButtonIcon = true;

		public bool _showSliderIcon;

		public bool _showPopupIcon;

		public bool _showCheckboxIcon;

		public FirstSelectControl _lastFirstSelectControl;

		private GamepadInputToSelectedPopup _gamepadPopupInput;

		private GameObject _currentTarget;

		private Collider _currentTargetCol;

		private float _speed;

		private bool _forceRefresh;
	}
}
