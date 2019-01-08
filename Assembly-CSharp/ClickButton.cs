using System;
using UnityEngine;

internal class ClickButton : AbstractButton
{
	public override void Update()
	{
		if (this.m_Rect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(0) && !this.pressed)
		{
			this.pressed = true;
			this.m_Button.Pressed();
			return;
		}
		if (Input.GetMouseButtonUp(0) && this.pressed)
		{
			this.pressed = false;
			this.m_Button.Released();
		}
	}

	private bool pressed;
}
