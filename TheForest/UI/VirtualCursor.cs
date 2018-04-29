using System;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.UI
{
	
	public class VirtualCursor : MonoBehaviour
	{
		
		private void Awake()
		{
			VirtualCursor.Instance = this;
			if (!CoopPeerStarter.DedicatedHost)
			{
				Cursor.SetCursor(this._mouseIcon, new Vector2(45f, 24f), CursorMode.Auto);
				this._position.x = (float)(Screen.width / 2);
				this._position.y = (float)(Screen.height / 2);
			}
			else
			{
				base.enabled = false;
			}
		}

		
		private void OnDestroy()
		{
			if (VirtualCursor.Instance == this)
			{
				VirtualCursor.Instance = null;
			}
		}

		
		private void LateUpdate()
		{
			this.GamepadSensitivityMult = this.GetAdjustedGamepadSensitivity();
			if (TheForest.Utils.Input.IsMouseLocked)
			{
				if (Cursor.lockState != CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.Locked;
				}
				if (Cursor.visible)
				{
					Cursor.visible = false;
				}
				if (this._active || this.IsVisibleSoftwareCursorActive)
				{
					this._active = false;
					if (Scene.HudGui)
					{
						Scene.HudGui.MouseSprite.gameObject.SetActive(false);
					}
				}
			}
			else
			{
				this.UseVirtualPosition = ((TheForest.Utils.Input.IsGamePad || (LocalPlayer.Inventory && (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book))) && Scene.HudGui);
				if (this.UseVirtualPosition && LocalPlayer.Inventory && LocalPlayer.Inventory.enabled && (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book))
				{
					this.SetCursorType(this._cursorType);
					this._mouseSpeedScaledX = this._mouseSpeed * (float)Screen.height / 600f;
					this._mouseSpeedScaledY = this._mouseSpeedScaledX;
					if (!TheForest.Utils.Input.IsGamePad)
					{
						this._mouseSpeedScaledX *= 1.5f;
						this._mouseSpeedScaledY *= 1.5f;
					}
					if (!this._active || Cursor.visible)
					{
						Cursor.lockState = CursorLockMode.Locked;
						Cursor.visible = false;
						Scene.HudGui.MouseSprite.gameObject.SetActive(true);
						this._active = true;
						if (TheForest.Utils.Input.player.controllers.Mouse != null && !TheForest.Utils.Input.WasGamePad)
						{
							if (!this.OverridingPosition)
							{
								this._position = TheForest.Utils.Input.player.controllers.Mouse.screenPosition;
							}
						}
						else if (!this.OverridingPosition)
						{
							this._position.x = (float)(Screen.width / 2);
							this._position.y = (float)(Screen.height / 2);
						}
					}
					if (!this.OverridingPosition)
					{
						float num = Time.unscaledDeltaTime / 0.0166666675f;
						if (TheForest.Utils.Input.IsGamePad)
						{
							float axis = TheForest.Utils.Input.GetAxis("Horizontal");
							float axis2 = TheForest.Utils.Input.GetAxis("Vertical");
							this._position.x = Mathf.Clamp(this._position.x + axis * this._mouseSpeedScaledX * this.GamepadSensitivityMult * num, 0f, (float)(Screen.width - 20));
							this._position.y = Mathf.Clamp(this._position.y + axis2 * this._mouseSpeedScaledY * this.GamepadSensitivityMult * num, 20f, (float)Screen.height);
						}
						else
						{
							this._position.x = Mathf.Clamp(this._position.x + TheForest.Utils.Input.GetAxis("Mouse X") * this._mouseSpeedScaledX, 0f, (float)(Screen.width - 20));
							this._position.y = Mathf.Clamp(this._position.y + TheForest.Utils.Input.GetAxis("Mouse Y") * this._mouseSpeedScaledY, 20f, (float)Screen.height);
						}
					}
					if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
					{
						this._position.z = 3f;
						Scene.HudGui.MouseSprite.position = Scene.HudGui.BookCam.ScreenToWorldPoint(this._position);
					}
					else if (Scene.HudGui.GuiCamC)
					{
						this._position.z = 1f;
						Scene.HudGui.MouseSprite.position = Scene.HudGui.GuiCamC.ScreenToWorldPoint(this._position);
					}
					else
					{
						this._position.z = 3f;
						Scene.HudGui.MouseSprite.position = Camera.main.ScreenToWorldPoint(this._position);
					}
				}
				else if (TheForest.Utils.Input.IsGamePad)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = false;
					this._active = true;
					if (Scene.HudGui && !this.IsVisibleSoftwareCursorActive)
					{
						Scene.HudGui.MouseSprite.gameObject.SetActive(true);
					}
				}
				else if (this._active)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					this._active = false;
					if (this.IsVisibleSoftwareCursorActive)
					{
						Scene.HudGui.MouseSprite.gameObject.SetActive(false);
					}
				}
				else
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
					if (this.IsVisibleSoftwareCursorActive)
					{
						Scene.HudGui.MouseSprite.gameObject.SetActive(false);
					}
				}
			}
		}

		
		public void SetTargetWorldPosition(Vector3 worldPos)
		{
			if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Book)
			{
				this._position = LocalPlayer.MainCam.WorldToScreenPoint(worldPos);
			}
			else
			{
				this._position = LocalPlayer.InventoryCam.WorldToScreenPoint(worldPos);
			}
		}

		
		public void SetCursorType(VirtualCursor.CursorTypes type)
		{
			if (this._cursorType != type)
			{
				this._cursorType = type;
				if (Scene.HudGui)
				{
					bool flag = type == VirtualCursor.CursorTypes.Inventory || type == VirtualCursor.CursorTypes.InventoryHover;
					Scene.HudGui.MouseSpriteHand.enabled = (type == VirtualCursor.CursorTypes.Hand);
					Scene.HudGui.MouseSpriteInventoryArrow.enabled = ((flag && !TheForest.Utils.Input.IsGamePad) || type == VirtualCursor.CursorTypes.Arrow);
					if (flag && TheForest.Utils.Input.IsGamePad)
					{
						Scene.HudGui.MouseSpriteInventoryInner.enabled = true;
						Scene.HudGui.MouseSpriteInventoryOuter.enabled = true;
						Scene.HudGui.MouseSpriteInventoryOuter.alpha = ((type != VirtualCursor.CursorTypes.InventoryHover) ? 0.25f : 1f);
					}
					else
					{
						Scene.HudGui.MouseSpriteInventoryInner.enabled = false;
						Scene.HudGui.MouseSpriteInventoryOuter.enabled = false;
					}
				}
			}
		}

		
		private float GetAdjustedGamepadSensitivity()
		{
			if (TheForest.Utils.Input.IsGamePad)
			{
				float num;
				if (!LocalPlayer.Inventory || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Inventory || LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Pause)
				{
					Vector2 vector = new Vector2(TheForest.Utils.Input.GetAxis("Horizontal"), TheForest.Utils.Input.GetAxis("Vertical"));
					num = Mathf.Clamp01(vector.magnitude);
				}
				else
				{
					Vector2 vector2 = new Vector2(TheForest.Utils.Input.GetAxis("Mouse X"), TheForest.Utils.Input.GetAxis("Mouse Y"));
					num = Mathf.Clamp01(vector2.magnitude);
				}
				num *= num;
				num /= 1.2f;
				num = 0.17f + num;
				return Mathf.Clamp(num, 0.17f, 1f);
			}
			return 1f;
		}

		
		
		
		public float GamepadSensitivityMult { get; private set; }

		
		
		public Vector3 Position
		{
			get
			{
				return this._position;
			}
		}

		
		
		public bool IsActive
		{
			get
			{
				return this._active;
			}
		}

		
		
		
		public GameObject OverridingPosition { get; set; }

		
		
		
		public bool UseVirtualPosition { get; private set; }

		
		
		public bool IsVisibleSoftwareCursorActive
		{
			get
			{
				return Scene.HudGui && Scene.HudGui.MouseSprite.gameObject.activeSelf;
			}
		}

		
		public float _mouseSpeed = 5f;

		
		public Texture2D _mouseIcon;

		
		public static VirtualCursor Instance;

		
		public VirtualCursor.CursorTypes _cursorType = VirtualCursor.CursorTypes.Hand;

		
		private float _mouseSpeedScaledX;

		
		private float _mouseSpeedScaledY;

		
		private bool _active;

		
		private Vector3 _position = Vector3.zero;

		
		public enum CursorTypes
		{
			
			Reset = -1,
			
			None,
			
			Hand,
			
			Inventory,
			
			InventoryHover,
			
			Arrow
		}
	}
}
