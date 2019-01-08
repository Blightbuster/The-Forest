using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OVRPlatformMenu : MonoBehaviour
{
	private OVRPlatformMenu.eBackButtonAction ResetAndSendAction(OVRPlatformMenu.eBackButtonAction action)
	{
		MonoBehaviour.print("ResetAndSendAction( " + action + " );");
		this.downCount = 0;
		this.upCount = 0;
		this.initialDownTime = -1f;
		return action;
	}

	private OVRPlatformMenu.eBackButtonAction HandleBackButtonState()
	{
		if (Input.GetKeyDown(this.keyCode))
		{
			this.downCount++;
			if (this.downCount == 1)
			{
				this.initialDownTime = Time.realtimeSinceStartup;
			}
		}
		else if (this.downCount > 0)
		{
			if (Input.GetKey(this.keyCode))
			{
				if (this.downCount <= this.upCount)
				{
					this.downCount++;
				}
				float num = Time.realtimeSinceStartup - this.initialDownTime;
				if (num > this.longPressDelay)
				{
					return this.ResetAndSendAction(OVRPlatformMenu.eBackButtonAction.NONE);
				}
			}
			else
			{
				bool flag = this.initialDownTime >= 0f;
				if (flag)
				{
					if (this.upCount < this.downCount)
					{
						this.upCount++;
					}
					float num2 = Time.realtimeSinceStartup - this.initialDownTime;
					if (num2 < this.doubleTapDelay)
					{
						if (this.downCount == 2 && this.upCount == 2)
						{
							return this.ResetAndSendAction(OVRPlatformMenu.eBackButtonAction.DOUBLE_TAP);
						}
					}
					else if (num2 > this.shortPressDelay && num2 < this.longPressDelay)
					{
						if (this.downCount == 1 && this.upCount == 1)
						{
							return this.ResetAndSendAction(OVRPlatformMenu.eBackButtonAction.SHORT_PRESS);
						}
					}
					else if (num2 > this.longPressDelay)
					{
						return this.ResetAndSendAction(OVRPlatformMenu.eBackButtonAction.NONE);
					}
				}
			}
		}
		return OVRPlatformMenu.eBackButtonAction.NONE;
	}

	private void Awake()
	{
		if (this.shortPressHandler == OVRPlatformMenu.eHandler.RetreatOneLevel && this.OnShortPress == null)
		{
			if (OVRPlatformMenu.<>f__mg$cache0 == null)
			{
				OVRPlatformMenu.<>f__mg$cache0 = new Func<bool>(OVRPlatformMenu.RetreatOneLevel);
			}
			this.OnShortPress = OVRPlatformMenu.<>f__mg$cache0;
		}
		if (!OVRManager.isHmdPresent)
		{
			base.enabled = false;
			return;
		}
		OVRPlatformMenu.sceneStack.Push(SceneManager.GetActiveScene().name);
	}

	private void OnApplicationFocus(bool focusState)
	{
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus)
		{
			Input.ResetInputAxes();
		}
	}

	private void ShowConfirmQuitMenu()
	{
	}

	private static bool RetreatOneLevel()
	{
		if (OVRPlatformMenu.sceneStack.Count > 1)
		{
			string sceneName = OVRPlatformMenu.sceneStack.Pop();
			SceneManager.LoadSceneAsync(sceneName);
			return false;
		}
		return true;
	}

	private void Update()
	{
	}

	public KeyCode keyCode = KeyCode.Escape;

	public OVRPlatformMenu.eHandler shortPressHandler;

	public Func<bool> OnShortPress;

	private static Stack<string> sceneStack = new Stack<string>();

	private float doubleTapDelay = 0.25f;

	private float shortPressDelay = 0.25f;

	private float longPressDelay = 0.75f;

	private int downCount;

	private int upCount;

	private float initialDownTime = -1f;

	[CompilerGenerated]
	private static Func<bool> <>f__mg$cache0;

	public enum eHandler
	{
		ShowConfirmQuit,
		RetreatOneLevel
	}

	private enum eBackButtonAction
	{
		NONE,
		DOUBLE_TAP,
		SHORT_PRESS
	}
}
