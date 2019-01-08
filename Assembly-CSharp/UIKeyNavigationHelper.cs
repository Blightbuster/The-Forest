using System;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

public class UIKeyNavigationHelper : MonoBehaviour
{
	private void Update()
	{
		if (this.CatchAction == InputMappingIcons.Actions.None)
		{
			return;
		}
		if (TheForest.Utils.Input.IsGamePad && UIKeyNavigation.current == this.TargetNavigation)
		{
			float axisDown = TheForest.Utils.Input.GetAxisDown(this.CatchAction.ToString());
			if (Mathf.Abs(axisDown) > this.DeadZone && Mathf.Abs(axisDown) > Mathf.Abs(TheForest.Utils.Input.GetAxisDown("Vertical")))
			{
				if (UIKeyNavigationHelper._hasNavigated)
				{
					return;
				}
				UIKeyNavigationHelper._hasNavigated = true;
				if (axisDown > 0f)
				{
					this.TargetNavigation.OnNavigate(KeyCode.RightArrow);
				}
				else
				{
					this.TargetNavigation.OnNavigate(KeyCode.LeftArrow);
				}
			}
			else
			{
				UIKeyNavigationHelper._hasNavigated = false;
			}
		}
	}

	public InputMappingIcons.Actions CatchAction;

	public UIKeyNavigation TargetNavigation;

	public float DeadZone = 0.02f;

	private static bool _hasNavigated;
}
