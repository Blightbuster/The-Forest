using System;
using TheForest.Items.Inventory;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;


public class SimpleMouseRotator : MonoBehaviour
{
	
	private void Start()
	{
		this.originalRotation = ((!this.useRigidbody) ? base.transform.localRotation : this.rb.rotation);
		this.prevSpeed = this.rotationSpeed;
		this.prevRange = this.rotationRange;
	}

	
	private void OnEnable()
	{
	}

	
	public void disableMouse()
	{
	}

	
	public void enableMouse()
	{
	}

	
	private void Update()
	{
		if (!LocalPlayer.FpCharacter.Locked && !this.lockRotation && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.ClosingInventory)
		{
			this.UpdateRotation();
		}
	}

	
	private void UpdateRotation()
	{
		if (!ForestVR.Enabled)
		{
			this.CheckResetOriginalRotation();
			if (this.cameraRotator)
			{
				if (this.originalRotation.y != 0f)
				{
					this.originalRotation.y = 0f;
				}
				if (this.originalRotation.z != 0f)
				{
					this.originalRotation.z = 0f;
				}
				if (this.originalRotation.x != 0f)
				{
					this.originalRotation.x = 0f;
				}
			}
			else
			{
				if (this.originalRotation.x != 0f)
				{
					this.originalRotation.x = 0f;
				}
				if (this.originalRotation.z != 0f)
				{
					this.originalRotation.z = 0f;
				}
			}
			Vector2 zero = Vector2.zero;
			Vector2 input = this.GetInput();
			this.ClampAngles(input);
			zero.y = this.targetAngles.y + this.yOffset;
			zero.x = this.targetAngles.x + this.xOffset;
			this.UpdateCameraRotation(zero);
		}
	}

	
	private void CheckResetOriginalRotation()
	{
		if (this.resetOriginalRotation)
		{
			if (this.cameraRotator)
			{
				this.originalRotation.x = ((!this.useRigidbody) ? base.transform.localRotation.x : this.rb.rotation.x);
			}
			else
			{
				this.originalRotation = ((!this.useRigidbody) ? base.transform.localRotation : this.rb.rotation);
			}
			this.targetAngles.y = this.targetAngles.y - this.targetAngles.y;
			this.targetAngles.x = this.targetAngles.x - this.targetAngles.x;
			this.followAngles = Vector2.zero;
			this.resetOriginalRotation = false;
		}
	}

	
	private void UpdateCameraRotation(Vector2 modAngles)
	{
		if (this.fixCameraRotation)
		{
			this.followAngles = modAngles;
			this.fixCameraRotation = false;
		}
		else
		{
			float smoothTime = Mathf.Clamp(this.dampingTime * Mathf.Lerp(0f, 10f, PlayerPreferences.MouseSmoothing) + this.dampingOverride, 0f, 0.1f);
			this.followAngles = Vector3.SmoothDamp(this.followAngles, modAngles, ref this.followVelocity, smoothTime);
		}
		if (this.stopInput)
		{
			return;
		}
		Quaternion quaternion = this.originalRotation * Quaternion.Euler(-this.followAngles.x, this.followAngles.y, 0f);
		if (this.useRigidbody)
		{
			this.rb.MoveRotation(quaternion);
		}
		else
		{
			base.transform.localRotation = quaternion;
		}
	}

	
	private void ClampAngles(Vector2 input)
	{
		if (this.targetAngles.y > 180f)
		{
			this.targetAngles.y = this.targetAngles.y - 360f;
			this.followAngles.y = this.followAngles.y - 360f;
		}
		if (this.targetAngles.x > 180f)
		{
			this.targetAngles.x = this.targetAngles.x - 360f;
			this.followAngles.x = this.followAngles.x - 360f;
		}
		if (this.targetAngles.y < -180f)
		{
			this.targetAngles.y = this.targetAngles.y + 360f;
			this.followAngles.y = this.followAngles.y + 360f;
		}
		if (this.targetAngles.x < -180f)
		{
			this.targetAngles.x = this.targetAngles.x + 360f;
			this.followAngles.x = this.followAngles.x + 360f;
		}
		this.targetAngles.y = this.targetAngles.y + input.x * this.rotationSpeed * VirtualCursor.Instance.GamepadSensitivityMult;
		this.targetAngles.x = this.targetAngles.x + input.y * this.rotationSpeed * VirtualCursor.Instance.GamepadSensitivityMult;
		this.targetAngles.y = Mathf.Clamp(this.targetAngles.y, -this.rotationRange.y * 0.5f, this.rotationRange.y * 0.5f);
		this.targetAngles.x = Mathf.Clamp(this.targetAngles.x, -this.rotationRange.x * 0.5f, this.rotationRange.x * 0.5f);
	}

	
	private Vector2 GetInput()
	{
		float num = Mathf.Lerp(0.25f, 3f, PlayerPreferences.MouseSensitivityX);
		float num2 = Mathf.Lerp(0.25f, 3f, PlayerPreferences.MouseSensitivityY);
		float num3 = 1f;
		if (TheForest.Utils.Input.IsGamePad)
		{
			num3 = Time.deltaTime / 0.0222222228f;
		}
		float num4 = TheForest.Utils.Input.GetAxis("Mouse X") * num3;
		float num5 = TheForest.Utils.Input.GetAxis("Mouse Y") * num3;
		num4 *= num;
		num5 *= num2;
		if (PlayerPreferences.MouseInvert)
		{
			num5 = -num5;
		}
		if (LocalPlayer.FpCharacter.MovementLocked || ForestVR.Enabled)
		{
			num4 = 0f;
			num5 = 0f;
		}
		return new Vector2(num4, num5);
	}

	
	public bool useRigidbody;

	
	public Vector2 rotationRange = new Vector3(70f, 70f);

	
	public float rotationSpeed = 10f;

	
	public float dampingTime = 0.2f;

	
	public float dampingOverride;

	
	public bool lockRotation;

	
	public bool resetOriginalRotation;

	
	public bool fixCameraRotation;

	
	public bool stopInput;

	
	public Vector3 targetAngles;

	
	public Vector3 followAngles;

	
	public float yOffset;

	
	public float xOffset;

	
	public bool debugEfficiency;

	
	public bool cameraRotator;

	
	private Vector3 followVelocity;

	
	public Quaternion originalRotation;

	
	public float prevSpeed;

	
	public Vector2 prevRange;

	
	public Rigidbody rb;
}
