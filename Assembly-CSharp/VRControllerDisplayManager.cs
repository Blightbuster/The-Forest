using System;
using System.Collections.Generic;
using Rewired;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class VRControllerDisplayManager : MonoBehaviour
{
	private void OnEnable()
	{
		if (VRControllerDisplayManager._managerInstances.Contains(this))
		{
			return;
		}
		VRControllerDisplayManager._managerInstances.Add(this);
	}

	private void OnDisable()
	{
		if (!VRControllerDisplayManager._managerInstances.Contains(this))
		{
			return;
		}
		VRControllerDisplayManager._managerInstances.Remove(this);
	}

	private void Update()
	{
		if (this.TestShow != InputMappingIcons.Actions.None)
		{
			this.ShowAction(this.TestShow, this.TestShowValue, null, ActionIcon.SideIconTypes.None, null, null, false);
		}
		if (this.TestAddTestTextLine)
		{
			this.AddTextLine(this.TestTextLine);
			this.TestAddTestTextLine = false;
			this.TestTextLine = null;
		}
		this.UpdateText();
	}

	private void UpdateText()
	{
		this.TrimTextLines();
		if (this.Text == null)
		{
			return;
		}
		if (this._textLines == null || this._textLines.Count == 0)
		{
			if (!this.Text.text.NullOrEmpty())
			{
				this.Text.text = string.Empty;
			}
			return;
		}
		this.Text.text = this._textLines.Join(Environment.NewLine);
	}

	private void TrimTextLines()
	{
		if (this._textLines == null || this._textLines.Count == 0)
		{
			return;
		}
		if (this._textLines.Count > this.MaxTextLines)
		{
			this._textLines.Dequeue();
		}
	}

	public void ShowAction(InputMappingIcons.Actions actionTarget, bool showValue, Texture2D iconTexture = null, ActionIcon.SideIconTypes sideIcon = ActionIcon.SideIconTypes.None, string actionStringOverride = null, DelayedAction delayedAction = null, bool useFillSprite = false)
	{
		Rewired.Player player = ReInput.players.GetPlayer(0);
		if (ReInput.players == null || player == null)
		{
			return;
		}
		Rewired.Player.ControllerHelper controllers = player.controllers;
		string stringValue = actionStringOverride.NullOrEmpty() ? this.GetActionText(actionTarget) : actionStringOverride;
		foreach (ControllerMap controllerMap in controllers.maps.GetAllMaps(ControllerType.Joystick))
		{
			if (controllerMap.enabled)
			{
				Controller controller = ReInput.controllers.GetController(ControllerType.Joystick, controllerMap.controllerId);
				if (controller.name.Equals(this.ControllerName))
				{
					foreach (ActionElementMap actionElementMap in controllerMap.AllMaps)
					{
						InputAction action = ReInput.mapping.GetAction(actionElementMap.actionId);
						if (action != null)
						{
							if (VRControllerDisplayManager.ActionMatches(actionTarget, action))
							{
								string elementIdentifierName = actionElementMap.elementIdentifierName;
								if (iconTexture == null && showValue)
								{
									this.RevertIcon(elementIdentifierName);
								}
								this.SetActive(elementIdentifierName, showValue, iconTexture, stringValue.IfNull(string.Empty).ToUpper(), this.ShouldShowTextBacking(), sideIcon, delayedAction, useFillSprite);
							}
						}
					}
				}
			}
		}
	}

	private static bool ActionMatches(InputMappingIcons.Actions actionTarget, InputAction action)
	{
		string name = action.name;
		return name.Equals(VRControllerDisplayManager.GetActionTargetName(actionTarget), StringComparison.InvariantCultureIgnoreCase);
	}

	private static string GetActionTargetName(InputMappingIcons.Actions actionTarget)
	{
		return actionTarget.ToString();
	}

	private bool ShouldShowTextBacking()
	{
		return TheForest.Utils.Input.GetState(InputState.Inventory);
	}

	private string GetActionText(InputMappingIcons.Actions actionTarget)
	{
		if (actionTarget == InputMappingIcons.Actions.None)
		{
			return string.Empty;
		}
		string text = actionTarget.ToString();
		if (TheForest.Utils.Input.GetState(InputState.Menu))
		{
			text = this.GetActionText(text, this.MenuTranslations);
		}
		else if (TheForest.Utils.Input.GetState(InputState.Inventory))
		{
			text = this.GetActionText(text, this.InventoryTranslations);
		}
		else if (TheForest.Utils.Input.GetState(InputState.World))
		{
			text = this.GetActionText(text, this.WorldTranslations);
		}
		text = text.ToUpper();
		return UiTranslationDatabase.TranslateKey(text, text, true);
	}

	private string GetActionText(string key, StringPairSet conversionSource)
	{
		if (conversionSource == null)
		{
			return key;
		}
		foreach (StringPairSet.StringPair stringPair in conversionSource.Items)
		{
			if (stringPair != null)
			{
				if (stringPair.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
				{
					return stringPair.Value;
				}
			}
		}
		return key;
	}

	private void RevertIcon(string buttonType)
	{
		foreach (VRButtonAction vrbuttonAction in this.Connections)
		{
			if (!(vrbuttonAction == null) && vrbuttonAction.IsType(buttonType))
			{
				vrbuttonAction.RevertIcon();
			}
		}
	}

	public void AddTextLine(string newLine)
	{
		if (this._textLines == null)
		{
			this._textLines = new Queue<string>();
		}
		this._textLines.Enqueue(newLine);
	}

	public void SetActive(string buttonType, bool activeValue, Texture2D sourceTexture = null, string text = null, bool showTextBacking = false, ActionIcon.SideIconTypes sideIcon = ActionIcon.SideIconTypes.None, DelayedAction delayedAction = null, bool useFillSprite = false)
	{
		foreach (VRButtonAction vrbuttonAction in this.Connections)
		{
			if (!(vrbuttonAction == null) && vrbuttonAction.IsType(buttonType))
			{
				vrbuttonAction.ButtonActive = activeValue;
				if (sourceTexture != null)
				{
					vrbuttonAction.SetIcon(sourceTexture);
				}
				vrbuttonAction.SetFillLink(delayedAction);
				vrbuttonAction.SetUseFillSprite(useFillSprite);
				vrbuttonAction.SetText(text, showTextBacking);
				vrbuttonAction.SetSideIcon(this.GetSideIcon(sideIcon));
			}
		}
	}

	private Texture2D GetSideIcon(ActionIcon.SideIconTypes sideIcon)
	{
		if (sideIcon == ActionIcon.SideIconTypes.UpArrow)
		{
			return this.UpArrowSideIcon;
		}
		if (sideIcon != ActionIcon.SideIconTypes.MiddleUpArrow)
		{
			return this.BlankSideIcon;
		}
		return this.MiddleUpArrowSideIcon;
	}

	public static void AutoShowAction(InputMappingIcons.Actions action, bool showValue, Texture2D iconTexture = null, ActionIcon.SideIconTypes sideIcon = ActionIcon.SideIconTypes.None, string actionStringOverride = null, DelayedAction delayedAction = null, bool useFillSprite = false)
	{
		foreach (VRControllerDisplayManager vrcontrollerDisplayManager in VRControllerDisplayManager._managerInstances)
		{
			vrcontrollerDisplayManager.ShowAction(action, showValue, iconTexture, sideIcon, actionStringOverride, delayedAction, useFillSprite);
		}
	}

	public static void AutoAddTextLine(string newLine, Hand.HandType handType)
	{
		VRControllerDisplayManager controllerDisplay = VRControllerDisplayManager.GetControllerDisplay(handType);
		if (controllerDisplay == null)
		{
			return;
		}
		controllerDisplay.AddTextLine(newLine);
	}

	public static VRControllerDisplayManager GetControllerDisplay(Hand.HandType handType)
	{
		foreach (VRControllerDisplayManager vrcontrollerDisplayManager in VRControllerDisplayManager._managerInstances)
		{
			if (!(vrcontrollerDisplayManager == null) && vrcontrollerDisplayManager.HandType == handType)
			{
				return vrcontrollerDisplayManager;
			}
		}
		return null;
	}

	public static VRControllerDisplayManager.VRControllerType GetActiveControllerType()
	{
		SteamVR instance = SteamVR.instance;
		string text = instance.hmd_TrackingSystemName.ToLowerInvariant();
		bool flag = text.Contains("lighthouse");
		bool flag2 = text.Contains("oculus");
		if (flag)
		{
			return VRControllerDisplayManager.VRControllerType.Vive;
		}
		if (flag2)
		{
			return VRControllerDisplayManager.VRControllerType.OculusTouch;
		}
		return VRControllerDisplayManager.VRControllerType.Unknown;
	}

	public static void ForceRefresh()
	{
		foreach (VRControllerDisplayManager vrcontrollerDisplayManager in VRControllerDisplayManager._managerInstances)
		{
			if (!(vrcontrollerDisplayManager == null) && vrcontrollerDisplayManager.Connections != null)
			{
				foreach (VRButtonAction vrbuttonAction in vrcontrollerDisplayManager.Connections)
				{
					if (!(vrbuttonAction == null))
					{
						vrbuttonAction.SetInactive();
					}
				}
			}
		}
	}

	private static List<VRControllerDisplayManager> _managerInstances = new List<VRControllerDisplayManager>();

	public string ControllerName;

	public Hand.HandType HandType;

	public TextMesh Text;

	public Texture2D UpArrowSideIcon;

	public Texture2D MiddleUpArrowSideIcon;

	public Texture2D BlankSideIcon;

	public int MaxTextLines = 6;

	public List<VRButtonAction> Connections = new List<VRButtonAction>();

	public InputMappingIcons.Actions TestShow;

	public bool TestShowValue;

	public string TestTextLine;

	public bool TestAddTestTextLine;

	public StringPairSet InventoryTranslations;

	public StringPairSet MenuTranslations;

	public StringPairSet WorldTranslations;

	private Queue<string> _textLines;

	public GameObject RespawnButton;

	public enum VRControllerType
	{
		Unknown,
		Vive,
		OculusTouch
	}
}
