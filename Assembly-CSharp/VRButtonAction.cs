using System;
using TheForest.UI;
using UnityEngine;

public class VRButtonAction : MonoBehaviour
{
	public bool ButtonActive
	{
		get
		{
			return this._buttonActive;
		}
		set
		{
			this._buttonActive = value;
		}
	}

	private void Awake()
	{
		if (this.Pivot == null)
		{
			this.Pivot = base.gameObject;
		}
		this._basePos = this.Pivot.transform.localPosition;
	}

	private void LateUpdate()
	{
		if (this.ButtonActive)
		{
			this.UpdateTransition(VRButtonAction.VRButtonState.IdleActive, VRButtonAction.VRButtonState.Activating, Vector3.one, this.ActiveSpeed);
		}
		else
		{
			this.UpdateTransition(VRButtonAction.VRButtonState.IdleInactive, VRButtonAction.VRButtonState.Inactivating, this.InactiveScale, this.InactiveSpeed);
			this._useFillSprite = false;
			this._delayedAction = null;
		}
		this.UpdateFill();
	}

	private void UpdateFill()
	{
		if (this.FillParent == null || this.FillRenderer == null)
		{
			return;
		}
		if (this._delayedAction == null || !this._delayedAction.gameObject.activeSelf)
		{
			this.FillAmount = 0f;
			if (this.FillParent.activeSelf)
			{
				this.FillParent.SetActive(false);
			}
		}
		else
		{
			this.FillAmount = this._delayedAction._fillIcon.fillAmount;
			if (!this.FillParent.activeSelf)
			{
				this.FillParent.SetActive(true);
			}
		}
		this.FillAmount = Mathf.Clamp01(this.FillAmount);
		if (this._fillMaterial == null)
		{
			this._fillMaterial = this.FillRenderer.material;
		}
		this._fillMaterial.SetFloat("_AlphaCutoff", Mathf.Max(0.01f, 1f - this.FillAmount));
	}

	private void OnDestroy()
	{
		if (this._iconMaterialInstance != null)
		{
			UnityEngine.Object.Destroy(this._iconMaterialInstance);
		}
		this._iconMaterialInstance = null;
		if (this._fillMaterial != null)
		{
			UnityEngine.Object.Destroy(this._fillMaterial);
		}
		this._fillMaterial = null;
	}

	public bool IsType(string buttonType)
	{
		foreach (string text in this.ButtonTypes)
		{
			if (text.Equals(buttonType))
			{
				return true;
			}
		}
		return false;
	}

	public void SetFillLink(DelayedAction delayedAction)
	{
		this._delayedAction = delayedAction;
	}

	public void SetUseFillSprite(bool useFillSprite)
	{
		this._useFillSprite = useFillSprite;
	}

	public void SetText(string text, bool showBacking = false)
	{
		if (this.Text == null)
		{
			return;
		}
		this.Text.text = (text ?? string.Empty);
		if (this.TextBacking != null)
		{
			this.TextBacking.SetActive(showBacking);
		}
	}

	public void SetIcon(Texture2D sourceTexture)
	{
		if (this.Icon == null)
		{
			return;
		}
		if (this._iconMaterialInstance == null)
		{
			this._iconMaterialInstance = this.Icon.material;
			this._iconDefault = (this._iconMaterialInstance.mainTexture as Texture2D);
		}
		if (this._iconMaterialInstance.mainTexture == sourceTexture)
		{
			return;
		}
		this._iconMaterialInstance.mainTexture = sourceTexture;
	}

	public void SetSideIcon(Texture2D sourceTexture)
	{
		if (this.SideIcon == null)
		{
			return;
		}
		if (this._sideIconMaterialInstance == null)
		{
			this._sideIconMaterialInstance = this.SideIcon.material;
		}
		if (this._sideIconMaterialInstance.mainTexture == sourceTexture)
		{
			return;
		}
		this._sideIconMaterialInstance.mainTexture = sourceTexture;
	}

	public void SetInactive()
	{
		this._state = VRButtonAction.VRButtonState.IdleActive;
		this.UpdateTransition(VRButtonAction.VRButtonState.IdleInactive, VRButtonAction.VRButtonState.Inactivating, this.InactiveScale, 1f / Time.deltaTime + 1f);
		this._buttonActive = false;
		this._state = VRButtonAction.VRButtonState.IdleInactive;
		this.Pivot.transform.localScale = this.InactiveScale;
	}

	public void RevertIcon()
	{
		if (this._iconMaterialInstance == null)
		{
			return;
		}
		this._iconMaterialInstance.mainTexture = this._iconDefault;
	}

	private void UpdateTransition(VRButtonAction.VRButtonState idleState, VRButtonAction.VRButtonState transitionState, Vector3 localScale, float speed)
	{
		if (this._state == idleState)
		{
			return;
		}
		if (this._state != transitionState)
		{
			this._transitionStarted = Time.time;
		}
		this._state = transitionState;
		this.ApplyTransforms(localScale, speed);
		if (this.ShouldIdle())
		{
			this._state = idleState;
			this.Pivot.transform.localScale = ((!this.ButtonActive) ? this.InactiveScale : Vector3.one);
		}
	}

	private bool ShouldIdle()
	{
		return Time.time - this._transitionStarted > this.IdleAfter;
	}

	private void ApplyTransforms(Vector3 targetLocalScale, float speed)
	{
		Vector3 localPosition = this.Pivot.transform.localPosition;
		Vector3 localEulerAngles = this.Pivot.transform.localEulerAngles;
		Vector3 localScale = this.Pivot.transform.localScale;
		float num = Time.deltaTime * speed;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		if (num >= 1f)
		{
			this.Pivot.transform.localScale = targetLocalScale;
		}
		else
		{
			this.Pivot.transform.localScale = Vector3.Slerp(localScale, targetLocalScale, num);
		}
	}

	public string[] ButtonTypes = new string[0];

	public GameObject Pivot;

	public TextMesh Text;

	public GameObject TextBacking;

	public Renderer Icon;

	public Renderer SideIcon;

	public float ActiveSpeed = 10f;

	public GameObject FillParent;

	public Renderer FillRenderer;

	public float InactiveSpeed = 20f;

	public Vector3 InactiveScale = Vector3.zero;

	public float IdleAfter = 3f;

	public float FillAmount;

	private bool _buttonActive;

	private float _transitionStarted;

	private VRButtonAction.VRButtonState _state;

	private Vector3 _basePos;

	private Texture2D _iconDefault;

	private Material _iconMaterialInstance;

	private DelayedAction _delayedAction;

	private Material _sideIconMaterialInstance;

	private Material _fillMaterial;

	private bool _useFillSprite;

	internal enum VRButtonState
	{
		IdleActive,
		Activating,
		IdleInactive,
		Inactivating
	}
}
